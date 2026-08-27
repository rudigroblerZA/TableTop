namespace TableTop.Presentation.Infrastructure;

/// <summary>A named group of players, saved for reuse.</summary>
public sealed class SavedRoster
{
    /// <summary>The name the player gave this roster, or the template's name if they didn't.</summary>
    public required string Name { get; init; }

    /// <summary>The template this roster was built from.</summary>
    public required string TemplateName { get; init; }

    /// <summary>The players configured into this roster, in entry order.</summary>
    public required IReadOnlyList<SavedPlayer> Players { get; init; }

    /// <summary>"Team · 3 players", for the saved-rosters list. Not persisted — computed from <see cref="Players"/>.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string Subtitle => $"{TemplateName} · {Players.Count} player{(Players.Count == 1 ? "" : "s")}";
}

/// <summary>
/// Persists saved rosters. Each head keeps its own storage — MAUI through
/// <c>Microsoft.Maui.Storage.Preferences</c>, WinUI through a local JSON
/// file — the same split <see cref="IAppSettings"/> already draws, and for
/// the same reason: only the shape (and the ViewModel that builds it) is
/// worth sharing.
/// </summary>
public interface IRosterStore
{
    /// <summary>Loads every saved roster, or an empty list if none exist or the stored data is unreadable.</summary>
    IReadOnlyList<SavedRoster> Load();

    /// <summary>Persists the full current list of saved rosters, replacing whatever was there before.</summary>
    void Save(IReadOnlyList<SavedRoster> rosters);
}
