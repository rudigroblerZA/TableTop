using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Domain.Cards;

namespace TableTop.Games.Couples;

/// <summary>
/// Day One (18+) — a 21-day campaign, not a one-evening game. Exactly one
/// card unlocks per real calendar day; the rest of the deck stays sealed
/// until tomorrow, no matter how curious you are.
///
/// Three phases, one card apiece per day:
///   Days 1–7   SPARK   — light, playful, a minute or two each.
///   Days 8–14  WARMTH  — a little closer, a little more said out loud.
///   Days 15–21 EMBERS  — the deep end. By now you've built up to it.
///
/// Miss a day? Nothing is lost — it's simply waiting for you, in order,
/// whenever you come back. The only thing you can't do is rush ahead.
///
/// This is the one couples game on the shelf that isn't about a single
/// night — it's about the appointment you keep with each other for three
/// weeks straight. Consider setting a daily reminder for whenever suits you both.
/// </summary>
public sealed class DayOneMode : IGameMode, IDailyDeckProvider
{
    /// <inheritdoc />
    public string Name => "Day One";

    /// <inheritdoc />
    public string Description =>
        "A 21-day campaign: one card unlocks per real day, Spark to Warmth to Embers. Miss a day and it just waits for you.";

    /// <inheritdoc />
    public IReadOnlyList<ICard> GetDailyDeck() =>
        DayOneCardBank.All;
}

/// <summary>Built-in 21-day campaign deck for Day One.</summary>
public static class DayOneCardBank
{
    // The three phases double as the cards' Category, so they are named
    // constants for the same reason every other bank's categories are: the
    // string appears once per card, and a typo in one of them would file a
    // day under a fourth phase that nothing else knows about.
    internal const string SparkPhase = "Spark";
    internal const string WarmthPhase = "Warmth";
    internal const string EmbersPhase = "Embers";

    /// <summary>All 21 days, in strict order — index 0 is Day 1.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── SPARK (Days 1–7) ─────────────────────────────────────────────────
        D(1, SparkPhase, "Tell them one thing about today, right now, that you haven't mentioned yet — however small.", Difficulty.Easy),
        D(2, SparkPhase, "Send (or say) one compliment about something that isn't their appearance.", Difficulty.Easy),
        D(3, SparkPhase, "Recreate the exact way you greeted each other the very first time you met.", Difficulty.Easy),
        D(4, SparkPhase, "Ask them: 'what's one thing you're looking forward to this week?' Actually listen to the answer.", Difficulty.Easy),
        D(5, SparkPhase, "Give a twenty-second hug — no phones, no talking, just stay in it a beat longer than usual.", Difficulty.Easy),
        D(6, SparkPhase, "Tell them your favourite thing they said to you in the last month. If you can't remember one, that's tomorrow's homework — pay attention today.", Difficulty.Medium),
        D(7, SparkPhase, "Week one, done. Each say one word for how this week felt. Just one word each. Compare.", Difficulty.Easy),

        // ── WARMTH (Days 8–14) ───────────────────────────────────────────────
        D(8, WarmthPhase, "Tell them one small worry you've been carrying that you haven't said out loud yet.", Difficulty.Medium),
        D(9, WarmthPhase, "Describe, honestly, the version of your future you daydream about that includes them.", Difficulty.Medium),
        D(10, WarmthPhase, "Hold hands and don't let go for the next ten minutes, whatever you're doing.", Difficulty.Medium),
        D(11, WarmthPhase, "Tell them one thing you've changed your mind about because of them.", Difficulty.Hard),
        D(12, WarmthPhase, "Slow dance to one song, foreheads touching, no talking until it ends.", Difficulty.Medium),
        D(13, WarmthPhase, "Tell them about a moment you felt truly proud of them — one they might not know you noticed.", Difficulty.Hard),
        D(14, WarmthPhase, "Two weeks in. Write each other one sentence — 'the thing I love most about these two weeks was…' — and trade papers without discussion.", Difficulty.Hard),

        // ── EMBERS (Days 15–21) ──────────────────────────────────────────────
        D(15, EmbersPhase, "Whisper the thing you've been wanting to say all week but kept putting off.", Difficulty.Hard),
        D(16, EmbersPhase, "Trace one word on their back with your fingertip — something true about how you feel right now. They guess it, or you tell them.", Difficulty.Hard),
        D(17, EmbersPhase, "Kiss them somewhere you haven't kissed them in a while. Take your time choosing.", Difficulty.Extreme),
        D(18, EmbersPhase, "Tell them, in detail, the moment this campaign started to feel different for you — if it did.", Difficulty.Extreme),
        D(19, EmbersPhase, "Plan the next hour together, out loud, starting from right now. No vetoes — just building on what the other suggests.", Difficulty.Extreme),
        D(20, EmbersPhase, "Tell them the thing about tonight, specifically, that you're hoping happens.", Difficulty.Extreme),
        D(21, EmbersPhase, "Day 21. Each finish this sentence out loud: 'starting this with you was worth it because…' Then decide together — is there a Day 22?", Difficulty.Extreme),
    ];

    private static ICard D(int day, string phase, string prompt, Difficulty difficulty) =>
        StandardCard.Create(
            $"Day {day}",
            "<b>Day " + day + " — " + phase + "</b>\n\n" + prompt,
            difficulty, phase);
}
