using TableTop.Core.Abstractions.Cards;

namespace TableTop.Core.Abstractions.Progression;

/// <summary>
/// Tracks a player's current position in the two-dimensional flow space:
/// <list type="bullet">
///   <item><description>
///     <b>Difficulty level</b> — which tier of card they receive (<see cref="Difficulty"/>).
///   </description></item>
///   <item><description>
///     <b>Pace</b> — how quickly automatic difficulty escalation advances (<see cref="TableTop.Core.Abstractions.Progression.FlowPace"/>).
///   </description></item>
/// </list>
///
/// Both dimensions can be changed freely at any time without penalty.
/// The state is mutable so the controller can update it in response to
/// player commands (<c>LevelUp</c>, <c>LevelDown</c>, <c>SpeedUp</c>, etc.).
/// </summary>
public sealed class FlowState
{
    private Difficulty _difficulty;
    /// <summary>Creates a <see cref="FlowState"/> at the given starting position.</summary>
    private FlowPace   _pace;

    /// <summary>Creates a <see cref="FlowState"/> at the given starting difficulty and pace.</summary>
    public FlowState(
        Difficulty initialDifficulty = Difficulty.Easy,
        FlowPace   initialPace       = FlowPace.Normal)
    {
        _difficulty = initialDifficulty;
        _pace       = initialPace;
    }

    // ── Current position ──────────────────────────────────────────────────────

    /// <summary>Current difficulty tier for this player.</summary>
    public Difficulty CurrentDifficulty => _difficulty;

    /// <summary>Current pace for this player.</summary>
    public FlowPace CurrentPace => _pace;

    // ── Movement — difficulty ─────────────────────────────────────────────────

    /// <summary>
    /// Moves the difficulty one step harder.
    /// No-op when already at <see cref="Difficulty.Extreme"/>.
    /// Returns true when the level changed.
    /// </summary>
    public bool LevelUp()
    {
        var next = _difficulty switch
        {
            Difficulty.Easy    => Difficulty.Medium,
            Difficulty.Medium  => Difficulty.Hard,
            Difficulty.Hard    => Difficulty.Extreme,
            _                  => _difficulty
        };
        if (next == _difficulty) return false;
        _difficulty = next;
        return true;
    }

    /// <summary>
    /// Moves the difficulty one step easier.
    /// No-op when already at <see cref="Difficulty.Easy"/>.
    /// Returns true when the level changed.
    /// </summary>
    public bool LevelDown()
    {
        var prev = _difficulty switch
        {
            Difficulty.Extreme => Difficulty.Hard,
            Difficulty.Hard    => Difficulty.Medium,
            Difficulty.Medium  => Difficulty.Easy,
            _                  => _difficulty
        };
        if (prev == _difficulty) return false;
        _difficulty = prev;
        return true;
    }

    /// <summary>Sets the difficulty to a specific tier immediately.</summary>
    public void SetDifficulty(Difficulty difficulty) => _difficulty = difficulty;

    // ── Movement — pace ───────────────────────────────────────────────────────

    /// <summary>
    /// Increases escalation speed by one step.
    /// Returns true when the pace changed.
    /// </summary>
    public bool SpeedUp()
    {
        var next = _pace switch
        {
            FlowPace.Slow    => FlowPace.Normal,
            FlowPace.Normal  => FlowPace.Fast,
            FlowPace.Fast    => FlowPace.Sprint,
            _                => _pace
        };
        if (next == _pace) return false;
        _pace = next;
        return true;
    }

    /// <summary>
    /// Reduces escalation speed by one step.
    /// Returns true when the pace changed.
    /// </summary>
    public bool SlowDown()
    {
        var prev = _pace switch
        {
            FlowPace.Sprint  => FlowPace.Fast,
            FlowPace.Fast    => FlowPace.Normal,
            FlowPace.Normal  => FlowPace.Slow,
            _                => _pace
        };
        if (prev == _pace) return false;
        _pace = prev;
        return true;
    }

    /// <summary>Sets pace directly.</summary>
    public void SetPace(FlowPace pace) => _pace = pace;

    // ── Cards at this position ─────────────────────────────────────────────────

    /// <summary>Number of cards played at the current difficulty before auto-escalation kicks in.</summary>
    public int CardsBeforeEscalation => _pace switch
    {
        FlowPace.Slow   => 8,
        FlowPace.Normal => 4,
        FlowPace.Fast   => 2,
        FlowPace.Sprint => 1,
        _               => 4
    };

    /// <summary>Total cards played at the current difficulty level this session.</summary>
    public int CardsPlayedAtCurrentLevel { get; private set; }

    /// <summary>
    /// Called by the strategy after each card at this level.
    /// Returns true when the auto-escalation threshold has been reached
    /// and the difficulty should advance.
    /// </summary>
    public bool RecordCardPlayed()
    {
        CardsPlayedAtCurrentLevel++;
        return CardsPlayedAtCurrentLevel >= CardsBeforeEscalation;
    }

    /// <summary>Resets the card counter when the difficulty level changes.</summary>
    public void ResetLevelCounter() => CardsPlayedAtCurrentLevel = 0;
}

/// <summary>How quickly difficulty automatically escalates.</summary>
public enum FlowPace
{
    /// <summary>Escalate after 8 cards (very gradual).</summary>
    Slow,

    /// <summary>Escalate after 4 cards (default).</summary>
    Normal,

    /// <summary>Escalate after 2 cards (quick).</summary>
    Fast,

    /// <summary>Escalate after every single card.</summary>
    Sprint,
}