using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Progression;
using TableTop.Hosting.Events;
using TableTop.Hosting.Hints;

namespace TableTop.Tests;

public sealed class HintEngineTests
{
    private static Player Male(string name = "Bob") =>
        TestFactory.MakePlayer(name, gender: "male");

    private static Player Female(string name = "Alice") =>
        TestFactory.MakePlayer(name, gender: "female");

    private static Player Neutral(string name = "Sam") =>
        TestFactory.MakePlayer(name, gender: "other");

    private static HintContext MakeCtx(
        IReadOnlyList<CardOutcome> outcomes,
        IReadOnlyList<Difficulty>? difficulties = null,
        FlowState? flow = null,
        int skipCount = 0,
        int round = 5) =>
        new(
            RecentOutcomes: outcomes,
            RecentDifficulties: difficulties ?? outcomes.Select(_ => Difficulty.Medium).ToList().AsReadOnly(),
            CurrentFlow: flow,
            SkipCount: skipCount,
            Round: round,
            Standings: []);

    private readonly DefaultHintEngine _engine = new();

    // ── No hint for empty history ─────────────────────────────────────────────

    [Fact]
    public void GenerateHint_EmptyHistory_ReturnsNull()
    {
        var hint = _engine.GenerateHint(Male(), MakeCtx([]));
        hint.Should().BeNull();
    }

    // ── Struggling rule ───────────────────────────────────────────────────────

    [Fact]
    public void GenerateHint_TwoFailures_SuggestsEasierDifficulty()
    {
        var outcomes = new[] { CardOutcome.Failed, CardOutcome.Failed, CardOutcome.Completed }
            .ToList().AsReadOnly();
        var diffs = new[] { Difficulty.Medium, Difficulty.Medium, Difficulty.Medium }
            .ToList().AsReadOnly();

        var hint = _engine.GenerateHint(Male(), MakeCtx(outcomes, diffs));

        hint.Should().NotBeNull();
        hint!.SuggestedDifficulty.Should().Be(Difficulty.Easy);
        hint.Urgency.Should().Be(HintUrgency.Strong);
        hint.Reason.Should().Be("Struggling");
    }

    [Fact]
    public void GenerateHint_TwoSkips_SuggestsEasier()
    {
        var outcomes = new[] { CardOutcome.Skipped, CardOutcome.Skipped }
            .ToList().AsReadOnly();

        var hint = _engine.GenerateHint(Female(), MakeCtx(outcomes));

        hint.Should().NotBeNull();
        hint!.SuggestedDifficulty.Should().Be(Difficulty.Easy);
        hint.Urgency.Should().Be(HintUrgency.Strong);
    }

    // ── Excelling rule ────────────────────────────────────────────────────────

    [Fact]
    public void GenerateHint_ThreeCompletionsOnHard_SuggestsExtreme()
    {
        var outcomes = Enumerable.Repeat(CardOutcome.Completed, 3).ToList().AsReadOnly();
        var diffs = Enumerable.Repeat(Difficulty.Hard, 3).ToList().AsReadOnly();

        var hint = _engine.GenerateHint(Male(), MakeCtx(outcomes, diffs));

        hint.Should().NotBeNull();
        hint!.SuggestedDifficulty.Should().Be(Difficulty.Extreme);
        hint.Urgency.Should().Be(HintUrgency.Strong);
        hint.Reason.Should().Be("Excelling");
    }

    [Fact]
    public void GenerateHint_ThreeCompletionsOnExtreme_NoEscalation()
    {
        var outcomes = Enumerable.Repeat(CardOutcome.Completed, 3).ToList().AsReadOnly();
        var diffs = Enumerable.Repeat(Difficulty.Extreme, 3).ToList().AsReadOnly();

        var hint = _engine.GenerateHint(Male(), MakeCtx(outcomes, diffs));

        // Already at max — can't go higher, no "excelling" hint
        hint?.SuggestedDifficulty.Should().NotBe(Difficulty.Extreme + 1);
    }

    // ── Consistent success rule ───────────────────────────────────────────────

    [Fact]
    public void GenerateHint_TwoCompletionsOnMedium_SuggestsHard()
    {
        var outcomes = new[] { CardOutcome.Completed, CardOutcome.Completed }
            .ToList().AsReadOnly();
        var diffs = new[] { Difficulty.Medium, Difficulty.Medium }
            .ToList().AsReadOnly();

        var hint = _engine.GenerateHint(Female(), MakeCtx(outcomes, diffs));

        hint.Should().NotBeNull();
        hint!.SuggestedDifficulty.Should().Be(Difficulty.Hard);
        hint.Urgency.Should().Be(HintUrgency.Gentle);
    }

    // ── Heavy skipping ────────────────────────────────────────────────────────

    [Fact]
    public void GenerateHint_ThreeSkips_SuggestsEasier_Moderate()
    {
        var outcomes = new[] { CardOutcome.Completed }.ToList().AsReadOnly();

        var hint = _engine.GenerateHint(Male(), MakeCtx(outcomes, skipCount: 3));

        hint.Should().NotBeNull();
        hint!.Reason.Should().Be("HeavySkipping");
        hint.Urgency.Should().Be(HintUrgency.Moderate);
    }

    // ── Gender-aware text ─────────────────────────────────────────────────────

    [Fact]
    public void HintText_ForMale_ReturnsHimHint()
    {
        var outcomes = new[] { CardOutcome.Failed, CardOutcome.Failed }.ToList().AsReadOnly();
        var diffs = new[] { Difficulty.Medium, Difficulty.Medium }.ToList().AsReadOnly();

        var hint = _engine.GenerateHint(Male("Bob"), MakeCtx(outcomes, diffs))!;

        var text = hint.ForPlayer(Male("Bob"));
        var anyKeyword = text.Contains("momentum") || text.Contains("back") || text.Contains("step");
        anyKeyword.Should().BeTrue("hint should reference momentum, back, or step");
        text.Should().NotBeEmpty();
    }

    [Fact]
    public void HintText_ForFemale_ReturnsDifferentText()
    {
        var outcomes = new[] { CardOutcome.Failed, CardOutcome.Failed }.ToList().AsReadOnly();
        var diffs = new[] { Difficulty.Medium, Difficulty.Medium }.ToList().AsReadOnly();

        var hint = _engine.GenerateHint(Female("Alice"), MakeCtx(outcomes, diffs))!;

        var himText = hint.ForPlayer(Male("Bob"));
        var herText = hint.ForPlayer(Female("Alice"));
        himText.Should().NotBeNullOrEmpty();
        herText.Should().NotBeNullOrEmpty();
        // Gender-specific hints should differ
        if (hint.HimHint is not null && hint.HerHint is not null)
            himText.Should().NotBe(herText);
    }

    [Fact]
    public void HintText_ForNeutralGender_ReturnsNeutralHint()
    {
        var outcomes = new[] { CardOutcome.Failed, CardOutcome.Failed }.ToList().AsReadOnly();
        var hint = _engine.GenerateHint(Neutral(), MakeCtx(outcomes))!;
        hint.ForPlayer(Neutral()).Should().Be(hint.NeutralHint);
    }

    // ── FlowState integration ─────────────────────────────────────────────────

    [Fact]
    public void GenerateHint_WithFlowState_SuggestsPaceSlowDown_WhenStruggling()
    {
        var outcomes = new[] { CardOutcome.Failed, CardOutcome.Failed }.ToList().AsReadOnly();
        var diffs = new[] { Difficulty.Medium, Difficulty.Medium }.ToList().AsReadOnly();
        var flow = new FlowState(Difficulty.Medium, FlowPace.Fast);

        var hint = _engine.GenerateHint(Male(), MakeCtx(outcomes, diffs, flow));

        hint!.SuggestedPaceChange.Should().Be(PaceHint.SlowDown);
    }

    [Fact]
    public void GenerateHint_WithFlowState_SuggestsPaceSpeedUp_WhenExcelling()
    {
        var outcomes = Enumerable.Repeat(CardOutcome.Completed, 3).ToList().AsReadOnly();
        var diffs = Enumerable.Repeat(Difficulty.Hard, 3).ToList().AsReadOnly();
        var flow = new FlowState(Difficulty.Hard, FlowPace.Normal);

        var hint = _engine.GenerateHint(Male(), MakeCtx(outcomes, diffs, flow));

        hint!.SuggestedPaceChange.Should().Be(PaceHint.SpeedUp);
    }

    // ── Controller integration — NextTurnHintEvent ────────────────────────────

    [Fact]
    public void Controller_EmitsNextTurnHintEvent_AfterStruggling()
    {
        var cards = TestFactory.MakeCards(10, Difficulty.Hard);
        var alice = TestFactory.MakePlayer("Alice", gender: "female");
        var ctrl = TestFactory.BuildController(cards, [alice]);

        var hints = new List<NextTurnHintEvent>();
        ctrl.NextTurnHint += (_, e) => hints.Add(e);
        ctrl.Start();

        // Fail twice to trigger Struggling rule
        ctrl.RecordOutcome(CardOutcome.Failed);
        ctrl.RecordOutcome(CardOutcome.Failed);

        hints.Should().NotBeEmpty();
        hints.Should().Contain(h => h.Reason == "Struggling");
    }

    [Fact]
    public void Controller_HintText_IsGenderResolvedForPlayer()
    {
        var cards = TestFactory.MakeCards(10, Difficulty.Hard);
        var bob = TestFactory.MakePlayer("Bob", gender: "male");
        var ctrl = TestFactory.BuildController(cards, [bob]);

        string? capturedHintText = null;
        ctrl.NextTurnHint += (_, e) => capturedHintText = e.HintText;
        ctrl.Start();
        ctrl.RecordOutcome(CardOutcome.Failed);
        ctrl.RecordOutcome(CardOutcome.Failed);

        capturedHintText.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Controller_NoHintOnFirstCard()
    {
        var cards = TestFactory.MakeCards(10);
        var ctrl = TestFactory.BuildController(cards);
        var hints = new List<NextTurnHintEvent>();
        ctrl.NextTurnHint += (_, e) => hints.Add(e);

        ctrl.Start();
        ctrl.RecordOutcome(CardOutcome.Completed); // first card

        // No hint after single completion
        hints.Should().NotContain(h => h.Reason == "Struggling" || h.Reason == "Excelling");
    }

    [Fact]
    public void Controller_CustomHintEngine_IsUsed()
    {
        var customEngine = new AlwaysStrugglingHintEngine();
        var cards = TestFactory.MakeCards(5);
        var ctrl = TestFactory.BuildController(cards, hintEngine: customEngine);

        string? capturedReason = null;
        ctrl.NextTurnHint += (_, e) => capturedReason = e.Reason;
        ctrl.Start();
        ctrl.RecordOutcome(CardOutcome.Completed);

        capturedReason.Should().Be("AlwaysStruggling");
    }

    // ── Fully UI-agnostic: no Console, no WPF, no rendering ──────────────────

    [Fact]
    public void HintEngine_IsFullyTestable_WithoutUI()
    {
        // This test proves the hint engine works without any UI infrastructure:
        // no Console, no WPF, no rendering, no file I/O — pure logic.
        var engine = new DefaultHintEngine();
        var player = Player.Create("TestPlayer");
        var context = new HintContext(
            RecentOutcomes: [CardOutcome.Failed, CardOutcome.Failed],
            RecentDifficulties: [Difficulty.Medium, Difficulty.Medium],
            CurrentFlow: null,
            SkipCount: 0,
            Round: 3,
            Standings: [(player.Id, 0)]);

        var hint = engine.GenerateHint(player, context);

        hint.Should().NotBeNull();
        hint!.NeutralHint.Should().NotBeNullOrEmpty();
        hint.SuggestedDifficulty.Should().Be(Difficulty.Easy);
    }
}

/// <summary>Stub hint engine for testing custom injection.</summary>
internal sealed class AlwaysStrugglingHintEngine : IHintEngine
{
    public NextTurnHint? GenerateHint(IPlayer player, HintContext ctx) =>
        new(
            SuggestedDifficulty: Difficulty.Easy,
            SuggestedPaceChange: PaceHint.SlowDown,
            NeutralHint: "Always struggling (test stub).",
            HimHint: null,
            HerHint: null,
            Urgency: HintUrgency.Strong,
            Reason: "AlwaysStruggling");
}