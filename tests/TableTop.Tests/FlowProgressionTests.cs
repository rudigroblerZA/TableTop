using TableTop.Core.Abstractions.Progression;
using TableTop.Core.Domain.Progression;
using TableTop.Hosting.Controllers;
using TableTop.Hosting.Events;

namespace TableTop.Tests;

public sealed class FlowProgressionTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"flow_{Guid.NewGuid()}");
    public FlowProgressionTests() => Directory.CreateDirectory(_dir);
    public void Dispose() { if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true); }

    // ── FlowState — difficulty movement ──────────────────────────────────────

    [Fact]
    public void FlowState_DefaultsToEasyNormal()
    {
        var fs = new FlowState();
        fs.CurrentDifficulty.Should().Be(Difficulty.Easy);
        fs.CurrentPace.Should().Be(FlowPace.Normal);
    }

    [Fact]
    public void LevelUp_AdvancesDifficulty()
    {
        var fs = new FlowState(Difficulty.Easy);
        fs.LevelUp().Should().BeTrue();
        fs.CurrentDifficulty.Should().Be(Difficulty.Medium);
    }

    [Fact]
    public void LevelUp_AtExtreme_ReturnsFalse()
    {
        var fs = new FlowState(Difficulty.Extreme);
        fs.LevelUp().Should().BeFalse();
        fs.CurrentDifficulty.Should().Be(Difficulty.Extreme);
    }

    [Fact]
    public void LevelDown_DecreasesDifficulty()
    {
        var fs = new FlowState(Difficulty.Hard);
        fs.LevelDown().Should().BeTrue();
        fs.CurrentDifficulty.Should().Be(Difficulty.Medium);
    }

    [Fact]
    public void LevelDown_AtEasy_ReturnsFalse()
    {
        var fs = new FlowState(Difficulty.Easy);
        fs.LevelDown().Should().BeFalse();
        fs.CurrentDifficulty.Should().Be(Difficulty.Easy);
    }

    [Fact]
    public void SetDifficulty_JumpsDirectly()
    {
        var fs = new FlowState();
        fs.SetDifficulty(Difficulty.Extreme);
        fs.CurrentDifficulty.Should().Be(Difficulty.Extreme);
    }

    // ── FlowState — pace movement ─────────────────────────────────────────────

    [Fact]
    public void SpeedUp_IncreasePace()
    {
        var fs = new FlowState(initialPace: FlowPace.Normal);
        fs.SpeedUp().Should().BeTrue();
        fs.CurrentPace.Should().Be(FlowPace.Fast);
    }

    [Fact]
    public void SlowDown_DecreasePace()
    {
        var fs = new FlowState(initialPace: FlowPace.Normal);
        fs.SlowDown().Should().BeTrue();
        fs.CurrentPace.Should().Be(FlowPace.Slow);
    }

    [Fact]
    public void SpeedUp_AtSprint_ReturnsFalse()
    {
        var fs = new FlowState(initialPace: FlowPace.Sprint);
        fs.SpeedUp().Should().BeFalse();
    }

    [Fact]
    public void SlowDown_AtSlow_ReturnsFalse()
    {
        var fs = new FlowState(initialPace: FlowPace.Slow);
        fs.SlowDown().Should().BeFalse();
    }

    // ── FlowState — auto-escalation ───────────────────────────────────────────

    [Fact]
    public void CardsBeforeEscalation_MatchesPace()
    {
        new FlowState(initialPace: FlowPace.Slow).CardsBeforeEscalation.Should().Be(8);
        new FlowState(initialPace: FlowPace.Normal).CardsBeforeEscalation.Should().Be(4);
        new FlowState(initialPace: FlowPace.Fast).CardsBeforeEscalation.Should().Be(2);
        new FlowState(initialPace: FlowPace.Sprint).CardsBeforeEscalation.Should().Be(1);
    }

    [Fact]
    public void RecordCardPlayed_ReturnsTrueAtThreshold()
    {
        var fs = new FlowState(initialPace: FlowPace.Fast); // threshold = 2
        fs.RecordCardPlayed().Should().BeFalse(); // 1st
        fs.RecordCardPlayed().Should().BeTrue();  // 2nd — escalate
    }

    [Fact]
    public void ResetLevelCounter_ClearsCount()
    {
        var fs = new FlowState(initialPace: FlowPace.Fast);
        fs.RecordCardPlayed();
        fs.ResetLevelCounter();
        fs.CardsPlayedAtCurrentLevel.Should().Be(0);
    }

    // ── FlowAwareProgressionStrategy ─────────────────────────────────────────

    [Fact]
    public void Strategy_CreatesDefaultFlowState_ForNewPlayer()
    {
        var strat = new FlowAwareProgressionStrategy();
        var player = Player.Create("Alice");
        var state = strat.GetFlowState(player.Id);
        state.Should().NotBeNull();
        state.CurrentDifficulty.Should().Be(Difficulty.Easy);
    }

    [Fact]
    public void Strategy_SelectsCardAtCurrentDifficulty()
    {
        var strat = new FlowAwareProgressionStrategy(Difficulty.Hard);
        var player = Player.Create("Alice");
        strat.SetFlowState(player.Id, new FlowState(Difficulty.Hard));

        var cards = new[]
        {
            StandardCard.Create("E1", "d", Difficulty.Easy,    "T"),
            StandardCard.Create("H1", "d", Difficulty.Hard,    "T"),
            StandardCard.Create("H2", "d", Difficulty.Hard,    "T"),
        };
        var deck = BuildDeck(cards);
        var ctx = new ProgressionContext(1, [], [player]);

        var drawnId = strat.SelectCandidate(player, deck, ctx); // advance
        deck.Peek(c => c.Id == drawnId)!.Difficulty.Should().Be(Difficulty.Hard);
    }

    [Fact]
    public void Strategy_FallsBackToNearest_WhenPreferredUnavailable()
    {
        var strat = new FlowAwareProgressionStrategy(Difficulty.Extreme);
        var player = Player.Create("Alice");

        var cards = new[]
        {
            StandardCard.Create("E1", "d", Difficulty.Easy, "T"),
            StandardCard.Create("M1", "d", Difficulty.Medium, "T"),
        };
        var deck = BuildDeck(cards);
        var ctx = new ProgressionContext(1, [], [player]);

        var drawnId = strat.SelectCandidate(player, deck, ctx); // advance
        drawnId.Should().NotBeNull(); // Falls back to nearest available
        drawnId.Should().NotBeNull(); // falls back to nearest
    }

    [Fact]
    public void Strategy_AutoEscalates_AfterThreshold()
    {
        var strat = new FlowAwareProgressionStrategy(Difficulty.Easy, FlowPace.Fast); // 2 cards
        var player = Player.Create("Alice");

        var cards = Enumerable.Range(1, 10)
            .Select(i => StandardCard.Create($"E{i}", "d", Difficulty.Easy, "T"))
            .Concat(Enumerable.Range(1, 5)
                .Select(i => StandardCard.Create($"M{i}", "d", Difficulty.Medium, "T")))
            .ToArray();

        var deck = BuildDeck(cards);
        var ctx = new ProgressionContext(1, [], [player]);

        // Draw 2 Easy cards (threshold for Fast)
        strat.SelectCandidate(player, deck, ctx); // advance
        strat.SelectCandidate(player, deck, ctx); // advance

        // After threshold, level should have advanced
        strat.GetFlowState(player.Id).CurrentDifficulty.Should().Be(Difficulty.Medium);
    }

    // ── Controller integration ────────────────────────────────────────────────

    [Fact]
    public void Controller_SupportsFlow_WhenFlowAwareStrategy()
    {
        var ctrl = BuildController(MakeCards(10), useFlow: true);
        ctrl.SupportsFlow.Should().BeTrue();
    }

    [Fact]
    public void Controller_SupportsFlow_False_WhenLinearStrategy()
    {
        var ctrl = BuildController(MakeCards(10), useFlow: false);
        ctrl.SupportsFlow.Should().BeFalse();
    }

    [Fact]
    public void LevelUp_RaisesFlowChangedEvent()
    {
        var alice = Player.Create("Alice");
        var ctrl = BuildController(MakeCards(10), players: [alice], useFlow: true);
        ctrl.Start();

        FlowChangedEvent? evt = null;
        ctrl.FlowChanged += (_, e) => evt = e;

        ctrl.LevelUp(alice.Id);

        evt.Should().NotBeNull();
        evt.Change.Should().Be("LevelUp");
        evt.NewDifficulty.Should().Be("Medium");
    }

    [Fact]
    public void LevelDown_RaisesFlowChangedEvent()
    {
        var alice = Player.Create("Alice");
        var ctrl = BuildController(MakeCards(10), players: [alice], useFlow: true);
        ctrl.Start();
        ctrl.LevelUp(alice.Id); // go to Medium first

        FlowChangedEvent? evt = null;
        ctrl.FlowChanged += (_, e) => evt = e;
        ctrl.LevelDown(alice.Id);

        evt!.NewDifficulty.Should().Be("Easy");
    }

    [Fact]
    public void SpeedUp_RaisesFlowChangedEvent()
    {
        var alice = Player.Create("Alice");
        var ctrl = BuildController(MakeCards(10), players: [alice], useFlow: true);
        ctrl.Start();

        FlowChangedEvent? evt = null;
        ctrl.FlowChanged += (_, e) => evt = e;
        ctrl.SpeedUp(alice.Id);

        evt!.Change.Should().Be("SpeedUp");
        evt.NewPace.Should().Be("Fast");
    }

    [Fact]
    public void JumpTo_SetsExactDifficulty()
    {
        var alice = Player.Create("Alice");
        var ctrl = BuildController(MakeCards(10), players: [alice], useFlow: true);
        ctrl.Start();

        FlowChangedEvent? evt = null;
        ctrl.FlowChanged += (_, e) => evt = e;
        ctrl.JumpTo(alice.Id, Difficulty.Extreme);

        evt!.Change.Should().Be("JumpTo");
        evt.NewDifficulty.Should().Be("Extreme");
        ctrl.GetFlowState(alice.Id)!.CurrentDifficulty.Should().Be(Difficulty.Extreme);
    }

    [Fact]
    public void ResetFlow_ReturnsToDifficulty_Easy()
    {
        var alice = Player.Create("Alice");
        var ctrl = BuildController(MakeCards(10), players: [alice], useFlow: true);
        ctrl.Start();

        ctrl.LevelUp(alice.Id);
        ctrl.LevelUp(alice.Id);
        ctrl.ResetFlow(alice.Id);

        ctrl.GetFlowState(alice.Id)!.CurrentDifficulty.Should().Be(Difficulty.Easy);
    }

    [Fact]
    public async Task FlowCommands_NoOp_WhenNotFlowAware()
    {
        var alice = Player.Create("Alice");
        var ctrl = BuildController(MakeCards(10), players: [alice], useFlow: false);

        FlowChangedEvent? evt = null;
        ctrl.FlowChanged += (_, e) => evt = e;
        ctrl.LevelUp(alice.Id);

        evt.Should().BeNull(); // no event when not flow-aware
    }

    // ── Snapshot round-trip with flow state ───────────────────────────────────

    [Fact]
    public async Task Save_IncludesFlowState_InSnapshot()
    {
        var filePath = Path.Combine(_dir, "flow_session.json");
        var repo = new TableTop.Hosting.Persistence.JsonSessionRepository(filePath);
        var alice = Player.Create("Alice");
        var ctrl = BuildController(MakeCards(10), players: [alice],
                            useFlow: true, repo: repo);
        ctrl.Start();
        ctrl.LevelUp(alice.Id);
        ctrl.SpeedUp(alice.Id);
        await ctrl.SaveAsync();

        var snap = await repo.LoadAsync();
        snap!.FlowStates.Should().NotBeNull();
        snap.FlowStates[alice.Id.ToString()].Difficulty.Should().Be("Medium");
        snap.FlowStates[alice.Id.ToString()].Pace.Should().Be("Fast");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IReadOnlyList<ICard> MakeCards(int n) =>
        Enumerable.Range(1, n / 2)
            .Select(i => (ICard)StandardCard.Create($"E{i}", "d", Difficulty.Easy, "T"))
            .Concat(Enumerable.Range(1, n / 2)
                .Select(i => StandardCard.Create($"M{i}", "d", Difficulty.Medium, "T")))
            .ToList().AsReadOnly();

    private static Core.Domain.Decks.Deck BuildDeck(IEnumerable<ICard> cards)
    {
        var list = cards.ToList();
        return new Core.Domain.Decks.Deck(Guid.NewGuid(), "Test", list);
    }

    private static CardTurnController BuildController(
        IReadOnlyList<ICard> cards,
        IReadOnlyList<Player>? players = null,
        bool useFlow = true,
        int maxRounds = 20,
        TableTop.Hosting.Persistence.ISessionRepository? repo = null)
    {
        IProgressionStrategy strat = useFlow
            ? new FlowAwareProgressionStrategy()
            : new LinearProgressionStrategy();

        return TestFactory.BuildController(
            cards,
            players?.Cast<Core.Abstractions.Players.IPlayer>().ToList(),
            maxRounds: maxRounds,
            progression: strat,
            sessionRepository: repo);
    }
}