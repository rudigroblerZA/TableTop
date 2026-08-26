namespace TableTop.Core.Abstractions.Cards;

/// <summary>
/// A card that grants a benefit or effect to the player who draws it.
/// The controller detects <see cref="IRewardCard"/> at draw time, applies
/// the <see cref="Effect"/> automatically (where possible), raises a
/// <c>RewardCardDrawnEvent</c>, and advances to the next turn.
/// No <c>RecordOutcome</c> call is needed.
/// </summary>
public interface IRewardCard : ICard
{
    /// <summary>The effect granted to the player who draws this card.</summary>
    RewardEffect Effect { get; }
}

/// <summary>
/// Base type for reward effects.
/// Extend the hierarchy to add new reward types without modifying existing code (OCP).
/// </summary>
public abstract record RewardEffect;

/// <summary>Adds a fixed number of points to the drawing player's score.</summary>
public sealed record ScoreBonusEffect(int Points) : RewardEffect;

/// <summary>Multiplies the drawing player's score by a factor.</summary>
public sealed record ScoreMultiplierEffect(double Multiplier) : RewardEffect;

/// <summary>
/// Transfers points from another player to the drawing player.
/// The target player is chosen by the drawing player at the table.
/// </summary>
public sealed record StealPointsEffect(int Points) : RewardEffect;

/// <summary>The drawing player may skip their next turn with no penalty.</summary>
public sealed record FreePassEffect : RewardEffect;

/// <summary>The drawing player immediately draws and plays an extra card.</summary>
public sealed record ExtraCardEffect : RewardEffect;

/// <summary>The drawing player may swap their next card with any other player.</summary>
public sealed record SwapCardEffect : RewardEffect;

/// <summary>
/// A purely narrative reward — the group decides what it means.
/// The engine raises the event but applies no automatic score change.
/// </summary>
public sealed record NarrativeRewardEffect(string Description) : RewardEffect;

/// <summary>
/// A drink penalty — the drawing player (or their partner) must take a drink.
/// No engine score change; purely physical/social.
/// </summary>
public sealed record DrinkPenaltyEffect(string DrinkDescription = "Take a drink") : RewardEffect;

/// <summary>
/// A timed massage reward. The drawing player receives a massage of the specified duration.
/// The controller starts the countdown timer if a renderer is attached.
/// </summary>
public sealed record TimedMassageEffect(int DurationMinutes, string Target = "partner") : RewardEffect;
