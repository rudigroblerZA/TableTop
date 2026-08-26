using TableTop.Core.Abstractions.Cards;

namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Lightweight discovery metadata for a game mode.
///
/// A manifest is computed once and cached — it lets selection screens, "surprise me"
/// features, and difficulty filters inspect a mode's shape without building a full
/// deck or instantiating a controller.
///
/// Obtain via <see cref="ModeManifestExtensions.GetManifest"/>.
/// </summary>
public sealed record ModeManifest
{
    // ── Card counts ───────────────────────────────────────────────────────────

    /// <summary>Total number of playable cards (unrestricted, full catalogue).</summary>
    public int TotalCards { get; init; }

    /// <summary>
    /// Cards per difficulty tier. Keys are every <see cref="Difficulty"/> value
    /// present in the mode; missing tiers are absent (not zero-valued).
    /// </summary>
    public IReadOnlyDictionary<Difficulty, int> CardsByDifficulty { get; init; } =
        new Dictionary<Difficulty, int>();

    // ── Categories ────────────────────────────────────────────────────────────

    /// <summary>
    /// Distinct category names used by this mode's cards, sorted alphabetically.
    /// Empty when cards carry no category metadata.
    /// </summary>
    public IReadOnlyList<string> Categories { get; init; } = [];

    /// <summary>Number of cards per category, sorted by count descending.</summary>
    public IReadOnlyDictionary<string, int> CardsByCategory { get; init; } =
        new Dictionary<string, int>();

    // ── Tags ──────────────────────────────────────────────────────────────────

    /// <summary>Union of all tags across all cards in the mode.</summary>
    public IReadOnlyList<string> AllTags { get; init; } = [];

    // ── Content flags ─────────────────────────────────────────────────────────

    /// <summary>True when any card carries an adult-only restriction.</summary>
    public bool HasAdultContent { get; init; }

    /// <summary>True when any card carries a couple-only restriction.</summary>
    public bool HasCouplesContent { get; init; }

    // ── Estimated play time ───────────────────────────────────────────────────

    /// <summary>
    /// Estimated minimum play time, assuming 90 seconds per card and a
    /// typical group of 4 players. Rounded to the nearest 5 minutes.
    /// </summary>
    public TimeSpan EstimatedMinPlayTime { get; init; }

    /// <summary>
    /// Estimated maximum play time using the full card catalogue at 3 minutes
    /// per card (deliberation + discussion). Rounded to the nearest 5 minutes.
    /// </summary>
    public TimeSpan EstimatedMaxPlayTime { get; init; }

    // ── Convenience ───────────────────────────────────────────────────────────

    /// <summary>
    /// Human-readable play time range, e.g. "20–45 min".
    /// Returns "varies" when the range cannot be estimated.
    /// </summary>
    public string PlayTimeDisplay =>
        TotalCards == 0
            ? "varies"
            : $"{(int)EstimatedMinPlayTime.TotalMinutes}–{(int)EstimatedMaxPlayTime.TotalMinutes} min";

    /// <summary>
    /// Human-readable difficulty summary, e.g. "Easy · Medium · Hard" or "Mixed".
    /// Returns "Easy" when every card is Easy, "Mixed" when four tiers are present.
    /// </summary>
    public string DifficultyDisplay
    {
        get
        {
            if (CardsByDifficulty.Count == 0) return "—";
            if (CardsByDifficulty.Count == 1) return CardsByDifficulty.Keys.First().ToString();
            if (CardsByDifficulty.Count == 4) return "Mixed";
            return string.Join(" · ", CardsByDifficulty.Keys.OrderBy(d => d).Select(d => d.ToString()));
        }
    }

    /// <summary>Dominant difficulty tier (the one with the most cards).</summary>
    public Difficulty? DominantDifficulty =>
        CardsByDifficulty.Count == 0
            ? null
            : CardsByDifficulty.MaxBy(kv => kv.Value).Key;
}

/// <summary>
/// Optional capability interface. Modes that know their manifest at compile time
/// (static card banks) can implement this to return a cached manifest in O(1)
/// instead of having <see cref="ModeManifestExtensions"/> enumerate all cards.
/// </summary>
public interface IModeManifestProvider
{
    /// <summary>Returns the pre-computed manifest for this mode.</summary>
    ModeManifest GetManifest();
}
