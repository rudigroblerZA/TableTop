using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Hosting.Events;

namespace TableTop.Hosting.Controllers.Services;

/// <summary>
/// Reverses the last completed turn: score, played-card history, engine
/// position, and the two events a host needs to redraw the screen.
///
/// Extracted from <see cref="CardTurnController"/> (backlog B.1). It is a
/// self-contained operation over <see cref="TurnHistoryTracker"/> and the game,
/// and it is the one place that has to know that undoing means rewinding the
/// ENGINE and not merely the scoreboard — see the remarks in <see cref="Undo"/>.
/// </summary>
internal sealed class UndoCoordinator
{
    private readonly IGame _game;
    private readonly TurnHistoryTracker _history;
    private readonly Core.Abstractions.IEngineDiagnostics _diagnostics;
    private readonly Func<IReadOnlyList<ScoreEntry>> _buildScores;
    private readonly Action<TurnUndoneEvent> _onTurnUndone;
    private readonly Action<CardReadyEvent> _onCardReady;

    public UndoCoordinator(
        IGame game,
        TurnHistoryTracker history,
        Core.Abstractions.IEngineDiagnostics diagnostics,
        Func<IReadOnlyList<ScoreEntry>> buildScores,
        Action<TurnUndoneEvent> onTurnUndone,
        Action<CardReadyEvent> onCardReady)
    {
        _game = game;
        _history = history;
        _diagnostics = diagnostics;
        _buildScores = buildScores;
        _onTurnUndone = onTurnUndone;
        _onCardReady = onCardReady;
    }

    /// <summary>
    /// Undoes the last turn and re-presents its card. Returns false when there
    /// is nothing to undo or the game is not active.
    /// </summary>
    public bool Undo()
    {
        if (_history.LastTurn is not { } last) return false;
        if (_game.State != GameState.Active) return false;

        // 1. Reverse the score
        _game.PlayerManager.ApplyScore(last.Player.Id, -last.ScoreDelta);

        // 2. Remove card from played-card history so it can be redrawn.
        //    GameMetadata tracks by (playerId, cardId) — remove that entry.
        _game.Metadata.RemoveCardPlayed(last.Player.Id, last.Card.Id);

        // 3. Rewind the ENGINE, not just the score. Without this the engine
        //    still holds the turn it had already advanced to, so the next
        //    recorded outcome would be applied to the following player and a
        //    card that was never shown — while the UI displayed the undone one.
        _game.RewindTurn(last.Player, last.Card);

        _history.RemoveLastTurn();
        _diagnostics.TurnUndone(last.Player, last.Card, last.Outcome, -last.ScoreDelta);

        _onTurnUndone(new TurnUndoneEvent(
            PlayerName: last.Player.DisplayName,
            CardTitle: last.Card.Title,
            ReversedOutcome: last.Outcome,
            ScoreRestored: -last.ScoreDelta,
            CurrentScores: _buildScores()));

        // Re-present the same card to the same player.
        var text = last.Card is IPromptCard p
            ? p.ResolvePrompt(last.Player)
            : last.Card.Description;

        _onCardReady(new CardReadyEvent(
            Player: last.Player,
            PlayerName: last.Player.DisplayName,
            Card: last.Card,
            CardTitle: last.Card.Title,
            CardText: text,
            Category: last.Card.Category,
            Difficulty: last.Card.Difficulty.ToString(),
            Restriction: last.Card.Restriction?.Description,
            Round: last.Round));

        return true;
    }
}
