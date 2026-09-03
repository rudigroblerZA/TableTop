namespace TableTop.Hosting.Persistence;

/// <summary>
/// Stores the set of game modes a player has starred, between sessions.
///
/// <para>
/// Modes are identified by <c>IGameMode.Name</c> rather than by an id, because
/// a mode has no id — the catalogue is compiled in and the registry keys on the
/// name everywhere else (<c>SessionResumer</c> and <c>SavedSessionLookup</c>
/// both resolve a saved session by mode name). Using anything else here would
/// invent a second identity for the same thing.
/// </para>
///
/// <para>
/// The consequence is worth stating plainly: renaming a mode drops its
/// favourites, exactly as it already drops its saved sessions. That is why
/// <see cref="FavouritesService"/> ignores names it cannot resolve rather than
/// treating them as an error.
/// </para>
/// </summary>
public interface IFavouritesRepository
{
    /// <summary>Loads the saved mode names. Order is not meaningful.</summary>
    Task<IReadOnlyList<string>> LoadAsync(CancellationToken ct = default);

    /// <summary>Persists the supplied mode names, replacing the saved set.</summary>
    Task SaveAsync(IEnumerable<string> modeNames, CancellationToken ct = default);

    /// <summary>Removes every saved favourite.</summary>
    Task ClearAsync(CancellationToken ct = default);
}
