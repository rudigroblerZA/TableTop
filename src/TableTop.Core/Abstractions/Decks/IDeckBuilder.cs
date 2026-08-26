using TableTop.Core.Abstractions.Cards;

namespace TableTop.Core.Abstractions.Decks;

/// <summary>
/// Builds a deck from one or more card sources, with optional filtering.
/// </summary>
public interface IDeckBuilder
{
    /// <summary>Add a card provider as a source.</summary>
    IDeckBuilder WithProvider(ICardProvider provider);

    /// <summary>Apply an additional filter to included cards.</summary>
    IDeckBuilder WithFilter(Func<ICard, bool> filter);

    /// <summary>Set the display name for the resulting deck.</summary>
    IDeckBuilder WithName(string name);

    /// <summary>Builds and returns the configured deck.</summary>
    Task<IDeck> BuildAsync(CancellationToken cancellationToken = default);
}
