using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Domain.Players;

/// <summary>
/// Mutable team entity. Score is maintained here, not on individual members.
/// </summary>
public sealed class Team : ITeam
{
    private readonly List<IPlayer> _members;

    /// <summary>Initialises a new <see cref="Team"/> instance.</summary>
    public Team(string name, IEnumerable<IPlayer> members)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Id = Guid.NewGuid();
        Name = name;
        _members = members?.ToList()
            ?? throw new ArgumentNullException(nameof(members));
        if (_members.Count == 0)
            throw new ArgumentException("A team must have at least one member.", nameof(members));
    }

    /// <inheritdoc />
    public Guid Id { get; }
    /// <inheritdoc />
    public string Name { get; }
    /// <inheritdoc />
    public int Score { get; private set; }

    /// <summary>Members.</summary>
    public IReadOnlyList<IPlayer> Members => _members.AsReadOnly();

    /// <inheritdoc />
    public bool Contains(Guid playerId) =>
        _members.Any(m => m.Id == playerId);

    /// <summary>Applies a score delta to this team (called by TeamPlayerManager).</summary>
    internal void ApplyScore(int delta) => Score += delta;

    /// <summary>
    /// Factory: builds N equally-sized teams from an ordered player list.
    /// Leftover players (when count is not divisible) go into the last team.
    /// </summary>
    public static IReadOnlyList<Team> SplitEvenly(
        IReadOnlyList<IPlayer> players,
        int teamCount,
        Func<int, string>? nameFactory = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(teamCount, 1);
        if (players.Count < teamCount)
            throw new ArgumentException(
                $"Cannot create {teamCount} teams from {players.Count} players.");

        nameFactory ??= i => $"Team {i + 1}";
        var size = players.Count / teamCount;
        var teams = new List<Team>();

        for (int i = 0; i < teamCount; i++)
        {
            var isLast = i == teamCount - 1;
            var members = isLast
                ? players.Skip(i * size).ToList()
                : players.Skip(i * size).Take(size).ToList();
            teams.Add(new Team(nameFactory(i), members));
        }

        return teams.AsReadOnly();
    }

    /// <summary>
    /// Factory: builds teams from explicit groups.
    /// </summary>
    public static IReadOnlyList<Team> FromGroups(
        IReadOnlyList<(string Name, IReadOnlyList<IPlayer> Members)> groups)
    {
        ArgumentOutOfRangeException.ThrowIfZero(groups.Count);
        return groups.Select(g => new Team(g.Name, g.Members)).ToList().AsReadOnly();
    }
}