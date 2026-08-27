using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Would You Know? — a couples game about knowing your partner's history.
///
/// How to play:
///   1. One partner describes a moment from your shared history (first date, argument, funny moment, milestone).
///   2. The other partner answers: what happened next, what was said, who suggested it, etc.
///   3. The describing partner reveals the actual answer.
///   4. Points for accuracy and for how close the guess was.
///
/// Cards cover different relationship moments: how you met, first kiss, silly arguments,
/// shared adventures, secrets revealed, adoption/marriage decisions, and more. Works for
/// any couple — dating, married, long-term, new. The fun is in discovering which memories
/// your partner has crystal clear and which they've completely rewritten.
///
/// Great for anniversary nights or couples therapy with laughs.
/// </summary>
public sealed class WouldYouKnowMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Would You Know?";
    /// <inheritdoc />
    public override string Description =>
        "Describe a moment from your relationship. Can your partner guess what happened next?";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Guessed";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [WouldYouKnowCardBank.HowWeMetCategory] = "#42A5F5",
            [WouldYouKnowCardBank.FirstTimesCategory] = "#EC407A",
            [WouldYouKnowCardBank.SillyMomentsCategory] = "#FFCA28",
            [WouldYouKnowCardBank.ArgumentsCategory] = "#EF5350",
            [WouldYouKnowCardBank.DecisionsCategory] = "#AB47BC",
            [WouldYouKnowCardBank.AdventuresCategory] = "#66BB6A",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        WouldYouKnowCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => WouldYouKnowCardBank.All;
}

/// <summary>Built-in card bank for Would You Know. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class WouldYouKnowCardBank
{
    internal const string HowWeMetCategory = "How We Met";
    internal const string FirstTimesCategory = "First Times";
    internal const string SillyMomentsCategory = "Silly Moments";
    internal const string ArgumentsCategory = "Arguments";
    internal const string DecisionsCategory = "Decisions";
    internal const string AdventuresCategory = "Adventures";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── HOW WE MET ────────────────────────────────────────────────────────
        Q(HowWeMetCategory,
            "Tell the story of the moment you first met. Partner: can you guess what you were wearing that day?",
            Difficulty.Hard),
        Q(HowWeMetCategory,
            "Describe where we were when I first asked you out (or when you knew it was heading that way). Partner: guess what I said.",
            Difficulty.Hard),
        Q(HowWeMetCategory,
            "Tell the story of our first conversation. Partner: what did I say that made you think 'okay, this person is interesting'?",
            Difficulty.Hard),
        Q(HowWeMetCategory,
            "Remember the moment you realized this was actually going to be a thing between us? Partner: when exactly did you realize it?",
            Difficulty.Hard),

        // ── FIRST TIMES ───────────────────────────────────────────────────────
        Q(FirstTimesCategory,
            "Describe where we were for our first kiss. Partner: what made you realize I was about to kiss you?",
            Difficulty.Hard),
        Q(FirstTimesCategory,
            "Tell me about the first time I said I love you. Partner: where were we, and what was I actually trying to say?",
            Difficulty.Hard),
        Q(FirstTimesCategory,
            "Describe the first time you met my family. Partner: what was your first impression of them?",
            Difficulty.Hard),
        Q(FirstTimesCategory,
            "Tell the story of the first time we had a real argument. Partner: what were we actually arguing about?",
            Difficulty.Hard),
        Q(FirstTimesCategory,
            "Describe our first trip together. Partner: what moment on that trip made you happiest?",
            Difficulty.Hard),

        // ── SILLY MOMENTS ────────────────────────────────────────────────────
        Q(SillyMomentsCategory,
            "Tell me about that time I did something really embarrassing. Partner: how much did that affect what you thought of me?",
            Difficulty.Medium),
        Q(SillyMomentsCategory,
            "Describe a time we were being ridiculous together. Partner: how often do you think about that moment and smile?",
            Difficulty.Medium),
        Q(SillyMomentsCategory,
            "Tell me about the funniest misunderstanding we've had. Partner: what did you think I meant at the time?",
            Difficulty.Hard),
        Q(SillyMomentsCategory,
            "Describe the messiest situation we've gotten ourselves into together. Partner: what was the moment you realized how bad it was?",
            Difficulty.Medium),

        // ── ARGUMENTS ────────────────────────────────────────────────────────
        Q(ArgumentsCategory,
            "Tell me about the worst argument we've ever had. Partner: what were you actually angry about?",
            Difficulty.Hard),
        Q(ArgumentsCategory,
            "Describe the argument that made you wonder if we'd make it. Partner: were you thinking the same thing?",
            Difficulty.Hard),
        Q(ArgumentsCategory,
            "Tell me about the stupidest thing we've argued about. Partner: did you actually care, or were you arguing about something else?",
            Difficulty.Hard),
        Q(ArgumentsCategory,
            "Describe the first time you truly forgave me for hurting you. Partner: what did I do to earn that forgiveness?",
            Difficulty.Hard),

        // ── DECISIONS ────────────────────────────────────────────────────────
        Q(DecisionsCategory,
            "Tell me about the moment we decided to move in together. Partner: what was your biggest worry at the time?",
            Difficulty.Medium),
        Q(DecisionsCategory,
            "Describe the day you knew you wanted to propose/get married. Partner: what gave it away that I was going to say yes?",
            Difficulty.Hard),
        Q(DecisionsCategory,
            "Tell me about the biggest risk we've taken together. Partner: at what point did you stop being terrified?",
            Difficulty.Medium),
        Q(DecisionsCategory,
            "Describe when we made a major life decision together (move, job, family planning). Partner: what was your biggest doubt?",
            Difficulty.Hard),

        // ── ADVENTURES ────────────────────────────────────────────────────────
        Q(AdventuresCategory,
            "Tell me about the best trip we've taken. Partner: what moment on that trip meant the most to you?",
            Difficulty.Medium),
        Q(AdventuresCategory,
            "Describe an adventure that scared you but I convinced you to do it. Partner: were you scared too?",
            Difficulty.Medium),
        Q(AdventuresCategory,
            "Tell me about the time something went completely wrong while we were doing something fun. Partner: did you blame me?",
            Difficulty.Medium),
        Q(AdventuresCategory,
            "Describe a moment from our relationship where you felt truly happy just being alive. Partner: did I know that's how you felt?",
            Difficulty.Hard),
    ];

    private static ICard Q(string category, string prompt, Difficulty d) =>
        StandardCard.Create(
            category,
            prompt +
            "\n\n<b>Describing partner:</b> Tell the full story or give context clues. Don't give away the answer.\n\n" +
            "<b>Other partner:</b> Write down your best guess. Then the describing partner reveals the truth.",
            d, category);
}
