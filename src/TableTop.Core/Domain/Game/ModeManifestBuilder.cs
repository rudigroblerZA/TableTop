using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;

namespace TableTop.Core.Domain.Game;

/// <summary>
/// Computes a <see cref="ModeManifest"/> by inspecting card metadata.
/// No deck is built, no rules are run, no players are needed.
///
/// Called by <see cref="ModeManifestExtensions.GetManifest"/> when the mode
/// does not implement <see cref="IModeManifestProvider"/> itself.
/// </summary>
public static class ModeManifestBuilder
{
    // Seconds per card assumptions used for play-time estimates
    private const int SecondsPerCardMin = TableTopDefaults.Manifest.SecondsPerCardMin;
    private const int SecondsPerCardMax = TableTopDefaults.Manifest.SecondsPerCardMax;

    /// <summary>
    /// Builds a manifest from an arbitrary card list.
    /// The list should be the mode's full unrestricted catalogue.
    /// </summary>
    public static ModeManifest Build(IReadOnlyList<ICard> cards)
    {
        if (cards.Count == 0)
            return new ModeManifest { TotalCards = 0 };

        // ── Difficulty histogram ───────────────────────────────────────────

        var byDifficulty = cards
            .GroupBy(c => c.Difficulty)
            .ToDictionary(g => g.Key, g => g.Count());

        // ── Category histogram ─────────────────────────────────────────────

        var byCategory = cards
            .Where(c => !string.IsNullOrWhiteSpace(c.Category))
            .GroupBy(c => c.Category, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var categories = byCategory.Keys.OrderBy(c => c).ToList().AsReadOnly();

        // ── Tags ───────────────────────────────────────────────────────────

        var allTags = cards
            .SelectMany(c => c.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList()
            .AsReadOnly();

        // ── Content flags ──────────────────────────────────────────────────

        var hasAdult = cards.Any(c =>
            c.Restriction?.Description?.Contains("adult", StringComparison.OrdinalIgnoreCase) == true
            || c.Tags.Any(t => t.Equals("adult", StringComparison.OrdinalIgnoreCase)));

        var hasCouples = cards.Any(c =>
            c.Restriction?.Description?.Contains("couple", StringComparison.OrdinalIgnoreCase) == true
            || c.Tags.Any(t => t.Equals("couple-member", StringComparison.OrdinalIgnoreCase)
                             || t.Equals("couples", StringComparison.OrdinalIgnoreCase)));

        // ── Play time estimate ─────────────────────────────────────────────

        var minSec = cards.Count * SecondsPerCardMin;
        var maxSec = cards.Count * SecondsPerCardMax;

        return new ModeManifest
        {
            TotalCards = cards.Count,
            CardsByDifficulty = byDifficulty,
            Categories = categories,
            CardsByCategory = byCategory,
            AllTags = allTags,
            HasAdultContent = hasAdult,
            HasCouplesContent = hasCouples,
            EstimatedMinPlayTime = RoundToFiveMinutes(TimeSpan.FromSeconds(minSec)),
            EstimatedMaxPlayTime = RoundToFiveMinutes(TimeSpan.FromSeconds(maxSec)),
        };
    }

    private static TimeSpan RoundToFiveMinutes(TimeSpan t)
    {
        var totalMins = (int)Math.Ceiling(t.TotalMinutes);
        var rounded = (int)(Math.Ceiling(totalMins / 5.0) * 5);
        return TimeSpan.FromMinutes(Math.Max(5, rounded));
    }
}
