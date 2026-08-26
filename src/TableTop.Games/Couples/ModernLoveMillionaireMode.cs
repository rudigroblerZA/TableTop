using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Domain.Cards;

namespace TableTop.Games.Couples;

/// <summary>
/// Millionaire: Modern Love (18+) — the hot-seat quiz, rebuilt for couples:
/// fifteen rungs of dating-app slang, situationship vocabulary, and the
/// dictionary of modern romance. Do you actually know what the apps are
/// saying about you?
///
/// Play it as a couple: one of you takes the hot seat, the other plays host
/// (dramatic pauses mandatory). The ladder keeps the score in pounds; the
/// house recommends converting winnings at the standard exchange rate of
/// one kiss per £1,000 — walk away with £32,000 and collect accordingly.
///
/// Tone contract: every term here is mainstream dating-culture vocabulary —
/// the kind covered by newspaper trend pieces — defined accurately and
/// worded for laughs, never explicitly.
/// </summary>
public sealed class ModernLoveMillionaireMode : IGameMode, IQuestionBankProvider
{
    /// <inheritdoc />
    public string Name => "Millionaire: Modern Love";

    /// <inheritdoc />
    public string Description =>
        "Hot-seat quiz of dating-app slang — 15 rungs from 'ghosting' to the deep cuts. Host dramatically; convert winnings to kisses.";

    /// <inheritdoc />
    public IReadOnlyList<MultipleChoiceCard> GetQuestionBank() => ModernLoveQuestionBank.All;
}

/// <summary>Built-in question bank for Millionaire: Modern Love.</summary>
public static class ModernLoveQuestionBank
{
    /// <summary>All questions; the controller ladders them by difficulty.</summary>
    public static IReadOnlyList<MultipleChoiceCard> All { get; } = Build();

    private static IReadOnlyList<MultipleChoiceCard> Build() =>
    [
        // ── EASY (rungs 1–5): terms your parents have heard of ──────────────
        MultipleChoiceCard.Create(
            "'Ghosting' someone means…",
            "Ending all contact with zero explanation, vanishing like a Victorian spirit",
            "Dressing your situationship in a bedsheet for Halloween",
            "Texting them exclusively between midnight and 3 a.m.",
            "Liking their photos from a fake account",
            AnswerLabel.A, Difficulty.Easy, "Modern Love"),
        MultipleChoiceCard.Create(
            "A 'situationship' is…",
            "A long-distance relationship conducted by post",
            "A romantic arrangement with all of the feelings and none of the labels",
            "A couple who met in an emergency situation",
            "The government's official term for flatmates",
            AnswerLabel.B, Difficulty.Easy, "Modern Love"),
        MultipleChoiceCard.Create(
            "A 'thirst trap' is…",
            "A desert survival technique",
            "A pub with no water on the menu",
            "A deliberately alluring photo posted to harvest attention",
            "The third date, historically",
            AnswerLabel.C, Difficulty.Easy, "Modern Love"),
        MultipleChoiceCard.Create(
            "'Sliding into the DMs' means…",
            "A figure-skating move scored by judges",
            "Losing an argument in a group chat",
            "Entering a room dramatically in socks",
            "Opening a private-message flirtation, ideally with unearned confidence",
            AnswerLabel.D, Difficulty.Easy, "Modern Love"),
        MultipleChoiceCard.Create(
            "'Catfishing' is…",
            "Pretending to be someone else online, profile photos and all",
            "A first date at an aquarium",
            "Fishing for compliments with cat photos",
            "When both people order the seafood to seem adventurous",
            AnswerLabel.A, Difficulty.Easy, "Modern Love"),
        MultipleChoiceCard.Create(
            "Having 'rizz' means having…",
            "An allergy to commitment",
            "Effortless charisma — the ability to charm without visible effort",
            "More than three dating apps installed",
            "A carbonated personality",
            AnswerLabel.B, Difficulty.Easy, "Modern Love"),

        // ── MEDIUM (rungs 6–10): app-literate territory ──────────────────────
        MultipleChoiceCard.Create(
            "'Breadcrumbing' someone means…",
            "Cooking for them on the second date",
            "Following them home, fairy-tale style",
            "Sending just enough flirty attention to keep them interested, with no intention of committing",
            "Leaving crumbs in their bed as a dominance display",
            AnswerLabel.C, Difficulty.Medium, "Modern Love"),
        MultipleChoiceCard.Create(
            "'Cuffing season' is…",
            "The autumn–winter rush to lock in a relationship before the cold, cosy months",
            "A police recruitment drive",
            "When retailers discount bracelets",
            "The week after Valentine's Day",
            AnswerLabel.A, Difficulty.Medium, "Modern Love"),
        MultipleChoiceCard.Create(
            "A 'soft launch' of a partner is…",
            "Introducing them to your gentlest friend first",
            "Posting a cryptic photo — a hand, a second coffee — hinting someone exists without revealing them",
            "A relationship that begins in a bouncy castle",
            "Dating them at 25% intensity for a trial period",
            AnswerLabel.B, Difficulty.Medium, "Modern Love"),
        MultipleChoiceCard.Create(
            "'Love bombing' is…",
            "A flash mob at a proposal",
            "Sending one heart emoji per hour, on schedule",
            "Valentine's Day, as practised by supermarkets",
            "Overwhelming someone with excessive affection early on — a classic manipulation red flag",
            AnswerLabel.D, Difficulty.Medium, "Modern Love"),
        MultipleChoiceCard.Create(
            "'Benching' someone means…",
            "Keeping them as a backup option while you play the field — on the team, never in the game",
            "Working out together as a couple",
            "Making them sit out an argument",
            "Introducing them to your gym friends",
            AnswerLabel.A, Difficulty.Medium, "Modern Love"),
        MultipleChoiceCard.Create(
            "Getting 'the ick' means…",
            "A rash from a dating app's notification sounds",
            "A sudden, irreversible cringe that kills the attraction — often triggered by something tiny",
            "Matching with a coworker",
            "The flu you catch exactly three dates in",
            AnswerLabel.B, Difficulty.Medium, "Modern Love"),
        MultipleChoiceCard.Create(
            "A 'sneaky link' is…",
            "A suspicious URL in a dating bio",
            "A golf course affair",
            "A secret romantic rendezvous — the person you see quietly, no posts, no announcements",
            "The friend who set you up and takes credit forever",
            AnswerLabel.C, Difficulty.Medium, "Modern Love"),

        // ── HARD (rungs 11–14): the deep app cuts ────────────────────────────
        MultipleChoiceCard.Create(
            "'Orbiting' is when an ex…",
            "Moves exactly one postcode away",
            "Dates only people from your friend group",
            "Keeps appearing in your recommended playlists",
            "Ghosts you but keeps watching every story and liking posts — present in orbit, never landing",
            AnswerLabel.D, Difficulty.Hard, "Modern Love"),
        MultipleChoiceCard.Create(
            "'Pocketing' your partner means…",
            "Hiding them from your friends, family, and feeds — dating them entirely off the record",
            "Paying for every date from one designated pocket",
            "Keeping their photo in your wallet like it's 1953",
            "Borrowing their hoodie and never returning it",
            AnswerLabel.A, Difficulty.Hard, "Modern Love"),
        MultipleChoiceCard.Create(
            "'Kittenfishing' is…",
            "Catfishing, but only using photos of your cat",
            "Mild profile fraud — real you, but older photos, generous measurements, borrowed hobbies",
            "Adopting a pet together too early",
            "Flirting exclusively via cat memes",
            AnswerLabel.B, Difficulty.Hard, "Modern Love"),
        MultipleChoiceCard.Create(
            "'Zombieing' is when someone…",
            "Only texts during horror films",
            "Dates you for your Netflix password",
            "Ghosts you, then rises from the dead months later with a casual 'hey you' as if nothing happened",
            "Walks noticeably slower once in a relationship",
            AnswerLabel.C, Difficulty.Hard, "Modern Love"),
        MultipleChoiceCard.Create(
            "'Cushioning' means…",
            "Only dating people with comfortable sofas",
            "Softening a breakup with baked goods",
            "Padding your texts with emojis to seem warmer",
            "Keeping a roster of flirtations on standby to soften the blow if your main relationship fails",
            AnswerLabel.D, Difficulty.Hard, "Modern Love"),
        MultipleChoiceCard.Create(
            "A 'beige flag' is…",
            "A trait that's not bad, just bafflingly boring — a quirk you screenshot for the group chat",
            "The flag of a neutral country, romantically speaking",
            "When their entire wardrobe is one colour",
            "A warning that they describe themselves as 'a foodie'... which is also answer A, honestly",
            AnswerLabel.A, Difficulty.Hard, "Modern Love"),
        MultipleChoiceCard.Create(
            "'Paperclipping' is when someone…",
            "Sends you their CV on the second date",
            "Pops back into your messages periodically for an ego boost, with zero intention of meeting — like that old office assistant nobody asked for",
            "Attaches themselves to your friend group after the breakup",
            "Organises the relationship in a shared spreadsheet",
            AnswerLabel.B, Difficulty.Hard, "Modern Love"),

        // ── EXTREME (rung 15): the million-kiss question ─────────────────────
        MultipleChoiceCard.Create(
            "'Groundhogging' means…",
            "Dating someone who only commits in February",
            "Hibernating together from November to March",
            "Dating the same type over and over, expecting different results — and being shocked, every time",
            "Re-running the identical first-date itinerary with every match",
            AnswerLabel.C, Difficulty.Extreme, "Modern Love"),
        MultipleChoiceCard.Create(
            "'Wokefishing' is…",
            "Pretending to hold progressive values to reel someone in, then revealing the catch later",
            "Fishing with sustainably sourced bait",
            "Only matching with people who post infographics",
            "Waking your partner up to debate at 2 a.m.",
            AnswerLabel.A, Difficulty.Extreme, "Modern Love"),
        MultipleChoiceCard.Create(
            "A 'textationship' is…",
            "A relationship conducted primarily in typos",
            "When your phones are dating but you aren't",
            "Any couple who met over SMS before 2010",
            "A connection that lives entirely in messages — endless chemistry in the chat, zero plans in the calendar",
            AnswerLabel.D, Difficulty.Extreme, "Modern Love"),
        MultipleChoiceCard.Create(
            "'Roaching' is when someone…",
            "Hides that they're dating several people, and when confronted, claims you never asked",
            "Moves into your flat one toothbrush at a time",
            "Survives every breakup attempt you make",
            "Scatters when you turn the lights on at their place",
            AnswerLabel.A, Difficulty.Extreme, "Modern Love"),
    ];
}
