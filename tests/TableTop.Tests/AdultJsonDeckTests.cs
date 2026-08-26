using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Games.Couples;
using TableTop.Games.Party;

namespace TableTop.Tests;

/// <summary>
/// Consent-structure guarantees for the adult decks, run against the content
/// that ACTUALLY SHIPS.
///
/// WHY THIS FILE EXISTS SEPARATELY FROM AfterglowTests / UndividedTests
/// ────────────────────────────────────────────────────────────────────
/// Those tests read <c>AfterglowCardBank.All</c> and <c>UndividedCardBank.All</c>
/// directly. These go through <see cref="IGameModeDefinition.GetCards"/>, which
/// is the path the engine actually deals from.
///
/// <para>
/// That distinction was originally load-bearing: every one of these modes loaded
/// its deck from a <c>.deck.json</c> file, so the bank was reached only if the
/// JSON failed, and the consent invariants — open on the ritual, close on
/// aftercare, opt-in language on every explicit card — were being enforced only
/// on the copy nobody played. With the JSON deck path removed (1.19.0) the two
/// sources have collapsed into one and these assertions now duplicate their
/// siblings.
/// </para>
///
/// <para>
/// Kept anyway, and deliberately: <c>GetCards</c> is still the correct thing to
/// assert consent structure against, because it is still where filtering,
/// ordering and pinning happen. If a future change puts a second source behind
/// it again, this file is what notices.
/// </para>
/// </summary>
public sealed class AdultJsonDeckTests
{
    private static readonly IReadOnlyList<IPlayer> Two =
    [
        TableTop.Core.Domain.Players.Player.Create("A"),
        TableTop.Core.Domain.Players.Player.Create("B"),
    ];

    private static IReadOnlyList<ICard> Shipped(IGameModeDefinition mode) =>
        mode.GetCards(Two);

    // ── Afterglow ─────────────────────────────────────────────────────────────

    [Fact]
    public void Afterglow_Json_OpensWithTheConsentRitual()
    {
        var deck = Shipped(new AfterglowMode());

        deck[0].Category.Should().Be("Consent");
        deck[1].Category.Should().Be("Consent");
        deck[2].Category.Should().Be("Consent");
        deck[0].Description.ToLowerInvariant().Should().Contain("safeword");
    }

    [Fact]
    public void Afterglow_Json_ConsentRitual_CoversSafeword_Boundaries_AndCheckIn()
    {
        var consent = string.Join(" ", Shipped(new AfterglowMode())
            .Where(c => c.Category == "Consent").Select(c => c.Description))
            .ToLowerInvariant();

        consent.Should().Contain("safeword");
        consent.Should().Contain("off the table");
        consent.Should().Contain("colour");
        consent.Should().Contain("enthusiasm is the only yes");
    }

    [Fact]
    public void Afterglow_Json_ClosesOnAftercare()
    {
        var deck = Shipped(new AfterglowMode());

        deck[^1].Category.Should().Be("Aftercare");
        deck.Count(c => c.Category == "Aftercare").Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void Afterglow_Json_EveryPlayCard_CarriesTheOptInAndFreePassLanguage()
    {
        string[] play = ["Warm Up", "Turn Up", "Heat", "Undone"];
        var cards = Shipped(new AfterglowMode()).Where(c => play.Contains(c.Category)).ToList();

        cards.Should().HaveCountGreaterThanOrEqualTo(10);
        foreach (var c in cards)
        {
            var text = c.Description.ToLowerInvariant();
            text.Should().Contain("invitation",
                $"'{c.Title}' must frame itself as an invitation");
            text.Should().Contain("pass is always free",
                $"'{c.Title}' must state passing is free");
            text.Should().Contain("enthusiasm is the only yes",
                $"'{c.Title}' must state the enthusiasm rule");
        }
    }

    // ── Undivided ─────────────────────────────────────────────────────────────

    [Fact]
    public void Undivided_Json_OpensOnConsent_AndClosesOnAftercare()
    {
        var deck = Shipped(new UndividedMode());

        deck[0].Category.Should().Be("Consent");
        deck[1].Category.Should().Be("Consent");
        deck[2].Category.Should().Be("Consent");
        deck[^1].Category.Should().Be("Aftercare");
    }

    [Fact]
    public void Undivided_Json_EveryPlayCard_CarriesTheReceiverSteersLanguage()
    {
        // Swap and Aftercare are the handover and landing rituals, not prompts
        // to act on, and by design carry no footer.
        string[] play = ["Attention", "Devotion", "Worship"];
        var cards = Shipped(new UndividedMode()).Where(c => play.Contains(c.Category)).ToList();

        cards.Should().HaveCountGreaterThanOrEqualTo(10);
        foreach (var c in cards)
        {
            var text = c.Description.ToLowerInvariant();
            text.Should().Contain("invitation", $"'{c.Title}' must frame itself as an invitation");
            text.Should().Contain("pass is always free", $"'{c.Title}' must state passing is free");
            text.Should().Contain("the receiver holds every yes",
                $"'{c.Title}' must keep the receiver in charge");
        }
    }

    [Fact]
    public void Undivided_Json_HasEnoughSwapCards_ToActuallyRotateRoles()
    {
        // The Swap mechanic is what distinguishes Undivided from Afterglow. With
        // only a couple of Swap cards in a 30-card deck the roles barely turn
        // over, and one partner can spend most of a session giving.
        Shipped(new UndividedMode())
            .Count(c => c.Category == "Swap")
            .Should().BeGreaterThanOrEqualTo(4);
    }

    // ── Between the Two of You ────────────────────────────────────────────────

    [Fact]
    public void BetweenTheTwoOfYou_Json_EveryAxisHasTheSameNumberOfQuestions()
    {
        // The quiz scores by counting A/B/C/D across an axis, so an axis with
        // fewer questions than its siblings carries less weight in the result
        // than the design intends. Balance is a correctness property here, not
        // a tidiness one.
        string[] axes = ["Lead & Follow", "Give & Receive", "Plan & Spark",
                         "Words & Touch", "Bold & Cosy"];

        var counts = Shipped(new BetweenTheTwoOfYouMode())
            .Where(c => axes.Contains(c.Category))
            .GroupBy(c => c.Category)
            .ToDictionary(g => g.Key, g => g.Count());

        counts.Should().HaveCount(axes.Length, "every axis must be present");
        counts.Values.Distinct().Should().ContainSingle(
            "all axes must carry equal weight; got " +
            string.Join(", ", counts.Select(kv => $"{kv.Key}={kv.Value}")));
    }

    [Fact]
    public void BetweenTheTwoOfYou_Json_EveryQuestion_OffersExactlyFourOptions()
    {
        // Results cards interpret "Mostly A/B/C/D", so a question missing an
        // option silently biases every result that counts it.
        string[] axes = ["Lead & Follow", "Give & Receive", "Plan & Spark",
                         "Words & Touch", "Bold & Cosy"];

        foreach (var c in Shipped(new BetweenTheTwoOfYouMode())
                     .Where(c => axes.Contains(c.Category)))
        {
            var lines = c.Description.Split('\n');
            foreach (var letter in new[] { "A)", "B)", "C)", "D)" })
                lines.Should().Contain(l => l.TrimStart().StartsWith(letter),
                    $"'{c.Title}' must offer option {letter}");
        }
    }

    [Fact]
    public void BetweenTheTwoOfYou_Json_HasOneResultsCardPerAxis()
    {
        var deck = Shipped(new BetweenTheTwoOfYouMode());

        deck.Count(c => c.Category == "Results").Should().Be(5);
        deck.Count(c => c.Category == "Grow Together").Should().Be(1);
    }

    // ── Heat Check ────────────────────────────────────────────────────────────

    [Fact]
    public void HeatCheck_Json_EveryCardOffersBothATemperature()
    {
        // The entire mechanic is that the pair choose between candle and fire
        // together, and any mismatch means candle. A card carrying only one
        // option removes the choice — which is the consent mechanism here.
        foreach (var c in Shipped(new HeatCheckMode()))
        {
            c.Description.Should().Contain("Candle:", $"'{c.Title}' needs a candle option");
            c.Description.Should().Contain("Fire:", $"'{c.Title}' needs a fire option");
            c.Description.ToLowerInvariant().Should().Contain("unanimous",
                $"'{c.Title}' must state that fire requires agreement");
        }
    }

    // ── Last Orders ───────────────────────────────────────────────────────────

    [Fact]
    public void LastOrders_Json_EveryDrinkCard_IsAgeGated_AndOffersTheSoftOption()
    {
        // This is the deck's whole safety posture: alcohol cards are gated to
        // players who meet the minimum age, and every single one states that a
        // sip is a sip and that the soft option scores identically. A drink card
        // that loses either half is the one real harm this deck can do.
        var forfeits = Shipped(new LastOrdersMode())
            .Where(c => c.Category == "Forfeits").ToList();

        forfeits.Should().NotBeEmpty();
        foreach (var c in forfeits)
        {
            c.Restriction.Should().NotBeNull($"'{c.Title}' pours a drink and must be age-gated");
            var text = c.Description.ToLowerInvariant();
            text.Should().Contain("never a shot", $"'{c.Title}' must rule out shots");
            text.Should().Contain("soft drinks count the same",
                $"'{c.Title}' must state the soft option scores the same");
        }
    }

    [Fact]
    public void LastOrders_Json_OnlyDrinkCards_AreAgeGated()
    {
        // The social dares carry no alcohol, so gating them would exclude
        // players from the half of the deck that was written to include them.
        foreach (var c in Shipped(new LastOrdersMode()).Where(c => c.Restriction is not null))
            c.Category.Should().Be("Forfeits",
                $"'{c.Title}' is gated but isn't a drink card");
    }

    [Fact]
    public void LastOrders_Json_LastRoundCards_AreNotFramedAsOptional()
    {
        // Last Round is the duty-of-care close — water, food, getting everyone
        // home. Those cards deliberately carry no "pass is always free" footer,
        // because "this card is not optional and not a joke" is the point of
        // the section. A pass footer here quietly undoes it.
        foreach (var c in Shipped(new LastOrdersMode()).Where(c => c.Category == "Last Round"))
            c.Description.ToLowerInvariant().Should().NotContain("pass is always free",
                $"'{c.Title}' is a wind-down card and must not invite a pass");
    }

    [Fact]
    public void LastOrders_Json_SocialCards_AllStateThatPassingIsFree()
    {
        string[] social = ["Warm Up", "Party Tricks", "Confessions"];

        foreach (var c in Shipped(new LastOrdersMode()).Where(c => social.Contains(c.Category)))
            c.Description.ToLowerInvariant().Should().Contain("pass is always free",
                $"'{c.Title}' must state passing is free");
    }
}
