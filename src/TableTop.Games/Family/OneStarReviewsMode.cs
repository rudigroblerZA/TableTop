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

    /// <summary>All one-star cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── NATURE ───────────────────────────────────────────────────────────
        R(NatureCategory, "Sunsets", Difficulty.Easy),
        R(NatureCategory, "The ocean", Difficulty.Easy),
        R(NatureCategory, "Rainbows", Difficulty.Easy),
        R(NatureCategory, "The moon", Difficulty.Medium),
        R(NatureCategory, "Autumn leaves", Difficulty.Medium),
        R(NatureCategory, "Snow (the first snow of the year, specifically)", Difficulty.Medium),
        R(NatureCategory, "Birdsong at dawn", Difficulty.Hard),
        R(NatureCategory, "Mountains", Difficulty.Medium),

        // ── SIMPLE JOYS ──────────────────────────────────────────────────────
        R(SimpleJoysCategory, "Puppies", Difficulty.Hard),
        R(SimpleJoysCategory, "Fresh bread smell", Difficulty.Medium),
        R(SimpleJoysCategory, "Naps", Difficulty.Medium),
        R(SimpleJoysCategory, "Bubble wrap", Difficulty.Easy),
        R(SimpleJoysCategory, "The other side of the pillow", Difficulty.Hard),
        R(SimpleJoysCategory, "Finding money in an old coat", Difficulty.Hard),
        R(SimpleJoysCategory, "Popcorn at the cinema", Difficulty.Easy),
        R(SimpleJoysCategory, "Hot chocolate on a cold day", Difficulty.Medium),

        // ── INSTITUTIONS ─────────────────────────────────────────────────────
        R(InstitutionsCategory, "Birthday parties", Difficulty.Easy),
        R(InstitutionsCategory, "Libraries", Difficulty.Hard),
        R(InstitutionsCategory, "Weekends", Difficulty.Medium),
        R(InstitutionsCategory, "Breakfast in bed", Difficulty.Medium),
        R(InstitutionsCategory, "High-fives", Difficulty.Medium),
        R(InstitutionsCategory, "Fireworks", Difficulty.Easy),
        R(InstitutionsCategory, "Road trips", Difficulty.Easy),
        R(InstitutionsCategory, "Grandma's cooking (a hypothetical, beloved grandma)", Difficulty.Extreme),

        // ── CONCEPTS ─────────────────────────────────────────────────────────
        R(ConceptsCategory, "Hope", Difficulty.Extreme),
        R(ConceptsCategory, "Friendship", Difficulty.Extreme),
        R(ConceptsCategory, "A good night's sleep", Difficulty.Medium),
        R(ConceptsCategory, "Nostalgia", Difficulty.Hard),
        R(ConceptsCategory, "Free time", Difficulty.Hard),
        R(ConceptsCategory, "Being tall enough to reach the top shelf", Difficulty.Medium),

        // ── CROSSOVER — review one thing AS something else ───────────────────
        X(CrossoverCategory, "Review GRAVITY as a frequent flyer.", Difficulty.Hard),
        X(CrossoverCategory, "Review the SUN as a vampire. Professional tone.", Difficulty.Medium),
        X(CrossoverCategory, "Review WINTER as a mosquito.", Difficulty.Medium),
        X(CrossoverCategory, "Review MONDAYS as someone who genuinely loves their job (find the ONE flaw).", Difficulty.Extreme),
        X(CrossoverCategory, "Review RAIN as a cat.", Difficulty.Easy),
        X(CrossoverCategory, "Review the INVENTION OF THE WHEEL as a horse.", Difficulty.Hard),
        X(CrossoverCategory, "Review MUSIC as your neighbour. Their walls are thin.", Difficulty.Medium),
        X(CrossoverCategory, "Review SLEEP as a newborn's parent. One star. Weep between sentences.", Difficulty.Hard),
    ];

    private static ICard R(string category, string subject, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>⭐ 1-star review time. The subject:</b>\n\n" +
            "<b>" + subject + "</b>\n\n" +
            "45 seconds. You booked this. You had EXPECTATIONS. It let you down, and " +
            "the review section will hear about it — specific grievances, wounded dignity, " +
            "a title like 'NEVER AGAIN'.\n\n" +
            "<i>Group votes. Pettiest plausible complaint takes the point.</i>",
            d, category);

    private static ICard X(string category, string brief, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>⭐ 1-star review, CROSSOVER EDITION:</b>\n\n" +
            "<b>" + brief + "</b>\n\n" +
            "45 seconds, fully in character, maximum disappointment.\n\n" +
            "<i>Group votes. Commitment beats comedy; both beat neither.</i>",
            d, category);
}
