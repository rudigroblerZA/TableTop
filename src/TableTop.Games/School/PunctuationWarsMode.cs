using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Punctuation Wars — place the marks and explain the rule, Grade 6.
///
/// Each card shows a sentence (or short passage) that is either:
///   • Completely unpunctuated — player must read it with correct phrasing
///     and name all punctuation marks needed and where they go.
///   • Wrongly punctuated — player must identify and fix the error(s).
///   • A rule card — player must state and explain the rule, then give an example.
///
/// Scoring:
///   Unpunctuated/Wrong: 1 pt per correct mark identified (max 3 per card).
///   Rule cards: 2 pts for stating + exemplifying the rule.
///
/// Categories: Apostrophe, Comma, Colon/Semicolon, Speech Marks, Sentence, Mixed.
/// </summary>
public sealed class PunctuationWarsMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Punctuation Wars";
    /// <inheritdoc />
    public override string Description =>
        "Read it right, place the marks, explain the rule. From apostrophes to semicolons.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ All marks correct (+3)";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "→ Next card";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [PunctuationWarsCardBank.ApostropheCategory] = "#42A5F5",
            [PunctuationWarsCardBank.CommaCategory] = "#66BB6A",
            [PunctuationWarsCardBank.ColonSemicolonCategory] = "#FFCA28",
            [PunctuationWarsCardBank.SpeechMarksCategory] = "#AB47BC",
            [PunctuationWarsCardBank.SentenceCategory] = "#EC407A",
            [PunctuationWarsCardBank.MixedCategory] = "#26C6DA",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 3);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        PunctuationWarsCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => PunctuationWarsCardBank.All;
}

/// <summary>Built-in card bank for PunctuationWars. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class PunctuationWarsCardBank
{
    internal const string ApostropheCategory = "Apostrophe";
    internal const string CommaCategory = "Comma";
    internal const string ColonSemicolonCategory = "Colon/Semicolon";
    internal const string SpeechMarksCategory = "Speech Marks";
    internal const string SentenceCategory = "Sentence";
    internal const string MixedCategory = "Mixed";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── EASY: single mark, clear rule ────────────────────────────────────

        P("its vs it's",
          ApostropheCategory, Difficulty.Easy,
          "✏️  <b>Fix the error:</b>\n\n" +
          "<b>\"The dog wagged it's tail and ran to it's owner.\"</b>\n\n" +
          "1️⃣  Find the two errors.\n" +
          "2️⃣  Explain the rule: when does <b>its</b> have an apostrophe, and when doesn't it?"),

        P("Possession singular",
          ApostropheCategory, Difficulty.Easy,
          "✏️  <b>Add punctuation:</b>\n\n" +
          "<b>\"The cats food was in the cats bowl near the cats basket.\"</b>\n\n" +
          "1️⃣  Add apostrophes in the correct places.\n" +
          "2️⃣  How many apostrophes does this sentence need?"),

        P("Possession plural",
          ApostropheCategory, Difficulty.Easy,
          "✏️  <b>Explain the difference:</b>\n\n" +
          "• <b>the dog's bone</b>  (one dog)\n" +
          "• <b>the dogs' bone</b>  (many dogs)\n\n" +
          "1️⃣  Where does the apostrophe go for a plural possessive ending in S?\n" +
          "2️⃣  Now write two sentences — one with a singular possessive, one with a plural possessive."),

        P("Contraction or possession?",
          ApostropheCategory, Difficulty.Easy,
          "✏️  <b>Identify each apostrophe:</b>\n\n" +
          "<b>\"I can't find the teacher's pen and I don't know where she's put it.\"</b>\n\n" +
          "1️⃣  How many apostrophes are in this sentence? Name each one.\n" +
          "2️⃣  For each: is it a <b>contraction</b> or a <b>possessive</b>?"),

        P("Comma in a list",
          CommaCategory, Difficulty.Easy,
          "✏️  <b>Add commas:</b>\n\n" +
          "<b>\"She packed a torch a sleeping bag three cereal bars a map and a compass.\"</b>\n\n" +
          "1️⃣  How many commas does this sentence need?\n" +
          "2️⃣  Should there be a comma before 'and'? Explain both sides."),

        P("Comma splice",
          CommaCategory, Difficulty.Easy,
          "✏️  <b>Fix the error:</b>\n\n" +
          "<b>\"It was raining, we stayed inside, nobody was happy.\"</b>\n\n" +
          "1️⃣  What is a comma splice?\n" +
          "2️⃣  Fix this sentence in <b>two different ways</b>."),

        P("Direct speech basics",
          SpeechMarksCategory, Difficulty.Easy,
          "✏️  <b>Add all punctuation:</b>\n\n" +
          "<b>\"Im not coming said James I dont feel well\"</b>\n\n" +
          "1️⃣  Write out the sentence with correct speech marks, commas, capital letters, and apostrophes.\n" +
          "2️⃣  How many punctuation marks did you add in total?"),

        P("Capital letters in sentences",
          SentenceCategory, Difficulty.Easy,
          "✏️  <b>Fix the capitalisation:</b>\n\n" +
          "<b>\"last tuesday, mr green told us that we would be studying shakespeare in january. he said we might even go to stratford-upon-avon.\"</b>\n\n" +
          "1️⃣  Name every word that needs a capital letter.\n" +
          "2️⃣  Give the rule for each category of word you capitalised."),

        P("Full stop, question mark, or exclamation?",
          SentenceCategory, Difficulty.Easy,
          "✏️  <b>Add the correct end punctuation:</b>\n\n" +
          "• <b>\"The exam starts at nine o'clock\"</b>\n" +
          "• <b>\"What time does the exam start\"</b>\n" +
          "• <b>\"I can't believe the exam is today\"</b>\n\n" +
          "1️⃣  Which mark ends each sentence? Why?\n" +
          "2️⃣  When should you NOT use an exclamation mark?"),

        // ── MEDIUM: multiple marks, subtler rules ─────────────────────────────

        P("Comma with subordinate clause",
          CommaCategory, Difficulty.Medium,
          "✏️  <b>Both sentences are correct — but explain why one needs a comma:</b>\n\n" +
          "• <b>\"Although she was tired, she continued walking.\"</b>\n" +
          "• <b>\"She continued walking although she was tired.\"</b>\n\n" +
          "1️⃣  When does a subordinate clause need a comma and when doesn't it?\n" +
          "2️⃣  Write one sentence with the clause at the start and one with it at the end."),

        P("Embedded clause commas",
          CommaCategory, Difficulty.Medium,
          "✏️  <b>Add punctuation:</b>\n\n" +
          "<b>\"The teacher who had been marking papers all evening gave back the tests without comment.\"</b>\n\n" +
          "1️⃣  Does this sentence need commas? Where?\n" +
          "2️⃣  What is the embedded clause? Could you remove it and still have a complete sentence?"),

        P("Colon to introduce",
          ColonSemicolonCategory, Difficulty.Medium,
          "✏️  <b>Explain when to use a colon:</b>\n\n" +
          "<b>\"You will need three things__ a pen, a ruler, and your textbook.\"</b>\n\n" +
          "1️⃣  What punctuation mark goes in the gap?\n" +
          "2️⃣  State the rule: what does a colon do that a comma cannot?\n" +
          "3️⃣  Write your own sentence using a colon to introduce a list."),

        P("Semicolon rule",
          ColonSemicolonCategory, Difficulty.Medium,
          "✏️  <b>Fix or confirm:</b>\n\n" +
          "<b>\"The match was brilliant; the crowd roared for twenty minutes.\"</b>\n\n" +
          "1️⃣  Is the semicolon used correctly here?\n" +
          "2️⃣  State the rule for using a semicolon.\n" +
          "3️⃣  Could you replace it with 'and'? What would change?"),

        P("Speech punctuation full",
          SpeechMarksCategory, Difficulty.Medium,
          "✏️  <b>Punctuate completely:</b>\n\n" +
          "<b>I think we should go now said Priya looking at the sky it doesnt look safe</b>\n\n" +
          "1️⃣  Write the passage with all correct punctuation.\n" +
          "2️⃣  Where does the comma go in relation to the closing speech marks?\n" +
          "3️⃣  Why does 'it' not start with a capital letter?"),

        P("New speaker, new line",
          SpeechMarksCategory, Difficulty.Medium,
          "✏️  <b>Reformat this passage correctly:</b>\n\n" +
          "<b>\"Are you coming?\" asked Finn. \"I'm not sure\" said Amara. \"It's getting dark.\" \"It's fine\" said Finn \"we have torches.\"</b>\n\n" +
          "1️⃣  How many paragraphs should this be? Why?\n" +
          "2️⃣  Find and fix the punctuation error in Finn's second line of speech."),

        P("Parenthetical commas vs brackets vs dashes",
          MixedCategory, Difficulty.Medium,
          "✏️  <b>Three ways to add extra information:</b>\n\n" +
          "Rewrite this sentence three times, adding the extra information in brackets, dashes, and commas:\n\n" +
          "<b>\"The prime minister delivered a speech in parliament.\"</b>\n" +
          "Extra information: <b>her longest yet</b>\n\n" +
          "1️⃣  Which version feels most formal? Which is most dramatic?"),

        // ── HARD: complex sentences, subtle errors, advanced rules ────────────

        P("Apostrophe with irregular plurals",
          ApostropheCategory, Difficulty.Hard,
          "✏️  <b>Explain the rule and fix the errors:</b>\n\n" +
          "• <b>\"The childrens' shoes were left at the door.\"</b>\n" +
          "• <b>\"The sheeps' wool was newly shorn.\"</b>\n" +
          "• <b>\"The womens' team won the tournament.\"</b>\n\n" +
          "1️⃣  What is the rule for irregular plural possessives?\n" +
          "2️⃣  Fix each sentence. Where exactly does the apostrophe go in each case?"),

        P("Colon vs semicolon decision",
          ColonSemicolonCategory, Difficulty.Hard,
          "✏️  <b>Choose the correct mark for each gap:</b>\n\n" +
          "• <b>\"She had one fear__ failure.\"</b>\n" +
          "• <b>\"She feared failure__ she had worked too hard to lose now.\"</b>\n" +
          "• <b>\"Three students passed__ Eli, Rosa, and James.\"</b>\n" +
          "• <b>\"She passed__ she was relieved.\"</b>\n\n" +
          "1️⃣  Colon or semicolon for each? State your reason."),

        P("The Oxford comma debate",
          CommaCategory, Difficulty.Hard,
          "✏️  <b>Discuss and decide:</b>\n\n" +
          "<b>\"I'd like to thank my parents, Beyoncé and God.\"</b>\n\n" +
          "1️⃣  What is wrong with this sentence as written?\n" +
          "2️⃣  How does adding the Oxford comma fix it?\n" +
          "3️⃣  Write two sentences where the Oxford comma changes the meaning completely."),

        P("Complex sentence with multiple clauses",
          MixedCategory, Difficulty.Hard,
          "✏️  <b>Add all missing punctuation:</b>\n\n" +
          "<b>\"Although the exam which had lasted three hours was finally over nobody left their seats until the examiner who was watching them carefully gave the signal\"</b>\n\n" +
          "1️⃣  Write out the sentence fully punctuated.\n" +
          "2️⃣  How many commas did you add? Explain the purpose of each one."),

        P("Dash vs hyphen vs ellipsis",
          MixedCategory, Difficulty.Hard,
          "✏️  <b>Name and distinguish:</b>\n\n" +
          "• <b>\"She paused — then continued.\"</b>  (em dash)\n" +
          "• <b>\"A well-known author visited.\"</b>  (hyphen)\n" +
          "• <b>\"I don't know... maybe?\"</b>  (ellipsis)\n\n" +
          "1️⃣  State the purpose of each mark.\n" +
          "2️⃣  Write one original sentence that uses all three correctly."),

        // ── EXTREME: full-passage punctuation, analysis ───────────────────────

        P("Unpunctuated passage",
          MixedCategory, Difficulty.Extreme,
          "✏️  <b>Punctuate this complete passage:</b>\n\n" +
          "<b>it was the kind of morning that makes you believe anything is possible the sun had barely risen when maya who hadnt slept much picked up her bag and said quietly to herself today is the day she walked to the door her mothers voice came from upstairs youre up early everything alright maya smiled and called back of course mum she thought to herself she wasnt sure everything was alright at all</b>\n\n" +
          "1️⃣  Write the passage fully punctuated.\n" +
          "2️⃣  How many punctuation marks did you add? List the types."),

        P("Find every error",
          MixedCategory, Difficulty.Extreme,
          "✏️  <b>Find ALL errors in this passage — there are 8:</b>\n\n" +
          "<b>\"Ive been waiting for you, said the old man, his voice quiet but firm. \"I know what you did\". Are'nt you going to explain. The boy looked down at his shoes, he couldnt think of a single thing to say.\"</b>\n\n" +
          "1️⃣  List every error with its line number.\n" +
          "2️⃣  Name the rule being broken for each one.\n" +
          "3️⃣  Write the passage correctly."),
    ];

    private static ICard P(string title, string category, Difficulty d, string desc) =>
        StandardCard.Create(title, desc, d, category);
}