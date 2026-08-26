using TableTop.Core.Domain.Game;
using TableTop.Core.Domain.Lifelines;

namespace TableTop.Tests;

public sealed class MillionaireTests
{
    // ── PrizeLadder ───────────────────────────────────────────────────────────

    [Fact]
    public void PrizeLadder_StartsAtRungZero()
    {
        var ladder = new PrizeLadder();
        ladder.CurrentRungIndex.Should().Be(0);
        ladder.CurrentRung.QuestionNumber.Should().Be(1);
    }

    [Fact]
    public void PrizeLadder_Advance_MovesToNextRung()
    {
        var ladder = new PrizeLadder();
        ladder.Advance();
        ladder.CurrentRungIndex.Should().Be(1);
    }

    [Fact]
    public void PrizeLadder_GuaranteedPrize_ZeroBeforeFirstSafeHaven()
    {
        var ladder = new PrizeLadder();
        ladder.Advance(); // Q1 → Q2
        ladder.Advance(); // Q2 → Q3
        ladder.GuaranteedPrize.Should().Be(0L);
    }

    [Fact]
    public void PrizeLadder_GuaranteedPrize_ReflectsLastSafeHaven()
    {
        var ladder = new PrizeLadder();
        // Advance through Q1–Q5 (safe haven at Q5 = £1,000)
        for (var i = 0; i < 5; i++) ladder.Advance();
        ladder.GuaranteedPrize.Should().Be(1_000L);
    }

    [Fact]
    public void PrizeLadder_IsComplete_AfterAllRungs()
    {
        var ladder = new PrizeLadder();
        for (var i = 0; i < 15; i++) ladder.Advance();
        ladder.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void PrizeLadder_Advance_ThrowsWhenComplete()
    {
        var ladder = new PrizeLadder();
        for (var i = 0; i < 15; i++) ladder.Advance();
        Xunit.Assert.Throws<InvalidOperationException>(() => ladder.Advance());
    }

    // ── MultipleChoiceCard ────────────────────────────────────────────────────

    [Fact]
    public void MultipleChoiceCard_IsCorrect_ReturnsTrueForCorrectLabel()
    {
        var card = MultipleChoiceCard.Create(
            "Q?", "A", "B", "C", "D", AnswerLabel.C, Difficulty.Easy);
        card.IsCorrect(AnswerLabel.C).Should().BeTrue();
    }

    [Fact]
    public void MultipleChoiceCard_IsCorrect_ReturnsFalseForWrongLabel()
    {
        var card = MultipleChoiceCard.Create(
            "Q?", "A", "B", "C", "D", AnswerLabel.C, Difficulty.Easy);
        card.IsCorrect(AnswerLabel.A).Should().BeFalse();
    }

    [Fact]
    public void MultipleChoiceCard_RequiresFourAnswers()
    {
        var act = () => new MultipleChoiceCard(
            Guid.NewGuid(), "Q?", "desc",
            new Dictionary<AnswerLabel, string>
            {
                [AnswerLabel.A] = "One",
                [AnswerLabel.B] = "Two",
            },
            AnswerLabel.A, Difficulty.Easy);
        Assert.Throws<ArgumentException>(() => act());
    }

    // ── FiftyFiftyLifeline ────────────────────────────────────────────────────

    [Fact]
    public void FiftyFifty_LeavesCorrectAnswerIntact()
    {
        var lifeline = new FiftyFiftyLifeline(new Random(42));
        var card = MultipleChoiceCard.Create(
            "Q?", "Wrong1", "Wrong2", "Right", "Wrong3", AnswerLabel.C, Difficulty.Easy);
        var player = Player.Create("Alice");

        var result = lifeline.Activate(card, player, []);

        result.RemainingOptions.Should().Contain(AnswerLabel.C);
        result.RemainingOptions.Should().HaveCount(2);
    }

    [Fact]
    public void FiftyFifty_IsUnavailableAfterUse()
    {
        var lifeline = new FiftyFiftyLifeline();
        var card = MultipleChoiceCard.Create(
            "Q?", "A", "B", "C", "D", AnswerLabel.A, Difficulty.Easy);
        var player = Player.Create("Alice");

        lifeline.Activate(card, player, []);
        lifeline.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void FiftyFifty_ThrowsWhenUsedTwice()
    {
        var lifeline = new FiftyFiftyLifeline();
        var card = MultipleChoiceCard.Create(
            "Q?", "A", "B", "C", "D", AnswerLabel.A, Difficulty.Easy);
        var player = Player.Create("Alice");

        lifeline.Activate(card, player, []);
        Xunit.Assert.Throws<InvalidOperationException>(() => lifeline.Activate(card, player, []));
    }

    // ── PhoneAFriend ──────────────────────────────────────────────────────────

    [Fact]
    public void PhoneAFriend_ProducesSuggestion()
    {
        var lifeline = new PhoneAFriendLifeline(new Random(1));
        var card = MultipleChoiceCard.Create(
            "Q?", "A", "B", "C", "D", AnswerLabel.B, Difficulty.Easy);
        var player = Player.Create("Bob");

        var result = lifeline.Activate(card, player, []);

        result.Suggestion.Should().NotBeNull();
        result.Narrative.Should().Contain("Ringing");
    }

    // ── AskTheAudience ────────────────────────────────────────────────────────

    [Fact]
    public void AskTheAudience_PercentagesSumTo100()
    {
        var lifeline = new AskTheAudienceLifeline(new Random(99));
        var card = MultipleChoiceCard.Create(
            "Q?", "A", "B", "C", "D", AnswerLabel.D, Difficulty.Medium);
        var player = Player.Create("Carol");

        var result = lifeline.Activate(card, player, []);
        // Extract percentages from the narrative lines
        var lines = result.Narrative.Split('\n')
            .Where(l => l.TrimStart().StartsWith("A:") ||
                        l.TrimStart().StartsWith("B:") ||
                        l.TrimStart().StartsWith("C:") ||
                        l.TrimStart().StartsWith("D:"))
            .ToList();

        lines.Should().HaveCount(4);

        var total = lines
            .Select(l => l.Trim().Split('%')[0].Split(':')[1].Trim())
            .Select(int.Parse)
            .Sum();

        total.Should().Be(100);
    }
}