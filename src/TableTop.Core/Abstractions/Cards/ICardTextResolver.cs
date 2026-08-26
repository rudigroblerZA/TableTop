using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Abstractions.Cards;

/// <summary>
/// Resolves the display text for a card at draw time, given the current player.
/// Implement this to produce gender-directed, localised, or AI-generated card text
/// without touching existing card or engine code (OCP).
/// </summary>
public interface ICardTextResolver
{
    /// <summary>
    /// Returns the text that should be shown to <paramref name="player"/> for this card.
    /// </summary>
    string Resolve(IPlayer player);
}
