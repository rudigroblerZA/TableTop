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

    private const string Deck = "Forbidden Words";

    /// <summary>All forbidden-words cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)
        // ── FOOD ──────────────────────────────────────────────────────────────
            .Category(FoodCategory)
            .Card(FoodCategory, Body("PIZZA", "cheese", "Italy", "slice"), Difficulty.Easy)
            .Card(FoodCategory, Body("BANANA", "yellow", "monkey", "peel"), Difficulty.Easy)
            .Card(FoodCategory, Body("CHOCOLATE", "sweet", "brown", "cocoa"), Difficulty.Easy)
            .Card(FoodCategory, Body("SPAGHETTI", "pasta", "long", "noodle"), Difficulty.Medium)
            .Card(FoodCategory, Body("POPCORN", "cinema", "kernel", "butter"), Difficulty.Medium)
            .Card(FoodCategory, Body("SUSHI", "fish", "rice", "Japan"), Difficulty.Medium)
            .Card(FoodCategory, Body("PANCAKE", "flip", "flat", "syrup"), Difficulty.Medium)
            .Card(FoodCategory, Body("ICE CREAM", "cold", "cone", "scoop"), Difficulty.Easy)

        // ── PLACES ────────────────────────────────────────────────────────────
            .Category(PlacesCategory)
            .Card(PlacesCategory, Body("BEACH", "sand", "sea", "sun"), Difficulty.Easy)
            .Card(PlacesCategory, Body("LIBRARY", "books", "quiet", "borrow"), Difficulty.Easy)
            .Card(PlacesCategory, Body("AIRPORT", "plane", "fly", "luggage"), Difficulty.Medium)
            .Card(PlacesCategory, Body("HOSPITAL", "doctor", "sick", "nurse"), Difficulty.Medium)
            .Card(PlacesCategory, Body("DESERT", "sand", "hot", "camel"), Difficulty.Medium)
            .Card(PlacesCategory, Body("CINEMA", "film", "screen", "popcorn"), Difficulty.Easy)
            .Card(PlacesCategory, Body("FARM", "animals", "tractor", "field"), Difficulty.Easy)
            .Card(PlacesCategory, Body("VOLCANO", "lava", "erupt", "mountain"), Difficulty.Hard)

        // ── THINGS ────────────────────────────────────────────────────────────
            .Category(ThingsCategory)
            .Card(ThingsCategory, Body("UMBRELLA", "rain", "wet", "open"), Difficulty.Easy)
            .Card(ThingsCategory, Body("TOOTHBRUSH", "teeth", "clean", "paste"), Difficulty.Easy)
            .Card(ThingsCategory, Body("PILLOW", "sleep", "soft", "head"), Difficulty.Medium)
            .Card(ThingsCategory, Body("MIRROR", "reflection", "look", "glass"), Difficulty.Medium)
            .Card(ThingsCategory, Body("CANDLE", "wax", "flame", "birthday"), Difficulty.Medium)
            .Card(ThingsCategory, Body("KEYBOARD", "type", "computer", "keys"), Difficulty.Medium)
            .Card(ThingsCategory, Body("TRAMPOLINE", "jump", "bounce", "springs"), Difficulty.Hard)
            .Card(ThingsCategory, Body("COMPASS", "north", "direction", "needle"), Difficulty.Hard)

        // ── ACTIONS ───────────────────────────────────────────────────────────
            .Category(ActionsCategory)
            .Card(ActionsCategory, Body("SNEEZE", "achoo", "nose", "bless"), Difficulty.Medium)
            .Card(ActionsCategory, Body("WHISPER", "quiet", "secret", "ear"), Difficulty.Medium)
            .Card(ActionsCategory, Body("JUGGLE", "balls", "throw", "circus"), Difficulty.Medium)
            .Card(ActionsCategory, Body("YAWN", "tired", "mouth", "sleepy"), Difficulty.Easy)
            .Card(ActionsCategory, Body("SWIM", "water", "pool", "stroke"), Difficulty.Easy)
            .Card(ActionsCategory, Body("HICCUP", "sound", "scare", "water"), Difficulty.Hard)
            .Card(ActionsCategory, Body("APPLAUD", "clap", "hands", "audience"), Difficulty.Medium)
            .Card(ActionsCategory, Body("SHIVER", "cold", "shake", "goosebumps"), Difficulty.Hard)

        // ── PEOPLE ────────────────────────────────────────────────────────────
            .Category(PeopleCategory)
            .Card(PeopleCategory, Body("FIREFIGHTER", "fire", "hose", "ladder"), Difficulty.Easy)
            .Card(PeopleCategory, Body("MAGICIAN", "trick", "rabbit", "hat"), Difficulty.Medium)
            .Card(PeopleCategory, Body("ASTRONAUT", "space", "rocket", "moon"), Difficulty.Easy)
            .Card(PeopleCategory, Body("REFEREE", "whistle", "sport", "rules"), Difficulty.Medium)
            .Card(PeopleCategory, Body("PIRATE", "ship", "treasure", "parrot"), Difficulty.Easy)
            .Card(PeopleCategory, Body("DENTIST", "teeth", "drill", "mouth"), Difficulty.Medium)
            .Card(PeopleCategory, Body("DETECTIVE", "mystery", "clues", "solve"), Difficulty.Hard)
            .Card(PeopleCategory, Body("LIFEGUARD", "pool", "rescue", "whistle"), Difficulty.Medium)

        // ── HARD MODE ─────────────────────────────────────────────────────────
            .Category(HardModeCategory)
            .Card(HardModeCategory, Body("GRAVITY", "fall", "Earth", "Newton"), Difficulty.Extreme)
            .Card(HardModeCategory, Body("ECHO", "sound", "repeat", "cave"), Difficulty.Extreme)
            .Card(HardModeCategory, Body("SHADOW", "dark", "light", "sun"), Difficulty.Extreme)
            .Card(HardModeCategory, Body("DÉJÀ VU", "before", "feeling", "again"), Difficulty.Extreme)
            .Card(HardModeCategory, Body("WIFI", "internet", "signal", "router"), Difficulty.Extreme)
            .Card(HardModeCategory, Body("MIDNIGHT", "twelve", "night", "clock"), Difficulty.Extreme)
            .Card(HardModeCategory, Body("NOSTALGIA", "past", "memory", "miss"), Difficulty.Extreme)
            .Card(HardModeCategory, Body("SARCASM", "joke", "tone", "mean"), Difficulty.Extreme)

        // ── EXPANSION: FAN FAVOURITES ─────────────────────────────────────────
            .Category(ThingsCategory)
            .Card(ThingsCategory, Body("SELFIE", "photo", "phone", "yourself"), Difficulty.Medium)
            .Card(ThingsCategory, Body("ALARM CLOCK", "wake", "morning", "ring"), Difficulty.Easy)
            .Card(ThingsCategory, Body("GLITTER", "sparkle", "craft", "everywhere"), Difficulty.Hard)
            .Card(ThingsCategory, Body("SOCKS", "feet", "pair", "missing"), Difficulty.Easy)
            .Category(ActionsCategory)
            .Card(ActionsCategory, Body("PROCRASTINATE", "later", "delay", "tomorrow"), Difficulty.Hard)
            .Card(ActionsCategory, Body("BINGE-WATCH", "episodes", "series", "one more"), Difficulty.Medium)
            .Card(ActionsCategory, Body("GHOSTING", "reply", "disappear", "message"), Difficulty.Hard)
            .Card(ActionsCategory, Body("EAVESDROP", "listen", "secret", "conversation"), Difficulty.Medium)
            .Category(PeopleCategory)
            .Card(PeopleCategory, Body("INFLUENCER", "followers", "post", "sponsored"), Difficulty.Medium)
            .Card(PeopleCategory, Body("VILLAIN", "evil", "hero", "plan"), Difficulty.Medium)
            .Card(PeopleCategory, Body("TODDLER", "small", "tantrum", "nap"), Difficulty.Easy)
            .Card(PeopleCategory, Body("CONSPIRACY THEORIST", "government", "secret", "truth"), Difficulty.Hard)
            .Category(HardModeCategory)
            .Card(HardModeCategory, Body("AWKWARD SILENCE", "quiet", "uncomfortable", "conversation"), Difficulty.Extreme)
            .Card(HardModeCategory, Body("REVENGE", "payback", "hurt", "even"), Difficulty.Extreme)
            .Card(HardModeCategory, Body("PASSWORD", "secret", "login", "forgot"), Difficulty.Extreme)
            .Card(HardModeCategory, Body("MONDAY", "week", "work", "morning"), Difficulty.Extreme)

            .Build();

    private static string Body(string target, string ban1, string ban2, string ban3) =>
        "<b>Your word: " + target + "</b>\n\n" +
        "<b>FORBIDDEN:</b> " + ban1 + " · " + ban2 + " · " + ban3 + "\n\n" +
        "Describe it out loud without saying your word, any forbidden word, or any form of them. " +
        "Everyone else shouts guesses — first correct guess scores for both of you.\n\n" +
        "<i>Slip up and say a forbidden word? Round over. The table decides. The table is merciless.</i>";
}
