using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Backwards Story — creative storytelling from endings.
///
/// How to play:
///   1. Read only the ENDING of a story aloud.
///   2. Everyone has 90 seconds to write down what they think the FULL story was.
///   3. Everyone reads their story aloud.
///   4. Vote on which story is funniest, most creative, or most logical.
///   5. Winner gets the point.
///
/// This is essentially collaborative fan fiction with a twist. The ending is fixed,
/// but the journey is pure imagination. Some people write tragedy, some write comedy,
/// some write "how did we get HERE??" stories that make no sense but are hilarious.
///
/// Perfect for creative minds and people who love improvisation. No right answer,
/// just the most entertaining version. Great for all ages.
/// </summary>
public sealed class BackwardsStoryMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Backwards Story";
    /// <inheritdoc />
    public override string Description =>
        "Read the ending. Write the full story. Vote on which version is best.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Created";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Pass";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Dramatic"] = "#EC407A",
            ["Silly"] = "#FFCA28",
            ["Mysterious"] = "#AB47BC",
            ["Heartfelt"] = "#66BB6A",
            ["Chaotic"] = "#EF5350",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        BackwardsStoryCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => BackwardsStoryCardBank.All;
}

/// <summary>Built-in card bank for Backwards Story. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class BackwardsStoryCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── DRAMATIC ──────────────────────────────────────────────────────────
        B("Dramatic",
            "...so that's why we never speak to the zookeeper anymore.",
            Difficulty.Medium),
        B("Dramatic",
            "...and that's how I ended up married to a mime.",
            Difficulty.Hard),
        B("Dramatic",
            "...which explains why the city banned squirrels from the library.",
            Difficulty.Hard),
        B("Dramatic",
            "...so now every Tuesday is dedicated to apologizing to the neighbours.",
            Difficulty.Medium),
        B("Dramatic",
            "...and that's why I can never show my face at the supermarket again.",
            Difficulty.Medium),
        B("Dramatic",
            "...so the city council had to make it illegal just for us.",
            Difficulty.Hard),

        // ── SILLY ────────────────────────────────────────────────────────────
        B("Silly",
            "...which is why we now have seventeen ducks living in our garage.",
            Difficulty.Easy),
        B("Silly",
            "...and that's the story of how I became allergic to spaghetti.",
            Difficulty.Medium),
        B("Silly",
            "...so now the only thing my dog will eat is lobster bisque.",
            Difficulty.Easy),
        B("Silly",
            "...which explains why we legally changed our family name to 'Potato'.",
            Difficulty.Easy),
        B("Silly",
            "...and that's why I can only communicate through interpretive dance now.",
            Difficulty.Hard),
        B("Silly",
            "...so we're moving to the moon next month.",
            Difficulty.Medium),

        // ── MYSTERIOUS ──────────────────────────────────────────────────────
        B("Mysterious",
            "...and I still don't know who left the mysterious note under my pillow.",
            Difficulty.Hard),
        B("Mysterious",
            "...which is how I discovered the secret door in the library.",
            Difficulty.Medium),
        B("Mysterious",
            "...and nobody has ever been able to explain what happened that night.",
            Difficulty.Hard),
        B("Mysterious",
            "...so I buried the evidence in the garden and haven't spoken of it since.",
            Difficulty.Medium),
        B("Mysterious",
            "...which led me to discover that my best friend isn't even human.",
            Difficulty.Hard),

        // ── HEARTFELT ────────────────────────────────────────────────────────
        B("Heartfelt",
            "...and that's when I realized what family really means.",
            Difficulty.Medium),
        B("Heartfelt",
            "...so I finally told them the truth, and everything changed for the better.",
            Difficulty.Medium),
        B("Heartfelt",
            "...and I've never been happier in my entire life.",
            Difficulty.Easy),
        B("Heartfelt",
            "...which is why I'm grateful for every single day with them.",
            Difficulty.Medium),
        B("Heartfelt",
            "...and we both learned that forgiveness is the greatest gift.",
            Difficulty.Medium),

        // ── CHAOTIC ──────────────────────────────────────────────────────────
        B("Chaotic",
            "...and that's how we ended up on national news three times in one week.",
            Difficulty.Hard),
        B("Chaotic",
            "...so the fire department now has our house memorized.",
            Difficulty.Medium),
        B("Chaotic",
            "...which is why we've been banned from four different venues.",
            Difficulty.Medium),
        B("Chaotic",
            "...and somehow we all survived with only minor injuries.",
            Difficulty.Medium),
    ];

    private static ICard B(string category, string ending, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>How did we get here?</b>\n\n" +
            "That's the ENDING of the story: \"" + ending + "\"\n\n" +
            "<b>Your turn:</b> You have 90 seconds to write the FULL story that leads to this ending.\n\n" +
            "Then everyone reads their story aloud, and vote on which one is best: funniest, most creative, or most logical.",
            d, category);
}
