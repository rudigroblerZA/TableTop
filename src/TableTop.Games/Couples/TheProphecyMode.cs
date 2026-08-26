using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// The Prophecy — tarot-inspired fortune telling meets personality quiz.
///
/// How to play:
///   1. One partner answers a hidden question about the other.
///   2. Based on their answer, a silly "prophecy" is read aloud.
///   3. The prophecy is hilariously cryptic and always somehow applies.
///   4. Both laugh. Points for the most absurd prophecy.
///
/// This is NOT meant to be accurate. It's a joke. The fortunes are intentionally vague,
/// theatrical, and ridiculous: "The stars suggest you will find love... with sandwich."
/// or "Your future contains three keys: mystery, chaos, and slightly regretted decisions."
///
/// Works for couples, close friends, or anyone who enjoys tarot parody. Great for
/// date night with a laugh. No actual mysticism involved, just vibes.
/// </summary>
public sealed class TheProphecyMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "The Prophecy";
    /// <inheritdoc />
    public override string Description =>
        "Answer a question. Receive a hilariously vague fortune. Laugh at the universe.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Prophesied";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Decline fate";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Love"] = "#EC407A",
            ["Career"] = "#42A5F5",
            ["Chaos"] = "#EF5350",
            ["Wisdom"] = "#AB47BC",
            ["Destiny"] = "#FFCA28",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        TheProphecyCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => TheProphecyCardBank.All;
}

/// <summary>Built-in card bank for The Prophecy. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class TheProphecyCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── LOVE ──────────────────────────────────────────────────────────────
        P("Love",
            "What's your partner's best quality?",
            "The stars decree: true love will find you... with someone named Kevin.",
            Difficulty.Easy),
        P("Love",
            "How would you rate your relationship on a scale of 1-10?",
            "The prophecy speaks: you will find eternal happiness... with tacos.",
            Difficulty.Easy),
        P("Love",
            "What's one thing you want more of in your relationship?",
            "The oracle has spoken: your soulmate is closer than you think... it's actually a very charming squirrel.",
            Difficulty.Medium),
        P("Love",
            "How do you know when your partner loves you?",
            "The universe whispers: romance will bloom in the most unexpected place... your local grocery store.",
            Difficulty.Easy),
        P("Love",
            "What would your ideal future together look like?",
            "Destiny calls: true love transcends time and space... and apparently also sanity.",
            Difficulty.Medium),
        P("Love",
            "How long have you been together?",
            "The stars are aligned: your love is written in the cosmos... in glitter and regret.",
            Difficulty.Easy),

        // ── CAREER ────────────────────────────────────────────────────────────
        P("Career",
            "What's your biggest dream job?",
            "Career prophecy: financial success awaits... if you become a professional nap consultant.",
            Difficulty.Medium),
        P("Career",
            "What skill do you wish you had?",
            "The fates have spoken: you will rise to great power... in the office chair.",
            Difficulty.Easy),
        P("Career",
            "What would your ideal work day look like?",
            "Prophecy: your career will soar higher than the eagles... if the eagles were procrastinating.",
            Difficulty.Medium),
        P("Career",
            "What's holding you back professionally?",
            "The oracle sees: tremendous success in your future... as a very motivated couch.",
            Difficulty.Medium),

        // ── CHAOS ────────────────────────────────────────────────────────────
        P("Chaos",
            "What's the most chaotic thing about your life?",
            "The universe warns: beware of three things — chaos, more chaos, and slightly worse chaos.",
            Difficulty.Easy),
        P("Chaos",
            "If you could restart your life, would you?",
            "Prophecy: you will live through five more ridiculous situations... tomorrow.",
            Difficulty.Easy),
        P("Chaos",
            "What's your biggest regret?",
            "The stars whisper: you will make worse decisions in the future. Congratulations.",
            Difficulty.Medium),
        P("Chaos",
            "What's one thing you've learned the hard way?",
            "Destiny speaks: the universe has already planned your next mistake... it's hilarious.",
            Difficulty.Easy),
        P("Chaos",
            "How do you handle stress?",
            "The prophecy states: you will eventually laugh about this... in about 5 years.",
            Difficulty.Medium),

        // ── WISDOM ────────────────────────────────────────────────────────────
        P("Wisdom",
            "What's the best advice you've ever received?",
            "The oracle intones: wisdom flows through you like a river... that's going the wrong direction.",
            Difficulty.Medium),
        P("Wisdom",
            "What do you know now that you wish you'd known before?",
            "The universe reveals: the key to happiness is knowing when to... stop reading prophecies.",
            Difficulty.Medium),
        P("Wisdom",
            "What's one thing you've figured out about life?",
            "Prophecy: you are wiser than you think... and also dumber than you admit.",
            Difficulty.Hard),
        P("Wisdom",
            "What does your gut usually tell you to do?",
            "The stars declare: trust your instincts... they've been wrong before and they will be again.",
            Difficulty.Medium),

        // ── DESTINY ───────────────────────────────────────────────────────────
        P("Destiny",
            "If you could see your future, would you want to?",
            "The fates declare: your destiny involves at least three surprises... none of them good.",
            Difficulty.Medium),
        P("Destiny",
            "What does your perfect life look like in 10 years?",
            "The oracle sees: you will achieve your dreams... after some truly embarrassing detours.",
            Difficulty.Medium),
        P("Destiny",
            "What's one thing you want to accomplish before you die?",
            "Prophecy: you will accomplish great things... mostly by accident.",
            Difficulty.Easy),
        P("Destiny",
            "How do you want to be remembered?",
            "The universe whispers: you will be remembered fondly... once everyone stops laughing.",
            Difficulty.Medium),

        // ── LOVE (more) ──────────────────────────────────────────────────────
        P("Love",
            "What's the most romantic thing your partner has ever done?",
            "The stars confirm: your love story will be adapted into a film... a low-budget one, straight to streaming.",
            Difficulty.Easy),
        P("Love",
            "What's a small habit of theirs you secretly adore?",
            "The oracle giggles: affection blooms in unexpected places... mostly the snack cupboard.",
            Difficulty.Easy),
        P("Love",
            "If your relationship were a weather forecast, what would it be?",
            "The prophecy declares: mostly sunny, with a 40% chance of bickering over the thermostat.",
            Difficulty.Medium),

        // ── CAREER (more) ────────────────────────────────────────────────────
        P("Career",
            "What's a compliment you wish you got more at work?",
            "The fates decree: recognition is coming... from a vending machine that finally accepts your card.",
            Difficulty.Medium),
        P("Career",
            "If you quit tomorrow, what would you actually do?",
            "The oracle foresees: a bold new career... as a professional overthinker, unpaid, full-time.",
            Difficulty.Easy),

        // ── CHAOS (more) ─────────────────────────────────────────────────────
        P("Chaos",
            "What's the weirdest thing currently in your bag or pockets?",
            "The universe confirms: that item will become surprisingly important... within the next 20 minutes.",
            Difficulty.Easy),
        P("Chaos",
            "What's a rule you constantly break for no good reason?",
            "The stars sigh: chaos is not a phase, it is your permanent personality setting.",
            Difficulty.Medium),
        P("Chaos",
            "What's the last thing that went wrong that was 100% your fault?",
            "Prophecy: this will happen again. The universe has already scheduled it.",
            Difficulty.Medium),

        // ── WISDOM (more) ─────────────────────────────────────────────────────
        P("Wisdom",
            "What's something you believed as a kid that turned out to be wrong?",
            "The oracle nods: further wrong beliefs are currently under construction. Stay tuned.",
            Difficulty.Medium),
        P("Wisdom",
            "What's the most useful thing you learned from a mistake?",
            "The stars affirm: your greatest teacher has always been, and will remain, poor planning.",
            Difficulty.Hard),

        // ── DESTINY (more) ────────────────────────────────────────────────────
        P("Destiny",
            "If a fortune teller gave you ONE true prediction, what would you want it to be?",
            "The universe regrets to inform you: that prediction is classified until further notice.",
            Difficulty.Medium),
        P("Destiny",
            "What's a sign you'd take as 'the universe telling you something'?",
            "The oracle confirms: the universe IS trying to tell you something. It's about the laundry.",
            Difficulty.Easy),
    ];

    private static ICard P(string category, string question, string prophecy, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>" + question + "</b>\n\n" +
            "(Write your answer privately — your partner doesn't see it)\n\n" +
            "<b>Your Prophecy:</b>\n\n" +
            prophecy,
            d, category);
}
