using TableTop.Core.Domain.Decks;

namespace TableTop.Tests;

public sealed class DeckTests
{
    private static StandardCard MakeCard(string title = "Card", Difficulty d = Difficulty.Easy) =>
        StandardCard.Create(title, "desc", d, "Test");

    [Fact]
    public void Draw_ReturnsCardsInOrder_UntilExhausted()
    {
        var cards = new[] { MakeCard("A"), MakeCard("B"), MakeCard("C") };
        var deck = new Deck(Guid.NewGuid(), "Test", cards);

        deck.Draw()!.Title.Should().Be("A");
        deck.Draw()!.Title.Should().Be("B");
        deck.Draw()!.Title.Should().Be("C");
        deck.Draw().Should().BeNull();
    }

    [Fact]
    public void IsEmpty_TrueWhenExhausted()
    {
        var deck = new Deck(Guid.NewGuid(), "Empty", [MakeCard()]);
        deck.Draw();
        deck.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Filter_ReturnsMatchingCards_WithoutRemoving()
    {
        var cards = new[]
        {
            MakeCard("Easy1", Difficulty.Easy),
            MakeCard("Hard1", Difficulty.Hard),
            MakeCard("Easy2", Difficulty.Easy)
        };
        var deck = new Deck(Guid.NewGuid(), "Mixed", cards);

        var easy = deck.Filter(c => c.Difficulty == Difficulty.Easy);

        easy.Should().HaveCount(2);
        deck.Count.Should().Be(3); // Filter does not remove
    }

    [Fact]
    public void Reset_RestoresOriginalOrder()
    {
        var cards = new[] { MakeCard("A"), MakeCard("B") };
        var deck = new Deck(Guid.NewGuid(), "Test", cards);
        deck.Draw();
        deck.Reset();

        deck.Count.Should().Be(2);
        deck.Draw()!.Title.Should().Be("A");
    }

    [Fact]
    public void Shuffle_ChangesOrder()
    {
        var cards = Enumerable.Range(1, 20)
            .Select(i => MakeCard($"Card{i}"))
            .ToList();
        var deck = new Deck(Guid.NewGuid(), "Big", cards);

        deck.Shuffle(new FisherYatesShuffleStrategy(new Random(42)));

        var after = deck.Cards.Select(c => c.Title).ToList();
        after.Should().NotEqual(cards.Select(c => c.Title));
    }

    [Fact]
    public async Task DeckBuilder_ComposesMultipleProviders()
    {
        var provider1 = new InMemoryCardProvider([MakeCard("A"), MakeCard("B")]);
        var provider2 = new InMemoryCardProvider([MakeCard("C")]);

        var deck = await new DeckBuilder()
            .WithName("Combined")
            .WithProvider(provider1)
            .WithProvider(provider2)
            .BuildAsync();

        deck.Count.Should().Be(3);
    }

    [Fact]
    public async Task DeckBuilder_AppliesFilters()
    {
        var cards = new[]
        {
            MakeCard("Easy", Difficulty.Easy),
            MakeCard("Hard", Difficulty.Hard)
        };
        var provider = new InMemoryCardProvider(cards);

        var deck = await new DeckBuilder()
            .WithProvider(provider)
            .WithFilter(c => c.Difficulty == Difficulty.Easy)
            .BuildAsync();

        deck.Count.Should().Be(1);
        deck.Cards[0].Title.Should().Be("Easy");
    }
}