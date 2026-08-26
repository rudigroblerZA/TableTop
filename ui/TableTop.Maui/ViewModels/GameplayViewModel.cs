using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Abstractions;
using TableTop.Presentation.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui.ViewModels;

/// <summary>
/// Thin MAUI wrapper around the shared <see cref="CardTurnGameViewModel"/> —
/// backlog item 1, the last real duplication, closed. This used to be 733
/// lines independently reimplementing everything the shared class now does;
/// what's left is only what genuinely cannot move: platform <see cref="Color"/>
/// values, resolved font families, and settings properties that must update
/// live while the screen is showing, none of which the shared, platform-free
/// project can reference or provide.
///
/// <para>
/// <b>Why this screen still needs a wrapper and Monogamy/Millionaire/Day One
/// don't.</b> Those three bind the shared ViewModel directly — no per-head
/// class at all. This one can't: MAUI's page renders a per-mode themed card
/// (<see cref="Theme"/>), a category-coloured strip with WCAG-checked ink
/// (<see cref="StripColor"/>/<see cref="StripTextColor"/>), and resolved font
/// families, none of which the other three screens have. Every one of those
/// needs a live platform <c>Color</c>/font-family value the shared project
/// cannot return.
/// </para>
///
/// <para>
/// <b>Live settings reactivity is MAUI-only, and deliberately stays here.</b>
/// The shared class's <c>ShowCardCount</c> is fixed once, at construction —
/// correct for WinUI, which has never re-read settings mid-session. MAUI's
/// page must, because a player can background the app, change a setting, and
/// come back to the same screen. The container-resolved <see cref="IAppSettings"/>
/// stays subscribed here, re-raising only the four properties actually
/// affected (see <see cref="OnSettingChanged"/>) — the shared instance never
/// needs to know this exists.
/// </para>
///
/// <para>
/// <b>What is a pure pass-through below</b> — every property with a name
/// matching the shared class exactly — needed no XAML changes to keep
/// working: <c>{Binding CardBodyText}</c> resolves against this wrapper,
/// which forwards to <see cref="_inner"/>'s value of the same name. Two
/// bindings did need renaming, because the shared class exposes flattened
/// strings instead of the raw domain objects MAUI bound sub-properties of
/// directly: <c>CurrentPlayer.DisplayName</c> → <c>PlayerName</c>,
/// <c>CurrentCard.Title</c> → <c>CardTitle</c>. <see cref="GameplayPage.xaml"/>
/// reflects both.
/// </para>
/// </summary>
public sealed class GameplayViewModel : BindableObject, IDisposable
{
    private readonly CardTurnGameViewModel _inner;
    private readonly IReadOnlyDictionary<string, string> _categoryColours;
    private readonly IAppSettings _settings;

    /// <summary>The visual skin for this mode. Defaults to baize for anything with no dedicated palette.</summary>
    public Theming.ModeTheme Theme { get; }

    // ── Fonts ─────────────────────────────────────────────────────────────────
    //
    // A deck's JSON can name a font family; App.xaml's own fonts are OnPlatform
    // values, so the fallback has to be resolved at runtime rather than baked
    // into the theme record.

    /// <summary>Heading font: the deck's if it named one, else the app default.</summary>
    public string DisplayFont => Theme.DisplayFont ?? AppFont("DisplayFont", "serif");
    /// <summary>Body font: the deck's if it named one, else the app default.</summary>
    public string BodyFont => Theme.BodyFont ?? AppFont("BodyFont", "serif");
    /// <summary>Counter/label font: the deck's if it named one, else the app default.</summary>
    public string UtilityFont => Theme.UtilityFont ?? AppFont("UtilityFont", "monospace");

    /// <summary>
    /// Reads a font family out of the app's resource dictionary. Never throws
    /// — a missing resource (design-time, unit tests) falls back silently
    /// rather than taking the screen down with it.
    /// </summary>
    private static string AppFont(string key, string fallback)
    {
        try
        {
            if (Application.Current?.Resources.TryGetValue(key, out var value) == true &&
                value is string family && !string.IsNullOrWhiteSpace(family))
                return family;
        }
        catch (Exception) { /* no application context; fall through */ }
        return fallback;
    }

    // ── Settings passthrough — live, unlike the shared class's ctor-only copy ──

    /// <summary>Whether to show the deck-count line.</summary>
    public bool ShowCardCount => _settings.ShowCardCount;
    /// <summary>Whether the card strip shows the difficulty badge.</summary>
    public bool ShowDifficultyBadge => _settings.ShowDifficultyBadge;
    /// <summary>Whether the card strip shows the category badge.</summary>
    public bool ShowCategoryBadge => _settings.ShowCategoryBadge;
    /// <summary>Card body font size, settings-driven.</summary>
    public double CardFontSize => _settings.CardFontSize;

    // ── Card face / strip colour — platform Color, computed from shared state ──

    /// <summary>The card's face colour — question stock, or the warmer answer stock once flipped.</summary>
    public Color CardFaceColor => _inner.IsFlipped ? Theme.CardStockFlipped : Theme.CardStock;

    /// <summary>Strip caption: category · difficulty (per settings), or ANSWER.</summary>
    public string StripText
    {
        get
        {
            if (_inner.IsFlipped) return "ANSWER";
            var parts = new List<string>(2);
            if (ShowCategoryBadge && _inner.CardCategory.Length > 0) parts.Add(_inner.CardCategory);
            if (ShowDifficultyBadge && _inner.CardDifficulty.Length > 0) parts.Add(_inner.CardDifficulty);
            return string.Join("  ·  ", parts);
        }
    }
    /// <summary>True when the strip has anything to say.</summary>
    public bool HasStrip => StripText.Length > 0;

    /// <summary>
    /// Strip colour. Precedence, highest first: amber when flipped to the
    /// answer face, the mode's own category-colour map, then difficulty
    /// colouring for modes that define no category map.
    /// </summary>
    public Color StripColor
    {
        get
        {
            if (_inner.IsFlipped) return Color.FromArgb("#A36A18");
            if (_inner.CardCategory.Length > 0 && _categoryColours.TryGetValue(_inner.CardCategory, out var hex))
                return Color.FromArgb(hex);
            return Color.FromArgb(_inner.CardDifficulty switch
            {
                "Easy" => "#2EA043",
                "Medium" => "#D29922",
                "Hard" => "#DB6D28",
                "Extreme" => "#DA3633",
                _ => "#C49E4C",
            });
        }
    }

    /// <summary>
    /// Text colour for the category strip, WCAG-contrast-picked per strip
    /// colour rather than fixed white — 73% of the catalogue's category
    /// colours fail 4.5:1 contrast with white bold 13px text.
    /// </summary>
    public Color StripTextColor => PickInk(StripColor);

    private static Color PickInk(Color background)
    {
        static double Channel(double c) =>
            c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);

        var luminance = 0.2126 * Channel(background.Red)
                      + 0.7152 * Channel(background.Green)
                      + 0.0722 * Channel(background.Blue);

        var againstWhite = 1.05 / (luminance + 0.05);
        var againstBlack = (luminance + 0.05) / 0.05;

        return againstBlack > againstWhite ? Colors.Black : Colors.White;
    }

    // ── Pass-through: everything below has a same-named property on _inner ──

    /// <summary>Current player's display name.</summary>
    public string PlayerName => _inner.PlayerName;
    /// <summary>Card title.</summary>
    public string CardTitle => _inner.CardTitle;
    /// <summary>Face-up text, prompt-resolved and HTML-stripped.</summary>
    public string CardBodyText => _inner.CardBodyText;
    /// <summary>Live scoreboard line.</summary>
    public string ScoresText => _inner.ScoresText;
    /// <summary>True once any score has been recorded.</summary>
    public bool HasScores => _inner.HasScores;
    /// <summary>Transient feedback line.</summary>
    public string FlashText => _inner.FlashText;
    /// <summary>True when there is a flash line to show.</summary>
    public bool HasFlash => _inner.HasFlash;
    /// <summary>Fraction of the session played, 0–1.</summary>
    public double Progress => _inner.Progress;
    /// <summary>"Round 3 · 19 cards left".</summary>
    public string CardCountText => _inner.CardCountText;
    /// <summary>Round number reported by the engine.</summary>
    public int Round => _inner.Round;
    /// <summary>Whether a per-card countdown runs.</summary>
    public bool TimerEnabled => _inner.TimerEnabled;
    /// <summary>"MM:SS" for the countdown.</summary>
    public string TimerDisplay => _inner.TimerDisplay;
    /// <summary>Hex colour for the hint urgency.</summary>
    public string HintColor => _inner.HintColor;
    /// <summary>True when a hint is worth showing.</summary>
    public bool HasHint => _inner.HasHint;
    /// <summary>The engine's next-turn hint text.</summary>
    public string HintText => _inner.HintText;
    /// <summary>Flip button caption for the current face.</summary>
    public string FlipButtonText => _inner.FlipButtonText;
    /// <summary>True when this card has a hidden answer face.</summary>
    public bool HasBack => _inner.HasBack;
    /// <summary>True when the current card is a tap-a-letter quiz card.</summary>
    public bool HasChoices => _inner.HasChoices;
    /// <summary>True for an ordinary card with no letter choices.</summary>
    public bool HasNoChoices => _inner.HasNoChoices;
    /// <summary>False for modes whose progression strategy isn't flow-aware.</summary>
    public bool SupportsFlow => _inner.SupportsFlow;
    /// <summary>True once a turn has been recorded and not yet undone.</summary>
    public bool CanUndo => _inner.CanUndo;
    /// <summary>True when a session is running and can be saved.</summary>
    public bool CanSave => _inner.CanSave;
    /// <summary>Skip button label, mode-specific.</summary>
    public string SkipLabel => _inner.SkipLabel;
    /// <summary>Positive-outcome button label, mode-specific.</summary>
    public string CompleteLabel => _inner.CompleteLabel;
    /// <summary>The mode's display title, JSON override applied.</summary>
    public string ModeTitle => _inner.ModeTitle;
    /// <summary>True once the engine has ended the game.</summary>
    public bool IsGameOver => _inner.IsGameOver;
    /// <summary>True while the session is live and loadable.</summary>
    public bool IsPlaying => _inner.IsPlaying;
    /// <summary>Controller-build failure message, or empty.</summary>
    public string LoadError => _inner.LoadError;
    /// <summary>True when the mode could not be started.</summary>
    public bool HasLoadError => _inner.HasLoadError;

    /// <summary>Raised once with the compiled final summary.</summary>
    public event Action<string>? GameOver
    {
        add { _inner.GameOver += value; }
        remove { _inner.GameOver -= value; }
    }

    // ── Actions — direct forwarding, same names as the shared class ─────────

    /// <summary>Records the current card's outcome as complete.</summary>
    public void Complete() => _inner.Complete();
    /// <summary>Skips the current card.</summary>
    public void Skip() => _inner.Skip();
    /// <summary>Ends the game early.</summary>
    public void Quit() => _inner.Quit();
    /// <summary>Saves the session so it can be resumed.</summary>
    public void SaveSession() => _inner.SaveSession();
    /// <summary>Reverses the last turn and re-presents its card.</summary>
    public void UndoLastTurn() => _inner.UndoLastTurn();
    /// <summary>Flips a two-faced card.</summary>
    public void FlipCard() => _inner.FlipCard();
    /// <summary>Tallies the current player's answer, then completes the turn.</summary>
    public void RecordChoice(char letter) => _inner.RecordChoice(letter);
    /// <summary>Nudges everyone's difficulty up.</summary>
    public void LevelUp() => _inner.LevelUp();
    /// <inheritdoc cref="LevelUp" />
    public void LevelDown() => _inner.LevelDown();
    /// <inheritdoc cref="LevelUp" />
    public void SpeedUp() => _inner.SpeedUp();
    /// <inheritdoc cref="LevelUp" />
    public void SlowDown() => _inner.SlowDown();

    // ── Construction ─────────────────────────────────────────────────────────

    /// <summary>
    /// Drives a gameplay session. Pass <paramref name="resumeFrom"/> to continue
    /// a saved session instead of starting fresh; obtain one from
    /// <c>ControllerFactory.LoadSavedSessionAsync</c>.
    /// </summary>
    public GameplayViewModel(
        INavigator navigator, IGameMode gameMode, List<IPlayer> players,
        TableTop.Hosting.Persistence.SessionSnapshot? resumeFrom = null)
    {
        // Skin and category colours come from the mode, read before the
        // controller is built so a controller failure still lands on a
        // correctly-themed error state rather than a half-styled screen.
        Theme = Theming.ModeTheme.For(gameMode);
        var definition = gameMode as TableTop.Games.Base.BaseGameModeDefinition;
        _categoryColours = definition?.CategoryColours ?? new Dictionary<string, string>();

        // Backlog item 5: IControllerFactory/IAppSettings resolved from the
        // app's container (MauiProgram.cs's AddTableTopHosting()) rather than
        // AppSettings.Instance and an implicit `new ControllerFactory()`
        // inside CreateAsync — same idiom GameSelectionPage.xaml.cs already
        // uses to reach SettingsPage from code that isn't itself
        // DI-constructed. A custom IControllerFactory registered in the
        // container had no effect on a real session before this.
        var services = IPlatformApplication.Current!.Services;
        _settings = services.GetRequiredService<IAppSettings>();
        var controllerFactory = services.GetRequiredService<IControllerFactory>();

        // Blocking is deadlock-free here, same as every prior MAUI merge
        // (MillionaireGamePage, DayOneGamePage) — CreateAsync itself catches
        // a controller-build failure into LoadError, so this constructor
        // needs no try/catch of its own the way the old implementation did.
        _inner = CardTurnGameViewModel.CreateAsync(
                navigator, gameMode, players.AsReadOnly(), _settings, resumeFrom, controllerFactory)
            .GetAwaiter().GetResult();

        // Forwards every property-changed notification 1:1 — this is what
        // makes every pass-through property above stay live without each one
        // needing its own explicit re-raise.
        _inner.PropertyChanged += (_, e) => OnPropertyChanged(e.PropertyName);

        _settings.Changed += OnSettingChanged;
    }

    private void OnSettingChanged(object? sender, string key)
    {
        switch (key)
        {
            case nameof(IAppSettings.ShowCardCount): OnPropertyChanged(nameof(ShowCardCount)); break;
            case nameof(IAppSettings.ShowDifficultyBadge): OnPropertyChanged(nameof(ShowDifficultyBadge)); RaiseStrip(); break;
            case nameof(IAppSettings.ShowCategoryBadge): OnPropertyChanged(nameof(ShowCategoryBadge)); RaiseStrip(); break;
            case nameof(IAppSettings.CardFontSize): OnPropertyChanged(nameof(CardFontSize)); break;
            case "*": OnPropertyChanged(null); break;
        }
    }

    private void RaiseStrip()
    {
        OnPropertyChanged(nameof(StripText));
        OnPropertyChanged(nameof(HasStrip));
        OnPropertyChanged(nameof(StripColor));
        OnPropertyChanged(nameof(StripTextColor));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _settings.Changed -= OnSettingChanged;
        _inner.Dispose();
    }
}
