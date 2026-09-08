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
    public override string Name => "Act It Out";
    /// <inheritdoc />
    public override string Description =>
        "Charades for the family — mime it, no words, no sounds. First to guess scores. Answer's on the back.";

    /// <summary>Label for a guessed card.</summary>
    public override string CompleteLabel => "Guessed!";
    /// <summary>Label for a card nobody got.</summary>
    public override string SkipLabel => "Nobody got it";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [ActItOutCardBank.AnimalsCategory] = "#66BB6A",
            [ActItOutCardBank.ActionsCategory] = "#42A5F5",
            [ActItOutCardBank.JobsCategory] = "#FFA726",
            [ActItOutCardBank.MoviesShowsCategory] = "#AB47BC",
            [ActItOutCardBank.WholeScenesCategory] = "#EF5350",
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
    internal const string AnimalsCategory = "Animals";
    internal const string ActionsCategory = "Actions";
    internal const string JobsCategory = "Jobs";
    internal const string MoviesShowsCategory = "Movies & Shows";
    internal const string WholeScenesCategory = "Whole Scenes";

    private const string Deck = "Act It Out";

    /// <summary>All cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)

            // ── ANIMALS — easiest, great for the youngest ────────────────────────
            .Category(AnimalsCategory)
            .Card(AnimalsCategory + " charade", Body(AnimalsCategory, "A monkey"), Difficulty.Easy)
            .Card(AnimalsCategory + " charade", Body(AnimalsCategory, "A snake"), Difficulty.Easy)
            .Card(AnimalsCategory + " charade", Body(AnimalsCategory, "A kangaroo"), Difficulty.Easy)
            .Card(AnimalsCategory + " charade", Body(AnimalsCategory, "A chicken laying an egg"), Difficulty.Medium)
            .Card(AnimalsCategory + " charade", Body(AnimalsCategory, "A cat knocking things off a table"), Difficulty.Medium)
            .Card(AnimalsCategory + " charade", Body(AnimalsCategory, "A sloth trying to catch a bus"), Difficulty.Hard)

            // ── ACTIONS — verbs and everyday doings ──────────────────────────────
            .Category(ActionsCategory)
            .Card(ActionsCategory + " charade", Body(ActionsCategory, "Brushing your teeth"), Difficulty.Easy)
            .Card(ActionsCategory + " charade", Body(ActionsCategory, "Swimming"), Difficulty.Easy)
            .Card(ActionsCategory + " charade", Body(ActionsCategory, "Tiptoeing past someone asleep"), Difficulty.Medium)
            .Card(ActionsCategory + " charade", Body(ActionsCategory, "Trying to open a jar that won't budge"), Difficulty.Medium)
            .Card(ActionsCategory + " charade", Body(ActionsCategory, "Stepping on a plug in bare feet"), Difficulty.Hard)
            .Card(ActionsCategory + " charade", Body(ActionsCategory, "Pretending you knew the answer all along"), Difficulty.Hard)

            // ── JOBS — occupations to mime ───────────────────────────────────────
            .Category(JobsCategory)
            .Card(JobsCategory + " charade", Body(JobsCategory, "A chef"), Difficulty.Easy)
            .Card(JobsCategory + " charade", Body(JobsCategory, "A firefighter"), Difficulty.Easy)
            .Card(JobsCategory + " charade", Body(JobsCategory, "A hairdresser"), Difficulty.Medium)
            .Card(JobsCategory + " charade", Body(JobsCategory, "A traffic officer directing cars"), Difficulty.Medium)
            .Card(JobsCategory + " charade", Body(JobsCategory, "A very unenthusiastic tour guide"), Difficulty.Hard)

            // ── MOVIES & SHOWS — mime the title/idea, no words ───────────────────
            .Category(MoviesShowsCategory)
            .Card(MoviesShowsCategory + " charade", Body(MoviesShowsCategory, "A superhero movie"), Difficulty.Medium)
            .Card(MoviesShowsCategory + " charade", Body(MoviesShowsCategory, "A nature documentary"), Difficulty.Medium)
            .Card(MoviesShowsCategory + " charade", Body(MoviesShowsCategory, "A cooking competition show"), Difficulty.Hard)
            .Card(MoviesShowsCategory + " charade", Body(MoviesShowsCategory, "A horror film where nothing works out"), Difficulty.Hard)

            // ── WHOLE SCENES — the absurd showstoppers ───────────────────────────
            .Category(WholeScenesCategory)
            .Card(WholeScenesCategory + " charade", Body(WholeScenesCategory, "A penguin realising it left the oven on"), Difficulty.Extreme)
            .Card(WholeScenesCategory + " charade", Body(WholeScenesCategory, "A robot slowly running out of battery"), Difficulty.Extreme)
            .Card(WholeScenesCategory + " charade", Body(WholeScenesCategory, "Someone winning the lottery on the bus, quietly"), Difficulty.Extreme)
            .Card(WholeScenesCategory + " charade", Body(WholeScenesCategory, "A wizard whose spell went slightly wrong"), Difficulty.Extreme)

            .Build();

    private static string Body(string category, string answer) =>
        "<b>🎭 ACT IT OUT — " + category.ToUpperInvariant() + "</b>\n\n" +
        "<i>Read silently, then act it out — no words, no sounds. 60 seconds. First to guess scores with you.</i>\n\n" +
        "Answer: " + answer;
}
