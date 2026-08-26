using TableTop.Core.Abstractions.Restrictions;

namespace TableTop.Tests;

public sealed class BreakAndRewardCardTests
{
    // This class used to write deck files to a temp directory and load them
    // through JsonCardProvider, so it needed IDisposable and a fixture. With the
    // deck file format gone (1.21.0) every surviving test constructs its cards
    // directly, which is both simpler and what the engine actually deals.

    // ── BreakCard domain model ────────────────────────────────────────────────

    [Fact]
    public void BreakCard_ImplementsIBreakCard() =>
        BreakCard.CreateGroupBreak("Rest", "Take a break.").Should()
            .BeAssignableTo<IBreakCard>();

    [Fact]
    public void BreakCard_ImplementsICard() =>
        BreakCard.CreateGroupBreak("Rest", "Take a break.").Should()
            .BeAssignableTo<ICard>();

    [Fact]
    public void BreakCard_Category_IsAlwaysBreak() =>
        BreakCard.CreateGroupBreak("Rest", "Desc.").Category.Should().Be("Break");

    [Fact]
    public void BreakCard_CreateSkipTurn_HasSkipTurnEffect()
    {
        var card = BreakCard.CreateSkipTurn("Skip", "Desc.");
        card.Effect.Should().BeOfType<SkipTurnEffect>();
        card.Scope.Should().Be(BreakScope.CurrentPlayer);
    }

    [Fact]
    public void BreakCard_CreateGroupBreak_HasAllPlayersScope()
    {
        var card = BreakCard.CreateGroupBreak("Rest", "Relax.");
        card.Scope.Should().Be(BreakScope.AllPlayers);
    }

    [Fact]
    public void BreakCard_NullEffect_AllowedForNarrativeCards()
    {
        var card = new BreakCard(Guid.NewGuid(), "Rest", "Just rest.", BreakScope.AllPlayers, null);
        card.Effect.Should().BeNull();
    }

    // ── RewardCard domain model ───────────────────────────────────────────────

    [Fact]
    public void RewardCard_ImplementsIRewardCard() =>
        RewardCard.CreateScoreBonus("Bonus", "Desc.", 5).Should()
            .BeAssignableTo<IRewardCard>();

    [Fact]
    public void RewardCard_Category_IsAlwaysReward() =>
        RewardCard.CreateScoreBonus("Bonus", "Desc.", 5).Category.Should().Be("Reward");

    [Fact]
    public void RewardCard_ScoreBonus_CorrectEffect()
    {
        var card = RewardCard.CreateScoreBonus("Bonus", "Desc.", 10);
        Xunit.Assert.Equal(10, ((card.Effect as ScoreBonusEffect)!.Points));
    }

    [Fact]
    public void RewardCard_StealPoints_CorrectEffect()
    {
        var card = RewardCard.CreateStealPoints("Steal", "Desc.", 3);
        Xunit.Assert.Equal(3, ((card.Effect as StealPointsEffect)!.Points));
    }

    [Fact]
    public void RewardCard_FreePass_CorrectEffect() =>
        RewardCard.CreateFreePass("Free", "Desc.").Effect.Should().BeOfType<FreePassEffect>();

    [Fact]
    public void RewardCard_ExtraCard_CorrectEffect() =>
        RewardCard.CreateExtraCard("Extra", "Desc.").Effect.Should().BeOfType<ExtraCardEffect>();

    // Seventeen tests lived below here, all reaching the card types through the
    // JSON deck format: loading break and reward cards from a file, validating a
    // bad cardType or reward effect, DeckExporter round-tripping each shape, and
    // lenient loading dropping only the one malformed card. They went with the
    // deck file format in 1.21.0, along with this class's temp-directory fixture
    // and its IDisposable.
    //
    // The card TYPES are unaffected and still covered above, constructed
    // directly — which is also how the engine builds them. What is no longer
    // covered is serialisation, and there is nothing left to serialise to.
}
