using TableTop.Core.Abstractions.Analysis;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Domain.Analysis;
using TableTop.Games.Couples;
using TableTop.Hosting.Controllers;

namespace TableTop.Tests;

/// <summary>
/// Love Languages' content, and the one property this mode needs that Big Five
/// does not: a usable <i>ranking</i>.
///
/// <para>
/// Big Five's output is five independent levels. This mode's output is an
/// order — "words lands hardest for you, gifts least" — so the tests that
/// matter here are the ones proving a ranking survives the scoring rather than
/// collapsing.
/// </para>
/// </summary>
public sealed class LoveLanguagesModeTests
{
    private static readonly LoveLanguagesMode Mode = new();

    private static readonly string[] Keys =
    [
        LoveLanguages.WordsKey, LoveLanguages.ServiceKey, LoveLanguages.GiftsKey,
        LoveLanguages.TimeKey, LoveLanguages.TouchKey,
    ];

    /// <summary>Answers every item, agreeing in the direction of <paramref name="favourite"/> and disagreeing elsewhere.</summary>
    private static TraitProfile ProfileFavouring(string favourite, string playerName = "P")
    {
        var builder = new TraitProfileBuilder(Mode.GetTraitScale());

        foreach (var item in Mode.GetItemBank())
        {
            var reversed = item.TraitWeights[item.Category] < 0;
            var wantsHigh = item.Category.Equals(favourite, StringComparison.OrdinalIgnoreCase);

            // Answer coherently toward a high score on the favourite language
            // and a low score on the rest.
            var agree = wantsHigh ? !reversed : reversed;
            builder.Record(playerName, item,
                agree ? LikertResponse.StronglyAgree : LikertResponse.StronglyDisagree);
        }

        return builder.Build(playerName);
    }

    // ── Content ──────────────────────────────────────────────────────────────

    [Fact]
    public void TheBankHasFortyItems() =>
        LoveLanguagesItemBank.All.Should().HaveCount(40);

    [Fact]
    public void EveryLanguageIsBalancedForwardAndReverse() =>
        LoveLanguagesItemBank.IsBalanced.Should().BeTrue();

    [Fact]
    public void EveryLanguageHasEightItems()
    {
        var perLanguage = LoveLanguagesItemBank.All
            .SelectMany(i => i.TraitWeights.Keys)
            .GroupBy(k => k, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        perLanguage.Should().HaveCount(5);
        perLanguage.Values.Should().OnlyContain(count => count == 8);
    }

    [Fact]
    public void EveryItemLoadsOnALanguageTheScaleHas()
    {
        var scale = Mode.GetTraitScale();
        LoveLanguagesItemBank.All
            .SelectMany(i => i.TraitWeights.Keys)
            .Where(k => !scale.Contains(k))
            .Should().BeEmpty();
    }

    [Fact]
    public void StatementsAndIdsAreDistinct()
    {
        LoveLanguagesItemBank.All.Select(i => i.Description).Should().OnlyHaveUniqueItems();
        LoveLanguagesItemBank.All.Select(i => i.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void ItemIdsAreStableAcrossRebuilds() =>
        new LoveLanguagesMode().GetItemBank().Select(i => i.Id)
            .Should().Equal(LoveLanguagesItemBank.All.Select(i => i.Id));

    [Fact]
    public void ItsIdsDoNotCollideWithBigFives()
    {
        // Both banks hash a statement into a Guid. They are salted with
        // different prefixes ("LoveLanguages|" vs "BigFive|") so two modes can
        // never produce the same card id -- TraitProfileBuilder keys responses
        // by id, and a collision across modes would silently merge two answers.
        var mine = LoveLanguagesItemBank.All.Select(i => i.Id).ToHashSet();
        var theirs = TableTop.Games.Fun.BigFiveItemBank.All.Select(i => i.Id).ToHashSet();
        mine.Overlaps(theirs).Should().BeFalse();
    }

    [Fact]
    public void TheScaleIsTheFiveLanguages() =>
        Mode.GetTraitScale().Traits.Select(t => t.Key).Should().Equal(Keys);

    [Fact]
    public void EveryCategoryHasAColour() =>
        LoveLanguagesItemBank.All.Select(i => i.Category).Distinct()
            .Should().OnlyContain(c => Mode.CategoryColours.ContainsKey(c));

    // ── The ranking, which is this mode's whole output ───────────────────────

    [Theory]
    [InlineData("WordsOfAffirmation")]
    [InlineData("ActsOfService")]
    [InlineData("Gifts")]
    [InlineData("QualityTime")]
    [InlineData("PhysicalTouch")]
    public void AnsweringTowardOneLanguage_MakesItTheTopLanguage(string favourite)
    {
        // The property this mode lives or dies on. Run for all five so a single
        // mis-keyed item in one language cannot hide behind the other four.
        var profile = ProfileFavouring(favourite);

        profile.Strongest().Should().ContainSingle()
            .Which.Trait.Key.Should().Be(favourite);
        profile.Find(favourite)!.Normalized.Should().Be(100d);
        profile.Scores.Where(s => s.Trait.Key != favourite)
            .Should().OnlyContain(s => s.Normalized == 0d);
    }

    [Fact]
    public void AgreeingWithEveryStatement_LeavesNoTopLanguage()
    {
        // Why the bank is balanced. On an all-positive bank an agreeable player
        // scores 100 on all five, which reads as a result and is not one -- the
        // ranking, which is the entire output, would be arbitrary.
        var builder = new TraitProfileBuilder(Mode.GetTraitScale());
        foreach (var item in Mode.GetItemBank())
            builder.Record("P", item, LikertResponse.StronglyAgree);

        var profile = builder.Build("P");
        profile.Scores.Should().OnlyContain(s => Math.Abs(s.Normalized - 50d) < 1e-9);
        profile.AnsweredItems.Should().Be(40);
    }

    // ── The comparison, which is why a couple plays it ───────────────────────

    [Fact]
    public void TwoPartnersWithOppositeTopLanguages_DivergeOnBoth()
    {
        var words = ProfileFavouring(LoveLanguages.WordsKey, "Ada");
        var touch = ProfileFavouring(LoveLanguages.TouchKey, "Bo");

        var comparison = TraitProfileComparer.Compare(words, touch);

        comparison.ComparedDimensions.Should().Be(5);
        comparison.GreatestDivergence!.Difference.Should().Be(100d,
            "one of them scores 100 where the other scores 0");

        // Three languages both rated at the floor: they genuinely agree there,
        // and the comparison should say so rather than only reporting conflict.
        comparison.ClosestAlignment!.Difference.Should().Be(0d);
    }

    [Fact]
    public void PartnersWhoAnswerIdentically_AreCompletelyAlike()
    {
        var a = ProfileFavouring(LoveLanguages.GiftsKey, "Ada");
        var b = ProfileFavouring(LoveLanguages.GiftsKey, "Bo");

        TraitProfileComparer.Compare(a, b).Similarity.Should().Be(100d);
    }

    // ── Dispatch ─────────────────────────────────────────────────────────────

    [Fact]
    public void ItResolvesToTheTraitProfileFamily() =>
        ControllerFamilies.For(Mode).Should().Be(ControllerFamily.TraitProfile);

    [Fact]
    public void TheManifestDescribesTheBankTheControllerIsHanded() =>
        Mode.GetManifest().TotalCards.Should().Be(Mode.GetItemBank().Count);

    [Fact]
    public async Task TheFactoryBuildsATraitProfileController()
    {
        var players = new[] { "Ada", "Bo" }
            .Select(n => (TableTop.Core.Abstractions.Players.IPlayer)Player.Create(n))
            .ToList().AsReadOnly();

        var controller = await new ControllerFactory().CreateAsync(Mode, players);
        try { controller.Should().BeOfType<TraitProfileController>(); }
        finally { controller.Dispose(); }
    }

    [Fact]
    public void ItIsRegisteredUnderCouples()
    {
        var couples = ArchetypeRegistry.Default().FindById("couples");
        couples.Should().NotBeNull();
        couples.AllModes.Select(m => m.Name).Should().Contain("Love Languages");
    }
}
