using System.Text.Json;
using System.Text.Json.Serialization;

namespace TableTop.Hosting.Persistence;

/// <summary>
/// Alias kept for backward compatibility. Prefer <see cref="IGamePersistence"/> in new code.
/// </summary>
public interface ISessionRepository : IGamePersistence { }

/// <summary>
/// JSON file-backed session repository.
///
/// Writes to a uniquely-named temp file first, then replaces the real file
/// atomically (<see cref="File.Move(string, string, bool)"/> with overwrite,
/// which is an atomic rename on every platform this project targets) — so a
/// crash mid-write leaves either the old file or the new one, never a partial
/// one. A <see cref="SemaphoreSlim"/> per instance additionally serialises
/// every call: two overlapping <see cref="SaveAsync"/>s used to share the
/// exact same temp filename (<c>session.json.tmp</c>), so the second call's
/// <see cref="File.Create(string)"/> could truncate the first's still-open
/// stream, and whichever <see cref="File.Move(string, string, bool)"/> ran
/// second would fail — the source it expected had already been consumed by
/// the first. A unique-per-call temp filename alone would still let two
/// writers' renames race for "last one wins" in an unpredictable order; the
/// gate makes every save (and load/delete, so a save's in-flight write is
/// never read half-committed by a concurrent caller sharing the same
/// instance) run start-to-finish before the next one begins.
/// </summary>
public sealed class JsonSessionRepository : IGamePersistence, ISessionRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);

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

        // Unique per call — see the type's remarks on why a shared name isn't safe.
        var tmp = $"{_filePath}.{Guid.NewGuid():N}.tmp";

        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            try
            {
                await using (var stream = File.Create(tmp))
                    await JsonSerializer.SerializeAsync(stream, snapshot, Options, ct).ConfigureAwait(false);

                File.Move(tmp, _filePath, overwrite: true);
            }
            catch
            {
                // The rename either happened (nothing left to clean up) or
                // didn't — in which case this call's own uniquely-named temp
                // file is the only thing that could be left behind.
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
    public async Task<SessionSnapshot?> LoadAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
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
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (File.Exists(_filePath)) File.Delete(_filePath);
        }
        finally
        {
            _gate.Release();
        }
    }
}