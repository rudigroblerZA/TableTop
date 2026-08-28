using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Decks;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Progression;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Engine;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers.Services;
using TableTop.Hosting.Events;
using TableTop.Hosting.Hints;
using TableTop.Hosting.Persistence;

namespace TableTop.Hosting.Controllers;

/// <summary>
/// Drives a card-per-turn game.
///
/// New logic implemented here:
///   • Skip policy:  first skip per player is free; subsequent skips deduct <see cref="SkipPenalty"/> points.
///   • Reward chance: after every <see cref="RewardChanceInterval"/> regular cards a reward/break card
///                    is injected from a configurable pool.
///   • Inspiration:  <see cref="IInspirationCard"/> is detected, saved, and auto-advanced.
///   • Save/Resume:  <see cref="SaveAsync"/> persists a <see cref="TableTop.Hosting.Persistence.SessionSnapshot"/>; the controller
///                   accepts a snapshot on construction to restore mid-session state.
/// </summary>
public sealed class CardTurnController : ICardTurnController
{
    private readonly IGame _game;
    private readonly IDeck _deck;
    private readonly IReadOnlyList<IPlayer> _players;
    private readonly string _modeName;
    private readonly string? _modeFilePath;
    private readonly TableTop.Core.Abstractions.IEngineDiagnostics _diagnostics;

    // ── Thread-safety ─────────────────────────────────────────────────────────
    // CardTurnController is single-threaded. Ownership transfers to the caller
    // of Start(). All mutating public methods assert they are on the owner thread
    // in Debug builds. See ThreadingGuard.cs for the full policy.
    private readonly ThreadingGuard _threadGuard = new();

    // ── Extracted services ────────────────────────────────────────────────────
    private readonly SkipPolicy? _skipPolicy;
    private readonly EffectApplicator? _effectApplicator;
    private readonly TurnHistoryTracker? _historyTracker;
    private readonly SpecialCardCoordinator? _specialCards;
    private readonly FlowCoordinator? _flow;
    private readonly PersistenceCoordinator? _persistence;
    private readonly HintCoordinator? _hints;
    private readonly UndoCoordinator? _undo;

    /// <summary>Score penalty per skip after the first (default: -1).</summary>
    public int SkipPenalty { get; init; } = -1;

    /// <summary>After this many regular cards a bonus reward/break card is injected.</summary>
    public int RewardChanceInterval { get; init; }

    private readonly List<ICard> _bonusPool = [];

    private readonly Dictionary<Guid, List<SavedInspiration>> _playerInspirations = [];

    /// <summary>PlayerInspirations.</summary>
    public IReadOnlyDictionary<Guid, IReadOnlyList<SavedInspiration>> PlayerInspirations =>
        _playerInspirations.ToDictionary(
            kv => kv.Key,
            kv => (IReadOnlyList<SavedInspiration>)kv.Value.AsReadOnly());

    // ── Session timing & report ───────────────────────────────────────────────

    private DateTimeOffset _sessionStartedAt;

    /// <inheritdoc />
    public Core.Domain.Game.SessionReport? SessionReport { get; private set; }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>CardReady.</summary>
    public event EventHandler<CardReadyEvent>? CardReady;
    /// <summary>TurnResult.</summary>
    public event EventHandler<TurnResultEvent>? TurnResult;
    /// <summary>TurnSkipped.</summary>
    public event EventHandler<TurnSkippedEvent>? TurnSkipped;
    /// <summary>SkipAttempted.</summary>
    public event EventHandler<SkipAttemptedEvent>? SkipAttempted;
    /// <summary>GameEnded.</summary>
    public event EventHandler<GameEndedEvent>? GameEnded;
    /// <summary>GamePaused.</summary>
    public event EventHandler<GamePausedEvent>? GamePaused;
    /// <summary>BreakCardDrawn.</summary>
    public event EventHandler<BreakCardDrawnEvent>? BreakCardDrawn;
    /// <summary>RewardCardDrawn.</summary>
    public event EventHandler<RewardCardDrawnEvent>? RewardCardDrawn;
    /// <summary>InspirationCardDrawn.</summary>
    public event EventHandler<InspirationCardDrawnEvent>? InspirationCardDrawn;
    /// <summary>SessionSaved.</summary>
    public event EventHandler<SessionSavedEvent>? SessionSaved;
    /// <summary>FlowChanged.</summary>
    public event EventHandler<FlowChangedEvent>? FlowChanged;
    /// <summary>NextTurnHint.</summary>
    public event EventHandler<NextTurnHintEvent>? NextTurnHint;
    /// <summary>TurnUndone.</summary>
    public event EventHandler<TurnUndoneEvent>? TurnUndone;
    /// <summary>
    /// Raised by an external timer component when a per-card time limit expires.
    /// Subscribe from the UI layer (WPF, MAUI) to receive expiry notifications.
    /// </summary>
    public event EventHandler<TimerExpiredEvent>? TimerExpired;

    /// <summary>
    /// Raises <see cref="TimerExpired"/>. A host owning the countdown calls this
    /// when a card's time limit runs out.
    ///
    /// This exists because the event could not previously be raised at all. Its
    /// own documentation said "raised by an external timer component", but a C#
    /// event is invocable only from the type that declares it, and nothing here
    /// raised it — so the event was permanently dead, and a host that subscribed
    /// as the docs instructed waited forever. The suppression pragma sitting on
    /// it hid that.
    /// </summary>
    public void NotifyTimerExpired(IPlayer player, ICard card, TimeSpan allowed)
    {
        _threadGuard.Assert();
        TimerExpired?.Invoke(this, new TimerExpiredEvent(
            PlayerName: player.DisplayName,
            CardTitle: card.Title,
            Elapsed: allowed));
    }

    /// <inheritdoc />
    public IReadOnlyList<IPlayer> Players => _players;

    /// <inheritdoc />
    public bool IsRunning => _game.State == GameState.Active;

    // ── Construction ──────────────────────────────────────────────────────────

    /// <summary>
    /// Initialises a new <see cref="CardTurnController"/> instance.
    ///
    /// This builds the deck synchronously. <see cref="CreateAsync"/> is the
    /// preferred entry point and does not block — see backlog B.3.
    /// </summary>
    public CardTurnController(
        IGameModeDefinition definition,
        IReadOnlyList<IPlayer> players,
        string modeName,
        int maxRounds,
        Core.Abstractions.Progression.IProgressionStrategy progression,
        CardTurnControllerOptions? options = null)
        : this(
            SessionDeckFactory.Build(definition, players, modeName,
                (options ?? CardTurnControllerOptions.Default).Gameplay ?? GameplayOptions.Default),
            definition, players, modeName, maxRounds, progression,
            options ?? CardTurnControllerOptions.Default)
    {
    }

    /// <summary>
    /// The real constructor. Takes an already-built deck so <see cref="CreateAsync"/>
    /// can await construction rather than blocking on it, while the public
    /// constructor above keeps its synchronous signature.
    /// </summary>
    private CardTurnController(
        IDeck deck,
        IGameModeDefinition definition,
        IReadOnlyList<IPlayer> players,
        string modeName,
        int maxRounds,
        Core.Abstractions.Progression.IProgressionStrategy progression,
        CardTurnControllerOptions options)
    {
        _players = players;
        _modeName = modeName;
        _modeFilePath = options.ModeFilePath;
        var repository = options.SessionRepository ?? new JsonSessionRepository();
        RewardChanceInterval = options.RewardChanceInterval;
        SkipPenalty = options.SkipPenalty;
        _diagnostics = options.Diagnostics ?? Core.Abstractions.NullEngineDiagnostics.Instance;

        if (options.BonusPool is not null) _bonusPool.AddRange(options.BonusPool);

        // Initialise extracted services
        _skipPolicy = new SkipPolicy(options.SkipPenalty);
        _historyTracker = new TurnHistoryTracker();
        _skipPolicy.Initialise(players);
        _historyTracker.Initialise(players);

        foreach (var p in players)
            _playerInspirations[p.Id] = [];

        // The deck arrives already built — from SessionDeckFactory.Build on the
        // synchronous path, or SessionDeckFactory.BuildAsync on the async one.
        // Either way it is built exactly once, by the single source of truth,
        // so GameplayOptions cannot reach a caller that then builds its own
        // deck and throws this one away (see the factory's remarks).
        _deck = deck;

        var flowStrategy = progression as IFlowAwareProgressionStrategy;
        var hints = options.HintEngine ?? new DefaultHintEngine();

        _game = new GameBuilder()
            // Deck order pins them; this keeps the progression strategy
            // from reaching past everything else to pick one early.
            .WithDeferredCategories(definition.CategoriesPinnedToEnd)
            .WithDeck(_deck)
            .WithPlayers(players)
            .WithProgression(progression)
            .WithScoring(definition.GetScoring())
            .WithRules(definition.GetRules())
            .WithMaxRounds(maxRounds)
            .WithDiagnostics(_diagnostics)
            // Alternates turn order between teams for modes that opt in.
            // Detected from the definition rather than plumbed through
            // IGameConfiguration, which carries no reference to the mode.
            .WithTeamPlay(definition is Core.Abstractions.Game.ITeamMode)
            .Build();

        _effectApplicator = new EffectApplicator(_game.PlayerManager, _skipPolicy);

        _specialCards = new SpecialCardCoordinator(
            _effectApplicator,
            _playerInspirations,
            _bonusPool,
            RewardChanceInterval,
            buildScores: BuildScores,
            onBreakCard: e => BreakCardDrawn?.Invoke(this, e),
            onRewardCard: e => RewardCardDrawn?.Invoke(this, e),
            onInspirationCard: e => InspirationCardDrawn?.Invoke(this, e),
            onCardReady: e => CardReady?.Invoke(this, e));

        _flow = new FlowCoordinator(
            flowStrategy,
            players,
            onFlowChanged: e => FlowChanged?.Invoke(this, e),
            getRound: () => _game.Round);

        _persistence = new PersistenceCoordinator(
            _game,
            repository,
            players,
            _skipPolicy,
            flowStrategy,
            _playerInspirations,
            _modeName,
            _modeFilePath,
            onSessionSaved: e => SessionSaved?.Invoke(this, e));

        _hints = new HintCoordinator(
            hints,
            _historyTracker,
            _skipPolicy,
            flowStrategy,
            players,
            getRound: () => _game.Round,
            onHint: e => NextTurnHint?.Invoke(this, e));

        _undo = new UndoCoordinator(
            _game,
            _historyTracker,
            _diagnostics,
            buildScores: BuildScores,
            onTurnUndone: e => TurnUndone?.Invoke(this, e),
            onCardReady: e => CardReady?.Invoke(this, e));

        _game.TurnCompleted += OnTurnCompleted;
        _game.GameEnded += OnGameEnded;

        // Restore mid-session state if resuming
        if (options.ResumeFrom is not null)
            _persistence.Restore(options.ResumeFrom);
    }

    // ── Async factory (preferred entry point) ─────────────────────────────────

    /// <summary>
    /// Preferred construction path, and what <c>ControllerFactory</c> uses.
    ///
    /// Genuinely asynchronous as of backlog B.3: the deck is awaited via
    /// <see cref="SessionDeckFactory.BuildAsync"/> and handed to a private
    /// constructor, so nothing blocks. Previously this returned an already
    /// completed task wrapping a constructor that called
    /// <c>GetAwaiter().GetResult()</c> internally — harmless while
    /// <c>DeckBuilder</c> completed synchronously, and a deadlock waiting to
    /// happen on a UI thread the moment a card provider did real I/O.
    ///
    /// The public constructor remains available and still builds synchronously.
    /// </summary>
    public static async Task<CardTurnController> CreateAsync(
        IGameModeDefinition definition,
        IReadOnlyList<IPlayer> players,
        string modeName,
        int maxRounds,
        Core.Abstractions.Progression.IProgressionStrategy progression,
        CardTurnControllerOptions? options = null,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var settings = options ?? CardTurnControllerOptions.Default;
        var deck = await SessionDeckFactory
            .BuildAsync(definition, players, modeName, settings.Gameplay ?? GameplayOptions.Default, ct)
            .ConfigureAwait(false);

        return new CardTurnController(
            deck, definition, players, modeName, maxRounds, progression, settings);
    }

    // ── ICardTurnController ───────────────────────────────────────────────────

    /// <inheritdoc />
    public void Start()
    {
        _threadGuard.TransferOwnership();
        _game.Start();
        _sessionStartedAt = DateTimeOffset.UtcNow;
        _diagnostics.GameStarted(_modeName, _players.Count);
        AdvanceTurn();
    }

    /// <inheritdoc />
    public void RecordOutcome(CardOutcome outcome) =>
        RecordTimedOutcome(outcome, elapsed: TimeSpan.Zero);

    /// <inheritdoc />
    public void RecordTimedOutcome(CardOutcome outcome, TimeSpan elapsed)
    {
        _threadGuard.Assert();
        if (_game.State != GameState.Active) return;

        // Stash elapsed so OnTurnCompleted can pick it up
        _pendingElapsed = elapsed > TimeSpan.Zero ? elapsed : (TimeSpan?)null;

        if (outcome == CardOutcome.Skipped)
        {
            outcome = HandleSkipPolicy(out var skipEvent);
            SkipAttempted?.Invoke(this, skipEvent);
        }

        _game.RecordOutcome(outcome);
        _pendingElapsed = null;
    }

    // Temporary store for elapsed time between RecordTimedOutcome and OnTurnCompleted
    private TimeSpan? _pendingElapsed;

    /// <inheritdoc />
    public bool UndoLastTurn()
    {
        _threadGuard.Assert();
        return _undo!.Undo();
    }

    /// <inheritdoc />
    public void TogglePause()
    {
        _threadGuard.Assert();
        if (_game.State == GameState.Active)
        {
            _game.Pause();
            GamePaused?.Invoke(this, new GamePausedEvent(true));
        }
        else if (_game.State == GameState.Paused)
        {
            _game.Resume();
            GamePaused?.Invoke(this, new GamePausedEvent(false));
        }
    }

    /// <inheritdoc />
    public void Quit()
    {
        _threadGuard.Assert();
        if (_game.State is GameState.Active or GameState.Paused)
            _game.End();
    }

    // ── Flow control ──────────────────────────────────────────────────────────

    /// <inheritdoc />
    public bool SupportsFlow => _flow!.SupportsFlow;

    /// <inheritdoc />
    public int CardsRemaining => _deck.Count;

    /// <inheritdoc />
    public FlowState? GetFlowState(Guid playerId) => _flow!.GetFlowState(playerId);

    /// <inheritdoc />
    public void LevelUp(Guid playerId) { _threadGuard.Assert(); _flow!.LevelUp(playerId); }
    /// <inheritdoc />
    public void LevelDown(Guid playerId) { _threadGuard.Assert(); _flow!.LevelDown(playerId); }
    /// <inheritdoc />
    public void SpeedUp(Guid playerId) { _threadGuard.Assert(); _flow!.SpeedUp(playerId); }
    /// <inheritdoc />
    public void SlowDown(Guid playerId) { _threadGuard.Assert(); _flow!.SlowDown(playerId); }

    /// <inheritdoc />
    public void JumpTo(Guid playerId, Core.Abstractions.Cards.Difficulty difficulty)
    {
        _threadGuard.Assert();
        _flow!.JumpTo(playerId, difficulty);
    }

    /// <inheritdoc />
    public void ResetFlow(Guid playerId)
    {
        _threadGuard.Assert();
        _flow!.ResetFlow(playerId);
    }

    /// <inheritdoc />
    public Task SaveAsync(CancellationToken ct = default) =>
        _persistence!.SaveAsync(ct);

    /// <inheritdoc />
    public void ApplySteal(Guid fromPlayerId, Guid toPlayerId, int points)
    {
        _threadGuard.Assert();
        _game.PlayerManager.ApplyScore(fromPlayerId, -points);
        _game.PlayerManager.ApplyScore(toPlayerId, +points);
    }

    // ── Skip policy ───────────────────────────────────────────────────────────

    private CardOutcome HandleSkipPolicy(out SkipAttemptedEvent evt)
    {
        var player = _game.CurrentPlayer;
        if (player is null)
        {
            // No active player — treat as no-op skip
            evt = new SkipAttemptedEvent("?", IsFree: true, SkipCount: 0, Penalty: 0,
                Round: _game.Round, CurrentScores: BuildScores());
            return CardOutcome.Skipped;
        }

        evt = _skipPolicy!.ProcessSkip(player, _game.Round, BuildScores());

        if (!evt.IsFree && evt.Penalty != 0)
            _game.PlayerManager.ApplyScore(player.Id, evt.Penalty);

        return CardOutcome.Skipped;
    }

    // ── Turn advancement ──────────────────────────────────────────────────────

    private void AdvanceTurn()
    {
        // NOTE: this is deliberately a LOOP, not recursion.
        //
        // When the engine can't serve the current player an eligible card it
        // returns null WITHOUT consuming the deck, and we move on to the next
        // player. If no player can legally play any remaining card — which
        // happens with heavily restricted decks (couples-only cards with a
        // roster that doesn't satisfy them, or a narrow difficulty filter) —
        // the deck never empties, so the old recursive retry never terminated
        // and blew the stack. Looping removes the stack growth; the skip
        // budget below removes the infinite spin.
        //
        // Budget: one full rotation of the roster. If every player has been
        // offered the remaining deck and none of them can play any of it, the
        // deck is unplayable for this table and ending is the honest outcome —
        // GameEnded fires normally so UIs show the results screen rather than
        // hanging or crashing.
        var skipBudget = Math.Max(1, _players.Count);
        var consecutiveSkips = 0;

        while (true)
        {
            if (_game.State != GameState.Active) return;

            // Bonus injection — delegated to SpecialCardCoordinator
            if (_specialCards!.TryInjectBonus(
                    advanceTurn: () => _game.AdvanceTurn(),
                    currentPlayer: () => _game.CurrentPlayer,
                    round: _game.Round))
                return;

            var card = _game.AdvanceTurn();

            if (card is null)
            {
                if (_deck.IsEmpty)
                {
                    _game.End();
                    return;
                }

                var noCardPlayer = _game.CurrentPlayer;
                if (noCardPlayer is not null)
                    _diagnostics.NoCardAvailable(noCardPlayer, _deck.Count, _game.Round);

                TurnSkipped?.Invoke(this, new TurnSkippedEvent(
                    _game.CurrentPlayer?.DisplayName ?? "?",
                    "No eligible card available",
                    _game.Round));

                if (++consecutiveSkips >= skipBudget)
                {
                    // A full rotation with nobody able to play: stop cleanly.
                    _game.End();
                    return;
                }

                continue;   // try the next player
            }

            EmitCard(card);
            return;
        }
    }

    /// <summary>
    /// Emits a drawn card — handling the special card types, then raising
    /// <see cref="CardReady"/> for a regular one. Split out of
    /// <see cref="AdvanceTurn"/> so that method stays a clean retry loop.
    /// </summary>
    private void EmitCard(ICard card)
    {
        var player = _game.CurrentPlayer!;
        _diagnostics.CardSelected(card, player, _game.Round);

        // ── Special card types — delegated to SpecialCardCoordinator ──────────

        if (_specialCards!.TryHandleSpecialCard(card, player, _game.Round))
        {
            _game.RecordOutcome(CardOutcome.Completed);
            return;
        }

        // ── Regular card ───────────────────────────────────────────────────────

        _specialCards!.IncrementRegularCard();

        var text = card is IPromptCard prompt
            ? prompt.ResolvePrompt(player)
            : card.Description;

        CardReady?.Invoke(this, new CardReadyEvent(
            Player: player,
            PlayerName: player.DisplayName,
            Card: card,
            CardTitle: card.Title,
            CardText: text,
            Category: card.Category,
            Difficulty: card.Difficulty.ToString(),
            Restriction: card.Restriction?.Description,
            Round: _game.Round));
    }

    // ── Engine event forwarding ───────────────────────────────────────────────

    private void OnTurnCompleted(object? sender, TurnCompletedEventArgs e)
    {
        // Record the full turn in history for stats, undo, and hint generation
        var lastCard = _game.PlayedCards.LastOrDefault();
        if (lastCard is not null)
        {
            var scoreAfter = _game.PlayerManager is Core.Domain.Players.RoundRobinPlayerManager rrm
                ? rrm.GetScore(e.Player.Id)
                : e.Player.Score;

            _historyTracker!.Record(
                player: e.Player,
                card: lastCard,
                outcome: e.Outcome,
                scoreDelta: e.ScoreDelta,
                scoreAfter: scoreAfter,
                round: e.Round,
                elapsed: _pendingElapsed);

            _diagnostics.TurnRecorded(e.Player, lastCard, e.Outcome, e.ScoreDelta, e.Round);
        }

        TurnResult?.Invoke(this, new TurnResultEvent(
            PlayerName: e.Player.DisplayName,
            Outcome: e.Outcome,
            ScoreDelta: e.ScoreDelta,
            Round: e.Round,
            CurrentScores: BuildScores()));

        // Generate and emit hint
        _hints!.Emit(e.Player);

        // Check for extra-card effect
        if (_effectApplicator!.ConsumeExtraCard(e.Player.Id))
        {
            if (_game.State == GameState.Active) AdvanceTurn();
            return;
        }

        if (_game.State == GameState.Active)
            AdvanceTurn();
    }

    private void OnGameEnded(object? sender, GameEndedEventArgs e)
    {
        // Build the session report from the full turn history
        var duration = DateTimeOffset.UtcNow - _sessionStartedAt;
        var report = Core.Domain.Game.SessionReport.Build(
            turns: _historyTracker!.AllTurns,
            players: _players,
            totalRounds: e.TotalRounds,
            duration: duration);

        SessionReport = report;

        _diagnostics.GameEnded(_modeName, e.TotalRounds, report.TotalTurns);

        GameEnded?.Invoke(this, new GameEndedEvent(
            FinalStandings: e.FinalStandings.Select(p =>
                new ScoreEntry(p.DisplayName, p.Score)).ToList().AsReadOnly(),
            TotalRounds: e.TotalRounds,
            Report: report));

        // Clean up saved session on natural game end
        _ = _persistence!.DeleteAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IReadOnlyList<ScoreEntry> BuildScores() =>
        _game.PlayerManager.Players
             .OrderByDescending(p => p.Score)
             .Select(p => new ScoreEntry(p.DisplayName, p.Score))
             .ToList().AsReadOnly();

    // ── IDisposable ───────────────────────────────────────────────────────────

    private bool _disposed;

    /// <summary>
    /// Unsubscribes from internal <see cref="IGame"/> events and nulls out all
    /// controller-level event fields so subscribers can be collected.
    ///
    /// Call this when a game session ends — either naturally (after
    /// <see cref="GameEndedEvent"/> fires) or via <see cref="Quit"/>.
    /// The controller is unusable after disposal.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Unsubscribe from game events — breaks the reference the Game holds to this controller
        _game.TurnCompleted -= OnTurnCompleted;
        _game.GameEnded -= OnGameEnded;

        // Null out all public event fields so any remaining UI subscribers
        // can be collected without waiting for the controller to be GC'd
        CardReady = null;
        TurnResult = null;
        TurnSkipped = null;
        SkipAttempted = null;
        GameEnded = null;
        GamePaused = null;
        BreakCardDrawn = null;
        RewardCardDrawn = null;
        InspirationCardDrawn = null;
        SessionSaved = null;
        FlowChanged = null;
        NextTurnHint = null;
        TurnUndone = null;
        TimerExpired = null;
    }
}