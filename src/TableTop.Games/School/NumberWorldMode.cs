using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Number World — general-knowledge maths for the classroom: the names of
/// shapes, units of measurement, famous numbers, and the vocabulary of maths,
/// as multiple-choice questions dealt one at a time.
///
/// This is maths TRIVIA, not arithmetic drills — kept deliberately distinct
/// from Mental Maths Sprint (rapid mental calculation) and Estimation Station
/// (Fermi reasoning). Here you recognise a hexagon, know how many sides a cube
/// has, and recall what 'pi' begins with — the general-knowledge side of maths.
/// </summary>
public sealed class NumberWorldMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name        => "Number World";
    /// <inheritdoc />
    public override string Description =>
        "Maths general knowledge — shapes, units, famous numbers, and maths vocabulary. Trivia, not drills. Multiple choice.";

    /// <summary>Label for a correctly answered card.</summary>
    public override string CompleteLabel => "Correct";
    /// <summary>Label for a passed card.</summary>
    public override string SkipLabel     => "Pass";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Shapes"]      = "#42A5F5",
            ["Measuring"]   = "#66BB6A",
            ["Famous Numbers"] = "#FFCA28",
            ["Maths Words"] = "#AB47BC",
            ["Number Facts"]= "#FF7043",
        };

    /// <summary>Harder questions score more.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in Number World card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        NumberWorldCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => NumberWorldCardBank.All;
}

/// <summary>Built-in card bank for Number World.</summary>
public static class NumberWorldCardBank
{
    /// <summary>All cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── SHAPES ───────────────────────────────────────────────────────────
        Q("Shapes", "How many sides does a triangle have?", "2", "3", "4", "5", AnswerLabel.B, Difficulty.Easy),
        Q("Shapes", "How many sides does a hexagon have?", "5", "6", "7", "8", AnswerLabel.B, Difficulty.Medium),
        Q("Shapes", "How many sides does a pentagon have?", "4", "5", "6", "7", AnswerLabel.B, Difficulty.Easy),
        Q("Shapes", "What is a shape with 8 equal sides called?", "Hexagon", "Heptagon", "Octagon", "Nonagon", AnswerLabel.C, Difficulty.Medium),
        Q("Shapes", "How many faces does a cube have?", "4", "6", "8", "12", AnswerLabel.B, Difficulty.Medium),
        Q("Shapes", "A shape with three equal sides is called a…?", "Right triangle", "Equilateral triangle", "Scalene triangle", "Isosceles triangle", AnswerLabel.B, Difficulty.Hard),

        // ── MEASURING — units ────────────────────────────────────────────────
        Q("Measuring", "How many centimetres are in one metre?", "10", "100", "1000", "12", AnswerLabel.B, Difficulty.Easy),
        Q("Measuring", "How many minutes are in one hour?", "30", "60", "90", "100", AnswerLabel.B, Difficulty.Easy),
        Q("Measuring", "How many millimetres are in one centimetre?", "5", "10", "100", "1000", AnswerLabel.B, Difficulty.Medium),
        Q("Measuring", "How many grams are in one kilogram?", "10", "100", "1000", "10000", AnswerLabel.C, Difficulty.Medium),
        Q("Measuring", "How many days are in a leap year?", "364", "365", "366", "367", AnswerLabel.C, Difficulty.Hard),
        Q("Measuring", "A right angle measures how many degrees?", "45", "90", "180", "360", AnswerLabel.B, Difficulty.Medium),

        // ── FAMOUS NUMBERS ───────────────────────────────────────────────────
        Q("Famous Numbers", "The number 'pi' begins with which digits?", "2.14", "3.14", "3.41", "1.34", AnswerLabel.B, Difficulty.Hard),
        Q("Famous Numbers", "What do we call a number that can only be divided by 1 and itself?", "Even number", "Prime number", "Square number", "Odd number", AnswerLabel.B, Difficulty.Hard),
        Q("Famous Numbers", "How many degrees are there in a full circle?", "90", "180", "270", "360", AnswerLabel.D, Difficulty.Easy),
        Q("Famous Numbers", "What is the smallest prime number?", "0", "1", "2", "3", AnswerLabel.C, Difficulty.Extreme),
        Q("Famous Numbers", "A 'dozen' means how many?", "6", "10", "12", "20", AnswerLabel.C, Difficulty.Easy),

        // ── MATHS WORDS — vocabulary ─────────────────────────────────────────
        Q("Maths Words", "What is the answer to an addition called?", "Sum", "Product", "Difference", "Quotient", AnswerLabel.A, Difficulty.Medium),
        Q("Maths Words", "What is the answer to a multiplication called?", "Sum", "Product", "Difference", "Total", AnswerLabel.B, Difficulty.Hard),
        Q("Maths Words", "The distance all the way around a shape is its…?", "Area", "Perimeter", "Volume", "Radius", AnswerLabel.B, Difficulty.Medium),
        Q("Maths Words", "The amount of space inside a flat shape is its…?", "Perimeter", "Area", "Length", "Height", AnswerLabel.B, Difficulty.Medium),
        Q("Maths Words", "The line from the centre of a circle to its edge is the…?", "Diameter", "Radius", "Circumference", "Chord", AnswerLabel.B, Difficulty.Hard),

        // ── NUMBER FACTS ─────────────────────────────────────────────────────
        Q("Number Facts", "Which of these is an even number?", "7", "13", "20", "31", AnswerLabel.C, Difficulty.Easy),
        Q("Number Facts", "What is half of 100?", "25", "40", "50", "75", AnswerLabel.C, Difficulty.Easy),
        Q("Number Facts", "In the number 356, what does the 5 stand for?", "5 ones", "5 tens", "5 hundreds", "5 thousands", AnswerLabel.B, Difficulty.Medium),
        Q("Number Facts", "What is a quarter written as a fraction?", "1/2", "1/3", "1/4", "1/5", AnswerLabel.C, Difficulty.Easy),
        Q("Number Facts", "Which is larger: 0.5 or 0.05?", "0.5", "0.05", "They're equal", "Cannot tell", AnswerLabel.A, Difficulty.Hard),
    ];

    private static ICard Q(string cat, string question, string a, string b, string c, string d, AnswerLabel correct, Difficulty diff) =>
        MultipleChoiceCard.Create(question, a, b, c, d, correct, diff, cat);
}
