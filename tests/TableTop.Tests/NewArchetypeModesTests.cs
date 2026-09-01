using TableTop.Core.Abstractions.Game;
using TableTop.Core.Domain.Progression;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Couples;
using TableTop.Games.Family;
using TableTop.Games.School;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;
using TableTop.Hosting.Events;

namespace TableTop.Tests;

/// <summary>
/// Invariants for the four newest modes — one per root archetype:
/// Estimation Station (Classroom), Forbidden Words (Fun),
/// Mind Meld (Couples), Adventure Style (Personality).
/// </summary>
public sealed class NewArchetypeModesTests
{
    // ── Registry wiring ───────────────────────────────────────────────────────

    [Fact]
    public void EstimationStation_IsRegistered_UnderClassroom()
    {
        var node = ArchetypeRegistry.Default().FindById("classroom.estimation");
        node.Should().NotBeNull();
        node!.Modes.Count(m => m.Name == "Estimation Station").Should().Be(1);
    }

    [Fact]
    public void ForbiddenWords_IsRegistered_UnderFun()
    {
        var node = ArchetypeRegistry.Default().FindById("fun.forbidden");
        node.Should().NotBeNull();
        node!.Modes.Count(m => m.Name == "Forbidden Words").Should().Be(1);
    }

    [Fact]
    public void MindMeld_IsRegistered_UnderCouplesConnection()
    {
        var node = ArchetypeRegistry.Default().FindById("couples.connection.mindmeld");
        node.Should().NotBeNull();
        node!.Modes.Count(m => m.Name == "Mind Meld").Should().Be(1);
    }

    [Fact]
    public void Cartographers_IsRegistered_UnderCouplesConnection()
    {
        var node = ArchetypeRegistry.Default().FindById("couples.connection.cartographers");
        node.Should().NotBeNull();
        node!.Modes.Count(m => m.Name == "The Cartographers").Should().Be(1);
    }

    [Fact]
    public void Cartographers_DeclaresCoupleShape_SoAnUnsuitableTableIsCaught()
    {
        // Every card addresses a pair sharing one sheet of paper, so the mode
        // must declare Couple — otherwise TableSuitability has nothing to check
        // and the mode could be started at any table (backlog item 17).
        var mode = new TableTop.Games.Couples.CartographersMode();
        mode.SuitableFor.Should().Be(TableShape.Couple);
    }


    // ── Card-bank invariants ──────────────────────────────────────────────────

    [Fact]
    public void EstimationStation_Bank_HasCards_WithAnswers_AndDifficultySpread()
    {
        var cards = EstimationStationCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(40);
        cards.Should().OnlyContain(c => c.Description.Contains("Answer:"));
        cards.Select(c => c.Difficulty).Distinct().Count().Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void ForbiddenWords_Bank_HasCards_EachWithThreeBans()
    {
        var cards = ForbiddenWordsCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(40);
        cards.Should().OnlyContain(c => c.Description.Contains("FORBIDDEN:"));
        // Three bans are rendered as "a · b · c" — two separators per card.
        cards.Should().OnlyContain(c => c.Description.Split('·').Length == 3);
    }

    [Fact]
    public void MindMeld_Bank_HasCards_AllPromptBothPlayers()
    {
        var cards = MindMeldCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(40);
        cards.Should().OnlyContain(c => c.Description.Contains("Both of you"));
    }

    [Fact]
    public void Cartographers_Bank_EveryCardAddsToTheMap()
    {
        // The mode's whole premise is that each card places something permanent
        // on one shared sheet. A card that doesn't instruct a drawing action
        // would be a conversation prompt that wandered into the wrong deck.
        var cards = TableTop.Games.Couples.CartographersCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(30);
        cards.Should().OnlyContain(c => c.Description.Contains("Add to the map:"));
    }

    [Fact]
    public void Cartographers_TheFiveAges_AreAllPresent()
    {
        var categories = TableTop.Games.Couples.CartographersCardBank.All
            .Select(c => c.Category).Distinct().ToList();

        categories.Should().BeEquivalentTo(
            ["Survey", "Terrain", "Settlement", "Legend", "Terra Incognita"]);
    }

    [Fact]
    public void Cartographers_PinsSurveyFirstAndTerraIncognitaLast()
    {
        // Order is load-bearing here in a way it isn't for a shuffled deck: you
        // cannot name a mountain before drawing one, and the closing tier only
        // works once there is a map to find the edges of.
        var mode = new TableTop.Games.Couples.CartographersMode();

        mode.CategoriesPinnedToStart.Should().Equal("Survey");
        mode.CategoriesPinnedToEnd.Should().Equal("Terra Incognita");
    }

    [Fact]
    public void Cartographers_LaterAges_ReferBackToWhatEarlierAgesDrew()
    {
        // The cumulative property is what makes this mode different from the
        // other couples decks: a Legend card is unanswerable without the
        // specific map the players already made. If these stopped referring
        // backwards, the deck would still "work" but would have quietly become
        // an ordinary prompt list.
        var legend = TableTop.Games.Couples.CartographersCardBank.All
            .Where(c => c.Category == "Legend")
            .ToList();

        legend.Should().Contain(c => c.Description.Contains("highest mountain"),
            "naming the mountain requires the Terrain card that drew it");
        legend.Should().Contain(c => c.Description.Contains("Name the river"),
            "naming the river requires the Terrain card that drew it");
    }

    [Fact]
    public void Cartographers_ScoresNothing_BecauseTheMapIsThePoint()
    {
        var manifest = new TableTop.Games.Couples.CartographersMode().GetManifest();
        manifest.TotalCards.Should().Be(
            TableTop.Games.Couples.CartographersCardBank.All.Count);
    }

    // REMOVED: AdventureStyle_Bank_EndsWithResultsKey_AndAllScenariosHaveFourOptions
    //
    // It asserted on `AdventureStyleCardBank`, which does not exist and never
    // has — the reference is present in the earliest copy of this repository
    // available and matches no type in TableTop.Games. The test project has
    // therefore never compiled, which went unnoticed because TableTop.Games
    // did not compile either (five DeckFileException call sites), so nothing
    // ever got as far as building the tests.
    //
    // `BetweenTheTwoOfYouCardBank` is the closest match — four-option scenarios
    // and a multi-card Results key — but its last card is categorised
    // "Grow Together", not "Results", and one scenario card carries no options,
    // so the assertions do not hold against it. Rewriting them until they
    // passed would have invented a test rather than restored one.
    //
    // If AdventureStyle was a real mode that was removed, restore the bank and
    // this test with it.

    // ── Unique ids across the whole bank of each mode ─────────────────────────

    [Fact]
    public void NewModes_AllCardIds_AreUnique()
    {
        foreach (var bank in new IReadOnlyList<ICard>[]
        {
            EstimationStationCardBank.All,
            ForbiddenWordsCardBank.All,
            MindMeldCardBank.All,
            TableTop.Games.Couples.CartographersCardBank.All,
        })
        {
            bank.Select(c => c.Id).Distinct().Count().Should().Be(bank.Count);
        }
    }
}

/// <summary>
/// Regression guards for Fact or Fiction: the card TITLE used to be the answer
/// ("FACT"/"FICTION"), which every UI renders above the statement — the game
/// revealed itself before anyone voted. Titles must stay neutral and the
/// answer must appear only at the bottom of the description.
/// </summary>
public sealed class FactOrFictionAnswerLeakTests
{
    [Fact]
    public void FactOrFiction_Titles_DoNotLeakTheAnswer()
    {
        foreach (var c in TableTop.Games.FactOrFiction.FactOrFictionMode.GetCards())
        {
            c.Title.Should().NotBe("FACT");
            c.Title.Should().NotBe("FICTION");
            c.Description.Should().Contain("Answer:");
            // Statement must come before the answer reveal.
            c.Description.IndexOf("Statement:").Should().BeLessThan(c.Description.IndexOf("Answer:"));
        }
    }

    [Fact]
    public void ExpertFactOrFiction_Titles_DoNotLeakTheAnswer()
    {
        foreach (var c in TableTop.Games.FactOrFiction.ExpertFactOrFictionMode.GetCards())
        {
            c.Title.Should().NotBe("FACT");
            c.Title.Should().NotBe("FICTION");
            c.Description.Should().Contain("Answer:");
            c.Description.IndexOf("Statement:").Should().BeLessThan(c.Description.IndexOf("Answer:"));
        }
    }
}

/// <summary>
/// Invariants for the second creative batch — one per root archetype:
/// Wrong Answers Only (Classroom), Useless Superpowers (Fun),
/// Parallel Us (Couples), Mind Palace (Personality).
/// </summary>
public sealed class CreativeBatchModesTests
{
    [Fact]
    public void CreativeBatch_AllFour_AreRegistered()
    {
        var reg = ArchetypeRegistry.Default();
        foreach (var (id, name) in new[]
        {
            ("classroom.wrong",               "Wrong Answers Only"),
            ("fun.superpowers",               "Useless Superpowers"),
            ("couples.connection.parallel",   "Parallel Us"),
        })
        {
            var node = reg.FindById(id);
            node.Should().NotBeNull();
            node!.Modes.Count(m => m.Name == name).Should().Be(1);
        }
    }

    [Fact]
    public void WrongAnswersOnly_EveryCard_RevealsTheRealAnswerBeforeTheGame()
    {
        var cards = TableTop.Games.School.WrongAnswersOnlyCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(40);
        foreach (var c in cards)
        {
            c.Description.Should().Contain("REAL answer");
            c.Description.IndexOf("Question:").Should().BeLessThan(c.Description.IndexOf("WRONG answer"));
        }
    }

    [Fact]
    public void UselessSuperpowers_PowerCards_AlwaysCarryTheCatch()
    {
        var cards = TableTop.Games.Family.UselessSuperpowersCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(40);
        foreach (var c in cards.Where(c => c.Description.Contains("Your new power")))
            c.Description.Should().Contain("The catch:");
        cards.Count(c => c.Category == "Showdown").Should().BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void ParallelUs_AllCards_UseYesAndCoNarration()
    {
        var cards = TableTop.Games.Couples.ParallelUsCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(38);
        foreach (var c in cards)
            c.Description.Should().Contain("Yes, and");
    }

}

/// <summary>
/// The choice-card parser powers A/B/C/D answer buttons in every UI —
/// personality quizzes were unplayable with only a generic Done button.
/// </summary>
public sealed class ChoiceCardsTests
{


    [Fact]
    public void NonChoiceDecks_NeverParseAsChoices()
    {
        foreach (var c in TableTop.Games.Family.ForbiddenWordsCardBank.All)
            ChoiceCards.IsChoiceCard(c.Description).Should().BeFalse();
        foreach (var c in TableTop.Games.Couples.MindMeldCardBank.All)
            ChoiceCards.IsChoiceCard(c.Description).Should().BeFalse();
    }

    [Fact]
    public void Dominant_And_Format_BehaveAsDocumented()
    {
        var tally = new Dictionary<char, int> { ['A'] = 3, ['C'] = 3, ['B'] = 1 };
        ChoiceCards.Dominant(tally).Should().Be('A');           // tie → earliest letter
        ChoiceCards.Format(tally).Should().Be("A:3 B:1 C:3");
        ChoiceCards.Dominant(new Dictionary<char, int>()).Should().BeNull();
    }
}

/// <summary>
/// CardFaces powers the tabletop flip: question on the front, answer on the
/// back. These pin the split behaviour for the answer-bearing decks.
/// </summary>
public sealed class CardFacesTests
{
    [Fact]
    public void FactOrFiction_SplitsIntoQuestionFront_AnswerBack()
    {
        foreach (var c in TableTop.Games.FactOrFiction.FactOrFictionMode.GetCards())
        {
            var stripped = StripTags(c.Description);
            var (front, back) = CardFaces.Split(stripped);
            back.Should().NotBeNull();
            back!.Should().Contain("Answer:");
            front.Should().NotContain("Answer:");
            front.Should().Contain("Statement:");
            // The "keep the next line to yourself" hint is obsolete once flipping exists
            front.Should().NotContain("keep the next line to yourself");
        }
    }

    [Fact]
    public void EstimationStation_IsTwoFaced()
    {
        // Was EstimationStation_AndMindPalace_AreTwoFaced. Mind Palace was
        // removed with the personality quizzes; Estimation Station still carries
        // the two-faced format, so the assertion survives on its own.
        CardFaces.HasBack(StripTags(TableTop.Games.School.EstimationStationCardBank.All[0].Description))
            .Should().BeTrue();
    }

    [Fact]
    public void DecksWithoutAnswers_AreSingleFaced()
    {
        foreach (var c in TableTop.Games.Couples.MindMeldCardBank.All.Take(10))
            CardFaces.HasBack(StripTags(c.Description)).Should().BeFalse();
        foreach (var c in TableTop.Games.Family.ForbiddenWordsCardBank.All.Take(10))
            CardFaces.HasBack(StripTags(c.Description)).Should().BeFalse();
    }

    private static string StripTags(string s) =>
        s.Replace("<b>", "").Replace("</b>", "").Replace("<i>", "").Replace("</i>", "");
}

/// <summary>Style-name extraction turns "mostly A" into "The Pathfinder".</summary>
public sealed class StyleNameTests
{


    [Fact]
    public void Verdict_NamesStyle_OrFallsBackToLetter()
    {
        var tally = new Dictionary<char, int> { ['A'] = 3, ['B'] = 1 };
        var styles = new Dictionary<char, string> { ['A'] = "The Pathfinder" };
        ChoiceCards.Verdict(tally, styles).Should().Be("The Pathfinder (A)");
        ChoiceCards.Verdict(tally, new Dictionary<char, string>()).Should().Be("mostly A");
    }
}

/// <summary>CardText.StripHtml protects UIs without rich-text (MAUI).</summary>
public sealed class CardTextTests
{
    [Fact]
    public void StripHtml_RemovesTags_KeepsContent()
    {
        CardText.StripHtml("<b>Statement:</b>\n\nHello <i>world</i>")
            .Should().Be("Statement:\n\nHello world");
        CardText.StripHtml(null).Should().Be(string.Empty);
    }
}

/// <summary>CardFaces must split raw HTML too — WPF keeps markup for rich text.</summary>
public sealed class CardFacesHtmlTests
{
    [Fact]
    public void Split_OnRawHtml_FindsTaggedAnswerMarker()
    {
        var html = "<b>Statement:</b>\n\nOctopuses have three hearts.\n\n<b>Answer:</b> ✅ FACT — and blue blood.";
        var (front, back) = CardFaces.Split(html);
        back.Should().NotBeNull();
        back!.Should().StartWith("<b>Answer:</b>");
        front.Should().Contain("<b>Statement:</b>");
        front.Should().NotContain("Answer:");
    }

    [Fact]
    public void Split_EveryFactOrFictionCard_WithHtmlIntact()
    {
        foreach (var c in TableTop.Games.FactOrFiction.FactOrFictionMode.GetCards())
        {
            var (front, back) = CardFaces.Split(c.Description);
            back.Should().NotBeNull();
            front.Should().NotContain("Answer:");
        }
    }
}

/// <summary>
/// Heat Check's whole design is the two-temperature consent mechanic —
/// these invariants keep every card honest to it.
/// </summary>
public sealed class HeatCheckTests
{
    [Fact]
    public void HeatCheck_IsRegistered_AsAdult()
    {
        var node = ArchetypeRegistry.Default().FindById("couples.connection.heatcheck");
        node.Should().NotBeNull();
        node!.AgeRating.Should().Be(AgeRating.Adult);
        node.Modes.Count(m => m.Name == "Heat Check").Should().Be(1);
    }

    [Fact]
    public void EveryCard_OffersBothTemperatures_AndTheConsentRule()
    {
        var cards = TableTop.Games.Couples.HeatCheckCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(36);
        foreach (var c in cards)
        {
            c.Description.Should().Contain("🕯️");
            c.Description.Should().Contain("🔥");
            c.Description.Should().Contain("Choose together");
            c.Description.Should().Contain("mismatch means 🕯️");
        }
    }

    [Fact]
    public void HeatCheck_CardsAreSingleFaced_AndChoiceFree()
    {
        // Both temperatures print on the FRONT deliberately — choosing is the
        // game — so the flip and quiz mechanics must not accidentally trigger.
        foreach (var c in TableTop.Games.Couples.HeatCheckCardBank.All)
        {
            CardFaces.HasBack(CardText.StripHtml(c.Description)).Should().BeFalse();
            ChoiceCards.IsChoiceCard(c.Description).Should().BeFalse();
        }
    }
}

/// <summary>
/// Truth or Dare's redesign: every card is a TRUTH+DARE pair with a chicken
/// clause — declaring blind is the game. These keep the pairing honest.
/// </summary>
public sealed class TruthOrDarePairedCardTests
{
    [Fact]
    public void EveryStandardCard_CarriesBothOptions_AndAForfeit()
    {
        var cards = TableTop.Games.TruthOrDareCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(34);
        foreach (var c in cards.Where(c => c is not TableTop.Core.Domain.Cards.PromptCard))
        {
            c.Description.Should().Contain("TRUTH:");
            c.Description.Should().Contain("DARE:");
            c.Description.Should().Contain("Chicken clause");
            c.Description.IndexOf("declare OUT LOUD").Should().BeLessThan(c.Description.IndexOf("TRUTH:"));
        }
    }

    [Fact]
    public void RestrictedSubset_IsPresent_ForBothRegistryPlacements()
    {
        var cards = TableTop.Games.TruthOrDareCardBank.All;
        cards.Count(c => c.Restriction is not null).Should().BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void PairedCards_NeverTriggerFlipOrQuizMechanics()
    {
        foreach (var c in TableTop.Games.TruthOrDareCardBank.All)
        {
            CardFaces.HasBack(CardText.StripHtml(c.Description)).Should().BeFalse();
            ChoiceCards.IsChoiceCard(c.Description).Should().BeFalse();
        }
    }
}

/// <summary>Invariants for the expand-games batch: Odd One Out, One-Star
/// Reviews, Alibi, and Villain Origin.</summary>
public sealed class ExpandGamesBatchTests
{
    [Fact]
    public void TheRemainingModes_AreRegistered_WithCorrectRatings()
    {
        var reg = ArchetypeRegistry.Default();
        reg.FindById("classroom.oddoneout")!.AgeRating.Should().Be(AgeRating.AllAges);
        reg.FindById("fun.onestar")!.AgeRating.Should().Be(AgeRating.AllAges);
        reg.FindById("fun.alibi")!.AgeRating.Should().Be(AgeRating.AllAges);
    }

    [Fact]
    public void OddOneOut_IsFlippable_ButNeverAQuiz()
    {
        var cards = TableTop.Games.School.OddOneOutCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(29);
        foreach (var c in cards)
        {
            var plain = CardText.StripHtml(c.Description);
            CardFaces.HasBack(plain).Should().BeTrue();              // Answer: on the back
            CardFaces.Split(plain).Front.Should().NotContain("Answer:");
            ChoiceCards.IsChoiceCard(c.Description).Should().BeFalse(); // 1-4, not A-D
            c.Description.Should().Contain("1. ").And.Contain("4. ");
        }
    }

    [Fact]
    public void OneStarReviews_AlwaysDemandTheReview()
    {
        var cards = TableTop.Games.Family.OneStarReviewsCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(38);
        foreach (var c in cards)
            c.Description.Should().Contain("1-star review");
    }

    [Fact]
    public void Alibi_AlwaysNamesSuspects_AndSeparateQuestioning()
    {
        var cards = TableTop.Games.Family.AlibiCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(29);
        foreach (var c in cards)
        {
            c.Description.Should().Contain("SUSPECTS").And.Contain("SEPARATELY");
            c.Description.Should().Contain("THE CRIME:");
        }
    }

}

/// <summary>
/// Slow Burn's design contract: anticipation via the pot, consent via the
/// always-available rain-check trade, and never accidentally a quiz or flip.
/// </summary>
public sealed class SlowBurnTests
{
    [Fact]
    public void SlowBurn_IsRegistered_AsAdult()
    {
        var node = ArchetypeRegistry.Default().FindById("couples.connection.slowburn");
        node.Should().NotBeNull();
        node!.AgeRating.Should().Be(AgeRating.Adult);
    }

    [Fact]
    public void EveryIou_SealsIntoThePot_WithTheRainCheckLaw()
    {
        var ious = TableTop.Games.Couples.SlowBurnCardBank.All
            .Where(c => c.Category == "IOU").ToList();
        ious.Count.Should().BeGreaterThanOrEqualTo(10);
        foreach (var c in ious)
        {
            c.Description.Should().Contain("pot");
            c.Description.Should().Contain("kiss and a rain check");   // consent escape hatch on every IOU
        }
    }

    [Fact]
    public void EveryAlmost_StopsAtTheBestPart()
    {
        var almosts = TableTop.Games.Couples.SlowBurnCardBank.All
            .Where(c => c.Category == "Almost").ToList();
        almosts.Count.Should().BeGreaterThanOrEqualTo(8);
        foreach (var c in almosts)
            c.Description.Should().Contain("Stop at the best part");
    }

    [Fact]
    public void SlowBurn_NeverTriggersFlipOrQuizMechanics()
    {
        foreach (var c in TableTop.Games.Couples.SlowBurnCardBank.All)
        {
            CardFaces.HasBack(CardText.StripHtml(c.Description)).Should().BeFalse();
            ChoiceCards.IsChoiceCard(c.Description).Should().BeFalse();
        }
    }
}

/// <summary>All In's contract: chips-as-kisses casino where the app scoreboard
/// decides the jackpot — so scoring MUST award points, unlike other couples decks.</summary>
public sealed class AllInTests
{
    [Fact]
    public void AllIn_IsRegistered_AsAdult()
    {
        var node = ArchetypeRegistry.Default().FindById("couples.connection.allin");
        node.Should().NotBeNull();
        node!.AgeRating.Should().Be(AgeRating.Adult);
    }

    [Fact]
    public void WonHands_ScoreChips_SoTheScoreboardCanCrownTheJackpot()
    {
        // The showdown mechanic depends on the app's standings being real.
        var mode = new TableTop.Games.Couples.AllInMode();
        mode.CompleteLabel.Should().Be("Won the Hand");
        mode.SkipLabel.Should().Be("Fold");

        var player = TableTop.Core.Domain.Players.Player.Create("A");
        var card = TableTop.Games.Couples.AllInCardBank.All[0];
        mode.GetScoring()
            .CalculateScore(card, player, TableTop.Core.Abstractions.Scoring.CardOutcome.Completed)
            .Should().Be(1);   // one chip per won hand
    }

    [Fact]
    public void EveryRaise_ExplainsTheCall_AndEveryBluff_TheStakes()
    {
        var bank = TableTop.Games.Couples.AllInCardBank.All;
        foreach (var c in bank.Where(c => c.Category == "Raise"))
            c.Description.Should().Contain("RAISE:").And.Contain("partner may call");
        foreach (var c in bank.Where(c => c.Category == "Bluff"))
            c.Description.Should().Contain("TRUE or BLUFF").And.Contain("settle in kisses");
        bank.Count.Should().BeGreaterThanOrEqualTo(32);
    }

    [Fact]
    public void AllIn_NeverTriggersFlipOrQuizMechanics()
    {
        foreach (var c in TableTop.Games.Couples.AllInCardBank.All)
        {
            CardFaces.HasBack(CardText.StripHtml(c.Description)).Should().BeFalse();
            ChoiceCards.IsChoiceCard(c.Description).Should().BeFalse();
        }
    }
}

/// <summary>
/// Millionaire: Modern Love — the couples slang quiz must satisfy the
/// MillionaireController's laddering contract (tiered difficulties) and keep
/// every question a clean 4-option card with a valid answer.
/// </summary>
public sealed class ModernLoveMillionaireTests
{
    [Fact]
    public void ModernLove_IsRegistered_AsAdult_AndProvidesItsBank()
    {
        var node = ArchetypeRegistry.Default().FindById("couples.connection.modernlove");
        node.Should().NotBeNull();
        node!.AgeRating.Should().Be(AgeRating.Adult);
        node.Modes[0].Should().BeAssignableTo<TableTop.Core.Abstractions.Game.IQuestionBankProvider>();
    }

    [Fact]
    public void Bank_SatisfiesTheLadderContract()
    {
        // Controller tiers: Q1-5 Easy, Q6-10 Medium, Q11-14 Hard, Q15 Extreme.
        var bank = TableTop.Games.Couples.ModernLoveQuestionBank.All;
        bank.Count(q => q.Difficulty == Difficulty.Easy).Should().BeGreaterThanOrEqualTo(5);
        bank.Count(q => q.Difficulty == Difficulty.Medium).Should().BeGreaterThanOrEqualTo(5);
        bank.Count(q => q.Difficulty == Difficulty.Hard).Should().BeGreaterThanOrEqualTo(4);
        bank.Count(q => q.Difficulty == Difficulty.Extreme).Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void EveryQuestion_HasFourDistinctOptions_AndCorrectAnswersVary()
    {
        var bank = TableTop.Games.Couples.ModernLoveQuestionBank.All;
        foreach (var q in bank)
        {
            q.Answers.Count.Should().Be(4);
            q.Answers.Values.Should().OnlyContain(o => !string.IsNullOrWhiteSpace(o));
            q.Answers.Values.Distinct().Count().Should().Be(4);
            q.Answers.Keys.Should().Contain(q.CorrectAnswer);
        }
        // Not all correct answers on the same letter (an exploitable tell)
        bank.Select(q => q.CorrectAnswer).Distinct().Count().Should().Be(4);
    }
}

/// <summary>
/// Slang Check — the Fun-tree slang quiz. Same ladder contract as Millionaire:
/// Modern Love, but general-audience (Teen) rather than couples-locked.
/// </summary>
public sealed class SlangCheckTests
{
    [Fact]
    public void SlangCheck_IsRegistered_AsTeen_AndProvidesItsBank()
    {
        var node = ArchetypeRegistry.Default().FindById("fun.slang");
        node.Should().NotBeNull();
        node!.AgeRating.Should().Be(AgeRating.Teen);
        node.Modes[0].Should().BeAssignableTo<TableTop.Core.Abstractions.Game.IQuestionBankProvider>();
    }

    [Fact]
    public void Bank_SatisfiesTheLadderContract()
    {
        var bank = TableTop.Games.Fun.SlangCheckQuestionBank.All;
        bank.Count(q => q.Difficulty == Difficulty.Easy).Should().BeGreaterThanOrEqualTo(5);
        bank.Count(q => q.Difficulty == Difficulty.Medium).Should().BeGreaterThanOrEqualTo(5);
        bank.Count(q => q.Difficulty == Difficulty.Hard).Should().BeGreaterThanOrEqualTo(4);
        bank.Count(q => q.Difficulty == Difficulty.Extreme).Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void EveryQuestion_HasFourDistinctOptions_AndAValidAnswer()
    {
        var bank = TableTop.Games.Fun.SlangCheckQuestionBank.All;
        foreach (var q in bank)
        {
            q.Answers.Count.Should().Be(4);
            q.Answers.Values.Should().OnlyContain(o => !string.IsNullOrWhiteSpace(o));
            q.Answers.Values.Distinct().Count().Should().Be(4);
            q.Answers.Keys.Should().Contain(q.CorrectAnswer);
        }
    }
}

/// <summary>Deterministic clock for testing day-gated logic without sleeping
/// for real days — set UtcNow directly to simulate time passing.</summary>
internal sealed class FakeClock : TableTop.Hosting.Abstractions.IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);
}

/// <summary>
/// DayOneController — the advent-calendar campaign engine. These pin the
/// day-gating contract: Day 1 immediate, later days locked until real time
/// elapses, missed days accumulate rather than vanish, persistence survives
/// a fresh controller instance, and the campaign completes cleanly.
/// </summary>
public sealed class DayOneControllerTests : IDisposable
{
    // NOTE: this project's sandbox test-shim instantiates the test class ONCE
    // per class, not once per [Fact] (unlike real xUnit) — so a shared tmp-file
    // FIELD would leak completed-day state between methods. Each test below
    // therefore creates its own file via NewTmpFile(), as a LOCAL, never shared.
    private readonly List<string> _createdFiles = new();
    private readonly List<TableTop.Core.Abstractions.Players.IPlayer> _players =
        [TableTop.Core.Domain.Players.Player.Create("Bob"), TableTop.Core.Domain.Players.Player.Create("Alice")];

    private string NewTmpFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"dayone-test-{Guid.NewGuid():N}.json");
        _createdFiles.Add(path);
        return path;
    }

    public void Dispose() { foreach (var f in _createdFiles) if (File.Exists(f)) File.Delete(f); }

    private static IReadOnlyList<TableTop.Core.Abstractions.Cards.ICard> ThreeDayDeck() =>
    [
        TableTop.Core.Domain.Cards.StandardCard.Create("Day 1", "First",  Difficulty.Easy, "Test"),
        TableTop.Core.Domain.Cards.StandardCard.Create("Day 2", "Second", Difficulty.Easy, "Test"),
        TableTop.Core.Domain.Cards.StandardCard.Create("Day 3", "Third",  Difficulty.Easy, "Test"),
    ];

    [Fact]
    public void Day1_IsAvailableImmediately_OnFirstStart()
    {
        var tmpFile = NewTmpFile();
        var clock = new FakeClock();
        var ctrl = new DayOneController(ThreeDayDeck(), _players, "Test", clock, tmpFile);
        DayReadyEvent? ready = null;
        ctrl.DayReady += (_, e) => ready = e;

        ctrl.Start();

        ready.Should().NotBeNull();
        ready!.DayNumber.Should().Be(1);
        ctrl.HasPendingCard.Should().BeTrue();
    }

    [Fact]
    public void CompletingDay1_BeforeADayPasses_ResultsIn_AllCaughtUp()
    {
        var tmpFile = NewTmpFile();
        var clock = new FakeClock();
        var ctrl = new DayOneController(ThreeDayDeck(), _players, "Test", clock, tmpFile);
        AllCaughtUpEvent? caughtUp = null;
        ctrl.AllCaughtUp += (_, e) => caughtUp = e;

        ctrl.Start();
        ctrl.CompleteToday();   // same instant — no time has passed

        caughtUp.Should().NotBeNull();
        caughtUp!.DayNumber.Should().Be(1);
        ctrl.HasPendingCard.Should().BeFalse();
        var diff = (caughtUp.TimeUntilNextUnlock - TimeSpan.FromDays(1)).Duration();
        (diff < TimeSpan.FromSeconds(5)).Should().BeTrue();
    }

    [Fact]
    public void OneRealDayLater_Day2Unlocks()
    {
        var tmpFile = NewTmpFile();
        var clock = new FakeClock();
        var ctrl = new DayOneController(ThreeDayDeck(), _players, "Test", clock, tmpFile);
        ctrl.Start();
        ctrl.CompleteToday();

        clock.UtcNow = clock.UtcNow.AddDays(1).AddMinutes(1);   // tomorrow arrives

        DayReadyEvent? ready = null;
        ctrl.DayReady += (_, e) => ready = e;
        ctrl.CompleteToday();   // no-op: nothing pending yet until re-evaluated
        // Re-Start (as a fresh session would) to force re-evaluation against the new clock
        ctrl.Start();

        ready.Should().NotBeNull();
        ready!.DayNumber.Should().Be(2);
    }

    [Fact]
    public void MissedDays_AccumulateRatherThanVanish()
    {
        var tmpFile = NewTmpFile();
        var clock = new FakeClock();
        var ctrl = new DayOneController(ThreeDayDeck(), _players, "Test", clock, tmpFile);
        ctrl.Start();                       // Day 1 unlocked, not yet played

        clock.UtcNow = clock.UtcNow.AddDays(5);   // 5 real days pass, untouched
        ctrl.Start();                              // re-evaluate

        // Still Day 1 pending — days don't vanish, they just wait in order.
        ctrl.HasPendingCard.Should().BeTrue();
        ctrl.DayNumber.Should().Be(1);

        ctrl.CompleteToday();
        // Day 2 should be immediately available too — it was already unlocked.
        ctrl.HasPendingCard.Should().BeTrue();
        ctrl.DayNumber.Should().Be(2);

        ctrl.CompleteToday();
        ctrl.HasPendingCard.Should().BeTrue();
        ctrl.DayNumber.Should().Be(3);
    }

    [Fact]
    public void CompletingTheFinalDay_FiresCampaignComplete()
    {
        var tmpFile = NewTmpFile();
        var clock = new FakeClock();
        var ctrl = new DayOneController(ThreeDayDeck(), _players, "Test", clock, tmpFile);
        ctrl.Start();
        clock.UtcNow = clock.UtcNow.AddDays(5);
        ctrl.Start();
        ctrl.CompleteToday();   // Day 1
        ctrl.CompleteToday();   // Day 2

        CampaignCompleteEvent? complete = null;
        ctrl.CampaignComplete += (_, e) => complete = e;
        ctrl.CompleteToday();   // Day 3 — the last one

        complete.Should().NotBeNull();
        complete!.TotalDays.Should().Be(3);
    }

    [Fact]
    public void Persistence_SurvivesAFreshControllerInstance()
    {
        var tmpFile = NewTmpFile();
        var clock = new FakeClock();
        var first = new DayOneController(ThreeDayDeck(), _players, "Test", clock, tmpFile);
        first.Start();
        first.CompleteToday();   // Day 1 done, same instant — Day 2 not due yet
        first.Dispose();

        clock.UtcNow = clock.UtcNow.AddDays(1).AddMinutes(1);   // now Day 2 is due

        // A brand-new instance, same file, one real day later — must resume
        // at Day 2 pending (reading the ORIGINAL start date from disk), not
        // restart the campaign from today.
        var second = new DayOneController(ThreeDayDeck(), _players, "Test", clock, tmpFile);
        DayReadyEvent? ready = null;
        second.DayReady += (_, e) => ready = e;
        second.Start();

        ready.Should().NotBeNull();
        ready!.DayNumber.Should().Be(2);
    }

    [Fact]
    public void SingleDayDeck_CompletesImmediately()
    {
        var tmpFile = NewTmpFile();
        var clock = new FakeClock();
        var oneDay = new[] { TableTop.Core.Domain.Cards.StandardCard.Create("Day 1", "Only", Difficulty.Easy, "Test") };
        var ctrl = new DayOneController(oneDay, _players, "Test", clock, tmpFile);
        CampaignCompleteEvent? complete = null;
        ctrl.CampaignComplete += (_, e) => complete = e;
        ctrl.Start();
        ctrl.CompleteToday();
        complete.Should().NotBeNull();
    }
}

/// <summary>Day One's own content: 21 strictly-ordered days across three phases.</summary>
public sealed class DayOneModeTests
{
    [Fact]
    public void DayOne_IsRegistered_AsAdult_AndProvidesADailyDeck()
    {
        var node = ArchetypeRegistry.Default().FindById("couples.connection.dayone");
        node.Should().NotBeNull();
        node!.AgeRating.Should().Be(AgeRating.Adult);
        node.Modes[0].Should().BeAssignableTo<TableTop.Core.Abstractions.Game.IDailyDeckProvider>();
    }

    [Fact]
    public void Deck_Has21Days_InPhaseOrder()
    {
        var deck = TableTop.Games.Couples.DayOneCardBank.All;
        deck.Count.Should().Be(21);
        deck.Take(7).Should().OnlyContain(c => c.Category == "Spark");
        deck.Skip(7).Take(7).Should().OnlyContain(c => c.Category == "Warmth");
        deck.Skip(14).Take(7).Should().OnlyContain(c => c.Category == "Embers");
    }

    [Fact]
    public void RoutesToDayOneController_ViaTheFactory()
    {
        var deck = TableTop.Games.Couples.DayOneCardBank.All;
        // Capability check mirrors what ControllerFactory pattern-matches on —
        // confirms this mode is unambiguously routed, not accidentally caught
        // by the generic IGameModeDefinition arm.
        var mode = new TableTop.Games.Couples.DayOneMode();
        mode.Should().BeAssignableTo<TableTop.Core.Abstractions.Game.IDailyDeckProvider>();
        // NotBeAssignableTo, not `mode is IGameModeDefinition`: the pattern match
        // is a compile-time constant false here (CS0184), so it asserted nothing.
        // This form is a real reflection check and still fails if the mode ever
        // starts implementing the interface.
        mode.Should().NotBeAssignableTo<TableTop.Core.Abstractions.Game.IGameModeDefinition>();
    }
}

/// <summary>
/// Review finding: Chronology Challenge labelled its four ordering events
/// A)/B)/C)/D), which ChoiceCards misread as a pick-one quiz — tapping any
/// letter would wrongly complete the turn instead of letting the table order
/// all four. Fixed to 1)/2)/3)/4); this pins the fix.
/// </summary>
public sealed class ChronologyChallengeReviewFixTests
{
    [Fact]
    public void Events_AreNumbered_NotLettered_SoTheyNeverTriggerChoiceButtons()
    {
        var cards = TableTop.Games.Family.ChronologyChallengeCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(14);
        foreach (var c in cards)
        {
            ChoiceCards.IsChoiceCard(c.Description).Should().BeFalse(
                $"'{c.Title}' is an ordering puzzle, not a pick-one quiz");
            c.Description.Should().Contain("1) ").And.Contain("4) ");
            CardFaces.HasBack(CardText.StripHtml(c.Description)).Should().BeTrue(
                "the chronological answer key must still flip-reveal");
        }
    }
}

/// <summary>
/// Content shape for Truth or Dare, the mode with the richest card features.
///
/// <para>
/// This class used to be <c>JsonDeckLoadingTests</c> and pinned the JSON-first
/// load contract: that the .deck.json was genuinely consulted, that a missing
/// or corrupt file fell back cleanly, and that every exported file was
/// non-empty. All four of those went with the JSON deck path in 1.19.0. What
/// remains is what those tests were incidentally also proving and is still
/// true of the C# bank — restrictions and gender-directed prompts survive
/// whatever deck construction does to them.
/// </para>
/// </summary>
public sealed class TruthOrDareContentTests
{
    [Fact]
    public void Restrictions_CorrectlyGateCards()
    {
        // Truth or Dare has couples-only and adult-only restricted cards, and
        // they must survive deck construction as live restriction objects.
        var loaded = new TableTop.Games.TruthOrDareMode()
            .GetCards(new List<TableTop.Core.Abstractions.Players.IPlayer>());
        loaded.Count(c => c.Restriction is not null).Should().BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void GenderDirectedPrompts_ResolvePerPlayer()
    {
        var loaded = new TableTop.Games.TruthOrDareMode()
            .GetCards(new List<TableTop.Core.Abstractions.Players.IPlayer>());
        var styleRegret = loaded.First(c => c.Title == "Style Regret");
        styleRegret.Should().BeAssignableTo<TableTop.Core.Abstractions.Cards.IPromptCard>();

        var malePlayer = TableTop.Core.Domain.Players.Player.Create("_", new Dictionary<string, string> { ["gender"] = "male" });
        var femalePlayer = TableTop.Core.Domain.Players.Player.Create("_", new Dictionary<string, string> { ["gender"] = "female" });
        var prompt = (TableTop.Core.Abstractions.Cards.IPromptCard)styleRegret;
        prompt.ResolvePrompt(malePlayer).Should().Contain("men's fashion");
        prompt.ResolvePrompt(femalePlayer).Should().Contain("beauty or fashion");
    }
}

/// <summary>
/// GameplayOptions is the missing link between UI settings (difficulty range,
/// shuffle, session length) and the engine that actually deals cards. These
/// prove the options genuinely change controller behaviour, not just that
/// the plumbing compiles.
/// </summary>
public sealed class GameplayOptionsTests
{
    private static List<TableTop.Core.Abstractions.Players.IPlayer> TwoPlayers() =>
        [TableTop.Core.Domain.Players.Player.Create("Bob"), TableTop.Core.Domain.Players.Player.Create("Alice")];

    [Fact]
    public async Task DifficultyRange_ExcludesCardsOutsideIt()
    {
        // Fact or Fiction spans multiple difficulties — narrowing to Easy-only
        // must shrink the dealt deck and every card seen must be Easy.
        var options = new GameplayOptions { MinDifficulty = Difficulty.Easy, MaxDifficulty = Difficulty.Easy };
        var raw = await new ControllerFactory().CreateAsync(
            new TableTop.Games.FactOrFiction.FactOrFictionMode(), TwoPlayers(),
            maxRounds: 1, gameplayOptions: options);
        var ctrl = (ICardTurnController)raw;

        var seenDifficulties = new HashSet<Difficulty>();
        ctrl.CardReady += (_, e) => seenDifficulties.Add(e.Card.Difficulty);
        ctrl.Start();

        var fullDeckCount = TableTop.Games.FactOrFiction.FactOrFictionCardBank.All.Count;
        (ctrl.CardsRemaining + 1).Should().BeLessThan(fullDeckCount, "Easy-only should be a strict subset");
        seenDifficulties.Should().OnlyContain(d => d == Difficulty.Easy);
        ctrl.Dispose();
    }

    [Fact]
    public async Task ImpossibleDifficultyRange_ThrowsAHelpfulError()
    {
        // Synthetic Easy-only deck (not a real production deck, so this can
        // never drift as content evolves), narrowed to Extreme-only, must
        // fail clearly rather than silently deal an empty/broken game.
        var def = new InlineModeDef(TestFactory.MakeCards(5, Difficulty.Easy));
        var options = new GameplayOptions { MinDifficulty = Difficulty.Extreme, MaxDifficulty = Difficulty.Extreme };

        InvalidOperationException? caught = null;
        try
        {
            await CardTurnController.CreateAsync(def, TwoPlayers(), "Test", 1, new DifficultyProgressionStrategy(),
                  new CardTurnControllerOptions { Gameplay = options });
        }
        catch (InvalidOperationException ex) { caught = ex; }

        caught.Should().NotBeNull();
        caught!.Message.Should().Contain("excludes every card");
    }

    [Fact]
    public async Task ShuffleOff_PreservesOriginalDeckOrder()
    {
        // Deliberately reversed-difficulty synthetic deck: Extreme first,
        // Easy last. DifficultyProgressionStrategy targets Easy on round 0
        // regardless of shuffle — that's real, pre-existing behaviour, not
        // what ShuffleDeck controls. So the correct thing to prove is that
        // deck.Peek() finds the EARLIEST matching card in ORIGINAL order —
        // i.e. the single Easy card, which sits LAST in this deck — rather
        // than a random one shuffling could have moved anywhere.
        var cards = new List<ICard>
        {
            StandardCard.Create("Extreme1", "x", Difficulty.Extreme, "Test"),
            StandardCard.Create("Extreme2", "x", Difficulty.Extreme, "Test"),
            StandardCard.Create("TheOnlyEasy", "x", Difficulty.Easy, "Test"),
        };
        var def = new InlineModeDef(cards);
        var options = new GameplayOptions { ShuffleDeck = false };

        var ctrl = await CardTurnController.CreateAsync(
            def, TwoPlayers(), "Test", 1, new DifficultyProgressionStrategy(),
            new CardTurnControllerOptions { Gameplay = options });

        ICard? first = null;
        ctrl.CardReady += (_, e) => first ??= e.Card;
        ctrl.Start();

        first!.Title.Should().Be("TheOnlyEasy",
            "with shuffling off, deck order must be exactly as authored — this card is only reachable if order was preserved");
        ctrl.Dispose();
    }

    [Fact]
    public async Task CardsPerPlayer_CapsTheSessionLength()
    {
        var players = TwoPlayers();   // 2 players
        var options = new GameplayOptions { CardsPerPlayer = 3 };   // cap = 6
        var raw = await new ControllerFactory().CreateAsync(
            new TableTop.Games.FactOrFiction.FactOrFictionMode(), players,
            maxRounds: 20, gameplayOptions: options);
        var ctrl = (ICardTurnController)raw;
        ctrl.Start();

        (ctrl.CardsRemaining + 1).Should().Be(6, "2 players × 3 cards each = 6-card session");
        ctrl.Dispose();
    }

    [Fact]
    public async Task DefaultOptions_ReproduceOriginalUnrestrictedBehaviour()
    {
        // Omitting GameplayOptions entirely (existing call sites) must still
        // deal the full, shuffled deck — zero behaviour change for callers
        // that don't know this feature exists yet.
        var raw = await new ControllerFactory().CreateAsync(
            new TableTop.Games.FactOrFiction.FactOrFictionMode(), TwoPlayers(), maxRounds: 1);
        var ctrl = (ICardTurnController)raw;
        ctrl.Start();
        (ctrl.CardsRemaining + 1).Should().Be(TableTop.Games.FactOrFiction.FactOrFictionCardBank.All.Count);
        GameplayOptions.Default.IsUnrestricted.Should().BeTrue();
        ctrl.Dispose();
    }
}

/// <summary>
/// ArchetypeFilter is the game-selection half of "comprehensive settings" —
/// GameplayOptions shapes a session once you're in it; this shapes what's
/// even offered on the selection screen.
/// </summary>
public sealed class ArchetypeFilterTests
{
    [Fact]
    public void AllAgesCeiling_HidesTeenAndAdultContent()
    {
        var full = ArchetypeRegistry.Default().RootArchetypes;
        var filtered = new ArchetypeFilter(AgeRating.AllAges).Apply(full);

        void AssertNoRestrictedContent(IReadOnlyList<Archetype> nodes)
        {
            foreach (var n in nodes)
            {
                n.AgeRating.Should().Be(AgeRating.AllAges);
                AssertNoRestrictedContent(n.SubArchetypes);
            }
        }
        AssertNoRestrictedContent(filtered);
    }

    [Fact]
    public void AdultCeiling_ShowsEverything_SameCountAsUnfiltered()
    {
        var full = ArchetypeRegistry.Default().RootArchetypes;
        var filter = new ArchetypeFilter(AgeRating.Adult);
        filter.CountSurvivingModes(full).Should().Be(ArchetypeRegistry.Default().AllModes.Count);
    }

    [Fact]
    public void RaisingTheCeiling_MonotonicallyIncreasesAvailableModes()
    {
        var full = ArchetypeRegistry.Default().RootArchetypes;
        var allAgesCount = new ArchetypeFilter(AgeRating.AllAges).CountSurvivingModes(full);
        var teenCount = new ArchetypeFilter(AgeRating.Teen).CountSurvivingModes(full);
        var adultCount = new ArchetypeFilter(AgeRating.Adult).CountSurvivingModes(full);

        allAgesCount.Should().BeLessThan(teenCount);
        teenCount.Should().BeLessThan(adultCount);
    }

    [Fact]
    public void ParentsThatWouldEndUpEmpty_AreDroppedEntirely_NotShownAsEmptyCategories()
    {
        // The Couples tree is entirely Adult-rated content in this catalog —
        // at the AllAges ceiling it should vanish, not appear as an empty folder.
        var full = ArchetypeRegistry.Default().RootArchetypes;
        var filtered = new ArchetypeFilter(AgeRating.AllAges).Apply(full);
        filtered.Should().NotContain(a => a.Modes.Count == 0 && a.SubArchetypes.Count == 0);
    }
}

/// <summary>
/// 60 Seconds — category-listing under a fixed shared window. Target counts
/// must scale with difficulty (the whole point of difficulty-based scoring
/// here), and the fixed-timer house rule must be stated on every card.
/// </summary>
public sealed class SixtySecondsTests
{
    [Fact]
    public void SixtySeconds_IsRegistered_AsAllAges()
    {
        var node = ArchetypeRegistry.Default().FindById("fun.sixtyseconds");
        node.Should().NotBeNull();
        node!.AgeRating.Should().Be(AgeRating.AllAges);
    }

    [Fact]
    public void EveryCard_StatesTheFixedSixtySecondRule_AndATarget()
    {
        var cards = TableTop.Games.Family.SixtySecondsCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(30);
        foreach (var c in cards)
        {
            c.Description.Should().Contain("SIXTY SECONDS");
            c.Description.Should().Contain("Target:");
        }
    }

    [Fact]
    public void TargetCounts_ScaleDownAsDifficultyRises()
    {
        // Harder categories should ask for fewer items (they're intrinsically
        // harder to fill), not more — the difficulty is in the CATEGORY, and
        // difficulty-based scoring already rewards hitting a harder target.
        var cards = TableTop.Games.Family.SixtySecondsCardBank.All;
        int TargetOf(TableTop.Core.Abstractions.Cards.ICard c) =>
            int.Parse(System.Text.RegularExpressions.Regex.Match(c.Description, @"Target: (\d+)").Groups[1].Value);

        var avgEasy = cards.Where(c => c.Difficulty == Difficulty.Easy).Average(TargetOf);
        var avgExtreme = cards.Where(c => c.Difficulty == Difficulty.Extreme).Average(TargetOf);
        avgExtreme.Should().BeLessThan(avgEasy);
    }

    [Fact]
    public void SixtySeconds_NeverAccidentallyTriggersFlipOrQuizMechanics()
    {
        foreach (var c in TableTop.Games.Family.SixtySecondsCardBank.All)
        {
            CardFaces.HasBack(CardText.StripHtml(c.Description)).Should().BeFalse();
            ChoiceCards.IsChoiceCard(c.Description).Should().BeFalse();
        }
    }

    [Fact]
    public void HittingATarget_ScoresMore_ForHarderCategories()
    {
        var scoring = new DifficultyBasedScoringStrategy();
        var easyCard = TableTop.Games.Family.SixtySecondsCardBank.All.First(c => c.Difficulty == Difficulty.Easy);
        var extremeCard = TableTop.Games.Family.SixtySecondsCardBank.All.First(c => c.Difficulty == Difficulty.Extreme);
        var player = TableTop.Core.Domain.Players.Player.Create("P");

        var easyScore = scoring.CalculateScore(easyCard, player, CardOutcome.Completed);
        var extremeScore = scoring.CalculateScore(extremeCard, player, CardOutcome.Completed);
        extremeScore.Should().BeGreaterThan(easyScore);
    }
}

/// <summary>
/// Logic Lab — flip-backed reasoning puzzles. Every front must be
/// spoiler-free, every back must exist and explain, and nothing may
/// accidentally trigger the A–D quiz machinery.
/// </summary>
public sealed class LogicLabTests
{
    [Fact]
    public void LogicLab_IsRegistered_AsAllAges()
    {
        var node = ArchetypeRegistry.Default().FindById("classroom.logiclab");
        node.Should().NotBeNull();
        node!.AgeRating.Should().Be(AgeRating.AllAges);
    }

    [Fact]
    public void EveryPuzzle_IsFlippable_WithASpoilerFreeFront_AndAnExplainedBack()
    {
        var cards = TableTop.Games.School.LogicLabCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(33);
        foreach (var c in cards)
        {
            var plain = CardText.StripHtml(c.Description);
            CardFaces.HasBack(plain).Should().BeTrue($"'{c.Title}' must flip");
            var (front, back) = CardFaces.Split(plain);
            front.Should().NotContain("Answer:", $"'{c.Title}' front must be spoiler-free");
            back!.Length.Should().BeGreaterThan(20, $"'{c.Title}' back must explain, not just state");
            ChoiceCards.IsChoiceCard(c.Description).Should().BeFalse($"'{c.Title}' is a puzzle, not a quiz");
        }
    }

    [Fact]
    public void DeductionClues_AreNumbered_NotLettered()
    {
        // Numbered clue lists (1. 2. 3.) — the same convention Odd One Out
        // established so clue lines can never read as tappable A–D answers.
        var deductions = TableTop.Games.School.LogicLabCardBank.All
            .Where(c => c.Category == "Deduction" && c.Description.Contains("1."));
        deductions.Should().NotBeEmpty();
        foreach (var c in deductions)
            c.Description.Should().NotContain("A)");
    }

    [Fact]
    public void HarderPuzzles_ScoreMore()
    {
        var scoring = new DifficultyBasedScoringStrategy();
        var player = TableTop.Core.Domain.Players.Player.Create("P");
        var easy = TableTop.Games.School.LogicLabCardBank.All.First(c => c.Difficulty == Difficulty.Easy);
        var extreme = TableTop.Games.School.LogicLabCardBank.All.First(c => c.Difficulty == Difficulty.Extreme);
        scoring.CalculateScore(extreme, player, CardOutcome.Completed)
            .Should().BeGreaterThan(scoring.CalculateScore(easy, player, CardOutcome.Completed));
    }
}

/// <summary>
/// Spy vs Spouse — the hidden-information couples deck. The contract that
/// makes the secrecy mechanic work: every Briefing must carry the
/// silent-read instruction, the consent hatch must be universal, and the
/// mission economy must reward difficulty.
/// </summary>
public sealed class SpyVsSpouseTests
{
    [Fact]
    public void SpyVsSpouse_IsRegistered_AsAdult()
    {
        var node = ArchetypeRegistry.Default().FindById("couples.connection.spyvsspouse");
        node.Should().NotBeNull();
        node!.AgeRating.Should().Be(AgeRating.Adult);
    }

    [Fact]
    public void EveryBriefing_DemandsSilentReading_AndNamesTheMission()
    {
        var briefings = TableTop.Games.Couples.SpyVsSpouseCardBank.All
            .Where(c => c.Category == "Briefing").ToList();
        briefings.Count.Should().BeGreaterThanOrEqualTo(14);
        foreach (var c in briefings)
        {
            c.Description.Should().Contain("SILENTLY", $"'{c.Title}' is a secret briefing");
            c.Description.Should().Contain("YOUR MISSION:");
            c.Description.Should().Contain("MISSION COMPLETE");
        }
    }

    [Fact]
    public void CategoryEconomy_IsBalanced_CoverTrafficOutnumbersNothing()
    {
        var bank = TableTop.Games.Couples.SpyVsSpouseCardBank.All;
        var byCat = bank.GroupBy(c => c.Category).ToDictionary(g => g.Key, g => g.Count());
        byCat["Briefing"].Should().Be(byCat["Cover Story"],
            "missions and the cover traffic they hide in should be 1:1 so missions always have somewhere to run");
        byCat["Counterintel"].Should().BeGreaterThanOrEqualTo(5);
        byCat["Dead Drop"].Should().BeGreaterThanOrEqualTo(5);
        bank.Count.Should().BeGreaterThanOrEqualTo(40);
    }

    [Fact]
    public void HarderMissions_PayMore()
    {
        var scoring = new DifficultyBasedScoringStrategy();
        var player = TableTop.Core.Domain.Players.Player.Create("Agent");
        var easy = TableTop.Games.Couples.SpyVsSpouseCardBank.All.First(c => c.Category == "Briefing" && c.Difficulty == Difficulty.Easy);
        var extreme = TableTop.Games.Couples.SpyVsSpouseCardBank.All.First(c => c.Category == "Briefing" && c.Difficulty == Difficulty.Extreme);
        scoring.CalculateScore(extreme, player, CardOutcome.Completed)
            .Should().BeGreaterThan(scoring.CalculateScore(easy, player, CardOutcome.Completed));
    }

    [Fact]
    public void SpyVsSpouse_NeverTriggersFlipOrQuizMechanics()
    {
        foreach (var c in TableTop.Games.Couples.SpyVsSpouseCardBank.All)
        {
            CardFaces.HasBack(CardText.StripHtml(c.Description)).Should().BeFalse(
                $"'{c.Title}' — briefings must never flip-hide their text; secrecy is social, not mechanical");
            ChoiceCards.IsChoiceCard(c.Description).Should().BeFalse();
        }
    }
}

/// <summary>
/// In Your Shoes — perspective-swap prediction. Every card must flip (prompt
/// front, scoring-guide back), instruct the guesser to answer AS their partner,
/// and give a three-tier reading so the table judges consistently.
/// </summary>
public sealed class InYourShoesTests
{
    [Fact]
    public void InYourShoes_IsRegistered_AsTeen()
    {
        var node = ArchetypeRegistry.Default().FindById("couples.connection.inyourshoes");
        node.Should().NotBeNull();
        node!.AgeRating.Should().Be(AgeRating.Teen);
    }

    [Fact]
    public void EveryCard_Flips_WithAThreeTierReading_AndTheSwapInstruction()
    {
        var cards = TableTop.Games.Couples.InYourShoesCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(20);
        foreach (var c in cards)
        {
            var plain = CardText.StripHtml(c.Description);
            CardFaces.HasBack(plain).Should().BeTrue($"'{c.Title}' must flip to a scoring guide");
            var (front, back) = CardFaces.Split(plain);
            // The swap is the whole mechanic — must be stated on the front.
            front.ToLowerInvariant().Should().Contain("as your partner", $"'{c.Title}' must tell you to answer as your partner");
            // Three-tier reading keeps scoring consistent.
            back!.Should().Contain("Nailed it");
            back.Should().Contain("Close");
            back.Should().Contain("Missed");
        }
    }

    [Fact]
    public void DeeperTiers_ScoreMore_ThanEverydayGuesses()
    {
        var scoring = new DifficultyBasedScoringStrategy();
        var player = TableTop.Core.Domain.Players.Player.Create("P");
        var everyday = TableTop.Games.Couples.InYourShoesCardBank.All.First(c => c.Category == "Everyday" && c.Difficulty == Difficulty.Easy);
        var us = TableTop.Games.Couples.InYourShoesCardBank.All.First(c => c.Category == "Us" && c.Difficulty == Difficulty.Extreme);
        scoring.CalculateScore(us, player, CardOutcome.Completed)
            .Should().BeGreaterThan(scoring.CalculateScore(everyday, player, CardOutcome.Completed));
    }
}

/// <summary>
/// The Long Game — appreciation & noticing. A conversation deck: it must NOT
/// flip (no hidden answer), must demand specificity, and carries the Keeper
/// mechanic across four escalating movements.
/// </summary>
public sealed class TheLongGameTests
{
    [Fact]
    public void TheLongGame_IsRegistered_AsAdult()
    {
        var node = ArchetypeRegistry.Default().FindById("couples.connection.longgame");
        node.Should().NotBeNull();
        node!.AgeRating.Should().Be(AgeRating.Adult);
    }

    [Fact]
    public void EveryCard_IsSingleFaced_AndInvitesAKeeper()
    {
        var cards = TableTop.Games.Couples.TheLongGameCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(20);
        foreach (var c in cards)
        {
            // Conversation deck — no hidden answer face to reveal.
            CardFaces.HasBack(CardText.StripHtml(c.Description)).Should().BeFalse(
                $"'{c.Title}' is a conversation card and must not flip");
            ChoiceCards.IsChoiceCard(c.Description).Should().BeFalse();
            // The Keeper mechanic is what makes this mode itself.
            c.Description.Should().Contain("Keeper", $"'{c.Title}' must carry the Keeper invitation");
        }
    }

    [Fact]
    public void AllFourMovements_ArePresent()
    {
        var cats = TableTop.Games.Couples.TheLongGameCardBank.All.Select(c => c.Category).Distinct().ToList();
        cats.Should().Contain("Noticing");
        cats.Should().Contain("Gratitude");
        cats.Should().Contain("Weathered");
        cats.Should().Contain("Vows");
    }

    [Fact]
    public void EveryCard_DemandsSpecificity()
    {
        // The mode's whole ethos: never vague. Each card's text should push
        // toward a concrete, specific answer rather than a general feeling.
        var cards = TableTop.Games.Couples.TheLongGameCardBank.All;
        var specificityWords = new[] { "specific", "exact", "one ", "name ", "moment", "plainly", "honestly" };
        foreach (var c in cards)
        {
            var text = CardText.StripHtml(c.Description).ToLowerInvariant();
            specificityWords.Any(w => text.Contains(w))
                .Should().BeTrue($"'{c.Title}' should push for a specific answer");
        }
    }
}

/// <summary>
/// Afterglow — explicit adult intimacy, consent-first. The tests exist to
/// guarantee the consent structure can't quietly rot away in a future edit:
/// the deck must OPEN on the consent ritual, CLOSE on aftercare, and every
/// explicit card must carry the opt-in/pass language.
/// </summary>
public sealed class AfterglowTests
{
    private static IReadOnlyList<TableTop.Core.Abstractions.Cards.ICard> Deck =>
        TableTop.Games.Couples.AfterglowCardBank.All;

    [Fact]
    public void Afterglow_IsRegistered_AsAdult()
    {
        var node = ArchetypeRegistry.Default().FindById("couples.intimate");
        node.Should().NotBeNull();
        node!.AgeRating.Should().Be(AgeRating.Adult);
        node.Modes.Any(m => m.Name == "Afterglow").Should().BeTrue();
    }

    [Fact]
    public void TheDeck_OpensWithTheConsentRitual()
    {
        // The first cards must be the consent setup — safeword, boundaries,
        // check-in — so the game can't be played past the start without it.
        Deck[0].Category.Should().Be("Consent");
        Deck[1].Category.Should().Be("Consent");
        Deck[2].Category.Should().Be("Consent");
        Deck[0].Description.ToLowerInvariant().Should().Contain("safeword");
    }

    [Fact]
    public void ConsentRitual_CoversSafeword_Boundaries_AndCheckIn()
    {
        var consent = string.Join(" ", Deck.Where(c => c.Category == "Consent")
            .Select(c => c.Description)).ToLowerInvariant();
        consent.Should().Contain("safeword");
        consent.Should().Contain("off the table");   // boundaries
        consent.Should().Contain("colour");          // check-in system
        consent.Should().Contain("enthusiasm is the only yes");
    }

    [Fact]
    public void TheDeck_ClosesOnAftercare()
    {
        // The last cards must be aftercare — how you land matters.
        Deck[^1].Category.Should().Be("Aftercare");
        Deck.Count(c => c.Category == "Aftercare").Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void EveryExplicitCard_CarriesTheOptInAndFreePassLanguage()
    {
        // Every play card (not the consent/aftercare rituals) must remind
        // players it's an invitation and that passing is always free.
        var playCategories = new[] { "Warm Up", "Turn Up", "Heat", "Undone" };
        var playCards = Deck.Where(c => playCategories.Contains(c.Category)).ToList();
        playCards.Count.Should().BeGreaterThanOrEqualTo(10);
        foreach (var c in playCards)
        {
            var text = c.Description.ToLowerInvariant();
            text.Should().Contain("invitation", $"'{c.Title}' must frame itself as an invitation");
            text.Should().Contain("pass is always free", $"'{c.Title}' must state passing is free");
            text.Should().Contain("enthusiasm is the only yes", $"'{c.Title}' must state the enthusiasm rule");
        }
    }

    [Fact]
    public void PassLabel_MakesClearPassingIsAlwaysOkay()
    {
        var mode = new TableTop.Games.Couples.AfterglowMode();
        mode.SkipLabel.ToLowerInvariant().Should().Contain("always");
    }
}

/// <summary>
/// Undivided — the giver/receiver variation of Afterglow. Same consent spine
/// (open on the ritual, close on aftercare, opt-in language throughout), plus
/// the Swap mechanic and the rule that the RECEIVER steers — the thing that
/// makes it a distinct variation rather than a reskin.
/// </summary>
public sealed class UndividedTests
{
    private static IReadOnlyList<TableTop.Core.Abstractions.Cards.ICard> Deck =>
        TableTop.Games.Couples.UndividedCardBank.All;

    [Fact]
    public void Undivided_IsRegistered_AsAdult_AlongsideAfterglow()
    {
        var node = ArchetypeRegistry.Default().FindById("couples.intimate");
        node.Should().NotBeNull();
        node!.AgeRating.Should().Be(AgeRating.Adult);
        node.Modes.Any(m => m.Name == "Undivided").Should().BeTrue();
        node.Modes.Any(m => m.Name == "Afterglow").Should().BeTrue("it's a variation, both should be present");
    }

    [Fact]
    public void KeepsAfterglowsConsentSpine_OpenRitual_AndAftercareClose()
    {
        Deck[0].Category.Should().Be("Consent");
        Deck[1].Category.Should().Be("Consent");
        Deck[2].Category.Should().Be("Consent");
        Deck[^1].Category.Should().Be("Aftercare");

        var consent = string.Join(" ", Deck.Where(c => c.Category == "Consent").Select(c => c.Description)).ToLowerInvariant();
        consent.Should().Contain("safeword");
        consent.Should().Contain("off the table");
        consent.Should().Contain("enthusiasm is the only yes");
    }

    [Fact]
    public void HasTheSwapMechanic_ThatMakesItAVariation()
    {
        // The turn-taking swap is the distinguishing mechanic — must be present
        // at least twice (there and back) and sit between giving stretches.
        var swaps = Deck.Where(c => c.Category == "Swap").ToList();
        swaps.Count.Should().BeGreaterThanOrEqualTo(2);
        foreach (var s in swaps)
            s.Description.ToLowerInvariant().Should().Contain("swap");
    }

    [Fact]
    public void EveryGivingCard_SaysTheReceiverSteers_AndPassIsFree()
    {
        var giving = new[] { "Attention", "Devotion", "Worship" };
        var cards = Deck.Where(c => giving.Contains(c.Category)).ToList();
        cards.Count.Should().BeGreaterThanOrEqualTo(10);
        foreach (var c in cards)
        {
            var text = c.Description.ToLowerInvariant();
            text.Should().Contain("receiver steers", $"'{c.Title}' must mark that the receiver is in control");
            text.Should().Contain("pass is always free", $"'{c.Title}' must state passing is free");
            text.Should().Contain("enthusiasm is the only green light", $"'{c.Title}' must state the enthusiasm rule");
        }
    }
}

/// <summary>
/// The three new family party games — Letter Rush (Scattergories), Act It Out
/// (charades), Draw It (Pictionary). All AllAges, all registered, each with a
/// bank that matches its mechanic.
/// </summary>
public sealed class FamilyPartyGamesTests
{
    [Fact]
    public void AllThree_AreRegistered_AsAllAges()
    {
        var reg = ArchetypeRegistry.Default();
        foreach (var id in new[] { "fun.family.letterrush", "fun.family.actitout", "fun.family.drawit" })
        {
            var node = reg.FindById(id);
            node.Should().NotBeNull($"{id} should be registered");
            node!.AgeRating.Should().Be(AgeRating.AllAges);
        }
    }

    [Fact]
    public void LetterRush_EveryCard_GivesALetterAndFiveCategories()
    {
        var cards = TableTop.Games.Family.LetterRushCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(12);
        foreach (var c in cards)
        {
            var text = c.Description;
            text.Should().Contain("Your letter:", $"'{c.Title}' must state a letter");
            // Five categories rendered as bullet lines.
            CardText.StripHtml(text).Split('\n').Count(l => l.TrimStart().StartsWith("•"))
                .Should().Be(5, $"'{c.Title}' should list five categories");
            text.Should().Contain("Match someone", $"'{c.Title}' must carry the no-points-for-matching rule");
        }
    }

    [Fact]
    public void ActItOut_AndDrawIt_EveryCard_Flips_ToItsAnswer()
    {
        // Both are guessing games — the answer must live on the back face.
        foreach (var c in TableTop.Games.Family.ActItOutCardBank.All)
            CardFaces.HasBack(CardText.StripHtml(c.Description)).Should().BeTrue($"Act It Out '{c.Title}' must flip");
        foreach (var c in TableTop.Games.Family.DrawItCardBank.All)
            CardFaces.HasBack(CardText.StripHtml(c.Description)).Should().BeTrue($"Draw It '{c.Title}' must flip");
    }

    [Fact]
    public void ActItOut_ForbidsWords_AndDrawIt_ForbidsLetters()
    {
        TableTop.Games.Family.ActItOutCardBank.All.Should().OnlyContain(
            c => c.Description.ToLowerInvariant().Contains("no words"));
        TableTop.Games.Family.DrawItCardBank.All.Should().OnlyContain(
            c => c.Description.ToLowerInvariant().Contains("no words, letters"));
    }

    [Fact]
    public void EachGame_SpansMultipleCategories_AndDifficulties()
    {
        foreach (var bank in new[]
        {
            (IReadOnlyList<TableTop.Core.Abstractions.Cards.ICard>)TableTop.Games.Family.LetterRushCardBank.All,
            TableTop.Games.Family.ActItOutCardBank.All,
            TableTop.Games.Family.DrawItCardBank.All,
        })
        {
            bank.Select(c => c.Category).Distinct().Count().Should().BeGreaterThanOrEqualTo(4);
            bank.Select(c => c.Difficulty).Distinct().Count().Should().BeGreaterThanOrEqualTo(3);
        }
    }
}

/// <summary>
/// The classroom general-knowledge subject decks (geography, science, history, — World Explorer (geography),
/// Science Sprint (science), Through the Ages (history & culture). All AllAges,
/// all multiple-choice, and each card must be internally well-formed: four
/// distinct options and a correct-answer label that points at a real option.
/// </summary>
public sealed class ClassroomGeneralKnowledgeTests
{
    private static readonly (string id, IReadOnlyList<TableTop.Core.Abstractions.Cards.ICard> bank)[] Decks =
    {
        ("classroom.geography", TableTop.Games.School.WorldExplorerCardBank.All),
        ("classroom.science",   TableTop.Games.School.ScienceSprintCardBank.All),
        ("classroom.history",   TableTop.Games.School.ThroughTheAgesCardBank.All),
        ("classroom.sport",     TableTop.Games.School.SportingChanceCardBank.All),
        ("classroom.animals",   TableTop.Games.School.AnimalKingdomCardBank.All),
        ("classroom.music",     TableTop.Games.School.SoundAndSongCardBank.All),
        ("classroom.maths",     TableTop.Games.School.NumberWorldCardBank.All),
    };

    [Fact]
    public void AllThree_AreRegistered_AsAllAges_UnderClassroom()
    {
        var reg = ArchetypeRegistry.Default();
        foreach (var (id, _) in Decks)
        {
            var node = reg.FindById(id);
            node.Should().NotBeNull($"{id} should be registered");
            node!.AgeRating.Should().Be(AgeRating.AllAges);
        }
    }

    [Fact]
    public void EveryCard_IsMultipleChoice_WithFourDistinctOptions()
    {
        foreach (var (id, bank) in Decks)
        {
            bank.Count.Should().BeGreaterThanOrEqualTo(24, $"{id} should be a substantial deck");
            foreach (var card in bank)
            {
                var mc = card as TableTop.Core.Domain.Cards.MultipleChoiceCard;
                mc.Should().NotBeNull($"every {id} card must be multiple choice");
                var opts = new[]
                {
                    mc!.Answers[AnswerLabel.A], mc.Answers[AnswerLabel.B],
                    mc.Answers[AnswerLabel.C], mc.Answers[AnswerLabel.D],
                };
                opts.Should().OnlyContain(o => !string.IsNullOrWhiteSpace(o));
                opts.Distinct().Count().Should().Be(4, $"a {id} card has duplicate options: {card.Title}");
            }
        }
    }

    [Fact]
    public void EveryCard_HasACorrectAnswer_PointingAtARealOption()
    {
        foreach (var (id, bank) in Decks)
            foreach (var card in bank.Cast<TableTop.Core.Domain.Cards.MultipleChoiceCard>())
            {
                card.Answers.ContainsKey(card.CorrectAnswer)
                    .Should().BeTrue($"{id} card '{card.Title}' marks a correct answer that exists");
                card.Answers[card.CorrectAnswer].Should().NotBeNullOrWhiteSpace();
            }
    }

    [Fact]
    public void EachDeck_SpansItsSubjectCategories_AndAllFourDifficulties()
    {
        foreach (var (id, bank) in Decks)
        {
            bank.Select(c => c.Category).Distinct().Count().Should().BeGreaterThanOrEqualTo(5, $"{id} should cover several topics");
            bank.Select(c => c.Difficulty).Distinct().Count().Should().Be(4, $"{id} should use all four difficulties");
        }
    }
}

/// <summary>
/// Between the Two of You — adult couples dynamics self-knowledge quiz. The
/// tests guarantee it stays a growth tool: every axis has a Results card, it's
/// couples-only, and it keeps the consent / no-wrong-answer framing.
/// </summary>
public sealed class BetweenTheTwoOfYouTests
{
    private static IReadOnlyList<TableTop.Core.Abstractions.Cards.ICard> Deck =>
        TableTop.Games.Couples.BetweenTheTwoOfYouCardBank.All;

    [Fact]
    public void IsRegistered_AsAdult_InCouplesConnection()
    {
        var node = ArchetypeRegistry.Default().FindById("couples.connection.dynamics");
        node.Should().NotBeNull();
        node!.AgeRating.Should().Be(AgeRating.Adult);
    }

    [Fact]
    public void CoversTheKeyDynamicAxes()
    {
        var cats = Deck.Select(c => c.Category).Distinct().ToList();
        cats.Should().Contain("Lead & Follow");   // dom/sub lean
        cats.Should().Contain("Give & Receive");  // giver/receiver lean
        cats.Should().Contain("Plan & Spark");
        cats.Should().Contain("Words & Touch");
        cats.Should().Contain("Bold & Cosy");
    }

    [Fact]
    public void EveryAxis_EndsWithAResultsCard_ThatReadsAllFourLeans()
    {
        var results = Deck.Where(c => c.Category == "Results").ToList();
        // One Results card per axis (5 axes).
        results.Count.Should().BeGreaterThanOrEqualTo(5);
        foreach (var r in results)
        {
            var text = r.Description;
            text.Should().Contain("Mostly A");
            text.Should().Contain("Mostly B");
            // Every results card offers a growth edge, not just a label.
            text.ToLowerInvariant().Should().Contain("grow");
        }
    }

    [Fact]
    public void EveryQuestion_IsCouplesOnly()
    {
        // The scenario questions (not Results/Grow synthesis cards) must be
        // gated to couples.
        var questionCategories = new[] { "Lead & Follow", "Give & Receive", "Plan & Spark", "Words & Touch", "Bold & Cosy" };
        var questions = Deck.Where(c => questionCategories.Contains(c.Category)).ToList();
        questions.Count.Should().BeGreaterThanOrEqualTo(10);
        foreach (var q in questions)
            q.Restriction.Should().NotBeNull($"'{q.Title}' should be couples-only");
    }

    [Fact]
    public void KeepsTheConsent_AndNoWrongAnswer_Framing()
    {
        var all = string.Join(" ", Deck.Select(c => c.Description)).ToLowerInvariant();
        all.Should().Contain("enthusiasm is the only yes");
        // Explicitly reassures there's no 'better' answer.
        (all.Contains("no result here is better") || all.Contains("no wrong answer") || all.Contains("isn't a flaw"))
            .Should().BeTrue("the quiz must reassure there's no wrong/better answer");
    }

    [Fact]
    public void ClosesOnAGrowTogetherSynthesis()
    {
        Deck[^1].Category.Should().Be("Grow Together");
        Deck[^1].Description.ToLowerInvariant().Should().Contain("where you match");
    }
}

/// <summary>
/// Regression tests for the CardTurnController.AdvanceTurn() stack overflow.
///
/// The bug: when the engine couldn't serve the current player an eligible card
/// it returned null WITHOUT consuming the deck, and the controller retried by
/// calling itself. With a heavily restricted deck (couples-only cards, or a
/// narrow difficulty filter) no player could ever play, the deck never
/// emptied, and the recursion ran until the stack blew. The fix turns that
/// retry into a bounded loop that ends the game cleanly instead.
/// </summary>
public sealed class AdvanceTurnExhaustionTests
{
    private static IReadOnlyList<TableTop.Core.Abstractions.Players.IPlayer> TwoPlayers() =>
        new List<TableTop.Core.Abstractions.Players.IPlayer>
        {
            TableTop.Core.Domain.Players.Player.Create("Ana"),
            TableTop.Core.Domain.Players.Player.Create("Ben"),
        }.AsReadOnly();

    [Fact]
    public async Task RestrictedDeck_DrivenPastExhaustion_EndsCleanly_InsteadOfOverflowing()
    {
        // Between the Two of You is couples-restricted, which is exactly the
        // shape that used to crash.
        var controller = (ICardTurnController)(await new ControllerFactory()
            .CreateAsync(new TableTop.Games.Couples.BetweenTheTwoOfYouMode(), TwoPlayers(), maxRounds: 100));

        var ended = false;
        controller.GameEnded += (_, _) => ended = true;
        controller.Start();

        // Far more outcomes than there are cards — the old code overflowed here.
        for (var i = 0; i < 500 && !ended; i++)
            controller.RecordOutcome(CardOutcome.Completed);

        ended.Should().BeTrue("the game must end cleanly rather than recursing forever");
        controller.Dispose();
    }

    [Fact]
    public async Task EveryCardTurnMode_CanBeDrivenToExhaustion_WithoutCrashing()
    {
        var modes = new List<TableTop.Core.Abstractions.Game.IGameMode>();
        void Walk(IEnumerable<Archetype> nodes)
        {
            foreach (var n in nodes) { modes.AddRange(n.Modes); Walk(n.SubArchetypes); }
        }
        Walk(ArchetypeRegistry.Default().RootArchetypes);

        var players = TwoPlayers();
        foreach (var mode in modes)
        {
            var controller = await new ControllerFactory()
                .CreateAsync(mode, players, maxRounds: 100);

            if (controller is ICardTurnController turn)
            {
                var ended = false;
                turn.GameEnded += (_, _) => ended = true;
                turn.Start();
                for (var i = 0; i < 300 && !ended; i++)
                    turn.RecordOutcome(CardOutcome.Completed);

                ended.Should().BeTrue($"'{mode.Name}' should end cleanly when driven to exhaustion");
            }

            controller.Dispose();
        }
    }
}

/// <summary>
/// Guards the other side of the AdvanceTurn fix: the bounded retry must not
/// end games EARLY. A player being unable to play one card is normal — the
/// game should only stop when nobody can play anything at all.
/// </summary>
public sealed class AdvanceTurnNoPrematureEndTests
{
    private static IReadOnlyList<TableTop.Core.Abstractions.Players.IPlayer> ThreePlayers() =>
        new List<TableTop.Core.Abstractions.Players.IPlayer>
        {
            TableTop.Core.Domain.Players.Player.Create("Ana"),
            TableTop.Core.Domain.Players.Player.Create("Ben"),
            TableTop.Core.Domain.Players.Player.Create("Cal"),
        }.AsReadOnly();

    private static int DealAll(TableTop.Core.Abstractions.Game.IGameMode mode, GameplayOptions? options = null)
    {
        var controller = (ICardTurnController)new ControllerFactory()
            .CreateAsync(mode, ThreePlayers(), maxRounds: 500, gameplayOptions: options)
            .GetAwaiter().GetResult();

        var dealt = 0; var ended = false;
        controller.CardReady += (_, _) => dealt++;
        controller.GameEnded += (_, _) => ended = true;
        controller.Start();
        for (var i = 0; i < 1000 && !ended; i++)
            controller.RecordOutcome(CardOutcome.Completed);
        controller.Dispose();
        return dealt;
    }

    [Fact]
    public void UnrestrictedDecks_DealEveryCard_BeforeEnding()
    {
        DealAll(new TableTop.Games.Family.SixtySecondsMode())
            .Should().Be(TableTop.Games.Family.SixtySecondsMode.GetCards().Count);
        DealAll(new TableTop.Games.Family.LetterRushMode())
            .Should().Be(TableTop.Games.Family.LetterRushMode.GetCards().Count);
        DealAll(new TableTop.Games.School.WorldExplorerMode())
            .Should().Be(TableTop.Games.School.WorldExplorerMode.GetCards().Count);
    }

    [Fact]
    public void DifficultyFilteredDeck_StillDealsEveryEligibleCard()
    {
        var options = new GameplayOptions
        {
            MinDifficulty = Difficulty.Easy,
            MaxDifficulty = Difficulty.Easy,
        };
        var expected = TableTop.Games.School.WorldExplorerMode.GetCards()
            .Count(c => c.Difficulty == Difficulty.Easy);

        DealAll(new TableTop.Games.School.WorldExplorerMode(), options)
            .Should().Be(expected, "filtering must not cause an early end");
    }
}

/// <summary>
/// Regression tests for UndoLastTurn.
///
/// The bug: undo reversed the score and re-presented the undone card in the UI,
/// but never rewound the ENGINE — which had already advanced to the next player
/// and drawn their card. So the next recorded outcome was applied to the wrong
/// player and a card that was never on screen, and the dealt card was lost from
/// the deck. Game.RewindTurn now restores the turn, returns the dealt card, and
/// steps the rotation and round counters back.
/// </summary>
public sealed class UndoLastTurnTests
{
    private sealed class UndoProbeMode : TableTop.Games.Base.BaseGameModeDefinition
    {
        public override string Name => "UndoProbe";
        public override string Description => "undo regression probe";
        protected override IScoringStrategy BuildScoring() => new DifficultyBasedScoringStrategy();
        protected override IReadOnlyList<TableTop.Core.Abstractions.Cards.ICard> BuildCards(
            IReadOnlyList<TableTop.Core.Abstractions.Players.IPlayer> players)
        {
            var cards = new List<TableTop.Core.Abstractions.Cards.ICard>();
            for (var i = 0; i < 12; i++)
                cards.Add(StandardCard.Create($"Card{i}", "x",
                    i % 2 == 0 ? Difficulty.Easy : Difficulty.Hard, "T"));
            return cards;
        }
    }

    private static ICardTurnController Make(int maxRounds = 20) =>
        (ICardTurnController)new ControllerFactory().CreateAsync(
            new UndoProbeMode(),
            new List<TableTop.Core.Abstractions.Players.IPlayer>
            {
                TableTop.Core.Domain.Players.Player.Create("Ana"),
                TableTop.Core.Domain.Players.Player.Create("Ben"),
                TableTop.Core.Domain.Players.Player.Create("Cal"),
            }.AsReadOnly(),
            maxRounds: maxRounds).GetAwaiter().GetResult();

    [Fact]
    public void AfterUndo_TheNextOutcome_AppliesToTheRePresentedTurn()
    {
        var controller = Make();
        var dealt = new List<string>();
        var scored = new List<string>();
        controller.CardReady += (_, e) => dealt.Add(e.PlayerName);
        controller.TurnResult += (_, e) => scored.Add(e.PlayerName);

        controller.Start();                                   // dealt to Ana
        controller.RecordOutcome(CardOutcome.Completed);      // Ana scores, Ben dealt
        controller.UndoLastTurn();                            // back to Ana
        controller.RecordOutcome(CardOutcome.Completed);      // must credit Ana, not Ben

        scored[^1].Should().Be("Ana", "the re-recorded outcome belongs to the re-presented player");
        controller.Dispose();
    }

    [Fact]
    public void Undo_ReturnsTheDealtCard_SoNoCardIsLost()
    {
        var controller = Make();
        controller.Start();
        var before = controller.CardsRemaining;

        controller.RecordOutcome(CardOutcome.Completed);
        controller.UndoLastTurn();

        controller.CardsRemaining.Should().Be(before, "the card dealt for the abandoned turn goes back");
        controller.Dispose();
    }

    [Fact]
    public void Undo_KeepsTurnOrder_SoNoPlayerIsSkipped()
    {
        var controller = Make();
        var dealt = new List<string>();
        controller.CardReady += (_, e) => dealt.Add(e.PlayerName);

        controller.Start();                                   // Ana
        controller.RecordOutcome(CardOutcome.Completed);      // Ben
        controller.UndoLastTurn();                            // Ana again
        controller.RecordOutcome(CardOutcome.Completed);      // should be Ben's turn

        dealt[^1].Should().Be("Ben", "play resumes with the player who legitimately follows");
        controller.Dispose();
    }

    [Fact]
    public void UndoThenRedo_DoesNotDoubleCountScore()
    {
        var controller = Make();
        var totals = new Dictionary<string, int>();
        controller.TurnResult += (_, e) =>
        {
            foreach (var s in e.CurrentScores) totals[s.Name] = s.Score;
        };

        controller.Start();
        controller.RecordOutcome(CardOutcome.Completed);
        var afterFirst = totals.Values.Sum();

        controller.UndoLastTurn();
        controller.RecordOutcome(CardOutcome.Completed);

        totals.Values.Sum().Should().Be(afterFirst);
        controller.Dispose();
    }

    [Fact]
    public void RepeatedUndoCycles_AndAFullGame_StayStable()
    {
        var controller = Make(maxRounds: 100);
        var ended = false;
        controller.GameEnded += (_, _) => ended = true;
        controller.Start();

        for (var i = 0; i < 5; i++)
        {
            controller.RecordOutcome(CardOutcome.Completed);
            controller.UndoLastTurn();
        }
        for (var i = 0; i < 200 && !ended; i++)
            controller.RecordOutcome(CardOutcome.Completed);

        ended.Should().BeTrue("the game still reaches a clean end after repeated undos");
        controller.Dispose();
    }

    [Fact]
    public void UndoWithNothingToUndo_IsASafeNoOp()
    {
        var controller = Make();
        controller.Start();
        controller.UndoLastTurn().Should().BeFalse();
        controller.Dispose();
    }
}

/// <summary>
/// The two fill-in-the-blank judged party games — Blank Slate (all ages) and
/// Questionable Choices (adult). Both adapt the format to a single shared
/// screen: every prompt carries its own numbered shortlist of answers.
/// </summary>
public sealed class FillInTheBlankGamesTests
{
    private static readonly (string id, AgeRating rating, IReadOnlyList<TableTop.Core.Abstractions.Cards.ICard> bank)[] Decks =
    {
        ("fun.family.blankslate", AgeRating.AllAges, TableTop.Games.Family.BlankSlateCardBank.All),
        ("fun.questionable",      AgeRating.Adult,   TableTop.Games.Party.QuestionableChoicesCardBank.All),
    };

    [Fact]
    public void BothAreRegistered_AtTheRightAgeRating()
    {
        var registry = ArchetypeRegistry.Default();
        foreach (var (id, rating, _) in Decks)
        {
            var node = registry.FindById(id);
            node.Should().NotBeNull($"{id} should be registered");
            node!.AgeRating.Should().Be(rating);
        }
    }

    [Fact]
    public void EveryPrompt_HasABlank_AndAShortlistOfAnswers()
    {
        foreach (var (id, _, bank) in Decks)
        {
            bank.Count.Should().BeGreaterThanOrEqualTo(18, $"{id} should be a usable deck");
            foreach (var card in bank)
            {
                var text = CardText.StripHtml(card.Description);

                // The blank is the whole mechanic.
                text.Should().Contain("____", $"'{card.Title}' must contain a blank to fill");

                // Numbered shortlist — at least 6 options to choose between.
                var numbered = text.Split('\n').Count(l => l.TrimStart().Length > 1
                    && char.IsDigit(l.TrimStart()[0]) && l.Contains('.'));
                numbered.Should().BeGreaterThanOrEqualTo(6,
                    $"'{card.Title}' should offer a real shortlist to pick from");

                // Inventing your own answer is always allowed.
                text.ToLowerInvariant().Should().Contain("invent",
                    $"'{card.Title}' must tell players they can make up their own");
            }
        }
    }

    [Fact]
    public void JudgeAwardsASinglePoint_PerRound()
    {
        new TableTop.Games.Family.BlankSlateMode().CompleteLabel.Should().Contain("Funniest");
        new TableTop.Games.Party.QuestionableChoicesMode().CompleteLabel.Should().Contain("Winner");
    }

    [Fact]
    public void EachDeck_SpansSeveralCategories_AndDifficulties()
    {
        foreach (var (id, _, bank) in Decks)
        {
            bank.Select(c => c.Category).Distinct().Count()
                .Should().BeGreaterThanOrEqualTo(4, $"{id} should vary its themes");
            bank.Select(c => c.Difficulty).Distinct().Count()
                .Should().BeGreaterThanOrEqualTo(3, $"{id} should vary its difficulty");
        }
    }

    [Fact]
    public void AllAgesDeck_StaysCleanForMixedAges()
    {
        // Blank Slate sits in the family tree — keep it genuinely family-safe.
        var text = string.Join(" ",
            TableTop.Games.Family.BlankSlateCardBank.All.Select(c => c.Description)).ToLowerInvariant();
        foreach (var word in new[] { "sex", "drunk", "drug", "kill" })
            text.Should().NotContain(word, $"the all-ages deck should not mention '{word}'");
    }
}

/// <summary>
/// The Family Atlas — the family-facing sibling of The Cartographers.
/// Same accumulate-on-one-page mechanic, no <see cref="TableShape"/>
/// restriction (unlike the couples deck), registered under fun.family.
/// </summary>
public sealed class FamilyAtlasModeTests
{
    [Fact]
    public void FamilyAtlas_IsRegistered_UnderFunFamily_AsAllAges()
    {
        var node = ArchetypeRegistry.Default().FindById("fun.family.atlas");
        node.Should().NotBeNull();
        node!.Modes.Count(m => m.Name == "The Family Atlas").Should().Be(1);
        node.AgeRating.Should().Be(AgeRating.AllAges);
    }

    [Fact]
    public void FamilyAtlas_Bank_EveryCardAddsToTheMap()
    {
        // Same premise as The Cartographers: every card places something
        // permanent on one shared sheet, not just a spoken prompt.
        var cards = TableTop.Games.Family.FamilyAtlasCardBank.All;
        cards.Count.Should().BeGreaterThanOrEqualTo(30);
        cards.Should().OnlyContain(c => c.Description.Contains("Add to the map:"));
    }

    [Fact]
    public void FamilyAtlas_TheFiveStages_AreAllPresent()
    {
        var categories = TableTop.Games.Family.FamilyAtlasCardBank.All
            .Select(c => c.Category).Distinct().ToList();

        categories.Should().BeEquivalentTo(
            ["Foundations", "Wilds", "Home Turf", "Legend", "Beyond the Map"]);
    }

    [Fact]
    public void FamilyAtlas_PinsFoundationsFirstAndBeyondTheMapLast()
    {
        var mode = new TableTop.Games.Family.FamilyAtlasMode();

        mode.CategoriesPinnedToStart.Should().Equal("Foundations");
        mode.CategoriesPinnedToEnd.Should().Equal("Beyond the Map");
    }

    [Fact]
    public void FamilyAtlas_LaterStages_ReferBackToWhatEarlierStagesDrew()
    {
        var legend = TableTop.Games.Family.FamilyAtlasCardBank.All
            .Where(c => c.Category == "Legend")
            .ToList();

        legend.Should().Contain(c => c.Description.Contains("tallest mountain"),
            "naming the mountain requires the Wilds card that drew it");
        legend.Should().Contain(c => c.Description.Contains("Name the river"),
            "naming the river requires the Wilds card that drew it");
    }

    [Fact]
    public void FamilyAtlas_ScoresNothing_BecauseTheMapIsThePoint()
    {
        var manifest = new TableTop.Games.Family.FamilyAtlasMode().GetManifest();
        manifest.TotalCards.Should().Be(
            TableTop.Games.Family.FamilyAtlasCardBank.All.Count);
    }

    [Fact]
    public void FamilyAtlas_DeclaresNoTableShape_SoAnyFamilyCanPlay()
    {
        // Unlike CartographersMode (which addresses a pair sharing one sheet
        // of paper and so declares Couple), every card here speaks to
        // whoever's at the table — no headcount or relationship assumed.
        // Same permissive default the rest of this namespace's family modes
        // use (e.g. ThisIsUsMode).
        new TableTop.Games.Family.FamilyAtlasMode()
            .Should().NotBeAssignableTo<ITableShapeMode>();
    }

    [Fact]
    public void AllAgesDeck_StaysCleanForMixedAges()
    {
        // Sits in the family tree alongside Blank Slate — keep it genuinely
        // family-safe for a table that may include kids.
        var text = string.Join(" ",
            TableTop.Games.Family.FamilyAtlasCardBank.All.Select(c => c.Description)).ToLowerInvariant();
        foreach (var word in new[] { "sex", "drunk", "drug", "kill" })
            text.Should().NotContain(word, $"the all-ages deck should not mention '{word}'");
    }
}

/// <summary>
/// Regression tests for the couples-tag gap.
///
/// The bug: CoupleOnlyRestriction requires a "couple-member" TAG on at least
/// two players in the session. WPF supplied it (via PlayerProfile.ToPlayer),
/// but MAUI and WinUI only ever set gender/age ATTRIBUTES and no tags — so on
/// those two heads every couples-gated card was silently unplayable. In
/// "Between the Two of You" that was 13 of 18 cards permanently unreachable.
/// </summary>
public sealed class CoupleMemberTagTests
{
    private static IReadOnlyList<TableTop.Core.Abstractions.Players.IPlayer> Untagged() =>
        new List<TableTop.Core.Abstractions.Players.IPlayer>
        {
            TableTop.Core.Domain.Players.Player.Create("Ana",
                new Dictionary<string, string> { ["gender"] = "female", ["age"] = "29" }),
            TableTop.Core.Domain.Players.Player.Create("Ben",
                new Dictionary<string, string> { ["gender"] = "male", ["age"] = "31" }),
        }.AsReadOnly();

    private static IReadOnlyList<TableTop.Core.Abstractions.Players.IPlayer> Tagged() =>
        new List<TableTop.Core.Abstractions.Players.IPlayer>
        {
            TableTop.Core.Domain.Players.Player.Create("Ana",
                new Dictionary<string, string> { ["gender"] = "female" },
                new[] { "couple-member", "adult" }),
            TableTop.Core.Domain.Players.Player.Create("Ben",
                new Dictionary<string, string> { ["gender"] = "male" },
                new[] { "couple-member", "adult" }),
        }.AsReadOnly();

    [Fact]
    public void CoupleOnlyRestriction_NeedsTheTag_NotJustAttributes()
    {
        var restriction = new TableTop.Core.Domain.Restrictions.CoupleOnlyRestriction();
        var untagged = Untagged();
        var tagged = Tagged();

        restriction.IsSatisfiedBy(untagged[0], untagged)
            .Should().BeFalse("gender/age attributes alone must not satisfy it");
        restriction.IsSatisfiedBy(tagged[0], tagged)
            .Should().BeTrue("the couple-member tag is what unlocks couples cards");
    }

    [Fact]
    public void TaggedCouple_CanReachEveryCard_InACouplesGatedDeck()
    {
        var deck = TableTop.Games.Couples.BetweenTheTwoOfYouCardBank.All;
        var tagged = Tagged();

        var gated = deck.Count(c => c.Restriction is not null);
        gated.Should().BeGreaterThan(0, "this deck is the regression case");

        var playable = deck.Count(c =>
            c.Restriction is null || c.Restriction.IsSatisfiedBy(tagged[0], tagged));

        playable.Should().Be(deck.Count, "a tagged couple should reach the whole deck");
    }

    [Fact]
    public void UntaggedPlayers_AreLockedOut_OfTheGatedPortion()
    {
        // Pins the shape of the bug so a future refactor can't silently reintroduce it.
        var deck = TableTop.Games.Couples.BetweenTheTwoOfYouCardBank.All;
        var untagged = Untagged();

        var playable = deck.Count(c =>
            c.Restriction is null || c.Restriction.IsSatisfiedBy(untagged[0], untagged));

        playable.Should().BeLessThan(deck.Count,
            "untagged players genuinely cannot reach couples-gated cards");
    }
}


/// <summary>
/// Pins the shape of the archetype tree that the pickers depend on.
///
/// The MAUI picker is three levels deep (type → variant → game) but the tree is
/// deeper in places: "Classroom → Grade 6" is a pure branch with no direct
/// modes. A picker that read only <c>node.Modes</c> showed an empty game list
/// there, which looked to players like selection was broken. The UI now
/// collects modes from the whole subtree; these tests document why that's
/// required, so nobody "simplifies" it back.
/// </summary>
public sealed class ArchetypeTreeDepthTests
{
    private static IEnumerable<Archetype> Descend(Archetype node)
    {
        yield return node;
        foreach (var child in node.SubArchetypes)
            foreach (var n in Descend(child))
                yield return n;
    }

    private static int ModesInSubtree(Archetype node) =>
        Descend(node).Sum(n => n.Modes.Count);

    [Fact]
    public void TheTree_IsDeeperThanTwoLevels_SoPickersMustRecurse()
    {
        var roots = ArchetypeRegistry.Default().RootArchetypes;

        var branchWithNoDirectModes = roots
            .SelectMany(r => r.SubArchetypes)
            .Any(sub => sub.Modes.Count == 0 && sub.SubArchetypes.Count > 0);

        branchWithNoDirectModes.Should().BeTrue(
            "at least one sub-archetype is a pure branch — a picker reading only " +
            "direct .Modes would show an empty list for it");
    }

    [Fact]
    public void EverySelectionPath_LeadsToAtLeastOneGame()
    {
        var roots = ArchetypeRegistry.Default().RootArchetypes;
        roots.Should().NotBeEmpty();

        foreach (var root in roots)
        {
            ModesInSubtree(root).Should().BeGreaterThan(0,
                $"'{root.Name}' must lead to at least one playable game");

            foreach (var sub in root.SubArchetypes)
                ModesInSubtree(sub).Should().BeGreaterThan(0,
                    $"'{root.Name} → {sub.Name}' must lead to at least one playable game");
        }
    }
}

// EmbeddedDeckTests lived here. It proved the shipped decks were embedded
// resources rather than <Content> copied beside the assembly — a distinction
// that cost real debugging, because <Content> silently resolves under MAUI
// fast-deployment and silently doesn't in a packaged APK. Both the decks and
// the resolver they exercised are gone (1.19.0); modes read their C# banks,
// which are compiled in and cannot go missing.
//
// The lesson outlives the tests and is recorded in TableTop.Games.csproj: any
// file-based content added to that assembly later must be <EmbeddedResource>.

// BindableSurfaceTests moved to PlayerSetupViewModelTests.cs and fixed: it
// used to look up two per-head type names
// (TableTop.Maui.ViewModels.PlayerSetupViewModel,
// TableTop.WinUI.ViewModels.PlayerSetupViewModel) that no longer exist since
// those classes were merged into TableTop.Presentation.ViewModels
// .PlayerSetupViewModel — its own "if (type is null) continue" guard meant it
// silently found neither and never ran its real assertion again. The full
// history (a static GenderOptions once shipped invisible to XAML binding,
// caught by nothing since every test set SelectedGender directly and bypassed
// the picker) now lives on the replacement test's own doc comment.
// used to look up two per-head type names
// (TableTop.Maui.ViewModels.PlayerSetupViewModel,
// TableTop.WinUI.ViewModels.PlayerSetupViewModel) that no longer exist since
// those classes were merged into TableTop.Presentation.ViewModels
// .PlayerSetupViewModel — its own "if (type is null) continue" guard meant it
// silently found neither and never ran its real assertion again.

/// <summary>
/// Pinned categories must survive BOTH the shuffle and the progression layer.
///
/// Deck order alone isn't enough. Progression strategies choose candidates by
/// peeking across the whole deck, so an Easy card sitting at the end — which
/// every results card is — could still be picked early. Ordering the deck fixed
/// where cards sat; deferring them in candidate selection fixes when they're
/// reachable. Both are needed, so this exercises the real controller.
/// </summary>
public sealed class PinnedCategoryTests
{
    private static IReadOnlyList<TableTop.Core.Abstractions.Players.IPlayer> Couple() =>
        new List<TableTop.Core.Abstractions.Players.IPlayer>
        {
            TableTop.Core.Domain.Players.Player.Create("Ana",
                new Dictionary<string, string> { ["gender"] = "female" },
                new[] { "couple-member", "adult" }),
            TableTop.Core.Domain.Players.Player.Create("Ben",
                new Dictionary<string, string> { ["gender"] = "male" },
                new[] { "couple-member", "adult" }),
        }.AsReadOnly();

    private static List<string> DealtCategories(
        TableTop.Core.Abstractions.Game.IGameMode mode)
    {
        var controller = (ICardTurnController)new ControllerFactory()
            .CreateAsync(mode, Couple(), maxRounds: 400,
                gameplayOptions: new GameplayOptions { ShuffleDeck = true })
            .GetAwaiter().GetResult();

        var order = new List<string>();
        controller.CardReady += (_, e) => order.Add(e.Category ?? "");
        var ended = false;
        controller.GameEnded += (_, _) => ended = true;
        controller.Start();
        for (var i = 0; i < 800 && !ended; i++)
            controller.RecordOutcome(CardOutcome.Completed);
        controller.Dispose();
        return order;
    }


    [Fact]
    public void ConsentDecks_OpenOnConsent_AndCloseOnAftercare()
    {
        foreach (var mode in new TableTop.Core.Abstractions.Game.IGameMode[]
        {
            new TableTop.Games.Couples.AfterglowMode(),
            new TableTop.Games.Couples.UndividedMode(),
        })
        {
            for (var run = 0; run < 5; run++)
            {
                var order = DealtCategories(mode);

                var consentCount = order.Count(c => c == "Consent");
                consentCount.Should().BeGreaterThan(0);
                order.Take(consentCount).Should().OnlyContain(c => c == "Consent",
                    $"'{mode.Name}' must agree a safeword before anything it governs");
                order[^1].Should().Be("Aftercare",
                    $"'{mode.Name}' must close on aftercare");
            }
        }
    }
}

/// <summary>
/// The per-card minimum-age mechanism.
///
/// No shipping deck uses this, deliberately: "more explicit" is a maturity
/// question and is already handled by the archetype AgeRating floor, not a
/// per-card age gate. The honest use is content where age is genuinely the
/// criterion — anything involving alcohol, for instance.
///
/// The behaviour that matters before anyone authors such a card is that it
/// FAILS CLOSED: player age is optional in every UI, so a card gated this way
/// is invisible to anyone who skipped the field. These tests pin that, and —
/// more importantly — prove a deck containing gated cards still plays to a
/// clean finish for a table that gave no ages at all.
/// </summary>
public sealed class MinimumAgeRestrictionTests
{
    private sealed class AgeGatedMode : TableTop.Games.Base.BaseGameModeDefinition
    {
        public override string Name => "AgeGatedProbe";
        public override string Description => "probe deck with a 21+ tier";

        protected override IScoringStrategy BuildScoring() => new FixedScoringStrategy(1);

        protected override IReadOnlyList<TableTop.Core.Abstractions.Cards.ICard> BuildCards(
            IReadOnlyList<TableTop.Core.Abstractions.Players.IPlayer> players)
        {
            var over21 = new TableTop.Core.Domain.Restrictions.MinimumAgeRestriction(21);
            var cards = new List<TableTop.Core.Abstractions.Cards.ICard>();
            for (var i = 0; i < 8; i++)
                cards.Add(StandardCard.Create($"Open {i}", "anyone may play this", Difficulty.Easy, "Open"));
            for (var i = 0; i < 4; i++)
                cards.Add(StandardCard.Create($"Gated {i}", "21 and over", Difficulty.Easy, "Gated",
                    restriction: over21));
            return cards;
        }
    }

    private static TableTop.Core.Abstractions.Players.IPlayer Aged(string name, int? age)
    {
        var attrs = new Dictionary<string, string>();
        if (age is not null) attrs["age"] = age.Value.ToString();
        return TableTop.Core.Domain.Players.Player.Create(name, attrs);
    }

    [Fact]
    public void TheGate_AllowsOldEnough_AndDeniesYoungerOrUnknown()
    {
        var restriction = new TableTop.Core.Domain.Restrictions.MinimumAgeRestriction(21);
        var table = new List<TableTop.Core.Abstractions.Players.IPlayer>().AsReadOnly();

        restriction.IsSatisfiedBy(Aged("Old", 21), table).Should().BeTrue("21 meets a 21+ gate");
        restriction.IsSatisfiedBy(Aged("Older", 40), table).Should().BeTrue();
        restriction.IsSatisfiedBy(Aged("Young", 20), table).Should().BeFalse();

        restriction.IsSatisfiedBy(Aged("Unstated", null), table).Should().BeFalse(
            "age is optional in the UIs, so an unstated age must NOT pass an age gate — " +
            "which is exactly why gating existing content would hide it from most players");
    }

    [Fact]
    public async Task ADeckWithGatedCards_StillFinishesCleanly_WhenNobodyGaveAnAge()
    {
        // The failure mode worth guarding: gated cards nobody can play must not
        // stall the game. This is the scenario that used to recurse forever
        // before AdvanceTurn became a bounded loop.
        var players = new List<TableTop.Core.Abstractions.Players.IPlayer>
            { Aged("Ana", null), Aged("Ben", null) }.AsReadOnly();

        var controller = (ICardTurnController)(await new ControllerFactory()
            .CreateAsync(new AgeGatedMode(), players, maxRounds: 200));

        var dealt = new List<string>();
        var ended = false;
        controller.CardReady += (_, e) => dealt.Add(e.Category ?? "");
        controller.GameEnded += (_, _) => ended = true;
        controller.Start();
        for (var i = 0; i < 400 && !ended; i++)
            controller.RecordOutcome(CardOutcome.Completed);

        ended.Should().BeTrue("an unplayable tier must not hang the game");
        dealt.Should().NotContain("Gated", "no ageless player may be dealt a 21+ card");
        dealt.Should().Contain("Open", "the rest of the deck must still play");
        controller.Dispose();
    }

    [Fact]
    public async Task GatedCards_AreDealt_WhenThePlayersAreOldEnough()
    {
        var players = new List<TableTop.Core.Abstractions.Players.IPlayer>
            { Aged("Ana", 30), Aged("Ben", 34) }.AsReadOnly();

        var controller = (ICardTurnController)(await new ControllerFactory()
            .CreateAsync(new AgeGatedMode(), players, maxRounds: 200));

        var dealt = new List<string>();
        var ended = false;
        controller.CardReady += (_, e) => dealt.Add(e.Category ?? "");
        controller.GameEnded += (_, _) => ended = true;
        controller.Start();
        for (var i = 0; i < 400 && !ended; i++)
            controller.RecordOutcome(CardOutcome.Completed);

        dealt.Should().Contain("Gated", "players over the gate must reach the gated tier");
        controller.Dispose();
    }
}

/// <summary>
/// Last Orders — a drinking-adjacent deck, so its safety properties are pinned
/// rather than left to good intentions. Drinking games do harm through volume,
/// speed, and pressure on people who aren't drinking; this deck removes all
/// three levers, and these tests keep them removed.
/// </summary>
public sealed class LastOrdersTests
{
    private static IReadOnlyList<TableTop.Core.Abstractions.Cards.ICard> Deck =>
        TableTop.Games.Party.LastOrdersCardBank.All;

    private static readonly string[] DrinkCategories = { "Forfeits" };

    [Fact]
    public void IsRegistered_AsAdult()
    {
        var node = ArchetypeRegistry.Default().FindById("fun.lastorders");
        node.Should().NotBeNull();
        node!.AgeRating.Should().Be(AgeRating.Adult);
    }

    [Fact]
    public void NoCard_InstructsVolumeOrSpeed()
    {
        // Volume and speed are what make drinking games dangerous, so no card
        // may instruct either.
        //
        // The check allows these words when they are NEGATED, because the
        // safety copy necessarily names what it forbids — "sips not shots",
        // "never a shot", "nothing here asks you to race". Banning the strings
        // outright would fail the deck for saying the right thing.
        var banned = new[]
        {
            "down it", "down your", "chug", "shot", "necks", "neck it",
            "finish your drink", "finish the glass", "race", "first to finish",
            "keep up", "bottoms up", "skull",
        };
        var negators = new[] { "not", "never", "nothing", "no ", "n't", "without" };

        foreach (var card in Deck)
        {
            var text = CardText.StripHtml(card.Description).ToLowerInvariant();
            foreach (var phrase in banned)
            {
                var at = text.IndexOf(phrase, StringComparison.Ordinal);
                while (at >= 0)
                {
                    var lookBehind = text[Math.Max(0, at - 60)..at];
                    negators.Any(lookBehind.Contains).Should().BeTrue(
                        $"'{card.Title}' uses '{phrase}' as an instruction rather than a prohibition");
                    at = text.IndexOf(phrase, at + phrase.Length, StringComparison.Ordinal);
                }
            }
        }
    }

    [Fact]
    public void EveryDrinkCard_OffersAnEqualSoftOption_AndIsAgeGated()
    {
        var drinkCards = Deck.Where(c => DrinkCategories.Contains(c.Category)).ToList();
        drinkCards.Should().NotBeEmpty();

        foreach (var card in drinkCards)
        {
            var text = CardText.StripHtml(card.Description).ToLowerInvariant();
            text.Should().Contain("soft",
                $"'{card.Title}' must offer a soft option in the same breath");
            text.Should().Contain("same",
                $"'{card.Title}' must say the soft option counts the same");

            card.Restriction.Should().NotBeNull(
                $"'{card.Title}' involves alcohol and must carry the age gate");
        }
    }

    [Fact]
    public void SoftOption_ScoresIdentically()
    {
        // Flat scoring is the mechanism: no card can be worth more for drinking.
        var mode = new TableTop.Games.Party.LastOrdersMode();
        var player = TableTop.Core.Domain.Players.Player.Create("P");
        var scores = Deck.Select(c =>
            new FixedScoringStrategy(1).CalculateScore(c, player, CardOutcome.Completed))
            .Distinct().ToList();

        scores.Count.Should().Be(1, "every card must be worth the same, drinking or not");
    }

    [Fact]
    public void HouseRulesOpen_AndLastRoundCloses()
    {
        Deck[0].Category.Should().Be("House Rules");
        Deck[^1].Category.Should().Be("Last Round");

        var rules = string.Join(" ", Deck.Where(c => c.Category == "House Rules")
            .Select(c => c.Description)).ToLowerInvariant();
        rules.Should().Contain("driving");
        rules.Should().Contain("water");
        rules.Should().Contain("pass is always free");
        rules.Should().Contain("legal drinking age");

        var last = string.Join(" ", Deck.Where(c => c.Category == "Last Round")
            .Select(c => c.Description)).ToLowerInvariant();
        last.Should().Contain("water");
        last.Should().Contain("home");
    }

    [Fact]
    public async Task AgelessTable_NeverSeesADrinkCard_ButStillPlaysTheDeck()
    {
        // The gate fails closed, which is the point: no stated age, no alcohol.
        var players = new List<TableTop.Core.Abstractions.Players.IPlayer>
        {
            TableTop.Core.Domain.Players.Player.Create("Ana"),
            TableTop.Core.Domain.Players.Player.Create("Ben"),
        }.AsReadOnly();

        var controller = (ICardTurnController)(await new ControllerFactory()
            .CreateAsync(new TableTop.Games.Party.LastOrdersMode(), players, maxRounds: 300));

        var dealt = new List<string>();
        var ended = false;
        controller.CardReady += (_, e) => dealt.Add(e.Category ?? "");
        controller.GameEnded += (_, _) => ended = true;
        controller.Start();
        for (var i = 0; i < 600 && !ended; i++)
            controller.RecordOutcome(CardOutcome.Completed);

        ended.Should().BeTrue("the deck must still finish cleanly");
        dealt.Should().NotContain("Forfeits", "an ageless table must never be dealt an alcohol card");
        dealt.Should().Contain("Warm Up", "the social dares must still play");
        controller.Dispose();
    }

    [Fact]
    public async Task TableOverTheAge_DoesGetTheDrinkCards()
    {
        var players = new List<TableTop.Core.Abstractions.Players.IPlayer>
        {
            TableTop.Core.Domain.Players.Player.Create("Ana",
                new Dictionary<string, string> { ["age"] = "30" }),
            TableTop.Core.Domain.Players.Player.Create("Ben",
                new Dictionary<string, string> { ["age"] = "27" }),
        }.AsReadOnly();

        var controller = (ICardTurnController)(await new ControllerFactory()
            .CreateAsync(new TableTop.Games.Party.LastOrdersMode(), players, maxRounds: 300));

        var dealt = new List<string>();
        var ended = false;
        controller.CardReady += (_, e) => dealt.Add(e.Category ?? "");
        controller.GameEnded += (_, _) => ended = true;
        controller.Start();
        for (var i = 0; i < 600 && !ended; i++)
            controller.RecordOutcome(CardOutcome.Completed);

        dealt.Should().Contain("Forfeits");
        controller.Dispose();
    }
}

