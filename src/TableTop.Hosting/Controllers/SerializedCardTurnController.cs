using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Progression;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Game;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Events;
using TableTop.Hosting.Persistence;

namespace TableTop.Hosting.Controllers;

/// <summary>
/// Adapter that enforces <see cref="CardTurnController"/>'s single-threaded contract
/// by construction rather than by assertion (backlog: critical finding on
/// <see cref="ThreadingGuard"/>'s Release-mode default).
///
/// <para>
/// <strong>The gap this closes.</strong> <see cref="ThreadingGuard"/> only detects
/// cross-thread misuse — it throws <see cref="InvalidOperationException"/> when
/// <see cref="ThreadingGuard.Enabled"/> is true, which is the default in Debug but
/// not in Release (see <c>ThreadingGuard.cs</c>). CI builds and tests run in Release,
/// and releases ship in Release, so a caller that never marshals onto the owner
/// thread — a raw <see cref="System.Threading.Timer"/> callback, a background
/// service, two callers sharing a session — hits silent state corruption in exactly
/// the build that ships, with nothing catching it.
/// </para>
///
/// <para>
/// <strong>What this does instead.</strong> Every member forwards to the wrapped
/// controller inside a <c>lock</c> (<see cref="System.Threading.Monitor"/>, which
/// .NET already implements as recursive per-thread). A call from the thread that
/// already holds the lock — the normal case, and specifically the case of the
/// controller's own event handlers calling back into it, which is how
/// <c>AdvanceTurn</c> re-enters itself — proceeds immediately, at the same cost and
/// on the same thread as calling the unwrapped controller directly. A correctly
/// single-threaded host (Console, MAUI, WinUI today — each calls every member from
/// the one thread that called <c>Start()</c>) sees no behavioural change: the same
/// thread runs every call and receives every event, exactly as before. A call from
/// any other thread blocks until the lock is free and then runs — so two threads
/// can never execute inside the controller at once, which is the actual guarantee
/// <see cref="ThreadingGuard"/> alone cannot give when disabled.
/// </para>
///
/// <para>
/// <strong>Known limitation, inherited by any synchronous cross-thread adapter.</strong>
/// The lock is held for the full duration of a call, including any event it raises
/// synchronously. If a host's event handler blocks waiting on another thread, and
/// that thread calls back into this same instance, the two threads deadlock — the
/// same risk <see cref="ThreadingGuard"/>'s own docstring gives as the reason it does
/// not add a lock internally. No event handler in this codebase does that today
/// (they either return immediately, or — for the MAUI/WinUI countdown — resume via
/// the UI thread's own <c>SynchronizationContext</c> rather than a blocking
/// cross-thread call). A host that introduces one takes on this risk knowingly
/// rather than by silent corruption, which is the trade this type makes.
/// </para>
///
/// <para>
/// <strong>Interaction with <see cref="ThreadingGuard.Enabled"/>.</strong> The
/// wrapped controller's own guard tracks the thread that first called <c>Start()</c>
/// as its owner. Because this adapter runs a foreign-thread call on the calling
/// thread itself (serialised by the lock, not marshalled onto the owner thread), a
/// build with <c>ThreadingGuard.Enabled = true</c> will still see — and throw on —
/// that thread mismatch, even though access was correctly serialised. Diagnostics
/// and this adapter are two different strategies for the same problem; leave
/// <see cref="ThreadingGuard.Enabled"/> off when wrapping a controller in this
/// adapter for genuine cross-thread traffic.
/// </para>
/// </summary>
public sealed class SerializedCardTurnController : ICardTurnController
{
    private readonly ICardTurnController _inner;
    private readonly object _gate = new();

    /// <summary>Wraps <paramref name="inner"/> so every member call is serialised.</summary>
    public SerializedCardTurnController(ICardTurnController inner) =>
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    private void Invoke(Action action) { lock (_gate) action(); }

    private T Invoke<T>(Func<T> func) { lock (_gate) return func(); }

    // ── Events ────────────────────────────────────────────────────────────────
    // Subscription itself is already thread-safe (compiler-generated add/remove
    // on the wrapped controller's field-like events), so these forward directly
    // without taking the gate.

    /// <inheritdoc />
    public event EventHandler<CardReadyEvent>? CardReady
    {
        add => _inner.CardReady += value;
        remove => _inner.CardReady -= value;
    }

    /// <inheritdoc />
    public event EventHandler<TurnResultEvent>? TurnResult
    {
        add => _inner.TurnResult += value;
        remove => _inner.TurnResult -= value;
    }

    /// <inheritdoc />
    public event EventHandler<TurnSkippedEvent>? TurnSkipped
    {
        add => _inner.TurnSkipped += value;
        remove => _inner.TurnSkipped -= value;
    }

    /// <inheritdoc />
    public event EventHandler<SkipAttemptedEvent>? SkipAttempted
    {
        add => _inner.SkipAttempted += value;
        remove => _inner.SkipAttempted -= value;
    }

    /// <inheritdoc />
    public event EventHandler<GameEndedEvent>? GameEnded
    {
        add => _inner.GameEnded += value;
        remove => _inner.GameEnded -= value;
    }

    /// <inheritdoc />
    public event EventHandler<GamePausedEvent>? GamePaused
    {
        add => _inner.GamePaused += value;
        remove => _inner.GamePaused -= value;
    }

    /// <inheritdoc />
    public event EventHandler<BreakCardDrawnEvent>? BreakCardDrawn
    {
        add => _inner.BreakCardDrawn += value;
        remove => _inner.BreakCardDrawn -= value;
    }

    /// <inheritdoc />
    public event EventHandler<RewardCardDrawnEvent>? RewardCardDrawn
    {
        add => _inner.RewardCardDrawn += value;
        remove => _inner.RewardCardDrawn -= value;
    }

    /// <inheritdoc />
    public event EventHandler<InspirationCardDrawnEvent>? InspirationCardDrawn
    {
        add => _inner.InspirationCardDrawn += value;
        remove => _inner.InspirationCardDrawn -= value;
    }

    /// <inheritdoc />
    public event EventHandler<SessionSavedEvent>? SessionSaved
    {
        add => _inner.SessionSaved += value;
        remove => _inner.SessionSaved -= value;
    }

    /// <inheritdoc />
    public event EventHandler<FlowChangedEvent>? FlowChanged
    {
        add => _inner.FlowChanged += value;
        remove => _inner.FlowChanged -= value;
    }

    /// <inheritdoc />
    public event EventHandler<NextTurnHintEvent>? NextTurnHint
    {
        add => _inner.NextTurnHint += value;
        remove => _inner.NextTurnHint -= value;
    }

    /// <inheritdoc />
    public event EventHandler<TurnUndoneEvent>? TurnUndone
    {
        add => _inner.TurnUndone += value;
        remove => _inner.TurnUndone -= value;
    }

    /// <inheritdoc />
    public event EventHandler<TimerExpiredEvent>? TimerExpired
    {
        add => _inner.TimerExpired += value;
        remove => _inner.TimerExpired -= value;
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public void Start() => Invoke(_inner.Start);

    /// <inheritdoc />
    public void RecordOutcome(CardOutcome outcome) => Invoke(() => _inner.RecordOutcome(outcome));

    /// <inheritdoc />
    public void RecordTimedOutcome(CardOutcome outcome, TimeSpan elapsed) =>
        Invoke(() => _inner.RecordTimedOutcome(outcome, elapsed));

    /// <inheritdoc />
    public void TogglePause() => Invoke(_inner.TogglePause);

    /// <inheritdoc />
    public void Quit() => Invoke(_inner.Quit);

    /// <inheritdoc />
    public Task SaveAsync(CancellationToken ct = default) => Invoke(() => _inner.SaveAsync(ct));

    /// <inheritdoc />
    public void ApplySteal(Guid fromPlayerId, Guid toPlayerId, int points) =>
        Invoke(() => _inner.ApplySteal(fromPlayerId, toPlayerId, points));

    /// <inheritdoc />
    public bool UndoLastTurn() => Invoke(_inner.UndoLastTurn);

    /// <inheritdoc />
    public SessionReport? SessionReport => Invoke(() => _inner.SessionReport);

    /// <inheritdoc />
    public IReadOnlyDictionary<Guid, IReadOnlyList<SavedInspiration>> PlayerInspirations =>
        Invoke(() => _inner.PlayerInspirations);

    // ── Flow control ──────────────────────────────────────────────────────────

    /// <inheritdoc />
    public bool SupportsFlow => Invoke(() => _inner.SupportsFlow);

    /// <inheritdoc />
    public int CardsRemaining => Invoke(() => _inner.CardsRemaining);

    /// <inheritdoc />
    public IReadOnlyList<IPlayer> Players => Invoke(() => _inner.Players);

    /// <inheritdoc />
    public void LevelUp(Guid playerId) => Invoke(() => _inner.LevelUp(playerId));

    /// <inheritdoc />
    public void LevelDown(Guid playerId) => Invoke(() => _inner.LevelDown(playerId));

    /// <inheritdoc />
    public void JumpTo(Guid playerId, Difficulty difficulty) => Invoke(() => _inner.JumpTo(playerId, difficulty));

    /// <inheritdoc />
    public void SpeedUp(Guid playerId) => Invoke(() => _inner.SpeedUp(playerId));

    /// <inheritdoc />
    public void SlowDown(Guid playerId) => Invoke(() => _inner.SlowDown(playerId));

    /// <inheritdoc />
    public void ResetFlow(Guid playerId) => Invoke(() => _inner.ResetFlow(playerId));

    /// <inheritdoc />
    public FlowState? GetFlowState(Guid playerId) => Invoke(() => _inner.GetFlowState(playerId));

    // ── IGameController ───────────────────────────────────────────────────────

    /// <inheritdoc />
    public bool IsRunning => Invoke(() => _inner.IsRunning);

    /// <inheritdoc />
    public void Dispose() => Invoke(_inner.Dispose);
}
