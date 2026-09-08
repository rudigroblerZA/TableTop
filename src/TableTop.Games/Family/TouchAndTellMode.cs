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

    private const string Deck = "Touch & Tell";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)
        // ── TEXTURE ───────────────────────────────────────────────────────────
            .Category(TextureCategory)
            .Card(TextureCategory, BodyT("Sandpaper", "Rough, grainy, harsh, abrasive texture"), Difficulty.Easy)
            .Card(TextureCategory, BodyT("Velvet fabric", "Smooth, soft, plush, luxurious feel"), Difficulty.Easy)
            .Card(TextureCategory, BodyT("Rope", "Braided, twisted, rough, fibrous, bumpy"), Difficulty.Easy)
            .Card(TextureCategory, BodyT("Bubble wrap", "Bumpy spheres under plastic, crackles (don't pop it)"), Difficulty.Easy)
            .Card(TextureCategory, BodyT("Silk scarf", "Smooth, slippery, flowing, delicate, cool"), Difficulty.Easy)

        // ── TEMPERATURE ──────────────────────────────────────────────────────
            .Category(TemperatureCategory)
            .Card(TemperatureCategory, BodyT("Ice cube (or cold object)", "Freezing cold, hard, smooth, slippery"), Difficulty.Easy)
            .Card(TemperatureCategory, BodyT("Warm water bottle (or heated object)", "Hot/warm, smooth, could be squishy"), Difficulty.Medium)
            .Card(TemperatureCategory, BodyT("Cold metal (spoon or key)", "Metal cold, smooth, hard, small, dense"), Difficulty.Medium)
            .Card(TemperatureCategory, BodyT("Hot plate or warm ceramic", "Hot, smooth, flat, ceramic texture"), Difficulty.Medium)

        // ── SHAPE ────────────────────────────────────────────────────────────
            .Category(ShapeCategory)
            .Card(ShapeCategory, BodyT("Ball (tennis ball, rubber ball, or similar)", "Spherical, bouncy or firm, textured surface"), Difficulty.Easy)
            .Card(ShapeCategory, BodyT("Cube or square box", "Hard edges, flat surfaces, geometric, smooth or textured"), Difficulty.Easy)
            .Card(ShapeCategory, BodyT("Spiral or coil", "Twisted, looped, continuous spiral pattern"), Difficulty.Medium)
            .Card(ShapeCategory, BodyT("Star shape", "Multiple points, flat, hard or soft"), Difficulty.Hard)
            .Card(ShapeCategory, BodyT("Hollow tube or pipe", "Cylindrical, hollow inside, can feel the emptiness"), Difficulty.Medium)

        // ── FOOD ──────────────────────────────────────────────────────────────
            .Category(FoodCategory)
            .Card(FoodCategory, BodyT("Orange (or citrus fruit)", "Bumpy/dimpled texture, round, squishy inside"), Difficulty.Easy)
            .Card(FoodCategory, BodyT("Banana", "Long, curved, slightly lumpy, soft peel"), Difficulty.Easy)
            .Card(FoodCategory, BodyT("Walnut or almond", "Small, hard, ridged, irregular shape"), Difficulty.Medium)
            .Card(FoodCategory, BodyT("Mushroom", "Soft cap, firm stem, organic shape, squishy"), Difficulty.Hard)
            .Card(FoodCategory, BodyT("Lettuce or cabbage leaf", "Crinkled, fragile, leafy, papery feel"), Difficulty.Medium)

        // ── WEIRD ────────────────────────────────────────────────────────────
            .Category(WeirdCategory)
            .Card(WeirdCategory, BodyT("A sponge", "Porous, full of holes, squishy, absorbent feel"), Difficulty.Easy)
            .Card(WeirdCategory, BodyT("Playdough or clay", "Squishy, moldable, smooth, slightly sticky"), Difficulty.Easy)
            .Card(WeirdCategory, BodyT("A feather", "Soft, light, fluffy, delicate, ticklish"), Difficulty.Medium)
            .Card(WeirdCategory, BodyT("Slime (or putty)", "Stretchy, squishy, slightly sticky, gooey"), Difficulty.Medium)
            .Card(WeirdCategory, BodyT("Cork stopper or cork board", "Light, bumpy, crumbly texture, squishy yet firm"), Difficulty.Hard)
            .Build();

    private static string BodyT(string objectName, string tactileClues) =>
        "<b>30-SECOND TACTILE CHALLENGE</b>\n\n" +
        "Object: " + objectName + "\n\n" +
        "Tactile description: " + tactileClues + "\n\n" +
        "<b>HOW TO PLAY:</b>\n" +
        "1. Game master: Blindfold the player\n" +
        "2. Place the object in their hands\n" +
        "3. Player has 30 seconds to guess what it is\n" +
        "4. They can ONLY use touch — no looking, no hints\n" +
        "5. Correct guess = 1 point. Wrong guess = group gets point\n\n" +
        "<b>Game master: Find the actual object and hand it to them!</b>";
}
