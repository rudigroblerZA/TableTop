using System.Collections.ObjectModel;
using System.Windows.Input;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Hosting;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;
using TableTop.Hosting.Events;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Presentation.ViewModels;

/// <summary>
/// The gameplay screen, shared by every head — the last real duplication:
/// 733 lines on MAUI (<c>GameplayViewModel</c>), 404 on WinUI
/// (<c>CardTurnGameViewModel</c> in <c>GameViewModels.cs</c>), both driving
/// the same <see cref="ICardTurnController"/> and the highest-traffic screen
/// in the app.
///
/// <para>
/// <b>Union of both, same as every merge before it.</b> WinUI had no timer at
/// all — MAUI's countdown moves here and both heads gain it. WinUI had no
/// constructor error handling — a controller that failed to build took the
/// whole app down instead of showing why, the identical shape of bug already
/// found and fixed for Monogamy, Millionaire and Day One when each of
/// <i>those</i> screens was merged; this is the same class of bug in the
/// fourth and largest screen, not a new one. WinUI had no hint urgency tier —
/// MAUI's three-level <see cref="HintUrgency"/> moves here too.
/// </para>
///
/// <para>
/// <b>What deliberately did not move.</b> MAUI's <c>Theme</c>,
/// <c>DisplayFont</c>/<c>BodyFont</c>/<c>UtilityFont</c>, and the
/// WCAG-contrast-checked <c>StripColor</c>/<c>StripTextColor</c> all return
/// platform <c>Color</c> types this project cannot reference. What moved
/// instead is the data those colours are computed from:
/// <see cref="CardCategory"/> and the category-colour map implicit in each
/// mode's <c>CategoryColours</c>, which MAUI's own wrapper can still read
/// directly since it isn't gone, only not duplicated here. WinUI's screen
/// currently renders no per-category strip at all, so nothing there needed a
/// counterpart.
/// </para>
///
/// <para>
/// <b>Choice cards: both surfaces kept, same duality as Millionaire's
/// <c>AnswerOption</c>.</b> WinUI bound an <see cref="ObservableCollection{T}"/>
/// of <see cref="ChoiceItem"/>s to a dynamic list; MAUI's XAML has four fixed
/// buttons whose code-behind calls <see cref="RecordChoice"/> with a literal
/// letter and never touched a collection at all. Both routes end at the same
/// tally logic — the collection is populated from the same
/// <see cref="OnCardReady"/> that drives everything else, so keeping both
/// costs nothing and let neither head's XAML need restructuring around a
/// shape it didn't already use.
/// </para>
/// </summary>
public sealed class CardTurnGameViewModel : ViewModelBase, IDisposable
{
    private readonly INavigator _navigator;
    private readonly ICardTurnController? _controller;
    private readonly string _loadError = "";
    private readonly Dictionary<string, Dictionary<char, int>> _tallies = new();
    private readonly IReadOnlyDictionary<char, string> _styleNames;
    private readonly bool _timerEnabled;
    private CancellationTokenSource? _timerCts;

    private string _playerName = "", _cardTitle = "", _cardCategory = "", _cardDifficulty = "";
    private string _frontText = "", _flashText = "", _summaryText = "", _cardCountText = "";
    private string? _backText;
    private bool _isFlipped, _isGameOver;
    private int _round, _played, _secondsRemaining;
    private string _hintText = "", _hintUrgency = "Gentle";
    private bool _canUndo;
    private IReadOnlyList<ScoreEntry> _lastScores = [];

    // ── Bindable state ───────────────────────────────────────────────────────

    /// <summary>Current player's display name.</summary>
    public string PlayerName { get => _playerName; private set => SetField(ref _playerName, value); }

    /// <summary>Round number reported by the engine.</summary>
    public int Round { get => _round; private set => SetField(ref _round, value); }

    /// <summary>Cards left in the engine's deck.</summary>
    public int CardsRemaining => _controller?.CardsRemaining ?? 0;

    /// <summary>Fraction of the session played, 0–1. 0 when nothing has happened yet.</summary>
    public double Progress => _played + CardsRemaining == 0 ? 0 : (double)_played / (_played + CardsRemaining);

    /// <summary>"Round 3  ·  19 cards left".</summary>
    public string CardCountText { get => _cardCountText; private set => SetField(ref _cardCountText, value); }

    /// <summary>Whether to show <see cref="CardCountText"/>.</summary>
    public bool ShowCardCount { get; }

    /// <summary>Card title.</summary>
    public string CardTitle { get => _cardTitle; private set => SetField(ref _cardTitle, value); }
    /// <summary>Card category label — the data a per-mode colour strip is computed from, not the colour itself.</summary>
    public string CardCategory { get => _cardCategory; private set => SetField(ref _cardCategory, value); }
    /// <summary>Card difficulty label.</summary>
    public string CardDifficulty { get => _cardDifficulty; private set => SetField(ref _cardDifficulty, value); }

    /// <summary>Live scoreboard line, e.g. "Bob 3  ·  Alice 2".</summary>
    public string ScoresText { get; private set; } = "";
    /// <summary>True once any score has been recorded.</summary>
    public bool HasScores => ScoresText.Length > 0;

    /// <summary>
    /// Scores as bindable rows, so a view can render brass pips instead of a
    /// string. <see cref="ScoresText"/> is kept alongside — the game-over
    /// summary and any head that wants a plain line still use it.
    /// </summary>
    public ObservableCollection<ScoreRow> Scores { get; } = [];

    /// <summary>Transient feedback line (points won, skips, etc.).</summary>
    public string FlashText { get => _flashText; private set { SetField(ref _flashText, value); OnPropertyChanged(nameof(HasFlash)); } }
    /// <summary>True when there is a flash line to show.</summary>
    public bool HasFlash => _flashText.Length > 0;

    /// <summary>Final standings text once the game ends.</summary>
    public string SummaryText { get => _summaryText; private set => SetField(ref _summaryText, value); }
    /// <summary>True once the engine has ended the game.</summary>
    public bool IsGameOver { get => _isGameOver; private set { SetField(ref _isGameOver, value); OnPropertyChanged(nameof(IsPlaying)); RaiseActionState(); } }
    /// <summary>True while the session is live and loadable.</summary>
    public bool IsPlaying => !IsGameOver && !HasLoadError;

    /// <summary>
    /// Raised once with the compiled final summary. MAUI's page subscribes to
    /// navigate on it; a head that would rather show the summary in place can
    /// ignore it and bind <see cref="SummaryText"/>/<see cref="IsGameOver"/>
    /// instead — both are set before this fires.
    /// </summary>
    public event Action<string>? GameOver;

    /// <summary>Controller-build failure message, or empty. Was MAUI-only — WinUI took the whole app down instead.</summary>
    public string LoadError => _loadError;
    /// <summary>True when the mode could not be started.</summary>
    public bool HasLoadError => _loadError.Length > 0;

    // ── Hint ──────────────────────────────────────────────────────────────────

    /// <summary>The engine's next-turn hint, or empty when there is none.</summary>
    public string HintText { get => _hintText; private set { SetField(ref _hintText, value); OnPropertyChanged(nameof(HasHint)); } }
    /// <summary>True when a hint is worth showing.</summary>
    public bool HasHint => _hintText.Length > 0;

    /// <summary>Gentle, Moderate or Strong — was MAUI-only; WinUI showed hint text with no urgency tier at all.</summary>
    public string HintUrgency { get => _hintUrgency; private set { SetField(ref _hintUrgency, value); OnPropertyChanged(nameof(HintColor)); } }

    /// <summary>Hex colour for <see cref="HintUrgency"/> — data, not a platform <c>Color</c>, for the same reason the strip colours don't live here.</summary>
    public string HintColor => _hintUrgency switch
    {
        "Strong" => "#EF4444",
        "Moderate" => "#F59E0B",
        _ => "#C49E4C",
    };

    // ── Undo / Save ───────────────────────────────────────────────────────────

    /// <summary>True once a turn has been recorded and not yet undone. The controller has no CanUndo of its own.</summary>
    public bool CanUndo { get => _canUndo; private set { SetField(ref _canUndo, value); RaiseActionState(); } }
    /// <summary>True when a session is running and can be saved.</summary>
    public bool CanSave => _controller is not null && !IsGameOver;

    /// <summary>Reverses the last turn and re-presents its card.</summary>
    public ICommand UndoCommand { get; }
    /// <summary>Saves the session so it can be resumed.</summary>
    public ICommand SaveCommand { get; }

    // ── Flip ──────────────────────────────────────────────────────────────────

    /// <summary>True when the answer face is currently showing.</summary>
    public bool IsFlipped => _isFlipped;
    /// <summary>True when this card has a hidden answer face.</summary>
    public bool HasBack => _backText is not null;
    /// <summary>The text of the currently visible face.</summary>
    public string CardBodyText => _isFlipped && _backText is not null ? _backText : _frontText;
    /// <summary>Flip button caption for the current face.</summary>
    public string FlipButtonText => _isFlipped ? "Back to question" : "Reveal answer";
    /// <summary>Flips a two-faced card. WinUI binds this; MAUI's code-behind calls it directly.</summary>
    public ICommand FlipCommand { get; }

    // ── Choice cards (A–D quiz) ───────────────────────────────────────────────

    /// <summary>A–D quiz choices for choice cards; empty otherwise. WinUI binds this.</summary>
    public ObservableCollection<ChoiceItem> Choices { get; } = [];
    /// <summary>True when the current card is a tap-a-letter quiz card.</summary>
    public bool HasChoices => Choices.Count > 0;
    /// <summary>True for an ordinary card with no letter choices.</summary>
    public bool HasNoChoices => !HasChoices;

    // ── Actions ───────────────────────────────────────────────────────────────

    /// <summary>Positive-outcome button label, mode-specific.</summary>
    public string CompleteLabel { get; }
    /// <summary>Skip button label, mode-specific.</summary>
    public string SkipLabel { get; }
    /// <summary>The mode's display title, JSON override applied.</summary>
    public string ModeTitle { get; }

    /// <summary>Records a completed card.</summary>
    public ICommand CompleteCommand { get; }
    /// <summary>Records a skipped card.</summary>
    public ICommand SkipCommand { get; }
    /// <summary>Leaves the game (quits the controller first).</summary>
    public ICommand QuitCommand { get; }

    /// <summary>Nudge difficulty and pace for everyone at the table.</summary>
    public ICommand LevelUpCommand { get; }
    /// <inheritdoc cref="LevelUpCommand" />
    public ICommand LevelDownCommand { get; }
    /// <inheritdoc cref="LevelUpCommand" />
    public ICommand SpeedUpCommand { get; }
    /// <inheritdoc cref="LevelUpCommand" />
    public ICommand SlowDownCommand { get; }

    /// <summary>False for modes whose progression strategy isn't flow-aware.</summary>
    public bool SupportsFlow => _controller?.SupportsFlow ?? false;

    // ── Timer — new to WinUI; MAUI has had it all along ─────────────────────

    /// <summary>Whether a per-card countdown runs.</summary>
    public bool TimerEnabled => _timerEnabled;
    /// <summary>Seconds left on the current card's timer.</summary>
    public int SecondsRemaining
    {
        get => _secondsRemaining;
        private set { SetField(ref _secondsRemaining, value); OnPropertyChanged(nameof(TimerDisplay)); OnPropertyChanged(nameof(TimerExpired)); }
    }
    /// <summary>"MM:SS" for the countdown.</summary>
    public string TimerDisplay => $"{SecondsRemaining / 60:D2}:{SecondsRemaining % 60:D2}";
    /// <summary>True once the timer has run out.</summary>
    public bool TimerExpired => TimerEnabled && SecondsRemaining == 0;

    // ── Construction ─────────────────────────────────────────────────────────

    /// <summary>
    /// Builds the screen around an already-created controller. Errors during
    /// controller creation are the caller's problem to surface — use
    /// <see cref="CreateAsync"/> to build the controller too and get a
    /// <see cref="LoadError"/> instead of an exception.
    /// </summary>
    public CardTurnGameViewModel(
        INavigator navigator, IGameMode mode, ICardTurnController controller,
        bool timerEnabled, int timerSeconds, bool showCardCount)
    {
        _navigator = navigator;
        _controller = controller;
        _timerEnabled = timerEnabled;
        _secondsRemaining = timerSeconds;
        ShowCardCount = showCardCount;

        var def = mode as TableTop.Games.Base.BaseGameModeDefinition;
        CompleteLabel = def?.CompleteLabel ?? "Completed";
        SkipLabel = def?.SkipLabel ?? "Skip";
        ModeTitle = def?.Name ?? mode.Name;

        _styleNames = mode is IGameModeDefinition gmd
            ? ChoiceCards.ExtractStyleNames(gmd.GetCards([]).Select(c => c.Description))
            : new Dictionary<char, string>();

        CompleteCommand = new RelayCommand(() => Complete(), () => IsPlaying);
        SkipCommand = new RelayCommand(() => Skip(), () => IsPlaying);
        QuitCommand = new RelayCommand(() => Quit());
        SaveCommand = new RelayCommand(() => SaveSession(), () => CanSave);
        UndoCommand = new RelayCommand(() => UndoLastTurn(), () => CanUndo);
        FlipCommand = new RelayCommand(() => FlipCard(), () => HasBack);
        LevelUpCommand = new RelayCommand(() => LevelUp(), () => SupportsFlow);
        LevelDownCommand = new RelayCommand(() => LevelDown(), () => SupportsFlow);
        SpeedUpCommand = new RelayCommand(() => SpeedUp(), () => SupportsFlow);
        SlowDownCommand = new RelayCommand(() => SlowDown(), () => SupportsFlow);

        _controller.CardReady += OnCardReady;
        _controller.TurnResult += OnTurnResult;
        _controller.TurnSkipped += OnTurnSkipped;
        _controller.SkipAttempted += OnSkipAttempted;
        _controller.TurnUndone += OnTurnUndone;
        _controller.GameEnded += OnGameEnded;
        _controller.SessionSaved += (_, _) => FlashText = "Session saved";
        _controller.NextTurnHint += (_, e) => { HintText = e.HintText; HintUrgency = e.Urgency; };

        _controller.Start(); // fires the first CardReady synchronously
    }

    /// <summary>Error-state constructor: no controller, just a message.</summary>
    private CardTurnGameViewModel(INavigator navigator, string loadError, bool showCardCount)
    {
        _navigator = navigator;
        _loadError = loadError;
        ShowCardCount = showCardCount;
        CompleteLabel = "Completed";
        SkipLabel = "Skip";
        ModeTitle = "";
        _styleNames = new Dictionary<char, string>();

        CompleteCommand = new RelayCommand(() => { }, () => false);
        SkipCommand = new RelayCommand(() => { }, () => false);
        QuitCommand = new RelayCommand(() => navigator.GoBack());
        SaveCommand = new RelayCommand(() => { }, () => false);
        UndoCommand = new RelayCommand(() => { }, () => false);
        FlipCommand = new RelayCommand(() => { }, () => false);
        LevelUpCommand = new RelayCommand(() => { }, () => false);
        LevelDownCommand = new RelayCommand(() => { }, () => false);
        SpeedUpCommand = new RelayCommand(() => { }, () => false);
        SlowDownCommand = new RelayCommand(() => { }, () => false);
    }

    /// <summary>
    /// Builds the controller from a mode and settings, surfacing a build
    /// failure as <see cref="LoadError"/> rather than an exception — MAUI's
    /// behaviour, which WinUI's constructor lacked entirely.
    /// </summary>
    public static async Task<CardTurnGameViewModel> CreateAsync(
        INavigator navigator, IGameMode mode, IReadOnlyList<IPlayer> players, IAppSettings settings,
        TableTop.Hosting.Persistence.SessionSnapshot? resumeFrom = null,
        IControllerFactory? controllerFactory = null)
    {
        try
        {
            var gameplayOptions = new GameplayOptions
            {
                ShuffleDeck = settings.ShuffleCards,
                MinDifficulty = (Difficulty)(settings.MinDifficulty + 1),
                MaxDifficulty = (Difficulty)(settings.MaxDifficulty + 1),
                CardsPerPlayer = settings.CardsPerPlayer > 0 ? settings.CardsPerPlayer : null,
            };

            var controller = await (controllerFactory ?? new ControllerFactory()).CreateAsync(
                mode, players, maxRounds: 10, gameplayOptions: gameplayOptions, resumeFrom: resumeFrom);

            if (controller is not ICardTurnController ctc)
            {
                controller.Dispose();
                throw new NotSupportedException(
                    $"'{mode.Name}' uses a specialised controller that this screen doesn't drive.");
            }

            return new CardTurnGameViewModel(
                navigator, mode, ctc, settings.EnableTimer, settings.TimerSeconds, settings.ShowCardCount);
        }
        catch (Exception ex)
        {
            return new CardTurnGameViewModel(navigator, ex.Message, settings.ShowCardCount);
        }
    }

    // ── Controller events ────────────────────────────────────────────────────

    private void OnCardReady(object? sender, CardReadyEvent e)
    {
        // A hint describes the turn just dealt, so it clears when the next
        // card arrives rather than lingering over someone else's turn.
        HintText = "";

        PlayerName = e.PlayerName;
        Round = e.Round;
        CardCountText = $"Round {e.Round}  ·  {_controller?.CardsRemaining ?? 0} cards left";
        CardTitle = e.CardTitle;
        CardCategory = e.Category;
        CardDifficulty = e.Card.Difficulty.ToString();

        var plain = CardText.StripHtml(e.CardText);
        var (front, back) = CardFaces.Split(plain);
        _frontText = front;
        _backText = back;
        _isFlipped = false;

        Choices.Clear();
        foreach (var (letter, text) in ChoiceCards.Extract(e.CardText))
            Choices.Add(new ChoiceItem(letter, text, this));

        OnPropertyChanged(nameof(CardsRemaining));
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(CardBodyText));
        OnPropertyChanged(nameof(HasBack));
        OnPropertyChanged(nameof(FlipButtonText));
        OnPropertyChanged(nameof(HasChoices));
        OnPropertyChanged(nameof(HasNoChoices));
        (FlipCommand as RelayCommand)?.RaiseCanExecuteChanged();

        SecondsRemaining = _secondsRemaining;
        if (TimerEnabled && !IsGameOver) _ = StartTimerAsync();
    }

    private void OnTurnResult(object? sender, TurnResultEvent e)
    {
        _played++;
        CanUndo = true;
        _lastScores = e.CurrentScores;
        RefreshScores();
        FlashText = e.ScoreDelta switch
        {
            > 0 => $"{e.PlayerName}  +{e.ScoreDelta}",
            < 0 => $"{e.PlayerName}  {e.ScoreDelta}",
            _ => FlashText.Contains(" picked ") ? FlashText : $"{e.PlayerName}  ·",
        };
    }

    private void OnTurnSkipped(object? sender, TurnSkippedEvent e)
    {
        _played++;
        CanUndo = true;
        FlashText = $"{e.PlayerName} skipped";
    }

    private void OnSkipAttempted(object? sender, SkipAttemptedEvent e)
    {
        _lastScores = e.CurrentScores;
        RefreshScores();
        if (e.Penalty != 0)
            FlashText = $"{e.PlayerName} skipped ({-Math.Abs(e.Penalty)})";
    }

    private void OnTurnUndone(object? sender, TurnUndoneEvent e)
    {
        _played = Math.Max(0, _played - 1);
        CanUndo = false; // one level of undo, matching the engine
        _lastScores = e.CurrentScores;
        RefreshScores();
        FlashText = $"Undid {e.CardTitle}";
    }

    private void RefreshScores()
    {
        ScoresText = string.Join("   ·   ", _lastScores.Select(s => $"{s.Name} {s.Score}"));
        OnPropertyChanged(nameof(ScoresText));
        OnPropertyChanged(nameof(HasScores));

        var leader = Math.Max(1, _lastScores.Count == 0 ? 1 : _lastScores.Max(s => s.Score));
        Scores.Clear();
        foreach (var s in _lastScores)
            Scores.Add(ScoreRow.For(s.Name, s.Score, leader));
    }

    private void OnGameEnded(object? sender, GameEndedEvent e)
    {
        StopTimer();

        var lines = new List<string>();
        var rank = 1;
        foreach (var s in e.FinalStandings)
            lines.Add($"{rank++}. {s.Name} — {s.Score}");
        lines.Add($"({e.TotalRounds} rounds)");

        var styles = TallySummaryText();
        if (styles.Length > 0)
        {
            lines.Add(string.Empty);
            lines.Add("Your styles:");
            lines.Add(styles);
        }

        SummaryText = string.Join("\n", lines);
        IsGameOver = true; // must be set before the event, so a subscriber reading it sees the final state
        GameOver?.Invoke(SummaryText);
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    /// <summary>Records the current card's outcome; the engine advances and rotates.</summary>
    public void Complete() { if (_controller is not null && !IsGameOver) { StopTimer(); _controller.RecordOutcome(CardOutcome.Completed); } }

    /// <summary>Skips the current card.</summary>
    public void Skip() { if (_controller is not null && !IsGameOver) { StopTimer(); _controller.RecordOutcome(CardOutcome.Skipped); } }

    /// <summary>Ends the game early — final standings arrive via <see cref="GameOver"/> and <see cref="SummaryText"/>.</summary>
    public void Quit()
    {
        if (_controller is not null && !IsGameOver) _controller.Quit();
        else _navigator.GoBack();
    }

    /// <summary>
    /// Saves the session so it can be resumed.
    ///
    /// Was fire-and-forget (<c>_ = _controller!.SaveAsync();</c>): a write
    /// failure became an unobserved task exception, and the player who just
    /// asked to save got no feedback at all — worse than silent, since even
    /// <see cref="FlashText"/> never changed to say anything went wrong. Saved
    /// sessions are the one persistence path in this app the player explicitly
    /// asks for, so unlike settings, a failure here is always worth reporting.
    /// </summary>
    public async void SaveSession()
    {
        if (!CanSave) return;
        try
        {
            await _controller!.SaveAsync();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            FlashText = "Couldn't save — check disk space and permissions";
        }
    }

    /// <summary>Reverses the last turn and re-presents its card.</summary>
    public void UndoLastTurn() => _controller?.UndoLastTurn();

    /// <summary>Flips a two-faced card.</summary>
    public void FlipCard()
    {
        if (!HasBack) return;
        _isFlipped = !_isFlipped;
        OnPropertyChanged(nameof(CardBodyText));
        OnPropertyChanged(nameof(FlipButtonText));
    }

    /// <summary>Tallies the current player's answer, then completes the turn.</summary>
    public void RecordChoice(char letter)
    {
        letter = char.ToUpperInvariant(letter);
        if (Choices.Count > 0 && Choices.All(c => c.Letter != letter)) return;

        var who = PlayerName.Length > 0 ? PlayerName : "?";
        if (!_tallies.TryGetValue(who, out var tally))
            _tallies[who] = tally = new Dictionary<char, int>();
        tally[letter] = tally.GetValueOrDefault(letter) + 1;

        FlashText = $"{who} picked {letter}";
        Complete();
    }

    /// <summary>Per-player style verdicts for a personality-quiz mode's game-over screen, or empty.</summary>
    public string TallySummaryText() =>
        _tallies.Count == 0
            ? string.Empty
            : string.Join("\n", _tallies.Select(kv =>
                $"{kv.Key}: {ChoiceCards.Format(kv.Value)} -> {ChoiceCards.Verdict(kv.Value, _styleNames)}"));

    private void ForEachPlayer(Action<Guid> apply)
    {
        if (_controller is null || !_controller.SupportsFlow) return;
        foreach (var p in _controller.Players) apply(p.Id);
    }

    /// <summary>Nudges everyone's difficulty up.</summary>
    public void LevelUp() => ForEachPlayer(id => _controller!.LevelUp(id));
    /// <inheritdoc cref="LevelUp" />
    public void LevelDown() => ForEachPlayer(id => _controller!.LevelDown(id));
    /// <inheritdoc cref="LevelUp" />
    public void SpeedUp() => ForEachPlayer(id => _controller!.SpeedUp(id));
    /// <inheritdoc cref="LevelUp" />
    public void SlowDown() => ForEachPlayer(id => _controller!.SlowDown(id));

    private void RaiseActionState()
    {
        (CompleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (SkipCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (UndoCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    // ── Timer ─────────────────────────────────────────────────────────────────

    private async Task StartTimerAsync()
    {
        StopTimer();
        _timerCts = new CancellationTokenSource();
        var token = _timerCts.Token;

        while (SecondsRemaining > 0 && !token.IsCancellationRequested)
        {
            try { await Task.Delay(1000, token); }
            catch (OperationCanceledException) { return; }
            if (!token.IsCancellationRequested) SecondsRemaining--;
        }
    }

    private void StopTimer()
    {
        _timerCts?.Cancel();
        _timerCts?.Dispose();
        _timerCts = null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        StopTimer();
        if (_controller is not null)
        {
            _controller.CardReady -= OnCardReady;
            _controller.TurnResult -= OnTurnResult;
            _controller.TurnSkipped -= OnTurnSkipped;
            _controller.SkipAttempted -= OnSkipAttempted;
            _controller.TurnUndone -= OnTurnUndone;
            _controller.GameEnded -= OnGameEnded;
            _controller.Dispose();
        }
    }

    /// <summary>One tappable A–D quiz choice.</summary>
    public sealed class ChoiceItem
    {
        private readonly CardTurnGameViewModel _owner;

        /// <summary>The letter A–D.</summary>
        public char Letter { get; }
        /// <summary>Display text including the letter prefix.</summary>
        public string Display { get; }
        /// <summary>Command that records this choice. WinUI binds this.</summary>
        public ICommand ChooseCommand { get; }

        internal ChoiceItem(char letter, string text, CardTurnGameViewModel owner)
        {
            Letter = letter;
            Display = $"{letter}) {text}";
            _owner = owner;
            ChooseCommand = new RelayCommand(() => owner.RecordChoice(letter));
        }

        /// <summary>Records this choice. Called directly by a head's code-behind, same duality as everywhere else on this screen.</summary>
        public void Invoke() => _owner.RecordChoice(Letter);
    }

    /// <summary>One player's score row: a name, a pip strip, and the numeral.</summary>
    public sealed record ScoreRow(string Name, int Score, string Pips, bool IsLeading)
    {
        /// <summary>Pips shown before the numeral has to carry the value alone.</summary>
        public const int MaxPips = 6;

        internal static ScoreRow For(string name, int score, int leader)
        {
            var filled = Math.Clamp(score, 0, MaxPips);
            return new ScoreRow(
                Name: name, Score: score,
                Pips: new string('\u25CF', filled) + new string('\u25CB', MaxPips - filled),
                IsLeading: score >= leader && score > 0);
        }
    }
}
