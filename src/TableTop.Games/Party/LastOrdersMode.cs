using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
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
    public override string Name => "Last Orders";
    /// <inheritdoc />
    public override string Description =>
        "Pub-night dares for grown-ups. Sips not shots, the soft option always counts the same, and nobody loses for drinking less.";

    /// <summary>Label for a completed dare.</summary>
    public override string CompleteLabel => "Done";
    /// <summary>Label for passing — free, always, no reason needed.</summary>
    public override string SkipLabel => "Pass (always fine)";

    /// <summary>House rules open the night; Last Round closes it.</summary>
    public override IReadOnlyList<string> CategoriesPinnedToStart => [LastOrdersCardBank.HouseRulesCategory];

    /// <inheritdoc cref="CategoriesPinnedToStart" />
    public override IReadOnlyList<string> CategoriesPinnedToEnd => [LastOrdersCardBank.LastRoundCategory];

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [LastOrdersCardBank.HouseRulesCategory] = "#26A69A",
            [LastOrdersCardBank.WarmUpCategory] = "#66BB6A",
            [LastOrdersCardBank.PartyTricksCategory] = "#42A5F5",
            [LastOrdersCardBank.ConfessionsCategory] = "#AB47BC",
            [LastOrdersCardBank.ForfeitsCategory] = "#FFA726",
            [LastOrdersCardBank.LastRoundCategory] = "#7E57C2",
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
    internal const string HouseRulesCategory = "House Rules";
    internal const string ConfessionsCategory = "Confessions";
    internal const string ForfeitsCategory = "Forfeits";

    internal const string WarmUpCategory = "Warm Up";
    internal const string PartyTricksCategory = "Party Tricks";
    internal const string LastRoundCategory = "Last Round";

    private const string Deck = "Last Orders";

    /// <summary>All cards, in intended play order.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build()
    {
        // Only the cards that actually involve alcohol carry the gate. The
        // social dares are open to everyone at the table.
        var drinkingAge = new MinimumAgeRestriction(18);

        return CardDeckBuilder.For(Deck)

            // ── HOUSE RULES — pinned first, and worth reading aloud ──────────
            .Category(HouseRulesCategory)
            .Card("House Rules — Before Anything", HouseRulesBody(
                "Read this out before you start.\n\n" +
                "• A sip means a sip. Nothing in this deck asks anyone to down a drink, race, or keep up.\n" +
                "• The soft option counts exactly the same. Water, a soft drink, or a mimed sip all score the point. Nobody explains why.\n" +
                "• Pass is always free, on any card, with no reason and no forfeit.\n" +
                "• Local law applies — the legal drinking age is 18 in some countries and 21 in others. Everyone here should be over whichever applies."), Difficulty.Easy)
            .Card("House Rules — The Night Itself", HouseRulesBody(
                "Agree these now, while everyone is sober enough to mean it.\n\n" +
                "• Who is driving, and what are they drinking? Answer: something soft, all night.\n" +
                "• Water between rounds. Put a jug on the table before the first card.\n" +
                "• Food happens. Order it early rather than at midnight.\n" +
                "• If someone has had enough, that's the end of it for them — no persuading, no jokes about it.\n" +
                "• Anyone can call last orders on the whole game at any point."), Difficulty.Easy)

            // ── WARM UP — social, no drinking at all ─────────────────────────
            .Category(WarmUpCategory)
            .Card("Round of Introductions", SocialBody(WarmUpCategory,
                "Introduce the person on your left as though they are a minor celebrity and you are their long-suffering agent."), Difficulty.Easy)
            .Card("The Group Photo", SocialBody(WarmUpCategory,
                "Direct everyone into a group photo in the style of a very serious album cover. You have thirty seconds."), Difficulty.Easy)
            .Card("Terrible Toast", SocialBody(WarmUpCategory,
                "Propose a toast to something gloriously unimportant. Everyone raises whatever they're drinking."), Difficulty.Easy)
            .Card("Accent Roulette", SocialBody(WarmUpCategory,
                "Order your next drink — real or imaginary — in an accent of the table's choosing."), Difficulty.Easy)
            .Card("Two Truths", SocialBody(WarmUpCategory,
                "Two truths and a lie about your worst night out. The table guesses."), Difficulty.Easy)
            .Card("The Nickname", SocialBody(WarmUpCategory,
                "Give everyone at the table a nickname based on the first thing you noticed about them tonight. They're keeping it for the rest of the game."), Difficulty.Easy)
            .Card("Two Minutes' Notice", SocialBody(WarmUpCategory,
                "You've been asked to give a two-minute speech at this table's wedding. You don't know whose. Begin."), Difficulty.Easy)

            // ── PARTY TRICKS — performative, still no drinking ───────────────
            .Category(PartyTricksCategory)
            .Card("The Impression", SocialBody(PartyTricksCategory,
                "Do your best impression of someone at this table. They get to rate it out of ten."), Difficulty.Easy)
            .Card("Sixty-Second Rant", SocialBody(PartyTricksCategory,
                "Rant passionately for sixty seconds about something trivial that genuinely annoys you."), Difficulty.Easy)
            .Card("The Dance Move", SocialBody(PartyTricksCategory,
                "Invent a dance move, name it, and teach it to the person on your right."), Difficulty.Easy)
            .Card("Sing It Badly", SocialBody(PartyTricksCategory,
                "Sing the chorus of any song, deliberately in the wrong style. Opera, sea shanty, lullaby — table picks."), Difficulty.Easy)
            .Card("The Statue", SocialBody(PartyTricksCategory,
                "Hold a dramatic pose until someone else draws a card. Commit to it."), Difficulty.Easy)
            .Card("Accent Relay", SocialBody(PartyTricksCategory,
                "Say the same sentence in three different accents. The table picks which one you're keeping for the next round."), Difficulty.Easy)
            .Card("Genuinely Useless Talent", SocialBody(PartyTricksCategory,
                "Demonstrate the most useless skill you possess. It must be genuinely useless and genuinely yours."), Difficulty.Easy)

            // ── CONFESSIONS — truth-style, no drinking ───────────────────────
            .Category(ConfessionsCategory)
            .Card("The Group Chat", SocialBody(ConfessionsCategory,
                "What is the most recent thing you sent to a group chat and immediately regretted?"), Difficulty.Easy)
            .Card("Worst Purchase", SocialBody(ConfessionsCategory,
                "What's the most money you've spent on something you used precisely once?"), Difficulty.Easy)
            .Card("The White Lie", SocialBody(ConfessionsCategory,
                "Name a small lie you tell regularly. Nothing serious — just the everyday kind."), Difficulty.Easy)
            .Card("Unpopular Opinion", SocialBody(ConfessionsCategory,
                "Share a genuinely unpopular opinion and defend it for thirty seconds."), Difficulty.Easy)
            .Card("The Text You Didn't Send", SocialBody(ConfessionsCategory,
                "Describe — don't read — a message you typed out and then deleted."), Difficulty.Easy)
            .Card("Left On Read", SocialBody(ConfessionsCategory,
                "What's the message you've left unanswered the longest, and what's the real reason?"), Difficulty.Easy)
            .Card("Worst Money", SocialBody(ConfessionsCategory,
                "What is the worst thing you have ever spent money on — and would you do it again?"), Difficulty.Easy)

            // ── FORFEITS — the drink-or-soft cards, age-gated ────────────────
            .Category(ForfeitsCategory)
            .Card("Cheers To That", DrinkBody(ForfeitsCategory,
                "Take a sip — or a soft sip, they're the same here — and say what you're actually toasting."), Difficulty.Easy, restriction: drinkingAge)
            .Card("The Last Person Who…", DrinkBody(ForfeitsCategory,
                "Last person to laugh takes a sip. Soft counts. Nobody keeps score of who's drinking what."), Difficulty.Easy, restriction: drinkingAge)
            .Card("Categories", DrinkBody(ForfeitsCategory,
                "Name a category. Go round the table. First to stumble takes a sip — or a soft one — and picks the next category."), Difficulty.Easy, restriction: drinkingAge)
            .Card("Never Have I Ever, Gently", DrinkBody(ForfeitsCategory,
                "Say something you've never done. Anyone who has takes a sip, or the soft equivalent, and may explain — or may not."), Difficulty.Easy, restriction: drinkingAge)
            .Card("Toast the Room", DrinkBody(ForfeitsCategory,
                "Raise your glass to someone at the table and say one true nice thing. Everyone sips with you, soft or otherwise."), Difficulty.Easy, restriction: drinkingAge)
            .Card("Swap Rounds", DrinkBody(ForfeitsCategory,
                "Buy or fetch the next round for the person opposite — including finding out what soft option they'd actually enjoy."), Difficulty.Easy, restriction: drinkingAge)
            .Card("Toast the Absent", DrinkBody(ForfeitsCategory,
                "Take a sip — soft counts, same as ever — and toast someone who isn't here tonight. Say why them."), Difficulty.Easy, restriction: drinkingAge)
            .Card("The Round You Owe", DrinkBody(ForfeitsCategory,
                "Take a sip, soft or otherwise, and name the person at this table you'd most like to buy a drink for, and what it would be."), Difficulty.Easy, restriction: drinkingAge)

            // ── LAST ROUND — pinned last ─────────────────────────────────────
            .Category(LastRoundCategory)
            .Card("Water Round", LastRoundBody(
                "Everyone gets a glass of water. All of you, now, before the next thing. This card is not optional and not a joke."), Difficulty.Easy)
            .Card("Something To Eat", LastRoundBody(
                "Food. Order it, raid the kitchen, walk somewhere that sells chips. Whatever's easiest — just eat something."), Difficulty.Easy)
            .Card("Getting Home", LastRoundBody(
                "Sort out how everyone is getting home, and check that the plan is the one you agreed at the start. " +
                "Confirm nobody who's been drinking is driving. Wait with anyone who's on their own."), Difficulty.Easy)
            .Card("Last Orders", LastRoundBody(
                "That's the deck. Check in with each other before you drift off — anyone quiet, anyone who's had more than they meant to, anyone who needs a lift or a sofa. " +
                "Good nights end with everyone accounted for."), Difficulty.Easy)
            .Card("The Good Bit", LastRoundBody(
                "Everyone names the best moment of the night so far. No repeats, so the slow ones have to think."), Difficulty.Easy)
            .Card("Tomorrow", LastRoundBody(
                "Everyone says one thing they're doing tomorrow. It's a good way to remember there's a tomorrow."), Difficulty.Easy)

            .Build();
    }

    // House rules: teal header, no gate — everyone reads these.
    private static string HouseRulesBody(string body) =>
        "<b>📋 HOUSE RULES</b>\n\n" + body;

    // Social dares: no alcohol, so no age gate.
    private static string SocialBody(string category, string body) =>
        "<b>" + Emoji(category) + " " + category.ToUpperInvariant() + "</b>\n\n" +
        body + "\n\n" +
        "<i>Pass is always free.</i>";

    // Drink cards: age-gated (by the caller), and the soft option is stated on every one.
    private static string DrinkBody(string category, string body) =>
        "<b>🍻 " + category.ToUpperInvariant() + "</b>\n\n" +
        body + "\n\n" +
        "<i>A sip is a sip — never a shot, never the whole glass. Soft drinks count the same and score the same. " +
        "Pass is always free.</i>";

    private static string LastRoundBody(string body) =>
        "<b>🌙 LAST ROUND</b>\n\n" + body;

    private static string Emoji(string category) => category switch
    {
        WarmUpCategory => "🌤️",
        PartyTricksCategory => "🎭",
        ConfessionsCategory => "🎤",
        _ => "•",
    };
}
