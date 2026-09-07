using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Restrictions;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games;

/// <summary>
/// Truth or Dare — the party classic, dealt the way the physical card game
/// plays:
///
///   1. The READER (player to your left) draws and asks: "Truth or dare?"
///   2. You declare OUT LOUD before hearing either option. No take-backs.
///   3. The reader reads only your chosen half. You do it — or you invoke
///      the card's chicken clause and pay its forfeit.
///
/// Every card carries BOTH a truth and a dare of matched difficulty, plus its
/// own forfeit, so declaring blind is a genuine gamble — which is the whole
/// game. Higher difficulties score more (DifficultyBasedScoringStrategy), so
/// the deck rewards the brave.
///
/// <para>
/// <b>Declaring is enforced, not just trusted.</b> Every card's text follows
/// the <c>TableTop.Hosting.TruthOrDareCards</c> convention — an intro,
/// then a line starting <c>TRUTH:</c>, then one starting <c>DARE:</c>, then a
/// <c>Chicken clause:</c> forfeit. The shared gameplay screen (and Console)
/// detect that shape and hide both halves behind a "Truth or Dare?" choice,
/// revealing only the one declared — no head needs mode-specific code for
/// this, and a head that predates the convention still shows the intro plus
/// both halves rather than breaking.
/// </para>
///
/// A restricted subset (couples-only, adults-only, gender-directed) mixes in
/// automatically when the table qualifies, which is why this one mode serves
/// both the Teen party node and the Adult couples node.
/// </summary>
public sealed class TruthOrDareMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Truth or Dare";
    /// <inheritdoc />
    public override string Description =>
        "Declare truth or dare BEFORE you hear it — every card holds both, plus a chicken clause. Gender-directed prompts included.";

    /// <summary>Label for the button that records a completed truth/dare.</summary>
    public override string CompleteLabel => "Did It";
    /// <summary>Label for the button that invokes the chicken clause.</summary>
    public override string SkipLabel => "Chickened Out";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [TruthOrDareCardBank.ClassicsCategory] = "#42A5F5",
            [TruthOrDareCardBank.SpotlightCategory] = "#AB47BC",
            [TruthOrDareCardBank.ChaosCategory] = "#FFA726",
            [TruthOrDareCardBank.HotSeatCategory] = "#EF5350",
            [TruthOrDareCardBank.LegendsCategory] = "#B71C4A",
        };

    /// <summary>Braver picks score more.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Builds the paired-card deck, including the restricted subset.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        TruthOrDareCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => TruthOrDareCardBank.All;
}

/// <summary>
/// Built-in paired-card bank for Truth or Dare, authored with
/// <see cref="CardDeckBuilder"/>'s fluent DSL. Every card's body is composed
/// by <see cref="Pair"/> in the shape <c>TableTop.Hosting.TruthOrDareCards</c>
/// knows how to split back apart at play time.
/// </summary>
public static class TruthOrDareCardBank
{
    internal const string ClassicsCategory = "Classics";
    internal const string SpotlightCategory = "Spotlight";
    internal const string ChaosCategory = "Chaos";
    internal const string HotSeatCategory = "Hot Seat";
    internal const string LegendsCategory = "Legends";

    private const string Deck = "Truth or Dare";
    private const string CardLabel = "Truth or Dare";

    /// <summary>All truth-or-dare cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build()
    {
        var adultsOnly = new AdultOnlyRestriction();
        var couplesOnly = new CoupleOnlyRestriction();

        var deck = CardDeckBuilder.For(Deck)

            // ── CLASSICS — the warm-up shuffle ───────────────────────────────
            .Category(ClassicsCategory)
            .Card(CardLabel, Pair(
                "What's the most embarrassing thing that happened to you as a kid?",
                "Do your best impression of another player until someone guesses who.",
                "you owe the group one round of applause for yourself, standing"),
                Difficulty.Easy)
            .Card(CardLabel, Pair(
                "What's your guiltiest pleasure — the one you'd deny in public?",
                "Speak in a terrible posh accent until your next turn.",
                "the group picks your accent for the NEXT two turns instead"),
                Difficulty.Easy)
            .Card(CardLabel, Pair(
                "What's the worst haircut, outfit, or phase you've ever committed to?",
                "Let the player to your right restyle your hair right now. It stays.",
                "you must show the group your oldest surviving photo of yourself"),
                Difficulty.Easy)
            .Card(CardLabel, Pair(
                "Reveal a secret talent nobody at this table knows about.",
                "Demonstrate ANY talent for 20 seconds. Confidence counts as talent.",
                "hum your own sad exit music while doing a lap of the room"),
                Difficulty.Easy)
            .Card(CardLabel, Pair(
                "What's the worst lie you've ever told — and did it work?",
                "Tell a 30-second story that's a complete lie; the group votes if it was convincing.",
                "you must answer the NEXT truth asked of anyone, honestly, as a bonus"),
                Difficulty.Easy)
            .Card(CardLabel, Pair(
                "What food do you pretend to like in social situations?",
                "Eat a spoonful of a condiment chosen by the group (from what's actually in the kitchen).",
                "you fetch snacks for the whole table, taking orders"),
                Difficulty.Easy)
            .Card(CardLabel, Pair(
                "What's the most childish thing you still do — and love?",
                "Play the rest of this round sitting on the floor like it's story time.",
                "your chair is gone for one round anyway, AND you lose the moral high ground"),
                Difficulty.Easy)
            .Card(CardLabel, Pair(
                "What song do you secretly know every single word to?",
                "Sing the chorus of any song — committed, full volume, air instruments included.",
                "the group picks the song and you HUM it with feeling"),
                Difficulty.Medium)

            // ── SPOTLIGHT — performance pieces ───────────────────────────────
            .Category(SpotlightCategory)
            .Card(CardLabel, Pair(
                "What's a moment you were secretly VERY proud of but never told anyone?",
                "Deliver a dramatic Oscar acceptance speech for the most mundane thing you did today.",
                "the group writes your acceptance speech and you read it verbatim"),
                Difficulty.Medium)
            .Card(CardLabel, Pair(
                "Who at this table would you trade lives with for a week, and why?",
                "Swap seats and IDENTITIES with the player opposite for the next two rounds — answer as them.",
                "they get to answer YOUR next truth for you"),
                Difficulty.Medium)
            .Card(CardLabel, Pair(
                "What's your most-used excuse — the one you keep in your back pocket?",
                "Sell the group an object within arm's reach like a late-night TV host. 45 seconds. They vote: sold or not.",
                "you must genuinely compliment each player's haggling skills, individually"),
                Difficulty.Medium)
            .Card(CardLabel, Pair(
                "Describe your worst date ever — no name needed, all details welcome.",
                "Reenact, solo, both sides of a disastrous first-date conversation.",
                "the player to your left narrates their GUESS of your worst date and you may not correct them"),
                Difficulty.Medium)
            .Card(CardLabel, Pair(
                "What's the weirdest thing you've ever googled at 2 a.m.?",
                "Hand your phone to the player on your right; they read your three most recent emoji aloud, with interpretive commentary.",
                "you describe your search history's general 'vibe' in three honest words"),
                Difficulty.Hard)
            .Card(CardLabel, Pair(
                "What compliment do you fish for most often?",
                "Walk the room like a runway model while the group provides fashion-week commentary.",
                "each player gives you the compliment you clearly wanted — sarcastically"),
                Difficulty.Medium)
            .Card(CardLabel, Pair(
                "If your life had a blooper reel, what moment is definitely on it?",
                "Reenact your most recent clumsy moment in slow motion with sound effects.",
                "the table reenacts how they IMAGINE it went and you must applaud"),
                Difficulty.Medium)

            // ── CHAOS — the table gets involved ──────────────────────────────
            .Category(ChaosCategory)
            .Card(CardLabel, Pair(
                "Rank everyone at this table by who'd survive longest in a zombie film. Justify last place.",
                "The group strikes a pose; you have 10 seconds to memorise it, then recreate ALL of them in sequence.",
                "you're officially first eaten in every hypothetical from now on"),
                Difficulty.Medium)
            .Card(CardLabel, Pair(
                "Which player's phone would be the most incriminating to read aloud, and why do you think so?",
                "Trade one shoe with the player across from you. Wear it until the deck says otherwise.",
                "BOTH your shoes go in the middle of the table as a monument to cowardice"),
                Difficulty.Medium)
            .Card(CardLabel, Pair(
                "What's a group opinion this table holds that you secretly disagree with?",
                "For the next three rounds, you must agree — enthusiastically — with everything anyone says.",
                "the group assigns you an opinion and you must defend it for one minute"),
                Difficulty.Hard)
            .Card(CardLabel, Pair(
                "Who at this table texts back the slowest, and what's your theory about why?",
                "Send a (harmless, group-approved) text to the last non-player person you messaged, dictated by the table.",
                "the table drafts the text they WOULD have sent and reads it aloud"),
                Difficulty.Hard)
            .Card(CardLabel, Pair(
                "If this friend group had a reality show, what would the season-one scandal be?",
                "Improvise the reality-show confessional interview about the player to your left. Camera three is imaginary but unblinking.",
                "you're the scandal now — the group writes the headline"),
                Difficulty.Medium)
            .Card(CardLabel, Pair(
                "What's something everyone here does that secretly drives you a little mad?",
                "The group invents a brand-new rule for the game right now; it applies only to you.",
                "TWO rules. They enjoy this too much."),
                Difficulty.Hard)

            // ── HOT SEAT — squirm-grade truths, blush-grade dares ────────────
            .Category(HotSeatCategory)
            .Card(CardLabel, Pair(
                "What's the pettiest grudge you are actively still holding?",
                "Call the player who most recently beat you at anything and formally, flowerily concede.",
                "you must publicly forgive the grudge — naming it counts"),
                Difficulty.Hard)
            .Card(CardLabel, Pair(
                "What's the closest you've come to getting caught doing something you shouldn't?",
                "Confess a small, real, never-admitted thing to the group's chosen 'judge', who assigns community service (one silly task).",
                "the judge assigns the task anyway, doubled, with a gavel sound"),
                Difficulty.Hard)
            .Card(CardLabel, Pair(
                "Whose approval do you want most — and does that person know?",
                "Text someone (group-approved) a sincere compliment right now and show the send screen.",
                "you give that compliment to every player here instead, maintaining eye contact"),
                Difficulty.Hard)
            .Card(CardLabel, Pair(
                "What's the most trouble you ever got into that your parents STILL don't know about?",
                "Let the group scroll exactly one screen of your camera roll (you pick the decade, they pick the direction).",
                "you describe the single worst photo of you in existence, in loving detail"),
                Difficulty.Extreme)
            .Card(CardLabel, Pair(
                "What's a promise you broke that still bothers you?",
                "Make one real, small promise to a player of the group's choosing — witnessed, dated, and enforceable at the next game night.",
                "the group sets the promise AND the penalty for breaking it"),
                Difficulty.Hard)

            // ── LEGENDS — the cards people talk about next week ──────────────
            .Category(LegendsCategory)
            .Card(CardLabel, Pair(
                "What is your single most embarrassing moment — the crown jewel, the one you'd delete from history?",
                "The group has one minute to design a dare using only what's in this room. You've already agreed.",
                "you tell the SECOND most embarrassing moment AND do a lap of honour"),
                Difficulty.Extreme)
            .Card(CardLabel, Pair(
                "If everyone here heard your internal monologue for one hour today, what would you owe apologies for?",
                "Perform one minute of interpretive dance titled 'My Week'. The group must guess three events from it.",
                "the group performs 'Your Week' AS THEY IMAGINE IT and you may not defend yourself"),
                Difficulty.Extreme)
            .Card(CardLabel, Pair(
                "What's the biggest risk you never took — and what do you think was on the other side of it?",
                "Do the thing you always say you'd do 'if I wasn't so embarrassed' — right now, 30-second version.",
                "you must toast, out loud, to the risk you'll take before next game night. Witnessed."),
                Difficulty.Extreme)
            .Card(CardLabel, Pair(
                "Tell the story you've been saving — the one that starts 'okay but you can't tell anyone'.",
                "Trust fall. The group catches. (The group MUST catch. That's the real dare and it's theirs.)",
                "you owe the story at the NEXT game night, and it accrues interest"),
                Difficulty.Extreme)

            // ── RESTRICTED SUBSET — appears only when the table qualifies ────
            .Card(CardLabel, Pair(
                "What did you ACTUALLY think after your first date with your partner?",
                "Recreate your partner's most characteristic gesture until they admit it's accurate.",
                "your partner answers the truth FOR you, and their version is now canon"),
                Difficulty.Medium, restriction: couplesOnly)
            .Card(CardLabel, Pair(
                "What's one thing your partner does that you'll never admit you find adorable? Admit it.",
                "Serenade your partner with 15 seconds of any song, hand on heart, full sincerity.",
                "your partner picks the song and conducts you"),
                Difficulty.Hard, restriction: couplesOnly)
            .Card(CardLabel, Pair(
                "What's the real story of your wildest night out — the unabridged edition?",
                "Reenact, PG-13 and solo, the dance move that defined your going-out era.",
                "the group rates your going-out era from its surviving photos. You provide one."),
                Difficulty.Hard, restriction: adultsOnly)
            .Card(CardLabel, Pair(
                "What's the most money you've ever spent on something you never told anyone about?",
                "Show the group your most shameful recent purchase in your order history (one item, your pick of app).",
                "the group guesses the amount, loudly, until you confirm hot or cold"),
                Difficulty.Extreme, restriction: adultsOnly)

            .Build();

        // Gender-directed prompt (kept from the original deck's promise). Not
        // a TRUTH/DARE pair — a single either-or line — so it's built directly
        // rather than through CardDeckBuilder, which only produces StandardCards.
        var genderDirected = PromptCard.CreateGenderDirected(
            title: "Style Regret",
            maleText: "What men's fashion trend did you fully commit to that you now regret? Truth — or DARE: recreate it with whatever's in this room.",
            femaleText: "What beauty or fashion trend did you fully commit to that you now regret? Truth — or DARE: recreate it with whatever's in this room.",
            otherText: "What fashion trend did you fully commit to that you now regret? Truth — or DARE: recreate it with whatever's in this room.",
            difficulty: Difficulty.Medium,
            category: SpotlightCategory);

        return [.. deck, genderDirected];
    }

    /// <summary>
    /// Composes one card's body in the shape
    /// <c>TableTop.Hosting.TruthOrDareCards</c> parses: an intro, a
    /// <c>TRUTH:</c> line, a <c>DARE:</c> line, then the chicken-clause
    /// forfeit — so the gameplay screen can hide both halves until the player
    /// declares one out loud, same as the physical game.
    /// </summary>
    private static string Pair(string truth, string dare, string forfeit) =>
        "The reader asks: \"Truth or dare?\" — declare OUT LOUD before hearing either.\n\n" +
        "TRUTH: " + truth + "\n\n" +
        "DARE: " + dare + "\n\n" +
        "Chicken clause: back out after hearing your pick, and " + forfeit + ".";
}
