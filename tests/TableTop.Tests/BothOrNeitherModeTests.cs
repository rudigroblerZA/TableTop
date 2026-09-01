using TableTop.Core.Abstractions.Game;
using TableTop.Games.Couples;

namespace TableTop.Tests;

/// <summary>
/// <see cref="BothOrNeitherMode"/> — the fifth Intimate mode. Its consent
/// guarantees are structural rather than advisory, so they're worth pinning
/// the same way <c>AdultJsonDeckTests</c> pins Afterglow's and Undivided's:
/// the opening ritual, the opt-in language on every play card, and the
/// aftercare close.
///
/// The mode-specific invariant here is that <b>Pass is present as an explicit
/// option on every single play card</b>. That's not decoration — the whole
/// design rests on a no being indistinguishable from a different pick, and a
/// card that offered only three real choices with no pass would break that
/// silently while still looking fine.
/// </summary>
public sealed class BothOrNeitherModeTests
{
    private static readonly string[] PlayCategories = ["Opening", "Warmer", "Serious", "No Mistaking"];

    private static IReadOnlyList<TableTop.Core.Abstractions.Cards.ICard> Deck() =>
        new BothOrNeitherMode().GetCards([]);

    [Fact]
    public void RegisteredUnderCouplesIntimate_AsAnAdultMode()
    {
        var node = ArchetypeRegistry.Default().FindById("couples.intimate");

        node.Should().NotBeNull();
        node.Modes.Should().Contain(m => m.Name == "Both Or Neither");
        node.AgeRating.Should().Be(AgeRating.Adult,
            "explicit content must stay behind the adult gate the node already provides");
    }

    [Fact]
    public void SuitableFor_IsCoupleOnly()
    {
        // The reveal mechanic is meaningless with any other number of people.
        new BothOrNeitherMode().SuitableFor.Should().Be(TableShape.Couple);
    }

    [Fact]
    public void Deck_OpensOnConsent_AndClosesOnAftercare()
    {
        var deck = Deck();

        deck[0].Category.Should().Be("Consent",
            "a safeword has to be agreed before any card it governs");
        deck[^1].Category.Should().Be("Aftercare",
            "how a session lands matters as much as anything in it");
    }

    [Fact]
    public void ConsentRitual_ExplainsTheRevealRules_BeforeAnyPlayCard()
    {
        // This mode's mechanic doesn't work if the rules aren't understood, so
        // the explanation is part of the pinned consent block rather than a
        // README nobody reads mid-session.
        var consent = Deck().Where(c => c.Category == "Consent").ToList();

        consent.Should().HaveCountGreaterThanOrEqualTo(3);
        consent.Should().Contain(c => c.Description.Contains("reveal together")
                                   || c.Description.Contains("Reveal on three")
                                   || c.Description.Contains("reveal"),
            "the reveal mechanic must be explained before it's relied on");
        consent.Should().Contain(c => c.Description.Contains("safeword"));
    }

    [Fact]
    public void ConsentRitual_WarnsThatMismatchesAreNormal()
    {
        // Without this, a run of mismatches reads as mutual rejection rather
        // than as the design working — which is the one way this mechanic
        // could hurt rather than help.
        Deck().Where(c => c.Category == "Consent")
            .Should().Contain(c => c.Description.Contains("not a rejection")
                                || c.Description.Contains("produce nothing"),
                "players must be told up front that most turns produce nothing");
    }

    [Fact]
    public void EveryPlayCard_OffersPassExplicitly()
    {
        // The load-bearing invariant. A no has to be available on every card
        // without anyone reaching for a special move.
        var play = Deck().Where(c => PlayCategories.Contains(c.Category)).ToList();

        play.Should().HaveCountGreaterThan(10);
        play.Should().OnlyContain(c => c.Description.Contains("Pass — turn the next card"),
            "Pass must be a listed option on every play card, not an implied escape hatch");
    }

    [Fact]
    public void EveryPlayCard_CarriesTheOptInFooterAndCheckIn()
    {
        // Same repeated-footer pattern as Afterglow and Undivided: stated on
        // every card on purpose, because nobody should have to recall a rule
        // from twenty minutes ago mid-session.
        var play = Deck().Where(c => PlayCategories.Contains(c.Category)).ToList();

        play.Should().OnlyContain(c => c.Description.Contains("Pass is always one of your three"));
        play.Should().OnlyContain(c => c.Description.Contains("mismatch is normal"));
        play.Should().OnlyContain(c => c.Description.Contains("colour?"));
    }

    [Fact]
    public void EveryPlayCard_OffersExactlyThreeRealOptionsPlusPass()
    {
        foreach (var card in Deck().Where(c => PlayCategories.Contains(c.Category)))
        {
            card.Description.Should().Contain("<b>A.</b>");
            card.Description.Should().Contain("<b>B.</b>");
            card.Description.Should().Contain("<b>C.</b>");
            card.Description.Should().Contain("<b>D.</b> Pass");
        }
    }

    [Fact]
    public void Deck_HasNoDuplicateIdsOrBodies()
    {
        var deck = Deck();
        deck.Select(c => c.Id).Distinct().Should().HaveCount(deck.Count);
        deck.Select(c => c.Description).Distinct().Should().HaveCount(deck.Count);
    }

    [Fact]
    public void Deck_IdsAreStable_AcrossRepeatedCalls()
    {
        // CardDeckBuilder derives ids from content, so a resumed session still
        // resolves its cards even on the C# fallback path.
        var mode = new BothOrNeitherMode();
        mode.GetCards([]).Select(c => c.Id)
            .Should().Equal(mode.GetCards([]).Select(c => c.Id));
    }

    [Fact]
    public void Manifest_ReportsNonZeroTotalCards()
    {
        // The bug class that excluded Claimed! from every capped SurpriseMe
        // query for a full version.
        new BothOrNeitherMode().GetManifest().TotalCards.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CategoryColours_CoverEveryCategoryUsedOnACard()
    {
        var mode = new BothOrNeitherMode();
        var used = mode.GetCards([]).Select(c => c.Category).Distinct();

        used.Should().OnlyContain(c => mode.CategoryColours.ContainsKey(c),
            "a category with no colour falls back to difficulty tinting and loses the rising register");
    }

    [Fact]
    public void SkipLabel_DoesNotFrameAMismatchAsADecline()
    {
        // "Skip" would be wrong here: nothing was declined, and neither
        // player knows what the other picked.
        var mode = new BothOrNeitherMode();
        mode.SkipLabel.Should().NotBe("Skip");
        mode.SkipLabel.Should().Contain("Match");
    }
}
