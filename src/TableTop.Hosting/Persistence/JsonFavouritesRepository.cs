using System.Text.Json;
using System.Text.Json.Serialization;

namespace TableTop.Hosting.Persistence;

/// <summary>
/// Persists starred mode names as a JSON file on the local file system — a
/// line-for-line sibling of <see cref="JsonRosterRepository"/> and
/// <see cref="JsonPlayerRepository"/>: a per-instance <see cref="SemaphoreSlim"/>,
/// a uniquely-named temp file, atomic replace, corrupt-file-tolerant load.
/// See <see cref="JsonSessionRepository"/>'s remarks for why the gate and the
/// unique temp name are both needed.
/// </summary>
public sealed class JsonFavouritesRepository : IFavouritesRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <param name="filePath">
    /// Path to the JSON file. Defaults to <c>favourites.json</c> next to the
    /// executable — a host that installs unelevated should pass an app-data
    /// path instead, the same as it does for the session, player and roster
    /// files.
    /// </param>
    public JsonFavouritesRepository(string? filePath = null)
    {
        _filePath = filePath
            ?? Path.Combine(AppContext.BaseDirectory, "favourites.json");
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> LoadAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (!File.Exists(_filePath))
                return [];

            try
            {
                await using var stream = File.OpenRead(_filePath);
                var names = await JsonSerializer.DeserializeAsync<List<string>>(
                    stream, SerializerOptions, ct).ConfigureAwait(false);

                // Null and blank entries are dropped here rather than downstream:
                // a hand-edited file is the realistic source of them, and every
                // consumer would otherwise need the same guard.
                return names?
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList()
                    .AsReadOnly() ?? (IReadOnlyList<string>)[];
            }
            catch (JsonException)
            {
                // Corrupted or pre-schema-change file — costs the stars, not a
                // crash on every launch.
                return [];
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(IEnumerable<string> modeNames, CancellationToken ct = default)
    {
        var list = modeNames.ToList();

        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var tmp = $"{_filePath}.{Guid.NewGuid():N}.tmp";

        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            try
            {
                await using (var stream = File.Create(tmp))
                    await JsonSerializer.SerializeAsync(stream, list, SerializerOptions, ct).ConfigureAwait(false);

                File.Move(tmp, _filePath, overwrite: true);
            }
            catch
            {
                // Best-effort, so a cleanup failure cannot replace the write
                // failure being rethrown below.
                TempFile.TryDelete(tmp);
                throw;
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }
        finally
        {
            _gate.Release();
        }
    }
}
