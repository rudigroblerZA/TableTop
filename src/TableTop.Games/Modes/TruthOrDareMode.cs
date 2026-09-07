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
/// <see cref="CardDeckBuilder"/>'s fluent DSL. Each card is a plain
/// <see cref="CardDeckBuilder.Card"/> carrying the intro, then
/// <see cref="CardDeckBuilder.WithPreActions"/> folds in the declare-gate —
/// which composes exactly the shape <c>TableTop.Hosting.TruthOrDareCards</c>
/// splits back apart at play time.
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

    private const string Intro =
        "The reader asks: \"Truth or dare?\" — declare OUT LOUD before hearing either.";

    /// <summary>All truth-or-dare cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build()
    {
        var adultsOnly = new AdultOnlyRestriction();
        var couplesOnly = new CoupleOnlyRestriction();

        var deck = CardDeckBuilder.For(Deck)

            // ── CLASSICS — the warm-up shuffle ───────────────────────────────
            .Category(ClassicsCategory)
            .Card(CardLabel, Intro, Difficulty.Easy).WithPreActions(a => a
                .AddButton("Truth", "What's the most embarrassing thing that happened to you as a kid?")
                .AddButton("Dare", "Do your best impression of another player until someone guesses who.")
                .AddFooter(Forfeit("you owe the group one round of applause for yourself, standing")))
            .Card(CardLabel, Intro, Difficulty.Easy).WithPreActions(a => a
                .AddButton("Truth", "What's your guiltiest pleasure — the one you'd deny in public?")
                .AddButton("Dare", "Speak in a terrible posh accent until your next turn.")
                .AddFooter(Forfeit("the group picks your accent for the NEXT two turns instead")))
            .Card(CardLabel, Intro, Difficulty.Easy).WithPreActions(a => a
                .AddButton("Truth", "What's the worst haircut, outfit, or phase you've ever committed to?")
                .AddButton("Dare", "Let the player to your right restyle your hair right now. It stays.")
                .AddFooter(Forfeit("you must show the group your oldest surviving photo of yourself")))
            .Card(CardLabel, Intro, Difficulty.Easy).WithPreActions(a => a
                .AddButton("Truth", "Reveal a secret talent nobody at this table knows about.")
                .AddButton("Dare", "Demonstrate ANY talent for 20 seconds. Confidence counts as talent.")
                .AddFooter(Forfeit("hum your own sad exit music while doing a lap of the room")))
            .Card(CardLabel, Intro, Difficulty.Easy).WithPreActions(a => a
                .AddButton("Truth", "What's the worst lie you've ever told — and did it work?")
                .AddButton("Dare", "Tell a 30-second story that's a complete lie; the group votes if it was convincing.")
                .AddFooter(Forfeit("you must answer the NEXT truth asked of anyone, honestly, as a bonus")))
            .Card(CardLabel, Intro, Difficulty.Easy).WithPreActions(a => a
                .AddButton("Truth", "What food do you pretend to like in social situations?")
                .AddButton("Dare", "Eat a spoonful of a condiment chosen by the group (from what's actually in the kitchen).")
                .AddFooter(Forfeit("you fetch snacks for the whole table, taking orders")))
            .Card(CardLabel, Intro, Difficulty.Easy).WithPreActions(a => a
                .AddButton("Truth", "What's the most childish thing you still do — and love?")
                .AddButton("Dare", "Play the rest of this round sitting on the floor like it's story time.")
                .AddFooter(Forfeit("your chair is gone for one round anyway, AND you lose the moral high ground")))
            .Card(CardLabel, Intro, Difficulty.Medium).WithPreActions(a => a
                .AddButton("Truth", "What song do you secretly know every single word to?")
                .AddButton("Dare", "Sing the chorus of any song — committed, full volume, air instruments included.")
                .AddFooter(Forfeit("the group picks the song and you HUM it with feeling")))

            // ── SPOTLIGHT — performance pieces ───────────────────────────────
            .Category(SpotlightCategory)
            .Card(CardLabel, Intro, Difficulty.Medium).WithPreActions(a => a
                .AddButton("Truth", "What's a moment you were secretly VERY proud of but never told anyone?")
                .AddButton("Dare", "Deliver a dramatic Oscar acceptance speech for the most mundane thing you did today.")
                .AddFooter(Forfeit("the group writes your acceptance speech and you read it verbatim")))
            .Card(CardLabel, Intro, Difficulty.Medium).WithPreActions(a => a
                .AddButton("Truth", "Who at this table would you trade lives with for a week, and why?")
                .AddButton("Dare", "Swap seats and IDENTITIES with the player opposite for the next two rounds — answer as them.")
                .AddFooter(Forfeit("they get to answer YOUR next truth for you")))
            .Card(CardLabel, Intro, Difficulty.Medium).WithPreActions(a => a
                .AddButton("Truth", "What's your most-used excuse — the one you keep in your back pocket?")
                .AddButton("Dare", "Sell the group an object within arm's reach like a late-night TV host. 45 seconds. They vote: sold or not.")
                .AddFooter(Forfeit("you must genuinely compliment each player's haggling skills, individually")))
            .Card(CardLabel, Intro, Difficulty.Medium).WithPreActions(a => a
                .AddButton("Truth", "Describe your worst date ever — no name needed, all details welcome.")
                .AddButton("Dare", "Reenact, solo, both sides of a disastrous first-date conversation.")
                .AddFooter(Forfeit("the player to your left narrates their GUESS of your worst date and you may not correct them")))
            .Card(CardLabel, Intro, Difficulty.Hard).WithPreActions(a => a
                .AddButton("Truth", "What's the weirdest thing you've ever googled at 2 a.m.?")
                .AddButton("Dare", "Hand your phone to the player on your right; they read your three most recent emoji aloud, with interpretive commentary.")
                .AddFooter(Forfeit("you describe your search history's general 'vibe' in three honest words")))
            .Card(CardLabel, Intro, Difficulty.Medium).WithPreActions(a => a
                .AddButton("Truth", "What compliment do you fish for most often?")
                .AddButton("Dare", "Walk the room like a runway model while the group provides fashion-week commentary.")
                .AddFooter(Forfeit("each player gives you the compliment you clearly wanted — sarcastically")))
            .Card(CardLabel, Intro, Difficulty.Medium).WithPreActions(a => a
                .AddButton("Truth", "If your life had a blooper reel, what moment is definitely on it?")
                .AddButton("Dare", "Reenact your most recent clumsy moment in slow motion with sound effects.")
                .AddFooter(Forfeit("the table reenacts how they IMAGINE it went and you must applaud")))

            // ── CHAOS — the table gets involved ──────────────────────────────
            .Category(ChaosCategory)
            .Card(CardLabel, Intro, Difficulty.Medium).WithPreActions(a => a
                .AddButton("Truth", "Rank everyone at this table by who'd survive longest in a zombie film. Justify last place.")
                .AddButton("Dare", "The group strikes a pose; you have 10 seconds to memorise it, then recreate ALL of them in sequence.")
                .AddFooter(Forfeit("you're officially first eaten in every hypothetical from now on")))
            .Card(CardLabel, Intro, Difficulty.Medium).WithPreActions(a => a
                .AddButton("Truth", "Which player's phone would be the most incriminating to read aloud, and why do you think so?")
                .AddButton("Dare", "Trade one shoe with the player across from you. Wear it until the deck says otherwise.")
                .AddFooter(Forfeit("BOTH your shoes go in the middle of the table as a monument to cowardice")))
            .Card(CardLabel, Intro, Difficulty.Hard).WithPreActions(a => a
                .AddButton("Truth", "What's a group opinion this table holds that you secretly disagree with?")
                .AddButton("Dare", "For the next three rounds, you must agree — enthusiastically — with everything anyone says.")
                .AddFooter(Forfeit("the group assigns you an opinion and you must defend it for one minute")))
            .Card(CardLabel, Intro, Difficulty.Hard).WithPreActions(a => a
                .AddButton("Truth", "Who at this table texts back the slowest, and what's your theory about why?")
                .AddButton("Dare", "Send a (harmless, group-approved) text to the last non-player person you messaged, dictated by the table.")
                .AddFooter(Forfeit("the table drafts the text they WOULD have sent and reads it aloud")))
            .Card(CardLabel, Intro, Difficulty.Medium).WithPreActions(a => a
                .AddButton("Truth", "If this friend group had a reality show, what would the season-one scandal be?")
                .AddButton("Dare", "Improvise the reality-show confessional interview about the player to your left. Camera three is imaginary but unblinking.")
                .AddFooter(Forfeit("you're the scandal now — the group writes the headline")))
            .Card(CardLabel, Intro, Difficulty.Hard).WithPreActions(a => a
                .AddButton("Truth", "What's something everyone here does that secretly drives you a little mad?")
                .AddButton("Dare", "The group invents a brand-new rule for the game right now; it applies only to you.")
                .AddFooter(Forfeit("TWO rules. They enjoy this too much.")))

            // ── HOT SEAT — squirm-grade truths, blush-grade dares ────────────
            .Category(HotSeatCategory)
            .Card(CardLabel, Intro, Difficulty.Hard).WithPreActions(a => a
                .AddButton("Truth", "What's the pettiest grudge you are actively still holding?")
                .AddButton("Dare", "Call the player who most recently beat you at anything and formally, flowerily concede.")
                .AddFooter(Forfeit("you must publicly forgive the grudge — naming it counts")))
            .Card(CardLabel, Intro, Difficulty.Hard).WithPreActions(a => a
                .AddButton("Truth", "What's the closest you've come to getting caught doing something you shouldn't?")
                .AddButton("Dare", "Confess a small, real, never-admitted thing to the group's chosen 'judge', who assigns community service (one silly task).")
                .AddFooter(Forfeit("the judge assigns the task anyway, doubled, with a gavel sound")))
            .Card(CardLabel, Intro, Difficulty.Hard).WithPreActions(a => a
                .AddButton("Truth", "Whose approval do you want most — and does that person know?")
                .AddButton("Dare", "Text someone (group-approved) a sincere compliment right now and show the send screen.")
                .AddFooter(Forfeit("you give that compliment to every player here instead, maintaining eye contact")))
            .Card(CardLabel, Intro, Difficulty.Extreme).WithPreActions(a => a
                .AddButton("Truth", "What's the most trouble you ever got into that your parents STILL don't know about?")
                .AddButton("Dare", "Let the group scroll exactly one screen of your camera roll (you pick the decade, they pick the direction).")
                .AddFooter(Forfeit("you describe the single worst photo of you in existence, in loving detail")))
            .Card(CardLabel, Intro, Difficulty.Hard).WithPreActions(a => a
                .AddButton("Truth", "What's a promise you broke that still bothers you?")
                .AddButton("Dare", "Make one real, small promise to a player of the group's choosing — witnessed, dated, and enforceable at the next game night.")
                .AddFooter(Forfeit("the group sets the promise AND the penalty for breaking it")))

            // ── LEGENDS — the cards people talk about next week ──────────────
            .Category(LegendsCategory)
            .Card(CardLabel, Intro, Difficulty.Extreme).WithPreActions(a => a
                .AddButton("Truth", "What is your single most embarrassing moment — the crown jewel, the one you'd delete from history?")
                .AddButton("Dare", "The group has one minute to design a dare using only what's in this room. You've already agreed.")
                .AddFooter(Forfeit("you tell the SECOND most embarrassing moment AND do a lap of honour")))
            .Card(CardLabel, Intro, Difficulty.Extreme).WithPreActions(a => a
                .AddButton("Truth", "If everyone here heard your internal monologue for one hour today, what would you owe apologies for?")
                .AddButton("Dare", "Perform one minute of interpretive dance titled 'My Week'. The group must guess three events from it.")
                .AddFooter(Forfeit("the group performs 'Your Week' AS THEY IMAGINE IT and you may not defend yourself")))
            .Card(CardLabel, Intro, Difficulty.Extreme).WithPreActions(a => a
                .AddButton("Truth", "What's the biggest risk you never took — and what do you think was on the other side of it?")
                .AddButton("Dare", "Do the thing you always say you'd do 'if I wasn't so embarrassed' — right now, 30-second version.")
                .AddFooter(Forfeit("you must toast, out loud, to the risk you'll take before next game night. Witnessed.")))
            .Card(CardLabel, Intro, Difficulty.Extreme).WithPreActions(a => a
                .AddButton("Truth", "Tell the story you've been saving — the one that starts 'okay but you can't tell anyone'.")
                .AddButton("Dare", "Trust fall. The group catches. (The group MUST catch. That's the real dare and it's theirs.)")
                .AddFooter(Forfeit("you owe the story at the NEXT game night, and it accrues interest")))

            // ── RESTRICTED SUBSET — appears only when the table qualifies ────
            .Card(CardLabel, Intro, Difficulty.Medium, restriction: couplesOnly).WithPreActions(a => a
                .AddButton("Truth", "What did you ACTUALLY think after your first date with your partner?")
                .AddButton("Dare", "Recreate your partner's most characteristic gesture until they admit it's accurate.")
                .AddFooter(Forfeit("your partner answers the truth FOR you, and their version is now canon")))
            .Card(CardLabel, Intro, Difficulty.Hard, restriction: couplesOnly).WithPreActions(a => a
                .AddButton("Truth", "What's one thing your partner does that you'll never admit you find adorable? Admit it.")
                .AddButton("Dare", "Serenade your partner with 15 seconds of any song, hand on heart, full sincerity.")
                .AddFooter(Forfeit("your partner picks the song and conducts you")))
            .Card(CardLabel, Intro, Difficulty.Hard, restriction: adultsOnly).WithPreActions(a => a
                .AddButton("Truth", "What's the real story of your wildest night out — the unabridged edition?")
                .AddButton("Dare", "Reenact, PG-13 and solo, the dance move that defined your going-out era.")
                .AddFooter(Forfeit("the group rates your going-out era from its surviving photos. You provide one.")))
            .Card(CardLabel, Intro, Difficulty.Extreme, restriction: adultsOnly).WithPreActions(a => a
                .AddButton("Truth", "What's the most money you've ever spent on something you never told anyone about?")
                .AddButton("Dare", "Show the group your most shameful recent purchase in your order history (one item, your pick of app).")
                .AddFooter(Forfeit("the group guesses the amount, loudly, until you confirm hot or cold")))

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
    /// Wraps a card's forfeit fragment in the chicken-clause sentence
    /// <c>WithPreActions</c> then prefixes with <c>Chicken clause:</c> — the
    /// exact wording <c>TableTop.Hosting.TruthOrDareCards</c> strips back off.
    /// </summary>
    private static string Forfeit(string consequence) =>
        "back out after hearing your pick, and " + consequence + ".";
}
