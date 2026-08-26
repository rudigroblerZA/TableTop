using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Domain.Cards;

namespace TableTop.Games.Fun;

/// <summary>
/// Slang Check (Millionaire format) — the hot-seat quiz of internet and dating
/// slang, for anyone who's ever nodded along to a term they didn't actually
/// know. Fifteen rungs, mainstream vocabulary only — the kind covered by
/// newspaper trend pieces and group-chat arguments about what a word "really"
/// means.
///
/// Play it like Millionaire: one person in the hot seat, someone else hosting
/// (dramatic pauses strongly encouraged), lifelines as normal.
///
/// Tone contract: every term here is general-audience internet/dating-culture
/// vocabulary, defined accurately and worded for laughs — nothing explicit,
/// nothing that needs an age gate.
/// </summary>
public sealed class SlangCheckMode : IGameMode, IQuestionBankProvider
{
    /// <inheritdoc />
    public string Name => "Slang Check";

    /// <inheritdoc />
    public string Description =>
        "Hot-seat quiz of internet and dating slang — 15 rungs from 'rizz' to the deep cuts. Do you actually know what people are saying?";

    /// <inheritdoc />
    public IReadOnlyList<MultipleChoiceCard> GetQuestionBank() => SlangCheckQuestionBank.All;
}

/// <summary>Built-in question bank for Slang Check.</summary>
public static class SlangCheckQuestionBank
{
    /// <summary>All questions; the controller ladders them by difficulty.</summary>
    public static IReadOnlyList<MultipleChoiceCard> All { get; } = Build();

    private static IReadOnlyList<MultipleChoiceCard> Build() =>
    [
        // ── EASY (rungs 1–5): everyone's heard these by now ──────────────────
        MultipleChoiceCard.Create(
            "'No cap' means…",
            "No lie — I'm being completely serious",
            "No hat, an outfit note",
            "Nothing left in the budget",
            "A promise with no time limit",
            AnswerLabel.A, Difficulty.Easy, "Slang Check"),
        MultipleChoiceCard.Create(
            "Calling something 'mid' means…",
            "It happened at midday",
            "Mediocre — distinctly unimpressive, whatever it is",
            "It's exactly average height",
            "It's halfway finished",
            AnswerLabel.B, Difficulty.Easy, "Slang Check"),
        MultipleChoiceCard.Create(
            "'Bet' as a one-word reply means…",
            "I'm placing a wager on this",
            "I disagree strongly",
            "Sure / agreed / it's confirmed",
            "I need more information first",
            AnswerLabel.C, Difficulty.Easy, "Slang Check"),
        MultipleChoiceCard.Create(
            "A 'glow up' is…",
            "A dramatic, usually flattering transformation over time",
            "A phone's screen brightness setting",
            "The moment a relationship becomes official",
            "A firework display",
            AnswerLabel.A, Difficulty.Easy, "Slang Check"),
        MultipleChoiceCard.Create(
            "Something 'sus' is…",
            "Extremely trustworthy",
            "Suspicious — it doesn't quite add up",
            "Sustainably sourced",
            "Guaranteed to succeed",
            AnswerLabel.B, Difficulty.Easy, "Slang Check"),
        MultipleChoiceCard.Create(
            "'Bussin' describes…",
            "Something (usually food) that's exceptionally good",
            "A very crowded bus route",
            "An argument that's just ended",
            "Something falling apart",
            AnswerLabel.A, Difficulty.Easy, "Slang Check"),

        // ── MEDIUM (rungs 6–10): app-literate territory ──────────────────────
        MultipleChoiceCard.Create(
            "Being 'left on read' means…",
            "Your message was seen and deliberately not replied to",
            "You were the last one to leave a book club",
            "Your text failed to send",
            "You've been added to a reading list",
            AnswerLabel.A, Difficulty.Medium, "Slang Check"),
        MultipleChoiceCard.Create(
            "A 'red flag' in dating means…",
            "A dealbreaker warning sign about someone's behaviour",
            "Someone's favourite colour is red",
            "A person who works in finance",
            "An overly enthusiastic first date",
            AnswerLabel.A, Difficulty.Medium, "Slang Check"),
        MultipleChoiceCard.Create(
            "'Delulu' means…",
            "Delightfully lucky",
            "Delusional — believing something wildly optimistic despite the evidence",
            "A type of dessert trending online",
            "Deleting your dating app, briefly, out of shame",
            AnswerLabel.B, Difficulty.Medium, "Slang Check"),
        MultipleChoiceCard.Create(
            "Calling someone an 'NPC' (outside gaming) means…",
            "They're acting without any personality or original thought — background-character energy",
            "They're new to the friend group",
            "They never post on social media",
            "They're the most popular person in the room",
            AnswerLabel.A, Difficulty.Medium, "Slang Check"),
        MultipleChoiceCard.Create(
            "'Main character energy' means…",
            "Acting like the protagonist of your own life — confident, deliberate, a little dramatic",
            "Only speaking to the most important person at a party",
            "Refusing to share your phone charger",
            "Being the one who orders for the table",
            AnswerLabel.A, Difficulty.Medium, "Slang Check"),
        MultipleChoiceCard.Create(
            "A 'ratio' (getting ratio'd) means…",
            "Your reply got far more engagement than the original post — a public online loss",
            "You split the bill unevenly",
            "You're dating two people at a 2:1 ratio",
            "A cooking measurement went wrong",
            AnswerLabel.A, Difficulty.Medium, "Slang Check"),

        // ── HARD (rungs 11–14): the deep cuts ────────────────────────────────
        MultipleChoiceCard.Create(
            "'Beige flag' behaviour is…",
            "Not bad, just bafflingly boring — a quirk you'd screenshot for the group chat",
            "A sign of exceptional wealth",
            "A relationship that moves too fast",
            "An unresolved argument still hanging over you both",
            AnswerLabel.A, Difficulty.Hard, "Slang Check"),
        MultipleChoiceCard.Create(
            "'Rizz' refers to…",
            "Effortless charisma — the ability to charm someone without visibly trying",
            "A carbonated drink brand",
            "A dating app's swipe feature",
            "The awkward silence after a joke falls flat",
            AnswerLabel.A, Difficulty.Hard, "Slang Check"),
        MultipleChoiceCard.Create(
            "'Brain rot' describes…",
            "The mental fog from consuming too much low-value online content",
            "Forgetting an anniversary",
            "A slow internet connection",
            "Studying too hard before an exam",
            AnswerLabel.A, Difficulty.Hard, "Slang Check"),
        MultipleChoiceCard.Create(
            "'Girl dinner' / 'boy dinner' refers to…",
            "An improvised, often mismatched meal of snacks — minimal effort, maximum vibes",
            "A formal first date at a nice restaurant",
            "Splitting the cooking duties in a household",
            "A meal eaten standing at the fridge, historically",
            AnswerLabel.A, Difficulty.Hard, "Slang Check"),
        MultipleChoiceCard.Create(
            "Someone being 'chronically online' means…",
            "Their references, humour, and reactions are shaped almost entirely by internet culture",
            "They have a slow-loading phone",
            "They work in IT support",
            "They never miss a livestream",
            AnswerLabel.A, Difficulty.Hard, "Slang Check"),
        MultipleChoiceCard.Create(
            "'Understood the assignment' means…",
            "Someone executed something — an outfit, a performance, a vibe — perfectly on brief",
            "A student turned in homework on time",
            "Someone followed instructions too literally",
            "A group project was divided fairly",
            AnswerLabel.A, Difficulty.Hard, "Slang Check"),

        // ── EXTREME (rung 15): the million-point question ────────────────────
        MultipleChoiceCard.Create(
            "'Rizzlementary' would jokingly describe…",
            "A documentary explaining someone's inexplicable charm, forensic-style",
            "An entry-level charisma certification",
            "A slang term with no real meaning, invented for this exact question",
            "The academic study of TikTok trends",
            AnswerLabel.A, Difficulty.Extreme, "Slang Check"),
        MultipleChoiceCard.Create(
            "A 'situationship' is best defined as…",
            "A romantic arrangement with all the feelings and none of the labels",
            "A relationship that only exists during specific situations, like holidays",
            "Any relationship shorter than three months",
            "A friendship that's secretly a business partnership",
            AnswerLabel.A, Difficulty.Extreme, "Slang Check"),
        MultipleChoiceCard.Create(
            "'Touch grass' is advice meaning…",
            "Step away from the screen and reconnect with real life for a bit",
            "Go gardening as a hobby",
            "Calm down during an argument",
            "Get more exercise specifically outdoors",
            AnswerLabel.A, Difficulty.Extreme, "Slang Check"),
        MultipleChoiceCard.Create(
            "'The ick' refers to…",
            "A sudden, often irrational wave of disgust that instantly kills attraction",
            "A stomach bug going around a friend group",
            "The awkward feeling before asking someone out",
            "A skincare routine gone wrong",
            AnswerLabel.A, Difficulty.Extreme, "Slang Check"),
    ];
}
