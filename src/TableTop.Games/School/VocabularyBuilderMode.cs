using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
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
    public override string SkipLabel => "→ Next word";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [VocabularyBuilderCardBank.AdjectiveCategory] = "#26C6DA",
            [VocabularyBuilderCardBank.NounCategory] = "#66BB6A",
            [VocabularyBuilderCardBank.VerbCategory] = "#FFCA28",
            ["Adverb"] = "#EC407A",
            [VocabularyBuilderCardBank.AcademicCategory] = "#AB47BC",
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

/// <summary>
/// Built-in card bank for Vocabulary Builder, authored with
/// <see cref="CardDeckBuilder"/>'s fluent DSL. Each card's category is its
/// part of speech, so the chain switches <see cref="CardDeckBuilder.Category"/>
/// as the words do rather than staying in tier order.
/// </summary>
public static class VocabularyBuilderCardBank
{
    internal const string AdjectiveCategory = "Adjective";
    internal const string NounCategory = "Noun";
    internal const string VerbCategory = "Verb";
    internal const string AcademicCategory = "Academic";

    private const string Deck = "Vocabulary Builder";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)

            // ── Easy: common Grade 5–6 vocabulary ────────────────────────────────
            .Category(AdjectiveCategory)
            .Card("Benevolent", "1️⃣  <b>Define:</b> BENEVOLENT\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for benevolent.", Difficulty.Easy)
            .Category(VerbCategory)
            .Card("Persevere", "1️⃣  <b>Define:</b> PERSEVERE\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an antonym for persevere.", Difficulty.Easy)
            .Category(AdjectiveCategory)
            .Card("Anxious", "1️⃣  <b>Define:</b> ANXIOUS\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for anxious.", Difficulty.Easy)
            .Card("Sufficient", "1️⃣  <b>Define:</b> SUFFICIENT\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an antonym for sufficient.", Difficulty.Easy)
            .Category(VerbCategory)
            .Card("Observe", "1️⃣  <b>Define:</b> OBSERVE\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for observe.", Difficulty.Easy)
            .Category(AdjectiveCategory)
            .Card("Diligent", "1️⃣  <b>Define:</b> DILIGENT\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an antonym for diligent.", Difficulty.Easy)
            .Card("Crucial", "1️⃣  <b>Define:</b> CRUCIAL\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for crucial.", Difficulty.Easy)
            .Card("Transparent", "1️⃣  <b>Define:</b> TRANSPARENT\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an antonym for transparent.", Difficulty.Easy)
            .Card("Eloquent", "1️⃣  <b>Define:</b> ELOQUENT\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for eloquent.", Difficulty.Easy)
            .Category(VerbCategory)
            .Card("Collaborate", "1️⃣  <b>Define:</b> COLLABORATE\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for collaborate.", Difficulty.Easy)

            // ── Medium: less common, curriculum vocabulary ────────────────────────
            .Category(AdjectiveCategory)
            .Card("Ambiguous", "1️⃣  <b>Define:</b> AMBIGUOUS\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an antonym for ambiguous.", Difficulty.Medium)
            .Category(VerbCategory)
            .Card("Proliferate", "1️⃣  <b>Define:</b> PROLIFERATE\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for proliferate.", Difficulty.Medium)
            .Category(AdjectiveCategory)
            .Card("Pensive", "1️⃣  <b>Define:</b> PENSIVE\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for pensive.", Difficulty.Medium)
            .Category(NounCategory)
            .Card("Resilience", "1️⃣  <b>Define:</b> RESILIENCE\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an antonym for resilience.", Difficulty.Medium)
            .Category(AdjectiveCategory)
            .Card("Hypocritical", "1️⃣  <b>Define:</b> HYPOCRITICAL\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for hypocritical.", Difficulty.Medium)
            .Card("Arbitrary", "1️⃣  <b>Define:</b> ARBITRARY\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an antonym for arbitrary.", Difficulty.Medium)
            .Category(NounCategory)
            .Card("Conjecture", "1️⃣  <b>Define:</b> CONJECTURE\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for conjecture.", Difficulty.Medium)
            .Category(AdjectiveCategory)
            .Card("Meticulous", "1️⃣  <b>Define:</b> METICULOUS\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an antonym for meticulous.", Difficulty.Medium)
            .Category("Verb/Noun")
            .Card("Advocate", "1️⃣  <b>Define:</b> ADVOCATE (as a verb AND as a noun — different meanings!)\n2️⃣  Use each form in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for the noun form.", Difficulty.Medium)
            .Category(AdjectiveCategory)
            .Card("Unprecedented", "1️⃣  <b>Define:</b> UNPRECEDENTED\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give a synonym for unprecedented.", Difficulty.Medium)

            // ── Hard: academic and subject-specific vocabulary ────────────────────
            .Category(AcademicCategory)
            .Card("Juxtaposition", "1️⃣  <b>Define:</b> JUXTAPOSITION\n2️⃣  Use it in a sentence about literature or art.\n3️⃣  <b>Bonus:</b> Give a real example of juxtaposition in a book you know.", Difficulty.Hard)
            .Card("Hegemony", "1️⃣  <b>Define:</b> HEGEMONY\n2️⃣  Use it in a sentence about history or politics.\n3️⃣  <b>Bonus:</b> Give a real historical example.", Difficulty.Hard)
            .Card("Empirical", "1️⃣  <b>Define:</b> EMPIRICAL\n2️⃣  Use it in a sentence about science.\n3️⃣  <b>Bonus:</b> Give an antonym and explain why it matters in science.", Difficulty.Hard)
            .Card("Paradigm", "1️⃣  <b>Define:</b> PARADIGM\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Explain what 'paradigm shift' means.", Difficulty.Hard)
            .Card("Dichotomy", "1️⃣  <b>Define:</b> DICHOTOMY\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> Give an example of a dichotomy in real life.", Difficulty.Hard)

            // ── Extreme: truly challenging academic vocabulary ─────────────────────
            .Card("Solipsism", "1️⃣  <b>Define:</b> SOLIPSISM\n2️⃣  Use it correctly in a sentence.\n3️⃣  <b>Bonus:</b> In which academic field would you most likely encounter this word?", Difficulty.Extreme)
            .Card("Epistemology", "1️⃣  <b>Define:</b> EPISTEMOLOGY\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> What is the epistemological question: 'How do we know what we know?' asking?", Difficulty.Extreme)
            .Category(AdjectiveCategory)
            .Card("Sycophantic", "1️⃣  <b>Define:</b> SYCOPHANTIC\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> What noun relates to this adjective?", Difficulty.Extreme)
            .Category("Adverb")
            .Card("Ostensibly", "1️⃣  <b>Define:</b> OSTENSIBLY\n2️⃣  Use it in a sentence.\n3️⃣  <b>Bonus:</b> What does it imply about reality vs appearance?", Difficulty.Extreme)

            .Build();
}