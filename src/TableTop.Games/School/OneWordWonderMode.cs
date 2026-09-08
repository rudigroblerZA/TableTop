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
    public override string Name => "One Word Wonder";
    /// <inheritdoc />
    public override string Description =>
        "Describe something in exactly one word. Everyone else guesses what it is. Hilarity ensues.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Guessed";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [OneWordWonderCardBank.AnimalCategory] = "#66BB6A",
            [OneWordWonderCardBank.ObjectCategory] = "#42A5F5",
            [OneWordWonderCardBank.PlaceCategory] = "#FFA726",
            [OneWordWonderCardBank.ConceptCategory] = "#AB47BC",
            [OneWordWonderCardBank.ActionCategory] = "#EC407A",
            [OneWordWonderCardBank.EmotionCategory] = "#EF5350",
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
    internal const string AnimalCategory = "Animal";
    internal const string ObjectCategory = "Object";
    internal const string PlaceCategory = "Place";
    internal const string ConceptCategory = "Concept";
    internal const string ActionCategory = "Action";
    internal const string EmotionCategory = "Emotion";

    private const string Deck = "One Word Wonder";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)

            // ── ANIMAL ────────────────────────────────────────────────────────────
            .Category(AnimalCategory)
            .Card(AnimalCategory, Body("A creature with a long neck that eats leaves from tall trees."), Difficulty.Easy)
            .Card(AnimalCategory, Body("A small furry pet that barks and wags its tail."), Difficulty.Easy)
            .Card(AnimalCategory, Body("A large aquatic mammal that jumps out of the water."), Difficulty.Easy)
            .Card(AnimalCategory, Body("A reptile that slithers on the ground and has no legs."), Difficulty.Easy)
            .Card(AnimalCategory, Body("A nocturnal creature that hangs upside down in caves."), Difficulty.Medium)
            .Card(AnimalCategory, Body("A bird of prey that hunts in the forest at night."), Difficulty.Hard)
            .Card(AnimalCategory, Body("A slow-moving creature that carries its home on its back."), Difficulty.Medium)
            .Card(AnimalCategory, Body("An insect with colourful wings that starts as a caterpillar."), Difficulty.Easy)
            .Card(AnimalCategory, Body("A creature that lives in Australia and looks like a bear."), Difficulty.Medium)
            .Card(AnimalCategory, Body("A bird that swims underwater to catch fish."), Difficulty.Medium)

            // ── OBJECT ────────────────────────────────────────────────────────────
            .Category(ObjectCategory)
            .Card(ObjectCategory, Body("A device you use to see distant things more clearly."), Difficulty.Medium)
            .Card(ObjectCategory, Body("A container you use to drink water or juice."), Difficulty.Easy)
            .Card(ObjectCategory, Body("An object you use to write on paper."), Difficulty.Easy)
            .Card(ObjectCategory, Body("A tool you use to hit a nail into wood."), Difficulty.Easy)
            .Card(ObjectCategory, Body("A chair with wheels that you roll around in."), Difficulty.Easy)
            .Card(ObjectCategory, Body("A device that keeps food cold inside your kitchen."), Difficulty.Easy)
            .Card(ObjectCategory, Body("Something you sleep on at night with a pillow."), Difficulty.Easy)
            .Card(ObjectCategory, Body("A flat surface you put things on to eat."), Difficulty.Medium)
            .Card(ObjectCategory, Body("A device that plays music and videos for entertainment."), Difficulty.Easy)
            .Card(ObjectCategory, Body("Something you wear on your feet when it rains."), Difficulty.Easy)

            // ── PLACE ────────────────────────────────────────────────────────────
            .Category(PlaceCategory)
            .Card(PlaceCategory, Body("A large body of water surrounded by land."), Difficulty.Easy)
            .Card(PlaceCategory, Body("A tall landform that reaches into the clouds."), Difficulty.Easy)
            .Card(PlaceCategory, Body("A location where you can see many types of animals in natural habitats."), Difficulty.Easy)
            .Card(PlaceCategory, Body("An establishment where you go to watch a film on a big screen."), Difficulty.Easy)
            .Card(PlaceCategory, Body("A building where you go to borrow books."), Difficulty.Easy)
            .Card(PlaceCategory, Body("A location at the beach where waves roll to shore."), Difficulty.Medium)
            .Card(PlaceCategory, Body("An underground room where people are buried."), Difficulty.Hard)
            .Card(PlaceCategory, Body("A location with tall buildings, busy streets, and lots of people."), Difficulty.Easy)
            .Card(PlaceCategory, Body("A building dedicated to making you laugh with comedy shows."), Difficulty.Medium)
            .Card(PlaceCategory, Body("An establishment where you go to buy medicine when you're sick."), Difficulty.Easy)

            // ── CONCEPT ──────────────────────────────────────────────────────────
            .Category(ConceptCategory)
            .Card(ConceptCategory, Body("The feeling you have when something bad happens."), Difficulty.Hard)
            .Card(ConceptCategory, Body("A state of mind when you don't know what to do."), Difficulty.Hard)
            .Card(ConceptCategory, Body("The quality of being kind and helpful to others."), Difficulty.Hard)
            .Card(ConceptCategory, Body("The ability to do something well from practice."), Difficulty.Hard)
            .Card(ConceptCategory, Body("A promise to do something in the future."), Difficulty.Medium)
            .Card(ConceptCategory, Body("The opposite of chaos — when everything is orderly."), Difficulty.Hard)
            .Card(ConceptCategory, Body("The freedom to make your own choices."), Difficulty.Hard)
            .Card(ConceptCategory, Body("A set of rules that a community agrees to follow."), Difficulty.Hard)
            .Card(ConceptCategory, Body("The quality of always telling the truth."), Difficulty.Hard)
            .Card(ConceptCategory, Body("A belief or idea that guides how you live."), Difficulty.Hard)

            // ── ACTION ────────────────────────────────────────────────────────────
            .Category(ActionCategory)
            .Card(ActionCategory, Body("To move quickly on foot from one place to another."), Difficulty.Easy)
            .Card(ActionCategory, Body("To propel yourself forward through water using limbs."), Difficulty.Easy)
            .Card(ActionCategory, Body("To move up from a lower place to a higher place."), Difficulty.Easy)
            .Card(ActionCategory, Body("To rest your head and body after being awake all day."), Difficulty.Easy)
            .Card(ActionCategory, Body("To make a loud sound with your mouth and vocal cords."), Difficulty.Easy)
            .Card(ActionCategory, Body("To laugh really hard at something funny."), Difficulty.Easy)
            .Card(ActionCategory, Body("To hug someone to show affection."), Difficulty.Easy)
            .Card(ActionCategory, Body("To eat food quickly without much chewing."), Difficulty.Medium)
            .Card(ActionCategory, Body("To fall through the air from a height."), Difficulty.Medium)
            .Card(ActionCategory, Body("To move your body to music."), Difficulty.Easy)

            // ── EMOTION ──────────────────────────────────────────────────────────
            .Category(EmotionCategory)
            .Card(EmotionCategory, Body("The feeling you have on your birthday."), Difficulty.Easy)
            .Card(EmotionCategory, Body("What you feel when something terrible happens."), Difficulty.Easy)
            .Card(EmotionCategory, Body("The emotion of finding something really hilarious."), Difficulty.Easy)
            .Card(EmotionCategory, Body("How you feel when you've done something wrong."), Difficulty.Easy)
            .Card(EmotionCategory, Body("The feeling when you see someone you love."), Difficulty.Easy)
            .Card(EmotionCategory, Body("The state of mind when you're running late for something important."), Difficulty.Medium)
            .Card(EmotionCategory, Body("What you feel when someone treats you unfairly."), Difficulty.Easy)
            .Card(EmotionCategory, Body("The emotion of being let down by someone."), Difficulty.Hard)
            .Card(EmotionCategory, Body("How you feel at the end of a perfect day."), Difficulty.Hard)
            .Card(EmotionCategory, Body("The sensation when something unexpected happens."), Difficulty.Medium)

            .Build();

    private static string Body(string description) =>
        "<b>Describe this in ONE WORD:</b>\n\n" + description +
        "\n\n<b>Active player:</b> Say one word that describes it. That's it. Just one word.\n\n" +
        "<b>Everyone else:</b> Guess what the original thing is from that one word.";
}
