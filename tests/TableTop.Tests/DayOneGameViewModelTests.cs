using TableTop.Games.Couples;
using TableTop.Hosting.Controllers;
using TableTop.Presentation.ViewModels;

namespace TableTop.Tests;

/// <summary>
/// Zero test references before this — part of item 2's untested shared-ViewModel set.
///
/// <see cref="DayOneController"/> persists to disk, and without an explicit
/// unique path it uses a shared default location — confirmed the hard way,
/// by running several tests in one process and watching later ones inherit
/// earlier ones' completed-day state. <see cref="DayOneControllerTests"/> in
/// <c>NewArchetypeModesTests.cs</c> already documents this exact trap for the
/// controller directly; it applies identically one layer up, through the
/// ViewModel. Every test below gets its own tmp file, cleaned up via
/// <see cref="IDisposable"/>.
/// </summary>
public sealed class DayOneGameViewModelTests : IDisposable
{
    private readonly List<string> _createdFiles = [];

    private string NewTmpFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"dayone-vm-test-{Guid.NewGuid():N}.json");
        _createdFiles.Add(path);
        return path;
    }

    public void Dispose()
    {
        foreach (var f in _createdFiles) if (File.Exists(f)) File.Delete(f);
    }

    private static Player Alice(string name = "Alice") => Player.Create(name);

    private DayOneController RealController(MutableClock? clock = null) =>
        new(
            new DayOneMode().GetDailyDeck(),
            [Alice()],
            "Day One",
            clock ?? new MutableClock(),
            NewTmpFile());

    [Fact]
    public void Constructor_StartsTheController_AndShowsDayOne()
    {
        using var ctrl = RealController();
        var vm = new DayOneGameViewModel(new FakeNavigator(), ctrl);

        vm.HasCard.Should().BeTrue();
        vm.DayLabel.Should().Contain("1");
        vm.CardTitle.Should().NotBeEmpty();
    }

    [Fact]
    public void CompleteToday_AdvancesToTheCaughtUpState()
    {
        // Day 2 has not unlocked yet — the clock has not advanced — so
        // completing today's card must leave the player caught up, not
        // showing a second card immediately.
        using var ctrl = RealController();
        var vm = new DayOneGameViewModel(new FakeNavigator(), ctrl);

        vm.CompleteToday();

        vm.HasCard.Should().BeFalse("day 2 has not unlocked yet");
        vm.StatusText.Should().NotBeEmpty();
    }

    [Fact]
    public void CompleteToday_WhenNoCardIsPending_IsANoOp()
    {
        using var ctrl = RealController();
        var vm = new DayOneGameViewModel(new FakeNavigator(), ctrl);
        vm.CompleteToday(); // now caught up, HasCard is false

        var statusBefore = vm.StatusText;
        var act = () => vm.CompleteToday(); // nothing pending

        act.Should().NotThrow();
        vm.StatusText.Should().Be(statusBefore, "a no-op must not change the caught-up message");
    }

    [Fact]
    public void CompleteTodayCommand_CanExecute_FollowsHasCard()
    {
        using var ctrl = RealController();
        var vm = new DayOneGameViewModel(new FakeNavigator(), ctrl);
        vm.CompleteTodayCommand.CanExecute(null).Should().BeTrue();

        vm.CompleteToday();

        vm.CompleteTodayCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void AdvancingTheClockPastADay_ThenRestartingTheController_UnlocksTheNextCard()
    {
        // DayOneController has no dedicated "recheck" method — Evaluate() only
        // runs from Start() or CompleteToday(), which matches its real use
        // case: the app reopens the next day and calls Start() fresh. Verified
        // that the already-subscribed ViewModel picks up the re-evaluation
        // through the same event handlers, rather than assumed.
        var clock = new MutableClock();
        using var ctrl = RealController(clock);
        var vm = new DayOneGameViewModel(new FakeNavigator(), ctrl);
        vm.CompleteToday(); // caught up, waiting for day 2

        clock.Advance(TimeSpan.FromDays(1.5));
        ctrl.Start(); // the real-world equivalent of reopening the app the next day

        vm.HasCard.Should().BeTrue();
        vm.DayLabel.Should().Contain("2");
    }

    [Fact]
    public void BackCommand_CallsNavigatorGoBack()
    {
        using var ctrl = RealController();
        var nav = new FakeNavigator();
        var vm = new DayOneGameViewModel(nav, ctrl);

        vm.BackCommand.Execute(null);

        nav.GoBackCount.Should().Be(1);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var ctrl = RealController();
        var vm = new DayOneGameViewModel(new FakeNavigator(), ctrl);
        var act = () => vm.Dispose();
        act.Should().NotThrow();
    }

    // ── CreateAsync / load-error path ────────────────────────────────────────
    //
    // ControllerFactory builds DayOneController with no filePath override, so
    // it falls back to a fixed, mode-name-scoped default
    // (AppContext.BaseDirectory + "dayone-{slug}.json") — shared by every
    // CreateAsync call for a mode of this name, across test runs, not just
    // within one. Confirmed by reading DayOneController's constructor, not
    // assumed. The test below cleans that known path up on both sides so it
    // cannot inherit — or leave behind — completed-day state for any other
    // test that happens to use a same-named mode through the real factory.

    private static string DefaultPersistPathFor(string modeName)
    {
        var slug = new string(modeName.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        return Path.Combine(AppContext.BaseDirectory, $"dayone-{slug}.json");
    }

    [Fact]
    public async Task CreateAsync_WithADayOneMode_BuildsARealController()
    {
        var path = DefaultPersistPathFor(new DayOneMode().Name);
        if (File.Exists(path)) File.Delete(path);
        try
        {
            var vm = await DayOneGameViewModel.CreateAsync(new FakeNavigator(), new DayOneMode(), [Alice()]);

            vm.HasLoadError.Should().BeFalse();
            vm.HasCard.Should().BeTrue();
            vm.Dispose();
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task CreateAsync_WithAnUnsupportedMode_SetsLoadErrorInsteadOfThrowing()
    {
        var vm = await DayOneGameViewModel.CreateAsync(new FakeNavigator(), new NotDayOneMode(), [Alice()]);

        vm.HasLoadError.Should().BeTrue();
    }

    private sealed class NotDayOneMode : TableTop.Core.Abstractions.Game.IGameMode, TableTop.Core.Abstractions.Game.IGameModeDefinition
    {
        public string Name => "Not Day One";
        public string Description => "test";
        public IReadOnlyList<TableTop.Core.Abstractions.Cards.ICard> GetCards(IReadOnlyList<TableTop.Core.Abstractions.Players.IPlayer> players) => [];
        public TableTop.Core.Abstractions.Scoring.IScoringStrategy GetScoring() => new TableTop.Core.Domain.Scoring.FixedScoringStrategy(1);
        public IEnumerable<TableTop.Core.Abstractions.Rules.IRule> GetRules() => [];
    }
}
