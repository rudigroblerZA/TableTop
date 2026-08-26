using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Spy vs Spouse (18+) — a perfectly innocent couples conversation game…
/// running on top of a second, secret game of missions, tradecraft, and
/// counterintelligence. You are both spies. You are both targets.
///
/// THE MECHANIC — hidden information, the one thing no other couples deck
/// uses. Four kinds of cards:
///
///   🕵️ BRIEFING — read SILENTLY, to yourself only (you're holding the
///      device; that's your cover). It assigns you a secret mission to pull
///      off during ordinary conversation. Memorise it, say "understood",
///      and pass play on like nothing happened. Multiple missions can be
///      live at once. Never reveal an active mission.
///   💬 COVER STORY — ordinary, genuinely lovely couple prompts. This is the
///      traffic your missions hide inside. Answer them honestly; the
///      conversation is real even when the agenda isn't.
///   🔎 COUNTERINTEL — accusation checkpoints. Either of you may name the
///      mission you think the other is running. Caught red-handed: the
///      accuser scores 3 and the mission is burned. Wrong: the accused
///      scores 1 for flawless tradecraft, and says "I'm flattered."
///   📦 DEAD DROP — joint wildcards: shared rituals, trades, and one-time
///      protocols that belong to neither spy alone.
///
/// COMPLETING A MISSION: the moment you believe you've pulled it off,
/// announce "MISSION COMPLETE", reveal the card's text from memory, and let
/// your partner verify it actually happened. Verified = press "Mission
/// Complete" (harder missions score more). Disputed = the table's only
/// judge is the two of you; settle it like professionals, or with kissing.
///
/// House law: missions are flirtation with extra steps — never tricks at
/// your partner's expense. Any mission either of you dislikes on sight is
/// burned freely with "Burn It", no questions, no cost. Spies retire rich;
/// couples retire happy; this game wants both.
/// </summary>
public sealed class SpyVsSpouseMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Spy vs Spouse";
    /// <inheritdoc />
    public override string Description =>
        "An innocent conversation game hiding a second, secret one: silent mission briefings, cover stories, and counterintelligence accusations.";

    /// <summary>Label for the button that records a verified mission or played card.</summary>
    public override string CompleteLabel => "Mission Complete";
    /// <summary>Label for the button that burns a card.</summary>
    public override string SkipLabel => "Burn It";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Briefing"] = "#37474F",
            ["Cover Story"] = "#EC407A",
            ["Counterintel"] = "#FFA726",
            ["Dead Drop"] = "#26A69A",
        };

    /// <summary>Harder missions pay more when verified.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in spy card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        SpyVsSpouseCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => SpyVsSpouseCardBank.All;
}

/// <summary>Built-in card bank for Spy vs Spouse.</summary>
public static class SpyVsSpouseCardBank
{
    /// <summary>All spy cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── 🕵️ BRIEFING — silent missions (say "understood", pass play on) ──
        B("Within the next three Cover Story cards, get your partner to say the word 'always' — without ever saying it yourself.",
          Difficulty.Medium),
        B("Make your partner laugh, then immediately say, completely deadpan: 'That's classified.' The mission completes if they laugh AGAIN at that.",
          Difficulty.Easy),
        B("Casually touch their wrist, twice, during two DIFFERENT cards. Natural. Unremarkable. Professional.",
          Difficulty.Easy),
        B("Get them to compliment your eyes — without asking anything about your eyes, your face, or compliments.",
          Difficulty.Hard),
        B("Manoeuvre them into offering to get you a drink. Accept it graciously. Toast 'to the mission' — if they ask what mission, say 'exactly.'",
          Difficulty.Medium),
        B("Work the phrase 'when we're old' into an answer so naturally that they respond to the CONTENT, not the phrase.",
          Difficulty.Medium),
        B("Invent a small gesture (a tap, a sign, a look) and use it three times until your partner unconsciously mirrors it once. The mirror completes the mission.",
          Difficulty.Extreme),
        B("Hint — never state — that you have a tiny harmless secret about today. Mission completes when they ask about it directly. (Have one ready. You'll need it.)",
          Difficulty.Hard),
        B("Answer one Cover Story entirely in questions without your partner noticing before the card ends.",
          Difficulty.Extreme),
        B("Get your partner to move at least one seat, cushion, or metre closer to you — by environmental means only. No requests. Adjust lighting, share a screen, hold something worth leaning toward.",
          Difficulty.Hard),
        B("Slip the word 'Prague' into conversation so smoothly they don't question it. If they DO question it: 'I've said too much.' Mission burns, but you score a kiss for style.",
          Difficulty.Medium),
        B("During any answer your partner gives, hold eye contact until THEY look away first. Twice. Smile like you know something.",
          Difficulty.Easy),
        B("Get them to correctly guess something you're thinking of — by steering with three planted clues across three different cards, never by telling.",
          Difficulty.Extreme),
        B("Compliment them using a word you have never once used about them before. Mission completes if they visibly react to the new word.",
          Difficulty.Medium),

        // ── 💬 COVER STORY — the real conversation your missions hide in ────
        C("What's a tiny thing I do that you've never told me you noticed?", Difficulty.Easy),
        C("Describe our first kiss from YOUR side of it — what were you actually thinking?", Difficulty.Medium),
        C("If we got one guaranteed do-over of any single evening together, which would you replay — to fix, or to repeat?", Difficulty.Medium),
        C("What's something you were nervous to tell me once, that turned out completely fine?", Difficulty.Hard),
        C("Which of my habits will you find endearing when we're eighty?", Difficulty.Easy),
        C("What's a place we've never been that you've secretly imagined us in?", Difficulty.Medium),
        C("What did you think the FIRST time you saw me — the unedited version?", Difficulty.Medium),
        C("What's one thing I taught you without meaning to?", Difficulty.Hard),
        C("If our relationship had a codename, what would it be — and why that?", Difficulty.Easy),
        C("What's the most 'us' thing we do that nobody else would understand?", Difficulty.Easy),
        C("When did you last feel proud of me and not say so?", Difficulty.Hard),
        C("What would eighteen-year-old you say if they could see who you ended up with?", Difficulty.Medium),
        C("What's a small risk we should take together this month?", Difficulty.Medium),
        C("Which of my laughs is your favourite? (Yes, you know there's more than one.)", Difficulty.Easy),

        // ── 🔎 COUNTERINTEL — the accusation game ───────────────────────────
        A("SECURITY SWEEP: either agent may now accuse — name the exact mission you believe your partner is running. Caught: accuser scores 3, mission burned. Wrong: accused scores 1 and says 'I'm flattered.' No accusation? Both take a slow sip and eye each other with respect.",
          Difficulty.Medium),
        A("DOUBLE AGENT CHECK: each of you states ONE true thing and ONE invented thing you 'noticed the other doing suspiciously' tonight. Partner picks which is real. Correct picks score 1 each.",
          Difficulty.Medium),
        A("POLYGRAPH: your partner may ask you one direct yes/no question about your CURRENT missions. You may answer truthfully — or take the Fifth by kissing them instead. (The Fifth reveals nothing and is legally binding.)",
          Difficulty.Hard),
        A("MOLE HUNT: for the next two cards, BOTH of you narrate one suspicion out loud whenever the other does anything — anything — slightly deliberate. Paranoia is the point. Best unfounded accusation scores 1, partner's verdict.",
          Difficulty.Easy),
        A("BURN NOTICE: you may force your partner to burn ONE active mission unseen — they choose which, you never learn what it was. They score 1 as severance. Use this power wisely; it cannot be used on the same agent twice in a row.",
          Difficulty.Hard),
        A("EXIT INTERVIEW: both reveal ONE mission you completed earlier that was never detected. Undetected completions pay double their difficulty now. If neither has one — impressive vigilance; both score 1.",
          Difficulty.Extreme),

        // ── 📦 DEAD DROP — joint protocols ──────────────────────────────────
        D("Establish a COVER IDENTITY each: new name, occupation, and how you two 'met'. Stay in character for the next Cover Story card. Breaking character costs a kiss to the one who held.",
          Difficulty.Medium),
        D("SAFE HOUSE: agree, right now, on a code word either of you can say — tonight or any night — that means 'pause everything and just hold me for a minute.' Write it down. This card never expires.",
          Difficulty.Hard),
        D("ASSET EXCHANGE: trade one small possession you're each carrying or wearing, to be returned at the end of the game — or kept until tomorrow if the debrief goes well.",
          Difficulty.Easy),
        D("ENCRYPTED TRANSMISSION: whisper a message into your partner's ear in a made-up 'cipher' (any nonsense). They must decode it however they like and act on their interpretation. Their interpretation is now canon.",
          Difficulty.Medium),
        D("JOINT OPERATION: you have until the next Counterintel card to make each other laugh using ONLY eye contact and eyebrow work. First laugh loses the round and pays one kiss. Worth it.",
          Difficulty.Easy),
        D("THE HANDLER'S TOAST: invent, together, a two-line toast your agency would give at your retirement party. Deliver it in unison. Drink to the finest partnership in the service.",
          Difficulty.Medium),
    ];

    private static ICard B(string mission, Difficulty d) =>
        StandardCard.Create("Briefing",
            "<b>🕵️ EYES ONLY — read this SILENTLY. Do not read aloud.</b>\n\n" +
            "<b>YOUR MISSION:</b> " + mission + "\n\n" +
            "<i>Memorise it. Say 'understood.' Pass play on like nothing happened. " +
            "Announce 'MISSION COMPLETE' only when it's done — partner verifies from memory.</i>",
            d, "Briefing");

    private static ICard C(string prompt, Difficulty d) =>
        StandardCard.Create("Cover Story",
            "<b>💬 COVER STORY — answer honestly, out loud:</b>\n\n" + prompt + "\n\n" +
            "<i>The conversation is real, even when the agenda isn't. Stay alert.</i>",
            d, "Cover Story");

    private static ICard A(string text, Difficulty d) =>
        StandardCard.Create("Counterintel",
            "<b>🔎 COUNTERINTELLIGENCE:</b>\n\n" + text,
            d, "Counterintel");

    private static ICard D(string text, Difficulty d) =>
        StandardCard.Create("Dead Drop",
            "<b>📦 DEAD DROP — joint protocol:</b>\n\n" + text,
            d, "Dead Drop");
}
