using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// The Family Atlas — the whole family spends the evening drawing one map of
/// its world, and keeps it.
///
/// <para>
/// The family-facing sibling of <see cref="Couples.CartographersMode"/>: same
/// mechanic — one shared sheet of paper, built up permanently across a fixed
/// sequence of stages, nothing erased — reused because it earns a second
/// audience rather than because it's easy to duplicate. Where the couples deck
/// addresses a pair, every card here speaks to whoever's at the table: two
/// parents, a parent and a kid, three generations. No card assumes a
/// headcount or a specific relationship, so the mode declares no
/// <see cref="Core.Abstractions.Game.TableShape"/> — same choice this
/// namespace's other family modes make.
/// </para>
///
/// <para>
/// <b>The five stages</b>, played in order — later stages name and extend
/// what earlier ones drew, so the sequence is pinned the same way the
/// couples deck's is:
/// </para>
///
/// <list type="bullet">
/// <item><b>Foundations</b> — the blank page. Coastline, compass, crest, and
///   the day this family started.</item>
/// <item><b>Wilds</b> — the terrain. Mountains climbed, rivers that keep
///   moving, the swamp of a hard year, the weather that blows in.</item>
/// <item><b>Home Turf</b> — where the family actually lives on the map.
///   Capital, roads worn thin, the lighthouse that keeps things off the
///   rocks.</item>
/// <item><b>Legend</b> — naming everything, and the family motto at the
///   bottom edge.</item>
/// <item><b>Beyond the Map</b> — the edges. What's unexplored, and the one
///   place the family wants to reach together next year. Pinned last on
///   purpose.</item>
/// </list>
///
/// <para>
/// <b>No points</b>, for the same reason as the couples deck — the map is
/// the score, and a number beside it would be worse than nothing.
/// </para>
/// </summary>
public sealed class FamilyAtlasMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "The Family Atlas";

    /// <inheritdoc />
    public override string Description =>
        "Draw one map of your family's world together — mountains you've crossed, the home you keep coming back to, and the places you haven't been yet. Bring paper. Keep the map.";

    /// <summary>Label for the button that records a feature added to the map.</summary>
    public override string CompleteLabel => "Drawn";

    /// <summary>
    /// Not "Skip" — on a map, a blank is a real answer, same reasoning as the
    /// couples deck this mechanic is borrowed from.
    /// </summary>
    public override string SkipLabel => "Leave It Blank";

    /// <summary>
    /// The stages must run in order: nothing can be named before it's drawn,
    /// and "Beyond the Map" only means anything once the rest of the map
    /// exists to have edges.
    /// </summary>
    public override IReadOnlyList<string> CategoriesPinnedToStart => ["Foundations"];

    /// <inheritdoc cref="CategoriesPinnedToStart" />
    public override IReadOnlyList<string> CategoriesPinnedToEnd => ["Beyond the Map"];

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Foundations"]   = "#26A69A",
            ["Wilds"]         = "#7CB342",
            ["Home Turf"]     = "#FFB300",
            ["Legend"]        = "#5C6BC0",
            ["Beyond the Map"] = "#AB47BC",
        };

    /// <summary>The map is the prize; a score next to it would cheapen it.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <inheritdoc />
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        FamilyAtlasCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => FamilyAtlasCardBank.All;
}

/// <summary>Built-in card bank for The Family Atlas.</summary>
public static class FamilyAtlasCardBank
{
    /// <summary>All Family Atlas cards, ordered by stage.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── FOUNDATIONS — the blank page becomes a family's world ─────────────
        C("Foundations",
          "Draw the outline of your family's island. One shape — everyone touches the pen at some point while it's being drawn.",
          "Every family map starts with a coastline. Odd shapes are the good kind.",
          Difficulty.Easy),
        C("Foundations",
          "Draw a compass rose in a corner, and agree together what each direction means for this family — adventure, calm, home, chaos.",
          "Whatever you pick, it colours everything else you draw tonight.",
          Difficulty.Medium),
        C("Foundations",
          "Mark an X for exactly where this family started — a wedding, a birth, an adoption, a day someone moved in. Write the year beside it.",
          "Every map needs an origin. Yours has a date.",
          Difficulty.Easy),
        C("Foundations",
          "Draw the border with the outside world, and label what's on the other side — school, work, grandparents, the internet.",
          "Every family has neighbours it didn't choose.",
          Difficulty.Medium),
        C("Foundations",
          "Draw the family crest — one emblem for this whole family. If you can't agree what goes on it, vote.",
          "It does not have to be dignified. It has to be agreed.",
          Difficulty.Medium),
        C("Foundations",
          "Draw the scale bar. Decide together: one centimetre on this map equals how much time?",
          "This decides whether the map covers a year or a lifetime — and how much room is left for later.",
          Difficulty.Hard),

        // ── WILDS — the terrain the family actually crossed ───────────────────
        C("Wilds",
          "Draw the tallest mountain on the map — the hardest thing this family has climbed together. Don't name it yet.",
          "Naming comes later. It's easier once the shape is on the page.",
          Difficulty.Hard),
        C("Wilds",
          "Add a river. Rivers are the things that keep moving through family life whether anyone tends them or not.",
          "Chores, money, a long illness, a house move — draw where it runs and where it floods.",
          Difficulty.Hard),
        C("Wilds",
          "There's a swamp somewhere — a stretch of time that was just slow and hard to get through. Draw it an honest size.",
          "A too-small swamp on the map usually means somebody's being generous.",
          Difficulty.Hard),
        C("Wilds",
          "Draw the family forest — the private jokes and habits nobody outside this family would understand.",
          "You don't have to explain the forest to the map.",
          Difficulty.Medium),
        C("Wilds",
          "Which direction does the bad weather usually blow in from? Draw storm clouds there and write what the storms are usually about.",
          "Most families can answer this in under ten seconds. That's the point.",
          Difficulty.Hard),
        C("Wilds",
          "Add one beach, hot spring, or picnic spot — the place this family goes to recover. Draw it generously.",
          "If nobody could think of one, that's the most useful thing this map has said all night.",
          Difficulty.Medium),
        C("Wilds",
          "Draw a valley the family has been in together, and the path that led back out of it. Mark who found the path first.",
          "Somebody usually does. Put their initial at the trailhead.",
          Difficulty.Extreme),

        // ── HOME TURF — where the family actually lives on it ─────────────────
        C("Home Turf",
          "Draw the capital city — the thing this family is actually organised around.",
          "It isn't always the obvious answer. Sometimes it's a kitchen table, a dog, or Sunday nights.",
          Difficulty.Hard),
        C("Home Turf",
          "Add the road the family travels most. Draw it thick and worn between the two places it connects.",
          "Every family wears one route into a groove. This is now the official record of yours.",
          Difficulty.Medium),
        C("Home Turf",
          "Draw a small town nobody's visited in ages, but that's definitely still there — an old hobby, a school, a friend group.",
          "Put it on the map anyway. It still counts.",
          Difficulty.Medium),
        C("Home Turf",
          "Mark the lighthouse — the thing that keeps this family off the rocks when it's dark.",
          "It might be a person, a rule, or one sentence somebody always says.",
          Difficulty.Extreme),
        C("Home Turf",
          "Add a monument to something the family got through together. Sketch it and put the year on the base.",
          "Monuments are for things that are over. Make sure it's actually over before you build it.",
          Difficulty.Hard),
        C("Home Turf",
          "Somewhere on the map, draw the room you're all sitting in right now — to scale, if anyone's brave enough.",
          "The most local landmark on the whole map, and the one that will look funniest in ten years.",
          Difficulty.Easy),

        // ── LEGEND — naming everything, and the motto at the bottom ──────────
        C("Legend",
          "Go back to the tallest mountain. Name it now, in your best handwriting.",
          "You've been looking at it for half an hour. The name should be obvious by now.",
          Difficulty.Hard),
        C("Legend",
          "Name the whole family kingdom. One word or several. Write it across the top in the biggest letters that fit.",
          "This is the title of the thing you're making. Give it the extra thirty seconds.",
          Difficulty.Hard),
        C("Legend",
          "Every map needs a key. Draw a box in a corner and invent a symbol for 'this is where we laugh the most'. Mark it in three places.",
          "Symbols are free and the map is yours — invent freely.",
          Difficulty.Medium),
        C("Legend",
          "Name the river, then name the swamp. At least one name should make somebody laugh.",
          "Naming a hard thing out loud is half of how a family gets over it.",
          Difficulty.Hard),
        C("Legend",
          "Add a compass rose, and in place of N/S/E/W write four words for the four directions of family life.",
          "Four words the whole table has to agree on. The hardest small thing on this map.",
          Difficulty.Extreme),
        C("Legend",
          "Write the family motto along the bottom edge — the one sentence a stranger would need to read this family correctly.",
          "Family mottos are traditionally embarrassing. Yours may be too.",
          Difficulty.Extreme),
        C("Legend",
          "Everyone signs the map, youngest to oldest, and adds today's date.",
          "This is the moment it stops being a drawing and becomes a document.",
          Difficulty.Easy),

        // ── BEYOND THE MAP — the edges, and what comes next ───────────────────
        C("Beyond the Map",
          "Find an empty stretch of the map. Write 'HERE BE DRAGONS' across it, then say out loud what the dragon actually is.",
          "The writing is the joke. Saying it out loud is the actual card.",
          Difficulty.Extreme),
        C("Beyond the Map",
          "Mark one place on this map that only one person here has ever seen. Everyone else gets three questions about it.",
          "Three questions, answered honestly. Then draw whatever the answers suggest.",
          Difficulty.Extreme),
        C("Beyond the Map",
          "Draw a dotted line heading off the edge of the page — somewhere the family hasn't been yet, but might.",
          "Dotted, not solid. A dotted line commits nobody to anything, which is what makes it safe to draw.",
          Difficulty.Medium),
        C("Beyond the Map",
          "There's a region nobody has drawn anything in. Leave it blank on purpose, and outline where it starts and ends.",
          "A deliberate blank is the most honest thing a map can hold. Don't fill it in tonight.",
          Difficulty.Hard),
        C("Beyond the Map",
          "Pick a place this family wants to reach together within a year. Mark it with a star and write the date.",
          "The only card on this map that points forward with a deadline. Make it a real one.",
          Difficulty.Extreme),
        C("Beyond the Map",
          "Draw the edge of the page, and agree out loud what you'd want next year's map to add.",
          "Then put the map somewhere everyone will see it. The fridge is traditional. The map is finished; the family isn't.",
          Difficulty.Extreme),
    ];

    /// <summary>
    /// Builds one atlas card: the instruction, then the note underneath it —
    /// same two-part shape as <see cref="Couples.CartographersCardBank"/>,
    /// for the same reason: the instruction is what you do, the note is why
    /// it's worth doing, and several notes only land once the drawing exists.
    /// </summary>
    private static ICard C(string category, string instruction, string note, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>Add to the map:</b>\n\n" + instruction +
            "\n\n<i>" + note + "</i>",
            d, category);
}
