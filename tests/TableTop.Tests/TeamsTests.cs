using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Games.Fun;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;
using TableTop.Presentation.ViewModels;

namespace TableTop.Tests;

/// <summary>
/// Team support rides on <c>IPlayer.Attributes["team"]</c> rather than a new
/// property on <see cref="IPlayer"/>. These pin the consequences of that
/// choice: membership survives without any persistence change, team totals
/// derive from member scores rather than being stored separately, and dealing
/// alternates rather than halving.
/// </summary>
public sealed class TeamsTests
{
    private static IPlayer P(string name, string? team = null) => Player.Create(name,
        attributes: team is null ? null : new Dictionary<string, string> { ["team"] = team });

    [Fact]
    public void TeamOf_ReadsMembershipFromAttributes()
    {
        Teams.TeamOf(P("Amy", "Red")).Should().Be("Red");
        Teams.TeamOf(P("Solo")).Should().BeNull();
    }

    [Fact]
    public void TeamOf_TreatsBlankAsNoTeam()
    {
        // A blank attribute is a data accident, not a team called "".
        Teams.TeamOf(P("Amy", "   ")).Should().BeNull();
    }

    [Fact]
    public void TeamNames_ReturnsDistinctTeamsInFirstAppearanceOrder()
    {
        var players = new[] { P("Amy", "Red"), P("Ben", "Blue"), P("Cara", "Red") };
        Teams.TeamNames(players).Should().Equal("Red", "Blue");
    }

    [Fact]
    public void TeamNames_IgnoresPlayersWithNoTeam()
    {
        Teams.TeamNames(new[] { P("Amy", "Red"), P("Solo") }).Should().Equal("Red");
    }

    [Fact]
    public void MembersOf_IsCaseInsensitive()
    {
        // Membership is a string, so the helpers absorb casing rather than
        // making every caller remember to.
        var players = new[] { P("Amy", "Red"), P("Ben", "red") };
        Teams.MembersOf(players, "RED").Should().HaveCount(2);
    }

    [Fact]
    public void Deal_AlternatesRatherThanHalving()
    {
        // The point of dealing: people are entered in seating order, so
        // halving tends to put one side of the sofa against the other.
        var dealt = Teams.Deal([P("Amy"), P("Ben"), P("Cara"), P("Dan")], 2);

        dealt[0].Team.Should().NotBe(dealt[1].Team);
        dealt[1].Team.Should().NotBe(dealt[2].Team);
    }

    [Fact]
    public void Deal_KeepsTeamSizesWithinOne_ForOddPlayerCounts()
    {
        var dealt = Teams.Deal([P("A"), P("B"), P("C"), P("D"), P("E")], 2);

        var red = dealt.Count(d => d.Team == "Red");
        var blue = dealt.Count(d => d.Team == "Blue");
        Math.Abs(red - blue).Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    public void Deal_RejectsFewerThanTwoTeams()
    {
        var act = () => Teams.Deal([P("A"), P("B")], 1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Deal_RejectsMoreTeamsThanPlayers()
    {
        // Otherwise a team exists with nobody on it and the turn order stalls.
        var act = () => Teams.Deal([P("Solo")], 2);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ScoreOf_SumsMemberScores_RatherThanStoringATeamTotal()
    {
        // Derived, not stored — a team total can never drift from the
        // individual scores it's made of.
        var manager = new TeamAlternatingPlayerManager();
        foreach (var p in new[] { P("Amy", "Red"), P("Cara", "Red"), P("Ben", "Blue") })
            manager.AddPlayer(p);

        manager.ApplyScore(manager.Players.First(p => p.DisplayName == "Amy").Id, 5);
        manager.ApplyScore(manager.Players.First(p => p.DisplayName == "Cara").Id, 3);

        Teams.ScoreOf(manager.Players, "Red").Should().Be(8);
        Teams.ScoreOf(manager.Players, "Blue").Should().Be(0);
    }

    [Fact]
    public void Standings_AreOrderedHighestFirst()
    {
        var manager = new TeamAlternatingPlayerManager();
        foreach (var p in new[] { P("Amy", "Red"), P("Ben", "Blue") })
            manager.AddPlayer(p);
        manager.ApplyScore(manager.Players.First(p => p.DisplayName == "Ben").Id, 10);

        var standings = Teams.Standings(manager.Players);

        standings[0].Name.Should().Be("Blue");
        standings[0].Score.Should().Be(10);
    }
}

/// <summary>
/// <see cref="TeamAlternatingPlayerManager"/> exists because round-robin walks
/// the player list in entry order, which alternates only by luck. These pin
/// the case that actually motivated it.
/// </summary>
public sealed class TeamAlternatingPlayerManagerTests
{
    private static IPlayer P(string name, string? team = null) => Player.Create(name,
        attributes: team is null ? null : new Dictionary<string, string> { ["team"] = team });

    private static TeamAlternatingPlayerManager WithGroupedEntry()
    {
        // Both of one team entered first — the order a host naturally types,
        // and the case plain round-robin gets wrong.
        var manager = new TeamAlternatingPlayerManager();
        foreach (var p in new[] { P("Amy", "Red"), P("Cara", "Red"), P("Ben", "Blue"), P("Dan", "Blue") })
            manager.AddPlayer(p);
        return manager;
    }

    [Fact]
    public void GetNextPlayer_AlternatesTeams_EvenWhenOneTeamWasEnteredFirst()
    {
        var manager = WithGroupedEntry();

        var teams = Enumerable.Range(0, 6)
            .Select(_ => Teams.TeamOf(manager.GetNextPlayer()!))
            .ToList();

        teams.Zip(teams.Skip(1)).Should().OnlyContain(pair => pair.First != pair.Second,
            "the same team must never take two turns in a row");
    }

    [Fact]
    public void GetNextPlayer_RotatesWithinEachTeam()
    {
        var manager = WithGroupedEntry();

        var names = Enumerable.Range(0, 4)
            .Select(_ => manager.GetNextPlayer()!.DisplayName)
            .ToList();

        names.Should().OnlyHaveUniqueItems("each team's members take turns before anyone repeats");
    }

    [Fact]
    public void GetNextPlayer_ReturnsNull_WithNoPlayers()
    {
        new TeamAlternatingPlayerManager().GetNextPlayer().Should().BeNull();
    }

    [Fact]
    public void GetNextPlayer_SkipsATeamWhoseMembersAreAllInactive()
    {
        // Otherwise the game stalls on an empty side.
        var manager = WithGroupedEntry();
        foreach (var p in manager.Players.Where(p => Teams.TeamOf(p) == "Blue"))
            manager.SetStatus(p.Id, PlayerStatus.Eliminated);

        var next = manager.GetNextPlayer();

        next.Should().NotBeNull();
        Teams.TeamOf(next!).Should().Be("Red");
    }

    [Fact]
    public void PlayersWithNoTeam_StillGetTurns()
    {
        // Degrade sensibly rather than silently skipping someone.
        var manager = new TeamAlternatingPlayerManager();
        manager.AddPlayer(P("Amy", "Red"));
        manager.AddPlayer(P("Solo"));

        var seen = Enumerable.Range(0, 4).Select(_ => manager.GetNextPlayer()!.DisplayName).ToList();

        seen.Should().Contain("Solo");
    }

    [Fact]
    public void RewindTo_PutsTheSameTeamBackOnTurn()
    {
        // Undo must not silently hand the next card to the other side — that
        // would be worse than the mistake being undone.
        var manager = WithGroupedEntry();
        var first = manager.GetNextPlayer()!;
        manager.GetNextPlayer();

        manager.RewindTo(first.Id);

        manager.GetNextPlayer()!.Id.Should().Be(first.Id);
    }
}

/// <summary>
/// <see cref="RivalsMode"/> — the first mode built for teams. Its mechanic
/// depends on every play card offering all three tiers, so that's pinned
/// rather than assumed.
/// </summary>
public sealed class RivalsModeTests
{
    private static IPlayer P(string name, string team) => Player.Create(name,
        attributes: new Dictionary<string, string> { ["team"] = team });

    [Fact]
    public void RegisteredInArchetypeTree()
    {
        var node = ArchetypeRegistry.Default().FindById("fun.rivals");
        node.Should().NotBeNull();
        node!.Modes.Should().Contain(m => m.Name == "Rivals");
    }

    [Fact]
    public void ImplementsITeamMode_WhichIsWhatSelectsTeamTurnOrder()
    {
        new RivalsMode().Should().BeAssignableTo<ITeamMode>();
    }

    [Fact]
    public void RequiresFourPlayers_BecauseTwoTeamsOfOneIsJustAnIndividualGame()
    {
        new RivalsMode().MinimumPlayers.Should().Be(4);
    }

    [Fact]
    public void Deck_OpensOnTheRulesCard()
    {
        // The mechanic doesn't work if nobody knows the other team chooses.
        new RivalsMode().GetCards([])[0].Category.Should().Be("How To Play");
    }

    [Fact]
    public void EveryPlayCard_OffersAllThreeTiers()
    {
        // The load-bearing invariant: a card missing a tier quietly collapses
        // the opposing team's decision into a smaller one.
        var play = new RivalsMode().GetCards([]).Where(c => c.Category != "How To Play").ToList();

        play.Should().HaveCountGreaterThan(15);
        play.Should().OnlyContain(c => c.Description.Contains("EASY — 1 point"));
        play.Should().OnlyContain(c => c.Description.Contains("HARD — 3 points"));
        play.Should().OnlyContain(c => c.Description.Contains("BRUTAL — 5 points"));
    }

    [Fact]
    public void EveryPlayCard_SaysWhoChooses()
    {
        var play = new RivalsMode().GetCards([]).Where(c => c.Category != "How To Play");
        play.Should().OnlyContain(c => c.Description.Contains("The other team picks"));
    }

    [Fact]
    public void Deck_HasNoDuplicateIdsOrBodies()
    {
        var deck = new RivalsMode().GetCards([]);
        deck.Select(c => c.Id).Distinct().Should().HaveCount(deck.Count);
        deck.Select(c => c.Description).Distinct().Should().HaveCount(deck.Count);
    }

    [Fact]
    public void Manifest_ReportsNonZeroTotalCards()
    {
        new RivalsMode().GetManifest().TotalCards.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CategoryColours_CoverEveryCategoryUsed()
    {
        var mode = new RivalsMode();
        mode.GetCards([]).Select(c => c.Category).Distinct()
            .Should().OnlyContain(c => mode.CategoryColours.ContainsKey(c));
    }

    [Fact]
    public async Task RealSession_AlternatesTeams_ThroughTheActualControllerFactory()
    {
        // The end-to-end proof that ITeamMode actually selects the team
        // manager — players deliberately entered one team at a time.
        var players = new IPlayer[] { P("Amy", "Red"), P("Cara", "Red"), P("Ben", "Blue"), P("Dan", "Blue") };
        var controller = (ICardTurnController)(await new ControllerFactory()
            .CreateAsync(new RivalsMode(), players));

        var order = new List<string>();
        controller.CardReady += (_, e) => order.Add(e.PlayerName);
        controller.Start();
        for (var i = 0; i < 6 && controller.IsRunning; i++)
            controller.RecordOutcome(CardOutcome.Completed);

        var teamOf = players.ToDictionary(p => p.DisplayName, p => Teams.TeamOf(p)!);
        var teams = order.Select(n => teamOf[n]).ToList();

        teams.Should().HaveCountGreaterThan(3);
        teams.Zip(teams.Skip(1)).Should().OnlyContain(pair => pair.First != pair.Second,
            "ITeamMode must select TeamAlternatingPlayerManager end to end");

        controller.Dispose();
    }
}

/// <summary>
/// The setup path. Team support shipped in the engine before any screen could
/// assign a team, which left <see cref="RivalsMode"/> running with every
/// player as their own side — playable, but with the central mechanic
/// pointing at nothing. These pin the fix.
/// </summary>
public sealed class PlayerSetupTeamAssignmentTests
{
    private static PlayerSetupViewModel Build(IGameMode mode) =>
        new(new FakeNavigator(), mode, new FakeAppSettings());

    private static PlayerSetupViewModel WithRoster(IGameMode mode, params string[] names)
    {
        var vm = Build(mode);
        foreach (var n in names) { vm.NewName = n; vm.AddPlayer(); }
        return vm;
    }

    [Fact]
    public void NonTeamMode_IsUnaffected()
    {
        // The other 88 modes must not gain a team attribute they never asked for.
        var vm = WithRoster(new TableTop.Games.WouldYouRatherMode(), "Amy");

        vm.IsTeamMode.Should().BeFalse();
        vm.BuildPlayers()[0].Attributes.Should().NotContainKey(Teams.AttributeKey);
    }

    [Fact]
    public void TeamMode_IsDetectedFromTheMode()
    {
        var vm = Build(new RivalsMode());
        vm.IsTeamMode.Should().BeTrue();
        vm.TeamCount.Should().Be(2);
    }

    [Fact]
    public void AssignTeams_FailsWithTooFewPlayers()
    {
        WithRoster(new RivalsMode(), "Amy").AssignTeams().Should().BeFalse();
    }

    [Fact]
    public void AssignTeams_WritesTeamsOntoBuiltPlayers()
    {
        var vm = WithRoster(new RivalsMode(), "Amy", "Cara", "Ben", "Dan");

        vm.AssignTeams().Should().BeTrue();
        var built = vm.BuildPlayers();

        built.Should().OnlyContain(p => Teams.TeamOf(p) != null);
        Teams.TeamNames(built).Should().HaveCount(2);
    }

    [Fact]
    public void RosterChanges_InvalidateAnExistingDeal()
    {
        // Otherwise a newly added player is the only one with no team.
        var vm = WithRoster(new RivalsMode(), "Amy", "Cara", "Ben", "Dan");
        vm.AssignTeams();
        vm.HasTeams.Should().BeTrue();

        vm.NewName = "Eve";
        vm.AddPlayer();

        vm.HasTeams.Should().BeFalse();
    }

    [Fact]
    public void RemovingAPlayer_AlsoInvalidatesTheDeal()
    {
        var vm = WithRoster(new RivalsMode(), "Amy", "Cara", "Ben", "Dan");
        vm.AssignTeams();

        vm.RemovePlayer(vm.Players[0]);

        vm.HasTeams.Should().BeFalse();
    }

    [Fact]
    public void TeamSummary_IsEmptyUntilDealt_ThenListsBothSides()
    {
        var vm = WithRoster(new RivalsMode(), "Amy", "Cara", "Ben", "Dan");
        vm.TeamSummary.Should().BeEmpty();

        vm.AssignTeams();

        vm.TeamSummary.Should().Contain("Red");
        vm.TeamSummary.Should().Contain("Blue");
    }

    [Fact]
    public async Task FullPath_FromSetupToSession_AlternatesByTeam()
    {
        // The end-to-end proof that the setup path actually feeds the engine's
        // team support — not just that each half works alone.
        var vm = WithRoster(new RivalsMode(), "Amy", "Cara", "Ben", "Dan");
        vm.AssignTeams();
        var players = vm.BuildPlayers();

        var controller = (ICardTurnController)(await new ControllerFactory()
            .CreateAsync(new RivalsMode(), players));

        var order = new List<string>();
        controller.CardReady += (_, e) => order.Add(e.PlayerName);
        controller.Start();
        for (var i = 0; i < 6 && controller.IsRunning; i++)
            controller.RecordOutcome(CardOutcome.Completed);

        var teamOf = players.ToDictionary(p => p.DisplayName, p => Teams.TeamOf(p)!);
        var teams = order.Select(n => teamOf[n]).ToList();

        teams.Should().HaveCountGreaterThan(3);
        teams.Zip(teams.Skip(1)).Should().OnlyContain(pair => pair.First != pair.Second);

        controller.Dispose();
    }
}
