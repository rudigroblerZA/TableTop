using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Controllers;
using TableTop.Presentation.ViewModels;

namespace TableTop.Tests;

/// <summary>
/// <see cref="ClaimedGameViewModel"/> against a real <see cref="ClaimedController"/> —
/// backlog item 4's WinUI/MAUI screen for the AreaControl family.
/// </summary>
public sealed class ClaimedGameViewModelTests
{
    private static IReadOnlyList<IPlayer> Players() =>
        [Player.Create("A"), Player.Create("B")];

    // Three territories, two cards each, so a challenge never exhausts one
    // outright mid-test.
    private static IReadOnlyList<ICard> Deck() =>
    [
        StandardCard.Create("Alpha 1", "desc", Difficulty.Easy, "Alpha"),
        StandardCard.Create("Alpha 2", "desc", Difficulty.Easy, "Alpha"),
        StandardCard.Create("Beta 1",  "desc", Difficulty.Easy, "Beta"),
        StandardCard.Create("Beta 2",  "desc", Difficulty.Easy, "Beta"),
        StandardCard.Create("Gamma 1", "desc", Difficulty.Easy, "Gamma"),
        StandardCard.Create("Gamma 2", "desc", Difficulty.Easy, "Gamma"),
    ];

    private static ClaimedController RealController(int winningTerritoryCount = 2) =>
        new(Players(), Deck(), winningTerritoryCount);

    [Fact]
    public void Constructor_StartsTheControllerAndPopulatesTheBoard()
    {
        // Start() raises no event — the board must be populated up front, not
        // left waiting on a controller announcement that never comes.
        using var ctrl = RealController();
        var vm = new ClaimedGameViewModel(new FakeNavigator(), ctrl);

        vm.CurrentPlayerName.Should().Be("A");
        vm.Territories.Should().HaveCount(3);
        vm.Territories.Should().OnlyContain(t => t.IsChallengeable, "nobody holds anything yet");
        vm.Territories.Should().OnlyContain(t => t.HolderDisplay == "Open");
    }

    [Fact]
    public void Challenge_ThenSucceed_ClaimsTheTerritoryAndAdvancesTheTurn()
    {
        using var ctrl = RealController();
        var vm = new ClaimedGameViewModel(new FakeNavigator(), ctrl);

        vm.Challenge("Alpha");

        vm.HasPendingChallenge.Should().BeTrue();
        vm.PendingCardTitle.Should().NotBeEmpty();
        vm.IsRaid.Should().BeFalse("Alpha was open ground");

        vm.Succeed();

        vm.HasPendingChallenge.Should().BeFalse();
        vm.Territories.Single(t => t.Name == "Alpha").HolderDisplay.Should().Be("A");
        vm.CurrentPlayerName.Should().Be("B", "the turn advances after a resolved challenge");
        vm.Flash.Should().Contain("A").And.Contain("Alpha");
    }

    [Fact]
    public void Challenge_ThenFail_LeavesTheTerritoryOpenButStillAdvancesTheTurn()
    {
        using var ctrl = RealController();
        var vm = new ClaimedGameViewModel(new FakeNavigator(), ctrl);

        vm.Challenge("Alpha");
        vm.Fail();

        vm.HasPendingChallenge.Should().BeFalse();
        vm.Territories.Single(t => t.Name == "Alpha").HolderDisplay.Should().Be("Open");
        vm.CurrentPlayerName.Should().Be("B");
    }

    [Fact]
    public void Raid_OnAHeldTerritory_IsFlaggedAsARaid()
    {
        using var ctrl = RealController();
        var vm = new ClaimedGameViewModel(new FakeNavigator(), ctrl);

        vm.Challenge("Alpha");
        vm.Succeed(); // A now holds Alpha; turn passes to B

        vm.Challenge("Alpha"); // B raids A's territory
        vm.IsRaid.Should().BeTrue();
        vm.PendingDefenderName.Should().Be("A");
    }

    [Fact]
    public void ReachingTheWinningCount_EndsTheGame()
    {
        using var ctrl = RealController(winningTerritoryCount: 2);
        var vm = new ClaimedGameViewModel(new FakeNavigator(), ctrl);

        vm.Challenge("Alpha"); vm.Succeed(); // A holds Alpha, turn -> B
        vm.Challenge("Beta"); vm.Succeed(); // B holds Beta,  turn -> A
        vm.Challenge("Gamma"); vm.Succeed(); // A holds Alpha + Gamma = 2 -> win

        vm.IsGameOver.Should().BeTrue();
        vm.HasPendingChallenge.Should().BeFalse();
        vm.Summary.Should().Contain("A").And.Contain("2");
    }

    [Fact]
    public void Create_WithAModeThatProvidesNoClaimedDeck_ProducesALoadError()
    {
        var vm = ClaimedGameViewModel.Create(new FakeNavigator(), new NoCapabilityMode(), Players());

        vm.HasLoadError.Should().BeTrue();
        vm.IsPlaying.Should().BeFalse();
    }

    private sealed class NoCapabilityMode : IGameMode
    {
        public string Name => "No Capability";
        public string Description => "Implements no capability interface at all.";
    }
}
