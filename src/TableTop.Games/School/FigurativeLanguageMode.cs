using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Figurative Language — identify, explain, and create figures of speech.
///
/// Each card presents a sentence or short extract. The active player must:
///   1. Name the figure of speech.
///   2. Explain the effect it creates.
///   3. Bonus: create an original example of the same device.
///
/// Scoring: 1 pt for naming, 1 pt for explaining, +1 bonus for creating.
/// Group adjudicates. Discussion encouraged.
///
/// Figures covered: simile, metaphor, personification, hyperbole, irony,
/// sarcasm, alliteration, onomatopoeia, oxymoron, paradox, pathetic fallacy,
/// synecdoche, euphemism, anaphora, juxtaposition.
/// </summary>
public sealed class FigurativeLanguageMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Figurative Language";
    /// <inheritdoc />
    public override string Description =>
        "Name the figure of speech, explain its effect, earn a bonus for creating your own. Grade 6 English.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ Named + explained (+2)";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "→ Next card";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [FigurativeLanguageCardBank.ComparisonCategory] = "#42A5F5",
            [FigurativeLanguageCardBank.SoundCategory] = "#66BB6A",
            ["Exaggeration"] = "#FFCA28",
            [FigurativeLanguageCardBank.ContrastCategory] = "#EC407A",
            [FigurativeLanguageCardBank.AtmosphereCategory] = "#AB47BC",
            [FigurativeLanguageCardBank.VoiceCategory] = "#26C6DA",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 2);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        FigurativeLanguageCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => FigurativeLanguageCardBank.All;
}

/// <summary>Built-in card bank for FigurativeLanguage. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class FigurativeLanguageCardBank
{
    internal const string ComparisonCategory = "Comparison";
    internal const string SoundCategory = "Sound";
    internal const string ContrastCategory = "Contrast";
    internal const string AtmosphereCategory = "Atmosphere";
    internal const string VoiceCategory = "Voice";

    private const string Deck = "Figurative Language";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)

            // ── EASY: the core eight — every student knows these ─────────────────
            .Category(ComparisonCategory)
            .Card("\"Her voice was music to his ears.\"",
                "Read aloud: <b>\"Her voice was music to his ears.\"</b>\n\n" +
                "1️⃣  Name the figure of speech.\n" +
                "2️⃣  Is this a simile or a metaphor? How do you tell the difference?\n\n" +
                "🌟 Bonus: Write your own metaphor about a person's laugh or smile.", Difficulty.Easy)
            .Card("\"As brave as a lion.\"",
                "Read aloud: <b>\"As brave as a lion.\"</b>\n\n" +
                "1️⃣  Name the figure of speech.\n" +
                "2️⃣  What effect does comparing a person to a lion create?\n\n" +
                "🌟 Bonus: Write a simile for something that is very fast.", Difficulty.Easy)
            .Category(VoiceCategory)
            .Card("\"The wind whispered through the trees.\"",
                "Read aloud: <b>\"The wind whispered through the trees.\"</b>\n\n" +
                "1️⃣  Name the figure of speech.\n" +
                "2️⃣  Why might a writer choose to give the wind a human action?\n\n" +
                "🌟 Bonus: Write a sentence where rain does something a person would do.", Difficulty.Easy)
            .Category(SoundCategory)
            .Card("\"The crash, bang, and clatter of the kitchen.\"",
                "Read aloud: <b>\"The crash, bang, and clatter of the kitchen.\"</b>\n\n" +
                "1️⃣  Name the figure of speech.\n" +
                "2️⃣  How do these word choices affect what the reader hears in their mind?\n\n" +
                "🌟 Bonus: Write two onomatopoeic words for water.", Difficulty.Easy)
            .Category("Exaggeration")
            .Card("\"I've told you a million times!\"",
                "Read aloud: <b>\"I've told you a million times!\"</b>\n\n" +
                "1️⃣  Name the figure of speech.\n" +
                "2️⃣  Why do writers use this device? What would be lost by saying 'many times'?\n\n" +
                "🌟 Bonus: Write a hyperbole about hunger or tiredness.", Difficulty.Easy)
            .Category(SoundCategory)
            .Card("\"Peter Piper picked a peck of pickled peppers.\"",
                "Read aloud: <b>\"Peter Piper picked a peck of pickled peppers.\"</b>\n\n" +
                "1️⃣  Name the figure of speech.\n" +
                "2️⃣  What sound effect is created? When might a writer use this deliberately?\n\n" +
                "🌟 Bonus: Write a three-word alliterative phrase using the letter S.", Difficulty.Easy)
            .Category(ContrastCategory)
            .Card("\"'What lovely weather,' she said, as the rain soaked her coat.\"",
                "Read aloud: <b>\"'What lovely weather,' she said, as the rain soaked her coat.\"</b>\n\n" +
                "1️⃣  Name the figure of speech.\n" +
                "2️⃣  How do you tell irony from sarcasm?\n\n" +
                "🌟 Bonus: Give a real-life example of irony (not sarcasm).", Difficulty.Easy)
            .Card("\"A deafening silence fell over the room.\"",
                "Read aloud: <b>\"A deafening silence fell over the room.\"</b>\n\n" +
                "1️⃣  Name the figure of speech.\n" +
                "2️⃣  Why do these two words seem to contradict each other? What effect does the combination create?\n\n" +
                "🌟 Bonus: Write your own oxymoron and explain what it means.", Difficulty.Easy)

            // ── MEDIUM: extended effects, context-dependent identification ─────────
            .Category(AtmosphereCategory)
            .Card("\"The darkness crept through every crack in the house.\"",
                "Read aloud: <b>\"The darkness crept through every crack in the house.\"</b>\n\n" +
                "1️⃣  Name the figure of speech.\n" +
                "2️⃣  What atmosphere does this create? How would the sentence feel without the personification?\n\n" +
                "🌟 Bonus: Write a sentence using pathetic fallacy to suggest sadness.", Difficulty.Medium)
            .Category(ComparisonCategory)
            .Card("\"Success is a journey, not a destination.\"",
                "Read aloud: <b>\"Success is a journey, not a destination.\"</b>\n\n" +
                "1️⃣  Name the figure of speech.\n" +
                "2️⃣  Explain what the comparison means — what is being said about how we should think about success?\n\n" +
                "🌟 Bonus: Create your own metaphor for failure.", Difficulty.Medium)
            .Card("\"She's not the brightest crayon in the box.\"",
                "Read aloud: <b>\"She's not the brightest crayon in the box.\"</b>\n\n" +
                "1️⃣  Name the figure of speech.\n" +
                "2️⃣  Is this an example of irony, idiom, or metaphor? Explain your reasoning.\n\n" +
                "🌟 Bonus: Rewrite it as a direct simile.", Difficulty.Medium)
            .Category(VoiceCategory)
            .Card("\"All hands on deck!\"",
                "Read aloud: <b>\"All hands on deck!\"</b>\n\n" +
                "1️⃣  What figure of speech uses a part of something to represent the whole?\n" +
                "2️⃣  What does 'hands' represent here? Why use this figure of speech?\n\n" +
                "🌟 Bonus: Give two more examples of synecdoche from everyday language.", Difficulty.Medium)
            .Category(AtmosphereCategory)
            .Card("\"The fog comes on little cat feet. / It sits looking over harbor and city / on silent haunches.\"",
                "Read the poem fragment aloud:\n<b>\"The fog comes on little cat feet.\nIt sits looking over harbor and city\non silent haunches.\"</b>  — Carl Sandburg\n\n" +
                "1️⃣  Name the main figure of speech.\n" +
                "2️⃣  What qualities of fog does comparing it to a cat emphasise?\n\n" +
                "🌟 Bonus: What is the effect of using very short, quiet words throughout?", Difficulty.Medium)
            .Category(VoiceCategory)
            .Card("\"He passed away peacefully.\"",
                "Read aloud: <b>\"He passed away peacefully.\"</b>\n\n" +
                "1️⃣  Name the figure of speech (a mild word used instead of a harsh one).\n" +
                "2️⃣  Why do we use this device in everyday language? Give two more examples.\n\n" +
                "🌟 Bonus: In what situations might avoiding euphemism be more honest or respectful?", Difficulty.Medium)
            .Category(ContrastCategory)
            .Card("\"To err is human; to forgive, divine.\"",
                "Read aloud: <b>\"To err is human; to forgive, divine.\"</b>  — Alexander Pope\n\n" +
                "1️⃣  Name the structural figure of speech (using parallel structures for contrast).\n" +
                "2️⃣  What is the effect of placing 'human' and 'divine' in parallel?\n\n" +
                "🌟 Bonus: Write your own antithesis using 'to... is...; to... is...' structure.", Difficulty.Medium)
            .Category(VoiceCategory)
            .Card("\"We shall fight on the beaches, we shall fight on the landing grounds, we shall fight in the fields.\"",
                "Read aloud: <b>\"We shall fight on the beaches, we shall fight on the landing grounds, we shall fight in the fields.\"</b>  — Churchill\n\n" +
                "1️⃣  Name the figure of speech (repetition of words at the start of clauses).\n" +
                "2️⃣  What emotional effect does this repetition create?\n\n" +
                "🌟 Bonus: Write three sentences using anaphora to create urgency.", Difficulty.Medium)

            // ── HARD: layers of meaning, extended and complex devices ─────────────
            .Category(ContrastCategory)
            .Card("\"It was the best of times, it was the worst of times.\"",
                "Read aloud: <b>\"It was the best of times, it was the worst of times.\"</b>  — Dickens\n\n" +
                "1️⃣  Name both the structural device and the figure of speech.\n" +
                "2️⃣  What does the paradox tell us about the historical period Dickens is describing?\n\n" +
                "🌟 Bonus: What novel does this open? What is its historical setting?", Difficulty.Hard)
            .Category(AtmosphereCategory)
            .Card("\"The curtain of night fell over the city, strangling the last breath of daylight.\"",
                "Read aloud: <b>\"The curtain of night fell over the city, strangling the last breath of daylight.\"</b>\n\n" +
                "1️⃣  Name every figure of speech you can identify (there are at least three).\n" +
                "2️⃣  What mood does this sentence create? How do the specific verbs contribute?\n\n" +
                "🌟 Bonus: Rewrite it to create a warm, positive atmosphere using similar devices.", Difficulty.Hard)
            .Category(ContrastCategory)
            .Card("\"Water, water, everywhere, nor any drop to drink.\"",
                "Read aloud: <b>\"Water, water, everywhere, nor any drop to drink.\"</b>  — Coleridge\n\n" +
                "1️⃣  Name the structural device and the figure of speech.\n" +
                "2️⃣  How does this line capture the speaker's torment more powerfully than saying 'we were surrounded by salt water'?\n\n" +
                "🌟 Bonus: What poem is this from? What is the figure of speech in its title 'The Ancient Mariner'?", Difficulty.Hard)
            .Card("\"I must be cruel only to be kind.\"",
                "Read aloud: <b>\"I must be cruel only to be kind.\"</b>  — Shakespeare, Hamlet\n\n" +
                "1️⃣  Name the figure of speech (an apparent contradiction that reveals a deeper truth).\n" +
                "2️⃣  Explain what Hamlet means. In what circumstances might someone say this in real life?\n\n" +
                "🌟 Bonus: Name the difference between a paradox and an oxymoron.", Difficulty.Hard)

            // ── EXTREME: extended literary analysis, multi-device ─────────────────
            .Category(ComparisonCategory)
            .Card("\"Full fathom five thy father lies; / Of his bones are coral made.\"",
                "Read aloud: <b>\"Full fathom five thy father lies;\nOf his bones are coral made.\"</b>  — Shakespeare, The Tempest\n\n" +
                "1️⃣  Name every figure of speech across both lines (at least three).\n" +
                "2️⃣  What transformation is described? How does the imagery affect our attitude toward death?\n\n" +
                "🌟 Bonus: Shakespeare wrote this as a song. How do the sound devices (alliteration, etc.) make it memorable?", Difficulty.Extreme)
            .Card("\"Hope is the thing with feathers / That perches in the soul.\"",
                "Read aloud: <b>\"Hope is the thing with feathers\nThat perches in the soul.\"</b>  — Emily Dickinson\n\n" +
                "1️⃣  Name the extended figure of speech Dickinson uses throughout this poem.\n" +
                "2️⃣  What qualities of a bird does she use to describe hope? Why is a bird an effective choice?\n\n" +
                "🌟 Bonus: Write the opening two lines of your own extended metaphor, choosing a different abstract noun (e.g. grief, love, fear).", Difficulty.Extreme)

            .Build();
}