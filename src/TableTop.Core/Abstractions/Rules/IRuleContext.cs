using TableTop.Core.Abstractions.Decks;
using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Abstractions.Rules;

/// <summary>
/// Read-only snapshot of game state passed to rule evaluators.
/// Rules depend on this abstraction, not on concrete game state (DIP).
/// </summary>
public interface IRuleContext
{
    /// <summary>Current round number (1-based).</summary>
    int Round { get; }

    /// <summary>All players in the session.</summary>
    IReadOnlyList<IPlayer> Players { get; }

    /// <summary>The deck currently in use.</summary>
    IDeck Deck { get; }

    /// <summary>Typed game session metadata. Use this instead of the raw dictionary.</summary>
    TableTop.Core.Abstractions.Game.GameMetadata Metadata { get; }
}

/// <summary>
/// The outcome of evaluating a rule against a card and player.
/// </summary>
public sealed record RuleResult(
    bool IsAllowed,
    string? Reason = null,
    int ScoreDelta = 0
)
{
    /// <summary>Initialises a new <see cref="Allow"/> instance.</summary>
    public static RuleResult Allow(int scoreDelta = 0) =>
        new(true, null, scoreDelta);

    /// <summary>Initialises a new <see cref="Deny"/> instance.</summary>
    public static RuleResult Deny(string reason) =>
        new(false, reason, 0);
}