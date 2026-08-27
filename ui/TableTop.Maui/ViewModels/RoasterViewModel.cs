using System.Collections.ObjectModel;
using TableTop.Maui.Services;
using TableTop.Presentation.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui.ViewModels;

/// <summary>
/// A starting shape a roster can be built from — real rules, not just a name
/// and a blurb: how many players it needs, and whether configuring one under
/// this template tags players as a couple the way <c>PlayerSetupViewModel</c>
/// already does for <c>TableShape.Couple</c>-only modes.
/// </summary>
public sealed class RoasterTemplate
{
    /// <summary>Short name shown in the templates list, e.g. "Couple".</summary>
    public required string Name { get; init; }

    /// <summary>One-line description of who this template is for.</summary>
    public required string Description { get; init; }

    /// <summary>Fewest players a roster built from this template can save with.</summary>
    public required int MinPlayers { get; init; }

    /// <summary>Most players this template allows, or null for no ceiling.</summary>
    public int? MaxPlayers { get; init; }

    /// <summary>
    /// Whether players configured under this template are tagged as a
    /// couple — the same <c>IsCoupleMember</c> flag <see cref="SavedPlayer"/>
    /// already carries, which <c>TableSuitability</c> reads to decide whether
    /// a table suits a <c>TableShape.Couple</c>-only mode.
    /// </summary>
    public bool TagAsCouple { get; init; }

    /// <summary>"Needs exactly 2 players." / "Needs at least 2." / "Needs 3–40 players."</summary>
    public string RequirementText => (MinPlayers, MaxPlayers) switch
    {
        (var min, var max) when max == min => $"Needs exactly {min} player{(min == 1 ? "" : "s")}.",
        (var min, null) => $"Needs at least {min} player{(min == 1 ? "" : "s")}.",
        (var min, var max) => $"Needs {min}–{max} players.",
    };
}

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
/// Drives the Roaster screen's three columns: pick a template, configure its
/// players, save it alongside whatever rosters are already saved.
///
/// Saved rosters persist across app restarts via <see cref="RosterStore"/> —
/// the same MAUI Preferences mechanism <c>AppSettings</c> uses, under its own
/// key so it doesn't disturb the settings schema.
/// </summary>
public sealed class RoasterViewModel : BindableObject
{
    // Gender options match PlayerSetupViewModel's convention exactly — "" is
    // "unspecified", the same three named genders, so a roster's players are
    // real SavedPlayer records indistinguishable from ones entered at setup.
    private static readonly string[] GenderChoices = ["", "male", "female", "other"];

    /// <summary>The fixed set of starting shapes offered in the first column.</summary>
    public ObservableCollection<RoasterTemplate> Templates { get; } =
    [
        new() { Name = "Couple", Description = "Two players, tagged as a couple", MinPlayers = 2, MaxPlayers = 2, TagAsCouple = true },
        new() { Name = "Friends", Description = "A casual group of any size", MinPlayers = 2 },
        new() { Name = "Team", Description = "Two or more players split into sides", MinPlayers = 4 },
        new() { Name = "Class", Description = "A larger group, e.g. a classroom", MinPlayers = 3, MaxPlayers = 40 },
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
            RaiseConfigState();
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
    public ObservableCollection<SavedPlayer> ConfiguredPlayers { get; } = [];

    /// <summary>Gender choices for the player-entry picker — "" reads as unspecified.</summary>
    public IReadOnlyList<string> GenderOptions => GenderChoices;

    private string _newPlayerName = "";
    /// <summary>Bound to the "add a player" name entry.</summary>
    public string NewPlayerName
    {
        get => _newPlayerName;
        set { if (_newPlayerName == value) return; _newPlayerName = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanAddPlayer)); }
    }

    private string _newPlayerAge = "";
    /// <summary>Bound to the "add a player" age entry. Free text, parsed on add — a blank or non-numeric entry just means "unspecified".</summary>
    public string NewPlayerAge
    {
        get => _newPlayerAge;
        set { if (_newPlayerAge == value) return; _newPlayerAge = value; OnPropertyChanged(); }
    }

    private string _selectedGender = "";
    /// <summary>Bound to the "add a player" gender picker.</summary>
    public string SelectedGender
    {
        get => _selectedGender;
        set { if (_selectedGender == value) return; _selectedGender = value; OnPropertyChanged(); }
    }

    /// <summary>True while the roster being configured has fewer than its template's ceiling, and a name is entered.</summary>
    public bool CanAddPlayer =>
        IsConfiguring && NewPlayerName.Trim().Length > 0 &&
        (SelectedTemplate!.MaxPlayers is not { } max || ConfiguredPlayers.Count < max);

    /// <summary>
    /// Why <see cref="SaveRoster"/> can't run yet, or "" when it can — bound
    /// to a hint label under the save button so a player sees the rule they
    /// haven't met instead of a button that just silently refuses to do
    /// anything.
    ///
    /// Only checks the floor: <see cref="CanAddPlayer"/> already refuses to
    /// add past a template's ceiling, so <c>ConfiguredPlayers.Count</c>
    /// exceeding <c>MaxPlayers</c> here isn't a case this screen can reach —
    /// asserting for it anyway would be exactly the kind of check that reads
    /// as load-bearing while being structurally unable to fire.
    /// </summary>
    public string SaveBlockedReason
    {
        get
        {
            if (SelectedTemplate is not { } t) return "";
            if (ConfiguredPlayers.Count < t.MinPlayers)
                return $"{t.RequirementText} ({ConfiguredPlayers.Count} so far)";
            return "";
        }
    }

    /// <summary>True when the configured roster satisfies its template's player-count rule.</summary>
    public bool CanSaveRoster => IsConfiguring && SaveBlockedReason.Length == 0;

    /// <summary>Rosters saved so far, newest last. Loaded from <see cref="RosterStore"/> on construction.</summary>
    public ObservableCollection<SavedRoster> SavedRosters { get; }

    public RoasterViewModel()
    {
        SavedRosters = new ObservableCollection<SavedRoster>(RosterStore.Instance.Load());
        ConfiguredPlayers.CollectionChanged += (_, _) => RaiseConfigState();
    }

    private void RaiseConfigState()
    {
        OnPropertyChanged(nameof(CanAddPlayer));
        OnPropertyChanged(nameof(SaveBlockedReason));
        OnPropertyChanged(nameof(CanSaveRoster));
    }

    /// <summary>
    /// Adds a player built from <see cref="NewPlayerName"/>/<see cref="NewPlayerAge"/>/
    /// <see cref="SelectedGender"/> to the roster being configured, tagged as
    /// a couple when the template calls for it, then clears the entry fields.
    /// </summary>
    public void AddPlayer()
    {
        if (!CanAddPlayer) return;

        var name = NewPlayerName.Trim();
        int? age = int.TryParse(NewPlayerAge.Trim(), out var a) ? a : null;
        var gender = SelectedGender.Length > 0 ? SelectedGender : null;

        ConfiguredPlayers.Add(new SavedPlayer(name, gender, age, SelectedTemplate!.TagAsCouple));
        NewPlayerName = "";
        NewPlayerAge = "";
        SelectedGender = "";
    }

    /// <summary>Removes one player from the roster being configured.</summary>
    public void RemovePlayer(SavedPlayer player) => ConfiguredPlayers.Remove(player);

    /// <summary>
    /// Saves the roster being configured into <see cref="SavedRosters"/>,
    /// persists the full list via <see cref="RosterStore"/>, and clears the
    /// middle column back to "pick a template". Does nothing if
    /// <see cref="CanSaveRoster"/> is false — the player-count rule isn't
    /// negotiable from here, only from removing or adding players.
    /// </summary>
    public void SaveRoster()
    {
        if (!CanSaveRoster) return;

        var name = RoasterName.Trim();
        SavedRosters.Add(new SavedRoster
        {
            Name = name.Length > 0 ? name : SelectedTemplate!.Name,
            TemplateName = SelectedTemplate!.Name,
            Players = ConfiguredPlayers.ToList(),
        });
        RosterStore.Instance.Save(SavedRosters);

        SelectedTemplate = null;
    }

    /// <summary>Deletes a saved roster and persists the change.</summary>
    public void DeleteRoster(SavedRoster roster)
    {
        SavedRosters.Remove(roster);
        RosterStore.Instance.Save(SavedRosters);
    }
}
