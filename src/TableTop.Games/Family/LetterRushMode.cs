using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Letter Rush — one letter, a handful of categories, and a scramble to fill
/// them all before the timer's up.
///
/// The mirror image of 60 Seconds: there it's one category and you name many;
/// here it's ONE LETTER across several categories and you name one of each.
///
/// How to play:
///   1. Draw a card — it lists 5 categories and gives you a letter (roll a
///      die against the letter strip on the card, or just use the suggested
///      one).
///   2. Everyone has 90 seconds to write an answer STARTING WITH THAT LETTER
///      for every category. "A country: Argentina. An animal: Aardvark…"
///   3. Reveal. Score 1 point per valid answer — but if two people wrote the
///      SAME answer, neither scores it. Reward for being obvious is zero;
///      the game rewards thinking sideways.
///   4. The table judges disputes; majority rules, laughter encouraged.
///
/// Fair across ages because a six-year-old and an adult both just need one
/// word per box — and the "no points for matching" rule quietly helps the
/// creative kid beat the know-it-all.
/// </summary>
public sealed class LetterRushMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Letter Rush";
    /// <inheritdoc />
    public override string Description =>
        "One letter, five categories, 90 seconds — fill them all. Match someone else's answer and it's worth nothing.";

    /// <summary>Label for a completed round.</summary>
    public override string CompleteLabel => "Scored It";
    /// <summary>Label for skipping a card.</summary>
    public override string SkipLabel => "Skip";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [LetterRushCardBank.ClassicCategory] = "#42A5F5",
            [LetterRushCardBank.AroundTheHouseCategory] = "#66BB6A",
            [LetterRushCardBank.OutInTheWorldCategory] = "#FFA726",
            [LetterRushCardBank.ImaginationCategory] = "#AB47BC",
            [LetterRushCardBank.TrickyCategory] = "#EF5350",
        };

    /// <summary>Trickier letter/category mixes are worth more.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in Letter Rush card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        LetterRushCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => LetterRushCardBank.All;
}

/// <summary>Built-in card bank for Letter Rush.</summary>
public static class LetterRushCardBank
{
    internal const string ClassicCategory = "Classic";
    internal const string AroundTheHouseCategory = "Around the House";
    internal const string OutInTheWorldCategory = "Out in the World";
    internal const string ImaginationCategory = "Imagination";
    internal const string TrickyCategory = "Tricky";

    private const string Deck = "Letter Rush";

    /// <summary>All cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    // A gentle default letter for each card (common, forgiving letters for the
    // easy ones; spicier letters for the tricky ones). Players can override
    // by rolling, but the suggestion keeps young kids moving.
    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)
        // ── CLASSIC — the timeless Scattergories staples ─────────────────────
            .Category(ClassicCategory)
            .Card("The Big Five", Body('B', "A country · An animal · A food · A name · A colour"), Difficulty.Easy)
            .Card("Everyday Five", Body('S', "Something you wear · A drink · A job · A city · A sport"), Difficulty.Easy)
            .Card("School Run", Body('M', "A school subject · Something in a pencil case · A shape · A number-word · A playground game"), Difficulty.Easy)

        // ── AROUND THE HOUSE — cosy, findable, kid-friendly ──────────────────
            .Category(AroundTheHouseCategory)
            .Card("Kitchen Sweep", Body('P', "Something in the fridge · A kitchen tool · A breakfast food · Something you drink · A snack"), Difficulty.Easy)
            .Card("Toy Box", Body('T', "A toy · A cartoon character · A board game · Something bouncy · A thing with wheels"), Difficulty.Medium)
            .Card("Getting Ready", Body('C', "Something in the bathroom · An item of clothing · Something you brush · A smell · Something soft"), Difficulty.Medium)

        // ── OUT IN THE WORLD — places, nature, going-places ──────────────────
            .Category(OutInTheWorldCategory)
            .Card("On Holiday", Body('H', "A place you'd visit · Something you pack · A type of weather · A thing at the beach · A souvenir"), Difficulty.Medium)
            .Card("Nature Walk", Body('F', "A tree · A flower · A bird · An insect · Something you'd find on the ground"), Difficulty.Medium)
            .Card("Big City", Body('G', "A capital city · Something tall · A vehicle · A shop · A landmark"), Difficulty.Hard)

        // ── IMAGINATION — creative, sideways, funnier ────────────────────────
            .Category(ImaginationCategory)
            .Card("Storybook", Body('D', "A magical creature · A hero's name · A spooky place · A superpower · A word in a spell"), Difficulty.Medium)
            .Card("Silly Business", Body('W', "A terrible band name · A made-up holiday · A rejected ice-cream flavour · A pet you shouldn't own · A worst-ever superpower"), Difficulty.Hard)
            .Card("Movie Night", Body('R', "A film · A film villain · A word in a movie title · A snack you'd sneak in · A genre"), Difficulty.Hard)

        // ── TRICKY — spicier letters, meaner categories ──────────────────────
            .Category(TrickyCategory)
            .Card("The Hard Letter", Body('K', "A country · A famous person · A food · An animal · A verb"), Difficulty.Extreme)
            .Card("Brain Stretch", Body('V', "A body part · A job · Something in space · A language · An adjective"), Difficulty.Extreme)
            .Card("No Easy Answers", Body('J', "A boys' or girls' name · A country · A fruit or vegetable · A verb · Something you'd find in a garage"), Difficulty.Extreme)

            .Build();

    private static string Body(char letter, string categories) =>
        "<b>🔤 LETTER RUSH — 90 seconds</b>\n\n" +
        "Your letter: <b>" + letter + "</b>  <i>(or roll for a new one)</i>\n\n" +
        "Fill each with something starting with <b>" + letter + "</b>:\n" +
        "• " + categories.Replace(" · ", "\n• ") + "\n\n" +
        "<i>1 point per valid answer. Match someone else and neither of you scores it — think sideways.</i>";
}
