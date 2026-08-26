using TableTop.Core.Domain.Decks;
using TableTop.Core.Abstractions.Restrictions;
using TableTop.Core.Domain.Restrictions;

namespace TableTop.Tests;

public sealed class RestrictionParserTests
{
    private static IRestriction Parse(string expr) =>
        RestrictionParser.Parse(expr)!;

    [Fact]
    public void Parse_Adult_ReturnsAdultOnlyRestriction() =>
        Parse("adult").Should().BeOfType<AdultOnlyRestriction>();

    [Fact]
    public void Parse_Male_ReturnsMaleOnlyRestriction() =>
        Parse("male").Should().BeOfType<MaleOnlyRestriction>();

    [Fact]
    public void Parse_Female_ReturnsFemaleOnlyRestriction() =>
        Parse("female").Should().BeOfType<FemaleOnlyRestriction>();

    [Fact]
    public void Parse_Couple_ReturnsCoupleOnlyRestriction() =>
        Parse("couple").Should().BeOfType<CoupleOnlyRestriction>();

    [Fact]
    public void Parse_Parent_ReturnsParentOnlyRestriction() =>
        Parse("parent").Should().BeOfType<ParentOnlyRestriction>();

    [Fact]
    public void Parse_Married_ReturnsMarriedOnlyRestriction() =>
        Parse("married").Should().BeOfType<MarriedOnlyRestriction>();

    [Fact]
    public void Parse_TagColon_ReturnsTagRestriction()
    {
        var r = Parse("tag:premium");
        r.Should().BeOfType<TagRestriction>();
        var player = Player.Create("Alice", tags: ["premium"]);
        r.IsSatisfiedBy(player, []).Should().BeTrue();
    }

    [Fact]
    public void Parse_AgeColon_ReturnsMinimumAgeRestriction()
    {
        var r = Parse("age:18");
        r.Should().BeOfType<MinimumAgeRestriction>();
        var adult = Player.Create("Alice", attributes: new Dictionary<string, string> { ["age"] = "21" });
        var minor = Player.Create("Bob",   attributes: new Dictionary<string, string> { ["age"] = "15" });
        r.IsSatisfiedBy(adult, []).Should().BeTrue();
        r.IsSatisfiedBy(minor, []).Should().BeFalse();
    }

    [Fact]
    public void Parse_AttrColon_ReturnsAttributeRestriction()
    {
        var r = Parse("attr:role=admin");
        r.Should().BeOfType<AttributeRestriction>();
        var admin  = Player.Create("Alice", attributes: new Dictionary<string, string> { ["role"] = "admin" });
        var normal = Player.Create("Bob");
        r.IsSatisfiedBy(admin, []).Should().BeTrue();
        r.IsSatisfiedBy(normal, []).Should().BeFalse();
    }

    [Fact]
    public void Parse_And_ReturnsAndRestriction()
    {
        var r      = Parse("and(adult,parent)");
        r.Should().BeOfType<AndRestriction>();
        var both   = Player.Create("Alice", tags: ["adult", "parent"]);
        var onlyAdult = Player.Create("Bob", tags: ["adult"]);
        r.IsSatisfiedBy(both,      []).Should().BeTrue();
        r.IsSatisfiedBy(onlyAdult, []).Should().BeFalse();
    }

    [Fact]
    public void Parse_Or_ReturnsOrRestriction()
    {
        var r      = Parse("or(parent,married)");
        r.Should().BeOfType<OrRestriction>();
        var parent  = Player.Create("Alice", tags: ["parent"]);
        var neither = Player.Create("Bob");
        r.IsSatisfiedBy(parent,  []).Should().BeTrue();
        r.IsSatisfiedBy(neither, []).Should().BeFalse();
    }

    [Fact]
    public void Parse_Not_ReturnsNotRestriction()
    {
        var r     = Parse("not(adult)");
        r.Should().BeOfType<NotRestriction>();
        var adult = Player.Create("Alice", tags: ["adult"]);
        var other = Player.Create("Bob");
        r.IsSatisfiedBy(adult, []).Should().BeFalse();
        r.IsSatisfiedBy(other, []).Should().BeTrue();
    }

    [Fact]
    public void Parse_NestedAnd_EvaluatesCorrectly()
    {
        // and(adult,or(parent,married))
        var r = Parse("and(adult,or(parent,married))");

        var adultParent  = Player.Create("A", tags: ["adult", "parent"]);
        var adultMarried = Player.Create("B", tags: ["adult", "married"]);
        var adultOnly    = Player.Create("C", tags: ["adult"]);
        var child        = Player.Create("D");

        r.IsSatisfiedBy(adultParent,  []).Should().BeTrue();
        r.IsSatisfiedBy(adultMarried, []).Should().BeTrue();
        r.IsSatisfiedBy(adultOnly,    []).Should().BeFalse();
        r.IsSatisfiedBy(child,        []).Should().BeFalse();
    }

    [Fact]
    public void Parse_CaseInsensitive_Works()
    {
        Parse("ADULT").Should().BeOfType<AdultOnlyRestriction>();
        Parse("AND(ADULT,MALE)").Should().BeOfType<AndRestriction>();
    }

    [Fact]
    public void Parse_InvalidKeyword_ThrowsFormatException()
        { Xunit.Assert.Throws<FormatException>(() => Parse("wizard")); }

    [Fact]
    public void Parse_MissingCloseParen_ThrowsFormatException()
        { Xunit.Assert.Throws<FormatException>(() => Parse("and(adult,male")); }

    [Fact]
    public void Parse_InvalidAge_ThrowsFormatException()
        { Xunit.Assert.Throws<FormatException>(() => Parse("age:notanumber")); }

    [Fact]
    public void Parse_MissingAttrEquals_ThrowsFormatException()
        { Xunit.Assert.Throws<FormatException>(() => Parse("attr:roleonly")); }

    [Fact]
    public void Parse_TrailingText_ThrowsFormatException()
        { Xunit.Assert.Throws<FormatException>(() => Parse("adult garbage")); }
}