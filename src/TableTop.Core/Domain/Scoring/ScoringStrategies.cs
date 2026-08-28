using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;

namespace TableTop.Core.Domain.Scoring;

/// <summary>
/// Awards a fixed number of points for completing a card, zero for skipping or failing.
/// </summary>
public sealed class FixedScoringStrategy : IScoringStrategy
{
    private readonly int _pointsPerCompletion;

    /// <summary>Initialises a new <see cref="FixedScoringStrategy"/> instance.</summary>
    public FixedScoringStrategy(int pointsPerCompletion = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(pointsPerCompletion);
        _pointsPerCompletion = pointsPerCompletion;
    }

    /// <inheritdoc />
    public string Name => "Fixed";

    /// <inheritdoc />
    public int CalculateScore(ICard card, IPlayer player, CardOutcome outcome, TimeSpan? elapsed = null) =>
        outcome == CardOutcome.Completed ? _pointsPerCompletion : 0;
}

/// <summary>
/// Awards points proportional to the card's difficulty.
/// Easy = 1, Medium = 2, Hard = 3, Extreme = 5.
/// Skipped and failed cards score zero.
/// </summary>
public sealed class DifficultyBasedScoringStrategy : IScoringStrategy
{
    /// <inheritdoc />
    public string Name => "DifficultyBased";

    /// <inheritdoc />
    public int CalculateScore(ICard card, IPlayer player, CardOutcome outcome, TimeSpan? elapsed = null)
    {
        if (outcome != CardOutcome.Completed) return 0;

        return card.Difficulty switch
        {
            Difficulty.Easy => 1,
            Difficulty.Medium => 2,
            Difficulty.Hard => 3,
            Difficulty.Extreme => 5,
            _ => 1
        };
    }
}

/// <summary>
/// Awards a multiplier for consecutive completions (streak).
/// Streak is tracked via player tags: "streak:{n}".
/// </summary>
public sealed class StreakScoringStrategy : IScoringStrategy
{
    private readonly IScoringStrategy _base;
    private readonly int _streakMultiplier;

    /// <summary>Initialises a new <see cref="StreakScoringStrategy"/> instance.</summary>
    public StreakScoringStrategy(IScoringStrategy baseStrategy, int streakMultiplier = TableTopDefaults.Scoring.StreakMultiplier)
    {
        _base = baseStrategy ?? throw new ArgumentNullException(nameof(baseStrategy));
        _streakMultiplier = streakMultiplier;
    }

    /// <inheritdoc />
    public string Name => $"Streak({_base.Name})";

    /// <inheritdoc />
    public int CalculateScore(ICard card, IPlayer player, CardOutcome outcome, TimeSpan? elapsed = null)
    {
        var baseScore = _base.CalculateScore(card, player, outcome);
        if (baseScore == 0) return 0;

        var streakTag = player.Tags
            .FirstOrDefault(t => t.StartsWith("streak:", StringComparison.OrdinalIgnoreCase));

        if (streakTag is not null &&
            int.TryParse(streakTag.Split(':')[1], out var streak) &&
            streak >= 3)
        {
            return baseScore * _streakMultiplier;
        }

        return baseScore;
    }
}