using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Act It Out — proper charades for the whole family. No famous people
/// required (that's Celebrity Impersonator's job); this is animals, actions,
/// jobs, movies, and gloriously specific everyday scenes to mime.
///
/// How to play:
///   1. The actor draws a card and reads it SILENTLY — nobody else sees it.
///   2. Flip the card face-down. On "go", act it out with NO words and NO
///      sounds — just movement and mime. (Little kids may make noises; the
///      table decides how strict to be.)
///   3. Everyone else shouts guesses. First correct guess scores a point for
///      the guesser AND the actor. 60-second limit per card.
///   4. Pass the deck to the next actor.
///
/// The answer is on the back of every card, so there's never an argument about
/// what it "was meant to be." Difficulty rises from one-word animals up to
/// whole absurd scenes ("a penguin realising it left the oven on").
/// </summary>
public sealed class ActItOutMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name        => "Act It Out";
    /// <inheritdoc />
    public override string Description =>
        "Charades for the family — mime it, no words, no sounds. First to guess scores. Answer's on the back.";

    /// <summary>Label for a guessed card.</summary>
    public override string CompleteLabel => "Guessed!";
    /// <summary>Label for a card nobody got.</summary>
    public override string SkipLabel     => "Nobody got it";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Animals"]  = "#66BB6A",
            ["Actions"]  = "#42A5F5",
            ["Jobs"]     = "#FFA726",
            ["Movies & Shows"] = "#AB47BC",
            ["Whole Scenes"] = "#EF5350",
        };

    /// <summary>Harder mimes score more.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in Act It Out card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        ActItOutCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => ActItOutCardBank.All;
}

/// <summary>Built-in card bank for Act It Out.</summary>
public static class ActItOutCardBank
{
    /// <summary>All cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── ANIMALS — easiest, great for the youngest ────────────────────────
        A("Animals", "A monkey", Difficulty.Easy),
        A("Animals", "A snake", Difficulty.Easy),
        A("Animals", "A kangaroo", Difficulty.Easy),
        A("Animals", "A chicken laying an egg", Difficulty.Medium),
        A("Animals", "A cat knocking things off a table", Difficulty.Medium),
        A("Animals", "A sloth trying to catch a bus", Difficulty.Hard),

        // ── ACTIONS — verbs and everyday doings ──────────────────────────────
        A("Actions", "Brushing your teeth", Difficulty.Easy),
        A("Actions", "Swimming", Difficulty.Easy),
        A("Actions", "Tiptoeing past someone asleep", Difficulty.Medium),
        A("Actions", "Trying to open a jar that won't budge", Difficulty.Medium),
        A("Actions", "Stepping on a plug in bare feet", Difficulty.Hard),
        A("Actions", "Pretending you knew the answer all along", Difficulty.Hard),

        // ── JOBS — occupations to mime ───────────────────────────────────────
        A("Jobs", "A chef", Difficulty.Easy),
        A("Jobs", "A firefighter", Difficulty.Easy),
        A("Jobs", "A hairdresser", Difficulty.Medium),
        A("Jobs", "A traffic officer directing cars", Difficulty.Medium),
        A("Jobs", "A very unenthusiastic tour guide", Difficulty.Hard),

        // ── MOVIES & SHOWS — mime the title/idea, no words ───────────────────
        A("Movies & Shows", "A superhero movie", Difficulty.Medium),
        A("Movies & Shows", "A nature documentary", Difficulty.Medium),
        A("Movies & Shows", "A cooking competition show", Difficulty.Hard),
        A("Movies & Shows", "A horror film where nothing works out", Difficulty.Hard),

        // ── WHOLE SCENES — the absurd showstoppers ───────────────────────────
        A("Whole Scenes", "A penguin realising it left the oven on", Difficulty.Extreme),
        A("Whole Scenes", "A robot slowly running out of battery", Difficulty.Extreme),
        A("Whole Scenes", "Someone winning the lottery on the bus, quietly", Difficulty.Extreme),
        A("Whole Scenes", "A wizard whose spell went slightly wrong", Difficulty.Extreme),
    ];

    private static ICard A(string category, string answer, Difficulty d) =>
        StandardCard.Create(
            category + " charade",
            "<b>🎭 ACT IT OUT — " + category.ToUpperInvariant() + "</b>\n\n" +
            "<i>Read silently, then act it out — no words, no sounds. 60 seconds. First to guess scores with you.</i>\n\n" +
            "Answer: " + answer,
            d, category);
}
