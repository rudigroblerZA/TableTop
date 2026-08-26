using System.Linq;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Family Quiz — 80 multiple-choice general knowledge questions for all ages.
///
/// Designed so children and adults can compete fairly:
///   Easy    → ages 6+ can answer these (animals, colours, basic geography)
///   Medium  → ages 10+ (world capitals, science basics, history)
///   Hard    → teenagers and adults (current events, literature, deeper science)
///   Extreme → adult challenge round (obscure facts, tricky maths, culture)
///
/// Played as: each player takes the hot seat and answers questions climbing a
/// prize ladder just like Millionaire. Correct = move up; wrong = fall back to
/// guaranteed level. Family lifelines can be introduced informally.
///
/// Uses <see cref="TableTop.Games.School.Grade6QuestionBank"/>-style MultipleChoiceCard format so it can
/// be slotted into a MillionaireController with a custom question bank.
/// </summary>
public sealed class FamilyQuizMode : BaseGameModeDefinition, IQuestionBankProvider
{
    /// <inheritdoc />
    public override string Name => "Family Quiz";
    /// <inheritdoc />
    public override string Description =>
        "80 general knowledge questions for all ages — from ages 6 to adult. Who gets furthest?";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ Correct!";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel     => "→ Next question";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Animals"]     = "#66BB6A",
            ["Science"]     = "#42A5F5",
            ["History"]     = "#FFCA28",
            ["Geography"]   = "#26C6DA",
            ["Language"]    = "#AB47BC",
            ["Pop Culture"] = "#EC407A",
            ["Numbers"]     = "#FF7043",
            ["Nature"]      = "#4CAF50",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>
    /// Builds the deck, JSON-first, falling back to <see cref="FamilyQuizCardBank"/>
    /// when the file is absent (e.g. a stripped publish).
    /// </summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        FamilyQuizCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => FamilyQuizCardBank.All.Cast<ICard>().ToList().AsReadOnly();

    /// <inheritdoc />
    public IReadOnlyList<MultipleChoiceCard> GetQuestionBank() => FamilyQuizCardBank.All;
}

/// <summary>Built-in card bank for FamilyQuiz. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class FamilyQuizCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<MultipleChoiceCard> All { get; } = Build();

    private static IReadOnlyList<MultipleChoiceCard> Build() =>
    [
        // ── EASY — ages 6 and up ─────────────────────────────────────────────

        MultipleChoiceCard.Create(
            "What sound does a cow make?",
            "Moo", "Woof", "Meow", "Baa",
            AnswerLabel.A, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "How many legs does a spider have?",
            "4", "6", "8", "10",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What colour do you get when you mix red and blue?",
            "Green", "Purple", "Orange", "Brown",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which animal is the tallest in the world?",
            "Elephant", "Horse", "Giraffe", "Camel",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What do caterpillars turn into?",
            "Moths only", "Beetles", "Butterflies or moths", "Dragonflies",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "How many sides does a triangle have?",
            "2", "3", "4", "5",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What is the name of the fairy tale character who had very long hair?",
            "Cinderella", "Sleeping Beauty", "Rapunzel", "Snow White",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which planet do we live on?",
            "Mars", "Venus", "Saturn", "Earth",
            AnswerLabel.D, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What fruit is yellow and comes in a bunch?",
            "Apple", "Banana", "Mango", "Pear",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "How many days are in a week?",
            "5", "6", "7", "8",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What is 5 × 5?",
            "20", "25", "30", "10",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which ocean is the largest?",
            "Atlantic", "Indian", "Arctic", "Pacific",
            AnswerLabel.D, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What is the capital of France?",
            "Berlin", "Rome", "Paris", "Madrid",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What do bees make?",
            "Milk", "Honey", "Silk", "Wax only",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "How many months are in a year?",
            "10", "11", "12", "13",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which season comes after winter?",
            "Autumn", "Summer", "Spring", "Monsoon",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What is 10 + 10?",
            "15", "18", "20", "22",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What animal is Simba in The Lion King?",
            "Tiger", "Cheetah", "Leopard", "Lion",
            AnswerLabel.D, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "How many fingers are on two hands?",
            "8", "9", "10", "12",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What colour is grass?",
            "Blue", "Green", "Yellow", "Brown",
            AnswerLabel.B, Difficulty.Easy),

        // ── MEDIUM — ages 10 and up ──────────────────────────────────────────

        MultipleChoiceCard.Create(
            "What is the capital of Australia?",
            "Sydney", "Melbourne", "Canberra", "Brisbane",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "How many bones are in the adult human body?",
            "126", "166", "206", "256",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What gas do plants produce during photosynthesis?",
            "Carbon dioxide", "Nitrogen", "Oxygen", "Hydrogen",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which country has the largest population?",
            "USA", "India", "Russia", "China",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the chemical symbol for water?",
            "WA", "HO", "H₂O", "O₂H",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "In what year did World War II end?",
            "1943", "1944", "1945", "1946",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is 12 × 12?",
            "132", "144", "148", "156",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which author wrote Charlie and the Chocolate Factory?",
            "J.K. Rowling", "Roald Dahl", "C.S. Lewis", "Terry Pratchett",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "On which continent is Egypt?",
            "Asia", "Europe", "South America", "Africa",
            AnswerLabel.D, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the fastest land animal?",
            "Lion", "Horse", "Cheetah", "Greyhound",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "How many centimetres are in a metre?",
            "10", "50", "100", "1000",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Who wrote Romeo and Juliet?",
            "Charles Dickens", "William Shakespeare", "Jane Austen", "John Keats",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the largest organ in the human body?",
            "Heart", "Liver", "Brain", "Skin",
            AnswerLabel.D, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What does DNA stand for?",
            "Double Neutral Acid", "Deoxyribonucleic Acid", "Dual Nucleus Array", "Dynamic Neural Agent",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which planet is known as the Red Planet?",
            "Venus", "Jupiter", "Mars", "Saturn",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What sport is played at Wimbledon?",
            "Cricket", "Golf", "Badminton", "Tennis",
            AnswerLabel.D, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is 15% of 200?",
            "20", "25", "30", "35",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "How many strings does a standard guitar have?",
            "4", "5", "6", "7",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the capital of Japan?",
            "Osaka", "Kyoto", "Hiroshima", "Tokyo",
            AnswerLabel.D, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which element is represented by the symbol Fe?",
            "Fluorine", "Copper", "Iron", "Gold",
            AnswerLabel.C, Difficulty.Medium),

        // ── HARD — teens and adults ──────────────────────────────────────────

        MultipleChoiceCard.Create(
            "What is the square root of 144?",
            "11", "12", "13", "14",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "In which year did the Berlin Wall fall?",
            "1987", "1989", "1991", "1993",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the name of the process by which a solid turns directly into a gas?",
            "Evaporation", "Condensation", "Sublimation", "Oxidation",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Which Shakespeare play features the line 'To be or not to be'?",
            "Othello", "Macbeth", "King Lear", "Hamlet",
            AnswerLabel.D, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the longest river in the world?",
            "Amazon", "Congo", "Nile", "Mississippi",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "In what year did the first human walk on the moon?",
            "1965", "1967", "1969", "1971",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the powerhouse of the cell?",
            "Nucleus", "Ribosome", "Mitochondria", "Vacuole",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Which country invented the printing press?",
            "China", "Germany", "England", "Italy",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the Pythagorean theorem?",
            "a + b = c", "a² + b² = c²", "a² - b² = c", "a × b = c²",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Who painted the Mona Lisa?",
            "Michelangelo", "Raphael", "Leonardo da Vinci", "Caravaggio",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the capital of Canada?",
            "Toronto", "Vancouver", "Montreal", "Ottawa",
            AnswerLabel.D, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Which gas makes up approximately 78% of Earth's atmosphere?",
            "Oxygen", "Carbon dioxide", "Nitrogen", "Argon",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the smallest country in the world?",
            "Monaco", "San Marino", "Vatican City", "Liechtenstein",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "In what century did the Renaissance begin?",
            "12th", "13th", "14th", "15th",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is 7 factorial (7!)?",
            "2520", "5040", "720", "40320",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Which composer wrote the Moonlight Sonata?",
            "Mozart", "Bach", "Beethoven", "Chopin",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What does the Latin phrase 'carpe diem' mean?",
            "Time flies", "Seize the day", "To the stars", "Remember death",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "In which country would you find the Serengeti?",
            "Kenya", "Tanzania", "South Africa", "Ethiopia",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is a group of lions called?",
            "Pack", "Herd", "Pride", "Colony",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Which element has atomic number 79?",
            "Silver", "Platinum", "Gold", "Copper",
            AnswerLabel.C, Difficulty.Hard),

        // ── EXTREME — adult challenge ────────────────────────────────────────

        MultipleChoiceCard.Create(
            "What is the value of the mathematical constant 'e' to two decimal places?",
            "2.61", "2.71", "2.81", "3.14",
            AnswerLabel.B, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "Which philosopher wrote the Critique of Pure Reason?",
            "Descartes", "Hume", "Kant", "Hegel",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "In what year was the Magna Carta signed?",
            "1066", "1215", "1348", "1485",
            AnswerLabel.B, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the term for a word that sounds the same as another but has a different meaning and spelling?",
            "Synonym", "Homophone", "Antonym", "Palindrome",
            AnswerLabel.B, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "Which two elements make up water?",
            "Hydrogen and nitrogen", "Oxygen and carbon", "Hydrogen and oxygen", "Helium and oxygen",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the speed of light to the nearest thousand km/s?",
            "200,000 km/s", "250,000 km/s", "300,000 km/s", "350,000 km/s",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "In which decade was the internet made publicly available?",
            "1970s", "1980s", "1990s", "2000s",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the collective noun for a group of crows?",
            "A murder", "A flock", "A colony", "A gang",
            AnswerLabel.A, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "Which country has won the most FIFA World Cups?",
            "Germany", "Argentina", "Italy", "Brazil",
            AnswerLabel.D, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the hardest natural substance on Earth?",
            "Titanium", "Quartz", "Diamond", "Sapphire",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "Which ancient wonder of the world still stands today?",
            "The Colossus of Rhodes", "The Lighthouse of Alexandria",
            "The Great Pyramid of Giza", "The Hanging Gardens of Babylon",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "Who wrote One Hundred Years of Solitude?",
            "Pablo Neruda", "Jorge Luis Borges", "Gabriel García Márquez", "Isabel Allende",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the chemical symbol for gold?",
            "Go", "Gd", "Ag", "Au",
            AnswerLabel.D, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "In music, how many semitones are in an octave?",
            "8", "10", "12", "14",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the approximate diameter of the Earth in kilometres?",
            "4,700 km", "9,700 km", "12,700 km", "17,000 km",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "Which country is home to the most UNESCO World Heritage Sites?",
            "China", "Italy", "France", "Spain",
            AnswerLabel.B, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the name for a word that reads the same forwards and backwards?",
            "Homophone", "Oxymoron", "Palindrome", "Anagram",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "Which organ produces bile?",
            "Kidney", "Stomach", "Pancreas", "Liver",
            AnswerLabel.D, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "In computing, what does 'HTML' stand for?",
            "High Transfer Markup Language", "HyperText Markup Language",
            "Home Tool Markup Language", "HyperText Media Language",
            AnswerLabel.B, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the term for a type of cloud that forms at low altitude and often causes drizzle?",
            "Cumulus", "Stratus", "Cirrus", "Nimbostratus",
            AnswerLabel.B, Difficulty.Extreme),
    ];
}