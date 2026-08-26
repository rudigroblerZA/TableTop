using TableTop.Core.Abstractions;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Rules;

namespace TableTop.Core.Domain.Rules;

/// <summary>
/// Runs all registered <see cref="IRule"/> instances in sequence.
/// Returns the first denial encountered, or an aggregated allow with summed score deltas.
/// When an <see cref="IEngineDiagnostics"/> sink is provided, each rule decision is
/// recorded — useful for diagnosing "why does this card keep getting skipped?"
/// </summary>
public sealed class RuleEvaluator : IRuleEvaluator
{
    private readonly IReadOnlyList<IRule> _rules;
    private readonly IEngineDiagnostics _diagnostics;

    /// <summary>Creates a RuleEvaluator with the default no-op diagnostics.</summary>
    public RuleEvaluator(IEnumerable<IRule> rules)
        : this(rules, NullEngineDiagnostics.Instance) { }

    /// <summary>Creates a RuleEvaluator with a diagnostics sink.</summary>
    public RuleEvaluator(IEnumerable<IRule> rules, IEngineDiagnostics diagnostics)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(diagnostics);
        _rules = rules.ToList().AsReadOnly();
        _diagnostics = diagnostics;
    }

    /// <inheritdoc />
    public RuleResult Evaluate(ICard card, IPlayer player, IRuleContext context)
    {
        var totalScore = 0;

        foreach (var rule in _rules)
        {
            var result = rule.Evaluate(card, player, context);
            if (!result.IsAllowed)
            {
                _diagnostics.RuleDenied(rule, card, player, result.Reason ?? rule.Name);
                return result;
            }
            if (result.ScoreDelta != 0)
                _diagnostics.RuleAllowed(rule, card, player, result.ScoreDelta);
            totalScore += result.ScoreDelta;
        }

        return RuleResult.Allow(totalScore);
    }
}
