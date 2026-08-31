using TableTop.Core.Abstractions.Cards;

namespace TableTop.Core.Abstractions.Analysis;

/// <summary>
/// A card that is a statement to agree or disagree with, carrying the trait
/// dimensions it loads onto.
///
/// <para>
/// Extends <see cref="ICard"/> so every existing piece of engine machinery —
/// decks, shuffling, restrictions, the manifest — handles it without a special
/// case (LSP). The prompt shown to the player is <see cref="ICard.Description"/>,
/// exactly as it is for every other card shape.
/// </para>
///
/// <para>
/// <b>Weights carry two pieces of information in one number.</b> The
/// <i>sign</i> is the keying direction and the <i>magnitude</i> is the loading.
/// A weight of <c>-1.0</c> on "Openness" means agreeing with this statement
/// counts <i>against</i> Openness — the item is reverse-keyed — and it counts
/// with the same strength a <c>+1.0</c> item counts for it. Encoding keying as
/// a sign rather than a separate <c>bool IsReversed</c> is what lets an item
/// load on two dimensions in opposite directions, which a flag cannot express.
/// </para>
///
/// <para>
/// Reverse-keyed items are not decoration. Without them a player who agrees
/// with everything scores maximum on every dimension, and acquiescence bias is
/// the single easiest way to make a personality result meaningless. An item
/// bank that is all positively keyed measures agreeableness-with-the-quiz.
/// </para>
/// </summary>
public interface ITraitItemCard : ICard
{
    /// <summary>
    /// Trait key → weight. Sign is the keying direction, magnitude the loading.
    /// An item may load on more than one dimension; keys not present in the
    /// scoring scale are ignored by <c>TraitProfileBuilder</c> rather than
    /// throwing, so a typo costs a dimension's worth of signal instead of the
    /// whole session.
    /// </summary>
    IReadOnlyDictionary<string, double> TraitWeights { get; }
}
