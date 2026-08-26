using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Vocabulary Builder — Grade 6 word knowledge game.
///
/// Each card shows a word. The active player must:
///   1. Give the <b>definition</b>.
///   2. Use it correctly in a <b>sentence</b>.
///   3. Bonus point: give a <b>synonym or antonym</b> as shown on the card.
///
/// Scoring: 1 pt per correct step (max 3 per card).
/// Group adjudicates whether each part is correct.
/// </summary>
public sealed class VocabularyBuilderMode : BaseGameModeDefinition, IFlowAwareMode
{
    /// <inheritdoc />
    public override string Name => "Vocabulary Builder";
    /// <inheritdoc />
    public override string Description =>
        "Define the word, use it in a sentence, and earn a bonus for a synonym. Grade 6 vocabulary.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ All three (+3)";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel     => "→ Next word";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Adjective"]  = "#26C6DA",
            ["Noun"]       = "#66BB6A",
            ["Verb"]       = "#FFCA28",
            ["Adverb"]     = "#EC407A",
            ["Academic"]   = "#AB47BC",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 3);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        VocabularyBuilderCardBank.All;

    /// <summary>Exposes the card bank for testing without a player list.</summary>
    public static IReadOnlyList<ICard> GetCards() => VocabularyBuilderCardBank.All;
}

/// <summary>Built-in card bank for VocabularyBuilder. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class VocabularyBuilderCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── Easy: common Grade 5–6 vocabulary ────────────────────────────────

        V("Benevolent",  "Adjective", Difficulty.Easy,
          "1️⃣  <b>Define:</b> BENEVOLENT\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for benevolent.",
          synonym: "kind / generous"),

        V("Persevere",   "Verb",      Difficulty.Easy,
          "1️⃣  <b>Define:</b> PERSEVERE\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an antonym for persevere.",
          synonym: "give up / quit"),

        V("Anxious",     "Adjective", Difficulty.Easy,
          "1️⃣  <b>Define:</b> ANXIOUS\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for anxious.",
          synonym: "worried / nervous"),

        V("Sufficient",  "Adjective", Difficulty.Easy,
          "1️⃣  <b>Define:</b> SUFFICIENT\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an antonym for sufficient.",
          synonym: "insufficient / inadequate"),

        V("Observe",     "Verb",      Difficulty.Easy,
          "1️⃣  <b>Define:</b> OBSERVE\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for observe.",
          synonym: "watch / notice"),

        V("Diligent",    "Adjective", Difficulty.Easy,
          "1️⃣  <b>Define:</b> DILIGENT\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an antonym for diligent.",
          synonym: "lazy / careless"),

        V("Crucial",     "Adjective", Difficulty.Easy,
          "1️⃣  <b>Define:</b> CRUCIAL\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for crucial.",
          synonym: "essential / vital"),

        V("Transparent", "Adjective", Difficulty.Easy,
          "1️⃣  <b>Define:</b> TRANSPARENT\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an antonym for transparent.",
          synonym: "opaque / hidden"),

        V("Eloquent",    "Adjective", Difficulty.Easy,
          "1️⃣  <b>Define:</b> ELOQUENT\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for eloquent.",
          synonym: "articulate / well-spoken"),

        V("Collaborate", "Verb",      Difficulty.Easy,
          "1️⃣  <b>Define:</b> COLLABORATE\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for collaborate.",
          synonym: "cooperate / work together"),

        // ── Medium: less common, curriculum vocabulary ────────────────────────

        V("Ambiguous",   "Adjective", Difficulty.Medium,
          "1️⃣  <b>Define:</b> AMBIGUOUS\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an antonym for ambiguous.",
          synonym: "clear / unambiguous"),

        V("Proliferate", "Verb",      Difficulty.Medium,
          "1️⃣  <b>Define:</b> PROLIFERATE\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for proliferate.",
          synonym: "multiply / spread"),

        V("Pensive",     "Adjective", Difficulty.Medium,
          "1️⃣  <b>Define:</b> PENSIVE\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for pensive.",
          synonym: "thoughtful / reflective"),

        V("Resilience",  "Noun",      Difficulty.Medium,
          "1️⃣  <b>Define:</b> RESILIENCE\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an antonym for resilience.",
          synonym: "fragility / weakness"),

        V("Hypocritical","Adjective", Difficulty.Medium,
          "1️⃣  <b>Define:</b> HYPOCRITICAL\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for hypocritical.",
          synonym: "two-faced / insincere"),

        V("Arbitrary",   "Adjective", Difficulty.Medium,
          "1️⃣  <b>Define:</b> ARBITRARY\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an antonym for arbitrary.",
          synonym: "systematic / reasoned"),

        V("Conjecture",  "Noun",      Difficulty.Medium,
          "1️⃣  <b>Define:</b> CONJECTURE\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for conjecture.",
          synonym: "speculation / guess"),

        V("Meticulous",  "Adjective", Difficulty.Medium,
          "1️⃣  <b>Define:</b> METICULOUS\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an antonym for meticulous.",
          synonym: "careless / sloppy"),

        V("Advocate",    "Verb/Noun", Difficulty.Medium,
          "1️⃣  <b>Define:</b> ADVOCATE (as a verb AND as a noun — different meanings!)\n2️⃣  Use each form in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for the noun form.",
          synonym: "supporter / champion"),

        V("Unprecedented","Adjective",Difficulty.Medium,
          "1️⃣  <b>Define:</b> UNPRECEDENTED\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for unprecedented.",
          synonym: "unheard-of / novel"),

        // ── Hard: academic and subject-specific vocabulary ────────────────────

        V("Juxtaposition","Academic", Difficulty.Hard,
          "1️⃣  <b>Define:</b> JUXTAPOSITION\n2️⃣  Use it in a sentence about literature or art.\n3️⃣  <b>Bonus:</b> Give a real example of juxtaposition in a book you know.",
          synonym: "(contrast placed side by side)"),

        V("Hegemony",    "Academic",  Difficulty.Hard,
          "1️⃣  <b>Define:</b> HEGEMONY\n2️⃣  Use it in a sentence about history or politics.\n3️⃣  <b>Bonus:</b> Give a real historical example.",
          synonym: "dominance / leadership"),

        V("Empirical",   "Academic",  Difficulty.Hard,
          "1️⃣  <b>Define:</b> EMPIRICAL\n2️⃣  Use it in a sentence about science.\n3️⃣  <b>Bonus:</b> Give an antonym and explain why it matters in science.",
          synonym: "theoretical / speculative"),

        V("Paradigm",    "Academic",  Difficulty.Hard,
          "1️⃣  <b>Define:</b> PARADIGM\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Explain what 'paradigm shift' means.",
          synonym: "model / framework"),

        V("Dichotomy",   "Academic",  Difficulty.Hard,
          "1️⃣  <b>Define:</b> DICHOTOMY\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an example of a dichotomy in real life.",
          synonym: "division / contrast"),

        // ── Extreme: truly challenging academic vocabulary ─────────────────────

        V("Solipsism",   "Academic",  Difficulty.Extreme,
          "1️⃣  <b>Define:</b> SOLIPSISM\n2️⃣  Use it correctly in a sentence.\n3️⃣  <b>Bonus:</b> In which academic field would you most likely encounter this word?",
          synonym: "(philosophy: only one's own mind exists)"),

        V("Epistemology","Academic",  Difficulty.Extreme,
          "1️⃣  <b>Define:</b> EPISTEMOLOGY\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> What is the epistemological question: 'How do we know what we know?' asking?",
          synonym: "(study of the nature of knowledge)"),

        V("Sycophantic", "Adjective", Difficulty.Extreme,
          "1️⃣  <b>Define:</b> SYCOPHANTIC\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> What noun relates to this adjective?",
          synonym: "flattering / obsequious"),

        V("Ostensibly",  "Adverb",    Difficulty.Extreme,
          "1️⃣  <b>Define:</b> OSTENSIBLY\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> What does it imply about reality vs appearance?",
          synonym: "apparently / seemingly"),
    ];

    private static ICard V(
        string word, string partOfSpeech, Difficulty d, string desc, string synonym) =>
        StandardCard.Create(word, desc, d, partOfSpeech);
}