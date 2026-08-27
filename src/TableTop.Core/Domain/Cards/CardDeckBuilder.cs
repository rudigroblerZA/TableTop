using System.Security.Cryptography;
using System.Text;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Restrictions;

namespace TableTop.Core.Domain.Cards;

/// <summary>
/// A fluent DSL for authoring a mode's card bank in C#.
///
/// <para>
/// <b>Why this exists.</b> Twelve mode files independently define the same
/// three-line local helper — <c>private static ICard C(category, title, body,
/// difficulty) =&gt; StandardCard.Create(...)</c> — because there was nowhere
/// shared to put it. This is that shared place, plus one correctness fix none
/// of the twelve had: stable ids by default.
/// </para>
///
/// <para>
/// <b>The id problem this fixes.</b> <see cref="StandardCard.Create"/> uses
/// <see cref="Guid.NewGuid"/> — a fresh random id every process start. For a
/// mode with a JSON deck this is masked, because loading is JSON-first and the
/// C# bank is only a fallback. But if that JSON ever goes missing, the
/// fallback's ids change on every restart, and any saved session referencing
/// those cards becomes unresolvable. <c>ClaimedController</c>'s card
/// bank hit this deliberately and worked around it with a hand-rolled hash;
/// this builder makes that the default instead of something eleven other
/// files would each have to reinvent correctly.
/// </para>
///
/// <para>
/// Ids are derived from <c>deckName|category|title|description</c>, so the
/// same card text always produces the same id, across restarts and rebuilds,
/// without the author doing anything. Changing a card's wording changes its
/// id — treated as a new card, not silent mutation of an old one, matching
/// how the JSON pipeline already behaves.
/// </para>
///
/// <code>
/// IReadOnlyList&lt;ICard&gt; cards = CardDeckBuilder
///     .For("Chronology Challenge")
///     .Category("History")
///         .Card("Ancient Egypt", "Order these events...", Difficulty.Hard)
///         .Card("The Renaissance", "Order these events...", Difficulty.Medium)
///     .Category("Pop Culture")
///         .Card("Streaming Wars", "Order these events...", Difficulty.Easy)
///     .Build();
/// </code>
/// </summary>
public sealed class CardDeckBuilder
{
    private readonly string _deckName;
    private readonly List<ICard> _cards = [];
    private string _currentCategory = "";

    private CardDeckBuilder(string deckName) => _deckName = deckName;

    /// <summary>
    /// Starts a new deck. <paramref name="deckName"/> seeds every card's id —
    /// pick something that won't change (the mode's name is the natural
    /// choice), since changing it silently reassigns every card's identity.
    /// </summary>
    public static CardDeckBuilder For(string deckName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deckName);
        return new CardDeckBuilder(deckName);
    }

    /// <summary>
    /// Sets the category every subsequent <see cref="Card"/> belongs to, until
    /// the next <see cref="Category"/> call. Required before the first card.
    /// </summary>
    public CardDeckBuilder Category(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _currentCategory = name;
        return this;
    }

    /// <summary>Adds one card to the current category.</summary>
    /// <exception cref="InvalidOperationException">No <see cref="Category"/> has been set yet.</exception>
    public CardDeckBuilder Card(
        string title,
        string description,
        Difficulty difficulty,
        IEnumerable<string>? tags = null,
        IRestriction? restriction = null)
    {
        if (_currentCategory.Length == 0)
            throw new InvalidOperationException(
                $"Call {nameof(Category)}(...) before the first {nameof(Card)}(...) — " +
                "every card needs one, and there is no sensible default to fall back to.");

        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        _cards.Add(new StandardCard(
            StableId(_deckName, _currentCategory, title, description),
            title, description, difficulty, _currentCategory, tags, restriction));

        return this;
    }

    /// <summary>Finishes authoring and returns the deck.</summary>
    /// <exception cref="InvalidOperationException">No cards were added.</exception>
    public IReadOnlyList<ICard> Build()
    {
        if (_cards.Count == 0)
            throw new InvalidOperationException(
                $"'{_deckName}' has no cards — a mode shipping an empty deck is " +
                "almost certainly a mistake, not an intentional empty bank.");

        return _cards.AsReadOnly();
    }

    /// <summary>
    /// Deterministic id from the deck name, category, title and body. Same
    /// inputs, same id — every time, every process, forever. This is the same
    /// technique <c>ClaimedCardBank</c> used by hand; centralising it here is
    /// the whole point.
    /// </summary>
    private static Guid StableId(string deckName, string category, string title, string description) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"{deckName}|{category}|{title}|{description}"))[..16]);
}
