using TableTop.Games.Couples;
using TableTop.Hosting.Controllers;
using TableTop.Presentation.ViewModels;

namespace TableTop.Tests;

/// <summary>
/// <see cref="MonogamyGameViewModel"/> is the sharpest case in the whole
/// shared-ViewModel set: WinUI's own version froze after every non-doubles
/// turn, silently, and was only found by accident while merging it into this
/// class — nothing here caught it before that. These tests exist so a
/// regression to the old <c>if (HasCard) { controller.Complete(); HasCard =
/// false; }</c> shape fails loudly instead of shipping quietly again.
///
/// Uses a real <see cref="MonogamyController"/> throughout, not a mock — the
/// bug was a synchronous re-entrancy issue in the real event cascade
/// (RecordOutcome → AdvanceToNextPlayer → BeginTurn → DiceRolled → DrawCard →
/// CardReady, all inside one call), and a mocked controller firing events on
/// demand would not reproduce it.
/// </summary>
public sealed class MonogamyGameViewModelTests
{
    private static Player Male(string name = "Adam") => Player.Create(name, attributes: new Dictionary<string, string> { ["gender"] = "male" });
    private static Player Female(string name = "Eve") => Player.Create(name, attributes: new Dictionary<string, string> { ["gender"] = "female" });

    private static MonogamyController RealController(int winningTokens = 200, Random? rng = null) =>
        new(
            [Male(), Female()],
            MonogamyCardBank.All.ToList(),
            winningTokens,
            rng);

    /// <summary>
    /// Finds a seed whose opening roll is doubles, WITHOUT the flaw the
    /// comment on <see cref="Complete_OnTheFirstNonDoublesTurn_LeavesTheNextCardVisible"/>
    /// describes: this probes a raw <see cref="MonogamyController"/> directly,
    /// never wraps the probed controller in a ViewModel, and discards it
    /// entirely once found. The caller builds its own fresh controller with
    /// the returned seed.
    /// </summary>
    private static int FindSeedWithOpeningDoubles()
    {
        for (var seed = 0; seed < 200; seed++)
        {
            using var probe = RealController(rng: new Random(seed));
            var doubles = false;
            probe.DoublesRolled += (_, _) => doubles = true;
            probe.Start();
            if (doubles) return seed;
        }
        throw new InvalidOperationException("No opening-doubles seed found in range 0-199.");
    }

    // ── The bug itself ────────────────────────────────────────────────────────

    [Fact]
    public void Complete_OnTheFirstNonDoublesTurn_LeavesTheNextCardVisible()
    {
        // The exact scenario that froze WinUI: complete a card, land on a
        // non-doubles roll, and the card that CardReady already delivered for
        // the next turn must still be there afterwards — HasCard must not be
        // wiped by the action that just finished.
        //
        // Was a search loop that built a throwaway MonogamyGameViewModel per
        // seed to probe AwaitingZone, kept the winning seed's controller, then
        // built a SECOND ViewModel around that already-used controller. Real
        // Windows run caught what that misses: the first (discarded) ViewModel
        // already consumed the controller's one-time CardReady firing, so the
        // second ViewModel's HasCard never got set at all — failing before
        // Complete() was even called. Seed 1 was already confirmed non-doubles
        // empirically while writing this file; using it directly needs only
        // one ViewModel, avoiding the flaw rather than working around it.
        using var ctrl = RealController(rng: new Random(1));
        var vm = new MonogamyGameViewModel(new FakeNavigator(), ctrl);
        vm.AwaitingZone.Should().BeFalse("seed 1's opening roll is confirmed non-doubles");
        vm.HasCard.Should().BeTrue("a card must be ready before completing");

        vm.Complete();

        vm.HasCard.Should().BeTrue(
            "the OLD bug wiped HasCard here even though CardReady had already fired for the next card");
    }

    [Fact]
    public void Skip_Negotiate_AndComplete_AllLeaveTheNextCardVisible()
    {
        // Every action routes through the same Submit() gate — confirm all
        // three, not just Complete.
        foreach (Action<MonogamyGameViewModel> action in new Action<MonogamyGameViewModel>[]
        {
            vm => vm.Complete(),
            vm => vm.Skip(),
            vm => vm.Negotiate(),
        })
        {
            using var ctrl = RealController(rng: new Random(1));
            var vm = new MonogamyGameViewModel(new FakeNavigator(), ctrl);
            var hadCardBefore = vm.HasCard;

            action(vm);

            if (hadCardBefore)
                vm.HasCard.Should().BeTrue("every action must leave the next card visible, not just Complete");
        }
    }

    [Fact]
    public void Submit_IsReentrancyGuarded_ATrueNestedCallDoesNotDoubleSubmit()
    {
        // Calling Complete() again AFTER the first call has already returned
        // is not re-entrancy — by then _submitting has reset in Submit()'s
        // finally block, and the second call is a legitimate new turn. That
        // was the first version of this test, and it was wrong: it "failed"
        // by completing two genuinely separate cards, which is correct
        // behaviour, not a re-entrancy bug — caught by executing it, not by
        // reasoning about what the guard does.
        //
        // The actual danger, per Submit()'s own doc comment: CompleteCard()
        // is synchronous, so by the time the outer call returns it has
        // already run the whole RecordOutcome → ... → CardReady cascade and
        // set HasCard = true for the NEXT card. _submitting exists to block a
        // call that arrives WHILE that cascade is still on the stack — so
        // this test creates a genuine nested call, from inside an event the
        // outer Complete() itself raises.
        using var ctrl = RealController(rng: new Random(1));
        var vm = new MonogamyGameViewModel(new FakeNavigator(), ctrl);

        var completions = 0;
        var reentered = false;
        ctrl.TokensAwarded += (_, _) =>
        {
            completions++;
            if (!reentered) { reentered = true; vm.Complete(); } // fires DURING the outer call
        };

        vm.Complete();

        completions.Should().Be(1, "a call arriving while the first is still on the stack must be blocked");
    }

    // ── Basic wiring, real controller ────────────────────────────────────────

    [Fact]
    public void Constructor_StartsTheControllerAndShowsTheFirstCard()
    {
        // Seeded for the same reason as OnTokensAwarded above: HasCard and
        // CardTitle are only set once a card is actually ready, which does
        // not happen on the opening turn if that roll happens to be doubles.
        using var ctrl = RealController(rng: new Random(1));
        var vm = new MonogamyGameViewModel(new FakeNavigator(), ctrl);

        vm.HasCard.Should().BeTrue();
        vm.CardTitle.Should().NotBeEmpty();
        vm.PlayerName.Should().NotBeEmpty();
    }

    [Fact]
    public void ZoneName_MatchesZone_AfterACardIsReady()
    {
        using var ctrl = RealController();
        var vm = new MonogamyGameViewModel(new FakeNavigator(), ctrl);

        vm.ZoneName.Should().Be(vm.Zone.ToString());
    }

    [Fact]
    public void OnTokensAwarded_UpdatesScoresText_AndHasScores()
    {
        // Seed fixed rather than random — an unseeded controller here is
        // flaky (confirmed empirically: fails ~10% of the time when the
        // opening roll happens to be doubles, since Complete() is then a
        // no-op with no card pending yet).
        using var ctrl = RealController(rng: new Random(1));
        var vm = new MonogamyGameViewModel(new FakeNavigator(), ctrl);
        vm.HasScores.Should().BeFalse("nothing awarded yet");

        vm.Complete();

        vm.HasScores.Should().BeTrue();
        vm.Scores.Should().NotBeEmpty(
            "HasScores being true and Scores being empty would itself be inconsistent state");
    }

    [Fact]
    public void ChooseZone_IgnoredWhenNotAwaitingAChoice()
    {
        using var ctrl = RealController(rng: new Random(1));
        var vm = new MonogamyGameViewModel(new FakeNavigator(), ctrl);
        if (vm.AwaitingZone) return; // this scenario needs a non-doubles state; skip if we rolled doubles

        var act = () => vm.ChooseZone(MonogamyZone.Wild);
        act.Should().NotThrow();
    }

    [Fact]
    public void ZoneOption_SelectCommand_ChoosesTheZone()
    {
        // WinUI's MonogamyGameView.xaml binds Command="{Binding SelectCommand}"
        // inside the ZoneChoices ItemsControl's DataTemplate. ZoneOption had no
        // such property until this fix — the binding resolved to nothing, so
        // WinUI's zone-choice buttons after a doubles roll were silently
        // inert. check-xaml-bindings.py did not catch it: it pools every
        // property name declared ANYWHERE in the codebase into one set rather
        // than resolving per-DataContext-type, and MillionaireGameViewModel's
        // unrelated AnswerOption.SelectCommand put "SelectCommand" in that
        // pool, so the name looked resolvable even though ZoneOption itself
        // never declared it.
        var seed = FindSeedWithOpeningDoubles();
        using var ctrl = RealController(rng: new Random(seed));
        var vm = new MonogamyGameViewModel(new FakeNavigator(), ctrl);

        vm.AwaitingZone.Should().BeTrue("the seed was chosen for an opening doubles roll");
        vm.ZoneChoices.Should().NotBeEmpty();

        var option = vm.ZoneChoices[0];
        option.SelectCommand.CanExecute(null).Should().BeTrue();

        option.SelectCommand.Execute(null);

        vm.AwaitingZone.Should().BeFalse("choosing a zone resolves the doubles prompt and deals the card");
        vm.HasCard.Should().BeTrue();
    }

    [Fact]
    public void ZoneOption_SelectCommandAndInvoke_ChooseTheSameZone()
    {
        // MAUI's code-behind calls Invoke() directly (MonogamyGamePage.xaml.cs's
        // OnZoneClicked); WinUI binds SelectCommand. Both must land on the same
        // zone for the same choice, matching AnswerOption's duality in
        // MillionaireGameViewModel — two fresh controllers on the same seed so
        // the doubles roll and the offered zones are identical, one driven
        // through each path.
        var seed = FindSeedWithOpeningDoubles();

        using var ctrlForCommand = RealController(rng: new Random(seed));
        var vmForCommand = new MonogamyGameViewModel(new FakeNavigator(), ctrlForCommand);
        var chosenZone = vmForCommand.ZoneChoices[0].Zone;
        vmForCommand.ZoneChoices[0].SelectCommand.Execute(null);

        using var ctrlForInvoke = RealController(rng: new Random(seed));
        var vmForInvoke = new MonogamyGameViewModel(new FakeNavigator(), ctrlForInvoke);
        vmForInvoke.ZoneChoices[0].Zone.Should().Be(chosenZone, "same seed must offer the same zones in the same order");
        vmForInvoke.ZoneChoices[0].Invoke();

        vmForCommand.Zone.Should().Be(chosenZone);
        vmForInvoke.Zone.Should().Be(chosenZone, "SelectCommand and Invoke must choose the zone the same way");
    }

    [Fact]
    public void BackCommand_QuitsTheControllerAndNavigatesBack()
    {
        var ctrl = RealController();
        var nav = new FakeNavigator();
        var vm = new MonogamyGameViewModel(nav, ctrl);

        vm.BackCommand.Execute(null);

        nav.GoBackCount.Should().Be(1);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var ctrl = RealController();
        var vm = new MonogamyGameViewModel(new FakeNavigator(), ctrl);
        var act = () => vm.Dispose();
        act.Should().NotThrow();
    }

    // ── Load-error path (was MAUI-only; WinUI took the whole app down) ───────

    [Fact]
    public async Task Create_WithAModeThatProvidesNoDeck_SetsLoadErrorInsteadOfThrowing()
    {
        var badMode = new NoDeckMode();
        var act = () => MonogamyGameViewModel.CreateAsync(new FakeNavigator(), badMode, [Male(), Female()]);

        await act.Should().NotThrowAsync("a bad mode must surface as LoadError, matching MAUI's original behaviour");

        var vm = await MonogamyGameViewModel.CreateAsync(new FakeNavigator(), badMode, [Male(), Female()]);
        vm.HasLoadError.Should().BeTrue();
        vm.IsPlaying.Should().BeFalse();
    }

    [Fact]
    public async Task Create_WithALoadError_CommandsAreAllDisabled_NotThrowing()
    {
        var vm = await MonogamyGameViewModel.CreateAsync(new FakeNavigator(), new NoDeckMode(), [Male(), Female()]);

        vm.CompleteCommand.CanExecute(null).Should().BeFalse();
        var act = () => vm.CompleteCommand.Execute(null);
        act.Should().NotThrow("a disabled command's Execute must still be safe to call directly, since MAUI's code-behind calls Complete() rather than the command");
    }

    private sealed class NoDeckMode : TableTop.Core.Abstractions.Game.IGameMode
    {
        public string Name => "No Deck";
        public string Description => "provides no IMonogamyDeckProvider on purpose";
    }
}
