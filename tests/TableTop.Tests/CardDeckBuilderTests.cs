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
        var first = SampleDeck();
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
        var blankBody = () => builder.Card("Title", "", Difficulty.Easy);

        blankTitle.Should().Throw<ArgumentException>();
        blankBody.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ThisOrThatCard_EmitsATwoOptionCard_InTheCurrentCategory()
    {
        var deck = CardDeckBuilder.For("Deck")
            .Category("Choices")
            .ThisOrThatCard("Morning", "Which morning?",
                new("Sunrise walk", "img-a", "detail a"),
                new("Lie-in", "img-b", "detail b"))
            .Build();

        deck.Should().HaveCount(1);
        var card = deck[0].Should().BeAssignableTo<Core.Abstractions.Cards.IThisOrThatCard>().Subject;
        card.Category.Should().Be("Choices");
        card.Title.Should().Be("Morning");
        card.Description.Should().Be("Which morning?");
        card.OptionA.Label.Should().Be("Sunrise walk");
        card.OptionB.ImageKey.Should().Be("img-b");
    }

    [Fact]
    public void ThisOrThatCard_MixesFreelyWithStandardCards_AndKeepsDeterministicIds()
    {
        static IReadOnlyList<Core.Abstractions.Cards.ICard> Deck() =>
            CardDeckBuilder.For("Mixed")
                .Category("Rules").Card("How To", "Body.", Difficulty.Easy)
                .Category("Play").ThisOrThatCard("A vs B", "Pick.",
                    new("A", Detail: "a"), new("B", Detail: "b"))
                .Build();

        Deck().Select(c => c.Id).Should().Equal(Deck().Select(c => c.Id));
    }

    [Fact]
    public void ThisOrThatCard_BeforeAnyCategory_Throws()
    {
        var act = () => CardDeckBuilder.For("Deck")
            .ThisOrThatCard("T", "Q", new("A"), new("B"));

        act.Should().Throw<InvalidOperationException>().WithMessage("*Category*");
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

    // ── WithPreActions / WithPostActions ─────────────────────────────────────
    //
    // These are sugar: the composed body must be exactly what
    // TableTop.Hosting.TruthOrDareCards and CardFaces already parse. Every test
    // here round-trips through those parsers rather than asserting on raw text,
    // so the day one of the conventions changes shape, these fail.

    private static Core.Abstractions.Cards.ICard GatedCard(string footer = "you owe the group a round of applause") =>
        CardDeckBuilder.For("Deck")
            .Category("Classics")
            .Card("Truth or Dare", "The reader asks: \"Truth or dare?\" — declare before hearing either.", Difficulty.Easy)
            .WithPreActions(a => a
                .AddButton("Truth", "What's the most embarrassing thing that happened to you as a kid?")
                .AddButton("Dare", "Do your best impression of another player.")
                .AddFooter(footer))
            .Build()[0];

    [Fact]
    public void WithPreActions_ComposesABodyTruthOrDareCardsParses()
    {
        var parsed = TruthOrDareCards.TryParse(GatedCard().Description);

        parsed.Should().NotBeNull();
        parsed!.Value.Intro.Should().Be("The reader asks: \"Truth or dare?\" — declare before hearing either.");
        parsed.Value.Truth.Should().Be("What's the most embarrassing thing that happened to you as a kid?");
        parsed.Value.Dare.Should().Be("Do your best impression of another player.");
        parsed.Value.Forfeit.Should().Be("you owe the group a round of applause");
    }

    [Fact]
    public void WithPreActions_FooterIsOptional()
    {
        var card = CardDeckBuilder.For("Deck")
            .Category("Classics")
            .Card("Truth or Dare", "Declare first.", Difficulty.Easy)
            .WithPreActions(a => a.AddButton("Truth", "A truth.").AddButton("Dare", "A dare."))
            .Build()[0];

        var parsed = TruthOrDareCards.TryParse(card.Description);
        parsed.Should().NotBeNull();
        parsed!.Value.Forfeit.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Dare", "Truth")]   // wrong order
    [InlineData("Truth", "Truth")]  // not the pair
    [InlineData("Left", "Right")]   // unknown labels
    public void WithPreActions_RejectsAnythingButTheTruthDarePairInOrder(string first, string second)
    {
        var act = () => CardDeckBuilder.For("Deck")
            .Category("C").Card("T", "intro", Difficulty.Easy)
            .WithPreActions(a => a.AddButton(first, "x").AddButton(second, "y"));

        act.Should().Throw<ArgumentException>().WithMessage("*Truth*Dare*");
    }

    [Fact]
    public void WithPreActions_BeforeAnyCard_Throws()
    {
        var act = () => CardDeckBuilder.For("Deck").Category("C")
            .WithPreActions(a => a.AddButton("Truth", "x").AddButton("Dare", "y"));

        act.Should().Throw<InvalidOperationException>().WithMessage("*Card*");
    }

    [Fact]
    public void WithPreActions_AppliedTwiceToTheSameCard_Throws()
    {
        var act = () => CardDeckBuilder.For("Deck")
            .Category("C").Card("T", "intro", Difficulty.Easy)
            .WithPreActions(a => a.AddButton("Truth", "x").AddButton("Dare", "y"))
            .WithPreActions(a => a.AddButton("Truth", "x2").AddButton("Dare", "y2"));

        act.Should().Throw<InvalidOperationException>().WithMessage("*one-per-card*");
    }

    [Fact]
    public void FoldingActions_ChangesTheCardId_SinceTheBodyChanged()
    {
        var plain = CardDeckBuilder.For("Deck")
            .Category("C").Card("T", "intro", Difficulty.Easy).Build()[0];
        var gated = CardDeckBuilder.For("Deck")
            .Category("C").Card("T", "intro", Difficulty.Easy)
            .WithPreActions(a => a.AddButton("Truth", "x").AddButton("Dare", "y")).Build()[0];

        gated.Id.Should().NotBe(plain.Id);
    }

    [Fact]
    public void WithPostActions_AppendsABackFaceCardFacesSplits()
    {
        var card = CardDeckBuilder.For("Deck")
            .Category("Quiz")
            .Card("Sky", "Why is the sky blue?", Difficulty.Easy)
            .WithPostActions(a => a.AddButton("Answer", "Rayleigh scattering."))
            .Build()[0];

        CardFaces.HasBack(card.Description).Should().BeTrue();
        var (front, back) = CardFaces.Split(card.Description);
        front.Should().Be("Why is the sky blue?");
        back.Should().Be("Answer: Rayleigh scattering.");
    }

    [Fact]
    public void WithPostActions_RejectsAMarkerCardFacesDoesNotSplitOn()
    {
        var act = () => CardDeckBuilder.For("Deck")
            .Category("C").Card("T", "body", Difficulty.Easy)
            .WithPostActions(a => a.AddButton("Aftercare", "check in with each other"));

        act.Should().Throw<ArgumentException>().WithMessage("*Answer*");
    }

    [Fact]
    public void PreAndPostActions_AreMutuallyExclusiveOnOneCard()
    {
        var act = () => CardDeckBuilder.For("Deck")
            .Category("C")
            .Card("T", "Declare first.", Difficulty.Easy)
            .WithPreActions(a => a.AddButton("Truth", "a truth").AddButton("Dare", "a dare"))
            .WithPostActions(a => a.AddButton("The reading", "the folklore reading"));

        act.Should().Throw<InvalidOperationException>().WithMessage("*mutually exclusive*");
    }

    [Fact]
    public void RevealLabels_AllSplitAsBackFaces_KeepingThemInStepWithCardFaces()
    {
        // Every marker WithPostActions accepts must be one CardFaces actually
        // splits on — otherwise the sugar composes a back face no head reveals.
        foreach (var marker in new[] { "Answer", "The reading" })
        {
            var card = CardDeckBuilder.For("Deck")
                .Category("C").Card("T", "front", Difficulty.Easy)
                .WithPostActions(a => a.AddButton(marker, "revealed"))
                .Build()[0];

            CardFaces.HasBack(card.Description).Should().BeTrue($"'{marker}:' should start a back face");
        }
    }
}
