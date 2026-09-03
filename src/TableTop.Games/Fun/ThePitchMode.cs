using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Fun;

/// <summary>
/// The Pitch — you have thirty seconds and an indefensible product. Sell it
/// anyway.
///
/// How to play:
///   1. Turn a card. It names a product nobody wants and a <b>catch</b> — an
///      angle you must take, a market you must aim at, or a word you may not
///      say.
///   2. Thirty seconds on the clock. Stand up. Pitch it as though the money is
///      real and the room is your last investor.
///   3. The table votes. You can't vote for yourself, and a pitch that ignored
///      its catch doesn't get counted no matter how funny it was.
///
/// <para>
/// <b>The catch is the game.</b> Without it this is just "be funny about a
/// silly object", which the room will exhaust in four cards — everyone reaches
/// for the same joke, which is that the product is bad. Forcing a specific
/// frame ("pitch it as an environmental breakthrough", "never use the word
/// regret") means the obvious joke is off the table from the first second, and
/// the interesting version is the only one left.
/// </para>
///
/// <para>
/// Sits opposite <c>OneStarReviewsMode</c> on purpose: that deck is about
/// demolishing something loved, this one about defending something
/// indefensible. Same muscle, opposite direction.
/// </para>
/// </summary>
public sealed class ThePitchMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <summary>
    /// Anything but a couple. The vote is the scoring mechanism, and with two
    /// people every round is one person judging the other — which is a
    /// different, much less fun game.
    /// </summary>
    public TableShape SuitableFor => TableShape.Family | TableShape.Team | TableShape.Group;

    /// <inheritdoc />
    public override string Name => "The Pitch";

    /// <inheritdoc />
    public override string Description =>
        "Thirty seconds to sell something nobody wants — and the card decides the angle you have to take. The table votes, and you can't vote for yourself.";

    /// <summary>Label for a pitch the room bought.</summary>
    public override string CompleteLabel => "Sold!";

    /// <summary>Label for a pitch that died on the table.</summary>
    public override string SkipLabel => "No Sale";

    /// <summary>Three, so there is someone left to vote after the pitcher and one rival.</summary>
    public override int MinimumPlayers => 3;

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [ThePitchCardBank.EverydayNonsenseCategory] = "#FFCA28",
            [ThePitchCardBank.TechnologyCategory] = "#42A5F5",
            [ThePitchCardBank.FoodAndDrinkCategory] = "#FF7043",
            [ThePitchCardBank.ServicesCategory] = "#26A69A",
            [ThePitchCardBank.RebrandCategory] = "#AB47BC",
        };

    /// <summary>One point per pitch the room bought. Difficulty here is how hard the catch is, not how much a win is worth.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Returns the built-in The Pitch card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        ThePitchCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => ThePitchCardBank.All;
}

/// <summary>
/// Built-in card bank for The Pitch. Authored with <see cref="CardDeckBuilder"/>
/// so ids derive from card content and stay stable across restarts.
/// </summary>
public static class ThePitchCardBank
{
    internal const string EverydayNonsenseCategory = "Everyday Nonsense";
    internal const string TechnologyCategory = "Technology";
    internal const string FoodAndDrinkCategory = "Food & Drink";
    internal const string ServicesCategory = "Services";
    internal const string RebrandCategory = "The Rebrand";

    /// <summary>All The Pitch cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    /// <summary>
    /// Formats a card. Product and catch are separated visually because the
    /// catch is the constraint people skip when they're mid-laugh, and it's the
    /// half that makes the round scoreable.
    /// </summary>
    private static string Pitch(string product, string constraint) =>
        "<b>💡 Thirty seconds. Stand up. Sell it.</b>\n\n" +
        "<b>The product:</b> " + product + "\n\n" +
        "<b>The catch:</b> " + constraint + "\n\n" +
        "<i>Table votes at the end — no voting for yourself. Ignore the catch and the pitch " +
        "doesn't count, however good it was.</i>";

    private static IReadOnlyList<ICard> Build() => CardDeckBuilder
        .For("The Pitch")

        // ── EVERYDAY NONSENSE ────────────────────────────────────────────────
        .Category(EverydayNonsenseCategory)
            .Card("The Single Sock Subscription",
                Pitch("A monthly subscription that posts you exactly one sock. Never a pair. Never the same colour twice.",
                      "Pitch it as an environmental breakthrough. You must sound like you believe it."),
                Difficulty.Easy)
            .Card("The Silent Alarm Clock",
                Pitch("An alarm clock with no sound, no light, no vibration and no display.",
                      "It has to actually wake people up, and you have to explain how."),
                Difficulty.Hard)
            .Card("Pre-Crumpled Paper",
                Pitch("A ream of A4 that arrives already screwed into a ball, then flattened out again.",
                      "Pitch it to an office manager with a budget and a straight face."),
                Difficulty.Medium)
            .Card("The Left-Handed Ladder",
                Pitch("A ladder specifically engineered for left-handed people. It looks identical to a normal ladder.",
                      "You must describe the engineering in detail, and use at least two invented technical terms."),
                Difficulty.Medium)
            .Card("Waterproof Teabags",
                Pitch("Teabags with a fully waterproof coating. Nothing gets in, nothing gets out.",
                      "Pitch it as a luxury item and name a price above £40."),
                Difficulty.Medium)
            .Card("The Inverted Umbrella",
                Pitch("An umbrella built upside down. It collects rain in a generous bowl above your head and, when full, empties it onto you.",
                      "Pitch it to parents of small children as a good idea."),
                Difficulty.Hard)

        // ── TECHNOLOGY ───────────────────────────────────────────────────────
        .Category(TechnologyCategory)
            .Card("The Honest Assistant",
                Pitch("A voice assistant that answers every question truthfully, including what it thinks of the question.",
                      "Pitch it as a wellbeing device. It must sound caring."),
                Difficulty.Medium)
            .Card("Wi-Fi That Gets Worse",
                Pitch("A router that deliberately degrades your connection a little more every hour you stay online.",
                      "Pitch it to a head teacher, at a school assembly."),
                Difficulty.Easy)
            .Card("The Argument App",
                Pitch("An app that takes the opposite side of anything you say and refuses to back down.",
                      "Name a specific target market in your first sentence and stay loyal to it."),
                Difficulty.Medium)
            .Card("Shoes That Judge You",
                Pitch("Smart trainers that rate every route you take and comment on your choices out loud.",
                      "Deliver the entire pitch in the voice of a nature documentary narrator."),
                Difficulty.Hard)
            .Card("The Unsend Button",
                Pitch("A button that deletes the last thing you said out loud from everyone's memory. It works about 60% of the time.",
                      "You may not use the words 'regret', 'sorry' or 'mistake' at any point."),
                Difficulty.Hard)
            .Card("The One-Button Phone",
                Pitch("A phone with a single button. No screen, no speaker, no camera.",
                      "You may only reveal what the button does in your final five seconds."),
                Difficulty.Extreme)

        // ── FOOD & DRINK ─────────────────────────────────────────────────────
        .Category(FoodAndDrinkCategory)
            .Card("Soup You Chew",
                Pitch("A soup with the consistency of a firm sandwich. Sold in a mug, eaten with your teeth.",
                      "Pitch it as a health food, with at least one confident nutritional claim."),
                Difficulty.Easy)
            .Card("The Flavourless Crisp",
                Pitch("A crisp engineered to taste of absolutely nothing. Texture only.",
                      "Pitch it as a luxury product to people with more money than sense."),
                Difficulty.Medium)
            .Card("Evening Cereal",
                Pitch("A breakfast cereal that may only be eaten after 9pm. The box locks until then.",
                      "Pitch it as a family tradition that's been in your family for generations."),
                Difficulty.Medium)
            .Card("Sleepy Coffee",
                Pitch("Coffee that tastes exactly like coffee and reliably puts you to sleep within ten minutes.",
                      "Pitch it to night-shift workers, and make it sound like you're on their side."),
                Difficulty.Hard)
            .Card("The Everlasting Single-Use Fork",
                Pitch("A disposable plastic fork guaranteed to last four hundred years. Single use only.",
                      "You must acknowledge the contradiction out loud, then defend it."),
                Difficulty.Hard)
            .Card("Ice Cream That Never Melts",
                Pitch("An ice cream that stays perfectly solid at any temperature. It also never gets any softer in your mouth.",
                      "Pitch it as a gift — you must name the exact occasion and the recipient."),
                Difficulty.Medium)

        // ── SERVICES ─────────────────────────────────────────────────────────
        .Category(ServicesCategory)
            .Card("Professional Queue Stander",
                Pitch("You hire a person to stand in a queue for you. They will not tell you what the queue is for.",
                      "Name your hourly rate in the first ten seconds and justify it in the last ten."),
                Difficulty.Easy)
            .Card("Rent-a-Grandparent",
                Pitch("An hour of someone else's grandparent — advice, opinions on your coat, and an unsolicited biscuit.",
                      "Pitch it as a public service that deserves government funding."),
                Difficulty.Medium)
            .Card("The Excuse Agency",
                Pitch("A firm that invents, documents and stands behind your excuse for anything you'd rather not attend.",
                      "Everything you describe must be entirely legal. Say so, convincingly."),
                Difficulty.Hard)
            .Card("Someone to Watch You Work",
                Pitch("A subscription where a silent stranger observes you working, on camera, all day.",
                      "Pitch it as productivity software. You may not refer to the watcher as a person."),
                Difficulty.Hard)
            .Card("Holiday Photos, No Holiday",
                Pitch("A service that produces a full album of your two weeks in Croatia. You never leave the house.",
                      "Pitch it with complete honesty — no pretending the holiday happened."),
                Difficulty.Medium)
            .Card("The Apology Service",
                Pitch("Professional writers craft your apology, deliver it, and take the resulting silence on your behalf.",
                      "Your entire pitch must itself be phrased as an apology."),
                Difficulty.Extreme)

        // ── THE REBRAND ──────────────────────────────────────────────────────
        .Category(RebrandCategory)
            .Card("Rebrand: Homework",
                Pitch("Homework. Unchanged in every respect — same amount, same subjects, same deadline.",
                      "Rebrand it so that a nine-year-old asks for more of it."),
                Difficulty.Hard)
            .Card("Rebrand: Monday",
                Pitch("Monday. You cannot move it, shorten it or abolish it.",
                      "Sell it as the single best day of the week, and give it a new name."),
                Difficulty.Medium)
            .Card("Rebrand: Broccoli",
                Pitch("Broccoli. Same taste, same smell, same steam.",
                      "Aim it squarely at teenagers. It must not sound like it came from an adult."),
                Difficulty.Medium)
            .Card("Rebrand: Waiting",
                Pitch("Waiting. The activity of doing nothing while something else happens.",
                      "Position it as a premium experience with a waiting list of its own."),
                Difficulty.Hard)
            .Card("Rebrand: Hoovering",
                Pitch("Vacuuming the living room carpet. All of it, including under the sofa.",
                      "Rebrand it as a competitive sport, complete with rules and a governing body."),
                Difficulty.Medium)
            .Card("Rebrand: Rain",
                Pitch("Rain. Three consecutive weeks of it, in a town with one café.",
                      "Sell it as a destination. You must name the town and quote a review."),
                Difficulty.Easy)

        .Build();
}
