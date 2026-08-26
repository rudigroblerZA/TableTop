using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// One Word Wonder — a rapid-fire description and guessing game for school.
///
/// How to play:
///   1. Read the description aloud.
///   2. The active player says ONE WORD that describes it. Just one.
///   3. Everyone else tries to guess what the original thing was from that one word.
///   4. Points for guessing correctly, and for choosing a word that helped others guess.
///
/// Cards range from simple objects and animals (everyone knows what you mean) to complex
/// concepts and abstract ideas (one word forces real creativity). Great for vocabulary,
/// lateral thinking, and hilarious failures ("Wet? Is it a cloud? A fish? A joke?").
///
/// Works for Grade 6 and up. Builds vocabulary naturally and makes kids think about
/// word choice and meaning. Plus it's genuinely funny when someone says "Ow" for a cactus.
/// </summary>
public sealed class OneWordWonderMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name        => "One Word Wonder";
    /// <inheritdoc />
    public override string Description =>
        "Describe something in exactly one word. Everyone else guesses what it is. Hilarity ensues.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Guessed";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel     => "Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Animal"]      = "#66BB6A",
            ["Object"]      = "#42A5F5",
            ["Place"]       = "#FFA726",
            ["Concept"]     = "#AB47BC",
            ["Action"]      = "#EC407A",
            ["Emotion"]     = "#EF5350",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        OneWordWonderCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => OneWordWonderCardBank.All;
}

/// <summary>Built-in card bank for One Word Wonder. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class OneWordWonderCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── ANIMAL ────────────────────────────────────────────────────────────
        W("Animal", "A creature with a long neck that eats leaves from tall trees.", Difficulty.Easy),
        W("Animal", "A small furry pet that barks and wags its tail.", Difficulty.Easy),
        W("Animal", "A large aquatic mammal that jumps out of the water.", Difficulty.Easy),
        W("Animal", "A reptile that slithers on the ground and has no legs.", Difficulty.Easy),
        W("Animal", "A nocturnal creature that hangs upside down in caves.", Difficulty.Medium),
        W("Animal", "A bird of prey that hunts in the forest at night.", Difficulty.Hard),
        W("Animal", "A slow-moving creature that carries its home on its back.", Difficulty.Medium),
        W("Animal", "An insect with colourful wings that starts as a caterpillar.", Difficulty.Easy),
        W("Animal", "A creature that lives in Australia and looks like a bear.", Difficulty.Medium),
        W("Animal", "A bird that swims underwater to catch fish.", Difficulty.Medium),

        // ── OBJECT ────────────────────────────────────────────────────────────
        W("Object", "A device you use to see distant things more clearly.", Difficulty.Medium),
        W("Object", "A container you use to drink water or juice.", Difficulty.Easy),
        W("Object", "An object you use to write on paper.", Difficulty.Easy),
        W("Object", "A tool you use to hit a nail into wood.", Difficulty.Easy),
        W("Object", "A chair with wheels that you roll around in.", Difficulty.Easy),
        W("Object", "A device that keeps food cold inside your kitchen.", Difficulty.Easy),
        W("Object", "Something you sleep on at night with a pillow.", Difficulty.Easy),
        W("Object", "A flat surface you put things on to eat.", Difficulty.Medium),
        W("Object", "A device that plays music and videos for entertainment.", Difficulty.Easy),
        W("Object", "Something you wear on your feet when it rains.", Difficulty.Easy),

        // ── PLACE ────────────────────────────────────────────────────────────
        W("Place", "A large body of water surrounded by land.", Difficulty.Easy),
        W("Place", "A tall landform that reaches into the clouds.", Difficulty.Easy),
        W("Place", "A location where you can see many types of animals in natural habitats.", Difficulty.Easy),
        W("Place", "An establishment where you go to watch a film on a big screen.", Difficulty.Easy),
        W("Place", "A building where you go to borrow books.", Difficulty.Easy),
        W("Place", "A location at the beach where waves roll to shore.", Difficulty.Medium),
        W("Place", "An underground room where people are buried.", Difficulty.Hard),
        W("Place", "A location with tall buildings, busy streets, and lots of people.", Difficulty.Easy),
        W("Place", "A building dedicated to making you laugh with comedy shows.", Difficulty.Medium),
        W("Place", "An establishment where you go to buy medicine when you're sick.", Difficulty.Easy),

        // ── CONCEPT ──────────────────────────────────────────────────────────
        W("Concept", "The feeling you have when something bad happens.", Difficulty.Hard),
        W("Concept", "A state of mind when you don't know what to do.", Difficulty.Hard),
        W("Concept", "The quality of being kind and helpful to others.", Difficulty.Hard),
        W("Concept", "The ability to do something well from practice.", Difficulty.Hard),
        W("Concept", "A promise to do something in the future.", Difficulty.Medium),
        W("Concept", "The opposite of chaos — when everything is orderly.", Difficulty.Hard),
        W("Concept", "The freedom to make your own choices.", Difficulty.Hard),
        W("Concept", "A set of rules that a community agrees to follow.", Difficulty.Hard),
        W("Concept", "The quality of always telling the truth.", Difficulty.Hard),
        W("Concept", "A belief or idea that guides how you live.", Difficulty.Hard),

        // ── ACTION ────────────────────────────────────────────────────────────
        W("Action", "To move quickly on foot from one place to another.", Difficulty.Easy),
        W("Action", "To propel yourself forward through water using limbs.", Difficulty.Easy),
        W("Action", "To move up from a lower place to a higher place.", Difficulty.Easy),
        W("Action", "To rest your head and body after being awake all day.", Difficulty.Easy),
        W("Action", "To make a loud sound with your mouth and vocal cords.", Difficulty.Easy),
        W("Action", "To laugh really hard at something funny.", Difficulty.Easy),
        W("Action", "To hug someone to show affection.", Difficulty.Easy),
        W("Action", "To eat food quickly without much chewing.", Difficulty.Medium),
        W("Action", "To fall through the air from a height.", Difficulty.Medium),
        W("Action", "To move your body to music.", Difficulty.Easy),

        // ── EMOTION ──────────────────────────────────────────────────────────
        W("Emotion", "The feeling you have on your birthday.", Difficulty.Easy),
        W("Emotion", "What you feel when something terrible happens.", Difficulty.Easy),
        W("Emotion", "The emotion of finding something really hilarious.", Difficulty.Easy),
        W("Emotion", "How you feel when you've done something wrong.", Difficulty.Easy),
        W("Emotion", "The feeling when you see someone you love.", Difficulty.Easy),
        W("Emotion", "The state of mind when you're running late for something important.", Difficulty.Medium),
        W("Emotion", "What you feel when someone treats you unfairly.", Difficulty.Easy),
        W("Emotion", "The emotion of being let down by someone.", Difficulty.Hard),
        W("Emotion", "How you feel at the end of a perfect day.", Difficulty.Hard),
        W("Emotion", "The sensation when something unexpected happens.", Difficulty.Medium),
    ];

    private static ICard W(string category, string description, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>Describe this in ONE WORD:</b>\n\n" + description +
            "\n\n<b>Active player:</b> Say one word that describes it. That's it. Just one word.\n\n" +
            "<b>Everyone else:</b> Guess what the original thing is from that one word.",
            d, category);
}
