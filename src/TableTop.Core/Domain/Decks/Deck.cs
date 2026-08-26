using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Decks;

namespace TableTop.Core.Domain.Decks;

/// <summary>
/// Default in-memory deck implementation.
/// </summary>
public sealed class Deck : IDeck
{
    private readonly List<ICard> _original;
    private readonly Queue<ICard> _remaining;

    /// <summary>Initialises a new <see cref="Deck"/> instance.</summary>
    public Deck(Guid id, string name, IEnumerable<ICard> cards)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(cards);

        Id = id;
        Name = name;
        _original = cards.ToList();
        _remaining = new Queue<ICard>(_original);
    }

    /// <inheritdoc />
    public Guid Id { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public IReadOnlyList<ICard> Cards => _remaining.ToList().AsReadOnly();

    /// <inheritdoc />
    public int Count => _remaining.Count;

    /// <inheritdoc />
    public bool IsEmpty => _remaining.Count == 0;

    /// <inheritdoc />
    public ICard? Draw() =>
        _remaining.TryDequeue(out var card) ? card : null;

    /// <inheritdoc />
    public IReadOnlyList<ICard> Filter(Func<ICard, bool> predicate) =>
        _remaining.Where(predicate).ToList().AsReadOnly();

    /// <inheritdoc />
    public void Shuffle(IShuffleStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        var shuffled = strategy.Shuffle(_remaining.ToList());
        _remaining.Clear();
        foreach (var card in shuffled)
            _remaining.Enqueue(card);
    }

    /// <inheritdoc />
    public ICard? Peek(Func<ICard, bool>? predicate = null) =>
        predicate is null
            ? _remaining.TryPeek(out var first) ? first : null
            : _remaining.FirstOrDefault(predicate);

    /// <inheritdoc />
    public void Return(ICard card)
    {
        ArgumentNullException.ThrowIfNull(card);

        // Queue has no push-front, so rebuild with the card at the head.
        var rest = _remaining.ToList();
        _remaining.Clear();
        _remaining.Enqueue(card);
        foreach (var c in rest)
            _remaining.Enqueue(c);
    }

    /// <inheritdoc />
    public ICard DrawById(Guid cardId)
    {
        // Rebuild the queue without the target card, returning the target.
        // O(n) but correct and avoids card loss during search.
        var cards = _remaining.ToList();
        var target = cards.FirstOrDefault(c => c.Id == cardId)
            ?? throw new InvalidOperationException(
                $"Card {cardId} is not present in deck '{Name}'.");

        _remaining.Clear();
        foreach (var c in cards)
            if (c.Id != cardId)
                _remaining.Enqueue(c);

        return target;
    }

    /// <inheritdoc />
    public void Reset()
    {
        _remaining.Clear();
        foreach (var card in _original)
            _remaining.Enqueue(card);
    }
}