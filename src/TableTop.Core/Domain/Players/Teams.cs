using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Domain.Players;

/// <summary>
/// Team membership for a session.
///
/// <para>
/// <b>Deliberately not a change to <see cref="IPlayer"/>.</b> A team is stored
/// as an ordinary entry in <see cref="IPlayer.Attributes"/> under
/// <see cref="AttributeKey"/> — the same mechanism already carrying "gender"
/// and "age". That choice buys three things outright, none of which a new
/// <c>Team</c> property on <c>IPlayer</c> would have: every existing
/// <c>IPlayer</c> implementation keeps compiling untouched; the save format
/// needs no migration, because <c>PersistenceCoordinator</c> already
/// round-trips the whole attribute dictionary; and a resumed session restores
/// team membership for free via <c>SessionResumer</c>, with no code aware that
/// teams exist.
/// </para>
///
/// <para>
/// The cost of that choice, stated honestly: team membership is a string, so
/// nothing at compile time stops a typo putting someone on team "Rde". The
/// mitigation is that helpers here are the only sanctioned way to read or
/// write it, and <see cref="TeamNames"/> exposes what a session actually
/// contains rather than what anyone assumed.
/// </para>
/// </summary>
public static class Teams
{
    /// <summary>The <see cref="IPlayer.Attributes"/> key holding a player's team name.</summary>
    public const string AttributeKey = "team";

    /// <summary>Returns the player's team name, or null when they aren't on one.</summary>
    public static string? TeamOf(IPlayer player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.Attributes.TryGetValue(AttributeKey, out var team) && !string.IsNullOrWhiteSpace(team)
            ? team
            : null;
    }

    /// <summary>True when the player has been assigned to a team.</summary>
    public static bool HasTeam(IPlayer player) => TeamOf(player) is not null;

    /// <summary>
    /// Every distinct team present among the given players, in first-appearance
    /// order — the order players were added, which is the order a host entered
    /// them, which is the order that reads naturally on a scoreboard.
    /// Players with no team are ignored.
    /// </summary>
    public static IReadOnlyList<string> TeamNames(IEnumerable<IPlayer> players)
    {
        ArgumentNullException.ThrowIfNull(players);

        var seen = new List<string>();
        foreach (var player in players)
        {
            var team = TeamOf(player);
            if (team is not null && !seen.Contains(team, StringComparer.OrdinalIgnoreCase))
                seen.Add(team);
        }
        return seen.AsReadOnly();
    }

    /// <summary>Players belonging to the named team.</summary>
    public static IReadOnlyList<IPlayer> MembersOf(IEnumerable<IPlayer> players, string teamName)
    {
        ArgumentNullException.ThrowIfNull(players);
        return players
            .Where(p => string.Equals(TeamOf(p), teamName, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Total score for a team — the sum of its members'. Team score is
    /// deliberately derived rather than stored: a single source of truth means
    /// a team total can never drift from the individual scores it's made of,
    /// and existing per-player scoring keeps working with no changes at all.
    /// </summary>
    public static int ScoreOf(IEnumerable<IPlayer> players, string teamName) =>
        MembersOf(players, teamName).Sum(p => p.Score);

    /// <summary>
    /// Team standings, highest first. Ties are preserved in the order returned
    /// rather than broken arbitrarily — the caller decides whether a tie means
    /// a draw, and several modes legitimately do.
    /// </summary>
    public static IReadOnlyList<TeamStanding> Standings(IEnumerable<IPlayer> players)
    {
        var all = players as IReadOnlyList<IPlayer> ?? players.ToList();
        return TeamNames(all)
            .Select(name => new TeamStanding(name, ScoreOf(all, name), MembersOf(all, name).Count))
            .OrderByDescending(s => s.Score)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Assigns players to <paramref name="teamCount"/> teams by dealing them
    /// out one at a time rather than splitting the list in half.
    ///
    /// <para>
    /// Dealing matters: people are usually entered in the order they're sitting,
    /// so halving the list tends to put one side of the sofa against the other.
    /// Dealing alternates them, which is what actually happens when a room
    /// picks teams. It also keeps sizes within one of each other for any player
    /// count, including odd ones.
    /// </para>
    /// </summary>
    public static IReadOnlyList<(IPlayer Player, string Team)> Deal(
        IReadOnlyList<IPlayer> players, int teamCount = 2, IReadOnlyList<string>? teamNames = null)
    {
        ArgumentNullException.ThrowIfNull(players);
        if (teamCount < 2)
            throw new ArgumentOutOfRangeException(nameof(teamCount), teamCount,
                "A team game needs at least two teams.");
        if (players.Count < teamCount)
            throw new ArgumentException(
                $"{players.Count} player(s) cannot fill {teamCount} teams — every team needs someone on it.",
                nameof(players));

        var names = teamNames ?? DefaultTeamNames.Take(teamCount).ToList();
        if (names.Count < teamCount)
            throw new ArgumentException(
                $"{names.Count} team name(s) supplied for {teamCount} teams.", nameof(teamNames));

        return players
            .Select((p, i) => (Player: p, Team: names[i % teamCount]))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Default team names. Colours rather than "Team 1"/"Team 2" because a
    /// table says them out loud all evening, and they need to survive being
    /// shouted across a room.
    /// </summary>
    public static IReadOnlyList<string> DefaultTeamNames { get; } =
        new[] { "Red", "Blue", "Green", "Gold" }.AsReadOnly();
}

/// <summary>One team's position on the scoreboard.</summary>
/// <param name="Name">The team's name.</param>
/// <param name="Score">Sum of its members' scores.</param>
/// <param name="MemberCount">How many players are on it.</param>
public readonly record struct TeamStanding(string Name, int Score, int MemberCount);
