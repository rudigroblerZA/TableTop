using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;

namespace TableTop.Core.Domain.Game;

/// <summary>
/// Immutable record of a single completed turn.
/// Used by <see cref="SessionReport"/> to compute post-game statistics,
/// and by <c>UndoLastTurn</c> to reverse the most recent turn.
/// </summary>
public sealed record TurnRecord
{
    /// <summary>Sequential turn number (1-based across the whole session).</summary>
    public required int TurnNumber { get; init; }

    /// <summary>Round in which this turn occurred.</summary>
    public required int Round { get; init; }

    /// <summary>The player who played this turn.</summary>
    public required IPlayer Player { get; init; }

    /// <summary>The card that was played.</summary>
    public required ICard Card { get; init; }

    /// <summary>What the player did with the card.</summary>
    public required CardOutcome Outcome { get; init; }

    /// <summary>Points awarded (positive) or deducted (negative) this turn.</summary>
    public required int ScoreDelta { get; init; }

    /// <summary>Score of the player at the end of this turn (cumulative).</summary>
    public required int ScoreAfter { get; init; }

    /// <summary>
    /// How long the player took to answer, when the host provided timing.
    /// Null when no timer was used.
    /// </summary>
    public TimeSpan? Elapsed { get; init; }

    /// <summary>When this turn was recorded (UTC).</summary>
    public DateTimeOffset RecordedAt { get; init; } = DateTimeOffset.UtcNow;
}
