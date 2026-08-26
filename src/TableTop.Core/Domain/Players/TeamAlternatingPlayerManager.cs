using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Domain.Players;

/// <summary>
/// Turn order that alternates between teams, taking the next unused player
/// from each team in rotation: Red's first, Blue's first, Red's second,
/// Blue's second, and so on.
///
/// <para>
/// <b>Why this exists rather than reusing <see cref="RoundRobinPlayerManager"/>.</b>
/// Round-robin walks the player list in entry order. For a team game that's
/// actively wrong: a host who enters "Amy, Ben, Cara, Dan" and deals them
/// Red/Blue/Red/Blue gets the order Amy(R), Ben(B), Cara(R), Dan(B) — which
/// happens to alternate. But a host who enters all of one team first, which
/// is at least as natural, gets both Red turns back to back. Team play needs
/// alternation to be a property of the manager, not an accident of data entry.
/// </para>
///
/// <para>
/// Everything else is deliberately delegated to a wrapped
/// <see cref="RoundRobinPlayerManager"/> — scores, statuses, the
/// <c>PlayerView</c> projection that makes score/status come from the manager
/// rather than the player object. Only <see cref="GetNextPlayer"/> and
/// <see cref="RewindTo"/> genuinely differ, so only those are reimplemented.
/// Reimplementing the rest would have meant duplicating the score-projection
/// logic, which is exactly the kind of duplication this codebase has spent a
/// lot of effort removing.
/// </para>
///
/// <para>
/// Players with no team are not dropped — they're treated as a team of their
/// own named after themselves, so a mixed session degrades to something
/// sensible instead of silently skipping people.
/// </para>
/// </summary>
public sealed class TeamAlternatingPlayerManager : IPlayerManager
{
    private readonly RoundRobinPlayerManager _inner = new();

    /// <summary>Index of the team whose turn is next, into <see cref="TeamOrder"/>.</summary>
    private int _teamCursor;

    /// <summary>Per-team index of which member goes next.</summary>
    private readonly Dictionary<string, int> _memberCursors = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public IReadOnlyList<IPlayer> Players => _inner.Players;

    /// <inheritdoc />
    public IReadOnlyList<IPlayer> ActivePlayers => _inner.ActivePlayers;

    /// <summary>Teams in the order they take turns — first-appearance order of their members.</summary>
    public IReadOnlyList<string> TeamOrder => TeamKeys(ActivePlayers);

    /// <inheritdoc />
    public void AddPlayer(IPlayer player) => _inner.AddPlayer(player);

    /// <inheritdoc />
    public void RemovePlayer(Guid playerId) => _inner.RemovePlayer(playerId);

    /// <inheritdoc />
    public void SetStatus(Guid playerId, PlayerStatus status) => _inner.SetStatus(playerId, status);

    /// <inheritdoc />
    public void ApplyScore(Guid playerId, int delta) => _inner.ApplyScore(playerId, delta);

    /// <inheritdoc />
    public IPlayer? GetNextPlayer()
    {
        var active = ActivePlayers;
        if (active.Count == 0) return null;

        var teams = TeamKeys(active);
        if (teams.Count == 0) return null;

        // Try each team in turn starting from the cursor, so a team whose
        // members have all gone inactive is skipped rather than stalling the
        // game on an empty side.
        for (var attempt = 0; attempt < teams.Count; attempt++)
        {
            var team = teams[_teamCursor % teams.Count];
            _teamCursor = (_teamCursor + 1) % teams.Count;

            var members = active.Where(p => KeyFor(p) == team).ToList();
            if (members.Count == 0) continue;

            var cursor = _memberCursors.TryGetValue(team, out var c) ? c : 0;
            var player = members[cursor % members.Count];
            _memberCursors[team] = (cursor + 1) % members.Count;
            return player;
        }

        return null;
    }

    /// <inheritdoc />
    public void RewindTo(Guid playerId)
    {
        // Undo has to put the pointer back so the SAME player's team goes
        // again next — otherwise undoing a turn silently hands the next card
        // to the other side, which is worse than the mistake being undone.
        var player = ActivePlayers.FirstOrDefault(p => p.Id == playerId);
        if (player is null) return;

        var teams = TeamKeys(ActivePlayers);
        var team  = KeyFor(player);

        var teamIndex = teams.ToList().FindIndex(t => string.Equals(t, team, StringComparison.OrdinalIgnoreCase));
        if (teamIndex >= 0) _teamCursor = teamIndex;

        var members = ActivePlayers.Where(p => KeyFor(p) == team).ToList();
        var memberIndex = members.FindIndex(p => p.Id == playerId);
        if (memberIndex >= 0) _memberCursors[team] = memberIndex;

        _inner.RewindTo(playerId);
    }

    /// <summary>Current score for a player, as tracked by this manager.</summary>
    public int GetScore(Guid playerId) => _inner.GetScore(playerId);

    /// <summary>Current status for a player, as tracked by this manager.</summary>
    public PlayerStatus GetStatus(Guid playerId) => _inner.GetStatus(playerId);

    /// <summary>
    /// A player's grouping key: their team, or their own name when they have
    /// none — so an unassigned player still gets turns instead of vanishing.
    /// </summary>
    private static string KeyFor(IPlayer player) => Teams.TeamOf(player) ?? player.DisplayName;

    private static IReadOnlyList<string> TeamKeys(IEnumerable<IPlayer> players)
    {
        var seen = new List<string>();
        foreach (var p in players)
        {
            var key = KeyFor(p);
            if (!seen.Contains(key, StringComparer.OrdinalIgnoreCase)) seen.Add(key);
        }
        return seen.AsReadOnly();
    }
}
