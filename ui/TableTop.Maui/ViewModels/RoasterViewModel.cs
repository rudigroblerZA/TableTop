using System.Collections.ObjectModel;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Maui.ViewModels;

/// <summary>A starting shape a roster can be built from.</summary>
public sealed class RoasterTemplate
{
    /// <summary>Short name shown in the templates list, e.g. "Couple".</summary>
    public required string Name { get; init; }

    /// <summary>One-line description of who this template is for.</summary>
    public required string Description { get; init; }
}

/// <summary>A named group of players, saved for reuse.</summary>
public sealed class SavedRoster
{
    /// <summary>The name the player gave this roster, or the template's name if they didn't.</summary>
    public required string Name { get; init; }

    /// <summary>The template this roster was built from.</summary>
    public required string TemplateName { get; init; }

    /// <summary>The players configured into this roster, in entry order.</summary>
    public required IReadOnlyList<string> PlayerNames { get; init; }

    /// <summary>"Team · 3 players", for the saved-rosters list.</summary>
    public string Subtitle => $"{TemplateName} · {PlayerNames.Count} player{(PlayerNames.Count == 1 ? "" : "s")}";
}

/// <summary>
/// Drives the Roaster screen's three columns: pick a template, configure its
/// players, save it alongside whatever rosters are already saved.
///
/// In-memory only for now — nothing here persists across app restarts or
/// reads from <c>IAppSettings.RecentPlayers</c>. That is the natural next
/// step once the shape of the feature is settled; this is deliberately just
/// the shape.
/// </summary>
public sealed class RoasterViewModel : BindableObject
{
    /// <summary>The fixed set of starting shapes offered in the first column.</summary>
    public ObservableCollection<RoasterTemplate> Templates { get; } =
    [
        new() { Name = "Couple", Description = "Two players, tagged as a couple" },
        new() { Name = "Friends", Description = "A casual group of any size" },
        new() { Name = "Team", Description = "Two or more players split into sides" },
        new() { Name = "Class", Description = "A larger group, e.g. a classroom" },
    ];

    private RoasterTemplate? _selectedTemplate;

    /// <summary>The template currently being configured, or null before one is picked.</summary>
    public RoasterTemplate? SelectedTemplate
    {
        get => _selectedTemplate;
        set
        {
            if (_selectedTemplate == value) return;
            _selectedTemplate = value;
            RoasterName = value?.Name ?? "";
            ConfiguredPlayers.Clear();
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsConfiguring));
            OnPropertyChanged(nameof(IsNotConfiguring));
        }
    }

    /// <summary>True once a template is picked — the middle column shows its configuration controls.</summary>
    public bool IsConfiguring => SelectedTemplate is not null;

    /// <summary>The inverse of <see cref="IsConfiguring"/>, for the "pick a template" hint.</summary>
    public bool IsNotConfiguring => SelectedTemplate is null;

    private string _roasterName = "";

    /// <summary>Editable name for the roster being configured. Defaults to the template's name.</summary>
    public string RoasterName
    {
        get => _roasterName;
        set
        {
            if (_roasterName == value) return;
            _roasterName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Players added to the roster being configured, in entry order.</summary>
    public ObservableCollection<string> ConfiguredPlayers { get; } = [];

    private string _newPlayerName = "";

    /// <summary>Bound to the "add a player" entry field.</summary>
    public string NewPlayerName
    {
        get => _newPlayerName;
        set
        {
            if (_newPlayerName == value) return;
            _newPlayerName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Rosters saved this session, newest last.</summary>
    public ObservableCollection<SavedRoster> SavedRosters { get; } = [];

    /// <summary>Adds <see cref="NewPlayerName"/> to the roster being configured, then clears the field.</summary>
    public void AddPlayer()
    {
        var name = NewPlayerName.Trim();
        if (name.Length == 0) return;
        ConfiguredPlayers.Add(name);
        NewPlayerName = "";
    }

    /// <summary>Removes one player from the roster being configured.</summary>
    public void RemovePlayer(string name) => ConfiguredPlayers.Remove(name);

    /// <summary>
    /// Saves the roster being configured into <see cref="SavedRosters"/> and
    /// clears the middle column back to "pick a template".
    /// </summary>
    public void SaveRoster()
    {
        if (SelectedTemplate is null || ConfiguredPlayers.Count == 0) return;

        var name = RoasterName.Trim();
        SavedRosters.Add(new SavedRoster
        {
            Name = name.Length > 0 ? name : SelectedTemplate.Name,
            TemplateName = SelectedTemplate.Name,
            PlayerNames = ConfiguredPlayers.ToList(),
        });

        SelectedTemplate = null;
    }
}
