using System.Text.Json;
using TableTop.Presentation.Infrastructure;

namespace TableTop.WinUI.Infrastructure;

/// <summary>
/// Persists saved rosters for WinUI — a small JSON file in
/// <see cref="WinUIAppPaths.DataDirectory"/>, the same pattern
/// <see cref="WinUIAppSettings"/> and <c>JsonPlayerRepository</c> already
/// use. Its own file rather than a key inside <c>settings.json</c>, for the
/// same reason MAUI's <c>RosterStore</c> uses its own Preferences key: a
/// roster is free to grow its own shape without touching the settings
/// schema.
/// </summary>
public sealed class WinUIRosterStore : IRosterStore
{
    private static readonly string DefaultPath =
        Path.Combine(WinUIAppPaths.DataDirectory, "rosters.json");

    // Same reasoning as WinUIAppSettings: a plain lock, since this class is
    // fully synchronous, plus a unique-per-call temp filename — a shared
    // "rosters.json.tmp" let two overlapping Save() calls stomp each other.
    private readonly object _gate = new();

    private readonly string _filePath;

    /// <summary>Uses <paramref name="filePath"/> if given, otherwise a file in <see cref="WinUIAppPaths.DataDirectory"/>.</summary>
    public WinUIRosterStore(string? filePath = null) => _filePath = filePath ?? DefaultPath;

    /// <inheritdoc />
    public IReadOnlyList<SavedRoster> Load()
    {
        lock (_gate)
        {
            if (!File.Exists(_filePath)) return [];

            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<SavedRoster>>(json) ?? [];
            }
            catch (JsonException)
            {
                return [];   // corrupt file — start fresh rather than crash
            }
        }
    }

    /// <inheritdoc />
    public void Save(IReadOnlyList<SavedRoster> rosters)
    {
        lock (_gate)
        {
            var tmp = $"{_filePath}.{Guid.NewGuid():N}.tmp";
            try
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(tmp, JsonSerializer.Serialize(rosters));
                File.Move(tmp, _filePath, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Best-effort, same as WinUIAppSettings.Persist — a failed save
                // shouldn't crash the app, and both real causes (disk full,
                // permissions) are caught rather than just the first of them.
                // The cleanup is guarded for the same reason it is there: an
                // unguarded delete can throw the very exception types this
                // handler exists to absorb, straight out of a method documented
                // as best-effort.
                TryDeleteTemp(tmp);
            }
        }
    }

    /// <summary>
    /// Removes a leftover temp file without ever throwing. No
    /// <see cref="File.Exists(string)"/> check first: <see cref="File.Delete(string)"/>
    /// already no-ops on a missing file. A temp file that survives is harmless
    /// — it is uniquely named, so it collides with nothing.
    /// </summary>
    private static void TryDeleteTemp(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Nothing useful to do: the save has already failed and been
            // absorbed, and this is only tidy-up.
        }
    }
}
