using System.Text.Json;
using System.Text.Json.Serialization;

namespace TableTop.Hosting.Persistence;

/// <summary>
/// Persists player profiles as a JSON file on the local file system.
///
/// Guarded by a per-instance <see cref="SemaphoreSlim"/> and written through
/// a uniquely-named temp file — see <see cref="JsonSessionRepository"/>'s
/// remarks for why both are needed; this class had the identical gap (a
/// shared <c>players.tmp</c> name and no synchronisation between overlapping
/// calls) and gets the identical fix.
/// </summary>
public sealed class JsonPlayerRepository : IPlayerRepository
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
    /// Path to the JSON file. Defaults to <c>players.json</c> next to the executable.
    /// </param>
    public JsonPlayerRepository(string? filePath = null)
    {
        _filePath = filePath
            ?? Path.Combine(
                AppContext.BaseDirectory,
                "players.json");
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlayerProfile>> LoadAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (!File.Exists(_filePath))
                return [];

            try
            {
                await using var stream = File.OpenRead(_filePath);
                var profiles = await JsonSerializer.DeserializeAsync<List<PlayerProfile>>(
                    stream, SerializerOptions, ct).ConfigureAwait(false);
                return profiles?.AsReadOnly() ?? (IReadOnlyList<PlayerProfile>)[];
            }
            catch (JsonException)
            {
                // Corrupted file — return empty rather than crashing
                return [];
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(IEnumerable<PlayerProfile> profiles, CancellationToken ct = default)
    {
        var list = profiles.ToList();

        // Ensure directory exists (relevant when BaseDirectory is unusual)
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        // Unique per call, not shared — two overlapping saves used to both
        // target "players.json.tmp" and could stomp each other's write.
        var tmp = $"{_filePath}.{Guid.NewGuid():N}.tmp";

        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            try
            {
                // Write to a temp file first, then replace atomically — avoids
                // corruption on crash mid-write.
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
