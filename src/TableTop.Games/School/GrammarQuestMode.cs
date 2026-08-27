using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Grammar Quest — card-per-turn grammar game for Grade 6.
///
/// Each card presents a sentence with a grammar problem. The active player must:
///   • Name the grammar rule being broken, AND
///   • Say the corrected version aloud.
///
/// The group (or teacher) judges correctness.
/// Scoring: 2 pts for rule + correction; 1 pt for correction only; 0 for neither.
///
/// Categories: Punctuation, Tense, Subject-Verb Agreement, Pronouns, Sentence Structure.
/// </summary>
public sealed class GrammarQuestMode : BaseGameModeDefinition, IFlowAwareMode
{
    /// <inheritdoc />
    public override string Name => "Grammar Quest";
    /// <inheritdoc />
    public override string Description =>
        "Fix the broken sentence and name the grammar rule. Grade 6 English Language Arts.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ Rule + Fix (+2)";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "→ Next card";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [GrammarQuestCardBank.PunctuationCategory] = "#26C6DA",
            [GrammarQuestCardBank.TenseCategory] = "#66BB6A",
            [GrammarQuestCardBank.AgreementCategory] = "#FFCA28",
            [GrammarQuestCardBank.PronounsCategory] = "#EC407A",
            [GrammarQuestCardBank.SentencesCategory] = "#AB47BC",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 2);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        GrammarQuestCardBank.All;

    /// <summary>Exposes the card bank for testing without a player list.</summary>
    public static IReadOnlyList<ICard> GetCards() => GrammarQuestCardBank.All;
}

/// <summary>60 grammar challenge cards across four difficulty tiers.</summary>
public static class GrammarQuestCardBank
{
    internal const string PunctuationCategory = "Punctuation";
    internal const string TenseCategory = "Tense";
    internal const string AgreementCategory = "Agreement";
    internal const string PronounsCategory = "Pronouns";
    internal const string SentencesCategory = "Sentences";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── EASY: single clear error, common rules ────────────────────────────

        G("Me and Sarah went to the shops.",
          PronounsCategory, Difficulty.Easy,
          "<b>Spot the error:</b> 'Me and Sarah went to the shops.'\n\nWhat is wrong? Say the corrected sentence."),

        G("She don't like broccoli.",
          AgreementCategory, Difficulty.Easy,
          "<b>Spot the error:</b> 'She don't like broccoli.'\n\nWhat is wrong? Say the corrected sentence."),

        G("The dogs is barking loudly.",
          AgreementCategory, Difficulty.Easy,
          "<b>Spot the error:</b> 'The dogs is barking loudly.'\n\nWhat is wrong? Say the corrected sentence."),

        G("I goed to the park yesterday.",
          TenseCategory, Difficulty.Easy,
          "<b>Spot the error:</b> 'I goed to the park yesterday.'\n\nWhat is wrong? Say the corrected sentence."),

        G("their going to the cinema later.",
          PunctuationCategory, Difficulty.Easy,
          "<b>Spot the error:</b> 'their going to the cinema later.'\n\nTwo errors here. Find them both."),

        G("The cat sat on it's mat.",
          PunctuationCategory, Difficulty.Easy,
          "<b>Spot the error:</b> 'The cat sat on it's mat.'\n\nWhen do we use an apostrophe in 'its'?"),

        G("We was very tired after the game.",
          AgreementCategory, Difficulty.Easy,
          "<b>Spot the error:</b> 'We was very tired after the game.'\n\nCorrect the subject-verb agreement."),

        G("Him and I played football.",
          PronounsCategory, Difficulty.Easy,
          "<b>Spot the error:</b> 'Him and I played football.'\n\nWhich pronoun is wrong? Why?"),

        G("She sitted down on the bench.",
          TenseCategory, Difficulty.Easy,
          "<b>Spot the error:</b> 'She sitted down on the bench.'\n\nWhat is the correct past tense of 'sit'?"),

        G("I have saw that film before.",
          TenseCategory, Difficulty.Easy,
          "<b>Spot the error:</b> 'I have saw that film before.'\n\nThis is present perfect tense. Fix it."),

        G("Running to the bus stop quickly by him.",
          SentencesCategory, Difficulty.Easy,
          "<b>Spot the error:</b> 'Running to the bus stop quickly by him.'\n\nIs this a complete sentence? What does it need?"),

        G("The children brung their lunch boxes.",
          TenseCategory, Difficulty.Easy,
          "<b>Spot the error:</b> 'The children brung their lunch boxes.'\n\nWhat is the correct past tense of 'bring'?"),

        // ── MEDIUM: less obvious errors, two-part problems ────────────────────

        G("Neither the boys nor the girl are ready.",
          AgreementCategory, Difficulty.Medium,
          "<b>Spot the error:</b> 'Neither the boys nor the girl are ready.'\n\nRule: with neither/nor, the verb agrees with the <b>nearest</b> subject. Fix it."),

        G("I should of told her the truth.",
          TenseCategory, Difficulty.Medium,
          "<b>Spot the error:</b> 'I should of told her the truth.'\n\nThis is a very common mistake. What should 'of' be?"),

        G("The team are playing well, aren't they?",
          AgreementCategory, Difficulty.Medium,
          "<b>Spot the error:</b> 'The team are playing well, aren't they?'\n\nIs 'team' singular or plural? Does the tag question match?"),

        G("Whoever arrives first, the prize goes to they.",
          PronounsCategory, Difficulty.Medium,
          "<b>Spot the error:</b> 'Whoever arrives first, the prize goes to they.'\n\nWhich pronoun form do we use after a preposition?"),

        G("Hopefully, the weather will be nice — we brought our umbrella's.",
          PunctuationCategory, Difficulty.Medium,
          "<b>Spot the error:</b> 'we brought our umbrella's.'\n\nShould 'umbrella's' have an apostrophe here? Why not?"),

        G("Between you and I, this is the best plan.",
          PronounsCategory, Difficulty.Medium,
          "<b>Spot the error:</b> 'Between you and I, this is the best plan.'\n\nWhich pronouns follow prepositions like 'between'?"),

        G("The data shows that climate change are affecting all countries.",
          AgreementCategory, Difficulty.Medium,
          "<b>Spot the error:</b> 'The data shows that climate change are affecting all countries.'\n\nWhich verb is wrong? Why?"),

        G("I enjoy to swim in the sea every summer.",
          TenseCategory, Difficulty.Medium,
          "<b>Spot the error:</b> 'I enjoy to swim in the sea every summer.'\n\nWhat form of the verb follows 'enjoy'? Gerund or infinitive?"),

        G("She asked me where did I live.",
          SentencesCategory, Difficulty.Medium,
          "<b>Spot the error:</b> 'She asked me where did I live.'\n\nThis is an indirect question. How does word order change?"),

        G("We discussed about the problem for an hour.",
          SentencesCategory, Difficulty.Medium,
          "<b>Spot the error:</b> 'We discussed about the problem for an hour.'\n\nWhich word is unnecessary? Why?"),

        // ── HARD: subtle, rule-based challenges ───────────────────────────────

        G("The number of students have increased this year.",
          AgreementCategory, Difficulty.Hard,
          "<b>Spot the error:</b> 'The number of students have increased this year.'\n\nIs 'the number' singular or plural? What about 'a number'?"),

        G("Everyone must bring their own pencils.",
          PronounsCategory, Difficulty.Hard,
          "<b>Is this correct?</b> 'Everyone must bring their own pencils.'\n\nExplain whether this is right or wrong, and why."),

        G("The criteria for success was not clearly defined.",
          AgreementCategory, Difficulty.Hard,
          "<b>Spot the error:</b> 'The criteria for success was not clearly defined.'\n\n'Criteria' is the plural of 'criterion'. Fix the sentence."),

        G("Having finished the exam, the room fell silent.",
          SentencesCategory, Difficulty.Hard,
          "<b>Spot the error:</b> 'Having finished the exam, the room fell silent.'\n\nThis is a <b>dangling modifier</b>. Who finished the exam? Rewrite it."),

        G("He was more cleverer than his classmates.",
          SentencesCategory, Difficulty.Hard,
          "<b>Spot the error:</b> 'He was more cleverer than his classmates.'\n\nWhat is this type of error called? Fix the comparative form."),

        G("I literally died laughing — it was hilarious.",
          SentencesCategory, Difficulty.Hard,
          "<b>Discuss:</b> 'I literally died laughing.'\n\nWhat does 'literally' mean? Is it being used correctly here? What should replace it?"),

        G("The teacher, together with the students, are going on the trip.",
          AgreementCategory, Difficulty.Hard,
          "<b>Spot the error:</b> 'The teacher, together with the students, are going on the trip.'\n\nWhat is the grammatical subject? Fix the verb agreement."),

        G("Whom shall I say is calling?",
          PronounsCategory, Difficulty.Hard,
          "<b>Spot the error:</b> 'Whom shall I say is calling?'\n\nWho vs Whom: who is the subject of 'is calling'. Fix it."),

        // ── EXTREME: advanced grammar for challenge ────────────────────────────

        G("If I was you, I wouldn't worry.",
          TenseCategory, Difficulty.Extreme,
          "<b>Advanced challenge:</b> 'If I was you, I wouldn't worry.'\n\nThis involves the <b>subjunctive mood</b>. What should 'was' be, and why?"),

        G("The phenomena was remarkable.",
          AgreementCategory, Difficulty.Extreme,
          "<b>Advanced challenge:</b> 'The phenomena was remarkable.'\n\n'Phenomena' is the plural of 'phenomenon'. Correct the sentence AND use both words in separate sentences."),

        G("She explained the rules clearly and with patience.",
          SentencesCategory, Difficulty.Extreme,
          "<b>Advanced challenge:</b> 'She explained the rules clearly and with patience.'\n\nThis has a <b>parallelism error</b>. Identify it and rewrite the sentence with correct parallel structure."),
    ];

    private static ICard G(string title, string category, Difficulty d, string desc) =>
        StandardCard.Create(title, desc, d, category);
}