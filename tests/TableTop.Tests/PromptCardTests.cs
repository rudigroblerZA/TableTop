namespace TableTop.Tests;

public sealed class PromptCardTests
{
    private static Player MakePlayer(string name, string gender, int age = 25) =>
        Player.Create(name,
            attributes: new Dictionary<string, string> { ["gender"] = gender, ["age"] = age.ToString() },
            tags: age >= 18 ? new string[] { "adult" } : new string[0]);

    [Fact]
    public void ResolvePrompt_MalePlayer_ReturnsMaleText()
    {
        var card = PromptCard.CreateGenderDirected(
            title: "Test",
            maleText:   "For males",
            femaleText: "For females",
            otherText:  "For others",
            Difficulty.Easy, "Test");

        var player = MakePlayer("Bob", "male");
        card.ResolvePrompt(player).Should().Be("For males");
    }

    [Fact]
    public void ResolvePrompt_FemalePlayer_ReturnsFemaleText()
    {
        var card = PromptCard.CreateGenderDirected(
            title: "Test",
            maleText:   "For males",
            femaleText: "For females",
            otherText:  "For others",
            Difficulty.Easy, "Test");

        var player = MakePlayer("Alice", "female");
        card.ResolvePrompt(player).Should().Be("For females");
    }

    [Fact]
    public void ResolvePrompt_OtherGender_ReturnsOtherText()
    {
        var card = PromptCard.CreateGenderDirected(
            title: "Test",
            maleText:   "For males",
            femaleText: "For females",
            otherText:  "For others",
            Difficulty.Easy, "Test");

        var player = MakePlayer("Alex", "other");
        card.ResolvePrompt(player).Should().Be("For others");
    }

    [Fact]
    public void ResolvePrompt_NoGenderAttribute_ReturnsOtherText()
    {
        var card = PromptCard.CreateGenderDirected(
            title: "Test",
            maleText:   "For males",
            femaleText: "For females",
            otherText:  "Default fallback",
            Difficulty.Easy, "Test");

        var player = Player.Create("Unknown"); // no attributes
        card.ResolvePrompt(player).Should().Be("Default fallback");
    }

    [Fact]
    public void ResolvePrompt_CaseInsensitiveGender_Matches()
    {
        var card = PromptCard.CreateGenderDirected(
            title: "Test",
            maleText:   "For males",
            femaleText: "For females",
            otherText:  "For others",
            Difficulty.Easy, "Test");

        var player = MakePlayer("Charlie", "MALE");
        card.ResolvePrompt(player).Should().Be("For males");
    }

    [Fact]
    public void PromptCard_ImplementsICard_TransparentlyForBaseConsumers()
    {
        // A consumer that only knows ICard should still receive a non-null description.
        ICard card = PromptCard.CreateGenderDirected(
            title: "Test",
            maleText:   "For males",
            femaleText: "For females",
            otherText:  "Base description",
            Difficulty.Easy, "Category");

        card.Description.Should().NotBeNullOrWhiteSpace();
        card.Title.Should().Be("Test");
    }

    [Fact]
    public void AttributeDirectedResolver_MatchesCustomAttribute()
    {
        var card = PromptCard.CreateAttributeDirected(
            title: "Role Prompt",
            attributeKey: "role",
            defaultText: "Everyone",
            variants: new Dictionary<string, string>
            {
                ["host"]  = "You organised this — own it.",
                ["guest"] = "You showed up — that counts.",
            },
            Difficulty.Easy, "Prompt");

        var host  = Player.Create("Host",  attributes: new Dictionary<string, string> { ["role"] = "host" });
        var guest = Player.Create("Guest", attributes: new Dictionary<string, string> { ["role"] = "guest" });
        var other = Player.Create("Other");

        card.ResolvePrompt(host).Should().Be("You organised this — own it.");
        card.ResolvePrompt(guest).Should().Be("You showed up — that counts.");
        card.ResolvePrompt(other).Should().Be("Everyone");
    }

    [Fact]
    public void PromptCard_IsDetectableAsIPromptCard()
    {
        ICard card = PromptCard.CreateGenderDirected(
            "T", "M", "F", "O", Difficulty.Easy, "Cat");

        card.Should().BeAssignableTo<IPromptCard>();
    }
}