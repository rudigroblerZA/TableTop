using System.Windows.Input;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;
using TableTop.Hosting.Events;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Presentation.ViewModels;

/// <summary>
/// The Day One screen, shared by every head.
///
/// <para>
/// A campaign mode: one card per real day, unlocked on a clock rather than by
/// playing faster. The three states it can be in — a card is waiting, you are
/// caught up until the next unlock, or the campaign is finished — come straight
/// from the controller's events, so this holds almost no logic of its own.
/// </para>
///
/// <para>
/// Merged surface: WinUI's commands, plus MAUI's <see cref="LoadError"/> and
/// <see cref="HasLoadError"/>. WinUI had no load-error path at all, so a mode
/// that failed to build its controller took the app down instead of saying why.
/// </para>
/// </summary>
public sealed class DayOneGameViewModel : ViewModelBase, IDisposable
{
    private readonly IDayOneController? _controller;
    private readonly string _loadError = "";

    private string _dayLabel = "", _cardTitle = "", _cardText = "", _statusText = "";
    private bool   _hasCard, _isDone;

    /// <summary>Returns to the previous screen.</summary>
    public ICommand BackCommand          { get; }

    /// <summary>Marks today's card complete and advances.</summary>
    public ICommand CompleteTodayCommand { get; }

    /// <summary>"Day 4 of 21", or similar.</summary>
    public string DayLabel   { get => _dayLabel;   private set => SetField(ref _dayLabel, value); }
    /// <summary>Title of today's card.</summary>
    public string CardTitle  { get => _cardTitle;  private set => SetField(ref _cardTitle, value); }
    /// <summary>Body text of today's card.</summary>
    public string CardText   { get => _cardText;   private set => SetField(ref _cardText, value); }
    /// <summary>Caught-up or completion message.</summary>
    public string StatusText { get => _statusText; private set => SetField(ref _statusText, value); }

    /// <summary>True when a card is waiting to be played.</summary>
    public bool HasCard { get => _hasCard; private set { SetField(ref _hasCard, value); RaiseCompleteState(); } }

    /// <summary>True once the whole campaign is finished.</summary>
    public bool IsDone  { get => _isDone;  private set => SetField(ref _isDone, value); }

    /// <summary>Controller-build failure message, or empty. Was MAUI-only.</summary>
    public string LoadError => _loadError;

    /// <summary>True when the mode could not be started.</summary>
    public bool HasLoadError => !string.IsNullOrEmpty(_loadError);

    /// <summary>Builds the screen around an already-created controller.</summary>
    /// <param name="navigator">Used to leave the screen.</param>
    /// <param name="controller">A Day One controller.</param>
    public DayOneGameViewModel(INavigator navigator, IDayOneController controller)
    {
        _controller = controller;

        BackCommand          = new RelayCommand(navigator.GoBack);
        CompleteTodayCommand = new RelayCommand(CompleteToday, () => HasCard);

        _controller.DayReady         += OnDayReady;
        _controller.AllCaughtUp      += OnAllCaughtUp;
        _controller.CampaignComplete += OnCampaignComplete;
        _controller.Start();
    }

    /// <summary>Error-state constructor: no controller, just a message.</summary>
    private DayOneGameViewModel(INavigator navigator, string loadError)
    {
        _loadError           = loadError;
        BackCommand          = new RelayCommand(navigator.GoBack);
        CompleteTodayCommand = new RelayCommand(() => { }, () => false);
    }

    /// <summary>
    /// Builds the controller from a mode, surfacing a failure as a message
    /// rather than an exception — which is what MAUI did and WinUI did not.
    /// </summary>
    public static async Task<DayOneGameViewModel> CreateAsync(
        INavigator             navigator,
        IGameMode              mode,
        IReadOnlyList<IPlayer> players,
        IControllerFactory?    controllerFactory = null)
    {
        try
        {
            var controller = await (controllerFactory ?? new ControllerFactory()).CreateAsync(mode, players, maxRounds: 30);
            if (controller is not IDayOneController dc)
            {
                controller.Dispose();
                throw new NotSupportedException($"'{mode.Name}' isn't a Day One-style mode.");
            }
            return new DayOneGameViewModel(navigator, dc);
        }
        catch (Exception ex)
        {
            return new DayOneGameViewModel(navigator, ex.Message);
        }
    }

    /// <summary>Marks today's card complete. No-op when nothing is waiting.</summary>
    public void CompleteToday()
    {
        if (!HasCard) return;
        _controller?.CompleteToday();
    }

    // ── Controller events ─────────────────────────────────────────────────────

    private void OnDayReady(object? sender, DayReadyEvent e)
    {
        DayLabel   = $"Day {e.DayNumber} of {e.TotalDays}";
        CardTitle  = e.Card.Title;
        CardText   = e.CardText;
        StatusText = "";
        HasCard    = true;
    }

    private void OnAllCaughtUp(object? sender, AllCaughtUpEvent e)
    {
        HasCard  = false;
        DayLabel = $"Day {e.DayNumber} of {e.TotalDays}";

        // Hours rather than a timestamp: the wait is what matters, and a
        // formatted date would need the head's locale.
        var h = (int)Math.Ceiling(e.TimeUntilNextUnlock.TotalHours);
        StatusText = h <= 1
            ? "You're all caught up. The next card unlocks within the hour."
            : $"You're all caught up. The next card unlocks in about {h} hours.";
    }

    private void OnCampaignComplete(object? sender, CampaignCompleteEvent e)
    {
        HasCard    = false;
        IsDone     = true;
        DayLabel   = $"All {e.TotalDays} days complete";
        StatusText = $"Finished in {(e.CompletedAt - e.StartedAt).Days + 1} days.";
    }

    private void RaiseCompleteState() =>
        (CompleteTodayCommand as RelayCommand)?.RaiseCanExecuteChanged();

    /// <inheritdoc />
    public void Dispose() => _controller?.Dispose();
}
