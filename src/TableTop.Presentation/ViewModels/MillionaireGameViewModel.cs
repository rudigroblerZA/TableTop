using System.Collections.ObjectModel;
using System.Windows.Input;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;
using TableTop.Hosting.Events;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Presentation.ViewModels;

/// <summary>
/// The Millionaire screen, shared by every head.
///
/// <para>
/// Merged surface: WinUI's commands plus MAUI's <see cref="LoadError"/> path
/// and the <c>Invoke()</c> methods its buttons call directly. The two versions
/// were the closest pair in the whole set — same properties, same handlers,
/// same prize formatting — which is its own argument for sharing: two files
/// that identical are two files that will diverge the moment either is touched.
/// </para>
/// </summary>
public sealed class MillionaireGameViewModel : ViewModelBase, IDisposable
{
    private readonly IMillionaireController? _controller;
    private readonly string _loadError = "";

    private string _questionText = "", _playerName = "", _prizeText = "";
    private string _guaranteedText = "", _flash = "", _summary = "";
    private bool   _isAnswered, _isGameOver;

    /// <summary>The four (or fewer, after 50:50) answer options.</summary>
    public ObservableCollection<AnswerOption>   Answers   { get; } = [];
    /// <summary>Available lifelines.</summary>
    public ObservableCollection<LifelineOption> Lifelines { get; } = [];

    /// <summary>Returns to the previous screen.</summary>
    public ICommand BackCommand     { get; }
    /// <summary>Walks away with the current guaranteed prize.</summary>
    public ICommand WalkAwayCommand { get; }

    /// <summary>The current question.</summary>
    public string QuestionText   { get => _questionText;   private set => SetField(ref _questionText, value); }
    /// <summary>Who is in the hot seat.</summary>
    public string PlayerName     { get => _playerName;     private set => SetField(ref _playerName, value); }
    /// <summary>What this question is worth.</summary>
    public string PrizeText      { get => _prizeText;      private set => SetField(ref _prizeText, value); }
    /// <summary>The locked-in safe-haven amount.</summary>
    public string GuaranteedText { get => _guaranteedText; private set => SetField(ref _guaranteedText, value); }
    /// <summary>Transient feedback after an action.</summary>
    public string Flash          { get => _flash;          private set => SetField(ref _flash, value); }
    /// <summary>Final standings.</summary>
    public string Summary        { get => _summary;        private set => SetField(ref _summary, value); }

    /// <summary>True once this question has been answered.</summary>
    public bool IsAnswered
    {
        get => _isAnswered;
        private set { SetField(ref _isAnswered, value); OnPropertyChanged(nameof(CanInteract)); RaiseActionState(); }
    }

    /// <summary>True once every player has finished.</summary>
    public bool IsGameOver
    {
        get => _isGameOver;
        private set
        {
            SetField(ref _isGameOver, value);
            OnPropertyChanged(nameof(CanInteract));
            OnPropertyChanged(nameof(IsPlaying));
            RaiseActionState();
        }
    }

    /// <summary>True while the session is live and loadable.</summary>
    public bool IsPlaying   => !IsGameOver && !HasLoadError;

    /// <summary>True when answers and lifelines should accept input.</summary>
    public bool CanInteract => !IsAnswered && !IsGameOver && !HasLoadError;

    /// <summary>Controller-build failure message, or empty. Was MAUI-only.</summary>
    public string LoadError    => _loadError;
    /// <summary>True when the mode could not be started.</summary>
    public bool   HasLoadError => !string.IsNullOrEmpty(_loadError);

    /// <summary>Builds the screen around an already-created controller.</summary>
    public MillionaireGameViewModel(INavigator navigator, IMillionaireController controller)
    {
        _controller = controller;

        BackCommand     = new RelayCommand(navigator.GoBack);
        WalkAwayCommand = new RelayCommand(WalkAway, () => CanInteract);

        _controller.HotSeatBegan     += OnHotSeatBegan;
        _controller.QuestionReady    += OnQuestionReady;
        _controller.LifelineUsed     += OnLifelineUsed;
        _controller.AnswerCorrect    += OnAnswerCorrect;
        _controller.AnswerWrong      += OnAnswerWrong;
        _controller.WalkedAway       += OnWalkedAway;
        _controller.MillionaireWon   += OnWon;
        _controller.GameEnded        += OnGameEnded;
        _controller.Start();
    }

    /// <summary>Error-state constructor: no controller, just a message.</summary>
    private MillionaireGameViewModel(INavigator navigator, string loadError)
    {
        _loadError      = loadError;
        BackCommand     = new RelayCommand(navigator.GoBack);
        WalkAwayCommand = new RelayCommand(() => { }, () => false);
    }

    /// <summary>
    /// Builds the controller from a mode, surfacing a failure as a message
    /// rather than an exception — MAUI's behaviour, which WinUI lacked.
    /// </summary>
    public static async Task<MillionaireGameViewModel> CreateAsync(
        INavigator navigator, IGameMode mode, IReadOnlyList<IPlayer> players,
        IControllerFactory? controllerFactory = null)
    {
        try
        {
            var controller = await (controllerFactory ?? new ControllerFactory()).CreateAsync(mode, players);
            if (controller is not IMillionaireController mc)
            {
                controller.Dispose();
                throw new NotSupportedException($"'{mode.Name}' isn't a Millionaire-style mode.");
            }
            return new MillionaireGameViewModel(navigator, mc);
        }
        catch (Exception ex)
        {
            return new MillionaireGameViewModel(navigator, ex.Message);
        }
    }

    // long, not int — prizes go to £1,000,000 and beyond; an int parameter
    // silently truncates at the top of the ladder, which is the only place
    // anyone looks.
    private static string Money(long amount) => $"£{amount:N0}";

    // ── Actions ───────────────────────────────────────────────────────────────

    /// <summary>Submits the given answer label.</summary>
    public void Answer(AnswerLabel label)
    {
        if (!CanInteract) return;
        _controller?.SubmitAnswer(label);
    }

    /// <summary>Uses the lifeline at the given index.</summary>
    public void UseLifeline(int index)
    {
        if (!CanInteract) return;
        _controller?.UseLifeline(index);
    }

    /// <summary>Walks away with the guaranteed prize.</summary>
    public void WalkAway()
    {
        if (!CanInteract) return;
        _controller?.WalkAway();
    }

    // ── Controller events ─────────────────────────────────────────────────────

    private void OnHotSeatBegan(object? sender, HotSeatBeganEvent e)
    {
        PlayerName = e.PlayerName;
        Flash      = $"{e.PlayerName} takes the hot seat!";
    }

    private void OnQuestionReady(object? sender, QuestionReadyEvent e)
    {
        IsAnswered   = false;
        QuestionText = e.QuestionText;

        Answers.Clear();
        foreach (var label in e.AvailableOptions)
            Answers.Add(new AnswerOption(label, e.Answers[label], this));

        Lifelines.Clear();
        for (var i = 0; i < e.Lifelines.Count; i++)
            Lifelines.Add(new LifelineOption(i, e.Lifelines[i].Name, e.Lifelines[i].IsAvailable, this));

        var rung = e.Ladder.Rungs.FirstOrDefault(r => r.IsCurrent);
        PrizeText      = rung is not null ? $"Playing for {Money(rung.PrizeAmount)}" : "";
        GuaranteedText = e.Ladder.GuaranteedPrize > 0 ? $"Guaranteed: {Money(e.Ladder.GuaranteedPrize)}" : "";
    }

    private void OnLifelineUsed(object? sender, LifelineUsedEvent e)
    {
        Flash = e.Narrative;

        // 50:50 and similar prune options — reflect the survivors.
        if (e.RemainingOptions.Count > 0 && e.RemainingOptions.Count < Answers.Count)
        {
            var keep = e.RemainingOptions.ToHashSet();
            for (var i = Answers.Count - 1; i >= 0; i--)
                if (!keep.Contains(Answers[i].Label)) Answers.RemoveAt(i);
        }

        foreach (var l in Lifelines) l.MarkUsedIfMatch(e.LifelineName);
    }

    private void OnAnswerCorrect(object? sender, AnswerCorrectEvent e)
    {
        IsAnswered = true;
        Flash = e.SafeHavenReached
            ? $"Correct! {Money(e.PrizeWon)} — and you've locked in {Money(e.GuaranteedPrize)}."
            : $"Correct! Now at {Money(e.PrizeWon)}.";
    }

    private void OnAnswerWrong(object? sender, AnswerWrongEvent e)
    {
        IsAnswered = true;
        Flash = $"Wrong — the answer was {e.CorrectLabel}) {e.CorrectText}. " +
                $"You leave with {Money(e.GuaranteedPrize)}.";
    }

    private void OnWalkedAway(object? sender, WalkedAwayEvent e)
    {
        IsAnswered = true;
        Flash = $"Walked away with {Money(e.Prize)}. Smart.";
    }

    private void OnWon(object? sender, MillionaireWonEvent e) =>
        Flash = $"🏆 {e.PlayerName} WON IT ALL!";

    private void OnGameEnded(object? sender, MillionaireGameEndedEvent e)
    {
        Summary = string.Join("\n", e.Results
            .OrderByDescending(r => r.Prize)
            .Select(r => $"{r.PlayerName}: {Money(r.Prize)}"));
        IsGameOver = true;
    }

    private void RaiseActionState() =>
        (WalkAwayCommand as RelayCommand)?.RaiseCanExecuteChanged();

    /// <inheritdoc />
    public void Dispose() => _controller?.Dispose();

    /// <summary>One selectable answer.</summary>
    public sealed class AnswerOption
    {
        private readonly MillionaireGameViewModel _owner;

        /// <summary>A, B, C or D.</summary>
        public AnswerLabel Label   { get; }
        /// <summary>"A)  Paris", ready to render.</summary>
        public string      Display { get; }

        /// <summary>Command that submits this answer. WinUI binds this.</summary>
        public ICommand SelectCommand { get; }

        internal AnswerOption(AnswerLabel label, string text, MillionaireGameViewModel owner)
        {
            Label = label; Display = $"{label})  {text}"; _owner = owner;
            SelectCommand = new RelayCommand(() => owner.Answer(label), () => owner.CanInteract);
        }

        /// <summary>Submits this answer. Called directly by MAUI's buttons.</summary>
        public void Invoke() => _owner.Answer(Label);
    }

    /// <summary>One lifeline.</summary>
    public sealed class LifelineOption : ViewModelBase
    {
        private readonly MillionaireGameViewModel _owner;
        private bool _available;

        /// <summary>Position in the controller's lifeline list.</summary>
        public int    Index { get; }
        /// <summary>Display name.</summary>
        public string Name  { get; }

        /// <summary>False once spent.</summary>
        public bool IsAvailable { get => _available; private set => SetField(ref _available, value); }

        internal LifelineOption(int index, string name, bool available, MillionaireGameViewModel owner)
        {
            Index = index; Name = name; _available = available; _owner = owner;
            UseCommand = new RelayCommand(() => { if (IsAvailable) owner.UseLifeline(index); },
                                          () => IsAvailable && owner.CanInteract);
        }

        /// <summary>Command that uses this lifeline. WinUI binds this.</summary>
        public ICommand UseCommand { get; }

        /// <summary>
        /// Marks this lifeline spent when the used one matches by name.
        ///
        /// By name rather than by re-reading controller state, because
        /// IMillionaireController exposes no lifeline list — MAUI's version
        /// queried one that does not exist on the interface.
        /// </summary>
        public void MarkUsedIfMatch(string usedName)
        {
            if (Name == usedName) IsAvailable = false;
        }

        /// <summary>Uses this lifeline. Called directly by MAUI's buttons.</summary>
        public void Invoke() => _owner.UseLifeline(Index);
    }
}
