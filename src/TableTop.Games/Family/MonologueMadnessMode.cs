using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Monologue Madness — improvise speeches on absurd topics.
///
/// How to play:
///   1. The active player receives a topic card.
///   2. They have 60 seconds to deliver an improvised monologue on that topic.
///   3. No preparation, no notes, just pure creativity under pressure.
///   4. Everyone else votes: funniest, most convincing, or most unhinged.
///   5. Points go to the speaker if the audience votes them best.
///
/// Topics are intentionally absurd: "Why I should be elected Mayor of Pigeons",
/// "A TED Talk on the Secret Life of Socks", "Defense Closing Argument: Why
/// the Toilet Paper Goes Over, Not Under". Players must commit fully, stay in
/// character, and just keep talking even if they have no idea what's happening.
///
/// Great for confidence building, pure entertainment, and discovering who's an
/// actual comedian. Works for all ages. Embrace the chaos.
/// </summary>
public sealed class MonologueMadnessMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name        => "Monologue Madness";
    /// <inheritdoc />
    public override string Description =>
        "You have 60 seconds. Improvise a speech on this topic. Go.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Spoke";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel     => "Pass";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Politics"]    = "#42A5F5",
            ["Philosophy"]  = "#AB47BC",
            ["Ridiculous"]  = "#EC407A",
            ["Corporate"]   = "#FFA726",
            ["Confessions"] = "#66BB6A",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        MonologueMadnessCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => MonologueMadnessCardBank.All;
}

/// <summary>Built-in card bank for Monologue Madness. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class MonologueMadnessCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── POLITICS ──────────────────────────────────────────────────────────
        M("Politics",
            "You are campaigning to be elected Mayor of Pigeons. What's your opening statement?",
            Difficulty.Medium),
        M("Politics",
            "You are a senator defending why socks disappear in the dryer.",
            Difficulty.Medium),
        M("Politics",
            "Give a presidential address about why pizza is a vegetable.",
            Difficulty.Easy),
        M("Politics",
            "You're running for Town Council on a platform of mandatory nap time.",
            Difficulty.Easy),
        M("Politics",
            "Deliver a speech convincing everyone that birds aren't real.",
            Difficulty.Hard),

        // ── PHILOSOPHY ───────────────────────────────────────────────────────
        M("Philosophy",
            "Give a TED Talk on the Secret Life of Household Objects.",
            Difficulty.Medium),
        M("Philosophy",
            "Explain the meaning of life to a confused potato.",
            Difficulty.Hard),
        M("Philosophy",
            "Defend the philosophical importance of doing absolutely nothing.",
            Difficulty.Medium),
        M("Philosophy",
            "Give an opening statement as if you're accepting a Nobel Prize for Sleeping.",
            Difficulty.Easy),
        M("Philosophy",
            "Present your argument: Procrastination is Actually a Superpower.",
            Difficulty.Easy),

        // ── RIDICULOUS ────────────────────────────────────────────────────────
        M("Ridiculous",
            "You are a tour guide showing tourists around your kitchen.",
            Difficulty.Easy),
        M("Ridiculous",
            "Deliver a eulogy for your favorite pair of socks.",
            Difficulty.Medium),
        M("Ridiculous",
            "Give a survival speech as if you're stuck on a deserted island with only cheese.",
            Difficulty.Medium),
        M("Ridiculous",
            "You are a sports commentator narrating someone eating a sandwich.",
            Difficulty.Easy),
        M("Ridiculous",
            "Give a dramatic opening monologue as a Victorian ghost haunting a gas station.",
            Difficulty.Hard),
        M("Ridiculous",
            "Convince us why you should be the next Bachelor/Bachelorette. (You're a houseplant.)",
            Difficulty.Medium),

        // ── CORPORATE ────────────────────────────────────────────────────────
        M("Corporate",
            "You're presenting the quarterly report. Business is chaos. Stay professional.",
            Difficulty.Medium),
        M("Corporate",
            "Give a motivational speech to your team about synergizing paradigm shifts.",
            Difficulty.Hard),
        M("Corporate",
            "Present your innovative startup idea: An app for something completely useless.",
            Difficulty.Medium),
        M("Corporate",
            "Deliver a safety briefing at an office where everything is slightly wrong.",
            Difficulty.Medium),
        M("Corporate",
            "Give a professional apology for something you definitely did.",
            Difficulty.Easy),

        // ── CONFESSIONS ───────────────────────────────────────────────────────
        M("Confessions",
            "Confess to something you definitely didn't do, and defend it passionately.",
            Difficulty.Hard),
        M("Confessions",
            "You are finally revealing your true feelings about pineapple on pizza.",
            Difficulty.Easy),
        M("Confessions",
            "Admit to something everyone suspects but nobody's said out loud.",
            Difficulty.Medium),
        M("Confessions",
            "Give your controversial opinion about something everyone cares about.",
            Difficulty.Hard),
        M("Confessions",
            "Confess to being secretly in love with something inanimate.",
            Difficulty.Medium),
    ];

    private static ICard M(string category, string topic, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>60-SECOND MONOLOGUE</b>\n\n" +
            topic + "\n\n" +
            "<b>GO:</b> No preparation. No notes. No stopping. Improvise for the full 60 seconds.\n\n" +
            "Everyone else votes: who was funniest, most convincing, or most unhinged?",
            d, category);
}
