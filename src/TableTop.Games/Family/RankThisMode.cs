using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Rank This — a ranking and prediction game for groups.
///
/// How to play:
///   1. Read the prompt aloud (an absurd item or scenario to rank).
///   2. Everyone privately ranks it 1–5 (where 1 = "never" and 5 = "absolutely").
///   3. Reveal rankings and discuss why they're so different.
///   4. Points awarded for agreements and for predicting how the group will vote.
///
/// Cards range from silly ("How much would you enjoy a sandwich made entirely of dessert?")
/// to thought-provoking ("How ready do you feel for a major life change?"). The fun is in
/// discovering that your friends are weirder — or more sane — than you expected.
///
/// Great for mixed ages because everyone's ranking is valid and defended.
/// </summary>
public sealed class RankThisMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Rank This";
    /// <inheritdoc />
    public override string Description =>
        "Rank absurd things 1–5. Reveal. Argue about why. Discover who's normal and who isn't.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Ranked";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Pass";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [RankThisCardBank.SillyCategory] = "#EC407A",
            [RankThisCardBank.PreferenceCategory] = "#FFCA28",
            [RankThisCardBank.ValuesCategory] = "#66BB6A",
            [RankThisCardBank.ScaryCategory] = "#EF5350",
            [RankThisCardBank.WeirdCategory] = "#AB47BC",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        RankThisCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => RankThisCardBank.All;
}

/// <summary>Built-in card bank for Rank This. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class RankThisCardBank
{
    internal const string SillyCategory = "Silly";
    internal const string PreferenceCategory = "Preference";
    internal const string ValuesCategory = "Values";
    internal const string ScaryCategory = "Scary";
    internal const string WeirdCategory = "Weird";

    private const string Deck = "Rank This";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)
        // ── SILLY ─────────────────────────────────────────────────────────────
            .Category(SillyCategory)
            .Card(SillyCategory, Body("How much would you enjoy a sandwich made entirely of dessert?"), Difficulty.Easy)
            .Card(SillyCategory, Body("How entertaining would it be to narrate your own life like a nature documentary?"), Difficulty.Easy)
            .Card(SillyCategory, Body("How funny is a penguin in a top hat?"), Difficulty.Easy)
            .Card(SillyCategory, Body("How practical would it be if gravity worked sideways?"), Difficulty.Medium)
            .Card(SillyCategory, Body("How good an idea is it to have a pet that's just a sentient sock?"), Difficulty.Easy)
            .Card(SillyCategory, Body("How would you rate having spaghetti for hair instead of actual hair?"), Difficulty.Easy)
            .Card(SillyCategory, Body("How much fun is a bathroom that's secretly a water slide?"), Difficulty.Medium)
            .Card(SillyCategory, Body("How useful would a TV remote that controls your life be?"), Difficulty.Medium)
            .Card(SillyCategory, Body("How great would it be if squirrels could talk?"), Difficulty.Easy)
            .Card(SillyCategory, Body("How hilarious would it be if everyone walked backwards on Tuesdays?"), Difficulty.Easy)
            .Card(SillyCategory, Body("How good of a career choice is professional pillow fort architect?"), Difficulty.Easy)
            .Card(SillyCategory, Body("How much would you enjoy living in a house made entirely of cheese?"), Difficulty.Medium)
            .Card(SillyCategory, Body("How good an idea is a doorbell that only plays kazoo music?"), Difficulty.Easy)
            .Card(SillyCategory, Body("How chaotic would a world without knees be?"), Difficulty.Medium)

        // ── PREFERENCE ───────────────────────────────────────────────────────
            .Category(PreferenceCategory)
            .Card(PreferenceCategory, Body("How much do you like pineapple on pizza?"), Difficulty.Easy)
            .Card(PreferenceCategory, Body("How important is having a shower vs. taking a bath?"), Difficulty.Easy)
            .Card(PreferenceCategory, Body("How essential is coffee to your happiness?"), Difficulty.Easy)
            .Card(PreferenceCategory, Body("How much do you enjoy spicy food?"), Difficulty.Easy)
            .Card(PreferenceCategory, Body("How much would you want your job to be your passion?"), Difficulty.Medium)
            .Card(PreferenceCategory, Body("How much do you prefer mountains or beaches?"), Difficulty.Easy)
            .Card(PreferenceCategory, Body("How important is having a big group of friends vs. a few close ones?"), Difficulty.Medium)
            .Card(PreferenceCategory, Body("How much do you love the smell of fresh bread?"), Difficulty.Easy)
            .Card(PreferenceCategory, Body("How much do you enjoy early mornings?"), Difficulty.Easy)
            .Card(PreferenceCategory, Body("How much would you want to live in a big city?"), Difficulty.Medium)
            .Card(PreferenceCategory, Body("How much do you prefer window seats over aisle seats?"), Difficulty.Easy)
            .Card(PreferenceCategory, Body("How much would you rather text than call?"), Difficulty.Easy)

        // ── VALUES ────────────────────────────────────────────────────────────
            .Category(ValuesCategory)
            .Card(ValuesCategory, Body("How important is honesty, even when it hurts?"), Difficulty.Hard)
            .Card(ValuesCategory, Body("How much does winning matter to you?"), Difficulty.Medium)
            .Card(ValuesCategory, Body("How important is helping others before helping yourself?"), Difficulty.Hard)
            .Card(ValuesCategory, Body("How much do you believe in second chances?"), Difficulty.Hard)
            .Card(ValuesCategory, Body("How important is tradition in your life?"), Difficulty.Medium)
            .Card(ValuesCategory, Body("How much do you believe everything happens for a reason?"), Difficulty.Hard)
            .Card(ValuesCategory, Body("How important is ambition in living a good life?"), Difficulty.Medium)
            .Card(ValuesCategory, Body("How much do you think forgiveness is stronger than holding a grudge?"), Difficulty.Hard)
            .Card(ValuesCategory, Body("How important is keeping a promise, even a small one?"), Difficulty.Medium)
            .Card(ValuesCategory, Body("How much does loyalty matter to you over honesty?"), Difficulty.Hard)

        // ── SCARY ────────────────────────────────────────────────────────────
            .Category(ScaryCategory)
            .Card(ScaryCategory, Body("How scary would it be to wake up with no memory?"), Difficulty.Medium)
            .Card(ScaryCategory, Body("How nervous would you be about public speaking at a huge event?"), Difficulty.Medium)
            .Card(ScaryCategory, Body("How terrifying would it be to see a ghost?"), Difficulty.Easy)
            .Card(ScaryCategory, Body("How scary is deep water?"), Difficulty.Easy)
            .Card(ScaryCategory, Body("How frightening would it be to make a huge mistake at work?"), Difficulty.Medium)
            .Card(ScaryCategory, Body("How scary is the idea of being truly alone?"), Difficulty.Hard)
            .Card(ScaryCategory, Body("How unsettling would it be to hear your own voice on an answering machine you don't remember leaving?"), Difficulty.Medium)
            .Card(ScaryCategory, Body("How scary is the idea of moving somewhere you know nobody?"), Difficulty.Medium)

        // ── WEIRD ────────────────────────────────────────────────────────────
            .Category(WeirdCategory)
            .Card(WeirdCategory, Body("How weird would it be if mirrors showed your future instead of your reflection?"), Difficulty.Medium)
            .Card(WeirdCategory, Body("How strange would it be if everyone had to wear their dreams on a shirt?"), Difficulty.Medium)
            .Card(WeirdCategory, Body("How bizarre would it be if you could taste colours?"), Difficulty.Medium)
            .Card(WeirdCategory, Body("How odd would it be if plants could communicate with you?"), Difficulty.Medium)
            .Card(WeirdCategory, Body("How unusual would it be if your shadow had a mind of its own?"), Difficulty.Medium)
            .Card(WeirdCategory, Body("How strange would it be if every door led somewhere different each time?"), Difficulty.Medium)
            .Card(WeirdCategory, Body("How odd would it be if your reflection was always one second behind you?"), Difficulty.Medium)

            .Build();

    private static string Body(string prompt) =>
        "<b>Rank this on a scale of 1–5:</b>\n\n" + prompt +
        "\n\n<i>1 = Not at all  ·  5 = Absolutely yes</i>\n\n" +
        "Everyone writes down your ranking privately. Then reveal and discuss!";
}
