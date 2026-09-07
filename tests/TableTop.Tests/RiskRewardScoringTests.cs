using TableTop.Core.Domain.Scoring;

namespace TableTop.Tests;

/// <summary>
/// The press-your-luck mechanic: attempting and failing costs points, declining
/// is free.
///
/// <para>
/// The property worth guarding is the sign. Every other strategy in the engine
/// returns zero for a failure, so a regression here does not throw or crash — it
/// quietly makes attempting free again, and the mode's card text goes on
/// promising a penalty that no longer exists.
/// </para>
/// </summary>
public sealed class RiskRewardScoringTests
{
    private static readonly Player Anyone = Player.Create("Sam");

    private static ICard CardOf(Difficulty difficulty) =>
        StandardCard.Create("Card", "Body", difficulty, "Category");

    private static RiskRewardScoringStrategy Wrapping(
        IScoringStrategy? baseStrategy = null, double ratio = 1.0) =>
        new(baseStrategy ?? new DifficultyBasedScoringStrategy(), ratio);

    // ── Completion is untouched ───────────────────────────────────────────────

    [Theory]
    [InlineData(Difficulty.Easy, 1)]
    [InlineData(Difficulty.Medium, 2)]
    [InlineData(Difficulty.Hard, 3)]
    [InlineData(Difficulty.Extreme, 5)]
    public void Completing_PaysExactlyWhatTheBaseStrategyPays(Difficulty difficulty, int expected)
    {
        Wrapping().CalculateScore(CardOf(difficulty), Anyone, CardOutcome.Completed)
            .Should().Be(expected);
    }

    // ── Failure costs ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(Difficulty.Easy, -1)]
    [InlineData(Difficulty.Medium, -2)]
    [InlineData(Difficulty.Hard, -3)]
    [InlineData(Difficulty.Extreme, -5)]
    public void Failing_CostsWhatTheCardWouldHavePaid(Difficulty difficulty, int expected)
    {
        Wrapping().CalculateScore(CardOf(difficulty), Anyone, CardOutcome.Failed)
            .Should().Be(expected);
    }

    [Fact]
    public void Failing_IsTheOnlyOutcomeThatCanGoNegative()
    {
        var strategy = Wrapping();
        var card = CardOf(Difficulty.Extreme);

        strategy.CalculateScore(card, Anyone, CardOutcome.Completed).Should().BePositive();
        strategy.CalculateScore(card, Anyone, CardOutcome.Skipped).Should().Be(0);
        strategy.CalculateScore(card, Anyone, CardOutcome.Failed).Should().BeNegative();
    }

    // ── Declining is free ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(Difficulty.Easy)]
    [InlineData(Difficulty.Extreme)]
    public void Skipping_IsAlwaysFree_SoTheChoiceStaysAChoice(Difficulty difficulty)
    {
        // If declining also cost points a player may as well gamble, which
        // collapses the mechanic back to "attempt everything".
        Wrapping().CalculateScore(CardOf(difficulty), Anyone, CardOutcome.Skipped)
            .Should().Be(0);
    }

    // ── Ratio ─────────────────────────────────────────────────────────────────

    [Fact]
    public void AHalfRatio_MakesAttemptingFavourableAtEvenOdds()
    {
        // Extreme pays 5 and costs 3 (rounded away from zero), so a player who
        // is right half the time comes out ahead.
        Wrapping(ratio: 0.5).CalculateScore(CardOf(Difficulty.Extreme), Anyone, CardOutcome.Failed)
            .Should().Be(-3);
    }

    [Fact]
    public void ATinyRatio_StillCostsAtLeastOnePoint()
    {
        // Rounding away from zero on purpose: a penalty that silently rounds to
        // free is worse than no penalty, because the card text promised one.
        Wrapping(ratio: 0.01).CalculateScore(CardOf(Difficulty.Easy), Anyone, CardOutcome.Failed)
            .Should().Be(-1);
    }

    [Fact]
    public void AZeroRatio_TurnsThePenaltyOffEntirely()
    {
        Wrapping(ratio: 0.0).CalculateScore(CardOf(Difficulty.Hard), Anyone, CardOutcome.Failed)
            .Should().Be(0);
    }

    [Fact]
    public void ANegativeRatio_IsRejected_BecauseItWouldPayPlayersForFailing()
    {
        var act = () => new RiskRewardScoringStrategy(new DifficultyBasedScoringStrategy(), -1.0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ANullBaseStrategy_IsRejected()
    {
        var act = () => new RiskRewardScoringStrategy(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── Composition ───────────────────────────────────────────────────────────

    [Fact]
    public void ABaseThatPaysNothingForTheCard_RisksNothingOnIt()
    {
        // FixedScoringStrategy(0) pays nothing, so there is no stake to lose.
        // Guarded explicitly so the delta is 0 rather than a negative zero that
        // reads as a penalty in the diagnostics log.
        Wrapping(new FixedScoringStrategy(0))
            .CalculateScore(CardOf(Difficulty.Extreme), Anyone, CardOutcome.Failed)
            .Should().Be(0);
    }

    [Fact]
    public void ItWrapsAnyStrategy_NotJustDifficultyBased()
    {
        Wrapping(new FixedScoringStrategy(4))
            .CalculateScore(CardOf(Difficulty.Easy), Anyone, CardOutcome.Failed)
            .Should().Be(-4);
    }

    [Fact]
    public void TheNameReportsWhatItWraps()
    {
        Wrapping(new FixedScoringStrategy()).Name.Should().Be("RiskReward(Fixed)");
    }
}
