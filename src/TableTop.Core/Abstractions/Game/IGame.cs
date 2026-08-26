using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;

namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Controls the lifecycle of a single game session.
/// </summary>
public interface IGame
{
    /// <summary>Unique session identifier.</summary>
    Guid Id { get; }

    /// <summary>Current lifecycle state of the game.</summary>
    GameState State { get; }

    /// <summary>Current round number (1-based).</summary>
    int Round { get; }

    /// <summary>The player whose turn it currently is.</summary>
    IPlayer? CurrentPlayer { get; }

    /// <summary>
    /// The card currently face-up awaiting an outcome, or null between turns.
    /// Revealed-but-unresolved cards count as spent for persistence purposes:
    /// the table has already read them.
    /// </summary>
    ICard? CurrentCard { get; }

    /// <summary>Cards played since the session started, in order.</summary>
    IReadOnlyList<ICard> PlayedCards { get; }

    /// <summary>
    /// Session metadata: played-card tracking, round state, and extension data.
    /// Exposed so hosts can seed played-card history when resuming from a snapshot.
    /// </summary>
    GameMetadata Metadata { get; }

    /// <summary>
    /// The player manager for this session.
    /// Exposed so hosts can apply score changes and status updates for special card types
    /// (e.g. reward cards, break cards) outside the normal RecordOutcome flow.
    /// </summary>
    IPlayerManager PlayerManager { get; }

    /// <summary>
    /// Starts the game. Transitions from <see cref="GameState.Pending"/> to
    /// <see cref="GameState.Active"/>.
    /// </summary>
    void Start();

    /// <summary>
    /// Advances to the next player's turn and returns the card assigned to them.
    /// </summary>
    /// <returns>
    /// The selected card for the current player, or null when no eligible card exists.
    /// </returns>
    ICard? AdvanceTurn();

    /// <summary>
    /// Records the outcome of the current player's card and applies scoring.
    /// </summary>
    void RecordOutcome(CardOutcome outcome);

    /// <summary>
    /// Rewinds the engine to an already-recorded turn so it can be replayed —
    /// the state half of an "undo".
    ///
    /// Restores <paramref name="player"/> and <paramref name="card"/> as the
    /// current turn, returns any card that had since been dealt back to the deck,
    /// drops the undone card from played history, and steps the round/turn
    /// counters back one turn. After this call, <see cref="RecordOutcome"/>
    /// applies to the rewound turn.
    /// </summary>
    void RewindTurn(IPlayer player, ICard card);

    /// <summary>Pauses an active game.</summary>
    void Pause();

    /// <summary>Resumes a paused game.</summary>
    void Resume();

    /// <summary>Ends the game session.</summary>
    void End();

    /// <summary>
    /// Raised each time a turn completes.
    /// </summary>
    event EventHandler<TurnCompletedEventArgs>? TurnCompleted;

    /// <summary>Raised when the game ends.</summary>
    event EventHandler<GameEndedEventArgs>? GameEnded;
}
