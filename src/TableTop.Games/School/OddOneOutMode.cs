using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Odd One Out — four things, one impostor, and the reason is the real answer.
///
/// How to play:
///   1. The reader shows the card: four items. On "three… two… one" everyone
///      POINTS at the impostor simultaneously.
///   2. Pointing right is worth guessing; explaining WHY is worth knowing.
///      Flip the card: the reveal names the impostor AND the rule.
///   3. Twist: many cards have a defensible second answer — arguing a clever
///      alternative rule that genuinely works earns the point too (table's
///      verdict). Knowledge wins, but so does wit.
///
/// Items are numbered 1–4 (not A–D) deliberately: these are point-at-it
/// cards, not tap-an-answer quiz cards.
/// </summary>
public sealed class OddOneOutMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name        => "Odd One Out";
    /// <inheritdoc />
    public override string Description =>
        "Four things, one impostor — everyone points on three, then flip for the rule. Clever alternative rules score too.";

    /// <summary>Label for the button that records a solved card.</summary>
    public override string CompleteLabel => "Got It";
    /// <summary>Label for the button that passes on a card.</summary>
    public override string SkipLabel     => "Stumped";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Animals"]  = "#66BB6A",
            ["Food"]     = "#FFA726",
            ["World"]    = "#42A5F5",
            ["Words"]    = "#AB47BC",
            ["Science"]  = "#26C6DA",
        };

    /// <summary>One point per solved impostor.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Returns the built-in odd-one-out card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        OddOneOutCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => OddOneOutCardBank.All;
}

/// <summary>Built-in card bank for Odd One Out.</summary>
public static class OddOneOutCardBank
{
    /// <summary>All odd-one-out cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── ANIMALS ──────────────────────────────────────────────────────────
        O("Animals", "Bat", "Ostrich", "Penguin", "Flying squirrel",
          "Bat — the only one that truly flies. (Flying squirrels glide; the birds gave up.)", Difficulty.Easy),
        O("Animals", "Dolphin", "Shark", "Whale", "Seal",
          "Shark — the only fish. The other three are air-breathing mammals.", Difficulty.Easy),
        O("Animals", "Octopus", "Squid", "Jellyfish", "Cuttlefish",
          "Jellyfish — no brain at all. The other three are famously clever cephalopods.", Difficulty.Medium),
        O("Animals", "Kangaroo", "Koala", "Wombat", "Sloth",
          "Sloth — the only non-marsupial (and the only one not from Australia).", Difficulty.Medium),
        O("Animals", "Bee", "Ant", "Termite", "Spider",
          "Spider — eight legs, not an insect. Bonus rule accepted: termite (the only one not in a Pixar film).", Difficulty.Easy),
        O("Animals", "Horse", "Zebra", "Donkey", "Camel",
          "Camel — the only non-equine. Alternative rule with merit: zebra, the only one never domesticated.", Difficulty.Hard),

        // ── FOOD ─────────────────────────────────────────────────────────────
        O("Food", "Tomato", "Cucumber", "Pumpkin", "Potato",
          "Potato — the only true vegetable here; botanically the rest are fruits.", Difficulty.Medium),
        O("Food", "Cashew", "Peanut", "Almond", "Walnut",
          "Peanut — a legume, not a nut. It grows underground and lies about it.", Difficulty.Medium),
        O("Food", "Croissant", "Baguette", "Pretzel", "Brioche",
          "Pretzel — the only one that isn't French. Germany would like a word.", Difficulty.Easy),
        O("Food", "Honey", "Milk", "Bread", "Cheese",
          "Honey — essentially never spoils. Archaeologists have eaten 3,000-year-old honey. On purpose.", Difficulty.Hard),
        O("Food", "Wasabi", "Chili", "Mustard", "Black pepper",
          "Chili — its heat (capsaicin) burns slow and stays; the others deliver a fast nose-hit that fades. Also the only New World native.", Difficulty.Extreme),
        O("Food", "Rice", "Wheat", "Corn", "Quinoa",
          "Quinoa — not a grass. It's a seed pretending, botanically closer to spinach.", Difficulty.Hard),

        // ── WORLD ────────────────────────────────────────────────────────────
        O("World", "Brazil", "Mexico", "Egypt", "Sudan",
          "Mexico — the only one without pyramids… is wrong, it HAS them. Real answer: Brazil — the only one with no pyramids. Gotcha rule: read carefully.", Difficulty.Hard),
        O("World", "Venice", "Amsterdam", "Bangkok", "Madrid",
          "Madrid — no canals. The other three all claim the title 'Venice of…' something.", Difficulty.Easy),
        O("World", "Japan", "New Zealand", "Iceland", "Mongolia",
          "Mongolia — the only landlocked one; the rest are island nations.", Difficulty.Easy),
        O("World", "Russia", "Canada", "China", "Australia",
          "Australia — the only one without a land border. Also accepted: China, the only one not among the top three largest by area… which is false. Stick with Australia.", Difficulty.Medium),
        O("World", "Nile", "Amazon", "Danube", "Mississippi",
          "Danube — the only one that isn't its continent's longest river.", Difficulty.Hard),
        O("World", "Rome", "Athens", "Cairo", "Sydney",
          "Sydney — the only one that isn't a capital. (Yes, really. It's Canberra.)", Difficulty.Medium),

        // ── WORDS ────────────────────────────────────────────────────────────
        O("Words", "Level", "Radar", "Kayak", "Table",
          "Table — the only word that isn't a palindrome.", Difficulty.Easy),
        O("Words", "Month", "Orange", "Silver", "Purple",
          "Month — the others are the classic 'nothing rhymes with me' words… and month belongs with them. Real rule: silver — the only one that's also a chemical element. Argue well.", Difficulty.Extreme),
        O("Words", "Whisper", "Buzz", "Sizzle", "Shout",
          "Shout — the only one that isn't onomatopoeia; it describes volume, not sound.", Difficulty.Medium),
        O("Words", "Bookkeeper", "Committee", "Balloon", "Assess",
          "Balloon — the only one without three consecutive double letters or trick spelling… rule: bookkeeper has THREE doubles in a row. Impostor: assess — the only one you can't say at a meeting without smiling.", Difficulty.Hard),
        O("Words", "Quiz", "Jazz", "Fizz", "Quit",
          "Quit — the only one worth few points in word games; the others carry the big-scoring letters Z and J. House rule: any Scrabble player may overrule.", Difficulty.Medium),

        // ── SCIENCE ──────────────────────────────────────────────────────────
        O("Science", "Mercury", "Venus", "Mars", "Neptune",
          "Neptune — the only one you can never see with the naked eye.", Difficulty.Medium),
        O("Science", "Gold", "Silver", "Bronze", "Copper",
          "Bronze — the only one that isn't an element; it's an alloy crashing the medal party.", Difficulty.Easy),
        O("Science", "Lightning", "Rainbow", "Aurora", "Thunder",
          "Thunder — the only one you hear instead of see.", Difficulty.Easy),
        O("Science", "Heart", "Liver", "Skin", "Kidney",
          "Skin — the only organ on the outside, and the body's largest. Most people forget it's an organ at all.", Difficulty.Medium),
        O("Science", "Virus", "Bacterium", "Fungus", "Algae",
          "Virus — the only one that isn't alive (by most definitions). It can't do anything without borrowing your cells.", Difficulty.Hard),
        O("Science", "Diamond", "Graphite", "Charcoal", "Quartz",
          "Quartz — the only one that isn't carbon. The other three are the same element in wildly different moods.", Difficulty.Hard),
    ];

    private static ICard O(string category, string i1, string i2, string i3, string i4,
        string reveal, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>🔍 One of these is not like the others. Everyone POINTS on three:</b>\n\n" +
            "1. " + i1 + "\n2. " + i2 + "\n3. " + i3 + "\n4. " + i4 + "\n\n" +
            "<i>Right finger = a point. A defensible alternative rule = also a point. Table decides.</i>\n\n" +
            "Answer: " + reveal,
            d, category);
}
