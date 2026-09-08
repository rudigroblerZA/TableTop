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

    private const string Deck = "Grammar Quest";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)

            // ── EASY: single clear error, common rules ────────────────────────────
            .Category(PronounsCategory)
            .Card("Me and Sarah went to the shops.",
                "<b>Spot the error:</b> 'Me and Sarah went to the shops.'\n\nWhat is wrong? Say the corrected sentence.", Difficulty.Easy)
            .Category(AgreementCategory)
            .Card("She don't like broccoli.",
                "<b>Spot the error:</b> 'She don't like broccoli.'\n\nWhat is wrong? Say the corrected sentence.", Difficulty.Easy)
            .Card("The dogs is barking loudly.",
                "<b>Spot the error:</b> 'The dogs is barking loudly.'\n\nWhat is wrong? Say the corrected sentence.", Difficulty.Easy)
            .Category(TenseCategory)
            .Card("I goed to the park yesterday.",
                "<b>Spot the error:</b> 'I goed to the park yesterday.'\n\nWhat is wrong? Say the corrected sentence.", Difficulty.Easy)
            .Category(PunctuationCategory)
            .Card("their going to the cinema later.",
                "<b>Spot the error:</b> 'their going to the cinema later.'\n\nTwo errors here. Find them both.", Difficulty.Easy)
            .Card("The cat sat on it's mat.",
                "<b>Spot the error:</b> 'The cat sat on it's mat.'\n\nWhen do we use an apostrophe in 'its'?", Difficulty.Easy)
            .Category(AgreementCategory)
            .Card("We was very tired after the game.",
                "<b>Spot the error:</b> 'We was very tired after the game.'\n\nCorrect the subject-verb agreement.", Difficulty.Easy)
            .Category(PronounsCategory)
            .Card("Him and I played football.",
                "<b>Spot the error:</b> 'Him and I played football.'\n\nWhich pronoun is wrong? Why?", Difficulty.Easy)
            .Category(TenseCategory)
            .Card("She sitted down on the bench.",
                "<b>Spot the error:</b> 'She sitted down on the bench.'\n\nWhat is the correct past tense of 'sit'?", Difficulty.Easy)
            .Card("I have saw that film before.",
                "<b>Spot the error:</b> 'I have saw that film before.'\n\nThis is present perfect tense. Fix it.", Difficulty.Easy)
            .Category(SentencesCategory)
            .Card("Running to the bus stop quickly by him.",
                "<b>Spot the error:</b> 'Running to the bus stop quickly by him.'\n\nIs this a complete sentence? What does it need?", Difficulty.Easy)
            .Category(TenseCategory)
            .Card("The children brung their lunch boxes.",
                "<b>Spot the error:</b> 'The children brung their lunch boxes.'\n\nWhat is the correct past tense of 'bring'?", Difficulty.Easy)

            // ── MEDIUM: less obvious errors, two-part problems ────────────────────
            .Category(AgreementCategory)
            .Card("Neither the boys nor the girl are ready.",
                "<b>Spot the error:</b> 'Neither the boys nor the girl are ready.'\n\nRule: with neither/nor, the verb agrees with the <b>nearest</b> subject. Fix it.", Difficulty.Medium)
            .Category(TenseCategory)
            .Card("I should of told her the truth.",
                "<b>Spot the error:</b> 'I should of told her the truth.'\n\nThis is a very common mistake. What should 'of' be?", Difficulty.Medium)
            .Category(AgreementCategory)
            .Card("The team are playing well, aren't they?",
                "<b>Spot the error:</b> 'The team are playing well, aren't they?'\n\nIs 'team' singular or plural? Does the tag question match?", Difficulty.Medium)
            .Category(PronounsCategory)
            .Card("Whoever arrives first, the prize goes to they.",
                "<b>Spot the error:</b> 'Whoever arrives first, the prize goes to they.'\n\nWhich pronoun form do we use after a preposition?", Difficulty.Medium)
            .Category(PunctuationCategory)
            .Card("Hopefully, the weather will be nice — we brought our umbrella's.",
                "<b>Spot the error:</b> 'we brought our umbrella's.'\n\nShould 'umbrella's' have an apostrophe here? Why not?", Difficulty.Medium)
            .Category(PronounsCategory)
            .Card("Between you and I, this is the best plan.",
                "<b>Spot the error:</b> 'Between you and I, this is the best plan.'\n\nWhich pronouns follow prepositions like 'between'?", Difficulty.Medium)
            .Category(AgreementCategory)
            .Card("The data shows that climate change are affecting all countries.",
                "<b>Spot the error:</b> 'The data shows that climate change are affecting all countries.'\n\nWhich verb is wrong? Why?", Difficulty.Medium)
            .Category(TenseCategory)
            .Card("I enjoy to swim in the sea every summer.",
                "<b>Spot the error:</b> 'I enjoy to swim in the sea every summer.'\n\nWhat form of the verb follows 'enjoy'? Gerund or infinitive?", Difficulty.Medium)
            .Category(SentencesCategory)
            .Card("She asked me where did I live.",
                "<b>Spot the error:</b> 'She asked me where did I live.'\n\nThis is an indirect question. How does word order change?", Difficulty.Medium)
            .Card("We discussed about the problem for an hour.",
                "<b>Spot the error:</b> 'We discussed about the problem for an hour.'\n\nWhich word is unnecessary? Why?", Difficulty.Medium)

            // ── HARD: subtle, rule-based challenges ───────────────────────────────
            .Category(AgreementCategory)
            .Card("The number of students have increased this year.",
                "<b>Spot the error:</b> 'The number of students have increased this year.'\n\nIs 'the number' singular or plural? What about 'a number'?", Difficulty.Hard)
            .Category(PronounsCategory)
            .Card("Everyone must bring their own pencils.",
                "<b>Is this correct?</b> 'Everyone must bring their own pencils.'\n\nExplain whether this is right or wrong, and why.", Difficulty.Hard)
            .Category(AgreementCategory)
            .Card("The criteria for success was not clearly defined.",
                "<b>Spot the error:</b> 'The criteria for success was not clearly defined.'\n\n'Criteria' is the plural of 'criterion'. Fix the sentence.", Difficulty.Hard)
            .Category(SentencesCategory)
            .Card("Having finished the exam, the room fell silent.",
                "<b>Spot the error:</b> 'Having finished the exam, the room fell silent.'\n\nThis is a <b>dangling modifier</b>. Who finished the exam? Rewrite it.", Difficulty.Hard)
            .Card("He was more cleverer than his classmates.",
                "<b>Spot the error:</b> 'He was more cleverer than his classmates.'\n\nWhat is this type of error called? Fix the comparative form.", Difficulty.Hard)
            .Card("I literally died laughing — it was hilarious.",
                "<b>Discuss:</b> 'I literally died laughing.'\n\nWhat does 'literally' mean? Is it being used correctly here? What should replace it?", Difficulty.Hard)
            .Category(AgreementCategory)
            .Card("The teacher, together with the students, are going on the trip.",
                "<b>Spot the error:</b> 'The teacher, together with the students, are going on the trip.'\n\nWhat is the grammatical subject? Fix the verb agreement.", Difficulty.Hard)
            .Category(PronounsCategory)
            .Card("Whom shall I say is calling?",
                "<b>Spot the error:</b> 'Whom shall I say is calling?'\n\nWho vs Whom: who is the subject of 'is calling'. Fix it.", Difficulty.Hard)

            // ── EXTREME: advanced grammar for challenge ────────────────────────────
            .Category(TenseCategory)
            .Card("If I was you, I wouldn't worry.",
                "<b>Advanced challenge:</b> 'If I was you, I wouldn't worry.'\n\nThis involves the <b>subjunctive mood</b>. What should 'was' be, and why?", Difficulty.Extreme)
            .Category(AgreementCategory)
            .Card("The phenomena was remarkable.",
                "<b>Advanced challenge:</b> 'The phenomena was remarkable.'\n\n'Phenomena' is the plural of 'phenomenon'. Correct the sentence AND use both words in separate sentences.", Difficulty.Extreme)
            .Category(SentencesCategory)
            .Card("She explained the rules clearly and with patience.",
                "<b>Advanced challenge:</b> 'She explained the rules clearly and with patience.'\n\nThis has a <b>parallelism error</b>. Identify it and rewrite the sentence with correct parallel structure.", Difficulty.Extreme)

            .Build();
}