using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Abstractions.Rules;

/// <summary>
/// Encapsulates a single piece of game logic that can be independently evaluated.
/// Rules are open for extension by registering new implementations (OCP).
/// </summary>
public interface IRule
{
    /// <summary>Unique name identifying this rule.</summary>
    string Name { get; }

    /// <summary>Human-readable explanation of what this rule does.</summary>
    string Description { get; }

    /// <summary>
    /// Evaluates the rule for the given card and player in the current game context.
    /// </summary>
    RuleResult Evaluate(ICard card, IPlayer player, IRuleContext context);
}
