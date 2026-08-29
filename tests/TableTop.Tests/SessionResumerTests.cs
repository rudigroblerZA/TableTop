using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Persistence;

namespace TableTop.Tests;

/// <summary>
/// <see cref="SessionResumer"/> had no test references at all and sat at 12.5%
/// line coverage — 42 of 48 lines never executed — despite being a pure
/// function with four documented refusal branches and being the resume path
/// <c>Directory.Build.props</c> names as the motivating bug-report example
/// ("the resume duplicate is back").
///
/// <para>
/// One test per branch, plus the two roster-merge rules that are easy to get
/// backwards: the live player wins on an id match, and scores are deliberately
/// <b>not</b> applied here because the controller restores them from the same
/// snapshot — doing both would double every score on screen.
/// </para>
/// </summary>
public sealed class SessionResumerTests
{
    // ── Refusal branches ─────────────────────────────────────────────────────

    [Fact]
    public void TryResolve_WithNullSnapshot_RefusesAndSaysWhy()
    {
        var result = SessionResumer.TryResolve(
            snapshot: null, AvailableModes, currentRoster: null, out var reason);

        result.Should().BeNull();
        reason.Should().Be("No saved session.");
    }

    [Fact]
    public void TryResolve_WithNoPlayers_RefusesAndSaysWhy()
    {
        var snapshot = Snapshot(ModeName, round: 3);
        snapshot.Players.Clear();

        var result = SessionResumer.TryResolve(snapshot, AvailableModes, null, out var reason);

        result.Should().BeNull();
        reason.Should().Be("The saved session has no players.");
    }

    [Fact]
    public void TryResolve_WhenTheModeIsGone_RefusesRatherThanSubstituting()
    {
        // Refusing beats dropping the table into a different game than the one
        // they were playing — the reason the method returns null here at all.
        var result = SessionResumer.TryResolve(
            Snapshot("A Mode That Was Deleted", round: 3), AvailableModes, null, out var reason);

        result.Should().BeNull();
        reason.Should().Contain("A Mode That Was Deleted").And.Contain("no longer available");
    }

    [Fact]
    public void TryResolve_MatchesTheModeNameCaseInsensitively()
    {
        var result = SessionResumer.TryResolve(
            Snapshot(ModeName.ToUpperInvariant(), round: 1), AvailableModes, null, out _);

        result.Should().NotBeNull("mode lookup is documented as OrdinalIgnoreCase");
        result!.Mode.Name.Should().Be(ModeName);
    }

    // ── The success path ─────────────────────────────────────────────────────

    [Fact]
    public void TryResolve_WithAKnownMode_ResolvesModePlayersAndSnapshot()
    {
        var snapshot = Snapshot(ModeName, round: 7);

        var result = SessionResumer.TryResolve(snapshot, AvailableModes, null, out var reason);

        result.Should().NotBeNull();
        reason.Should().BeEmpty("reason is only populated when resolution fails");
        result!.Mode.Name.Should().Be(ModeName);
        result.Players.Should().HaveCount(2);
        result.Snapshot.Should().BeSameAs(snapshot);
        result.Round.Should().Be(7);
        result.SavedAt.Should().Be(snapshot.SavedAt);
    }

    [Fact]
    public void TryResolve_BuildsThePlayerSummary_ForTheResumePrompt()
    {
        var result = SessionResumer.TryResolve(Snapshot(ModeName, round: 1), AvailableModes, null, out _);

        result!.PlayerSummary.Should().Be("Alice, Bob");
    }

    // ── Roster merge ─────────────────────────────────────────────────────────

    [Fact]
    public void TryResolve_PrefersTheLiveRosterPlayer_WhenTheIdMatches()
    {
        // The live object is the more current truth: a name changed since the
        // save should survive the resume.
        var snapshot = Snapshot(ModeName, round: 2);
        var savedId = snapshot.Players[0].PlayerId;
        var renamed = new Player(savedId, "Alice Renamed");

        var result = SessionResumer.TryResolve(snapshot, AvailableModes, [renamed], out _);

        result!.Players[0].Should().BeSameAs(renamed);
        result.Players[0].DisplayName.Should().Be("Alice Renamed");
    }

    [Fact]
    public void TryResolve_RebuildsPlayersMissingFromTheRoster()
    {
        var snapshot = Snapshot(ModeName, round: 2);
        var onlyFirst = new Player(snapshot.Players[0].PlayerId, "Alice");

        var result = SessionResumer.TryResolve(snapshot, AvailableModes, [onlyFirst], out _);

        result!.Players.Should().HaveCount(2);
        result.Players[1].Should().NotBeSameAs(onlyFirst);
        result.Players[1].DisplayName.Should().Be("Bob", "a player absent from the roster is rebuilt from the snapshot");
    }

    [Fact]
    public void TryResolve_RestoresAttributesAndTags_ForARebuiltPlayer()
    {
        var snapshot = Snapshot(ModeName, round: 2);
        snapshot.Players[0].Attributes["gender"] = "Female";
        snapshot.Players[0].Tags.Add("couple-member");

        var result = SessionResumer.TryResolve(snapshot, AvailableModes, null, out _);

        result!.Players[0].Attributes.Should().ContainKey("gender").WhoseValue.Should().Be("Female");
        result.Players[0].Tags.Should().Contain("couple-member");
    }

    [Fact]
    public void TryResolve_DoesNotApplyScores()
    {
        // Explicitly documented on Restore: the controller restores scores from
        // the snapshot when it resumes, and setting them twice would double
        // every score on screen.
        var snapshot = Snapshot(ModeName, round: 2);
        snapshot.Players[0].Score = 25;

        var result = SessionResumer.TryResolve(snapshot, AvailableModes, null, out _);

        result!.Players[0].Score.Should().Be(0,
            "SessionResumer deliberately leaves scores to the controller — applying them here double-counts");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static IReadOnlyList<IGameMode> AvailableModes => ArchetypeRegistry.Default().AllModes;

    private static string ModeName => ArchetypeRegistry.Default().AllModes[0].Name;

    private static SessionSnapshot Snapshot(string modeName, int round) => new()
    {
        ModeName = modeName,
        Round = round,
        Players =
        [
            new PlayerSessionState { PlayerId = Guid.NewGuid(), DisplayName = "Alice" },
            new PlayerSessionState { PlayerId = Guid.NewGuid(), DisplayName = "Bob" },
        ],
    };
}
