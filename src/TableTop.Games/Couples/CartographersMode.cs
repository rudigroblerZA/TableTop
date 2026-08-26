using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// The Cartographers — you spend the evening drawing one map of your
/// relationship as if it were a country, and you keep it.
///
/// <para>
/// <b>The mechanic, and why it isn't any of the other twenty-two.</b> Every
/// other couples deck here produces conversation and then evaporates: the
/// answers are said aloud and gone. This one is <i>constructive</i>. You need
/// one large sheet of paper and something to draw with, and every card
/// instructs you to add a permanent feature to a single shared map. Nothing is
/// erased. By the last card the sheet is a physical object that did not exist
/// at the start of the evening, and it goes on the fridge.
/// </para>
///
/// <para>
/// The second half of the mechanic is that the map <b>accumulates</b>. Early
/// cards place terrain; later cards explicitly operate on whatever you already
/// drew — name that mountain, chart the route between two things you placed an
/// hour ago, mark where the border has moved since. A card late in the deck is
/// unanswerable without the specific map the two of you happened to make,
/// which means no two sessions can produce the same object even from an
/// identical deck. Drawing ability is irrelevant and openly mocked by the card
/// text; stick figures and lumpy blobs are the intended aesthetic.
/// </para>
///
/// <para>
/// <b>The five ages</b>, played in order — the deck is pinned so the map is
/// built in the only sequence that works, because you cannot name a landmark
/// before placing one:
/// </para>
///
/// <list type="bullet">
/// <item><b>Survey</b> — the blank page. Coastline, borders, the shape of the
///   country. Where does it end, and what is outside it.</item>
/// <item><b>Terrain</b> — the features. Mountains you climbed, the swamp of
///   that one year, rivers, the weather that comes in from the west.</item>
/// <item><b>Settlement</b> — where you actually live on this map. Cities,
///   the capital, the roads worn between the places you go most.</item>
/// <item><b>Legend</b> — naming. Every feature gets a name, the map gets a
///   key, and the key is where the jokes live.</item>
/// <item><b>Terra Incognita</b> — the edges. What is unexplored, what the map
///   deliberately leaves blank, and the one place you both intend to go next.
///   This is the vulnerable tier and it is pinned last on purpose.</item>
/// </list>
///
/// <para>
/// <b>No points.</b> Scoring is zero throughout — the map is the score, and a
/// number next to it would be actively worse. "Drawn" advances; "Leave It
/// Blank" is a legitimate and permanent cartographic choice, not a skip, and
/// several cards say so.
/// </para>
/// </summary>
public sealed class CartographersMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <summary>Two people and one sheet of paper. Every card addresses a pair.</summary>
    public TableShape SuitableFor => TableShape.Couple;

    /// <inheritdoc />
    public override string Name        => "The Cartographers";

    /// <inheritdoc />
    public override string Description =>
        "Draw one map of your relationship as a country — terrain, cities, names, and the parts still unexplored. Bring paper. Keep the map.";

    /// <summary>Label for the button that records a feature added to the map.</summary>
    public override string CompleteLabel => "Drawn";

    /// <summary>
    /// Not "Skip" — on a map, a blank is a real answer, and the deck treats it
    /// as one.
    /// </summary>
    public override string SkipLabel     => "Leave It Blank";

    /// <summary>
    /// The ages must run in order: you cannot name a mountain you have not
    /// drawn, and Terra Incognita only means anything once the rest of the
    /// map exists to be edged.
    /// </summary>
    public override IReadOnlyList<string> CategoriesPinnedToStart => ["Survey"];

    /// <inheritdoc cref="CategoriesPinnedToStart" />
    public override IReadOnlyList<string> CategoriesPinnedToEnd => ["Terra Incognita"];

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Survey"]          = "#8D6E63",
            ["Terrain"]         = "#66BB6A",
            ["Settlement"]      = "#FFA726",
            ["Legend"]          = "#42A5F5",
            ["Terra Incognita"] = "#7E57C2",
        };

    /// <summary>The map is the prize; a score next to it would cheapen it.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <inheritdoc />
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        CartographersCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => CartographersCardBank.All;
}

/// <summary>Built-in card bank for The Cartographers.</summary>
public static class CartographersCardBank
{
    /// <summary>All cartographers cards, ordered by age.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── SURVEY — the blank page becomes a country ─────────────────────────
        C("Survey",
          "Draw the coastline. One continuous line, no lifting the pen, both hands on the pen at once.",
          "That wobbly shape is now the official border of your relationship. It is not negotiable and it is slightly ridiculous. Good.",
          Difficulty.Easy),
        C("Survey",
          "Mark NORTH with an arrow — but first agree what 'north' means here. Ambition? Calm? The future?",
          "Whatever you pick orients every other thing you draw tonight. Choose before you draw the arrow.",
          Difficulty.Medium),
        C("Survey",
          "Put a dot where the two of you are standing RIGHT NOW on this map. Just one dot, for both of you.",
          "If you argued about where the dot goes, that argument is more interesting than the dot. Draw it where you settled.",
          Difficulty.Medium),
        C("Survey",
          "Draw the border you share with the outside world — and label who or what is on the other side of it.",
          "Family, work, the group chat, the past. Everyone's map has neighbours.",
          Difficulty.Hard),
        C("Survey",
          "Somewhere on this map is the exact spot where it started. Mark it with an X and write the year beside it.",
          "Every map needs an origin. Yours has a date.",
          Difficulty.Easy),
        C("Survey",
          "Draw the scale bar. Decide together: one centimetre equals how much time?",
          "This determines whether your map is a week or a decade. It also determines how much room you left for later.",
          Difficulty.Medium),

        // ── TERRAIN — the features you actually crossed ───────────────────────
        C("Terrain",
          "Draw the highest mountain on the map. It is the hardest thing you have crossed together. Do not name it yet.",
          "Naming comes later, and it will be easier once the shape is on the page.",
          Difficulty.Hard),
        C("Terrain",
          "Add a river. Rivers are the things that keep moving through your relationship whether you tend them or not.",
          "Money, health, someone's family, the commute. Draw where it runs and where it floods.",
          Difficulty.Hard),
        C("Terrain",
          "Somewhere there is a swamp — a season that was just slow and unpleasant to walk through. Draw it. Give it an honest size.",
          "Understating the swamp is the most common cartographic error couples make.",
          Difficulty.Hard),
        C("Terrain",
          "Draw the forest: the part of your life together that is dense, private, and belongs to nobody else.",
          "You do not have to explain to the map what is in the forest.",
          Difficulty.Medium),
        C("Terrain",
          "Which direction does your weather come from? Draw the clouds at that edge, and write what the storms are usually about.",
          "Most couples can name their prevailing wind in under ten seconds. The speed of the answer is the point.",
          Difficulty.Hard),
        C("Terrain",
          "Add one hot spring, oasis, or beach — the reliable place you go to recover. Draw it generously.",
          "If you struggled to find one, that is the single most useful thing this map has told you tonight.",
          Difficulty.Medium),
        C("Terrain",
          "There is a bridge somewhere on this map. Draw it, and mark whether it is currently in good repair.",
          "Bridges connect two things that would otherwise be separate. You both know which two.",
          Difficulty.Hard),
        C("Terrain",
          "Draw a valley — somewhere low you have both been, and come up out of.",
          "Mark the path out of it. Someone found that path first; put their initial at the trailhead.",
          Difficulty.Extreme),

        // ── SETTLEMENT — where you live on it ────────────────────────────────
        C("Settlement",
          "Draw your capital city. It is the thing your relationship is actually organised around.",
          "It is not always romance. Sometimes it's a kid, a business, a dog, or Tuesday nights.",
          Difficulty.Hard),
        C("Settlement",
          "Add the road you travel most. Draw it thick and worn between the two places it connects.",
          "Every couple has one route they wear a groove into. Yours is now permanent record.",
          Difficulty.Medium),
        C("Settlement",
          "Draw a small town neither of you has visited in a long time, but which is definitely still there.",
          "An old hobby, a set of friends, a version of one of you. Put it on the map anyway. It exists.",
          Difficulty.Hard),
        C("Settlement",
          "Mark the lighthouse: the thing that keeps you off the rocks when it is dark.",
          "It might be a person, a rule you keep, or one specific sentence one of you says.",
          Difficulty.Extreme),
        C("Settlement",
          "Add a monument to something you survived. Sketch it and put the year on the plinth.",
          "Monuments are for the things that were hard and are over. Make sure it's over before you build it.",
          Difficulty.Hard),
        C("Settlement",
          "Draw the harbour — where new things arrive into your life from outside.",
          "Then mark whether it is currently busy or quiet. Both are legitimate seasons.",
          Difficulty.Medium),
        C("Settlement",
          "Somewhere on this map, draw the room you are sitting in right now. To scale, if you dare.",
          "The most local possible landmark. It will be the most dated part of the map in ten years, which is exactly why it goes on.",
          Difficulty.Easy),

        // ── LEGEND — naming, and the key at the bottom ───────────────────────
        C("Legend",
          "Go back to your highest mountain. Name it now. Write the name on the map in your best handwriting.",
          "You have been looking at it for half an hour. The name should be obvious by now — and if it's a joke, even better.",
          Difficulty.Hard),
        C("Legend",
          "Name your country. One word or several. Write it across the top in the largest letters that will fit.",
          "This is the title of the object you are making. Take the extra thirty seconds.",
          Difficulty.Hard),
        C("Legend",
          "Every map has a key. Draw a box in a corner and invent a symbol for 'here we laugh a lot'. Mark three places with it.",
          "Symbols are cheap and the map is yours. Invent freely.",
          Difficulty.Medium),
        C("Legend",
          "Invent a symbol for 'handle with care' and place it honestly — on at least one spot, at most three.",
          "Placing this one takes negotiation. That negotiation is the card.",
          Difficulty.Extreme),
        C("Legend",
          "Name the river. Then name the swamp. The swamp name should make at least one of you laugh.",
          "Naming a hard thing is how you stop being afraid of it, and cartography has known this for centuries.",
          Difficulty.Hard),
        C("Legend",
          "Add a compass rose, and in place of N/S/E/W write four words that describe the four directions of your life together.",
          "Four words, agreed by both. This is the hardest small thing on the card list.",
          Difficulty.Extreme),
        C("Legend",
          "Write the map's motto along the bottom edge — the sentence a stranger would need in order to read this country correctly.",
          "Mottos are traditionally in Latin. Yours may be in whatever you actually say to each other.",
          Difficulty.Extreme),
        C("Legend",
          "Sign it. Both of you. Corner of your choosing, and add today's date.",
          "This is the moment it becomes a document rather than a drawing.",
          Difficulty.Easy),

        // ── TERRA INCOGNITA — the edges, and what comes next ─────────────────
        C("Terra Incognita",
          "Find an empty area of the map. Write 'HERE BE DRAGONS' across it, and then say out loud what the dragon actually is.",
          "The writing is a joke. The saying-out-loud is not, and it is the reason this card is in the deck.",
          Difficulty.Extreme),
        C("Terra Incognita",
          "Mark one place on this map that only ONE of you has ever been. Let the other ask three questions about it.",
          "Three questions, honestly answered. Then draw whatever the answers suggest.",
          Difficulty.Extreme),
        C("Terra Incognita",
          "Draw a dotted line heading off the edge of the paper — a route you have not taken yet but might.",
          "Dotted, not solid. Nobody is committing to anything by drawing a dotted line, which is what makes it safe to draw.",
          Difficulty.Hard),
        C("Terra Incognita",
          "There is a region on this map neither of you has drawn anything in. Leave it blank on purpose, and outline it.",
          "A deliberate blank is the most honest thing a map can contain. Do not fill it in tonight.",
          Difficulty.Hard),
        C("Terra Incognita",
          "Pick a spot you both want to reach within a year. Mark it with a star and write the date you'd like to arrive.",
          "This is the only card on the map that points forward with a deadline. Choose something real.",
          Difficulty.Extreme),
        C("Terra Incognita",
          "Draw the edge of the map — and agree out loud what you'd want this map to look like the next time you draw one.",
          "Then put it somewhere you will both see it. The fridge is traditional. The map is finished; the country is not.",
          Difficulty.Extreme),
    ];

    /// <summary>
    /// Builds one cartography card: the instruction, then the note underneath
    /// it. The note is separated deliberately — the instruction is what you do,
    /// the note is why it is worth doing, and several notes only make sense
    /// after the drawing has started.
    /// </summary>
    private static ICard C(string category, string instruction, string note, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>Add to the map:</b>\n\n" + instruction +
            "\n\n<i>" + note + "</i>",
            d, category);
}
