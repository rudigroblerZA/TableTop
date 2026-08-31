using TableTop.Core.Abstractions.Analysis;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Analysis;
using TableTop.Hosting.Controllers;
using TableTop.Presentation.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Tests;

/// <summary>
/// The shared trait-assessment screen, which WinUI, MAUI and the native Android
/// head all drive.
///
/// <para>
/// The tests that earn their place are the ones covering the re-entrancy
/// documented on the ViewModel: <c>SubmitResponses</c> raises
/// <c>ItemRecorded</c> and then advances inside the same call, so
/// <c>OnItemReady</c> and <c>OnAssessmentCompleted</c> can both run before
/// <see cref="TraitProfileGameViewModel.Submit"/> returns. Strict ownership is
/// what makes that safe, and it is invisible unless asserted.
/// </para>
/// </summary>
public sealed class TraitProfileGameViewModelTests
{
    private const string T = "Trait";

    private static TraitScale Scale =>
        new("Test", [new TraitDefinition(T, "Trait", "low end", "high end", "d")]);

    private static IReadOnlyList<IPlayer> Players(params string[] names)
    {
        var roster = names.Length > 0 ? names : new[] { "Ada", "Bo" };
        return roster.Select(n => (IPlayer)Player.Create(n)).ToList().AsReadOnly();
    }

    private static IReadOnlyList<TraitItemCard> Items(int count) =>
        Enumerable.Range(0, count).Select(i => TraitItemCard.Single($"statement {i}", T)).ToList();

    private static TraitProfileGameViewModel Build(int items = 3, params string[] names) =>
        new(new FakeNavigator(),
            new TraitProfileController(Players(names), Scale, Items(items)));

    private static void AnswerAll(TraitProfileGameViewModel vm, LikertResponse response)
    {
        foreach (var entry in vm.PlayerResponses) entry.Response = response;
    }

    // ── Flow ─────────────────────────────────────────────────────────────────

    [Fact]
    public void ConstructionStartsTheSessionAndShowsTheFirstStatement()
    {
        using var vm = Build();

        vm.ItemNumber.Should().Be(1);
        vm.TotalItems.Should().Be(3);
        vm.Statement.Should().NotBeEmpty();
        vm.IsPlaying.Should().BeTrue();
        vm.PlayerResponses.Should().HaveCount(2);
    }

    [Fact]
    public void SubmittingAdvancesToTheNextStatement_AndClearsTheResponses()
    {
        // The re-entrancy case. OnItemReady runs inside Submit, rebuilding the
        // entries — so a stale answer must not survive into the next statement.
        using var vm = Build(items: 3);
        var first = vm.Statement;

        AnswerAll(vm, LikertResponse.Agree);
        vm.Submit();

        vm.ItemNumber.Should().Be(2);
        vm.Statement.Should().NotBe(first);
        vm.PlayerResponses.Should().OnlyContain(r => r.Response == null);
        vm.AnyAnswered.Should().BeFalse();
    }

    [Fact]
    public void AnsweringEveryStatement_EndsInResults()
    {
        using var vm = Build(items: 2, names: ["Ada"]);

        for (var i = 0; i < 2; i++) { AnswerAll(vm, LikertResponse.StronglyAgree); vm.Submit(); }

        vm.IsComplete.Should().BeTrue();
        vm.IsPlaying.Should().BeFalse();
        vm.Profiles.Should().ContainSingle();
        vm.Profiles[0].Scores.Should().ContainSingle()
            .Which.Normalized.Should().Be(100d);
    }

    [Fact]
    public void SkipAdvancesWithoutRecording()
    {
        using var vm = Build(items: 2, names: ["Ada"]);

        vm.Skip();
        AnswerAll(vm, LikertResponse.StronglyAgree);
        vm.Submit();

        vm.IsComplete.Should().BeTrue();
        vm.Profiles.Should().ContainSingle().Which.AnsweredItems.Should().Be(1);
    }

    [Fact]
    public void ProgressTracksThroughTheBank()
    {
        using var vm = Build(items: 4, names: ["Ada"]);

        vm.Progress.Should().Be(0.25);
        vm.ProgressLabel.Should().Be("1 / 4");

        AnswerAll(vm, LikertResponse.Agree);
        vm.Submit();

        vm.Progress.Should().Be(0.5);
        vm.ProgressLabel.Should().Be("2 / 4");
    }

    // ── Submit guard ─────────────────────────────────────────────────────────

    [Fact]
    public void SubmitIsDisabledUntilSomeoneAnswers()
    {
        using var vm = Build();

        vm.SubmitCommand.CanExecute(null).Should().BeFalse("nobody has answered yet");

        vm.PlayerResponses[0].Response = LikertResponse.Agree;

        vm.AnyAnswered.Should().BeTrue();
        vm.AllAnswered.Should().BeFalse("only one of two answered");
        vm.SubmitCommand.CanExecute(null).Should().BeTrue();

        vm.PlayerResponses[1].Response = LikertResponse.Disagree;
        vm.AllAnswered.Should().BeTrue();
    }

    [Fact]
    public void PickCommandAcceptsTheStringXamlSends()
    {
        // WinUI and MAUI both pass CommandParameter="3" as a string. A
        // RelayCommand<int> would bind and then silently never execute.
        using var vm = Build(names: ["Ada"]);
        var entry = vm.PlayerResponses[0];

        entry.PickCommand.Execute("3");
        entry.Response.Should().Be(LikertResponse.Neutral);
        entry.SelectedValue.Should().Be(3);
        entry.HasAnswered.Should().BeTrue();

        // The native Android head passes a real int from code.
        entry.PickCommand.Execute(5);
        entry.Response.Should().Be(LikertResponse.StronglyAgree);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void PickIgnoresValuesOutsideTheScale(int value)
    {
        using var vm = Build(names: ["Ada"]);
        var entry = vm.PlayerResponses[0];

        entry.Pick(3);
        entry.Pick(value);

        entry.Response.Should().BeNull("an out-of-range pick clears rather than keeps a stale answer");
    }

    [Fact]
    public void AMalformedCommandParameterIsAnIgnoredTapRatherThanACrash()
    {
        using var vm = Build(names: ["Ada"]);
        var entry = vm.PlayerResponses[0];

        var act = () => entry.PickCommand.Execute("not a number");

        act.Should().NotThrow();
        entry.Response.Should().BeNull();
    }

    [Fact]
    public void ParameterRelayCommand_AsIntHandlesTheThreeShapesItSees()
    {
        ParameterRelayCommand.AsInt(4).Should().Be(4);
        ParameterRelayCommand.AsInt("4").Should().Be(4);
        ParameterRelayCommand.AsInt(null).Should().Be(0);
        ParameterRelayCommand.AsInt("nonsense", fallback: -1).Should().Be(-1);
    }

    // ── Results ──────────────────────────────────────────────────────────────

    [Fact]
    public void ResultsCarryTheBandInTheTraitsOwnWords()
    {
        using var vm = Build(items: 1, names: ["Ada"]);

        AnswerAll(vm, LikertResponse.StronglyAgree);
        vm.Submit();

        var row = vm.Profiles[0].Scores[0];
        row.TraitName.Should().Be("Trait");
        row.Rounded.Should().Be(100);
        row.Fraction.Should().Be(1d);
        row.BandLabel.Should().Be("high end", "the band renders as the trait's own label, not 'VeryHigh'");
        row.HasData.Should().BeTrue();
    }

    [Fact]
    public void TwoPlayersProduceAComparison_OneDoesNot()
    {
        using var pair = Build(items: 1);
        AnswerAll(pair, LikertResponse.Agree);
        pair.Submit();
        pair.HasComparison.Should().BeTrue();
        pair.ComparisonSummary.Should().Contain("alike");

        using var solo = Build(items: 1, names: ["Ada"]);
        AnswerAll(solo, LikertResponse.Agree);
        solo.Submit();
        solo.HasComparison.Should().BeFalse();
        solo.ComparisonSummary.Should().BeEmpty();
    }

    [Fact]
    public void TheTopTraitIsNamed_WhichIsTheHeadlineForARankingMode()
    {
        using var vm = Build(items: 1, names: ["Ada"]);

        AnswerAll(vm, LikertResponse.StronglyAgree);
        vm.Submit();

        vm.Profiles[0].HasTopTrait.Should().BeTrue();
        vm.Profiles[0].TopTrait.Should().Be("Trait");
    }

    [Fact]
    public void NobodyAnswering_ReportsThatRatherThanAnEmptyScreen()
    {
        using var vm = Build(items: 2, names: ["Ada"]);

        vm.Skip();
        vm.Skip();

        vm.IsComplete.Should().BeTrue();
        vm.Profiles.Should().BeEmpty();
        vm.Summary.Should().Contain("nothing to report");
    }

    [Fact]
    public void BackQuitsTheSessionAndLeavesTheScreen()
    {
        var navigator = new FakeNavigator();
        using var vm = new TraitProfileGameViewModel(
            navigator, new TraitProfileController(Players(), Scale, Items(5)));

        vm.BackCommand.Execute(null);

        navigator.GoBackCount.Should().Be(1);
        vm.IsComplete.Should().BeTrue("quitting ends the session and reports what was answered");
    }

    // ── Failure path ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ABadModeSurfacesAsAMessageRatherThanAnException()
    {
        // The pattern every game ViewModel here follows: a failed controller
        // build used to take the whole app down.
        var vm = await TraitProfileGameViewModel.CreateAsync(
            new FakeNavigator(), new TableTop.Games.Fun.HerdMode(), Players(),
            new ControllerFactory());

        vm.HasLoadError.Should().BeTrue("Herd is not a trait-assessment mode");
        vm.IsPlaying.Should().BeFalse();
        vm.SubmitCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task ARealTraitModeBuildsThroughTheFactory()
    {
        var vm = await TraitProfileGameViewModel.CreateAsync(
            new FakeNavigator(), new TableTop.Games.Couples.LoveLanguagesMode(), Players(),
            new ControllerFactory());

        using var _ = vm;
        vm.HasLoadError.Should().BeFalse();
        vm.TotalItems.Should().Be(40);
        vm.PlayerResponses.Should().HaveCount(2);
    }
}
