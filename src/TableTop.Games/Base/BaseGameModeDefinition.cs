using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Rules;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Rules;

namespace TableTop.Games.Base;

/// <summary>
/// Abstract base for all card-per-turn game modes.
/// Provides the mode's identity, card catalogue, scoring, and rules.
/// Contains zero UI or console references — any host (Console, WinUI, MAUI) can
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
    /// Shown by Console and WinUI as the primary action button text.
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
