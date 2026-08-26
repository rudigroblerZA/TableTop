namespace TableTop.Core.Abstractions.Scoring;

/// <summary>
/// Pairs a <see cref="CardOutcome"/> with the elapsed time the player took to answer.
///
/// Passed to <see cref="IScoringStrategy"/> when the host has per-card timing enabled. Scoring strategies that don't care
/// about timing simply ignore the elapsed parameter.
///
/// The elapsed time is also stored in <see cref="TableTop.Core.Domain.Game.TurnRecord"/> for post-game stats.
/// </summary>
public sealed record TimedCardOutcome(CardOutcome Outcome, TimeSpan Elapsed)
{
    /// <summary>Creates a timed outcome from a stopwatch reading.</summary>
    public static TimedCardOutcome From(CardOutcome outcome, TimeSpan elapsed) =>
        new(outcome, elapsed);

    /// <summary>Creates an untimed outcome (elapsed = zero, no timing data).</summary>
    public static TimedCardOutcome Untimed(CardOutcome outcome) =>
        new(outcome, TimeSpan.Zero);

    /// <summary>Whether timing data was actually recorded (elapsed &gt; zero).</summary>
    public bool HasTiming => Elapsed > TimeSpan.Zero;
}
