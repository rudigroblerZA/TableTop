using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Restrictions;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Restrictions;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Memory Lane — shared relationship memories told from both perspectives.
///
/// Each card names a category of moment or milestone. Both players recall and
/// describe their memory of the same type of moment. The format reveals how
/// each person experienced the same relationship differently.
///
/// Three card types:
///   First Times  — the early relationship: first impressions, early discoveries
///   Milestones   — the arc of the relationship: decisions made, changes lived through
///   Hidden Views — what one person noticed that the other never knew they noticed
///
/// No scoring — the point is the comparing of memories.
/// Played as: draw a card, both recall independently (30 seconds silent thought),
/// then one shares first, then the other.
/// </summary>
public sealed class MemoryLaneMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Memory Lane";
    /// <inheritdoc />
    public override string Description =>
        "Each card: one memory, both your versions. How differently did you live the same moment?";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "→ Both shared";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "⤳ Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [MemoryLaneCardBank.FirstTimesCategory] = "#42A5F5",
            [MemoryLaneCardBank.MilestonesCategory] = "#66BB6A",
            [MemoryLaneCardBank.HiddenViewsCategory] = "#EC407A",
            [MemoryLaneCardBank.RightNowCategory] = "#FFCA28",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0); // conversation game

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        MemoryLaneCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => MemoryLaneCardBank.All;
}

/// <summary>Built-in card bank for MemoryLane. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class MemoryLaneCardBank
{
    internal const string FirstTimesCategory = "First Times";
    internal const string MilestonesCategory = "Milestones";
    internal const string HiddenViewsCategory = "Hidden Views";
    internal const string RightNowCategory = "Right Now";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build()
    {
        var couplesOnly = new CoupleOnlyRestriction();

        return
        [
            // ════════════════════════════════════════════════════════════════
            // FIRST TIMES — early relationship moments
            // ════════════════════════════════════════════════════════════════

            M("The First Time We Met",
              "Each of you describes your memory of the first time you met.\n\nWho went first? What did you notice first? What did you think?\n\nCompare versions — what did the other person not know you were thinking?",
              FirstTimesCategory, Difficulty.Easy, couplesOnly),

            M("The Moment I Thought It Might Be Real",
              "Each of you recalls the specific moment — not the general feeling, but the actual moment — when you thought this could be something serious.\n\nWas it the same moment for both of you?",
              FirstTimesCategory, Difficulty.Easy, couplesOnly),

            M("What I Told My Friends About You",
              "What did you tell your friends about your partner early on — before you knew where it was going?\n\nWhat were your exact words? Was it accurate?",
              FirstTimesCategory, Difficulty.Easy, couplesOnly),

            M("The First Fight",
              "Describe the first real argument you had as a couple — what it was about, how it ended, what you each took from it.\n\nDo you remember it the same way?",
              FirstTimesCategory, Difficulty.Medium, couplesOnly),

            M("The First Time I Felt Comfortable",
              "When did you first feel fully at ease with your partner — not performing, not on guard?\n\nDescribe the specific moment or period. Did you know at the time that you'd relaxed?",
              FirstTimesCategory, Difficulty.Easy, couplesOnly),

            M("The First Time I Was Afraid of Losing You",
              "When was the first time you felt afraid of losing your partner — not yet certain of them, worried they might not stay?\n\nDid you show it? Did they know?",
              FirstTimesCategory, Difficulty.Medium, couplesOnly),

            M("What I First Got Wrong About You",
              "Name one thing you assumed, predicted, or expected about your partner early on that turned out to be wrong.\n\nWas it a pleasant or an unpleasant surprise?",
              FirstTimesCategory, Difficulty.Easy, couplesOnly),

            M("The First Night You Spent Together",
              "Without details — just the feeling of it. What do you each remember? What did it mean to you at the time?",
              FirstTimesCategory, Difficulty.Medium,
              couplesOnly.And(new AdultOnlyRestriction())),

            M("The First Trip Together",
              "If you've travelled together: describe the first trip from your own point of view — what you noticed, what felt new, what you saw in your partner that you hadn't seen before.",
              FirstTimesCategory, Difficulty.Easy, couplesOnly),

            M("The First Time I Said I Love You",
              "Who said it first? What were the exact circumstances? How did the other person respond?\n\nTell both versions — the person who said it and the person who heard it.",
              FirstTimesCategory, Difficulty.Medium, couplesOnly),

            // ════════════════════════════════════════════════════════════════
            // MILESTONES — the arc of the relationship
            // ════════════════════════════════════════════════════════════════

            M("A Decision That Changed Everything",
              "Name a decision — yours, your partner's, or made together — that changed the shape of your relationship.\n\nDid you both see it as significant at the time, or only in hindsight?",
              MilestonesCategory, Difficulty.Medium, couplesOnly),

            M("The Hardest Period",
              "Name a period in your relationship that was genuinely hard — for one of you or both.\n\nEach describe it from your own perspective: what was hardest, what got you through.",
              MilestonesCategory, Difficulty.Hard, couplesOnly),

            M("The Thing We Built Together",
              "Name something — tangible or not — that you built together that you couldn't have built alone.\n\nHow do you each see your contribution to it?",
              MilestonesCategory, Difficulty.Medium, couplesOnly),

            M("The Time I Chose You Again",
              "Not the beginning — a specific later moment when you made a choice to stay, to recommit, to show up. Name it.\n\nDid your partner know that was happening for you?",
              MilestonesCategory, Difficulty.Hard, couplesOnly),

            M("The Loss We Shared",
              "If you've experienced a loss together — a person, a plan, a chapter of your lives — how did you each carry it?\n\nDid you grieve the same way, or differently?",
              MilestonesCategory, Difficulty.Hard, couplesOnly),

            M("A Moment of Pride",
              "Name a moment in your partner's life — not something they did for you — that made you quietly, genuinely proud.\n\nDid you tell them at the time?",
              MilestonesCategory, Difficulty.Medium, couplesOnly),

            M("When the Relationship Changed",
              "Name a point where the relationship shifted — became more serious, more comfortable, more complicated, or more certain.\n\nDid you both experience the shift at the same time?",
              MilestonesCategory, Difficulty.Hard, couplesOnly),

            M("Something I Wish Had Gone Differently",
              "Each of you names one moment or period in your relationship that you wish had gone differently — not a regret about the other person, but about your own response or handling.",
              MilestonesCategory, Difficulty.Hard, couplesOnly),

            M("The Memory I Return To",
              "What memory of your relationship do you return to most often — the one that feels like a representative version of what you have?\n\nAre you surprised by each other's choices?",
              MilestonesCategory, Difficulty.Medium, couplesOnly),

            // ════════════════════════════════════════════════════════════════
            // HIDDEN VIEWS — what one person noticed that the other didn't know
            // ════════════════════════════════════════════════════════════════

            M("Something I Notice About You",
              "Name something physical or behavioural that you have noticed about your partner — something specific they do that they probably don't know you've clocked.\n\nHow long have you been noticing it?",
              HiddenViewsCategory, Difficulty.Easy, couplesOnly),

            M("When You Don't Think I'm Looking",
              "Describe what your partner looks like when they're absorbed in something — reading, working, cooking, thinking. What do you see?\n\nHave you ever told them?",
              HiddenViewsCategory, Difficulty.Easy, couplesOnly),

            M("The Habit I Love That You Don't Know I Love",
              "Name something your partner does regularly — a habit, a phrase, a way of moving — that you secretly love and have never mentioned.",
              HiddenViewsCategory, Difficulty.Easy, couplesOnly),

            M("What I See When You're Sad",
              "Describe what your partner looks like when they're sad — specifically, the exact signs you've learned to read.\n\nDo they think you can tell?",
              HiddenViewsCategory, Difficulty.Hard, couplesOnly),

            M("The Version of You I Protect",
              "Is there something about your partner that you deliberately don't bring up — a sensitivity, a doubt, a fear — because you're protecting them from it?\n\nDoes protecting them from it actually help?",
              HiddenViewsCategory, Difficulty.Hard, couplesOnly),

            M("What I Think When You're Asleep",
              "What do you actually think when you're watching your partner sleep? What does it make you feel?\n\nName the last time you noticed it.",
              HiddenViewsCategory, Difficulty.Medium, couplesOnly),

            M("The Version of You Nobody Else Sees",
              "Describe a version of your partner that only you have access to — a side that friends, family, or colleagues don't see.",
              HiddenViewsCategory, Difficulty.Medium, couplesOnly),

            M("The Worry I Carry for You",
              "Is there something about your partner's life — health, happiness, work, a relationship they have — that you worry about privately and don't usually raise?\n\nWhy don't you raise it?",
              HiddenViewsCategory, Difficulty.Hard, couplesOnly),

            // ════════════════════════════════════════════════════════════════
            // RIGHT NOW — the present moment of the relationship
            // ════════════════════════════════════════════════════════════════

            M("Where We Are Tonight",
              "How would you describe where you are as a couple right now — not in general, not historically, but specifically tonight?\n\nIs that description the same for both of you?",
              RightNowCategory, Difficulty.Hard, couplesOnly),

            M("What I Want for Us This Year",
              "Each of you names one thing — one change, one experience, one shift — that you want for your relationship in the next twelve months.\n\nHave either of you said this before?",
              RightNowCategory, Difficulty.Medium, couplesOnly),

            M("The Thing I'm Carrying Tonight",
              "Is there something you arrived at this evening carrying — from your day, your week, your life right now — that I don't know about?\n\nTell me.",
              RightNowCategory, Difficulty.Hard, couplesOnly),

            M("What I Want You to Know Right Now",
              "Each of you says one true thing to the other — something you want them to know about how you feel right now, in this moment.\n\nNo context required. Just say it.",
              RightNowCategory, Difficulty.Extreme, couplesOnly),
        ];
    }

    private static ICard M(string title, string text, string category, Difficulty d, IRestriction restriction) =>
        StandardCard.Create(title, text, d, category, restriction: restriction);
}