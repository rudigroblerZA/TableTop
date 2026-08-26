using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Players;
using TableTop.Games.Fun;
using TableTop.Hosting;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;
using TableTop.Hosting.Events;

namespace TableTop.Tests;

/// <summary>
/// <see cref="HerdController"/> — the first controller shape with no active
/// player. Scoring turns on agreement, so most of what's worth pinning is the
/// grouping and the two scoring rules interacting.
/// </summary>
public sealed class HerdControllerTests
{
    private static ICard Prompt(string question) =>
        new StandardCard(Guid.NewGuid(), "P", question, Difficulty.Easy, "Test");

    private static IReadOnlyList<IPlayer> Players(params string[] names) =>
        names.Select(n => (IPlayer)Player.Create(n)).ToList().AsReadOnly();

    private static HerdController Build(int prompts = 3, params string[] names) =>
        new(Players(names.Length > 0 ? names : ["A", "B", "C", "D"]),
            Enumerable.Range(0, prompts).Select(i => Prompt($"q{i}")).ToList());

    // ── The regression ───────────────────────────────────────────────────────

    [Fact]
    public void ShortRoster_DoesNotThrow()
    {
        // This controller originally threw below three players. That made it
        // the only controller in the codebase that could refuse to construct,
        // which broke EveryCardTurnMode_CanBeDrivenToExhaustion_WithoutCrashing
        // (a registry-wide sweep using two players) and would also have thrown
        // when resuming a saved session whose roster had shrunk.
        //
        // MinimumPlayers is advisory everywhere else — RivalsMode declares 4
        // and the engine runs it with 2; PlayerSetupViewModel.CanStartGame is
        // what actually gates it. This matches that.
        var act = () => new HerdController(Players("A", "B"), [Prompt("q")]);
        act.Should().NotThrow();
    }

    [Fact]
    public void ShortRoster_DegradesHonestly_RatherThanScoringNonsense()
    {
        // With two players who disagree there is no herd and nobody uniquely
        // alone, so nothing scores — which tells the table the game isn't
        // working, without crashing.
        var controller = new HerdController(Players("A", "B"), [Prompt("q")]);
        HerdRoundResolvedEvent? resolved = null;
        controller.RoundResolved += (_, e) => resolved = e;
        controller.Start();

        controller.SubmitAnswers(new Dictionary<string, string> { ["A"] = "x", ["B"] = "y" });

        resolved!.Scores.Values.Sum().Should().Be(0);
        resolved.HerdAnswer.Should().BeNull();
    }

    [Fact]
    public void EmptyDeck_StillThrows()
    {
        // Unlike a short roster, an empty deck has no sane degraded behaviour
        // — there is nothing to ask anyone.
        var act = () => new HerdController(Players("A", "B", "C"), []);
        act.Should().Throw<ArgumentException>();
    }

    // ── Scoring ──────────────────────────────────────────────────────────────

    [Fact]
    public void MatchingTheHerd_ScoresEveryMemberOfTheLargestGroup()
    {
        var controller = Build();
        HerdRoundResolvedEvent? resolved = null;
        controller.RoundResolved += (_, e) => resolved = e;
        controller.Start();

        controller.SubmitAnswers(new Dictionary<string, string>
            { ["A"] = "cornflakes", ["B"] = "cornflakes", ["C"] = "cornflakes", ["D"] = "weetabix" });

        resolved!.HerdAnswer.Should().Be("cornflakes");
        resolved.Scores["A"].Should().Be(HerdController.HerdPoints);
        resolved.Scores["B"].Should().Be(HerdController.HerdPoints);
        resolved.Scores["C"].Should().Be(HerdController.HerdPoints);
    }

    [Fact]
    public void TheLoneVoice_ScoresLessThanTheHerd()
    {
        // The whole balance: being the only one to say something is worth
        // points, but fewer — so it stays a gamble rather than a better line.
        HerdController.LoneVoicePoints.Should().BeLessThan(HerdController.HerdPoints);

        var controller = Build();
        HerdRoundResolvedEvent? resolved = null;
        controller.RoundResolved += (_, e) => resolved = e;
        controller.Start();

        controller.SubmitAnswers(new Dictionary<string, string>
            { ["A"] = "x", ["B"] = "x", ["C"] = "x", ["D"] = "alone" });

        resolved!.LoneVoiceName.Should().Be("D");
        resolved.Scores["D"].Should().Be(HerdController.LoneVoicePoints);
    }

    [Fact]
    public void EveryoneDifferent_ScoresNothing()
    {
        // No herd, and nobody uniquely alone when everyone is. A real
        // outcome — the prompt was too open — not an edge case.
        var controller = Build();
        HerdRoundResolvedEvent? resolved = null;
        controller.RoundResolved += (_, e) => resolved = e;
        controller.Start();

        controller.SubmitAnswers(new Dictionary<string, string>
            { ["A"] = "w", ["B"] = "x", ["C"] = "y", ["D"] = "z" });

        resolved!.HerdAnswer.Should().BeNull();
        resolved.LoneVoiceName.Should().BeNull();
        resolved.Scores.Values.Sum().Should().Be(0);
    }

    [Fact]
    public void Unanimous_ScoresTheHerd_WithNoLoneVoice()
    {
        var controller = Build();
        HerdRoundResolvedEvent? resolved = null;
        controller.RoundResolved += (_, e) => resolved = e;
        controller.Start();

        controller.SubmitAnswers(new Dictionary<string, string>
            { ["A"] = "same", ["B"] = "same", ["C"] = "same", ["D"] = "same" });

        resolved!.HerdAnswer.Should().Be("same");
        resolved.LoneVoiceName.Should().BeNull();
        resolved.Scores.Values.Should().OnlyContain(v => v == HerdController.HerdPoints);
    }

    [Fact]
    public void EvenSplit_PicksAHerd_ButHasNoLoneVoice()
    {
        var controller = Build();
        HerdRoundResolvedEvent? resolved = null;
        controller.RoundResolved += (_, e) => resolved = e;
        controller.Start();

        controller.SubmitAnswers(new Dictionary<string, string>
            { ["A"] = "x", ["B"] = "x", ["C"] = "y", ["D"] = "y" });

        resolved!.HerdAnswer.Should().NotBeNull();
        resolved.LoneVoiceName.Should().BeNull("nobody stood alone in a 2-2 split");
    }

    // ── Answer normalisation ─────────────────────────────────────────────────

    [Fact]
    public void Answers_AreGroupedIgnoringCaseSpaceAndPunctuation()
    {
        var controller = Build();
        HerdRoundResolvedEvent? resolved = null;
        controller.RoundResolved += (_, e) => resolved = e;
        controller.Start();

        controller.SubmitAnswers(new Dictionary<string, string>
            { ["A"] = "Corn Flakes", ["B"] = "corn flakes", ["C"] = " CORN FLAKES! ", ["D"] = "weetabix" });

        resolved!.Groups[0].PlayerNames.Should().HaveCount(3);
    }

    [Fact]
    public void Normalisation_DoesNotJoinWords()
    {
        // "cornflakes" genuinely is a different answer from "corn flakes";
        // collapsing them would need whitespace stripping, which would wrongly
        // merge distinct answers elsewhere. Deliberate limit, not an oversight.
        var controller = Build();
        HerdRoundResolvedEvent? resolved = null;
        controller.RoundResolved += (_, e) => resolved = e;
        controller.Start();

        controller.SubmitAnswers(new Dictionary<string, string>
            { ["A"] = "corn flakes", ["B"] = "cornflakes", ["C"] = "x", ["D"] = "y" });

        resolved!.HerdAnswer.Should().BeNull("no two answers matched");
    }

    [Fact]
    public void BlankAnswers_ScoreNothing()
    {
        var controller = Build();
        HerdRoundResolvedEvent? resolved = null;
        controller.RoundResolved += (_, e) => resolved = e;
        controller.Start();

        controller.SubmitAnswers(new Dictionary<string, string>
            { ["A"] = "x", ["B"] = "x", ["C"] = "   ", ["D"] = "" });

        resolved!.Scores.Should().NotContainKey("C");
        resolved.Scores.Should().NotContainKey("D");
    }

    // ── Session flow ─────────────────────────────────────────────────────────

    [Fact]
    public void Start_RaisesTheFirstPrompt()
    {
        var controller = Build();
        HerdPromptReadyEvent? prompt = null;
        controller.PromptReady += (_, e) => prompt = e;

        controller.Start();

        prompt.Should().NotBeNull();
        prompt!.RoundNumber.Should().Be(1);
        prompt.TotalRounds.Should().Be(3);
    }

    [Fact]
    public void Session_EndsWhenTheDeckIsExhausted()
    {
        var controller = Build(prompts: 2);
        HerdGameEndedEvent? ended = null;
        controller.GameEnded += (_, e) => ended = e;
        controller.Start();

        controller.SubmitAnswers(new Dictionary<string, string> { ["A"] = "x", ["B"] = "x", ["C"] = "y", ["D"] = "z" });
        controller.SubmitAnswers(new Dictionary<string, string> { ["A"] = "x", ["B"] = "x", ["C"] = "x", ["D"] = "q" });

        ended.Should().NotBeNull();
        ended!.RoundsPlayed.Should().Be(2);
        controller.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void Ties_ReportEveryLeader()
    {
        // Same choice ClaimedController makes for a tied ending.
        var controller = Build(prompts: 1);
        HerdGameEndedEvent? ended = null;
        controller.GameEnded += (_, e) => ended = e;
        controller.Start();

        controller.SubmitAnswers(new Dictionary<string, string>
            { ["A"] = "x", ["B"] = "x", ["C"] = "y", ["D"] = "y" });

        ended!.WinnerNames.Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public void SubmitAnswers_AfterTheSessionEnds_IsANoOp()
    {
        var controller = Build(prompts: 1);
        controller.Start();
        controller.SubmitAnswers(new Dictionary<string, string> { ["A"] = "x", ["B"] = "x", ["C"] = "y", ["D"] = "z" });

        var act = () => controller.SubmitAnswers(new Dictionary<string, string> { ["A"] = "x" });

        act.Should().NotThrow();
    }
}

/// <summary>Registration and wiring for <see cref="HerdMode"/>.</summary>
public sealed class HerdModeTests
{
    [Fact]
    public void RegisteredInArchetypeTree()
    {
        var node = ArchetypeRegistry.Default().FindById("fun.herd");
        node.Should().NotBeNull();
        node!.Modes.Should().Contain(m => m.Name == "Herd");
    }

    [Fact]
    public void FactoryDispatchesToHerdController()
    {
        var players = new[] { (IPlayer)Player.Create("A"), Player.Create("B"), Player.Create("C") };
        var controller = new ControllerFactory().CreateAsync(new HerdMode(), players).GetAwaiter().GetResult();

        controller.Should().BeAssignableTo<IHerdController>();
        controller.Dispose();
    }

    [Fact]
    public void Manifest_ReportsNonZeroTotalCards()
    {
        // The bug class that excluded Claimed! from every capped SurpriseMe
        // query for a full version — a capability interface added without its
        // ModeManifestExtensions arm.
        new HerdMode().GetManifest().TotalCards.Should().BeGreaterThan(0);
    }

    [Fact]
    public void HerdDeck_ExcludesTheRulesCard_ButGetCardsKeepsIt()
    {
        // The deck feeds the controller directly as prompts, so leaving the
        // rules card in made round 1 ask the table to simultaneously answer a
        // page of instructions. Found by playing a session, not by reading it.
        var mode = new HerdMode();

        mode.GetHerdDeck().Should().NotContain(c => c.Category == "How To Play");
        mode.GetCards([]).Should().Contain(c => c.Category == "How To Play");
    }

    [Fact]
    public void FirstPrompt_IsAnActualQuestion()
    {
        var players = new[] { (IPlayer)Player.Create("A"), Player.Create("B"), Player.Create("C") };
        var controller = (IHerdController)new ControllerFactory()
            .CreateAsync(new HerdMode(), players).GetAwaiter().GetResult();

        HerdPromptReadyEvent? prompt = null;
        controller.PromptReady += (_, e) => prompt = e;
        controller.Start();

        prompt!.Category.Should().NotBe("How To Play");
        controller.Dispose();
    }

    [Fact]
    public void Deck_HasNoDuplicateIdsAndIsDeterministic()
    {
        var mode = new HerdMode();
        var deck = mode.GetCards([]);

        deck.Select(c => c.Id).Distinct().Should().HaveCount(deck.Count);
        mode.GetCards([]).Select(c => c.Id).Should().Equal(deck.Select(c => c.Id));
    }
}
