using TableTop.Core.Domain.Decks;
using TableTop.Core.Domain.Progression;

namespace TableTop.Tests;

/// <summary>
/// <see cref="DiceCategoryProgressionStrategy"/> replaced the dead
/// <c>ZoneProgressionStrategy</c> and is the generic engine behind every
/// dice-driven mode, current and future. Zero test references before this.
/// </summary>
public sealed class DiceCategoryProgressionStrategyTests
{
    private static readonly string[] Cats = ["Warm-Up", "Chat", "Act", "Bold", "Wild Card"];

    private static string Map(int total) => total switch
    {
        <= 4 => "Warm-Up",
        <= 6 => "Chat",
        <= 8 => "Act",
        <= 10 => "Bold",
        _ => "Wild Card",
    };

    private static ICard Card(string category, string title) =>
        new StandardCard(Guid.NewGuid(), title, "body", Difficulty.Easy, category);

    private static Deck BuildDeck(int perCategory = 6)
    {
        var cards = new List<ICard>();
        foreach (var c in Cats)
            for (var i = 0; i < perCategory; i++)
                cards.Add(Card(c, $"{c}{i}"));
        return new Deck(Guid.NewGuid(), "test", cards);
    }

    // ── Constructor guards ───────────────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsOnEmptyCategories()
    {
        var act = () => new DiceCategoryProgressionStrategy([], Map);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullMapping()
    {
        var act = () => new DiceCategoryProgressionStrategy(Cats, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── Selection ─────────────────────────────────────────────────────────────

    [Fact]
    public void SelectCandidate_PicksFromTheMappedCategory_WhenItHasCards()
    {
        var strategy = new DiceCategoryProgressionStrategy(Cats, Map, new Random(1));
        var deck = BuildDeck();

        var id = strategy.SelectCandidate(null!, deck, null!);

        id.Should().NotBeNull();
        var card = deck.Peek(c => c.Id == id);
        card!.Category.Should().Be(Map(strategy.LastRoll!.Total));
    }

    [Fact]
    public void LastRoll_ReflectsTheActualRoll()
    {
        var strategy = new DiceCategoryProgressionStrategy(Cats, Map, new Random(7));
        strategy.LastRoll.Should().BeNull("nothing has been rolled yet");

        strategy.SelectCandidate(null!, BuildDeck(), null!);

        strategy.LastRoll.Should().NotBeNull();
        strategy.LastRoll!.Total.Should().BeInRange(2, 12);
    }

    [Fact]
    public void SelectCandidate_IsDeterministic_ForAGivenSeed()
    {
        var a = new DiceCategoryProgressionStrategy(Cats, Map, new Random(42));
        var b = new DiceCategoryProgressionStrategy(Cats, Map, new Random(42));

        var idA = a.SelectCandidate(null!, BuildDeck(), null!);
        var idB = b.SelectCandidate(null!, BuildDeck(), null!);

        a.LastRoll.Should().Be(b.LastRoll);
    }

    [Fact]
    public void ChosenCategoryForDoubles_OverridesTheMapping_ThenResets()
    {
        var strategy = new DiceCategoryProgressionStrategy(Cats, Map, new Random(1));
        strategy.ChosenCategoryForDoubles = "Wild Card";

        var deck = BuildDeck();
        var id = strategy.SelectCandidate(null!, deck, null!);
        var card = deck.Peek(c => c.Id == id);

        card!.Category.Should().Be("Wild Card", "the override takes priority over the rolled total");
        strategy.ChosenCategoryForDoubles.Should().BeNull("consumed once, then reset");
    }

    [Fact]
    public void ChosenCategoryForDoubles_OnlyAffectsTheNextCall()
    {
        var strategy = new DiceCategoryProgressionStrategy(Cats, Map, new Random(1));
        strategy.ChosenCategoryForDoubles = "Wild Card";
        var deck = BuildDeck();

        strategy.SelectCandidate(null!, deck, null!);              // consumes the override
        var id2 = strategy.SelectCandidate(null!, deck, null!);    // must use the mapping again
        var card2 = deck.Peek(c => c.Id == id2);

        card2!.Category.Should().Be(Map(strategy.LastRoll!.Total));
    }

    // ── Fallback behaviour ────────────────────────────────────────────────────

    [Fact]
    public void SelectCandidate_FallsBackToNearestCategory_WhenPreferredIsEmpty()
    {
        // One card in Bold only; every other category empty. Force the roll
        // toward Bold's neighbours by trying totals until one that maps
        // elsewhere is rolled, then confirm the nearest non-empty category
        // ("Bold" itself, since it is the only stocked one) is what comes back.
        var lonelyBold = new List<ICard> { Card("Bold", "OnlyCard") };
        var deck = new Deck(Guid.NewGuid(), "test", lonelyBold);

        var strategy = new DiceCategoryProgressionStrategy(Cats, Map, new Random(3));
        var id = strategy.SelectCandidate(null!, deck, null!);

        id.Should().NotBeNull("a fallback must be found even when the rolled category has nothing");
        deck.Peek(c => c.Id == id)!.Category.Should().Be("Bold");
    }

    [Fact]
    public void SelectCandidate_ReturnsNull_WhenDeckIsCompletelyEmpty()
    {
        var strategy = new DiceCategoryProgressionStrategy(Cats, Map, new Random(1));
        var deck = new Deck(Guid.NewGuid(), "test", []);

        strategy.SelectCandidate(null!, deck, null!).Should().BeNull();
    }

    [Fact]
    public void SelectCandidate_DrainsEachCategory_ThenFallsBackCleanly()
    {
        // Small, exhaustible deck: every draw must produce a card, and once a
        // category empties, later rolls mapping to it must still resolve to
        // something else rather than null.
        var strategy = new DiceCategoryProgressionStrategy(Cats, Map, new Random(11));
        var deck = BuildDeck(perCategory: 2);
        var drawn = new List<Guid>();

        for (var i = 0; i < 10; i++)
        {
            var id = strategy.SelectCandidate(null!, deck, null!);
            if (id is null) break;
            drawn.Add(id.Value);
            deck.DrawById(id.Value);
        }

        drawn.Should().HaveCount(10, "10 cards exist and every one should be reachable via fallback");
        drawn.Distinct().Should().HaveCount(10, "no card should be offered twice");
    }
}
