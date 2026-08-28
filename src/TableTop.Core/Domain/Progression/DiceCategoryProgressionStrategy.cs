using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Decks;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Progression;

namespace TableTop.Core.Domain.Progression;

/// <summary>
/// Rolls two dice each turn and selects a card from the category the total
/// maps to. Doubles offer the drawing player a free choice of category.
///
/// <para>
/// Generalised from Monogamy's dice mechanic, which lived as a hardcoded
/// <c>MonogamyZone</c> mapping directly on <see cref="DiceRoll"/> — meaning no
/// other mode could roll dice for category selection without pulling in an
/// enum that had nothing to do with it. This strategy takes the category set
/// and the total-to-category mapping as constructor arguments instead, so any
/// mode with categorised cards can use it. <c>MonogamyController</c> still
/// rolls its own dice inline rather than through this — it predates this
/// abstraction and has its own token/turn bookkeeping this strategy doesn't
/// carry — but nothing stops a future refactor from adopting it.
/// </para>
///
/// <para>
/// Falls back to the nearest category by "distance" in the supplied ordering
/// if the rolled one has no cards left, then to any card at all, so the game
/// never stalls on an empty category.
/// </para>
/// </summary>
public sealed class DiceCategoryProgressionStrategy : IProgressionStrategy
{
    private readonly Random _rng;
    private readonly IReadOnlyCollection<string> _categoriesInOrder;
    private readonly Func<int, string> _totalToCategory;
    private string? _overrideCategory;

    /// <summary>
    /// Builds a strategy over the given categories.
    /// </summary>
    /// <param name="categoriesInOrder">
    /// Every category this mode uses, in "distance" order — adjacent entries
    /// are what a rolled-but-empty category falls back to first. Also the
    /// menu offered on doubles.
    /// </param>
    /// <param name="totalToCategory">Maps a dice total (2–12) to one of <paramref name="categoriesInOrder"/>.</param>
    /// <param name="rng">Random source; omit to use <see cref="Random.Shared"/>.</param>
    public DiceCategoryProgressionStrategy(
        IReadOnlyList<string> categoriesInOrder,
        Func<int, string> totalToCategory,
        Random? rng = null)
    {
        ArgumentNullException.ThrowIfNull(totalToCategory);
        if (categoriesInOrder is null || categoriesInOrder.Count == 0)
            throw new ArgumentException("At least one category is required.", nameof(categoriesInOrder));

        _categoriesInOrder = categoriesInOrder;
        _totalToCategory = totalToCategory;
        _rng = rng ?? Random.Shared;
    }

    /// <summary>The dice result from the most recent <see cref="SelectCandidate"/> call.</summary>
    public DiceRoll? LastRoll { get; private set; }

    /// <summary>
    /// When the last roll was doubles, set this before the next
    /// <see cref="SelectCandidate"/> call to override which category is drawn
    /// from. Consumed once, then reset to null.
    /// </summary>
    public string? ChosenCategoryForDoubles
    {
        get => _overrideCategory;
        set => _overrideCategory = value;
    }

    /// <inheritdoc />
    public string Name => "DiceCategory";

    /// <inheritdoc />
    public Guid? SelectCandidate(IPlayer player, IDeck deck, IProgressionContext context)
    {
        var roll = DiceRoll.Roll(_rng);
        LastRoll = roll;

        var target = _overrideCategory ?? _totalToCategory(roll.Total);
        _overrideCategory = null;

        var candidate = deck.Peek(c => string.Equals(c.Category, target, StringComparison.OrdinalIgnoreCase))
                     ?? PeekNearestCategory(deck, target)
                     ?? deck.Peek();

        return candidate?.Id;
    }

    private ICard? PeekNearestCategory(IDeck deck, string preferred)
    {
        var preferredIndex = _categoriesInOrder
            .Select((c, i) => (c, i))
            .FirstOrDefault(x => string.Equals(x.c, preferred, StringComparison.OrdinalIgnoreCase))
            .i;

        var byDistance = _categoriesInOrder
            .Select((c, i) => (c, distance: Math.Abs(i - preferredIndex)))
            .OrderBy(x => x.distance)
            .Skip(1) // the preferred category itself already failed
            .Select(x => x.c);

        foreach (var category in byDistance)
        {
            var card = deck.Peek(c => string.Equals(c.Category, category, StringComparison.OrdinalIgnoreCase));
            if (card is not null) return card;
        }
        return null;
    }
}
