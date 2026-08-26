using TableTop.Hosting.Persistence;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;
using SC = System.Console;
using CC = System.ConsoleColor;
using CK = System.ConsoleKey;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Hosting;
using TableTop.Games;

namespace TableTop.Console;

/// <summary>
/// Console entry point. Handles the session loop:
///   1. Seed default players on first run (Bob + Alice).
///   2. Player setup (loads saved profiles, allows edits).
///   3. Archetype selection via <see cref="ConsoleArchetypePicker"/>.
///   4. Game launch.
///   5. Play again prompt.
/// Zero game logic — only orchestration and renderer delegation.
/// </summary>
internal sealed class ConsoleGameLauncher
{
    /// <summary>
    /// The controller families the console app can render — the two arms of
    /// the switch in <see cref="Launch"/> that aren't the fallback.
    ///
    /// <para>
    /// Declared as data so the gap is inspectable rather than implicit. The
    /// console deliberately supports fewer families than the graphical heads;
    /// what matters is that the shortfall is stated somewhere a reader (or a
    /// test) can find, instead of being the absence of a switch arm.
    /// </para>
    /// </summary>
    internal static IReadOnlyList<ControllerFamily> SupportedFamilies { get; } =
    [
        ControllerFamily.CardTurn,
        ControllerFamily.Quiz,
        ControllerFamily.Monogamy,
        ControllerFamily.DailyCampaign,
        ControllerFamily.AreaControl,
        ControllerFamily.SimultaneousAnswer,
    ];

    private readonly IPlayerRepository  _repository;
    private readonly IControllerFactory _controllerFactory;

    public ConsoleGameLauncher(
        IPlayerRepository?  repository        = null,
        IControllerFactory? controllerFactory  = null)
    {
        _repository        = repository        ?? new JsonPlayerRepository();
        _controllerFactory = controllerFactory ?? new ControllerFactory();
        SeedDefaultsIfEmpty();
    }

    public void Run()
    {
        while (true)
        {
            ConsoleUi.Clear();
            ConsoleUi.Banner();

            var players = ConsolePlayerSetup.Run(_repository);

            // Build archetype registry (includes any JSON modes from ./modes/)
            var registry = BuildRegistry();
            var mode     = ConsoleArchetypePicker.Run(registry);
            if (mode is null) break;

            var suitability = TableSuitability.Check(mode, players);
            if (!suitability.Suits)
            {
                ConsoleUi.Clear();
                ConsoleUi.Banner();
                ConsoleUi.PrintError(suitability.Explanation!);
                SC.WriteLine();
                continue;
            }

            ConsoleUi.Clear();
            ConsoleUi.Banner();
            ConsoleUi.SectionHeader($"STARTING: {mode.Name.ToUpperInvariant()}");
            ConsoleUi.PrintMessage(mode.Description);
            SC.WriteLine();

            RunMode(mode, players);

            if (!ConsoleUi.PromptYesNo("\nPlay another game?")) break;
        }

        ConsoleUi.Clear();
        ConsoleUi.Banner();
        ConsoleUi.PrintMessage("Thanks for playing. Goodbye!");
    }

    // ── Registry construction ─────────────────────────────────────────────────

    private static IArchetypeRegistry BuildRegistry() => ArchetypeRegistry.Default();

    // This used to scan ./modes for user-supplied .json game modes, report each
    // unreadable file to the player, and hand the survivors to
    // ArchetypeRegistry.WithJsonModes. Runtime-loaded modes were removed in
    // 1.21.0 along with JsonGameMode, so the catalogue is now entirely compiled
    // in and there is nothing to scan or fail at.

    // ── Game dispatch ─────────────────────────────────────────────────────────

    private void RunMode(IGameMode mode, IReadOnlyList<IPlayer> players)
    {
        // Monogamy needs a token target before building the controller, so handle it first.
        // Capability dispatch, not a concrete-type check — matches how
        // ControllerFactory and ControllerFamilies pick this family, and
        // generalises to any future IMonogamyDeckProvider mode rather than
        // staying pinned to the one that exists today.
        if (mode is IMonogamyDeckProvider monogamyProvider)
        {
            var target     = ConsoleUi.PromptInt("Tokens to win?", 3, 30);
            var controller = new MonogamyController(
                players,
                monogamyProvider.GetDeck(),
                winningTokenCount: target);
            new ConsoleMonogamyRenderer(controller).RunBlocking();
            return;
        }

        // For card-turn modes, allow the user to choose rounds/progression before
        // delegating to ControllerFactory. Millionaire modes need no extra input.
        int maxRounds = mode is IGameModeDefinition ? ConsoleUi.PromptInt("How many rounds?", 1, 50) : 10;

        // Delegate controller creation to the injected factory — the single source of
        // truth for mode→controller mapping.
        var ctrl = _controllerFactory.CreateAsync(mode, players, maxRounds)
                                     .GetAwaiter().GetResult();

        switch (ctrl)
        {
            case IMillionaireController mill:
                new ConsoleMillionaireRenderer(mill).RunBlocking();
                break;

            // Flow-aware modes (school literacy) use the richer renderer.
            // The IFlowAwareMode marker is the single source of truth — no concrete types here.
            case ICardTurnController turn when mode is IFlowAwareMode:
                new ConsoleSchoolRenderer(turn, mode.Name).RunBlocking();
                break;

            case ICardTurnController turn:
                new ConsoleCardTurnRenderer(turn, mode.Name).RunBlocking();
                break;

            case IDayOneController day:
                new ConsoleDayOneRenderer(day).RunBlocking();
                break;

            case IClaimedController claimed:
                new ConsoleClaimedRenderer(claimed).RunBlocking();
                break;

            case IHerdController herd:
                new ConsoleHerdRenderer(herd).RunBlocking();
                break;

            // Every family the catalogue can produce has a renderer now
            // (backlog item 4) — this arm stays as a safety net for a future
            // family that ships a controller before Console's renderer for
            // it, not because any mode currently reaches it. It previously
            // fell off the end of the switch entirely and the launcher simply
            // returned, leaving the player at a menu with no indication
            // anything had happened — four modes behaved that way, two of
            // them for several versions, before this arm existed.
            default:
                SC.WriteLine();
                SC.ForegroundColor = CC.Yellow;
                SC.WriteLine($"  '{mode.Name}' needs a {ControllerFamilies.For(mode)} screen, "
                           + "which the console app doesn't have.");
                SC.ResetColor();
                SC.WriteLine();
                ctrl.Dispose();
                break;
        }
    }

    // ── First-run defaults ────────────────────────────────────────────────────

    private void SeedDefaultsIfEmpty()
    {
        var existing = _repository.LoadAsync().GetAwaiter().GetResult();
        if (existing.Count > 0) return;

        _repository.SaveAsync([
            new PlayerProfile
            {
                Id = Guid.NewGuid(), Name = "Bob",
                Gender = "male",   Age = 44,
                IsMarried = true,  IsCoupleMember = true,
            },
            new PlayerProfile
            {
                Id = Guid.NewGuid(), Name = "Alice",
                Gender = "female", Age = 39,
                IsMarried = true,  IsCoupleMember = true,
            },
        ]).GetAwaiter().GetResult();
    }
}
