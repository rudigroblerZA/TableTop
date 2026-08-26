using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Fun;

/// <summary>
/// Roll With It — real dice on the table decide what kind of card comes up.
///
/// <para>
/// The second live consumer of the dice mechanic, after Monogamy — and the
/// first through the generic path. Monogamy's dice-to-zone logic used to live
/// directly on the shared <c>DiceRoll</c> record, which meant no other mode
/// could roll dice for category selection without pulling in
/// <c>MonogamyZone</c>. That coupling is what this mode's existence forced out:
/// <c>DiceRoll</c> is now just two numbers, the mapping lives in
/// <see cref="IDiceProgressionMode.CategoryForTotal"/>, and
/// <c>ControllerFactory</c> dispatches to
/// <c>DiceCategoryProgressionStrategy</c> for any mode that implements it —
/// this one included, with zero controller code of its own.
/// </para>
///
/// <para>
/// Five categories across the 2–12 range, low to high energy: <b>Warm-Up</b>
/// (2–4), <b>Chat</b> (5–6), <b>Act</b> (7–8), <b>Bold</b> (9–10),
/// <b>Wild Card</b> (11–12). Doubles let the roller choose their category
/// outright — the same "doubles = your call" beat Monogamy uses, reused here
/// because it already works: a double is memorable and rare enough (6 in 36)
/// to feel like a real event at the table, not a rules exception nobody
/// notices.
/// </para>
/// </summary>
public sealed class RollWithItMode : BaseGameModeDefinition, IDiceProgressionMode
{
    /// <inheritdoc />
    public override string Name => "Roll With It";

    /// <inheritdoc />
    public override string Description =>
        "Roll two dice — the total picks your category, low-key to wild. Doubles let you choose.";

    /// <inheritdoc />
    public IReadOnlyList<string> CategoriesInOrder => ["Warm-Up", "Chat", "Act", "Bold", "Wild Card"];

    /// <inheritdoc />
    public string CategoryForTotal(int diceTotal) => diceTotal switch
    {
        <= 4  => "Warm-Up",
        <= 6  => "Chat",
        <= 8  => "Act",
        <= 10 => "Bold",
        _     => "Wild Card",
    };

    /// <inheritdoc />
    public override string CompleteLabel => "Rolled With It";
    /// <inheritdoc />
    public override string SkipLabel => "Pass the Dice";

    /// <inheritdoc />
    protected override IScoringStrategy BuildScoring() => new FixedScoringStrategy(1);

    /// <inheritdoc />
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        RollWithItCardBank.All;
}

/// <summary>
/// Compiled fallback for <see cref="RollWithItMode"/>, authored with
/// <see cref="TableTop.Core.Domain.Cards.CardDeckBuilder"/> — the first mode
/// written with it since the builder shipped, rather than a hand-rolled local
/// helper repeating the same three lines every other bank has.
/// </summary>
internal static class RollWithItCardBank
{
    public static IReadOnlyList<ICard> All { get; } = CardDeckBuilder
        .For("Roll With It")

        .Category("Warm-Up")
            .Card("Two Truths",       "Say two true things about your week. Everyone else says which they'd guess is more interesting.", Difficulty.Easy)
            .Card("Favourite Sound",  "Name a sound you genuinely love hearing — a real one, not the obvious answer.", Difficulty.Easy)
            .Card("Small Thing",      "Name one small thing that made today better than it could have been.", Difficulty.Easy)
            .Card("Pick a Side",      "Pancakes or waffles? Defend your answer in one sentence.", Difficulty.Easy)
            .Card("Warm Memory",      "Describe a memory that still makes you smile, in ten seconds or less.", Difficulty.Easy)
            .Card("Quick Compliment", "Give the person to your left a genuine compliment. No deflecting it.", Difficulty.Easy)

        .Category("Chat")
            .Card("Unpopular Opinion", "Share an opinion you know most people at this table will disagree with.", Difficulty.Medium)
            .Card("Would You Rather",  "Would you rather always know when someone's lying, or always get away with lying yourself?", Difficulty.Medium)
            .Card("Old Dream",         "What did you want to be when you were ten? How close did you get?", Difficulty.Medium)
            .Card("Honest Rating",     "Rate your cooking skills out of ten, honestly, and defend the number.", Difficulty.Medium)
            .Card("Group Debate",      "Cats or dogs — the table votes, then the minority defends their choice.", Difficulty.Medium)
            .Card("Recent Regret",     "What's something small you wish you'd done differently this month?", Difficulty.Medium)

        .Category("Act")
            .Card("Silent Movie",       "Act out 'getting ready in the morning' with no sound and no talking for 15 seconds.", Difficulty.Medium)
            .Card("Accent Swap",        "Tell a 10-second story in an accent the table picks for you.", Difficulty.Medium)
            .Card("Freeze Frame",       "Strike a pose that means 'victory' and hold it without laughing for 10 seconds.", Difficulty.Medium)
            .Card("Weather Report",     "Deliver a 15-second weather forecast like it's the biggest news of the year.", Difficulty.Medium)
            .Card("Animal Impression",  "Do your best impression of an animal the table picks after you draw this card.", Difficulty.Easy)
            .Card("Emotional Reading",  "Say 'I can't believe it's already Monday' as if you just won the lottery.", Difficulty.Medium)

        .Category("Bold")
            .Card("Cold Read",         "Let someone read your last three text messages out loud. Your choice who.", Difficulty.Hard)
            .Card("Honest Feedback",   "Ask the table what your worst habit is. They have to answer.", Difficulty.Hard)
            .Card("Sing It",           "Sing the chorus of a song the table picks, no matter how bad your voice is.", Difficulty.Hard)
            .Card("Dare Swap",         "Trade tonight's next turn with another player — they take your dice roll, you take theirs.", Difficulty.Medium)
            .Card("Public Vote",       "The table votes on something embarrassing you have to do right now.", Difficulty.Hard)
            .Card("No Filter",         "Answer the next question anyone asks you with total, unfiltered honesty.", Difficulty.Hard)

        .Category("Wild Card")
            .Card("Rule Change",   "Invent one new rule for the rest of the game. The table votes whether it sticks.", Difficulty.Medium)
            .Card("Swap Seats",    "Everyone swaps seats clockwise. The game continues from wherever you land.", Difficulty.Easy)
            .Card("Double Dice",   "Roll again immediately — this category, then whatever you land on next.", Difficulty.Medium)
            .Card("Steal a Turn",  "Skip the next player's turn and take an extra one yourself.", Difficulty.Medium)
            .Card("Category Freeze", "Wild Card is locked in for everyone's next roll, no matter what they roll.", Difficulty.Medium)
            .Card("Table's Choice", "The table picks which category you play next turn, not the dice.", Difficulty.Easy)

        .Build();
}
