using TableTop.Core.Abstractions;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Decks;

namespace TableTop.Core.Domain.Decks;

/// <summary>
/// Implements the Fisher-Yates shuffle algorithm for an unbiased random ordering.
/// Accepts <see cref="IRandomSource"/> for full determinism when seeded.
/// </summary>
public sealed class FisherYatesShuffleStrategy : IShuffleStrategy
{
    private readonly IRandomSource _rng;

    /// <summary>Default constructor — uses <see cref="SharedRandomSource"/> (non-deterministic).</summary>
    public FisherYatesShuffleStrategy() : this(SharedRandomSource.Instance) { }

    /// <summary>Seeded constructor — pass a <see cref="SeededRandomSource"/> for reproducible shuffles.</summary>
    public FisherYatesShuffleStrategy(IRandomSource rng) =>
        _rng = rng ?? throw new ArgumentNullException(nameof(rng));

    /// <summary>Backward-compat overload for code that passes a raw <see cref="Random"/>.</summary>
    public FisherYatesShuffleStrategy(Random random)
        : this(new LegacyRandomSource(random)) { }

    /// <inheritdoc />
    public IReadOnlyList<ICard> Shuffle(IReadOnlyList<ICard> cards)
    {
        ArgumentNullException.ThrowIfNull(cards);
        var list = cards.ToList();
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list.AsReadOnly();
    }
}

/// <summary>Adapts a raw <see cref="Random"/> to <see cref="IRandomSource"/> for backward compat.</summary>
internal sealed class LegacyRandomSource : IRandomSource
{
    private readonly Random _r;
    public LegacyRandomSource(Random r) => _r = r;
    /// <inheritdoc />
    public int Next(int maxValue) => _r.Next(maxValue);
    /// <inheritdoc />
    public int Next(int minValue, int maxValue) => _r.Next(minValue, maxValue);
    /// <inheritdoc />
    public int Next() => _r.Next();
    /// <inheritdoc />
    public double NextDouble() => _r.NextDouble();
    /// <inheritdoc />
    public void NextBytes(Span<byte> buffer) => _r.NextBytes(buffer);
}