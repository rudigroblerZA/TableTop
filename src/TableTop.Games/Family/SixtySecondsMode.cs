using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// 60 Seconds — one category, one shared sixty-second window, and everything
/// you can name before the clock runs out.
///
/// How to play:
///   1. Draw a card. It names a category and a target count.
///   2. Set a SIXTY-SECOND timer — always sixty, every card, no exceptions.
///      That fixed window is the whole identity of this mode, so it's a
///      house rule baked into the card text rather than something the
///      per-player Settings timer should override (the same convention
///      other timed modes already use — Alibi's "30 seconds", One-Star
///      Reviews' "45 seconds" — a real-world instruction on the card, not
///      an engine-enforced clock).
///   3. The active player names as many valid items as they can before time's
///      up. Anyone at the table can challenge a dubious answer; majority rules.
///   4. Hit the target count? That's a completion. Fall short? It's a miss —
///      no shame, the categories get genuinely harder to fill on purpose.
///
/// Target counts are calibrated to difficulty, not just picked at random:
/// Easy categories are broad enough that ten-plus items should flow easily;
/// Extreme categories are narrow enough that even five is a real fight
/// against the clock. Difficulty-based scoring rewards that honestly —
/// clearing a hard category is worth more than clearing an easy one.
/// </summary>
public sealed class SixtySecondsMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "60 Seconds";
    /// <inheritdoc />
    public override string Description =>
        "One category, one sixty-second window — name as many as you can before the clock runs out.";

    /// <summary>Label for the button that records hitting the target count.</summary>
    public override string CompleteLabel => "Hit the Target";
    /// <summary>Label for the button that records falling short.</summary>
    public override string SkipLabel => "Time's Up";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [SixtySecondsCardBank.EverydayThingsCategory] = "#66BB6A",
            [SixtySecondsCardBank.NatureCategory] = "#26A69A",
            [SixtySecondsCardBank.FoodDrinkCategory] = "#FFA726",
            [SixtySecondsCardBank.PlacesCategory] = "#42A5F5",
            [SixtySecondsCardBank.EntertainmentCategory] = "#AB47BC",
            [SixtySecondsCardBank.WordsCategory] = "#EC407A",
            [SixtySecondsCardBank.WildcardCategory] = "#EF5350",
        };

    /// <summary>Harder-to-fill categories score more when hit.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in 60 Seconds card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        SixtySecondsCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => SixtySecondsCardBank.All;
}

/// <summary>Built-in card bank for 60 Seconds.</summary>
public static class SixtySecondsCardBank
{
    internal const string EverydayThingsCategory = "Everyday Things";
    internal const string NatureCategory = "Nature";
    internal const string FoodDrinkCategory = "Food & Drink";
    internal const string PlacesCategory = "Places";
    internal const string EntertainmentCategory = "Entertainment";
    internal const string WordsCategory = "Words";
    internal const string WildcardCategory = "Wildcard";

    /// <summary>All 60-second category cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── EVERYDAY THINGS ──────────────────────────────────────────────────
        S(EverydayThingsCategory, "Things you'd find in a kitchen", 10, Difficulty.Easy),
        S(EverydayThingsCategory, "Things that are round", 10, Difficulty.Easy),
        S(EverydayThingsCategory, "Things you charge with a cable", 8, Difficulty.Medium),
        S(EverydayThingsCategory, "Things you'd pack for a beach day", 8, Difficulty.Medium),
        S(EverydayThingsCategory, "Things that come in pairs", 6, Difficulty.Hard),
        S(EverydayThingsCategory, "Things you'd find specifically in a junk drawer", 5, Difficulty.Extreme),

        // ── NATURE ───────────────────────────────────────────────────────────
        S(NatureCategory, "Animals", 10, Difficulty.Easy),
        S(NatureCategory, "Birds", 10, Difficulty.Easy),
        S(NatureCategory, "Animals that live in the ocean", 8, Difficulty.Medium),
        S(NatureCategory, "Trees", 8, Difficulty.Medium),
        S(NatureCategory, "Animals with stripes", 6, Difficulty.Hard),
        S(NatureCategory, "Venomous creatures", 5, Difficulty.Extreme),
        S(NatureCategory, "Extinct animals", 5, Difficulty.Extreme),

        // ── FOOD & DRINK ─────────────────────────────────────────────────────
        S(FoodDrinkCategory, "Fruits", 10, Difficulty.Easy),
        S(FoodDrinkCategory, "Pizza toppings", 10, Difficulty.Easy),
        S(FoodDrinkCategory, "Breakfast foods", 8, Difficulty.Medium),
        S(FoodDrinkCategory, "Types of pasta", 8, Difficulty.Medium),
        S(FoodDrinkCategory, "Cheeses", 6, Difficulty.Hard),
        S(FoodDrinkCategory, "Spices", 6, Difficulty.Hard),
        S(FoodDrinkCategory, "Foods that are technically berries", 5, Difficulty.Extreme),

        // ── PLACES ───────────────────────────────────────────────────────────
        S(PlacesCategory, "Countries", 10, Difficulty.Easy),
        S(PlacesCategory, "US states", 10, Difficulty.Easy),
        S(PlacesCategory, "European capitals", 8, Difficulty.Medium),
        S(PlacesCategory, "Islands", 8, Difficulty.Medium),
        S(PlacesCategory, "Countries that border France", 6, Difficulty.Hard),
        S(PlacesCategory, "Landlocked countries", 5, Difficulty.Extreme),

        // ── ENTERTAINMENT ────────────────────────────────────────────────────
        S(EntertainmentCategory, "Disney movies", 10, Difficulty.Easy),
        S(EntertainmentCategory, "Superheroes", 10, Difficulty.Easy),
        S(EntertainmentCategory, "TV shows with one-word titles", 8, Difficulty.Medium),
        S(EntertainmentCategory, "Board games", 8, Difficulty.Medium),
        S(EntertainmentCategory, "Oscar-winning actors", 6, Difficulty.Hard),
        S(EntertainmentCategory, "Shakespeare plays", 5, Difficulty.Extreme),

        // ── WORDS ────────────────────────────────────────────────────────────
        S(WordsCategory, "Words that start with 'S'", 10, Difficulty.Easy),
        S(WordsCategory, "Colours", 10, Difficulty.Easy),
        S(WordsCategory, "Words that rhyme with 'day'", 8, Difficulty.Medium),
        S(WordsCategory, "Compound words containing 'sun'", 6, Difficulty.Hard),
        S(WordsCategory, "Words ending in '-ology'", 5, Difficulty.Extreme),
        S(WordsCategory, "Palindromes", 5, Difficulty.Extreme),

        // ── WILDCARD ─────────────────────────────────────────────────────────
        S(WildcardCategory, "Things that are sticky", 10, Difficulty.Easy),
        S(WildcardCategory, "Things you'd find at a birthday party", 10, Difficulty.Easy),
        S(WildcardCategory, "Excuses for being late", 8, Difficulty.Medium),
        S(WildcardCategory, "Things that are surprisingly heavy", 8, Difficulty.Medium),
        S(WildcardCategory, "Things you'd never want to find in your shoe", 6, Difficulty.Hard),
        S(WildcardCategory, "Things banned on an airplane", 5, Difficulty.Extreme),
    ];

    /// <summary>
    /// Lowercases only the FIRST letter of the prompt, so it reads naturally
    /// mid-sentence ("Name as many fruits...") without mangling embedded
    /// proper nouns or acronyms ("US states", "Disney movies", "'S'").
    /// </summary>
    private static string LowerFirstLetterOnly(string s) =>
        s.Length == 0 ? s : char.ToLowerInvariant(s[0]) + s[1..];

    private static ICard S(string category, string prompt, int target, Difficulty d) =>
        StandardCard.Create(
            "60 Seconds: " + prompt,
            "<b>⏱️ SIXTY SECONDS. GO.</b>\n\n" +
            "Name as many <b>" + LowerFirstLetterOnly(prompt) + "</b> as you can before time's up.\n\n" +
            "<i>Target: " + target + " or more to hit it. Table judges any dubious answers — majority rules.</i>",
            d, category);
}
