namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Tracks the prize ladder for a hot-seat quiz session.
/// Encapsulates question tiers, safe-haven levels, and prize amounts.
/// </summary>
public interface IPrizeLadder
{
    /// <summary>All rungs, ordered from easiest (index 0) to hardest.</summary>
    IReadOnlyList<PrizeLadderRung> Rungs { get; }

    /// <summary>The rung corresponding to the current question (0-based index).</summary>
    int CurrentRungIndex { get; }

    /// <summary>The rung the player is currently on.</summary>
    PrizeLadderRung CurrentRung { get; }

    /// <summary>
    /// The prize banked if the player walks away or answers incorrectly.
    /// Equals the last safe-haven prize reached, or zero if none reached.
    /// </summary>
    long GuaranteedPrize { get; }

    /// <summary>Advances to the next rung after a correct answer.</summary>
    void Advance();

    /// <summary>True when the player has answered all questions correctly.</summary>
    bool IsComplete { get; }
}

/// <summary>A single rung on the prize ladder.</summary>
public sealed record PrizeLadderRung(
    int QuestionNumber,    // 1-based
    long PrizeAmount,
    bool IsSafeHaven
);
