using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Predict This — a betting and deduction game about knowing your friends.
///
/// How to play:
///   1. Read a question that will be asked to one specific person.
///   2. Everyone else bets points on what they think that person will answer.
///   3. The person answers, and anyone who bet correctly gets the points back plus a bonus.
///   4. You can bet on specifics ("They'll say coffee") or general categories ("Something hot").
///
/// It's a game about how well you know people and how well you can bluff. Some people are
/// predictable (they'll always pick coffee). Some are chaos agents (nobody knows what they'll say).
/// The fun is in the bets: do you bet safely on "something they like" or risk it all on "the weirdest
/// possible answer"?
///
/// Great for groups that know each other well OR groups that are still figuring each other out.
/// Works as both a knowledge test and a game of calculated risk.
/// </summary>
public sealed class PredictThisMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Predict This";
    /// <inheritdoc />
    public override string Description =>
        "Bet points on what someone will answer. Guess correctly, win double.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Answered";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [PredictThisCardBank.PreferencesCategory] = "#42A5F5",
            [PredictThisCardBank.ChoicesCategory] = "#66BB6A",
            [PredictThisCardBank.PersonalityCategory] = "#AB47BC",
            [PredictThisCardBank.SecretsCategory] = "#EC407A",
            [PredictThisCardBank.LiesCategory] = "#EF5350",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        PredictThisCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => PredictThisCardBank.All;
}

/// <summary>Built-in card bank for Predict This. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class PredictThisCardBank
{
    internal const string PreferencesCategory = "Preferences";
    internal const string ChoicesCategory = "Choices";
    internal const string PersonalityCategory = "Personality";
    internal const string SecretsCategory = "Secrets";
    internal const string LiesCategory = "Lies";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── PREFERENCES ───────────────────────────────────────────────────────
        P(PreferencesCategory,
            "What's their favourite food?",
            "Everyone bets points on what they'll say. They answer. Correct bets double their points.",
            Difficulty.Easy),
        P(PreferencesCategory,
            "If they could only drink one beverage forever, what would it be?",
            "Everyone bets on the answer. Winner takes the pot plus bonus.",
            Difficulty.Medium),
        P(PreferencesCategory,
            "What's their guilty pleasure movie or show?",
            "Everyone predicts. Correct predictions get rewarded double.",
            Difficulty.Medium),
        P(PreferencesCategory,
            "What music artist would they never admit to liking?",
            "Bet on the answer. Get it right, get the points.",
            Difficulty.Hard),
        P(PreferencesCategory,
            "What food do they claim to hate but secretly love?",
            "Everyone bets. Correct bets win big.",
            Difficulty.Hard),

        // ── CHOICES ───────────────────────────────────────────────────────────
        P(ChoicesCategory,
            "In a zombie apocalypse, what would be their first tool?",
            "Everyone bets on their choice. Correct predictions win.",
            Difficulty.Medium),
        P(ChoicesCategory,
            "If stuck on an island, what one item would they take?",
            "Bet on the answer. Predictions win double.",
            Difficulty.Medium),
        P(ChoicesCategory,
            "They find $100. What's their first purchase?",
            "Everyone predicts. Win big if you're right.",
            Difficulty.Easy),
        P(ChoicesCategory,
            "Given a time machine, which era would they visit?",
            "Bet on their answer. Correct bets doubled.",
            Difficulty.Hard),

        // ── PERSONALITY ───────────────────────────────────────────────────────
        P(PersonalityCategory,
            "How would they describe themselves in one word?",
            "Everyone bets. Closest answer wins.",
            Difficulty.Hard),
        P(PersonalityCategory,
            "What's a trait they secretly don't like about themselves?",
            "Bet on the confession. Predictions win.",
            Difficulty.Hard),
        P(PersonalityCategory,
            "What's their biggest fear?",
            "Everyone predicts. Correct bets win double.",
            Difficulty.Hard),
        P(PersonalityCategory,
            "If they could change one thing about their life, what would it be?",
            "Bet on the answer. Get it right, win big.",
            Difficulty.Hard),

        // ── SECRETS ───────────────────────────────────────────────────────────
        P(SecretsCategory,
            "What's a secret they've never told anyone in this group?",
            "Everyone bets on what they'll reveal. Correct predictions win.",
            Difficulty.Hard),
        P(SecretsCategory,
            "What's the most embarrassing thing that's happened to them?",
            "Bet on their confession. Predictions get rewarded.",
            Difficulty.Hard),
        P(SecretsCategory,
            "What's one thing they're ashamed of?",
            "Everyone predicts. Correct bets doubled.",
            Difficulty.Hard),

        // ── LIES ──────────────────────────────────────────────────────────────
        P(LiesCategory,
            "They will tell one lie mixed with truths. What's the lie?",
            "Everyone bets on which answer is false. Correct predictions win big.",
            Difficulty.Hard),
        P(LiesCategory,
            "They will describe a fake memory. Can you predict which one is the lie?",
            "Bet on the false memory. Win if you're right.",
            Difficulty.Hard),
        P(LiesCategory,
            "They will give fake credentials. Which one is the lie?",
            "Everyone predicts the false claim. Correct bets doubled.",
            Difficulty.Hard),
    ];

    private static ICard P(string category, string question, string mechanics, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>PREDICTION BET</b>\n\n" +
            "Question for [chosen player]: " + question + "\n\n" +
            mechanics + "\n\n" +
            "<b>RULES:</b>\n" +
            "• Everyone starts with 5 points to bet\n" +
            "• Bet privately on what they'll say\n" +
            "• They answer truthfully (unless it's a Lies round)\n" +
            "• Correct predictions double the bet points",
            d, category);
}
