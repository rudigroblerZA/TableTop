using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Domain.Cards;

namespace TableTop.Games.Fun;

/// <summary>
/// Claimed! — area control played with prompt cards instead of pieces.
///
/// Five territories, each its own kind of challenge. On your turn, challenge
/// open ground to claim it, or challenge a rival's territory to try to take it
/// — succeed and you either gain it or steal it; fail and nothing changes,
/// turn passes. First to hold three territories at once wins outright; if
/// every territory's deck runs dry first, whoever holds the most wins.
///
/// Every other mode in the catalogue is either a card-turn loop (draw, react,
/// score) or, for the handful with a different shape — Millionaire's ladder,
/// Monogamy's zones — a bespoke controller built specifically for that shape.
/// This is a third bespoke shape: a shared board the whole table can see and
/// contest, rather than content addressed to one player at a time. It needs no
/// new content format — territories are just <see cref="ICard.Category"/>
/// groups, so any multi-category deck already qualifies; this mode ships its
/// own five.
/// </summary>
public sealed class ClaimedMode : IGameMode, IClaimedDeckProvider
{
    /// <inheritdoc />
    public string Name => "Claimed!";

    /// <inheritdoc />
    public string Description =>
        "Challenge open ground to claim it, or raid a rival's territory to steal it. " +
        "Hold three at once and it's yours.";

    /// <inheritdoc />
    public int WinningTerritoryCount => 3;

    /// <inheritdoc />
    public IReadOnlyList<ICard> GetClaimedDeck() =>
        ClaimedCardBank.All;
}

/// <summary>
/// Compiled fallback for <see cref="ClaimedMode"/>. A static list, so card ids
/// stay stable across runs — <c>ClaimedController</c>
/// shuffles within each territory at construction time, but the ids themselves
/// have to be fixed or the same card could look "new" on a rebuild.
/// </summary>
internal static class ClaimedCardBank
{
    internal const string TriviaCategory = "Trivia";
    internal const string WordplayCategory = "Wordplay";
    internal const string PerformanceCategory = "Performance";
    internal const string LogicCategory = "Logic";
    internal const string SpeedCategory = "Speed";

    private static ICard C(string territory, string title, string body, Difficulty difficulty) =>
        new StandardCard(
            id: StableId(territory, title, body),
            title: title,
            description: body,
            difficulty: difficulty,
            category: territory);

    private static Guid StableId(string territory, string title, string body) =>
        new(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"claimed|{territory}|{title}|{body}"))[..16]);

    public static IReadOnlyList<ICard> All { get; } =
    [
        // ── Trivia — general knowledge, table judges from memory or a phone ─────
        C(TriviaCategory, "Planets",   "How many planets are in our solar system?", Difficulty.Easy),
        C(TriviaCategory, "Elements",  "Name the chemical symbol for gold.", Difficulty.Medium),
        C(TriviaCategory, "Capitals",  "What's the capital of Australia? (It isn't Sydney.)", Difficulty.Medium),
        C(TriviaCategory, "Records",   "What's the world's longest river — Nile or Amazon? Depends who you ask, but pick one and defend it.", Difficulty.Hard),
        C(TriviaCategory, "Inventors", "Who's credited with inventing the telephone?", Difficulty.Medium),
        C(TriviaCategory, "Bodies",    "How many bones are in the adult human body?", Difficulty.Hard),

        // ── Wordplay — puzzle it out under time pressure ────────────────────────
        C(WordplayCategory, "Anagram",    "Unscramble: NEAPL. (One word, six letters — an appliance.)", Difficulty.Medium),
        C(WordplayCategory, "Rhyme Time", "Name three words that rhyme with 'light' — go, out loud, no repeats.", Difficulty.Easy),
        C(WordplayCategory, "Hidden Word","Find the smaller word hidden inside 'CARPET.' (There are two — name either.)", Difficulty.Medium),
        C(WordplayCategory, "Opposite Day","Say the opposite of 'transparent' — the real word, not just 'not transparent.'", Difficulty.Hard),
        C(WordplayCategory, "Compound",   "Combine 'sun' with another word to make a real compound word. Name three in 15 seconds.", Difficulty.Easy),
        C(WordplayCategory, "Missing Vowels", "What word is this with the vowels removed: 'BC_S_'?", Difficulty.Hard),

        // ── Performance — act it out, the table judges ──────────────────────────
        C(PerformanceCategory, "Silent Movie",  "Act out 'brushing your teeth' with no sound and no props for 15 seconds. Table judges if it read clearly.", Difficulty.Easy),
        C(PerformanceCategory, "One-Word Story","Tell a 10-second story using only the word 'banana,' with tone and gesture doing the rest.", Difficulty.Medium),
        C(PerformanceCategory, "Animal Impression", "Do your best impression of an animal of the table's choosing. They pick after you draw this card.", Difficulty.Easy),
        C(PerformanceCategory, "Emotion Swap",  "Say the sentence 'I can't believe it's already Monday' as if you just won the lottery.", Difficulty.Medium),
        C(PerformanceCategory, "Freeze Frame",  "Strike a pose that represents 'victory' and hold it for 10 seconds without laughing.", Difficulty.Hard),
        C(PerformanceCategory, "Weather Report","Deliver a 15-second weather forecast as if it were the most exciting news of the year.", Difficulty.Medium),

        // ── Logic — riddles and deduction, answer explained if it stumps them ───
        C(LogicCategory, "Weight",     "Which weighs more: a kilogram of feathers or a kilogram of bricks?", Difficulty.Easy),
        C(LogicCategory, "Sequence",   "What comes next in the sequence: 2, 4, 8, 16, __?", Difficulty.Medium),
        C(LogicCategory, "Riddle",     "What has keys but can't open locks, space but no room, and you can enter but not go inside?", Difficulty.Hard),
        C(LogicCategory, "Liar",       "Two people: one always lies, one always tells the truth. You can ask one question to find the truth-teller — what do you ask?", Difficulty.Hard),
        C(LogicCategory, "Odd One Out","Which doesn't belong: apple, banana, carrot, orange? Explain your reasoning, not just the answer.", Difficulty.Medium),
        C(LogicCategory, "Counting",   "A farmer has 17 sheep. All but 9 die. How many are left?", Difficulty.Medium),

        // ── Speed — ten seconds, no thinking time ───────────────────────────────
        C(SpeedCategory, "Colours",   "Name five colours in 10 seconds. Go.", Difficulty.Easy),
        C(SpeedCategory, "Countries", "Name five countries in 10 seconds. Go.", Difficulty.Easy),
        C(SpeedCategory, "Fruits",    "Name five fruits in 10 seconds. Go.", Difficulty.Easy),
        C(SpeedCategory, "Movies",    "Name five movies in 10 seconds. Go.", Difficulty.Medium),
        C(SpeedCategory, "Body Parts","Name eight body parts in 10 seconds. Go.", Difficulty.Medium),
        C(SpeedCategory, "Kitchen",   "Name six things you'd find in a kitchen in 10 seconds. Go.", Difficulty.Easy),
    ];
}
