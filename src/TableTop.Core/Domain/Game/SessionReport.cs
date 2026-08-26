using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;

namespace TableTop.Core.Domain.Game;

/// <summary>
/// Post-game statistics report computed from the full turn history.
///
/// Generated at game end and attached to the game-ended event.
/// Intended to feed a stats screen, replay value, and achievements.
///
/// Create via <see cref="SessionReport.Build"/>.
/// </summary>
public sealed class SessionReport
{
    private SessionReport() { }

    // ── Overview ──────────────────────────────────────────────────────────────

    /// <summary>All turns played this session, in chronological order.</summary>
    public IReadOnlyList<TurnRecord> Turns { get; private init; } = [];

    /// <summary>Total turns completed (includes skips).</summary>
    public int TotalTurns => Turns.Count;

    /// <summary>Turns that were completed (not skipped or failed).</summary>
    public int CompletedTurns => Turns.Count(t => t.Outcome == CardOutcome.Completed);

    /// <summary>Turns that were skipped.</summary>
    public int SkippedTurns => Turns.Count(t => t.Outcome == CardOutcome.Skipped);

    /// <summary>Total rounds played.</summary>
    public int TotalRounds { get; private init; }

    /// <summary>Total wall-clock duration of the session.</summary>
    public TimeSpan Duration { get; private init; }

    // ── Per-player stats ──────────────────────────────────────────────────────

    /// <summary>Stats broken down per player.</summary>
    public IReadOnlyList<PlayerStats> PlayerStats { get; private init; } = [];

    // ── Achievements (session-scope "bests") ──────────────────────────────────

    /// <summary>
    /// Player who completed the most consecutive cards without skipping.
    /// Null when no one completed more than one.
    /// </summary>
    public StreakRecord? LongestStreak { get; private init; }

    /// <summary>
    /// Hardest card cleared (completed, not skipped) this session.
    /// Null when no cards were completed.
    /// </summary>
    public TurnRecord? HardestCardCleared { get; private init; }

    /// <summary>
    /// Fastest timed answer recorded (lowest elapsed, must be completed).
    /// Null when no timed outcomes were recorded.
    /// </summary>
    public TurnRecord? FastestAnswer { get; private init; }

    /// <summary>
    /// Player who skipped the most times.
    /// Null when nobody skipped.
    /// </summary>
    public (IPlayer Player, int SkipCount)? MostSkips { get; private init; }

    /// <summary>
    /// Player who scored the most points.
    /// </summary>
    public IPlayer? HighScorer { get; private init; }

    // ── Factory ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a <see cref="SessionReport"/> from the complete turn history.
    /// Called by the controller at game end.
    /// </summary>
    public static SessionReport Build(
        IReadOnlyList<TurnRecord> turns,
        IReadOnlyList<IPlayer>    players,
        int                       totalRounds,
        TimeSpan                  duration)
    {
        var perPlayer = players.Select(p => BuildPlayerStats(p, turns)).ToList().AsReadOnly();

        return new SessionReport
        {
            Turns          = turns,
            TotalRounds    = totalRounds,
            Duration       = duration,
            PlayerStats    = perPlayer,
            LongestStreak  = FindLongestStreak(turns, players),
            HardestCardCleared = turns
                .Where(t => t.Outcome == CardOutcome.Completed)
                .OrderByDescending(t => t.Card.Difficulty)
                .ThenByDescending(t => t.ScoreDelta)
                .FirstOrDefault(),
            FastestAnswer  = turns
                .Where(t => t.Outcome == CardOutcome.Completed && t.Elapsed.HasValue && t.Elapsed > TimeSpan.Zero)
                .OrderBy(t => t.Elapsed)
                .FirstOrDefault(),
            MostSkips      = perPlayer
                .Where(s => s.SkippedTurns > 0)
                .OrderByDescending(s => s.SkippedTurns)
                .Select(s => ((IPlayer)s.Player, s.SkippedTurns))
                .Cast<(IPlayer, int)?>()
                .FirstOrDefault(),
            HighScorer     = perPlayer
                .OrderByDescending(s => s.FinalScore)
                .Select(s => s.Player)
                .FirstOrDefault(),
        };
    }

    private static PlayerStats BuildPlayerStats(IPlayer player, IReadOnlyList<TurnRecord> turns)
    {
        var mine = turns.Where(t => t.Player.Id == player.Id).ToList();

        var avgElapsed = mine
            .Where(t => t.Elapsed.HasValue && t.Elapsed > TimeSpan.Zero)
            .Select(t => t.Elapsed!.Value.TotalSeconds)
            .DefaultIfEmpty()
            .Average();

        return new PlayerStats
        {
            Player          = player,
            TotalTurns      = mine.Count,
            CompletedTurns  = mine.Count(t => t.Outcome == CardOutcome.Completed),
            SkippedTurns    = mine.Count(t => t.Outcome == CardOutcome.Skipped),
            FinalScore      = mine.LastOrDefault()?.ScoreAfter ?? 0,
            TotalScoreDelta = mine.Sum(t => t.ScoreDelta),
            HardestCleared  = mine
                .Where(t => t.Outcome == CardOutcome.Completed)
                .OrderByDescending(t => t.Card.Difficulty)
                .FirstOrDefault()?.Card.Difficulty,
            AverageAnswerSeconds = avgElapsed > 0 ? avgElapsed : null,
        };
    }

    private static StreakRecord? FindLongestStreak(
        IReadOnlyList<TurnRecord> turns,
        IReadOnlyList<IPlayer>    players)
    {
        StreakRecord? best = null;

        foreach (var player in players)
        {
            var mine = turns.Where(t => t.Player.Id == player.Id).ToList();
            int current = 0;
            int max = 0;

            foreach (var turn in mine)
            {
                if (turn.Outcome == CardOutcome.Completed)
                {
                    current++;
                    if (current > max) max = current;
                }
                else
                {
                    current = 0;
                }
            }

            if (max > 0 && (best is null || max > best.Length))
                best = new StreakRecord(player, max);
        }

        return best;
    }
}

/// <summary>Statistics for a single player within a session.</summary>
public sealed record PlayerStats
{
    /// <summary>Player.</summary>
    public required IPlayer Player         { get; init; }
    /// <summary>TotalTurns.</summary>
    public required int TotalTurns         { get; init; }
    /// <summary>CompletedTurns.</summary>
    public required int CompletedTurns     { get; init; }
    /// <summary>SkippedTurns.</summary>
    public required int SkippedTurns       { get; init; }
    /// <summary>Current score value.</summary>
    public required int FinalScore         { get; init; }
    /// <summary>Current score value.</summary>
    public required int TotalScoreDelta    { get; init; }

    /// <summary>Highest difficulty tier successfully completed by this player.</summary>
    public Difficulty? HardestCleared { get; init; }

    /// <summary>Average seconds to answer when timing data was available. Null otherwise.</summary>
    public double? AverageAnswerSeconds { get; init; }
/// <summary>Fraction of turns completed (not skipped) by this player; 0.0–1.0.</summary>

    public double CompletionRate =>
        TotalTurns == 0 ? 0 : (double)CompletedTurns / TotalTurns;
}

/// <summary>Longest consecutive completion streak in a session.</summary>
public sealed record StreakRecord(IPlayer Player, int Length);