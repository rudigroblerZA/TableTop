using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Restrictions;

namespace TableTop.Core.Domain.Cards;

/// <summary>
/// A fluent DSL for authoring a mode's card bank in C#. <see cref="Card"/>
/// emits <see cref="StandardCard"/>s; <see cref="ThisOrThatCard"/> emits
/// two-option <see cref="TableTop.Core.Domain.Cards.ThisOrThatCard"/>s in the
/// same chain.
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
    private readonly HashSet<string> _lastCardRewrites = [];
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
        _lastCardRewrites.Clear();

        return this;
    }

    /// <summary>
    /// Adds one two-option comparison card
    /// (<see cref="TableTop.Core.Domain.Cards.ThisOrThatCard"/>) to the current
    /// category — the overload that makes this builder more than
    /// StandardCard-only. Same deterministic-id guarantee as <see cref="Card"/>,
    /// with the two option labels folded into the seed as well. A distinct name
    /// rather than a <see cref="Card"/> overload so a target-typed
    /// <c>new(...)</c> option argument stays unambiguous at the call site.
    /// </summary>
    /// <exception cref="InvalidOperationException">No <see cref="Category"/> has been set yet.</exception>
    public CardDeckBuilder ThisOrThatCard(
        string title,
        string question,
        ThisOrThatOption optionA,
        ThisOrThatOption optionB,
        Difficulty difficulty = Difficulty.Easy)
    {
        if (_currentCategory.Length == 0)
            throw new InvalidOperationException(
                $"Call {nameof(Category)}(...) before the first {nameof(ThisOrThatCard)}(...) — " +
                "every card needs one, and there is no sensible default to fall back to.");

        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(question);

        // ThisOrThatCard.Create already derives a stable id from
        // deck|category|title|body|labelA|labelB and validates the options.
        _cards.Add(TableTop.Core.Domain.Cards.ThisOrThatCard.Create(
            _deckName, title, question, difficulty, _currentCategory, optionA, optionB));

        return this;
    }

    /// <summary>
    /// Folds a declare-before-reveal gate into the card just added: its current
    /// <c>description</c> becomes the intro shown up front, and each
    /// <see cref="CardActionSet.AddButton"/> becomes a branch that stays hidden
    /// until the player picks it out loud — after which only that branch (plus
    /// any <see cref="CardActionSet.AddFooter"/> line) is shown.
    ///
    /// <para>
    /// This is <b>sugar over the existing text convention</b>, not a new card
    /// model. The composed body is exactly the shape
    /// <c>TableTop.Hosting.TruthOrDareCards</c> parses — an intro, then a
    /// <c>LABEL:</c> line per branch, then an optional <c>Chicken clause:</c>
    /// footer — so the shared gameplay screen renders the gate with no
    /// per-mode UI code. Because that parser currently recognises exactly the
    /// <c>Truth</c>/<c>Dare</c> pair, this method requires those two labels, in
    /// that order; widening it means teaching <c>TruthOrDareCards</c> the new
    /// markers first (and updating <see cref="GateLabels"/> here to match).
    /// </para>
    /// </summary>
    /// <exception cref="InvalidOperationException">No <see cref="Card"/> was added first, or actions were already applied to it.</exception>
    /// <exception cref="ArgumentException">The buttons are not the <c>Truth</c>/<c>Dare</c> pair in order.</exception>
    public CardDeckBuilder WithPreActions(Action<CardActionSet> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var set = new CardActionSet();
        configure(set);

        if (!set.Buttons.Select(b => b.Label).SequenceEqual(GateLabels, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException(
                $"{nameof(WithPreActions)} composes the shared declare-gate, which currently recognises " +
                $"only these buttons, in this order: {string.Join(", ", GateLabels)}. " +
                "Add exactly those, then widen TruthOrDareCards + GateLabels together to support more.",
                nameof(configure));

        ReplaceLastCardBody(current =>
        {
            var parts = new List<string> { current };
            parts.AddRange(set.Buttons.Select(b => $"{b.Label.ToUpperInvariant()}: {b.Text}"));
            if (set.Footer is not null)
                parts.Add($"Chicken clause: {set.Footer}");
            return string.Join("\n\n", parts);
        });

        return this;
    }

    /// <summary>
    /// Appends a reveal-after face to the card just added: one
    /// <see cref="CardActionSet.AddButton"/> whose label is a marker
    /// <c>TableTop.Hosting.CardFaces</c> splits onto the back
    /// (<c>Answer</c> or <c>The reading</c>), so a head that flips answer-bearing
    /// cards shows the question first and this text only after the flip.
    ///
    /// <para>
    /// Sugar over the same text convention: the body gains a trailing
    /// <c>Answer: …</c> (or <c>The reading: …</c>) line and nothing else. Kept
    /// in step with <c>CardFaces.BackMarkers</c> by <see cref="RevealLabels"/>
    /// and a cross-check test.
    /// </para>
    /// </summary>
    /// <exception cref="InvalidOperationException">No <see cref="Card"/> was added first, or actions were already applied to it.</exception>
    /// <exception cref="ArgumentException">Not exactly one button, its label is not a known reveal marker, or a footer was set.</exception>
    public CardDeckBuilder WithPostActions(Action<CardActionSet> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var set = new CardActionSet();
        configure(set);

        if (set.Buttons.Count != 1 || set.Footer is not null)
            throw new ArgumentException(
                $"{nameof(WithPostActions)} takes exactly one {nameof(CardActionSet.AddButton)} and no footer — " +
                "it appends a single reveal-after face.", nameof(configure));

        var (label, text) = set.Buttons[0];
        if (!RevealLabels.Contains(label, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException(
                $"'{label}' is not a reveal marker CardFaces splits on. Use one of: {string.Join(", ", RevealLabels)}.",
                nameof(configure));

        ReplaceLastCardBody(current => $"{current.TrimEnd()}\n\n{label}: {text}");
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

    /// <summary>
    /// Branch buttons <see cref="WithPreActions"/> accepts, in the order the
    /// composed body must list them. Mirrors the <c>TRUTH:</c>/<c>DARE:</c>
    /// markers <c>TableTop.Hosting.TruthOrDareCards</c> hard-codes — widen both
    /// together. <c>CardDeckBuilderTests</c> pins that they stay in step.
    /// </summary>
    private static readonly string[] GateLabels = ["Truth", "Dare"];

    /// <summary>
    /// Reveal markers <see cref="WithPostActions"/> accepts. Mirrors
    /// <c>TableTop.Hosting.CardFaces.BackMarkers</c> (minus the colon) — a
    /// cross-check test keeps the two lists identical.
    /// </summary>
    private static readonly string[] RevealLabels = ["Answer", "The reading"];

    /// <summary>
    /// Replaces the most recently added card with a copy whose body is
    /// <paramref name="rewrite"/> applied to the current body — re-deriving the
    /// id, since the text (and therefore the card's identity) has changed.
    /// At most one rewrite per card: <see cref="WithPreActions"/> and
    /// <see cref="WithPostActions"/> are mutually exclusive on a single card
    /// (a declare-gate and a flip-reveal on the same card would have their
    /// markers step on each other), and neither may be applied twice.
    /// </summary>
    private void ReplaceLastCardBody(Func<string, string> rewrite, [CallerMemberName] string caller = "")
    {
        if (_cards.Count == 0)
            throw new InvalidOperationException(
                $"Call {nameof(Card)}(...) before {caller}(...) — it modifies the card just added.");
        if (_cards[^1] is not StandardCard last)
            throw new InvalidOperationException(
                $"{caller}(...) only applies to a {nameof(Card)}(...) card, not {_cards[^1].GetType().Name}.");
        if (_lastCardRewrites.Count > 0)
            throw new InvalidOperationException(
                $"'{last.Title}' already has {string.Join("/", _lastCardRewrites)} folded in. " +
                $"{nameof(WithPreActions)} and {nameof(WithPostActions)} are one-per-card and mutually exclusive.");
        _lastCardRewrites.Add(caller);

        var body = rewrite(last.Description);
        _cards[^1] = new StandardCard(
            StableId(_deckName, last.Category, last.Title, body),
            last.Title, body, last.Difficulty, last.Category, last.Tags, last.Restriction);
    }
}

/// <summary>
/// Collects the branch/reveal buttons for a <see cref="CardDeckBuilder.WithPreActions"/>
/// or <see cref="CardDeckBuilder.WithPostActions"/> call. A button is a label
/// plus the text shown once it is chosen; <see cref="AddFooter"/> is the single
/// consequence line shown after any branch (Truth or Dare's chicken clause).
/// </summary>
public sealed class CardActionSet
{
    internal CardActionSet() { }

    internal List<(string Label, string Text)> Buttons { get; } = [];
    internal string? Footer { get; private set; }

    /// <summary>Adds a button. <paramref name="revealText"/> is what the player sees after picking it.</summary>
    public CardActionSet AddButton(string label, string revealText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(revealText);
        Buttons.Add((label.Trim(), revealText.Trim()));
        return this;
    }

    /// <summary>Sets the consequence line shown after a branch is chosen. Pre-actions only.</summary>
    public CardActionSet AddFooter(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        Footer = text.Trim();
        return this;
    }
}
