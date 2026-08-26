using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Heat Check (18+) — every card, two temperatures. You pick together.
///
/// How to play:
///   1. Draw a card. It offers the same prompt at two intensities:
///        🕯️ CANDLE — warm, playful, fully clothed in every sense.
///        🔥 FIRE   — the same idea with the thermostat up.
///   2. BEFORE anyone acts, you both vote: candle or fire. Any mismatch,
///      any hesitation, any "hmm" — candle wins automatically. Fire is only
///      fire when it's unanimous and enthusiastic.
///   3. Do the chosen version. Then next card.
///
/// The design point: consent isn't a rules-box footnote here, it IS the
/// mechanic. Choosing together every single card keeps both players exactly
/// as warm as they actually want to be — and makes the choosing itself part
/// of the flirting.
/// </summary>
public sealed class HeatCheckMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <summary>Two people, together. The consent ritual, the aftercare and the register all assume it.</summary>
    public TableShape SuitableFor => TableShape.Couple;

    /// <inheritdoc />
    public override string Name        => "Heat Check";
    /// <inheritdoc />
    public override string Description =>
        "Every card at two temperatures — 🕯️ candle or 🔥 fire. You choose together, every time. Mismatch means candle.";

    /// <summary>Label for the button that records a played card.</summary>
    public override string CompleteLabel => "Played It";
    /// <summary>Label for the button that passes on a card entirely.</summary>
    public override string SkipLabel     => "Too Hot / Not Tonight";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Confessions"] = "#EC407A",
            ["Dares"]       = "#EF5350",
            ["Scenes"]      = "#AB47BC",
            ["Closer"]      = "#B71C4A",
        };

    /// <summary>No points — the reward system is built into the cards.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Returns the built-in heat-check card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        HeatCheckCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => HeatCheckCardBank.All;
}

/// <summary>Built-in card bank for Heat Check.</summary>
public static class HeatCheckCardBank
{
    /// <summary>All heat-check cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── CONFESSIONS — say the thing ──────────────────────────────────────
        H("Confessions",
          "Tell them one thing they wore, ever, that you still think about.",
          "Tell them what you were thinking the first time you saw them — the unedited version, eye contact mandatory.",
          Difficulty.Easy),
        H("Confessions",
          "Name your favourite ordinary moment with them from this month.",
          "Name the most recent moment you wanted to kiss them and didn't. Explain what stopped you. Then stop letting it.",
          Difficulty.Medium),
        H("Confessions",
          "Admit one tiny thing they do that always improves your day.",
          "Admit the thing they do — a gesture, a look, a tone of voice — that works on you every single time. Yes, now they know. That was the point.",
          Difficulty.Medium),
        H("Confessions",
          "Share a compliment you've said about them to someone else.",
          "Share the compliment about them you've never said OUT LOUD to anyone — including them. Until now.",
          Difficulty.Hard),
        H("Confessions",
          "Tell them your favourite place the two of you have ever been.",
          "Tell them the place you'd take them tonight if the babysitter/boss/world allowed it — and exactly what the first hour would look like.",
          Difficulty.Medium),
        H("Confessions",
          "Describe your first impression of their laugh.",
          "Describe, slowly, your favourite thing about the way they move. Be specific enough that they blush.",
          Difficulty.Hard),
        H("Confessions",
          "Confess a song that secretly makes you think of them.",
          "Confess the daydream about them you had at a spectacularly inappropriate time and place. Full context required.",
          Difficulty.Hard),
        H("Confessions",
          "Tell them one thing you hope never changes about the two of you.",
          "Finish this sentence honestly and out loud: 'Later tonight, I am hoping…' — and don't you dare say 'to sleep'.",
          Difficulty.Extreme),
        H("Confessions",
          "Name something small they did this week that you noticed and loved.",
          "Name what you notice FIRST when they walk into a room — the honest answer, not the polite one.",
          Difficulty.Medium),
        H("Confessions",
          "Tell them the compliment you've thought about them today but haven't said.",
          "Tell them the thought you had about them today that you decided not to say out loud. Say it now.",
          Difficulty.Medium),
        H("Confessions",
          "Name the moment tonight you'd most like to repeat.",
          "Name the thing you were quietly hoping this game would give you an excuse to do.",
          Difficulty.Hard),
        H("Confessions",
          "Tell them one thing you find attractive that you've never mentioned.",
          "Tell them the one you've never mentioned because saying it felt too revealing — and say why it is.",
          Difficulty.Extreme),

        // ── DARES — do the thing ─────────────────────────────────────────────
        H("Dares",
          "Hold eye contact for 20 full seconds. No talking. Smiling permitted, barely.",
          "Hold eye contact for 60 seconds, hands in theirs, and whatever happens at second 61 is between you two.",
          Difficulty.Easy),
        H("Dares",
          "Give them a 30-second hand massage like you mean it.",
          "Two minutes: shoulders, neck, and you're not allowed to speak — communicate everything another way.",
          Difficulty.Medium),
        H("Dares",
          "Kiss them on the cheek like it's 1952 and their parents are watching.",
          "Kiss them somewhere you have never kissed them before. Take your time choosing.",
          Difficulty.Hard),
        H("Dares",
          "Slow dance to no music for one verse of a song only you can hear.",
          "Slow dance to no music — but this time you lead with your eyes closed and let them steer you by touch alone.",
          Difficulty.Medium),
        H("Dares",
          "Whisper your favourite memory of them into their ear.",
          "Whisper exactly what you're thinking right now into their ear. Complete sentences. No editing.",
          Difficulty.Hard),
        H("Dares",
          "Recreate your very first physical contact — the handshake, the accidental arm brush, whatever it was.",
          "Recreate your first kiss — but the director's cut: the version you'd film if you got a second take at it.",
          Difficulty.Hard),
        H("Dares",
          "Trade one accessory — watch, ring, hair tie — and wear it for the rest of the game.",
          "They close their eyes. You have 30 seconds and one fingertip. Make it count. They guess the message you traced.",
          Difficulty.Extreme),
        H("Dares",
          "Compliment them in your best terrible foreign accent until they laugh.",
          "Compliment them, sincerely and slowly, from three inches away. First one to break eye contact does the dishes tomorrow.",
          Difficulty.Medium),
        H("Dares",
          "Take a flattering photo of them right now, as they are.",
          "Take a photo of them right now that ONLY the two of you will ever see — art-directed by you, veto power theirs.",
          Difficulty.Extreme),
        H("Dares",
          "Kiss them somewhere you've never deliberately kissed them before.",
          "Same — but they choose the spot, and you take a full slow minute getting there.",
          Difficulty.Medium),
        H("Dares",
          "Whisper what you'd like to happen next.",
          "Whisper it, then do the first half of it and stop.",
          Difficulty.Hard),
        H("Dares",
          "Take one minute of their completely undivided attention, however you like.",
          "Take five, and say out loud what you're doing as you do it.",
          Difficulty.Extreme),
        H("Dares",
          "Put their hand exactly where you want it and hold it there for a full minute.",
          "Put their hand exactly where you want it, tell them precisely what to do with it, and don't let go until they have.",
          Difficulty.Extreme),
        H("Dares",
          "Undo one thing they're wearing, slowly, and stop there.",
          "Undo one thing — then let them decide whether the next comes off, and take as long over it as they'll allow.",
          Difficulty.Hard),

        // ── SCENES — play the thing ──────────────────────────────────────────
        H("Scenes",
          "You're teenagers whose curfews are in ten minutes. Say goodnight at the imaginary front door.",
          "You're teenagers whose curfews are in ten minutes — and the porch light just went out. Improvise until 'curfew'.",
          Difficulty.Medium),
        H("Scenes",
          "Reenact meeting for the first time — but as your current selves, who somehow know they've found something.",
          "Strangers in a bar, tonight, five lines of dialogue each — and the last line has to be an invitation.",
          Difficulty.Hard),
        H("Scenes",
          "One of you is a fortune teller reading the other's palm. Predict a wonderful, wholesome week.",
          "Fortune teller again — but this palm says something is going to happen TONIGHT. Trace the line while you describe it.",
          Difficulty.Medium),
        H("Scenes",
          "You're spies exchanging a coded message in a café. Deliver the pass-phrase with maximum drama.",
          "Spies again — but the code is physical: hide a folded note somewhere on your person and describe the dead-drop rules. Retrieval is the other agent's problem.",
          Difficulty.Extreme),
        H("Scenes",
          "Formal ballroom introduction: bow/curtsy, kiss the hand, one gallant compliment in period language.",
          "The ballroom emptied an hour ago. You two stayed. Narrate — and act — the dance the chaperones prevented.",
          Difficulty.Hard),
        H("Scenes",
          "You're co-hosts of a cooking show making an imaginary dessert. Big TV energy.",
          "Cooking show, but the 'taste test' is conducted blindfold-style: eyes closed, they guess three real things you touch to their lips (chef's choice, kitchen ingredients).",
          Difficulty.Extreme),
        H("Scenes",
          "Movie premiere: one of you interviews the other on the red carpet about 'your latest romance'.",
          "Same premiere — but you're the couple the cameras caught leaving early. Improvise the car conversation.",
          Difficulty.Hard),
        H("Scenes",
          "You've just matched on a dating app — as yourselves. Improvise the first three messages out loud.",
          "The dating-app match went SO well you're writing tomorrow's 'so last night…' texts to your best friends. Read them to each other.",
          Difficulty.Medium),
        H("Scenes",
          "Airport reunion: one of you has 'been away for months'. Stick the landing.",
          "Airport goodbye instead — you have sixty seconds before 'boarding'. Make the sixty seconds legendary, then don't board.",
          Difficulty.Hard),
        H("Scenes",
          "You're strangers seated next to each other on a long flight. Make conversation.",
          "Same flight — except one of you has privately decided this is going somewhere. Play it out until the seatbelt sign.",
          Difficulty.Medium),
        H("Scenes",
          "You haven't seen each other in six months. Reunite at the arrivals gate.",
          "Same reunion, but you've made it as far as the car park and there is nobody watching.",
          Difficulty.Hard),
        H("Scenes",
          "It's the night you first met, replayed — but this time you both already know how it ends.",
          "The same night, except you skip every part you were too polite to skip the first time.",
          Difficulty.Extreme),
        H("Scenes",
          "You're the last two at a party everyone else has left. Say the thing you'd say.",
          "Same party, same room, and nobody is coming back. Take it as far as you both want it to go.",
          Difficulty.Extreme),
        H("Scenes",
          "A hotel corridor, the wrong room, a key that works anyway. Play the first sixty seconds.",
          "Play it past the door.",
          Difficulty.Hard),

        // ── CLOSER — the thing behind the thing ──────────────────────────────
        H("Closer",
          "Sit back to back for one minute and each say one thing you're grateful for about the other.",
          "Sit face to face, knees touching, one minute of silence first — THEN say the thing you've been saving. You know the one.",
          Difficulty.Hard),
        H("Closer",
          "Plan your ideal lazy Sunday morning together, out loud, in detail.",
          "Plan tonight. Out loud. In detail. Starting from the moment this game ends. Both of you contribute alternate steps.",
          Difficulty.Extreme),
        H("Closer",
          "Tell them the exact moment you knew this was something real.",
          "Tell them the moment you last fell for them AGAIN — recently. There's always a recent one. Find it.",
          Difficulty.Hard),
        H("Closer",
          "Exchange one promise for the coming week. Small ones count double.",
          "Exchange one promise for the next hour. Be brave. Shake on it — or seal it however you prefer.",
          Difficulty.Extreme),
        H("Closer",
          "Describe the other person to an imaginary stranger, glowingly, while they listen.",
          "Describe the other person to an imaginary stranger — as your lover, not your partner. Watch the vocabulary change. They're allowed to enjoy it.",
          Difficulty.Extreme),
        H("Closer",
          "Hold hands, close your eyes, and each picture your favourite future scene. Compare.",
          "Hold hands, close your eyes, and each picture tonight going PERFECTLY. Compare notes. Reconcile any differences… practically.",
          Difficulty.Extreme),
        H("Closer",
          "Say thank you for one thing you've never officially thanked them for.",
          "Say out loud the sentence you usually only say in your head at 2 a.m. watching them sleep. Yes, that one.",
          Difficulty.Extreme),
        H("Closer",
          "End-of-round check-in: each rate tonight so far out of ten, and name what would make it a point higher.",
          "Each name the ONE card from tonight you'd like to replay before bed — fire version. The game politely looks away.",
          Difficulty.Extreme),
        H("Closer",
          "Write (or say) a two-line love note the other can keep for a bad day.",
          "Whisper a two-line note the other is NOT allowed to repeat, quote, or forget. Delivery matters more than poetry.",
          Difficulty.Hard),
        H("Closer",
          "Tell them the thing about your life together you're most quietly proud of.",
          "Tell them the thing you want from the next year that you've never said out loud.",
          Difficulty.Hard),
        H("Closer",
          "Forehead to forehead, one minute, breathing in time.",
          "Forehead to forehead until one of you says the thing you're both already thinking.",
          Difficulty.Extreme),
        H("Closer",
          "Say what you'd want them to remember about tonight.",
          "Say what you'd want them to remember about you.",
          Difficulty.Extreme),
        H("Closer",
          "Tell them the one thing you want from tonight.",
          "Tell them the one thing you want from tonight, in the exact words you'd use if nobody else existed — then go and get it.",
          Difficulty.Extreme),
        H("Closer",
          "Lie down together and each say one thing you're grateful for.",
          "Lie down together, and whoever speaks first decides how the rest of the night goes.",
          Difficulty.Extreme),
    ];

    private static ICard H(string category, string candle, string fire, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>Choose together before anyone moves — any mismatch means 🕯️:</b>\n\n" +
            "🕯️ <b>Candle:</b> " + candle + "\n\n" +
            "🔥 <b>Fire:</b> " + fire + "\n\n" +
            "<i>Fire is only fire when it's unanimous and enthusiastic. Candle is never a loss.</i>",
            d, category);
}
