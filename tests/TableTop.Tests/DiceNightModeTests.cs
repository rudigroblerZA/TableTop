using TableTop.Core.Abstractions.Game;
using TableTop.Games.Family;
using TableTop.Hosting.Controllers;

namespace TableTop.Tests;

/// <summary>Zero test references before this — same shape as <see cref="RollWithItModeTests"/>, the mode this one's mechanic is proven against.</summary>
public sealed class DiceNightModeTests
{
    [Fact]
    public void RegisteredInArchetypeTree_AtTheExpectedId()
    {
        var node = ArchetypeRegistry.Default().FindById("fun.family.dicenight");
        node.Should().NotBeNull();
        node!.Modes.Should().Contain(m => m.Name == "Dice Night");
    }

    [Fact]
    public void AgeRating_IsAllAges()
    {
        // Unlike RollWithItMode's Teen floor — this one is written for a
        // table that includes young kids, and the whole point is giving
        // fun.family a dice-driven option of its own.
        var node = ArchetypeRegistry.Default().FindById("fun.family.dicenight");
        node!.AgeRating.Should().Be(AgeRating.AllAges);
    }

    [Fact]
    public void Deck_Has30Cards_AcrossFiveCategories()
    {
        var deck = new DiceNightMode().GetCards([]);
        deck.Should().HaveCount(30);
        deck.Select(c => c.Category).Distinct().Should().HaveCount(5);
    }

    [Fact]
    public void Deck_HasNoDuplicateBodiesOrIds()
    {
        var deck = new DiceNightMode().GetCards([]);
        deck.Select(c => c.Description).Distinct().Should().HaveCount(deck.Count);
        deck.Select(c => c.Id).Distinct().Should().HaveCount(deck.Count);
    }

    [Fact]
    public void Deck_IdsAreStable_AcrossRepeatedCalls()
    {
        var mode = new DiceNightMode();
        var first = mode.GetCards([]);
        var second = mode.GetCards([]);

        first.Select(c => c.Id).Should().Equal(second.Select(c => c.Id));
    }

    [Fact]
    public void Manifest_ReportsNonZeroTotalCards()
    {
        // The exact bug class fixed for Claimed! and Herd (backlog item 10):
        // a mode whose manifest reports zero is silently excluded from every
        // SurpriseMe(maxCards:) query.
        new DiceNightMode().GetManifest().TotalCards.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(2, "Icebreaker")]
    [InlineData(4, "Icebreaker")]
    [InlineData(5, "Giggle")]
    [InlineData(6, "Giggle")]
    [InlineData(7, "Story Time")]
    [InlineData(8, "Story Time")]
    [InlineData(9, "Silly Challenge")]
    [InlineData(10, "Silly Challenge")]
    [InlineData(11, "Grand Finale")]
    [InlineData(12, "Grand Finale")]
    public void CategoryForTotal_MapsEveryBoundaryCorrectly(int total, string expectedCategory)
    {
        new DiceNightMode().CategoryForTotal(total).Should().Be(expectedCategory);
    }

    [Fact]
    public void CategoriesInOrder_MatchesTheCategoriesActuallyOnCards()
    {
        var mode = new DiceNightMode();
        var deck = mode.GetCards([]);

        mode.CategoriesInOrder.Should().HaveCount(5);
        deck.Select(c => c.Category).Distinct().Should().OnlyContain(c => mode.CategoriesInOrder.Contains(c));
    }

    [Fact]
    public async Task FullPlaythrough_ReachesMultipleCategories_ThroughTheRealFactory()
    {
        var players = new[] { (TableTop.Core.Abstractions.Players.IPlayer)Player.Create("Alice"), Player.Create("Bob") };
        var controller = (TableTop.Hosting.Abstractions.ICardTurnController)
            (await new ControllerFactory().CreateAsync(new DiceNightMode(), players));

        var categoriesSeen = new HashSet<string>();
        controller.CardReady += (_, e) => categoriesSeen.Add(e.Category);
        controller.Start();
        for (var i = 0; i < 15 && controller.IsRunning; i++)
            controller.RecordOutcome(TableTop.Core.Abstractions.Scoring.CardOutcome.Completed);

        categoriesSeen.Count.Should().BeGreaterThan(1, "dice-driven selection should reach more than one category");
        controller.Dispose();
    }
}
