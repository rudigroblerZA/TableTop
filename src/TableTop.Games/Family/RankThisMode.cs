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

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── SILLY ─────────────────────────────────────────────────────────────
        R(SillyCategory, "How much would you enjoy a sandwich made entirely of dessert?", Difficulty.Easy),
        R(SillyCategory, "How entertaining would it be to narrate your own life like a nature documentary?", Difficulty.Easy),
        R(SillyCategory, "How funny is a penguin in a top hat?", Difficulty.Easy),
        R(SillyCategory, "How practical would it be if gravity worked sideways?", Difficulty.Medium),
        R(SillyCategory, "How good an idea is it to have a pet that's just a sentient sock?", Difficulty.Easy),
        R(SillyCategory, "How would you rate having spaghetti for hair instead of actual hair?", Difficulty.Easy),
        R(SillyCategory, "How much fun is a bathroom that's secretly a water slide?", Difficulty.Medium),
        R(SillyCategory, "How useful would a TV remote that controls your life be?", Difficulty.Medium),
        R(SillyCategory, "How great would it be if squirrels could talk?", Difficulty.Easy),
        R(SillyCategory, "How hilarious would it be if everyone walked backwards on Tuesdays?", Difficulty.Easy),
        R(SillyCategory, "How good of a career choice is professional pillow fort architect?", Difficulty.Easy),
        R(SillyCategory, "How much would you enjoy living in a house made entirely of cheese?", Difficulty.Medium),

        // ── PREFERENCE ───────────────────────────────────────────────────────
        R(PreferenceCategory, "How much do you like pineapple on pizza?", Difficulty.Easy),
        R(PreferenceCategory, "How important is having a shower vs. taking a bath?", Difficulty.Easy),
        R(PreferenceCategory, "How essential is coffee to your happiness?", Difficulty.Easy),
        R(PreferenceCategory, "How much do you enjoy spicy food?", Difficulty.Easy),
        R(PreferenceCategory, "How much would you want your job to be your passion?", Difficulty.Medium),
        R(PreferenceCategory, "How much do you prefer mountains or beaches?", Difficulty.Easy),
        R(PreferenceCategory, "How important is having a big group of friends vs. a few close ones?", Difficulty.Medium),
        R(PreferenceCategory, "How much do you love the smell of fresh bread?", Difficulty.Easy),
        R(PreferenceCategory, "How much do you enjoy early mornings?", Difficulty.Easy),
        R(PreferenceCategory, "How much would you want to live in a big city?", Difficulty.Medium),

        // ── VALUES ────────────────────────────────────────────────────────────
        R(ValuesCategory, "How important is honesty, even when it hurts?", Difficulty.Hard),
        R(ValuesCategory, "How much does winning matter to you?", Difficulty.Medium),
        R(ValuesCategory, "How important is helping others before helping yourself?", Difficulty.Hard),
        R(ValuesCategory, "How much do you believe in second chances?", Difficulty.Hard),
        R(ValuesCategory, "How important is tradition in your life?", Difficulty.Medium),
        R(ValuesCategory, "How much do you believe everything happens for a reason?", Difficulty.Hard),
        R(ValuesCategory, "How important is ambition in living a good life?", Difficulty.Medium),
        R(ValuesCategory, "How much do you think forgiveness is stronger than holding a grudge?", Difficulty.Hard),

        // ── SCARY ────────────────────────────────────────────────────────────
        R(ScaryCategory, "How scary would it be to wake up with no memory?", Difficulty.Medium),
        R(ScaryCategory, "How nervous would you be about public speaking at a huge event?", Difficulty.Medium),
        R(ScaryCategory, "How terrifying would it be to see a ghost?", Difficulty.Easy),
        R(ScaryCategory, "How scary is deep water?", Difficulty.Easy),
        R(ScaryCategory, "How frightening would it be to make a huge mistake at work?", Difficulty.Medium),
        R(ScaryCategory, "How scary is the idea of being truly alone?", Difficulty.Hard),

        // ── WEIRD ────────────────────────────────────────────────────────────
        R(WeirdCategory, "How weird would it be if mirrors showed your future instead of your reflection?", Difficulty.Medium),
        R(WeirdCategory, "How strange would it be if everyone had to wear their dreams on a shirt?", Difficulty.Medium),
        R(WeirdCategory, "How bizarre would it be if you could taste colours?", Difficulty.Medium),
        R(WeirdCategory, "How odd would it be if plants could communicate with you?", Difficulty.Medium),
        R(WeirdCategory, "How unusual would it be if your shadow had a mind of its own?", Difficulty.Medium),
    ];

    private static ICard R(string category, string prompt, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>Rank this on a scale of 1–5:</b>\n\n" + prompt +
            "\n\n<i>1 = Not at all  ·  5 = Absolutely yes</i>\n\n" +
            "Everyone writes down your ranking privately. Then reveal and discuss!",
            d, category);
}
