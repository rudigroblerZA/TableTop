using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Forbidden Words — describe the target without saying the words you most want to say.
///
/// How to play:
///   1. The active player reads the card silently. It shows a TARGET word and
///      three FORBIDDEN words.
///   2. They describe the target out loud — but may not say the target itself,
///      any forbidden word, or any part/derivative of them ("sun" bans "sunny").
///   3. Everyone else shouts guesses. First correct guess: describer AND guesser
///      each take a point.
///   4. Say a forbidden word? Round over, no points, next player. The group are
///      the referees — and they will enjoy it.
///
/// The forbidden list is always the three most natural clues, so easy targets
/// become hilariously hard: try explaining PIZZA without cheese, Italy, or slice.
/// Fast, loud, zero setup — the purest party game shape there is.
/// </summary>
public sealed class ForbiddenWordsMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Forbidden Words";
    /// <inheritdoc />
    public override string Description =>
        "Describe the word without saying the three words you most want to say. Fast, loud party classic.";

    /// <summary>Label shown on the button that records a completed round.</summary>
    public override string CompleteLabel => "Guessed";
    /// <summary>Label shown on the button that skips the current card.</summary>
    public override string SkipLabel => "Busted";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [ForbiddenWordsCardBank.FoodCategory] = "#FFA726",
            [ForbiddenWordsCardBank.PlacesCategory] = "#42A5F5",
            [ForbiddenWordsCardBank.ThingsCategory] = "#AB47BC",
            [ForbiddenWordsCardBank.ActionsCategory] = "#EC407A",
            [ForbiddenWordsCardBank.PeopleCategory] = "#66BB6A",
            [ForbiddenWordsCardBank.HardModeCategory] = "#EF5350",
        };

    /// <summary>Describer and guesser each score one on success.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Returns the built-in forbidden-words card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        ForbiddenWordsCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => ForbiddenWordsCardBank.All;
}

/// <summary>Built-in card bank for Forbidden Words.</summary>
public static class ForbiddenWordsCardBank
{
    internal const string FoodCategory = "Food";
    internal const string PlacesCategory = "Places";
    internal const string ThingsCategory = "Things";
    internal const string ActionsCategory = "Actions";
    internal const string PeopleCategory = "People";
    internal const string HardModeCategory = "Hard Mode";

    /// <summary>All forbidden-words cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── FOOD ──────────────────────────────────────────────────────────────
        F(FoodCategory, "PIZZA",      "cheese", "Italy", "slice", Difficulty.Easy),
        F(FoodCategory, "BANANA",     "yellow", "monkey", "peel", Difficulty.Easy),
        F(FoodCategory, "CHOCOLATE",  "sweet", "brown", "cocoa", Difficulty.Easy),
        F(FoodCategory, "SPAGHETTI",  "pasta", "long", "noodle", Difficulty.Medium),
        F(FoodCategory, "POPCORN",    "cinema", "kernel", "butter", Difficulty.Medium),
        F(FoodCategory, "SUSHI",      "fish", "rice", "Japan", Difficulty.Medium),
        F(FoodCategory, "PANCAKE",    "flip", "flat", "syrup", Difficulty.Medium),
        F(FoodCategory, "ICE CREAM",  "cold", "cone", "scoop", Difficulty.Easy),

        // ── PLACES ────────────────────────────────────────────────────────────
        F(PlacesCategory, "BEACH",    "sand", "sea", "sun", Difficulty.Easy),
        F(PlacesCategory, "LIBRARY",  "books", "quiet", "borrow", Difficulty.Easy),
        F(PlacesCategory, "AIRPORT",  "plane", "fly", "luggage", Difficulty.Medium),
        F(PlacesCategory, "HOSPITAL", "doctor", "sick", "nurse", Difficulty.Medium),
        F(PlacesCategory, "DESERT",   "sand", "hot", "camel", Difficulty.Medium),
        F(PlacesCategory, "CINEMA",   "film", "screen", "popcorn", Difficulty.Easy),
        F(PlacesCategory, "FARM",     "animals", "tractor", "field", Difficulty.Easy),
        F(PlacesCategory, "VOLCANO",  "lava", "erupt", "mountain", Difficulty.Hard),

        // ── THINGS ────────────────────────────────────────────────────────────
        F(ThingsCategory, "UMBRELLA",   "rain", "wet", "open", Difficulty.Easy),
        F(ThingsCategory, "TOOTHBRUSH", "teeth", "clean", "paste", Difficulty.Easy),
        F(ThingsCategory, "PILLOW",     "sleep", "soft", "head", Difficulty.Medium),
        F(ThingsCategory, "MIRROR",     "reflection", "look", "glass", Difficulty.Medium),
        F(ThingsCategory, "CANDLE",     "wax", "flame", "birthday", Difficulty.Medium),
        F(ThingsCategory, "KEYBOARD",   "type", "computer", "keys", Difficulty.Medium),
        F(ThingsCategory, "TRAMPOLINE", "jump", "bounce", "springs", Difficulty.Hard),
        F(ThingsCategory, "COMPASS",    "north", "direction", "needle", Difficulty.Hard),

        // ── ACTIONS ───────────────────────────────────────────────────────────
        F(ActionsCategory, "SNEEZE",   "achoo", "nose", "bless", Difficulty.Medium),
        F(ActionsCategory, "WHISPER",  "quiet", "secret", "ear", Difficulty.Medium),
        F(ActionsCategory, "JUGGLE",   "balls", "throw", "circus", Difficulty.Medium),
        F(ActionsCategory, "YAWN",     "tired", "mouth", "sleepy", Difficulty.Easy),
        F(ActionsCategory, "SWIM",     "water", "pool", "stroke", Difficulty.Easy),
        F(ActionsCategory, "HICCUP",   "sound", "scare", "water", Difficulty.Hard),
        F(ActionsCategory, "APPLAUD",  "clap", "hands", "audience", Difficulty.Medium),
        F(ActionsCategory, "SHIVER",   "cold", "shake", "goosebumps", Difficulty.Hard),

        // ── PEOPLE ────────────────────────────────────────────────────────────
        F(PeopleCategory, "FIREFIGHTER", "fire", "hose", "ladder", Difficulty.Easy),
        F(PeopleCategory, "MAGICIAN",    "trick", "rabbit", "hat", Difficulty.Medium),
        F(PeopleCategory, "ASTRONAUT",   "space", "rocket", "moon", Difficulty.Easy),
        F(PeopleCategory, "REFEREE",     "whistle", "sport", "rules", Difficulty.Medium),
        F(PeopleCategory, "PIRATE",      "ship", "treasure", "parrot", Difficulty.Easy),
        F(PeopleCategory, "DENTIST",     "teeth", "drill", "mouth", Difficulty.Medium),
        F(PeopleCategory, "DETECTIVE",   "mystery", "clues", "solve", Difficulty.Hard),
        F(PeopleCategory, "LIFEGUARD",   "pool", "rescue", "whistle", Difficulty.Medium),

        // ── HARD MODE ─────────────────────────────────────────────────────────
        F(HardModeCategory, "GRAVITY",     "fall", "Earth", "Newton", Difficulty.Extreme),
        F(HardModeCategory, "ECHO",        "sound", "repeat", "cave", Difficulty.Extreme),
        F(HardModeCategory, "SHADOW",      "dark", "light", "sun", Difficulty.Extreme),
        F(HardModeCategory, "DÉJÀ VU",     "before", "feeling", "again", Difficulty.Extreme),
        F(HardModeCategory, "WIFI",        "internet", "signal", "router", Difficulty.Extreme),
        F(HardModeCategory, "MIDNIGHT",    "twelve", "night", "clock", Difficulty.Extreme),
        F(HardModeCategory, "NOSTALGIA",   "past", "memory", "miss", Difficulty.Extreme),
        F(HardModeCategory, "SARCASM",     "joke", "tone", "mean", Difficulty.Extreme),

        // ── EXPANSION: FAN FAVOURITES ─────────────────────────────────────────
        F(ThingsCategory, "SELFIE",       "photo", "phone", "yourself", Difficulty.Medium),
        F(ThingsCategory, "ALARM CLOCK",  "wake", "morning", "ring", Difficulty.Easy),
        F(ThingsCategory, "GLITTER",      "sparkle", "craft", "everywhere", Difficulty.Hard),
        F(ThingsCategory, "SOCKS",        "feet", "pair", "missing", Difficulty.Easy),
        F(ActionsCategory, "PROCRASTINATE", "later", "delay", "tomorrow", Difficulty.Hard),
        F(ActionsCategory, "BINGE-WATCH", "episodes", "series", "one more", Difficulty.Medium),
        F(ActionsCategory, "GHOSTING",    "reply", "disappear", "message", Difficulty.Hard),
        F(ActionsCategory, "EAVESDROP",   "listen", "secret", "conversation", Difficulty.Medium),
        F(PeopleCategory, "INFLUENCER",   "followers", "post", "sponsored", Difficulty.Medium),
        F(PeopleCategory, "VILLAIN",      "evil", "hero", "plan", Difficulty.Medium),
        F(PeopleCategory, "TODDLER",      "small", "tantrum", "nap", Difficulty.Easy),
        F(PeopleCategory, "CONSPIRACY THEORIST", "government", "secret", "truth", Difficulty.Hard),
        F(HardModeCategory, "AWKWARD SILENCE", "quiet", "uncomfortable", "conversation", Difficulty.Extreme),
        F(HardModeCategory, "REVENGE",   "payback", "hurt", "even", Difficulty.Extreme),
        F(HardModeCategory, "PASSWORD",  "secret", "login", "forgot", Difficulty.Extreme),
        F(HardModeCategory, "MONDAY",    "week", "work", "morning", Difficulty.Extreme),
    ];

    private static ICard F(string category, string target, string ban1, string ban2, string ban3, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>Your word: " + target + "</b>\n\n" +
            "<b>FORBIDDEN:</b> " + ban1 + " · " + ban2 + " · " + ban3 + "\n\n" +
            "Describe it out loud without saying your word, any forbidden word, or any form of them. " +
            "Everyone else shouts guesses — first correct guess scores for both of you.\n\n" +
            "<i>Slip up and say a forbidden word? Round over. The table decides. The table is merciless.</i>",
            d, category);
}
