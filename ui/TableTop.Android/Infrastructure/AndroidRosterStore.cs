using System.Text.Json;
using Android.Content;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Droid.Infrastructure;

/// <summary>
/// <see cref="IRosterStore"/> backed by a single JSON string in
/// <see cref="ISharedPreferences"/> — ported from MAUI's
/// <c>Services/RosterStore.cs</c>. A corrupt or pre-schema-change value costs
/// the saved rosters, not a crash on every launch.
/// </summary>
public sealed class AndroidRosterStore : IRosterStore
{
    private const string PrefsName = "tabletop";
    private const string Key = "tt_saved_rosters";

    private readonly ISharedPreferences _prefs;

    /// <summary>Opens the shared-preferences file the roster list is stored in.</summary>
    public AndroidRosterStore(Context context) =>
        _prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private)!;

    /// <inheritdoc />
    public IReadOnlyList<SavedRoster> Load()
    {
        var raw = _prefs.GetString(Key, "")!;
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

    /// <inheritdoc />
    public void Save(IReadOnlyList<SavedRoster> rosters) =>
        _prefs.Edit()!.PutString(Key, JsonSerializer.Serialize(rosters))!.Apply();
}
