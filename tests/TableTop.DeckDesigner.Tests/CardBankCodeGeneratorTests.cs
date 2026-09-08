using FluentAssertions;
using TableTop.Core.Abstractions.Cards;
using TableTop.DeckDesigner.Core;
using Xunit;

namespace TableTop.DeckDesigner.Tests;

public sealed class CardBankCodeGeneratorTests
{
    private static DeckDraft SampleDeck() => new()
    {
        Name = "Chronology Challenge",
        Cards =
        {
            new CardDraft { Category = "History", Title = "Ancient Egypt", Description = "Order these.", Difficulty = Difficulty.Hard },
            new CardDraft { Category = "Pop Culture", Title = "Streaming Wars", Description = "Order these.", Difficulty = Difficulty.Easy, Tags = ["fun", "modern"] },
        },
    };

    [Fact]
    public void Generate_EmitsExpectedClassShape()
    {
        var source = CardBankCodeGenerator.Generate(SampleDeck(), "TableTop.Games.Custom", "ChronologyChallengeCardBank");

        source.Should().Contain("namespace TableTop.Games.Custom;");
        source.Should().Contain("public static class ChronologyChallengeCardBank");
        source.Should().Contain("private const string Deck = \"Chronology Challenge\";");
        source.Should().Contain("public static IReadOnlyList<ICard> All { get; } = Build();");
        source.Should().Contain("CardDeckBuilder.For(Deck)");
        source.Should().Contain(".Build();");
    }

    [Fact]
    public void Generate_EachCategoryChangeEmitsOneCategoryCall()
    {
        var source = CardBankCodeGenerator.Generate(SampleDeck(), "TableTop.Games.Custom", "Bank");

        source.Should().Contain(".Category(\"History\")");
        source.Should().Contain(".Category(\"Pop Culture\")");
    }

    [Fact]
    public void Generate_RepeatedCategory_EmitsOnlyOneCategoryCallForTheRun()
    {
        var deck = new DeckDraft
        {
            Name = "Deck",
            Cards =
            {
                new CardDraft { Category = "History", Title = "A", Description = "D" },
                new CardDraft { Category = "History", Title = "B", Description = "D" },
            },
        };

        var source = CardBankCodeGenerator.Generate(deck, "Ns", "Bank");

        source.Split(".Category(\"History\")").Length.Should().Be(2); // one occurrence
    }

    [Fact]
    public void Generate_StandardCard_EmitsCardCallWithDifficulty()
    {
        var source = CardBankCodeGenerator.Generate(SampleDeck(), "Ns", "Bank");

        source.Should().Contain(".Card(\"Ancient Egypt\", \"Order these.\", Difficulty.Hard)");
    }

    [Fact]
    public void Generate_TagsEmittedAsArrayLiteral()
    {
        var source = CardBankCodeGenerator.Generate(SampleDeck(), "Ns", "Bank");

        source.Should().Contain("tags: new[] { \"fun\", \"modern\" }");
    }

    [Fact]
    public void Generate_NoTags_OmitsTagsArgument()
    {
        var source = CardBankCodeGenerator.Generate(SampleDeck(), "Ns", "Bank");

        source.Should().Contain(".Card(\"Ancient Egypt\", \"Order these.\", Difficulty.Hard)");
        source.Should().NotContain("Ancient Egypt\", \"Order these.\", Difficulty.Hard, tags:");
    }

    [Fact]
    public void Generate_EscapesQuotesBackslashesAndNewlines()
    {
        var deck = new DeckDraft
        {
            Name = "Deck",
            Cards =
            {
                new CardDraft { Category = "Cat", Title = "He said \"hi\"", Description = "Line one\nLine two \\ end" },
            },
        };

        var source = CardBankCodeGenerator.Generate(deck, "Ns", "Bank");

        source.Should().Contain("\"He said \\\"hi\\\"\"");
        source.Should().Contain("Line one\\nLine two \\\\ end");
        // Escaped, not a literal newline splitting the .Card(...) call across lines.
        source.Should().NotContain("Line one\nLine two");
    }

    [Fact]
    public void Generate_ThisOrThatCard_EmitsOptionsWithDetailOnlyWhenPresent()
    {
        var deck = new DeckDraft
        {
            Name = "Deck",
            Cards =
            {
                new CardDraft
                {
                    Category = "Cat", Title = "T", Description = "Q", IsThisOrThat = true,
                    OptionALabel = "Beach", OptionADetail = "Sun and sand.",
                    OptionBLabel = "Mountains",
                },
            },
        };

        var source = CardBankCodeGenerator.Generate(deck, "Ns", "Bank");

        source.Should().Contain(".ThisOrThatCard(\"T\", \"Q\", " +
            "new ThisOrThatOption(\"Beach\", Detail: \"Sun and sand.\"), " +
            "new ThisOrThatOption(\"Mountains\"), Difficulty.Medium)");
    }
}
