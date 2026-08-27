using System.Text.Json;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Maui.Services;

/// <summary>
/// Persists saved rosters via MAUI Preferences, JSON-encoded under its own
/// key — separate from <see cref="AppSettings"/>'s schema, so a roster is
/// free to grow its own shape without touching the settings surface that
/// <c>IAppSettings</c> commits to across both heads.
///
/// Singleton for the same reason <see cref="AppSettings.Instance"/> is:
/// <c>Preferences</c> is itself process-wide storage, so one in-memory owner
/// of "the current saved-rosters list" avoids two independent reads/writes
/// racing each other within a session.
/// </summary>
public sealed class RosterStore : IRosterStore
{
    private const string Key = "tt_saved_rosters";

    public static RosterStore Instance { get; } = new();
    private RosterStore() { }

    /// <summary>
    /// Loads every saved roster, or an empty list if none have been saved yet
    /// or the stored JSON is unreadable — a corrupt or pre-schema-change
    /// value costs the saved rosters, not a crash on every launch.
    /// </summary>
    public IReadOnlyList<SavedRoster> Load()
    {
        var raw = Preferences.Get(Key, "");
        if (string.IsNullOrWhiteSpace(raw)) return [];

        try
        {
            return JsonSerializer.Deserialize<List<SavedRoster>>(raw) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    /// <summary>Persists the full current list of saved rosters, replacing whatever was there before.</summary>
    public void Save(IReadOnlyList<SavedRoster> rosters) =>
        Preferences.Set(Key, JsonSerializer.Serialize(rosters));
}
