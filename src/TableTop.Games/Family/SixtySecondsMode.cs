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

    private const string Deck = "60 Seconds";

    /// <summary>All 60-second category cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)
        // ── EVERYDAY THINGS ──────────────────────────────────────────────────
            .Category(EverydayThingsCategory)
            .Card("60 Seconds: " + "Things you'd find in a kitchen", Body("Things you'd find in a kitchen", 10), Difficulty.Easy)
            .Card("60 Seconds: " + "Things that are round", Body("Things that are round", 10), Difficulty.Easy)
            .Card("60 Seconds: " + "Things you charge with a cable", Body("Things you charge with a cable", 8), Difficulty.Medium)
            .Card("60 Seconds: " + "Things you'd pack for a beach day", Body("Things you'd pack for a beach day", 8), Difficulty.Medium)
            .Card("60 Seconds: " + "Things that come in pairs", Body("Things that come in pairs", 6), Difficulty.Hard)
            .Card("60 Seconds: " + "Things you'd find specifically in a junk drawer", Body("Things you'd find specifically in a junk drawer", 5), Difficulty.Extreme)

        // ── NATURE ───────────────────────────────────────────────────────────
            .Category(NatureCategory)
            .Card("60 Seconds: " + "Animals", Body("Animals", 10), Difficulty.Easy)
            .Card("60 Seconds: " + "Birds", Body("Birds", 10), Difficulty.Easy)
            .Card("60 Seconds: " + "Animals that live in the ocean", Body("Animals that live in the ocean", 8), Difficulty.Medium)
            .Card("60 Seconds: " + "Trees", Body("Trees", 8), Difficulty.Medium)
            .Card("60 Seconds: " + "Animals with stripes", Body("Animals with stripes", 6), Difficulty.Hard)
            .Card("60 Seconds: " + "Venomous creatures", Body("Venomous creatures", 5), Difficulty.Extreme)
            .Card("60 Seconds: " + "Extinct animals", Body("Extinct animals", 5), Difficulty.Extreme)

        // ── FOOD & DRINK ─────────────────────────────────────────────────────
            .Category(FoodDrinkCategory)
            .Card("60 Seconds: " + "Fruits", Body("Fruits", 10), Difficulty.Easy)
            .Card("60 Seconds: " + "Pizza toppings", Body("Pizza toppings", 10), Difficulty.Easy)
            .Card("60 Seconds: " + "Breakfast foods", Body("Breakfast foods", 8), Difficulty.Medium)
            .Card("60 Seconds: " + "Types of pasta", Body("Types of pasta", 8), Difficulty.Medium)
            .Card("60 Seconds: " + "Cheeses", Body("Cheeses", 6), Difficulty.Hard)
            .Card("60 Seconds: " + "Spices", Body("Spices", 6), Difficulty.Hard)
            .Card("60 Seconds: " + "Foods that are technically berries", Body("Foods that are technically berries", 5), Difficulty.Extreme)

        // ── PLACES ───────────────────────────────────────────────────────────
            .Category(PlacesCategory)
            .Card("60 Seconds: " + "Countries", Body("Countries", 10), Difficulty.Easy)
            .Card("60 Seconds: " + "US states", Body("US states", 10), Difficulty.Easy)
            .Card("60 Seconds: " + "European capitals", Body("European capitals", 8), Difficulty.Medium)
            .Card("60 Seconds: " + "Islands", Body("Islands", 8), Difficulty.Medium)
            .Card("60 Seconds: " + "Countries that border France", Body("Countries that border France", 6), Difficulty.Hard)
            .Card("60 Seconds: " + "Landlocked countries", Body("Landlocked countries", 5), Difficulty.Extreme)

        // ── ENTERTAINMENT ────────────────────────────────────────────────────
            .Category(EntertainmentCategory)
            .Card("60 Seconds: " + "Disney movies", Body("Disney movies", 10), Difficulty.Easy)
            .Card("60 Seconds: " + "Superheroes", Body("Superheroes", 10), Difficulty.Easy)
            .Card("60 Seconds: " + "TV shows with one-word titles", Body("TV shows with one-word titles", 8), Difficulty.Medium)
            .Card("60 Seconds: " + "Board games", Body("Board games", 8), Difficulty.Medium)
            .Card("60 Seconds: " + "Oscar-winning actors", Body("Oscar-winning actors", 6), Difficulty.Hard)
            .Card("60 Seconds: " + "Shakespeare plays", Body("Shakespeare plays", 5), Difficulty.Extreme)

        // ── WORDS ────────────────────────────────────────────────────────────
            .Category(WordsCategory)
            .Card("60 Seconds: " + "Words that start with 'S'", Body("Words that start with 'S'", 10), Difficulty.Easy)
            .Card("60 Seconds: " + "Colours", Body("Colours", 10), Difficulty.Easy)
            .Card("60 Seconds: " + "Words that rhyme with 'day'", Body("Words that rhyme with 'day'", 8), Difficulty.Medium)
            .Card("60 Seconds: " + "Compound words containing 'sun'", Body("Compound words containing 'sun'", 6), Difficulty.Hard)
            .Card("60 Seconds: " + "Words ending in '-ology'", Body("Words ending in '-ology'", 5), Difficulty.Extreme)
            .Card("60 Seconds: " + "Palindromes", Body("Palindromes", 5), Difficulty.Extreme)

        // ── WILDCARD ─────────────────────────────────────────────────────────
            .Category(WildcardCategory)
            .Card("60 Seconds: " + "Things that are sticky", Body("Things that are sticky", 10), Difficulty.Easy)
            .Card("60 Seconds: " + "Things you'd find at a birthday party", Body("Things you'd find at a birthday party", 10), Difficulty.Easy)
            .Card("60 Seconds: " + "Excuses for being late", Body("Excuses for being late", 8), Difficulty.Medium)
            .Card("60 Seconds: " + "Things that are surprisingly heavy", Body("Things that are surprisingly heavy", 8), Difficulty.Medium)
            .Card("60 Seconds: " + "Things you'd never want to find in your shoe", Body("Things you'd never want to find in your shoe", 6), Difficulty.Hard)
            .Card("60 Seconds: " + "Things banned on an airplane", Body("Things banned on an airplane", 5), Difficulty.Extreme)

            .Build();

    /// <summary>
    /// Lowercases only the FIRST letter of the prompt, so it reads naturally
    /// mid-sentence ("Name as many fruits...") without mangling embedded
    /// proper nouns or acronyms ("US states", "Disney movies", "'S'").
    /// </summary>
    private static string LowerFirstLetterOnly(string s) =>
        s.Length == 0 ? s : char.ToLowerInvariant(s[0]) + s[1..];

    private static string Body(string prompt, int target) =>
        "<b>⏱️ SIXTY SECONDS. GO.</b>\n\n" +
        "Name as many <b>" + LowerFirstLetterOnly(prompt) + "</b> as you can before time's up.\n\n" +
        "<i>Target: " + target + " or more to hit it. Table judges any dubious answers — majority rules.</i>";
}
