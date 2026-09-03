using TableTop.Core.Abstractions.Game;
using TableTop.Hosting.Persistence;

namespace TableTop.Hosting;

/// <summary>
/// The game-facing half of favourites: an in-memory set that a picker can query
/// synchronously while it renders, backed by an <see cref="IFavouritesRepository"/>
/// that it writes through to on every change.
///
/// <para>
/// <b>Why a service and not just the repository.</b> A mode list asks "is this
/// one starred?" once per row, during layout, on the UI thread. Making that an
/// <c>await</c> per row would either block the render or force every head to
/// build the same cache. So the set is loaded once via
/// <see cref="LoadAsync"/> and read synchronously thereafter; only mutations go
/// to disk.
/// </para>
///
/// <para>
/// <b>Write-through, not write-behind.</b> <see cref="ToggleAsync"/> updates the
/// in-memory set and persists in the same call, so a star survives a kill as
/// well as a clean exit. If the write throws, the in-memory change is rolled
/// back before rethrowing — otherwise the UI would show a star that no longer
/// exists anywhere, which is the worse of the two failures.
/// </para>
///
/// <para>
/// Names are matched case-insensitively but stored as first written, matching
/// how the registry treats mode names elsewhere.
/// </para>
/// </summary>
public sealed class FavouritesService
{
    private readonly IFavouritesRepository _repository;
    private readonly HashSet<string> _names = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Creates a service over the given store.</summary>
    public FavouritesService(IFavouritesRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    /// <summary>Raised after a mode is starred or unstarred, so a list can refresh.</summary>
    public event EventHandler<FavouriteChangedEventArgs>? FavouriteChanged;

    /// <summary>
    /// Every starred mode name. Includes names that no longer resolve to a mode
    /// in the catalogue — see <see cref="IFavouritesRepository"/> on renames.
    /// </summary>
    public IReadOnlyCollection<string> Names => _names.ToList().AsReadOnly();

    /// <summary>How many modes are starred.</summary>
    public int Count => _names.Count;

    /// <summary>
    /// Loads the saved set. Call once at startup; every other member on this
    /// type is synchronous or writes through.
    /// </summary>
    public async Task LoadAsync(CancellationToken ct = default)
    {
        var saved = await _repository.LoadAsync(ct).ConfigureAwait(false);

        _names.Clear();
        foreach (var name in saved)
            _names.Add(name);
    }

    /// <summary>True when this mode is starred.</summary>
    public bool IsFavourite(IGameMode mode)
    {
        ArgumentNullException.ThrowIfNull(mode);
        return IsFavourite(mode.Name);
    }

    /// <inheritdoc cref="IsFavourite(IGameMode)" />
    public bool IsFavourite(string modeName) =>
        !string.IsNullOrWhiteSpace(modeName) && _names.Contains(modeName);

    /// <summary>
    /// Stars an unstarred mode or unstars a starred one, persisting the result.
    /// Returns the new state.
    /// </summary>
    public Task<bool> ToggleAsync(IGameMode mode, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(mode);
        return ToggleAsync(mode.Name, ct);
    }

    /// <inheritdoc cref="ToggleAsync(IGameMode, CancellationToken)" />
    public async Task<bool> ToggleAsync(string modeName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modeName);

        var nowFavourite = !_names.Contains(modeName);

        if (nowFavourite) _names.Add(modeName);
        else _names.Remove(modeName);

        try
        {
            await _repository.SaveAsync(_names, ct).ConfigureAwait(false);
        }
        catch
        {
            // Roll back, or the list shows a star that survived nowhere.
            if (nowFavourite) _names.Remove(modeName);
            else _names.Add(modeName);
            throw;
        }

        FavouriteChanged?.Invoke(this, new FavouriteChangedEventArgs(modeName, nowFavourite));
        return nowFavourite;
    }

    /// <summary>Unstars everything, persisting the result.</summary>
    public async Task ClearAsync(CancellationToken ct = default)
    {
        var cleared = _names.ToList();
        _names.Clear();

        try
        {
            await _repository.ClearAsync(ct).ConfigureAwait(false);
        }
        catch
        {
            foreach (var name in cleared) _names.Add(name);
            throw;
        }

        foreach (var name in cleared)
            FavouriteChanged?.Invoke(this, new FavouriteChangedEventArgs(name, false));
    }

    /// <summary>
    /// The starred modes drawn from <paramref name="modes"/>, in the order given.
    ///
    /// <para>
    /// Filters the caller's list rather than resolving <see cref="Names"/> against
    /// the registry, so a favourite whose mode has been renamed or removed simply
    /// does not appear — no lookup failure to handle, and a "Favourites" screen
    /// cannot show a row that cannot be started.
    /// </para>
    /// </summary>
    public IReadOnlyList<IGameMode> FilterFavourites(IEnumerable<IGameMode> modes)
    {
        ArgumentNullException.ThrowIfNull(modes);
        return modes.Where(IsFavourite).ToList().AsReadOnly();
    }

    /// <summary>
    /// The same modes with starred ones first, each group keeping its original
    /// relative order.
    ///
    /// <para>
    /// A stable ordering matters more than it looks: the picker is long, and a
    /// sort that reshuffled the unstarred remainder would move every row the
    /// moment a player stars anything.
    /// </para>
    /// </summary>
    public IReadOnlyList<IGameMode> FavouritesFirst(IEnumerable<IGameMode> modes)
    {
        ArgumentNullException.ThrowIfNull(modes);

        var list = modes.ToList();
        return list.Where(IsFavourite)
            .Concat(list.Where(m => !IsFavourite(m)))
            .ToList()
            .AsReadOnly();
    }
}

/// <summary>Reports which mode changed and what it changed to.</summary>
public sealed class FavouriteChangedEventArgs : EventArgs
{
    /// <summary>Creates the event payload.</summary>
    public FavouriteChangedEventArgs(string modeName, bool isFavourite)
    {
        ModeName = modeName;
        IsFavourite = isFavourite;
    }

    /// <summary>The mode that was starred or unstarred.</summary>
    public string ModeName { get; }

    /// <summary>True when the mode is now starred.</summary>
    public bool IsFavourite { get; }
}
