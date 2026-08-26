using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Family Stories — collaborative oral storytelling for all ages.
///
/// The active player reads the card, which gives a story opening or constraint.
/// Then the group builds a story together:
///   • Each person adds one sentence going around the circle.
///   • No one can repeat what was just said.
///   • The story must end within three rounds of the table.
///
/// Card types:
///   Opening    — a vivid first line to continue
///   Constraint — a rule that must be obeyed while building the story
///   Twist      — an element that must be added to whatever story the group is in
///   Solo       — one person builds and tells a complete short story solo (60 seconds)
///
/// Age-inclusive: every type of contribution is valid. Younger children can
/// add simple sentences; adults add complexity. The story belongs to everyone.
/// </summary>
public sealed class FamilyStoriesMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Family Stories";
    /// <inheritdoc />
    public override string Description =>
        "Build a story together — one sentence at a time. Openings, twists, and solo challenges.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "→ Story done";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel     => "⤳ Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Opening"]    = "#42A5F5",
            ["Constraint"] = "#FFCA28",
            ["Twist"]      = "#EC407A",
            ["Solo"]       = "#AB47BC",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        FamilyStoriesCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => FamilyStoriesCardBank.All;
}

/// <summary>Built-in card bank for FamilyStories. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class FamilyStoriesCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── OPENINGS ─────────────────────────────────────────────────────────

        O("The Smallest Dragon",
          "🌟 <b>Build a story together — one sentence each, around the circle.</b>\n\n" +
          "Start here:\n\n<b>\"The dragon was the smallest one anyone had ever seen — about the size of a hamster — and it had accidentally moved into the kitchen.\"</b>\n\n" +
          "Three rounds of the table to finish it."),

        O("The Wrong House",
          "🌟 <b>Build a story together — one sentence each, around the circle.</b>\n\n" +
          "Start here:\n\n<b>\"When they arrived at number 47, they immediately knew they had knocked on completely the wrong door.\"</b>"),

        O("The Letter",
          "🌟 <b>Build a story together — one sentence each, around the circle.</b>\n\n" +
          "Start here:\n\n<b>\"The letter had been in the drawer for fifty years. Tonight, finally, someone opened it.\"</b>"),

        O("The Incredibly Good Day",
          "🌟 <b>Build a story together — one sentence each, around the circle.</b>\n\n" +
          "Start here:\n\n<b>\"It started as the best day of the year — and then it got even better.\"</b>\n\n" +
          "The story must stay genuinely positive all the way through."),

        O("The Dog That Could Talk",
          "🌟 <b>Build a story together — one sentence each, around the circle.</b>\n\n" +
          "Start here:\n\n<b>\"The dog had always been able to talk. The family had simply never asked him anything important enough before.\"</b>"),

        O("The Last Cake",
          "🌟 <b>Build a story together — one sentence each, around the circle.</b>\n\n" +
          "Start here:\n\n<b>\"There was one slice of cake left. Five people wanted it. The negotiations began.\"</b>"),

        O("The Map Under the Floor",
          "🌟 <b>Build a story together — one sentence each, around the circle.</b>\n\n" +
          "Start here:\n\n<b>\"When they pulled up the old carpet, they found a map drawn on the floorboards underneath. It appeared to show this house — but with an extra room that didn't exist.\"</b>"),

        O("The Visitor",
          "🌟 <b>Build a story together — one sentence each, around the circle.</b>\n\n" +
          "Start here:\n\n<b>\"The visitor arrived exactly as promised — three hundred years late.\"</b>"),

        O("The Machine That Remembered Everything",
          "🌟 <b>Build a story together — one sentence each, around the circle.</b>\n\n" +
          "Start here:\n\n<b>\"The machine in the garage had been there so long no one could remember where it came from. Today it switched itself on.\"</b>"),

        O("The Competition",
          "🌟 <b>Build a story together — one sentence each, around the circle.</b>\n\n" +
          "Start here:\n\n<b>\"The competition was the most ridiculous anyone had ever seen — and somehow every person in the family had entered without telling the others.\"</b>"),

        O("The Enormous Sandwich",
          "🌟 <b>Build a story together — one sentence each, around the circle.</b>\n\n" +
          "Start here:\n\n<b>\"Nobody could explain where the sandwich had come from. It was approximately the size of a door. And it smelled incredible.\"</b>"),

        O("The Night the Clocks Stopped",
          "🌟 <b>Build a story together — one sentence each, around the circle.</b>\n\n" +
          "Start here:\n\n<b>\"At exactly midnight, every clock in the house stopped at the same moment. The next morning, nobody could agree on what time it actually was.\"</b>"),

        // ── CONSTRAINTS ──────────────────────────────────────────────────────

        C("No Names",
          "🔒 <b>RULE: No one may use any character's name during the story.</b>\n\n" +
          "Build a story together — one sentence each. Any character can be introduced but must be referred to only by description or 'they'.\n\n" +
          "Group picks a story opening between themselves. Begin."),

        C("Every Sentence Ends with a Question",
          "🔒 <b>RULE: Every sentence must end with a question.</b>\n\n" +
          "Build a story together — one sentence each, around the circle. Each sentence must end with a genuine question that the next person must answer in their sentence.\n\n" +
          "One person starts the story. Begin."),

        C("Every Sentence, Someone New Arrives",
          "🔒 <b>RULE: Each sentence must introduce a new character.</b>\n\n" +
          "Build a story together. Each person's sentence must introduce at least one named character who wasn't in the story before.\n\n" +
          "See how many characters you can weave in before the story collapses.",
          Difficulty.Medium),

        C("No One May Use the Letter E",
          "🔒 <b>RULE: No word in any sentence may contain the letter E.</b>\n\n" +
          "Build a story together — one sentence each. Anyone who uses a word with the letter E in it must start their sentence again.\n\n" +
          "Group picks a starting idea. Begin.",
          Difficulty.Hard),

        C("The Story Must Keep Getting Better",
          "🔒 <b>RULE: Each sentence must make the situation better for the characters than the previous sentence did.</b>\n\n" +
          "No reversals, no new problems. The story can only improve. It must end in the most wonderful way possible.",
          Difficulty.Medium),

        C("Past Tense Only",
          "🔒 <b>RULE: The entire story must be told in the past tense.</b>\n\n" +
          "No present tense. No future tense. Build a story together — one sentence each. Anyone who uses a non-past-tense verb starts again.",
          Difficulty.Medium),

        C("All Characters Are Household Objects",
          "🔒 <b>RULE: Every character in the story is a household object.</b>\n\n" +
          "No people, no animals. Build the entire story with objects as characters — but they must behave with genuine personality and motivation.\n\n" +
          "Group picks three objects to star in it. Begin.",
          Difficulty.Medium),

        // ── TWISTS ────────────────────────────────────────────────────────────

        TW("It Starts to Rain — Indoors",
           "⚡ <b>TWIST!</b> Add this to the story you're currently building — or start a new one:\n\n" +
           "<b>It begins to rain inside the building. Just a light drizzle at first.</b>\n\n" +
           "Continue the story from here."),

        TW("Someone Falls Asleep",
           "⚡ <b>TWIST!</b> Add this to the story you're currently building — or start a new one:\n\n" +
           "<b>One character falls completely and instantly asleep. No one can wake them up.</b>\n\n" +
           "Continue the story from here."),

        TW("A Time Limit",
           "⚡ <b>TWIST!</b> Add this to the story you're currently building — or start a new one:\n\n" +
           "<b>Someone announces that there are exactly seven minutes left before something very important happens — but won't say what.</b>\n\n" +
           "Continue the story from here."),

        TW("The Map Was Wrong",
           "⚡ <b>TWIST!</b> Add this to the story you're currently building — or start a new one:\n\n" +
           "<b>The map — or plan, or instructions — turns out to be completely wrong. They are in entirely the wrong place.</b>\n\n" +
           "Continue the story from here."),

        TW("It Was a Dream — But Not Quite",
           "⚡ <b>TWIST!</b> Add this to the story you're currently building — or start a new one:\n\n" +
           "<b>One character wakes up. But they find one object from the dream in their hand.</b>\n\n" +
           "Continue the story from here."),

        TW("The Unexpected Expert",
           "⚡ <b>TWIST!</b> Add this to the story you're currently building — or start a new one:\n\n" +
           "<b>The youngest or smallest person in the story turns out to be the world's leading expert in exactly what they need.</b>\n\n" +
           "Continue the story from here."),

        // ── SOLO CHALLENGES ──────────────────────────────────────────────────

        S("The Three-Object Story",
          "⏱ <b>SOLO — 60 seconds</b>\n\n" +
          "Look around the room. Pick <b>three objects</b> at random.\n\n" +
          "Tell a complete story in 60 seconds that uses all three objects as important plot elements.\n\n" +
          "The story must have a beginning, a middle, and an end."),

        S("The Bedtime Story",
          "⏱ <b>SOLO — 90 seconds</b>\n\n" +
          "Tell the most comforting bedtime story you can invent on the spot.\n\n" +
          "It must be designed to send someone to sleep. It must be about something completely ordinary made to sound magical.\n\n" +
          "The group rates it out of 10 for restfulness.",
          Difficulty.Medium),

        S("The Scary Story That Isn't",
          "⏱ <b>SOLO — 90 seconds</b>\n\n" +
          "Tell a story that starts in the most frightening way possible — and then has the most anti-climactic ending you can invent.\n\n" +
          "The build-up must be genuinely tense. The ending must be completely harmless.",
          Difficulty.Medium),

        S("The Moral of the Story",
          "⏱ <b>SOLO — 90 seconds</b>\n\n" +
          "Announce a ridiculous moral first — something like: 'the moral of this story is: always carry an umbrella when visiting a library in a thunderstorm.'\n\n" +
          "Then tell a complete story that leads to exactly that moral.",
          Difficulty.Hard),

        S("The News Story",
          "⏱ <b>SOLO — 60 seconds</b>\n\n" +
          "Report a completely made-up news story as if you are a live TV correspondent at the scene.\n\n" +
          "It must be local, specific, and completely absurd.\n\n" +
          "Group rates your credibility out of 10.",
          Difficulty.Medium),
    ];

    private static ICard O(string title, string text, Difficulty d = Difficulty.Easy) =>
        StandardCard.Create(title, text, d, "Opening");

    private static ICard C(string title, string text, Difficulty d = Difficulty.Easy) =>
        StandardCard.Create(title, text, d, "Constraint");

    private static ICard TW(string title, string text, Difficulty d = Difficulty.Easy) =>
        StandardCard.Create(title, text, d, "Twist");

    private static ICard S(string title, string text, Difficulty d = Difficulty.Easy) =>
        StandardCard.Create(title, text, d, "Solo");
}