using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Persistence;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Tests;

/// <summary>
/// This suite's own history is the point of it.
///
/// <para>
/// It previously carried a docstring claiming the "a session was found" path
/// <i>could not be exercised</i> — "not 'wasn't gotten to,' genuinely can't
/// be" — because <c>SavedSessionLookup</c> hardcoded <c>new ControllerFactory()</c>
/// with no injection point. That was true when written and stopped being true
/// when the constructor gained an optional <see cref="IControllerFactory"/>.
/// The docstring stayed, all five tests kept using the parameterless
/// constructor, and the found path kept having no coverage.
/// </para>
///
/// <para>
/// That gap had teeth. The fallback the parameterless constructor reaches has
/// <c>_persistence == null</c>, and <c>LoadSavedSessionAsync</c> returns null
/// unconditionally in that case — so a head that forgot to pass its factory got
/// a permanently false <c>CanResume</c> and no error. WinUI and MAUI both did
/// exactly that, and shipped unable to resume a session they were successfully
/// writing to disk. The tests below cover both sides now: the found path with an
/// injected factory, and the degradation cases that were always here.
/// </para>
/// </summary>
public sealed class SavedSessionLookupTests
{
    // ── The found path — what had no coverage ────────────────────────────────

    [Fact]
    public async Task RefreshAsync_WithAFactoryThatHasASavedSession_OffersIt()
    {
        var mode = ArchetypeRegistry.Default().AllModes[0];
        var lookup = new SavedSessionLookup(FactoryReturning(SnapshotFor(mode.Name, round: 4)));

        await lookup.RefreshAsync();

        lookup.CanResume.Should().BeTrue(
            "a factory whose persistence holds a resumable snapshot is the entire " +
            "reason SavedSessionLookup takes an IControllerFactory — this is the " +
            "assertion that was missing while WinUI and MAUI shipped unable to resume");
        lookup.Resumable.Should().NotBeNull();
        lookup.Resumable.Mode.Name.Should().Be(mode.Name);
        lookup.Resumable.Round.Should().Be(4);
    }

    [Fact]
    public async Task RefreshAsync_WithAFactoryThatHasASavedSession_BuildsTheResumeLabel()
    {
        var mode = ArchetypeRegistry.Default().AllModes[0];
        var lookup = new SavedSessionLookup(FactoryReturning(SnapshotFor(mode.Name, round: 4)));

        await lookup.RefreshAsync();

        lookup.ResumeText.Should().Be("Continue — Alice, Bob · round 4");
    }

    [Fact]
    public async Task RefreshAsync_UsesTheInjectedFactory_NotAFreshOne()
    {
        // The specific defect: an ignored factory looks identical to "nothing
        // saved" from the outside, so assert the injected one was actually
        // consulted rather than only that the answer came out right.
        var factory = FactoryReturning(SnapshotFor(ArchetypeRegistry.Default().AllModes[0].Name, round: 1));
        var lookup = new SavedSessionLookup(factory);

        await lookup.RefreshAsync();

        factory.LoadCallCount.Should().Be(1);
    }

    [Fact]
    public async Task RefreshAsync_WhenTheSavedModeNoLongerExists_OffersNothing()
    {
        // SessionResumer refuses rather than dropping the table into a
        // different game; the lookup must surface that as "no resume offer".
        var lookup = new SavedSessionLookup(
            FactoryReturning(SnapshotFor("A Mode That Was Deleted", round: 2)));

        await lookup.RefreshAsync();

        lookup.CanResume.Should().BeFalse();
        lookup.ResumeText.Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshAsync_WhenTheFactoryThrows_DegradesToNoOffer()
    {
        // Per the class's own doc comment: a corrupt save costs the player the
        // resume button, nothing more.
        var lookup = new SavedSessionLookup(new ThrowingControllerFactory());

        var act = async () => await lookup.RefreshAsync();

        await act.Should().NotThrowAsync();
        lookup.CanResume.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshAsync_AfterFindingASession_ClearsItWhenTheSaveGoesAway()
    {
        // MAUI re-checks on OnAppearing so a finished game doesn't leave a
        // stale offer. That only works if a later empty result overwrites the
        // earlier hit.
        var factory = FactoryReturning(SnapshotFor(ArchetypeRegistry.Default().AllModes[0].Name, round: 3));
        var lookup = new SavedSessionLookup(factory);

        await lookup.RefreshAsync();
        lookup.CanResume.Should().BeTrue();

        factory.Snapshot = null;
        await lookup.RefreshAsync();

        lookup.CanResume.Should().BeFalse("a resumable session must not survive the save being deleted");
    }

    // ── Degradation — a plain factory, now named rather than defaulted ───────
    //
    // These used `new SavedSessionLookup()`. Backlog X.2 removed that default,
    // so they say `PlainControllerFactory()` instead. The behaviour asserted is
    // identical; what changed is that choosing a persistence-less factory is
    // now a visible decision at the call site rather than what you get by
    // omission. That distinction is the entire fix — these five tests passing
    // while WinUI and MAUI could not resume is what it looks like when it is
    // missing.

    [Fact]
    public void InitialState_HasNothingToResume()
    {
        var lookup = new SavedSessionLookup(TestFactory.PlainControllerFactory());

        lookup.CanResume.Should().BeFalse();
        lookup.Resumable.Should().BeNull();
        lookup.ResumeText.Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshAsync_WithNoPersistenceConfigured_LeavesNothingToResume()
    {
        // Kept deliberately: this documents what a caller that forgets to pass
        // a factory actually gets. It is no longer the only case covered, which
        // is what made it misleading rather than wrong.
        var lookup = new SavedSessionLookup(TestFactory.PlainControllerFactory());

        await lookup.RefreshAsync();

        lookup.CanResume.Should().BeFalse();
        lookup.Resumable.Should().BeNull();
    }

    [Fact]
    public async Task RefreshAsync_NeverThrows()
    {
        var lookup = new SavedSessionLookup(TestFactory.PlainControllerFactory());

        var act = async () => await lookup.RefreshAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RefreshAsync_CanBeCalledRepeatedly_WithoutAccumulatingState()
    {
        var lookup = new SavedSessionLookup(TestFactory.PlainControllerFactory());

        await lookup.RefreshAsync();
        await lookup.RefreshAsync();
        await lookup.RefreshAsync();

        lookup.CanResume.Should().BeFalse("repeated calls with nothing to find must not somehow produce a stale resumable session");
    }

    [Fact]
    public void Constructor_RejectsANullFactory()
    {
        // The guarantee X.2 buys: passing nothing is a compile error, and
        // passing null explicitly fails immediately rather than degrading into
        // a lookup that silently reports "no saved session" forever.
        var act = () => new SavedSessionLookup(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ResumeText_IsEmpty_WhenThereIsNothingToResume()
    {
        var lookup = new SavedSessionLookup(TestFactory.PlainControllerFactory());
        lookup.ResumeText.Should().Be(string.Empty);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static StubControllerFactory FactoryReturning(SessionSnapshot snapshot) => new() { Snapshot = snapshot };

    private static SessionSnapshot SnapshotFor(string modeName, int round) => new()
    {
        ModeName = modeName,
        Round = round,
        Players =
        [
            new PlayerSessionState { PlayerId = Guid.NewGuid(), DisplayName = "Alice" },
            new PlayerSessionState { PlayerId = Guid.NewGuid(), DisplayName = "Bob" },
        ],
    };

    /// <summary>
    /// Stands in for a head's configured factory. Only
    /// <see cref="LoadSavedSessionAsync"/> is reachable from
    /// <c>SavedSessionLookup</c>; <see cref="CreateAsync"/> throws so a future
    /// change that starts calling it fails loudly rather than silently.
    /// </summary>
    private sealed class StubControllerFactory : IControllerFactory
    {
        public SessionSnapshot? Snapshot { get; set; }
        public int LoadCallCount { get; private set; }

        public Task<IGameController> CreateAsync(
            IGameMode mode, IReadOnlyList<IPlayer> players, int maxRounds = 10,
            GameplayOptions? gameplayOptions = null, SessionSnapshot? resumeFrom = null,
            int? monogamyWinningTokenCount = null, CancellationToken ct = default) =>
            throw new NotSupportedException("SavedSessionLookup should never build a controller.");

        public Task<SessionSnapshot?> LoadSavedSessionAsync(CancellationToken ct = default)
        {
            LoadCallCount++;
            return Task.FromResult(Snapshot);
        }
    }

    private sealed class ThrowingControllerFactory : IControllerFactory
    {
        public Task<IGameController> CreateAsync(
            IGameMode mode, IReadOnlyList<IPlayer> players, int maxRounds = 10,
            GameplayOptions? gameplayOptions = null, SessionSnapshot? resumeFrom = null,
            int? monogamyWinningTokenCount = null, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<SessionSnapshot?> LoadSavedSessionAsync(CancellationToken ct = default) =>
            throw new IOException("save file is unreadable");
    }
}
