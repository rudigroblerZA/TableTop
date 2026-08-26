using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Abstractions.Restrictions;
using TableTop.Core.Domain.Restrictions;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Party;

/// <summary>
/// Last Orders — a pub-night dare deck for adults, built so that the drinking
/// is the smallest part of it.
///
/// Three things shape the design, and they are structural rather than
/// decorative:
///
///   1. THE SOFT OPTION IS EQUAL. Every card that involves a drink offers a
///      soft version in the same breath, and they score identically. Nobody is
///      ever behind for not drinking, and nobody has to announce why. Plenty of
///      people at any table are driving, pregnant, on medication, in recovery,
///      or simply not in the mood.
///
///   2. SIPS, NOT SHOTS. There is no card here that says down it, chug, finish
///      your drink, or race anyone. A sip is the whole unit. Drinking games
///      cause harm through volume and speed, so the deck removes both levers —
///      you cannot lose this game by drinking less.
///
///   3. THE DECK OPENS AND CLOSES ON CARE. House Rules come first (pinned, so
///      shuffling cannot bury them) and set the pace, the water, and the way
///      home. Last Round closes it: water, food, and checking on each other.
///
/// The cards that actually involve alcohol carry a minimum-age restriction, so
/// they are dealt only to players who have entered an age that meets it. That
/// gate FAILS CLOSED: a player who left age blank simply never sees them, and
/// the rest of the deck plays normally around it.
///
/// Adult (18+), and the House Rules card says plainly that local legal drinking
/// age applies — it is 21 in some countries and this deck does not know where
/// it is being played.
/// </summary>
public sealed class LastOrdersMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <summary>A pub game for adults out together. Drink cards are age-gated, but the whole framing assumes a night out.</summary>
    public TableShape SuitableFor => TableShape.Group | TableShape.Team;

    /// <summary>The legal-drinking-age floor used for the alcohol cards.</summary>
    private const int DrinkingAge = 18;

    /// <inheritdoc />
    public override string Name        => "Last Orders";
    /// <inheritdoc />
    public override string Description =>
        "Pub-night dares for grown-ups. Sips not shots, the soft option always counts the same, and nobody loses for drinking less.";

    /// <summary>Label for a completed dare.</summary>
    public override string CompleteLabel => "Done";
    /// <summary>Label for passing — free, always, no reason needed.</summary>
    public override string SkipLabel     => "Pass (always fine)";

    /// <summary>House rules open the night; Last Round closes it.</summary>
    public override IReadOnlyList<string> CategoriesPinnedToStart => ["House Rules"];

    /// <inheritdoc cref="CategoriesPinnedToStart" />
    public override IReadOnlyList<string> CategoriesPinnedToEnd => ["Last Round"];

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["House Rules"]  = "#26A69A",
            ["Warm Up"]      = "#66BB6A",
            ["Party Tricks"] = "#42A5F5",
            ["Confessions"]  = "#AB47BC",
            ["Forfeits"]     = "#FFA726",
            ["Last Round"]   = "#7E57C2",
        };

    /// <summary>Flat scoring — the soft option must never score less.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Returns the built-in Last Orders card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        LastOrdersCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => LastOrdersCardBank.All;

    /// <summary>The age gate applied to cards that involve alcohol.</summary>
    internal static int MinimumDrinkingAge => DrinkingAge;
}

/// <summary>Built-in card bank for Last Orders.</summary>
public static class LastOrdersCardBank
{
    /// <summary>All cards, in intended play order.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build()
    {
        // Only the cards that actually involve alcohol carry the gate. The
        // social dares are open to everyone at the table.
        var drinkingAge = new MinimumAgeRestriction(18);

        return
        [
            // ── HOUSE RULES — pinned first, and worth reading aloud ──────────
            H("House Rules — Before Anything",
              "Read this out before you start.\n\n" +
              "• A sip means a sip. Nothing in this deck asks anyone to down a drink, race, or keep up.\n" +
              "• The soft option counts exactly the same. Water, a soft drink, or a mimed sip all score the point. Nobody explains why.\n" +
              "• Pass is always free, on any card, with no reason and no forfeit.\n" +
              "• Local law applies — the legal drinking age is 18 in some countries and 21 in others. Everyone here should be over whichever applies."),
            H("House Rules — The Night Itself",
              "Agree these now, while everyone is sober enough to mean it.\n\n" +
              "• Who is driving, and what are they drinking? Answer: something soft, all night.\n" +
              "• Water between rounds. Put a jug on the table before the first card.\n" +
              "• Food happens. Order it early rather than at midnight.\n" +
              "• If someone has had enough, that's the end of it for them — no persuading, no jokes about it.\n" +
              "• Anyone can call last orders on the whole game at any point."),

            // ── WARM UP — social, no drinking at all ─────────────────────────
            S("Warm Up", "Round of Introductions",
              "Introduce the person on your left as though they are a minor celebrity and you are their long-suffering agent."),
            S("Warm Up", "The Group Photo",
              "Direct everyone into a group photo in the style of a very serious album cover. You have thirty seconds."),
            S("Warm Up", "Terrible Toast",
              "Propose a toast to something gloriously unimportant. Everyone raises whatever they're drinking."),
            S("Warm Up", "Accent Roulette",
              "Order your next drink — real or imaginary — in an accent of the table's choosing."),
            S("Warm Up", "Two Truths",
              "Two truths and a lie about your worst night out. The table guesses."),
            S("Warm Up", "The Nickname",
              "Give everyone at the table a nickname based on the first thing you noticed about them tonight. They're keeping it for the rest of the game."),
            S("Warm Up", "Two Minutes' Notice",
              "You've been asked to give a two-minute speech at this table's wedding. You don't know whose. Begin."),

            // ── PARTY TRICKS — performative, still no drinking ───────────────
            S("Party Tricks", "The Impression",
              "Do your best impression of someone at this table. They get to rate it out of ten."),
            S("Party Tricks", "Sixty-Second Rant",
              "Rant passionately for sixty seconds about something trivial that genuinely annoys you."),
            S("Party Tricks", "The Dance Move",
              "Invent a dance move, name it, and teach it to the person on your right."),
            S("Party Tricks", "Sing It Badly",
              "Sing the chorus of any song, deliberately in the wrong style. Opera, sea shanty, lullaby — table picks."),
            S("Party Tricks", "The Statue",
              "Hold a dramatic pose until someone else draws a card. Commit to it."),
            S("Party Tricks", "Accent Relay",
              "Say the same sentence in three different accents. The table picks which one you're keeping for the next round."),
            S("Party Tricks", "Genuinely Useless Talent",
              "Demonstrate the most useless skill you possess. It must be genuinely useless and genuinely yours."),

            // ── CONFESSIONS — truth-style, no drinking ───────────────────────
            S("Confessions", "The Group Chat",
              "What is the most recent thing you sent to a group chat and immediately regretted?"),
            S("Confessions", "Worst Purchase",
              "What's the most money you've spent on something you used precisely once?"),
            S("Confessions", "The White Lie",
              "Name a small lie you tell regularly. Nothing serious — just the everyday kind."),
            S("Confessions", "Unpopular Opinion",
              "Share a genuinely unpopular opinion and defend it for thirty seconds."),
            S("Confessions", "The Text You Didn't Send",
              "Describe — don't read — a message you typed out and then deleted."),
            S("Confessions", "Left On Read",
              "What's the message you've left unanswered the longest, and what's the real reason?"),
            S("Confessions", "Worst Money",
              "What is the worst thing you have ever spent money on — and would you do it again?"),

            // ── FORFEITS — the drink-or-soft cards, age-gated ────────────────
            D("Forfeits", "Cheers To That",
              "Take a sip — or a soft sip, they're the same here — and say what you're actually toasting.",
              drinkingAge),
            D("Forfeits", "The Last Person Who…",
              "Last person to laugh takes a sip. Soft counts. Nobody keeps score of who's drinking what.",
              drinkingAge),
            D("Forfeits", "Categories",
              "Name a category. Go round the table. First to stumble takes a sip — or a soft one — and picks the next category.",
              drinkingAge),
            D("Forfeits", "Never Have I Ever, Gently",
              "Say something you've never done. Anyone who has takes a sip, or the soft equivalent, and may explain — or may not.",
              drinkingAge),
            D("Forfeits", "Toast the Room",
              "Raise your glass to someone at the table and say one true nice thing. Everyone sips with you, soft or otherwise.",
              drinkingAge),
            D("Forfeits", "Swap Rounds",
              "Buy or fetch the next round for the person opposite — including finding out what soft option they'd actually enjoy.",
              drinkingAge),
            D("Forfeits", "Toast the Absent",
              "Take a sip — soft counts, same as ever — and toast someone who isn't here tonight. Say why them.",
              drinkingAge),
            D("Forfeits", "The Round You Owe",
              "Take a sip, soft or otherwise, and name the person at this table you'd most like to buy a drink for, and what it would be.",
              drinkingAge),

            // ── LAST ROUND — pinned last ─────────────────────────────────────
            L("Water Round",
              "Everyone gets a glass of water. All of you, now, before the next thing. This card is not optional and not a joke."),
            L("Something To Eat",
              "Food. Order it, raid the kitchen, walk somewhere that sells chips. Whatever's easiest — just eat something."),
            L("Getting Home",
              "Sort out how everyone is getting home, and check that the plan is the one you agreed at the start. " +
              "Confirm nobody who's been drinking is driving. Wait with anyone who's on their own."),
            L("Last Orders",
              "That's the deck. Check in with each other before you drift off — anyone quiet, anyone who's had more than they meant to, anyone who needs a lift or a sofa. " +
              "Good nights end with everyone accounted for."),
            L("The Good Bit",
              "Everyone names the best moment of the night so far. No repeats, so the slow ones have to think."),
            L("Tomorrow",
              "Everyone says one thing they're doing tomorrow. It's a good way to remember there's a tomorrow."),
        ];
    }

    // House rules: teal header, no gate — everyone reads these.
    private static ICard H(string title, string body) =>
        StandardCard.Create(title,
            "<b>📋 HOUSE RULES</b>\n\n" + body,
            Difficulty.Easy, "House Rules");

    // Social dares: no alcohol, so no age gate.
    private static ICard S(string category, string title, string body) =>
        StandardCard.Create(title,
            "<b>" + Emoji(category) + " " + category.ToUpperInvariant() + "</b>\n\n" +
            body + "\n\n" +
            "<i>Pass is always free.</i>",
            Difficulty.Easy, category);

    // Drink cards: age-gated, and the soft option is stated on every one.
    private static ICard D(string category, string title, string body, IRestriction gate) =>
        StandardCard.Create(title,
            "<b>🍻 " + category.ToUpperInvariant() + "</b>\n\n" +
            body + "\n\n" +
            "<i>A sip is a sip — never a shot, never the whole glass. Soft drinks count the same and score the same. " +
            "Pass is always free.</i>",
            Difficulty.Easy, category, restriction: gate);

    private static ICard L(string title, string body) =>
        StandardCard.Create(title,
            "<b>🌙 LAST ROUND</b>\n\n" + body,
            Difficulty.Easy, "Last Round");

    private static string Emoji(string category) => category switch
    {
        "Warm Up"      => "🌤️",
        "Party Tricks" => "🎭",
        "Confessions"  => "🎤",
        _              => "•",
    };
}
