using TableTop.Core.Abstractions.Analysis;

namespace TableTop.Core.Domain.Analysis;

/// <summary>
/// The default trait scoring model: reverse-key by reflecting the response
/// across the scale, then weight by the item's loading.
///
/// <para>
/// <b>The arithmetic, in full.</b> For an item weighting trait <c>k</c> by
/// <c>w</c>, and a response <c>r</c> on a scale running
/// <see cref="Minimum"/>..<see cref="Maximum"/>:
/// </para>
///
/// <code>
///     effective    = w &gt;= 0  ?  r  :  (Minimum + Maximum - r)
///     contribution = |w| * effective
/// </code>
///
/// <para>
/// Reflection rather than negation is the part worth understanding. Negating
/// (<c>-r</c>) would make a reverse-keyed item's contribution range run
/// -5..-1 while a forward item's runs 1..5, so the two could not be summed into
/// one total without a correction somewhere else. Reflecting maps 1↔5 and 2↔4
/// and fixes 3, which keeps every item — forward or reverse — contributing on
/// the same 1..5 range. That is what lets the totals simply add up, and it is
/// the same transform paper inventories describe as "reverse-score item 4".
/// </para>
///
/// <para>
/// Bounds are reported per item as <c>|w| * Minimum</c> and <c>|w| * Maximum</c>,
/// which is what makes a heavily-loaded item widen the denominator as much as it
/// widens the numerator. Without that, an item weighted 2.0 could push a score
/// past the top of a range computed as though every item weighed 1.0 —
/// <see cref="TraitScore.Normalize"/> clamps, so the visible symptom would be a
/// dimension pinned at 100 rather than an obvious error.
/// </para>
/// </summary>
public sealed class WeightedLikertScoring : ITraitScoringStrategy
{
    /// <summary>Lowest value on the response scale — <see cref="LikertResponse.StronglyDisagree"/>.</summary>
    public const int Minimum = (int)LikertResponse.StronglyDisagree;

    /// <summary>Highest value on the response scale — <see cref="LikertResponse.StronglyAgree"/>.</summary>
    public const int Maximum = (int)LikertResponse.StronglyAgree;

    /// <inheritdoc />
    public string Name => "Weighted Likert";

    /// <inheritdoc />
    public TraitContribution? Contribute(ITraitItemCard item, string traitKey, LikertResponse response)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(traitKey);

        if (!item.TraitWeights.TryGetValue(traitKey, out var weight) || weight == 0d)
            return null;

        var raw = (int)response;
        var effective = weight >= 0d ? raw : Minimum + Maximum - raw;
        var loading = Math.Abs(weight);

        return new TraitContribution(
            loading * effective,
            loading * Minimum,
            loading * Maximum);
    }
}
