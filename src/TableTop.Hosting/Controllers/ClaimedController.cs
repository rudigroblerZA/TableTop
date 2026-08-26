using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Events;

namespace TableTop.Hosting.Controllers;

/// <summary>
/// Drives a "Claimed!" area-control session. Has no knowledge of any UI, and —
/// like <see cref="MillionaireController"/> and <see cref="MonogamyController"/>
/// — no knowledge of <see cref="Core.Abstractions.Game.IGame"/> either. A
/// territory-holding game has no per-card outcome loop to reuse; it has turns,
/// a board, and a win check, so it owns all three itself.
/// </summary>
public sealed class ClaimedController : IClaimedController
{
    private readonly IReadOnlyList<IPlayer>          _players;
    private readonly int                             _winningTerritoryCount;
    private readonly Dictionary<string, List<ICard>> _pools;      // territory -> remaining cards, shuffled
    private readonly Dictionary<string, Guid?>       _holders;    // territory -> holding player id, null = open

    private int    _currentIndex;
    private string? _pendingTerritory;
    private bool     _pendingWasRaid;

    /// <inheritdoc />
    public event EventHandler<TerritoryChallengeReadyEvent>? TerritoryChallengeReady;
    /// <inheritdoc />
    public event EventHandler<TerritoryClaimedEvent>?         TerritoryClaimed;
    /// <inheritdoc />
    public event EventHandler<TerritoryStolenEvent>?          TerritoryStolen;
    /// <inheritdoc />
    public event EventHandler<ChallengeFailedEvent>?          ChallengeFailed;
    /// <inheritdoc />
    public event EventHandler<ClaimedGameEndedEvent>?         GameEnded;

    /// <inheritdoc />
    public bool IsRunning { get; private set; }

    /// <param name="players">Players in the session, turn order = list order.</param>
    /// <param name="deck">The challenge deck; grouped into territories by <see cref="ICard.Category"/>.</param>
    /// <param name="winningTerritoryCount">Territories held simultaneously to win outright.</param>
    public ClaimedController(
        IReadOnlyList<IPlayer> players,
        IReadOnlyList<ICard>   deck,
        int                    winningTerritoryCount)
    {
        if (players.Count < 2)
            throw new ArgumentException("Claimed! needs at least two players.", nameof(players));

        _players               = players;
        _winningTerritoryCount = winningTerritoryCount;

        var rng = new Random();
        _pools = deck
            .GroupBy(c => c.Category)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(_ => rng.Next()).ToList());

        if (_pools.Count < winningTerritoryCount)
            throw new ArgumentException(
                $"The deck has {_pools.Count} territories but winningTerritoryCount is " +
                $"{winningTerritoryCount} — nobody could ever win. Check the deck's categories.",
                nameof(deck));

        _holders = _pools.Keys.ToDictionary(t => t, _ => (Guid?)null);
    }

    // ── IClaimedController ───────────────────────────────────────────────────

    /// <inheritdoc />
    public void Start() => IsRunning = true;

    /// <inheritdoc />
    public string CurrentPlayerName => _players[_currentIndex].DisplayName;

    /// <inheritdoc />
    public IReadOnlyList<string> ChallengeableTerritories
    {
        get
        {
            var mine = _players[_currentIndex].Id;
            return _pools
                .Where(kv => kv.Value.Count > 0 && _holders[kv.Key] != mine)
                .Select(kv => kv.Key)
                .ToList();
        }
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string?> TerritoryHolders =>
        _holders.ToDictionary(
            kv => kv.Key,
            kv => kv.Value is { } id ? _players.First(p => p.Id == id).DisplayName : null);

    /// <inheritdoc />
    public void ChallengeTerritory(string territoryName)
    {
        if (!IsRunning) return;
        if (!ChallengeableTerritories.Contains(territoryName)) return;   // not legal this turn — silently ignored, same as Millionaire's SubmitAnswer guard

        var card = _pools[territoryName][^1];
        _pools[territoryName].RemoveAt(_pools[territoryName].Count - 1);

        _pendingTerritory = territoryName;
        var holderId = _holders[territoryName];
        _pendingWasRaid = holderId is not null;

        TerritoryChallengeReady?.Invoke(this, new TerritoryChallengeReadyEvent(
            PlayerName:    CurrentPlayerName,
            TerritoryName: territoryName,
            CardTitle:     card.Title,
            CardText:      card.Description,
            Difficulty:    card.Difficulty.ToString(),
            DefenderName:  holderId is { } id ? _players.First(p => p.Id == id).DisplayName : null));
    }

    /// <inheritdoc />
    public void ResolveChallenge(bool succeeded)
    {
        if (_pendingTerritory is null) return;   // nothing pending — ChallengeTerritory wasn't called, or already resolved

        var territory  = _pendingTerritory;
        var wasRaid     = _pendingWasRaid;
        var player      = _players[_currentIndex];
        _pendingTerritory = null;

        if (!succeeded)
        {
            ChallengeFailed?.Invoke(this, new ChallengeFailedEvent(player.DisplayName, territory, wasRaid));
            AdvanceTurnOrEnd();
            return;
        }

        var previousHolderId = _holders[territory];
        _holders[territory] = player.Id;

        if (wasRaid)
        {
            var defender = _players.First(p => p.Id == previousHolderId);
            TerritoryStolen?.Invoke(this, new TerritoryStolenEvent(
                AttackerName:            player.DisplayName,
                DefenderName:            defender.DisplayName,
                TerritoryName:           territory,
                AttackerHeldTerritories: HeldBy(player.Id),
                DefenderHeldTerritories: HeldBy(defender.Id)));
        }
        else
        {
            TerritoryClaimed?.Invoke(this, new TerritoryClaimedEvent(
                PlayerName:      player.DisplayName,
                TerritoryName:   territory,
                HeldTerritories: HeldBy(player.Id)));
        }

        if (HeldBy(player.Id).Count >= _winningTerritoryCount)
        {
            End(ClaimedEndReason.ThreeHeld, [player.DisplayName]);
            return;
        }

        AdvanceTurnOrEnd();
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private List<string> HeldBy(Guid playerId) =>
        _holders.Where(kv => kv.Value == playerId).Select(kv => kv.Key).ToList();

    private void AdvanceTurnOrEnd()
    {
        // Exhausted when nobody, from any seat, has a legal move left — not
        // merely when the current player is out, since the next player around
        // the table might still have an open territory or a raid available.
        var anyoneHasAMove = _pools.Any(kv =>
            kv.Value.Count > 0 && _players.Any(p => _holders[kv.Key] != p.Id));

        if (!anyoneHasAMove)
        {
            var maxHeld = _players.Max(p => HeldBy(p.Id).Count);
            var winners = _players.Where(p => HeldBy(p.Id).Count == maxHeld)
                                   .Select(p => p.DisplayName)
                                   .ToList();
            End(ClaimedEndReason.DeckExhausted, winners);
            return;
        }

        _currentIndex = (_currentIndex + 1) % _players.Count;
    }

    private void End(ClaimedEndReason reason, IReadOnlyList<string> winners)
    {
        IsRunning = false;
        var holdings = _players.ToDictionary(
            p => p.DisplayName,
            IReadOnlyList<string> (p) => HeldBy(p.Id));

        GameEnded?.Invoke(this, new ClaimedGameEndedEvent(reason, winners, holdings));
    }

    /// <inheritdoc />
    public void Dispose() { /* no managed resources to release */ }
}
