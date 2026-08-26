using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Caption This — a fast, funny party game about quick wit.
///
/// How to play:
///   1. The active player reads the scene aloud.
///   2. Everyone has fifteen seconds to invent a caption, headline, or one-liner.
///   3. Go round the circle, share them, and vote for the funniest.
///   4. The winner of each round takes the point.
///
/// No drawing, no props, no losers — just describe the most ridiculous version of
/// the moment you can. Three card types keep it varied:
///   Scene     — an absurd freeze-frame to caption
///   Headline  — write the tabloid headline for the event
///   Overheard — invent what someone in the situation just said out loud
///
/// Designed to be relentlessly silly and safe for mixed ages.
/// </summary>
public sealed class CaptionThisMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Caption This";
    /// <inheritdoc />
    public override string Description =>
        "Invent the funniest caption, headline, or one-liner for an absurd scene. Quickest wit wins the round.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "🏆 Funniest";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "⤳ Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Scene"] = "#42A5F5",
            ["Headline"] = "#FFCA28",
            ["Overheard"] = "#EC407A",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        CaptionThisCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => CaptionThisCardBank.All;
}

/// <summary>Built-in card bank for Caption This. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class CaptionThisCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── SCENES ───────────────────────────────────────────────────────────
        Scene("A penguin is standing at a bus stop, holding a tiny umbrella, looking furious.", Difficulty.Easy),
        Scene("A cat has knocked every single item off a desk and is now sitting in the exact centre, staring at you.", Difficulty.Easy),
        Scene("A man in a full suit of medieval armour is trying to order a coffee at a busy café.", Difficulty.Easy),
        Scene("Three pigeons are gathered around a single chip, clearly mid-negotiation.", Difficulty.Easy),
        Scene("A dog has somehow climbed onto the roof of a shed and refuses to acknowledge that anything is wrong.", Difficulty.Easy),
        Scene("A toddler has dressed the family dog in a wedding veil and is conducting a ceremony.", Difficulty.Medium),
        Scene("A businessman is sprinting through an airport while a single shoe trails ten metres behind him.", Difficulty.Medium),
        Scene("A goat has escaped onto a golf course and appears to be lining up a putt.", Difficulty.Medium),
        Scene("Someone has built an enormous, elaborate sandcastle and a single seagull is standing on top like a king.", Difficulty.Medium),
        Scene("A robot vacuum has cornered the family cat and the two of them are locked in a tense standoff.", Difficulty.Hard),
        Scene("A elderly woman is having an intense argument with a self-checkout machine while everyone watches.", Difficulty.Easy),
        Scene("A squirrel is standing on its hind legs, holding a sandwich, staring directly into a security camera.", Difficulty.Easy),
        Scene("A person has somehow gotten their head stuck in a vending machine and is dangling from it.", Difficulty.Medium),
        Scene("A couple of ducks are waddling through a fast-food drive-thru queue like they own the place.", Difficulty.Easy),
        Scene("A man is attempting to parallel park a minivan, has been at it for five minutes, and is clearly losing his mind.", Difficulty.Medium),
        Scene("A child has built a blanket fort so elaborate it now has a moat.", Difficulty.Easy),
        Scene("A library has run out of bookshelf space and someone is stacking books on the floor in increasingly creative ways.", Difficulty.Medium),
        Scene("A corgi has stolen a baguette and is sprinting away from its owner at full speed, baguette held high.", Difficulty.Easy),
        Scene("A person has dressed their houseplant in tiny clothes and is taking it to brunch.", Difficulty.Medium),
        Scene("A queue of people is standing outside a shop at 5am on a freezing morning waiting for the sales to start.", Difficulty.Easy),

        // ── HEADLINES ────────────────────────────────────────────────────────
        Headline("Local man wins argument with self-checkout machine; experts baffled.", Difficulty.Easy),
        Headline("Squirrel reportedly 'knows what it did' after week-long campaign against bird feeder.", Difficulty.Easy),
        Headline("Town's only roundabout achieves sentience; demands respect.", Difficulty.Medium),
        Headline("Scientists confirm the last biscuit always tastes better; nation unsurprised.", Difficulty.Medium),
        Headline("Cat elected mayor of small village 'by accident', refuses to step down.", Difficulty.Medium),
        Headline("Man who said 'I'll just have one' returns home at 4am with a kebab and a new friend named Dave.", Difficulty.Hard),
        Headline("Local family's GPS develops opinions, begins offering life advice.", Difficulty.Hard),
        Headline("Penguin spotted queuing for bus; local transport authority very confused.", Difficulty.Easy),
        Headline("Dog discovers ability to climb onto roof; immediately regrets all decisions.", Difficulty.Easy),
        Headline("Seagull crowns itself king of sandcastle; refuses to surrender.", Difficulty.Medium),
        Headline("Robot vacuum declares war on family feline; standoff enters hour three.", Difficulty.Medium),
        Headline("Ducks stage successful infiltration of drive-thru; nuggets acquired.", Difficulty.Easy),
        Headline("Corgi's baguette heist sparks international manhunt.", Difficulty.Easy),
        Headline("Houseplant's brunch outing leaves restaurant staff speechless.", Difficulty.Medium),
        Headline("Queue of bargain hunters forms 24 hours early; camping chairs spotted.", Difficulty.Easy),

        // ── OVERHEARD ────────────────────────────────────────────────────────
        Overheard("...what someone says the moment before everything goes catastrophically wrong at a barbecue.", Difficulty.Easy),
        Overheard("...what a dog would say if it could speak, the first time it sees snow.", Difficulty.Easy),
        Overheard("...what the last person on Earth says when they hear a knock at the door.", Difficulty.Medium),
        Overheard("...what your phone's autocorrect would say if it finally snapped.", Difficulty.Medium),
        Overheard("...what a houseplant is thinking as you walk past it for the fifth day without watering it.", Difficulty.Medium),
        Overheard("...what the pilot says over the intercom that makes everyone immediately put down their drinks.", Difficulty.Hard),
        Overheard("...what a satnav says after you ignore its directions for the eleventh time.", Difficulty.Hard),
        Overheard("...what a penguin mutters while standing in the rain at a bus stop.", Difficulty.Easy),
        Overheard("...what a cat thinks while plotting your demise from the top of the wardrobe.", Difficulty.Easy),
        Overheard("...what a medieval knight whispers to themselves while staring at a flat white.", Difficulty.Medium),
        Overheard("...what a seagull screams victoriously from the top of a sandcastle.", Difficulty.Easy),
        Overheard("...what a dog yells from the shed roof when it realizes the error of its ways.", Difficulty.Medium),
        Overheard("...what a squirrel says after stealing your entire lunch bag.", Difficulty.Easy),
        Overheard("...what a corgi thinks while sprinting away with its baguette prize.", Difficulty.Easy),
        Overheard("...what someone says after their head gets stuck in a vending machine.", Difficulty.Medium),
    ];

    private static ICard Scene(string text, Difficulty d) =>
        StandardCard.Create(
            "Scene",
            "<b>Caption this scene:</b>\n\n" + text +
            "\n\nEveryone has fifteen seconds. Best caption wins the round.",
            d, "Scene");

    private static ICard Headline(string text, Difficulty d) =>
        StandardCard.Create(
            "Headline",
            "<b>Write the tabloid headline:</b>\n\n" + text +
            "\n\nActually — that <i>is</i> the event. Now everyone writes a better, funnier headline for it.",
            d, "Headline");

    private static ICard Overheard(string text, Difficulty d) =>
        StandardCard.Create(
            "Overheard",
            "<b>Finish the moment — say out loud</b> " + text +
            "\n\nGo round the circle. Funniest line takes the point.",
            d, "Overheard");
}
