using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Through the Ages — general-knowledge history and culture for the classroom:
/// the ancient world, inventions and discoveries, famous figures, and a little
/// art and culture, as multiple-choice questions dealt one at a time.
///
/// A browsable subject deck to work through by topic, not a hot-seat ladder.
/// Deliberately spread across eras and places, and kept to broadly settled,
/// curriculum-friendly facts. Four difficulties, younger pupils to the whole
/// staff room.
/// </summary>
public sealed class ThroughTheAgesMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Through the Ages";
    /// <inheritdoc />
    public override string Description =>
        "History & culture general knowledge — the ancient world, inventions, famous figures, and the arts. Multiple choice, four difficulties.";

    /// <summary>Label for a correctly answered card.</summary>
    public override string CompleteLabel => "Correct";
    /// <summary>Label for a passed card.</summary>
    public override string SkipLabel => "Pass";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [ThroughTheAgesCardBank.AncientWorldCategory] = "#FFCA28",
            [ThroughTheAgesCardBank.InventionsCategory] = "#42A5F5",
            [ThroughTheAgesCardBank.FamousFiguresCategory] = "#AB47BC",
            [ThroughTheAgesCardBank.ArtsCultureCategory] = "#EC407A",
            [ThroughTheAgesCardBank.MilestonesCategory] = "#26A69A",
        };

    /// <summary>Harder questions score more.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in Through the Ages card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        ThroughTheAgesCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => ThroughTheAgesCardBank.All;
}

/// <summary>Built-in card bank for Through the Ages.</summary>
public static class ThroughTheAgesCardBank
{
    internal const string AncientWorldCategory = "Ancient World";
    internal const string InventionsCategory = "Inventions";
    internal const string FamousFiguresCategory = "Famous Figures";
    internal const string ArtsCultureCategory = "Arts & Culture";
    internal const string MilestonesCategory = "Milestones";

    /// <summary>All cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── ANCIENT WORLD ────────────────────────────────────────────────────
        Q(AncientWorldCategory, "The pyramids of Giza were built by the ancient…?", "Greeks", "Romans", "Egyptians", "Persians", AnswerLabel.C, Difficulty.Easy),
        Q(AncientWorldCategory, "Which ancient civilisation held the first Olympic Games?", "Rome", "Greece", "Egypt", "China", AnswerLabel.B, Difficulty.Medium),
        Q(AncientWorldCategory, "Roman numerals: what number is X?", "5", "10", "50", "100", AnswerLabel.B, Difficulty.Easy),
        Q(AncientWorldCategory, "The Great Wall was built to protect which country?", "Japan", "India", "China", "Mongolia", AnswerLabel.C, Difficulty.Easy),
        Q(AncientWorldCategory, "Which writing used pictures and symbols in ancient Egypt?", "Cuneiform", "Hieroglyphs", "Runes", "Braille", AnswerLabel.B, Difficulty.Hard),
        Q(AncientWorldCategory, "Which people built roads, aqueducts, and the Colosseum?", "Greeks", "Romans", "Vikings", "Egyptians", AnswerLabel.B, Difficulty.Medium),

        // ── INVENTIONS & DISCOVERIES ─────────────────────────────────────────
        Q(InventionsCategory, "What did the printing press make easier to produce?", "Cars", "Books", "Clothes", "Ships", AnswerLabel.B, Difficulty.Easy),
        Q(InventionsCategory, "Which invention let people talk over long distances by wire?", "Radio", "Telephone", "Television", "Telegraph", AnswerLabel.B, Difficulty.Medium),
        Q(InventionsCategory, "The lightbulb is most associated with which inventor?", "Tesla", "Bell", "Edison", "Newton", AnswerLabel.C, Difficulty.Medium),
        Q(InventionsCategory, "What machine, improved by James Watt, powered the Industrial Revolution?", "Steam engine", "Jet engine", "Windmill", "Water wheel", AnswerLabel.A, Difficulty.Hard),
        Q(InventionsCategory, "The World Wide Web was invented in which decade?", "1970s", "1980s", "1990s", "2000s", AnswerLabel.B, Difficulty.Extreme),
        Q(InventionsCategory, "Which of these is used to tell direction?", "Barometer", "Compass", "Thermometer", "Telescope", AnswerLabel.B, Difficulty.Easy),

        // ── FAMOUS FIGURES ───────────────────────────────────────────────────
        Q(FamousFiguresCategory, "Who is said to have discovered gravity watching an apple fall?", "Einstein", "Newton", "Galileo", "Darwin", AnswerLabel.B, Difficulty.Medium),
        Q(FamousFiguresCategory, "Which scientist developed the theory of evolution by natural selection?", "Newton", "Darwin", "Curie", "Mendel", AnswerLabel.B, Difficulty.Hard),
        Q(FamousFiguresCategory, "Marie Curie won Nobel Prizes for her work on what?", "Gravity", "Radioactivity", "Electricity", "Genetics", AnswerLabel.B, Difficulty.Hard),
        Q(FamousFiguresCategory, "Who painted the Mona Lisa?", "Michelangelo", "Raphael", "Leonardo da Vinci", "Donatello", AnswerLabel.C, Difficulty.Medium),
        Q(FamousFiguresCategory, "Which explorer's voyages reached the Americas in 1492?", "Magellan", "Columbus", "Cook", "Vasco da Gama", AnswerLabel.B, Difficulty.Medium),

        // ── ARTS & CULTURE ───────────────────────────────────────────────────
        Q(ArtsCultureCategory, "Who wrote the plays 'Romeo and Juliet' and 'Hamlet'?", "Dickens", "Shakespeare", "Tolkien", "Austen", AnswerLabel.B, Difficulty.Easy),
        Q(ArtsCultureCategory, "How many strings does a standard guitar have?", "4", "5", "6", "8", AnswerLabel.C, Difficulty.Easy),
        Q(ArtsCultureCategory, "Which art form is ballet?", "Painting", "Dance", "Sculpture", "Poetry", AnswerLabel.B, Difficulty.Easy),
        Q(ArtsCultureCategory, "A 'haiku' is a short form of what?", "Song", "Poem", "Painting", "Dance", AnswerLabel.B, Difficulty.Hard),
        Q(ArtsCultureCategory, "The orchestra section with violins and cellos is the…?", "Brass", "Woodwind", "Strings", "Percussion", AnswerLabel.C, Difficulty.Medium),

        // ── MILESTONES — broadly settled 'firsts' and events ─────────────────
        Q(MilestonesCategory, "In 1969, humans first walked on the…?", "Mars", "Moon", "Sun", "Venus", AnswerLabel.B, Difficulty.Easy),
        Q(MilestonesCategory, "Which ship famously sank in 1912?", "Titanic", "Mayflower", "Endeavour", "Victory", AnswerLabel.A, Difficulty.Medium),
        Q(MilestonesCategory, "The Wright brothers are famous for the first powered…?", "Car", "Aeroplane", "Boat", "Rocket", AnswerLabel.B, Difficulty.Medium),
        Q(MilestonesCategory, "Which ancient wonder is the only one still largely standing today?", "Hanging Gardens", "Great Pyramid of Giza", "Colossus of Rhodes", "Lighthouse of Alexandria", AnswerLabel.B, Difficulty.Hard),
        Q(MilestonesCategory, "About how long is a century?", "10 years", "50 years", "100 years", "1000 years", AnswerLabel.C, Difficulty.Easy),
    ];

    private static ICard Q(string cat, string question, string a, string b, string c, string d, AnswerLabel correct, Difficulty diff) =>
        MultipleChoiceCard.Create(question, a, b, c, d, correct, diff, cat);
}
