using TableTop.Presentation.Infrastructure;
namespace TableTop.Maui.Services;

/// <summary>
/// A remembered player from a previous session: display name plus an optional
/// gender ("male"/"female"/"other") and age. Gender/age are null when the
/// player didn't specify them.
/// </summary>
// SavedPlayer now lives in TableTop.Presentation.Infrastructure — it was
// declared identically in both heads, free to drift apart with nothing to
// catch it. One declaration now, referenced via the using above.

/// <summary>
/// Persists and exposes all user settings via the MAUI Preferences API.
///
/// MAUI Preferences writes to:
///   Android  — SharedPreferences
///   iOS/Mac  — NSUserDefaults
///   Windows  — ApplicationData
///
/// All reads are cheap (in-memory after first read). Changes are immediately
/// written through. The app reads settings at startup; individual consumers
/// (GameplayViewModel, App.xaml.cs) subscribe to Changed to react at runtime.
/// </summary>
public sealed class AppSettings : IAppSettings
{
    // ── Preference keys ────────────────────────────────────────────────────
    private const string KeyTheme            = "tt_theme";
    private const string KeyCardFontSize     = "tt_card_font_size";
    private const string KeyShuffleCards     = "tt_shuffle_cards";
    private const string KeyMinDifficulty    = "tt_min_difficulty";
    private const string KeyMaxDifficulty    = "tt_max_difficulty";
    private const string KeyMinAgeRating     = "tt_min_age_rating";
    private const string KeyShowCardCount    = "tt_show_card_count";
    private const string KeyShowDifficulty   = "tt_show_difficulty";
    private const string KeyShowCategory     = "tt_show_category";
    private const string KeyAutoNextPlayer   = "tt_auto_next_player";
    private const string KeyCardsPerPlayer   = "tt_cards_per_player";
    private const string KeyTimerSeconds     = "tt_timer_seconds";
    private const string KeyEnableTimer      = "tt_enable_timer";
    private const string KeyRecentPlayers    = "tt_recent_players";

    // ── Singleton ──────────────────────────────────────────────────────────
    public static AppSettings Instance { get; } = new();
    private AppSettings() { }

    /// <summary>Fired whenever any setting changes. Consumers update their UI.</summary>
    public event EventHandler<string>? Changed;

    // ── Appearance ─────────────────────────────────────────────────────────

    /// <summary>App colour theme: "dark", "light", or "system".</summary>
    public string Theme
    {
        get => Preferences.Get(KeyTheme, "dark");
        set { Preferences.Set(KeyTheme, value); Notify(nameof(Theme)); }
    }

    /// <summary>Card body text size in SP units. Sensible range: 12–22.</summary>
    public int CardFontSize
    {
        get => Preferences.Get(KeyCardFontSize, 15);
        set { Preferences.Set(KeyCardFontSize, Math.Clamp(value, 12, 22)); Notify(nameof(CardFontSize)); }
    }

    // ── Gameplay ───────────────────────────────────────────────────────────

    /// <summary>Shuffle the card deck before each game.</summary>
    public bool ShuffleCards
    {
        get => Preferences.Get(KeyShuffleCards, true);
        set { Preferences.Set(KeyShuffleCards, value); Notify(nameof(ShuffleCards)); }
    }

    /// <summary>
    /// Minimum difficulty to include (0=Easy, 1=Medium, 2=Hard, 3=Extreme).
    /// Cards below this level are excluded.
    /// </summary>
    public int MinDifficulty
    {
        get => Preferences.Get(KeyMinDifficulty, 0);
        set { Preferences.Set(KeyMinDifficulty, Math.Clamp(value, 0, 3)); Notify(nameof(MinDifficulty)); }
    }

    /// <summary>
    /// Maximum difficulty to include (0=Easy, 1=Medium, 2=Hard, 3=Extreme).
    /// Cards above this level are excluded.
    /// </summary>
    public int MaxDifficulty
    {
        get => Preferences.Get(KeyMaxDifficulty, 3);
        set { Preferences.Set(KeyMaxDifficulty, Math.Clamp(value, 0, 3)); Notify(nameof(MaxDifficulty)); }
    }

    /// <summary>
    /// Minimum age rating to show (0=AllAges, 1=Teen, 2=Adult).
    /// Filters game modes below this rating from the selection screen.
    /// </summary>
    public int MinAgeRating
    {
        get => Preferences.Get(KeyMinAgeRating, 0);
        set { Preferences.Set(KeyMinAgeRating, Math.Clamp(value, 0, 2)); Notify(nameof(MinAgeRating)); }
    }

    /// <summary>
    /// How many cards each player gets per turn (0 = unlimited, play all).
    /// </summary>
    public int CardsPerPlayer
    {
        get => Preferences.Get(KeyCardsPerPlayer, 0);
        set { Preferences.Set(KeyCardsPerPlayer, Math.Max(0, value)); Notify(nameof(CardsPerPlayer)); }
    }

    /// <summary>Automatically advance to the next player after a card is completed.</summary>
    public bool AutoNextPlayer
    {
        get => Preferences.Get(KeyAutoNextPlayer, false);
        set { Preferences.Set(KeyAutoNextPlayer, value); Notify(nameof(AutoNextPlayer)); }
    }

    /// <summary>Enable per-card countdown timer.</summary>
    public bool EnableTimer
    {
        get => Preferences.Get(KeyEnableTimer, false);
        set { Preferences.Set(KeyEnableTimer, value); Notify(nameof(EnableTimer)); }
    }

    /// <summary>Seconds for the per-card countdown timer. 0 = off.</summary>
    public int TimerSeconds
    {
        get => Preferences.Get(KeyTimerSeconds, 60);
        set { Preferences.Set(KeyTimerSeconds, Math.Clamp(value, 10, 300)); Notify(nameof(TimerSeconds)); }
    }

    // ── Display ────────────────────────────────────────────────────────────

    /// <summary>Show the "Card X of Y" progress indicator on the gameplay screen.</summary>
    public bool ShowCardCount
    {
        get => Preferences.Get(KeyShowCardCount, true);
        set { Preferences.Set(KeyShowCardCount, value); Notify(nameof(ShowCardCount)); }
    }

    /// <summary>Show the difficulty badge on each card.</summary>
    public bool ShowDifficultyBadge
    {
        get => Preferences.Get(KeyShowDifficulty, true);
        set { Preferences.Set(KeyShowDifficulty, value); Notify(nameof(ShowDifficultyBadge)); }
    }

    /// <summary>Show the category label on each card.</summary>
    public bool ShowCategoryBadge
    {
        get => Preferences.Get(KeyShowCategory, true);
        set { Preferences.Set(KeyShowCategory, value); Notify(nameof(ShowCategoryBadge)); }
    }

    // ── Roster memory ──────────────────────────────────────────────────────

    /// <summary>
    /// The roster from the most recent game — name plus optional gender/age —
    /// so the setup screen can pre-fill everything instead of making the same
    /// group re-enter it each session. Stored one player per line in
    /// Preferences (primitives only), each line encoded as
    /// <c>name|gender|age</c> where gender/age may be empty.
    ///
    /// Reading tolerates the old name-only format (a line with no <c>|</c>),
    /// so upgrading doesn't wipe a previously-saved roster.
    /// </summary>
    public IReadOnlyList<SavedPlayer> RecentPlayers
    {
        get
        {
            var raw = Preferences.Get(KeyRecentPlayers, "");
            if (string.IsNullOrEmpty(raw)) return [];

            var list = new List<SavedPlayer>();
            foreach (var line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split('|');
                var name = parts[0].Trim();
                if (name.Length == 0) continue;
                var gender = parts.Length > 1 && parts[1].Length > 0 ? parts[1] : null;
                int? age = parts.Length > 2 && int.TryParse(parts[2], out var a) ? a : null;
                var couple = parts.Length > 3 && parts[3] == "1";
                list.Add(new SavedPlayer(name, gender, age, couple));
            }
            return list;
        }
        set
        {
            // Encode as name|gender|age per line. '|' and newline can't be
            // typed into the setup fields, so they're safe separators.
            var joined = value is null
                ? ""
                : string.Join('\n', value
                    .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                    .Select(p => $"{p.Name}|{p.Gender ?? ""}|{(p.Age?.ToString() ?? "")}|{(p.IsCoupleMember ? "1" : "0")}"));
            Preferences.Set(KeyRecentPlayers, joined);
            Notify(nameof(RecentPlayers));
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>Resets all settings to factory defaults.</summary>
    public void ResetToDefaults()
    {
        Theme            = "dark";
        CardFontSize     = 15;
        ShuffleCards     = true;
        MinDifficulty    = 0;
        MaxDifficulty    = 3;
        MinAgeRating     = 0;
        CardsPerPlayer   = 0;
        AutoNextPlayer   = false;
        EnableTimer      = false;
        TimerSeconds     = 60;
        ShowCardCount    = true;
        ShowDifficultyBadge = true;
        ShowCategoryBadge   = true;
        RecentPlayers    = [];

        Notify("*");
    }

    private void Notify(string key) => Changed?.Invoke(this, key);
}
