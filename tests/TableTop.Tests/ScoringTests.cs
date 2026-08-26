using TableTop.Core.Domain.Scoring;

namespace TableTop.Tests;

public sealed class ScoringTests
{
    private static Player MakePlayer() => Player.Create("Alice");

    [Theory]
    [InlineData(CardOutcome.Completed, 1)]
    [InlineData(CardOutcome.Skipped,   0)]
    [InlineData(CardOutcome.Failed,    0)]
    public void FixedScoring_ReturnsConfiguredPoints(CardOutcome outcome, int expected)
    {
        var strategy = new FixedScoringStrategy(1);
        var card = StandardCard.Create("T", "D", Difficulty.Easy, "Cat");
        strategy.CalculateScore(card, MakePlayer(), outcome).Should().Be(expected);
    }

    [Theory]
    [InlineData(Difficulty.Easy,    1)]
    [InlineData(Difficulty.Medium,  2)]
    [InlineData(Difficulty.Hard,    3)]
    [InlineData(Difficulty.Extreme, 5)]
    public void DifficultyScoring_ReturnsCorrectPoints(Difficulty diff, int expected)
    {
        var strategy = new DifficultyBasedScoringStrategy();
        var card = StandardCard.Create("T", "D", diff, "Cat");
        strategy.CalculateScore(card, MakePlayer(), CardOutcome.Completed).Should().Be(expected);
    }

    [Fact]
    public void DifficultyScoring_SkippedCard_ReturnsZero()
    {
        var strategy = new DifficultyBasedScoringStrategy();
        var card = StandardCard.Create("T", "D", Difficulty.Hard, "Cat");
        strategy.CalculateScore(card, MakePlayer(), CardOutcome.Skipped).Should().Be(0);
    }

    [Fact]
    public void StreakScoring_StreakAboveThreshold_MultipliesScore()
    {
        var baseStrategy = new FixedScoringStrategy(2);
        var streakStrategy = new StreakScoringStrategy(baseStrategy, streakMultiplier: 3);

        var player = Player.Create("Alice",
            tags: ["streak:4"]);
        var card = StandardCard.Create("T", "D", Difficulty.Easy, "Cat");

        streakStrategy.CalculateScore(card, player, CardOutcome.Completed).Should().Be(6); // 2 * 3
    }

    [Fact]
    public void StreakScoring_BelowThreshold_ReturnsBaseScore()
    {
        var baseStrategy = new FixedScoringStrategy(2);
        var streakStrategy = new StreakScoringStrategy(baseStrategy, streakMultiplier: 3);
        var player = Player.Create("Alice", tags: ["streak:2"]); // below threshold of 3
        var card = StandardCard.Create("T", "D", Difficulty.Easy, "Cat");

        streakStrategy.CalculateScore(card, player, CardOutcome.Completed).Should().Be(2);
    }
}