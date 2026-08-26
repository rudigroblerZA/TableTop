using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Domain.Players;

/// <summary>
/// Manages players with a round-robin turn order, skipping non-active players.
///
/// Stores players as <see cref="IPlayer"/> (not the concrete <see cref="Player"/> type)
/// so any IPlayer implementation can be registered — required for testability and
/// for hosts that wrap players in decorators.
///
/// Score and status mutations still work because IPlayerManager exposes
/// <see cref="ApplyScore"/> and <see cref="SetStatus"/> directly rather than
/// relying on mutable concrete properties.
/// </summary>
public sealed class RoundRobinPlayerManager : IPlayerManager
{
    // Issue 5: store as IPlayer, not the concrete Player type
    private readonly List<IPlayer>  _players = [];
    private readonly Dictionary<Guid, int>          _scores  = [];
    private readonly Dictionary<Guid, PlayerStatus> _statuses = [];
    private int _currentIndex = -1;

    // ── IPlayerManager ────────────────────────────────────────────────────────

    // Issue 6: cached read-only views — rebuilt only on mutation
    private IReadOnlyList<IPlayer>? _playersCache;
    private IReadOnlyList<IPlayer>? _activeCache;

    /// <summary>Players.</summary>
    public IReadOnlyList<IPlayer> Players =>
        _playersCache ??= _players.Select(Wrap).ToList().AsReadOnly();

    /// <summary>ActivePlayers.</summary>
    public IReadOnlyList<IPlayer> ActivePlayers =>
        _activeCache  ??= _players
            .Where(p => GetStatus(p.Id) == PlayerStatus.Active)
            .Select(Wrap)
            .ToList()
            .AsReadOnly();

    /// <inheritdoc />
    public void AddPlayer(IPlayer player)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (_players.Any(p => p.Id == player.Id))
            throw new InvalidOperationException($"Player {player.Id} is already registered.");

        _players.Add(player);
        _scores[player.Id]   = player.Score;   // seed from initial score
        _statuses[player.Id] = player.Status;  // seed from initial status
        InvalidateCache();
    }

    /// <inheritdoc />
    public void RemovePlayer(Guid playerId)
    {
        var player = _players.FirstOrDefault(p => p.Id == playerId)
            ?? throw new KeyNotFoundException($"Player {playerId} not found.");
        _players.Remove(player);
        _scores.Remove(playerId);
        _statuses.Remove(playerId);
        InvalidateCache();
    }

    /// <inheritdoc />
    public IPlayer? GetNextPlayer()
    {
        if (_players.Count == 0) return null;

        for (var i = 0; i < _players.Count; i++)
        {
            _currentIndex = (_currentIndex + 1) % _players.Count;
            var candidate = _players[_currentIndex];
            if (GetStatus(candidate.Id) == PlayerStatus.Active)
                return candidate;
        }
        return null;
    }

    /// <inheritdoc />
    public void RewindTo(Guid playerId)
    {
        var index = _players.FindIndex(p => p.Id == playerId);
        if (index >= 0) _currentIndex = index;
    }

    /// <inheritdoc />
    public void SetStatus(Guid playerId, PlayerStatus status)
    {
        EnsureKnown(playerId);
        _statuses[playerId] = status;
        InvalidateCache();
    }

    /// <inheritdoc />
    public void ApplyScore(Guid playerId, int delta)
    {
        EnsureKnown(playerId);
        _scores[playerId] += delta;
    }

    // ── IPlayer score/status projection ──────────────────────────────────────
    // The manager owns score and status; the IPlayer projection reads from here.

    /// <summary>Returns the current score managed by this manager for the given player.</summary>
    public int GetScore(Guid playerId) =>
        _scores.TryGetValue(playerId, out var s) ? s : 0;

    /// <summary>Returns the current status managed by this manager for the given player.</summary>
    public PlayerStatus GetStatus(Guid playerId) =>
        _statuses.TryGetValue(playerId, out var st) ? st : PlayerStatus.Active;

    // ── Private ───────────────────────────────────────────────────────────────

    private void EnsureKnown(Guid playerId)
    {
        if (!_scores.ContainsKey(playerId))
            throw new KeyNotFoundException($"Player {playerId} not found.");
    }

    private void InvalidateCache()
    {
        _playersCache = null;
        _activeCache  = null;
    }

    /// <summary>
    /// Projects an <see cref="IPlayer"/> with Score and Status sourced from this manager's
    /// dictionaries rather than from the underlying player object's own fields.
    /// This allows any IPlayer implementation to be managed without requiring mutable properties.
    /// </summary>
    private sealed class PlayerView : IPlayer
    {
        private readonly IPlayer                   _inner;
        private readonly RoundRobinPlayerManager   _manager;

        public PlayerView(IPlayer inner, RoundRobinPlayerManager manager)
        {
            _inner   = inner;
            _manager = manager;
        }

        /// <inheritdoc />
        public Guid   Id            => _inner.Id;
        /// <inheritdoc />
        public string DisplayName   => _inner.DisplayName;
        public IReadOnlyDictionary<string, string> Attributes => _inner.Attributes;
        public IReadOnlyList<string> Tags => _inner.Tags;

        // Score and Status are sourced from the manager, not the underlying player
        /// <inheritdoc />
        public int          Score  => _manager.GetScore(_inner.Id);
        public PlayerStatus Status => _manager.GetStatus(_inner.Id);
    }

    private PlayerView Wrap(IPlayer player) => new(player, this);
}