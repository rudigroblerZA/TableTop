using System.Windows.Input;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Presentation.ViewModels;

/// <summary>
/// The settings screen, shared by every head.
///
/// <para>
/// This replaces two separate implementations — MAUI's 153-line
/// <c>BindableObject</c> version and WinUI's ~45-line <c>ViewModelBase</c> one —
/// which had already drifted: MAUI exposed theme, font size and timer controls
/// that WinUI simply never offered, so the same product had different settings
/// depending on which head you opened.
/// </para>
///
/// <para>
/// The surface here is the <b>union</b> of the two, not the intersection.
/// Dropping MAUI's extra properties to make the merge easy would have removed
/// working features from a shipping head; exposing them everywhere instead
/// means WinUI gains settings it should always have had. A head that has no
/// control for a property simply doesn't bind it — that costs nothing, whereas
/// a missing property costs a feature.
/// </para>
///
/// <para>
/// Every setter writes through to <see cref="IAppSettings"/> immediately, which
/// each head persists in its own way. No save button, matching what both
/// implementations already did.
/// </para>
/// </summary>
public sealed class SettingsViewModel : ViewModelBase
{
    private readonly IAppSettings _s;

    /// <summary>Returns to the previous screen.</summary>
    public ICommand BackCommand { get; }

    /// <summary>Resets every setting to its default and refreshes the whole screen.</summary>
    public ICommand ResetCommand { get; }

    /// <summary>Initialises the settings screen.</summary>
    /// <param name="navigator">Used only to go back.</param>
    /// <param name="settings">The head's own settings store.</param>
    public SettingsViewModel(INavigator navigator, IAppSettings settings)
    {
        _s = settings;
        BackCommand = new RelayCommand(navigator.GoBack);

        // Passing null to OnPropertyChanged tells binding engines "every property
        // changed" — the whole screen re-reads after a reset, without listing
        // fifteen names that would go stale the moment one is added.
        ResetCommand = new RelayCommand(ResetToDefaults);
    }

    // Plain method alongside ResetCommand: WinUI binds the ICommand,
    // MAUI's SettingsPage.xaml.cs calls this directly after its own
    // confirmation dialog. Same underlying action either way.

    /// <summary>Resets every setting to its default and refreshes the whole screen.</summary>
    public void ResetToDefaults()
    {
        _s.ResetToDefaults();
        // null tells binding engines "every property changed" — the whole
        // screen re-reads after a reset, without listing every name would
        // itself go stale the moment a setting is added.
        OnPropertyChanged(null);
    }

    // ── Option lists, for pickers ─────────────────────────────────────────────
    // Previously MAUI-only. Static display data, so they cost WinUI nothing
    // until it binds them.

    /// <summary>Display labels for <see cref="ThemeIndex"/>.</summary>
    public IReadOnlyList<string> ThemeOptions { get; } = ["Dark", "Light", "System"];
    /// <summary>Display labels for <see cref="FontSizeIndex"/>.</summary>
    public IReadOnlyList<string> FontSizeOptions { get; } = ["Small (12)", "Medium (15)", "Large (18)", "Extra Large (20)"];
    /// <summary>Display labels for the difficulty bounds.</summary>
    public IReadOnlyList<string> DifficultyOptions { get; } = ["Easy", "Medium", "Hard", "Extreme"];
    /// <summary>
    /// Display labels for <see cref="MinAgeRatingIndex"/>.
    ///
    /// WinUI's wording, which explained what the setting actually does rather
    /// than naming the rating. It was hardcoded in that head's markup; MAUI bound
    /// the ViewModel's terser list and got the worse labels. Now both get these.
    /// </summary>
    public IReadOnlyList<string> AgeOptions { get; } =
        ["All ages (show everything)", "Teen and up", "Adult only"];
    /// <summary>Display labels for <see cref="TimerIndex"/>.</summary>
    public IReadOnlyList<string> TimerOptions { get; } = ["30 seconds", "60 seconds", "90 seconds", "2 minutes", "3 minutes", "5 minutes"];

    private static readonly int[] FontSizeValues = [12, 15, 18, 20];
    private static readonly int[] TimerSecValues = [30, 60, 90, 120, 180, 300];

    // ── Appearance ────────────────────────────────────────────────────────────

    /// <summary>Index into <see cref="ThemeOptions"/>.</summary>
    public int ThemeIndex
    {
        get => _s.Theme switch { "light" => 1, "system" => 2, _ => 0 };
        set { _s.Theme = value switch { 1 => "light", 2 => "system", _ => "dark" }; OnPropertyChanged(); }
    }

    /// <summary>Index into <see cref="FontSizeOptions"/>.</summary>
    public int FontSizeIndex
    {
        get { var i = Array.IndexOf(FontSizeValues, _s.CardFontSize); return i < 0 ? 1 : i; }
        set { _s.CardFontSize = FontSizeValues[Math.Clamp(value, 0, FontSizeValues.Length - 1)]; OnPropertyChanged(); }
    }

    // ── Gameplay ──────────────────────────────────────────────────────────────

    /// <summary>Shuffle the deck before each game.</summary>
    public bool ShuffleCards
    {
        get => _s.ShuffleCards;
        set { _s.ShuffleCards = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Difficulty floor. Cross-clamped with the ceiling so the pair can never
    /// invert — both heads implemented this identically, which is a good sign
    /// it belongs in one place.
    /// </summary>
    public int MinDifficultyIndex
    {
        get => _s.MinDifficulty;
        set
        {
            _s.MinDifficulty = value;
            if (_s.MaxDifficulty < value) _s.MaxDifficulty = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MaxDifficultyIndex));
        }
    }

    /// <summary>Difficulty ceiling, cross-clamped with <see cref="MinDifficultyIndex"/>.</summary>
    public int MaxDifficultyIndex
    {
        get => _s.MaxDifficulty;
        set
        {
            _s.MaxDifficulty = value;
            if (_s.MinDifficulty > value) _s.MinDifficulty = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MinDifficultyIndex));
        }
    }

    /// <summary>Index into <see cref="AgeOptions"/>.</summary>
    public int MinAgeRatingIndex
    {
        get => _s.MinAgeRating;
        set { _s.MinAgeRating = value; OnPropertyChanged(); }
    }

    /// <summary>Cards dealt per player; 0 means the whole deck.</summary>
    public int CardsPerPlayer
    {
        get => _s.CardsPerPlayer;
        set { _s.CardsPerPlayer = value; OnPropertyChanged(); }
    }

    /// <summary>Advance to the next player automatically after a card resolves.</summary>
    public bool AutoNextPlayer
    {
        get => _s.AutoNextPlayer;
        set { _s.AutoNextPlayer = value; OnPropertyChanged(); }
    }

    // ── Timer ─────────────────────────────────────────────────────────────────

    /// <summary>Whether a per-card countdown runs.</summary>
    public bool EnableTimer
    {
        get => _s.EnableTimer;
        set { _s.EnableTimer = value; OnPropertyChanged(); }
    }

    /// <summary>Index into <see cref="TimerOptions"/>.</summary>
    public int TimerIndex
    {
        get { var i = Array.IndexOf(TimerSecValues, _s.TimerSeconds); return i < 0 ? 1 : i; }
        set { _s.TimerSeconds = TimerSecValues[Math.Clamp(value, 0, TimerSecValues.Length - 1)]; OnPropertyChanged(); }
    }

    // ── Display toggles ───────────────────────────────────────────────────────

    /// <summary>Show the "card N of M" progress line.</summary>
    public bool ShowCardCount
    {
        get => _s.ShowCardCount;
        set { _s.ShowCardCount = value; OnPropertyChanged(); }
    }

    /// <summary>Show the difficulty badge on each card.</summary>
    public bool ShowDifficultyBadge
    {
        get => _s.ShowDifficultyBadge;
        set { _s.ShowDifficultyBadge = value; OnPropertyChanged(); }
    }

    /// <summary>Show the category badge on each card.</summary>
    public bool ShowCategoryBadge
    {
        get => _s.ShowCategoryBadge;
        set { _s.ShowCategoryBadge = value; OnPropertyChanged(); }
    }
}
