using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Touch &amp; Tell — blindfolded tactile guessing. Feel it, guess it.
///
/// How to play:
///   1. One player is blindfolded.
///   2. Game master hands them an object matching the card description.
///   3. They have 30 seconds to guess what it is using ONLY touch.
///   4. If they guess correctly, they get the point.
///   5. If they guess wrong, the group gets the point.
///
/// Objects can be anything: textured, smooth, squishy, hard, warm, cold, shaped oddly.
/// The challenge is not knowing what it is when you can't see. A orange? A ball? A potato?
/// Something worse? Chaos ensues.
///
/// Great for parties, family game nights, and getting people out of their comfort zones.
/// Physical, tactile, and genuinely challenging. Works for all ages. Guaranteed laughter
/// when someone guesses a banana is a shoe.
///
/// NOTE: Game master will need to source tactile objects. Suggestions: fruit, household items,
/// textures, plushies, ice cubes, warm objects, sandpaper, velvet, etc.
/// </summary>
public sealed class TouchAndTellMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Touch & Tell";
    /// <inheritdoc />
    public override string Description =>
        "Blindfolded. Feel the object. 30 seconds to guess what it is.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Guessed";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [TouchAndTellCardBank.TextureCategory] = "#42A5F5",
            [TouchAndTellCardBank.TemperatureCategory] = "#EF5350",
            [TouchAndTellCardBank.ShapeCategory] = "#FFCA28",
            [TouchAndTellCardBank.FoodCategory] = "#66BB6A",
            [TouchAndTellCardBank.WeirdCategory] = "#AB47BC",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        TouchAndTellCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => TouchAndTellCardBank.All;
}

/// <summary>Built-in card bank for Touch &amp; Tell. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class TouchAndTellCardBank
{
    internal const string TextureCategory = "Texture";
    internal const string TemperatureCategory = "Temperature";
    internal const string ShapeCategory = "Shape";
    internal const string FoodCategory = "Food";
    internal const string WeirdCategory = "Weird";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── TEXTURE ───────────────────────────────────────────────────────────
        T(TextureCategory,
            "Sandpaper",
            "Rough, grainy, harsh, abrasive texture",
            Difficulty.Easy),
        T(TextureCategory,
            "Velvet fabric",
            "Smooth, soft, plush, luxurious feel",
            Difficulty.Easy),
        T(TextureCategory,
            "Rope",
            "Braided, twisted, rough, fibrous, bumpy",
            Difficulty.Easy),
        T(TextureCategory,
            "Bubble wrap",
            "Bumpy spheres under plastic, crackles (don't pop it)",
            Difficulty.Easy),
        T(TextureCategory,
            "Silk scarf",
            "Smooth, slippery, flowing, delicate, cool",
            Difficulty.Easy),

        // ── TEMPERATURE ──────────────────────────────────────────────────────
        T(TemperatureCategory,
            "Ice cube (or cold object)",
            "Freezing cold, hard, smooth, slippery",
            Difficulty.Easy),
        T(TemperatureCategory,
            "Warm water bottle (or heated object)",
            "Hot/warm, smooth, could be squishy",
            Difficulty.Medium),
        T(TemperatureCategory,
            "Cold metal (spoon or key)",
            "Metal cold, smooth, hard, small, dense",
            Difficulty.Medium),
        T(TemperatureCategory,
            "Hot plate or warm ceramic",
            "Hot, smooth, flat, ceramic texture",
            Difficulty.Medium),

        // ── SHAPE ────────────────────────────────────────────────────────────
        T(ShapeCategory,
            "Ball (tennis ball, rubber ball, or similar)",
            "Spherical, bouncy or firm, textured surface",
            Difficulty.Easy),
        T(ShapeCategory,
            "Cube or square box",
            "Hard edges, flat surfaces, geometric, smooth or textured",
            Difficulty.Easy),
        T(ShapeCategory,
            "Spiral or coil",
            "Twisted, looped, continuous spiral pattern",
            Difficulty.Medium),
        T(ShapeCategory,
            "Star shape",
            "Multiple points, flat, hard or soft",
            Difficulty.Hard),
        T(ShapeCategory,
            "Hollow tube or pipe",
            "Cylindrical, hollow inside, can feel the emptiness",
            Difficulty.Medium),

        // ── FOOD ──────────────────────────────────────────────────────────────
        T(FoodCategory,
            "Orange (or citrus fruit)",
            "Bumpy/dimpled texture, round, squishy inside",
            Difficulty.Easy),
        T(FoodCategory,
            "Banana",
            "Long, curved, slightly lumpy, soft peel",
            Difficulty.Easy),
        T(FoodCategory,
            "Walnut or almond",
            "Small, hard, ridged, irregular shape",
            Difficulty.Medium),
        T(FoodCategory,
            "Mushroom",
            "Soft cap, firm stem, organic shape, squishy",
            Difficulty.Hard),
        T(FoodCategory,
            "Lettuce or cabbage leaf",
            "Crinkled, fragile, leafy, papery feel",
            Difficulty.Medium),

        // ── WEIRD ────────────────────────────────────────────────────────────
        T(WeirdCategory,
            "A sponge",
            "Porous, full of holes, squishy, absorbent feel",
            Difficulty.Easy),
        T(WeirdCategory,
            "Playdough or clay",
            "Squishy, moldable, smooth, slightly sticky",
            Difficulty.Easy),
        T(WeirdCategory,
            "A feather",
            "Soft, light, fluffy, delicate, ticklish",
            Difficulty.Medium),
        T(WeirdCategory,
            "Slime (or putty)",
            "Stretchy, squishy, slightly sticky, gooey",
            Difficulty.Medium),
        T(WeirdCategory,
            "Cork stopper or cork board",
            "Light, bumpy, crumbly texture, squishy yet firm",
            Difficulty.Hard),
    ];

    private static ICard T(string category, string objectName, string tactileClues, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>30-SECOND TACTILE CHALLENGE</b>\n\n" +
            "Object: " + objectName + "\n\n" +
            "Tactile description: " + tactileClues + "\n\n" +
            "<b>HOW TO PLAY:</b>\n" +
            "1. Game master: Blindfold the player\n" +
            "2. Place the object in their hands\n" +
            "3. Player has 30 seconds to guess what it is\n" +
            "4. They can ONLY use touch — no looking, no hints\n" +
            "5. Correct guess = 1 point. Wrong guess = group gets point\n\n" +
            "<b>Game master: Find the actual object and hand it to them!</b>",
            d, category);
}
