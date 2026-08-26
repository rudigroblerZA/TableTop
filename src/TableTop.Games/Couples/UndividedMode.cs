using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Undivided — a turn-taking variation of Afterglow for two consenting adult
/// partners. Same consent-first spine (safeword and boundaries before you
/// play, aftercare to close), but a different core loop.
///
/// Where Afterglow has you both act together and escalate as a pair, Undivided
/// splits every round into two roles:
///
///   • THE RECEIVER does nothing but receive — and steers. They set the pace,
///     say more / slower / colour, redirect anything, and hold the safeword.
///     The luxury of the game is being paid complete attention with zero
///     obligation to reciprocate in the moment.
///   • THE GIVER focuses entirely on the Receiver. Every card is addressed to
///     the Giver, an invitation for how to attend to their partner. The Giver
///     watches, listens, and follows the Receiver's lead.
///
/// Play a stretch of cards with one person receiving, then SWAP — the game
/// tells you when. Because control lives with whoever is receiving, consent
/// stays exactly where it should the whole time: the person being touched is
/// always the person deciding.
///
/// Rising warmth in each role — Attention · Devotion · Worship — and you stop
/// wherever you both want. Explicit; the point is being wholly attended to.
///
/// Adult (18+). For established, consenting partners only.
/// </summary>
public sealed class UndividedMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <summary>Two people, together. The consent ritual, the aftercare and the register all assume it.</summary>
    public TableShape SuitableFor => TableShape.Couple;

    /// <inheritdoc />
    public override string Name        => "Undivided";
    /// <inheritdoc />
    public override string Description =>
        "The turn-taking variation of Afterglow — one of you receives undivided attention while the other gives, then swap. The receiver always steers.";

    /// <summary>Label for a card you did together.</summary>
    public override string CompleteLabel => "Given";
    /// <summary>Label for passing — free, always, no reason needed.</summary>
    public override string SkipLabel     => "Pass (always okay)";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Consent"]   = "#26A69A",
            ["Attention"] = "#FFCA28",
            ["Devotion"]  = "#FFA726",
            ["Worship"]   = "#AD1457",
            ["Swap"]      = "#42A5F5",
            ["Aftercare"] = "#7E57C2",
        };

    /// <summary>Deeper attention is worth more — but the score isn't the point.</summary>
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

    /// <summary>Returns the built-in Undivided card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        UndividedCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => UndividedCardBank.All;
}

/// <summary>
/// Built-in card bank for Undivided. Ordered for play: consent ritual first,
/// then a giving stretch, a Swap card, another stretch, and aftercare last.
/// </summary>
public static class UndividedCardBank
{
    /// <summary>All cards, in intended play order.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── CONSENT — the shared opening ritual (same spine as Afterglow) ────
        C("Before Anything — Your Safeword",
          "Choose a safeword together — one word that stops EVERYTHING instantly, no explanation owed. Something you'd never say by accident. Say it aloud now, twice, so it's real. Agree a tap-out gesture too, for when words are hard.\n\n" +
          "In this game the safeword lives with whoever is RECEIVING — the person being touched is always the person in charge.",
          Difficulty.Easy),
        C("Before Anything — Tonight's Edges",
          "Take turns: each name at least one thing that's OFF the table tonight — no reasons required, \"not sure\" means don't go there. Then each name one thing you'd love to receive.\n\n" +
          "Decide who receives first. It's a gift either way — there's no worse seat.",
          Difficulty.Easy),
        C("Before Anything — How The Receiver Steers",
          "The Receiver runs the pace with a simple call: green means more, yellow means slow down or stay right here, red means stop. The Giver checks \"colour?\" whenever they're unsure, and follows every steer without needing a reason.\n\n" +
          "Enthusiasm is the only yes. A maybe, a flinch, a nervous laugh all mean no — and no is always free. Turn the next card only when you both are ready.",
          Difficulty.Easy),

        // ── ATTENTION — warm, clothed-to-bare, the Giver follows the Receiver ─
        R("Attention", "Just Look",
          "Giver: take a slow moment to really look at your Receiver, and tell them one specific thing you love about looking at them right now. Then kiss them once, unhurried.",
          Difficulty.Easy),
        R("Attention", "Where They Want",
          "Receiver: name one place you'd love attention first. Giver: begin exactly there — hands or mouth, still mostly clothed — and stay until the Receiver says more or moves you on.",
          Difficulty.Easy),
        R("Attention", "Unhurried Hands",
          "Giver: run your hands over your Receiver, following their breath, going only where you're welcomed. Receiver: steer freely — slower, higher, stay. There's no rush and nowhere to get to.",
          Difficulty.Medium),

        // ── SWAP — trade Giver and Receiver ──────────────────────────────────
        SW("Swap Now",
          "Trade roles. Whoever was giving now receives — settle in and let yourself be attended to. Whoever was receiving now gives. Take a breath, check in with a smile, and carry on from the same warmth."),

        // ── ATTENTION — warm, clothed-to-bare, the Giver follows the Receiver ─
        R("Attention", "Undress Them",
          "Giver: take off one layer for your Receiver, slowly, kissing whatever you uncover. Receiver: green for more, yellow to linger. Stop there.",
          Difficulty.Medium),
        R("Attention", "Ask, Then Begin",
              "Giver: ask your Receiver where they'd like to be touched first. Do exactly that, and only that, for a slow minute.",
          Difficulty.Easy),
        R("Attention", "Read the Breath",
              "Giver: touch your Receiver slowly and watch nothing but their breathing. Wherever it changes, stay there. Let their body do the talking.",
          Difficulty.Medium),

        // ── SWAP — trade Giver and Receiver ──────────────────────────────────
        SW("Swap Again",
          "Trade back — or trade forward. The one who just gave now gets to receive. Take your time settling into it; being received well is the whole point."),

        // ── DEVOTION — skin, the Giver serves the Receiver ───────────────────
        R("Devotion", "Tell Me, I'll Do It",
          "Receiver: describe out loud exactly how you like to be touched right now. Giver: do precisely that, adjusting as they talk, asking nothing in return.",
          Difficulty.Medium),
        R("Devotion", "Slow Trace",
          "Giver: trace a slow path down your Receiver with fingertips or mouth, pausing wherever their breath catches. Follow the reactions, not a plan. Receiver: steer.",
          Difficulty.Hard),
        R("Devotion", "Their Hands, Your Guide",
          "Receiver: take the Giver's hand and show them exactly the pressure and pace you like, then let them take over. Giver: keep doing it just like that.",
          Difficulty.Hard),
        R("Devotion", "Make Them Wait",
          "Giver: attend to your Receiver everywhere but where they most want it, for two full minutes — building the wait. Receiver: you set when the wait ends.",
          Difficulty.Hard),
        R("Devotion", "Nothing Back",
              "Giver: this whole card is your hands only. Receiver: you're not allowed to reciprocate — your only job is to receive it and say what's working.",
          Difficulty.Medium),
        R("Devotion", "Past The Point",
              "Receiver: name the one thing you like most. Giver: do it, and keep doing it well past the point you'd normally move on. Only the Receiver decides when it's done.",
          Difficulty.Hard),

        // ── SWAP — trade Giver and Receiver ──────────────────────────────────
        SW("Swap On Their Word",
              "The Receiver decides when. Whenever they say swap, you swap — mid-anything. Until they say it, nothing changes."),

        // ── WORSHIP — explicit, the Receiver still holds every yes ───────────
        R("Worship", "Undivided",
          "Giver: use your mouth on your Receiver however they like, checking \"colour?\" as you go and staying only on green. This is entirely about them; there's nothing you need back.",
          Difficulty.Hard),
        R("Worship", "Exactly What You Want",
          "Receiver: say out loud, explicit, exactly what you want next. Giver: if you're both enthusiastic, give them precisely that — their pleasure is the only agenda.",
          Difficulty.Extreme),
        R("Worship", "All The Way",
          "Receiver: if you want to be taken all the way like this, say so — and how. Giver: follow it exactly, staying close and vocal, green means more, anything else means pause.",
          Difficulty.Extreme),
        R("Worship", "Your Way",
          "No card knows the two of you better than you do. Receiver: ask for anything. Giver: give it, if you both want to. Set the deck aside — this round is theirs.",
          Difficulty.Extreme),
        R("Worship", "Slower Than They Want",
              "Giver: use your mouth on your Receiver, and go slower than they'd like. Check \"colour?\" as you go, and stay only on green.",
          Difficulty.Hard),

        // ── SWAP — trade Giver and Receiver ──────────────────────────────────
        SW("Swap Halfway",
              "Stop at the halfway point of whatever's happening and trade roles. The new Giver picks up exactly where the last one left off — same pace, same place."),

        // ── WORSHIP — explicit, the Receiver still holds every yes ───────────
        R("Worship", "Until They Say",
              "Giver: this ends when the Receiver says it ends — not before, not after. Receiver: say it out loud when you're ready, and take as long as you want getting there.",
          Difficulty.Extreme),
        R("Worship", "Hands And Mouth",
          "Giver: hands and mouth, both, and the Receiver says which goes where. Change the instant they ask, and don't get clever — do the obvious thing they asked for.",
          Difficulty.Hard),
        R("Worship", "Every Inch",
          "Giver: start at your Receiver's mouth and work down, slowly, missing nothing. They say where to linger and what to skip, and skipped is skipped — no doubling back to try your luck.",
          Difficulty.Hard),
        R("Worship", "Don't Stop",
          "Receiver: when it's right, say \"don't stop\" — and the Giver changes nothing. Not the pace, not the pressure, not the place. Giver: hold exactly that until they say otherwise.",
          Difficulty.Extreme),
        R("Worship", "Watch Me",
          "Receiver: keep your eyes open. Giver: keep yours on theirs, whatever else you're doing. Either of you may close them at any point, and that's its own answer.",
          Difficulty.Extreme),
        R("Worship", "Again, If You Can",
          "Receiver: if you want it a second time, ask. Giver: give it slower than the first, and let them set every part of the pace.",
          Difficulty.Extreme),

        // ── AFTERCARE — the closing ritual, together again ───────────────────
        A("Come Back Together",
          "However you finished, come back to each other — lie close, breathe together, no rush to move or speak. You're both here now, no roles, just tangled up.",
          Difficulty.Easy),
        A("What You Loved",
          "Each of you: tell the other one genuine thing you loved — as giver or receiver — about the last while. Say the specific thing.",
          Difficulty.Easy),
        A("Take Care",
          "Get each other whatever makes the landing soft — water, a blanket, a warm cloth, a snack. Tend to the person who just spoiled you, and the one who received.",
          Difficulty.Easy),
        A("Anything To Say?",
          "Gently check in: anything you'd want again? Anything you'd change, or a role you'd like more of next time? No defensiveness — just learning each other. Then hold on and let the game be over.",
          Difficulty.Medium),
        A("Say What They Gave You",
              "Whoever received: tell your Giver one specific thing they did that you'll remember. Whoever gave: tell them what you liked about giving it.",
          Difficulty.Easy),
    ];

    private static ICard C(string title, string body, Difficulty d) =>
        StandardCard.Create(title,
            "<b>🛟 CONSENT — set this up before you play on</b>\n\n" + body,
            d, "Consent");

    private static ICard R(string category, string title, string body, Difficulty d) =>
        StandardCard.Create(title,
            "<b>" + Emoji(category) + " " + category.ToUpperInvariant() + "  ·  the receiver steers</b>\n\n" +
            body + "\n\n" +
            "<i>An invitation, never an order. The receiver holds every yes; pass is always free, and enthusiasm is the only green light. Call \"colour?\" anytime.</i>",
            d, category);

    private static ICard SW(string title, string body) =>
        StandardCard.Create(title,
            "<b>🔄 SWAP</b>\n\n" + body,
            Difficulty.Easy, "Swap");

    private static ICard A(string title, string body, Difficulty d) =>
        StandardCard.Create(title,
            "<b>💜 AFTERCARE</b>\n\n" + body,
            d, "Aftercare");

    private static string Emoji(string category) => category switch
    {
        "Attention" => "🌤️",
        "Devotion"  => "🔥",
        "Worship"   => "🌶️",
        _           => "•",
    };
}
