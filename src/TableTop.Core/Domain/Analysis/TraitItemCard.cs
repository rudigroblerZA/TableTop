using TableTop.Core.Abstractions.Analysis;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Restrictions;
using TableTop.Core.Domain.Cards;

namespace TableTop.Core.Domain.Analysis;

/// <summary>
/// A statement a player agrees or disagrees with, weighted onto one or more
/// trait dimensions.
/// </summary>
public sealed class TraitItemCard : BaseCard, ITraitItemCard
{
    /// <summary>Initialises a new <see cref="TraitItemCard"/>.</summary>
    /// <param name="id">Unique card id.</param>
    /// <param name="statement">The statement shown to the player. Becomes <see cref="ICard.Description"/>.</param>
    /// <param name="traitWeights">Trait key → weight; sign is keying, magnitude is loading.</param>
    /// <param name="category">Thematic grouping, conventionally the primary trait's name.</param>
    /// <param name="difficulty">Difficulty rating; assessment items are conventionally <see cref="Difficulty.Easy"/>.</param>
    /// <param name="title">Short label. Defaults to the category when omitted.</param>
    /// <param name="tags">Optional tags.</param>
    /// <param name="restriction">Optional eligibility restriction.</param>
    /// <exception cref="ArgumentException"><paramref name="traitWeights"/> is empty, or any weight is zero or non-finite.</exception>
    public TraitItemCard(
        Guid id,
        string statement,
        IReadOnlyDictionary<string, double> traitWeights,
        string category,
        Difficulty difficulty = Difficulty.Easy,
        string? title = null,
        IEnumerable<string>? tags = null,
        IRestriction? restriction = null)
        : base(id, string.IsNullOrWhiteSpace(title) ? category : title,
               statement, difficulty, category, tags, restriction)
    {
        ArgumentNullException.ThrowIfNull(traitWeights);

        if (traitWeights.Count == 0)
            throw new ArgumentException(
                "A trait item must load on at least one dimension — an item with no " +
                "weights can never affect a profile and is dead content.",
                nameof(traitWeights));

        var copy = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, weight) in traitWeights)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(traitWeights));

            // A zero weight is almost always a typo for "reverse-keyed" — the
            // author reached for 0 meaning "counts against". It would silently
            // contribute nothing while still looking like a scored item in the
            // bank, so it is rejected rather than normalised away.
            if (weight == 0d || !double.IsFinite(weight))
                throw new ArgumentException(
                    $"Weight for trait '{key}' is {weight}. Weights must be finite and " +
                    "non-zero; use a negative weight for a reverse-keyed item.",
                    nameof(traitWeights));

            if (!copy.TryAdd(key, weight))
                throw new ArgumentException(
                    $"Duplicate trait key '{key}' in item weights (keys are compared " +
                    "case-insensitively).", nameof(traitWeights));
        }

        TraitWeights = copy;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, double> TraitWeights { get; }

    /// <summary>
    /// Convenience factory for the overwhelmingly common shape: one dimension,
    /// full loading, keyed either way.
    /// </summary>
    /// <param name="statement">The statement shown to the player.</param>
    /// <param name="traitKey">The single dimension this item loads on.</param>
    /// <param name="reverseKeyed">True when agreeing counts <i>against</i> the trait.</param>
    /// <param name="category">Thematic grouping. Defaults to <paramref name="traitKey"/>.</param>
    public static TraitItemCard Single(
        string statement,
        string traitKey,
        bool reverseKeyed = false,
        string? category = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(traitKey);

        return new TraitItemCard(
            Guid.NewGuid(),
            statement,
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                [traitKey] = reverseKeyed ? -1d : 1d,
            },
            string.IsNullOrWhiteSpace(category) ? traitKey : category);
    }
}
