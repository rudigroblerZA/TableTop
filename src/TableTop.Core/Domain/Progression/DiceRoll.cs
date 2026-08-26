using TableTop.Core.Abstractions.Cards;

namespace TableTop.Core.Domain.Progression;

/// <summary>
/// The result of rolling two dice.
///
/// <para>
/// Deliberately game-agnostic. This used to carry a <c>ToZone()</c> method
/// mapping the total straight to <see cref="MonogamyZone"/>, which meant no
/// other mode could use dice at all without dragging Monogamy's zone enum in —
/// discovered while adding a second dice-driven mode. The mapping moved to
/// where it belongs, next to the enum it produces; this record now knows
/// nothing beyond what two dice actually are.
/// </para>
/// </summary>
public sealed record DiceRoll(int Die1, int Die2)
{
    /// <summary>Sum of both dice.</summary>
    public int Total => Die1 + Die2;

    /// <summary>True when both dice show the same face.</summary>
    public bool IsDouble => Die1 == Die2;

    /// <summary>Rolls two dice using the supplied random source.</summary>
    public static DiceRoll Roll(Random? rng = null)
    {
        rng ??= Random.Shared;
        return new DiceRoll(rng.Next(1, 7), rng.Next(1, 7));
    }
}