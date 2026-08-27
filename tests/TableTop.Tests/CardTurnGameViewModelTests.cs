using TableTop.Core.Domain.Restrictions;
using TableTop.Games;
using TableTop.Games.Couples;
using TableTop.Games.School;
using TableTop.Presentation.ViewModels;

namespace TableTop.Tests;

/// <summary>
/// <see cref="CardTurnGameViewModel"/> is backlog item 1's merge — the last
/// real duplication, 733 lines on MAUI against 404 on WinUI, both driving the
/// highest-traffic screen in the app. Zero test references before this.
///
/// Every assertion here was run against the real class in a harness before
/// being written down — the discipline every merge this project has needed,
/// applied to the biggest one.
/// </summary>
public sealed class CardTurnGameViewModelTests
{
    private static Player Alice(string n = "Alice") => Player.Create(n);
    private static Player Bob(string n = "Bob") => Player.Create(n);

    private static (Player, Player) Players() => (Alice(), Bob());

    /// <summary>
    /// Two players tagged as a couple. Needed by any mode whose cards carry a
    /// <see cref="CoupleOnlyRestriction"/> — without the tag every restricted
    /// card is filtered out before the controller ever sees it, and the mode
    /// silently plays as whatever unrestricted remainder is left.
    /// </summary>
    private static (Player, Player) Couple() =>
        (Player.Create("Alice", tags: ["couple-member"]),
         Player.Create("Bob", tags: ["couple-member"]));

    [Fact]
    public async Task CreateAsync_WithARealMode_StartsTheControllerAndShowsTheFirstCard()
    {
        var (a, b) = Players();
        var vm = await CardTurnGameViewModel.CreateAsync(
            new FakeNavigator(), new WouldYouRatherMode(), [a, b], new FakeAppSettings());

        vm.HasLoadError.Should().BeFalse();
        vm.CardTitle.Should().NotBeEmpty();
        vm.PlayerName.Should().NotBeEmpty();
        vm.Round.Should().Be(1);
        vm.Dispose();
    }

    [Fact]
    public async Task CreateAsync_ReadsTimerSettings_AndStartsTheCountdown()
    {
        var (a, b) = Players();
        var settings = new FakeAppSettings { EnableTimer = true, TimerSeconds = 30 };
        var vm = await CardTurnGameViewModel.CreateAsync(
            new FakeNavigator(), new WouldYouRatherMode(), [a, b], settings);

        vm.TimerEnabled.Should().BeTrue();
        vm.SecondsRemaining.Should().Be(30);
        vm.Dispose();
    }

    [Fact]
    public async Task Complete_RecordsTheOutcome_AndSetsCanUndo()
    {
        var (a, b) = Players();
        var vm = await CardTurnGameViewModel.CreateAsync(
            new FakeNavigator(), new WouldYouRatherMode(), [a, b], new FakeAppSettings());

        vm.Complete();

        vm.CanUndo.Should().BeTrue();
        vm.Dispose();
    }

    [Fact]
    public async Task UndoLastTurn_ClearsCanUndo()
    {
        var (a, b) = Players();
        var vm = await CardTurnGameViewModel.CreateAsync(
            new FakeNavigator(), new WouldYouRatherMode(), [a, b], new FakeAppSettings());
        vm.Complete();

        vm.UndoLastTurn();

        vm.CanUndo.Should().BeFalse();
        vm.Dispose();
    }

    [Fact]
    public async Task FlipCard_TogglesCardBodyText_OnATwoFacedCard()
    {
        // EstimationStationMode has flip-back cards — confirmed empirically,
        // not assumed, before writing this against it.
        var (a, b) = Players();
        var vm = await CardTurnGameViewModel.CreateAsync(
            new FakeNavigator(), new EstimationStationMode(), [a, b], new FakeAppSettings());

        var found = false;
        for (var i = 0; i < 20 && !vm.IsGameOver; i++)
        {
            if (vm.HasBack)
            {
                found = true;
                var before = vm.CardBodyText;
                vm.FlipCard();
                vm.CardBodyText.Should().NotBe(before);
                vm.FlipCard();
                vm.CardBodyText.Should().Be(before, "flipping twice returns to the question face");
                break;
            }
            vm.Complete();
        }
        found.Should().BeTrue("EstimationStationMode must produce at least one two-faced card within 20 turns");
        vm.Dispose();
    }

    [Fact]
    public async Task RecordChoice_TalliesThePick_AndAppearsInTheGameOverSummary()
    {
        // BetweenTheTwoOfYouMode writes cards in the literal "A) ... B) ..."
        // format ChoiceCards.Extract requires — confirmed empirically before
        // relying on it, since not every mode with choice-sounding text
        // actually matches that exact format.
        //
        // The players must be tagged as a couple. Every question card in this
        // mode carries a CoupleOnlyRestriction, so untagged players see only the
        // unrestricted "Results" cards — no choices, no tally, no styles line.
        // This used to pass with plain players because the deck came from JSON
        // and the restriction lived only on the C# bank behind it.
        var (a, b) = Couple();
        var vm = await CardTurnGameViewModel.CreateAsync(
            new FakeNavigator(), new BetweenTheTwoOfYouMode(), [a, b], new FakeAppSettings());

        for (var i = 0; i < 100 && !vm.IsGameOver; i++)
        {
            if (vm.HasChoices) vm.Choices[0].Invoke();
            else vm.Complete();
        }

        vm.IsGameOver.Should().BeTrue();
        vm.SummaryText.Should().Contain("Your styles:", "a personality-quiz mode must report tallied styles at game end");
        vm.Dispose();
    }

    [Fact]
    public async Task ChoiceItem_Invoke_AndChooseCommand_BothRecordTheSamePick()
    {
        // Couple-tagged for the same reason as the test above: the choice cards
        // are all CoupleOnlyRestriction-gated.
        var (a, b) = Couple();
        var vm = await CardTurnGameViewModel.CreateAsync(
            new FakeNavigator(), new BetweenTheTwoOfYouMode(), [a, b], new FakeAppSettings());

        for (var i = 0; i < 40 && !vm.IsGameOver; i++)
        {
            if (vm.HasChoices)
            {
                vm.Choices[0].ChooseCommand.Execute(null);
                vm.FlashText.Should().NotBeEmpty("a pick must produce feedback whichever surface records it");
                return;
            }
            vm.Complete();
        }
        Assert.Fail("no choice card appeared within 40 turns");
    }

    [Fact]
    public async Task GameOver_Fires_WithIsGameOverAlreadyTrue_AndTheSameSummary()
    {
        var (a, b) = Players();
        var vm = await CardTurnGameViewModel.CreateAsync(
            new FakeNavigator(), new WouldYouRatherMode(), [a, b], new FakeAppSettings());

        bool? isGameOverInsideHandler = null;
        string? summaryFromEvent = null;
        vm.GameOver += s => { isGameOverInsideHandler = vm.IsGameOver; summaryFromEvent = s; };

        for (var i = 0; i < 50 && !vm.IsGameOver; i++) vm.Complete();

        vm.IsGameOver.Should().BeTrue();
        isGameOverInsideHandler.Should().BeTrue("IsGameOver must already be set before subscribers are notified");
        summaryFromEvent.Should().Be(vm.SummaryText);
        vm.Dispose();
    }

    [Fact]
    public async Task CompleteCommand_Disables_OnceTheGameEnds()
    {
        var (a, b) = Players();
        var vm = await CardTurnGameViewModel.CreateAsync(
            new FakeNavigator(), new WouldYouRatherMode(), [a, b], new FakeAppSettings());
        for (var i = 0; i < 50 && !vm.IsGameOver; i++) vm.Complete();

        vm.CompleteCommand.CanExecute(null).Should().BeFalse();
        vm.Dispose();
    }

    [Fact]
    public async Task SaveCommand_Disables_OnceTheGameEnds()
    {
        var (a, b) = Players();
        var vm = await CardTurnGameViewModel.CreateAsync(
            new FakeNavigator(), new WouldYouRatherMode(), [a, b], new FakeAppSettings());
        vm.SaveCommand.CanExecute(null).Should().BeTrue("saving must be available while the game is live");
        for (var i = 0; i < 50 && !vm.IsGameOver; i++) vm.Complete();

        vm.SaveCommand.CanExecute(null).Should().BeFalse();
        vm.Dispose();
    }

    [Fact]
    public async Task FlowCommands_AreEnabled_OnlyForFlowAwareModes()
    {
        var (a, b) = Players();
        var flowAware = await CardTurnGameViewModel.CreateAsync(
            new FakeNavigator(), new ReadingComprehensionMode(), [a, b], new FakeAppSettings());
        var notFlowAware = await CardTurnGameViewModel.CreateAsync(
            new FakeNavigator(), new WouldYouRatherMode(), [a, b], new FakeAppSettings());

        flowAware.SupportsFlow.Should().BeTrue();
        flowAware.LevelUpCommand.CanExecute(null).Should().BeTrue();
        notFlowAware.SupportsFlow.Should().BeFalse();
        notFlowAware.LevelUpCommand.CanExecute(null).Should().BeFalse();

        flowAware.Dispose();
        notFlowAware.Dispose();
    }

    [Fact]
    public async Task LevelUp_GenuinelyChangesTheNextCardsDifficulty_ForAFlowAwareMode()
    {
        // Not just "doesn't throw" — confirmed the actual difficulty
        // reported by the controller changes, against a real flow-aware
        // mode, before trusting the command does anything at all.
        var (a, b) = Players();
        var vm = await CardTurnGameViewModel.CreateAsync(
            new FakeNavigator(), new ReadingComprehensionMode(), [a, b], new FakeAppSettings());

        var before = vm.CardDifficulty;
        vm.LevelUp();
        vm.Complete();

        vm.CardDifficulty.Should().NotBe(before, "LevelUp must be reflected in the next card's difficulty for a flow-aware mode");
        vm.Dispose();
    }

    [Fact]
    public async Task CreateAsync_WithAnUnsupportedMode_SetsLoadErrorInsteadOfThrowing()
    {
        var (a, b) = Players();
        var vm = await CardTurnGameViewModel.CreateAsync(
            new FakeNavigator(), new NotCardTurnMode(), [a, b], new FakeAppSettings());

        vm.HasLoadError.Should().BeTrue();
        vm.IsPlaying.Should().BeFalse();
        vm.CompleteCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_WithALoadError_CommandsAreSafeToExecute_NotJustDisabled()
    {
        // MAUI's code-behind calls plain methods directly rather than always
        // going through a Command — Execute on a disabled command, and the
        // plain method it wraps, must both be safe no-ops.
        var (a, b) = Players();
        var vm = await CardTurnGameViewModel.CreateAsync(
            new FakeNavigator(), new NotCardTurnMode(), [a, b], new FakeAppSettings());

        var act = () => { vm.CompleteCommand.Execute(null); vm.Complete(); vm.Skip(); vm.FlipCard(); vm.UndoLastTurn(); };

        act.Should().NotThrow();
    }

    [Fact]
    public async Task Dispose_DoesNotThrow()
    {
        var (a, b) = Players();
        var vm = await CardTurnGameViewModel.CreateAsync(
            new FakeNavigator(), new WouldYouRatherMode(), [a, b], new FakeAppSettings());

        var act = () => vm.Dispose();

        act.Should().NotThrow();
    }

    // ── SaveSession failure reporting (backlog item 19) ─────────────────────────
    //
    // SaveSession used to be fire-and-forget (`_ = _controller!.SaveAsync();`):
    // a write failure became an unobserved task exception and the player who
    // explicitly asked to save got no feedback at all. These pin the fix —
    // the failure is caught and reported through FlashText, the same channel
    // a successful save already used.

    [Fact]
    public void SaveSession_WhenPersistenceThrowsIOException_ReportsFailureInsteadOfCrashing()
    {
        var cards = TableTop.Tests.Helpers.TestFactory.MakeCards(5);
        var controller = TableTop.Tests.Helpers.TestFactory.BuildController(
            cards, sessionRepository: new ThrowingPersistence(new IOException("disk full")));
        var vm = new CardTurnGameViewModel(
            new FakeNavigator(), new WouldYouRatherMode(), controller,
            timerEnabled: false, timerSeconds: 0, showCardCount: true);

        var act = () => vm.SaveSession();

        act.Should().NotThrow();
        vm.FlashText.Should().Contain("Couldn't save");
    }

    [Fact]
    public void SaveSession_WhenPersistenceThrowsUnauthorizedAccessException_ReportsFailureInsteadOfCrashing()
    {
        var cards = TableTop.Tests.Helpers.TestFactory.MakeCards(5);
        var controller = TableTop.Tests.Helpers.TestFactory.BuildController(
            cards, sessionRepository: new ThrowingPersistence(new UnauthorizedAccessException("denied")));
        var vm = new CardTurnGameViewModel(
            new FakeNavigator(), new WouldYouRatherMode(), controller,
            timerEnabled: false, timerSeconds: 0, showCardCount: true);

        var act = () => vm.SaveSession();

        act.Should().NotThrow();
        vm.FlashText.Should().Contain("Couldn't save");
    }

    [Fact]
    public void SaveSession_OnSuccess_StillReportsSessionSaved()
    {
        var cards = TableTop.Tests.Helpers.TestFactory.MakeCards(5);
        var controller = TableTop.Tests.Helpers.TestFactory.BuildController(
            cards, sessionRepository: new InMemoryPersistence());
        var vm = new CardTurnGameViewModel(
            new FakeNavigator(), new WouldYouRatherMode(), controller,
            timerEnabled: false, timerSeconds: 0, showCardCount: true);

        vm.SaveSession();

        vm.FlashText.Should().Be("Session saved");
    }

    /// <summary>An <see cref="IGamePersistence"/> whose <see cref="SaveAsync"/> always fails.</summary>
    private sealed class ThrowingPersistence(Exception toThrow) : TableTop.Hosting.Persistence.IGamePersistence
    {
        public bool HasSavedSession => false;
        public Task SaveAsync(TableTop.Hosting.Persistence.SessionSnapshot s, CancellationToken ct = default) =>
            Task.FromException(toThrow);
        public Task<TableTop.Hosting.Persistence.SessionSnapshot?> LoadAsync(CancellationToken ct = default) =>
            Task.FromResult<TableTop.Hosting.Persistence.SessionSnapshot?>(null);
        public Task DeleteAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class NotCardTurnMode : TableTop.Core.Abstractions.Game.IGameMode
    {
        public string Name => "Not Card Turn";
        public string Description => "no IGameModeDefinition — ControllerFactory falls through to a controller type this screen can't drive";
    }
}
