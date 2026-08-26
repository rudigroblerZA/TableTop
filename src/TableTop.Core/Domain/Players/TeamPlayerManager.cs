using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Domain.Players;

/// <summary>
/// Extends <see cref="RoundRobinPlayerManager"/> with team awareness.
///
/// Individual players still take turns in round-robin order.
/// Score deltas are credited to the <strong>team</strong> rather than
/// (or in addition to) the individual player, depending on <see cref="ScoreMode"/>.
///
/// Usage:
/// <code>
/// var teams = Team.SplitEvenly(players, teamCount: 2);
/// var manager = new TeamPlayerManager(teams);
/// // Drop into a controller exactly like RoundRobinPlayerManager
/// </code>
/// </summary>
public sealed class TeamPlayerManager : IPlayerManager
{
    private readonly RoundRobinPlayerManager _inner = new();
    private readonly List<Team> _teams;
    private readonly Dictionary<Guid, Team> _playerTeam = new(); // playerId → team

    /// <summary>
    /// Controls where score deltas land.
    /// </summary>
    public TeamScoreMode ScoreMode { get; }

    /// <summary>Initialises a new <see cref="TeamPlayerManager"/> instance.</summary>
    public TeamPlayerManager(IReadOnlyList<Team> teams, TeamScoreMode mode = TeamScoreMode.TeamOnly)
    {
        if (teams is null || teams.Count == 0)
            throw new ArgumentException("At least one team is required.", nameof(teams));

        _teams    = teams.ToList();
        ScoreMode = mode;

        foreach (var team in _teams)
        {
            foreach (var member in team.Members)
            {
                _inner.AddPlayer(member);
                _playerTeam[member.Id] = team;
            }
        }
    }

    // ── IPlayerManager ─────────────────────────────────────────────────────────

    /// <summary>Players.</summary>
    public IReadOnlyList<IPlayer> Players      => _inner.Players;
    /// <summary>ActivePlayers.</summary>
    public IReadOnlyList<IPlayer> ActivePlayers => _inner.ActivePlayers;

    /// <inheritdoc />
    public void AddPlayer(IPlayer player)
    {
        // Players added after construction join as solo "teams"
        _inner.AddPlayer(player);
        var solo = new Team(player.DisplayName, [player]);
        _teams.Add(solo);
        _playerTeam[player.Id] = solo;
    }

    /// <inheritdoc />
    public void RemovePlayer(Guid playerId)
    {
        _inner.RemovePlayer(playerId);
        _playerTeam.Remove(playerId);
        _teams.RemoveAll(t => !t.Members.Any(m => m.Id != playerId)
                             && t.Members.Any(m => m.Id == playerId)
                             && t.Members.Count == 1);
    }

    /// <inheritdoc />
    public IPlayer? GetNextPlayer() => _inner.GetNextPlayer();

    /// <inheritdoc />
    public void RewindTo(Guid playerId) => _inner.RewindTo(playerId);

    /// <inheritdoc />
    public void SetStatus(Guid playerId, PlayerStatus status) =>
        _inner.SetStatus(playerId, status);

    /// <summary>
    /// Applies a score delta. Where the delta lands depends on <see cref="ScoreMode"/>:
    /// - <c>TeamOnly</c>  — added to the team total; individual stays at 0.
    /// - <c>Both</c>      — added to both the team total and the individual.
    /// - <c>Individual</c> — added only to the individual (team play disabled for scoring).
    /// </summary>
    public void ApplyScore(Guid playerId, int delta)
    {
        if (ScoreMode is TeamScoreMode.Individual or TeamScoreMode.Both)
            _inner.ApplyScore(playerId, delta);

        if (ScoreMode is TeamScoreMode.TeamOnly or TeamScoreMode.Both)
        {
            if (_playerTeam.TryGetValue(playerId, out var team))
                team.ApplyScore(delta);
        }
    }

    // ── Team-specific API ──────────────────────────────────────────────────────

    /// <summary>All teams in this session.</summary>
    public IReadOnlyList<ITeam> Teams => _teams.Cast<ITeam>().ToList().AsReadOnly();

    /// <summary>Returns the team a player belongs to, or null for unaffiliated players.</summary>
    public ITeam? GetTeam(Guid playerId) =>
        _playerTeam.TryGetValue(playerId, out var t) ? t : null;

    /// <summary>Returns teams ranked by score, descending.</summary>
    public IReadOnlyList<ITeam> GetStandings() =>
        _teams.OrderByDescending(t => t.Score).Cast<ITeam>().ToList().AsReadOnly();
}

/// <summary>Where score deltas land in a team game.</summary>
public enum TeamScoreMode
{
    /// <summary>Deltas go only to the team total. Individual scores stay at zero.</summary>
    TeamOnly,

    /// <summary>Deltas go to both the team total and the individual player.</summary>
    Both,

    /// <summary>Deltas go only to the individual — team totals are not updated.</summary>
    Individual,
}