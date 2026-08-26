using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Future Us — a forward-looking conversation game for couples.
///
/// Where most couples' games ask about the past or the present, this one is about
/// what you're building together. Cards move through four horizons:
///   Soon      — the next year: trips, habits, small shared goals
///   Someday    — the medium term: home, work, lifestyle, money
///   Big Picture — the long arc: values, legacy, who you want to become together
///   Dream      — pure imagination: the no-limits version of your life together
///
/// How to play:
///   1. Read the card aloud.
///   2. Both partners answer — out loud, honestly.
///   3. Then ask the follow-up underneath: "How close are our answers?"
///
/// Not a quiz and not a test. The point is to discover where your visions of the
/// future already line up — and where you've never actually asked.
/// </summary>
public sealed class FutureUsMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Future Us";
    /// <inheritdoc />
    public override string Description =>
        "A forward-looking conversation for couples — from next year's plans to your wildest shared dreams.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Shared";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Pass";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Soon"] = "#66BB6A",
            ["Someday"] = "#42A5F5",
            ["Big Picture"] = "#AB47BC",
            ["Dream"] = "#EC407A",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        FutureUsCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => FutureUsCardBank.All;
}

/// <summary>Built-in card bank for Future Us. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class FutureUsCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── SOON (next year) ─────────────────────────────────────────────────
        Card("Soon", "What is one thing you'd love for us to do together in the next twelve months that we've never done before?",
            "Did you both name the same kind of thing — adventure, rest, or learning?", Difficulty.Easy),
        Card("Soon", "If we could build one new shared habit this year, what should it be?",
            "Who would find it easier to keep — and how could the other help?", Difficulty.Easy),
        Card("Soon", "Where in the world should we go on our next trip, and why there?",
            "Was your partner's answer a surprise, or exactly what you expected?", Difficulty.Easy),
        Card("Soon", "What's one small thing we could do every week that would make us feel closer?",
            "Have we ever actually tried it? If not — why not?", Difficulty.Easy),
        Card("Soon", "What's something you're hoping changes for the better for us this year?",
            "Is it something we control, or something we just have to weather together?", Difficulty.Medium),
        Card("Soon", "What's one skill you'd like to learn together in the next year?",
            "Would you encourage each other, or would you both be hopeless?", Difficulty.Easy),
        Card("Soon", "If we had an extra £500, what would you want to spend it on together?",
            "How far apart were your answers?", Difficulty.Easy),
        Card("Soon", "What's one thing we haven't tried together that you think I'd enjoy?",
            "Does your partner agree — or have they been waiting for you to suggest it?", Difficulty.Medium),

        // ── SOMEDAY (medium term) ────────────────────────────────────────────
        Card("Someday", "Describe the home you picture us living in five years from now. Where is it, and what does it feel like?",
            "How similar were your two homes? What was different?", Difficulty.Medium),
        Card("Someday", "If money were no object but we still had to work, what work would each of us be doing?",
            "Did your answer for your partner match the one they gave for themselves?", Difficulty.Medium),
        Card("Someday", "What does a perfect ordinary weekend look like for us in a few years' time?",
            "Whose version was busier? Whose was quieter?", Difficulty.Medium),
        Card("Someday", "What's one thing we should start saving for now that future-us will thank us for?",
            "Are we actually doing it? What would it take to start?", Difficulty.Medium),
        Card("Someday", "How do you hope we handle money together as the years go on?",
            "Where do our instincts about money agree, and where do they pull apart?", Difficulty.Hard),
        Card("Someday", "What would you want your career to look like in five years, and where does our relationship fit in that picture?",
            "Is your partner's career vision one you knew, or was it new to hear?", Difficulty.Hard),
        Card("Someday", "Imagine us in five years — describe a typical Tuesday in your version.",
            "How many details match your partner's vision?", Difficulty.Medium),
        Card("Someday", "What's one life goal you have that you'd like us to pursue together?",
            "Is it something your partner knows you care about?", Difficulty.Medium),

        // ── BIG PICTURE (the long arc) ───────────────────────────────────────
        Card("Big Picture", "When we're old and looking back, what do you hope we'll be most proud of about our life together?",
            "Did you each name the same thing — or two halves of the same picture?", Difficulty.Hard),
        Card("Big Picture", "What value do you most want to define us as a couple — and are we living it now?",
            "If a friend described us, would they name that value too?", Difficulty.Hard),
        Card("Big Picture", "Who do you hope I'll have become in twenty years — and how can you help me get there?",
            "How does it feel to hear what your partner hopes for you?", Difficulty.Hard),
        Card("Big Picture", "What's something you never want us to lose, no matter how much our life changes?",
            "Is it something we have to actively protect — or does it take care of itself?", Difficulty.Hard),
        Card("Big Picture", "If we could be remembered for one thing as a couple, what should it be?",
            "Were your answers about each other, or about the world beyond us?", Difficulty.Hard),
        Card("Big Picture", "What does the best version of 'us' look like to you, when we're at our absolute best together?",
            "How often do you think we actually live that version?", Difficulty.Hard),
        Card("Big Picture", "What legacy do you want us to leave — not stuff, but the impact we've had?",
            "Would your partner have guessed that was important to you?", Difficulty.Hard),
        Card("Big Picture", "How do you want our story to feel — the way we told it as we get older?",
            "Is it a story of adventure, patience, discovery, or something else entirely?", Difficulty.Hard),

        // ── DREAM (no limits) ────────────────────────────────────────────────
        Card("Dream", "If we could live anywhere on Earth for one year with no consequences, where would we go and what would we do?",
            "How fast did you agree? What was the sticking point?", Difficulty.Medium),
        Card("Dream", "Invent the most ridiculous, wonderful tradition for us to start and keep forever. What is it?",
            "Could we actually start it this year — even as a joke?", Difficulty.Easy),
        Card("Dream", "If we won a life-changing amount of money tomorrow, what's the first thing we'd do together — not buy, do?",
            "Did either of us say something the other had never heard before?", Difficulty.Medium),
        Card("Dream", "Picture the best day of our future life together, start to finish. Describe it.",
            "How much of that day could we actually have, right now, this year?", Difficulty.Hard),
        Card("Dream", "If we could master one skill together over the next decade, what should it be?",
            "Whose idea would the other be most willing to commit to?", Difficulty.Medium),
        Card("Dream", "If you could design our ideal life with zero constraints, what would it look like?",
            "What's the biggest difference between that dream and our actual life right now?", Difficulty.Hard),
        Card("Dream", "If we could spend a whole month doing absolutely anything together, what would we choose?",
            "Is it somewhere you'd actually want to go, or pure fantasy?", Difficulty.Easy),
        Card("Dream", "What's the wildest adventure you can imagine us going on together?",
            "Would your partner actually do it if you asked, or were you just having fun?", Difficulty.Medium),
    ];

    private static ICard Card(string category, string prompt, string followUp, Difficulty d) =>
        StandardCard.Create(
            category,
            prompt + "\n\n<b>Then ask each other:</b> " + followUp,
            d, category);
}
