using System.Collections.ObjectModel;
using System.Windows.Input;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;
using TableTop.Hosting.Events;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Presentation.ViewModels;

/// <summary>
/// The Claimed! screen, shared by every head.
///
/// <para>
/// Same shape as <see cref="MonogamyGameViewModel"/> — controller-injected
/// public constructor for WinUI (which builds the controller centrally via
/// <c>ControllerFactory</c>), plus a mode-and-players <see cref="Create"/>
/// factory for MAUI that turns a deck-build failure into <see cref="LoadError"/>
/// instead of a crash.
/// </para>
///
/// <para>
/// Unlike Monogamy's dice roll, <see cref="IClaimedController.Start"/> raises no
/// event — the board exists the moment the controller is constructed, there is
/// simply nothing to announce. So the constructor calls <see cref="RefreshBoard"/>
/// once, synchronously, right after starting, instead of waiting on an event that
/// never comes.
/// </para>
///
/// <para>
/// <b>A second ordering hazard, same family as
/// <see cref="HerdGameViewModel"/>'s.</b> <see cref="IClaimedController.ResolveChallenge"/>
/// raises <see cref="IClaimedController.TerritoryClaimed"/> (or
/// <see cref="IClaimedController.TerritoryStolen"/>/<see cref="IClaimedController.ChallengeFailed"/>)
/// <i>before</i> it advances the turn — so <c>CurrentPlayerName</c> and
/// <c>TerritoryHolders</c> still describe the previous turn while one of those
/// handlers is running. Those handlers therefore only ever set
/// <see cref="Flash"/>; <see cref="RefreshBoard"/> runs once, in
/// <see cref="Resolve"/>, after <c>ResolveChallenge</c> has fully returned.
/// </para>
/// </summary>
public sealed class ClaimedGameViewModel : ViewModelBase, IDisposable
{
    private readonly IClaimedController? _controller;

    /// <summary>Every territory, its holder, and whether the current player may challenge it.</summary>
    public ObservableCollection<TerritoryOption> Territories { get; } = [];

    /// <summary>Returns to the previous screen.</summary>
    public ICommand BackCommand    { get; }
    /// <summary>Judges the pending challenge a success.</summary>
    public ICommand SucceedCommand { get; }
    /// <summary>Judges the pending challenge a failure.</summary>
    public ICommand FailCommand    { get; }

    private string _currentPlayerName = "", _flash = "", _summary = "";
    private string _pendingCardTitle = "", _pendingCardText = "", _pendingDifficulty = "";
    private string? _pendingDefenderName;
    private readonly string _loadError = "";
    private bool _hasPendingChallenge, _isRaid, _isGameOver;

    /// <summary>Whose turn it is.</summary>
    public string CurrentPlayerName { get => _currentPlayerName; private set => SetField(ref _currentPlayerName, value); }

    /// <summary>True while a drawn card is waiting for the table to judge it.</summary>
    public bool HasPendingChallenge { get => _hasPendingChallenge; private set => SetField(ref _hasPendingChallenge, value); }

    /// <summary>Title of the pending challenge card.</summary>
    public string PendingCardTitle { get => _pendingCardTitle; private set => SetField(ref _pendingCardTitle, value); }
    /// <summary>Body text of the pending challenge card.</summary>
    public string PendingCardText  { get => _pendingCardText;  private set => SetField(ref _pendingCardText, value); }
    /// <summary>Difficulty of the pending challenge card.</summary>
    public string PendingDifficulty { get => _pendingDifficulty; private set => SetField(ref _pendingDifficulty, value); }
    /// <summary>The defending player's name, or null when the territory is open ground.</summary>
    public string? PendingDefenderName { get => _pendingDefenderName; private set => SetField(ref _pendingDefenderName, value); }
    /// <summary>True when the pending challenge is a raid on a held territory rather than a claim on open ground.</summary>
    public bool IsRaid { get => _isRaid; private set => SetField(ref _isRaid, value); }

    /// <summary>Transient feedback after a challenge resolves.</summary>
    public string Flash { get => _flash; private set => SetField(ref _flash, value); }

    /// <summary>True once the session has ended.</summary>
    public bool IsGameOver
    {
        get => _isGameOver;
        private set { SetField(ref _isGameOver, value); OnPropertyChanged(nameof(IsPlaying)); }
    }

    /// <summary>End-of-game standings.</summary>
    public string Summary { get => _summary; private set => SetField(ref _summary, value); }

    /// <summary>True while the session is live and loadable.</summary>
    public bool IsPlaying => !IsGameOver && !HasLoadError;

    /// <summary>Deck-load failure message, or empty.</summary>
    public string LoadError => _loadError;

    /// <summary>True when the deck could not be loaded.</summary>
    public bool HasLoadError => !string.IsNullOrEmpty(_loadError);

    /// <summary>Builds the screen around an already-created controller.</summary>
    public ClaimedGameViewModel(INavigator navigator, IClaimedController controller)
    {
        _controller = controller;

        BackCommand    = new RelayCommand(() => { _controller?.Dispose(); navigator.GoBack(); });
        SucceedCommand = new RelayCommand(() => Resolve(true),  () => HasPendingChallenge);
        FailCommand    = new RelayCommand(() => Resolve(false), () => HasPendingChallenge);

        _controller.TerritoryChallengeReady += OnTerritoryChallengeReady;
        _controller.TerritoryClaimed        += OnTerritoryClaimed;
        _controller.TerritoryStolen         += OnTerritoryStolen;
        _controller.ChallengeFailed         += OnChallengeFailed;
        _controller.GameEnded               += OnGameEnded;

        if (!_controller.IsRunning) _controller.Start();
        RefreshBoard();
    }

    /// <summary>Error-state constructor: no controller, just a message.</summary>
    private ClaimedGameViewModel(INavigator navigator, string loadError)
    {
        _loadError     = loadError;
        BackCommand    = new RelayCommand(navigator.GoBack);
        SucceedCommand = new RelayCommand(() => { }, () => false);
        FailCommand    = new RelayCommand(() => { }, () => false);
    }

    /// <summary>
    /// Builds the controller from a mode, surfacing a deck-load failure as a
    /// message rather than an exception — same reasoning as
    /// <see cref="MonogamyGameViewModel.Create"/>.
    /// </summary>
    public static ClaimedGameViewModel Create(
        INavigator navigator, IGameMode mode, IReadOnlyList<IPlayer> players)
    {
        try
        {
            var provider = mode as IClaimedDeckProvider
                ?? throw new NotSupportedException($"'{mode.Name}' provides no Claimed! deck.");

            return new ClaimedGameViewModel(
                navigator,
                new ClaimedController(players, provider.GetClaimedDeck(), provider.WinningTerritoryCount));
        }
        catch (Exception ex)
        {
            return new ClaimedGameViewModel(navigator, ex.Message);
        }
    }

    // ── Controller events ─────────────────────────────────────────────────────

    private void OnTerritoryChallengeReady(object? sender, TerritoryChallengeReadyEvent e)
    {
        HasPendingChallenge = true;
        PendingCardTitle    = e.CardTitle;
        PendingCardText     = e.CardText;
        PendingDifficulty   = e.Difficulty;
        PendingDefenderName = e.DefenderName;
        IsRaid              = e.DefenderName is not null;
        RaiseActionCommands();
    }

    // These three fire from inside ResolveChallenge, BEFORE it calls
    // AdvanceTurnOrEnd — so CurrentPlayerName/TerritoryHolders are still the
    // PREVIOUS turn's values while a handler here is running. Same class of
    // ordering hazard as HerdGameViewModel's RoundResolved/PromptReady split:
    // these handlers only record what happened (Flash), never the board
    // state — Resolve() below refreshes the board once, after the controller
    // call fully returns and the turn has actually advanced.
    private void OnTerritoryClaimed(object? sender, TerritoryClaimedEvent e) =>
        Flash = $"{e.PlayerName} claims {e.TerritoryName}!";

    private void OnTerritoryStolen(object? sender, TerritoryStolenEvent e) =>
        Flash = $"{e.AttackerName} raids {e.TerritoryName} from {e.DefenderName}!";

    private void OnChallengeFailed(object? sender, ChallengeFailedEvent e) =>
        Flash = e.WasRaid
            ? $"{e.PlayerName}'s raid on {e.TerritoryName} fails."
            : $"{e.PlayerName} fails to claim {e.TerritoryName}.";

    private void OnGameEnded(object? sender, ClaimedGameEndedEvent e)
    {
        IsGameOver = true;
        var heldCount = e.FinalHoldings[e.WinnerNames[0]].Count;
        Summary = e.Reason == ClaimedEndReason.ThreeHeld
            ? $"{string.Join(" & ", e.WinnerNames)} wins by holding {heldCount} territories!"
            : $"The decks ran dry — {string.Join(" & ", e.WinnerNames)} held the most territory.";
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    /// <summary>Challenges a territory. No-op unless it's currently challengeable.</summary>
    public void Challenge(string territoryName)
    {
        if (HasPendingChallenge) return;
        _controller?.ChallengeTerritory(territoryName);
    }

    /// <summary>Judges the pending challenge a success. No-op when nothing is pending.</summary>
    public void Succeed() => Resolve(true);

    /// <summary>Judges the pending challenge a failure. No-op when nothing is pending.</summary>
    public void Fail() => Resolve(false);

    private void Resolve(bool succeeded)
    {
        if (!HasPendingChallenge || _controller is null) return;
        HasPendingChallenge = false;

        _controller.ResolveChallenge(succeeded);

        // Only now — after ResolveChallenge has fully returned — do
        // CurrentPlayerName and TerritoryHolders reflect the advanced turn.
        RefreshBoard();
        RaiseActionCommands();
    }

    /// <summary>
    /// Rebuilds <see cref="Territories"/> and <see cref="CurrentPlayerName"/>
    /// from the controller's current state. Called once up front (Start raises
    /// no event to hang this off) and again after every turn.
    /// </summary>
    private void RefreshBoard()
    {
        if (_controller is null) return;

        CurrentPlayerName = _controller.CurrentPlayerName;
        var challengeable = _controller.ChallengeableTerritories;

        Territories.Clear();
        foreach (var (name, holder) in _controller.TerritoryHolders)
            Territories.Add(new TerritoryOption(name, holder, challengeable.Contains(name), this));
    }

    private void RaiseActionCommands()
    {
        (SucceedCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (FailCommand    as RelayCommand)?.RaiseCanExecuteChanged();
    }

    /// <inheritdoc />
    public void Dispose() => _controller?.Dispose();

    /// <summary>One territory on the board.</summary>
    public sealed class TerritoryOption
    {
        private readonly ClaimedGameViewModel _owner;

        /// <summary>The territory's name (its deck category).</summary>
        public string Name { get; }
        /// <summary>The holder's name, or "Open" for unclaimed ground.</summary>
        public string HolderDisplay { get; }
        /// <summary>True when the current player may challenge this territory right now.</summary>
        public bool IsChallengeable { get; }

        /// <summary>Command that challenges this territory. WinUI binds this.</summary>
        public ICommand ChallengeCommand { get; }

        internal TerritoryOption(string name, string? holder, bool isChallengeable, ClaimedGameViewModel owner)
        {
            Name             = name;
            HolderDisplay    = holder ?? "Open";
            IsChallengeable  = isChallengeable;
            _owner           = owner;
            ChallengeCommand = new RelayCommand(() => owner.Challenge(name), () => isChallengeable && !owner.HasPendingChallenge);
        }

        /// <summary>Challenges this territory. Called directly by MAUI's buttons.</summary>
        public void Invoke() => _owner.Challenge(Name);
    }
}
