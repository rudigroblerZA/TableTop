using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// The Long Game — the quiet, grown-up couples deck about noticing, naming,
/// and keeping the good things. Not a party game and not a spicy one: it's the
/// one you reach for on an anniversary, a hard week, or a slow Sunday, when the
/// point is to say the specific true thing out loud.
///
/// Every card asks for <em>specificity</em> — never "what do you love about
/// me" but "name the exact moment this week you were glad it was me." Vague
/// answers are gently disallowed by the card text itself; the whole mode is a
/// trainer for the kind of attention long relationships run on.
///
/// The Keeper mechanic: when an answer lands — when someone says something
/// worth remembering — either partner calls "Keeper," and you write it down
/// (a note, a shared list, the back of a receipt). Over many sessions the
/// keepers become a record of the relationship in its own words. "Revealed"
/// scores the round; there's no losing here, only more kept.
///
/// Four movements, roughly escalating in vulnerability:
///   Noticing   — small, recent, concrete admiration
///   Gratitude  — the harder-to-say thank-yous
///   Weathered  — what you've survived together, named honestly
///   Vows       — forward-facing promises, small and real
/// </summary>
public sealed class TheLongGameMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <summary>Two people, together. The consent ritual, the aftercare and the register all assume it.</summary>
    public TableShape SuitableFor => TableShape.Couple;

    /// <inheritdoc />
    public override string Name => "The Long Game";
    /// <inheritdoc />
    public override string Description =>
        "The quiet deck about noticing and keeping the good things — specific admiration, real thank-yous, honest promises.";

    /// <summary>Label for a completed exchange.</summary>
    public override string CompleteLabel => "Kept";
    /// <summary>Label for passing a card.</summary>
    public override string SkipLabel => "Not tonight";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Noticing"] = "#66BB6A",
            ["Gratitude"] = "#FFCA28",
            ["Weathered"] = "#78909C",
            ["Vows"] = "#EC407A",
        };

    /// <summary>Deeper movements are worth more — but this is a game you can't lose.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        TheLongGameCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => TheLongGameCardBank.All;
}

/// <summary>Built-in card bank for The Long Game.</summary>
public static class TheLongGameCardBank
{
    /// <summary>All cards, ordered by movement.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── NOTICING — small, recent, concrete ───────────────────────────────
        N("Noticing", "This Week",
          "Name one specific moment THIS WEEK you were glad it was them beside you. Not a general feeling — the actual moment.",
          Difficulty.Easy),
        N("Noticing", "The Unglamorous Thing",
          "Thank them for something deeply unglamorous they do that keeps your shared life running. Be specific enough that they know you've noticed.",
          Difficulty.Easy),
        N("Noticing", "A Way They Move",
          "Describe one small physical habit of theirs — how they laugh, hold a mug, sleep — that you'd miss if it were gone.",
          Difficulty.Easy),
        N("Noticing", "Better Because",
          "Finish honestly: 'I'm better at ___ than I used to be, and it's partly because of you.' Name the actual skill or trait.",
          Difficulty.Medium),
        N("Noticing", "The Thing You Almost Missed",
          "Tell them one good thing about themselves they might not know you noticed.",
          Difficulty.Medium),
        N("Noticing", "In A Room",
          "Describe how you feel when they walk into a room you're already in. One honest sentence.",
          Difficulty.Easy),
        N("Noticing", "Their Competence",
          "Name a thing they are genuinely, impressively good at — and a specific time you watched them do it well.",
          Difficulty.Medium),
        N("Noticing", "The Small Repair",
          "Name one small thing they did this week that quietly fixed something — a job, a mood, a whole day.",
          Difficulty.Easy),
        N("Noticing", "In A Room",
          "Name something they do in company that you're proud to stand next to.",
          Difficulty.Easy),
        N("Noticing", "The Change",
          "Name one way they've changed for the better since you met — and say what you think it cost them.",
          Difficulty.Medium),

        // ── GRATITUDE — the harder-to-say thank-yous ─────────────────────────
        G("Gratitude", "The Sacrifice",
          "Name something they gave up, changed, or carried for your sake — and thank them for it out loud, plainly.",
          Difficulty.Hard),
        G("Gratitude", "When You Were Hard To Love",
          "Think of a stretch when you were difficult to be with. Thank them for staying, and name what you know it cost them.",
          Difficulty.Extreme),
        G("Gratitude", "The Ordinary Loyalty",
          "Thank them for a small, repeated loyalty you've come to count on without saying so.",
          Difficulty.Medium),
        G("Gratitude", "Something You've Never Said",
          "Say one true thank-you you've meant for a long time but never actually put into words.",
          Difficulty.Extreme),
        G("Gratitude", "The Rescue",
          "Recall a time they quietly rescued a day, a plan, or you — and never made it a big deal. Make it one now.",
          Difficulty.Hard),
        G("Gratitude", "For Your People",
          "Thank them for how they treat someone YOU love — your family, a friend, a pet, your past self.",
          Difficulty.Medium),
        G("Gratitude", "The Unasked Favour",
          "Thank them for something they do that you have never once had to ask for.",
          Difficulty.Medium),
        G("Gratitude", "The Cost",
          "Name something in your life that is measurably better and exists only because of them.",
          Difficulty.Hard),
        G("Gratitude", "The Person You Became",
          "Name one way being with them made you a better person — and be specific about how it actually happened.",
          Difficulty.Extreme),

        // ── WEATHERED — what you've survived, named honestly ─────────────────
        W("Weathered", "The Hard Season",
          "Name a genuinely hard season you came through together. What did they do then that you still carry?",
          Difficulty.Hard),
        W("Weathered", "The Old Fight",
          "Recall an argument you're both past now. What did you learn about loving them from the other side of it?",
          Difficulty.Hard),
        W("Weathered", "Proof",
          "Tell them about a moment that became your private proof that this is real and worth it.",
          Difficulty.Hard),
        W("Weathered", "What Changed In You",
          "Name one way loving them has changed who you are — for the better — that you didn't expect going in.",
          Difficulty.Extreme),
        W("Weathered", "The Almost",
          "Was there a moment things could have gone another way? Name it honestly, and why you're glad they didn't.",
          Difficulty.Extreme),
        W("Weathered", "Steady",
          "Describe what they're like in a crisis, and what it means to have them beside you when things go wrong.",
          Difficulty.Medium),
        W("Weathered", "Carried",
          "Name a stretch when they carried more than their share. Say that you noticed, and say what it looked like from where you stood.",
          Difficulty.Medium),
        W("Weathered", "The Night It Turned",
          "Name a night that could have gone badly and didn't. What did one of you do?",
          Difficulty.Hard),
        W("Weathered", "The Argument We Survived",
          "Name an argument you're glad you had. What did it settle that needed settling?",
          Difficulty.Extreme),

        // ── VOWS — small, real, forward-facing ───────────────────────────────
        V("Vows", "One Small Promise",
          "Make one small, specific, keepable promise for the coming month. Not grand — real. Say it as a promise.",
          Difficulty.Medium),
        V("Vows", "I'll Keep Doing",
          "Name one good thing you already do for them that you promise to keep doing, on purpose, even when it's hard.",
          Difficulty.Medium),
        V("Vows", "The Repair",
          "Name one thing you'll try to do better — and ask them, genuinely, how they'd like you to.",
          Difficulty.Hard),
        V("Vows", "In Ten Years",
          "Describe one thing you hope is still true about the two of you in ten years — and one thing you'll do to protect it.",
          Difficulty.Hard),
        V("Vows", "The Standing Invitation",
          "Offer them one standing 'you can always…' — a permission or a promise they can lean on anytime. Mean it.",
          Difficulty.Extreme),
        V("Vows", "Choose Again",
          "Knowing everything you now know, tell them plainly that you'd choose them again — and name the clearest reason why.",
          Difficulty.Hard),
        V("Vows", "A Standing Appointment",
          "Promise one recurring thing — weekly or monthly — that belongs to the two of you and nobody else. Name the day out loud.",
          Difficulty.Medium),
        V("Vows", "The Thing I'll Stop",
          "Name one thing you'll stop doing, starting now, because you know what it costs them. Say it as a promise, not an intention.",
          Difficulty.Hard),
        V("Vows", "In Ten Years",
          "Say one thing you promise will still be true of the two of you in ten years — then say what it'll take to keep it true.",
          Difficulty.Hard),
    ];

    // Each movement gets its own emoji header; all share the "specific, out loud" ethos.
    private static ICard N(string cat, string title, string prompt, Difficulty d) => Make("🌱", cat, title, prompt, d);
    private static ICard G(string cat, string title, string prompt, Difficulty d) => Make("🕯️", cat, title, prompt, d);
    private static ICard W(string cat, string title, string prompt, Difficulty d) => Make("⚓", cat, title, prompt, d);
    private static ICard V(string cat, string title, string prompt, Difficulty d) => Make("💍", cat, title, prompt, d);

    private static ICard Make(string emoji, string category, string title, string prompt, Difficulty d) =>
        StandardCard.Create(
            title,
            "<b>" + emoji + " " + category.ToUpperInvariant() + "</b>\n\n" +
            prompt + "\n\n" +
            "<i>Be specific — the specific thing is the whole gift. If it lands, either of you can call \"Keeper\" and write it down.</i>",
            d, category);
}
