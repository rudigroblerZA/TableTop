using TableTop.Core.Abstractions.Analysis;
using TableTop.Core.Domain.Analysis;
using TableTop.Games.Fun;

namespace TableTop.Tests;

/// <summary>
/// Big Five's content, and the properties its results depend on.
///
/// <para>
/// The item bank is the part most likely to be edited by someone adding a
/// statement, and the damage from getting it wrong is silent — a tilted
/// dimension still produces a plausible number. These tests are what turn
/// "someone counted the items" into something a build can check.
/// </para>
/// </summary>
public sealed class BigFiveModeTests
{
    private static readonly BigFiveMode Mode = new();

    [Fact]
    public void TheBankHasFiftyItems() =>
        BigFiveItemBank.All.Should().HaveCount(50);

    [Fact]
    public void EveryDimensionCarriesTheSameNumberOfForwardAndReverseItems()
    {
        // The property acquiescence balance rests on. Exposed on the bank so a
        // future item that tilts a dimension fails here rather than shifting
        // every player's score on that dimension by a few points.
        BigFiveItemBank.IsBalanced.Should().BeTrue();
    }

    [Fact]
    public void EveryDimensionHasTenItems()
    {
        var perTrait = BigFiveItemBank.All
            .SelectMany(i => i.TraitWeights.Keys)
            .GroupBy(k => k, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        perTrait.Should().HaveCount(5);
        perTrait.Values.Should().OnlyContain(count => count == 10);
    }

    [Fact]
    public void EveryItemLoadsOnADimensionTheScaleActuallyHas()
    {
        // A typo'd key is silently ignored by the builder — by design, so one
        // bad item costs a dimension's worth of signal rather than the session.
        // That is the right runtime behaviour and exactly why it needs a test:
        // nothing at runtime will ever complain.
        var scale = Mode.GetTraitScale();

        var unknown = BigFiveItemBank.All
            .SelectMany(i => i.TraitWeights.Keys)
            .Where(k => !scale.Contains(k))
            .Distinct()
            .ToList();

        unknown.Should().BeEmpty();
    }

    [Fact]
    public void StatementsAreDistinct() =>
        BigFiveItemBank.All.Select(i => i.Description)
            .Should().OnlyHaveUniqueItems();

    [Fact]
    public void ItemIdsAreDistinctAndStableAcrossRebuilds()
    {
        // Ids are a SHA-256 of the statement. TraitProfileBuilder keys responses
        // by card id, so regenerated ids would make a resumed session re-ask
        // everything already answered.
        BigFiveItemBank.All.Select(i => i.Id).Should().OnlyHaveUniqueItems();

        var again = new BigFiveMode().GetItemBank();
        again.Select(i => i.Id).Should().Equal(BigFiveItemBank.All.Select(i => i.Id));
    }

    [Fact]
    public void TheScaleIsTheFiveOceanDimensions()
    {
        Mode.GetTraitScale().Traits.Select(t => t.Key)
            .Should().Equal(
                BigFiveTraits.OpennessKey,
                BigFiveTraits.ConscientiousnessKey,
                BigFiveTraits.ExtraversionKey,
                BigFiveTraits.AgreeablenessKey,
                BigFiveTraits.NeuroticismKey);
    }

    [Fact]
    public void TheFifthDimensionIsKeyedNeuroticismButShownAsSensitivity()
    {
        // Deliberate: the key has to be identifiable to anyone who knows the
        // model, and the label has to be sayable to someone at a party.
        var trait = Mode.GetTraitScale().Find(BigFiveTraits.NeuroticismKey)!;
        trait.Key.Should().Be("Neuroticism");
        trait.Name.Should().Be("Sensitivity");
    }

    [Fact]
    public void EveryCategoryHasAColour()
    {
        // Categories are the trait keys; a missing colour is a card rendered
        // with default chrome on one screen and tinted on every other.
        var categories = BigFiveItemBank.All.Select(i => i.Category).Distinct();
        categories.Should().OnlyContain(c => Mode.CategoryColours.ContainsKey(c));
    }

    // ── The end-to-end property ──────────────────────────────────────────────

    [Fact]
    public void AgreeingWithEveryStatement_LandsDeadCentreOnAllFiveDimensions()
    {
        // The whole reason the bank is balanced, measured through the real
        // content rather than a synthetic one. Without reverse-keyed items this
        // reads 100 across the board and the mode reports that everyone is
        // maximally everything.
        var builder = new TraitProfileBuilder(Mode.GetTraitScale());
        foreach (var item in Mode.GetItemBank())
            builder.Record("P", item, LikertResponse.StronglyAgree);

        var profile = builder.Build("P");

        profile.AnsweredItems.Should().Be(50);
        profile.Scores.Should().HaveCount(5);
        profile.Scores.Should().OnlyContain(s => Math.Abs(s.Normalized - 50d) < 1e-9);
        profile.Scores.Should().OnlyContain(s => s.ItemCount == 10);
    }

    [Fact]
    public void DisagreeingWithEveryStatement_AlsoLandsDeadCentre()
    {
        var builder = new TraitProfileBuilder(Mode.GetTraitScale());
        foreach (var item in Mode.GetItemBank())
            builder.Record("P", item, LikertResponse.StronglyDisagree);

        builder.Build("P").Scores.Should().OnlyContain(s => Math.Abs(s.Normalized - 50d) < 1e-9);
    }

    [Fact]
    public void AnsweringForwardAndReverseItemsConsistently_ProducesAnExtremeScore()
    {
        // The counterpart to the two tests above: a player who answers
        // *coherently* rather than uniformly must be able to reach the ends of
        // the scale. If reverse-keying were applied twice, or not at all, this
        // would collapse to the midpoint too — and the balanced-bank tests alone
        // would not notice.
        var builder = new TraitProfileBuilder(Mode.GetTraitScale());

        foreach (var item in Mode.GetItemBank())
        {
            // Every item in this bank loads on exactly one dimension, and its
            // category is that dimension's key.
            var reversed = item.TraitWeights[item.Category] < 0;

            builder.Record("P", item,
                reversed ? LikertResponse.StronglyDisagree : LikertResponse.StronglyAgree);
        }

        builder.Build("P").Scores.Should().OnlyContain(s => Math.Abs(s.Normalized - 100d) < 1e-9);
    }

    [Fact]
    public void TheModeDeclaresItselfPlayableByOnePerson() =>
        Mode.MinimumPlayers.Should().Be(1);
}
