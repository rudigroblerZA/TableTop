namespace TableTop.Presentation.Infrastructure;

/// <summary>
/// A player remembered between sessions.
///
/// Previously declared twice — once in MAUI's <c>AppSettings.cs</c> and once in
/// WinUI's <c>WinUIAppSettings.cs</c> — as identical records that could drift
/// apart without anything noticing. One declaration now.
///
/// <para>
/// <paramref name="Team"/> is null for every roster built before the Roaster's
/// "Team" template dealt sides (backlog item 26), and stays null for any
/// template that doesn't. When set, <c>PlayerSetupViewModel.LoadRoster</c>
/// carries it into <c>IPlayer.Attributes["team"]</c> via <c>Teams</c>, so a
/// saved Team roster starts a team mode with sides already assigned instead of
/// an unassigned table. The delimited <c>RecentPlayers</c> encoding in each
/// head's settings store does not persist it — that list is a flat setup
/// prefill, not a roster.
/// </para>
/// </summary>
public sealed record SavedPlayer(
    string Name, string? Gender, int? Age, bool IsCoupleMember = false, string? Team = null);

/// <summary>
/// The settings surface a shared ViewModel can rely on.
///
/// <para>
/// This interface was not designed so much as <i>discovered</i>: MAUI's
/// <c>AppSettings</c> and WinUI's <c>WinUIAppSettings</c> had already converged
/// on the same thirteen properties independently, differing only in MAUI's extra
/// <c>AutoNextPlayer</c>, which WinUI has now gained rather than MAUI losing it. Two stores maintained separately, agreeing by
/// coincidence rather than by contract, is precisely the drift this project
/// keeps paying for — so the agreement is written down here and compiled.
/// </para>
///
/// <para>
/// Each head keeps its own <i>storage</i>: MAUI persists through
/// <c>Microsoft.Maui.Storage.Preferences</c>, WinUI through its own local
/// store. Only the shape is shared, which is the part that was duplicated.
/// </para>
/// </summary>
public interface IAppSettings
{
    /// <summary>Raised when any setting changes; the payload is the property name.</summary>
    event EventHandler<string>? Changed;

    // ── Appearance ────────────────────────────────────────────────────────────

    /// <summary>"dark", "light", or "system".</summary>
    string Theme { get; set; }

    /// <summary>Card body font size in points.</summary>
    int CardFontSize { get; set; }

    // ── Gameplay ──────────────────────────────────────────────────────────────

    /// <summary>Shuffle the deck before each game.</summary>
    bool ShuffleCards { get; set; }

    /// <summary>Difficulty floor (0=Easy … 3=Extreme).</summary>
    int MinDifficulty { get; set; }

    /// <summary>Difficulty ceiling (0=Easy … 3=Extreme).</summary>
    int MaxDifficulty { get; set; }

    /// <summary>Age-rating floor (0=AllAges, 1=Teen, 2=Adult).</summary>
    int MinAgeRating { get; set; }

    /// <summary>Cards dealt per player; 0 means the whole deck.</summary>
    int CardsPerPlayer { get; set; }

    /// <summary>Advance to the next player automatically after a card resolves.</summary>
    bool AutoNextPlayer { get; set; }

    /// <summary>Whether a per-card countdown runs.</summary>
    bool EnableTimer { get; set; }

    /// <summary>Countdown length in seconds when <see cref="EnableTimer"/> is set.</summary>
    int TimerSeconds { get; set; }

    // ── Display toggles ───────────────────────────────────────────────────────

    /// <summary>Show the "card N of M" progress line.</summary>
    bool ShowCardCount { get; set; }

    /// <summary>Show the difficulty badge on each card.</summary>
    bool ShowDifficultyBadge { get; set; }

    /// <summary>Show the category badge on each card.</summary>
    bool ShowCategoryBadge { get; set; }

    // ── Roster ────────────────────────────────────────────────────────────────

    /// <summary>The remembered roster, saved explicitly rather than on game start.</summary>
    IReadOnlyList<SavedPlayer> RecentPlayers { get; set; }

    /// <summary>Restores every setting to its default.</summary>
    void ResetToDefaults();
}
