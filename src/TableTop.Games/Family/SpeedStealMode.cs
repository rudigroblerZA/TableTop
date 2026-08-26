using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Speed Steal — a rapid-fire game where you answer questions quickly and steal points from others.
///
/// How to play:
///   1. Active player reads their question and answers it (5 seconds max).
///   2. Other players can challenge: "I have a better answer!"
///   3. If they answer better/faster, they steal the point.
///   4. If they answer worse, they lose a point to the original answerer.
///   5. Next player's turn. Points cascade and flip constantly.
///
/// It's chaotic, loud, and fast. Questions are deliberately open-ended so better answers
/// are subjective: "What's your best meal?" Player A says "pizza", Player B says "my
/// grandma's lasagna on Sunday" — which is better? ARGUE!
///
/// Great for groups that love banter and quick thinking under pressure. Keeps everyone
/// engaged because you never know who's going to steal your point. Makes people COMMIT
/// to their answers and defend them.
/// </summary>
public sealed class SpeedStealMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Speed Steal";
    /// <inheritdoc />
    public override string Description =>
        "Answer quickly. Others challenge. Better answer = you steal their point.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Answered";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Personal"] = "#42A5F5",
            ["Creative"] = "#FFCA28",
            ["Preference"] = "#EC407A",
            ["Story"] = "#66BB6A",
            ["Opinion"] = "#AB47BC",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        SpeedStealCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => SpeedStealCardBank.All;
}

/// <summary>Built-in card bank for Speed Steal. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class SpeedStealCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── PERSONAL ──────────────────────────────────────────────────────────
        S("Personal",
            "What's your best meal ever?",
            "Others challenge with THEIR best meal. Better answer steals your point.",
            Difficulty.Easy),
        S("Personal",
            "Describe your perfect day in three words.",
            "Others try to beat your description. Judges decide: who wins?",
            Difficulty.Medium),
        S("Personal",
            "What skill do you wish you had?",
            "Others counter with a skill that's more impressive or funny.",
            Difficulty.Easy),
        S("Personal",
            "What's something nobody knows about you?",
            "Others challenge: reveal something more shocking or weird.",
            Difficulty.Hard),

        // ── CREATIVE ──────────────────────────────────────────────────────────
        S("Creative",
            "If you could invent one new thing, what would it be?",
            "Others pitch inventions that sound more useful or hilarious.",
            Difficulty.Medium),
        S("Creative",
            "Give your life a movie title.",
            "Others create titles that better describe your actual life.",
            Difficulty.Medium),
        S("Creative",
            "What's the worst superpower you could have?",
            "Others come up with even worse superpowers. Funniest wins.",
            Difficulty.Easy),
        S("Creative",
            "Describe a colour without naming it.",
            "Others describe it better or more creatively.",
            Difficulty.Medium),

        // ── PREFERENCE ────────────────────────────────────────────────────────
        S("Preference",
            "Mountains or beaches — and why?",
            "Others defend the opposite choice harder.",
            Difficulty.Easy),
        S("Preference",
            "What's your guilty pleasure that you're not guilty about?",
            "Others claim a more shameless pleasure.",
            Difficulty.Easy),
        S("Preference",
            "Coffee or tea — and what does it say about you?",
            "Others argue their choice reveals more about them.",
            Difficulty.Medium),
        S("Preference",
            "What would you eat if calories didn't exist?",
            "Others propose something more interesting or ridiculous.",
            Difficulty.Easy),

        // ── STORY ────────────────────────────────────────────────────────────
        S("Story",
            "Tell a story about your most embarrassing moment (3 sentences).",
            "Others challenge with a more embarrassing moment.",
            Difficulty.Hard),
        S("Story",
            "What's your biggest failure and why it was actually good?",
            "Others describe a failure with a better silver lining.",
            Difficulty.Hard),
        S("Story",
            "Describe a time someone surprised you.",
            "Others share a better/funnier surprise story.",
            Difficulty.Medium),
        S("Story",
            "What's the craziest thing you've ever done?",
            "Others counter with something crazier.",
            Difficulty.Hard),

        // ── OPINION ───────────────────────────────────────────────────────────
        S("Opinion",
            "Is cereal a soup? Defend your answer.",
            "Others argue the opposite position harder.",
            Difficulty.Easy),
        S("Opinion",
            "What's overrated that everyone loves?",
            "Others counter with something MORE overrated.",
            Difficulty.Easy),
        S("Opinion",
            "What's underrated that nobody appreciates?",
            "Others argue their choice is MORE underrated.",
            Difficulty.Medium),
        S("Opinion",
            "What's the most annoying thing about people?",
            "Others nominate something MORE annoying.",
            Difficulty.Easy),
    ];

    private static ICard S(string category, string question, string stealMechanic, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>SPEED STEAL QUESTION</b>\n\n" +
            question + "\n\n" +
            "<b>PLAY:</b>\n" +
            "1. Active player answers (5 seconds)\n" +
            "2. Others shout 'STEAL!' to challenge\n" +
            "3. " + stealMechanic + "\n" +
            "4. Group votes: better answer wins the point\n\n" +
            "<b>RULES:</b> Be fast. Be bold. Defend your answer. No half-measures.",
            d, category);
}
