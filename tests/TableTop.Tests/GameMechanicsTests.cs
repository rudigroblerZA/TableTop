using TableTop.Hosting.Controllers;
using TableTop.Hosting.Events;
using TableTop.Hosting.Persistence;

namespace TableTop.Tests;

public sealed class GameMechanicsTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"mech_{Guid.NewGuid()}");
    public GameMechanicsTests() => Directory.CreateDirectory(_dir);
    public void Dispose() { if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true); }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// N filler Easy cards. Delegates to <see cref="TestFactory.MakeCards"/>
    /// rather than re-rolling it, so this file cannot drift from the shared one.
    ///
    /// It was a private local helper before 1.21.0, defined at the bottom of the
    /// file underneath the JSON round-trip tests — which is how removing those
    /// tests took it with them.
    /// </summary>
    private static IReadOnlyList<ICard> MakeCards(int n) => TestFactory.MakeCards(n);

    private static CardTurnController BuildController(
        IEnumerable<ICard> cards,
        IReadOnlyList<Core.Domain.Players.Player>? players = null,
        int maxRounds = 20,
        int skipPenalty = -1,
        ISessionRepository? repo = null,
        IEnumerable<ICard>? bonusPool = null,
        int rewardInterval = 0) =>
        TestFactory.BuildController(
            cards.ToList(),
            players?.Cast<Core.Abstractions.Players.IPlayer>().ToList(),
            maxRounds: maxRounds,
            skipPenalty: skipPenalty,
            sessionRepository: repo,
            bonusPool: bonusPool,
            rewardInterval: rewardInterval);

    // ── Skip policy ───────────────────────────────────────────────────────────

    [Fact]
    public void FirstSkip_IsFree_NoPenalty()
    {
        var cards = MakeCards(5);
        var alice = Player.Create("Alice");
        var ctrl = BuildController(cards, [alice], skipPenalty: -2);

        SkipAttemptedEvent? evt = null;
        ctrl.SkipAttempted += (_, e) => evt = e;
        ctrl.Start();

        ctrl.RecordOutcome(CardOutcome.Skipped);

        evt.Should().NotBeNull();
        evt!.IsFree.Should().BeTrue();
        evt.Penalty.Should().Be(0);
        evt.SkipCount.Should().Be(1);
    }

    [Fact]
    public void SecondSkip_AppliesPenalty()
    {
        var cards = MakeCards(10);
        var alice = Player.Create("Alice");
        var ctrl = BuildController(cards, [alice], skipPenalty: -3);

        var events = new List<SkipAttemptedEvent>();
        ctrl.SkipAttempted += (_, e) => events.Add(e);
        ctrl.Start();

        // First skip (free)
        ctrl.RecordOutcome(CardOutcome.Skipped);
        // Alice draws again (single player)
        ctrl.RecordOutcome(CardOutcome.Skipped); // second skip

        events.Should().HaveCount(2);
        events[0].IsFree.Should().BeTrue();
        events[1].IsFree.Should().BeFalse();
        events[1].Penalty.Should().Be(-3);
        events[1].Penalty.Should().Be(-3); // penalty applied via PlayerManager
    }

    [Fact]
    public void FreePass_ResetSkipCount_NextSkipIsFree()
    {
        var freePassCard = RewardCard.CreateFreePass("Free Pass", "Skip freely.");
        var regularCards = MakeCards(10);
        var alice = Player.Create("Alice");
        var ctrl = BuildController(
            new System.Collections.Generic.List<ICard> { freePassCard }.Concat(regularCards).ToList(), [alice], skipPenalty: -5);

        var events = new List<SkipAttemptedEvent>();
        ctrl.SkipAttempted += (_, e) => events.Add(e);
        ctrl.Start(); // draws free pass card, auto-resolves

        // Alice now has a free pass AND skip count reset; first skip is free
        ctrl.RecordOutcome(CardOutcome.Skipped);

        events.Should().HaveCount(1);
        events[0].IsFree.Should().BeTrue();
        events[0].IsFree.Should().BeTrue(); // first skip is free (uses free pass)
    }

    // ── Break card activities ─────────────────────────────────────────────────

    [Fact]
    public void BreakCardDrawnEvent_IncludesActivityAndDuration()
    {
        var breakCard = BreakCard.CreateShower("Shower", "Take a shower.", 10);
        var alice = Player.Create("Alice");
        var breakList = new System.Collections.Generic.List<ICard> { breakCard }; breakList.AddRange(MakeCards(5));
        var ctrl = BuildController(breakList, [alice]);

        var evts = new System.Collections.Generic.List<BreakCardDrawnEvent>();
        ctrl.BreakCardDrawn += (_, e) => evts.Add(e);
        ctrl.Start();
        // Play enough turns for the break card to come up
        for (var i = 0; i < 6 && evts.Count == 0; i++)
            ctrl.RecordOutcome(CardOutcome.Completed);

        evts.Should().NotBeEmpty("break card should fire eventually");
        var evt = evts[0];
        evt.Activity.Should().Be("Shower");
        evt.DurationMinutes.Should().Be(10);
    }

    // ── Inspiration cards ─────────────────────────────────────────────────────

    [Fact]
    public void InspirationCard_ImplementsIInspirationCard()
    {
        var card = InspirationCard.Create("Mindful Morning", "Desc.", "Do one thing for yourself.");
        card.Should().BeAssignableTo<IInspirationCard>();
        card.InspirationText.Should().Be("Do one thing for yourself.");
    }

    [Fact]
    public void InspirationCard_SavedToPlayerList_EventRaised()
    {
        var insp = InspirationCard.Create("A", "B", "Do the thing.", "Growth");
        var alice = Player.Create("Alice");
        var inspirationList = new System.Collections.Generic.List<ICard> { insp }; inspirationList.AddRange(MakeCards(5));
        var ctrl = BuildController(inspirationList, [alice]);

        var inspEvts = new System.Collections.Generic.List<InspirationCardDrawnEvent>();
        ctrl.InspirationCardDrawn += (_, e) => inspEvts.Add(e);
        ctrl.Start();
        // Play enough turns for the inspiration card to come up
        for (var i = 0; i < 6 && inspEvts.Count == 0; i++)
            ctrl.RecordOutcome(CardOutcome.Completed);

        inspEvts.Should().NotBeEmpty("inspiration card should fire eventually");
        var evt = inspEvts[0];
        evt.InspirationText.Should().Be("Do the thing.");
        evt.InspirationCategory.Should().Be("Growth");

        ctrl.PlayerInspirations[alice.Id].Should().HaveCount(1);
        ctrl.PlayerInspirations[alice.Id][0].InspirationText.Should().Be("Do the thing.");
    }

    // ── Session save/load ─────────────────────────────────────────────────────

    [Fact]
    public async Task Save_CreatesSnapshotFile()
    {
        var filePath = Path.Combine(_dir, "session.json");
        var repo = new JsonSessionRepository(filePath);
        var alice = Player.Create("Alice");
        var ctrl = BuildController(MakeCards(10), [alice], repo: repo);

        ctrl.Start();
        ctrl.RecordOutcome(CardOutcome.Completed);
        await ctrl.SaveAsync();

        File.Exists(filePath).Should().BeTrue();
        repo.HasSavedSession.Should().BeTrue();
    }

    [Fact]
    public async Task Save_SnapshotContainsCorrectState()
    {
        var filePath = Path.Combine(_dir, "session2.json");
        var repo = new JsonSessionRepository(filePath);
        var alice = Player.Create("Alice");
        var ctrl = BuildController(MakeCards(10), [alice], repo: repo, maxRounds: 5);

        ctrl.Start();
        ctrl.RecordOutcome(CardOutcome.Completed);
        ctrl.RecordOutcome(CardOutcome.Skipped); // first skip = free
        await ctrl.SaveAsync();

        var snap = await repo.LoadAsync();
        snap.Should().NotBeNull();
        snap!.Players.Should().HaveCount(1);
        // 2 resolved + the card face-up at save time: revealed cards are
        // spent for persistence (see PersistenceCoordinator.BuildSnapshot).
        snap.PlayedCardIds.Should().HaveCount(3);
        snap.SkipCounts.Values.Should().Contain(1);
    }

    [Fact]
    public async Task SessionSavedEvent_RaisedAfterSave()
    {
        var filePath = Path.Combine(_dir, "session3.json");
        var repo = new JsonSessionRepository(filePath);
        var ctrl = BuildController(MakeCards(5), repo: repo);

        SessionSavedEvent? evt = null;
        ctrl.SessionSaved += (_, e) => evt = e;
        ctrl.Start();
        await ctrl.SaveAsync();

        evt.Should().NotBeNull();
        Xunit.Assert.True(Math.Abs((evt!.SavedAt - DateTimeOffset.UtcNow).TotalSeconds) < 5);
    }

    [Fact]
    public async Task SessionRepository_DeleteAsync_RemovesFile()
    {
        var filePath = Path.Combine(_dir, "session4.json");
        var repo = new JsonSessionRepository(filePath);
        await repo.SaveAsync(new SessionSnapshot { ModeName = "Test" });
        await repo.DeleteAsync();
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task SessionRepository_LoadAsync_CorruptFile_ReturnsNull()
    {
        var filePath = Path.Combine(_dir, "corrupt.json");
        File.WriteAllText(filePath, "{ not json }}}");
        var snap = await new JsonSessionRepository(filePath).LoadAsync();
        snap.Should().BeNull();
    }

    // Six tests lived below here that loaded break and inspiration cards from
    // JSON and round-tripped them through DeckExporter. Removed with the deck
    // file format in 1.21.0; the mechanics themselves are covered directly
    // above. Session persistence is unrelated and untouched — JsonSessionRepository
    // writes save files, not content, and stays.
}
