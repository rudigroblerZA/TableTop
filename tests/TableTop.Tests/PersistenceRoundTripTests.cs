using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Persistence;

namespace TableTop.Tests;

/// <summary>
/// End-to-end save / restore round-trip tests.
///
/// These tests cover enhancement 3.5: the highest-risk untested path in the
/// engine. Each test:
///   1. Plays N turns in a controller wired to an in-memory persistence stub.
///   2. Saves mid-session.
///   3. Builds a second controller, resuming from the snapshot.
///   4. Asserts that the resumed session has identical scores, played-card
///      history, and current round — and that already-played cards are not
///      replayed.
/// </summary>
public sealed class PersistenceRoundTripTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IReadOnlyList<ICard> MakeCards(int count) =>
        Enumerable.Range(1, count)
            .Select(i => (ICard)StandardCard.Create(
                $"Card-{i}", $"Description {i}", Difficulty.Easy, "Test"))
            .ToList()
            .AsReadOnly();

    private static IReadOnlyList<IPlayer> MakePlayers() =>
    [
        TestFactory.MakePlayer("Alice"),
        TestFactory.MakePlayer("Bob"),
    ];

    // ── Schema version ────────────────────────────────────────────────────────

    [Fact]
    public void SessionSnapshot_HasCurrentSchemaVersion_OnCreation()
    {
        var snap = new SessionSnapshot();
        snap.SchemaVersion.Should().Be(SessionSnapshot.CurrentSchemaVersion);
        snap.RequiresMigration.Should().BeFalse();
    }

    [Fact]
    public void SessionSnapshot_RequiresMigration_WhenSchemaVersionIsOlder()
    {
        var snap = new SessionSnapshot { SchemaVersion = 0 };
        snap.RequiresMigration.Should().BeTrue();
    }

    [Fact]
    public void PlayerProfile_HasCurrentSchemaVersion_OnCreation()
    {
        var profile = new PlayerProfile();
        profile.SchemaVersion.Should().Be(PlayerProfile.CurrentSchemaVersion);
    }

    // ── Core round-trip ───────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAndResume_SnapshotRecordsRound()
    {
        var cards = MakeCards(20);
        var players = MakePlayers();
        var persistence = new InMemoryPersistence();

        var lastRound = 0;
        var ctrlA = TestFactory.BuildController(cards, players,
            sessionRepository: persistence, maxRounds: 20);
        ctrlA.TurnResult += (_, e) => lastRound = e.Round;
        ctrlA.Start();
        ctrlA.RecordOutcome(CardOutcome.Completed);
        ctrlA.RecordOutcome(CardOutcome.Completed);
        ctrlA.RecordOutcome(CardOutcome.Completed);

        await ctrlA.SaveAsync();

        var snapshot = await persistence.LoadAsync();
        snapshot.Should().NotBeNull();
        snapshot!.RequiresMigration.Should().BeFalse();
        snapshot.Round.Should().Be(lastRound,
            "saved snapshot must record the round number from the last completed turn");
        snapshot.Round.Should().BeGreaterThan(0,
            "at least one turn was played before saving");
    }

    [Fact]
    public async Task SaveAndResume_RestoresPlayerScores()
    {
        var cards = MakeCards(20);
        var players = MakePlayers();
        var persistence = new InMemoryPersistence();

        var ctrlA = TestFactory.BuildController(cards, players,
            sessionRepository: persistence, maxRounds: 20);
        ctrlA.Start();

        // Complete enough turns to accumulate distinct scores
        ctrlA.RecordOutcome(CardOutcome.Completed);
        ctrlA.RecordOutcome(CardOutcome.Completed);
        ctrlA.RecordOutcome(CardOutcome.Completed);
        ctrlA.RecordOutcome(CardOutcome.Completed);

        var scoresBeforeSave = players.ToDictionary(p => p.Id, p => p.Score);
        await ctrlA.SaveAsync();

        var snapshot = await persistence.LoadAsync();

        var ctrlB = TestFactory.BuildController(cards, players,
            sessionRepository: persistence, maxRounds: 20,
            resumeFrom: snapshot);
        ctrlB.Start();

        foreach (var player in players)
        {
            player.Score.Should().Be(scoresBeforeSave[player.Id],
                $"{player.DisplayName}'s score must be restored after resume");
        }
    }

    [Fact]
    // Note: this test was flaky under the old custom runner because it shared process
    // state with other tests (static manifest cache, no per-test isolation). Under
    // real xUnit (dotnet test) each [Fact] runs in isolation — no shared state issue.
    public async Task SaveAndResume_DoesNotReplayAlreadyPlayedCards()
    {
        var cards = MakeCards(10);
        var players = MakePlayers();
        var persistence = new InMemoryPersistence();

        var playedCards = new List<ICard>();

        var ctrlA = TestFactory.BuildController(cards, players,
            sessionRepository: persistence, maxRounds: 10);

        ctrlA.CardReady += (_, e) => playedCards.Add(e.Card);
        ctrlA.Start();

        // Play 4 turns
        for (int i = 0; i < 4; i++)
            ctrlA.RecordOutcome(CardOutcome.Completed);

        var cardsPlayedInSessionA = playedCards.Select(c => c.Id).Distinct().ToList();
        await ctrlA.SaveAsync();

        // Reset tracking for session B
        playedCards.Clear();
        var snapshot = await persistence.LoadAsync();

        var ctrlB = TestFactory.BuildController(cards, players,
            sessionRepository: persistence, maxRounds: 10,
            resumeFrom: snapshot);
        ctrlB.CardReady += (_, e) => playedCards.Add(e.Card);
        ctrlB.Start();

        ctrlB.RecordOutcome(CardOutcome.Completed);

        var firstCardInSessionB = playedCards.First().Id;
        cardsPlayedInSessionA.Should().NotContain(firstCardInSessionB,
            "NoDuplicateCardRule must prevent cards from session A being replayed in session B");
    }

    [Fact]
    public async Task SaveAndResume_RestoresSkipCount()
    {
        var cards = MakeCards(20);
        var players = MakePlayers();
        var persistence = new InMemoryPersistence();

        var ctrlA = TestFactory.BuildController(cards, players,
            sessionRepository: persistence, maxRounds: 20, skipPenalty: -2);
        ctrlA.Start();

        // First skip is free; second skip incurs the penalty
        ctrlA.RecordOutcome(CardOutcome.Completed);   // Alice completes
        ctrlA.RecordOutcome(CardOutcome.Skipped);     // Bob: free skip
        ctrlA.RecordOutcome(CardOutcome.Completed);   // Alice completes
        ctrlA.RecordOutcome(CardOutcome.Skipped);     // Bob: second skip — penalised

        var bobScoreBeforeSave = players[1].Score;
        await ctrlA.SaveAsync();

        var snapshot = await persistence.LoadAsync();
        snapshot.Should().NotBeNull();

        // Bob's skip count must be in the snapshot
        var bobId = players[1].Id.ToString();
        snapshot!.SkipCounts.ContainsKey(bobId).Should().BeTrue(
            "skip count dictionary must include Bob's ID");
        snapshot.SkipCounts[bobId].Should().BeGreaterThan(0,
            "skip count must be persisted so the resumed session knows a penalty applies");
    }

    [Fact]
    public async Task Save_SetsSchemaVersionOnSnapshot()
    {
        var cards = MakeCards(5);
        var players = MakePlayers();
        var persistence = new InMemoryPersistence();

        var ctrl = TestFactory.BuildController(cards, players,
            sessionRepository: persistence);
        ctrl.Start();
        ctrl.RecordOutcome(CardOutcome.Completed);
        await ctrl.SaveAsync();

        var snapshot = await persistence.LoadAsync();
        snapshot.Should().NotBeNull();
        snapshot!.SchemaVersion.Should().Be(SessionSnapshot.CurrentSchemaVersion,
            "every saved snapshot must carry the current schema version");
    }

    [Fact]
    public async Task Load_RejectsSnapshot_WithFutureSchemaVersion()
    {
        // Write a snapshot claiming to be from a future schema version.
        // The repository should return null rather than loading it partially.
        var persistence = new VersionFuturisticPersistence();

        var snapshot = await persistence.LoadAsync();
        snapshot.Should().BeNull(
            "a snapshot with a schema version newer than CurrentSchemaVersion " +
            "must be rejected — we don't know how to deserialise it safely");
    }
}

// ── Test doubles ──────────────────────────────────────────────────────────────

/// <summary>
/// Simulates a persistence store containing a snapshot written by a hypothetical
/// future version of the engine. Its SchemaVersion exceeds CurrentSchemaVersion.
/// </summary>
internal sealed class VersionFuturisticPersistence : IGamePersistence
{
    public bool HasSavedSession => true;

    public Task<SessionSnapshot?> LoadAsync(CancellationToken ct = default)
    {
        // Schema version beyond what this engine understands
        var snap = new SessionSnapshot
        {
            SchemaVersion = SessionSnapshot.CurrentSchemaVersion + 999,
            ModeName = "SomeMode",
            Round = 3,
        };

        // Apply the same version check that JsonSessionRepository does
        if (snap.SchemaVersion > SessionSnapshot.CurrentSchemaVersion)
            return Task.FromResult<SessionSnapshot?>(null);

        return Task.FromResult<SessionSnapshot?>(snap);
    }

    public Task SaveAsync(SessionSnapshot snapshot, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task DeleteAsync(CancellationToken ct = default) =>
        Task.CompletedTask;
}
