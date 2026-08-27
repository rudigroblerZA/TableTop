using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;
using TableTop.Games.Fun;

namespace TableTop.Games.Family;

/// <summary>
/// Dice Night — real dice on the table decide what kind of card comes up.
///
/// <para>
/// The third live consumer of the generic dice mechanic
/// <see cref="RollWithItMode"/> opened up, and the first written for the
/// family table specifically: everything here is All Ages, unlike
/// <see cref="RollWithItMode"/>'s Teen floor, so a table with young kids in
/// it has a dice-driven option too, not just the quiz/dares/charades style
/// already in <c>fun.family</c>.
/// </para>
///
/// <para>
/// Five categories across the 2–12 range, low to high energy:
/// <b>Icebreaker</b> (2–4), <b>Giggle</b> (5–6), <b>Story Time</b> (7–8),
/// <b>Silly Challenge</b> (9–10), <b>Grand Finale</b> (11–12). Doubles let
/// the roller choose their category outright — the same "doubles = your
/// call" beat <see cref="RollWithItMode"/> and Monogamy both use.
/// </para>
/// </summary>
public sealed class DiceNightMode : BaseGameModeDefinition, IDiceProgressionMode
{
    /// <inheritdoc />
    public override string Name => "Dice Night";

    /// <inheritdoc />
    public override string Description =>
        "Roll two dice — the total picks your category, calm to chaotic. Doubles let you choose. Fun for the whole family.";

    /// <inheritdoc />
    public IReadOnlyList<string> CategoriesInOrder =>
        ["Icebreaker", "Giggle", "Story Time", "Silly Challenge", "Grand Finale"];

    /// <inheritdoc />
    public string CategoryForTotal(int diceTotal) => diceTotal switch
    {
        <= 4 => "Icebreaker",
        <= 6 => "Giggle",
        <= 8 => "Story Time",
        <= 10 => "Silly Challenge",
        _ => "Grand Finale",
    };

    /// <inheritdoc />
    public override string CompleteLabel => "Nailed It!";
    /// <inheritdoc />
    public override string SkipLabel => "Skip This Roll";

    /// <inheritdoc />
    protected override IScoringStrategy BuildScoring() => new FixedScoringStrategy(1);

    /// <inheritdoc />
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        DiceNightCardBank.All;
}

/// <summary>
/// Compiled fallback for <see cref="DiceNightMode"/>, authored with
/// <see cref="TableTop.Core.Domain.Cards.CardDeckBuilder"/> — same shape as
/// <see cref="RollWithItMode"/>'s bank, kept wholesome for a table that
/// includes young kids.
/// </summary>
internal static class DiceNightCardBank
{
    public static IReadOnlyList<ICard> All { get; } = CardDeckBuilder
        .For("Dice Night")

        .Category("Icebreaker")
            .Card("Favourite Season", "What's your favourite season of the year, and what's the best thing about it?", Difficulty.Easy)
            .Card("Best Snack", "Name the best snack in the house right now. No wrong answers.", Difficulty.Easy)
            .Card("Animal Pick", "If you could have any animal as a pet, no rules, what would you pick?", Difficulty.Easy)
            .Card("Good Day", "Describe your perfect Saturday from start to finish in ten seconds.", Difficulty.Easy)
            .Card("Superpower", "Pick one superpower you'd actually want to have, and why.", Difficulty.Easy)
            .Card("Favourite Place", "What's your favourite place you've ever visited, or want to visit someday?", Difficulty.Easy)

        .Category("Giggle")
            .Card("Silly Sound", "Make the silliest sound you can think of. The table rates it out of ten.", Difficulty.Easy)
            .Card("Made-Up Word", "Invent a brand-new word and tell the table what it means.", Difficulty.Easy)
            .Card("Funny Face", "Pull the funniest face you can. Hold it for five seconds without laughing.", Difficulty.Easy)
            .Card("Rhyme Time", "Say a word. The table has ten seconds to find a word that rhymes with it.", Difficulty.Medium)
            .Card("Silly Walk", "Walk across the room in the silliest way you can invent.", Difficulty.Easy)
            .Card("Joke Time", "Tell the table your favourite joke, even if everyone's heard it before.", Difficulty.Easy)

        .Category("Story Time")
            .Card("Once Upon A Time", "Start a story with 'Once upon a time...' — the next player continues it.", Difficulty.Medium)
            .Card("What Happened Next", "Describe what happens next in the last movie or show you watched, but make it up.", Difficulty.Medium)
            .Card("Adventure Begins", "You just found a mysterious map. Where does it lead? Tell the table.", Difficulty.Medium)
            .Card("Talking Animal", "Tell a short story where the main character is a talking animal of your choice.", Difficulty.Medium)
            .Card("Family Tale", "Tell the table a favourite memory from a family trip or holiday.", Difficulty.Easy)
            .Card("The Twist", "Tell a very short story that ends with a surprising twist.", Difficulty.Medium)

        .Category("Silly Challenge")
            .Card("Balance Test", "Balance something small on your head for ten seconds without touching it.", Difficulty.Medium)
            .Card("One-Breath Sentence", "Say the longest sentence you can in a single breath.", Difficulty.Medium)
            .Card("Mystery Object", "Close your eyes. Someone hands you an object. Guess what it is by touch alone.", Difficulty.Medium)
            .Card("Freeze Dance", "Dance until someone says 'freeze' — then hold your pose for five seconds.", Difficulty.Medium)
            .Card("Tower Build", "Build the tallest tower you can out of whatever's on the table in thirty seconds.", Difficulty.Medium)
            .Card("Impression Round", "Do an impression of another player at the table — kindly! Others guess who.", Difficulty.Medium)

        .Category("Grand Finale")
            .Card("Group Cheer", "Invent a team cheer or chant for the whole table and perform it together.", Difficulty.Medium)
            .Card("Trade Places", "Swap seats with the player across from you and stay there for the next round.", Difficulty.Easy)
            .Card("Double Roll", "Roll again immediately — this category, then whatever you land on next.", Difficulty.Medium)
            .Card("Table Vote", "The table votes on one silly thing everyone has to do together right now.", Difficulty.Medium)
            .Card("New Rule", "Invent one new house rule for the rest of the game. The table votes whether it sticks.", Difficulty.Medium)
            .Card("Victory Lap", "Take a lap around the room celebrating like you just won a gold medal.", Difficulty.Easy)

        .Build();
}
