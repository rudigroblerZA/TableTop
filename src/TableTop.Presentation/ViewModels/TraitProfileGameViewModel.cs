using System.Collections.ObjectModel;
using System.Windows.Input;
using TableTop.Core.Abstractions.Analysis;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Events;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Presentation.ViewModels;

/// <summary>
/// The trait-assessment screen (Big Five, Love Languages), shared by every head.
///
/// <para>
/// <b>The same re-entrancy hazard <see cref="HerdGameViewModel"/> documents
/// applies here, for the same reason.</b>
/// <see cref="ITraitProfileController.SubmitResponses"/> raises
/// <see cref="ITraitProfileController.ItemRecorded"/> and then advances
/// <i>inside the same call</i>, raising either
/// <see cref="ITraitProfileController.ItemReady"/> or
/// <see cref="ITraitProfileController.AssessmentCompleted"/> before
/// <c>SubmitResponses</c> returns. So the handlers keep strict ownership:
/// <see cref="OnItemReady"/> owns the current-item properties and the response
/// entries, <see cref="OnAssessmentCompleted"/> owns the results, and neither
/// reads or clears anything the other writes. It does not matter which order
/// they fire in within one <see cref="Submit"/> call.
/// </para>
///
/// <para>
/// <b>Why a "results" screen rather than a scoreboard.</b> This is the only
/// family whose controller produces no score. Every other game screen ends by
/// naming a winner; this one ends by rendering a profile per player and the
/// comparison between them, which is why <see cref="Profiles"/> and
/// <see cref="ComparisonSummary"/> exist where the other ViewModels have a
/// <c>Summary</c> string.
/// </para>
/// </summary>
public sealed class TraitProfileGameViewModel : ViewModelBase, IDisposable
{
    private readonly ITraitProfileController? _controller;

    /// <summary>One entry per player, for a shared-device answer sheet.</summary>
    public ObservableCollection<PlayerResponseEntry> PlayerResponses { get; } = [];

    /// <summary>One profile per player, populated when the assessment ends.</summary>
    public ObservableCollection<PlayerProfileView> Profiles { get; } = [];

    /// <summary>Records every answered response and advances.</summary>
    public ICommand SubmitCommand { get; }
    /// <summary>Skips the current statement for everyone.</summary>
    public ICommand SkipCommand { get; }
    /// <summary>Ends the session early and returns to the previous screen.</summary>
    public ICommand BackCommand { get; }

    // Current item — owned by OnItemReady. Never touched by OnAssessmentCompleted.
    private int _itemNumber, _totalItems;
    private string _statement = "", _category = "";

    // Results — owned by OnAssessmentCompleted. Never touched by OnItemReady.
    private string _comparisonSummary = "", _summary = "";
    private bool _isComplete;

    private readonly string _loadError = "";

    /// <summary>1-based position of the current statement.</summary>
    public int ItemNumber { get => _itemNumber; private set { SetField(ref _itemNumber, value); OnPropertyChanged(nameof(Progress)); OnPropertyChanged(nameof(ProgressLabel)); } }

    /// <summary>How many statements this session plays.</summary>
    public int TotalItems { get => _totalItems; private set { SetField(ref _totalItems, value); OnPropertyChanged(nameof(Progress)); OnPropertyChanged(nameof(ProgressLabel)); } }

    /// <summary>The statement on screen.</summary>
    public string Statement { get => _statement; private set => SetField(ref _statement, value); }

    /// <summary>The current statement's category — conventionally the dimension it loads on.</summary>
    public string Category { get => _category; private set => SetField(ref _category, value); }

    /// <summary>Progress through the bank, 0-1, for a bar.</summary>
    public double Progress => _totalItems <= 0 ? 0d : Math.Clamp((double)_itemNumber / _totalItems, 0d, 1d);

    /// <summary>Progress as "7 / 50".</summary>
    public string ProgressLabel => _totalItems <= 0 ? "" : $"{_itemNumber} / {_totalItems}";

    /// <summary>True once the assessment has finished.</summary>
    public bool IsComplete
    {
        get => _isComplete;
        private set { SetField(ref _isComplete, value); OnPropertyChanged(nameof(IsPlaying)); }
    }

    /// <summary>How the profiles read against each other, or empty for a single player.</summary>
    public string ComparisonSummary
    {
        get => _comparisonSummary;
        private set { SetField(ref _comparisonSummary, value); OnPropertyChanged(nameof(HasComparison)); }
    }

    /// <summary>True when there were at least two profiles to compare.</summary>
    public bool HasComparison => _comparisonSummary.Length > 0;

    /// <summary>A one-line close for the results screen.</summary>
    public string Summary { get => _summary; private set => SetField(ref _summary, value); }

    /// <summary>
    /// The standing caveat, kept on the ViewModel so every head shows the same
    /// words rather than each inventing its own.
    ///
    /// <para>
    /// Not decoration: <see cref="TraitScore.Normalized"/> is a position on this
    /// instrument's own range and reads as a percentile to nearly everyone who
    /// sees a number out of 100 beside a personality trait.
    /// </para>
    /// </summary>
    /// <remarks>
    /// An <b>instance</b> property, not static, and that is load-bearing:
    /// <c>{Binding}</c> resolves against the instance on both XAML heads, so a
    /// static one binds to nothing and renders empty. It was static first and
    /// <c>check-xaml-bindings.py</c> caught it — the exact silently-empty-UI
    /// failure that gate exists for.
    /// </remarks>
    public string ResultsCaveat =>
        "Scores show where your answers landed on this quiz's own range — they are not "
        + "percentiles, and this is not a personality test.";

    /// <summary>True while the assessment is live and loadable.</summary>
    public bool IsPlaying => !IsComplete && !HasLoadError;

    /// <summary>Deck-load failure message, or empty.</summary>
    public string LoadError => _loadError;

    /// <summary>True when the item bank could not be loaded.</summary>
    public bool HasLoadError => !string.IsNullOrEmpty(_loadError);

    /// <summary>Builds the screen around an already-created controller.</summary>
    public TraitProfileGameViewModel(INavigator navigator, ITraitProfileController controller)
    {
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(controller);

        _controller = controller;

        SubmitCommand = new RelayCommand(Submit, () => AnyAnswered);
        SkipCommand = new RelayCommand(Skip);
        BackCommand = new RelayCommand(() => { _controller?.Quit(); navigator.GoBack(); });

        _controller.ItemReady += OnItemReady;
        _controller.AssessmentCompleted += OnAssessmentCompleted;

        if (!_controller.IsRunning) _controller.Start();
    }

    /// <summary>Error-state constructor: no controller, just a message.</summary>
    private TraitProfileGameViewModel(INavigator navigator, string loadError)
    {
        _loadError = loadError;
        SubmitCommand = new RelayCommand(() => { }, () => false);
        SkipCommand = new RelayCommand(() => { }, () => false);
        BackCommand = new RelayCommand(navigator.GoBack);
    }

    /// <summary>
    /// Builds the controller from a mode, surfacing a load failure as a message
    /// rather than an exception — and going through
    /// <see cref="IControllerFactory"/> rather than constructing
    /// <c>TraitProfileController</c> directly, for the reason recorded on
    /// <see cref="HerdGameViewModel.CreateAsync"/> (backlog X.2 / N.1).
    /// </summary>
    /// <param name="navigator">Used to leave the screen.</param>
    /// <param name="mode">The mode to play.</param>
    /// <param name="players">The players at the table.</param>
    /// <param name="controllerFactory">The host's factory. Required.</param>
    public static async Task<TraitProfileGameViewModel> CreateAsync(
        INavigator navigator,
        IGameMode mode,
        IReadOnlyList<IPlayer> players,
        IControllerFactory controllerFactory)
    {
        ArgumentNullException.ThrowIfNull(controllerFactory);

        try
        {
            var controller = await controllerFactory.CreateAsync(mode, players);
            if (controller is not ITraitProfileController tp)
            {
                controller.Dispose();
                throw new NotSupportedException($"'{mode.Name}' isn't a trait-assessment mode.");
            }

            return new TraitProfileGameViewModel(navigator, tp);
        }
        catch (Exception ex)
        {
            return new TraitProfileGameViewModel(navigator, ex.Message);
        }
    }

    // ── Controller events ─────────────────────────────────────────────────────

    private void OnItemReady(object? sender, TraitItemReadyEvent e)
    {
        ItemNumber = e.ItemNumber;
        TotalItems = e.TotalItems;
        Statement = e.Statement;
        Category = e.Category;

        // Rebuilding the entries rather than clearing each one's Response is
        // deliberate: a stale entry for a player no longer on the roster would
        // otherwise survive, and Submit would send a name the controller drops.
        foreach (var entry in PlayerResponses) entry.PropertyChanged -= OnEntryChanged;
        PlayerResponses.Clear();

        foreach (var name in _controller!.PlayerNames)
        {
            var entry = new PlayerResponseEntry(name);
            entry.PropertyChanged += OnEntryChanged;
            PlayerResponses.Add(entry);
        }

        RaiseSubmitCanExecuteChanged();
    }

    private void OnAssessmentCompleted(object? sender, TraitAssessmentCompletedEvent e)
    {
        IsComplete = true;

        Profiles.Clear();
        foreach (var profile in e.Profiles) Profiles.Add(new PlayerProfileView(profile));

        Summary = e.Profiles.Count switch
        {
            0 => "Nobody answered anything, so there is nothing to report.",
            1 => $"{e.Profiles[0].PlayerName} answered {e.Profiles[0].AnsweredItems} of {TotalItems}.",
            _ => $"{e.Profiles.Count} profiles from {e.ItemsAnswered} statements.",
        };

        ComparisonSummary = BuildComparison(e);
    }

    /// <summary>
    /// Renders the pairwise read. Separate and static so a head can be tested
    /// against the wording without driving a whole session.
    /// </summary>
    internal static string BuildComparison(TraitAssessmentCompletedEvent e)
    {
        if (e.MostAlike is not { } alike || alike.ComparedDimensions == 0) return "";

        var lines = new List<string>
        {
            $"{alike.Left.PlayerName} & {alike.Right.PlayerName} — "
            + $"{alike.Similarity:0}% alike across {alike.ComparedDimensions} traits",
        };

        if (alike.GreatestDivergence is { } gap)
            lines.Add($"Furthest apart on {gap.Trait.Name}: "
                    + $"{gap.Left.Normalized:0} vs {gap.Right.Normalized:0}");

        if (alike.ClosestAlignment is { } same)
            lines.Add($"Most aligned on {same.Trait.Name}: "
                    + $"{same.Left.Normalized:0} vs {same.Right.Normalized:0}");

        // Only worth saying when there are at least three players, where the
        // closest and furthest pairs are genuinely different pairs.
        if (e.MostDifferent is { } apart && !ReferenceEquals(apart, alike))
            lines.Add($"Furthest overall: {apart.Left.PlayerName} & {apart.Right.PlayerName} "
                    + $"({apart.Similarity:0}% alike)");

        return string.Join(Environment.NewLine, lines);
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    /// <summary>True once at least one player has picked a response for this statement.</summary>
    public bool AnyAnswered => PlayerResponses.Any(r => r.Response is not null);

    /// <summary>True once every player has picked a response for this statement.</summary>
    public bool AllAnswered => PlayerResponses.Count > 0 && PlayerResponses.All(r => r.Response is not null);

    /// <summary>
    /// Sends every picked response and advances. Players who picked nothing are
    /// simply absent, which the controller treats as a skip for them — not as a
    /// neutral answer.
    /// </summary>
    public void Submit()
    {
        var responses = new Dictionary<string, LikertResponse>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in PlayerResponses)
            if (entry.Response is { } response) responses[entry.PlayerName] = response;

        _controller?.SubmitResponses(responses);
    }

    /// <summary>Skips this statement for everyone.</summary>
    public void Skip() => _controller?.Skip();

    private void OnEntryChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(PlayerResponseEntry.Response)) return;
        RaiseSubmitCanExecuteChanged();
    }

    private void RaiseSubmitCanExecuteChanged()
    {
        OnPropertyChanged(nameof(AnyAnswered));
        OnPropertyChanged(nameof(AllAnswered));
        (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var entry in PlayerResponses) entry.PropertyChanged -= OnEntryChanged;

        if (_controller is not null)
        {
            _controller.ItemReady -= OnItemReady;
            _controller.AssessmentCompleted -= OnAssessmentCompleted;
            _controller.Dispose();
        }
    }

}

/// <summary>One player's answer to the current statement.</summary>
public sealed class PlayerResponseEntry : ViewModelBase
{
    private LikertResponse? _response;

    /// <summary>Initialises an unanswered entry for <paramref name="playerName"/>.</summary>
    public PlayerResponseEntry(string playerName)
    {
        PlayerName = playerName;
        PickCommand = new ParameterRelayCommand(p => Pick(ParameterRelayCommand.AsInt(p)));
    }

    /// <summary>The player this answer belongs to.</summary>
    public string PlayerName { get; }

    /// <summary>
    /// Picks this player's response. The parameter is the 1-5 value; see
    /// <see cref="ParameterRelayCommand"/> for why it is loosely typed.
    /// </summary>
    public ICommand PickCommand { get; }

    /// <summary>What they picked, or null when they have not answered.</summary>
    public LikertResponse? Response
    {
        get => _response;
        set { SetField(ref _response, value); OnPropertyChanged(nameof(HasAnswered)); OnPropertyChanged(nameof(SelectedValue)); }
    }

    /// <summary>True once this player has picked something.</summary>
    public bool HasAnswered => _response is not null;

    /// <summary>The picked value as 1-5, or 0 when unanswered — for heads that bind to an int.</summary>
    public int SelectedValue => _response is { } r ? (int)r : 0;

    /// <summary>Picks a response from a 1-5 value; anything else clears it.</summary>
    public void Pick(int value) =>
        Response = value is >= 1 and <= 5 ? (LikertResponse)value : null;
}

/// <summary>One player's finished profile, shaped for display.</summary>
public sealed class PlayerProfileView
{
    /// <summary>Builds a display view over a finished profile.</summary>
    public PlayerProfileView(TraitProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        PlayerName = profile.PlayerName;
        AnsweredItems = profile.AnsweredItems;
        Scores = new ObservableCollection<TraitScoreView>(
            profile.Scores.Select(s => new TraitScoreView(s)));

        var top = profile.Strongest();
        TopTrait = top.Count > 0 ? top[0].Trait.Name : "";
    }

    /// <summary>Who this profile belongs to.</summary>
    public string PlayerName { get; }

    /// <summary>How many statements they answered.</summary>
    public int AnsweredItems { get; }

    /// <summary>One row per dimension, in the scale's order.</summary>
    public ObservableCollection<TraitScoreView> Scores { get; }

    /// <summary>
    /// Their highest-scoring dimension, or empty when they answered nothing.
    ///
    /// <para>
    /// The headline for a ranking mode like Love Languages, where which
    /// dimension is highest matters more than how high it is.
    /// </para>
    /// </summary>
    public string TopTrait { get; }

    /// <summary>True when there is a highest dimension worth naming.</summary>
    public bool HasTopTrait => TopTrait.Length > 0;
}

/// <summary>One dimension's score, shaped for display.</summary>
public sealed class TraitScoreView
{
    /// <summary>Builds a display view over one dimension's score.</summary>
    public TraitScoreView(TraitScore score)
    {
        ArgumentNullException.ThrowIfNull(score);

        TraitName = score.Trait.Name;
        Description = score.Trait.Description;
        Normalized = score.Normalized;
        HasData = score.HasData;

        BandLabel = !score.HasData ? "no answers" : score.Band switch
        {
            TraitBand.VeryLow or TraitBand.Low => score.Trait.LowLabel,
            TraitBand.High or TraitBand.VeryHigh => score.Trait.HighLabel,
            _ => "in the middle",
        };
    }

    /// <summary>The dimension's display name.</summary>
    public string TraitName { get; }

    /// <summary>What the dimension measures, in a sentence.</summary>
    public string Description { get; }

    /// <summary>Where they landed, 0-100.</summary>
    public double Normalized { get; }

    /// <summary>The score rounded, for a label.</summary>
    public int Rounded => (int)Math.Round(Normalized, MidpointRounding.AwayFromZero);

    /// <summary>0-1, for a progress bar that wants a fraction.</summary>
    public double Fraction => Normalized / 100d;

    /// <summary>The band in the dimension's own words, not "VeryHigh".</summary>
    public string BandLabel { get; }

    /// <summary>True when at least one answered item loaded on this dimension.</summary>
    public bool HasData { get; }
}
