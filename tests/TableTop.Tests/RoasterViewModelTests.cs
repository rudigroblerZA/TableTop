using TableTop.Presentation.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Tests;

/// <summary>
/// Backlog item 24: <see cref="RoasterViewModel"/> was the one shared ViewModel
/// with no test of its own. This covers the behaviour that item named as
/// untested — template selection resetting the in-progress roster,
/// <see cref="RoasterViewModel.CanAddPlayer"/> gating on a template's ceiling,
/// <see cref="RoasterViewModel.SaveBlockedReason"/>'s message, the
/// <see cref="RoasterViewModel.SaveRoster"/> no-op guard, and the
/// <see cref="IRosterStore"/> round-trip — plus the item 26 change: the "Team"
/// template now actually deals sides.
/// </summary>
public sealed class RoasterViewModelTests
{
    private static (RoasterViewModel vm, FakeRosterStore store, FakeNavigator nav) Build(
        IReadOnlyList<SavedRoster>? seed = null)
    {
        var store = new FakeRosterStore(seed);
        var nav = new FakeNavigator();
        return (new RoasterViewModel(nav, store), store, nav);
    }

    private static RoasterTemplate Template(RoasterViewModel vm, string name) =>
        vm.Templates.Single(t => t.Name == name);

    private static void AddPlayer(RoasterViewModel vm, string name, string gender = "", string age = "")
    {
        vm.NewPlayerName = name;
        vm.SelectedGender = gender;
        vm.NewPlayerAge = age;
        vm.AddPlayer();
    }

    // ── Template selection ───────────────────────────────────────────────────

    [Fact]
    public void SelectingATemplate_StartsConfiguring_AndSeedsTheName()
    {
        var (vm, _, _) = Build();

        vm.IsConfiguring.Should().BeFalse();
        vm.IsNotConfiguring.Should().BeTrue();

        vm.SelectedTemplate = Template(vm, "Friends");

        vm.IsConfiguring.Should().BeTrue();
        vm.IsNotConfiguring.Should().BeFalse();
        vm.RoasterName.Should().Be("Friends");
    }

    [Fact]
    public void ChangingTemplate_ClearsTheInProgressRoster()
    {
        var (vm, _, _) = Build();
        vm.SelectedTemplate = Template(vm, "Friends");
        AddPlayer(vm, "Amy");
        AddPlayer(vm, "Ben");
        vm.ConfiguredPlayers.Should().HaveCount(2);

        vm.SelectedTemplate = Template(vm, "Class");

        vm.ConfiguredPlayers.Should().BeEmpty();
        vm.RoasterName.Should().Be("Class");
    }

    // ── AddPlayer / CanAddPlayer ─────────────────────────────────────────────

    [Fact]
    public void CanAddPlayer_IsFalse_BeforeATemplateIsPicked()
    {
        var (vm, _, _) = Build();
        vm.NewPlayerName = "Amy";

        vm.CanAddPlayer.Should().BeFalse();
    }

    [Fact]
    public void CanAddPlayer_IsFalse_WithoutAName()
    {
        var (vm, _, _) = Build();
        vm.SelectedTemplate = Template(vm, "Friends");

        vm.CanAddPlayer.Should().BeFalse();

        vm.NewPlayerName = "   ";
        vm.CanAddPlayer.Should().BeFalse();
    }

    [Fact]
    public void AddPlayer_ParsesAgeAndGender_AndClearsTheEntryFields()
    {
        var (vm, _, _) = Build();
        vm.SelectedTemplate = Template(vm, "Friends");

        AddPlayer(vm, "  Amy  ", gender: "female", age: "31");

        var added = vm.ConfiguredPlayers.Should().ContainSingle().Subject;
        added.Name.Should().Be("Amy");
        added.Gender.Should().Be("female");
        added.Age.Should().Be(31);
        added.IsCoupleMember.Should().BeFalse();

        vm.NewPlayerName.Should().BeEmpty();
        vm.NewPlayerAge.Should().BeEmpty();
        vm.SelectedGender.Should().BeEmpty();
    }

    [Fact]
    public void AddPlayer_UnderACoupleTemplate_TagsTheCoupleFlag()
    {
        var (vm, _, _) = Build();
        vm.SelectedTemplate = Template(vm, "Couple");

        AddPlayer(vm, "Amy");

        vm.ConfiguredPlayers.Single().IsCoupleMember.Should().BeTrue();
    }

    [Fact]
    public void AddPlayer_StopsAtTheTemplateCeiling()
    {
        var (vm, _, _) = Build();
        vm.SelectedTemplate = Template(vm, "Couple");   // MaxPlayers = 2

        AddPlayer(vm, "Amy");
        AddPlayer(vm, "Ben");
        vm.CanAddPlayer.Should().BeFalse("a couple template allows exactly two");

        AddPlayer(vm, "Cara");
        vm.ConfiguredPlayers.Should().HaveCount(2);
    }

    [Fact]
    public void RemovePlayer_TakesOneOff()
    {
        var (vm, _, _) = Build();
        vm.SelectedTemplate = Template(vm, "Friends");
        AddPlayer(vm, "Amy");
        AddPlayer(vm, "Ben");

        vm.RemovePlayer(vm.ConfiguredPlayers[0]);

        vm.ConfiguredPlayers.Single().Name.Should().Be("Ben");
    }

    // ── SaveBlockedReason / CanSaveRoster ────────────────────────────────────

    [Fact]
    public void SaveBlockedReason_NamesTheFloor_UntilItIsMet()
    {
        var (vm, _, _) = Build();
        vm.SelectedTemplate = Template(vm, "Class");   // MinPlayers = 3, MaxPlayers = 40

        vm.SaveBlockedReason.Should().Be("Needs 3–40 players. (0 so far)");
        vm.CanSaveRoster.Should().BeFalse();

        AddPlayer(vm, "Amy");
        AddPlayer(vm, "Ben");
        AddPlayer(vm, "Cara");

        vm.SaveBlockedReason.Should().BeEmpty();
        vm.CanSaveRoster.Should().BeTrue();
    }

    [Fact]
    public void SaveBlockedReason_UsesTheTemplatesOwnRequirementWording()
    {
        var (vm, _, _) = Build();
        vm.SelectedTemplate = Template(vm, "Friends");   // MinPlayers = 2, no ceiling

        AddPlayer(vm, "Amy");

        vm.SaveBlockedReason.Should().Be("Needs at least 2 players. (1 so far)");
    }

    // ── SaveRoster ──────────────────────────────────────────────────────────

    [Fact]
    public void SaveRoster_WhenBlocked_DoesNothing()
    {
        var (vm, store, _) = Build();
        vm.SelectedTemplate = Template(vm, "Friends");
        AddPlayer(vm, "Amy");   // Friends needs 2

        vm.SaveRoster();

        vm.SavedRosters.Should().BeEmpty();
        store.Saved.Should().BeEmpty();
        vm.SelectedTemplate.Should().NotBeNull("a blocked save must not reset the column");
    }

    [Fact]
    public void SaveRoster_PersistsTheList_AndResetsToTemplatePicking()
    {
        var (vm, store, _) = Build();
        vm.SelectedTemplate = Template(vm, "Friends");
        AddPlayer(vm, "Amy");
        AddPlayer(vm, "Ben");
        vm.RoasterName = "Friday Regulars";

        vm.SaveRoster();

        var saved = vm.SavedRosters.Should().ContainSingle().Subject;
        saved.Name.Should().Be("Friday Regulars");
        saved.TemplateName.Should().Be("Friends");
        saved.Players.Select(p => p.Name).Should().Equal("Amy", "Ben");

        store.Saved.Should().BeEquivalentTo(vm.SavedRosters);
        vm.SelectedTemplate.Should().BeNull();
    }

    [Fact]
    public void SaveRoster_WithABlankName_FallsBackToTheTemplateName()
    {
        var (vm, _, _) = Build();
        vm.SelectedTemplate = Template(vm, "Friends");
        AddPlayer(vm, "Amy");
        AddPlayer(vm, "Ben");
        vm.RoasterName = "   ";

        vm.SaveRoster();

        vm.SavedRosters.Single().Name.Should().Be("Friends");
    }

    // ── IRosterStore round-trip ─────────────────────────────────────────────

    [Fact]
    public void Construction_LoadsWhateverTheStoreAlreadyHas()
    {
        var existing = new SavedRoster
        {
            Name = "Old Crew",
            TemplateName = "Friends",
            Players = [new SavedPlayer("Amy", null, null)],
        };

        var (vm, _, _) = Build(seed: [existing]);

        vm.SavedRosters.Should().ContainSingle().Which.Name.Should().Be("Old Crew");
    }

    [Fact]
    public void DeleteRoster_RemovesAndPersists()
    {
        var existing = new SavedRoster
        {
            Name = "Old Crew",
            TemplateName = "Friends",
            Players = [new SavedPlayer("Amy", null, null), new SavedPlayer("Ben", null, null)],
        };
        var (vm, store, _) = Build(seed: [existing]);

        vm.DeleteRoster(vm.SavedRosters[0]);

        vm.SavedRosters.Should().BeEmpty();
        store.Saved.Should().BeEmpty();
    }

    // ── Item 26: the "Team" template deals sides ────────────────────────────

    [Fact]
    public void TeamTemplate_DealsConfiguredPlayersIntoAlternatingSides_OnSave()
    {
        var (vm, _, _) = Build();
        vm.SelectedTemplate = Template(vm, "Team");   // MinPlayers 4, DealTeams
        AddPlayer(vm, "Amy");
        AddPlayer(vm, "Ben");
        AddPlayer(vm, "Cara");
        AddPlayer(vm, "Dan");

        vm.SaveRoster();

        var players = vm.SavedRosters.Single().Players;
        players.Select(p => p.Team).Should().Equal("Red", "Blue", "Red", "Blue");
    }

    [Fact]
    public void NonTeamTemplate_LeavesEveryPlayersTeamNull()
    {
        var (vm, _, _) = Build();
        vm.SelectedTemplate = Template(vm, "Friends");
        AddPlayer(vm, "Amy");
        AddPlayer(vm, "Ben");

        vm.SaveRoster();

        vm.SavedRosters.Single().Players.Should().OnlyContain(p => p.Team == null);
    }

    [Fact]
    public void TeamTemplate_Description_MatchesWhatItNowDoes()
    {
        var (vm, _, _) = Build();
        var team = Template(vm, "Team");

        team.DealTeams.Should().BeTrue();
        team.Description.Should().NotContain("split into sides",
            "the reworded description must not re-make the claim item 26 was filed against");
    }
}
