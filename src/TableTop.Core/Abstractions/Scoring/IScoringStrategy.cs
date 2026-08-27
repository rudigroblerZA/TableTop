using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Abstractions.Scoring;

/// <summary>
/// Calculates the score delta when a player completes (or skips) a card.
/// Swap implementations without touching core engine logic (OCP, DIP).
/// </summary>
public interface IScoringStrategy
{
    /// <summary>Display name of this scoring model.</summary>
    string Name { get; }

    /// <summary>
    /// Returns the score change for the player after completing a card.
    /// </summary>
    int CalculateScore(ICard card, IPlayer player, CardOutcome outcome, TimeSpan? elapsed = null);
}

/// <summary>Represents what a player did with their card.</summary>
public enum CardOutcome
{
    /// <summary>The player successfully completed the prompt.</summary>
    Completed,
    /// <summary>The player skipped this card; a penalty may apply.</summary>
    Skipped,
    /// <summary>The player attempted but failed to complete the prompt.</summary>
    Failed
}