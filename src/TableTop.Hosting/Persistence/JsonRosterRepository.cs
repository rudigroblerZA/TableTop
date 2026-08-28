using System.Text.Json;
using System.Text.Json.Serialization;

namespace TableTop.Hosting.Persistence;

/// <summary>
/// Persists saved rosters as a JSON file on the local file system — a
/// line-for-line sibling of <see cref="JsonPlayerRepository"/>: a per-instance
/// <see cref="SemaphoreSlim"/>, a uniquely-named temp file, atomic replace,
/// corrupt-file-tolerant load. See <see cref="JsonSessionRepository"/>'s
/// remarks for why the gate and the unique temp name are both needed.
/// </summary>
public sealed class JsonRosterRepository : IRosterRepository
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
    /// Path to the JSON file. Defaults to <c>rosters.json</c> next to the
    /// executable — a host that installs unelevated should pass an app-data
    /// path instead, the same as it does for the session and player files.
    /// </param>
    public JsonRosterRepository(string? filePath = null)
    {
        _filePath = filePath
            ?? Path.Combine(AppContext.BaseDirectory, "rosters.json");
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RosterProfile>> LoadAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (!File.Exists(_filePath))
                return [];

            try
            {
                await using var stream = File.OpenRead(_filePath);
                var rosters = await JsonSerializer.DeserializeAsync<List<RosterProfile>>(
                    stream, SerializerOptions, ct).ConfigureAwait(false);
                return rosters?.AsReadOnly() ?? (IReadOnlyList<RosterProfile>)[];
            }
            catch (JsonException)
            {
                // Corrupted or pre-schema-change file — costs the saved
                // rosters, not a crash on every launch.
                return [];
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(IEnumerable<RosterProfile> rosters, CancellationToken ct = default)
    {
        var list = rosters.ToList();

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
                if (File.Exists(tmp)) File.Delete(tmp);
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
