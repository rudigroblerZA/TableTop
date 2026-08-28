namespace TableTop.Hosting.Persistence;

/// <summary>
/// Stores and retrieves saved rosters (<see cref="RosterProfile"/>) between
/// sessions — the roster-shaped sibling of <see cref="IPlayerRepository"/>.
///
/// Added for backlog item 28: Console had a player-setup flow but no way to
/// save a whole group and start a later game from it. Lives in Hosting rather
/// than a head so it is unit-testable and reusable by any non-graphical head.
/// </summary>
public interface IRosterRepository
{
    /// <summary>Loads every saved roster, or an empty list if none exist or the stored data is unreadable.</summary>
    Task<IReadOnlyList<RosterProfile>> LoadAsync(CancellationToken ct = default);

    /// <summary>Persists the supplied rosters, replacing any previously saved list.</summary>
    Task SaveAsync(IEnumerable<RosterProfile> rosters, CancellationToken ct = default);

    /// <summary>Removes all saved rosters.</summary>
    Task ClearAsync(CancellationToken ct = default);
}
