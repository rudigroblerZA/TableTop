using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Game;
using TableTop.Core.Domain.Players;
using TableTop.Core.Domain.Scoring;
using TableTop.Hosting.Events;

namespace TableTop.Tests;

/// <summary>
/// Tests for Tier 5 gameplay enhancements:
///   5.1 Team play
///   5.2 Per-card timer / TimeBasedScoringStrategy
///   5.3 Session stats / SessionReport
///   5.4 Undo last turn
/// </summary>
public sealed class GameplayFeaturesTests
{
    // ── 5.1 Team play ─────────────────────────────────────────────────────────

    [Fact]
    public void Team_Create_WithMembers_HasCorrectName()
    {
        var alice = Player.Create("Alice");
        var bob   = Player.Create("Bob");
        var team  = new Team("Dream Team", [alice, bob]);

        team.Name.Should().Be("Dream Team");
        team.Members.Should().HaveCount(2);
        team.Score.Should().Be(0);
    }

    [Fact]
    public void Team_Contains_ReturnsTrueForMember()
    {
        var alice = Player.Create("Alice");
        var team  = new Team("Solo", [alice]);

        team.Contains(alice.Id).Should().BeTrue();
        team.Contains(Guid.NewGuid()).Should().BeFalse();
    }

    [Fact]
    public void Team_SplitEvenly_TwoTeams_EvenSplit()
    {
        var players = Enumerable.Range(1, 4)
            .Select(i => (IPlayer)Player.Create($"P{i}"))
            .ToList()
            .AsReadOnly();

        var teams = Team.SplitEvenly(players, teamCount: 2);

        teams.Should().HaveCount(2);
        teams[0].Members.Should().HaveCount(2);
        teams[1].Members.Should().HaveCount(2);
    }

    [Fact]
    public void Team_SplitEvenly_OddSplit_RemaiderGoesToLastTeam()
    {
        var players = Enumerable.Range(1, 5)
            .Select(i => (IPlayer)Player.Create($"P{i}"))
            .ToList()
            .AsReadOnly();

        var teams = Team.SplitEvenly(players, teamCount: 2);

        teams[0].Members.Should().HaveCount(2);
        teams[1].Members.Should().HaveCount(3); // remainder
    }

    [Fact]
    public void Team_FromGroups_BuildsCorrectTeams()
    {
        var alice = Player.Create("Alice");
        var bob   = Player.Create("Bob");
        var eve   = Player.Create("Eve");

        var teams = Team.FromGroups([
            ("Red",  new[] { alice, bob }.ToList<IPlayer>().AsReadOnly()),
            ("Blue", new[] { eve }.ToList<IPlayer>().AsReadOnly()),
        ]);

        teams.Should().HaveCount(2);
        teams[0].Name.Should().Be("Red");
        teams[1].Name.Should().Be("Blue");
    }

    [Fact]
    public void TeamPlayerManager_TeamOnly_CreditToTeamNotIndividual()
    {
        var alice = Player.Create("Alice");
        var bob   = Player.Create("Bob");
        var team  = new Team("A+B", [alice, bob]);
        var mgr   = new TeamPlayerManager([team], TeamScoreMode.TeamOnly);

        mgr.ApplyScore(alice.Id, 10);

        // Team score accumulates
        mgr.Teams[0].Score.Should().Be(10);

        // Individual score stays at zero (TeamOnly mode)
        mgr.Players.First(p => p.Id == alice.Id).Score.Should().Be(0);
    }

    [Fact]
    public void TeamPlayerManager_Both_CreditToTeamAndIndividual()
    {
        var alice = Player.Create("Alice");
        var team  = new Team("Solo", [alice]);
        var mgr   = new TeamPlayerManager([team], TeamScoreMode.Both);

        mgr.ApplyScore(alice.Id, 5);

        mgr.Teams[0].Score.Should().Be(5);
        mgr.Players.First(p => p.Id == alice.Id).Score.Should().Be(5);
    }

    [Fact]
    public void TeamPlayerManager_Individual_DoesNotCreditTeam()
    {
        var alice = Player.Create("Alice");
        var team  = new Team("Solo", [alice]);
        var mgr   = new TeamPlayerManager([team], TeamScoreMode.Individual);

        mgr.ApplyScore(alice.Id, 7);

        mgr.Teams[0].Score.Should().Be(0);  // team unchanged
        mgr.Players.First(p => p.Id == alice.Id).Score.Should().Be(7);
    }

    [Fact]
    public void TeamPlayerManager_GetStandings_OrdersByScore()
    {
        var p1    = Player.Create("P1");
        var p2    = Player.Create("P2");
        var t1    = new Team("Low",  [p1]);
        var t2    = new Team("High", [p2]);
        var mgr   = new TeamPlayerManager([t1, t2], TeamScoreMode.TeamOnly);

        mgr.ApplyScore(p1.Id, 3);
        mgr.ApplyScore(p2.Id, 9);

        var standings = mgr.GetStandings();
        standings[0].Name.Should().Be("High");
        standings[1].Name.Should().Be("Low");
    }

    [Fact]
    public void TeamPlayerManager_GetTeam_ReturnsCorrectTeam()
    {
        var alice = Player.Create("Alice");
        var team  = new Team("A", [alice]);
        var mgr   = new TeamPlayerManager([team]);

        var found = mgr.GetTeam(alice.Id);
        found.Should().NotBeNull();
        found!.Name.Should().Be("A");
    }

    // ── 5.2 Per-card timer / TimeBasedScoringStrategy ─────────────────────────

    [Fact]
    public void TimedCardOutcome_HasTiming_WhenElapsedNonZero()
    {
        var t = TimedCardOutcome.From(CardOutcome.Completed, TimeSpan.FromSeconds(15));
        t.HasTiming.Should().BeTrue();
        t.Elapsed.Should().Be(TimeSpan.FromSeconds(15));
    }

    [Fact]
    public void TimedCardOutcome_Untimed_HasTimingFalse()
    {
        var t = TimedCardOutcome.Untimed(CardOutcome.Completed);
        t.HasTiming.Should().BeFalse();
    }

    [Fact]
    public void TimeBasedScoring_Fast_EarnsMaxPoints()
    {
        var strat  = new TimeBasedScoringStrategy(
            fastThreshold:   TimeSpan.FromSeconds(10),
            mediumThreshold: TimeSpan.FromSeconds(30),
            slowThreshold:   TimeSpan.FromSeconds(60),
            fastPoints: 3, mediumPoints: 2, slowPoints: 1);

        var card   = StandardCard.Create("Q", "desc", Difficulty.Easy, "Test");
        var player = Player.Create("Alice");

        strat.CalculateScore(card, player, CardOutcome.Completed,
            elapsed: TimeSpan.FromSeconds(5)).Should().Be(3);
    }

    [Fact]
    public void TimeBasedScoring_Medium_EarnsMediumPoints()
    {
        var strat  = new TimeBasedScoringStrategy();
        var card   = StandardCard.Create("Q", "desc", Difficulty.Easy, "Test");
        var player = Player.Create("Alice");

        strat.CalculateScore(card, player, CardOutcome.Completed,
            elapsed: TimeSpan.FromSeconds(20)).Should().Be(2);
    }

    [Fact]
    public void TimeBasedScoring_Slow_EarnsSlowPoints()
    {
        var strat  = new TimeBasedScoringStrategy();
        var card   = StandardCard.Create("Q", "desc", Difficulty.Easy, "Test");
        var player = Player.Create("Alice");

        strat.CalculateScore(card, player, CardOutcome.Completed,
            elapsed: TimeSpan.FromSeconds(45)).Should().Be(1);
    }

    [Fact]
    public void TimeBasedScoring_TooSlow_EarnsZero()
    {
        var strat  = new TimeBasedScoringStrategy();
        var card   = StandardCard.Create("Q", "desc", Difficulty.Easy, "Test");
        var player = Player.Create("Alice");

        strat.CalculateScore(card, player, CardOutcome.Completed,
            elapsed: TimeSpan.FromSeconds(90)).Should().Be(0);
    }

    [Fact]
    public void TimeBasedScoring_Skipped_AlwaysZero()
    {
        var strat  = new TimeBasedScoringStrategy();
        var card   = StandardCard.Create("Q", "desc", Difficulty.Easy, "Test");
        var player = Player.Create("Alice");

        strat.CalculateScore(card, player, CardOutcome.Skipped,
            elapsed: TimeSpan.FromSeconds(1)).Should().Be(0);
    }

    [Fact]
    public void TimeBasedScoring_NoElapsed_FallsBackToOnePoint()
    {
        var strat  = new TimeBasedScoringStrategy();
        var card   = StandardCard.Create("Q", "desc", Difficulty.Easy, "Test");
        var player = Player.Create("Alice");

        // Standard IScoringStrategy overload — no elapsed
        strat.CalculateScore(card, player, CardOutcome.Completed).Should().Be(1);
    }

    // ── 5.3 Session stats / SessionReport ────────────────────────────────────

    [Fact]
    public void TurnRecord_StoresAllFields()
    {
        var player = Player.Create("Alice");
        var card   = StandardCard.Create("Q", "desc", Difficulty.Hard, "Cat");
        var rec    = new TurnRecord
        {
            TurnNumber  = 1,
            Round       = 1,
            Player      = player,
            Card        = card,
            Outcome     = CardOutcome.Completed,
            ScoreDelta  = 3,
            ScoreAfter  = 3,
            Elapsed     = TimeSpan.FromSeconds(12),
        };

        rec.Player.DisplayName.Should().Be("Alice");
        rec.Card.Difficulty.Should().Be(Difficulty.Hard);
        rec.Elapsed.Should().Be(TimeSpan.FromSeconds(12));
    }

    [Fact]
    public void SessionReport_Build_CountsTurnsCorrectly()
    {
        var alice  = Player.Create("Alice");
        var bob    = Player.Create("Bob");
        var card   = MakeCard(Difficulty.Easy);

        var turns = new List<TurnRecord>
        {
            MakeTurn(1, 1, alice, card, CardOutcome.Completed, 2, 2),
            MakeTurn(2, 1, bob,   card, CardOutcome.Skipped,   0, 0),
            MakeTurn(3, 2, alice, card, CardOutcome.Completed, 2, 4),
        };

        var report = SessionReport.Build(turns, [alice, bob], totalRounds: 2,
            duration: TimeSpan.FromMinutes(10));

        report.TotalTurns.Should().Be(3);
        report.CompletedTurns.Should().Be(2);
        report.SkippedTurns.Should().Be(1);
        report.TotalRounds.Should().Be(2);
    }

    [Fact]
    public void SessionReport_LongestStreak_IdentifiesCorrectPlayer()
    {
        var alice = Player.Create("Alice");
        var bob   = Player.Create("Bob");
        var card  = MakeCard(Difficulty.Easy);

        var turns = new List<TurnRecord>
        {
            MakeTurn(1, 1, alice, card, CardOutcome.Completed, 1, 1),
            MakeTurn(2, 1, bob,   card, CardOutcome.Skipped,   0, 0),
            MakeTurn(3, 2, alice, card, CardOutcome.Completed, 1, 2),
            MakeTurn(4, 2, alice, card, CardOutcome.Completed, 1, 3), // streak of 3 (turns 1,3,4 — but not consecutive in session)
        };

        // Alice has 3 completed, Bob 0. Alice streak = 3 in her individual list (1,3,4 are her 1st,2nd,3rd turns)
        var report = SessionReport.Build(turns, [alice, bob], 2, TimeSpan.FromMinutes(5));

        report.LongestStreak.Should().NotBeNull();
        report.LongestStreak!.Player.DisplayName.Should().Be("Alice");
        report.LongestStreak.Length.Should().Be(3);
    }

    [Fact]
    public void SessionReport_HardestCardCleared_IsCorrect()
    {
        var alice = Player.Create("Alice");
        var easy  = MakeCard(Difficulty.Easy);
        var hard  = MakeCard(Difficulty.Hard);

        var turns = new List<TurnRecord>
        {
            MakeTurn(1, 1, alice, easy, CardOutcome.Completed, 1, 1),
            MakeTurn(2, 1, alice, hard, CardOutcome.Completed, 2, 3),
        };

        var report = SessionReport.Build(turns, [alice], 1, TimeSpan.Zero);

        report.HardestCardCleared.Should().NotBeNull();
        report.HardestCardCleared!.Card.Difficulty.Should().Be(Difficulty.Hard);
    }

    [Fact]
    public void SessionReport_FastestAnswer_ReturnsLowestElapsed()
    {
        var alice = Player.Create("Alice");
        var card  = MakeCard(Difficulty.Easy);

        var turns = new List<TurnRecord>
        {
            MakeTurn(1, 1, alice, card, CardOutcome.Completed, 1, 1, TimeSpan.FromSeconds(30)),
            MakeTurn(2, 2, alice, card, CardOutcome.Completed, 1, 2, TimeSpan.FromSeconds(8)),
            MakeTurn(3, 3, alice, card, CardOutcome.Completed, 1, 3, TimeSpan.FromSeconds(20)),
        };

        var report = SessionReport.Build(turns, [alice], 3, TimeSpan.Zero);

        report.FastestAnswer.Should().NotBeNull();
        report.FastestAnswer!.Elapsed.Should().Be(TimeSpan.FromSeconds(8));
    }

    [Fact]
    public void SessionReport_MostSkips_FindsSkippingPlayer()
    {
        var alice = Player.Create("Alice");
        var bob   = Player.Create("Bob");
        var card  = MakeCard(Difficulty.Easy);

        var turns = new List<TurnRecord>
        {
            MakeTurn(1, 1, alice, card, CardOutcome.Completed, 1, 1),
            MakeTurn(2, 1, bob,   card, CardOutcome.Skipped,   0, 0),
            MakeTurn(3, 2, bob,   card, CardOutcome.Skipped,   0, 0),
        };

        var report = SessionReport.Build(turns, [alice, bob], 2, TimeSpan.Zero);

        report.MostSkips.Should().NotBeNull();
        report.MostSkips!.Value.Player.DisplayName.Should().Be("Bob");
        report.MostSkips.Value.SkipCount.Should().Be(2);
    }

    [Fact]
    public void SessionReport_HighScorer_IsPlayerWithMostPoints()
    {
        var alice = Player.Create("Alice");
        var bob   = Player.Create("Bob");
        var card  = MakeCard(Difficulty.Easy);

        var turns = new List<TurnRecord>
        {
            MakeTurn(1, 1, alice, card, CardOutcome.Completed, 5, 5),
            MakeTurn(2, 1, bob,   card, CardOutcome.Completed, 2, 2),
        };

        var report = SessionReport.Build(turns, [alice, bob], 1, TimeSpan.Zero);

        report.HighScorer!.DisplayName.Should().Be("Alice");
    }

    [Fact]
    public void PlayerStats_CompletionRate_IsCorrect()
    {
        var alice = Player.Create("Alice");
        var card  = MakeCard(Difficulty.Easy);

        var turns = new List<TurnRecord>
        {
            MakeTurn(1, 1, alice, card, CardOutcome.Completed, 1, 1),
            MakeTurn(2, 2, alice, card, CardOutcome.Skipped,   0, 1),
            MakeTurn(3, 3, alice, card, CardOutcome.Completed, 1, 2),
            MakeTurn(4, 4, alice, card, CardOutcome.Completed, 1, 3),
        };

        var report = SessionReport.Build(turns, [alice], 4, TimeSpan.Zero);
        var stats  = report.PlayerStats[0];

        stats.CompletionRate.Should().Be(0.75); // 3 of 4
    }

    // ── 5.4 Undo last turn ─────────────────────────────────────────────────────

    [Fact]
    public void UndoLastTurn_RevertsScore()
    {
        var cards   = MakeCardList(10);
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };
        var ctrl    = TestFactory.BuildController(cards, players, maxRounds: 5);

        int scoreAfterComplete = 0;
        int scoreAfterUndo     = 0;

        ctrl.TurnResult += (_, e) =>
        {
            // Capture Alice's score from the event (authoritative source)
            var aliceEntry = e.CurrentScores.FirstOrDefault(s => s.Name == "Alice");
            if (aliceEntry is not null) scoreAfterComplete = aliceEntry.Score;
        };

        ctrl.TurnUndone += (_, e) =>
        {
            var aliceEntry = e.CurrentScores.FirstOrDefault(s => s.Name == "Alice");
            if (aliceEntry is not null) scoreAfterUndo = aliceEntry.Score;
        };

        ctrl.Start();
        ctrl.RecordOutcome(CardOutcome.Completed);

        scoreAfterComplete.Should().BeGreaterThan(0,
            "at least one point should have been awarded for completion");

        ctrl.UndoLastTurn();

        scoreAfterUndo.Should().BeLessThan(scoreAfterComplete,
            "undo must reverse the score delta of the last turn");
    }

    [Fact]
    public void UndoLastTurn_RaisesEvent()
    {
        var cards   = MakeCardList(10);
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };
        var ctrl    = TestFactory.BuildController(cards, players, maxRounds: 5);

        TurnUndoneEvent? undoEvent = null;
        ctrl.TurnUndone += (_, e) => undoEvent = e;

        ctrl.Start();
        ctrl.RecordOutcome(CardOutcome.Completed);
        ctrl.UndoLastTurn();

        undoEvent.Should().NotBeNull("TurnUndone event must fire on successful undo");
        undoEvent!.PlayerName.Should().Be("Alice");
    }

    [Fact]
    public void UndoLastTurn_ReturnsFalse_WhenNothingToUndo()
    {
        var cards   = MakeCardList(10);
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };
        var ctrl    = TestFactory.BuildController(cards, players, maxRounds: 5);

        ctrl.Start();

        // No turns recorded yet — undo should return false
        var result = ctrl.UndoLastTurn();
        result.Should().BeFalse("there is nothing to undo before the first turn");
    }

    [Fact]
    public void UndoLastTurn_ReRaisesCardReady()
    {
        var cards   = MakeCardList(10);
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };
        var ctrl    = TestFactory.BuildController(cards, players, maxRounds: 5);

        var readyEvents = new List<CardReadyEvent>();
        ctrl.CardReady += (_, e) => readyEvents.Add(e);

        ctrl.Start();
        ctrl.RecordOutcome(CardOutcome.Completed);
        ctrl.UndoLastTurn();

        // After undo, CardReady must be raised again so the UI shows the card
        readyEvents.Should().HaveCountGreaterThan(1,
            "CardReady must fire again after UndoLastTurn so the UI re-shows the card");
    }

    [Fact]
    public void RecordTimedOutcome_StoresElapsedInReport()
    {
        var cards   = MakeCardList(10);
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };
        var ctrl    = TestFactory.BuildController(cards, players, maxRounds: 1);

        GameEndedEvent? endEvent = null;
        ctrl.GameEnded += (_, e) => endEvent = e;

        ctrl.Start();

        // Play all turns using RecordTimedOutcome
        var elapsed = TimeSpan.FromSeconds(12);
        while (ctrl.IsRunning)
            ctrl.RecordTimedOutcome(CardOutcome.Completed, elapsed);

        endEvent.Should().NotBeNull();
        endEvent!.Report.Should().NotBeNull();

        var timedTurns = endEvent.Report.Turns.Where(t => t.Elapsed.HasValue).ToList();
        timedTurns.Should().NotBeEmpty(
            "at least some turns should have elapsed time from RecordTimedOutcome");
    }

    [Fact]
    public void GameEndedEvent_IncludesSessionReport()
    {
        var cards   = MakeCardList(5);
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };
        var ctrl    = TestFactory.BuildController(cards, players, maxRounds: 1);

        GameEndedEvent? endEvent = null;
        ctrl.GameEnded += (_, e) => endEvent = e;

        ctrl.Start();
        while (ctrl.IsRunning) ctrl.RecordOutcome(CardOutcome.Completed);

        endEvent.Should().NotBeNull();
        endEvent!.Report.Should().NotBeNull();
        endEvent.Report.TotalTurns.Should().BeGreaterThan(0);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ICard MakeCard(Difficulty d) =>
        StandardCard.Create($"Card-{d}", "desc", d, "Test");

    private static IReadOnlyList<ICard> MakeCardList(int count) =>
        Enumerable.Range(0, count)
            .Select(_ => (ICard)StandardCard.Create("Q", "desc", Difficulty.Easy, "Test"))
            .ToList().AsReadOnly();

    private static TurnRecord MakeTurn(
        int turnNum, int round,
        IPlayer player, ICard card,
        CardOutcome outcome, int delta, int after,
        TimeSpan? elapsed = null) =>
        new()
        {
            TurnNumber  = turnNum,
            Round       = round,
            Player      = player,
            Card        = card,
            Outcome     = outcome,
            ScoreDelta  = delta,
            ScoreAfter  = after,
            Elapsed     = elapsed,
        };
}
