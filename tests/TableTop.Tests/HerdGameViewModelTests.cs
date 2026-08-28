using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Controllers;
using TableTop.Presentation.ViewModels;

namespace TableTop.Tests;

/// <summary>
/// <see cref="HerdGameViewModel"/> against a real <see cref="HerdController"/> —
/// backlog item 4's WinUI/MAUI screen for the SimultaneousAnswer family.
///
/// <para>
/// The load-bearing test here is <see cref="Reveal_PopulatesLastRoundResult_AndAdvancesToTheNextRoundInTheSameCall"/>:
/// <see cref="IHerdController.SubmitAnswers"/> raises <c>RoundResolved</c> and
/// then <c>PromptReady</c> (or <c>GameEnded</c>) before returning, which is the
/// same synchronous-cascade shape that broke WinUI's old Monogamy screen. This
/// pins that <see cref="HerdGameViewModel"/> keeps the two states separate.
/// </para>
/// </summary>
public sealed class HerdGameViewModelTests
{
    private static IReadOnlyList<IPlayer> Players() =>
        [Player.Create("A"), Player.Create("B"), Player.Create("C")];

    private static IReadOnlyList<ICard> Deck(int rounds) =>
        Enumerable.Range(1, rounds)
            .Select(i => (ICard)StandardCard.Create($"Prompt {i}", $"Name something #{i}", Difficulty.Easy, "General"))
            .ToList();

    [Fact]
    public void Constructor_StartsTheControllerAndShowsTheFirstPrompt()
    {
        using var ctrl = new HerdController(Players(), Deck(2));
        var vm = new HerdGameViewModel(new FakeNavigator(), ctrl);

        vm.RoundNumber.Should().Be(1);
        vm.TotalRounds.Should().Be(2);
        vm.Prompt.Should().Be("Name something #1");
        vm.PlayerAnswers.Should().HaveCount(3);
        vm.PlayerAnswers.Should().OnlyContain(a => a.Answer == "");
    }

    [Fact]
    public void Reveal_PopulatesLastRoundResult_AndAdvancesToTheNextRoundInTheSameCall()
    {
        using var ctrl = new HerdController(Players(), Deck(2));
        var vm = new HerdGameViewModel(new FakeNavigator(), ctrl);

        vm.PlayerAnswers.First(a => a.PlayerName == "A").Answer = "Cereal";
        vm.PlayerAnswers.First(a => a.PlayerName == "B").Answer = "Cereal";
        vm.PlayerAnswers.First(a => a.PlayerName == "C").Answer = "Toast";

        vm.Reveal();

        // OnRoundResolved's state — round 1's outcome.
        vm.ShowingLastRound.Should().BeTrue();
        vm.LastHerdAnswer.Should().Be("Cereal");
        vm.LastLoneVoice.Should().Be("C");
        vm.HasScores.Should().BeTrue();

        // OnPromptReady's state — round 2 is ALREADY current by the time
        // Reveal() returns, because SubmitAnswers advances internally. If
        // either handler clobbered the other's properties, one of these two
        // assertion groups would fail.
        vm.RoundNumber.Should().Be(2, "PromptReady must fire correctly even though RoundResolved fired first in the same call");
        vm.Prompt.Should().Be("Name something #2");
        vm.PlayerAnswers.Should().OnlyContain(a => a.Answer == "", "answers must reset for the new round, not carry over");
        vm.IsGameOver.Should().BeFalse();
    }

    [Fact]
    public void Reveal_OnTheFinalRound_EndsTheGame_WhileKeepingTheLastRoundResultVisible()
    {
        using var ctrl = new HerdController(Players(), Deck(1));
        var vm = new HerdGameViewModel(new FakeNavigator(), ctrl);

        foreach (var a in vm.PlayerAnswers) a.Answer = "Same";
        vm.Reveal();

        vm.IsGameOver.Should().BeTrue();
        vm.ShowingLastRound.Should().BeTrue("the final round's result must still be visible alongside game over");
        vm.LastHerdAnswer.Should().Be("Same");
        vm.Summary.Should().NotBeEmpty();
    }

    [Fact]
    public void DismissLastRound_OnlyHidesThePanel_AndDoesNotTouchTheController()
    {
        using var ctrl = new HerdController(Players(), Deck(2));
        var vm = new HerdGameViewModel(new FakeNavigator(), ctrl);
        foreach (var a in vm.PlayerAnswers) a.Answer = "X";
        vm.Reveal();

        var roundBefore = vm.RoundNumber;
        vm.DismissLastRound();

        vm.ShowingLastRound.Should().BeFalse();
        vm.RoundNumber.Should().Be(roundBefore, "dismissing is a display toggle, not a controller action");
    }

    [Fact]
    public async Task Create_WithAModeThatProvidesNoHerdDeck_ProducesALoadError()
    {
        var vm = await HerdGameViewModel.CreateAsync(new FakeNavigator(), new NoCapabilityMode(), Players());

        vm.HasLoadError.Should().BeTrue();
        vm.IsPlaying.Should().BeFalse();
    }

    private sealed class NoCapabilityMode : IGameMode
    {
        public string Name => "No Capability";
        public string Description => "Implements no capability interface at all.";
    }
}
