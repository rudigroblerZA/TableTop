namespace TableTop.Hosting.Persistence;

/// <summary>
/// Stores and retrieves player profiles between sessions.
/// Implementations may use JSON files, databases, cloud storage, etc. (OCP).
/// </summary>
public interface IPlayerRepository
{
    /// <summary>Loads all saved profiles, ordered by most recently used.</summary>
    Task<IReadOnlyList<PlayerProfile>> LoadAsync(CancellationToken ct = default);

    /// <summary>
    /// Persists the supplied profiles, replacing any previously saved list.
    /// </summary>
    Task SaveAsync(IEnumerable<PlayerProfile> profiles, CancellationToken ct = default);

    /// <summary>Removes all saved profiles.</summary>
    Task ClearAsync(CancellationToken ct = default);
}
