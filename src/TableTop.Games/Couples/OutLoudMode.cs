using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Out Loud (18+) — the whole game is saying it.
///
/// Every other intimate mode in the catalogue escalates what you <i>do</i>:
/// Monogamy through four zones, Heat Check by temperature, Undivided by whose
/// turn it is to receive. None of them escalates what you're willing to
/// <i>say</i>. This one has no other axis. You never draw a card that asks you
/// to act — only to speak, to their face, in your own words.
///
/// The wager behind it: most couples find saying it far harder than doing it.
/// Doing has momentum and the dark to hide in; saying has neither. A deck that
/// makes articulation the whole challenge is the hottest thing on the table
/// while you're playing, and the only one that leaves you with something you
/// can still use on an ordinary Tuesday.
///
/// How to play:
///   1. Draw. The card tells you what to say, not what to do.
///   2. <b>The Specificity Rule.</b> Vague doesn't count, and your partner is
///      the only judge. "Something nice about you" fails. The actual sentence
///      passes. If they say "be more specific", you owe them a better one.
///   3. Say it out loud. To their face. Not written down, not muttered at the
///      ceiling, not "…you know what I mean." You do not get to gesture.
///   4. Passing is free, silent, and costs nothing. No explanation is owed and
///      none should be asked for — that is exactly what keeps the top of the
///      deck honest rather than performed.
///
/// The four tiers are how frankly you're required to speak, not how far things
/// go:
///
///   <b>Admissions</b> — true and specific, about them.
///   <b>Confessions</b> — a want you've never said aloud.
///   <b>Direction</b>  — tell them exactly what to do. They do it. You keep talking.
///   <b>Narration</b>  — describe it while it's happening.
///
/// The last card is a Closer, pinned to the end of the deck: the deck stops
/// asking and hands the conversation back.
/// </summary>
public sealed class OutLoudMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <summary>Two people. The whole premise is one voice and one listener.</summary>
    public TableShape SuitableFor => TableShape.Couple;

    /// <inheritdoc />
    public override string Name        => "Out Loud";

    /// <inheritdoc />
    public override string Description =>
        "The whole game is saying it — out loud, to their face, in your own words. " +
        "Four tiers of how frankly you're willing to speak. Vague doesn't count.";

    /// <summary>Recorded when the words actually left your mouth.</summary>
    public override string CompleteLabel => "Said It";

    /// <summary>Free, silent, and costs nothing — which is what keeps the top of the deck honest.</summary>
    public override string SkipLabel     => "Couldn't Say It";

    /// <summary>
    /// The Closer sits at the end: after it, the deck stops asking and the two
    /// of you carry on without prompts. Pinning it means a shuffle can't strand
    /// it in the middle, where it would read as just another card.
    /// </summary>
    public override IReadOnlyList<string> CategoriesPinnedToEnd => ["Closer"];

    /// <summary>Category → hex colour, warming as the deck asks for more.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Admissions"]  = "#D68FA8",   // soft rose — the warm-up
            ["Confessions"] = "#C2557A",   // deeper
            ["Direction"]   = "#A02D5B",   // command register
            ["Narration"]   = "#6E1338",   // the dark end
            ["Closer"]      = "#C49E4C",   // house gold, so it reads as an ending
        };

    /// <inheritdoc />
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        OutLoudCardBank.All;

    /// <summary>
    /// Scoreless, like Heat Check. A number on the screen would turn "who was
    /// willing to say more" into a competition, which is the one thing this deck
    /// must not become — the pass has to stay genuinely free.
    /// </summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);
}

/// <summary>
/// Compiled fallback for <see cref="OutLoudMode"/>, used only when the JSON is
/// absent from a stripped publish. A static list, so the card ids are stable
/// across runs — an unpinned deck re-deals cards the table has already heard
/// after a resume, which in this mode means asking someone to confess the same
/// thing twice.
/// </summary>
internal static class OutLoudCardBank
{
    private static ICard Card(string tier, string body, Difficulty difficulty, string note) =>
        new StandardCard(
            id:          StableId(tier, body),
            title:       tier,
            description: $"<b>{body}</b>\n\n<i>{note}</i>",
            difficulty:  difficulty,
            category:    tier,
            // Card-level gate. The mode already declares TableShape.Couple and the
            // Intimate node carries AgeRating.Adult, so "adult" alone matches every
            // other adult deck rather than inventing a second convention.
            restriction: TableTop.Core.Domain.Decks.RestrictionParser.Parse("adult"));

    // Deterministic id from the card's own text, so the bank and the exported
    // JSON agree and neither drifts when cards are reordered.
    private static Guid StableId(string tier, string body) =>
        new(System.Security.Cryptography.MD5.HashData(
            System.Text.Encoding.UTF8.GetBytes($"out-loud|{tier}|{body}")));

    /// <summary>Every card, in authored order. The Closer is pinned to the end by the mode.</summary>
    public static readonly IReadOnlyList<ICard> All =
    [
        // ── Admissions — true, specific, about them ───────────────────────────
        Card("Admissions", "Tell them the exact moment you knew you wanted them. Not the story you usually tell — the actual moment, with the room in it.", Difficulty.Easy,
             "If they've heard this one before, they get to ask for a different moment."),
        Card("Admissions", "Name one thing they do when they think you aren't looking, and tell them what it does to you.", Difficulty.Easy,
             "Specificity Rule: name the thing, not the category."),
        Card("Admissions", "Describe their hands as if you were describing them to someone who has never seen a pair.", Difficulty.Easy,
             "You may not use the word 'nice'."),
        Card("Admissions", "Tell them the last time you were genuinely, uncomplicatedly proud of them — and why.", Difficulty.Easy,
             "This one is allowed to make somebody cry."),
        Card("Admissions", "Say what you'd miss first. Not what you'd miss most — what you'd notice missing on day one.", Difficulty.Easy,
             "The small answer is the better answer here."),
        Card("Admissions", "Finish this out loud: \"The thing I've never told you I noticed is…\"", Difficulty.Easy,
             "Then let it land. Don't rescue the silence."),
        Card("Admissions", "Tell them the physical thing about them you noticed first — and whether it's still the first thing you notice now.", Difficulty.Easy,
             "If the answer changed, say what it changed to."),
        Card("Admissions", "Say the compliment you keep meaning to give them and keep forgetting to.", Difficulty.Easy,
             "The forgetting is real. Say it anyway."),
        Card("Admissions", "Name a moment from this month you felt lucky to be with them, and say exactly what made it that.", Difficulty.Easy,
             "'Just because' fails the Specificity Rule. There's a real reason — find it."),
        Card("Admissions", "Tell them one thing about their voice you could pick out of a crowded room.", Difficulty.Easy,
             "Not the obvious answer. The actual one."),
        Card("Admissions", "Describe, out loud, the exact face they make right before they laugh.", Difficulty.Easy,
             "If they make it right now, that's not cheating. That's the card working."),
        Card("Admissions", "Tell them something you do differently since you've been with them — and that it's a good thing.", Difficulty.Easy,
             "Small counts. Smaller is often truer."),

        // ── Confessions — a want you've never said aloud ──────────────────────
        Card("Confessions", "Say one thing you've wanted for a while and never asked for. Just the sentence. No preamble, no apology, no 'this is silly, but'.", Difficulty.Medium,
             "The preamble is the part you're hiding behind. Cut it."),
        Card("Confessions", "Tell them something you think about when they're not in the room.", Difficulty.Medium,
             "You choose how far. You don't get to choose vague."),
        Card("Confessions", "Name the compliment you'd most like to be given, in the words you'd most like to hear it in.", Difficulty.Medium,
             "They may use it immediately, or save it. Both are fair."),
        Card("Confessions", "Describe the last time you wanted them and said nothing. Where you were, what stopped you.", Difficulty.Medium,
             "'What stopped you' is the half that matters."),
        Card("Confessions", "Tell them one thing you'd like more of, and one thing you'd like less of. In that order, and be exact.", Difficulty.Medium,
             "Less-of is harder to say and worth more to hear. Say both."),
        Card("Confessions", "Say out loud the thing you'd only ever say with the lights off.", Difficulty.Medium,
             "Lights stay on. That's the card."),
        Card("Confessions", "Tell them what you think they're best at, in bed, in words you'd never normally use in a lit room.", Difficulty.Medium,
             "They are allowed to ask you to say it again."),
        Card("Confessions", "Tell them a version of a fantasy you've had about them. Soften it as much or as little as you like — but say it.", Difficulty.Medium,
             "A softened true answer beats a vivid invented one. Stay honest."),
        Card("Confessions", "Say what you'd ask for if asking cost you nothing — no embarrassment, no chance of being told no.", Difficulty.Medium,
             "The cost is imaginary. Ask like it is."),
        Card("Confessions", "Tell them what you think about right before you fall asleep next to them.", Difficulty.Medium,
             "Last night counts. So does most nights."),
        Card("Confessions", "Name the thing you've wanted more of lately, and be honest about why you haven't said it yet.", Difficulty.Medium,
             "The why is the harder half of this card. Don't skip it."),
        Card("Confessions", "Tell them one thing that surprised you about wanting them — something you didn't expect to want.", Difficulty.Medium,
             "Surprise, not shame. If it comes out apologetic, say it again without the apology."),
        Card("Confessions", "Say the line you've rehearsed in your head and never actually said out loud.", Difficulty.Medium,
             "You know the one. Say it now, badly if you have to."),

        // ── Direction — tell them exactly what to do, and keep talking ────────
        Card("Direction", "Tell them where to put their hands. Be precise enough that they don't have to ask. They do it; you keep talking.", Difficulty.Hard,
             "Running out of words ends the card. Try not to."),
        Card("Direction", "Give them one instruction, then correct them twice — out loud, kindly, until it's exactly right.", Difficulty.Hard,
             "The correcting is the card. Most people skip straight to grateful."),
        Card("Direction", "Tell them to slow down. Then keep telling them, in new words, for a full minute.", Difficulty.Hard,
             "New words each time. 'Slower' four times doesn't count."),
        Card("Direction", "Ask for exactly what you want, in a full sentence, using the words you actually mean rather than the polite ones.", Difficulty.Hard,
             "If you soften it, they get to ask for the unsoftened version."),
        Card("Direction", "Tell them one thing to do and one thing not to do yet. Hold the 'not yet' as long as you can stand.", Difficulty.Hard,
             "You end the 'not yet' out loud, too. No nodding."),
        Card("Direction", "Take their hand and narrate where it's going before it gets there.", Difficulty.Hard,
             "Arriving before the sentence does is a pass, not a win."),
        Card("Direction", "Tell them to stay exactly where they are, and then say why you want them there.", Difficulty.Hard,
             "The why is the whole card."),
        Card("Direction", "Choose a pace out loud, then hold them to it out loud, every single time it slips.", Difficulty.Hard,
             "Naming the slip is the instruction. Naming it kindly is the skill."),
        Card("Direction", "Give them one word to listen for. Explain out loud what it will mean before you use it.", Difficulty.Hard,
             "Then use it. Silence after they hear it is not a valid response from either of you."),
        Card("Direction", "Set one rule for the next few minutes and state it plainly. They may only break it if you say so — out loud.", Difficulty.Hard,
             "You have to actually say so. A look doesn't count."),
        Card("Direction", "Tell them to ask permission before touching anywhere specific — and answer out loud, every time they ask.", Difficulty.Hard,
             "Silence is not an answer. Say yes or say not yet."),
        Card("Direction", "Give an instruction, then immediately say out loud why you wanted that and not something else.", Difficulty.Hard,
             "The reason is worth more than the instruction was."),

        // ── Narration — describe it while it happens ──────────────────────────
        Card("Narration", "Say what you're doing while you do it. Present tense. Don't stop when it gets difficult to talk.", Difficulty.Extreme,
             "The difficulty is the point. Whisper if you must — but keep going."),
        Card("Narration", "Describe what you can see, in detail, without stopping for as long as they can take it.", Difficulty.Extreme,
             "They call time, not you."),
        Card("Narration", "Tell them what you're about to do, in full, and then make them wait through the whole description before you do any of it.", Difficulty.Extreme,
             "No skipping ahead. The waiting is the card."),
        Card("Narration", "Say what you want to happen next — not what's happening, what's next. Keep saying it until it does.", Difficulty.Extreme,
             "Future tense only. It's harder than it sounds."),
        Card("Narration", "Describe them to themselves, right now, exactly as you're seeing them.", Difficulty.Extreme,
             "Eye contact if you can manage it. This one is worth managing it for."),
        Card("Narration", "Ask them to tell you what they want, and don't do anything at all until they've said it in a full sentence.", Difficulty.Extreme,
             "You wait. However long it takes. That's your half of this card."),
        Card("Narration", "Narrate what you're feeling, not just what you're doing — name the actual physical sensation as it happens.", Difficulty.Extreme,
             "'Good' is not a sensation. Find the real word."),
        Card("Narration", "Describe out loud exactly what changes in their face as things go on.", Difficulty.Extreme,
             "You have to actually be watching for this one."),
        Card("Narration", "Say what sound you want to hear next, before it happens — then say out loud when you get it.", Difficulty.Extreme,
             "Both halves are required. Asking without confirming is only half the card."),
        Card("Narration", "Narrate exactly where your attention is, moment to moment, without going quiet.", Difficulty.Extreme,
             "Quiet ends the card. Keep talking, even in fragments."),
        Card("Narration", "Describe out loud what you plan to do in the next thirty seconds, in full — then do exactly what you said.", Difficulty.Extreme,
             "No improvising once you've said it. The sentence is the contract."),
        Card("Narration", "Say out loud the moment you stop being able to talk — and try to keep going ten more seconds past that.", Difficulty.Extreme,
             "Ten seconds is a long time here. That's the point of the card."),

        // ── Closer — pinned to the end. Three, so the last card isn't always the
        //    same one; ApplyPinnedCategories keeps their authored relative
        //    order, so which one lands last still depends only on how many the
        //    session actually draws to.
        Card("Closer", "Last card. Tell them one thing from tonight you want to happen again — and say when.", Difficulty.Easy,
             "Then put the deck down. It has run out of questions and you clearly haven't."),
        Card("Closer", "Last card. Say one thing tonight taught you about each other that you didn't know this morning.", Difficulty.Easy,
             "Learning something counts even if it's small. Especially if it's small."),
        Card("Closer", "Last card. Name the version of tonight you'd want to repeat, word for word if you could — then decide together when.", Difficulty.Easy,
             "An actual when. Not 'sometime.'"),
    ];
}
