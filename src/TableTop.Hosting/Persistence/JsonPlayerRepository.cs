using System.Text.Json;
using System.Text.Json.Serialization;

namespace TableTop.Hosting.Persistence;

/// <summary>
/// Persists player profiles as a JSON file on the local file system.
/// Thread-safe for sequential access (no concurrent write protection needed for this use case).
/// </summary>
public sealed class JsonPlayerRepository : IPlayerRepository
{
    private readonly string _filePath;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented          = true,
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

    /// <inheritdoc />
    public async Task SaveAsync(IEnumerable<PlayerProfile> profiles, CancellationToken ct = default)
    {
        var list = profiles.ToList();

        // Ensure directory exists (relevant when BaseDirectory is unusual)
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        // Write to a temp file first, then replace — avoids corruption on crash mid-write
        var tmp = _filePath + ".tmp";
        await using (var stream = File.Create(tmp))
            await JsonSerializer.SerializeAsync(stream, list, SerializerOptions, ct).ConfigureAwait(false);

        File.Move(tmp, _filePath, overwrite: true);
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken ct = default)
    {
        if (File.Exists(_filePath))
            File.Delete(_filePath);
        return Task.CompletedTask;
    }
}
