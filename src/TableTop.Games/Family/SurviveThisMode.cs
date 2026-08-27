using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Survive This! — rate survival chances in ridiculously specific scenarios.
///
/// How to play:
///   1. Read an absurd scenario aloud.
///   2. Everyone privately rates their survival chances 1-5 (1=dead instantly, 5=thriving).
///   3. Reveal ratings and discuss strategies.
///   4. Points for good reasoning, creativity, and not taking it seriously.
///
/// Scenarios range from mildly inconvenient ("Stuck on an elevator with a mime") to
/// genuinely impossible ("You are now made of pasta, discuss"). The fun is in the
/// absurdity and the creative "solutions" people propose: "Well, I'd seduce the mime.
/// They can't resist charm."
///
/// Works as a casual conversation starter or a laugh-out-loud game. Great for groups
/// that enjoy lateral thinking and dark humour. Embrace the stupidity.
/// </summary>
public sealed class SurviveThisMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Survive This!";
    /// <inheritdoc />
    public override string Description =>
        "Rate your survival chances 1-5. Explain your strategy. Discuss chaos.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Survived";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Forfeit";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [SurviveThisCardBank.InconvenientCategory] = "#FFCA28",
            [SurviveThisCardBank.RidiculousCategory] = "#EC407A",
            [SurviveThisCardBank.ImpossibleCategory] = "#EF5350",
            [SurviveThisCardBank.WeirdCategory] = "#AB47BC",
            [SurviveThisCardBank.SocialCategory] = "#42A5F5",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        SurviveThisCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => SurviveThisCardBank.All;
}

/// <summary>Built-in card bank for Survive This. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class SurviveThisCardBank
{
    internal const string InconvenientCategory = "Inconvenient";
    internal const string RidiculousCategory = "Ridiculous";
    internal const string ImpossibleCategory = "Impossible";
    internal const string WeirdCategory = "Weird";
    internal const string SocialCategory = "Social";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── INCONVENIENT ──────────────────────────────────────────────────────
        S(InconvenientCategory,
            "Stuck on an elevator with a mime. It's broken. Could take hours.",
            Difficulty.Easy),
        S(InconvenientCategory,
            "Your phone dies right before you were about to open your boarding pass.",
            Difficulty.Easy),
        S(InconvenientCategory,
            "You're alone at a dinner party where you know nobody and can't find the bathroom.",
            Difficulty.Easy),
        S(InconvenientCategory,
            "Coffee shop has the wrong order but your name sounds exactly like someone else's.",
            Difficulty.Easy),
        S(InconvenientCategory,
            "You're stuck next to someone on a plane who won't stop talking.",
            Difficulty.Easy),

        // ── RIDICULOUS ────────────────────────────────────────────────────────
        S(RidiculousCategory,
            "You can only communicate through interpretive dance for the next 24 hours.",
            Difficulty.Medium),
        S(RidiculousCategory,
            "You've been cursed to tell the absolute truth no matter what.",
            Difficulty.Medium),
        S(RidiculousCategory,
            "Suddenly you can only speak in rhyme. Forever.",
            Difficulty.Medium),
        S(RidiculousCategory,
            "Everything you say comes out in dramatic Shakespearean English.",
            Difficulty.Medium),
        S(RidiculousCategory,
            "You're now three inches tall. Society doesn't change. Discuss.",
            Difficulty.Hard),
        S(RidiculousCategory,
            "Everyone around you speaks in movie quotes only. You can't break character.",
            Difficulty.Medium),

        // ── IMPOSSIBLE ────────────────────────────────────────────────────────
        S(ImpossibleCategory,
            "You are now made entirely of pasta. Spaghetti, to be specific.",
            Difficulty.Hard),
        S(ImpossibleCategory,
            "Gravity is backwards. The sky is now below you.",
            Difficulty.Hard),
        S(ImpossibleCategory,
            "You are now the size of a building. Buildings haven't been resized. Survive.",
            Difficulty.Hard),
        S(ImpossibleCategory,
            "Time now moves backwards but your memories go forward. Discuss.",
            Difficulty.Hard),
        S(ImpossibleCategory,
            "Everything is made of jello. Including you. Including the ground.",
            Difficulty.Hard),

        // ── WEIRD ────────────────────────────────────────────────────────────
        S(WeirdCategory,
            "You've switched bodies with your pet. You're now your pet. Your pet is you.",
            Difficulty.Medium),
        S(WeirdCategory,
            "You discover that pigeons are sentient and they're angry.",
            Difficulty.Easy),
        S(WeirdCategory,
            "All plants have declared war on humanity. They're very slow but very angry.",
            Difficulty.Medium),
        S(WeirdCategory,
            "Your shadow has achieved sentience and wants independence.",
            Difficulty.Medium),
        S(WeirdCategory,
            "All cats have secretly been running society this whole time. They reveal themselves.",
            Difficulty.Easy),
        S(WeirdCategory,
            "Inanimate objects are now mildly sentient. They're judging you.",
            Difficulty.Medium),

        // ── SOCIAL ────────────────────────────────────────────────────────────
        S(SocialCategory,
            "You've just realized you've been mispronouncing someone's name for three years.",
            Difficulty.Easy),
        S(SocialCategory,
            "You sent a text meant for your friend to your boss. It was unflattering.",
            Difficulty.Easy),
        S(SocialCategory,
            "You've waved back at someone who was waving at the person behind you. Very awkwardly.",
            Difficulty.Easy),
        S(SocialCategory,
            "You're at a party and someone keeps trying to hug you but you're not a hugger.",
            Difficulty.Easy),
        S(SocialCategory,
            "Your mom finds your secret social media account and starts commenting on everything.",
            Difficulty.Medium),
    ];

    private static ICard S(string category, string scenario, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>RATE YOUR SURVIVAL CHANCES:</b>\n\n" +
            scenario + "\n\n" +
            "<b>1 = Dead instantly  ·  5 = Thriving</b>\n\n" +
            "Vote privately, then explain your strategy or why you'd perish spectacularly.",
            d, category);
}
