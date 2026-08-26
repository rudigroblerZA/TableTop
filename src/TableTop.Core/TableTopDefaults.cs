namespace TableTop.Core;

/// <summary>
/// Every tunable default the engine ships with, in one place (backlog D.2).
///
/// These were scattered across their use sites — the time-scoring thresholds in
/// <c>TimeBasedScoringStrategy</c>, the hint window in <c>DefaultHintEngine</c>,
/// the play-time estimate in <c>ModeManifestBuilder</c>, the round cap in
/// <c>ControllerFactory</c>. Each was reasonable where it sat, and collectively
/// they meant that tuning how the engine feels required knowing which file to
/// open. That is the whole complaint: not that the numbers are wrong, but that
/// they are unfindable.
///
/// Every one of these already has a constructor or options override, so nothing
/// here is load-bearing at runtime — changing a value here changes the default,
/// not the ceiling.
///
/// <b>Why this lives in Core</b> rather than being split per assembly: a couple
/// of these (hint window, history depth) are Hosting concerns, and putting them
/// here means Core declares constants it never uses. That is a small layering
/// smell, accepted deliberately — constants create no coupling, and splitting
/// the file across two assemblies would defeat the one thing this is for, which
/// is having a single place to look.
/// </summary>
public static class TableTopDefaults
{
    /// <summary>Session shape.</summary>
    public static class Session
    {
        /// <summary>Rounds a session runs for unless the caller says otherwise.</summary>
        public const int MaxRounds = 10;
    }

    /// <summary>Points awarded and deducted.</summary>
    public static class Scoring
    {
        /// <summary>Points for completing a card under the fixed strategy.</summary>
        public const int PointsPerCompletion = 1;

        /// <summary>
        /// Points deducted per skip after the first, which is free.
        /// Negative by convention — it is added, not subtracted.
        /// </summary>
        public const int SkipPenalty = -1;

        /// <summary>Multiplier applied to a scoring streak.</summary>
        public const int StreakMultiplier = 2;
    }

    /// <summary>
    /// Thresholds and awards for time-based scoring. The bands are generous on
    /// purpose: these are party games, and a table that feels rushed stops
    /// talking to each other, which is the opposite of the point.
    /// </summary>
    public static class TimeScoring
    {
        /// <summary>Answers inside this earn <see cref="FastPoints"/>.</summary>
        public const int FastSeconds = 10;

        /// <summary>Answers inside this earn <see cref="MediumPoints"/>.</summary>
        public const int MediumSeconds = 30;

        /// <summary>Answers inside this earn <see cref="SlowPoints"/>.</summary>
        public const int SlowSeconds = 60;

        /// <summary>Award for a fast answer.</summary>
        public const int FastPoints = 3;

        /// <summary>Award for a medium answer.</summary>
        public const int MediumPoints = 2;

        /// <summary>Award for a slow answer.</summary>
        public const int SlowPoints = 1;
    }

    /// <summary>
    /// Play-time estimation for a mode manifest. The spread is wide because it
    /// covers both "answer and move on" and "this one starts a conversation",
    /// and a single figure would be wrong for most decks.
    /// </summary>
    public static class Manifest
    {
        /// <summary>Lower bound per card — a quick answer, then move on.</summary>
        public const int SecondsPerCardMin = 90;

        /// <summary>Upper bound per card — full discussion or deliberation.</summary>
        public const int SecondsPerCardMax = 180;
    }

    /// <summary>Per-player turn history retained for hints and undo.</summary>
    public static class History
    {
        /// <summary>
        /// Turns kept per player. Deep enough for the hint engine's window with
        /// room to spare; not a full session, because this is held in memory for
        /// every player for the whole game.
        /// </summary>
        public const int MaxDepth = 10;
    }

    /// <summary>Hint engine tuning.</summary>
    public static class Hints
    {
        /// <summary>
        /// How many recent outcomes the engine reasons over. Three is short on
        /// purpose — a longer window makes hints lag behind how the player is
        /// actually doing.
        /// </summary>
        public const int WindowSize = 3;

        /// <summary>
        /// Total skips before the engine suggests an easier level. Counted for
        /// the whole session, not the recent window.
        /// </summary>
        public const int HeavySkipThreshold = 3;
    }
}
