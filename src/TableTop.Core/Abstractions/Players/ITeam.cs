namespace TableTop.Core.Abstractions.Players;

/// <summary>
/// A named group of players who share a score pool.
///
/// Teams sit above <see cref="IPlayer"/> — individual players still take turns
/// and earn personal score deltas, but those deltas are credited to the team's
/// aggregate instead of (or in addition to) the individual's tally.
///
/// Supported formats:
///   - Couples vs Couples: two teams of two competing head-to-head
///   - Pub quiz: mixed teams of 3–6 competing for the highest total
///   - Solo: each "team" has one member (degenerate case, always works)
/// </summary>
public interface ITeam
{
    /// <summary>Stable identifier for this team.</summary>
    Guid Id { get; }

    /// <summary>Display name, e.g. "Team Alice &amp; Bob".</summary>
    string Name { get; }

    /// <summary>Players who are members of this team.</summary>
    IReadOnlyList<IPlayer> Members { get; }

    /// <summary>Combined score for this team across the current session.</summary>
    int Score { get; }

    /// <summary>Returns true when the given player is a member of this team.</summary>
    bool Contains(Guid playerId);
}
