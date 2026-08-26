using System.Collections.ObjectModel;
using TableTop.Core.Abstractions.Game;
using TableTop.Hosting;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Maui.ViewModels;

public sealed class GameSelectionViewModel : BindableObject
{
    // Was a duplicate of WinUI's IntroViewModel resume lookup, near
    // byte-for-byte. One implementation now, in TableTop.Presentation.
    private readonly SavedSessionLookup _savedSession = new();
    private readonly IAppSettings _settings;

    /// <summary>True when there is a saved session worth offering.</summary>
    public bool CanResume => _savedSession.CanResume;

    /// <summary>"Continue — Alice, Bob · round 4", or empty.</summary>
    public string ResumeText => _savedSession.ResumeText;

    /// <summary>The resolved session, for the page to hand to gameplay.</summary>
    public ResumableSession? Resumable => _savedSession.Resumable;

    /// <summary>
    /// Looks for a saved session so the landing page can offer it.
    ///
    /// On a phone this is the case that matters most: the OS terminates
    /// backgrounded apps, so a call mid-game used to lose the session outright.
    /// </summary>
    public async Task LookForSavedSessionAsync()
    {
        await _savedSession.RefreshAsync();
        OnPropertyChanged(nameof(CanResume));
        OnPropertyChanged(nameof(ResumeText));
    }

    private Archetype? _selectedArchetype;
    private Archetype? _selectedSubArchetype;
    private IGameMode? _selectedGameMode;

    /// <summary>
    /// True while the bound collections are being rebuilt.
    ///
    /// Rebuilding clears and refills collections that CollectionViews are
    /// actively rendering, and nulls the selections — which pushes back through
    /// the TwoWay SelectedItem bindings and re-raises SelectionChanged. Without
    /// a guard the view feeds those events straight back into the ViewModel and
    /// mutates the collections re-entrantly, which on Android crashes the
    /// underlying RecyclerView with an inconsistency error, typically after two
    /// or three interactions rather than the first.
    ///
    /// GameSelectionPage checks this and ignores selection events raised while
    /// it is set.
    /// </summary>
    public bool IsRebuilding { get; private set; }

    public ObservableCollection<Archetype> Archetypes { get; }
    public ObservableCollection<Archetype> SubArchetypes { get; } = [];
    /// <summary>
    /// Rows for the game list. <see cref="GameModeItem"/> rather than raw
    /// <c>IGameMode</c> so a <c>BaseGameModeDefinition</c> subclass's own name
    /// and description can be shown — binding the domain object directly
    /// could never surface them, since <c>IGameMode</c> has no such members.
    /// </summary>
    public ObservableCollection<GameModeItem> GameModes { get; } = [];

    /// <summary>
    /// The chosen top-level category. Setting this cascades into
    /// <see cref="SubArchetypes"/> and <see cref="GameModes"/>.
    ///
    /// GameSelectionPage sets these properties from code-behind rather than
    /// through bindings on the item templates: inside a CollectionView
    /// DataTemplate a Frame eats the tap on Android, and an {x:Reference}
    /// command binding can't resolve out of the template's namescope, so the
    /// binding route failed silently. See GameSelectionPage.xaml.cs.
    /// </summary>
    public Archetype? SelectedArchetype
    {
        get => _selectedArchetype;
        set
        {
            if (_selectedArchetype == value) return;
            _selectedArchetype = value;
            OnPropertyChanged();
            UpdateSubArchetypes();
        }
    }

    public Archetype? SelectedSubArchetype
    {
        get => _selectedSubArchetype;
        set
        {
            if (_selectedSubArchetype == value) return;
            _selectedSubArchetype = value;
            OnPropertyChanged();
            UpdateGameModes();
        }
    }

    public IGameMode? SelectedGameMode
    {
        get => _selectedGameMode;
        set
        {
            if (_selectedGameMode == value) return;
            _selectedGameMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedItem));
        }
    }

    /// <summary>
    /// The row matching <see cref="SelectedGameMode"/>, for the CollectionView's
    /// two-way SelectedItem binding. Kept as a projection rather than a second
    /// piece of state, so the mode remains the single source of truth for what
    /// is selected.
    /// </summary>
    public GameModeItem? SelectedItem
    {
        get => GameModes.FirstOrDefault(i => ReferenceEquals(i.Mode, _selectedGameMode));
        set => SelectedGameMode = value?.Mode;
    }

    public GameSelectionViewModel(IAppSettings settings)
    {
        _settings = settings;
        Archetypes = new ObservableCollection<Archetype>(BuildFilteredArchetypes());
        _settings.Changed += OnSettingsChanged;
    }

    /// <summary>
    /// Applies the player's age-rating ceiling (Settings → "Show games up
    /// to…") to the picker. This was previously stored but never actually
    /// hid anything — <see cref="ArchetypeFilter"/> is what makes the
    /// setting real.
    /// </summary>
    private IReadOnlyList<Archetype> BuildFilteredArchetypes()
    {
        // AppSettings.MinAgeRating is genuinely a FLOOR here — the Settings
        // page's own label says "Hides games BELOW the selected rating", so
        // selecting "Adult" shows ONLY Adult content, not everything up to
        // Adult. Confirmed against the actual UI copy before wiring this —
        // it reads unusually (most apps want a ceiling), but it's consistent
        // across the property name, its doc comment, and the settings label,
        // so that's the real, deliberate behaviour to honour.
        var floor = (AgeRating)_settings.MinAgeRating;
        return new ArchetypeFilter(minAgeRating: floor, maxAgeRating: AgeRating.Adult)
            .Apply(ArchetypeRegistry.Default().RootArchetypes);
    }

    private void OnSettingsChanged(object? sender, string key)
    {
        if (key != nameof(IAppSettings.MinAgeRating) && key != "*") return;

        Archetypes.Clear();
        foreach (var a in BuildFilteredArchetypes()) Archetypes.Add(a);
        SelectedArchetype = null;
        SubArchetypes.Clear();
        GameModes.Clear();
    }

    private void UpdateSubArchetypes()
    {
        if (IsRebuilding) return;      // never rebuild from inside a rebuild
        IsRebuilding = true;
        try
        {
            SubArchetypes.Clear();
            GameModes.Clear();
            SelectedSubArchetype = null;
            SelectedGameMode = null;

            if (SelectedArchetype is null) return;

            foreach (var sub in SelectedArchetype.SubArchetypes)
                SubArchetypes.Add(sub);

            // Direct modes of a leaf archetype. If this archetype is a pure branch
            // (no direct modes) the list stays empty until a variant is picked,
            // which is the intended flow — but a leaf must still be playable.
            foreach (var mode in SelectedArchetype.Modes)
                GameModes.Add(new GameModeItem(mode));

            // Auto-select single sub-archetype
            if (SubArchetypes.Count == 1)
                SelectedSubArchetype = SubArchetypes[0];
        }
        finally { IsRebuilding = false; }
    }

    private void UpdateGameModes()
    {
        var reentrant = IsRebuilding;
        IsRebuilding = true;
        try
        {
            GameModes.Clear();
            SelectedGameMode = null;

            if (SelectedSubArchetype is null) return;

            // Gather modes from the whole subtree, not just direct children.
            //
            // This picker is three levels deep (type → variant → game). The archetype
            // tree used to be deeper in one place — "Classroom → Grade 6" had its own
            // sub-archetypes and NO direct modes, so reading only .Modes produced an
            // empty game list and selection looked broken. Grade 6 has since been
            // flattened into Classroom and every node now carries its own modes, so
            // nothing currently needs this recursion.
            //
            // Kept anyway: it is correct for any depth, and the alternative is a
            // picker that silently shows nothing the next time someone adds a
            // grouping node. The bug it was written for was invisible until a player
            // hit it.
            foreach (var mode in CollectModes(SelectedSubArchetype))
                GameModes.Add(new GameModeItem(mode));

            // .Mode, not the row: GameModes holds GameModeItem wrappers and
            // SelectedGameMode is the IGameMode itself. SelectedGameModeItem is the
            // property that takes a row, and it unwraps to this one.
            if (GameModes.Count == 1)
                SelectedGameMode = GameModes[0].Mode;
        }
        finally { IsRebuilding = reentrant; }
    }

    /// <summary>
    /// Every mode at or beneath <paramref name="node"/>, depth-first, so a
    /// branch node still yields a playable list.
    /// </summary>
    private static IEnumerable<IGameMode> CollectModes(Archetype node)
    {
        foreach (var mode in node.Modes)
            yield return mode;

        foreach (var child in node.SubArchetypes)
            foreach (var mode in CollectModes(child))
                yield return mode;
    }
}
