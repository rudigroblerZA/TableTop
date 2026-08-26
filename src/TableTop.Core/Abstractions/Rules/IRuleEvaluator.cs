using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Abstractions.Rules;

/// <summary>
/// Runs all registered rules in sequence for a given card and player.
/// Consumers depend on this interface, not on the concrete pipeline (DIP).
/// </summary>
public interface IRuleEvaluator
{
    /// <summary>
    /// Evaluates all registered rules for the card/player combination.
    /// Returns the first denial if any rule blocks the action, otherwise aggregated allow.
    /// </summary>
    RuleResult Evaluate(ICard card, IPlayer player, IRuleContext context);
}
