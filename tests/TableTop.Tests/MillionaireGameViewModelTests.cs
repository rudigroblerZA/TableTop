using TableTop.Games;
using TableTop.Hosting.Controllers;
using TableTop.Presentation.ViewModels;
using TableTop.Tests.Helpers;

namespace TableTop.Tests;

/// <summary>Zero test references before this — part of item 2's untested shared-ViewModel set.</summary>
public sealed class MillionaireGameViewModelTests
{
    private static Player Alice(string name = "Alice") => Player.Create(name);

    private static MillionaireController RealController() =>
        new([Alice()], new MillionaireMode().GetQuestionBank());

    [Fact]
    public void Constructor_StartsTheController_AndShowsTheFirstQuestion()
    {
        using var ctrl = RealController();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);

        vm.QuestionText.Should().NotBeEmpty();
        vm.Answers.Should().NotBeEmpty();
        vm.CanInteract.Should().BeTrue();
        vm.IsAnswered.Should().BeFalse();
    }

    [Fact]
    public void AnswerOption_Display_FormatsLabelAndText()
    {
        using var ctrl = RealController();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);

        var first = vm.Answers[0];
        first.Display.Should().StartWith($"{first.Label})");
    }

    [Fact]
    public void Answer_CorrectOrWrong_SetsIsAnswered()
    {
        using var ctrl = RealController();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);
        var anyLabel = vm.Answers[0].Label;

        vm.Answer(anyLabel);

        vm.IsAnswered.Should().BeTrue("right or wrong, the question is settled either way");
        vm.CanInteract.Should().BeFalse();
    }

    [Fact]
    public void AnswerOption_Invoke_RoutesThroughTheSameAnswerPath()
    {
        // MAUI's buttons call Invoke() directly; WinUI binds SelectCommand.
        // Both must produce the same effect.
        using var ctrl = RealController();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);

        vm.Answers[0].Invoke();

        vm.IsAnswered.Should().BeTrue();
    }

    [Fact]
    public void AnswerOption_SelectCommand_RoutesThroughTheSameAnswerPath()
    {
        using var ctrl = RealController();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);

        vm.Answers[0].SelectCommand.Execute(null);

        vm.IsAnswered.Should().BeTrue();
    }

    [Fact]
    public void Answer_WhenAlreadyAnswered_IsANoOp()
    {
        using var ctrl = RealController();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);
        var flashesBefore = 0;
        vm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(vm.Flash)) flashesBefore++; };

        vm.Answer(vm.Answers[0].Label);
        var flashAfterFirst = flashesBefore;
        vm.Answer(vm.Answers[0].Label); // already answered — must not settle a second time

        flashesBefore.Should().Be(flashAfterFirst, "CanInteract is false, so a second answer must not be submitted");
    }

    [Fact]
    public void UseLifeline_MarksItUnavailable()
    {
        using var ctrl = RealController();
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
        using var ctrl = RealController();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);
        var lifeline = vm.Lifelines[0];

        lifeline.Invoke();

        lifeline.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void UseLifeline_AlreadyUsed_IsANoOp()
    {
        using var ctrl = RealController();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);
        vm.UseLifeline(0);
        var flashAfterFirst = vm.Flash;

        vm.UseLifeline(0); // already spent

        vm.Flash.Should().Be(flashAfterFirst, "an already-used lifeline must not fire a second narrative");
    }

    [Fact]
    public void WalkAway_SetsIsAnswered_AndDisablesFurtherInteraction()
    {
        using var ctrl = RealController();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);

        vm.WalkAway();

        vm.IsAnswered.Should().BeTrue();
        vm.CanInteract.Should().BeFalse();
    }

    [Fact]
    public void WalkAwayCommand_CanExecute_FollowsCanInteract()
    {
        using var ctrl = RealController();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);
        vm.WalkAwayCommand.CanExecute(null).Should().BeTrue();

        vm.WalkAway();

        vm.WalkAwayCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void BackCommand_CallsNavigatorGoBack()
    {
        using var ctrl = RealController();
        var nav = new FakeNavigator();
        var vm = new MillionaireGameViewModel(nav, ctrl);

        vm.BackCommand.Execute(null);

        nav.GoBackCount.Should().Be(1);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var ctrl = RealController();
        var vm = new MillionaireGameViewModel(new FakeNavigator(), ctrl);
        var act = () => vm.Dispose();
        act.Should().NotThrow();
    }

    // ── CreateAsync / load-error path ────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WithAMillionaireMode_BuildsARealController()
    {
        var vm = await MillionaireGameViewModel.CreateAsync(
            new FakeNavigator(), new MillionaireMode(), [Alice()]);

        vm.HasLoadError.Should().BeFalse();
        vm.QuestionText.Should().NotBeEmpty();
        vm.Dispose();
    }

    [Fact]
    public async Task CreateAsync_WithAnUnsupportedMode_SetsLoadErrorInsteadOfThrowing()
    {
        var vm = await MillionaireGameViewModel.CreateAsync(
            new FakeNavigator(), new NotMillionaireMode(), [Alice()]);

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
