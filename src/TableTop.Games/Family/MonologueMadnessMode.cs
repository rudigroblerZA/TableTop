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
    public override string Name => "Monologue Madness";
    /// <inheritdoc />
    public override string Description =>
        "You have 60 seconds. Improvise a speech on this topic. Go.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Spoke";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Pass";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [MonologueMadnessCardBank.PoliticsCategory] = "#42A5F5",
            [MonologueMadnessCardBank.PhilosophyCategory] = "#AB47BC",
            [MonologueMadnessCardBank.RidiculousCategory] = "#EC407A",
            [MonologueMadnessCardBank.CorporateCategory] = "#FFA726",
            [MonologueMadnessCardBank.ConfessionsCategory] = "#66BB6A",
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
    internal const string PoliticsCategory = "Politics";
    internal const string PhilosophyCategory = "Philosophy";
    internal const string RidiculousCategory = "Ridiculous";
    internal const string CorporateCategory = "Corporate";
    internal const string ConfessionsCategory = "Confessions";

    private const string Deck = "Monologue Madness";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)
        // ── POLITICS ──────────────────────────────────────────────────────────
            .Category(PoliticsCategory)
            .Card(PoliticsCategory, Body("You are campaigning to be elected Mayor of Pigeons. What's your opening statement?"), Difficulty.Medium)
            .Card(PoliticsCategory, Body("You are a senator defending why socks disappear in the dryer."), Difficulty.Medium)
            .Card(PoliticsCategory, Body("Give a presidential address about why pizza is a vegetable."), Difficulty.Easy)
            .Card(PoliticsCategory, Body("You're running for Town Council on a platform of mandatory nap time."), Difficulty.Easy)
            .Card(PoliticsCategory, Body("Deliver a speech convincing everyone that birds aren't real."), Difficulty.Hard)

        // ── PHILOSOPHY ───────────────────────────────────────────────────────
            .Category(PhilosophyCategory)
            .Card(PhilosophyCategory, Body("Give a TED Talk on the Secret Life of Household Objects."), Difficulty.Medium)
            .Card(PhilosophyCategory, Body("Explain the meaning of life to a confused potato."), Difficulty.Hard)
            .Card(PhilosophyCategory, Body("Defend the philosophical importance of doing absolutely nothing."), Difficulty.Medium)
            .Card(PhilosophyCategory, Body("Give an opening statement as if you're accepting a Nobel Prize for Sleeping."), Difficulty.Easy)
            .Card(PhilosophyCategory, Body("Present your argument: Procrastination is Actually a Superpower."), Difficulty.Easy)

        // ── RIDICULOUS ────────────────────────────────────────────────────────
            .Category(RidiculousCategory)
            .Card(RidiculousCategory, Body("You are a tour guide showing tourists around your kitchen."), Difficulty.Easy)
            .Card(RidiculousCategory, Body("Deliver a eulogy for your favorite pair of socks."), Difficulty.Medium)
            .Card(RidiculousCategory, Body("Give a survival speech as if you're stuck on a deserted island with only cheese."), Difficulty.Medium)
            .Card(RidiculousCategory, Body("You are a sports commentator narrating someone eating a sandwich."), Difficulty.Easy)
            .Card(RidiculousCategory, Body("Give a dramatic opening monologue as a Victorian ghost haunting a gas station."), Difficulty.Hard)
            .Card(RidiculousCategory, Body("Convince us why you should be the next Bachelor/Bachelorette. (You're a houseplant.)"), Difficulty.Medium)

        // ── CORPORATE ────────────────────────────────────────────────────────
            .Category(CorporateCategory)
            .Card(CorporateCategory, Body("You're presenting the quarterly report. Business is chaos. Stay professional."), Difficulty.Medium)
            .Card(CorporateCategory, Body("Give a motivational speech to your team about synergizing paradigm shifts."), Difficulty.Hard)
            .Card(CorporateCategory, Body("Present your innovative startup idea: An app for something completely useless."), Difficulty.Medium)
            .Card(CorporateCategory, Body("Deliver a safety briefing at an office where everything is slightly wrong."), Difficulty.Medium)
            .Card(CorporateCategory, Body("Give a professional apology for something you definitely did."), Difficulty.Easy)

        // ── CONFESSIONS ───────────────────────────────────────────────────────
            .Category(ConfessionsCategory)
            .Card(ConfessionsCategory, Body("Confess to something you definitely didn't do, and defend it passionately."), Difficulty.Hard)
            .Card(ConfessionsCategory, Body("You are finally revealing your true feelings about pineapple on pizza."), Difficulty.Easy)
            .Card(ConfessionsCategory, Body("Admit to something everyone suspects but nobody's said out loud."), Difficulty.Medium)
            .Card(ConfessionsCategory, Body("Give your controversial opinion about something everyone cares about."), Difficulty.Hard)
            .Card(ConfessionsCategory, Body("Confess to being secretly in love with something inanimate."), Difficulty.Medium)

            .Build();

    private static string Body(string topic) =>
        "<b>60-SECOND MONOLOGUE</b>\n\n" +
        topic + "\n\n" +
        "<b>GO:</b> No preparation. No notes. No stopping. Improvise for the full 60 seconds.\n\n" +
        "Everyone else votes: who was funniest, most convincing, or most unhinged?";
}
