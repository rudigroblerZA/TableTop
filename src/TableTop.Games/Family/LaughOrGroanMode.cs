using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Laugh or Groan — family dilemmas, scenarios, and silly choices.
///
/// Each card presents either:
///   Would You Rather — two equally unappealing or equally appealing choices
///   Scenario         — "what would you do if..." with a genuinely ridiculous situation
///   Hot Take         — an opinion everyone must rate Laugh, Groan, or Hmm
///
/// Works like this: the active player reads the card. Everyone votes or answers.
/// The conversation after the answer is the point.
///
/// Age-inclusive: designed so every answer is worth hearing regardless of age.
/// Nothing risqué, nothing embarrassing in a mean way — just reliably silly.
/// </summary>
public sealed class LaughOrGroanMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Laugh or Groan";
    /// <inheritdoc />
    public override string Description =>
        "Silly dilemmas, ridiculous scenarios, and hot takes the whole family will argue about.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "→ Next";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "⤳ Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [LaughOrGroanCardBank.WouldYouRatherCategory] = "#FFCA28",
            [LaughOrGroanCardBank.ScenarioCategory] = "#42A5F5",
            [LaughOrGroanCardBank.HotTakeCategory] = "#EC407A",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        LaughOrGroanCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => LaughOrGroanCardBank.All;
}

/// <summary>Built-in card bank for LaughOrGroan. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class LaughOrGroanCardBank
{
    internal const string WouldYouRatherCategory = "Would You Rather";
    internal const string ScenarioCategory = "Scenario";
    internal const string HotTakeCategory = "Hot Take";

    private const string Deck = "Laugh or Groan";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)
            .Category(WouldYouRatherCategory)
        // ── WOULD YOU RATHER ─────────────────────────────────────────────────

            .Card(WouldYouRatherCategory, "Would you rather have hands for feet, or feet for hands?", Difficulty.Easy)

            .Card(WouldYouRatherCategory, "Would you rather only be able to whisper for the rest of your life, or only be able to shout?", Difficulty.Easy)

            .Card(WouldYouRatherCategory, "Would you rather eat a spoonful of wasabi or a spoonful of vegemite?", Difficulty.Easy)

            .Card(WouldYouRatherCategory, "Would you rather live in a house that is entirely upside down (floor on top, ceiling below), or one that is rotated 90° so you walk on the walls?", Difficulty.Easy)

            .Card(WouldYouRatherCategory, "Would you rather have a pet dinosaur (the size of a dog) or a pet dragon (the size of a hamster)?", Difficulty.Easy)

            .Card(WouldYouRatherCategory, "Would you rather only be able to move by skipping, or only be able to move by crawling?", Difficulty.Easy)

            .Card(WouldYouRatherCategory, "Would you rather eat your meals through a straw forever, or eat only soup forever but with a fork?", Difficulty.Easy)

            .Card(WouldYouRatherCategory, "Would you rather always have to speak in rhyme, or always have to sing instead of talking?", Difficulty.Easy)

            .Card(WouldYouRatherCategory, "Would you rather know what every animal in the world is thinking, or know what every person in your town is thinking?", Difficulty.Medium)

            .Card(WouldYouRatherCategory, "Would you rather always arrive one hour early everywhere, or always arrive thirty minutes late?", Difficulty.Medium)

            .Card(WouldYouRatherCategory, "Would you rather only ever wear one colour for the rest of your life (you pick the colour), or wear a completely random outfit every morning chosen by someone else?", Difficulty.Medium)

            .Card(WouldYouRatherCategory, "Would you rather be able to speak every language in the world, or be able to play every instrument?", Difficulty.Medium)

            .Card(WouldYouRatherCategory, "Would you rather live for one year on the moon (you can breathe, everything works), or spend one year at the bottom of the ocean (same rules)?", Difficulty.Medium)

            .Card(WouldYouRatherCategory, "Would you rather have to apologise to a stranger every time you pass one on the street, or have to high-five every person you know when you see them?", Difficulty.Medium)

            .Card(WouldYouRatherCategory, "Would you rather taste everything you touch, or smell every sound you hear?", Difficulty.Medium)

            .Card(WouldYouRatherCategory, "Would you rather have a pause button for real life (pauses everyone else for up to five minutes, three times a day), or a rewind button (goes back thirty seconds, once a day)?", Difficulty.Medium)

            .Card(WouldYouRatherCategory, "Would you rather live in a world with no music, or a world with no books?", Difficulty.Hard)

            .Card(WouldYouRatherCategory, "Would you rather know the exact day you're going to die, or never know but know it will be before you're 50?", Difficulty.Hard)

            .Card(WouldYouRatherCategory, "Would you rather have total recall (remember everything perfectly), but with no ability to dream — or dream vividly every night but forget everything within three days?", Difficulty.Hard)

            .Card(WouldYouRatherCategory, "Would you rather always know when someone is lying but never be able to call it out, or be completely unable to tell when someone is lying?", Difficulty.Hard)

            .Category(ScenarioCategory)
        // ── SCENARIOS ────────────────────────────────────────────────────────

            .Card(ScenarioCategory, "You wake up one morning and find that gravity has reversed — everything is floating. You have one hour before it goes back to normal. What is the first thing you do?", Difficulty.Easy)

            .Card(ScenarioCategory, "A genie gives you the power to make any food taste like your favourite food for one week. What food do you choose as your favourite — and what unexpected consequence would this cause?", Difficulty.Easy)

            .Card(ScenarioCategory, "You discover that for the next 24 hours you can understand and speak any language — but only while making a different animal sound for each language. French is a duck, German is a cow, and so on. Do you use it? How?", Difficulty.Easy)

            .Card(ScenarioCategory, "You are granted the ability to move any single object in your house with your mind — but only that one object, forever. Which object do you choose and why?", Difficulty.Easy)

            .Card(ScenarioCategory, "A wizard curses your household: for one month, every time someone tells a lie — any lie, even tiny ones — everyone in the house can hear a loud foghorn. How does your household cope?", Difficulty.Medium)

            .Card(ScenarioCategory, "You are offered a job where you earn three times your current salary, but every day at exactly 3pm you must stop whatever you're doing and do the Macarena for three minutes. Do you take it?", Difficulty.Medium)

            .Card(ScenarioCategory, "You can time-travel once, but only backwards, only one year, and you must bring one family member with you. Where — or rather when — do you go, and what do you plan to do there?", Difficulty.Medium)

            .Card(ScenarioCategory, "Your dog can suddenly speak — but only in the voice and vocabulary of a Victorian butler. What is the first thing he says, and how does your family respond?", Difficulty.Medium)

            .Card(ScenarioCategory, "A TV production company wants to make a reality show about your family. You can approve it — but you have no creative control over the title, the editing, or the theme music. Do you agree? And what do you think the title would be?", Difficulty.Medium)

            .Card(ScenarioCategory, "You wake up tomorrow with the ability to fly — but only at walking speed, and only one metre off the ground. How does this change your daily life?", Difficulty.Medium)

            .Card(ScenarioCategory, "You must host a dinner party for five guests from any point in history. The catch: you can only serve food from one decade (the 1970s, for example), and all five guests hate at least two of your other guests. Who do you invite?", Difficulty.Hard)

            .Card(ScenarioCategory, "You are offered a pill that removes your ability to feel embarrassed — ever again. The side effect is that you also lose the ability to feel pride. Do you take it?", Difficulty.Hard)

            .Card(ScenarioCategory, "You discover that every important decision you've made in the last ten years was actually secretly made by a different version of you from an alternate timeline. Your decisions have all been right. Does this bother you?", Difficulty.Hard)

            .Category(HotTakeCategory)
        // ── HOT TAKES ────────────────────────────────────────────────────────

            .Card(HotTakeCategory, HtBody("Cats are objectively better pets than dogs."), Difficulty.Easy)

            .Card(HotTakeCategory, HtBody("Breakfast is the best meal of the day and anyone who disagrees is wrong."), Difficulty.Easy)

            .Card(HotTakeCategory, HtBody("Pineapple on pizza is genuinely good and the controversy about it is annoying."), Difficulty.Easy)

            .Card(HotTakeCategory, HtBody("It is completely fine to recline your seat on an aeroplane."), Difficulty.Easy)

            .Card(HotTakeCategory, HtBody("Monopoly is a bad game and the person who invented it was trying to ruin families."), Difficulty.Easy)

            .Card(HotTakeCategory, HtBody("Winter is better than summer."), Difficulty.Easy)

            .Card(HotTakeCategory, HtBody("The best part of Christmas is the food, not the presents."), Difficulty.Easy)

            .Card(HotTakeCategory, HtBody("It should be socially acceptable to eat your dinner in any order you like — starting with dessert if you want."), Difficulty.Easy)

            .Card(HotTakeCategory, HtBody("Reading a book is always better than watching the film version."), Difficulty.Easy)

            .Card(HotTakeCategory, HtBody("Rollercoasters are not fun. They are just scary. Fun and scary are not the same thing."), Difficulty.Easy)

            .Card(HotTakeCategory, HtBody("The last biscuit in the packet should always be left for someone else."), Difficulty.Easy)

            .Card(HotTakeCategory, HtBody("Singing in the car when you think no one can hear you is one of life's genuine pleasures."), Difficulty.Easy)

            .Card(HotTakeCategory, HtBody("Socks and sandals is actually a sensible combination that has been unfairly mocked."), Difficulty.Easy)

            .Card(HotTakeCategory, HtBody("The best age to be is the age you are right now."), Difficulty.Easy)

            .Card(HotTakeCategory, HtBody("Silence is an underrated form of entertainment."), Difficulty.Easy)

            .Card(HotTakeCategory, HtBody("Leftovers always taste better the next day."), Difficulty.Easy)

            .Build();

    private static string HtBody(string text) =>
        text + "\n\n<b>Each person rates it:</b> 👏 Laugh (agree) · 😬 Groan (disagree) · 🤔 Hmm (depends).\n\nMajority must defend their position for thirty seconds.";
}