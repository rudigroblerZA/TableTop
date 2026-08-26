using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Slow Burn (18+) — a game of sealed promises and beautiful almosts.
///
/// The mechanic is ANTICIPATION. You'll need paper, a pen, and a bowl (the
/// POT). Four kinds of cards:
///
///   ✉️ IOU — write something small and secret, fold it, drop it in the pot.
///      Nobody reads it yet. The pot just… sits there. Growing.
///   🫧 ALMOST — do the thing, but stop just before the best part. Yes,
///      really. That's the card. The card is a menace.
///   📜 HOUSE RULE — a rule that stays in force until the next House Rule
///      replaces it. The game slowly rewrites the room.
///   🔓 CASH IN — draw one folded IOU from the pot and redeem it now.
///      Author's handwriting, redeemer's timing.
///
/// THE POT RITUAL: when the game ends, open everything left in the pot
/// together, one at a time, alternating who unfolds. Redeem them in whatever
/// order you negotiate. Negotiation is part of the game. So is tonight.
///
/// House law: anything written can be redeemed as written or renegotiated —
/// enthusiasm is the only valid currency, and either of you can always trade
/// any IOU for a kiss and a rain check. The pot keeps no grudges.
/// </summary>
public sealed class SlowBurnMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <summary>Two people, together. The consent ritual, the aftercare and the register all assume it.</summary>
    public TableShape SuitableFor => TableShape.Couple;

    /// <inheritdoc />
    public override string Name => "Slow Burn";
    /// <inheritdoc />
    public override string Description =>
        "Sealed promises, beautiful almosts, and a pot of folded IOUs that pays out when the game ends. Bring paper. Bring patience.";

    /// <summary>Label for the button that records a played card.</summary>
    public override string CompleteLabel => "Delivered";
    /// <summary>Label for the button that passes on a card.</summary>
    public override string SkipLabel => "Saving It";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["IOU"] = "#EC407A",
            ["Almost"] = "#EF5350",
            ["House Rule"] = "#AB47BC",
            ["Cash In"] = "#B71C4A",
        };

    /// <summary>No points — the pot is the prize.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Returns the built-in slow-burn card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        SlowBurnCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => SlowBurnCardBank.All;
}

/// <summary>Built-in card bank for Slow Burn.</summary>
public static class SlowBurnCardBank
{
    /// <summary>All slow-burn cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── ✉️ IOU — write it, fold it, let it smoulder in the pot ───────────
        I("Write ONE WORD naming a place. Fold it. When the pot opens, that's where they'll be kissed. They won't know which word is yours until then. Choose like an artist.",
          Difficulty.Medium),
        I("Write a number between 1 and 10. Fold it. At the pot: that's how many minutes of your undivided, phone-in-another-room attention they can claim, doing anything they choose.",
          Difficulty.Easy),
        I("Write the first five words of a sentence you'll be made to finish OUT LOUD when the pot opens. Make future-you sweat a little.",
          Difficulty.Medium),
        I("Write one item of clothing. Fold it. Pot rules: its owner surrenders it as a trophy for the rest of the night. Choose with strategy or mercy — your call.",
          Difficulty.Hard),
        I("Write a time (like 11:20). Fold it. When the pot opens, whatever's happening at that exact minute tonight, you both stop and kiss like the movie's ending.",
          Difficulty.Medium),
        I("Write ONE thing you want whispered to you later — verbatim, no paraphrasing allowed. Sign it. The pot remembers.",
          Difficulty.Hard),
        I("Write a dare for the pot that starts with the words 'Slowly…'. You will not remember writing this calmly.",
          Difficulty.Extreme),
        I("Write the name of a song. Pot rules: it plays, and for its full length you're not allowed to let go of each other. Pick a short song if you're a coward.",
          Difficulty.Medium),
        I("Write 'yes' or 'no' — that's your sealed answer to a question they get to ask ONLY when the pot opens. They should start drafting the question now.",
          Difficulty.Extreme),
        I("Each write one rule for 'after the game ends'. Fold both into one shared wad. That wad opens LAST, after everything else in the pot. Yes, last. The pot has a finale.",
          Difficulty.Extreme),
        I(
          "Write ONE WORD naming a length of time. Fold it. When the pot opens, that's how long the next kiss lasts. Be generous, or be cruel.",
          Difficulty.Medium),
        I(
          "Write ONE WORD naming something you want them to say to you. Fold it. When the pot opens they say it — in their own voice, looking at you.",
          Difficulty.Extreme),

        // ── 🫧 ALMOST — stop at the best part. The card is a menace. ─────────
        A("Lean in for a kiss with full cinematic commitment — and stop one inch away. Hold for a slow count of five, breathing the same air. Then say 'later' and calmly draw the next card.",
          Difficulty.Hard),
        A("Trace one fingertip from their wrist to their shoulder as slowly as you can physically bear. Stop AT the shoulder. Remove the finger. Comment on the weather.",
          Difficulty.Medium),
        A("Start telling them, in your lowest voice, exactly what you'd like to do later tonight — and stop mid-sentence at the most unfair possible word. That sentence goes in the pot, unfinished.",
          Difficulty.Extreme),
        A("Undo exactly one button, clasp, or tie — theirs or yours, your choice. One. Then sit back like a person of tremendous restraint.",
          Difficulty.Extreme),
        A("Slow-dance for precisely twenty seconds. When the twenty seconds end, stop MID-SWAY and bow formally, as if the orchestra left. The orchestra will return when the pot opens.",
          Difficulty.Medium),
        A("Whisper the first half of a compliment into their ear — 'the thing I can never stop thinking about is…' — and finish it silently, mouthing the words with no sound. Lip-readers prosper.",
          Difficulty.Hard),
        A("Give a shoulder massage of exactly four presses. FOUR. On the fourth, lean down as if to kiss their neck, and instead whisper 'that's the free sample.'",
          Difficulty.Hard),
        A("Hold their face like the kiss is happening — thumbs on cheekbones, the whole production — then gently turn their head and kiss them on the forehead. Accept the consequences with dignity.",
          Difficulty.Medium),
        A(
          "Take their hand as though you're about to lead them somewhere. Stand up. Then sit back down and say 'not yet.'",
          Difficulty.Medium),
        A(
          "Tell them the first four words of what you want to happen tonight. Stop at the fourth. The pot is holding the rest.",
          Difficulty.Hard),

        // ── 📜 HOUSE RULE — the game slowly rewrites the room ────────────────
        R("Until the next House Rule: every time either of you laughs, you owe each other three seconds of close-range eye contact before the game continues.",
          Difficulty.Easy),
        R("Until the next House Rule: you must be touching — hand, knee, shoulder, anything — at all times. Breaking contact costs the breaker one folded IOU of the other's dictation.",
          Difficulty.Medium),
        R("Until the next House Rule: everything you say to each other must be said within a hand-span of their ear. The game is now a whispering game. The neighbours hear nothing.",
          Difficulty.Hard),
        R("Until the next House Rule: each of you picks one word (say them now). Whenever the other person's word is spoken by anyone, its owner collects a kiss on the hand. Choose common words at your peril.",
          Difficulty.Medium),
        R("Until the next House Rule: the person whose turn it is decides where the other one sits. Yes, 'closer' is a location.",
          Difficulty.Hard),
        R("Until the next House Rule: no saying each other's names — only endearments. Repeats are forbidden. Watch the inventory run dangerously low.",
          Difficulty.Medium),
        R("Lights: the current player adjusts the room lighting to their liking right now. This isn't even a rule that expires. It's just better now.",
          Difficulty.Easy),
        R("Until the next House Rule: before every card, the drawer must give the other a once-over — a slow, obvious, appreciative look — and say nothing about it.",
          Difficulty.Hard),
        R(
          "Until the next House Rule: neither of you may say the other's name without touching them somewhere at the same moment.",
          Difficulty.Easy),
        R(
          "Until the next House Rule: every card is read aloud in a whisper, close enough that it has to be.",
          Difficulty.Medium),

        // ── 🔓 CASH IN — the pot pays early ──────────────────────────────────
        C("Draw ONE folded IOU from the pot and redeem it right now. Author's handwriting is law; your timing is the only mercy you're owed.",
          Difficulty.Hard),
        C("Draw an IOU from the pot but DON'T open it. Trade it, sight unseen, for one kiss of the other player's design. The IOU goes back in, unread, now radioactive with mystery.",
          Difficulty.Medium),
        C("Pot audit: shake the bowl dramatically and each guess how many IOUs are inside. Closest guess may either cash one in now or add one written by dictating it to the other player — their hand, your words.",
          Difficulty.Medium),
        C("The pot is feeling generous: BOTH draw one IOU and redeem them simultaneously. Choreography is your problem. Take your time solving it.",
          Difficulty.Extreme),
        C("If the pot is empty, this card is a scandal: write one JOINT IOU together, one word each, alternating, exactly ten words. Fold it. It opens last tonight, after even the finale wad.",
          Difficulty.Extreme),
        C("Draw an IOU. The AUTHOR must redeem it upon the drawer instead — every promise reverses. The pot enjoys irony.",
          Difficulty.Extreme),
        C(
          "Draw ONE folded IOU and read it aloud — then fold it and put it back. Now you both know it's coming, and neither of you knows when.",
          Difficulty.Medium),
        C(
          "Empty the pot. Read them all in the order they come out, and honour every one before the night is over.",
          Difficulty.Extreme),
    ];

    private static ICard I(string text, Difficulty d) =>
        StandardCard.Create("IOU",
            "<b>✉️ Seal it in the pot:</b>\n\n" + text +
            "\n\n<i>Fold it. Nobody reads it yet. The pot opens when the game ends — and either of you can always trade any IOU for a kiss and a rain check.</i>",
            d, "IOU");

    private static ICard A(string text, Difficulty d) =>
        StandardCard.Create("Almost",
            "<b>🫧 Stop at the best part:</b>\n\n" + text +
            "\n\n<i>Yes, stopping is the whole card. What you start now, the pot finishes later.</i>",
            d, "Almost");

    private static ICard R(string text, Difficulty d) =>
        StandardCard.Create("House Rule",
            "<b>📜 New standing rule:</b>\n\n" + text,
            d, "House Rule");

    private static ICard C(string text, Difficulty d) =>
        StandardCard.Create("Cash In",
            "<b>🔓 The pot pays early:</b>\n\n" + text,
            d, "Cash In");
}
