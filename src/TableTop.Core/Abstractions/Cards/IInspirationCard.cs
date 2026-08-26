namespace TableTop.Core.Abstractions.Cards;

/// <summary>
/// A card that is saved to the drawing player's personal inspiration list
/// rather than played immediately. The player reads it and it is persisted
/// for them to return to later — outside the game session.
///
/// The controller detects <see cref="IInspirationCard"/> at draw time,
/// raises an <c>InspirationCardDrawnEvent</c>, saves the card to the
/// player's profile, and advances the turn automatically.
/// </summary>
public interface IInspirationCard : ICard
{
    /// <summary>
    /// A short call-to-action or reflection prompt.
    /// This is the text saved to the player's inspiration list.
    /// May differ from <see cref="ICard.Description"/> which is shown on the card face.
    /// </summary>
    string InspirationText { get; }

    /// <summary>
    /// Optional category tag for the inspiration (e.g. "Mindfulness", "Creativity", "Connection").
    /// </summary>
    string? InspirationCategory { get; }
}
