using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Explain It Back — you're not scored on saying it. You're scored on whether
/// they could have said it back.
///
/// Every reasoning mode in the catalogue — Logic Lab, Odd One Out, Wrong
/// Answers Only, the whole Grade 6 shelf — scores the person holding the card.
/// They answer, they're right or wrong, points follow. None of them scores the
/// <i>listener's</i> understanding, which is the one thing a classroom actually
/// exists to produce. This mode does.
///
/// The mechanic is the protégé effect, real and well-studied: you understand
/// something best once you've had to make someone else understand it, because
/// explaining exposes exactly which part you were only pretending to know.
/// Every card is built the same way — a concept to teach, and a Check the
/// listener has to pass afterward. The card-holder is graded entirely on
/// whether the Check succeeds. Reciting a correct definition that leaves the
/// listener unable to answer the Check is not a win here; a rough, stumbling
/// explanation that gets them through it is.
///
/// How to play:
///   1. Read the concept to yourself — not aloud. Take a moment.
///   2. Teach it. Out loud, in your own words, no reading the card verbatim,
///      no jargon they haven't already got from you first.
///   3. Read the Check aloud and let the listener answer it — without help.
///   4. They got it → Completed. They didn't → Didn't Land, and say which part
///      lost them; that part is usually the actual lesson.
///
/// Six subjects, general-knowledge level rather than curriculum-bound — this
/// sits alongside Logic Lab and Odd One Out rather than nested under Grade 6,
/// since nothing here is grade-specific.
/// </summary>
public sealed class ExplainItBackMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Explain It Back";

    /// <inheritdoc />
    public override string Description =>
        "Teach the concept in your own words. Then they answer the Check — " +
        "unaided. You're scored on whether they got it, not on whether you said it right.";

    /// <summary>Awarded when the Check succeeds — the listener's understanding, not the explainer's recitation.</summary>
    public override string CompleteLabel => "They Got It";

    /// <summary>The honest outcome when the explanation didn't land. Says where the teaching failed, not the player.</summary>
    public override string SkipLabel => "Didn't Land";

    /// <summary>Category → hex colour, one per subject.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [ExplainItBackCardBank.MathsCategory] = "#42A5F5",
            [ExplainItBackCardBank.ScienceCategory] = "#66BB6A",
            [ExplainItBackCardBank.EnglishCategory] = "#AB47BC",
            [ExplainItBackCardBank.HistoryCategory] = "#EF5350",
            [ExplainItBackCardBank.GeographyCategory] = "#26A69A",
            [ExplainItBackCardBank.LogicCategory] = "#FFA726",
        };

    /// <summary>
    /// Scoreless by design, same reasoning as Constraint Master and Wrong
    /// Answers Only: the group judges the Check by ear, in the moment, and a
    /// running number would pressure the explainer toward a fast, safe
    /// recitation instead of the slower plain-language attempt that actually
    /// teaches.
    /// </summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <inheritdoc />
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        ExplainItBackCardBank.All;
}

/// <summary>
/// Compiled fallback for <see cref="ExplainItBackMode"/>. A static list, so
/// card ids stay stable across runs — the whole engine's played-card tracking
/// depends on that for save/resume, and this mode is no exception.
/// </summary>
internal static class ExplainItBackCardBank
{
    internal const string MathsCategory = "Maths";
    internal const string ScienceCategory = "Science";
    internal const string EnglishCategory = "English";
    internal const string HistoryCategory = "History";
    internal const string GeographyCategory = "Geography";
    internal const string LogicCategory = "Logic";

    private static ICard C(string category, string body, Difficulty difficulty) =>
        new StandardCard(
            id: StableId(category, body),
            title: category,
            description: body,
            difficulty: difficulty,
            category: category);

    private static Guid StableId(string category, string body) =>
        new(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"explain-it-back|{category}|{body}"))[..16]);

    public static IReadOnlyList<ICard> All { get; } =
    [
        // ── Maths ──────────────────────────────────────────────────────────────
        C(MathsCategory,
            "Teach: why you flip the second fraction when you divide by a fraction.\n\n" +
            "Check: 3 ÷ 1/2 — what's the answer, and why did flipping and multiplying give it to you?",
            Difficulty.Medium),
        C(MathsCategory,
            "Teach: what a percentage actually is — not the rule for finding one, the idea itself.\n\n" +
            "Check: which is bigger, 30% of 200 or 60% of 90 — without a calculator, how do they know?",
            Difficulty.Easy),
        C(MathsCategory,
            "Teach: why a negative number times a negative number is positive.\n\n" +
            "Check: what is -4 × -3, and can they say why in their own words, not just recite the rule?",
            Difficulty.Hard),
        C(MathsCategory,
            "Teach: the difference between area and perimeter, using something in the room.\n\n" +
            "Check: point to something and ask which one changed and which one didn't when you moved to a bigger example.",
            Difficulty.Easy),
        C(MathsCategory,
            "Teach: what a prime number is, and why 1 isn't one.\n\n" +
            "Check: is 21 prime? What about 23? They have to explain why, not just answer yes or no.",
            Difficulty.Medium),

        // ── Science ────────────────────────────────────────────────────────────
        C(ScienceCategory,
            "Teach: why the sky is blue.\n\n" +
            "Check: using only what you just taught them, why is a sunset orange instead?",
            Difficulty.Hard),
        C(ScienceCategory,
            "Teach: the difference between weather and climate.\n\n" +
            "Check: \"it snowed today so climate change isn't real\" — what's wrong with that sentence?",
            Difficulty.Medium),
        C(ScienceCategory,
            "Teach: why we have seasons — it isn't distance from the sun.\n\n" +
            "Check: why are Australia's seasons opposite to ours at the same time of year?",
            Difficulty.Medium),
        C(ScienceCategory,
            "Teach: what actually happens when something dissolves.\n\n" +
            "Check: does the sugar disappear when it dissolves in tea? Where did it go?",
            Difficulty.Easy),
        C(ScienceCategory,
            "Teach: why astronauts float in the space station — it isn't \"no gravity up there.\"\n\n" +
            "Check: is there gravity at the space station's altitude? Why does everything still float?",
            Difficulty.Hard),

        // ── English ────────────────────────────────────────────────────────────
        C(EnglishCategory,
            "Teach: the difference between a metaphor and a simile.\n\n" +
            "Check: \"the exam was a mountain\" and \"the exam was like a mountain\" — which is which, and does it matter?",
            Difficulty.Easy),
        C(EnglishCategory,
            "Teach: what makes a sentence a run-on, and one way to fix one.\n\n" +
            "Check: give them a run-on sentence out loud — can they name the problem and fix it?",
            Difficulty.Medium),
        C(EnglishCategory,
            "Teach: the difference between its and it's.\n\n" +
            "Check: \"the dog wagged ___ tail\" — which one, and can they say the rule that got them there?",
            Difficulty.Easy),
        C(EnglishCategory,
            "Teach: what irony actually is — not just \"a coincidence\" or \"bad luck.\"\n\n" +
            "Check: is a fire station burning down ironic? What would make it actually ironic instead of just unlucky?",
            Difficulty.Hard),

        // ── History ────────────────────────────────────────────────────────────
        C(HistoryCategory,
            "Teach: why the Roman Empire is usually split into a Republic and an Empire — what changed.\n\n" +
            "Check: what's one thing that was true under the Republic that wasn't true under the Empire?",
            Difficulty.Hard),
        C(HistoryCategory,
            "Teach: what the printing press actually changed, beyond \"books got cheaper.\"\n\n" +
            "Check: name one effect it had on the world that had nothing to do with the price of books.",
            Difficulty.Medium),
        C(HistoryCategory,
            "Teach: the real reason for time zones — what problem they were invented to solve.\n\n" +
            "Check: why couldn't every town just keep using local noon, the way they used to?",
            Difficulty.Medium),
        C(HistoryCategory,
            "Teach: what the Silk Road actually was — not a single road, and not just for silk.\n\n" +
            "Check: name one non-silk thing that moved along it, in either direction.",
            Difficulty.Easy),

        // ── Geography ──────────────────────────────────────────────────────────
        C(GeographyCategory,
            "Teach: why it's colder at the top of a mountain than at sea level, even though it's closer to the sun.\n\n" +
            "Check: so why doesn't getting closer to the sun make it warmer?",
            Difficulty.Medium),
        C(GeographyCategory,
            "Teach: the difference between weather-driven erosion and a river carving a canyon.\n\n" +
            "Check: which one made the Grand Canyon, and roughly how long does that kind of thing take?",
            Difficulty.Medium),
        C(GeographyCategory,
            "Teach: why some countries are landlocked and why that actually matters economically.\n\n" +
            "Check: name one real disadvantage a landlocked country has that a coastal one doesn't.",
            Difficulty.Hard),
        C(GeographyCategory,
            "Teach: what makes a peninsula a peninsula — and name a real one.\n\n" +
            "Check: is an island a peninsula? What's the actual difference?",
            Difficulty.Easy),

        // ── Logic ──────────────────────────────────────────────────────────────
        C(LogicCategory,
            "Teach: the difference between correlation and causation, with an everyday example.\n\n" +
            "Check: \"ice cream sales and drowning deaths both rise in summer\" — does ice cream cause drowning? Why not?",
            Difficulty.Hard),
        C(LogicCategory,
            "Teach: what a logical fallacy is, using one specific example (not the word itself).\n\n" +
            "Check: give a new example of the SAME fallacy you just taught, from a totally different topic.",
            Difficulty.Hard),
        C(LogicCategory,
            "Teach: the difference between \"possible\" and \"likely.\"\n\n" +
            "Check: is it possible to flip a coin and get heads ten times in a row? Is it likely? Why isn't the answer the same for both?",
            Difficulty.Medium),
        C(LogicCategory,
            "Teach: what a counter-example is and why just one of them can break a rule.\n\n" +
            "Check: \"all birds can fly\" — what's the counter-example, and why does one exception really break the whole rule?",
            Difficulty.Easy),
    ];
}
