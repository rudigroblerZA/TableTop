namespace TableTop.Core.Abstractions.Analysis;

/// <summary>
/// How far apart two players landed on one dimension.
/// </summary>
public sealed class TraitDivergence
{
    /// <summary>Initialises a new <see cref="TraitDivergence"/>.</summary>
    /// <param name="trait">The dimension being compared.</param>
    /// <param name="left">The first player's score on it.</param>
    /// <param name="right">The second player's score on it.</param>
    public TraitDivergence(TraitDefinition trait, TraitScore left, TraitScore right)
    {
        ArgumentNullException.ThrowIfNull(trait);
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        Trait = trait;
        Left = left;
        Right = right;
        Difference = Math.Abs(left.Normalized - right.Normalized);
    }

    /// <summary>The dimension being compared.</summary>
    public TraitDefinition Trait { get; }

    /// <summary>The first player's score.</summary>
    public TraitScore Left { get; }

    /// <summary>The second player's score.</summary>
    public TraitScore Right { get; }

    /// <summary>Absolute gap between the two normalised scores, 0-100.</summary>
    public double Difference { get; }

    /// <summary>
    /// True when both sides actually answered items on this dimension.
    ///
    /// <para>
    /// A gap computed against a midpoint nobody earned is not a real gap, and
    /// the comparison excludes these from every aggregate for that reason.
    /// </para>
    /// </summary>
    public bool IsMeaningful => Left.HasData && Right.HasData;
}
