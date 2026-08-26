using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// World Explorer — general-knowledge geography for the classroom. Capitals,
/// countries, landmarks, rivers, and natural wonders, as multiple-choice
/// questions that deal one at a time so a class can work through them together
/// or in teams.
///
/// Unlike Estimation Station (reasoning about numbers) or Quiz Night (a single
/// hot-seat ladder), this is a browsable subject deck: pick geography, deal a
/// card, discuss, reveal. Four difficulties from "which is a country" up to
/// deeper cuts, so it stretches from younger pupils to the class know-it-all.
/// </summary>
public sealed class WorldExplorerMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name        => "World Explorer";
    /// <inheritdoc />
    public override string Description =>
        "Geography general knowledge — capitals, countries, landmarks, and natural wonders. Multiple choice, four difficulties.";

    /// <summary>Label for a correctly answered card.</summary>
    public override string CompleteLabel => "Correct";
    /// <summary>Label for a passed card.</summary>
    public override string SkipLabel     => "Pass";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Capitals"]   = "#26C6DA",
            ["Countries"]  = "#66BB6A",
            ["Landmarks"]  = "#FFA726",
            ["Natural World"] = "#4CAF50",
            ["On the Map"] = "#42A5F5",
        };

    /// <summary>Harder questions score more.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in World Explorer card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        WorldExplorerCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => WorldExplorerCardBank.All;
}

/// <summary>Built-in card bank for World Explorer.</summary>
public static class WorldExplorerCardBank
{
    /// <summary>All cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── CAPITALS ─────────────────────────────────────────────────────────
        Q("Capitals", "What is the capital of France?", "Lyon", "Paris", "Marseille", "Nice", AnswerLabel.B, Difficulty.Easy),
        Q("Capitals", "What is the capital of Japan?", "Osaka", "Kyoto", "Tokyo", "Nagoya", AnswerLabel.C, Difficulty.Easy),
        Q("Capitals", "What is the capital of Australia?", "Sydney", "Melbourne", "Perth", "Canberra", AnswerLabel.D, Difficulty.Medium),
        Q("Capitals", "What is the capital of Canada?", "Toronto", "Ottawa", "Vancouver", "Montreal", AnswerLabel.B, Difficulty.Medium),
        Q("Capitals", "What is the capital of Egypt?", "Cairo", "Alexandria", "Giza", "Luxor", AnswerLabel.A, Difficulty.Medium),
        Q("Capitals", "What is the capital of Brazil?", "Rio de Janeiro", "São Paulo", "Brasília", "Salvador", AnswerLabel.C, Difficulty.Hard),
        Q("Capitals", "What is the capital of New Zealand?", "Auckland", "Wellington", "Christchurch", "Hamilton", AnswerLabel.B, Difficulty.Hard),

        // ── COUNTRIES ────────────────────────────────────────────────────────
        Q("Countries", "Which of these is a country?", "Rome", "Egypt", "Paris", "London", AnswerLabel.B, Difficulty.Easy),
        Q("Countries", "Which country is shaped like a boot?", "Spain", "Greece", "Italy", "Portugal", AnswerLabel.C, Difficulty.Easy),
        Q("Countries", "Which is the largest country by area?", "China", "USA", "Canada", "Russia", AnswerLabel.D, Difficulty.Medium),
        Q("Countries", "Which country has the most people?", "India", "China", "USA", "Indonesia", AnswerLabel.A, Difficulty.Hard),
        Q("Countries", "On which continent is Kenya?", "Asia", "South America", "Africa", "Europe", AnswerLabel.C, Difficulty.Easy),
        Q("Countries", "Which country is both in Europe and Asia?", "Egypt", "Turkey", "Greece", "Iran", AnswerLabel.B, Difficulty.Hard),

        // ── LANDMARKS ────────────────────────────────────────────────────────
        Q("Landmarks", "In which city is the Eiffel Tower?", "London", "Rome", "Paris", "Berlin", AnswerLabel.C, Difficulty.Easy),
        Q("Landmarks", "The Great Pyramid stands near which city?", "Cairo", "Athens", "Baghdad", "Istanbul", AnswerLabel.A, Difficulty.Medium),
        Q("Landmarks", "The Statue of Liberty is in which country?", "France", "UK", "USA", "Canada", AnswerLabel.C, Difficulty.Easy),
        Q("Landmarks", "The Colosseum is found in which country?", "Greece", "Italy", "Spain", "Turkey", AnswerLabel.B, Difficulty.Medium),
        Q("Landmarks", "Machu Picchu was built by which civilisation?", "Aztec", "Maya", "Inca", "Olmec", AnswerLabel.C, Difficulty.Hard),

        // ── NATURAL WORLD ────────────────────────────────────────────────────
        Q("Natural World", "What is the longest river in the world?", "Amazon", "Nile", "Yangtze", "Mississippi", AnswerLabel.B, Difficulty.Medium),
        Q("Natural World", "What is the largest ocean?", "Atlantic", "Indian", "Arctic", "Pacific", AnswerLabel.D, Difficulty.Easy),
        Q("Natural World", "What is the tallest mountain above sea level?", "K2", "Everest", "Kilimanjaro", "Denali", AnswerLabel.B, Difficulty.Easy),
        Q("Natural World", "The Sahara Desert is on which continent?", "Asia", "Australia", "Africa", "South America", AnswerLabel.C, Difficulty.Easy),
        Q("Natural World", "What is the largest desert on Earth (deserts can be cold)?", "Sahara", "Gobi", "Arabian", "Antarctica", AnswerLabel.D, Difficulty.Extreme),
        Q("Natural World", "Which rainforest is the world's largest?", "Congo", "Amazon", "Daintree", "Borneo", AnswerLabel.B, Difficulty.Medium),

        // ── ON THE MAP — flags, oceans, borders ──────────────────────────────
        Q("On the Map", "How many continents are there?", "5", "6", "7", "8", AnswerLabel.C, Difficulty.Easy),
        Q("On the Map", "Which line divides Earth into north and south?", "Prime Meridian", "Equator", "Tropic of Cancer", "Axis", AnswerLabel.B, Difficulty.Medium),
        Q("On the Map", "Which two colours are on the flag of Japan?", "Red and white", "Blue and white", "Red and yellow", "Green and white", AnswerLabel.A, Difficulty.Easy),
        Q("On the Map", "Which country has a maple leaf on its flag?", "USA", "Canada", "Lebanon", "Norway", AnswerLabel.B, Difficulty.Easy),
        Q("On the Map", "Which sea lies between Europe and Africa?", "Red Sea", "Black Sea", "Mediterranean", "Caspian", AnswerLabel.C, Difficulty.Medium),
    ];

    private static ICard Q(string cat, string question, string a, string b, string c, string d, AnswerLabel correct, Difficulty diff) =>
        MultipleChoiceCard.Create(question, a, b, c, d, correct, diff, cat);
}
