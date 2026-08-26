using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// The Paradox — solve logical impossibilities, paradoxes, and brain-bender riddles.
///
/// How to play:
///   1. Read the paradox or impossible scenario aloud.
///   2. Everyone has 2 minutes to come up with the BEST solution or explanation.
///   3. Read aloud. Vote on best answer: most logical, most creative, or most hilarious.
///   4. Winner gets the point.
///
/// Some are philosophical ("If a tree falls and nobody hears it, does it make a sound?"),
/// some are logical ("A man pushes a car. His wife pushes it from inside. Who pushed harder?"),
/// some are just silly ("How can mirrors reverse left-right but not top-bottom?"). The
/// answers matter less than the discussion and creativity.
///
/// Great for critical thinking, lateral problem-solving, and finding that your friends'
/// minds work in WILDLY different ways. Works for all ages and gets better in smart groups
/// that actually like debating philosophical nonsense.
/// </summary>
public sealed class TheParadoxMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "The Paradox";
    /// <inheritdoc />
    public override string Description =>
        "Solve an impossible scenario. Best logic, creativity, or absurdity wins.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Solved";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Logic"] = "#42A5F5",
            ["Philosophy"] = "#AB47BC",
            ["Physics"] = "#FFA726",
            ["Riddle"] = "#FFCA28",
            ["Impossible"] = "#EF5350",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        TheParadoxCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => TheParadoxCardBank.All;
}

/// <summary>Built-in card bank for The Paradox. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class TheParadoxCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── LOGIC ────────────────────────────────────────────────────────────
        P("Logic",
            "A man goes to bed at 8pm, sets his alarm for 9am, and wakes up at 9pm. How is this possible?",
            "Hint: It's not the next day.",
            Difficulty.Medium),
        P("Logic",
            "A man's wife pushes a car, and he pushes it from the inside. The car moves. Who pushed harder?",
            "Hint: It's not about force.",
            Difficulty.Hard),
        P("Logic",
            "How can you go 10 days without sleep and not feel tired?",
            "Hint: You're not breaking any laws of physics.",
            Difficulty.Easy),
        P("Logic",
            "If you're running a race and you pass the person in 2nd place, what place are you in now?",
            "Hint: The obvious answer might be wrong.",
            Difficulty.Easy),

        // ── PHILOSOPHY ────────────────────────────────────────────────────────
        P("Philosophy",
            "If a tree falls in a forest and nobody hears it, does it make a sound?",
            "Is sound a physical wave or a conscious experience?",
            Difficulty.Medium),
        P("Philosophy",
            "If you replace every part of a ship, is it still the same ship?",
            "What defines identity?",
            Difficulty.Hard),
        P("Philosophy",
            "Can God create a rock so heavy He can't lift it?",
            "Explore the limits of omnipotence.",
            Difficulty.Hard),
        P("Philosophy",
            "Is it morally worse to lie or to tell a harmful truth?",
            "No right answer — just defend your position.",
            Difficulty.Hard),

        // ── PHYSICS ───────────────────────────────────────────────────────────
        P("Physics",
            "How can mirrors reverse left and right, but not up and down?",
            "Hint: They might not be reversing at all.",
            Difficulty.Hard),
        P("Physics",
            "If you're in a train moving at light speed and turn on the headlights, what do you see?",
            "Hint: Einstein has something to say about this.",
            Difficulty.Hard),
        P("Physics",
            "What happens if an unstoppable force meets an immovable object?",
            "Hint: They cannot both exist.",
            Difficulty.Medium),

        // ── RIDDLE ────────────────────────────────────────────────────────────
        P("Riddle",
            "I speak without a mouth and hear without ears. I have no body, but come alive with wind. What am I?",
            "Hint: It's not a ghost.",
            Difficulty.Easy),
        P("Riddle",
            "What has a head and a tail but no body?",
            "Hint: It's not an animal.",
            Difficulty.Easy),
        P("Riddle",
            "What can travel around the world while staying in a corner?",
            "Hint: It's something you send.",
            Difficulty.Medium),
        P("Riddle",
            "If you have a bowl with six apples and you take away four, how many do you have?",
            "Hint: The answer is not two.",
            Difficulty.Easy),

        // ── IMPOSSIBLE ────────────────────────────────────────────────────────
        P("Impossible",
            "How can you be standing in front of me, standing behind me, and standing beside me, all at the same time?",
            "Hint: No mirrors, no cloning, no portals.",
            Difficulty.Hard),
        P("Impossible",
            "If you're wearing a helmet made of glass and someone breaks it while it's on your head, you don't get hurt. How?",
            "Hint: It's glass, not magic.",
            Difficulty.Hard),
        P("Impossible",
            "What word looks the same upside down and backward?",
            "Hint: It's a real English word.",
            Difficulty.Hard),
    ];

    private static ICard P(string category, string paradox, string hint, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>2-MINUTE PARADOX</b>\n\n" +
            paradox + "\n\n" +
            hint + "\n\n" +
            "<b>SOLVE IT:</b> Write your best answer, explanation, or wild guess.\n\n" +
            "Vote on best: most logical, most creative, most hilarious, or closest to correct.",
            d, category);
}
