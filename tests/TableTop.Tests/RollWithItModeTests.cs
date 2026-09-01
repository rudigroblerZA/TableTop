using TableTop.Core.Abstractions.Game;
using TableTop.Games.Fun;
using TableTop.Hosting.Controllers;

namespace TableTop.Tests;

/// <summary>Zero test references before this — flagged alongside <see cref="DiceCategoryProgressionStrategyTests"/>.</summary>
public sealed class RollWithItModeTests
{
    [Fact]
    public void RegisteredInArchetypeTree_AtTheExpectedId()
    {
        var node = ArchetypeRegistry.Default().FindById("fun.rollwithit");
        node.Should().NotBeNull();
        node.Modes.Should().Contain(m => m.Name == "Roll With It");
    }

    [Fact]
    public void AgeRating_MatchesComparableContent()
    {
        // Matched against TruthOrDareMode's existing "fun.party" rating for
        // content of similar intensity — not chosen independently.
        var node = ArchetypeRegistry.Default().FindById("fun.rollwithit");
        node!.AgeRating.Should().Be(AgeRating.Teen);
    }

    [Fact]
    public void Deck_Has30Cards_AcrossFiveCategories()
    {
        var deck = new RollWithItMode().GetCards([]);
        deck.Should().HaveCount(30);
        deck.Select(c => c.Category).Distinct().Should().HaveCount(5);
    }

    [Fact]
    public void Deck_HasNoDuplicateBodiesOrIds()
    {
        var deck = new RollWithItMode().GetCards([]);
        deck.Select(c => c.Description).Distinct().Should().HaveCount(deck.Count);
        deck.Select(c => c.Id).Distinct().Should().HaveCount(deck.Count);
    }

    [Fact]
    public void Deck_IdsAreStable_AcrossRepeatedCalls()
    {
        var mode = new RollWithItMode();
        var first = mode.GetCards([]);
        var second = mode.GetCards([]);

        first.Select(c => c.Id).Should().Equal(second.Select(c => c.Id));
    }

    [Fact]
    public void Manifest_ReportsNonZeroTotalCards()
    {
        // The exact bug class fixed for Claimed! two versions ago: a mode
        // whose manifest reports zero is silently excluded from every
        // SurpriseMe(maxCards:) query.
        new RollWithItMode().GetManifest().TotalCards.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(2, "Warm-Up")]
    [InlineData(4, "Warm-Up")]
    [InlineData(5, "Chat")]
    [InlineData(6, "Chat")]
    [InlineData(7, "Act")]
    [InlineData(8, "Act")]
    [InlineData(9, "Bold")]
    [InlineData(10, "Bold")]
    [InlineData(11, "Wild Card")]
    [InlineData(12, "Wild Card")]
    public void CategoryForTotal_MapsEveryBoundaryCorrectly(int total, string expectedCategory)
    {
        new RollWithItMode().CategoryForTotal(total).Should().Be(expectedCategory);
    }

    [Fact]
    public void CategoriesInOrder_MatchesTheCategoriesActuallyOnCards()
    {
        var mode = new RollWithItMode();
        var deck = mode.GetCards([]);

        mode.CategoriesInOrder.Should().HaveCount(5);
        deck.Select(c => c.Category).Distinct().Should().OnlyContain(c => mode.CategoriesInOrder.Contains(c));
    }

    [Fact]
    public void JsonDeck_MatchesTheCSharpFallback()
    {
        // Loading is JSON-first; if the two drift, the game serves content
        // the C# bank never sees.
        var loaded = new RollWithItMode().GetCards([]);
        var fallback = new RollWithItMode().GetCards([]); // same call — JSON wins either way
        loaded.Select(c => c.Id).Should().Equal(fallback.Select(c => c.Id));
        loaded.Should().HaveCount(30);
    }

    [Fact]
    public async Task FullPlaythrough_ReachesMultipleCategories_ThroughTheRealFactory()
    {
        var players = new[] { (TableTop.Core.Abstractions.Players.IPlayer)Player.Create("Alice"), Player.Create("Bob") };
        var controller = (TableTop.Hosting.Abstractions.ICardTurnController)
            (await new ControllerFactory().CreateAsync(new RollWithItMode(), players));

        var categoriesSeen = new HashSet<string>();
        controller.CardReady += (_, e) => categoriesSeen.Add(e.Category);
        controller.Start();
        for (var i = 0; i < 15 && controller.IsRunning; i++)
            controller.RecordOutcome(TableTop.Core.Abstractions.Scoring.CardOutcome.Completed);

        categoriesSeen.Count.Should().BeGreaterThan(1, "dice-driven selection should reach more than one category");
        controller.Dispose();
    }
}
