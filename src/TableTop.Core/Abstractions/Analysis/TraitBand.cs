namespace TableTop.Core.Abstractions.Analysis;

/// <summary>
/// A coarse label for a normalised trait score, so a UI can say "high" without
/// every head re-deciding where "high" starts.
///
/// <para>
/// <b>These bands are scale-relative, not norm-referenced.</b> They describe
/// where a player landed between the lowest and highest score the item set
/// could produce — not where they sit against a population. A real inventory
/// reports percentiles against a normed sample; this reports a position on its
/// own scale. The distinction matters enough that
/// <see cref="TraitScore.Normalized"/> repeats it, because "you scored 82 on
/// Openness" reads as a percentile to almost everyone who sees it, and here it
/// is not one.
/// </para>
/// </summary>
public enum TraitBand
{
    /// <summary>Bottom fifth of the available range.</summary>
    VeryLow = 0,

    /// <summary>Second fifth.</summary>
    Low = 1,

    /// <summary>Middle fifth — the item set did not push either way.</summary>
    Average = 2,

    /// <summary>Fourth fifth.</summary>
    High = 3,

    /// <summary>Top fifth of the available range.</summary>
    VeryHigh = 4,
}
