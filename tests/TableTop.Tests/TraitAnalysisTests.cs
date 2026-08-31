using TableTop.Core.Abstractions.Analysis;
using TableTop.Core.Domain.Analysis;

namespace TableTop.Tests;

/// <summary>
/// The trait-analysis layer's arithmetic.
///
/// <para>
/// Weighted heavily toward the scoring, because that is where a mistake is
/// invisible: a wrong reverse-keying rule still produces a plausible-looking
/// 0-100 number on every dimension, and nothing downstream can tell it is
/// wrong. The acquiescence tests below are the ones that would actually catch
/// it.
/// </para>
/// </summary>
public sealed class TraitAnalysisTests
{
    private const string T = "Trait";
    private const string Other = "Other";

    private static TraitScale Scale(params string[] keys) =>
        new("Test", keys.Select(k => new TraitDefinition(k, k, "low", "high", "d")));

    private static TraitItemCard Item(string key, bool reverse = false, double? weight = null) =>
        weight is null
            ? TraitItemCard.Single($"statement {Guid.NewGuid()}", key, reverse)
            : new TraitItemCard(Guid.NewGuid(), $"statement {Guid.NewGuid()}",
                new Dictionary<string, double> { [key] = weight.Value }, key);

    private static double ScoreOf(
        IEnumerable<(TraitItemCard Item, LikertResponse Response)> answers,
        string key = T)
    {
        var builder = new TraitProfileBuilder(Scale(key));
        foreach (var (item, response) in answers) builder.Record("P", item, response);
        return builder.Build("P").Find(key)!.Normalized;
    }

    // ── Keying and normalisation ─────────────────────────────────────────────

    [Fact]
    public void AllForwardItems_AgreedWith_ScoreMaximum() =>
        ScoreOf(Enumerable.Range(0, 10)
            .Select(_ => (Item(T), LikertResponse.StronglyAgree))).Should().Be(100d);

    [Fact]
    public void AllForwardItems_DisagreedWith_ScoreMinimum() =>
        ScoreOf(Enumerable.Range(0, 10)
            .Select(_ => (Item(T), LikertResponse.StronglyDisagree))).Should().Be(0d);

    [Fact]
    public void ReverseKeyedItem_InvertsTheResponse()
    {
        ScoreOf([(Item(T, reverse: true), LikertResponse.StronglyAgree)]).Should().Be(0d);
        ScoreOf([(Item(T, reverse: true), LikertResponse.StronglyDisagree)]).Should().Be(100d);
    }

    [Theory]
    [InlineData(LikertResponse.StronglyDisagree, LikertResponse.StronglyAgree)]
    [InlineData(LikertResponse.Disagree, LikertResponse.Agree)]
    [InlineData(LikertResponse.Neutral, LikertResponse.Neutral)]
    [InlineData(LikertResponse.Agree, LikertResponse.Disagree)]
    [InlineData(LikertResponse.StronglyAgree, LikertResponse.StronglyDisagree)]
    public void ReverseKeying_ReflectsAcrossTheScale_RatherThanNegating(
        LikertResponse given, LikertResponse equivalent)
    {
        // A reverse item answered `given` must score the same as a forward item
        // answered `equivalent`. Negation would put reverse items on a different
        // range entirely and the two could not be summed into one total.
        ScoreOf([(Item(T, reverse: true), given)])
            .Should().Be(ScoreOf([(Item(T), equivalent)]));
    }

    [Fact]
    public void BalancedBank_NeutralisesAgreeingWithEverything()
    {
        // THE test for this layer. A player who agrees with every statement in a
        // five-forward/five-reverse bank must land exactly in the middle, not at
        // the top. An all-positive bank measures nothing but agreeableness
        // toward the quiz itself.
        var balanced = Enumerable.Range(0, 5).Select(_ => Item(T))
            .Concat(Enumerable.Range(0, 5).Select(_ => Item(T, reverse: true)))
            .ToList();

        ScoreOf(balanced.Select(i => (i, LikertResponse.StronglyAgree))).Should().Be(50d);
        ScoreOf(balanced.Select(i => (i, LikertResponse.StronglyDisagree))).Should().Be(50d);
    }

    [Fact]
    public void NeutralAnswers_LandMidRegardlessOfKeying() =>
        ScoreOf([
            (Item(T), LikertResponse.Neutral),
            (Item(T, reverse: true), LikertResponse.Neutral),
            (Item(T, weight: 2d), LikertResponse.Neutral),
        ]).Should().Be(50d);

    [Fact]
    public void HeavierLoading_WidensBothTheNumeratorAndTheDenominator()
    {
        // If the bounds ignored the loading, a weight-2 item answered at the top
        // would push past a range computed as though everything weighed 1 —
        // Normalize clamps, so the symptom would be a dimension stuck at 100
        // rather than an obvious fault.
        ScoreOf([(Item(T, weight: 2d), LikertResponse.StronglyAgree)]).Should().Be(100d);

        // 2.0 at max (10) + 1.0 at min (1) = 11, over a range of 3..15.
        ScoreOf([
            (Item(T, weight: 2d), LikertResponse.StronglyAgree),
            (Item(T), LikertResponse.StronglyDisagree),
        ]).Should().BeApproximately((11d - 3d) / (15d - 3d) * 100d, 1e-9);
    }

    [Fact]
    public void ItemsThatDoNotLoadOnATrait_DoNotWidenItsRange()
    {
        // An item that cannot move a dimension must not count toward that
        // dimension's denominator, or every score drifts toward the middle as
        // the bank grows.
        var builder = new TraitProfileBuilder(Scale(T, Other));
        builder.Record("P", Item(T), LikertResponse.StronglyAgree);
        builder.Record("P", Item(Other), LikertResponse.StronglyDisagree);

        var profile = builder.Build("P");
        profile.Find(T)!.Normalized.Should().Be(100d);
        profile.Find(T)!.ItemCount.Should().Be(1);
        profile.Find(Other)!.Normalized.Should().Be(0d);
    }

    [Fact]
    public void ADimensionNobodyAnsweredFor_ReportsTheMidpointAndNoData()
    {
        // Not zero: zero is a real, meaningful score meaning every answer went
        // the other way. Reporting "no data" as the strongest possible negative
        // result is the kind of quiet lie that reads as a working feature.
        var profile = new TraitProfileBuilder(Scale(T)).Build("P");

        profile.Find(T)!.Normalized.Should().Be(50d);
        profile.Find(T)!.HasData.Should().BeFalse();
        profile.AnsweredItems.Should().Be(0);
    }

    [Theory]
    [InlineData(0d, TraitBand.VeryLow)]
    [InlineData(19.99, TraitBand.VeryLow)]
    [InlineData(20d, TraitBand.Low)]
    [InlineData(40d, TraitBand.Average)]
    [InlineData(59.99, TraitBand.Average)]
    [InlineData(60d, TraitBand.High)]
    [InlineData(80d, TraitBand.VeryHigh)]
    [InlineData(100d, TraitBand.VeryHigh)]
    public void BandBoundaries(double normalized, TraitBand expected) =>
        TraitScore.BandFor(normalized).Should().Be(expected);

    [Fact]
    public void Normalize_OnADegenerateRange_ReturnsTheMidpoint() =>
        TraitScore.Normalize(5d, 5d, 5d).Should().Be(50d);

    // ── Content validation ───────────────────────────────────────────────────

    [Fact]
    public void AnItemMustLoadOnSomething()
    {
        var act = () => new TraitItemCard(Guid.NewGuid(), "s", new Dictionary<string, double>(), "c");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void AZeroOrNonFiniteWeight_IsRejected(double weight)
    {
        // Zero is almost always a typo for "reverse-keyed" — the author reached
        // for 0 meaning "counts against". It would contribute nothing while
        // still looking like a scored item in the bank.
        var act = () => new TraitItemCard(
            Guid.NewGuid(), "s", new Dictionary<string, double> { [T] = weight }, "c");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ATraitScaleRejectsDuplicateKeys()
    {
        var act = () => new TraitScale("s",
        [
            new TraitDefinition("k", "A", "l", "h", "d"),
            new TraitDefinition("K", "B", "l", "h", "d"),
        ]);
        act.Should().Throw<ArgumentException>("keys are compared case-insensitively");
    }

    [Fact]
    public void ATraitScaleNeedsAtLeastOneDimension()
    {
        var act = () => new TraitScale("s", []);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TraitScale_FindIsCaseInsensitive_AndNullForUnknown()
    {
        var scale = Scale(T);
        scale.Find("trait").Should().NotBeNull();
        scale.Contains("TRAIT").Should().BeTrue();
        scale.Find("nope").Should().BeNull();
    }

    // ── Builder behaviour ────────────────────────────────────────────────────

    [Fact]
    public void ReAnsweringAnItem_ReplacesTheFirstAnswerRatherThanAddingToIt()
    {
        // What a back button needs. A running-total design would double-count,
        // and the symptom — one dimension quietly inflated for players who
        // changed their mind — is invisible without knowing the answer already.
        var item = Item(T);
        var builder = new TraitProfileBuilder(Scale(T));

        builder.Record("P", item, LikertResponse.StronglyAgree);
        builder.Record("P", item, LikertResponse.StronglyDisagree);

        builder.AnsweredCount("P").Should().Be(1);
        builder.Build("P").Find(T)!.Normalized.Should().Be(0d);
    }

    [Fact]
    public void AnItemWeightedOnATraitTheScaleDoesNotHave_IsIgnored()
    {
        // A typo costs a dimension's worth of signal, not the whole session.
        var builder = new TraitProfileBuilder(Scale(T));
        builder.Record("P", Item("Typo"), LikertResponse.StronglyAgree);

        var profile = builder.Build("P");
        profile.AnsweredItems.Should().Be(1);
        profile.Find(T)!.HasData.Should().BeFalse();
    }

    [Fact]
    public void PlayersAreReportedInFirstResponseOrder()
    {
        // Dictionary enumeration order is explicitly not part of its contract,
        // so this is kept separately. A results screen whose player order
        // shuffles between runs reads as a bug.
        var builder = new TraitProfileBuilder(Scale(T));
        foreach (var name in new[] { "Zoe", "Adam", "Mia" })
            builder.Record(name, Item(T), LikertResponse.Agree);

        builder.Players.Should().ContainInOrder("Zoe", "Adam", "Mia");
        builder.BuildAll().Select(p => p.PlayerName).Should().ContainInOrder("Zoe", "Adam", "Mia");
    }

    [Fact]
    public void ClearDiscardsEverything()
    {
        var builder = new TraitProfileBuilder(Scale(T));
        builder.Record("P", Item(T), LikertResponse.Agree);
        builder.Clear();

        builder.Players.Should().BeEmpty();
        builder.AnsweredCount("P").Should().Be(0);
    }

    [Fact]
    public void Strongest_ExcludesDimensionsWithNoData()
    {
        // An unanswered dimension sitting at the midpoint is not a finding, and
        // letting it rank would put "we know nothing" above a genuine 48.
        var builder = new TraitProfileBuilder(Scale(T, Other));
        builder.Record("P", Item(T), LikertResponse.Agree);

        var strongest = builder.Build("P").Strongest(2);
        strongest.Should().ContainSingle().Which.Trait.Key.Should().Be(T);
    }

    // ── Comparison ───────────────────────────────────────────────────────────

    private static TraitProfile ProfileWith(string name, params (string Key, LikertResponse R)[] answers)
    {
        var builder = new TraitProfileBuilder(Scale(answers.Select(a => a.Key).Distinct().ToArray()));
        foreach (var (key, r) in answers) builder.Record(name, Item(key), r);
        return builder.Build(name);
    }

    [Fact]
    public void IdenticalProfilesAreCompletelyAlike()
    {
        var a = ProfileWith("A", (T, LikertResponse.StronglyAgree));
        var b = ProfileWith("B", (T, LikertResponse.StronglyAgree));

        TraitProfileComparer.Compare(a, b).Similarity.Should().Be(100d);
    }

    [Fact]
    public void OppositeProfilesAreCompletelyUnalike()
    {
        var a = ProfileWith("A", (T, LikertResponse.StronglyAgree));
        var b = ProfileWith("B", (T, LikertResponse.StronglyDisagree));

        var comparison = TraitProfileComparer.Compare(a, b);
        comparison.Similarity.Should().Be(0d);
        comparison.GreatestDivergence!.Difference.Should().Be(100d);
    }

    [Fact]
    public void AConstantOffsetOnEveryDimension_IsNotPerfectAgreement()
    {
        // The reason similarity is mean distance rather than a correlation. Two
        // people offset by the same amount on every dimension have identically
        // *shaped* profiles, so a correlation reports perfect agreement. Asked
        // "how alike are we", that is the wrong answer, and it is wrong in the
        // direction that flatters the players.
        var a = ProfileWith("A", (T, LikertResponse.Neutral), (Other, LikertResponse.Neutral));
        var b = ProfileWith("B", (T, LikertResponse.StronglyAgree), (Other, LikertResponse.StronglyAgree));

        var comparison = TraitProfileComparer.Compare(a, b);
        comparison.ComparedDimensions.Should().Be(2);
        comparison.Similarity.Should().Be(50d, "both dimensions differ by 50 points");
    }

    [Fact]
    public void ProfilesSharingNoMeasuredDimension_ReportTheMidpointAndSayWhy()
    {
        // Not 100 ("identical") and not 0 ("opposites") — both are claims. The
        // midpoint with a count of zero beside it is not.
        var a = ProfileWith("A", (T, LikertResponse.Agree));
        var b = new TraitProfileBuilder(Scale(T)).Build("B");   // answered nothing

        var comparison = TraitProfileComparer.Compare(a, b);
        comparison.ComparedDimensions.Should().Be(0);
        comparison.Similarity.Should().Be(50d);
        comparison.GreatestDivergence.Should().BeNull();
        comparison.ClosestAlignment.Should().BeNull();
    }

    [Fact]
    public void CompareAll_ProducesEachUnorderedPairOnce()
    {
        var profiles = new[] { "A", "B", "C", "D" }
            .Select(n => ProfileWith(n, (T, LikertResponse.Agree)))
            .ToList();

        TraitProfileComparer.CompareAll(profiles).Should().HaveCount(6);
    }

    [Fact]
    public void MostAlikeAndMostDifferent_PickTheRightPairs()
    {
        var a = ProfileWith("A", (T, LikertResponse.StronglyAgree));
        var b = ProfileWith("B", (T, LikertResponse.StronglyAgree));
        var c = ProfileWith("C", (T, LikertResponse.StronglyDisagree));
        var all = new[] { a, b, c };

        var alike = TraitProfileComparer.MostAlike(all)!;
        alike.Similarity.Should().Be(100d);
        new[] { alike.Left.PlayerName, alike.Right.PlayerName }
            .Should().BeEquivalentTo(new[] { "A", "B" });

        TraitProfileComparer.MostDifferent(all)!.Similarity.Should().Be(0d);
    }

    [Fact]
    public void MostAlike_IsNullForFewerThanTwoProfiles() =>
        TraitProfileComparer.MostAlike([ProfileWith("A", (T, LikertResponse.Agree))])
            .Should().BeNull();
}
