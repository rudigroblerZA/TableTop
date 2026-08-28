using Android.Content;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Droid.Infrastructure;

/// <summary>
/// <see cref="IAppSettings"/> backed by Android <see cref="ISharedPreferences"/>.
///
/// <para>
/// A near-line-for-line port of MAUI's <c>Services/AppSettings.cs</c>: the same
/// keys, the same defaults, the same <c>name|gender|age|couple</c> line encoding
/// for <see cref="RecentPlayers"/> (which also tolerates the older name-only
/// format), and the same immediate write-through with a <see cref="Changed"/>
/// notification. Only the storage backend differs — MAUI persists through
/// <c>Microsoft.Maui.Storage.Preferences</c>, which on Android is exactly this.
/// </para>
/// </summary>
public sealed class AndroidAppSettings : IAppSettings
{
    private const string PrefsName = "tabletop";

    private const string KeyTheme = "tt_theme";
    private const string KeyCardFontSize = "tt_card_font_size";
    private const string KeyShuffleCards = "tt_shuffle_cards";
    private const string KeyMinDifficulty = "tt_min_difficulty";
    private const string KeyMaxDifficulty = "tt_max_difficulty";
    private const string KeyMinAgeRating = "tt_min_age_rating";
    private const string KeyShowCardCount = "tt_show_card_count";
    private const string KeyShowDifficulty = "tt_show_difficulty";
    private const string KeyShowCategory = "tt_show_category";
    private const string KeyAutoNextPlayer = "tt_auto_next_player";
    private const string KeyCardsPerPlayer = "tt_cards_per_player";
    private const string KeyTimerSeconds = "tt_timer_seconds";
    private const string KeyEnableTimer = "tt_enable_timer";
    private const string KeyRecentPlayers = "tt_recent_players";

    private readonly ISharedPreferences _prefs;

    /// <summary>Opens the shared-preferences file this head stores its settings in.</summary>
    public AndroidAppSettings(Context context) =>
        _prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private)!;

    /// <inheritdoc />
    public event EventHandler<string>? Changed;

    // ── Appearance ────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public string Theme
    {
        get => _prefs.GetString(KeyTheme, "dark")!;
        set => Put(KeyTheme, value, nameof(Theme));
    }

    /// <inheritdoc />
    public int CardFontSize
    {
        get => _prefs.GetInt(KeyCardFontSize, 15);
        set => Put(KeyCardFontSize, Math.Clamp(value, 12, 22), nameof(CardFontSize));
    }

    // ── Gameplay ──────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public bool ShuffleCards
    {
        get => _prefs.GetBoolean(KeyShuffleCards, true);
        set => Put(KeyShuffleCards, value, nameof(ShuffleCards));
    }

    /// <inheritdoc />
    public int MinDifficulty
    {
        get => _prefs.GetInt(KeyMinDifficulty, 0);
        set => Put(KeyMinDifficulty, Math.Clamp(value, 0, 3), nameof(MinDifficulty));
    }

    /// <inheritdoc />
    public int MaxDifficulty
    {
        get => _prefs.GetInt(KeyMaxDifficulty, 3);
        set => Put(KeyMaxDifficulty, Math.Clamp(value, 0, 3), nameof(MaxDifficulty));
    }

    /// <inheritdoc />
    public int MinAgeRating
    {
        get => _prefs.GetInt(KeyMinAgeRating, 0);
        set => Put(KeyMinAgeRating, Math.Clamp(value, 0, 2), nameof(MinAgeRating));
    }

    /// <inheritdoc />
    public int CardsPerPlayer
    {
        get => _prefs.GetInt(KeyCardsPerPlayer, 0);
        set => Put(KeyCardsPerPlayer, Math.Max(0, value), nameof(CardsPerPlayer));
    }

    /// <inheritdoc />
    public bool AutoNextPlayer
    {
        get => _prefs.GetBoolean(KeyAutoNextPlayer, false);
        set => Put(KeyAutoNextPlayer, value, nameof(AutoNextPlayer));
    }

    /// <inheritdoc />
    public bool EnableTimer
    {
        get => _prefs.GetBoolean(KeyEnableTimer, false);
        set => Put(KeyEnableTimer, value, nameof(EnableTimer));
    }

    /// <inheritdoc />
    public int TimerSeconds
    {
        get => _prefs.GetInt(KeyTimerSeconds, 60);
        set => Put(KeyTimerSeconds, Math.Clamp(value, 10, 300), nameof(TimerSeconds));
    }

    // ── Display toggles ───────────────────────────────────────────────────────

    /// <inheritdoc />
    public bool ShowCardCount
    {
        get => _prefs.GetBoolean(KeyShowCardCount, true);
        set => Put(KeyShowCardCount, value, nameof(ShowCardCount));
    }

    /// <inheritdoc />
    public bool ShowDifficultyBadge
    {
        get => _prefs.GetBoolean(KeyShowDifficulty, true);
        set => Put(KeyShowDifficulty, value, nameof(ShowDifficultyBadge));
    }

    /// <inheritdoc />
    public bool ShowCategoryBadge
    {
        get => _prefs.GetBoolean(KeyShowCategory, true);
        set => Put(KeyShowCategory, value, nameof(ShowCategoryBadge));
    }

    // ── Roster ────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public IReadOnlyList<SavedPlayer> RecentPlayers
    {
        get
        {
            var raw = _prefs.GetString(KeyRecentPlayers, "")!;
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
            var joined = value is null
                ? ""
                : string.Join('\n', value
                    .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                    .Select(p => $"{p.Name}|{p.Gender ?? ""}|{(p.Age?.ToString() ?? "")}|{(p.IsCoupleMember ? "1" : "0")}"));
            Put(KeyRecentPlayers, joined, nameof(RecentPlayers));
        }
    }

    /// <inheritdoc />
    public void ResetToDefaults()
    {
        Theme = "dark";
        CardFontSize = 15;
        ShuffleCards = true;
        MinDifficulty = 0;
        MaxDifficulty = 3;
        MinAgeRating = 0;
        CardsPerPlayer = 0;
        AutoNextPlayer = false;
        EnableTimer = false;
        TimerSeconds = 60;
        ShowCardCount = true;
        ShowDifficultyBadge = true;
        ShowCategoryBadge = true;
        RecentPlayers = [];

        Changed?.Invoke(this, "*");
    }

    private void Put(string key, string value, string name)
    {
        _prefs.Edit()!.PutString(key, value)!.Apply();
        Changed?.Invoke(this, name);
    }

    private void Put(string key, int value, string name)
    {
        _prefs.Edit()!.PutInt(key, value)!.Apply();
        Changed?.Invoke(this, name);
    }

    private void Put(string key, bool value, string name)
    {
        _prefs.Edit()!.PutBoolean(key, value)!.Apply();
        Changed?.Invoke(this, name);
    }
}
