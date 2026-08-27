using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;

namespace TableTop.Core.Domain.Scoring;

/// <summary>
/// Awards more points for faster completions.
///
/// Time tiers (configurable; defaults below):
///
/// | Elapsed        | Points |
/// |----------------|--------|
/// | ≤ fast         |   3    |
/// | ≤ medium       |   2    |
/// | ≤ slow         |   1    |
/// | &gt; slow       |   0    |
/// | Skip or Fail   |   0    |
///
/// When no elapsed time is available (host not using timed outcomes), falls back
/// to 1 point for completion — identical to <see cref="FixedScoringStrategy"/>(1).
/// </summary>
public sealed class TimeBasedScoringStrategy : IScoringStrategy
{
    private readonly TimeSpan _fastThreshold;
    private readonly TimeSpan _mediumThreshold;
    private readonly TimeSpan _slowThreshold;
    private readonly int _fastPoints;
    private readonly int _mediumPoints;
    private readonly int _slowPoints;

    /// <summary>
    /// Creates a time-based strategy with sensible defaults.
    /// </summary>
    /// <param name="fastThreshold">Answers within this time earn <paramref name="fastPoints"/>. Default: 10 s.</param>
    /// <param name="mediumThreshold">Answers within this time earn <paramref name="mediumPoints"/>. Default: 30 s.</param>
    /// <param name="slowThreshold">Answers within this time earn <paramref name="slowPoints"/>. Default: 60 s.</param>
    /// <param name="fastPoints">Points for fast answers. Default: 3.</param>
    /// <param name="mediumPoints">Points for medium answers. Default: 2.</param>
    /// <param name="slowPoints">Points for slow answers. Default: 1.</param>
    public TimeBasedScoringStrategy(
        TimeSpan? fastThreshold = null,
        TimeSpan? mediumThreshold = null,
        TimeSpan? slowThreshold = null,
        int fastPoints = TableTopDefaults.TimeScoring.FastPoints,
        int mediumPoints = TableTopDefaults.TimeScoring.MediumPoints,
        int slowPoints = TableTopDefaults.TimeScoring.SlowPoints)
    {
        _fastThreshold = fastThreshold ?? TimeSpan.FromSeconds(TableTopDefaults.TimeScoring.FastSeconds);
        _mediumThreshold = mediumThreshold ?? TimeSpan.FromSeconds(TableTopDefaults.TimeScoring.MediumSeconds);
        _slowThreshold = slowThreshold ?? TimeSpan.FromSeconds(TableTopDefaults.TimeScoring.SlowSeconds);
        _fastPoints = fastPoints;
        _mediumPoints = mediumPoints;
        _slowPoints = slowPoints;
    }
    /// <inheritdoc />

    public string Name => "TimeBased";

    /// <summary>
    /// Scores based on elapsed time. The <paramref name="elapsed"/> parameter is
    /// provided when the host calls <c>RecordTimedOutcome</c>. When null (untimed),
    /// falls back to 1 point for completion.
    /// </summary>
    public int CalculateScore(
        ICard card,
        IPlayer player,
        CardOutcome outcome,
        TimeSpan? elapsed = null)
    {
        if (outcome != CardOutcome.Completed) return 0;

        // No timing data — give one point for completion
        if (elapsed is null || elapsed == TimeSpan.Zero)
            return 1;

        if (elapsed <= _fastThreshold) return _fastPoints;
        if (elapsed <= _mediumThreshold) return _mediumPoints;
        if (elapsed <= _slowThreshold) return _slowPoints;
        return 0;
    }
}