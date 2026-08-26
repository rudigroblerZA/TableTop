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
            ["Food"] = "#FFA726",
            ["Places"] = "#42A5F5",
            ["Things"] = "#AB47BC",
            ["Actions"] = "#EC407A",
            ["People"] = "#66BB6A",
            ["Hard Mode"] = "#EF5350",
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
    /// <summary>All forbidden-words cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── FOOD ──────────────────────────────────────────────────────────────
        F("Food", "PIZZA",      "cheese", "Italy", "slice", Difficulty.Easy),
        F("Food", "BANANA",     "yellow", "monkey", "peel", Difficulty.Easy),
        F("Food", "CHOCOLATE",  "sweet", "brown", "cocoa", Difficulty.Easy),
        F("Food", "SPAGHETTI",  "pasta", "long", "noodle", Difficulty.Medium),
        F("Food", "POPCORN",    "cinema", "kernel", "butter", Difficulty.Medium),
        F("Food", "SUSHI",      "fish", "rice", "Japan", Difficulty.Medium),
        F("Food", "PANCAKE",    "flip", "flat", "syrup", Difficulty.Medium),
        F("Food", "ICE CREAM",  "cold", "cone", "scoop", Difficulty.Easy),

        // ── PLACES ────────────────────────────────────────────────────────────
        F("Places", "BEACH",    "sand", "sea", "sun", Difficulty.Easy),
        F("Places", "LIBRARY",  "books", "quiet", "borrow", Difficulty.Easy),
        F("Places", "AIRPORT",  "plane", "fly", "luggage", Difficulty.Medium),
        F("Places", "HOSPITAL", "doctor", "sick", "nurse", Difficulty.Medium),
        F("Places", "DESERT",   "sand", "hot", "camel", Difficulty.Medium),
        F("Places", "CINEMA",   "film", "screen", "popcorn", Difficulty.Easy),
        F("Places", "FARM",     "animals", "tractor", "field", Difficulty.Easy),
        F("Places", "VOLCANO",  "lava", "erupt", "mountain", Difficulty.Hard),

        // ── THINGS ────────────────────────────────────────────────────────────
        F("Things", "UMBRELLA",   "rain", "wet", "open", Difficulty.Easy),
        F("Things", "TOOTHBRUSH", "teeth", "clean", "paste", Difficulty.Easy),
        F("Things", "PILLOW",     "sleep", "soft", "head", Difficulty.Medium),
        F("Things", "MIRROR",     "reflection", "look", "glass", Difficulty.Medium),
        F("Things", "CANDLE",     "wax", "flame", "birthday", Difficulty.Medium),
        F("Things", "KEYBOARD",   "type", "computer", "keys", Difficulty.Medium),
        F("Things", "TRAMPOLINE", "jump", "bounce", "springs", Difficulty.Hard),
        F("Things", "COMPASS",    "north", "direction", "needle", Difficulty.Hard),

        // ── ACTIONS ───────────────────────────────────────────────────────────
        F("Actions", "SNEEZE",   "achoo", "nose", "bless", Difficulty.Medium),
        F("Actions", "WHISPER",  "quiet", "secret", "ear", Difficulty.Medium),
        F("Actions", "JUGGLE",   "balls", "throw", "circus", Difficulty.Medium),
        F("Actions", "YAWN",     "tired", "mouth", "sleepy", Difficulty.Easy),
        F("Actions", "SWIM",     "water", "pool", "stroke", Difficulty.Easy),
        F("Actions", "HICCUP",   "sound", "scare", "water", Difficulty.Hard),
        F("Actions", "APPLAUD",  "clap", "hands", "audience", Difficulty.Medium),
        F("Actions", "SHIVER",   "cold", "shake", "goosebumps", Difficulty.Hard),

        // ── PEOPLE ────────────────────────────────────────────────────────────
        F("People", "FIREFIGHTER", "fire", "hose", "ladder", Difficulty.Easy),
        F("People", "MAGICIAN",    "trick", "rabbit", "hat", Difficulty.Medium),
        F("People", "ASTRONAUT",   "space", "rocket", "moon", Difficulty.Easy),
        F("People", "REFEREE",     "whistle", "sport", "rules", Difficulty.Medium),
        F("People", "PIRATE",      "ship", "treasure", "parrot", Difficulty.Easy),
        F("People", "DENTIST",     "teeth", "drill", "mouth", Difficulty.Medium),
        F("People", "DETECTIVE",   "mystery", "clues", "solve", Difficulty.Hard),
        F("People", "LIFEGUARD",   "pool", "rescue", "whistle", Difficulty.Medium),

        // ── HARD MODE ─────────────────────────────────────────────────────────
        F("Hard Mode", "GRAVITY",     "fall", "Earth", "Newton", Difficulty.Extreme),
        F("Hard Mode", "ECHO",        "sound", "repeat", "cave", Difficulty.Extreme),
        F("Hard Mode", "SHADOW",      "dark", "light", "sun", Difficulty.Extreme),
        F("Hard Mode", "DÉJÀ VU",     "before", "feeling", "again", Difficulty.Extreme),
        F("Hard Mode", "WIFI",        "internet", "signal", "router", Difficulty.Extreme),
        F("Hard Mode", "MIDNIGHT",    "twelve", "night", "clock", Difficulty.Extreme),
        F("Hard Mode", "NOSTALGIA",   "past", "memory", "miss", Difficulty.Extreme),
        F("Hard Mode", "SARCASM",     "joke", "tone", "mean", Difficulty.Extreme),

        // ── EXPANSION: FAN FAVOURITES ─────────────────────────────────────────
        F("Things", "SELFIE",       "photo", "phone", "yourself", Difficulty.Medium),
        F("Things", "ALARM CLOCK",  "wake", "morning", "ring", Difficulty.Easy),
        F("Things", "GLITTER",      "sparkle", "craft", "everywhere", Difficulty.Hard),
        F("Things", "SOCKS",        "feet", "pair", "missing", Difficulty.Easy),
        F("Actions", "PROCRASTINATE", "later", "delay", "tomorrow", Difficulty.Hard),
        F("Actions", "BINGE-WATCH", "episodes", "series", "one more", Difficulty.Medium),
        F("Actions", "GHOSTING",    "reply", "disappear", "message", Difficulty.Hard),
        F("Actions", "EAVESDROP",   "listen", "secret", "conversation", Difficulty.Medium),
        F("People", "INFLUENCER",   "followers", "post", "sponsored", Difficulty.Medium),
        F("People", "VILLAIN",      "evil", "hero", "plan", Difficulty.Medium),
        F("People", "TODDLER",      "small", "tantrum", "nap", Difficulty.Easy),
        F("People", "CONSPIRACY THEORIST", "government", "secret", "truth", Difficulty.Hard),
        F("Hard Mode", "AWKWARD SILENCE", "quiet", "uncomfortable", "conversation", Difficulty.Extreme),
        F("Hard Mode", "REVENGE",   "payback", "hurt", "even", Difficulty.Extreme),
        F("Hard Mode", "PASSWORD",  "secret", "login", "forgot", Difficulty.Extreme),
        F("Hard Mode", "MONDAY",    "week", "work", "morning", Difficulty.Extreme),
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
