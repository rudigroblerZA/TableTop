using TableTop.Core.Abstractions.Analysis;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Analysis;
using TableTop.Games.Fun;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;
using TableTop.Hosting.Events;

namespace TableTop.Tests;

/// <summary>
/// <see cref="TraitProfileController"/> — the first controller shape that ends
/// without a score. Most of what is worth pinning is what it does with
/// responses it should not trust, and the difference between a skip and a
/// neutral answer.
/// </summary>
public sealed class TraitProfileControllerTests
{
    private const string T = "Trait";

    private static TraitScale Scale =>
        new("Test", [new TraitDefinition(T, T, "low", "high", "d")]);

    private static IReadOnlyList<IPlayer> Players(params string[] names)
    {
        var roster = names.Length > 0 ? names : new[] { "A", "B" };
        return roster.Select(n => (IPlayer)Player.Create(n)).ToList().AsReadOnly();
    }

    private static IReadOnlyList<TraitItemCard> Items(int count) =>
        Enumerable.Range(0, count).Select(i => TraitItemCard.Single($"statement {i}", T)).ToList();

    private static TraitProfileController Build(int items = 3, params string[] names) =>
        new(Players(names), Scale, Items(items));

    private static Dictionary<string, LikertResponse> All(LikertResponse r, params string[] names) =>
        names.ToDictionary(n => n, _ => r, StringComparer.OrdinalIgnoreCase);

    // ── Flow ─────────────────────────────────────────────────────────────────

    [Fact]
    public void StartRaisesTheFirstItem()
    {
        using var ctrl = Build();
        TraitItemReadyEvent? seen = null;
        ctrl.ItemReady += (_, e) => seen = e;

        ctrl.Start();

        seen.Should().NotBeNull();
        seen.ItemNumber.Should().Be(1);
        seen.TotalItems.Should().Be(3);
        ctrl.IsRunning.Should().BeTrue();
    }

    [Fact]
    public void AnsweringEveryItem_EndsTheSessionAndReportsProfiles()
    {
        using var ctrl = Build(items: 2);
        TraitAssessmentCompletedEvent? done = null;
        ctrl.AssessmentCompleted += (_, e) => done = e;

        ctrl.Start();
        ctrl.SubmitResponses(All(LikertResponse.StronglyAgree, "A", "B"));
        ctrl.SubmitResponses(All(LikertResponse.StronglyAgree, "A", "B"));

        ctrl.IsRunning.Should().BeFalse();
        done.Should().NotBeNull();
        done.Profiles.Should().HaveCount(2);
        done.ItemsAnswered.Should().Be(2);
        done.Profiles.Should().OnlyContain(p => Math.Abs(p.Find(T)!.Normalized - 100d) < 1e-9);
    }

    [Fact]
    public void ProfilesComeBackInRosterOrder_NotAnswerOrder()
    {
        // A results screen listing players in whatever sequence they happened to
        // first respond reads as arbitrary.
        using var ctrl = Build(items: 1, names: ["Ada", "Bo", "Cy"]);
        TraitAssessmentCompletedEvent? done = null;
        ctrl.AssessmentCompleted += (_, e) => done = e;

        ctrl.Start();
        ctrl.SubmitResponses(new Dictionary<string, LikertResponse>
        {
            ["Cy"] = LikertResponse.Agree,
            ["Ada"] = LikertResponse.Agree,
            ["Bo"] = LikertResponse.Agree,
        });

        done!.Profiles.Select(p => p.PlayerName).Should().ContainInOrder("Ada", "Bo", "Cy");
    }

    [Fact]
    public void SkipAdvancesWithoutRecordingAnything()
    {
        using var ctrl = Build(items: 2);
        TraitAssessmentCompletedEvent? done = null;
        ctrl.AssessmentCompleted += (_, e) => done = e;

        ctrl.Start();
        ctrl.Skip();
        ctrl.SubmitResponses(All(LikertResponse.StronglyAgree, "A", "B"));

        done!.Profiles.Should().OnlyContain(p => p.AnsweredItems == 1);
    }

    [Fact]
    public void ASkippedItemIsAnAbsence_NotANeutralAnswer()
    {
        // The distinction that matters. Recording a skip as Neutral would pull
        // the dimension toward its midpoint; a skip must leave the denominator
        // alone entirely. A player who skips most items should show a thin real
        // profile, not a full flattened one.
        using var skipped = Build(items: 2, names: ["A"]);
        TraitAssessmentCompletedEvent? skippedDone = null;
        skipped.AssessmentCompleted += (_, e) => skippedDone = e;
        skipped.Start();
        skipped.SubmitResponses(All(LikertResponse.StronglyAgree, "A"));
        skipped.Skip();

        using var neutral = Build(items: 2, names: ["A"]);
        TraitAssessmentCompletedEvent? neutralDone = null;
        neutral.AssessmentCompleted += (_, e) => neutralDone = e;
        neutral.Start();
        neutral.SubmitResponses(All(LikertResponse.StronglyAgree, "A"));
        neutral.SubmitResponses(All(LikertResponse.Neutral, "A"));

        skippedDone!.Profiles[0].Find(T)!.Normalized.Should().Be(100d);
        skippedDone.Profiles[0].Find(T)!.ItemCount.Should().Be(1);

        neutralDone!.Profiles[0].Find(T)!.Normalized.Should().Be(75d);
        neutralDone.Profiles[0].Find(T)!.ItemCount.Should().Be(2);
    }

    // ── Input it should not trust ────────────────────────────────────────────

    [Fact]
    public void AResponseFromSomeoneNotOnTheRoster_IsDropped()
    {
        // Otherwise a head that mis-keys its dictionary silently creates a
        // profile for a player who does not exist, and that shows up as a stray
        // column on the results screen rather than as an error anyone can trace.
        using var ctrl = Build(items: 1, names: ["A"]);
        TraitAssessmentCompletedEvent? done = null;
        ctrl.AssessmentCompleted += (_, e) => done = e;

        ctrl.Start();
        ctrl.SubmitResponses(new Dictionary<string, LikertResponse>
        {
            ["A"] = LikertResponse.Agree,
            ["Ghost"] = LikertResponse.StronglyAgree,
        });

        done!.Profiles.Should().ContainSingle().Which.PlayerName.Should().Be("A");
    }

    [Fact]
    public void AnOutOfRangeResponse_IsDropped()
    {
        // An undefined enum value sails through the arithmetic and produces a
        // score outside the range the bounds describe, which Normalize then
        // clamps — so the symptom is a dimension pinned at 0 or 100, not a fault.
        using var ctrl = Build(items: 1, names: ["A"]);
        TraitAssessmentCompletedEvent? done = null;
        ctrl.AssessmentCompleted += (_, e) => done = e;

        ctrl.Start();
        ctrl.SubmitResponses(new Dictionary<string, LikertResponse> { ["A"] = (LikertResponse)99 });

        done!.Profiles.Should().BeEmpty("the only response was invalid, so nobody answered anything");
    }

    [Fact]
    public void PlayerNamesAreMatchedCaseInsensitively()
    {
        using var ctrl = Build(items: 1, names: ["Ada"]);
        TraitAssessmentCompletedEvent? done = null;
        ctrl.AssessmentCompleted += (_, e) => done = e;

        ctrl.Start();
        ctrl.SubmitResponses(new Dictionary<string, LikertResponse> { ["ADA"] = LikertResponse.Agree });

        done!.Profiles.Should().ContainSingle();
    }

    [Fact]
    public void PlayersWhoAnsweredNothing_AreLeftOutOfTheResults()
    {
        // An all-midpoint profile built from zero items is not a result, and
        // printing one beside a real profile invites it to be read as one.
        using var ctrl = Build(items: 1, names: ["A", "B"]);
        TraitAssessmentCompletedEvent? done = null;
        ctrl.AssessmentCompleted += (_, e) => done = e;

        ctrl.Start();
        ctrl.SubmitResponses(All(LikertResponse.Agree, "A"));

        done!.Profiles.Should().ContainSingle().Which.PlayerName.Should().Be("A");
    }

    [Fact]
    public void QuittingEarly_ReportsProfilesFromWhatWasAnswered()
    {
        using var ctrl = Build(items: 10);
        TraitAssessmentCompletedEvent? done = null;
        ctrl.AssessmentCompleted += (_, e) => done = e;

        ctrl.Start();
        ctrl.SubmitResponses(All(LikertResponse.StronglyAgree, "A", "B"));
        ctrl.Quit();

        ctrl.IsRunning.Should().BeFalse();
        done!.Profiles.Should().HaveCount(2);
        done.Profiles.Should().OnlyContain(p => p.AnsweredItems == 1);
    }

    [Fact]
    public void SubmittingBeforeStart_DoesNothing()
    {
        using var ctrl = Build();
        var act = () => ctrl.SubmitResponses(All(LikertResponse.Agree, "A", "B"));
        act.Should().NotThrow();
        ctrl.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void TwoPlayers_ProduceAComparison()
    {
        using var ctrl = Build(items: 1);
        TraitAssessmentCompletedEvent? done = null;
        ctrl.AssessmentCompleted += (_, e) => done = e;

        ctrl.Start();
        ctrl.SubmitResponses(new Dictionary<string, LikertResponse>
        {
            ["A"] = LikertResponse.StronglyAgree,
            ["B"] = LikertResponse.StronglyDisagree,
        });

        done!.Comparisons.Should().ContainSingle();
        done.MostAlike.Should().NotBeNull();
        done.MostAlike.Similarity.Should().Be(0d, "they answered at opposite ends");
    }

    [Fact]
    public void ASinglePlayer_ProducesAProfileButNoComparison()
    {
        using var ctrl = Build(items: 1, names: ["Solo"]);
        TraitAssessmentCompletedEvent? done = null;
        ctrl.AssessmentCompleted += (_, e) => done = e;

        ctrl.Start();
        ctrl.SubmitResponses(All(LikertResponse.Agree, "Solo"));

        done!.Profiles.Should().ContainSingle();
        done.Comparisons.Should().BeEmpty();
        done.MostAlike.Should().BeNull();
        done.MostDifferent.Should().BeNull();
    }

    // ── Dispatch ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task BigFiveResolvesToTheTraitProfileFamily_AndTheFactoryBuildsThatController()
    {
        var mode = new BigFiveMode();
        ControllerFamilies.For(mode).Should().Be(ControllerFamily.TraitProfile);

        var controller = await new ControllerFactory().CreateAsync(mode, Players());
        try { controller.Should().BeOfType<TraitProfileController>(); }
        finally { controller.Dispose(); }
    }

    [Fact]
    public void TheManifestDescribesTheBankTheControllerIsHanded()
    {
        // The property backlog item 10 fixed for Herd and item 13 for Claimed!:
        // a manifest built from a different deck than the controller plays makes
        // TotalCards a number nobody can act on — and README's card count is
        // derived from it.
        var mode = new BigFiveMode();
        mode.GetManifest().TotalCards.Should().Be(mode.GetItemBank().Count);
    }
}
