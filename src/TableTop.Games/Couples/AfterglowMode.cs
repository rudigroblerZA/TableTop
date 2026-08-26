using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Afterglow — an explicit intimacy game for two consenting adult partners,
/// built so that consent isn't a disclaimer stapled to the front but the
/// structure the whole game runs on.
///
/// Three things make the consent real rather than decorative:
///
///   1. THE OPENING RITUAL. The deck literally cannot be played past its first
///      card until you've both done the setup: agree a safeword that stops
///      everything instantly with no explanation owed, and each name at least
///      one thing that's off the table tonight. Nothing is assumed.
///
///   2. EVERY CARD IS AN INVITATION, NOT AN ORDER. Each prompt is offered, and
///      either partner can take it, soften it, trade it, or pass — always, for
///      any reason or none. "Pass" costs nothing and needs no justification;
///      passing is a completely normal move, not a failure. Enthusiasm is the
///      only green light. A "maybe", a flinch, a laugh-to-cover — all count as
///      no, and the game says so on the cards.
///
///   3. THE CLOSING RITUAL. The deck ends on aftercare — checking in, holding,
///      saying the kind thing — because how you land matters as much as
///      anything in between.
///
/// Four rising movements, and you stop wherever you both want to; there's no
/// obligation to reach the end. Warm Up · Turn Up · Heat · Undone. Explicit,
/// yes — but the point is closeness, and the game keeps handing you the wheel.
///
/// Adult (18+). For established, consenting partners only.
/// </summary>
public sealed class AfterglowMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <summary>Two people, together. The consent ritual, the aftercare and the register all assume it.</summary>
    public TableShape SuitableFor => TableShape.Couple;

    /// <inheritdoc />
    public override string Name => "Afterglow";
    /// <inheritdoc />
    public override string Description =>
        "An explicit intimacy game where consent is the mechanic — safeword and boundaries first, every card an invitation you opt into, aftercare to close.";

    /// <summary>Label for a card you both chose to do.</summary>
    public override string CompleteLabel => "Together";
    /// <summary>Label for passing — free, always, no reason needed.</summary>
    public override string SkipLabel => "Pass (always okay)";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Consent"] = "#26A69A",
            ["Warm Up"] = "#FFCA28",
            ["Turn Up"] = "#FFA726",
            ["Heat"] = "#EF5350",
            ["Undone"] = "#AD1457",
            ["Aftercare"] = "#7E57C2",
        };

    /// <summary>Deeper movements are worth more — but this isn't really about the score.</summary>
    /// <summary>
    /// The consent ritual opens and aftercare closes, regardless of the
    /// shuffle setting. These positions are a safety property of the deck, not
    /// a stylistic preference: a safeword has to be agreed before the cards it
    /// governs, and how a session lands matters as much as anything in it.
    /// </summary>
    public override IReadOnlyList<string> CategoriesPinnedToStart => ["Consent"];

    /// <inheritdoc cref="CategoriesPinnedToStart" />
    public override IReadOnlyList<string> CategoriesPinnedToEnd => ["Aftercare"];

    /// <inheritdoc />
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in Afterglow card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        AfterglowCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => AfterglowCardBank.All;
}

/// <summary>
/// Built-in card bank for Afterglow. Ordered deliberately: the consent ritual
/// is FIRST (and the deck is authored to be played in order, shuffle off), the
/// aftercare cards are LAST, and the explicit movements rise in between.
/// </summary>
public static class AfterglowCardBank
{
    /// <summary>All cards, in intended play order.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── CONSENT — the opening ritual (do not skip; the game rests on this) ──
        C("Before Anything — Your Safeword",
          "Stop here and choose a safeword together — one word either of you can say to halt EVERYTHING instantly, no explanation owed, no mood-killing guilt. Pick something you'd never say by accident (\"pineapple\" beats \"stop\"). Say it out loud now, twice, so it's real.\n\n" +
          "A tap-out gesture too, in case words are hard in the moment. When either of you uses it, the game is over the second it's said — that's the deal that makes everything else safe.",
          Difficulty.Easy),
        C("Before Anything — Tonight's Edges",
          "Take turns. Each of you names at least one thing that is OFF the table tonight — a no-go, a not-in-the-mood, a hard limit, anything. No reasons required, and \"I don't know yet\" is a valid edge that means don't go there.\n\n" +
          "Then each name one thing you're genuinely curious about or hoping for. Nothing here is a promise — it's a map, so nobody has to guess in the dark.",
          Difficulty.Easy),
        C("Before Anything — How We'll Check In",
          "Agree on a quick check-in you'll actually use: a simple \"colour?\" where green means more, yellow means slow down or stay here, red means stop. Either of you can call for a colour anytime, and either can offer one unasked.\n\n" +
          "Rule for the whole game: enthusiasm is the only yes. A maybe, a wince, a nervous laugh, silence — all mean no, and no is always free. Ready? Turn the next card only when you both are.",
          Difficulty.Easy),

        // ── WARM UP — clothed, close, building ───────────────────────────────
        M("Warm Up", "Slow Kiss",
          "Kiss — unhurried, the kind you'd give if you had nowhere else to be for an hour. Let it be the only thing happening.",
          Difficulty.Easy),
        M("Warm Up", "Where To Start",
          "Tell your partner one place you'd love to be kissed or touched first tonight — then let them start exactly there.",
          Difficulty.Easy),
        M("Warm Up", "Hands, Slowly",
          "One of you: run your hands over your partner — still clothed — everywhere you're welcome, slow enough to make them wait. Ask \"here?\" as you go.",
          Difficulty.Easy),
        M("Warm Up", "The Whisper",
          "Whisper one honest thing you want tonight into your partner's ear. Watch what it does to them.",
          Difficulty.Medium),
        M("Warm Up", "Undress One Layer",
          "Take one item of clothing off your partner — their choice which — and take your time. Stop there.",
          Difficulty.Medium),
        M("Warm Up", "Ask First",
              "Ask your partner one question — what would you like more of tonight? Listen to the whole answer before you touch them at all. Then begin there.",
          Difficulty.Easy),
        M("Warm Up", "One Thing",
              "Remove one item of your partner's clothing, slowly, and only that one. Then go back to kissing as though nothing had happened.",
          Difficulty.Medium),

        // ── TURN UP — skin, heat rising ──────────────────────────────────────
        M("Turn Up", "Trace",
          "With fingertips or mouth, trace a slow path from your partner's neck downward — pausing anywhere their breath changes. Follow the reactions, not a script.",
          Difficulty.Medium),
        M("Turn Up", "Tell Me What You Like",
          "Partner in front: describe out loud exactly how you like to be touched right now. Partner behind or beside: do precisely that, and adjust as they talk.",
          Difficulty.Medium),
        M("Turn Up", "Skin",
          "Undress each other the rest of the way — or as far as you both want — trading one piece at a time, kissing whatever you reveal.",
          Difficulty.Hard),
        M("Turn Up", "Hold The Line",
          "Kiss and touch anywhere you like above the waist for two full minutes — and agree not to go lower until the timer's up. Making each other wait is the game.",
          Difficulty.Hard),
        M("Turn Up", "Show Me",
          "Take your partner's hand and show them exactly how you like to be touched, guiding the pace and pressure. Let them take over when they've got it.",
          Difficulty.Hard),
        M("Turn Up", "Somewhere Unobvious",
              "Kiss somewhere your partner wouldn't have guessed — the inside of an elbow, the back of a knee, the base of the spine. Stay there longer than seems necessary.",
          Difficulty.Medium),
        M("Turn Up", "Hands Away",
              "One of you: hands behind your back, no touching allowed. The other: two minutes, do as you like. The one who can't touch says when the two minutes are up.",
          Difficulty.Hard),

        // ── HEAT — explicit, still opt-in every step ─────────────────────────
        M("Heat", "Down",
          "One partner: kiss a slow path downward and use your mouth on your partner however they like — check in with a \"colour?\" partway, keep going only on green.",
          Difficulty.Hard),
        M("Heat", "Hands On",
          "Touch each other where you most want to be touched, at the same time — watching each other's faces, matching the rhythm they set.",
          Difficulty.Hard),
        M("Heat", "Say It",
          "Tell your partner, out loud and explicit, exactly what you want next. If you both want it and you're both enthusiastic — do that.",
          Difficulty.Extreme),
        M("Heat", "Take The Lead",
          "One of you takes the lead completely for the next few minutes; the other simply receives and says yes, slow, or colour. Then swap if you both want to.",
          Difficulty.Extreme),
        M("Heat", "Together Now",
          "If you both want to and you've got whatever you need to be safe, come together however you like best — staying close, staying vocal about what feels good. Green means more; anything else means pause.",
          Difficulty.Extreme),
        M("Heat", "Say It While It Happens",
              "One partner: keep your hands or mouth busy. The other: say out loud what you want next, as it occurs to you. Instructions get followed exactly.",
          Difficulty.Hard),
        M("Heat", "Half Speed",
              "Whatever is happening, halve the speed of it. Stay at half speed for as long as you can both stand — then check in before you change anything.",
          Difficulty.Extreme),
        M("Undone", "Your Way",
          "No card knows the two of you better than you do. Set this one aside and do exactly what you both want — this is your night, not the deck's.",
          Difficulty.Extreme),
        M("Undone", "Again, Slower",
          "Whatever just happened — do a piece of it again, slower, with your eyes open and on each other.",
          Difficulty.Extreme),
        M("Undone", "The Unsaid Thing",
              "Tell your partner the thing you've been thinking about all evening and haven't said. Then decide together, out loud, whether tonight is the night for it. Either answer is a good one.",
          Difficulty.Extreme),
        M("Undone", "Nothing New",
              "No new ideas on this card. Do the thing that has always worked for the two of you — the old reliable — and give it your whole attention, as though it were the first time.",
          Difficulty.Extreme),
        M("Undone", "Stay Here",
              "Don't move on to anything else. Whatever this is, stay in it — no escalating, no switching — until one of you says otherwise.",
          Difficulty.Extreme),

        // ── AFTERCARE — the closing ritual (how you land matters) ────────────
        A("Come Back Down",
          "Stop, breathe, and come back to each other. Lie close, catch your breath together. No rush to move or talk — just be here, tangled up, for a minute.",
          Difficulty.Easy),
        A("The Kind Thing",
          "Tell your partner one genuine thing you loved about the last while — something they did, something you felt, something you're glad you shared.",
          Difficulty.Easy),
        A("Water & Warmth",
          "Get your partner whatever makes the landing soft — water, a blanket, a snack, a warm cloth. Take care of the body you just enjoyed.",
          Difficulty.Easy),
        A("Anything To Say?",
          "Gently check in: anything felt great and you'd want again? Anything you'd change, or that you'd rather not repeat? No defensiveness — just two people getting to know each other better. Then hold each other and let the game be over.",
          Difficulty.Medium),
        A("One Thing That Landed",
              "Tell each other one specific moment from tonight you'll still be thinking about tomorrow. Be precise — the exact moment, not the general idea.",
          Difficulty.Medium),
    ];

    private static ICard C(string title, string body, Difficulty d) =>
        StandardCard.Create(title,
            "<b>🛟 CONSENT — set this up before you play on</b>\n\n" + body,
            d, "Consent");

    private static ICard M(string category, string title, string body, Difficulty d) =>
        StandardCard.Create(title,
            "<b>" + Emoji(category) + " " + category.ToUpperInvariant() + "</b>\n\n" +
            body + "\n\n" +
            "<i>An invitation, never an order. Take it, soften it, trade it, or pass — pass is always free. Enthusiasm is the only yes; call \"colour?\" anytime.</i>",
            d, category);

    private static ICard A(string title, string body, Difficulty d) =>
        StandardCard.Create(title,
            "<b>💜 AFTERCARE</b>\n\n" + body,
            d, "Aftercare");

    private static string Emoji(string category) => category switch
    {
        "Warm Up" => "🌤️",
        "Turn Up" => "🔥",
        "Heat" => "🌶️",
        "Undone" => "💥",
        _ => "•",
    };
}
