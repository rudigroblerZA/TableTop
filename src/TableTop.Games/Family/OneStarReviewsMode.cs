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
    public override string Name        => "One-Star Reviews";
    /// <inheritdoc />
    public override string Description =>
        "Deliver a scathing one-star review of something universally beloved. Pettiest plausible grievance wins.";

    /// <summary>Label for the button that records the round's winning review.</summary>
    public override string CompleteLabel => "Devastating";
    /// <summary>Label for the button that skips a card.</summary>
    public override string SkipLabel     => "5 Stars, Actually";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Nature"]      = "#66BB6A",
            ["Simple Joys"] = "#FFA726",
            ["Institutions"]= "#42A5F5",
            ["Concepts"]    = "#AB47BC",
            ["Crossover"]   = "#EF5350",
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
    /// <summary>All one-star cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── NATURE ───────────────────────────────────────────────────────────
        R("Nature", "Sunsets", Difficulty.Easy),
        R("Nature", "The ocean", Difficulty.Easy),
        R("Nature", "Rainbows", Difficulty.Easy),
        R("Nature", "The moon", Difficulty.Medium),
        R("Nature", "Autumn leaves", Difficulty.Medium),
        R("Nature", "Snow (the first snow of the year, specifically)", Difficulty.Medium),
        R("Nature", "Birdsong at dawn", Difficulty.Hard),
        R("Nature", "Mountains", Difficulty.Medium),

        // ── SIMPLE JOYS ──────────────────────────────────────────────────────
        R("Simple Joys", "Puppies", Difficulty.Hard),
        R("Simple Joys", "Fresh bread smell", Difficulty.Medium),
        R("Simple Joys", "Naps", Difficulty.Medium),
        R("Simple Joys", "Bubble wrap", Difficulty.Easy),
        R("Simple Joys", "The other side of the pillow", Difficulty.Hard),
        R("Simple Joys", "Finding money in an old coat", Difficulty.Hard),
        R("Simple Joys", "Popcorn at the cinema", Difficulty.Easy),
        R("Simple Joys", "Hot chocolate on a cold day", Difficulty.Medium),

        // ── INSTITUTIONS ─────────────────────────────────────────────────────
        R("Institutions", "Birthday parties", Difficulty.Easy),
        R("Institutions", "Libraries", Difficulty.Hard),
        R("Institutions", "Weekends", Difficulty.Medium),
        R("Institutions", "Breakfast in bed", Difficulty.Medium),
        R("Institutions", "High-fives", Difficulty.Medium),
        R("Institutions", "Fireworks", Difficulty.Easy),
        R("Institutions", "Road trips", Difficulty.Easy),
        R("Institutions", "Grandma's cooking (a hypothetical, beloved grandma)", Difficulty.Extreme),

        // ── CONCEPTS ─────────────────────────────────────────────────────────
        R("Concepts", "Hope", Difficulty.Extreme),
        R("Concepts", "Friendship", Difficulty.Extreme),
        R("Concepts", "A good night's sleep", Difficulty.Medium),
        R("Concepts", "Nostalgia", Difficulty.Hard),
        R("Concepts", "Free time", Difficulty.Hard),
        R("Concepts", "Being tall enough to reach the top shelf", Difficulty.Medium),

        // ── CROSSOVER — review one thing AS something else ───────────────────
        X("Crossover", "Review GRAVITY as a frequent flyer.", Difficulty.Hard),
        X("Crossover", "Review the SUN as a vampire. Professional tone.", Difficulty.Medium),
        X("Crossover", "Review WINTER as a mosquito.", Difficulty.Medium),
        X("Crossover", "Review MONDAYS as someone who genuinely loves their job (find the ONE flaw).", Difficulty.Extreme),
        X("Crossover", "Review RAIN as a cat.", Difficulty.Easy),
        X("Crossover", "Review the INVENTION OF THE WHEEL as a horse.", Difficulty.Hard),
        X("Crossover", "Review MUSIC as your neighbour. Their walls are thin.", Difficulty.Medium),
        X("Crossover", "Review SLEEP as a newborn's parent. One star. Weep between sentences.", Difficulty.Hard),
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
