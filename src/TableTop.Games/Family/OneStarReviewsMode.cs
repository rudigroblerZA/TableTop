using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// One-Star Reviews — deliver a devastating review of something beloved.
///
/// How to play:
///   1. Draw a card naming something the whole world agrees is wonderful.
///   2. You have 45 seconds to deliver its scathing ONE-STAR review, in the
///      voice of the world's most disappointed customer. Stay committed:
///      you booked this sunset, and it let you down.
///   3. The group votes. Pettiest plausible grievance wins the point.
///
/// House style: great one-star reviews are SPECIFIC ("the moon was advertised
/// as full") and aggrieved, never mean-spirited toward people.
/// </summary>
public sealed class OneStarReviewsMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "One-Star Reviews";
    /// <inheritdoc />
    public override string Description =>
        "Deliver a scathing one-star review of something universally beloved. Pettiest plausible grievance wins.";

    /// <summary>Label for the button that records the round's winning review.</summary>
    public override string CompleteLabel => "Devastating";
    /// <summary>Label for the button that skips a card.</summary>
    public override string SkipLabel => "5 Stars, Actually";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [OneStarReviewsCardBank.NatureCategory] = "#66BB6A",
            [OneStarReviewsCardBank.SimpleJoysCategory] = "#FFA726",
            [OneStarReviewsCardBank.InstitutionsCategory] = "#42A5F5",
            [OneStarReviewsCardBank.ConceptsCategory] = "#AB47BC",
            [OneStarReviewsCardBank.CrossoverCategory] = "#EF5350",
        };

    /// <summary>One point to the group's voted-most-devastating review.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Returns the built-in one-star card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        OneStarReviewsCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => OneStarReviewsCardBank.All;
}

/// <summary>Built-in card bank for One-Star Reviews.</summary>
public static class OneStarReviewsCardBank
{
    internal const string NatureCategory = "Nature";
    internal const string SimpleJoysCategory = "Simple Joys";
    internal const string InstitutionsCategory = "Institutions";
    internal const string ConceptsCategory = "Concepts";
    internal const string CrossoverCategory = "Crossover";

    private const string Deck = "One-Star Reviews";

    /// <summary>All one-star cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)
        // ── NATURE ───────────────────────────────────────────────────────────
            .Category(NatureCategory)
            .Card(NatureCategory, ReviewBody("Sunsets"), Difficulty.Easy)
            .Card(NatureCategory, ReviewBody("The ocean"), Difficulty.Easy)
            .Card(NatureCategory, ReviewBody("Rainbows"), Difficulty.Easy)
            .Card(NatureCategory, ReviewBody("The moon"), Difficulty.Medium)
            .Card(NatureCategory, ReviewBody("Autumn leaves"), Difficulty.Medium)
            .Card(NatureCategory, ReviewBody("Snow (the first snow of the year, specifically)"), Difficulty.Medium)
            .Card(NatureCategory, ReviewBody("Birdsong at dawn"), Difficulty.Hard)
            .Card(NatureCategory, ReviewBody("Mountains"), Difficulty.Medium)

        // ── SIMPLE JOYS ──────────────────────────────────────────────────────
            .Category(SimpleJoysCategory)
            .Card(SimpleJoysCategory, ReviewBody("Puppies"), Difficulty.Hard)
            .Card(SimpleJoysCategory, ReviewBody("Fresh bread smell"), Difficulty.Medium)
            .Card(SimpleJoysCategory, ReviewBody("Naps"), Difficulty.Medium)
            .Card(SimpleJoysCategory, ReviewBody("Bubble wrap"), Difficulty.Easy)
            .Card(SimpleJoysCategory, ReviewBody("The other side of the pillow"), Difficulty.Hard)
            .Card(SimpleJoysCategory, ReviewBody("Finding money in an old coat"), Difficulty.Hard)
            .Card(SimpleJoysCategory, ReviewBody("Popcorn at the cinema"), Difficulty.Easy)
            .Card(SimpleJoysCategory, ReviewBody("Hot chocolate on a cold day"), Difficulty.Medium)

        // ── INSTITUTIONS ─────────────────────────────────────────────────────
            .Category(InstitutionsCategory)
            .Card(InstitutionsCategory, ReviewBody("Birthday parties"), Difficulty.Easy)
            .Card(InstitutionsCategory, ReviewBody("Libraries"), Difficulty.Hard)
            .Card(InstitutionsCategory, ReviewBody("Weekends"), Difficulty.Medium)
            .Card(InstitutionsCategory, ReviewBody("Breakfast in bed"), Difficulty.Medium)
            .Card(InstitutionsCategory, ReviewBody("High-fives"), Difficulty.Medium)
            .Card(InstitutionsCategory, ReviewBody("Fireworks"), Difficulty.Easy)
            .Card(InstitutionsCategory, ReviewBody("Road trips"), Difficulty.Easy)
            .Card(InstitutionsCategory, ReviewBody("Grandma's cooking (a hypothetical, beloved grandma)"), Difficulty.Extreme)

        // ── CONCEPTS ─────────────────────────────────────────────────────────
            .Category(ConceptsCategory)
            .Card(ConceptsCategory, ReviewBody("Hope"), Difficulty.Extreme)
            .Card(ConceptsCategory, ReviewBody("Friendship"), Difficulty.Extreme)
            .Card(ConceptsCategory, ReviewBody("A good night's sleep"), Difficulty.Medium)
            .Card(ConceptsCategory, ReviewBody("Nostalgia"), Difficulty.Hard)
            .Card(ConceptsCategory, ReviewBody("Free time"), Difficulty.Hard)
            .Card(ConceptsCategory, ReviewBody("Being tall enough to reach the top shelf"), Difficulty.Medium)

        // ── CROSSOVER — review one thing AS something else ───────────────────
            .Category(CrossoverCategory)
            .Card(CrossoverCategory, CrossoverBody("Review GRAVITY as a frequent flyer."), Difficulty.Hard)
            .Card(CrossoverCategory, CrossoverBody("Review the SUN as a vampire. Professional tone."), Difficulty.Medium)
            .Card(CrossoverCategory, CrossoverBody("Review WINTER as a mosquito."), Difficulty.Medium)
            .Card(CrossoverCategory, CrossoverBody("Review MONDAYS as someone who genuinely loves their job (find the ONE flaw)."), Difficulty.Extreme)
            .Card(CrossoverCategory, CrossoverBody("Review RAIN as a cat."), Difficulty.Easy)
            .Card(CrossoverCategory, CrossoverBody("Review the INVENTION OF THE WHEEL as a horse."), Difficulty.Hard)
            .Card(CrossoverCategory, CrossoverBody("Review MUSIC as your neighbour. Their walls are thin."), Difficulty.Medium)
            .Card(CrossoverCategory, CrossoverBody("Review SLEEP as a newborn's parent. One star. Weep between sentences."), Difficulty.Hard)

            .Build();

    private static string ReviewBody(string subject) =>
        "<b>⭐ 1-star review time. The subject:</b>\n\n" +
        "<b>" + subject + "</b>\n\n" +
        "45 seconds. You booked this. You had EXPECTATIONS. It let you down, and " +
        "the review section will hear about it — specific grievances, wounded dignity, " +
        "a title like 'NEVER AGAIN'.\n\n" +
        "<i>Group votes. Pettiest plausible complaint takes the point.</i>";

    private static string CrossoverBody(string brief) =>
        "<b>⭐ 1-star review, CROSSOVER EDITION:</b>\n\n" +
        "<b>" + brief + "</b>\n\n" +
        "45 seconds, fully in character, maximum disappointment.\n\n" +
        "<i>Group votes. Commitment beats comedy; both beat neither.</i>";
}
