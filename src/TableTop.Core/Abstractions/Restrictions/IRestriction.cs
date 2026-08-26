using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Abstractions.Restrictions;

/// <summary>
/// Determines whether a player or group satisfies the conditions for a card.
/// Restrictions are independently evaluatable and combinable via AND / OR / NOT (ISP, OCP).
/// </summary>
public interface IRestriction
{
    /// <summary>
    /// Evaluates the restriction against a player and the full player context.
    /// </summary>
    /// <param name="player">The player being evaluated.</param>
    /// <param name="context">All players in the session (for group restrictions).</param>
    /// <returns>True when the player satisfies the restriction.</returns>
    bool IsSatisfiedBy(IPlayer player, IReadOnlyList<IPlayer> context);

    /// <summary>Human-readable description of this restriction.</summary>
    string Description { get; }
}
