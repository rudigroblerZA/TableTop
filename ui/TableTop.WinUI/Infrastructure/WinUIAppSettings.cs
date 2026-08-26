using System.Text.Json;
using System.Text.Json.Serialization;
using TableTop.Presentation.Infrastructure;

namespace TableTop.WinUI.Infrastructure;

/// <summary>
/// A remembered player from a saved roster: display name plus an optional
/// gender ("male"/"female"/"other") and age. Gender/age are null when the
/// player didn't specify them. Serialised directly as JSON in settings.json.
/// </summary>
// SavedPlayer now lives in TableTop.Presentation.Infrastructure — it was
// declared identically in both heads, free to drift apart with nothing to
// catch it. One declaration now, referenced via the using above.

/// <summary>
/// Persists and exposes all user settings for the WinUI app — the desktop
/// counterpart to MAUI's <c>AppSettings</c>. WPF has no equivalent of MAUI's
/// Preferences API, so this writes a small JSON file next to the executable
/// instead (the same pattern <c>JsonPlayerRepository</c> already uses).
///
/// Two kinds of settings live here, matching the split established when
/// these were first wired to the real engine:
///   • Gameplay settings (shuffle, difficulty range, cards per player, age
///     floor) feed <see cref="TableTop.Core.Abstractions.Game.GameplayOptions"/>
///     and <see cref="TableTop.Hosting.ArchetypeFilter"/> — they genuinely
///     change what's offered and how a session plays.
///   • Display settings (theme, font size, badge visibility) stay purely
///     cosmetic, read directly by the views that show them.
/// </summary>
public sealed class WinUIAppSettings : TableTop.Presentation.Infrastructure.IAppSettings
{
    private static readonly string DefaultPath =
        Path.Combine(AppContext.BaseDirectory, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly string _filePath;
    private SettingsData _data;

    /// <summary>The app-lifetime singleton every screen reads and writes.</summary>
    public static WinUIAppSettings Instance { get; } = new();

    private WinUIAppSettings(string? filePath = null)
    {
        _filePath = filePath ?? DefaultPath;
        _data = Load();
    }

    /// <summary>Fired whenever any setting changes, with the changed property's name (or "*" for a full reset).</summary>
    public event EventHandler<string>? Changed;

    // ── Appearance ─────────────────────────────────────────────────────────

    /// <summary>App colour theme: "dark", "light", or "system". WPF currently only ships dark, but the setting is honoured if a light theme is added later.</summary>
    public string Theme
    {
        get => _data.Theme;
        set { _data.Theme = value; Persist(nameof(Theme)); }
    }

    /// <summary>Card body text size in points. Sensible range: 12–22.</summary>
    public int CardFontSize
    {
        get => _data.CardFontSize;
        set { _data.CardFontSize = Math.Clamp(value, 12, 22); Persist(nameof(CardFontSize)); }
    }

    // ── Gameplay — these reach the real engine via GameplayOptions ──────────

    /// <summary>Shuffle the card deck before each game.</summary>
    public bool ShuffleCards
    {
        get => _data.ShuffleCards;
        set { _data.ShuffleCards = value; Persist(nameof(ShuffleCards)); }
    }

    /// <summary>Minimum difficulty to include (0=Easy, 1=Medium, 2=Hard, 3=Extreme).</summary>
    public int MinDifficulty
    {
        get => _data.MinDifficulty;
        set { _data.MinDifficulty = Math.Clamp(value, 0, 3); Persist(nameof(MinDifficulty)); }
    }

    /// <summary>Maximum difficulty to include (0=Easy, 1=Medium, 2=Hard, 3=Extreme).</summary>
    public int MaxDifficulty
    {
        get => _data.MaxDifficulty;
        set { _data.MaxDifficulty = Math.Clamp(value, 0, 3); Persist(nameof(MaxDifficulty)); }
    }

    /// <summary>
    /// Age-rating FLOOR (0=AllAges, 1=Teen, 2=Adult) — hides games rated
    /// BELOW the selected value, matching MAUI's settings screen wording
    /// exactly ("Hides games below the selected age rating").
    /// </summary>
    public int MinAgeRating
    {
        get => _data.MinAgeRating;
        set { _data.MinAgeRating = Math.Clamp(value, 0, 2); Persist(nameof(MinAgeRating)); }
    }

    /// <summary>How many cards each player gets per session (0 = unlimited).</summary>
    public int CardsPerPlayer
    {
        get => _data.CardsPerPlayer;
        set { _data.CardsPerPlayer = Math.Max(0, value); Persist(nameof(CardsPerPlayer)); }
    }

    /// <summary>Enable a per-card countdown timer.</summary>
    public bool EnableTimer
    {
        get => _data.EnableTimer;
        set { _data.EnableTimer = value; Persist(nameof(EnableTimer)); }
    }

    /// <summary>Seconds for the per-card countdown timer.</summary>
    public int TimerSeconds
    {
        get => _data.TimerSeconds;
        set { _data.TimerSeconds = Math.Clamp(value, 10, 300); Persist(nameof(TimerSeconds)); }
    }

    // ── Display ────────────────────────────────────────────────────────────

    /// <summary>Show the "Card X of Y" progress indicator during play.</summary>
    public bool ShowCardCount
    {
        get => _data.ShowCardCount;
        set { _data.ShowCardCount = value; Persist(nameof(ShowCardCount)); }
    }

    /// <summary>
    /// Advance to the next player automatically after a card resolves.
    /// Added when the settings screen was shared: MAUI had always offered this
    /// and WinUI never did, so the same product behaved differently depending on
    /// which head you opened.
    /// </summary>
    public bool AutoNextPlayer
    {
        get => _data.AutoNextPlayer;
        set { _data.AutoNextPlayer = value; Persist(nameof(AutoNextPlayer)); }
    }

    /// <summary>Show the difficulty badge on each card.</summary>
    public bool ShowDifficultyBadge
    {
        get => _data.ShowDifficultyBadge;
        set { _data.ShowDifficultyBadge = value; Persist(nameof(ShowDifficultyBadge)); }
    }

    /// <summary>Show the category label on each card.</summary>
    public bool ShowCategoryBadge
    {
        get => _data.ShowCategoryBadge;
        set { _data.ShowCategoryBadge = value; Persist(nameof(ShowCategoryBadge)); }
    }

    // ── Roster memory ──────────────────────────────────────────────────────

    /// <summary>
    /// The roster saved from the setup screen's "Save roster" button — name
    /// plus optional gender/age for each player, so a regular group can
    /// pre-fill everyone next session instead of re-entering them. Unlike
    /// MAUI (which auto-saves at game start), WinUI only writes this when the
    /// user explicitly saves, so an experimental roster never clobbers the
    /// remembered one. Stored as real JSON objects — no delimiter encoding.
    /// </summary>
    public IReadOnlyList<SavedPlayer> RecentPlayers
    {
        get => _data.RecentPlayers;
        set { _data.RecentPlayers = value?.ToList() ?? []; Persist(nameof(RecentPlayers)); }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>Resets every setting to its factory default.</summary>
    public void ResetToDefaults()
    {
        _data = new SettingsData();
        Persist("*");
    }

    private SettingsData Load()
    {
        if (!File.Exists(_filePath)) return new SettingsData();
        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<SettingsData>(json, JsonOptions) ?? new SettingsData();
        }
        catch (JsonException)
        {
            return new SettingsData();   // corrupt file — start fresh rather than crash
        }
    }

    private void Persist(string changedKey)
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            var tmp = _filePath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(_data, JsonOptions));
            File.Move(tmp, _filePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Best-effort — a failed save shouldn't crash the app. Both are
            // named explicitly because they're the two real causes (disk full,
            // permissions denied) and only the first used to be caught here:
            // a permissions failure threw UnauthorizedAccessException straight
            // through this method instead of being swallowed like this comment
            // always claimed.
        }

        Changed?.Invoke(this, changedKey);
    }

    /// <summary>Plain data shape serialised to <c>settings.json</c>.</summary>
    private sealed class SettingsData
    {
        public string Theme { get; set; } = "dark";
        public int CardFontSize { get; set; } = 15;
        public bool ShuffleCards { get; set; } = true;
        public int MinDifficulty { get; set; } = 0;
        public int MaxDifficulty { get; set; } = 3;
        public int MinAgeRating { get; set; } = 0;
        public int CardsPerPlayer { get; set; } = 0;
        public bool EnableTimer { get; set; } = false;
        public int TimerSeconds { get; set; } = 60;
        public bool AutoNextPlayer { get; set; } = true;
        public bool ShowCardCount { get; set; } = true;
        public bool ShowDifficultyBadge { get; set; } = true;
        public bool ShowCategoryBadge { get; set; } = true;
        public List<SavedPlayer> RecentPlayers { get; set; } = [];
    }
}
