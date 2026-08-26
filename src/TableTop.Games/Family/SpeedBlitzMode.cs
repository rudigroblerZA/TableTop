using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Speed Blitz — Rapid-fire timed challenges for quick thinking.
///
/// How to play:
///   1. Read the challenge aloud.
///   2. Set a timer (15, 30, or 60 seconds depending on difficulty).
///   3. The active player attempts the challenge.
///   4. Did they finish in time? Correct? Award a point.
///
/// Challenges range from naming things in a category ("Name 5 colours in 20 seconds")
/// through word games ("List words that start with B") to trivia sprint ("Answer 3 questions about flags").
///
/// Age-inclusive: mix of physical (say words), mental (solve math), and creative (make rhymes).
/// No winners or losers — just pure speed and fun.
/// </summary>
public sealed class SpeedBlitzMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name        => "Speed Blitz";
    /// <inheritdoc />
    public override string Description =>
        "Rapid-fire timed challenges — name things, answer trivia, solve riddles, all against the clock.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ Success";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel     => "✗ Timeout";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Name"]    = "#42A5F5",
            ["Words"]   = "#66BB6A",
            ["Trivia"]  = "#FFA726",
            ["Riddle"]  = "#EC407A",
            ["Math"]    = "#AB47BC",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        SpeedBlitzCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => SpeedBlitzCardBank.All;
}

/// <summary>Built-in card bank for Speed Blitz.</summary>
public static class SpeedBlitzCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── NAME (categories, 20-30 sec) ────────────────────────────────────
        Card("Name", "Animals",
            "Name 5 animals that start with 'M' in 20 seconds.",
            Difficulty.Easy),
        Card("Name", "Countries",
            "Name 4 countries in Africa in 25 seconds.",
            Difficulty.Easy),
        Card("Name", "Breakfast Foods",
            "List 5 things people eat for breakfast in 20 seconds.",
            Difficulty.Easy),
        Card("Name", "Colors",
            "Name 6 shades of blue in 25 seconds.",
            Difficulty.Medium),
        Card("Name", "Celebrities",
            "Name 5 actors from superhero films in 20 seconds.",
            Difficulty.Medium),
        Card("Name", "Sports",
            "List 4 sports played with a ball in 15 seconds.",
            Difficulty.Easy),
        Card("Name", "Fruits",
            "Name 7 fruits in 20 seconds.",
            Difficulty.Easy),
        Card("Name", "Brands",
            "Name 5 car manufacturers in 20 seconds.",
            Difficulty.Medium),
        Card("Name", "Things in a Kitchen",
            "List 6 kitchen appliances in 20 seconds.",
            Difficulty.Easy),
        Card("Name", "Professions",
            "Name 8 jobs that require university training in 25 seconds.",
            Difficulty.Medium),

        // ── WORDS (rhymes, anagrams, word chains) ─────────────────────────
        Card("Words", "Rhyme Sprint",
            "Say as many words that rhyme with 'MAKE' as you can in 20 seconds.",
            Difficulty.Easy),
        Card("Words", "Same Start",
            "List words that start with 'ST' in 25 seconds — get at least 5.",
            Difficulty.Medium),
        Card("Words", "Opposites",
            "Give the opposite of 5 words we call out: START! (opposite: STOP). Go!",
            Difficulty.Easy),
        Card("Words", "Hidden Word",
            "Find a word hidden in: SCRAMBLED. (Answer: CRUMBLED/CRAB/BRED/SCAM/CRAM) — 15 seconds.",
            Difficulty.Medium),
        Card("Words", "Alliteration",
            "Think of 3 sentences where most words start with the same letter in 20 seconds.",
            Difficulty.Medium),
        Card("Words", "Vowel Count",
            "How many vowels in the word BEAUTIFUL? (Answer: 5 — E, A, U, I, U) — 10 seconds.",
            Difficulty.Easy),
        Card("Words", "Missing Letter",
            "C_T, D_G, B_T, F_SH — fill in the missing letters (5 seconds each).",
            Difficulty.Easy),
        Card("Words", "Compound Words",
            "List 4 compound words (e.g., SUNFLOWER) in 20 seconds.",
            Difficulty.Medium),

        // ── TRIVIA (quick answer rounds) ────────────────────────────────────
        Card("Trivia", "Olympics",
            "Which country hosted the 2016 Summer Olympics? (Answer: Brazil) — 5 seconds.",
            Difficulty.Easy),
        Card("Trivia", "Shakespeare",
            "Complete: 'To be or ___?' (Answer: not to be) — 5 seconds.",
            Difficulty.Easy),
        Card("Trivia", "Planets",
            "How many planets orbit the Sun? (Answer: 8) — 5 seconds.",
            Difficulty.Easy),
        Card("Trivia", "Literature",
            "Who wrote Pride and Prejudice? (Answer: Jane Austen) — 5 seconds.",
            Difficulty.Medium),
        Card("Trivia", "Science",
            "What's the chemical symbol for gold? (Answer: Au) — 5 seconds.",
            Difficulty.Medium),
        Card("Trivia", "Geography",
            "What's the capital of Australia? (Answer: Canberra) — 5 seconds.",
            Difficulty.Medium),
        Card("Trivia", "History",
            "In what year did the Titanic sink? (Answer: 1912) — 5 seconds.",
            Difficulty.Easy),

        // ── RIDDLE (solve or think fast) ────────────────────────────────────
        Card("Riddle", "Classic",
            "What has hands but can't clap? (Answer: Clock) — 20 seconds.",
            Difficulty.Easy),
        Card("Riddle", "Logic",
            "I'm tall when young, short when old. What am I? (Answer: Candle) — 20 seconds.",
            Difficulty.Easy),
        Card("Riddle", "Wordplay",
            "What gets wet while drying? (Answer: Towel) — 15 seconds.",
            Difficulty.Easy),
        Card("Riddle", "Lateral Thinking",
            "A man pushes his car to a hotel and tells the owner he's bankrupt. Why? (Answer: Playing Monopoly) — 30 seconds.",
            Difficulty.Hard),
        Card("Riddle", "Clever",
            "The more you take, the more you leave behind. What am I? (Answer: Footprints) — 20 seconds.",
            Difficulty.Medium),
        Card("Riddle", "Tricky",
            "A doctor and a boy were fishing. The boy was the doctor's son, but the doctor was not the boy's father. Who was the doctor? (Answer: His mother) — 30 seconds.",
            Difficulty.Hard),

        // ── MATH (quick calculation) ────────────────────────────────────────
        Card("Math", "Times Tables",
            "What is 9 × 8? (Answer: 72) — 5 seconds.",
            Difficulty.Easy),
        Card("Math", "Quick Division",
            "What is 144 ÷ 12? (Answer: 12) — 5 seconds.",
            Difficulty.Easy),
        Card("Math", "Percentage",
            "What is 10% of 250? (Answer: 25) — 10 seconds.",
            Difficulty.Easy),
        Card("Math", "Two-Step",
            "If a book costs £8 and is 25% off, what's the sale price? (Answer: £6) — 15 seconds.",
            Difficulty.Medium),
        Card("Math", "Mental Math",
            "Add these: 47 + 38 + 15 = ? (Answer: 100) — 10 seconds.",
            Difficulty.Medium),
        Card("Math", "Fraction",
            "What is 3/4 of 40? (Answer: 30) — 10 seconds.",
            Difficulty.Medium),
        Card("Math", "Geometry",
            "A square has sides of 6cm. What's its area? (Answer: 36 cm²) — 10 seconds.",
            Difficulty.Easy),
        Card("Math", "Sequence",
            "What's the next number: 2, 4, 8, 16, ___? (Answer: 32) — 10 seconds.",
            Difficulty.Easy),
    ];

    private static ICard Card(string category, string title, string prompt, Difficulty d) =>
        StandardCard.Create(
            title,
            prompt + "\n\n<b>Set timer for the suggested time. Go!</b>",
            d, category);
}
