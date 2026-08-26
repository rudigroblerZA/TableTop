using TableTop.Core.Abstractions.Scoring;
using TableTop.Hosting.Events;
using TableTop.Hosting.Persistence;

namespace TableTop.Hosting.Abstractions;

/// <summary>
/// Controls a card-per-turn game session.
/// All state changes are announced via typed events; the UI reacts to those.
/// </summary>
public interface ICardTurnController : IGameController
{
    // ── Events ────────────────────────────────────────────────────────────────
    /// <summary>CardReady.</summary>
    event EventHandler<CardReadyEvent> CardReady;
    /// <summary>TurnResult.</summary>
    event EventHandler<TurnResultEvent> TurnResult;
    /// <summary>TurnSkipped.</summary>
    event EventHandler<TurnSkippedEvent> TurnSkipped;
    /// <summary>SkipAttempted.</summary>
    event EventHandler<SkipAttemptedEvent> SkipAttempted;
    /// <summary>GameEnded.</summary>
    event EventHandler<GameEndedEvent> GameEnded;
    /// <summary>GamePaused.</summary>
    event EventHandler<GamePausedEvent> GamePaused;
    /// <summary>BreakCardDrawn.</summary>
    event EventHandler<BreakCardDrawnEvent> BreakCardDrawn;
    /// <summary>RewardCardDrawn.</summary>
    event EventHandler<RewardCardDrawnEvent> RewardCardDrawn;
    /// <summary>InspirationCardDrawn.</summary>
    event EventHandler<InspirationCardDrawnEvent> InspirationCardDrawn;
    /// <summary>SessionSaved.</summary>
    event EventHandler<SessionSavedEvent> SessionSaved;
    /// <summary>FlowChanged.</summary>
    event EventHandler<FlowChangedEvent> FlowChanged;
    /// <summary>NextTurnHint.</summary>
    event EventHandler<NextTurnHintEvent> NextTurnHint;

    /// <summary>Raised when UndoLastTurn() successfully reverses the previous turn.</summary>
    event EventHandler<TurnUndoneEvent> TurnUndone;

    /// <summary>Raised when the engine-side card timer expires (if running).</summary>
    event EventHandler<TimerExpiredEvent> TimerExpired;

    // ── Commands ──────────────────────────────────────────────────────────────

    /// <summary>Starts the game and draws the first card.</summary>
    void Start();

    /// <summary>
    /// Records the outcome for the current card and advances to the next turn.
    /// If the outcome is <see cref="CardOutcome.Skipped"/> the controller applies
    /// the free-skip / penalty logic automatically.
    /// </summary>
    void RecordOutcome(CardOutcome outcome);

    /// <summary>Toggles pause/resume.</summary>
    void TogglePause();

    /// <summary>Ends the game immediately.</summary>
    void Quit();

    /// <summary>
    /// Saves the current session state to disk.
    /// Raises <see cref="SessionSaved"/> on success.
    /// </summary>
    Task SaveAsync(CancellationToken ct = default);

    /// <summary>
    /// Applies a steal-points effect: removes <paramref name="points"/> from
    /// <paramref name="toPlayerId"/> and adds them to the current reward recipient.
    /// Called by the host after the player has chosen their target.
    /// </summary>
    void ApplySteal(Guid fromPlayerId, Guid toPlayerId, int points);

    /// <summary>
    /// Records the outcome for the current card with elapsed time.
    /// Passes the elapsed time to the scoring strategy (enables time-based scoring)
    /// and stores it in the turn record for post-game stats.
    /// </summary>
    void RecordTimedOutcome(Core.Abstractions.Scoring.CardOutcome outcome, TimeSpan elapsed);

    /// <summary>
    /// Reverses the most recently completed turn. Restores the player's score,
    /// removes the card from played-card history (so it can be redrawn),
    /// and re-presents the same card to the same player.
    /// Returns false when there is nothing to undo.
    /// </summary>
    bool UndoLastTurn();

    /// <summary>
    /// Post-game statistics report. Populated after the game ends (GameEndedEvent fires).
    /// Null during play.
    /// </summary>
    TableTop.Core.Domain.Game.SessionReport? SessionReport { get; }

    /// <summary>Inspiration category filter for saving — player ID to list of inspirations.</summary>
    IReadOnlyDictionary<Guid, IReadOnlyList<SavedInspiration>> PlayerInspirations { get; }

    // ── Flow control ──────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true when the current progression strategy supports free-flow navigation.
    /// When false, the Level/Pace commands below are no-ops.
    /// </summary>
    bool SupportsFlow { get; }

    /// <summary>Number of cards left in the session deck. Decreases as cards are drawn.</summary>
    int CardsRemaining { get; }

    /// <summary>
    /// Everyone at the table, in seating order.
    ///
    /// The flow controls below take a player id, and until this existed a host
    /// had no way to get one except by accumulating them from
    /// <c>CardReady</c> as turns went by — so "level up everyone" only reached
    /// the players who had already had a turn. The controller has known the
    /// roster all along.
    /// </summary>
    IReadOnlyList<Core.Abstractions.Players.IPlayer> Players { get; }

    /// <summary>Move the specified player one difficulty level harder.</summary>
    void LevelUp(Guid playerId);

    /// <summary>Move the specified player one difficulty level easier.</summary>
    void LevelDown(Guid playerId);

    /// <summary>Jump the specified player directly to a difficulty tier.</summary>
    void JumpTo(Guid playerId, Core.Abstractions.Cards.Difficulty difficulty);

    /// <summary>Increase the auto-escalation pace for the specified player.</summary>
    void SpeedUp(Guid playerId);

    /// <summary>Decrease the auto-escalation pace for the specified player.</summary>
    void SlowDown(Guid playerId);

    /// <summary>Reset the specified player's flow state to initial position.</summary>
    void ResetFlow(Guid playerId);

    /// <summary>
    /// Returns the current flow state for the specified player,
    /// or null when <see cref="SupportsFlow"/> is false.
    /// </summary>
    Core.Abstractions.Progression.FlowState? GetFlowState(Guid playerId);
}