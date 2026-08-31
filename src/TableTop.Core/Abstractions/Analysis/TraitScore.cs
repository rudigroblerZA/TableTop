namespace TableTop.Core.Abstractions.Analysis;

/// <summary>
/// One player's result on one dimension.
///
/// <para>
/// Carries the raw total <i>and</i> the range it fell in, not just the
/// normalised percentage. Two players can both normalise to 60 off very
/// different item counts, and a results screen that wants to say "from 10
/// items" needs the arithmetic kept rather than thrown away once the
/// percentage is computed.
/// </para>
/// </summary>
public sealed class TraitScore
{
    /// <summary>Initialises a new <see cref="TraitScore"/>.</summary>
    /// <param name="trait">The dimension this score is for.</param>
    /// <param name="raw">Sum of every contribution to this dimension.</param>
    /// <param name="minimum">Sum of the minimum each contributing item could have given.</param>
    /// <param name="maximum">Sum of the maximum each contributing item could have given.</param>
    /// <param name="itemCount">How many answered items loaded on this dimension.</param>
    public TraitScore(TraitDefinition trait, double raw, double minimum, double maximum, int itemCount)
    {
        ArgumentNullException.ThrowIfNull(trait);
        ArgumentOutOfRangeException.ThrowIfNegative(itemCount);

        Trait = trait;
        Raw = raw;
        Minimum = minimum;
        Maximum = maximum;
        ItemCount = itemCount;
        Normalized = Normalize(raw, minimum, maximum);
        Band = BandFor(Normalized);
    }

    /// <summary>The dimension this score is for.</summary>
    public TraitDefinition Trait { get; }

    /// <summary>Sum of every contribution to this dimension.</summary>
    public double Raw { get; }

    /// <summary>Sum of the minimum each contributing item could have given.</summary>
    public double Minimum { get; }

    /// <summary>Sum of the maximum each contributing item could have given.</summary>
    public double Maximum { get; }

    /// <summary>How many answered items loaded on this dimension.</summary>
    public int ItemCount { get; }

    /// <summary>
    /// Where <see cref="Raw"/> sits between <see cref="Minimum"/> and
    /// <see cref="Maximum"/>, as 0-100.
    ///
    /// <para>
    /// <b>This is a position on this instrument's own scale, not a percentile.</b>
    /// 82 does not mean "higher than 82% of people" — it means the answers
    /// landed 82% of the way up the range these particular items could produce.
    /// Reporting it as a percentile would require a normed population sample,
    /// which a party game does not have and should not pretend to.
    /// </para>
    ///
    /// <para>
    /// A dimension nobody answered an item for normalises to 50 rather than 0.
    /// Zero is a real, meaningful score — it means every answer went the other
    /// way — and reporting "no data" as the strongest possible negative result
    /// is the kind of quiet lie that reads as a working feature.
    /// </para>
    /// </summary>
    public double Normalized { get; }

    /// <summary>Coarse label for <see cref="Normalized"/>.</summary>
    public TraitBand Band { get; }

    /// <summary>True when at least one answered item loaded on this dimension.</summary>
    public bool HasData => ItemCount > 0;

    /// <summary>
    /// Maps a raw total onto 0-100. Degenerate ranges — no items, or items that
    /// could only ever produce one value — return the midpoint, for the reason
    /// given on <see cref="Normalized"/>.
    /// </summary>
    public static double Normalize(double raw, double minimum, double maximum)
    {
        var span = maximum - minimum;

        // Not `span == 0`: these are doubles accumulated over many items, so an
        // exact zero is not something to rely on. A span this small cannot carry
        // a meaningful position regardless.
        if (span <= double.Epsilon) return 50d;

        return Math.Clamp((raw - minimum) / span * 100d, 0d, 100d);
    }

    /// <summary>The band a normalised 0-100 score falls in.</summary>
    public static TraitBand BandFor(double normalized) => normalized switch
    {
        < 20d => TraitBand.VeryLow,
        < 40d => TraitBand.Low,
        < 60d => TraitBand.Average,
        < 80d => TraitBand.High,
        _ => TraitBand.VeryHigh,
    };
}
