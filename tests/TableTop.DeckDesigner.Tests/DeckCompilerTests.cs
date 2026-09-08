using FluentAssertions;
using TableTop.Core.Abstractions.Cards;
using TableTop.DeckDesigner.Core;
using Xunit;

namespace TableTop.DeckDesigner.Tests;

/// <summary>
/// <see cref="DeckCompiler"/> exists to guarantee the tool's preview and any
/// generated card bank match what <c>CardDeckBuilder</c> would actually
/// produce — these tests exercise that real compilation, not a mock of it.
/// </summary>
public sealed class DeckCompilerTests
{
    private static DeckDraft ValidDeck() => new()
    {
        Name = "Sample Deck",
        Cards =
        {
            new CardDraft { Category = "History", Title = "Ancient Egypt", Description = "Order these.", Difficulty = Difficulty.Hard },
            new CardDraft { Category = "History", Title = "The Renaissance", Description = "Order these.", Difficulty = Difficulty.Medium },
            new CardDraft { Category = "Pop Culture", Title = "Streaming Wars", Description = "Order these.", Difficulty = Difficulty.Easy },
        },
    };

    [Fact]
    public void Compile_ValidDeck_Succeeds()
    {
        var result = DeckCompiler.Compile(ValidDeck());

        result.Succeeded.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Cards.Should().HaveCount(3);
    }

    [Fact]
    public void Compile_ValidDeck_PreservesOrderAndFields()
    {
        var cards = DeckCompiler.Compile(ValidDeck()).Cards!;

        cards[0].Title.Should().Be("Ancient Egypt");
        cards[0].Category.Should().Be("History");
        cards[0].Difficulty.Should().Be(Difficulty.Hard);
        cards[2].Category.Should().Be("Pop Culture");
    }

    [Fact]
    public void Compile_SameDraft_ProducesDeterministicIds()
    {
        // The whole reason DeckCompiler drives CardDeckBuilder directly
        // rather than assigning its own ids.
        var first = DeckCompiler.Compile(ValidDeck()).Cards!;
        var second = DeckCompiler.Compile(ValidDeck()).Cards!;

        first.Select(c => c.Id).Should().Equal(second.Select(c => c.Id));
    }

    [Fact]
    public void Compile_BlankDeckName_Fails()
    {
        var deck = ValidDeck();
        deck.Name = "  ";

        var result = DeckCompiler.Compile(deck);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Deck name"));
    }

    [Fact]
    public void Compile_NoCards_Fails()
    {
        var result = DeckCompiler.Compile(new DeckDraft { Name = "Empty" });

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("at least one card"));
    }

    [Fact]
    public void Compile_CardMissingCategory_FailsAndNamesTheCard()
    {
        var deck = new DeckDraft { Name = "Deck", Cards = { new CardDraft { Title = "T", Description = "D" } } };

        var result = DeckCompiler.Compile(deck);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.StartsWith("Card 1") && e.Contains("category"));
    }

    [Fact]
    public void Compile_ThisOrThatWithIdenticalLabels_FailsViaBuilderGuard()
    {
        var deck = new DeckDraft
        {
            Name = "Deck",
            Cards =
            {
                new CardDraft
                {
                    Category = "Cat", Title = "T", Description = "Q", IsThisOrThat = true,
                    OptionALabel = "Beach", OptionBLabel = "beach",
                },
            },
        };

        var result = DeckCompiler.Compile(deck);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Compile_ThisOrThatCard_ProducesIThisOrThatCardWithOptions()
    {
        var deck = new DeckDraft
        {
            Name = "Deck",
            Cards =
            {
                new CardDraft
                {
                    Category = "Cat", Title = "T", Description = "Which would you rather?",
                    IsThisOrThat = true,
                    OptionALabel = "Beach", OptionADetail = "Sun and sand.",
                    OptionBLabel = "Mountains",
                },
            },
        };

        var result = DeckCompiler.Compile(deck);

        result.Succeeded.Should().BeTrue();
        var card = result.Cards!.Single().Should().BeAssignableTo<IThisOrThatCard>().Subject;
        card.OptionA.Label.Should().Be("Beach");
        card.OptionA.Detail.Should().Be("Sun and sand.");
        card.OptionB.Label.Should().Be("Mountains");
        card.OptionB.HasDetail.Should().BeFalse();
    }

    [Fact]
    public void Compile_NonContiguousRepeatedCategory_StillTagsEachCardCorrectly()
    {
        // CardDeckBuilder re-tags on every .Category(...) call rather than
        // merging same-named categories, so History/PopCulture/History (not
        // grouped) must still leave every card correctly categorised.
        var deck = new DeckDraft
        {
            Name = "Deck",
            Cards =
            {
                new CardDraft { Category = "History", Title = "A", Description = "D" },
                new CardDraft { Category = "Pop Culture", Title = "B", Description = "D" },
                new CardDraft { Category = "History", Title = "C", Description = "D" },
            },
        };

        var cards = DeckCompiler.Compile(deck).Cards!;

        cards[0].Category.Should().Be("History");
        cards[1].Category.Should().Be("Pop Culture");
        cards[2].Category.Should().Be("History");
    }
}
