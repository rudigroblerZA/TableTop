using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Mental Maths Sprint — rapid-fire arithmetic for Grade 6 (age 11–12).
///
/// How to play:
///   1. Read the problem aloud.
///   2. The active player answers from memory — no paper, no calculator.
///   3. Reveal the answer below the fold and award the point if correct.
///
/// Cards escalate from single-step facts (times tables, simple addition) through
/// two-step problems (order of operations, fractions of amounts) up to multi-step
/// word problems that demand genuine mental working. Every card carries its worked
/// answer so a parent or teacher can adjudicate instantly.
///
/// Curriculum-aligned to Grade 6 number and arithmetic: four operations, fractions,
/// percentages, factors and multiples, and BODMAS.
/// </summary>
public sealed class MentalMathsSprintMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Mental Maths Sprint";
    /// <inheritdoc />
    public override string Description =>
        "Rapid-fire mental arithmetic for Grade 6 — times tables, fractions, percentages, and BODMAS. No paper allowed.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ Correct";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "✗ Pass";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [MentalMathsSprintCardBank.NumberCategory] = "#42A5F5",
            [MentalMathsSprintCardBank.FractionsCategory] = "#66BB6A",
            [MentalMathsSprintCardBank.PercentagesCategory] = "#FFA726",
            [MentalMathsSprintCardBank.BODMASCategory] = "#AB47BC",
            [MentalMathsSprintCardBank.WordProblemCategory] = "#EC407A",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        MentalMathsSprintCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => MentalMathsSprintCardBank.All;
}

/// <summary>Built-in card bank for Mental Maths Sprint. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class MentalMathsSprintCardBank
{
    internal const string NumberCategory = "Number";
    internal const string FractionsCategory = "Fractions";
    internal const string PercentagesCategory = "Percentages";
    internal const string BODMASCategory = "BODMAS";
    internal const string WordProblemCategory = "Word Problem";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── NUMBER (four operations) ─────────────────────────────────────────
        Q(NumberCategory, "What is 7 × 8?", "56", Difficulty.Easy),
        Q(NumberCategory, "What is 9 × 6?", "54", Difficulty.Easy),
        Q(NumberCategory, "What is 144 ÷ 12?", "12", Difficulty.Easy),
        Q(NumberCategory, "What is 45 + 38?", "83", Difficulty.Easy),
        Q(NumberCategory, "What is 91 − 47?", "44", Difficulty.Easy),
        Q(NumberCategory, "What is 12 × 12?", "144", Difficulty.Easy),
        Q(NumberCategory, "What is 8 × 7?", "56", Difficulty.Easy),
        Q(NumberCategory, "What is 63 ÷ 9?", "7", Difficulty.Easy),
        Q(NumberCategory, "What is 52 + 28?", "80", Difficulty.Easy),
        Q(NumberCategory, "What is 100 − 37?", "63", Difficulty.Easy),
        Q(NumberCategory, "What is 11 × 11?", "121", Difficulty.Easy),
        Q(NumberCategory, "What is 5 × 9?", "45", Difficulty.Easy),
        Q(NumberCategory, "What is 156 ÷ 4?", "39", Difficulty.Medium),
        Q(NumberCategory, "What is 234 + 189?", "423", Difficulty.Medium),
        Q(NumberCategory, "What is 1000 − 367?", "633", Difficulty.Medium),
        Q(NumberCategory, "What is 25 × 16?", "400  (25 × 16 = 25 × 4 × 4 = 100 × 4)", Difficulty.Hard),
        Q(NumberCategory, "What is 48 ÷ 6?", "8", Difficulty.Easy),
        Q(NumberCategory, "What is 36 + 64?", "100", Difficulty.Easy),
        Q(NumberCategory, "What is 72 − 45?", "27", Difficulty.Easy),
        Q(NumberCategory, "What is 15 × 4?", "60", Difficulty.Easy),
        Q(NumberCategory, "What is 200 ÷ 8?", "25", Difficulty.Medium),
        Q(NumberCategory, "What is 456 + 234?", "690", Difficulty.Medium),
        Q(NumberCategory, "What is 500 − 182?", "318", Difficulty.Medium),
        Q(NumberCategory, "What is 13 × 13?", "169", Difficulty.Medium),

        // ── FRACTIONS ────────────────────────────────────────────────────────
        Q(FractionsCategory, "What is ½ of 48?", "24", Difficulty.Easy),
        Q(FractionsCategory, "What is ¼ of 60?", "15", Difficulty.Easy),
        Q(FractionsCategory, "What is ⅓ of 27?", "9", Difficulty.Easy),
        Q(FractionsCategory, "What is ¾ of 40?", "30  (¼ of 40 = 10, so ¾ = 30)", Difficulty.Medium),
        Q(FractionsCategory, "What is ⅖ of 35?", "14  (⅕ of 35 = 7, so ⅖ = 14)", Difficulty.Medium),
        Q(FractionsCategory, "What is ½ + ¼?", "¾", Difficulty.Medium),
        Q(FractionsCategory, "What is ⅔ + ⅙?", "⅚  (⅔ = 4/6, plus 1/6 = 5/6)", Difficulty.Hard),
        Q(FractionsCategory, "What is ⅝ of 64?", "40  (⅛ of 64 = 8, so ⅝ = 40)", Difficulty.Hard),
        Q(FractionsCategory, "What is ½ of 100?", "50", Difficulty.Easy),
        Q(FractionsCategory, "What is ⅕ of 50?", "10", Difficulty.Easy),
        Q(FractionsCategory, "What is ¾ of 60?", "45", Difficulty.Medium),
        Q(FractionsCategory, "What is ⅓ + ⅓?", "⅔", Difficulty.Easy),
        Q(FractionsCategory, "What is ½ − ¼?", "¼", Difficulty.Medium),
        Q(FractionsCategory, "What is ⅗ of 25?", "15  (⅕ = 5, so ⅗ = 15)", Difficulty.Medium),

        // ── PERCENTAGES ──────────────────────────────────────────────────────
        Q(PercentagesCategory, "What is 10% of 250?", "25", Difficulty.Easy),
        Q(PercentagesCategory, "What is 50% of 84?", "42", Difficulty.Easy),
        Q(PercentagesCategory, "What is 25% of 160?", "40", Difficulty.Easy),
        Q(PercentagesCategory, "What is 20% of 75?", "15  (10% = 7.5, double it)", Difficulty.Medium),
        Q(PercentagesCategory, "What is 15% of 200?", "30  (10% = 20, 5% = 10, add them)", Difficulty.Medium),
        Q(PercentagesCategory, "What is 30% of 90?", "27  (10% = 9, ×3)", Difficulty.Medium),
        Q(PercentagesCategory, "What is 75% of 48?", "36  (¾ of 48)", Difficulty.Hard),
        Q(PercentagesCategory, "A £60 coat is reduced by 35%. What is the new price?", "£39  (35% of 60 = 21, 60 − 21)", Difficulty.Hard),
        Q(PercentagesCategory, "What is 1% of 500?", "5", Difficulty.Easy),
        Q(PercentagesCategory, "What is 100% of 35?", "35", Difficulty.Easy),
        Q(PercentagesCategory, "What is 5% of 200?", "10", Difficulty.Easy),
        Q(PercentagesCategory, "What is 40% of 50?", "20", Difficulty.Medium),
        Q(PercentagesCategory, "What is 60% of 75?", "45", Difficulty.Medium),
        Q(PercentagesCategory, "A £100 item costs £80 after a discount. What % off was it?", "20%  (20 ÷ 100 = 0.2 = 20%)", Difficulty.Hard),

        // ── BODMAS (order of operations) ─────────────────────────────────────
        Q(BODMASCategory, "What is 3 + 4 × 2?", "11  (multiply first: 4 × 2 = 8, then + 3)", Difficulty.Medium),
        Q(BODMASCategory, "What is (6 + 2) × 5?", "40  (brackets first: 6 + 2 = 8, then × 5)", Difficulty.Medium),
        Q(BODMASCategory, "What is 20 − 3 × 4?", "8  (3 × 4 = 12, then 20 − 12)", Difficulty.Medium),
        Q(BODMASCategory, "What is 5² + 10?", "35  (5² = 25, then + 10)", Difficulty.Medium),
        Q(BODMASCategory, "What is 100 ÷ (2 + 3)?", "20  (brackets: 2 + 3 = 5, then 100 ÷ 5)", Difficulty.Hard),
        Q(BODMASCategory, "What is 2 × 3² − 4?", "14  (3² = 9, × 2 = 18, − 4)", Difficulty.Hard),
        Q(BODMASCategory, "What is 10 + 2 × 3?", "16  (2 × 3 = 6, then + 10)", Difficulty.Medium),
        Q(BODMASCategory, "What is 24 ÷ 3 + 2?", "10  (24 ÷ 3 = 8, then + 2)", Difficulty.Medium),
        Q(BODMASCategory, "What is 2 + 3 × 5?", "17  (3 × 5 = 15, then + 2)", Difficulty.Medium),
        Q(BODMASCategory, "What is 4² − 2 × 3?", "10  (4² = 16, 2 × 3 = 6, 16 − 6)", Difficulty.Hard),

        // ── WORD PROBLEMS ────────────────────────────────────────────────────
        Q(WordProblemCategory,
            "A baker makes 6 trays of muffins. Each tray holds 8 muffins. She sells 35. How many are left?",
            "13  (6 × 8 = 48, then 48 − 35)", Difficulty.Medium),
        Q(WordProblemCategory,
            "A train leaves at 14:25 and arrives at 16:10. How long is the journey, in minutes?",
            "105 minutes  (1 hour 45 minutes)", Difficulty.Medium),
        Q(WordProblemCategory,
            "Three friends share £24.60 equally. How much does each get?",
            "£8.20  (24.60 ÷ 3)", Difficulty.Medium),
        Q(WordProblemCategory,
            "A rectangle is 12 cm long and 7 cm wide. What is its area?",
            "84 cm²  (12 × 7)", Difficulty.Medium),
        Q(WordProblemCategory,
            "A jug holds 1.5 litres. How many 250 ml glasses can it fill?",
            "6  (1500 ÷ 250)", Difficulty.Hard),
        Q(WordProblemCategory,
            "Sam has 5 packs of 24 stickers. He gives away ⅓ of them. How many does he keep?",
            "80  (5 × 24 = 120, ⅓ = 40 given, 120 − 40)", Difficulty.Hard),
        Q(WordProblemCategory,
            "A film starts at 19:45 and lasts 135 minutes. What time does it end?",
            "22:00  (135 min = 2 h 15 min, 19:45 + 2:15)", Difficulty.Hard),
        Q(WordProblemCategory,
            "A shop has 200 apples. It sells 3/5 of them. How many are left?",
            "80  (3/5 of 200 = 120, 200 − 120)", Difficulty.Medium),
        Q(WordProblemCategory,
            "If a book costs £8 and is 20% off, what is the sale price?",
            "£6.40  (20% of £8 = £1.60, 8 − 1.60)", Difficulty.Medium),
        Q(WordProblemCategory,
            "A class has 24 pupils. 1/4 are absent. How many are present?",
            "18  (1/4 of 24 = 6, 24 − 6)", Difficulty.Easy),
    ];

    private static ICard Q(string category, string question, string answer, Difficulty d) =>
        StandardCard.Create(
            category,
            question + "\n\n<b>Answer:</b> " + answer,
            d,
            category);
}
