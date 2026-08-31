using TableTop.Core.Abstractions.Analysis;

namespace TableTop.Core.Domain.Analysis;

/// <summary>
/// Reads two profiles against each other.
///
/// <para>
/// Static because it holds nothing: the comparison is a pure function of two
/// profiles. Kept out of <see cref="TraitProfileComparison"/>'s constructor so
/// the dimension-pairing rules below are testable on their own, and so a caller
/// holding two profiles from different sources can compare them without going
/// back through a builder.
/// </para>
/// </summary>
public static class TraitProfileComparer
{
    /// <summary>
    /// Compares two profiles dimension by dimension.
    ///
    /// <para>
    /// <b>Pairing is by trait key, driven by the left profile's scale.</b> Two
    /// profiles scored against different instruments only share the dimensions
    /// whose keys match; anything the right profile does not have is skipped
    /// rather than compared against a zero. Comparing a Big Five profile to a
    /// three-dimension one should report on the three they share and say so via
    /// <see cref="TraitProfileComparison.ComparedDimensions"/>, not invent
    /// agreement on two dimensions one side never measured.
    /// </para>
    /// </summary>
    /// <param name="left">The first profile.</param>
    /// <param name="right">The second profile.</param>
    public static TraitProfileComparison Compare(TraitProfile left, TraitProfile right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var divergences = new List<TraitDivergence>();

        foreach (var trait in left.Scale.Traits)
        {
            if (left.Find(trait.Key) is not { } l) continue;
            if (right.Find(trait.Key) is not { } r) continue;

            divergences.Add(new TraitDivergence(trait, l, r));
        }

        return new TraitProfileComparison(left, right, divergences);
    }

    /// <summary>
    /// Compares every pair in <paramref name="profiles"/>, once per unordered
    /// pair, in input order.
    ///
    /// <para>
    /// For a couples game this is the two-player case and returns a single
    /// comparison. It generalises to a table because the party modes here run
    /// to six or eight players, where "who in this room is most like me" is the
    /// question worth answering — and at those sizes the pair count (28 at
    /// eight players) is still trivial to compute.
    /// </para>
    /// </summary>
    public static IReadOnlyList<TraitProfileComparison> CompareAll(IEnumerable<TraitProfile> profiles)
    {
        ArgumentNullException.ThrowIfNull(profiles);

        var list = profiles.ToList();
        var pairs = new List<TraitProfileComparison>();

        for (var i = 0; i < list.Count; i++)
            for (var j = i + 1; j < list.Count; j++)
                pairs.Add(Compare(list[i], list[j]));

        return pairs.AsReadOnly();
    }

    /// <summary>
    /// The most similar pair in <paramref name="profiles"/>, or null when fewer
    /// than two profiles were supplied or no pair shares a measured dimension.
    /// </summary>
    public static TraitProfileComparison? MostAlike(IEnumerable<TraitProfile> profiles) =>
        CompareAll(profiles)
            .Where(c => c.ComparedDimensions > 0)
            .OrderByDescending(c => c.Similarity)
            .ThenBy(c => c.Left.PlayerName, StringComparer.Ordinal)
            .ThenBy(c => c.Right.PlayerName, StringComparer.Ordinal)
            .FirstOrDefault();

    /// <summary>
    /// The least similar pair, or null under the same conditions as
    /// <see cref="MostAlike"/>.
    /// </summary>
    public static TraitProfileComparison? MostDifferent(IEnumerable<TraitProfile> profiles) =>
        CompareAll(profiles)
            .Where(c => c.ComparedDimensions > 0)
            .OrderBy(c => c.Similarity)
            .ThenBy(c => c.Left.PlayerName, StringComparer.Ordinal)
            .ThenBy(c => c.Right.PlayerName, StringComparer.Ordinal)
            .FirstOrDefault();
}
