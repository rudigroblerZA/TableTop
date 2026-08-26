using TableTop.Core.Abstractions.Cards;

namespace TableTop.Core.Domain.Decks;

/// <summary>
/// A simple card provider backed by an in-memory list.
/// Useful for testing, prototyping, and hardcoded card sets.
/// </summary>
public sealed class InMemoryCardProvider : ICardProvider
{
    private readonly IReadOnlyList<ICard> _cards;

    /// <summary>Initialises a new <see cref="InMemoryCardProvider"/> instance.</summary>
    public InMemoryCardProvider(IEnumerable<ICard> cards)
    {
        ArgumentNullException.ThrowIfNull(cards);
        _cards = cards.ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ICard>> GetCardsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_cards);
}