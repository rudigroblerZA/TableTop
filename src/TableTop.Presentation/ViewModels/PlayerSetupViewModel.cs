using System.Collections.ObjectModel;
using System.Windows.Input;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Players;
using TableTop.Hosting;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Presentation.ViewModels;

/// <summary>
/// The player-setup screen, shared by every head.
///
/// <para>
/// The two implementations described the same screen in different words:
/// <c>CurrentPlayerName</c>/<c>NewName</c>, <c>CurrentAge</c>/<c>NewAge</c>,
/// <c>GameMode</c>/<c>Mode</c>, <c>HasPrefilledPlayers</c>/<c>HasPlayers</c>.
/// Nothing about the behaviour differed — only the vocabulary — which is the
/// quiet kind of drift, because it never breaks anything and permanently
/// doubles the cost of every change.
/// </para>
///
/// <para>
/// WinUI's names are canonical here (<c>New*</c> for the pending entry) and
/// MAUI's XAML was rebound to match, rather than carrying alias properties
/// forever. Both heads keep everything they had: WinUI's commands and its
/// <see cref="Error"/> / <see cref="RosterStatus"/> feedback, and MAUI's
/// public methods, which its code-behind calls directly.
/// </para>
/// </summary>
public sealed class PlayerSetupViewModel : ViewModelBase
{
    private static readonly string[] GenderChoices = ["", "male", "female", "other"];

    private readonly IGameMode   _mode;
    private readonly IAppSettings _settings;

    private string _newName = "", _newAge = "", _selectedGender = "";
    private string _error = "", _rosterStatus = "";
    private bool   _newIsCouple;

    /// <summary>The players added so far.</summary>
    public ObservableCollection<PlayerEntry> Players { get; } = [];

    /// <summary>The mode being set up.</summary>
    public IGameMode Mode => _mode;

    /// <summary>Selectable gender values; empty string means unspecified.</summary>
    public IReadOnlyList<string> GenderOptions => GenderChoices;

    /// <summary>Adds the pending entry to <see cref="Players"/>.</summary>
    public ICommand AddPlayerCommand    { get; }
    /// <summary>Empties the roster.</summary>
    public ICommand ClearPlayersCommand { get; }
    /// <summary>Saves the roster as the remembered default.</summary>
    public ICommand SaveRosterCommand   { get; }
    /// <summary>Returns to the previous screen.</summary>
    public ICommand BackCommand         { get; }

    /// <summary>
    /// Validates the roster and hands the built players to the head.
    ///
    /// The validation is shared; the navigation that follows is not — WinUI
    /// swaps a ViewModel, MAUI pushes a Page. So the head supplies a callback
    /// and this owns only the part both do identically.
    /// </summary>
    public ICommand StartCommand        { get; }

    /// <summary>Name of the player being entered.</summary>
    public string NewName { get => _newName; set { SetField(ref _newName, value); RaiseAddState(); } }

    /// <summary>Age of the player being entered, as typed.</summary>
    public string NewAge { get => _newAge; set => SetField(ref _newAge, value); }

    /// <summary>Gender of the player being entered.</summary>
    public string SelectedGender { get => _selectedGender; set => SetField(ref _selectedGender, value); }

    /// <summary>Whether the player being entered is part of the couple.</summary>
    public bool NewIsCouple { get => _newIsCouple; set => SetField(ref _newIsCouple, value); }

    /// <summary>Validation message, or empty. Was WinUI-only.</summary>
    public string Error
    {
        get => _error;
        private set { SetField(ref _error, value); OnPropertyChanged(nameof(HasError)); }
    }

    /// <summary>
    /// True when there is a validation message.
    ///
    /// A computed bool rather than a string-to-visibility converter: WinUI has
    /// one, MAUI does not, and adding a second converter to a second head is
    /// how the two drift again. Both can bind a bool.
    /// </summary>
    public bool HasError => _error.Length > 0;

    /// <summary>Roster-save confirmation, or empty. Was WinUI-only.</summary>
    public string RosterStatus
    {
        get => _rosterStatus;
        private set { SetField(ref _rosterStatus, value); OnPropertyChanged(nameof(HasRosterStatus)); }
    }

    /// <summary>True when there is a save confirmation to show.</summary>
    public bool HasRosterStatus => _rosterStatus.Length > 0;

    /// <summary>True once at least one player has been added.</summary>
    public bool HasPlayers => Players.Count > 0;

    /// <summary>True when enough players are present to start.</summary>
    public bool CanStartGame => Players.Count >= MinimumPlayers;

    /// <summary>
    /// How many players this mode needs — asked of the mode, not hardcoded.
    ///
    /// WinUI already did this, with a comment worth preserving: personality
    /// quizzes are self-assessments and play fine with a single player, so a
    /// blanket "2" locks people out of modes that work alone. A first draft of
    /// this merge did hardcode it and would have regressed exactly that.
    /// </summary>
    public int MinimumPlayers => (_mode as IGameModeDefinition)?.MinimumPlayers ?? 2;

    /// <summary>Builds the screen.</summary>
    /// <param name="navigator">Used to leave the screen.</param>
    /// <param name="mode">The mode being set up.</param>
    /// <param name="settings">Settings store, for the remembered roster.</param>
    /// <param name="onStart">
    /// Invoked with the built players once validation passes. The head does its
    /// own navigation here.
    /// </param>
    public PlayerSetupViewModel(
        INavigator                             navigator,
        IGameMode                              mode,
        IAppSettings                           settings,
        Func<IReadOnlyList<IPlayer>, Task>?    onStart = null)
    {
        _mode     = mode;
        _settings = settings;
        _onStart  = onStart;

        AddPlayerCommand    = new RelayCommand(AddPlayer, () => NewName.Trim().Length > 0);
        ClearPlayersCommand = new RelayCommand(ClearPlayers);
        SaveRosterCommand   = new RelayCommand(SaveRosterAsDefault);
        BackCommand         = new RelayCommand(navigator.GoBack);
        StartCommand        = new AsyncRelayCommand(StartAsync, onError: ex => Error = ex.Message);

        // Prefill from the remembered roster. Saving is explicit in both heads
        // now — starting a game no longer overwrites it silently.
        foreach (var p in settings.RecentPlayers)
            Players.Add(new PlayerEntry(p.Name, p.Gender, p.Age, p.IsCoupleMember));

        Players.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasPlayers));
            OnPropertyChanged(nameof(CanStartGame));
        };
    }

    /// <summary>Adds the pending entry. No-op when the name is blank.</summary>
    public void AddPlayer()
    {
        var name = NewName.Trim();
        if (name.Length == 0) { Error = "Enter a name first."; return; }

        if (Players.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            Error = $"{name} is already on the list.";
            return;
        }

        int? age = int.TryParse(NewAge.Trim(), out var a) ? a : null;
        Players.Add(new PlayerEntry(
            name,
            SelectedGender.Length > 0 ? SelectedGender : null,
            age,
            NewIsCouple));

        NewName = NewAge = SelectedGender = "";
        NewIsCouple = false;
        Error = "";

        // Any roster change invalidates a previous deal: teams are dealt FROM
        // the roster, so a stale assignment would leave a new player with no
        // team while everyone else has one.
        ClearTeams();
    }

    /// <summary>Removes a player from the roster.</summary>
    public void RemovePlayer(PlayerEntry player)
    {
        Players.Remove(player);
        ClearTeams();   // see AddPlayer
    }

    /// <summary>Empties the roster.</summary>
    public void ClearPlayers()
    {
        Players.Clear();
        RosterStatus = "";
        ClearTeams();
    }

    /// <summary>
    /// True when the chosen mode is played in teams. Drives whether the setup
    /// screen should offer team assignment at all.
    /// </summary>
    public bool IsTeamMode => _mode is ITeamMode;

    /// <summary>How many teams this mode wants. Meaningless unless <see cref="IsTeamMode"/>.</summary>
    public int TeamCount => (_mode as ITeamMode)?.PreferredTeamCount ?? 0;

    /// <summary>Team assignments by player name, applied in <see cref="BuildPlayers"/>.</summary>
    private readonly Dictionary<string, string> _teamAssignments = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Returns the team a player has been assigned to, or null.</summary>
    public string? TeamFor(string playerName) =>
        _teamAssignments.TryGetValue(playerName, out var team) ? team : null;

    /// <summary>True once teams have been dealt.</summary>
    public bool HasTeams => _teamAssignments.Count > 0;

    /// <summary>Human-readable summary, e.g. "Red: Amy, Cara   ·   Blue: Ben, Dan".</summary>
    public string TeamSummary =>
        _teamAssignments.Count == 0
            ? string.Empty
            : string.Join("   ·   ", _teamAssignments
                .GroupBy(kv => kv.Value)
                .Select(g => $"{g.Key}: {string.Join(", ", g.Select(kv => kv.Key))}"));

    /// <summary>
    /// Deals the current roster into teams.
    ///
    /// <para>
    /// Without this, a team mode still ran — <c>TeamAlternatingPlayerManager</c>
    /// treats an unassigned player as a team of one, so nobody was skipped and
    /// the game stayed playable. But there were no actual sides, which for a
    /// mode like Rivals ("the other team picks your difficulty") leaves the
    /// central mechanic with nothing to point at. The engine supported teams
    /// before any screen could create one; this closes that gap.
    /// </para>
    /// </summary>
    /// <returns>True when teams were assigned; false when there aren't enough players yet.</returns>
    public bool AssignTeams()
    {
        var count = TeamCount;
        if (count < 2 || Players.Count < count) return false;

        var dealt = Teams.Deal(
            Players.Select(p => (IPlayer)new Player(Guid.NewGuid(), p.Name, null, null)).ToList(),
            count);

        _teamAssignments.Clear();
        foreach (var (player, team) in dealt)
            _teamAssignments[player.DisplayName] = team;

        RaiseTeamState();
        return true;
    }

    /// <summary>Clears any team assignment — used when the roster changes underneath it.</summary>
    public void ClearTeams()
    {
        if (_teamAssignments.Count == 0) return;
        _teamAssignments.Clear();
        RaiseTeamState();
    }

    private void RaiseTeamState()
    {
        OnPropertyChanged(nameof(TeamSummary));
        OnPropertyChanged(nameof(HasTeams));
    }

    /// <summary>
    /// Materialises the roster into engine players, carrying gender and age as
    /// attributes so gender-directed cards resolve correctly.
    /// </summary>
    public IReadOnlyList<IPlayer> BuildPlayers() =>
        Players.Select(p =>
        {
            var attrs = new Dictionary<string, string>();
            if (p.Gender is { Length: > 0 } g) attrs["gender"] = g;
            if (p.Age is { } age)              attrs["age"]    = age.ToString();

            // Team membership rides on Attributes rather than a property on
            // IPlayer — see Teams. Written only when the mode uses teams AND
            // they've been dealt, so nothing changes for the other 88 modes.
            if (TeamFor(p.Name) is { } team) attrs[Teams.AttributeKey] = team;

            var tags = new List<string>();
            if (p.IsCoupleMember) tags.Add("couple-member");
            if (p.Age is >= 18)   tags.Add("adult");

            return (IPlayer)new Player(Guid.NewGuid(), p.Name, attrs, tags);
        }).ToList();

    /// <summary>
    /// Saves the roster as the remembered default. Explicit only — starting a
    /// game does not do this, in either head.
    /// </summary>
    public void SaveRosterAsDefault()
    {
        _settings.RecentPlayers = Players
            .Select(p => new SavedPlayer(p.Name, p.Gender, p.Age, p.IsCoupleMember))
            .ToList();

        RosterStatus = Players.Count == 1
            ? "Saved 1 player."
            : $"Saved {Players.Count} players.";
    }

    private readonly Func<IReadOnlyList<IPlayer>, Task>? _onStart;

    /// <summary>
    /// Validates and starts. Sets <see cref="Error"/> and does nothing else when
    /// the roster is too small.
    /// </summary>
    public async Task StartAsync()
    {
        var need = MinimumPlayers;
        if (Players.Count < need)
        {
            Error = need == 1 ? "Add a player." : $"Add at least {need} players.";
            return;
        }

        var players     = BuildPlayers();
        var suitability = TableSuitability.Check(_mode, players);
        if (!suitability.Suits)
        {
            Error = suitability.Explanation!;
            return;
        }

        Error = "";
        if (_onStart is not null) await _onStart(players);
    }

    private void RaiseAddState() => (AddPlayerCommand as RelayCommand)?.RaiseCanExecuteChanged();

    /// <summary>One player on the setup list.</summary>
    public sealed class PlayerEntry
    {
        /// <summary>Display name.</summary>
        public string  Name           { get; }
        /// <summary>Gender, or null if unspecified.</summary>
        public string? Gender         { get; }
        /// <summary>Age, or null if unspecified.</summary>
        public int?    Age            { get; }
        /// <summary>Whether this player is part of the couple.</summary>
        public bool    IsCoupleMember { get; }

        /// <summary>First letter, for an avatar badge. Was WinUI-only.</summary>
        public string Initial => Name.Length > 0 ? Name[..1].ToUpperInvariant() : "?";

        /// <summary>Gender, age and couple status as one line, or empty.</summary>
        public string Detail
        {
            get
            {
                var bits = new List<string>();
                if (Gender is { Length: > 0 } g) bits.Add(g);
                if (Age is { } a)               bits.Add($"{a}");
                if (IsCoupleMember)             bits.Add("couple");
                return string.Join(" · ", bits);
            }
        }

        /// <summary>True when there is anything to show in <see cref="Detail"/>.</summary>
        public bool HasDetail => Detail.Length > 0;

        /// <summary>Creates an entry.</summary>
        public PlayerEntry(string name, string? gender, int? age, bool isCoupleMember = false)
        {
            Name = name; Gender = gender; Age = age; IsCoupleMember = isCoupleMember;
        }
    }
}
