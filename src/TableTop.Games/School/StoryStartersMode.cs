using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Story Starters — oral creative writing and collaborative storytelling for Grade 6.
///
/// Each card gives a story opening (or a constraint/challenge).
/// The active player speaks for 60–90 seconds continuing or starting the story.
/// The next player may then continue, or the group discusses and awards points.
///
/// Three card types:
///   STARTER — a vivid first line: player continues the story for 90 seconds.
///   CONSTRAINT — a challenge rule: player must continue while obeying the rule.
///   TWIST — an unexpected element dropped into an ongoing story.
///
/// Scoring: 2 pts for compelling delivery; 1 pt for completing the challenge.
/// Constraints/Twists are harder and worth 3 pts on completion.
/// </summary>
public sealed class StoryStartersMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Story Starters";
    /// <inheritdoc />
    public override string Description =>
        "Begin a story, add a twist, or tell for 90 seconds with a constraint. Creative writing out loud.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ Story told (+2)";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel     => "→ Next card";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Starter"]    = "#42A5F5",
            ["Constraint"] = "#FFCA28",
            ["Twist"]      = "#EC407A",
            ["Challenge"]  = "#AB47BC",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 2);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        StoryStartersCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => StoryStartersCardBank.All;
}

/// <summary>Built-in card bank for StoryStarters. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class StoryStartersCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── STARTERS: Easy — vivid, accessible first lines ────────────────────

        S("The Door at the End of the Garden",
          "Starter", Difficulty.Easy,
          "⏱ <b>90 seconds</b> — continue this story:\n\n" +
          "<b>\"The door had always been there, at the end of the garden, behind the old apple tree. Nobody ever talked about it. Nobody ever opened it. Until the morning Maya decided she would.\"</b>\n\n" +
          "Continue the story from here. Where does the door lead? What does Maya find?"),

        S("The Last Person on Earth",
          "Starter", Difficulty.Easy,
          "⏱ <b>90 seconds</b> — continue this story:\n\n" +
          "<b>\"Alex woke up on an ordinary Tuesday and realised, very slowly, that every other person on Earth had simply... gone.\"</b>\n\n" +
          "Continue. What does Alex do first? What do they find?"),

        S("The Letter",
          "Starter", Difficulty.Easy,
          "⏱ <b>90 seconds</b> — continue this story:\n\n" +
          "<b>\"Inside the old book was a letter. The handwriting was her grandmother's. But her grandmother had been dead for twenty years. And the date at the top of the letter was yesterday.\"</b>\n\n" +
          "What does the letter say? What does she do next?"),

        S("Best Day",
          "Starter", Difficulty.Easy,
          "⏱ <b>90 seconds</b> — continue this story:\n\n" +
          "<b>\"It started as the worst day of the year. By noon, it had become the best.\"</b>\n\n" +
          "You decide what happens in between. Make us believe both halves."),

        S("The Stowaway",
          "Starter", Difficulty.Easy,
          "⏱ <b>90 seconds</b> — continue this story:\n\n" +
          "<b>\"The spaceship had been travelling for three years when the captain heard the noise coming from Cargo Bay Seven — the one that was supposed to be empty.\"</b>\n\n" +
          "What made the noise? What happens next?"),

        S("The Gift",
          "Starter", Difficulty.Easy,
          "⏱ <b>90 seconds</b> — continue this story:\n\n" +
          "<b>\"The box arrived with no return address, no postage, and no explanation. Inside was exactly what she had always wanted — something she had never told anyone about.\"</b>\n\n" +
          "What is in the box? Where did it come from?"),

        S("The Map",
          "Starter", Difficulty.Easy,
          "⏱ <b>90 seconds</b> — continue this story:\n\n" +
          "<b>\"The map showed a place that didn't exist on any other map in the world. And someone had drawn a red circle around exactly where they were standing right now.\"</b>\n\n" +
          "What is marked on the map? What do they do?"),

        S("The Apology",
          "Starter", Difficulty.Easy,
          "⏱ <b>90 seconds</b> — continue this story:\n\n" +
          "<b>\"She had been practising the apology for days. But when she finally knocked on the door and it opened, the words completely disappeared.\"</b>\n\n" +
          "What had happened? What does she say instead?"),

        // ── STARTERS: Medium — more literary, require development ────────────

        S("The Interview",
          "Starter", Difficulty.Medium,
          "⏱ <b>90 seconds</b> — continue this story:\n\n" +
          "<b>\"The job interview was going perfectly until she asked the one question no one had ever asked before: 'What is the worst thing you have ever done?'\"</b>\n\n" +
          "Continue. What does the character answer? What happens next?"),

        S("The Photograph",
          "Starter", Difficulty.Medium,
          "⏱ <b>90 seconds</b> — continue this story:\n\n" +
          "<b>\"The photograph showed him standing in a street in a city he had never visited, in a year before he was born, wearing his own face.\"</b>\n\n" +
          "Continue the story. How do you explain it?"),

        S("The Last Library",
          "Starter", Difficulty.Medium,
          "⏱ <b>90 seconds</b> — continue this story:\n\n" +
          "<b>\"In the year when books were made illegal, the last library was hidden underground. Only twelve people knew where it was. Tonight, someone had told a thirteenth.\"</b>\n\n" +
          "Continue. What happens next? Who was the thirteenth person?"),

        S("Two Sides",
          "Starter", Difficulty.Medium,
          "⏱ <b>90 seconds</b> — tell a story with this rule:\n\n" +
          "Your character makes a choice. Tell the story <b>twice</b>: once for each possible outcome. Both outcomes must be equally believable.\n\n" +
          "The choice can be anything — small or huge. You decide what it is."),

        S("The Unreliable Narrator",
          "Starter", Difficulty.Medium,
          "⏱ <b>90 seconds</b> — tell a story from the point of view of a character who is <b>lying</b> or <b>mistaken</b> about something important.\n\n" +
          "The group must figure out what the narrator is getting wrong.\n\n" +
          "You decide the scenario, the lie, and the truth beneath it."),

        // ── CONSTRAINTS: follow the rule while telling ───────────────────────

        S("No Adjectives",
          "Constraint", Difficulty.Medium,
          "⏱ <b>60 seconds</b> — tell a story about any subject you choose.\n\n" +
          "<b>RULE: You may not use a single adjective.</b>\n\n" +
          "No describing words — only nouns, verbs, and adverbs.\n\n" +
          "The group listens for adjectives. One slip = no bonus point."),

        S("Start with the End",
          "Constraint", Difficulty.Medium,
          "⏱ <b>90 seconds</b> — tell a story.\n\n" +
          "<b>RULE: Begin with the final sentence, then work backwards.</b>\n\n" +
          "Your opening line is where the story ends. Then explain how you got there.\n\n" +
          "You decide the final sentence."),

        S("Every Sentence: New Character",
          "Constraint", Difficulty.Medium,
          "⏱ <b>60 seconds</b> — tell a story.\n\n" +
          "<b>RULE: Every new sentence must introduce a new named character.</b>\n\n" +
          "See how many characters you can weave in while keeping a coherent story.\n\n" +
          "Minimum: four characters in 60 seconds."),

        S("The Rule of Three",
          "Constraint", Difficulty.Medium,
          "⏱ <b>90 seconds</b> — tell a story.\n\n" +
          "<b>RULE: Every important thing in your story must happen exactly three times.</b>\n\n" +
          "Three characters, three attempts, three results — you decide how.\n\n" +
          "If you only use something once or twice, the group calls it out."),

        S("No Said",
          "Constraint", Difficulty.Hard,
          "⏱ <b>90 seconds</b> — tell a story that includes at least three lines of dialogue.\n\n" +
          "<b>RULE: You may never use the word 'said' or 'says'.</b>\n\n" +
          "You must use a different dialogue verb every time a character speaks.\n\n" +
          "The group tracks your verb choices — no repeats!"),

        S("All Senses",
          "Constraint", Difficulty.Hard,
          "⏱ <b>90 seconds</b> — tell a story about any scene.\n\n" +
          "<b>RULE: You must include at least one detail for each of the five senses:</b>\n" +
          "sight, sound, smell, touch, taste.\n\n" +
          "The group checks off each sense as you use it. Missed one = no bonus."),

        // ── TWISTS: add these to an existing story or start fresh ─────────────

        S("Unexpected Weather",
          "Twist", Difficulty.Easy,
          "⚡ <b>TWIST</b> — drop this into any story, or start a new one:\n\n" +
          "<b>Suddenly, impossibly, it began to snow indoors.</b>\n\n" +
          "Either add this to the story in progress, or begin a new story that explains it.\n\n" +
          "⏱ 60 seconds."),

        S("The Wrong Person",
          "Twist", Difficulty.Easy,
          "⚡ <b>TWIST</b> — drop this into any story, or start a new one:\n\n" +
          "<b>The message was meant for someone else entirely.</b>\n\n" +
          "Either add this to the story in progress, or begin a new story that explains it.\n\n" +
          "⏱ 60 seconds."),

        S("The Secret Room",
          "Twist", Difficulty.Medium,
          "⚡ <b>TWIST</b> — drop this into any story, or start a new one:\n\n" +
          "<b>Behind the bookcase was a room that hadn't existed this morning.</b>\n\n" +
          "Either add this to the story in progress, or begin a new story built around it.\n\n" +
          "⏱ 60 seconds."),

        S("The Villain Explains",
          "Twist", Difficulty.Medium,
          "⚡ <b>TWIST</b> — drop this into any story, or start a new one:\n\n" +
          "<b>The villain sat down and explained, very calmly and very reasonably, exactly why they were right.</b>\n\n" +
          "Tell the villain's side of the story. The group votes: are they right?\n\n" +
          "⏱ 90 seconds."),

        // ── CHALLENGES: harder, literary, analytical ─────────────────────────

        S("Change the Genre",
          "Challenge", Difficulty.Hard,
          "⏱ <b>90 seconds</b> — take a fairy tale you know (Cinderella, Little Red Riding Hood, Hansel & Gretel, etc.) and retell it in a <b>completely different genre</b>:\n\n" +
          "• Crime thriller  • Science fiction  • Dystopian  • Horror  • Comedy\n\n" +
          "You choose the fairy tale AND the genre."),

        S("The Unreliable Setting",
          "Challenge", Difficulty.Hard,
          "⏱ <b>90 seconds</b> — tell a story where the setting itself cannot be trusted.\n\n" +
          "The character thinks they are somewhere — but the reader can tell they're wrong.\n\n" +
          "Hint: The setting might be a dream, a memory, a simulation, or something else entirely. You decide."),

        S("The Six-Word Story",
          "Challenge", Difficulty.Hard,
          "No time limit — create a <b>complete story in exactly six words</b>.\n\n" +
          "The most famous: <b>\"For sale: baby shoes, never worn.\"</b>  — Ernest Hemingway\n\n" +
          "Criteria: it must have an implied beginning, middle and end.\n" +
          "The group votes on whether it works as a complete story.\n\n" +
          "🌟 Bonus: Explain what the story implies beyond the literal words."),

        S("Perspective Swap",
          "Challenge", Difficulty.Hard,
          "⏱ <b>90 seconds</b> — retell a famous story from a minor or unexpected perspective.\n\n" +
          "Examples: the third pig's builder, the wolf's defence lawyer, Juliet's nurse, the giant's wife.\n\n" +
          "You choose the story AND the perspective. Make the familiar feel completely new."),

        S("The Unreliable Memory",
          "Challenge", Difficulty.Extreme,
          "⏱ <b>90 seconds</b> — tell a true story (from your own life or a famous event) as if the narrator only half-remembers it.\n\n" +
          "Include at least three moments of genuine uncertainty:\n" +
          "<b>\"I think it was a Tuesday... or maybe a Wednesday.\"</b>\n" +
          "<b>\"She said something — I can't remember exactly what.\"</b>\n\n" +
          "The group discusses: does unreliable memory make the story more or less powerful?"),

        S("The Letter Never Sent",
          "Challenge", Difficulty.Extreme,
          "⏱ <b>90 seconds</b> — write and read a letter from one character to another — a letter that was <b>never sent</b>.\n\n" +
          "You choose the characters and the relationship. The letter must reveal something the speaker could never say out loud.\n\n" +
          "Criteria: Does it reveal character? Is there subtext beneath the words?\n\n" +
          "🌟 Bonus: What literary term describes the gap between what is said and what is meant?"),
    ];

    private static ICard S(string title, string category, Difficulty d, string desc) =>
        StandardCard.Create(title, desc, d, category);
}