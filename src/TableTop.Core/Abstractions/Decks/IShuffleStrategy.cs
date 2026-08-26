using TableTop.Core.Abstractions.Cards;

namespace TableTop.Core.Abstractions.Decks;

/// <summary>
/// Strategy for shuffling a list of cards.
/// Implement to provide deterministic, seeded, or weighted shuffle behaviour (OCP).
/// </summary>
public interface IShuffleStrategy
{
    /// <summary>Returns a new ordering of the provided cards.</summary>
    IReadOnlyList<ICard> Shuffle(IReadOnlyList<ICard> cards);
}
