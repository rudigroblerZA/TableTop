using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Abstractions.Restrictions;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Family Dares — physical and performative dares for all ages.
///
/// Designed so children and adults do exactly the same dares together.
/// No adult content. Everything is physical, silly, performative, or mildly
/// embarrassing in a family-friendly way.
///
/// Four tiers:
///   Easy    — short, simple, anyone can do these (ages 4+)
///   Medium  — more involved, requires concentration or performance
///   Hard    — genuinely challenging physical or performance dares
///   Extreme — the ones that end up on family video calls
///
/// A group may negotiate or modify any dare — the spirit of the dare
/// is more important than the letter.
/// </summary>
public sealed class FamilyDaresMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Family Dares";
    /// <inheritdoc />
    public override string Description =>
        "Silly, physical, performative dares for all ages. No winners — just chaos.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ Did it!";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel     => "~ Can't do it";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Physical"]     = "#66BB6A",
            ["Performance"]  = "#FFCA28",
            ["Memory"]       = "#42A5F5",
            ["Teamwork"]     = "#26C6DA",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        FamilyDaresCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => FamilyDaresCardBank.All;
}

/// <summary>Built-in card bank for FamilyDares. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class FamilyDaresCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── EASY ─────────────────────────────────────────────────────────────

        D("Funny Walk",
          "Walk from one wall to the other with the silliest walk you can invent. No repeating how you walked last time.",
          "Physical", Difficulty.Easy),

        D("Animal Impression",
          "Do your best impression of an animal chosen by the person on your left. Hold it for ten seconds.",
          "Performance", Difficulty.Easy),

        D("Frozen",
          "Freeze in the most dramatic pose you can think of and hold it for fifteen seconds — no moving, no laughing.",
          "Physical", Difficulty.Easy),

        D("Tongue Twister",
          "Say this three times fast: <b>\"She sells seashells by the seashore.\"</b>\n\nNo mistakes allowed — the group decides.",
          "Memory", Difficulty.Easy),

        D("The Robot",
          "Walk across the room as a robot for thirty seconds. Narrate your own movement in robot voice.",
          "Performance", Difficulty.Easy),

        D("Backward Alphabet",
          "Say the alphabet backwards. As far as you can get in twenty seconds counts.",
          "Memory", Difficulty.Easy),

        D("The Slow-Motion Action Hero",
          "Re-enact an action hero diving away from an explosion — entirely in slow motion.",
          "Performance", Difficulty.Easy),

        D("Balance",
          "Stand on one leg for thirty seconds. You can choose which leg. No leaning on walls.",
          "Physical", Difficulty.Easy),

        D("Impression of Someone in the Room",
          "Do your best impression of someone else in the room — their voice, their phrases, the way they move. They get to rate it out of 10.",
          "Performance", Difficulty.Easy),

        D("Compliment the Object",
          "Pick up the nearest object on the table and spend thirty seconds convincing the group it is the most beautiful thing they have ever seen.",
          "Performance", Difficulty.Easy),

        D("Count to Thirty",
          "Count from 1 to 30, replacing every multiple of 3 with 'fizz'. No pauses, no mistakes.",
          "Memory", Difficulty.Easy),

        D("The Statue",
          "Strike a pose as a famous statue or monument — the group has to guess which one.",
          "Performance", Difficulty.Easy),

        // ── MEDIUM ────────────────────────────────────────────────────────────

        D("Seven Items",
          "Name seven items in any one category (animals, countries, vegetables) in ten seconds. The group picks the category.",
          "Memory", Difficulty.Medium),

        D("Silent Film",
          "Act out a scene — chosen by the group — entirely in mime. No sounds whatsoever. Two minutes.",
          "Performance", Difficulty.Medium),

        D("The Weather Forecast",
          "Deliver a completely made-up weather forecast for a fictional place (the group names it) in your best news-presenter voice.",
          "Performance", Difficulty.Medium),

        D("Backwards Sentence",
          "Someone in the group says a sentence. You must repeat it with every word in reverse order.",
          "Memory", Difficulty.Medium),

        D("Keep a Straight Face",
          "For thirty seconds, sit completely still with a straight face while everyone else tries to make you laugh. Any smile and you fail.",
          "Physical", Difficulty.Medium),

        D("News Reader",
          "Read the last text message you received aloud — in the voice of a serious news anchor breaking an urgent story.",
          "Performance", Difficulty.Medium),

        D("Twenty Questions Object",
          "You are secretly one object in the room. The group asks yes/no questions. They have ten guesses.",
          "Memory", Difficulty.Medium),

        D("Air Orchestra",
          "Conduct an entire invisible orchestra performing Beethoven's Fifth in full. Thirty seconds. Maximum commitment required.",
          "Performance", Difficulty.Medium),

        D("The Documentary",
          "Narrate what you are doing right now as if David Attenborough is filming you for a nature documentary. Sixty seconds.",
          "Performance", Difficulty.Medium),

        D("Name Chain",
          "Go around the room: first person names an animal. Each person names another animal beginning with the last letter of the previous one. Anyone who hesitates or repeats is out — last one standing wins.",
          "Memory", Difficulty.Medium),

        D("Backwards Compliment",
          "Give the person on your left a ten-second genuine compliment — but say every word backwards. They must try to decode it.",
          "Memory", Difficulty.Medium),

        D("Thirty Seconds of Fame",
          "You have thirty seconds to convince the group you are the most interesting person alive. Invent whatever biography you need.",
          "Performance", Difficulty.Medium),

        // ── HARD ─────────────────────────────────────────────────────────────

        D("Full Dramatic Monologue",
          "Deliver an entirely improvised dramatic monologue about a subject chosen by the group — minimum sixty seconds, maximum ham.",
          "Performance", Difficulty.Hard),

        D("Translate the Gibberish",
          "Someone speaks an invented foreign language for thirty seconds (genuine gibberish). You must translate it — completely confidently — into a real speech.",
          "Performance", Difficulty.Hard),

        D("Ten Things Fast",
          "Name ten things in a category — the group picks a tricky one — in fifteen seconds. No hesitations.",
          "Memory", Difficulty.Hard),

        D("The Slow-Motion Sports Commentary",
          "Someone mimes a sporting action in slow motion. You provide live commentary — also in slow motion.",
          "Teamwork", Difficulty.Hard),

        D("Read Their Mind",
          "You and another player face away from the group. The group selects an emotion. You must have a full conversation for sixty seconds in which you both perform that emotion without knowing the other knows what it is.",
          "Teamwork", Difficulty.Hard),

        D("Theme Song",
          "Invent and perform a thirty-second theme song for another person in the room. It must include their name, one fact about them, and a chorus.",
          "Performance", Difficulty.Hard),

        D("Commercial Break",
          "You have sixty seconds to shoot and present a TV commercial for a product chosen by the group. Name it. Pitch it. Demonstrate it. Close the sale.",
          "Performance", Difficulty.Hard),

        D("Master Chef Presentation",
          "Take an ordinary object from the room and present it as a dish to a Michelin-star restaurant. Describe its flavours, textures, and provenance for forty-five seconds.",
          "Performance", Difficulty.Hard),

        D("Three-Person Story",
          "The group tells a story together — but you must narrate the whole thing without speaking a single word. Mime only. Other players provide the words based on what you're doing.",
          "Teamwork", Difficulty.Hard),

        D("Emotion Switch",
          "Start delivering a speech about any topic. Every ten seconds, someone calls out a new emotion — and you must instantly switch. Keep the speech going throughout.",
          "Performance", Difficulty.Hard),

        // ── EXTREME ───────────────────────────────────────────────────────────

        D("Entire Film in One Minute",
          "Choose any film the whole family has seen. You have sixty seconds to perform the entire plot — every major scene, every character, every twist — alone.",
          "Performance", Difficulty.Extreme),

        D("The Choir of One",
          "Perform 'Happy Birthday' as if you are an entire choir of four different voice parts simultaneously. All parts, all words, no accompaniment.",
          "Performance", Difficulty.Extreme),

        D("The Newscast",
          "Breaking news: you have ninety seconds to deliver a live TV news report about something completely ordinary that happened today. Make it sound like a global emergency.",
          "Performance", Difficulty.Extreme),

        D("Simultaneous Translation",
          "Someone tells a story for sixty seconds. You must translate it into a fictional language — simultaneously — with matching emotion and timing.",
          "Teamwork", Difficulty.Extreme),

        D("The Grand Tour",
          "You are an estate agent. Lead the group on a guided tour of the room you are currently in — as if it is a five-million-pound property. Ninety seconds. Point out every feature.",
          "Performance", Difficulty.Extreme),

        D("One-Minute Musical",
          "Create and perform a sixty-second musical about an event that happened today. Must include: an opening number, a plot twist, and a closing song.",
          "Performance", Difficulty.Extreme),
    ];

    private static ICard D(string title, string text, string category, Difficulty d) =>
        StandardCard.Create(title, text, d, category);
}