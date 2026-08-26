using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Decks;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Progression;

namespace TableTop.Core.Domain.Progression;

/// <summary>
/// Selects the next card in deck order regardless of difficulty or category.
/// </summary>
public sealed class LinearProgressionStrategy : IProgressionStrategy
{
    /// <inheritdoc />
    public string Name => "Linear";

    /// <inheritdoc />
    public Guid? SelectCandidate(IPlayer player, IDeck deck, IProgressionContext context) =>
        deck.Peek()?.Id;
}

/// <summary>
/// Selects a uniformly random card from the remaining deck.
/// Does NOT mutate the deck — returns only the ID of the chosen card.
/// </summary>
public sealed class RandomProgressionStrategy : IProgressionStrategy
{
    private readonly Random _random;

    /// <summary>Initialises a new <see cref="RandomProgressionStrategy"/> instance.</summary>
    public RandomProgressionStrategy() : this(Random.Shared) { }
    /// <summary>Initialises a new <see cref="RandomProgressionStrategy"/> instance.</summary>
    public RandomProgressionStrategy(Random random) =>
        _random = random ?? throw new ArgumentNullException(nameof(random));

    /// <inheritdoc />
    public string Name => "Random";

    /// <inheritdoc />
    public Guid? SelectCandidate(IPlayer player, IDeck deck, IProgressionContext context)
    {
        var cards = deck.Cards;
        if (cards.Count == 0) return null;
        return cards[_random.Next(cards.Count)].Id;
    }
}

/// <summary>
/// Advances through difficulty tiers: Easy → Medium → Hard → Extreme.
/// </summary>
public sealed class DifficultyProgressionStrategy : IProgressionStrategy
{
    private static readonly Difficulty[] _order =
    [
        Difficulty.Easy,
        Difficulty.Medium,
        Difficulty.Hard,
        Difficulty.Extreme
    ];

    /// <inheritdoc />
    public string Name => "DifficultyAscending";

    /// <inheritdoc />
    public Guid? SelectCandidate(IPlayer player, IDeck deck, IProgressionContext context)
    {
        var tierIndex        = Math.Min(context.Round / 3, _order.Length - 1);
        var targetDifficulty = _order[tierIndex];

        return (deck.Peek(c => c.Difficulty == targetDifficulty)
             ?? deck.Peek())?.Id;
    }
}

/// <summary>
/// Cycles through registered categories in order.
/// </summary>
public sealed class CategoryProgressionStrategy : IProgressionStrategy
{
    private readonly IReadOnlyList<string> _categories;
    private int _categoryIndex;

    /// <summary>Initialises a new <see cref="CategoryProgressionStrategy"/> instance.</summary>
    public CategoryProgressionStrategy(IEnumerable<string> categories)
    {
        ArgumentNullException.ThrowIfNull(categories);
        _categories = categories.ToList().AsReadOnly();
        if (_categories.Count == 0)
            throw new ArgumentException("At least one category is required.", nameof(categories));
    }

    /// <inheritdoc />
    public string Name => "Category";

    /// <inheritdoc />
    public Guid? SelectCandidate(IPlayer player, IDeck deck, IProgressionContext context)
    {
        for (var attempt = 0; attempt < _categories.Count; attempt++)
        {
            var category  = _categories[_categoryIndex % _categories.Count];
            _categoryIndex++;

            var candidate = deck.Peek(c =>
                string.Equals(c.Category, category, StringComparison.OrdinalIgnoreCase));
            if (candidate is not null) return candidate.Id;
        }
        return deck.Peek()?.Id; // Fallback to first available
    }
}

/// <summary>
/// Calibrates difficulty to the player's current standing relative to others.
/// </summary>
public sealed class ScoreBasedProgressionStrategy : IProgressionStrategy
{
    /// <inheritdoc />
    public string Name => "ScoreBased";

    /// <inheritdoc />
    public Guid? SelectCandidate(IPlayer player, IDeck deck, IProgressionContext context)
    {
        var allPlayers = context.Players;
        if (allPlayers.Count == 0) return deck.Peek()?.Id;

        var maxScore = allPlayers.Max(p => p.Score);
        var minScore = allPlayers.Min(p => p.Score);
        var range    = maxScore - minScore;

        var targetDifficulty = range == 0 || player.Score <= minScore
            ? Difficulty.Easy
            : player.Score >= maxScore
                ? Difficulty.Hard
                : Difficulty.Medium;

        return (deck.Peek(c => c.Difficulty == targetDifficulty)
             ?? deck.Peek())?.Id;
    }
}