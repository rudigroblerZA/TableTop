using TableTop.Core.Domain.Progression;
using TableTop.Games.Couples;
using TableTop.Hosting.Controllers;
using TableTop.Hosting.Events;

namespace TableTop.Tests;

public sealed class MonogamyTests
{
    private static Player Male(string name = "Adam") => Player.Create(name, attributes: new Dictionary<string, string> { ["gender"] = "male" });
    private static Player Female(string name = "Eve") => Player.Create(name, attributes: new Dictionary<string, string> { ["gender"] = "female" });

    private static MonogamyController BuildController(
        IReadOnlyList<MonogamyCard>? cards = null,
        IReadOnlyList<Player>? players = null,
        int winningTokens = 10,
        Random? rng = null)
    {
        var p = players ?? [Male(), Female()];
        var c = cards ?? MonogamyCardBank.All.ToList();
        return new MonogamyController(
            p.Cast<Core.Abstractions.Players.IPlayer>().ToList().AsReadOnly(),
            c, winningTokens, rng);
    }

    // ── MonogamyCard ──────────────────────────────────────────────────────────

    [Fact]
    public void MonogamyCard_ImplementsIMonogamyCard()
    {
        var card = MonogamyCard.CreateNeutral("Test", "Do this.", MonogamyZone.Foreplay, CardTarget.ForBoth);
        card.Should().BeAssignableTo<IMonogamyCard>();
    }

    [Fact]
    public void MonogamyCard_ImplementsIPromptCard()
    {
        var card = MonogamyCard.Create("Test", "Him text.", "Her text.", "Neutral.",
            MonogamyZone.Sensual, CardTarget.ForDrawer);
        card.Should().BeAssignableTo<IPromptCard>();
    }

    [Fact]
    public void MonogamyCard_ZoneMapsToCorrectDifficulty()
    {
        MonogamyCard.CreateNeutral("F", "d", MonogamyZone.Foreplay, CardTarget.ForBoth).Difficulty.Should().Be(Difficulty.Easy);
        MonogamyCard.CreateNeutral("S", "d", MonogamyZone.Sensual, CardTarget.ForBoth).Difficulty.Should().Be(Difficulty.Medium);
        MonogamyCard.CreateNeutral("St", "d", MonogamyZone.Steamy, CardTarget.ForBoth).Difficulty.Should().Be(Difficulty.Hard);
        MonogamyCard.CreateNeutral("W", "d", MonogamyZone.Wild, CardTarget.ForBoth).Difficulty.Should().Be(Difficulty.Extreme);
    }

    [Fact]
    public void MonogamyCard_CategoryMatchesZone()
    {
        var card = MonogamyCard.CreateNeutral("T", "d", MonogamyZone.Steamy, CardTarget.ForDrawer);
        card.Category.Should().Be("Steamy");
    }

    [Fact]
    public void MonogamyCard_ResolvePrompt_ForMale_ReturnsHimText()
    {
        var card = MonogamyCard.Create("Test", "For him.", "For her.", "Neutral.",
            MonogamyZone.Sensual, CardTarget.ForDrawer);
        var male = Male();
        ((IPromptCard)card).ResolvePrompt(male).Should().Be("For him.");
    }

    [Fact]
    public void MonogamyCard_ResolvePrompt_ForFemale_ReturnsHerText()
    {
        var card = MonogamyCard.Create("Test", "For him.", "For her.", "Neutral.",
            MonogamyZone.Sensual, CardTarget.ForDrawer);
        var female = Female();
        ((IPromptCard)card).ResolvePrompt(female).Should().Be("For her.");
    }

    [Fact]
    public void MonogamyCard_TokenValue_DefaultsToOne()
    {
        MonogamyCard.CreateNeutral("T", "d", MonogamyZone.Foreplay, CardTarget.ForBoth)
            .TokenValue.Should().Be(1);
    }

    // ── DiceRoll ──────────────────────────────────────────────────────────────

    // Every boundary of the 2-12 range, both sides. The ranges were re-cut when
    // the Fantasy zone was added — five zones share what four used to — and two
    // of these cases silently went wrong then: 7 moved Sensual -> Steamy, and 12
    // moved Wild -> Fantasy. Boundaries rather than midpoints, because a re-cut
    // shifts edges first.
    [Theory]
    [InlineData(1, 1, MonogamyZone.Foreplay)]   //  2  lower bound
    [InlineData(2, 2, MonogamyZone.Foreplay)]   //  4  Foreplay's top
    [InlineData(2, 3, MonogamyZone.Sensual)]    //  5  Sensual's floor
    [InlineData(3, 3, MonogamyZone.Sensual)]    //  6  Sensual's top
    [InlineData(3, 4, MonogamyZone.Steamy)]     //  7  Steamy's floor
    [InlineData(4, 4, MonogamyZone.Steamy)]     //  8  Steamy's top
    [InlineData(4, 5, MonogamyZone.Wild)]       //  9  Wild's floor
    [InlineData(5, 5, MonogamyZone.Wild)]       // 10  Wild's top
    [InlineData(5, 6, MonogamyZone.Fantasy)]    // 11  Fantasy's floor
    [InlineData(6, 6, MonogamyZone.Fantasy)]    // 12  upper bound
    public void DiceRoll_MapsToCorrectZone(int d1, int d2, MonogamyZone expected)
    {
        new DiceRoll(d1, d2).ToZone().Should().Be(expected);
    }

    [Fact]
    public void DiceRoll_EveryTotalMapsToADeclaredZone()
    {
        // Guards the re-cut itself: no total in 2-12 may fall through to a
        // default, and every zone must be reachable by some roll. A zone no
        // roll can produce is content nobody sees.
        var reached = Enumerable.Range(1, 6)
            .SelectMany(a => Enumerable.Range(1, 6).Select(b => new DiceRoll(a, b).ToZone()))
            .Distinct()
            .ToList();

        reached.Should().HaveCount(Enum.GetValues<MonogamyZone>().Length,
            "every declared zone must be reachable by some roll");
    }

    [Fact]
    public void DiceRoll_IsDouble_TrueWhenBothDiceMatch()
    {
        new DiceRoll(3, 3).IsDouble.Should().BeTrue();
        new DiceRoll(3, 4).IsDouble.Should().BeFalse();
    }

    [Fact]
    public void DiceRoll_Total_SumsBothDice()
    {
        new DiceRoll(4, 5).Total.Should().Be(9);
    }

    // ── MonogamyController — basic flow ───────────────────────────────────────

    [Fact]
    public void Controller_RequiresAtLeastTwoPlayers()
    {
        var act = () => new MonogamyController(
            [Male()], MonogamyCardBank.All.ToList(), 10);
        Assert.Throws<ArgumentException>(() => act());
    }

    [Fact]
    public void Controller_Start_EmitsDiceRolledEvent()
    {
        var ctrl = BuildController();
        DiceRolledEvent? evt = null;
        ctrl.DiceRolled += (_, e) => evt = e;

        ctrl.Start();

        evt.Should().NotBeNull();
        evt.Die1.Should().BeInRange(1, 6);
        evt.Die2.Should().BeInRange(1, 6);
    }

    [Fact]
    public void Controller_CardReady_EmittedAfterNonDoubles()
    {
        // Force a non-doubles roll with fixed seed
        var rng = FindNonDoublesRng();
        var ctrl = BuildController(rng: rng);

        MonogamyCardReadyEvent? evt = null;
        ctrl.CardReady += (_, e) => evt = e;
        ctrl.Start();

        evt.Should().NotBeNull();
        evt.CardTitle.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Controller_CompleteCard_AwardsTokens()
    {
        var rng = FindNonDoublesRng();
        var ctrl = BuildController(rng: rng);

        TokensAwardedEvent? evt = null;
        ctrl.TokensAwarded += (_, e) => evt = e;
        ctrl.Start();
        ctrl.CompleteCard();

        evt.Should().NotBeNull();
        evt.TokensEarned.Should().BeGreaterThan(0);
        evt.TotalTokens.Should().Be(evt.TokensEarned);
    }

    [Fact]
    public void Controller_SkipCard_AwardsNoTokens()
    {
        var rng = FindNonDoublesRng();
        var ctrl = BuildController(rng: rng);

        TokensAwardedEvent? awarded = null;
        ctrl.TokensAwarded += (_, e) => awarded = e;
        ctrl.Start();
        ctrl.SkipCard();

        awarded.Should().BeNull(); // no tokens for a skip
    }

    [Fact]
    public void Controller_NegotiateCard_AwardsHalfTokens()
    {
        // Use a Wild card (4 tokens) to verify halving
        var card = MonogamyCard.CreateNeutral("Wild", "d", MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4);
        var rng = FindNonDoublesRng();
        var ctrl = BuildController(cards: [card], rng: rng);

        TokensAwardedEvent? evt = null;
        ctrl.TokensAwarded += (_, e) => evt = e;
        ctrl.Start();
        ctrl.NegotiateCard();

        evt.Should().NotBeNull();
        evt.TokensEarned.Should().Be(2); // 4 / 2
    }

    [Fact]
    public void Controller_WinCondition_EndsGameWhenTargetReached()
    {
        // Single-token card, win at 1 token
        var card = MonogamyCard.CreateNeutral("T", "d", MonogamyZone.Foreplay, CardTarget.ForBoth, tokenValue: 1);
        var rng = FindNonDoublesRng();
        var ctrl = BuildController(cards: [card], winningTokens: 1, rng: rng);

        MonogamyGameEndedEvent? ended = null;
        ctrl.GameEnded += (_, e) => ended = e;
        ctrl.Start();
        ctrl.CompleteCard();

        ended.Should().NotBeNull();
        ended.WinnerName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Controller_Doubles_EmitsDoublesRolledEvent_ThenAwaitsZoneChoice()
    {
        // Use a fixed-doubles RNG: die1 and die2 always equal
        // Brute-force find a seed where BOTH the first shuffle AND first roll produce doubles
        Random? doublesRng = null;
        for (var seed = 0; seed < 100_000; seed++)
        {
            var testRng = new Random(seed);
            // Simulate shuffle consuming rng values (for a deck of ~41 cards: 41 swaps)
            var deckSize = MonogamyCardBank.All.Count;
            for (var i = deckSize - 1; i > 0; i--) testRng.Next(i + 1);
            // Now check if the first roll produces doubles
            var roll = DiceRoll.Roll(testRng);
            if (roll.IsDouble) { doublesRng = new Random(seed); break; }
        }
        Xunit.Assert.NotNull(doublesRng);

        var ctrl = BuildController(rng: doublesRng);

        DoublesRolledEvent? doublesEvt = null;
        MonogamyCardReadyEvent? cardEvt = null;
        ctrl.DoublesRolled += (_, e) => doublesEvt = e;
        ctrl.CardReady += (_, e) => cardEvt = e;
        ctrl.Start();

        doublesEvt.Should().NotBeNull();
        cardEvt.Should().BeNull(); // card not shown until zone is chosen

        ctrl.ChooseZone(MonogamyZone.Wild);

        cardEvt.Should().NotBeNull();
        cardEvt.Zone.Should().Be("Wild");
    }

    [Fact]
    public void Controller_TokensByZone_TracksPerZone()
    {
        var foreplayCard = MonogamyCard.CreateNeutral("F", "d", MonogamyZone.Foreplay, CardTarget.ForBoth, tokenValue: 2);
        var rng = FindNonDoublesRng();
        var ctrl = BuildController(cards: [foreplayCard, foreplayCard], rng: rng);

        ctrl.Start();
        ctrl.CompleteCard();

        var firstPlayerId = ctrl.Tokens.Keys.First();
        ctrl.TokensByZone[firstPlayerId].Keys.Should().Contain(k => k == "Foreplay");
    }

    // ── Card bank ─────────────────────────────────────────────────────────────

    [Fact]
    public void MonogamyCardBank_HasCardsInAllZones()
    {
        var cards = MonogamyCardBank.All;
        cards.Should().Contain(c => c.Zone == MonogamyZone.Foreplay);
        cards.Should().Contain(c => c.Zone == MonogamyZone.Sensual);
        cards.Should().Contain(c => c.Zone == MonogamyZone.Steamy);
        cards.Should().Contain(c => c.Zone == MonogamyZone.Wild);
    }

    [Fact]
    public void MonogamyCardBank_AllCardsHaveAdultRestriction()
    {
        MonogamyCardBank.All.Should()
            .OnlyContain(c => c.Restriction != null,
                "all Monogamy cards should be restricted to adults");
    }

    [Fact]
    public void MonogamyCardBank_GenderDirectedCards_ResolveDifferentlyForMaleAndFemale()
    {
        var genderedCards = MonogamyCardBank.All
            .Where(c => c is IPromptCard)
            .Take(5)
            .ToList();

        genderedCards.Should().NotBeEmpty();

        foreach (var card in genderedCards.OfType<IPromptCard>())
        {
            var maleText = card.ResolvePrompt(Male());
            var femaleText = card.ResolvePrompt(Female());
            // At least some cards should differ — not all are neutral
            maleText.Should().NotBeNullOrEmpty();
            femaleText.Should().NotBeNullOrEmpty();
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Finds a Random seed that produces a non-doubles roll immediately.</summary>
    private static Random FindNonDoublesRng()
    {
        for (var seed = 0; seed < 10000; seed++)
        {
            var roll = DiceRoll.Roll(new Random(seed));
            if (!roll.IsDouble) return new Random(seed);
        }
        throw new InvalidOperationException("Could not find non-doubles seed");
    }

}