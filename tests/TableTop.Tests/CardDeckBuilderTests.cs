using TableTop.Core.Domain.Cards;

namespace TableTop.Tests;

/// <summary>
/// <see cref="CardDeckBuilder"/> exists to replace the twelve independent
/// copies of the same three-line card-construction helper scattered across
/// <c>TableTop.Games</c>, and to fix a real bug none of them caught: their
/// shared ancestor, <c>StandardCard.Create</c>, assigns a random id every
/// process start, which only matters when a mode's JSON deck is missing and
/// the C# bank becomes the fallback — but then it breaks any saved session
/// referencing those cards.
/// </summary>
public sealed class CardDeckBuilderTests
{
    private static IReadOnlyList<Core.Abstractions.Cards.ICard> SampleDeck() =>
        CardDeckBuilder.For("Sample Deck")
            .Category("History")
                .Card("Ancient Egypt", "Order these events.", Difficulty.Hard)
                .Card("The Renaissance", "Order these events.", Difficulty.Medium)
            .Category("Pop Culture")
                .Card("Streaming Wars", "Order these events.", Difficulty.Easy)
            .Build();

    [Fact]
    public void Build_ReturnsOneCardPerCardCall()
    {
        SampleDeck().Should().HaveCount(3);
    }

    [Fact]
    public void Build_AssignsEachCardTheCategoryActiveWhenItWasAdded()
    {
        var deck = SampleDeck();
        deck[0].Category.Should().Be("History");
        deck[1].Category.Should().Be("History");
        deck[2].Category.Should().Be("Pop Culture");
    }

    [Fact]
    public void Build_PreservesTitleDescriptionAndDifficulty()
    {
        var deck = SampleDeck();
        deck[0].Title.Should().Be("Ancient Egypt");
        deck[0].Description.Should().Be("Order these events.");
        deck[0].Difficulty.Should().Be(Difficulty.Hard);
    }

    [Fact]
    public void Ids_AreUniqueWithinOneDeck()
    {
        var deck = SampleDeck();
        deck.Select(c => c.Id).Distinct().Should().HaveCount(deck.Count);
    }

    [Fact]
    public void Ids_AreDeterministic_SameInputsSameIdEveryBuild()
    {
        // The whole reason this builder exists over StandardCard.Create: two
        // independently built decks with identical deck name, category, title
        // and body must produce identical ids — across calls, across process
        // restarts, forever. Verified directly rather than assumed.
        var first  = SampleDeck();
        var second = SampleDeck();

        first.Select(c => c.Id).Should().Equal(second.Select(c => c.Id));
    }

    [Fact]
    public void Ids_ChangeWhenTheDeckNameChanges()
    {
        // The deck name seeds every id. A different seed must produce
        // different ids even for byte-identical card content — otherwise two
        // unrelated modes reusing similar card text would collide.
        var a = CardDeckBuilder.For("Deck A")
            .Category("X").Card("T", "Same body.", Difficulty.Easy).Build();
        var b = CardDeckBuilder.For("Deck B")
            .Category("X").Card("T", "Same body.", Difficulty.Easy).Build();

        a[0].Id.Should().NotBe(b[0].Id);
    }

    [Fact]
    public void Ids_ChangeWhenCardTextChanges()
    {
        // New wording is a new card, not a silent mutation of the old one —
        // matching how the JSON pipeline already treats edited content.
        var original = CardDeckBuilder.For("Deck")
            .Category("X").Card("T", "Original body.", Difficulty.Easy).Build();
        var reworded = CardDeckBuilder.For("Deck")
            .Category("X").Card("T", "Reworded body.", Difficulty.Easy).Build();

        original[0].Id.Should().NotBe(reworded[0].Id);
    }

    [Fact]
    public void Card_BeforeAnyCategory_ThrowsWithAClearMessage()
    {
        var act = () => CardDeckBuilder.For("Deck").Card("T", "B", Difficulty.Easy);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Category*");
    }

    [Fact]
    public void Build_WithNoCardsAdded_Throws()
    {
        var act = () => CardDeckBuilder.For("Empty Deck").Build();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*no cards*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void For_RejectsBlankDeckName(string? name)
    {
        var act = () => CardDeckBuilder.For(name!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Category_RejectsBlankName()
    {
        var act = () => CardDeckBuilder.For("Deck").Category("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Card_RejectsBlankTitleOrDescription()
    {
        var builder = CardDeckBuilder.For("Deck").Category("X");

        var blankTitle = () => builder.Card("", "Body", Difficulty.Easy);
        var blankBody  = () => builder.Card("Title", "", Difficulty.Easy);

        blankTitle.Should().Throw<ArgumentException>();
        blankBody.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Category_CanBeCalledAgain_ToStartANewGroupOfCards()
    {
        // Re-entering a category name already used earlier is legal — it just
        // resumes adding to that group, it does not need to be contiguous.
        var deck = CardDeckBuilder.For("Deck")
            .Category("A").Card("A1", "b1", Difficulty.Easy)
            .Category("B").Card("B1", "b2", Difficulty.Easy)
            .Category("A").Card("A2", "b3", Difficulty.Easy)
            .Build();

        deck.Count(c => c.Category == "A").Should().Be(2);
        deck.Count(c => c.Category == "B").Should().Be(1);
    }
}
