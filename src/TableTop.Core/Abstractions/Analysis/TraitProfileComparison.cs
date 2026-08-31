namespace TableTop.Core.Abstractions.Analysis;

/// <summary>
/// Two profiles read against each other — the analysis a couples game actually
/// wants, as opposed to two scorecards printed side by side.
///
/// <para>
/// <b>Similarity is the mean gap subtracted from 100, and nothing cleverer.</b>
/// A correlation across five points would be the textbook move and is the wrong
/// one here: with five dimensions it is wildly unstable, it is undefined when
/// either player answers flatly, and it reports <i>shape</i> agreement, so two
/// people who differ by a constant 30 points on every dimension score a perfect
/// 1.0. For "how alike are we", the average distance is both more honest and
/// the thing players think they are being told.
/// </para>
///
/// <para>
/// Dimensions where either side has no data are excluded from every aggregate
/// rather than counted as agreement. Scoring an unanswered dimension as a
/// zero-width gap is what turns a half-finished session into a suspiciously
/// high compatibility number.
/// </para>
/// </summary>
public sealed class TraitProfileComparison
{
    /// <summary>Initialises a new <see cref="TraitProfileComparison"/>.</summary>
    /// <param name="left">The first profile.</param>
    /// <param name="right">The second profile.</param>
    /// <param name="divergences">Per-dimension gaps, in the scale's order.</param>
    public TraitProfileComparison(
        TraitProfile left,
        TraitProfile right,
        IEnumerable<TraitDivergence> divergences)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        ArgumentNullException.ThrowIfNull(divergences);

        Left = left;
        Right = right;
        Divergences = divergences.ToList().AsReadOnly();

        var meaningful = Divergences.Where(d => d.IsMeaningful).ToList();
        ComparedDimensions = meaningful.Count;

        // No shared ground: report 50 and let ComparedDimensions say why, rather
        // than 100 ("identical") or 0 ("opposites"). Both of those are claims;
        // the midpoint with a count of zero beside it is not.
        Similarity = meaningful.Count == 0
            ? 50d
            : Math.Clamp(100d - meaningful.Average(d => d.Difference), 0d, 100d);

        // Ordinal tiebreak on key so a tie resolves the same way every run —
        // OrderBy is stable but the input order is the scale's, and a UI that
        // highlights "your biggest difference" flickering between two equal
        // dimensions across runs looks like a bug.
        GreatestDivergence = meaningful
            .OrderByDescending(d => d.Difference)
            .ThenBy(d => d.Trait.Key, StringComparer.Ordinal)
            .FirstOrDefault();

        ClosestAlignment = meaningful
            .OrderBy(d => d.Difference)
            .ThenBy(d => d.Trait.Key, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    /// <summary>The first profile.</summary>
    public TraitProfile Left { get; }

    /// <summary>The second profile.</summary>
    public TraitProfile Right { get; }

    /// <summary>Per-dimension gaps, in the scale's order.</summary>
    public IReadOnlyList<TraitDivergence> Divergences { get; }

    /// <summary>
    /// How alike the two profiles are, 0-100: 100 minus the mean gap across
    /// dimensions both players answered. 50 when they share none.
    /// </summary>
    public double Similarity { get; }

    /// <summary>How many dimensions both players actually answered items on.</summary>
    public int ComparedDimensions { get; }

    /// <summary>The dimension they differ on most, or null when they share none.</summary>
    public TraitDivergence? GreatestDivergence { get; }

    /// <summary>The dimension they are closest on, or null when they share none.</summary>
    public TraitDivergence? ClosestAlignment { get; }
}
