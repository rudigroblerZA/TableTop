using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// All In (18+) — a casino of flirtation. The currency of this table is
/// kisses; the stakes are each other.
///
/// SETUP (before the first card): each of you secretly writes THE JACKPOT —
/// one thing you want tonight if you win — folds it, and sets it face-down
/// under your side of the table. Nobody peeks.
///
/// PLAY: cards are hands. Winning a hand scores a chip (the app keeps the
/// chip count — that's the scoreboard). Four kinds of hands:
///
///   ♠ ANTE — small charm stakes everyone can afford. Play it, score it.
///   ♥ RAISE — a base move plus a RAISE clause. After you play the base,
///     your partner may call the raise: play it too and the hand pays DOUBLE
///     (record the extra by winning the next Ante automatically). Fold on a
///     called raise and the chip goes to them, paid with a kiss on the hand.
///   ♣ BLUFF — deliver the card's statement with a perfect poker face; your
///     partner calls TRUE or BLUFF. Right call: their chip. Wrong call: yours.
///     All debts settle in kisses, immediately, house rules.
///   ♦ JACKPOT — rare, expensive, unforgettable. Play it or fold it; folding
///     a Jackpot costs one chip to the other side.
///
/// SHOWDOWN: when the deck (or the evening) ends, the chip leader on the
/// scoreboard flips their folded JACKPOT and claims it. The runner-up's
/// jackpot isn't burned — it becomes the opening bid of the rematch.
///
/// House law: any bet can be renegotiated before it's played, and either
/// player may always fold anything for a kiss. The house never shames a fold.
/// </summary>
public sealed class AllInMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <summary>Two people, together. The consent ritual, the aftercare and the register all assume it.</summary>
    public TableShape SuitableFor => TableShape.Couple;

    /// <inheritdoc />
    public override string Name        => "All In";
    /// <inheritdoc />
    public override string Description =>
        "A casino of flirtation: antes, raises, bluff-calling — chips are kisses, and the scoreboard leader claims a secretly written jackpot.";

    /// <summary>Label for the button that records a won hand (scores a chip).</summary>
    public override string CompleteLabel => "Won the Hand";
    /// <summary>Label for the button that folds a card.</summary>
    public override string SkipLabel     => "Fold";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Ante"]    = "#66BB6A",
            ["Raise"]   = "#EC407A",
            ["Bluff"]   = "#42A5F5",
            ["Jackpot"] = "#B71C4A",
        };

    /// <summary>One chip per won hand — the scoreboard IS the chip stack,
    /// and the chip leader claims the jackpot at showdown.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Returns the built-in all-in card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        AllInCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => AllInCardBank.All;
}

/// <summary>Built-in card bank for All In.</summary>
public static class AllInCardBank
{
    /// <summary>All all-in cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── ♠ ANTE — everyone can afford these stakes ────────────────────────
        N("Compliment their hands. Specifically their hands. You have never thought this hard about hands.",
          Difficulty.Easy),
        N("Look at them the way you looked at them the very first time. Hold it until they notice the difference.",
          Difficulty.Easy),
        N("Tell them one thing they're wearing right now that's working. Even pyjamas contain multitudes.",
          Difficulty.Easy),
        N("Refill or fetch their drink unasked — and deliver it with a completely unnecessary bow, wink, or hand-kiss. Dealer's choice.",
          Difficulty.Easy),
        N("Whisper the pet name you've never dared use. If they laugh, you still win the hand — the house respects courage.",
          Difficulty.Medium),
        N("Rearrange your seating so that some part of you is touching some part of them. Announce 'house rules' as you do it.",
          Difficulty.Easy),
        N("Give the toast this evening deserves — two sentences, glasses or mugs raised, sincerity mandatory.",
          Difficulty.Medium),
        N("Tuck their hair, straighten their collar, or fix nothing at all — the point is the two-second excuse to be that close.",
          Difficulty.Medium),
        N(
          "Compliment the way they say your name. You had four seconds to think about this and you are already out of time.",
          Difficulty.Easy),
        N(
          "Name the item of their clothing you'd remove first, and give exactly one reason. Committee of one. No appeal.",
          Difficulty.Medium),

        // ── ♥ RAISE — base move + a raise your partner may call ─────────────
        S("BASE: kiss them once, anywhere above the collarbone.",
          "RAISE: same kiss — but you take ten full seconds to choose the spot, out loud, narrating the shortlist.",
          Difficulty.Medium),
        S("BASE: give a 20-second shoulder massage.",
          "RAISE: two minutes, and they get to direct — pressure, place, and pace — like a spa critic.",
          Difficulty.Medium),
        S("BASE: tell them one thing you find irresistible about them.",
          "RAISE: three things, escalating, and the third must be something you've never said out loud.",
          Difficulty.Hard),
        S("BASE: slow dance for thirty seconds to music or none.",
          "RAISE: the same dance, but foreheads touching the whole time and you hum the song.",
          Difficulty.Medium),
        S("BASE: hold eye contact for thirty seconds without talking.",
          "RAISE: sixty seconds — and the last ten are spent an inch closer than is reasonable.",
          Difficulty.Hard),
        S("BASE: trace one shape on the back of their hand; they guess it.",
          "RAISE: trace one WORD on their back instead. They guess it or you whisper it.",
          Difficulty.Hard),
        S("BASE: describe your favourite evening you've ever spent together.",
          "RAISE: describe the evening you two haven't had yet — the one you're planning right now, apparently.",
          Difficulty.Hard),
        S("BASE: kiss their hand like visiting royalty.",
          "RAISE: work your way up to the inside of the wrist, at a pace the house can only describe as 'legally slow'.",
          Difficulty.Extreme),
        S("BASE: swap one accessory or item of clothing, your pick.",
          "RAISE: they pick. Both items. No explanations owed to anyone, ever.",
          Difficulty.Extreme),
        S("BASE: whisper what you first noticed about them.",
          "RAISE: whisper what you noticed about them TONIGHT — updated inventory, full candour, minimum distance.",
          Difficulty.Extreme),
        S(
          "BASE: hold their hand across the table for a full thirty seconds.",
          "RAISE: the same thirty seconds — eye contact throughout, and neither of you may smile.",
          Difficulty.Medium),
        S(
          "BASE: tell them one thing you want to do later.",
          "RAISE: the same sentence, whispered, close enough that they feel it — and then you don't mention it again all game.",
          Difficulty.Hard),

        // ── ♣ BLUFF — poker face on; partner calls TRUE or BLUFF ────────────
        B("'I have thought about kissing you at least once today before this game started.'", Difficulty.Easy),
        B("'There is a photo of you on my phone that I look at more often than I'd admit.'", Difficulty.Medium),
        B("'I remember exactly what you were wearing on our first date.' (If called TRUE, you must prove it.)", Difficulty.Medium),
        B("'I have practised saying something to you in the mirror.' (If TRUE, tonight you finally say it.)", Difficulty.Hard),
        B("'I once pretended to be asleep so you'd stay close a little longer.'", Difficulty.Hard),
        B("'I know your exact coffee/tea order well enough to write it down right now.' (Calls of TRUE demand the written proof.)", Difficulty.Medium),
        B("'I have a favourite freckle, scar, or line of yours, and I know precisely where it is.' (TRUE = point to it.)", Difficulty.Hard),
        B("'Something about tonight was my plan all along.' (If TRUE, reveal the plan. The house loves a schemer.)", Difficulty.Extreme),
        B(
          "'I noticed what you were wearing tonight before you'd finished walking into the room.'",
          Difficulty.Medium),
        B(
          "'There is something I have wanted to do since this game started, and I still haven't done it.'",
          Difficulty.Hard),

        // ── ♦ JACKPOT — expensive, unforgettable ─────────────────────────────
        J("Recreate — right now, furniture permitting — the exact moment you knew you were in trouble with this person. Director's commentary encouraged.",
          Difficulty.Extreme),
        J("Write a two-line note, seal it, and hide it somewhere they'll find it within a week. When they find it, this card pays out again: one bonus kiss, redeemable on sight.",
          Difficulty.Hard),
        J("The Silent Hand: set a timer for two minutes. No words allowed. Communicate exactly one complete message using anything else. They state the message; if they're right, you BOTH win the hand.",
          Difficulty.Extreme),
        J("Give them sixty seconds of your best undivided flirting as if you'd just met tonight and everything was still to play for. No history allowed — earn it from scratch.",
          Difficulty.Extreme),
        J("Trade phones for one minute. Each may set ONE reminder on the other's phone for a random day next month. The message: something that will make them blush in a meeting.",
          Difficulty.Hard),
        J("The House Round: invent one brand-new card for this deck, together, right now — and then play it. If it's good, it gets played every time you play All In. You're legends now.",
          Difficulty.Extreme),
        J(
          "Re-enact your first kiss with full historical accuracy — same positions, same hesitation, same appalling soundtrack. Then perform the version you'd do now.",
          Difficulty.Hard),
        J(
          "Say out loud the one thing about them you've never quite managed to put into words. Take as long as you need. The table waits.",
          Difficulty.Extreme),
    ];

    private static ICard N(string text, Difficulty d) =>
        StandardCard.Create("Ante",
            "<b>♠ ANTE — play it, score it:</b>\n\n" + text,
            d, "Ante");

    private static ICard S(string baseMove, string raise, Difficulty d) =>
        StandardCard.Create("Raise",
            "<b>♥ THE HAND:</b>\n\n" + baseMove + "\n\n" + raise + "\n\n" +
            "<i>Play the base, then your partner may call the raise: deliver it and this hand pays double (you also win the next Ante automatically). Fold a called raise and the chip is theirs — paid with a kiss on the hand.</i>",
            d, "Raise");

    private static ICard B(string statement, Difficulty d) =>
        StandardCard.Create("Bluff",
            "<b>♣ POKER FACE. Deliver this line, then they call TRUE or BLUFF:</b>\n\n" +
            statement + "\n\n" +
            "<i>Right call: their chip. Wrong call: yours. All debts settle in kisses, immediately — house rules.</i>",
            d, "Bluff");

    private static ICard J(string text, Difficulty d) =>
        StandardCard.Create("Jackpot",
            "<b>♦ JACKPOT HAND — expensive, unforgettable:</b>\n\n" + text + "\n\n" +
            "<i>Play it and the chip is yours; fold it and one chip slides across the table. The house never shames a fold.</i>",
            d, "Jackpot");
}
