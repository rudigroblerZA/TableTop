namespace TableTop.Core.Abstractions.Analysis;

/// <summary>
/// A player's answer to a trait item, on the five-point agreement scale that
/// personality inventories conventionally use.
///
/// <para>
/// <b>The numeric values are load-bearing and deliberately 1-5, not 0-4.</b>
/// <see cref="Domain.Analysis.WeightedLikertScoring"/> reverse-keys an item by
/// computing <c>Minimum + Maximum - response</c>, which maps 1↔5 and 2↔4 and
/// leaves 3 fixed. That identity only holds when the endpoints are the actual
/// enum values, so casting this enum to <see cref="int"/> is part of the
/// contract rather than an implementation detail — hence explicit values on
/// every member rather than relying on declaration order.
/// </para>
/// </summary>
public enum LikertResponse
{
    /// <summary>The statement is clearly untrue of the player.</summary>
    StronglyDisagree = 1,

    /// <summary>The statement is more untrue than true of the player.</summary>
    Disagree = 2,

    /// <summary>The player is neutral, or the statement does not apply.</summary>
    Neutral = 3,

    /// <summary>The statement is more true than untrue of the player.</summary>
    Agree = 4,

    /// <summary>The statement is clearly true of the player.</summary>
    StronglyAgree = 5,
}
