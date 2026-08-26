using FluentAssertions;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Domain.Players;
using TableTop.Games.Base;
using TableTop.Games.Fun;
using TableTop.Hosting;
using Xunit;

namespace TableTop.Tests;

/// <summary>
/// The three modes added to fill mechanical gaps: co-operative, team-vs-team and
/// elimination. Every other mode in the catalogue is competitive-individual or
/// whole-table turn-taking.
/// </summary>
public sealed class NewFormatModesTests
{
    private static readonly IReadOnlyList<Core.Abstractions.Players.IPlayer> Four =
    [
        Player.Create("A"), Player.Create("B"), Player.Create("C"), Player.Create("D"),
    ];

    // ── All Together Now ──────────────────────────────────────────────────────

    [Fact]
    public void AllTogetherNow_OpensOnTheBrief_AndClosesOnTheDebrief()
    {
        // The brief is where the target gets agreed out loud; without it the mode
        // has no win condition at all, since the shared score isn't in the engine.
        var deck = ((IGameModeDefinition)new AllTogetherNowMode()).GetCards(Four);

        deck[0].Category.Should().Be("Brief");
        deck[^1].Category.Should().Be("Debrief");
    }

    [Fact]
    public void AllTogetherNow_BriefStatesThatTheDeckCanWin()
    {
        // A co-op game where losing is impossible isn't a game.
        var brief = string.Join(" ", ((IGameModeDefinition)new AllTogetherNowMode())
            .GetCards(Four).Where(c => c.Category == "Brief").Select(c => c.Description))
            .ToLowerInvariant();

        brief.Should().Contain("target");
        brief.Should().Contain("deck is allowed to beat you");
    }

    [Fact]
    public void AllTogetherNow_BriefCommitsToAdaptingCardsRatherThanExcludingPeople()
    {
        var brief = string.Join(" ", ((IGameModeDefinition)new AllTogetherNowMode())
            .GetCards(Four).Where(c => c.Category == "Brief").Select(c => c.Description))
            .ToLowerInvariant();

        brief.Should().Contain("adapt",
            "several cards assume standing, seeing or hearing, so the mode has to say what happens when someone can't");
    }

    // ── Split the Room ────────────────────────────────────────────────────────

    [Fact]
    public void SplitTheRoom_NeedsFourPlayers_BecauseTwoTeamsOfOneIsNotTeams()
    {
        new SplitTheRoomMode().MinimumPlayers.Should().Be(4);
    }

    [Fact]
    public void SplitTheRoom_EveryScoringCardStatesWhatItIsWorth()
    {
        // The engine has no team score, so the points live on the card. A card
        // that forgets to say is unplayable.
        var deck = ((IGameModeDefinition)new SplitTheRoomMode()).GetCards(Four);

        foreach (var card in deck.Where(c => c.Category is not ("Setup" or "Decider")))
            card.Description.Should().Contain("point",
                $"'{card.Title}' has to state its value");
    }

    [Fact]
    public void SplitTheRoom_SetupExplainsTheHandicapAndWhoHoldsThePen()
    {
        var setup = string.Join(" ", ((IGameModeDefinition)new SplitTheRoomMode())
            .GetCards(Four).Where(c => c.Category == "Setup").Select(c => c.Description))
            .ToLowerInvariant();

        setup.Should().Contain("smaller team goes first", "that is the entire handicap system");
        setup.Should().Contain("keeps score");
    }

    [Fact]
    public void SplitTheRoom_OffersADrawRatherThanForcingASuddenDeath()
    {
        var deciders = ((IGameModeDefinition)new SplitTheRoomMode())
            .GetCards(Four).Where(c => c.Category == "Decider").ToList();

        deciders.Should().HaveCountGreaterThanOrEqualTo(2);
        deciders.Should().Contain(c => c.Description.Contains("draw", StringComparison.OrdinalIgnoreCase),
            "a level score shouldn't have to end in an argument");
    }

    // ── Last One Standing ─────────────────────────────────────────────────────

    [Fact]
    public void LastOneStanding_NeedsThreePlayers_BecauseTwoEndsOnTheFirstElimination()
    {
        new LastOneStandingMode().MinimumPlayers.Should().Be(3);
    }

    [Fact]
    public void LastOneStanding_GivesEliminatedPlayersAJob()
    {
        // Elimination's known failure is the person knocked out early watching
        // everyone else play. Judging is the mitigation and it has to be stated.
        var rules = string.Join(" ", ((IGameModeDefinition)new LastOneStandingMode())
            .GetCards(Four).Where(c => c.Category == "Rules").Select(c => c.Description))
            .ToLowerInvariant();

        rules.Should().Contain("judge");
    }

    [Fact]
    public void LastOneStanding_HasRevivalCards_SoEliminationIsNotPermanent()
    {
        ((IGameModeDefinition)new LastOneStandingMode()).GetCards(Four)
            .Count(c => c.Category == "Revival")
            .Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void LastOneStanding_RulesCoverTheAllOutAndCantComplyCases()
    {
        var rules = string.Join(" ", ((IGameModeDefinition)new LastOneStandingMode())
            .GetCards(Four).Where(c => c.Category == "Rules").Select(c => c.Description))
            .ToLowerInvariant();

        rules.Should().Contain("nobody goes out",
            "a card that eliminates everyone simultaneously would end the game with no winner");
        rules.Should().Contain("physically can't",
            "eliminating someone for a body they didn't choose is the one outcome this format must rule out");
    }

    [Fact]
    public void LastOneStanding_ClosesOnTheTwoPlayerEndgame()
    {
        ((IGameModeDefinition)new LastOneStandingMode()).GetCards(Four)[^1]
            .Category.Should().Be("Final");
    }

    // ── all three ─────────────────────────────────────────────────────────────

    [Fact]
    public void AllThree_DeclareThemselvesUnsuitableForACouple()
    {
        // Co-op wants a crowd, two teams need four, and elimination with two
        // players ends on the first card.
        ITableShapeMode[] modes =
            [new AllTogetherNowMode(), new SplitTheRoomMode(), new LastOneStandingMode()];

        foreach (var mode in modes)
            mode.SuitableFor.Suits(TableShape.Couple).Should().BeFalse(
                $"{mode.GetType().Name} doesn't work for two");
    }

    [Fact]
    public void AllThree_AreReachableFromTheRegistry()
    {
        var names = ArchetypeRegistry.Default().AllModes.Select(m => m.Name).ToList();

        names.Should().Contain("All Together Now");
        names.Should().Contain("Split the Room");
        names.Should().Contain("Last One Standing");
    }

    [Fact]
    public void AllThree_LoadTheirDeckFromJson_NotTheFallbackBank()
    {
        // Both representations exist from the start for these modes; if the JSON
        // stops being found, the card count silently changes to the bank's.
        (IGameModeDefinition Mode, int Expected)[] cases =
        [
            (new AllTogetherNowMode(),  AllTogetherNowCardBank.All.Count),
            (new SplitTheRoomMode(),    SplitTheRoomCardBank.All.Count),
            (new LastOneStandingMode(), LastOneStandingCardBank.All.Count),
        ];

        foreach (var (mode, expected) in cases)
            mode.GetCards(Four).Should().HaveCount(expected,
                "JSON and bank were generated together and must stay in step");
    }

    [Fact]
    public void AllThree_HaveAColourForEveryCategoryTheyUse()
    {
        // An uncoloured category renders with default chrome, which reads as a
        // bug rather than a choice.
        BaseGameModeDefinition[] modes =
            [new AllTogetherNowMode(), new SplitTheRoomMode(), new LastOneStandingMode()];

        foreach (var mode in modes)
            foreach (var category in ((IGameModeDefinition)mode).GetCards(Four)
                         .Select(c => c.Category).Distinct())
                mode.CategoryColours.Should().ContainKey(category,
                    $"{mode.Name} uses '{category}' but declares no colour for it");
    }
}
