using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Both Or Neither — an explicit intimacy game for two consenting adult
/// partners, built around one mechanic: <b>a card only happens if you both
/// independently chose it.</b>
///
/// <para>
/// <b>How it works.</b> Every card offers three options. You each privately
/// pick one — or pick <i>Pass</i> — and reveal at the same moment, on a count
/// of three. If you both picked the same thing, that's what happens. If you
/// picked differently, <b>nothing happens</b>, and you turn the next card
/// without discussing it.
/// </para>
///
/// <para>
/// <b>Why that's the whole point, and not a gimmick.</b> The other four
/// Intimate modes all rely on someone offering and someone accepting, which
/// means someone always has to say no out loud to the person they're in bed
/// with. That's a real cost, and it's exactly where people go along with
/// things. Here a no is <i>invisible</i>: a mismatch looks identical whether
/// your partner passed outright or just wanted the other option. Nobody has
/// to decline anything, nobody has to explain, and neither of you ever learns
/// which it was — so there is no read-the-room pressure to fake enthusiasm.
/// Enthusiasm isn't merely the only yes here; it's mechanically the only thing
/// that produces anything at all.
/// </para>
///
/// <para>
/// The two consequences worth naming to players, which the cards do:
/// a mismatch is a completely normal, frequent outcome and not a failure or a
/// rejection — most turns produce nothing, by design; and <i>Pass</i> is
/// always one of the choices on every single card, never a special move you
/// have to reach for.
/// </para>
///
/// <para>
/// Same consent spine as its siblings — safeword and edges agreed before the
/// first real card, opt-in language throughout, aftercare to close — because
/// the reveal mechanic protects against pressure, not against everything.
/// </para>
///
/// Adult (18+). For established, consenting partners only.
/// </summary>
public sealed class BothOrNeitherMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <summary>Two people. The reveal mechanic is meaningless with any other number.</summary>
    public TableShape SuitableFor => TableShape.Couple;

    /// <inheritdoc />
    public override string Name => "Both Or Neither";

    /// <inheritdoc />
    public override string Description =>
        "Three options a card, both of you pick in secret, reveal together — it only happens if you both chose it. A no is invisible, so nobody ever has to say one.";

    /// <summary>Label for a matched reveal — the only thing that produces anything.</summary>
    public override string CompleteLabel => "Both Chose It";

    /// <summary>Label for a mismatch. Deliberately not "Skip": nothing was declined, and nobody knows what happened.</summary>
    public override string SkipLabel => "No Match — Move On";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Consent"]     = "#26A69A",
            ["Opening"]      = "#FFCA28",
            ["Warmer"]       = "#FFA726",
            ["Serious"]      = "#EF5350",
            ["No Mistaking"] = "#AD1457",
            ["Aftercare"]    = "#7E57C2",
        };

    /// <summary>
    /// The consent ritual opens and aftercare closes, whatever the shuffle
    /// setting — a safety property of the deck, not a stylistic preference. A
    /// safeword has to exist before the cards it governs, and this mode needs
    /// its reveal rules explained before the first real card or the mechanic
    /// simply doesn't work.
    /// </summary>
    public override IReadOnlyList<string> CategoriesPinnedToStart => ["Consent"];

    /// <inheritdoc cref="CategoriesPinnedToStart" />
    public override IReadOnlyList<string> CategoriesPinnedToEnd => ["Aftercare"];

    /// <inheritdoc />
    protected override IScoringStrategy BuildScoring() => new DifficultyBasedScoringStrategy();

    /// <inheritdoc />
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        BothOrNeitherCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => BothOrNeitherCardBank.All;
}

/// <summary>
/// Built-in card bank for Both Or Neither. Authored with
/// <see cref="CardDeckBuilder"/> — deterministic ids from card content, so a
/// saved session still resolves its cards after a restart even on the C#
/// fallback path.
///
/// Ordered deliberately: consent ritual first, aftercare last, the tiers
/// rising in between. Every play card carries the same opt-in footer as its
/// sibling modes — repeated per card on purpose, not duplicated by accident:
/// nobody should have to remember a rule stated once twenty minutes ago while
/// half-undressed.
/// </summary>
public static class BothOrNeitherCardBank
{
    /// <summary>All cards, in intended play order.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private const string Footer =
        "\n\n<i>Pass is always one of your three. A mismatch is normal and means nothing — " +
        "you'll never know which of you it was, and that's the point. Call \"colour?\" anytime.</i>";

    /// <summary>Formats a three-option card. Pass is listed explicitly every time, never implied.</summary>
    private static string Options(string setup, string a, string b, string c) =>
        $"{setup}\n\n" +
        $"<b>A.</b> {a}\n" +
        $"<b>B.</b> {b}\n" +
        $"<b>C.</b> {c}\n" +
        $"<b>D.</b> Pass — turn the next card.\n\n" +
        "<i>Pick in secret. Reveal on three.</i>" +
        Footer;

    private static IReadOnlyList<ICard> Build() => CardDeckBuilder
        .For("Both Or Neither")

        // ── CONSENT — the opening ritual, and the rules this mode needs ───────
        .Category("Consent")
            .Card("Before Anything — Your Safeword",
                "Stop here and choose a safeword together — one word either of you can say to halt EVERYTHING instantly, no explanation owed and no guilt for killing the mood. Pick something you'd never say by accident (\"pineapple\" beats \"stop\"). Say it aloud now, twice, so it's real.\n\n" +
                "Agree a tap-out gesture too, for when words are hard. The moment either of you uses either one, the game is over — that's the deal that makes the rest of it safe.",
                Difficulty.Easy)
            .Card("Before Anything — Tonight's Edges",
                "Take turns. Each of you names at least one thing that is OFF the table tonight — a hard limit, a not-tonight, a don't-know-yet. No reasons required, and \"I'm not sure\" counts as an edge that means don't go there.\n\n" +
                "Then each name one thing you're genuinely hoping for. Nothing here is a promise — it's a map, so neither of you is guessing in the dark.",
                Difficulty.Easy)
            .Card("Before Anything — How The Reveal Works",
                "This is the whole game, so get it straight before the first real card.\n\n" +
                "Every card gives you three options plus Pass. You each pick one <b>privately</b> — fingers behind your back, or type it and turn the screen over. Then reveal together on a count of three.\n\n" +
                "<b>Matched?</b> That's what happens. <b>Didn't match?</b> Nothing happens, and you turn the next card <b>without discussing it</b> — no \"what did you pick?\", no negotiating. That rule is what makes a no cost nothing.",
                Difficulty.Easy)
            .Card("Before Anything — What A Mismatch Means",
                "One more thing, because it matters more than it sounds: <b>most turns will produce nothing.</b> Three options and a pass means matching is genuinely uncommon, and that is the design working, not the two of you failing.\n\n" +
                "A mismatch is not a rejection. You cannot tell a pass from a different pick, and you're not allowed to ask — so there's nothing to read into and no room to feel turned down. Enthusiasm is the only yes; here it's also the only thing that does anything at all.\n\n" +
                "Ready? Turn the next card only when you both are.",
                Difficulty.Easy)

        // ── OPENING — clothed, close, finding the range ───────────────────────
        .Category("Opening")
            .Card("How This Starts",
                Options("Where does tonight begin?",
                    "A long kiss, and nothing else for a full minute.",
                    "One of you undresses the other by exactly one layer.",
                    "Sit facing each other and say one thing you want, out loud."),
                Difficulty.Easy)
            .Card("Hands",
                Options("Still dressed. Where do the hands go?",
                    "Over your partner, slowly, everywhere you're welcome.",
                    "Held — just held, palm to palm, while you look at each other.",
                    "Nowhere yet. Sit close enough to touch and don't."),
                Difficulty.Easy)
            .Card("Say It",
                Options("Something gets said before anything else happens.",
                    "Whisper the filthiest true thing you're thinking.",
                    "Say the sweetest true thing you're thinking.",
                    "Say what you want to happen in the next ten minutes."),
                Difficulty.Easy)
            .Card("The Pace",
                Options("Set the tempo for what follows.",
                    "Deliberately slow — nothing rushed for the next while.",
                    "No restraint. Whatever pace it wants to go.",
                    "Stop-start: one of you decides when things pause."),
                Difficulty.Medium)
            .Card("Eyes",
                Options("What are you doing with your eyes?",
                    "Open, and on each other, for everything that follows.",
                    "One of you closes them and is guided by touch only.",
                    "Lights off or eyes shut for both of you."),
                Difficulty.Medium)

        // ── WARMER — clothes coming off, intent obvious ───────────────────────
        .Category("Warmer")
            .Card("Layers",
                Options("Clothing.",
                    "Both down to underwear, and stay there a while.",
                    "One of you fully undressed, the other still dressed.",
                    "You each remove one thing from the other, taking turns."),
                Difficulty.Medium)
            .Card("Mouth",
                Options("Somewhere gets kissed, properly.",
                    "Neck and shoulders, slowly, until they can't stay still.",
                    "Anywhere your partner names — they choose, you deliver.",
                    "Somewhere they haven't been kissed in a long time."),
                Difficulty.Medium)
            .Card("Who's Driving",
                Options("For the next stretch, one of you decides everything.",
                    "You do. They follow, and say if anything isn't wanted.",
                    "They do, and you follow on the same terms.",
                    "Neither — nobody leads, see where it goes."),
                Difficulty.Medium)
            .Card("Ask For It",
                Options("Something specific gets requested out loud.",
                    "Ask your partner for exactly what you want right now.",
                    "Ask them what they want and then do that instead.",
                    "Both of you ask, at the same time, and sort it out after."),
                Difficulty.Medium)
            .Card("Watched",
                Options("One of you has an audience.",
                    "Touch yourself while they watch, however you like.",
                    "Undress completely while they sit and don't touch you.",
                    "Tell them what you'd do if they weren't here."),
                Difficulty.Hard)
            .Card("Hold Off",
                Options("Something is deliberately not happening yet.",
                    "Everything but the obvious thing, for as long as you can stand.",
                    "Hands only — no mouths — until one of you gives in.",
                    "Nobody's allowed to escalate for five whole minutes."),
                Difficulty.Hard)

        // ── SERIOUS — explicit, no ambiguity about where this is going ────────
        .Category("Serious")
            .Card("Give",
                Options("One of you is entirely on the receiving end.",
                    "Them. You do whatever they ask for, they do nothing back.",
                    "You. Same deal, other way round.",
                    "Take it in turns — one, then the other, no overlap."),
                Difficulty.Hard)
            .Card("Mouth, Properly",
                Options("No euphemism on this one.",
                    "You go down on them, at their pace, until they say otherwise.",
                    "They do the same for you.",
                    "Both — at once, if that works for the two of you."),
                Difficulty.Hard)
            .Card("Position",
                Options("If you're going to, how?",
                    "Face to face, close enough to keep kissing.",
                    "However gets one of you there fastest.",
                    "Whichever one you always mean to try and never do."),
                Difficulty.Hard)
            .Card("Out Loud",
                Options("Noise.",
                    "Say what you're doing while you do it, in plain words.",
                    "As loud as you actually want to be — no holding back.",
                    "Completely silent. Nothing but breathing."),
                Difficulty.Hard)
            .Card("The Edge",
                Options("Someone gets taken close and held there.",
                    "Them — right to the edge, then stopped, more than once.",
                    "You, on the same terms.",
                    "Nobody's stopping anything. Straight through."),
                Difficulty.Extreme)
            .Card("Instructions",
                Options("One of you is talked through it.",
                    "You tell them exactly what to do to you, in detail, as it happens.",
                    "They tell you, and you do precisely that and nothing more.",
                    "Swap halfway, mid-sentence."),
                Difficulty.Extreme)

        // ── NO MISTAKING — the top of the deck, and it hands the wheel back ───
        .Category("No Mistaking")
            .Card("The Thing You Haven't Asked For",
                Options("Each of you is thinking of something. This is the card for it.",
                    "You say yours out loud, and you both decide together whether tonight's the night. Either answer is a good one.",
                    "They say theirs, same terms.",
                    "Both say them, then pick one — or neither, which is also fine."),
                Difficulty.Extreme)
            .Card("Your Way",
                Options("No card knows the two of you better than you do.",
                    "Set the deck aside entirely and do exactly what you both want.",
                    "Do the thing that has always worked, and give it your whole attention.",
                    "Whatever just happened — again, slower."),
                Difficulty.Extreme)
            .Card("Finish",
                Options("How does this end?",
                    "Together, if you can manage it.",
                    "One of you, properly seen to, then the other.",
                    "It doesn't have to. Stop here and go to aftercare."),
                Difficulty.Extreme)
            .Card("Stay",
                Options("Nothing new gets introduced.",
                    "Stay exactly in whatever this is — no escalating, no switching.",
                    "Go back to the best thing from earlier tonight and stay there.",
                    "Stop moving altogether and just be like this a while."),
                Difficulty.Extreme)

        // ── AFTERCARE — how you land, pinned last ─────────────────────────────
        .Category("Aftercare")
            .Card("Come Back Down",
                "No options on this one — you both do it. Stay close, get comfortable, and don't go anywhere for a few minutes. Water, a blanket, skin on skin, whatever the two of you actually want.\n\n" +
                "No performance and no talking about it yet if you don't want to.",
                Difficulty.Easy)
            .Card("The Kind Thing",
                "Each of you say one thing you loved about tonight. Specific, not general — \"when you did X\" rather than \"that was nice\".\n\n" +
                "Then, if either of you wants: one thing you'd do differently or want more of next time. Only if you want. Silence is a fine answer.",
                Difficulty.Easy)
            .Card("Anything Sitting Oddly?",
                "Last card. Check in properly: is anything sitting oddly, physically or otherwise? A no is a perfectly good answer, and so is bringing something up now rather than sitting on it for a week.\n\n" +
                "Whatever came up tonight, you both got here by choosing it — that's what the whole deck was for.",
                Difficulty.Easy)

        .Build();
}
