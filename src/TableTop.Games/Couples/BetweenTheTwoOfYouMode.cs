using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Restrictions;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Between the Two of You — a self-knowledge quiz for adult couples about the
/// dynamics that shape intimacy: whether you lean toward leading or following,
/// giving or receiving, planning or spontaneity, words or touch, adventure or
/// comfort. It is NOT a scoreboard and there is no "better" answer on any axis
/// — the whole point is to see where each of you naturally sits, where you
/// mirror each other, where you differ, and to turn that into things to talk
/// about and grow.
///
/// How to play:
///   1. Each partner answers the same questions privately (A/B/C/D), jotting
///      their letters — this is about YOUR honest lean, not what you think you
///      "should" say.
///   2. Each axis ends with a Results card that reads what your lean tends to
///      mean, its strength, and one gentle growth edge.
///   3. Compare. Where you match, name what works. Where you differ, treat it
///      as information, not a problem — most friction between partners is just
///      two different leans that were never said out loud.
///
/// Every card is couples-only and framed around communication and consent.
/// Explicit in subject, tasteful in language — a conversation starter, not
/// erotica. Adult (18+), for established partners.
/// </summary>
public sealed class BetweenTheTwoOfYouMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <summary>Two people, together. The consent ritual, the aftercare and the register all assume it.</summary>
    public TableShape SuitableFor => TableShape.Couple;


    /// <inheritdoc />
    public override string Name => "Between the Two of You";
    /// <inheritdoc />
    public override string Description =>
        "A self-knowledge quiz for couples on the dynamics of intimacy — lead/follow, give/receive, and more. Find your leans, then grow together.";

    /// <summary>Label for a completed question.</summary>
    public override string CompleteLabel => "Noted";
    /// <summary>Label for a skipped question.</summary>
    public override string SkipLabel => "Pass";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [BetweenTheTwoOfYouCardBank.LeadFollowCategory] = "#7E57C2",
            [BetweenTheTwoOfYouCardBank.GiveReceiveCategory] = "#EC407A",
            [BetweenTheTwoOfYouCardBank.PlanSparkCategory] = "#42A5F5",
            [BetweenTheTwoOfYouCardBank.WordsTouchCategory] = "#26A69A",
            [BetweenTheTwoOfYouCardBank.BoldCosyCategory] = "#FFA726",
            ["Results"] = "#FFD700",
            ["Grow Together"] = "#66BB6A",
        };

    /// <summary>Flat scoring — this is a self-knowledge quiz, not a contest.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(1);

    /// <summary>Returns the built-in card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        BetweenTheTwoOfYouCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => BetweenTheTwoOfYouCardBank.All;
}

/// <summary>Built-in card bank for Between the Two of You.</summary>
public static class BetweenTheTwoOfYouCardBank
{
    internal const string LeadFollowCategory = "Lead & Follow";
    internal const string GiveReceiveCategory = "Give & Receive";
    internal const string PlanSparkCategory = "Plan & Spark";
    internal const string WordsTouchCategory = "Words & Touch";
    internal const string BoldCosyCategory = "Bold & Cosy";

    /// <summary>All cards, ordered so each axis's questions are followed by its Results card.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build()
    {
        var couples = new CoupleOnlyRestriction();

        ICard Q(string category, string title, string prompt, Difficulty d) =>
            StandardCard.Create(title, prompt, d, category, restriction: couples);

        ICard Result(string title, string body) =>
            StandardCard.Create(title, body, Difficulty.Easy, "Results");

        return
        [
            // ══ LEAD & FOLLOW — where you sit on taking charge vs handing it over ══
            Q(LeadFollowCategory, "Who Sets the Pace",
              "When things get intimate, you're most drawn to:\n" +
              "A) Taking charge — setting the pace and leading where it goes.\n" +
              "B) Being led — letting your partner set the pace and following.\n" +
              "C) Trading off — sometimes leading, sometimes following, by mood.\n" +
              "D) Fully mutual — neither leads; you move together.",
              Difficulty.Easy),
            Q(LeadFollowCategory, "Being Told vs Asking",
              "Which lands better for you in the moment?\n" +
              "A) Saying what you want to happen and having it followed.\n" +
              "B) Being told what to do, and enjoying letting go.\n" +
              "C) A back-and-forth where you swap who's steering.\n" +
              "D) Reading each other wordlessly, no one 'in charge'.",
              Difficulty.Medium),
            Q(LeadFollowCategory, "Where You Feel Most Free",
              "You feel most yourself when you're:\n" +
              "A) Holding the reins and being trusted with them.\n" +
              "B) Handing the reins over and being taken care of.\n" +
              "C) Free to switch depending on the day.\n" +
              "D) On completely equal footing, deciding together.",
              Difficulty.Medium),
            Result("Your Lean: Lead & Follow",
              "<b>Count your letters for this axis.</b>\n\n" +
              "<b>Mostly A — You lean toward leading.</b> You like holding the wheel and being trusted with it. Strength: you make things happen and your partner can relax into that. Grow: leading well means reading the other's yes closely — practise handing over control sometimes and see how receiving feels.\n\n" +
              "<b>Mostly B — You lean toward following.</b> You find freedom in letting go and being taken care of. Strength: you can be fully present without managing. Grow: following is a choice, not a default — practise voicing what you want, so your partner is leading toward YOUR desires, not guessing.\n\n" +
              "<b>Mostly C — You're a switch.</b> You move between leading and following by mood. Strength: range and adaptability. Grow: say which one you're in the mood for out loud — switches can leave partners unsure who's steering.\n\n" +
              "<b>Mostly D — You lean fully mutual.</b> You like moving as equals with no one in charge. Strength: deep attunement. Grow: try letting one person lead on purpose sometimes — a little structure can be its own kind of freedom.\n\n" +
              "<i>Compare with your partner: a leader + a follower can fit beautifully — but only when both chose it, out loud. Two leaders or two followers just means taking turns is your growth work.</i>"),

            // ══ GIVE & RECEIVE — the pleasure of giving vs the pleasure of receiving ══
            Q(GiveReceiveCategory, "Where Your Attention Goes",
              "You get the most out of intimacy when you're:\n" +
              "A) Focused on your partner's pleasure — giving is your joy.\n" +
              "B) Able to receive — letting yourself be the focus.\n" +
              "C) Both flowing back and forth, roughly even.\n" +
              "D) Honestly not sure — you've never really thought about it.",
              Difficulty.Easy),
            Q(GiveReceiveCategory, "The Harder One to Accept",
              "Which is harder for you?\n" +
              "A) Sitting still and simply receiving without 'returning' it.\n" +
              "B) Letting your partner give while you do nothing back.\n" +
              "C) Neither — you're comfortable on both sides.\n" +
              "D) Asking for what you want at all.",
              Difficulty.Medium),
            Q(GiveReceiveCategory, "If You Had to Pick",
              "A whole evening about just ONE of you — you'd rather it be about:\n" +
              "A) Your partner. You'd love an evening devoted to them.\n" +
              "B) You. You'd love, just once, to be thoroughly spoiled.\n" +
              "C) Can't choose — you'd want to swap halfway.\n" +
              "D) The idea of being the sole focus makes you a little shy.",
              Difficulty.Medium),
            Result("Your Lean: Give & Receive",
              "<b>Count your letters for this axis.</b>\n\n" +
              "<b>Mostly A — You lean toward giving.</b> Your partner's pleasure is your pleasure. Strength: generosity and attentiveness. Grow: receiving is a gift you give THEM too — many givers struggle to be on the receiving end. Practise staying still and simply accepting.\n\n" +
              "<b>Mostly B — You lean toward receiving.</b> You can let yourself be the focus, which many people can't. Strength: you let your partner's generosity land. Grow: make sure giving-back is on your radar too — ask your partner what THEY love to receive.\n\n" +
              "<b>Mostly C — You're balanced.</b> You flow both ways comfortably. Strength: easy reciprocity. Grow: 'balanced' can quietly become 'keeping score' — sometimes give freely with nothing expected back, and let yourself receive the same.\n\n" +
              "<b>Mostly D — You're still figuring this out.</b> That's completely normal and a great place to start. Grow: this whole quiz is your friend — notice, next time, which side feels easier, and tell your partner one thing you'd like to try.\n\n" +
              "<i>Compare: a giver and a receiver can be a perfect match — as long as the giver also gets received, and the receiver also gets to give. Two givers? Practise being 'selfish' on purpose. Two receivers? Take turns spoiling.</i>"),

            // ══ PLAN & SPARK — planned intimacy vs spontaneous ══
            Q(PlanSparkCategory, "Anticipation vs Surprise",
              "Intimacy works best for you when it's:\n" +
              "A) Planned and looked forward to — anticipation is half the fun.\n" +
              "B) Spontaneous — the best moments aren't scheduled.\n" +
              "C) A mix — some planned, some out of nowhere.\n" +
              "D) Whenever it happens; you don't think in those terms.",
              Difficulty.Easy),
            Q(PlanSparkCategory, "A Date on the Calendar",
              "'Intimacy night' written in the calendar for Friday makes you feel:\n" +
              "A) Great — something to build toward all week.\n" +
              "B) A bit flat — scheduling takes the spark out for you.\n" +
              "C) Fine occasionally, not as a rule.\n" +
              "D) Neutral — you'd go with the flow either way.",
              Difficulty.Medium),
            Q(PlanSparkCategory, "The Free Evening",
              "An unexpected free evening turns up in both your diaries. You'd rather:\n" +
              "A) Make a plan for it now and enjoy the wait.\n" +
              "B) Leave it completely open and see what happens.\n" +
              "C) Have a loose idea, with room to change it.\n" +
              "D) Not think about it either way until you're in it.",
              Difficulty.Medium),
            Result("Your Lean: Plan & Spark",
              "<b>Count your letters for this axis.</b>\n\n" +
              "<b>Mostly A — You lean toward planned.</b> Anticipation is your aphrodisiac. Strength: intimacy actually happens because you make room for it. Grow: leave the occasional gap for the unplanned — surprise has its own charge.\n\n" +
              "<b>Mostly B — You lean toward spontaneous.</b> The unscheduled moment is the magic for you. Strength: playfulness and presence. Grow: pure spontaneity can mean intimacy quietly gets crowded out by busy weeks — a little planning isn't unromantic, it's making space.\n\n" +
              "<b>Mostly C / D — You're flexible.</b> You can take it either way. Strength: no friction here. Grow: find out your PARTNER's lean — this is the axis where a planner and a spontaneous partner most often quietly frustrate each other.\n\n" +
              "<i>Compare: if one of you needs the calendar and the other needs surprise, the fix is a blend — a loosely-planned window that leaves room for spontaneity inside it.</i>"),

            // ══ WORDS & TOUCH — how you most naturally express and receive desire ══
            Q(WordsTouchCategory, "How Desire Reaches You",
              "You most feel wanted when your partner:\n" +
              "A) Tells you — says out loud what they want and love about you.\n" +
              "B) Shows you — reaches for you, touch that says it without words.\n" +
              "C) Both equally; you need to hear it AND feel it.\n" +
              "D) Through effort — they make time and pay attention.",
              Difficulty.Easy),
            Q(WordsTouchCategory, "Talking During",
              "Talking, direction, and sound during intimacy is something you:\n" +
              "A) Love — words turn you on and keep you connected.\n" +
              "B) Prefer without — you go quieter and more physical.\n" +
              "C) Enjoy in small doses at the right moments.\n" +
              "D) Feel shy about but might like to try more.",
              Difficulty.Medium),
            Q(WordsTouchCategory, "In the Middle of the Day",
              "It's an ordinary Tuesday afternoon. What reaches you most?\n" +
              "A) A message spelling out exactly what they're thinking about.\n" +
              "B) Coming home to them reaching for you before either of you speaks.\n" +
              "C) Both — the message, and then the hands.\n" +
              "D) Finding they've quietly cleared the evening so you have it together.",
              Difficulty.Medium),
            Result("Your Lean: Words & Touch",
              "<b>Count your letters for this axis.</b>\n\n" +
              "<b>Mostly A — You're wired for words.</b> Being told is how desire reaches you, and speaking is how you express it. Strength: you can ask for what you want. Grow: your quieter partner may show love through touch — learn to read their language too.\n\n" +
              "<b>Mostly B — You're wired for touch.</b> You express and receive desire physically, not verbally. Strength: presence and physical attunement. Grow: a few words cost little and can mean the world to a word-wired partner — try saying one thing out loud.\n\n" +
              "<b>Mostly C — You need both.</b> Strength: you connect on two channels. Grow: tell your partner you need both, so they don't over-rely on the one that comes easiest to them.\n\n" +
              "<b>Mostly D — You express it through effort and attention.</b> Strength: consistency and care. Grow: effort is beautiful but easy to miss in the moment — pair it with an explicit word or touch so it lands.\n\n" +
              "<i>Compare: a words person paired with a touch person can each feel unloved while both are trying hard — in different languages. Name yours; ask for theirs.</i>"),

            // ══ BOLD & COSY — appetite for adventure vs the beloved familiar ══
            Q(BoldCosyCategory, "New vs Known",
              "When it comes to trying new things together, you're:\n" +
              "A) The adventurer — keen to explore and push gently at edges.\n" +
              "B) The homebody — you love the familiar, trusted, and safe.\n" +
              "C) Adventurous with the right build-up and trust.\n" +
              "D) Curious but cautious — interested, slow to leap.",
              Difficulty.Easy),
            Q(BoldCosyCategory, "The Suggestion",
              "Your partner suggests something new. Your first honest reaction:\n" +
              "A) 'Yes — tell me more.' You light up at new.\n" +
              "B) 'I'm happy with what we have.' New isn't a need for you.\n" +
              "C) 'Maybe — walk me through it.' Warm but needs to feel safe.\n" +
              "D) 'Let me think.' You want time before you decide.",
              Difficulty.Medium),
            Q(BoldCosyCategory, "Somewhere That Isn't Here",
              "The idea of being together somewhere other than your usual place:\n" +
              "A) Exciting — you're already thinking of where.\n" +
              "B) Not for you; your own space is where you actually relax.\n" +
              "C) Good, as long as you'd talked it through and felt safe first.\n" +
              "D) Interesting, but you'd want to sit with the idea a while.",
              Difficulty.Medium),
            Result("Your Lean: Bold & Cosy",
              "<b>Count your letters for this axis.</b>\n\n" +
              "<b>Mostly A — You lean bold.</b> Novelty and gentle edges keep things alive for you. Strength: you keep the relationship exploring. Grow: 'new' should never be pressure — your partner's slower yes is as valid as your quick one. The sexiest thing is an enthusiastic partner, not a talked-into one.\n\n" +
              "<b>Mostly B — You lean cosy.</b> The trusted and familiar is where you feel safe and free. Strength: depth and security. Grow: safety and novelty aren't opposites — one small new thing, chosen by you, on your terms, can deepen the familiar rather than threaten it.\n\n" +
              "<b>Mostly C / D — You're adventurous with trust / curious but cautious.</b> Strength: you can grow, given safety and time. Grow: tell your partner exactly what makes a 'yes' feel safe — build-up, veto, going slow — so they can offer new things the way you can actually receive them.\n\n" +
              "<i>Compare: a bold partner and a cosy one is the most common pairing of all. It works when the bold one treats every 'not yet' as fine, and the cosy one offers the occasional 'let's try' as a gift. Enthusiasm is the only yes.</i>"),

            // ══ GROW TOGETHER — the closing synthesis ══
            StandardCard.Create("Bringing It Together",
                "<b>🌱 GROW TOGETHER</b>\n\n" +
                "Lay your letters side by side across all five axes. Talk through:\n\n" +
                "• <b>Where you match.</b> Name one thing that already works because of it — and protect it.\n" +
                "• <b>Where you differ.</b> Pick the biggest gap. It's not a flaw in either of you; it's just un-said. What would honour BOTH leans?\n" +
                "• <b>One growth edge each.</b> Each name one small thing you'd like to try or get better at — and one thing you'd love your partner to know.\n\n" +
                "<i>No result here is better than another, and today's leans can change. The couples who thrive aren't the ones who match on everything — they're the ones who said it all out loud, kindly. Enthusiasm is the only yes; anything you name is an invitation, never an obligation.</i>",
                Difficulty.Easy, "Grow Together", restriction: couples),
        ];
    }
}
