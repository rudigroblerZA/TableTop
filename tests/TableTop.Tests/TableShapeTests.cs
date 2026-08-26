using FluentAssertions;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Players;
using TableTop.Games;
using TableTop.Games.Couples;
using TableTop.Games.Party;
using TableTop.Hosting;
using Xunit;

namespace TableTop.Tests;

/// <summary>
/// Table shape: declaring who a mode is for, inferring who is actually present,
/// and filtering the selection tree by the two together.
/// </summary>
public sealed class TableShapeTests
{
    private static Player Tagged(string name, params string[] tags) =>
        Player.Create(name, tags: tags);

    // ── matching ──────────────────────────────────────────────────────────────

    [Fact]
    public void Suits_MatchesOnOverlap_NotEquality()
    {
        // A mode for teams-or-groups must show up for a table that is just a
        // group; requiring an exact match would hide most of the catalogue.
        var declared = TableShape.Team | TableShape.Group;

        declared.Suits(TableShape.Group).Should().BeTrue();
        declared.Suits(TableShape.Team).Should().BeTrue();
        declared.Suits(TableShape.Couple).Should().BeFalse();
    }

    [Fact]
    public void ATableOfSeveralShapes_MatchesAnythingFittingAnyOfThem()
    {
        var colleaguesWhoAreAlsoFriends = TableShape.Team | TableShape.Group;

        TableShape.Team.Suits(colleaguesWhoAreAlsoFriends).Should().BeTrue();
        TableShape.Couple.Suits(colleaguesWhoAreAlsoFriends).Should().BeFalse();
    }

    [Fact]
    public void AnUnknownTable_MatchesEverything()
    {
        TableShape.Couple.Suits(TableShape.None).Should().BeTrue();
        TableComposition.Unknown.Shape.Should().Be(TableShapes.Any);
    }

    // ── inference ─────────────────────────────────────────────────────────────

    [Fact]
    public void TwoPartners_AreACouple()
    {
        var table = TableComposition.From(
            [Tagged("A", "couple-member"), Tagged("B", "couple-member")]);

        table.Shape.Should().HaveFlag(TableShape.Couple);
        table.PlayerCount.Should().Be(2);
    }

    [Fact]
    public void ThreePeopleTaggedAsPartners_AreNotACouple()
    {
        // Whatever that table is, the couples decks' two-person framing doesn't fit it.
        TableComposition.From(
            [Tagged("A", "couple-member"), Tagged("B", "couple-member"), Tagged("C", "couple-member")])
            .Shape.Should().NotHaveFlag(TableShape.Couple);
    }

    [Fact]
    public void AParentAndAChild_AreAFamily()
    {
        TableComposition.From([Tagged("Mum", "parent"), Tagged("Kid", "child")])
            .Shape.Should().HaveFlag(TableShape.Family);
    }

    [Fact]
    public void TwoParentsWithoutChildren_AreNotAFamilyTable()
    {
        // Two parents on a night out are a group, and should see the group
        // content rather than the family content.
        var table = TableComposition.From([Tagged("A", "parent"), Tagged("B", "parent")]);

        table.Shape.Should().NotHaveFlag(TableShape.Family);
        table.Shape.Should().HaveFlag(TableShape.Group);
    }

    [Fact]
    public void ColleaguesAreATeam_AndAlsoPlayableAsAGroup()
    {
        var table = TableComposition.From(
            [Tagged("A", "colleague"), Tagged("B", "colleague"), Tagged("C", "colleague")]);

        table.Shape.Should().HaveFlag(TableShape.Team);
        table.Shape.Should().HaveFlag(TableShape.Group);
    }

    [Fact]
    public void UntaggedPlayers_FallBackToGroup_RatherThanToNothing()
    {
        // Most real tables carry no tags at all. Inferring "nothing" would hide
        // the entire catalogue from them.
        var table = TableComposition.From([Player.Create("A"), Player.Create("B"), Player.Create("C")]);

        table.Shape.Should().Be(TableShape.Group);
    }

    [Fact]
    public void NoPlayers_IsUnknown_NotEmpty()
    {
        TableComposition.From([]).Shape.Should().Be(TableShapes.Any);
    }

    // ── the permissive default, which is the whole safety property ────────────

    [Fact]
    public void AModeThatDeclaresNothing_SurvivesEveryTableFilter()
    {
        // ~85 of ~92 modes declare no shape. If the default were restrictive,
        // setting any table filter would empty the selection screen, and it
        // would read as a filter bug rather than as missing annotations.
        var undeclared = new TruthOrDareMode();
        undeclared.Should().NotBeAssignableTo<ITableShapeMode>();

        var tree = new List<Archetype>
        {
            new("t", "Test", "d", "🎲", [undeclared]),
        };

        foreach (var shape in new[]
                 { TableShape.Couple, TableShape.Family, TableShape.Team, TableShape.Group })
            new ArchetypeFilter(AgeRating.AllAges, AgeRating.Adult, shape)
                .Apply(tree).Should().ContainSingle(because: $"{shape} must not hide unannotated modes");
    }

    [Fact]
    public void CouplesModes_AreHiddenFromAFamilyTable()
    {
        var tree = new List<Archetype>
        {
            new("c", "Couples", "d", "💞", [new UndividedMode(), new AfterglowMode()]),
        };

        var family = TableComposition.From([Tagged("Mum", "parent"), Tagged("Kid", "child")]);

        new ArchetypeFilter(family).Apply(tree)
            .Should().BeEmpty("a node left with no modes is dropped entirely");
    }

    [Fact]
    public void CouplesModes_SurviveACoupleTable()
    {
        var tree = new List<Archetype>
        {
            new("c", "Couples", "d", "💞", [new UndividedMode(), new AfterglowMode()]),
        };

        var couple = TableComposition.From(
            [Tagged("A", "couple-member"), Tagged("B", "couple-member")]);

        new ArchetypeFilter(couple).Apply(tree)
            .Should().ContainSingle().Which.Modes.Should().HaveCount(2);
    }


    [Fact]
    public void ALargerFamily_IsStillNotAGenericAdultGroup()
    {
        // The head-count rule would otherwise add Group to a family of four,
        // and everything written for adults out together would reappear in
        // front of the children.
        var table = TableComposition.From(
            [Tagged("Mum", "parent"), Tagged("Dad", "parent"),
             Tagged("Kid", "child"), Tagged("Teen", "teen")]);

        table.Shape.Should().HaveFlag(TableShape.Family);
        table.Shape.Should().NotHaveFlag(TableShape.Group);
    }

    [Fact]
    public void TheDrinkingGameIsHiddenFromAFamilyOfFour()
    {
        var tree = new List<Archetype>
        {
            new("f", "Fun", "d", "🎉", [new LastOrdersMode()]),
        };

        var family = TableComposition.From(
            [Tagged("Mum", "parent"), Tagged("Dad", "parent"),
             Tagged("Kid", "child"), Tagged("Teen", "teen")]);

        new ArchetypeFilter(family).Apply(tree).Should().BeEmpty();
    }

    [Fact]
    public void TheDrinkingGameIsHiddenFromAFamilyTable()
    {
        var tree = new List<Archetype>
        {
            new("f", "Fun", "d", "🎉", [new LastOrdersMode()]),
        };

        var family = TableComposition.From([Tagged("Dad", "parent"), Tagged("Teen", "teen")]);

        new ArchetypeFilter(family).Apply(tree).Should().BeEmpty();
    }

    // ── it composes with the filter that was already there ────────────────────

    [Fact]
    public void ShapeAndAgeRating_BothApply()
    {
        var tree = new List<Archetype>
        {
            new("c", "Couples", "d", "💞", [new UndividedMode()], null, AgeRating.Adult),
        };

        var couple = TableComposition.From(
            [Tagged("A", "couple-member"), Tagged("B", "couple-member")]);

        // Right table, but the rating ceiling still excludes it.
        new ArchetypeFilter(couple, maxAgeRating: AgeRating.Teen).Apply(tree).Should().BeEmpty();
        new ArchetypeFilter(couple, maxAgeRating: AgeRating.Adult).Apply(tree).Should().ContainSingle();
    }

    [Fact]
    public void DefaultFilter_IsUnchangedByThisFeature()
    {
        // Every existing caller passes no shape and must see exactly what it saw
        // before.
        var tree = new List<Archetype>
        {
            new("c", "Couples", "d", "💞", [new UndividedMode(), new TruthOrDareMode()]),
        };

        ArchetypeFilter.ShowEverything.Apply(tree)
            .Single().Modes.Should().HaveCount(2);
    }

    [Fact]
    public void EveryWiredMode_DeclaresSomethingReachable()
    {
        ITableShapeMode[] wired =
        [
            new UndividedMode(), new AfterglowMode(), new HeatCheckMode(), new SlowBurnMode(),
            new AllInMode(), new RelationshipDaresMode(), new TheLongGameMode(),
            new BetweenTheTwoOfYouMode(), new LastOrdersMode(),
        ];

        foreach (var mode in wired)
        {
            mode.SuitableFor.Should().NotBe(TableShape.None,
                $"{mode.GetType().Name} would be invisible at every table");
            mode.SuitableFor.Suits(TableShapes.Any).Should().BeTrue();
        }
    }

    // ── TableSuitability: checked at launch, not just at the picker ───────────
    //
    // Backlog item 17. ArchetypeFilter only ever ran on the selection screen;
    // nothing consulted SuitableFor on the way into a game, so an incompatible
    // roster could start a couple-only mode, have every question card stripped
    // by its CoupleOnlyRestriction, and get a session with no explanation for
    // why it played nothing.

    [Fact]
    public void UnsuitableTable_IsReportedNotSilentlyAccepted()
    {
        var untaggedPair = new IPlayer[] { Player.Create("A"), Player.Create("B") };

        var result = TableSuitability.Check(new BetweenTheTwoOfYouMode(), untaggedPair);

        result.Suits.Should().BeFalse();
        result.Explanation.Should().Contain("Couple");
    }

    [Fact]
    public void SuitableTable_HasNoExplanation()
    {
        var couple = new IPlayer[] { Tagged("A", "couple-member"), Tagged("B", "couple-member") };

        var result = TableSuitability.Check(new BetweenTheTwoOfYouMode(), couple);

        result.Suits.Should().BeTrue();
        result.Explanation.Should().BeNull();
    }

    [Fact]
    public void AModeThatDeclaresNothing_AlwaysSuits()
    {
        // Same permissive default as ArchetypeFilter — most of the catalogue
        // has no real constraint, so absence of ITableShapeMode must not read
        // as "suits nothing".
        var result = TableSuitability.Check(new TruthOrDareMode(), [Player.Create("A")]);

        result.Suits.Should().BeTrue();
        result.Required.Should().Be(TableShape.None);
    }

    [Fact]
    public void UnsuitableGroupMode_NamesEveryShapeItAccepts()
    {
        // LastOrdersMode wants Group or Team; the explanation should say both,
        // not just the first, so a player knows every way to fix their table.
        // A couple table is neither: two people, tagged as partners, is
        // exactly the shape Group/Team's own count-based inference excludes.
        var couple = new IPlayer[] { Tagged("A", "couple-member"), Tagged("B", "couple-member") };

        var result = TableSuitability.Check(new LastOrdersMode(), couple);

        result.Suits.Should().BeFalse();
        result.Explanation.Should().Contain("Group").And.Contain("Team");
    }
}
