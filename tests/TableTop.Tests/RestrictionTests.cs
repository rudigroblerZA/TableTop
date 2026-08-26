using System.Collections.Generic;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Restrictions;
using TableTop.Core.Domain.Restrictions;

namespace TableTop.Tests;

public sealed class RestrictionTests
{
    private static Player MakePlayer(string name, IDictionary<string, string>? attrs = null, IEnumerable<string>? tags = null) =>
        Player.Create(name, attrs, tags);

    // ── TagRestriction ────────────────────────────────────────────────────────

    [Fact]
    public void TagRestriction_PlayerHasTag_ReturnsTrue()
    {
        var r = new TagRestriction("adult");
        var player = MakePlayer("Alice", tags: ["adult"]);
        r.IsSatisfiedBy(player, []).Should().BeTrue();
    }

    [Fact]
    public void TagRestriction_PlayerMissingTag_ReturnsFalse()
    {
        var r = new TagRestriction("adult");
        var player = MakePlayer("Bob");
        r.IsSatisfiedBy(player, []).Should().BeFalse();
    }

    // ── AttributeRestriction ──────────────────────────────────────────────────

    [Fact]
    public void AttributeRestriction_MatchingAttribute_ReturnsTrue()
    {
        var r = new AttributeRestriction("gender", "male");
        var player = MakePlayer("Charlie", new Dictionary<string, string> { ["gender"] = "male" });
        r.IsSatisfiedBy(player, []).Should().BeTrue();
    }

    [Fact]
    public void AttributeRestriction_CaseInsensitiveMatch_ReturnsTrue()
    {
        var r = new AttributeRestriction("gender", "Male");
        var player = MakePlayer("Dave", new Dictionary<string, string> { ["gender"] = "MALE" });
        r.IsSatisfiedBy(player, []).Should().BeTrue();
    }

    // ── CompositeRestrictions ─────────────────────────────────────────────────

    [Fact]
    public void AndRestriction_BothTrue_ReturnsTrue()
    {
        var r = new TagRestriction("adult").And(new TagRestriction("parent"));
        var player = MakePlayer("Eve", tags: ["adult", "parent"]);
        r.IsSatisfiedBy(player, []).Should().BeTrue();
    }

    [Fact]
    public void AndRestriction_OneFalse_ReturnsFalse()
    {
        var r = new TagRestriction("adult").And(new TagRestriction("parent"));
        var player = MakePlayer("Frank", tags: ["adult"]);
        r.IsSatisfiedBy(player, []).Should().BeFalse();
    }

    [Fact]
    public void OrRestriction_OnlyOneTrue_ReturnsTrue()
    {
        var r = new TagRestriction("adult").Or(new TagRestriction("parent"));
        var player = MakePlayer("Grace", tags: ["parent"]);
        r.IsSatisfiedBy(player, []).Should().BeTrue();
    }

    [Fact]
    public void NotRestriction_InvertsResult()
    {
        var r = new TagRestriction("adult").Not();
        var player = MakePlayer("Heidi", tags: ["adult"]);
        r.IsSatisfiedBy(player, []).Should().BeFalse();
    }

    // ── CoupleOnlyRestriction ─────────────────────────────────────────────────

    [Fact]
    public void CoupleOnly_RequiresPartnerInContext()
    {
        var r = new CoupleOnlyRestriction();
        var player1 = MakePlayer("Alice", tags: ["couple-member"]);
        var player2 = MakePlayer("Bob", tags: ["couple-member"]);

        r.IsSatisfiedBy(player1, [player1, player2]).Should().BeTrue();
    }

    [Fact]
    public void CoupleOnly_NoPartnerInContext_ReturnsFalse()
    {
        var r = new CoupleOnlyRestriction();
        var player = MakePlayer("Alice", tags: ["couple-member"]);
        r.IsSatisfiedBy(player, [player]).Should().BeFalse();
    }

    // ── MinimumAgeRestriction ─────────────────────────────────────────────────

    [Fact]
    public void MinimumAge_PlayerMeetsAge_ReturnsTrue()
    {
        var r = new MinimumAgeRestriction(18);
        var player = MakePlayer("Alice", new Dictionary<string, string> { ["age"] = "21" });
        r.IsSatisfiedBy(player, []).Should().BeTrue();
    }

    [Fact]
    public void MinimumAge_PlayerTooYoung_ReturnsFalse()
    {
        var r = new MinimumAgeRestriction(18);
        var player = MakePlayer("Bob", new Dictionary<string, string> { ["age"] = "16" });
        r.IsSatisfiedBy(player, []).Should().BeFalse();
    }
}