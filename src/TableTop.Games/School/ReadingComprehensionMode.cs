using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Reading Comprehension — Grade 6 passage-and-question cards.
///
/// Each card contains a short passage (2–6 sentences) followed by a question.
/// The active player reads the passage aloud, then answers the question.
/// Others can discuss before the group awards points.
///
/// Question types: literal, inferential, vocabulary-in-context, author's purpose.
/// No time pressure — discussion is encouraged.
///
/// Scoring: 2 pts for a full answer; 1 pt for a partial answer.
/// </summary>
public sealed class ReadingComprehensionMode : BaseGameModeDefinition, IFlowAwareMode
{
    /// <inheritdoc />
    public override string Name => "Reading Comprehension";
    /// <inheritdoc />
    public override string Description =>
        "Read the passage, answer the question. Literal, inferential and vocabulary questions. Grade 6.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ Full answer (+2)";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel     => "→ Next passage";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Literal"]      = "#26C6DA",
            ["Inferential"]  = "#66BB6A",
            ["Vocabulary"]   = "#FFCA28",
            ["Author"]       = "#EC407A",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 2);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        ReadingComprehensionCardBank.All;

    /// <summary>Exposes the card bank for testing without a player list.</summary>
    public static IReadOnlyList<ICard> GetCards() => ReadingComprehensionCardBank.All;
}

/// <summary>Built-in card bank for ReadingComprehension. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class ReadingComprehensionCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── Easy: literal comprehension ───────────────────────────────────────

        P("The Stubborn Oak",
          "Literal", Difficulty.Easy,
          "An oak tree stood at the edge of the field, older than anyone in the village could remember. " +
          "Its roots spread wide and deep, gripping the earth like hands refusing to let go. " +
          "Storms had bent its branches and lightning had scarred its trunk, but still it stood.\n\n" +
          "<b>Question:</b> What evidence does the passage give that the oak tree has survived difficult conditions?"),

        P("The Market at Dawn",
          "Literal", Difficulty.Easy,
          "Before the sun had fully risen, the market was already alive with noise. " +
          "Vendors called out prices, crates scraped against cobblestones, and the smell of fresh bread drifted " +
          "from the baker's stall at the corner. A small girl clutched a coin and studied each stall carefully.\n\n" +
          "<b>Question:</b> List three details the passage gives about what the market was like at dawn."),

        P("Ice in the Antarctic",
          "Literal", Difficulty.Easy,
          "Antarctica holds about 70% of the world's fresh water, locked inside its vast ice sheets. " +
          "The ice can be up to 4.7 kilometres thick in some places. " +
          "Scientists study ice cores drilled from these sheets to understand Earth's climate history, " +
          "since each layer of ice contains trapped air from thousands of years ago.\n\n" +
          "<b>Question:</b> According to the passage, why do scientists study Antarctic ice cores?"),

        P("A Dog's Sense of Smell",
          "Literal", Difficulty.Easy,
          "A dog's nose contains about 300 million olfactory receptors, compared to just 6 million in humans. " +
          "This means dogs can detect smells at concentrations nearly 100,000 times lower than humans can. " +
          "Police and rescue services use this ability to track missing people, detect drugs, and even identify certain illnesses.\n\n" +
          "<b>Question:</b> Name two practical uses of a dog's sense of smell mentioned in the passage."),

        P("The Invention of the Printing Press",
          "Literal", Difficulty.Easy,
          "Johannes Gutenberg invented the printing press in the mid-15th century. " +
          "Before this, books had to be copied by hand, which made them rare and expensive. " +
          "The printing press made it possible to produce many copies quickly and cheaply, " +
          "helping ideas spread across Europe much faster than before.\n\n" +
          "<b>Question:</b> Why were books rare and expensive before Gutenberg's invention?"),

        // ── Medium: inferential questions ─────────────────────────────────────

        P("The Empty Chair",
          "Inferential", Difficulty.Medium,
          "Every morning, Mrs Patel set two cups on the kitchen table. " +
          "She had done this for forty years. She sat at her usual chair and read the paper, " +
          "not looking at the other cup, which remained untouched until she quietly poured it away.\n\n" +
          "<b>Question:</b> What do you think has happened in Mrs Patel's life? What clues tell you this? " +
          "What does her behaviour suggest about how she feels?"),

        P("The Last Forest",
          "Inferential", Difficulty.Medium,
          "The logging company's notice had been nailed to the oak near the river path for three weeks now. " +
          "The children from Northfield School walked past it every day on the way to their outdoor classroom, " +
          "but nobody had mentioned it aloud — as if saying nothing might make it go away.\n\n" +
          "<b>Question:</b> Why do you think the children don't mention the notice? " +
          "What does the passage suggest they might be feeling?"),

        P("Two Athletes",
          "Inferential", Difficulty.Medium,
          "Maya crossed the finish line first, arms raised. She turned and waited, watching the track. " +
          "A minute later, when Priya finally crossed — limping, face twisted — Maya was the first to run to her.\n\n" +
          "<b>Question:</b> What does Maya's action at the end tell us about her character? " +
          "Why might the author have chosen not to include any dialogue?"),

        P("The Tide Comes In",
          "Inferential", Difficulty.Medium,
          "Sandcastles take the most effort to build when the tide is furthest out. " +
          "Children work hard in the sun, adding towers and moats and flags. " +
          "Then, inevitably, the sea returns. Within minutes, the effort of hours is gone. " +
          "Yet the next low tide always brings new builders to the same shore.\n\n" +
          "<b>Question:</b> This passage could be read as a metaphor. What might the sandcastles represent? " +
          "What might the sea represent?"),

        P("Climate Migration",
          "Inferential", Difficulty.Medium,
          "In the Maldives, a country of 1,200 low-lying islands in the Indian Ocean, the government has been " +
          "purchasing land in other countries for decades. " +
          "Average elevation is just 1.8 metres above sea level. " +
          "Scientists estimate that sea levels could rise by as much as one metre by 2100.\n\n" +
          "<b>Question:</b> Why might the Maldives government be purchasing land in other countries? " +
          "The passage never states this directly — what can you infer?"),

        // ── Medium: vocabulary in context ─────────────────────────────────────

        P("The Negotiation",
          "Vocabulary", Difficulty.Medium,
          "After hours of talks, the two sides finally reached a <i>tentative</i> agreement. " +
          "Neither was entirely satisfied, but both recognised that a fragile deal was better than none. " +
          "The treaty would need to be ratified by both governments before it could take effect.\n\n" +
          "<b>Question:</b> What does 'tentative' mean as used in this passage? " +
          "What clues in the passage help you work out the meaning?"),

        P("The Prodigy",
          "Vocabulary", Difficulty.Medium,
          "At twelve, Maria was already considered a prodigy. " +
          "While other children her age were learning scales, she was composing her own sonatas. " +
          "Critics marvelled at the <i>precocious</i> talent that had emerged seemingly from nowhere.\n\n" +
          "<b>Question:</b> What does 'precocious' mean? How does the passage help you understand it?"),

        P("The Journalist",
          "Vocabulary", Difficulty.Medium,
          "The journalist's report was described by supporters as 'courageous investigative journalism', " +
          "but critics called it <i>sensationalist</i> — more interested in drama than in facts. " +
          "The truth, as usual, probably lay somewhere between the two extremes.\n\n" +
          "<b>Question:</b> What do you think 'sensationalist' means? " +
          "How does the word 'drama' in the same sentence help you?"),

        // ── Hard: author's purpose and technique ──────────────────────────────

        P("The Salesman",
          "Author", Difficulty.Hard,
          "\"This is not just a vacuum cleaner,\" he said, leaning forward as though sharing a secret. " +
          "\"This is a revolution. This is the end of dust. This is the beginning of a new era in your home.\" " +
          "Mrs Johnson nodded politely and quietly decided she would stick with her broom.\n\n" +
          "<b>Question:</b> What technique does the salesman use in his speech? " +
          "What does Mrs Johnson's reaction suggest about the author's view of persuasive language?"),

        P("The Warning Sign",
          "Author", Difficulty.Hard,
          "There are 1.3 billion cars in the world. " +
          "Each one burns, on average, 1.6 litres of fuel for every 16 kilometres driven. " +
          "That is roughly 130 billion litres of petrol burned every day. " +
          "The atmosphere is thin — only about 12 kilometres deep. " +
          "Consider both of those numbers together.\n\n" +
          "<b>Question:</b> Why does the author end with the instruction 'Consider both of those numbers together' " +
          "rather than stating a conclusion? What effect does this have on the reader?"),

        P("Two Openings",
          "Author", Difficulty.Hard,
          "Opening A: 'The war began on a Tuesday. Nobody expected it to last seven years.'\n\n" +
          "Opening B: 'It was a warm and pleasant Tuesday when the news came through — shocking, of course, " +
          "but the sun continued to shine and the birds continued to sing as they always had.'\n\n" +
          "<b>Question:</b> Both passages describe the same event. " +
          "Compare the effect of each opening. " +
          "What different emotions does each create in the reader, and how?"),

        // ── Extreme: complex literary analysis ────────────────────────────────

        P("The Map",
          "Inferential", Difficulty.Extreme,
          "The old map on the library wall showed a city that no longer existed. " +
          "Streets had been renamed, buildings demolished, and whole neighbourhoods replaced. " +
          "Yet people still came to stare at it, tracing the old routes with their fingers, " +
          "whispering names of places as though calling them might bring them back.\n\n" +
          "<b>Question:</b> What might the map symbolise? " +
          "Discuss at least two possible interpretations, referring closely to the language of the passage."),

        P("The Unreliable Garden",
          "Author", Difficulty.Extreme,
          "My grandmother insisted the roses were red. I remember them as yellow. " +
          "My brother, who claims not to remember the garden at all, nevertheless " +
          "described the smell of lavender unprompted when asked about childhood summers.\n\n" +
          "<b>Question:</b> What does this passage suggest about the nature of memory? " +
          "What technique is the author using by presenting contradictory accounts? " +
          "How might this be relevant in a wider literary context?"),

        P("The Photograph",
          "Inferential", Difficulty.Extreme,
          "She found it at the back of a drawer: herself at seven, grinning, holding a fish. " +
          "She had no memory of the fish, or the river, or whoever had taken the picture. " +
          "She kept it anyway — this evidence of a self she could not access — " +
          "and placed it on the mantelpiece where she would see it every morning.\n\n" +
          "<b>Question:</b> Why might she keep a photograph of a moment she cannot remember? " +
          "What does the phrase 'evidence of a self she could not access' suggest about identity? " +
          "How does the final detail (placing it to see every morning) add to the passage's meaning?"),
    ];

    private static ICard P(string title, string category, Difficulty d, string desc) =>
        StandardCard.Create(title, desc, d, category);
}