using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TableTop.Core.Abstractions.Game;
using TableTop.Hosting;
using TableTop.Presentation.Infrastructure;
using TableTop.Presentation.ViewModels;
using TableTop.WinUI.Infrastructure;

namespace TableTop.WinUI.ViewModels;

/// <summary>Landing screen: resume saved players or start fresh, open Settings.</summary>
public sealed class IntroViewModel : ViewModelBase
{
    private readonly Navigator _navigator;

    // Was a duplicate of MAUI's resume lookup, near byte-for-byte. One
    // implementation now, in TableTop.Presentation.
    private readonly SavedSessionLookup _savedSession = new();

    /// <summary>Command that begins the play flow.</summary>
    public ICommand PlayCommand { get; }
    /// <summary>Command that opens the settings screen.</summary>
    public ICommand SettingsCommand { get; }

    /// <summary>Command that opens the roster builder.</summary>
    public ICommand RoasterCommand { get; }

    /// <summary>Continues the saved session. Hidden when there isn't one.</summary>
    public ICommand ResumeCommand { get; }

    /// <summary>True when there is a session worth offering.</summary>
    public bool CanResume => _savedSession.CanResume;

    /// <summary>"Alice, Bob · round 4" for the button, or empty.</summary>
    public string ResumeText => _savedSession.ResumeText;

    /// <summary>Initialises the intro screen.</summary>
    public IntroViewModel(Navigator navigator)
    {
        _navigator = navigator;
        PlayCommand = new RelayCommand(Launch);
        ResumeCommand = new AsyncRelayCommand(ResumeAsync, () => CanResume);
        _ = LookForSavedSessionAsync();
        SettingsCommand = new RelayCommand(() => _navigator.Navigate(new SettingsViewModel(_navigator, WinUIAppSettings.Instance)));

        // Lives here rather than on SettingsViewModel — SettingsViewModel is
        // shared and holds only INavigator (GoBack only, by design, so it
        // stays constructible from MAUI too). Opening an arbitrary new
        // screen needs the concrete Navigator.Navigate(ViewModelBase), which
        // only WinUI-local ViewModels like this one hold. MAUI's equivalent
        // button lives on its own SettingsPage code-behind for the same
        // reason: per-head navigation plumbing, not a shared-VM concern.
        RoasterCommand = new RelayCommand(() => _navigator.Navigate(
            new RoasterViewModel(_navigator, new WinUIRosterStore())));
    }

    /// <summary>
    /// Looks for a saved session at launch so the intro screen can offer it.
    ///
    /// Fire-and-forget on purpose: a missing or unreadable save must never delay
    /// the intro screen, so the button simply appears a moment later if there is
    /// something to resume.
    /// </summary>
    private async Task LookForSavedSessionAsync()
    {
        await _savedSession.RefreshAsync();
        OnPropertyChanged(nameof(CanResume));
        OnPropertyChanged(nameof(ResumeText));
    }

    /// <summary>
    /// Was <c>.GetAwaiter().GetResult()</c> on the UI thread — backlog item
    /// 20. <see cref="ResumeCommand"/> being an <see cref="AsyncRelayCommand"/>
    /// (rather than the plain <see cref="RelayCommand"/> it used to be) is
    /// what makes awaiting here safe: the command disables itself for the
    /// duration instead of the dispatcher blocking on it, the same shape
    /// <see cref="PlayerSetupViewModel.StartCommand"/> already used for its
    /// own async build.
    /// </summary>
    private async Task ResumeAsync()
    {
        var resumable = _savedSession.Resumable;
        if (resumable is null) return;

        _navigator.Navigate(await GameViewModelFactory.CreateAsync(
            _navigator, resumable.Mode, resumable.Players, resumable.Snapshot));
    }

    private void Launch()
    {
        var registry = ArchetypeRegistry.Default();

        // Same age-floor semantics as WPF/MAUI: "hides games below the
        // selected rating" — applied once here so every downstream picker
        // sees the filtered tree automatically.
        var floor = (AgeRating)WinUIAppSettings.Instance.MinAgeRating;
        IArchetypeRegistry effective = floor == AgeRating.AllAges
            ? registry
            : new FilteredArchetypeRegistry(registry, floor);

        _navigator.Navigate(new ArchetypePickerViewModel(_navigator, effective));
    }
}

/// <summary>Top-level archetype (category) picker.</summary>
public sealed class ArchetypePickerViewModel : ViewModelBase
{
    private readonly Navigator _navigator;
    private readonly IArchetypeRegistry _registry;

    /// <summary>Root archetypes to display.</summary>
    public ObservableCollection<Archetype> Archetypes { get; }
    /// <summary>Command bound to a tapped archetype.</summary>
    public ICommand SelectCommand { get; }
    /// <summary>Command that returns to the intro screen.</summary>
    public ICommand BackCommand { get; }

    /// <summary>Initialises the picker over the (possibly filtered) registry.</summary>
    public ArchetypePickerViewModel(Navigator navigator, IArchetypeRegistry registry)
    {
        _navigator = navigator;
        _registry = registry;
        Archetypes = new ObservableCollection<Archetype>(registry.RootArchetypes);
        BackCommand = new RelayCommand(() => _navigator.GoBack());
        SelectCommand = new RelayCommandOf<Archetype>(a =>
        {
            if (a.SubArchetypes.Count > 0)
                _navigator.Navigate(new SubArchetypePickerViewModel(_navigator, a));
            else
                _navigator.Navigate(new GameSelectionViewModel(_navigator, a));
        });
    }
}

/// <summary>Second-level picker for archetypes that have children.</summary>
public sealed class SubArchetypePickerViewModel : ViewModelBase
{
    private readonly Navigator _navigator;

    /// <summary>The parent archetype whose children are shown.</summary>
    public Archetype Parent { get; }
    /// <summary>Child archetypes to display.</summary>
    public ObservableCollection<Archetype> Children { get; }
    /// <summary>Command bound to a tapped child.</summary>
    public ICommand SelectCommand { get; }
    /// <summary>Command that returns to the previous picker.</summary>
    public ICommand BackCommand { get; }

    /// <summary>Initialises the sub-picker for <paramref name="parent"/>.</summary>
    public SubArchetypePickerViewModel(Navigator navigator, Archetype parent)
    {
        _navigator = navigator;
        Parent = parent;
        Children = new ObservableCollection<Archetype>(parent.SubArchetypes);
        BackCommand = new RelayCommand(() => _navigator.GoBack());
        SelectCommand = new RelayCommandOf<Archetype>(a =>
        {
            if (a.SubArchetypes.Count > 0)
                _navigator.Navigate(new SubArchetypePickerViewModel(_navigator, a));
            else
                _navigator.Navigate(new GameSelectionViewModel(_navigator, a));
        });
    }
}

/// <summary>Final picker: the playable modes inside one archetype node.</summary>
public sealed class GameSelectionViewModel : ViewModelBase
{
    private readonly Navigator _navigator;

    /// <summary>The archetype whose modes are listed.</summary>
    public Archetype Node { get; }

    /// <summary>
    /// The playable modes, resolved for display.
    ///
    /// Was <c>ObservableCollection&lt;IGameMode&gt;</c>, bound directly as
    /// <c>{Binding Name}</c> — which can never show a JSON title override,
    /// because <see cref="IGameMode"/> has no such member. MAUI's own
    /// <c>GameModeItem</c> exists specifically because that bug was found and
    /// fixed there; WinUI had the identical bug the whole time, because
    /// nothing shared the fix. Now <see cref="ModeListItem"/>, backed by the
    /// same <see cref="ModeDisplayResolver"/> MAUI uses.
    /// </summary>
    public ObservableCollection<ModeListItem> Modes { get; }

    /// <summary>Command bound to a chosen mode → player setup.</summary>
    public ICommand SelectCommand { get; }
    /// <summary>Command that returns to the previous picker.</summary>
    public ICommand BackCommand { get; }

    /// <summary>Initialises the game list for <paramref name="node"/>.</summary>
    public GameSelectionViewModel(Navigator navigator, Archetype node)
    {
        _navigator = navigator;
        Node = node;
        Modes = new ObservableCollection<ModeListItem>(node.Modes.Select(m => new ModeListItem(m)));
        BackCommand = new RelayCommand(() => _navigator.GoBack());
        SelectCommand = new RelayCommandOf<ModeListItem>(row =>
        {
            var m = row.Mode;
            // Backlog item 5: resolved from the composition root instead of
            // the hand-picked WinUIAppSettings.Instance singleton, so an
            // IAppSettings override registered in App.xaml.cs's container
            // actually reaches the screen that declares it as a dependency.
            _navigator.Navigate(new PlayerSetupViewModel(
                _navigator, m, _navigator.Services.GetRequiredService<IAppSettings>(),
                onStart: async players =>
                    _navigator.Navigate(await GameViewModelFactory.CreateAsync(_navigator, m, players)),
                rosterStore: new WinUIRosterStore()));
        });
    }
}

// PlayerSetupViewModel now lives in TableTop.Presentation and is shared.
// The two versions described the same screen in different words — NewName vs
// CurrentPlayerName, Mode vs GameMode, HasPlayers vs HasPrefilledPlayers.
// WinUI's vocabulary is canonical; MAUI's XAML was rebound to match.

public sealed class RelayCommandOf<T> : ICommand
{
    private readonly Action<T> _execute;

    /// <summary>Initialises the command with its typed action.</summary>
    public RelayCommandOf(Action<T> execute) => _execute = execute;

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged { add { } remove { } }

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => parameter is T;

    /// <inheritdoc />
    public void Execute(object? parameter) { if (parameter is T t) _execute(t); }
}
