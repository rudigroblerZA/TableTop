using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Decks;

namespace TableTop.Core.Domain.Decks;

/// <summary>
/// Builds a <see cref="Deck"/> by composing multiple <see cref="ICardProvider"/> sources
/// and applying optional filters.
/// </summary>
public sealed class DeckBuilder : IDeckBuilder
{
    private readonly List<ICardProvider> _providers = [];
    private readonly List<Func<ICard, bool>> _filters = [];
    private string _name = "Deck";

    /// <inheritdoc />
    public IDeckBuilder WithProvider(ICardProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _providers.Add(provider);
        return this;
    }

    /// <inheritdoc />
    public IDeckBuilder WithFilter(Func<ICard, bool> filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        _filters.Add(filter);
        return this;
    }

    /// <inheritdoc />
    public IDeckBuilder WithName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _name = name;
        return this;
    }

    /// <inheritdoc />
    public async Task<IDeck> BuildAsync(CancellationToken cancellationToken = default)
    {
        var allCards = new List<ICard>();

        foreach (var provider in _providers)
        {
            var cards = await provider.GetCardsAsync(cancellationToken);
            allCards.AddRange(cards);
        }

        // Apply all filters (conjunction)
        IEnumerable<ICard> filtered = allCards;
        foreach (var filter in _filters)
            filtered = filtered.Where(filter);

        return new Deck(Guid.NewGuid(), _name, filtered);
    }
}
