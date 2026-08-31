namespace TableTop.Core.Abstractions.Analysis;

/// <summary>
/// Turns one player response to one item into that item's contribution to one
/// trait — and, critically, into the bounds that contribution sits between.
///
/// <para>
/// <b>Why this is not <c>IScoringStrategy</c>.</b> The existing scoring contract
/// returns <see cref="int"/>: a card produces a scalar delta and the engine adds
/// it to a running total. That is the right shape for a game with a winner, and
/// the wrong shape here for two independent reasons. A trait assessment produces
/// a <i>vector</i> — one running total per dimension, from one response — and
/// the totals are meaningless without knowing the range they could have fallen
/// in, because the answer a player wants is "where did I land", not "how many
/// points did I get". Bolting either onto <c>IScoringStrategy</c> would have
/// meant every existing scalar strategy growing members that mean nothing for
/// it, so this is a sibling contract rather than an extension of that one.
/// </para>
/// </summary>
public interface ITraitScoringStrategy
{
    /// <summary>Display name of this scoring model.</summary>
    string Name { get; }

    /// <summary>
    /// The contribution <paramref name="response"/> makes to
    /// <paramref name="traitKey"/> for <paramref name="item"/>, together with
    /// the minimum and maximum that contribution could have been.
    ///
    /// <para>
    /// Returns <c>null</c> when the item does not load on that trait at all,
    /// which is the common case: a 50-item Big Five bank has each item loading
    /// on one of five dimensions, so four out of five calls are misses. A null
    /// contributes nothing <i>and</i> widens no bounds — an item that cannot
    /// move a dimension must not count toward that dimension's denominator, or
    /// every score drifts toward the middle as the bank grows.
    /// </para>
    /// </summary>
    TraitContribution? Contribute(ITraitItemCard item, string traitKey, LikertResponse response);
}
