namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Typed game session metadata passed to progression strategies and rule evaluators.
///
/// Replaces the freeform <c>Dictionary&lt;string,object&gt;</c> that previously held
/// critical invariants under stringly-typed keys, making engine contracts explicit
/// and catching misuse at compile time rather than at runtime.
/// </summary>
public sealed class GameMetadata
{
    private readonly HashSet<string> _playedCardKeys = [];

    // ── Played-card tracking (replaces "played:{playerId}:{cardId}" keys) ───────

    /// <summary>Records that a card has been played by a player.</summary>
    public void MarkCardPlayed(Guid playerId, Guid cardId) =>
        _playedCardKeys.Add(PlayedKey(playerId, cardId));

    /// <summary>Returns true when the card has already been played by the given player.</summary>
    public bool HasCardBeenPlayedBy(Guid playerId, Guid cardId) =>
        _playedCardKeys.Contains(PlayedKey(playerId, cardId));

    /// <summary>
    /// Removes the played-card record for the given player/card pair.
    /// Called by <c>UndoLastTurn</c> so the card can be redrawn.
    /// </summary>
    public void RemoveCardPlayed(Guid playerId, Guid cardId) =>
        _playedCardKeys.Remove(PlayedKey(playerId, cardId));

    /// <summary>
    /// Seeds the played-card tracking from a persisted snapshot, allowing
    /// <see cref="TableTop.Core.Domain.Rules.NoDuplicateCardRule"/> to correctly exclude already-played cards
    /// after a session is resumed.
    /// </summary>
    /// <param name="playerIds">All player IDs in the session.</param>
    /// <param name="playedCardIds">Card IDs that were played before the save point.</param>
    /// <remarks>
    /// Because the snapshot stores card IDs but not which player played which card,
    /// we mark every card as played by every player. This is conservative but correct:
    /// it prevents any played card from being replayed by any player in the resumed session.
    /// </remarks>
    public void SeedFromSnapshot(IEnumerable<Guid> playerIds, IEnumerable<Guid> playedCardIds)
    {
        var playerList = playerIds.ToList();
        foreach (var cardId in playedCardIds)
            foreach (var playerId in playerList)
                MarkCardPlayed(playerId, cardId);
    }

    // ── Round snapshot (supports stable round progression) ───────────────────

    /// <summary>
    /// Number of active players snapshotted at the start of the current round.
    /// Used to determine when a full round has been completed without being affected
    /// by mid-round status changes.
    /// </summary>
    public int ActivePlayersAtRoundStart { get; set; }

    // ── Extension dictionary (for mode-specific state) ───────────────────────

    /// <summary>
    /// Arbitrary mode-specific metadata keyed by convention.
    /// Use typed properties above for core engine invariants;
    /// reserve this for extension data that doesn't belong in the main contract.
    /// </summary>
    private readonly Dictionary<string, object> _extensions = [];
    /// <summary>Stores an arbitrary extension value keyed by <paramref name="key"/>.</summary>

    /// <summary>Retrieves a typed extension value. Returns false when the key is absent or the value is the wrong type.</summary>
    public void SetExtension(string key, object value) => _extensions[key] = value;
    /// <summary>Retrieves a typed extension value. Returns false when the key is absent or the value is the wrong type.</summary>
    public bool TryGetExtension<T>(string key, out T? value)
    {
        if (_extensions.TryGetValue(key, out var raw) && raw is T typed)
        { value = typed; return true; }
        value = default;
        return false;
    }

    private static string PlayedKey(Guid playerId, Guid cardId) =>
        $"{playerId}:{cardId}";
}