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
            ["Celebrity"] = "#EC407A",
            ["Historical"] = "#42A5F5",
            ["Fictional"] = "#FFCA28",
            ["Absurd"] = "#AB47BC",
            ["Cultural"] = "#66BB6A",
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
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── CELEBRITY ────────────────────────────────────────────────────────
        I("Celebrity", "Elvis Presley", "Sneer, hip swivel, deep voice, 'Uh huh'", Difficulty.Easy),
        I("Celebrity", "Marilyn Monroe", "Breathy voice, blonde glamour, sultry whisper", Difficulty.Medium),
        I("Celebrity", "James Bond", "Suave, posh British accent, martini hand gesture", Difficulty.Medium),
        I("Celebrity", "Darth Vader", "Heavy breathing, cape swish, deep menacing voice", Difficulty.Easy),
        I("Celebrity", "Arnold Schwarzenegger", "Austrian accent, 'I'll be back', flexing", Difficulty.Easy),
        I("Celebrity", "Marilyn Monroe", "High-pitched giggle, breathy voice, hair flip", Difficulty.Medium),

        // ── HISTORICAL ───────────────────────────────────────────────────────
        I("Historical", "Winston Churchill", "Cigar, growl, stern disapproving look, posh British accent", Difficulty.Hard),
        I("Historical", "Napoleon Bonaparte", "Hand in jacket, stern military gaze, French accent", Difficulty.Medium),
        I("Historical", "Albert Einstein", "Wild hair touching, tongue out, thoughtful genius", Difficulty.Medium),
        I("Historical", "Cleopatra", "Regal, dramatic gestures, ancient Egyptian flair", Difficulty.Hard),
        I("Historical", "Leonardo da Vinci", "Painting gestures, thoughtful beard stroking, mystery", Difficulty.Hard),

        // ── FICTIONAL ────────────────────────────────────────────────────────
        I("Fictional", "Sherlock Holmes", "Deerstalker, analytical frown, 'Elementary'", Difficulty.Medium),
        I("Fictional", "Yoda", "Backwards sentence structure, 'Hmmmm', small creature mannerisms", Difficulty.Medium),
        I("Fictional", "SpongeBob SquarePants", "High-pitched laugh, innocent enthusiasm, nautical references", Difficulty.Easy),
        I("Fictional", "Shrek", "Ogre accent (vaguely Scottish), gruffness, 'I'm an ogre'", Difficulty.Medium),
        I("Fictional", "Gollum", "Hissy voice, 'my precious', weird head movements", Difficulty.Medium),

        // ── ABSURD ───────────────────────────────────────────────────────────
        I("Absurd", "A sentient anxiety disorder with a British accent", "Nervous, apologetic, overthinking everything constantly", Difficulty.Hard),
        I("Absurd", "A self-aware houseplant with trust issues", "Slow movements, dramatic sighs, 'I've been watered twice this month'", Difficulty.Hard),
        I("Absurd", "An existential crisis in human form", "Stares into nothing, asks 'what's the point', confused gestures", Difficulty.Hard),
        I("Absurd", "A motivational poster that became sentient and is now regretful", "Aggressive positivity mixed with despair, jazz hands", Difficulty.Hard),
        I("Absurd", "A sentient WiFi router from 2005", "Confused beeping, dies randomly, complains about being replaced", Difficulty.Hard),

        // ── CULTURAL ─────────────────────────────────────────────────────────
        I("Cultural", "A pirate", "Arr matey, pirate accent, hand gestures, ship movements", Difficulty.Easy),
        I("Cultural", "A strict teacher", "Disappointed look, red pen, stern voice, finger pointing", Difficulty.Easy),
        I("Cultural", "A mad scientist", "Wild hair, mad laugh, explosive hand gestures", Difficulty.Easy),
        I("Cultural", "A cheerleader from the 80s", "Pom-poms, high energy, 'Go team!', enthusiastic attitude", Difficulty.Easy),
        I("Cultural", "A conspiracy theorist", "Wide eyes, 'they're listening', pointing, connecting dots", Difficulty.Medium),
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
