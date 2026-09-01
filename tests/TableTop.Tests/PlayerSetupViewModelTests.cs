using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Rules;
using TableTop.Presentation.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Tests;

/// <summary>
/// Zero test references before this — the class-level file count in the
/// backlog was misleading: the only real hit was
/// <c>BindableSurfaceTests.GenderOptions_MustBeAnInstanceProperty_OnEveryHead</c>,
/// which looked up two per-head class names that no longer exist since the
/// merge and silently skipped its own assertion every time — see the fixed
/// version below.
/// </summary>
public sealed class PlayerSetupViewModelTests
{
    /// <summary>A mode whose <see cref="IGameModeDefinition.MinimumPlayers"/> is settable, for the split below.</summary>
    private sealed class FakeMode(int minimumPlayers = 2) : IGameMode, IGameModeDefinition
    {
        public string Name => "Fake Mode";
        public string Description => "test";
        public int MinimumPlayers => minimumPlayers;
        public IReadOnlyList<ICard> GetCards(IReadOnlyList<IPlayer> players) => [];
        public IScoringStrategy GetScoring() => new TableTop.Core.Domain.Scoring.FixedScoringStrategy(1);
        public IEnumerable<IRule> GetRules() => [];
    }

    /// <summary>A team mode for exercising <see cref="PlayerSetupViewModel.AssignTeams"/> — takes the default <see cref="ITeamMode.PreferredTeamCount"/> of 2.</summary>
    private sealed class FakeTeamMode : IGameMode, ITeamMode
    {
        public string Name => "Fake Team Mode";
        public string Description => "test";
    }

    private static (PlayerSetupViewModel vm, FakeAppSettings settings, FakeNavigator nav) Build(
        IGameMode? mode = null, Func<IReadOnlyList<IPlayer>, Task>? onStart = null,
        IRosterStore? rosterStore = null)
    {
        var settings = new FakeAppSettings();
        var nav = new FakeNavigator();
        var vm = new PlayerSetupViewModel(nav, mode ?? new FakeMode(), settings, onStart, rosterStore);
        return (vm, settings, nav);
    }

    // ── AddPlayer ─────────────────────────────────────────────────────────────

    [Fact]
    public void AddPlayer_WithABlankName_SetsErrorAndAddsNothing()
    {
        var (vm, _, _) = Build();
        vm.NewName = "   ";
        vm.AddPlayer();

        vm.Players.Should().BeEmpty();
        vm.HasError.Should().BeTrue();
    }

    [Fact]
    public void AddPlayer_WithARealName_AddsAndClearsThePendingEntry()
    {
        var (vm, _, _) = Build();
        vm.NewName = "Alice";
        vm.NewAge = "30";
        vm.SelectedGender = "female";
        vm.NewIsCouple = true;

        vm.AddPlayer();

        vm.Players.Should().ContainSingle();
        vm.Players[0].Name.Should().Be("Alice");
        vm.Players[0].Age.Should().Be(30);
        vm.Players[0].Gender.Should().Be("female");
        vm.Players[0].IsCoupleMember.Should().BeTrue();
        vm.NewName.Should().BeEmpty("the pending entry clears after a successful add");
        vm.HasError.Should().BeFalse();
    }

    [Fact]
    public void AddPlayer_RejectsACaseInsensitiveDuplicateName()
    {
        var (vm, _, _) = Build();
        vm.NewName = "Alice"; vm.AddPlayer();
        vm.NewName = "ALICE"; vm.AddPlayer();

        vm.Players.Should().ContainSingle("a duplicate, even case-different, must not be added");
        vm.HasError.Should().BeTrue();
    }

    [Fact]
    public void AddPlayer_WithNoAgeTyped_LeavesAgeNull()
    {
        var (vm, _, _) = Build();
        vm.NewName = "Bob";
        vm.NewAge = "not a number";
        vm.AddPlayer();

        vm.Players[0].Age.Should().BeNull();
    }

    [Fact]
    public void AddPlayerCommand_CanExecute_OnlyWhenNameIsNonBlank()
    {
        var (vm, _, _) = Build();
        vm.AddPlayerCommand.CanExecute(null).Should().BeFalse();
        vm.NewName = "Alice";
        vm.AddPlayerCommand.CanExecute(null).Should().BeTrue();
    }

    // ── RemovePlayer / ClearPlayers ──────────────────────────────────────────

    [Fact]
    public void RemovePlayer_RemovesExactlyThatEntry()
    {
        var (vm, _, _) = Build();
        vm.NewName = "Alice"; vm.AddPlayer();
        vm.NewName = "Bob"; vm.AddPlayer();
        var alice = vm.Players.First(p => p.Name == "Alice");

        vm.RemovePlayer(alice);

        vm.Players.Should().ContainSingle().Which.Name.Should().Be("Bob");
    }

    [Fact]
    public void ClearPlayers_EmptiesTheRoster_AndClearsRosterStatus()
    {
        var (vm, _, _) = Build();
        vm.NewName = "Alice"; vm.AddPlayer();
        vm.SaveRosterAsDefault();

        vm.ClearPlayers();

        vm.Players.Should().BeEmpty();
        vm.HasRosterStatus.Should().BeFalse();
    }

    // ── HasPlayers / CanStartGame / MinimumPlayers ───────────────────────────

    [Fact]
    public void MinimumPlayers_IsAskedOfTheMode_NotHardcoded()
    {
        // The exact regression a first draft of the real merge introduced and
        // caught before shipping: hardcoding "couples need 2, everything else
        // 1" would have locked people out of personality-quiz-style modes
        // that legitimately need only one player.
        var (soloVm, _, _) = Build(new FakeMode(minimumPlayers: 1));
        var (pairVm, _, _) = Build(new FakeMode(minimumPlayers: 2));

        soloVm.MinimumPlayers.Should().Be(1);
        pairVm.MinimumPlayers.Should().Be(2);
    }

    [Fact]
    public void CanStartGame_FollowsMinimumPlayers_NotAFixedCount()
    {
        var (vm, _, _) = Build(new FakeMode(minimumPlayers: 1));
        vm.CanStartGame.Should().BeFalse("no players yet");

        vm.NewName = "Alice"; vm.AddPlayer();
        vm.CanStartGame.Should().BeTrue("this mode only needs one");
    }

    [Fact]
    public void HasPlayers_ReflectsWhetherAnyoneHasBeenAdded()
    {
        var (vm, _, _) = Build();
        vm.HasPlayers.Should().BeFalse();
        vm.NewName = "Alice"; vm.AddPlayer();
        vm.HasPlayers.Should().BeTrue();
    }

    // ── BuildPlayers ──────────────────────────────────────────────────────────

    [Fact]
    public void BuildPlayers_CarriesGenderAndAgeAsAttributes()
    {
        var (vm, _, _) = Build();
        vm.NewName = "Alice"; vm.NewAge = "25"; vm.SelectedGender = "female";
        vm.AddPlayer();

        var built = vm.BuildPlayers().Single();
        built.Attributes["gender"].Should().Be("female");
        built.Attributes["age"].Should().Be("25");
    }

    [Fact]
    public void BuildPlayers_TagsAdultsAndCoupleMembers()
    {
        var (vm, _, _) = Build();
        vm.NewName = "Alice"; vm.NewAge = "25"; vm.NewIsCouple = true;
        vm.AddPlayer();

        var built = vm.BuildPlayers().Single();
        built.Tags.Should().Contain("adult");
        built.Tags.Should().Contain("couple-member");
    }

    [Fact]
    public void BuildPlayers_DoesNotTagMinorsAsAdult()
    {
        var (vm, _, _) = Build();
        vm.NewName = "Kid"; vm.NewAge = "10";
        vm.AddPlayer();

        vm.BuildPlayers().Single().Tags.Should().NotContain("adult");
    }

    // ── SaveRosterAsDefault ───────────────────────────────────────────────────

    [Fact]
    public void SaveRosterAsDefault_WritesToSettings_AndSetsRosterStatus()
    {
        var (vm, settings, _) = Build();
        vm.NewName = "Alice"; vm.AddPlayer();

        vm.SaveRosterAsDefault();

        settings.RecentPlayers.Should().ContainSingle().Which.Name.Should().Be("Alice");
        vm.HasRosterStatus.Should().BeTrue();
    }

    [Fact]
    public void SaveRosterAsDefault_DoesNotHappenImplicitlyOnStart()
    {
        // Explicit only, in both heads — starting a game must not silently
        // overwrite whatever roster was saved before.
        var (vm, settings, _) = Build(onStart: _ => Task.CompletedTask);
        settings.RecentPlayers = [new(Name: "OldRoster", Gender: null, Age: null)];
        vm.NewName = "Alice"; vm.AddPlayer();

        vm.StartCommand.Execute(null);

        settings.RecentPlayers.Should().ContainSingle().Which.Name.Should().Be("OldRoster");
    }

    [Fact]
    public void Constructor_PrefillsFromRecentPlayers()
    {
        var settings = new FakeAppSettings
        {
            RecentPlayers = [new(Name: "Remembered", Gender: "male", Age: 40)],
        };
        var vm = new PlayerSetupViewModel(new FakeNavigator(), new FakeMode(), settings);

        vm.Players.Should().ContainSingle().Which.Name.Should().Be("Remembered");
    }

    // ── Saved rosters (backlog item 25) ──────────────────────────────────────

    private static SavedRoster Roster(string name, params SavedPlayer[] players) =>
        new() { Name = name, TemplateName = "Friends", Players = players };

    [Fact]
    public void Constructor_WithNoRosterStore_OffersNoSavedRosters()
    {
        // The default before this feature existed, and still the shape a
        // caller that doesn't wire an IRosterStore gets — a test, or any
        // future host that hasn't decided on one yet.
        var (vm, _, _) = Build();

        vm.SavedRosters.Should().BeEmpty();
        vm.HasSavedRosters.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithARosterStore_LoadsItsSavedRosters()
    {
        var store = new FakeRosterStore([
            Roster("Regulars", new SavedPlayer("Alice", "female", 30), new SavedPlayer("Bob", "male", 32)),
            Roster("Classroom", new SavedPlayer("Cara", null, 10)),
        ]);

        var (vm, _, _) = Build(rosterStore: store);

        vm.HasSavedRosters.Should().BeTrue();
        vm.SavedRosters.Select(r => r.Name).Should().Equal("Regulars", "Classroom");
    }

    [Fact]
    public void LoadRoster_ReplacesWhateverWasAlreadyTyped()
    {
        var (vm, _, _) = Build();
        vm.NewName = "Typed"; vm.AddPlayer();

        vm.LoadRoster(Roster("Regulars",
            new SavedPlayer("Alice", "female", 30, IsCoupleMember: true),
            new SavedPlayer("Bob", "male", 32)));

        vm.Players.Select(p => p.Name).Should().Equal("Alice", "Bob");
        vm.Players[0].IsCoupleMember.Should().BeTrue();
        vm.Players[0].Age.Should().Be(30);
    }

    [Fact]
    public void LoadRoster_ClearsAnyPriorError_AndSetsRosterStatus()
    {
        var (vm, _, _) = Build();
        vm.NewName = "   "; vm.AddPlayer();   // sets Error
        vm.HasError.Should().BeTrue("test setup: this should have failed to add");

        vm.LoadRoster(Roster("Regulars", new SavedPlayer("Alice", "female", 30)));

        vm.HasError.Should().BeFalse();
        vm.HasRosterStatus.Should().BeTrue();
        vm.RosterStatus.Should().Contain("Regulars");
    }

    [Fact]
    public void LoadRoster_ClearsAnyPriorTeamAssignment()
    {
        // Same reasoning as AddPlayer/RemovePlayer: a team deal is dealt FROM
        // the roster, so replacing the roster must invalidate it — otherwise
        // a player from the OLD roster could be left with a team assignment
        // that no longer corresponds to anyone actually at the table.
        var (vm, _, _) = Build(new FakeTeamMode());
        vm.NewName = "Alice"; vm.AddPlayer();
        vm.NewName = "Bob"; vm.AddPlayer();
        vm.NewName = "Cara"; vm.AddPlayer();
        vm.NewName = "Dan"; vm.AddPlayer();
        vm.AssignTeams().Should().BeTrue("test setup: four players is enough for two teams");
        vm.HasTeams.Should().BeTrue("test setup");

        vm.LoadRoster(Roster("Regulars", new SavedPlayer("Eve", null, null)));

        vm.HasTeams.Should().BeFalse();
    }

    [Fact]
    public void LoadRoster_WithATeamRoster_RestoresTheSidesItCarries()
    {
        // Backlog item 26: the Roaster's "Team" template deals sides and stamps
        // SavedPlayer.Team. Loading such a roster must bring those assignments
        // in, so a team mode started from it has real sides — not the
        // unassigned table the item was filed against.
        var (vm, _, _) = Build(new FakeTeamMode());

        vm.LoadRoster(Roster("Regulars",
            new SavedPlayer("Amy", null, null, Team: "Red"),
            new SavedPlayer("Ben", null, null, Team: "Blue"),
            new SavedPlayer("Cara", null, null, Team: "Red"),
            new SavedPlayer("Dan", null, null, Team: "Blue")));

        vm.HasTeams.Should().BeTrue();
        vm.TeamSummary.Should().Contain("Red: Amy, Cara").And.Contain("Blue: Ben, Dan");

        var built = vm.BuildPlayers();
        built.Should().OnlyContain(p => p.Attributes.ContainsKey("team"));
        built.Single(p => p.DisplayName == "Amy").Attributes["team"].Should().Be("Red");
        built.Single(p => p.DisplayName == "Ben").Attributes["team"].Should().Be("Blue");
    }

    [Fact]
    public void LoadRoster_WithATeamlessRoster_AssignsNoTeams()
    {
        var (vm, _, _) = Build(new FakeTeamMode());

        vm.LoadRoster(Roster("Regulars",
            new SavedPlayer("Amy", null, null),
            new SavedPlayer("Ben", null, null)));

        vm.HasTeams.Should().BeFalse();
        vm.BuildPlayers().Should().OnlyContain(p => !p.Attributes.ContainsKey("team"));
    }

    [Fact]
    public void SavedRosterOption_Invoke_LoadsThatRoster()
    {
        var store = new FakeRosterStore([Roster("Regulars", new SavedPlayer("Alice", "female", 30))]);
        var (vm, _, _) = Build(rosterStore: store);
        var option = vm.SavedRosters.Single();

        option.Invoke();

        vm.Players.Should().ContainSingle().Which.Name.Should().Be("Alice");
    }

    [Fact]
    public void SavedRosterOption_LoadCommand_LoadsThatRoster()
    {
        // WinUI binds LoadCommand rather than calling Invoke() directly —
        // both must reach the same place, the same duality every other
        // per-item option class in this project carries (ZoneOption,
        // TerritoryOption, …).
        var store = new FakeRosterStore([Roster("Regulars", new SavedPlayer("Alice", "female", 30))]);
        var (vm, _, _) = Build(rosterStore: store);
        var option = vm.SavedRosters.Single();

        option.LoadCommand.Execute(null);

        vm.Players.Should().ContainSingle().Which.Name.Should().Be("Alice");
    }

    // ── StartAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task StartAsync_BelowMinimumPlayers_SetsErrorAndDoesNotInvokeCallback()
    {
        var invoked = false;
        var (vm, _, _) = Build(new FakeMode(minimumPlayers: 2), onStart: _ => { invoked = true; return Task.CompletedTask; });
        vm.NewName = "Alice"; vm.AddPlayer(); // only 1, needs 2

        await vm.StartAsync();

        invoked.Should().BeFalse();
        vm.HasError.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsync_AtMinimumPlayers_InvokesCallbackWithBuiltPlayers()
    {
        IReadOnlyList<IPlayer>? received = null;
        var (vm, _, _) = Build(new FakeMode(minimumPlayers: 1),
            onStart: players => { received = players; return Task.CompletedTask; });
        vm.NewName = "Alice"; vm.AddPlayer();

        await vm.StartAsync();

        received.Should().NotBeNull();
        received.Should().ContainSingle().Which.DisplayName.Should().Be("Alice");
        vm.HasError.Should().BeFalse();
    }

    // ── BackCommand ───────────────────────────────────────────────────────────

    [Fact]
    public void BackCommand_CallsNavigatorGoBack()
    {
        var (vm, _, nav) = Build();
        vm.BackCommand.Execute(null);
        nav.GoBackCount.Should().Be(1);
    }
}

/// <summary>
/// Replaces <c>BindableSurfaceTests.GenderOptions_MustBeAnInstanceProperty_OnEveryHead</c>,
/// which reflected over two per-head type names
/// (<c>TableTop.Maui.ViewModels.PlayerSetupViewModel</c>,
/// <c>TableTop.WinUI.ViewModels.PlayerSetupViewModel</c>) that stopped
/// existing once those classes were merged into
/// <see cref="TableTop.Presentation.ViewModels.PlayerSetupViewModel"/>. Its
/// own guard — <c>if (type is null) continue;</c> — meant it never found
/// either type and never ran its real assertion again: the same "cannot fail,
/// so proves nothing" shape as the tautological
/// <c>Registry_AllModes_HasNoDuplicates</c> test fixed earlier this project.
/// </summary>
public sealed class BindableSurfaceTests
{
    [Fact]
    public void GenderOptions_IsAnInstanceProperty_OnTheSharedViewModel()
    {
        // Located by reflection on the real, single, shared type rather than
        // two dead per-head names — this now actually runs its assertion.
        var type = typeof(TableTop.Presentation.ViewModels.PlayerSetupViewModel);

        var instance = type.GetProperty("GenderOptions",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var @static = type.GetProperty("GenderOptions",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        instance.Should().NotBeNull("the gender picker binds to GenderOptions, so it must be an instance property");
        @static.Should().BeNull("a static member is invisible to XAML {Binding} — that was the original bug this test caught");
    }
}
