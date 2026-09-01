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
/// The Herd screen, shared by every head.
///
/// <para>
/// <b>The one thing that makes this mode different from Monogamy and Claimed!,
/// and the reason it needs its own care.</b>
/// <see cref="IHerdController.SubmitAnswers"/> raises
/// <see cref="IHerdController.RoundResolved"/> and then calls its own
/// "advance to next prompt" step <i>inside the same call</i>, which raises
/// either <see cref="IHerdController.PromptReady"/> or
/// <see cref="IHerdController.GameEnded"/> before <c>SubmitAnswers</c> returns.
/// This is the exact class of bug <see cref="MonogamyGameViewModel.Submit"/>
/// documents: WinUI's old Monogamy screen wiped <c>HasCard</c> after every
/// action because the controller's next event had already set it by the time
/// the action "finished". Here the fix is a strict separation of ownership:
/// <see cref="OnRoundResolved"/> only ever writes the <c>Last*</c> properties
/// below it, and <see cref="OnPromptReady"/> only ever writes the
/// current-round properties above it. Neither handler reads or clears a
/// property the other owns, so it does not matter which order they fire in
/// within one <see cref="Reveal"/> call.
/// </para>
/// </summary>
public sealed class HerdGameViewModel : ViewModelBase, IDisposable
{
    private readonly IHerdController? _controller;

    /// <summary>One entry per player, for a shared-device answer sheet.</summary>
    public ObservableCollection<PlayerAnswerEntry> PlayerAnswers { get; } = [];

    /// <summary>Submits every player's answer and reveals the round.</summary>
    public ICommand RevealCommand { get; }
    /// <summary>Hides the last round's result panel.</summary>
    public ICommand DismissLastRoundCommand { get; }
    /// <summary>Returns to the previous screen.</summary>
    public ICommand BackCommand { get; }

    // Current round — owned by OnPromptReady, read by Reveal(). Never touched by OnRoundResolved.
    private int _roundNumber, _totalRounds;
    private string _prompt = "", _category = "";

    // Last round's result — owned by OnRoundResolved. Never touched by OnPromptReady.
    private string _lastHerdAnswer = "", _lastLoneVoice = "", _lastRoundSummary = "";
    private bool _showingLastRound;

    private string _scores = "", _summary = "";
    private readonly string _loadError = "";
    private bool _isGameOver;

    /// <summary>1-based round number.</summary>
    public int RoundNumber { get => _roundNumber; private set => SetField(ref _roundNumber, value); }
    /// <summary>Total rounds in the session.</summary>
    public int TotalRounds { get => _totalRounds; private set => SetField(ref _totalRounds, value); }
    /// <summary>The current prompt.</summary>
    public string Prompt { get => _prompt; private set => SetField(ref _prompt, value); }
    /// <summary>The current prompt's category.</summary>
    public string Category { get => _category; private set => SetField(ref _category, value); }

    /// <summary>The most-given answer last round, or empty when nobody matched.</summary>
    public string LastHerdAnswer { get => _lastHerdAnswer; private set { SetField(ref _lastHerdAnswer, value); OnPropertyChanged(nameof(HasLastHerdAnswer)); } }
    /// <summary>True when last round had a herd answer.</summary>
    public bool HasLastHerdAnswer => _lastHerdAnswer.Length > 0;
    /// <summary>The lone voice last round, or empty when nobody stood alone.</summary>
    public string LastLoneVoice { get => _lastLoneVoice; private set { SetField(ref _lastLoneVoice, value); OnPropertyChanged(nameof(HasLastLoneVoice)); } }
    /// <summary>True when last round had a lone voice.</summary>
    public bool HasLastLoneVoice => _lastLoneVoice.Length > 0;
    /// <summary>Every answer group from last round, rendered for display.</summary>
    public string LastRoundSummary { get => _lastRoundSummary; private set => SetField(ref _lastRoundSummary, value); }
    /// <summary>True while the last round's result panel is showing.</summary>
    public bool ShowingLastRound { get => _showingLastRound; private set => SetField(ref _showingLastRound, value); }

    /// <summary>Running totals, rendered for display.</summary>
    public string Scores
    {
        get => _scores;
        private set { SetField(ref _scores, value); OnPropertyChanged(nameof(HasScores)); }
    }

    /// <summary>True once any round has been scored.</summary>
    public bool HasScores => _scores.Length > 0;

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
    public HerdGameViewModel(INavigator navigator, IHerdController controller)
    {
        _controller = controller;

        RevealCommand = new RelayCommand(Reveal);
        DismissLastRoundCommand = new RelayCommand(() => ShowingLastRound = false);
        BackCommand = new RelayCommand(() => { _controller?.Quit(); navigator.GoBack(); });

        _controller.PromptReady += OnPromptReady;
        _controller.RoundResolved += OnRoundResolved;
        _controller.GameEnded += OnGameEnded;

        if (!_controller.IsRunning) _controller.Start();
    }

    /// <summary>Error-state constructor: no controller, just a message.</summary>
    private HerdGameViewModel(INavigator navigator, string loadError)
    {
        _loadError = loadError;
        RevealCommand = new RelayCommand(() => { }, () => false);
        DismissLastRoundCommand = new RelayCommand(() => { });
        BackCommand = new RelayCommand(navigator.GoBack);
    }

    /// <summary>
    /// Builds the controller from a mode, surfacing a deck-load failure as a
    /// message rather than an exception — same reasoning as
    /// <see cref="MonogamyGameViewModel.CreateAsync"/>, including going
    /// through <see cref="IControllerFactory"/> rather than constructing
    /// <c>HerdController</c> directly.
    /// </summary>
    /// <param name="navigator">Used to leave the screen.</param>
    /// <param name="mode">The mode to play.</param>
    /// <param name="players">The players at the table.</param>
    /// <param name="controllerFactory">
    /// The host's factory. <b>Required</b> as of backlog X.2 — this was an
    /// optional parameter defaulting to <c>new ControllerFactory()</c>, which
    /// silently substituted a factory carrying none of the persistence,
    /// diagnostics or DI registration the host had configured. That default
    /// turned a forgotten argument into a behaviour change rather than a
    /// compile error, and it is how resume shipped broken on two heads (N.1).
    /// Pass <c>new ControllerFactory()</c> explicitly if plain defaults really
    /// are what you want.
    /// </param>
    public static async Task<HerdGameViewModel> CreateAsync(
        INavigator navigator,
        IGameMode mode,
        IReadOnlyList<IPlayer> players,
        IControllerFactory controllerFactory)
    {
        ArgumentNullException.ThrowIfNull(controllerFactory);

        try
        {
            var controller = await controllerFactory.CreateAsync(mode, players);
            if (controller is not IHerdController hc)
            {
                controller.Dispose();
                throw new NotSupportedException($"'{mode.Name}' isn't a Herd-style mode.");
            }

            return new HerdGameViewModel(navigator, hc);
        }
        catch (Exception ex)
        {
            return new HerdGameViewModel(navigator, ex.Message);
        }
    }

    // ── Controller events ─────────────────────────────────────────────────────

    private void OnPromptReady(object? sender, HerdPromptReadyEvent e)
    {
        RoundNumber = e.RoundNumber;
        TotalRounds = e.TotalRounds;
        Prompt = e.Prompt;
        Category = e.Category;

        PlayerAnswers.Clear();
        foreach (var name in _controller!.Scores.Keys)
            PlayerAnswers.Add(new PlayerAnswerEntry(name));
    }

    private void OnRoundResolved(object? sender, HerdRoundResolvedEvent e)
    {
        LastHerdAnswer = e.HerdAnswer ?? "";
        LastLoneVoice = e.LoneVoiceName ?? "";
        LastRoundSummary = string.Join("   ·   ", e.Groups.Select(g =>
            $"{g.Answer} ({g.PlayerNames.Count})"));
        ShowingLastRound = true;

        Scores = string.Join("   ·   ", _controller!.Scores.Select(kv => $"{kv.Key} {kv.Value}"));
    }

    private void OnGameEnded(object? sender, HerdGameEndedEvent e)
    {
        IsGameOver = true;
        Summary = e.WinnerNames.Count switch
        {
            0 => "Session ended.",
            1 => $"{e.WinnerNames[0]} wins after {e.RoundsPlayed} rounds.",
            _ => $"{string.Join(" & ", e.WinnerNames)} tie after {e.RoundsPlayed} rounds.",
        };
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    /// <summary>Submits every player's current answer and reveals the round.</summary>
    public void Reveal()
    {
        var answers = PlayerAnswers.ToDictionary(a => a.PlayerName, a => a.Answer);
        _controller?.SubmitAnswers(answers);
    }

    /// <summary>Hides the last round's result panel. Purely a display toggle — the controller has already advanced.</summary>
    public void DismissLastRound() => ShowingLastRound = false;

    /// <inheritdoc />
    public void Dispose() => _controller?.Dispose();

}

/// <summary>One player's answer for the current round.</summary>
public sealed class PlayerAnswerEntry(string playerName) : ViewModelBase
{
    private string _answer = "";

    /// <summary>The player this answer belongs to.</summary>
    public string PlayerName { get; } = playerName;

    /// <summary>What they've typed so far. Blank counts as no answer.</summary>
    public string Answer { get => _answer; set => SetField(ref _answer, value); }
}
