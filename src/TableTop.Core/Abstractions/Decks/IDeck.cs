using TableTop.Core.Abstractions.Cards;

namespace TableTop.Core.Abstractions.Decks;

/// <summary>
/// Represents a collection of cards that can be filtered, shuffled, and drawn from.
/// </summary>
public interface IDeck
{
    /// <summary>Unique identifier for this deck.</summary>
    Guid Id { get; }

    /// <summary>Display name of the deck.</summary>
    string Name { get; }

    /// <summary>All cards currently in the deck.</summary>
    IReadOnlyList<ICard> Cards { get; }

    /// <summary>Number of cards remaining.</summary>
    int Count { get; }

    /// <summary>Returns true when no cards remain.</summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Draws the next card from the deck.
    /// </summary>
    /// <returns>The drawn card, or null when the deck is exhausted.</returns>
    ICard? Draw();

    /// <summary>
    /// Returns cards matching the supplied predicate without removing them.
    /// </summary>
    IReadOnlyList<ICard> Filter(Func<ICard, bool> predicate);

    /// <summary>Randomises the remaining card order using the provided strategy.</summary>
    void Shuffle(IShuffleStrategy strategy);

    /// <summary>Resets the deck to its original card order.</summary>
    void Reset();

    /// <summary>
    /// Returns the first card matching <paramref name="predicate"/> without removing it.
    /// Returns null when no match exists.
    /// Used by progression strategies to identify a candidate before the engine
    /// performs the definitive draw — separating selection from consumption.
    /// </summary>
    ICard? Peek(Func<ICard, bool>? predicate = null);

    /// <summary>
    /// Removes and returns the specific card with the given ID.
    /// Throws <see cref="InvalidOperationException"/> when the card is not present.
    /// Used by the engine to perform the single definitive draw after a candidate
    /// has been selected and validated.
    /// </summary>
    ICard DrawById(Guid cardId);

    /// <summary>
    /// Puts <paramref name="card"/> back at the FRONT of the remaining cards, so
    /// it is the next one drawn. Used when a turn is rewound (undo): the card
    /// dealt for the abandoned turn must go back rather than being silently lost.
    /// </summary>
    void Return(ICard card);
}
