using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Decks;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Decks;

namespace TableTop.Hosting.Controllers.Services;

/// <summary>
/// Turns a mode definition plus <see cref="GameplayOptions"/> into the deck a
/// session will actually be dealt from: difficulty filter, shuffle, pinned
/// categories, and the optional per-player card cap.
///
/// Extracted from <see cref="CardTurnController"/>'s constructor (backlog B.1).
/// This is the single source of truth for deck construction — <c>CreateAsync</c>
/// used to build a separate throwaway deck of its own, which meant
/// <see cref="GameplayOptions"/> could reach it and never affect a dealt card.
/// Keeping the logic in one place is what stops that recurring.
/// </summary>
internal static class SessionDeckFactory
{
    /// <summary>
    /// Builds the session deck without blocking (backlog B.3). This is the path
    /// <see cref="CardTurnController.CreateAsync"/> uses.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The configured difficulty range excludes every card in the mode.
    /// </exception>
    public static async Task<IDeck> BuildAsync(
        IGameModeDefinition    definition,
        IReadOnlyList<IPlayer> players,
        string                 modeName,
        GameplayOptions        options,
        CancellationToken      ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var builder = PrepareBuilder(definition, players, modeName, options);
        var deck    = await builder.BuildAsync(ct).ConfigureAwait(false);

        return Finish(deck, definition, players, modeName, options);
    }

    /// <summary>
    /// Blocking deck build, kept for the public <see cref="CardTurnController"/>
    /// constructor, which is synchronous and part of the shipped API.
    ///
    /// <c>DeckBuilder.BuildAsync</c> completes synchronously today, so this is
    /// safe — but it is sync-over-async, and it stops being safe the moment a
    /// card provider does real I/O. Prefer <see cref="BuildAsync"/>, which
    /// <c>CreateAsync</c> now uses, and treat this as the legacy path.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The configured difficulty range excludes every card in the mode.
    /// </exception>
    public static IDeck Build(
        IGameModeDefinition    definition,
        IReadOnlyList<IPlayer> players,
        string                 modeName,
        GameplayOptions        options)
    {
        var builder = PrepareBuilder(definition, players, modeName, options);
        var deck    = builder.BuildAsync().GetAwaiter().GetResult();

        return Finish(deck, definition, players, modeName, options);
    }

    private static IDeckBuilder PrepareBuilder(
        IGameModeDefinition    definition,
        IReadOnlyList<IPlayer> players,
        string                 modeName,
        GameplayOptions        options)
    {
        var cards    = definition.GetCards(players);
        var provider = new InMemoryCardProvider(cards);

        var builder = new DeckBuilder().WithName(modeName).WithProvider(provider);
        if (options.MinDifficulty != Difficulty.Easy || options.MaxDifficulty != Difficulty.Extreme)
            builder = builder.WithFilter(c => c.Difficulty >= options.MinDifficulty && c.Difficulty <= options.MaxDifficulty);

        return builder;
    }

    /// <summary>Shuffle, pinning and capping — identical for both entry points.</summary>
    private static IDeck Finish(
        IDeck                  deck,
        IGameModeDefinition    definition,
        IReadOnlyList<IPlayer> players,
        string                 modeName,
        GameplayOptions        options)
    {
        if (deck.Count == 0)
            throw new InvalidOperationException(
                $"The difficulty range {options.MinDifficulty}–{options.MaxDifficulty} excludes every card in " +
                $"'{modeName}'. Widen the range in Settings and try again.");

        // Remember the authored order BEFORE shuffling. Pinned categories are
        // restored to it below: a results key whose own cards are shuffled
        // means "How to read your result" can land after the type it explains,
        // which is worse than not pinning at all.
        var authoredOrder = deck.Cards
            .Select((card, index) => (card.Id, index))
            .ToDictionary(x => x.Id, x => x.index);

        if (options.ShuffleDeck)
            deck.Shuffle(new FisherYatesShuffleStrategy());

        // Some categories carry meaning through their POSITION, so they are
        // restored after the shuffle rather than being left where chance put
        // them. Relative order within each group is preserved.
        deck = ApplyPinnedCategories(deck, definition, authoredOrder);

        if (options.CardsPerPlayer is int perPlayer and > 0)
        {
            var cap = Math.Min(deck.Count, perPlayer * Math.Max(1, players.Count));
            deck = new Deck(deck.Id, deck.Name, deck.Cards.Take(cap));
        }

        return deck;
    }

    /// <summary>
    /// Moves pinned categories back to the ends of the deck after shuffling.
    /// A no-op for the overwhelming majority of modes, which pin nothing.
    /// </summary>
    private static IDeck ApplyPinnedCategories(
        IDeck deck, IGameModeDefinition definition, IReadOnlyDictionary<Guid, int> authoredOrder)
    {
        var first = definition.CategoriesPinnedToStart;
        var last  = definition.CategoriesPinnedToEnd;
        if (first.Count == 0 && last.Count == 0) return deck;

        bool In(IReadOnlyList<string> set, ICard c) =>
            c.Category is not null &&
            set.Contains(c.Category, StringComparer.OrdinalIgnoreCase);

        // Pinned groups go back to their AUTHORED order; the middle keeps
        // whatever order the shuffle produced.
        int Authored(ICard c) => authoredOrder.TryGetValue(c.Id, out var i) ? i : int.MaxValue;

        var ordered = deck.Cards.Where(c => In(first, c)).OrderBy(Authored)
            .Concat(deck.Cards.Where(c => !In(first, c) && !In(last, c)))
            .Concat(deck.Cards.Where(c => In(last, c)).OrderBy(Authored))
            .ToList();

        return new Deck(deck.Id, deck.Name, ordered);
    }
}
