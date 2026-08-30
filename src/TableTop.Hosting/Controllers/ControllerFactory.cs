using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Progression;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Persistence;

namespace TableTop.Hosting.Controllers;

/// <summary>
/// Default <see cref="TableTop.Hosting.Abstractions.IControllerFactory"/> implementation.
///
/// Maps every known game mode to its correct controller type and configuration.
/// UIs call <see cref="CreateAsync"/> and receive an <see cref="IGameController"/>
/// — they never construct controllers directly.
///
/// Dispatch is driven entirely by <i>capability interfaces the mode implements</i>.
/// No concrete mode type is referenced anywhere in this class. A new mode that fits
/// an existing controller shape needs <b>no change here</b> — it implements the
/// relevant capability interface and the factory routes it automatically (OCP, DIP):
///
/// <list type="bullet">
/// <item><see cref="IMonogamyDeckProvider"/> → <see cref="MonogamyController"/></item>
/// <item><see cref="IQuestionBankProvider"/> → <see cref="MillionaireController"/></item>
/// <item><see cref="IHerdDeckProvider"/> → <see cref="HerdController"/></item>
/// <item><see cref="IClaimedDeckProvider"/> → <see cref="ClaimedController"/></item>
/// <item><see cref="IDailyDeckProvider"/> → <see cref="DayOneController"/></item>
/// <item><see cref="IFlowAwareMode"/> + <see cref="IGameModeDefinition"/> → <see cref="CardTurnController"/> with <c>FlowAwareProgressionStrategy</c></item>
/// <item><see cref="IDiceProgressionMode"/> + <see cref="IGameModeDefinition"/> → <see cref="CardTurnController"/> with <c>DiceCategoryProgressionStrategy</c></item>
/// <item><see cref="IGameModeDefinition"/> → <see cref="CardTurnController"/> with <c>DifficultyProgressionStrategy</c></item>
/// </list>
/// </summary>
public sealed class ControllerFactory : IControllerFactory
{
    private readonly IGamePersistence? _persistence;

    /// <param name="persistence">
    /// Optional persistence implementation injected into controllers that support
    /// save/resume. If null, controllers use <see cref="JsonGamePersistence"/> by default.
    /// </param>
    /// <remarks>
    /// This constructor once also took an <c>IEngineDiagnostics</c>, whose only job
    /// was to assign the process-wide <c>JsonDeckLoader.Diagnostics</c> static so a
    /// mode falling back from its JSON deck to its C# bank was reported. The JSON
    /// deck path is gone (1.19.0), and with it the static — which was a real hazard
    /// while it lasted: the assignment ran on every construction including with a
    /// null argument, and this type is registered transient, so any resolution
    /// anywhere silently cleared a sink another host had set.
    /// Engine diagnostics are unaffected; they were never routed through here.
    /// </remarks>
    public ControllerFactory(IGamePersistence? persistence = null) =>
        _persistence = persistence;

    /// <inheritdoc />
    public Task<IGameController> CreateAsync(
        IGameMode mode,
        IReadOnlyList<IPlayer> players,
        int maxRounds = Core.TableTopDefaults.Session.MaxRounds,
        GameplayOptions? gameplayOptions = null,
        Persistence.SessionSnapshot? resumeFrom = null,
        int? monogamyWinningTokenCount = null,
        CancellationToken ct = default)
    {
        // GameplayOptions (shuffle/difficulty-range/session-length) currently
        // shapes the CardTurnController path, which is what the vast majority
        // of modes use. Millionaire's ladder, Monogamy's zone deck, and Day
        // One's daily campaign each have their own progression model where
        // "shuffle" or "difficulty range" doesn't map cleanly onto the same
        // concept, so those branches accept the parameter but don't apply it
        // — an honest scope boundary rather than a forced, misleading fit.
        // monogamyWinningTokenCount gets the same treatment in the other
        // direction: only the Monogamy arm reads it.
        // Dispatch on ControllerFamilies.TryFor rather than re-testing the
        // capability interfaces here (backlog L.3).
        //
        // This used to be its own if/else chain over the same seven interfaces,
        // in an order that had to be kept identical to TryFor's by hand — both
        // files said so in comments, and it was wrong once for real: Monogamy
        // and Quiz were transposed while a comment claimed they could not
        // disagree. Nothing caught it, because no mode in the catalogue
        // implements two capability interfaces, so every input the parity test
        // could actually be given produced the same answer either way.
        //
        // ModeManifestExtensions.GetManifest was moved onto TryFor for exactly
        // this reason and the divergence it had became structurally impossible.
        // This is the last of the three. The casts are safe by construction:
        // the family was decided by these very interfaces.
        return ControllerFamilies.TryFor(mode) switch
        {
            // ── Monogamy — mode supplies its own deck + win condition ─────────
            ControllerFamily.Monogamy => Task.FromResult<IGameController>(
                new MonogamyController(
                    players,
                    ((IMonogamyDeckProvider)mode).GetDeck(),
                    winningTokenCount: monogamyWinningTokenCount
                        ?? ((IMonogamyDeckProvider)mode).WinningTokenCount)),

            // ── Millionaire family — mode supplies its own question bank ──────
            ControllerFamily.Quiz => Task.FromResult<IGameController>(
                new MillionaireController(players, ((IQuestionBankProvider)mode).GetQuestionBank())),

            // ── Herd — everyone answers at once; no single active player ──────
            ControllerFamily.SimultaneousAnswer => Task.FromResult<IGameController>(
                new HerdController(players, ((IHerdDeckProvider)mode).GetHerdDeck())),

            // ── Claimed! — mode supplies its own territory-challenge deck ─────
            ControllerFamily.AreaControl => Task.FromResult<IGameController>(
                new ClaimedController(
                    players,
                    ((IClaimedDeckProvider)mode).GetClaimedDeck(),
                    winningTerritoryCount: ((IClaimedDeckProvider)mode).WinningTerritoryCount)),

            // ── Day One — mode supplies a strictly-ordered daily campaign deck ─
            ControllerFamily.DailyCampaign => Task.FromResult<IGameController>(
                new DayOneController(((IDailyDeckProvider)mode).GetDailyDeck(), players, mode.Name)),

            // ── Card-turn — one controller type, three progression strategies ─
            // IFlowAwareMode and IDiceProgressionMode change the strategy, not
            // the controller, which is why TryFor folds all three into CardTurn
            // and the choice between them lives here rather than up there.
            ControllerFamily.CardTurn =>
                CreateCardTurnAsync(
                    (IGameModeDefinition)mode, players, mode.Name, maxRounds,
                    ProgressionFor(mode), gameplayOptions, resumeFrom, ct),

            _ => throw new NotSupportedException(
                $"No controller registered for mode '{mode.Name}' " +
                $"(type: {mode.GetType().Name}). Implement IGameModeDefinition, " +
                $"IQuestionBankProvider, IMonogamyDeckProvider, IDailyDeckProvider, IClaimedDeckProvider, IHerdDeckProvider, IFlowAwareMode, or IDiceProgressionMode on the mode.")
        };
    }

    /// <summary>
    /// Which progression strategy a card-turn mode drives its deck with.
    ///
    /// <para>
    /// Separate from family dispatch on purpose: every arm here produces the
    /// same <c>CardTurnController</c>, so this is a *within-family* choice and
    /// does not belong in <see cref="ControllerFamilies.TryFor"/>. Adding a
    /// third progression flavour means editing this method only; adding a new
    /// controller shape still means a new family and an arm above.
    /// </para>
    /// </summary>
    private static Core.Abstractions.Progression.IProgressionStrategy ProgressionFor(IGameMode mode) =>
        mode switch
        {
            IFlowAwareMode => new FlowAwareProgressionStrategy(),
            IDiceProgressionMode dice =>
                new DiceCategoryProgressionStrategy(dice.CategoriesInOrder, dice.CategoryForTotal),
            _ => new DifficultyProgressionStrategy(),
        };

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<IGameController> CreateCardTurnAsync(
        IGameModeDefinition definition,
        IReadOnlyList<IPlayer> players,
        string modeName,
        int maxRounds,
        Core.Abstractions.Progression.IProgressionStrategy progression,
        GameplayOptions? gameplayOptions,
        Persistence.SessionSnapshot? resumeFrom,
        CancellationToken ct)
    {
        var controller = await CardTurnController.CreateAsync(
            definition: definition,
            players: players,
            modeName: modeName,
            maxRounds: maxRounds,
            progression: progression,
            options: new CardTurnControllerOptions
            {
                Gameplay = gameplayOptions,
                SessionRepository = _persistence,
                ResumeFrom = resumeFrom,
            },
            ct: ct)
            .ConfigureAwait(false);

        // CardTurnController is explicitly single-threaded (see ThreadingGuard.cs),
        // but ThreadingGuard.Enabled defaults off in Release, so nothing actually
        // stopped a caller that never marshals onto the owner thread from
        // corrupting state in the build that ships. SerializedCardTurnController
        // enforces serialized access unconditionally, at no behavioural cost to a
        // host (every UI head today) that already calls everything from one thread.
        return new SerializedCardTurnController(controller);
    }

    /// <inheritdoc />
    public Task<Persistence.SessionSnapshot?> LoadSavedSessionAsync(CancellationToken ct = default) =>
        _persistence is null
            ? Task.FromResult<Persistence.SessionSnapshot?>(null)
            : _persistence.LoadAsync(ct);

}

/// <summary>
/// JSON-backed game persistence. Alias for <see cref="JsonSessionRepository"/>
/// that uses the public <see cref="IGamePersistence"/> name. Prefer this name
/// in new code.
/// </summary>
public sealed class JsonGamePersistence : IGamePersistence
{
    private readonly JsonSessionRepository _inner;

    /// <summary>Initialises a new <see cref="JsonGamePersistence"/> instance.</summary>
    public JsonGamePersistence(string? filePath = null) =>
        _inner = new JsonSessionRepository(filePath);

    /// <inheritdoc />
    public bool HasSavedSession => _inner.HasSavedSession;
    /// <inheritdoc />
    public Task SaveAsync(SessionSnapshot snapshot, CancellationToken ct = default) => _inner.SaveAsync(snapshot, ct);
    /// <inheritdoc />
    public Task<SessionSnapshot?> LoadAsync(CancellationToken ct = default) => _inner.LoadAsync(ct);
    /// <inheritdoc />
    public Task DeleteAsync(CancellationToken ct = default) => _inner.DeleteAsync(ct);
}
