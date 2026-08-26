using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Progression;
using TableTop.Core.Abstractions.Rules;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Players;
using TableTop.Core.Domain.Progression;
using TableTop.Core.Domain.Rules;

namespace TableTop.Core.Engine;

/// <summary>
/// Orchestrates a complete game session: player turns, card selection, rule evaluation, and scoring.
///
/// Engine invariants (see doc/engine-invariants.md):
///
///   DECK MUTATION   — <see cref="AdvanceTurn"/> calls <see cref="Abstractions.Decks.IDeck.DrawById"/>
///                     exactly once per turn, only after a candidate has been validated.
///                     Progression strategies use <see cref="Abstractions.Decks.IDeck.Peek"/> — non-mutating.
///
///   SCORING         — FinalScore = ScoringStrategy.CalculateScore + RuleEvaluator.ScoreDelta.
///                     Both contributions are summed in <see cref="RecordOutcome"/> and applied atomically.
///
///   ROUND COUNTING  — Active player count is snapshotted at the start of each round (stored in
///                     <see cref="GameMetadata.ActivePlayersAtRoundStart"/>).
///                     Rounds advance when <c>_turnsThisRound == snapshotCount</c>, immune to
///                     mid-round status changes.
///
///   MAXROUNDS       — MaxRounds means "completed playable rounds". The game ends after the round
///                     counter reaches MaxRounds and the last turn of that round is recorded.
///                     End condition: Round == MaxRounds after a completed round.
///
///   SPECIAL CARDS   — Break/reward/inspiration cards auto-complete via <see cref="RecordOutcome"/>.
///                     They receive zero scoring by default (<see cref="SpecialCardScoringPolicy.NoScore"/>)
///                     unless the mode definition overrides this.
/// </summary>
public sealed class Game : IGame
{
    private readonly IGameConfiguration _config;
    private readonly IPlayerManager     _playerManager;
    private readonly IRuleEvaluator     _ruleEvaluator;
    private readonly List<ICard>        _playedCards = [];
    private readonly GameMetadata       _metadata    = new();

    /// <summary>
    /// Exposes the session metadata for seeding (e.g. played-card history when
    /// resuming from a a session snapshot).
    /// </summary>
    public GameMetadata Metadata => _metadata;

    private IPlayer? _currentPlayer;
    private ICard?   _currentCard;

    /// <inheritdoc />
    public ICard? CurrentCard => _currentCard;
    private int      _turnsThisRound;        // Issue 3: explicit per-round turn counter
    private int      _activePlayerSnapshot;  // Issue 3: snapshotted at round start

    /// <summary>Initialises a new <see cref="Game"/> instance.</summary>
    public Game(
        IGameConfiguration config,
        IPlayerManager     playerManager,
        IRuleEvaluator     ruleEvaluator)
    {
        _config        = config        ?? throw new ArgumentNullException(nameof(config));
        _playerManager = playerManager ?? throw new ArgumentNullException(nameof(playerManager));
        _ruleEvaluator = ruleEvaluator ?? throw new ArgumentNullException(nameof(ruleEvaluator));

        Id    = Guid.NewGuid();
        State = GameState.Pending;
        Round = 0;

        foreach (var player in config.Players)
            _playerManager.AddPlayer(player);
    }

    /// <inheritdoc />
    public Guid      Id            { get; }
    /// <inheritdoc />
    public GameState State         { get; private set; }
    /// <inheritdoc />
    public int       Round         { get; private set; }
    /// <inheritdoc />
    public IPlayer?  CurrentPlayer => _currentPlayer;

    /// <summary>PlayedCards.</summary>
    public IReadOnlyList<ICard> PlayedCards => _playedCards.AsReadOnly();

    /// <summary>PlayerManager.</summary>
    public IPlayerManager PlayerManager => _playerManager;

    /// <summary>TurnCompleted.</summary>
    public event EventHandler<TurnCompletedEventArgs>? TurnCompleted;
    /// <summary>GameEnded.</summary>
    public event EventHandler<GameEndedEventArgs>?     GameEnded;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public void Start()
    {
        EnsureState(GameState.Pending);
        State = GameState.Active;
        Round = 1;
        SnapshotActivePlayerCount();  // Issue 3
    }

    // ── Issue 2: non-mutating selection + single definitive draw ─────────────

    /// <inheritdoc />
    public ICard? AdvanceTurn()
    {
        EnsureState(GameState.Active);

        _currentPlayer = _playerManager.GetNextPlayer();
        if (_currentPlayer is null) return null;

        var progressionContext = BuildProgressionContext();
        var ruleContext        = BuildRuleContext();

        // Phase 1: ask the strategy for a candidate ID — deck is NOT mutated
        ICard? selected      = null;
        var    deckSize      = _config.Deck.Count;
        var    attempts      = 0;
        var    skipIds       = new HashSet<Guid>(); // cards found ineligible this pass

        // Hold back deferred categories while anything else is still playable.
        //
        // Deck order alone can't keep a card last: the progression strategy
        // peeks across the WHOLE deck and may pick, say, an easy card from the
        // end long before the deck runs down. Seeding skipIds with the deferred
        // cards takes them out of contention entirely — until they're all
        // that's left, at which point they're released and play out in order.
        if (_config.DeferredCategories.Count > 0)
        {
            var deferred = _config.Deck.Filter(c =>
                c.Category is not null &&
                _config.DeferredCategories.Contains(c.Category, StringComparer.OrdinalIgnoreCase));

            // Hold them back only while something else is actually PLAYABLE.
            //
            // Checking "are these all that remain?" isn't enough: if every
            // other card is ineligible for this player — a couples-gated deck
            // with nobody eligible, say — the deferred cards would stay held
            // back forever and the game would end without ever dealing them.
            // A results key that never appears is worse than one that appears
            // slightly early.
            var othersPlayable = _config.Deck
                .Filter(c => !deferred.Any(d => d.Id == c.Id))
                .Any(c => _ruleEvaluator
                    .Evaluate(c, _currentPlayer, ruleContext).IsAllowed);

            if (othersPlayable)
                foreach (var card in deferred)
                    skipIds.Add(card.Id);
        }

        while (attempts++ < deckSize && !_config.Deck.IsEmpty)
        {
            // Peek, skipping previously rejected candidates this pass
            var candidateId = _config.ProgressionStrategy
                .SelectCandidate(_currentPlayer, _config.Deck, progressionContext);

            // If strategy is stuck returning a rejected id, find the next non-rejected card
            if (candidateId is null || skipIds.Contains(candidateId.Value))
            {
                // Try any card not in skipIds
                var fallback = _config.Deck.Peek(c => !skipIds.Contains(c.Id));
                if (fallback is null) break; // all remaining cards rejected
                candidateId = fallback.Id;
            }

            var candidateCard = _config.Deck.Peek(c => c.Id == candidateId.Value);
            if (candidateCard is null) break;

            // Phase 2: validate against rules — still no deck mutation
            var ruleResult = _ruleEvaluator.Evaluate(candidateCard, _currentPlayer, ruleContext);
            if (ruleResult.IsAllowed)
            {
                // Phase 3: single definitive draw
                selected = _config.Deck.DrawById(candidateId.Value);
                break;
            }

            skipIds.Add(candidateId.Value);
        }

        _currentCard = selected;
        return selected;
    }

    // ── Issue 1: unified scoring pipeline; Issue 8: special card policy ──────

    /// <inheritdoc />
    public void RecordOutcome(CardOutcome outcome)
    {
        EnsureState(GameState.Active);

        if (_currentPlayer is null || _currentCard is null)
            throw new InvalidOperationException(
                "No active turn to record. Call AdvanceTurn() first.");

        // Determine scoring based on card type and policy
        int scoreDelta;
        if (IsSpecialCard(_currentCard))
        {
            // Issue 8: special cards use explicit NoScore policy by default
            scoreDelta = _config.SpecialCardScoringPolicy switch
            {
                SpecialCardScoringPolicy.NoScore    => 0,
                SpecialCardScoringPolicy.FixedBonus => _config.SpecialCardBonusScore,
                SpecialCardScoringPolicy.ModeDefined =>
                    _config.ScoringStrategy.CalculateScore(_currentCard, _currentPlayer, outcome),
                _ => 0
            };
        }
        else
        {
            // Issue 1: FinalScore = ScoringStrategy + RuleEvaluator.ScoreDelta
            var strategyScore = _config.ScoringStrategy
                .CalculateScore(_currentCard, _currentPlayer, outcome);

            var ruleContext   = BuildRuleContext();
            var ruleResult    = _ruleEvaluator
                .Evaluate(_currentCard, _currentPlayer, ruleContext);

            scoreDelta = strategyScore + ruleResult.ScoreDelta;
        }

        _playerManager.ApplyScore(_currentPlayer.Id, scoreDelta);

        _metadata.MarkCardPlayed(_currentPlayer.Id, _currentCard.Id);
        _playedCards.Add(_currentCard);

        var args = new TurnCompletedEventArgs
        {
            Player     = _currentPlayer,
            Card       = _currentCard,
            Outcome    = outcome,
            ScoreDelta = scoreDelta,
            Round      = Round,
        };

        // Null out before firing events so AdvanceTurn() during event handling
        // can set fresh values without being overwritten afterwards.
        _currentCard   = null;
        _currentPlayer = null;

        TurnCompleted?.Invoke(this, args);

        // Issue 3 & 4: explicit turn counter + snapshotted player count for round advancement
        AdvanceRoundIfComplete();
    }

    /// <inheritdoc />
    public void RewindTurn(IPlayer player, ICard card)
    {
        EnsureState(GameState.Active);
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(card);

        // A card was already dealt for the turn we're abandoning — put it back
        // at the front of the deck so it isn't lost and is dealt again next.
        if (_currentCard is not null)
        {
            _config.Deck.Return(_currentCard);
            _currentCard = null;
        }

        // Drop the undone card from played history (most recent occurrence).
        for (var i = _playedCards.Count - 1; i >= 0; i--)
        {
            if (_playedCards[i].Id == card.Id)
            {
                _playedCards.RemoveAt(i);
                break;
            }
        }

        // Step the turn counter back. If the undone turn closed a round, roll the
        // round back too and re-open it one turn short.
        if (_turnsThisRound > 0)
        {
            _turnsThisRound--;
        }
        else if (Round > 1)
        {
            Round--;
            _turnsThisRound = Math.Max(0, _activePlayerSnapshot - 1);
        }

        // Restore the turn itself, and point the rotation at this player so the
        // NEXT turn goes to whoever legitimately follows them.
        _currentPlayer = player;
        _currentCard   = card;
        _playerManager.RewindTo(player.Id);
    }

    /// <inheritdoc />
    public void Pause()  { EnsureState(GameState.Active); State = GameState.Paused; }
    /// <inheritdoc />
    public void Resume() { EnsureState(GameState.Paused); State = GameState.Active; }

    /// <inheritdoc />
    public void End()
    {
        if (State == GameState.Ended) return;
        State = GameState.Ended;

        var standings = _playerManager.Players
            .OrderByDescending(p => p.Score)
            .ToList()
            .AsReadOnly();

        GameEnded?.Invoke(this, new GameEndedEventArgs
        {
            FinalStandings = standings,
            TotalRounds    = Round,
        });
    }

    // ── Round progression (Issue 3 & 4) ──────────────────────────────────────

    private void SnapshotActivePlayerCount()
    {
        _activePlayerSnapshot                     = _playerManager.ActivePlayers.Count;
        _metadata.ActivePlayersAtRoundStart        = _activePlayerSnapshot;
        _turnsThisRound                            = 0;
    }

    private void AdvanceRoundIfComplete()
    {
        _turnsThisRound++;

        // A round completes when every player who was active at its start has had a turn.
        // Status changes mid-round do not affect this count (immune to drift).
        var playersInRound = Math.Max(1, _activePlayerSnapshot);
        if (_turnsThisRound < playersInRound) return;

        // Issue 4: MaxRounds = completed playable rounds. End condition: Round == MaxRounds.
        if (_config.MaxRounds.HasValue && Round >= _config.MaxRounds.Value)
        {
            End();
            return;
        }

        Round++;
        SnapshotActivePlayerCount();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool IsSpecialCard(ICard card) =>
        card is IBreakCard or IRewardCard or IInspirationCard;

    private void EnsureState(GameState expected)
    {
        if (State != expected)
            throw new InvalidOperationException(
                $"Operation requires state '{expected}', current is '{State}'.");
    }

    private IProgressionContext BuildProgressionContext() =>
        new ProgressionContext(Round, _playedCards, _playerManager.Players, _metadata);

    private IRuleContext BuildRuleContext() =>
        new RuleContext(Round, _playerManager.Players, _config.Deck, _metadata);
}