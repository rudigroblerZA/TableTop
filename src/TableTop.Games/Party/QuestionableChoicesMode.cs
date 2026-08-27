using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Party;

/// <summary>
/// Questionable Choices — the fill-in-the-blank judged party game, adult edition.
/// Same format as Blank Slate, considerably more jaded. All prompts and answers
/// are original writing.
///
/// ADAPTED FOR ONE SCREEN. The tabletop version of this format deals each player
/// a private hand of answer cards; on a shared device that doesn't work, so each
/// prompt carries its own numbered shortlist. Players secretly pick a number or
/// invent their own — inventing is encouraged and usually wins.
///
/// How to play:
///   1. The active player is the JUDGE and reads the prompt aloud.
///   2. Everyone else secretly picks a number, or makes something up.
///   3. Read them all out loud, filled into the blank.
///   4. The judge awards the round to whoever earned it — hit "🏆 Winner".
///   5. Pass the judging along.
///
/// ON THE HUMOUR: this deck is dark, rude, and fond of despair — the comedy is
/// in adult life being quietly humiliating, in workplaces, in bodies that have
/// started making noises, and in the abyss. What it deliberately does NOT do is
/// mine race, disability, or religion for punchlines, which is the lazy half of
/// this genre and not actually where the laughs are. Rude, not cruel.
///
/// Adult (18+).
/// </summary>
public sealed class QuestionableChoicesMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Questionable Choices";
    /// <inheritdoc />
    public override string Description =>
        "Fill in the blank, worst-best answer wins. Dark, rude, and deeply tired — the judge decides. 18+.";

    /// <summary>Label for awarding the round.</summary>
    public override string CompleteLabel => "🏆 Winner";
    /// <summary>Label for skipping a prompt.</summary>
    public override string SkipLabel => "⤳ Skip";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [QuestionableChoicesCardBank.ModernLifeCategory] = "#42A5F5",
            [QuestionableChoicesCardBank.TheOfficeCategory] = "#78909C",
            [QuestionableChoicesCardBank.RelationshipsCategory] = "#EC407A",
            [QuestionableChoicesCardBank.ExistentialCategory] = "#7E57C2",
            [QuestionableChoicesCardBank.ChaosCategory] = "#EF5350",
        };

    /// <summary>One point to whoever wins the round.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Returns the built-in card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        QuestionableChoicesCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => QuestionableChoicesCardBank.All;
}

/// <summary>Built-in card bank for Questionable Choices. All content is original.</summary>
public static class QuestionableChoicesCardBank
{
    internal const string ModernLifeCategory = "Modern Life";
    internal const string TheOfficeCategory = "The Office";
    internal const string RelationshipsCategory = "Relationships";
    internal const string ExistentialCategory = "Existential";
    internal const string ChaosCategory = "Chaos";

    /// <summary>All cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── MODERN LIFE ──────────────────────────────────────────────────────
        P(ModernLifeCategory, "My entire personality is now just ______.",
          ["being tired in a variety of fonts", "having a strong opinion about a bin day",
           "the podcast I have not listened to", "back pain and a water bottle",
           "aggressively recommending one restaurant", "my phone screen time report",
           "sighing at the price of cheese", "a houseplant I am slowly killing"], Difficulty.Easy),
        P(ModernLifeCategory, "Nothing says 'I have my life together' quite like ______.",
          ["nine unopened letters from the bank", "a fridge containing condiments and despair",
           "buying a planner instead of making a plan", "twelve half-drunk glasses of water",
           "a gym membership as a personality", "paying extra to skip one advert",
           "owning a steamer I have never once used", "googling my own symptoms at 2am"], Difficulty.Easy),
        P(ModernLifeCategory, "The most stressful part of adulthood is ______.",
          ["a phone call I have to make", "the sheer number of passwords",
           "someone saying 'we need to talk about the boiler'", "an unexpected knock at the door",
           "having to choose a dentist", "the letter that just says 'IMPORTANT'",
           "realising I am the responsible adult present", "how quickly bins come back around"], Difficulty.Medium),
        P(ModernLifeCategory, "I could fix my life tomorrow if it weren't for ______.",
          ["my own personality, mainly", "one specific app",
           "the crushing gravitational pull of the sofa", "having already ordered the food",
           "a small amount of money and a large amount of denial",
           "the fact that mornings exist", "my capacity to start things",
           "the group chat"], Difficulty.Medium),
        P(ModernLifeCategory, "There is no feeling worse than ______.",
          ["the text that says 'can I call you'", "hearing your own voice played back",
           "waving at someone waving past you", "the queue moving faster in the other lane",
           "remembering something you said in 2011", "your card being declined confidently",
           "standing up too fast at your age", "the silence after your joke"], Difficulty.Hard),

        // ── THE OFFICE ───────────────────────────────────────────────────────
        P(TheOfficeCategory, "This meeting could have been ______.",
          ["an email", "a brief moment of silence",
           "avoided entirely by one competent person", "twelve seconds long",
           "held without me, ideally", "cancelled and celebrated",
           "a single sentence in a group chat", "nothing. It could have been nothing."], Difficulty.Easy),
        P(TheOfficeCategory, "My greatest professional achievement is ______.",
          ["looking busy for eleven consecutive years", "one email that ended a debate forever",
           "never once being asked to present", "renaming a folder and taking the credit",
           "surviving a restructure by being forgotten", "the day I fixed the printer",
           "successfully leaving at 5pm", "getting away with the tone of one reply"], Difficulty.Medium),
        P(TheOfficeCategory, "HR would like a quiet word about ______.",
          ["the microwave. Again.", "what I said on the all-company call",
           "my out-of-office message", "the birthday card I signed too honestly",
           "an incident involving the fridge", "my definition of 'working from home'",
           "the nickname that caught on", "something I did at the Christmas party"], Difficulty.Hard),
        P(TheOfficeCategory, "The company's new strategy is basically ______.",
          ["the old strategy with a new font", "a word nobody will define",
           "hoping very hard", "three people doing nine jobs",
           "a poster in the stairwell", "whatever the consultant said last",
           "renaming everything and moving the desks", "a synergy we will never speak of again"], Difficulty.Medium),
        P(TheOfficeCategory, "I'd be far more productive if it weren't for ______.",
          ["the job", "one colleague and their chair noise",
           "meetings about upcoming meetings", "the software we paid a fortune for",
           "the person who says 'just quickly'", "my own browser tabs",
           "having to look approachable", "the open-plan office as a concept"], Difficulty.Medium),

        // ── RELATIONSHIPS ────────────────────────────────────────────────────
        P(RelationshipsCategory, "The fastest way to end a first date is ______.",
          ["mentioning my spreadsheet about them", "how I treated the waiter",
           "answering 'what are you looking for' completely honestly",
           "explaining my sleep schedule", "bringing up my ex in minute four",
           "a strong opinion about their coat", "asking to split it down to the penny",
           "showing them 400 photos of my dog"], Difficulty.Medium),
        P(RelationshipsCategory, "The true test of a long relationship is ______.",
          ["assembling furniture together", "sharing one duvet honestly",
           "a long car journey and one map", "who is going to ring the plumber",
           "watching them eat crisps for years", "deciding where to eat, forever",
           "the thermostat war", "hearing the same story at every party"], Difficulty.Medium),
        P(RelationshipsCategory, "The most romantic thing my partner has ever done is ______.",
          ["dealt with the thing in the sink", "handled a phone call I was dreading",
           "let me have the last one and said nothing", "pretended to like it",
           "remembered a small thing from ages ago", "got up first, every time",
           "lied about my haircut convincingly", "left before the party got bad, with me"], Difficulty.Hard),
        P(RelationshipsCategory, "Our love language is ______.",
          ["sending each other the same video separately", "aggressive snack provision",
           "complaining in perfect harmony", "silently agreeing to leave",
           "narrating the pet's inner thoughts", "one shared, deeply petty grudge",
           "the exact right cup of tea", "saying nothing about the noise I made"], Difficulty.Hard),

        // ── EXISTENTIAL ──────────────────────────────────────────────────────
        P(ExistentialCategory, "At 3am, the thought that arrives is ______.",
          ["every conversation I have ever mishandled", "the exact scale of the universe",
           "did I lock it", "the noise the car has started making",
           "how everyone I know is also just guessing", "a bill with a date on it",
           "that I am the oldest I have ever been", "the sound of my own heartbeat, unhelpfully"], Difficulty.Hard),
        P(ExistentialCategory, "It turns out the meaning of life was ______ all along.",
          ["snacks and a decent chair", "the admin we did along the way",
           "one good afternoon in 2016", "nobody knowing what they're doing, together",
           "a nap of exactly the right length", "the group chat",
           "getting the parking space", "not this, apparently"], Difficulty.Extreme),
        P(ExistentialCategory, "My therapist went quiet when I mentioned ______.",
          ["how I organise my fridge", "what I do in the car alone",
           "the running total I keep on people", "my relationship with the self-checkout",
           "the plan I have for a specific ex", "how many tabs are open",
           "what I named my houseplants", "the dream about the corridor"], Difficulty.Extreme),
        P(ExistentialCategory, "The unspoken agreement all adults share is ______.",
          ["nobody read the terms and conditions", "we are all just doing an impression of one",
           "the bins are the only real deadline", "everyone is tired and lying about it",
           "no one knows how tax works", "we all just kept going after school ended",
           "being fine is a rumour we spread", "the food shop is eternal"], Difficulty.Hard),

        // ── CHAOS ────────────────────────────────────────────────────────────
        P(ChaosCategory, "The wedding was going perfectly until ______.",
          ["the best man's opening sentence", "an uninvited swan",
           "the DJ's one commitment issue", "someone's toast lasting nineteen minutes",
           "the cake's structural failure", "an ex arriving with confidence",
           "the seating plan achieving what it was always going to",
           "somebody's dad and the microphone"], Difficulty.Medium),
        P(ChaosCategory, "The holiday photos don't show ______.",
          ["the four hours before that smile", "what the room actually smelled like",
           "the argument in the car park", "how much it cost, truly",
           "everyone's silence at the buffet", "the injury just out of frame",
           "the queue that took the whole morning", "what happened to the third person"], Difficulty.Medium),
        P(ChaosCategory, "I have never fully recovered from ______.",
          ["calling a teacher 'mum'", "a group photo I was not ready for",
           "the day I was ambushed with a happy birthday", "one review of my cooking",
           "waving back at nobody", "an audible stomach in a silent room",
           "the school talent show", "sending it to the wrong person"], Difficulty.Hard),
        P(ChaosCategory, "The police report simply read: ______.",
          ["'A dispute regarding a parking space.'", "'Involved a swan. Again.'",
           "'The suspect was extremely apologetic.'", "'It was a bin, not a person.'",
           "'Both parties blame the sat-nav.'", "'No crime occurred. Everyone was just like that.'",
           "'The trifle was, in fact, the weapon.'", "'A neighbourly disagreement about a hedge.'"], Difficulty.Extreme),
    ];

    private static ICard P(string category, string prompt, string[] answers, Difficulty d) =>
        StandardCard.Create(
            prompt.Length > 42 ? prompt[..39].TrimEnd() + "…" : prompt,
            "<b>🃏 " + category.ToUpperInvariant() + "</b>\n\n" +
            "<b>" + prompt + "</b>\n\n" +
            "<i>Judge reads it out. Everyone else secretly picks a number — or invents something worse.</i>\n\n" +
            string.Join("\n", answers.Select((a, i) => $"{i + 1}. {a}")),
            d, category);
}
