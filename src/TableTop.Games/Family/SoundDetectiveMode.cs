using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Sound Detective — guess what sound is being described through increasingly abstract clues.
///
/// How to play:
///   1. Read the first clue (most obvious, describes the sound directly).
///   2. Everyone has 10 seconds to guess what sound it is.
///   3. If no one guesses, read the second clue (more abstract).
///   4. Continue until someone guesses correctly or all clues are exhausted.
///   5. Points go to whoever guesses first. Earlier clues = more points.
///
/// Clues start practical ("A loud noise when metal hits metal") and get progressively
/// weirder ("Angry percussion having an identity crisis"). The fun is in the weird
/// descriptions and the moment it clicks: "OH! A hammer!"
///
/// Great for testing how people think about sounds, language, and metaphor. No audio
/// required — just creative description. Works for all ages.
/// </summary>
public sealed class SoundDetectiveMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Sound Detective";
    /// <inheritdoc />
    public override string Description =>
        "Guess the sound from increasingly abstract clues. First correct = most points.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Guessed";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [SoundDetectiveCardBank.NatureCategory] = "#66BB6A",
            [SoundDetectiveCardBank.HumanCategory] = "#EC407A",
            [SoundDetectiveCardBank.MechanicalCategory] = "#42A5F5",
            [SoundDetectiveCardBank.AnimalCategory] = "#FFCA28",
            [SoundDetectiveCardBank.AbstractCategory] = "#AB47BC",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        SoundDetectiveCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => SoundDetectiveCardBank.All;
}

/// <summary>Built-in card bank for Sound Detective. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class SoundDetectiveCardBank
{
    internal const string NatureCategory = "Nature";
    internal const string HumanCategory = "Human";
    internal const string MechanicalCategory = "Mechanical";
    internal const string AnimalCategory = "Animal";
    internal const string AbstractCategory = "Abstract";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── NATURE ────────────────────────────────────────────────────────────
        D(NatureCategory,
            "A persistent watery rush from above during storms.",
            "Nature's percussion section having a very emotional day.",
            "The sound of the sky crying aggressively.",
            "Answer: RAIN",
            Difficulty.Easy),
        D(NatureCategory,
            "Leaves being destroyed en masse by wind.",
            "A thousand tiny betrayals happening simultaneously.",
            "Autumn's violent panic attack.",
            "Answer: RUSTLING LEAVES",
            Difficulty.Medium),
        D(NatureCategory,
            "The ocean forcefully hitting the shore.",
            "Liquid violence in rhythmic waves.",
            "The earth's way of punching water.",
            "Answer: CRASHING WAVES",
            Difficulty.Easy),

        // ── HUMAN ────────────────────────────────────────────────────────────
        D(HumanCategory,
            "Air being pushed through your mouth quickly and forcefully.",
            "Angry nasal expression of frustration.",
            "The sound of 'yeah, whatever' without words.",
            "Answer: SIGH",
            Difficulty.Easy),
        D(HumanCategory,
            "Your hands hitting together repeatedly in appreciation.",
            "The physical manifestation of 'good job'.",
            "Rhythmic hand violence out of respect.",
            "Answer: CLAPPING",
            Difficulty.Easy),
        D(HumanCategory,
            "Vocal cords vibrating at high speed due to joy overload.",
            "The sound of chaos coming from your face.",
            "What happens when your brain breaks from happiness.",
            "Answer: LAUGHING",
            Difficulty.Easy),

        // ── MECHANICAL ────────────────────────────────────────────────────────
        D(MechanicalCategory,
            "Metal striking metal with force and purpose.",
            "Angry percussion in the construction department.",
            "What happens when two pieces of metal disagree violently.",
            "Answer: HAMMER",
            Difficulty.Medium),
        D(MechanicalCategory,
            "A rubber wheel rolling quickly across pavement.",
            "The sound of a vehicle aggressively abandoning a location.",
            "The dying scream of pavement under assault.",
            "Answer: SCREECHING TIRES",
            Difficulty.Medium),
        D(MechanicalCategory,
            "Electric device requesting your attention repeatedly.",
            "A robot's way of saying 'um, excuse me?'",
            "The sound of impatience coming from your pocket.",
            "Answer: PHONE NOTIFICATION",
            Difficulty.Easy),

        // ── ANIMAL ────────────────────────────────────────────────────────────
        D(AnimalCategory,
            "A four-legged creature with sharp teeth expressing unhappiness.",
            "Nature's way of saying 'personal space, buddy'.",
            "The angry voice of a creature that could eat you.",
            "Answer: DOG GROWL",
            Difficulty.Easy),
        D(AnimalCategory,
            "A feline mammal in supreme judgment of you.",
            "The sound of disdain from a creature that owns your house.",
            "What happens when a cat's soul leaves its body out of annoyance.",
            "Answer: CAT HISS",
            Difficulty.Easy),
        D(AnimalCategory,
            "A bird enthusiastically declaring the morning's arrival.",
            "Nature's alarm clock with a superiority complex.",
            "The sound of 'wake up you lazy humans' set to music.",
            "Answer: BIRD SONG/CHIRPING",
            Difficulty.Easy),

        // ── ABSTRACT ──────────────────────────────────────────────────────────
        D(AbstractCategory,
            "Something heavy hitting something solid.",
            "An object expressing its regrets to the ground.",
            "Gravity's victory lap.",
            "Answer: THUD",
            Difficulty.Medium),
        D(AbstractCategory,
            "Air escaping from a small opening rapidly.",
            "Pressure deciding it's had enough.",
            "The sound of 'I'm outta here' but physical.",
            "Answer: WHOOSH/AIR ESCAPE",
            Difficulty.Hard),
        D(AbstractCategory,
            "Friction happening at extremely close range.",
            "Two surfaces in direct conflict about their existence.",
            "The sound of 'stop touching me' in material form.",
            "Answer: SCRAPING/SCRATCHING",
            Difficulty.Hard),
    ];

    private static ICard D(string category, string clue1, string clue2, string clue3, string answer, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>GUESS THE SOUND</b>\n\n" +
            "<b>Clue 1 (most obvious):</b> " + clue1 + "\n\n" +
            "<b>Clue 2 (more abstract):</b> " + clue2 + "\n\n" +
            "<b>Clue 3 (very abstract):</b> " + clue3 + "\n\n" +
            "<b>Read one clue at a time. Everyone guesses. First correct answer wins.</b>\n\n" +
            answer,
            d, category);
}
