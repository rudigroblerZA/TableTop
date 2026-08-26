using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Game;

namespace TableTop.Hosting.Controllers.Services;

/// <summary>
/// Records a full audit trail of every turn played this session.
///
/// Used for:
///   - Hint engine context (recent outcomes and difficulties per player)
///   - <see cref="SessionReport"/> generation at game end
///   - <c>UndoLastTurn</c> — the most recent record is reversible
/// </summary>
public sealed class TurnHistoryTracker
{
    private const int MaxHistoryDepth = Core.TableTopDefaults.History.MaxDepth;

    // Fast per-player lookups for the hint engine
    private readonly Dictionary<Guid, List<CardOutcome>> _outcomes = new();
    private readonly Dictionary<Guid, List<Difficulty>> _difficulties = new();

    // Full ordered history for reports and undo
    private readonly List<TurnRecord> _allTurns = new();
    private int _turnCounter;

    /// <inheritdoc />
    public void Initialise(IReadOnlyList<IPlayer> players)
    {
        foreach (var p in players)
        {
            _outcomes[p.Id] = new List<CardOutcome>();
            _difficulties[p.Id] = new List<Difficulty>();
        }
    }

    /// <summary>
    /// Records a completed turn. Call after the score has been applied so
    /// <paramref name="scoreAfter"/> captures the player's current total.
    /// </summary>
    public void Record(
        IPlayer player,
        ICard card,
        CardOutcome outcome,
        int scoreDelta,
        int scoreAfter,
        int round,
        TimeSpan? elapsed = null)
    {
        _turnCounter++;

        // Hint engine fast path
        EnsureInitialised(player.Id);
        Prepend(_outcomes[player.Id], outcome, MaxHistoryDepth);
        Prepend(_difficulties[player.Id], card.Difficulty, MaxHistoryDepth);

        // Full audit record
        _allTurns.Add(new TurnRecord
        {
            TurnNumber = _turnCounter,
            Round = round,
            Player = player,
            Card = card,
            Outcome = outcome,
            ScoreDelta = scoreDelta,
            ScoreAfter = scoreAfter,
            Elapsed = elapsed,
        });
    }

    // ── Backward-compat shim for callers that only have IDs ───────────────────

    /// <summary>Legacy overload used by callers that don't have an ICard reference yet.</summary>
    public void Record(Guid playerId, CardOutcome outcome, Difficulty difficulty)
    {
        EnsureInitialised(playerId);
        Prepend(_outcomes[playerId], outcome, MaxHistoryDepth);
        Prepend(_difficulties[playerId], difficulty, MaxHistoryDepth);
        // NOTE: does not add to _allTurns — caller should prefer the full overload
    }

    // ── Hint engine queries ────────────────────────────────────────────────────

    /// <summary>Initialises a new <see cref="GetOutcomes"/> instance.</summary>
    public IReadOnlyList<CardOutcome> GetOutcomes(Guid playerId) =>
        _outcomes.TryGetValue(playerId, out var list) ? list : [];

    /// <summary>Initialises a new <see cref="GetDifficulties"/> instance.</summary>
    public IReadOnlyList<Difficulty> GetDifficulties(Guid playerId) =>
        _difficulties.TryGetValue(playerId, out var list) ? list : [];

    // ── Report & undo ─────────────────────────────────────────────────────────

    /// <summary>
    /// All turns recorded this session, in chronological order.
    /// Used by <see cref="SessionReport.Build"/>.
    /// </summary>
    public IReadOnlyList<TurnRecord> AllTurns => _allTurns.AsReadOnly();

    /// <summary>
    /// The most recent turn record, or null when no turns have been played.
    /// Used by <c>UndoLastTurn</c>.
    /// </summary>
    public TurnRecord? LastTurn => _allTurns.Count > 0 ? _allTurns[^1] : null;

    /// <summary>
    /// Removes the last turn record from the history (called after a successful undo).
    /// The hint-engine fast-path lists are also trimmed by one entry for the player.
    /// </summary>
    public void RemoveLastTurn()
    {
        if (_allTurns.Count == 0) return;

        var last = _allTurns[^1];
        _allTurns.RemoveAt(_allTurns.Count - 1);
        _turnCounter--;

        if (_outcomes.TryGetValue(last.Player.Id, out var outcomes) && outcomes.Count > 0)
            outcomes.RemoveAt(0);
        if (_difficulties.TryGetValue(last.Player.Id, out var diffs) && diffs.Count > 0)
            diffs.RemoveAt(0);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void EnsureInitialised(Guid playerId)
    {
        if (!_outcomes.ContainsKey(playerId))
        {
            _outcomes[playerId] = new List<CardOutcome>();
            _difficulties[playerId] = new List<Difficulty>();
        }
    }

    private static void Prepend<T>(List<T> list, T value, int maxDepth)
    {
        list.Insert(0, value);
        if (list.Count > maxDepth)
            list.RemoveAt(list.Count - 1);
    }
}