using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Presentation;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Rules;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Rules;

namespace TableTop.Games.Base;

/// <summary>
/// Abstract base for all card-per-turn game modes.
/// Provides the mode's identity, card catalogue, scoring, and rules.
/// Contains zero UI or console references — any host (Console, WPF, MAUI) can
/// consume it through <see cref="IGameModeDefinition"/>.
///
/// Replaces the old <c>BaseGameMode</c> which mixed card definitions with
/// a console-specific game loop.
/// </summary>
public abstract class BaseGameModeDefinition : IGameMode, IGameModeDefinition
{
    // ── IGameMode ─────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract string Description { get; }

    // ── Presentation ──────────────────────────────────────────────────────────
    //
    // This used to be loaded from each mode's .deck.json. Those files were
    // removed in 1.18.0 and the loader that read them in 1.19.0, so there is no
    // longer any source that can override a compiled-in value. Presentation is
    // therefore always None, and every Resolved* member below falls through to
    // the C# value.
    //
    // The members are kept rather than deleted because both heads, the shared
    // ViewModels and the public API snapshot bind to them. Collapsing them into
    // the plain Name/CompleteLabel/CategoryColours members they now just return
    // is a head-facing change and belongs in its own commit — see BACKLOG.md.

    /// <summary>
    /// Always <see cref="ModePresentation.None"/>. Kept so the Resolved*
    /// members below retain their shape for consumers. Never null.
    /// </summary>
    public ModePresentation Presentation => ModePresentation.None;

    // ── Resolved presentation ─────────────────────────────────────────────────
    //
    // WHY THESE EXIST ALONGSIDE Name, CompleteLabel AND FRIENDS
    // ─────────────────────────────────────────────────────────
    // Name and Description are abstract and the labels are virtual, so all 92
    // modes already override them. Teaching those members to consult JSON would
    // not work: a subclass override wins over any base implementation, so JSON
    // would be silently ignored on precisely the modes that bothered to set a
    // value. Making them non-virtual would break every mode in the catalogue.
    //
    // So the resolution happens here instead, in members no subclass overrides.
    // Hosts should read these; the originals remain as the compiled-in default
    // each one falls back to. JSON wins where it speaks, C# stands where it
    // doesn't, and a deck with no presentation block behaves exactly as before.

    /// <summary>Name to display: JSON title if set, otherwise <see cref="Name"/>.</summary>
    public string DisplayName => Presentation.Title ?? Name;

    /// <summary>Description to display: JSON if set, otherwise <see cref="Description"/>.</summary>
    public string DisplayDescription => Presentation.Description ?? Description;

    /// <summary>Primary action label: JSON if set, otherwise <see cref="CompleteLabel"/>.</summary>
    public string ResolvedCompleteLabel => Presentation.CompleteLabel ?? CompleteLabel;

    /// <summary>Secondary action label: JSON if set, otherwise <see cref="SkipLabel"/>.</summary>
    public string ResolvedSkipLabel => Presentation.SkipLabel ?? SkipLabel;

    /// <summary>Table minimum: JSON if set, otherwise <see cref="MinimumPlayers"/>.</summary>
    public int ResolvedMinimumPlayers => Presentation.MinimumPlayers ?? MinimumPlayers;

    /// <summary>Category colours: JSON if set, otherwise <see cref="CategoryColours"/>.</summary>
    public IReadOnlyDictionary<string, string> ResolvedCategoryColours =>
        Presentation.CategoryColours ?? CategoryColours;

    /// <summary>Categories pinned first: JSON if set, otherwise <see cref="CategoriesPinnedToStart"/>.</summary>
    public IReadOnlyList<string> ResolvedCategoriesPinnedToStart =>
        Presentation.CategoriesPinnedToStart ?? CategoriesPinnedToStart;

    /// <summary>Categories pinned last: JSON if set, otherwise <see cref="CategoriesPinnedToEnd"/>.</summary>
    public IReadOnlyList<string> ResolvedCategoriesPinnedToEnd =>
        Presentation.CategoriesPinnedToEnd ?? CategoriesPinnedToEnd;

    /// <summary>Colours and fonts from JSON, or null when the deck declares none.</summary>
    public ThemePalette? Theme => Presentation.Theme;

    // ── IGameModeDefinition ───────────────────────────────────────────────────

    /// <inheritdoc />
    public IReadOnlyList<ICard> GetCards(IReadOnlyList<IPlayer> players) =>
        BuildCards(players);

    /// <inheritdoc />
    public IScoringStrategy GetScoring() => BuildScoring();

    /// <inheritdoc />
    public IEnumerable<IRule> GetRules() => BuildRules();

    // ── Subclass hooks ────────────────────────────────────────────────────────

    /// <summary>Returns the full card catalogue for this mode.</summary>
    protected abstract IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players);

    /// <summary>Returns the scoring strategy for this mode.</summary>
    protected abstract IScoringStrategy BuildScoring();

    /// <summary>
    /// Returns the rules pipeline for this mode.
    /// Default: restriction check + no-duplicate + skip-player.
    /// </summary>
    protected virtual IEnumerable<IRule> BuildRules() =>
    [
        new RestrictionRule(),
        new NoDuplicateCardRule(),
        new SkipPlayerRule(),
    ];

    // ── UI hint properties ────────────────────────────────────────────────────
    // These are pure data — no console code. Hosts read them to customise
    // their rendering without needing a subclass per renderer.

    /// <summary>
    /// Short label describing what "completing" a card means in this mode.
    /// Shown by Console and WPF as the primary action button text.
    /// Default: "Completed".
    /// </summary>
    public virtual string CompleteLabel => "Completed";

    /// <summary>
    /// Short label for skipping a card.
    /// Default: "Skipped".
    /// </summary>
    public virtual string SkipLabel => "Skipped";

    /// <summary>
    /// Optional category-specific colour hint used by renderers.
    /// Key = category name, value = hex colour string (e.g. "#42A5F5").
    /// Renderers may ignore this; it is purely advisory.
    /// </summary>
    /// <inheritdoc />
    public virtual int MinimumPlayers => 2;

    /// <inheritdoc />
    public virtual IReadOnlyList<string> CategoriesPinnedToStart => [];

    /// <inheritdoc />
    public virtual IReadOnlyList<string> CategoriesPinnedToEnd => [];

    /// <inheritdoc />
    public virtual IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>();
}
