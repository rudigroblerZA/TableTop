using TableTop.Games;
using TableTop.Hosting.Controllers;
using TableTop.Presentation.ViewModels;

namespace TableTop.Tests;

/// <summary>Zero test references before this — part of item 2's untested shared-ViewModel set.</summary>
public sealed class MillionaireGameViewModelTests
{
    private static Player Alice(string name = "Alice") => Player.Create(name);

    /// <summary>
    /// A controller over a fixed bank whose correct answer is always D — so
    /// <c>Answers[0]</c> (label A) is reliably WRONG.
    ///
    /// <para>
    /// This existed as <c>RealController()</c>, over the live
    /// <see cref="MillionaireMode"/> bank, and was the cause of backlog item 22.
    /// <c>MillionaireController.BuildQuestionPool</c> orders by difficulty and
    /// then by <c>Random.Shared.Next()</c>, so which question reached rung one
    /// — and therefore whether <c>Answers[0]</c> happened to be the correct
    /// one — changed every run.
    /// </para>
    ///
    /// <para>
    /// That mattered because a CORRECT answer does not leave the question
    /// settled: <c>SubmitAnswer</c> raises <c>AnswerCorrect</c> (the ViewModel
    /// sets <c>IsAnswered = true</c>) and then calls <c>LoadNextQuestion</c>,
    /// whose <c>QuestionReady</c> handler sets <c>IsAnswered = false</c> again
    /// for the new question. Measured on the live bank, the first option was
    /// correct in 22 of 400 runs, which is what made three tests here fail
    /// roughly one suite run in three — a different one each time.
    /// </para>
    ///
    /// <para>
    /// Answer with <see cref="WrongLabel"/> to settle a question, or
    /// <see cref="CorrectLabel"/> to advance the ladder. Deciding which path
    /// you are on is now the test's job, not the shuffle's.
    /// </para>
    /// </summary>
    private static MillionaireController Controller() =>
        new([Alice()], FixedBank);

    /// <summary>The label that is correct in <see cref="FixedBank"/>.</summary>
    private const AnswerLabel CorrectLabel = AnswerLabel.D;

    /// <summary>The label at <c>Answers[0]</c>, and deliberately not the correct one.</summary>
    private const AnswerLabel WrongLabel = AnswerLabel.A;

    /// <summary>
    /// Enough questions at every difficulty for the ladder to keep loading
    /// them: <c>PickQuestion</c> maps rungs 1–5 to Easy, 6–10 Medium, 11–14
    /// Hard, 15 Extreme, and falls back to any remaining card.
    /// </summary>
    private static IReadOnlyList<MultipleChoiceCard> FixedBank { get; } =
    [
        Q("Fixed easy question",    Difficulty.Easy),
        Q("Fixed easy question 2",  Difficulty.Easy),
        Q("Fixed medium question",  Difficulty.Medium),
        Q("Fixed hard question",    Difficulty.Hard),
        Q("Fixed extreme question", Difficulty.Extreme),
    ];

    private static MultipleChoiceCard Q(string question, Difficulty difficulty) =>
        new(Guid.NewGuid(),
            question,
            "Fixture question — the correct answer is always D.",
            new Dictionary<AnswerLabel, string>
            {
                [AnswerLabel.A] = "Wrong",
                [AnswerLabel.B] = "Also wrong",
                [AnswerLabel.C] = "Still wrong",
                [AnswerLabel.D] = "Correct",
            },
            CorrectLabel,
            difficulty);

    /// <summary>
    /// The option a test should click to settle the current question: the
    /// wrong one. Named rather than indexed, because <c>Answers[0]</c> reading
    /// as "the one that ends the round" is precisely the assumption that broke.
    /// </summary>
    private static MillionaireGameViewModel.AnswerOption WrongOption(MillionaireGameViewModel vm) =>
        vm.Answers.Single(a => a.Label == WrongLabel);

    [Fact]
    public void Constructor_StartsTheController_AndShowsTheFirstQuestion()
    {
        using var ctrl = Controller();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);

        vm.QuestionText.Should().NotBeEmpty();
        vm.Answers.Should().NotBeEmpty();
        vm.CanInteract.Should().BeTrue();
        vm.IsAnswered.Should().BeFalse();
    }

    [Fact]
    public void AnswerOption_Display_FormatsLabelAndText()
    {
        using var ctrl = Controller();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);

        var first = vm.Answers[0];
        first.Display.Should().StartWith($"{first.Label})");
    }

    [Fact]
    public void Answer_Wrong_SettlesTheQuestion()
    {
        using var ctrl = Controller();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);

        vm.Answer(WrongLabel);

        vm.IsAnswered.Should().BeTrue("a wrong answer ends the round");
        vm.CanInteract.Should().BeFalse();
    }

    [Fact]
    public void Answer_Correct_AdvancesToTheNextQuestion_AndReopensInteraction()
    {
        // The other half of the pair, and the behaviour that used to arrive by
        // accident: a correct answer settles the question and then immediately
        // loads the next one, which reopens the screen for input. Asserting it
        // deliberately means the next person to read IsAnswered knows both
        // outcomes are intended rather than inferring one from a flaky run.
        using var ctrl = Controller();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);
        var firstQuestion = vm.QuestionText;

        vm.Answer(CorrectLabel);

        vm.QuestionText.Should().NotBe(firstQuestion, "the ladder advances to a new question");
        vm.IsAnswered.Should().BeFalse("the new question has not been answered");
        vm.CanInteract.Should().BeTrue();
    }

    [Fact]
    public void AnswerOption_Invoke_RoutesThroughTheSameAnswerPath()
    {
        // MAUI's buttons call Invoke() directly; WinUI binds SelectCommand.
        // Both must produce the same effect.
        using var ctrl = Controller();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);

        WrongOption(vm).Invoke();

        vm.IsAnswered.Should().BeTrue();
    }

    [Fact]
    public void AnswerOption_SelectCommand_RoutesThroughTheSameAnswerPath()
    {
        using var ctrl = Controller();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);

        WrongOption(vm).SelectCommand.Execute(null);

        vm.IsAnswered.Should().BeTrue();
    }

    [Fact]
    public void Answer_WhenAlreadyAnswered_IsANoOp()
    {
        using var ctrl = Controller();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);
        var flashesBefore = 0;
        vm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(vm.Flash)) flashesBefore++; };

        // Wrong on purpose: a correct answer would load the next question and
        // reopen interaction, so the second call would legitimately be accepted
        // and this test would be asserting nothing.
        vm.Answer(WrongLabel);
        var flashAfterFirst = flashesBefore;
        vm.Answer(WrongLabel); // already answered — must not settle a second time

        flashesBefore.Should().Be(flashAfterFirst, "CanInteract is false, so a second answer must not be submitted");
    }

    [Fact]
    public void UseLifeline_MarksItUnavailable()
    {
        using var ctrl = Controller();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);
        vm.Lifelines.Should().NotBeEmpty();
        var lifeline = vm.Lifelines[0];
        lifeline.IsAvailable.Should().BeTrue();

        vm.UseLifeline(0);

        lifeline.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void LifelineOption_Invoke_RoutesThroughTheSamePath()
    {
        using var ctrl = Controller();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);
        var lifeline = vm.Lifelines[0];

        lifeline.Invoke();

        lifeline.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void UseLifeline_AlreadyUsed_IsANoOp()
    {
        using var ctrl = Controller();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);
        vm.UseLifeline(0);
        var flashAfterFirst = vm.Flash;

        vm.UseLifeline(0); // already spent

        vm.Flash.Should().Be(flashAfterFirst, "an already-used lifeline must not fire a second narrative");
    }

    [Fact]
    public void WalkAway_SetsIsAnswered_AndDisablesFurtherInteraction()
    {
        using var ctrl = Controller();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);

        vm.WalkAway();

        vm.IsAnswered.Should().BeTrue();
        vm.CanInteract.Should().BeFalse();
    }

    [Fact]
    public void WalkAwayCommand_CanExecute_FollowsCanInteract()
    {
        using var ctrl = Controller();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);
        vm.WalkAwayCommand.CanExecute(null).Should().BeTrue();

        vm.WalkAway();

        vm.WalkAwayCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void BackCommand_CallsNavigatorGoBack()
    {
        using var ctrl = Controller();
        var nav = new FakeNavigator();
        var vm = new MillionaireGameViewModel(nav, ctrl);

        vm.BackCommand.Execute(null);

        nav.GoBackCount.Should().Be(1);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var ctrl = Controller();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);
        var act = () => vm.Dispose();
        act.Should().NotThrow();
    }

    // ── CreateAsync / load-error path ────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WithAMillionaireMode_BuildsARealController()
    {
        var vm = await MillionaireGameViewModel.CreateAsync(
            new FakeNavigator(), new MillionaireMode(), [Alice()], TestFactory.PlainControllerFactory());

        vm.HasLoadError.Should().BeFalse();
        vm.QuestionText.Should().NotBeEmpty();
        vm.Dispose();
    }

    [Fact]
    public async Task CreateAsync_WithAnUnsupportedMode_SetsLoadErrorInsteadOfThrowing()
    {
        var vm = await MillionaireGameViewModel.CreateAsync(
            new FakeNavigator(), new NotMillionaireMode(), [Alice()], TestFactory.PlainControllerFactory());

        vm.HasLoadError.Should().BeTrue();
        vm.IsPlaying.Should().BeFalse();
    }

    private sealed class NotMillionaireMode : TableTop.Core.Abstractions.Game.IGameMode, TableTop.Core.Abstractions.Game.IGameModeDefinition
    {
        public string Name => "Not Millionaire";
        public string Description => "test";
        public IReadOnlyList<TableTop.Core.Abstractions.Cards.ICard> GetCards(IReadOnlyList<TableTop.Core.Abstractions.Players.IPlayer> players) => [];
        public TableTop.Core.Abstractions.Scoring.IScoringStrategy GetScoring() => new TableTop.Core.Domain.Scoring.FixedScoringStrategy(1);
        public IEnumerable<TableTop.Core.Abstractions.Rules.IRule> GetRules() => [];
    }
}
