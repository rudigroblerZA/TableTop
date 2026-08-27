using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Celebrity Impersonator — act like someone famous (or infamous). Others guess who.
///
/// How to play:
///   1. Draw a card with a famous person (celebrity, historical figure, fictional character).
///   2. Spend 30 seconds acting like them — voice, mannerisms, catchphrases, attitude.
///   3. Everyone else writes down their guess.
///   4. Reveal the answer. Correct guesses get points.
///   5. You also get points if people guess correctly — bonus for fooling half the group.
///
/// Some are obvious (Elvis, just do the lip twitch). Some are absurd (a sentient houseplant
/// with anxiety). Some make people yell "WHO IS THAT??" The fun is in committing fully to
/// the bit and seeing who people think you are.
///
/// Great for performance, creativity, and inside jokes. No talent required — just commitment
/// to the bit. Works for all ages (keep it family-friendly).
/// </summary>
public sealed class CelebrityImpersonatorMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Celebrity Impersonator";
    /// <inheritdoc />
    public override string Description =>
        "Act like someone famous. Voice, mannerisms, catchphrases. Can others guess you?";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Performed";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Pass";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [CelebrityImpersonatorCardBank.CelebrityCategory] = "#EC407A",
            [CelebrityImpersonatorCardBank.HistoricalCategory] = "#42A5F5",
            [CelebrityImpersonatorCardBank.FictionalCategory] = "#FFCA28",
            [CelebrityImpersonatorCardBank.AbsurdCategory] = "#AB47BC",
            [CelebrityImpersonatorCardBank.CulturalCategory] = "#66BB6A",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        CelebrityImpersonatorCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => CelebrityImpersonatorCardBank.All;
}

/// <summary>Built-in card bank for Celebrity Impersonator. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class CelebrityImpersonatorCardBank
{
    internal const string CelebrityCategory = "Celebrity";
    internal const string HistoricalCategory = "Historical";
    internal const string FictionalCategory = "Fictional";
    internal const string AbsurdCategory = "Absurd";
    internal const string CulturalCategory = "Cultural";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── CELEBRITY ────────────────────────────────────────────────────────
        I(CelebrityCategory, "Elvis Presley", "Sneer, hip swivel, deep voice, 'Uh huh'", Difficulty.Easy),
        I(CelebrityCategory, "Marilyn Monroe", "Breathy voice, blonde glamour, sultry whisper", Difficulty.Medium),
        I(CelebrityCategory, "James Bond", "Suave, posh British accent, martini hand gesture", Difficulty.Medium),
        I(CelebrityCategory, "Darth Vader", "Heavy breathing, cape swish, deep menacing voice", Difficulty.Easy),
        I(CelebrityCategory, "Arnold Schwarzenegger", "Austrian accent, 'I'll be back', flexing", Difficulty.Easy),
        I(CelebrityCategory, "Marilyn Monroe", "High-pitched giggle, breathy voice, hair flip", Difficulty.Medium),

        // ── HISTORICAL ───────────────────────────────────────────────────────
        I(HistoricalCategory, "Winston Churchill", "Cigar, growl, stern disapproving look, posh British accent", Difficulty.Hard),
        I(HistoricalCategory, "Napoleon Bonaparte", "Hand in jacket, stern military gaze, French accent", Difficulty.Medium),
        I(HistoricalCategory, "Albert Einstein", "Wild hair touching, tongue out, thoughtful genius", Difficulty.Medium),
        I(HistoricalCategory, "Cleopatra", "Regal, dramatic gestures, ancient Egyptian flair", Difficulty.Hard),
        I(HistoricalCategory, "Leonardo da Vinci", "Painting gestures, thoughtful beard stroking, mystery", Difficulty.Hard),

        // ── FICTIONAL ────────────────────────────────────────────────────────
        I(FictionalCategory, "Sherlock Holmes", "Deerstalker, analytical frown, 'Elementary'", Difficulty.Medium),
        I(FictionalCategory, "Yoda", "Backwards sentence structure, 'Hmmmm', small creature mannerisms", Difficulty.Medium),
        I(FictionalCategory, "SpongeBob SquarePants", "High-pitched laugh, innocent enthusiasm, nautical references", Difficulty.Easy),
        I(FictionalCategory, "Shrek", "Ogre accent (vaguely Scottish), gruffness, 'I'm an ogre'", Difficulty.Medium),
        I(FictionalCategory, "Gollum", "Hissy voice, 'my precious', weird head movements", Difficulty.Medium),

        // ── ABSURD ───────────────────────────────────────────────────────────
        I(AbsurdCategory, "A sentient anxiety disorder with a British accent", "Nervous, apologetic, overthinking everything constantly", Difficulty.Hard),
        I(AbsurdCategory, "A self-aware houseplant with trust issues", "Slow movements, dramatic sighs, 'I've been watered twice this month'", Difficulty.Hard),
        I(AbsurdCategory, "An existential crisis in human form", "Stares into nothing, asks 'what's the point', confused gestures", Difficulty.Hard),
        I(AbsurdCategory, "A motivational poster that became sentient and is now regretful", "Aggressive positivity mixed with despair, jazz hands", Difficulty.Hard),
        I(AbsurdCategory, "A sentient WiFi router from 2005", "Confused beeping, dies randomly, complains about being replaced", Difficulty.Hard),

        // ── CULTURAL ─────────────────────────────────────────────────────────
        I(CulturalCategory, "A pirate", "Arr matey, pirate accent, hand gestures, ship movements", Difficulty.Easy),
        I(CulturalCategory, "A strict teacher", "Disappointed look, red pen, stern voice, finger pointing", Difficulty.Easy),
        I(CulturalCategory, "A mad scientist", "Wild hair, mad laugh, explosive hand gestures", Difficulty.Easy),
        I(CulturalCategory, "A cheerleader from the 80s", "Pom-poms, high energy, 'Go team!', enthusiastic attitude", Difficulty.Easy),
        I(CulturalCategory, "A conspiracy theorist", "Wide eyes, 'they're listening', pointing, connecting dots", Difficulty.Medium),
    ];

    private static ICard I(string category, string character, string hints, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>30-SECOND IMPERSONATION</b>\n\n" +
            "You are: <b>" + character + "</b>\n\n" +
            "Act like them. Voice, mannerisms, catchphrases, attitude.\n\n" +
            "Tips: " + hints + "\n\n" +
            "Hints to remember:\n" +
            "• Commit fully to the bit\n" +
            "• Use voice/accent if you can\n" +
            "• Physical mannerisms count\n" +
            "• Everyone else guesses who you are",
            d, category);
}
