using TableTop.Core.Abstractions.Rules;
using TableTop.Core.Domain.Rules;

namespace TableTop.Tests;

public sealed class RuleEvaluatorTests
{
    private static IRuleContext MakeCtx()
    {
        var cards = TestFactory.MakeCards(3);
        var deck = new TableTop.Core.Domain.Decks.Deck(Guid.NewGuid(), "test", cards);
        return new RuleContext(1, [], deck, null);
    }

    [Fact]
    public void Evaluator_NoRules_ReturnsAllow()
    {
        var evaluator = new RuleEvaluator([]);
        var card = StandardCard.Create("T", "D", Difficulty.Easy, "Cat");
        var player = Player.Create("Alice");
        var result = evaluator.Evaluate(card, player, MakeCtx());
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluator_AllBuiltInRules_AllowValidCard()
    {
        var evaluator = new RuleEvaluator([new RestrictionRule(), new NoDuplicateCardRule()]);
        var card = StandardCard.Create("T", "D", Difficulty.Easy, "Cat");
        var player = Player.Create("Alice");
        var result = evaluator.Evaluate(card, player, MakeCtx());
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void RestrictionRule_BlocksAdultCard_ForNonAdultPlayer()
    {
        var rule = new RestrictionRule();
        var adultCard = new StandardCard(Guid.NewGuid(), "Adult", "d",
            Difficulty.Easy, "T", [], new TableTop.Core.Domain.Restrictions.AdultOnlyRestriction());
        var player = Player.Create("Alice"); // no adult tag
        var result = rule.Evaluate(adultCard, player, MakeCtx());
        result.IsAllowed.Should().BeFalse();
    }

    [Fact]
    public void RestrictionRule_AllowsAdultCard_ForAdultPlayer()
    {
        var rule = new RestrictionRule();
        var adultCard = new StandardCard(Guid.NewGuid(), "Adult", "d",
            Difficulty.Easy, "T", [], new TableTop.Core.Domain.Restrictions.AdultOnlyRestriction());
        var player = Player.Create("Alice", tags: new[] { "adult" });
        var result = rule.Evaluate(adultCard, player, MakeCtx());
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void NoDuplicateCardRule_AllowsFirstPlay()
    {
        var rule = new NoDuplicateCardRule();
        var card = StandardCard.Create("T", "D", Difficulty.Easy, "Cat");
        var player = Player.Create("Alice");
        var result = rule.Evaluate(card, player, MakeCtx());
        result.IsAllowed.Should().BeTrue();
    }
}
