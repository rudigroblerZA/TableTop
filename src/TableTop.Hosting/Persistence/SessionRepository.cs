using System.Text.Json;
using System.Text.Json.Serialization;

namespace TableTop.Hosting.Persistence;

/// <summary>
/// Alias kept for backward compatibility. Prefer <see cref="IGamePersistence"/> in new code.
/// </summary>
public interface ISessionRepository : IGamePersistence { }

/// <summary>
/// JSON file-backed session repository.
/// Writes to a temp file first to prevent corruption on crash (same pattern as JsonPlayerRepository).
/// </summary>
public sealed class JsonSessionRepository : IGamePersistence, ISessionRepository
{
    private readonly string _filePath;

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Absolute path to the JSON file where sessions are persisted.</summary>
    public string FilePath => _filePath;

    /// <summary>Initialises a new <see cref="JsonSessionRepository"/> instance.</summary>
    public JsonSessionRepository(string? filePath = null)
    {
        _filePath = filePath
            ?? Path.Combine(AppContext.BaseDirectory, "session.json");
    }

    /// <inheritdoc />
    public bool HasSavedSession => File.Exists(_filePath);

    /// <inheritdoc />
    public async Task SaveAsync(SessionSnapshot snapshot, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        snapshot.SavedAt = DateTimeOffset.UtcNow;

        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        var tmp = _filePath + ".tmp";
        await using (var stream = File.Create(tmp))
            await JsonSerializer.SerializeAsync(stream, snapshot, Options, ct).ConfigureAwait(false);

        File.Move(tmp, _filePath, overwrite: true);
    }

    /// <inheritdoc />
    public async Task<SessionSnapshot?> LoadAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_filePath)) return null;

        try
        {
            await using var stream = File.OpenRead(_filePath);
            var snapshot = await JsonSerializer.DeserializeAsync<SessionSnapshot>(stream, Options, ct).ConfigureAwait(false);

            if (snapshot is null) return null;

            // Reject snapshots written by a future schema version (forward-incompatible).
            // Snapshots from an older version trigger RequiresMigration but are returned
            // so the caller can decide whether to migrate or discard.
            if (snapshot.SchemaVersion > SessionSnapshot.CurrentSchemaVersion)
            {
                // Future schema — we don't know how to read this safely.
                return null;
            }

            return snapshot;
        }
        catch (JsonException)
        {
            return null; // corrupted snapshot — treat as no session
        }
    }

    /// <inheritdoc />
    public Task DeleteAsync(CancellationToken ct = default)
    {
        if (File.Exists(_filePath)) File.Delete(_filePath);
        return Task.CompletedTask;
    }
}