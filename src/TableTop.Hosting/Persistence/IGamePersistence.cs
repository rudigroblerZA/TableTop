namespace TableTop.Hosting.Persistence;

/// <summary>
/// Contract for saving and restoring a game session.
///
/// This is the public-facing persistence API. Controllers, UI layers, and test
/// doubles all depend on this interface — never on a concrete implementation.
///
/// Replaces the older <see cref="ISessionRepository"/> name, which is kept as
/// a backward-compatible sub-interface.
/// </summary>
public interface IGamePersistence
{
    /// <summary>True when a saved snapshot exists and can be resumed.</summary>
    bool HasSavedSession { get; }

    /// <summary>Persists the current session state to durable storage.</summary>
    Task SaveAsync(SessionSnapshot snapshot, CancellationToken ct = default);

    /// <summary>
    /// Restores the most recent saved snapshot, or <c>null</c> if none exists
    /// or the snapshot is unreadable.
    /// </summary>
    Task<SessionSnapshot?> LoadAsync(CancellationToken ct = default);

    /// <summary>
    /// Deletes the saved snapshot. Called automatically when a session ends
    /// naturally so stale saves don't interfere with future sessions.
    /// </summary>
    Task DeleteAsync(CancellationToken ct = default);
}
