namespace TableTop.Core.Abstractions.Cards;

/// <summary>
/// A card that interrupts normal turn flow and resolves immediately without
/// requiring an outcome from the current player.
/// </summary>
public interface IBreakCard : ICard
{
    /// <summary>Who this break card affects.</summary>
    BreakScope Scope { get; }

    /// <summary>
    /// Optional engine effect applied automatically when the card is drawn.
    /// Null means purely narrative.
    /// </summary>
    BreakEffect? Effect { get; }

    /// <summary>
    /// The real-world activity this break card asks players to do.
    /// Null means the activity is described purely in <see cref="ICard.Description"/>.
    /// </summary>
    BreakActivity? Activity { get; }

    /// <summary>
    /// Optional suggested duration for this break in minutes.
    /// Null means no specific duration is enforced.
    /// </summary>
    int? DurationMinutes { get; }
}

/// <summary>Who a break card targets.</summary>
public enum BreakScope
{
    /// <summary>The card addresses all players in the session.</summary>
    AllPlayers,

    /// <summary>The card addresses only the player who drew it.</summary>
    CurrentPlayer,
}

/// <summary>
/// The real-world activity type of a break card.
/// Used by renderers to show an appropriate icon, timer, or instruction.
/// </summary>
public enum BreakActivity
{
    /// <summary>Take a bath — typically 15–30 minutes.</summary>
    Bath,

    /// <summary>Take a shower — typically 5–15 minutes.</summary>
    Shower,

    /// <summary>Give or receive a massage.</summary>
    Massage,

    /// <summary>Eat a snack or meal.</summary>
    Eat,

    /// <summary>Get a drink (water, tea, coffee, or otherwise).</summary>
    Drink,

    /// <summary>Rest, lie down, or simply breathe.</summary>
    Rest,

    /// <summary>Do some light exercise, stretch, or go for a walk.</summary>
    Exercise,

    /// <summary>Rotate seating positions.</summary>
    Rotate,

    /// <summary>A general group pause with no specific activity.</summary>
    GroupPause,

    /// <summary>The activity is described in the card text only.</summary>
    Custom,
}

/// <summary>
/// An optional automatic engine effect applied when a break card is drawn.
/// </summary>
public abstract record BreakEffect;

/// <summary>Purely narrative — the group acts on it themselves, no engine change.</summary>
public sealed record GroupBreakEffect(string Activity) : BreakEffect;

/// <summary>The current player's turn is automatically skipped.</summary>
public sealed record SkipTurnEffect : BreakEffect;

/// <summary>All players rotate seats.</summary>
public sealed record RotatePlayersEffect : BreakEffect;
