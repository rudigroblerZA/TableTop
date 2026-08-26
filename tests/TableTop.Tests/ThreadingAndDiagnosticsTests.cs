using TableTop.Core.Abstractions;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Rules;
using TableTop.Core.Domain.Rules;

namespace TableTop.Tests;

/// <summary>
/// Tests for enhancements 3.1 (threading guard) and 3.2 (engine diagnostics).
///
/// 3.1 — The ThreadingGuard asserts that all mutating controller methods are called
///        from the thread that called Start(). This is [Conditional("DEBUG")] so
///        only fires in Debug builds; in Release it is a zero-cost no-op.
///
/// 3.2 — IEngineDiagnostics receives rule denials, card selections, turn events,
///        and session lifecycle calls. NullEngineDiagnostics is the default (zero cost).
///        LoggerEngineDiagnostics wraps an ILogger. RuleEvaluator calls the sink for
///        every denial and every scored allow.
/// </summary>
/// <summary>
/// Shares a collection with <see cref="ControllerThreadingTests"/> so the two
/// never run concurrently. <c>ThreadingGuard.Enabled</c> is process-wide, and
/// xunit parallelises across test classes by default — the other class switches
/// the guard on to exercise it in Release, and if that window overlapped the
/// Release branch below, a call this test expects to be silent would throw.
/// </summary>
[Collection("ThreadingGuard")]
public sealed class ThreadingAndDiagnosticsTests
{
    // ── 3.1 ThreadingGuard ────────────────────────────────────────────────────

    [Fact]
    public void ThreadingGuard_BeforeStart_AllowsAnyThread()
    {
        // Before Start() is called _ownerThreadId == -1, so any thread is allowed.
        // This lets the controller be constructed on an async task thread via CreateAsync.
        var guard = new TableTop.Hosting.Controllers.ThreadingGuard_TestAccessor();

        // Should not throw — guard hasn't been armed yet
        var ex = Record.Exception(() => guard.Assert("SomeMethod"));
        ex.Should().BeNull("guard must be a no-op before TransferOwnership() is called");
    }

    [Fact]
    public void ThreadingGuard_SameThread_Passes()
    {
        var guard = new TableTop.Hosting.Controllers.ThreadingGuard_TestAccessor();
        guard.TransferOwnership();

        // Calling from the same thread must not throw
        var ex = Record.Exception(() => guard.Assert("SomeMethod"));
        ex.Should().BeNull("same thread must always pass the guard");
    }

    [Fact]
    public void ThreadingGuard_DifferentThread_ThrowsInDebug()
    {
        var guard = new TableTop.Hosting.Controllers.ThreadingGuard_TestAccessor();
        guard.TransferOwnership();  // arm on THIS thread

        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try { guard.Assert("RecordOutcome"); }
            catch (Exception e) { threadEx = e; }
        });
        thread.Start();
        thread.Join();

#if DEBUG
        threadEx.Should().NotBeNull("wrong-thread call must throw in Debug builds");
        threadEx.Should().BeOfType<InvalidOperationException>();
        threadEx!.Message.Should().Contain("CardTurnController.RecordOutcome");
        threadEx.Message.Should().Contain("not thread-safe");
#else
        // Release: the guard is off by default (ThreadingGuard.Enabled), so no
        // throw is expected. It used to be [Conditional("DEBUG")], which removed
        // the call site outright; backlog L.4 made it a runtime switch so a
        // Release build can turn it on to diagnose a threading bug. The
        // observable default is unchanged — hence the same assertion.
        threadEx.Should().BeNull("the guard is off by default in Release builds");
#endif
    }

    [Fact]
    public void Controller_Start_TransfersOwnershipToCallingThread()
    {
        var cards = MakeCards(10);
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };
        var ctrl = TestFactory.BuildController(cards, players, maxRounds: 3);

        // Start() must complete without throwing on the test thread
        var ex = Record.Exception(() => ctrl.Start());
        ex.Should().BeNull("Start() must transfer ownership to the calling thread");
    }

    [Fact]
    public void Controller_RecordOutcome_PassesOnOwnerThread()
    {
        var cards = MakeCards(10);
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };
        var ctrl = TestFactory.BuildController(cards, players, maxRounds: 2);
        ctrl.Start();

        var ex = Record.Exception(() => ctrl.RecordOutcome(CardOutcome.Completed));
        ex.Should().BeNull("RecordOutcome from the owner thread must not throw");
    }

    // ── 3.2 IEngineDiagnostics ────────────────────────────────────────────────

    [Fact]
    public void NullEngineDiagnostics_IsDefaultSingleton()
    {
        // NullEngineDiagnostics.Instance is a shared singleton — all methods are no-ops
        var d = NullEngineDiagnostics.Instance;
        d.Should().NotBeNull();
        d.Should().BeSameAs(NullEngineDiagnostics.Instance);

        // All calls must be no-ops (no throw, no side effects)
        var card = StandardCard.Create("Q", "desc", Difficulty.Easy, "Test");
        var player = TableTop.Core.Domain.Players.Player.Create("Alice");
        var rule = new RestrictionRule();
        IEngineDiagnostics nullDiag = d;  // call via interface to reach default impls

        var ex = Record.Exception(() =>
        {
            nullDiag.RuleDenied(rule, card, player, "reason");
            nullDiag.RuleAllowed(rule, card, player, 1);
            nullDiag.CardSelected(card, player, 1);
            nullDiag.NoCardAvailable(player, 5, 1);
            nullDiag.TurnRecorded(player, card, CardOutcome.Completed, 2, 1);
            nullDiag.TurnUndone(player, card, CardOutcome.Completed, 2);
            nullDiag.GameStarted("TestMode", 2);
            nullDiag.GameEnded("TestMode", 3, 10);
        });
        ex.Should().BeNull("NullEngineDiagnostics must never throw");
    }

    [Fact]
    public void RuleEvaluator_CallsDiagnostics_OnDenial()
    {
        var spy = new DiagnosticsSpy();
        var evaluator = new RuleEvaluator(
            [new RestrictionRule()],   // RestrictionRule will deny an adult-only card
            spy);

        var adultCard = StandardCard.Create(
            "Adult Card", "For adults",
            Difficulty.Easy, "Test",
            restriction: new TableTop.Core.Domain.Restrictions.AdultOnlyRestriction());

        var youngPlayer = TableTop.Core.Domain.Players.Player.Create("Teen",
            attributes: new Dictionary<string, string> { ["age"] = "16" });

        var context = TestFactory.MakeRuleContext();
        evaluator.Evaluate(adultCard, youngPlayer, context);

        spy.DeniedCalls.Should().HaveCountGreaterThan(0,
            "RestrictionRule must trigger RuleDenied on the diagnostics sink");
        spy.DeniedCalls[0].Rule.Should().BeOfType<RestrictionRule>();
        spy.DeniedCalls[0].Card.Should().BeSameAs(adultCard);
        spy.DeniedCalls[0].Player.Should().BeSameAs(youngPlayer);
        spy.DeniedCalls[0].Reason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RuleEvaluator_CallsDiagnostics_OnScoredAllow()
    {
        var spy = new DiagnosticsSpy();
        var evaluator = new RuleEvaluator(
            [new DifficultyScoreRule()],   // gives +1 for Hard cards
            spy);

        var hardCard = StandardCard.Create("Hard Q", "desc", Difficulty.Hard, "Test");
        var player = TableTop.Core.Domain.Players.Player.Create("Alice");
        var context = TestFactory.MakeRuleContext();

        var result = evaluator.Evaluate(hardCard, player, context);

        result.IsAllowed.Should().BeTrue();
        spy.AllowedCalls.Should().HaveCountGreaterThan(0,
            "DifficultyScoreRule gives +score, which should trigger RuleAllowed");
    }

    [Fact]
    public void RuleEvaluator_NullDiagnostics_IsDefault()
    {
        // Constructing without a sink must not throw and must evaluate correctly
        var evaluator = new RuleEvaluator([new RestrictionRule()]);
        var card = StandardCard.Create("Q", "desc", Difficulty.Easy, "Test");
        var player = TableTop.Core.Domain.Players.Player.Create("Alice");
        var context = TestFactory.MakeRuleContext();

        var result = evaluator.Evaluate(card, player, context);
        result.IsAllowed.Should().BeTrue("unrestricted card for any player must pass");
    }

    [Fact]
    public void Controller_CallsDiagnostics_GameStarted()
    {
        var spy = new DiagnosticsSpy();
        var cards = MakeCards(5);
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };
        var ctrl = TestFactory.BuildController(cards, players, maxRounds: 1, diagnostics: spy);

        ctrl.Start();

        spy.GameStartedCalls.Should().HaveCount(1);
        spy.GameStartedCalls[0].PlayerCount.Should().Be(2);
    }

    [Fact]
    public void Controller_CallsDiagnostics_TurnRecorded()
    {
        var spy = new DiagnosticsSpy();
        var cards = MakeCards(10);
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };
        var ctrl = TestFactory.BuildController(cards, players, maxRounds: 2, diagnostics: spy);

        ctrl.Start();
        ctrl.RecordOutcome(CardOutcome.Completed);

        spy.TurnRecordedCalls.Should().HaveCountGreaterThan(0,
            "TurnRecorded must fire after RecordOutcome");
    }

    [Fact]
    public void Controller_CallsDiagnostics_TurnUndone()
    {
        var spy = new DiagnosticsSpy();
        var cards = MakeCards(10);
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };
        var ctrl = TestFactory.BuildController(cards, players, maxRounds: 3, diagnostics: spy);

        ctrl.Start();
        ctrl.RecordOutcome(CardOutcome.Completed);
        ctrl.UndoLastTurn();

        spy.TurnUndoneCalls.Should().HaveCount(1, "UndoLastTurn must fire TurnUndone diagnostic");
    }

    [Fact]
    public void Controller_CallsDiagnostics_GameEnded()
    {
        var spy = new DiagnosticsSpy();
        var cards = MakeCards(4);
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };
        var ctrl = TestFactory.BuildController(cards, players, maxRounds: 1, diagnostics: spy);

        ctrl.Start();
        while (ctrl.IsRunning) ctrl.RecordOutcome(CardOutcome.Completed);

        spy.GameEndedCalls.Should().HaveCount(1, "GameEnded must fire once when the game finishes");
        spy.GameEndedCalls[0].ModeName.Should().NotBeEmpty();
    }

    [Fact]
    public void Controller_DiagnosticsDoNotAffectGameOutcome()
    {
        // Ensure diagnostics are truly a side channel — the game plays identically
        // with and without them.
        var cards = MakeCards(6);
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };

        var withDiag = TestFactory.BuildController(cards, players, maxRounds: 1,
            diagnostics: new DiagnosticsSpy());
        var withoutDiag = TestFactory.BuildController(cards, players, maxRounds: 1);

        int scoresWithDiag = 0;
        int scoresWithoutDiag = 0;

        withDiag.TurnResult += (_, e) => scoresWithDiag += e.ScoreDelta;
        withoutDiag.TurnResult += (_, e) => scoresWithoutDiag += e.ScoreDelta;

        withDiag.Start();
        while (withDiag.IsRunning) withDiag.RecordOutcome(CardOutcome.Completed);

        withoutDiag.Start();
        while (withoutDiag.IsRunning) withoutDiag.RecordOutcome(CardOutcome.Completed);

        scoresWithDiag.Should().Be(scoresWithoutDiag,
            "diagnostics must not alter game scoring or turn flow");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IReadOnlyList<ICard> MakeCards(int n) =>
        Enumerable.Range(0, n)
            .Select(_ => (ICard)StandardCard.Create("Q", "desc", Difficulty.Easy, "Test"))
            .ToList().AsReadOnly();
}

// ── Test doubles ──────────────────────────────────────────────────────────────

/// <summary>Recording spy for IEngineDiagnostics — captures all call arguments.</summary>
internal sealed class DiagnosticsSpy : IEngineDiagnostics
{
    public record DenialCall(IRule Rule, ICard Card, IPlayer Player, string Reason);
    public record AllowCall(IRule Rule, ICard Card, IPlayer Player, int ScoreDelta);
    public record TurnCall(IPlayer Player, ICard Card, CardOutcome Outcome, int ScoreDelta, int Round);
    public record UndoCall(IPlayer Player, ICard Card, CardOutcome Reversed, int ScoreRestored);
    public record StartCall(string ModeName, int PlayerCount);
    public record EndCall(string ModeName, int TotalRounds, int TotalTurns);

    public List<DenialCall> DeniedCalls { get; } = [];
    public List<AllowCall> AllowedCalls { get; } = [];
    public List<TurnCall> TurnRecordedCalls { get; } = [];
    public List<UndoCall> TurnUndoneCalls { get; } = [];
    public List<StartCall> GameStartedCalls { get; } = [];
    public List<EndCall> GameEndedCalls { get; } = [];

    public void RuleDenied(IRule rule, ICard card, IPlayer player, string reason)
        => DeniedCalls.Add(new(rule, card, player, reason));

    public void RuleAllowed(IRule rule, ICard card, IPlayer player, int scoreDelta)
        => AllowedCalls.Add(new(rule, card, player, scoreDelta));

    public void TurnRecorded(IPlayer player, ICard card, CardOutcome outcome, int scoreDelta, int round)
        => TurnRecordedCalls.Add(new(player, card, outcome, scoreDelta, round));

    public void TurnUndone(IPlayer player, ICard card, CardOutcome reversed, int scoreRestored)
        => TurnUndoneCalls.Add(new(player, card, reversed, scoreRestored));

    public void GameStarted(string modeName, int playerCount)
        => GameStartedCalls.Add(new(modeName, playerCount));

    public void GameEnded(string modeName, int totalRounds, int totalTurns)
        => GameEndedCalls.Add(new(modeName, totalRounds, totalTurns));
}
