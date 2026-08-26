using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Game;

namespace TableTop.Hosting.Events;

// ── Turn lifecycle ─────────────────────────────────────────────────────────────

/// <summary>Raised when a new card has been selected and is ready for the current player to respond to.</summary>
public sealed record CardReadyEvent(
    IPlayer Player,
    string PlayerName,
    ICard Card,
    string CardTitle,
    string CardText,
    string Category,
    string Difficulty,
    string? Restriction,
    int Round);

/// <summary>Raised when a turn outcome has been recorded and scored.</summary>
public sealed record TurnResultEvent(
    string PlayerName,
    TableTop.Core.Abstractions.Scoring.CardOutcome Outcome,
    int ScoreDelta,
    int Round,
    IReadOnlyList<ScoreEntry> CurrentScores);

/// <summary>Raised when a turn is skipped automatically (no eligible card found for the current player).</summary>
public sealed record TurnSkippedEvent(
    string PlayerName,
    string Reason,
    int Round);

/// <summary>Raised when the game session ends, either naturally after all rounds or via <c>Quit()</c>.</summary>
/// <param name="FinalStandings">Players ranked by score, descending.</param>
/// <param name="TotalRounds">Total rounds played in this session.</param>
/// <param name="Report">Post-game statistics. Always present; never null.</param>
public sealed record GameEndedEvent(
    IReadOnlyList<ScoreEntry> FinalStandings,
    int TotalRounds,
    SessionReport Report);

/// <summary>Raised when the game is paused or resumed. <c>IsPaused = true</c> means the game is now paused.</summary>
public sealed record GamePausedEvent(bool IsPaused);

// ── Undo ───────────────────────────────────────────────────────────────────────

/// <summary>
/// Raised when UndoLastTurn() successfully reverses the most recent turn.
/// </summary>
public sealed record TurnUndoneEvent(
    string PlayerName,
    string CardTitle,
    TableTop.Core.Abstractions.Scoring.CardOutcome ReversedOutcome,
    int ScoreRestored,
    IReadOnlyList<ScoreEntry> CurrentScores);

// ── Timer ──────────────────────────────────────────────────────────────────────

/// <summary>
/// Raised by the engine-side card timer when the countdown reaches zero.
/// </summary>
public sealed record TimerExpiredEvent(
    string PlayerName,
    string CardTitle,
    TimeSpan Elapsed);

// ── Special cards ──────────────────────────────────────────────────────────────

/// <summary>Raised when a break card is drawn. The host should present the activity and call <c>RecordOutcome</c> when done.</summary>
public sealed record BreakCardDrawnEvent(
    string PlayerName,
    string CardTitle,
    string CardText,
    string Activity,
    int? DurationMinutes,
    string Scope,
    string EffectType,
    int Round);

/// <summary>Raised when a reward card is drawn and its effect has been applied.</summary>
public sealed record RewardCardDrawnEvent(
    string PlayerName,
    string CardTitle,
    string CardText,
    string EffectType,
    string EffectDescription,
    int ScoreDelta,
    int Round,
    IReadOnlyList<ScoreEntry> CurrentScores);

/// <summary>Raised when a player attempts to skip. Contains whether the skip was free or penalised.</summary>
public sealed record SkipAttemptedEvent(
    string PlayerName,
    bool IsFree,
    int Penalty,
    int SkipCount,
    int Round,
    IReadOnlyList<ScoreEntry> CurrentScores);

/// <summary>Raised when an inspiration card is drawn and stored in the player's inspiration list.</summary>
public sealed record InspirationCardDrawnEvent(
    string PlayerName,
    string CardTitle,
    string InspirationText,
    string InspirationCategory,
    int Round);

/// <summary>Raised after <c>SaveAsync()</c> writes a session snapshot to disk.</summary>
public sealed record SessionSavedEvent(string FilePath, DateTimeOffset SavedAt);

/// <summary>Raised after a flow-control command (LevelUp, LevelDown, SpeedUp, SlowDown, JumpTo, Reset) changes a player's flow state.</summary>
public sealed record FlowChangedEvent(
    string PlayerName,
    string Change,
    string NewDifficulty,
    string NewPace,
    int CardsBeforeEscalation,
    int Round);

/// <summary>Raised when the hint engine has a suggestion for the next card selection.</summary>
public sealed record NextTurnHintEvent(
    string PlayerName,
    string HintText,
    string SuggestedDifficulty,
    string? SuggestedPaceChange,
    string Urgency,
    string Reason);

// ── Score ──────────────────────────────────────────────────────────────────────

/// <summary>A player's name and current score, used in standings and score snapshots within events.</summary>
public sealed record ScoreEntry(string Name, int Score);
