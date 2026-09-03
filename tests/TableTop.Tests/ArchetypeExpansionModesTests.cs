using TableTop.Core.Abstractions.Game;
using TableTop.Games.Base;
using TableTop.Games.Couples;
using TableTop.Games.Fun;
using TableTop.Games.School;

namespace TableTop.Tests;

/// <summary>
/// Invariants for the three modes added in 1.40.0 — one per root archetype:
/// Hypothesis! (Classroom), The Pitch (Fun), House Rules (Couples).
///
/// <para>
/// Same shape as <see cref="NewArchetypeModesTests"/>, and for the same reason:
/// a mode that builds and even renders can still be unreachable, because
/// nothing about writing the class puts it in the registry. Registration is the
/// first assertion in each group here on purpose.
/// </para>
/// </summary>
public sealed class ArchetypeExpansionModesTests
{
    private static IReadOnlyList<ICard> CardsOf(BaseGameModeDefinition mode) =>
        ((IGameModeDefinition)mode).GetCards([]);

    private static BaseGameModeDefinition[] AllThree =>
        [new HypothesisMode(), new ThePitchMode(), new HouseRulesMode()];

    // ── Registry wiring ───────────────────────────────────────────────────────

    [Fact]
    public void Hypothesis_IsRegistered_UnderClassroom()
    {
        var node = ArchetypeRegistry.Default().FindById("classroom.hypothesis");
        node.Should().NotBeNull();
        node.Modes.Count(m => m.Name == "Hypothesis!").Should().Be(1);
    }

    [Fact]
    public void ThePitch_IsRegistered_UnderFun()
    {
        var node = ArchetypeRegistry.Default().FindById("fun.pitch");
        node.Should().NotBeNull();
        node.Modes.Count(m => m.Name == "The Pitch").Should().Be(1);
    }

    [Fact]
    public void HouseRules_IsRegistered_UnderCouplesConnection()
    {
        var node = ArchetypeRegistry.Default().FindById("couples.connection.houserules");
        node.Should().NotBeNull();
        node.Modes.Count(m => m.Name == "House Rules").Should().Be(1);
    }

    [Fact]
    public void AllThree_AreReachableFromTheRegistry()
    {
        var names = ArchetypeRegistry.Default().AllModes.Select(m => m.Name).ToList();

        names.Should().Contain("Hypothesis!");
        names.Should().Contain("The Pitch");
        names.Should().Contain("House Rules");
    }

    // ── Controller family ─────────────────────────────────────────────────────

    [Fact]
    public void AllThree_ResolveToCardTurn_SoEveryHeadCanAlreadyRenderThem()
    {
        // None of the three needs a new controller. If one ever does, this is
        // the test that says so before a head falls through to a screen that
        // rejects it — the failure mode ControllerFamily was introduced for.
        foreach (var mode in AllThree)
            ControllerFamilies.For(mode).Should().Be(ControllerFamily.CardTurn,
                $"{mode.Name} is a plain card-turn mode");
    }

    [Fact]
    public void AllThree_ArePlayableByEveryHead()
    {
        var modes = AllThree.Cast<IGameMode>().ToList();

        ControllerFamilies.UnsupportedIn(modes, ControllerFamilies.All).Should().BeEmpty();
    }

    // ── Deck shape ────────────────────────────────────────────────────────────

    [Fact]
    public void AllThree_HaveAColourForEveryCategoryTheyUse()
    {
        // An uncoloured category renders with default chrome, which reads as a
        // bug rather than a choice.
        foreach (var mode in AllThree)
            foreach (var category in CardsOf(mode).Select(c => c.Category).Distinct())
                mode.CategoryColours.Should().ContainKey(category,
                    $"{mode.Name} uses this category but declares no colour for it");
    }

    [Fact]
    public void AllThree_GiveEveryCardItsOwnTitle()
    {
        // These are title-style decks, not label-style ones — every card is
        // named. DeckTitleConventionTests fails a deck that ends up half of
        // each; this states which half these three were meant to be, so a later
        // batch of cards pasted in under a shared heading fails here first with
        // a clearer message.
        foreach (var mode in AllThree)
            CardsOf(mode).Select(c => c.Title).Should().OnlyHaveUniqueItems(
                $"{mode.Name} names every card");
    }

    [Fact]
    public void AllThree_HaveNoDuplicateCardIds()
    {
        // CardDeckBuilder derives ids from deck|category|title|body, so two
        // cards identical in all four collide silently and the second becomes
        // unreachable to anything resolving a saved session by id.
        foreach (var mode in AllThree)
            CardsOf(mode).Select(c => c.Id).Should().OnlyHaveUniqueItems(
                $"{mode.Name} has two cards with identical text");
    }

    [Fact]
    public void AllThree_HaveStableIdsAcrossCalls()
    {
        // The point of authoring through CardDeckBuilder rather than
        // StandardCard.Create: Guid.NewGuid would mint a fresh id per call and
        // break session resume.
        foreach (var mode in AllThree)
            CardsOf(mode).Select(c => c.Id).Should().Equal(
                CardsOf(mode).Select(c => c.Id),
                $"{mode.Name} must not mint new card ids per call");
    }

    // ── Hypothesis! ───────────────────────────────────────────────────────────

    [Fact]
    public void Hypothesis_PutsAnAnswerOnEveryCard()
    {
        // The reveal is the mode. A card without one is a question nobody can
        // settle, which is worse here than in a trivia deck because the point
        // is the mechanism rather than the outcome.
        CardsOf(new HypothesisMode()).Should().OnlyContain(
            c => c.Description.Contains("Answer:", StringComparison.Ordinal));
    }

    [Fact]
    public void Hypothesis_TellsPlayersToCommitBeforeFlipping_OnEveryCard()
    {
        CardsOf(new HypothesisMode()).Should().OnlyContain(
            c => c.Description.Contains("Predict first", StringComparison.Ordinal),
            "a rule stated once at the start is a rule nobody follows by card nine");
    }

    [Fact]
    public void Hypothesis_SpansEveryDifficulty()
    {
        // Scoring is difficulty-based, so a deck bunched at one tier would make
        // the strategy a constant with extra steps.
        CardsOf(new HypothesisMode()).Select(c => c.Difficulty).Distinct()
            .Should().HaveCount(4);
    }

    // ── The Pitch ─────────────────────────────────────────────────────────────

    [Fact]
    public void ThePitch_PutsACatchOnEveryCard()
    {
        // Without the catch the room reaches for the same joke every round —
        // that the product is bad — and the deck is spent in four cards.
        CardsOf(new ThePitchMode()).Should().OnlyContain(
            c => c.Description.Contains("<b>The catch:</b>", StringComparison.Ordinal));
    }

    [Fact]
    public void ThePitch_IsUnsuitableForACouple()
    {
        // The vote is the scoring mechanism; with two people every round is one
        // person judging the other.
        new ThePitchMode().SuitableFor.Suits(TableShape.Couple).Should().BeFalse();
    }

    [Fact]
    public void ThePitch_NeedsThreePlayers()
    {
        new ThePitchMode().MinimumPlayers.Should().Be(3);
    }

    // ── House Rules ───────────────────────────────────────────────────────────

    [Fact]
    public void HouseRules_DeclaresCoupleShape()
    {
        new HouseRulesMode().SuitableFor.Should().Be(TableShape.Couple);
    }

    [Fact]
    public void HouseRules_OpensWithSetupAndClosesWithThePact()
    {
        var mode = new HouseRulesMode();
        var cards = CardsOf(mode);

        mode.CategoriesPinnedToStart.Should().Equal("Before You Start");
        mode.CategoriesPinnedToEnd.Should().Equal("The Pact");

        // Declaring a pin for a category no card carries pins nothing at all.
        cards.Should().Contain(c => c.Category == "Before You Start");
        cards.Should().Contain(c => c.Category == "The Pact");

        cards[0].Category.Should().Be("Before You Start",
            "the bank's own order should already match the pins, shuffle aside");
        cards[^1].Category.Should().Be("The Pact");
    }

    [Fact]
    public void HouseRules_OffersParkingOnEveryDomainCard()
    {
        // Parking is what stops the deck manufacturing agreements neither
        // partner means. The setup and pact cards explain it in prose instead
        // of carrying the footer, so they are excluded here rather than
        // weakening the assertion to "most cards".
        var domainCards = CardsOf(new HouseRulesMode())
            .Where(c => c.Category != "Before You Start"
                     && c.Category != "The Pact")
            .ToList();

        domainCards.Should().NotBeEmpty();
        domainCards.Should().OnlyContain(
            c => c.Description.Contains("Park it", StringComparison.Ordinal));
    }

    [Fact]
    public void HouseRules_KeepsParkingFreeInTheSkipLabel()
    {
        // "Skipped" would tell a couple they failed the card. The label is the
        // only place most players will ever read the rule.
        new HouseRulesMode().SkipLabel.Should().Be("Park It");
    }

    [Fact]
    public void HouseRules_CarriesNoAdultContent()
    {
        // It sits at Teen in the registry, next to Future Us. If a card ever
        // arrives tagged adult, the rating and the manifest disagree, and
        // SurpriseMe would hand it to someone who asked for family-safe.
        new HouseRulesMode().GetManifest().HasAdultContent.Should().BeFalse();
    }
}
