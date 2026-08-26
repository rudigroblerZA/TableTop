using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Restrictions;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games;

/// <summary>
/// Get to Know You — conversation prompt cards arranged in four depth tiers.
///
/// No winners, no scoring pressure. Each player reads their prompt aloud and answers it.
/// Others can respond, disagree, or share their own take before moving on.
/// Rounds progress from light icebreakers through to genuinely revealing questions.
/// </summary>
public sealed class GetToKnowYouMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Get to Know You";
    /// <inheritdoc />
    public override string Description =>
        "Conversation prompt cards. Four depth tiers — from light icebreakers to the real stuff.";

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Next";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Icebreaker"] = "#26C6DA",
            ["Surface"] = "#66BB6A",
            ["Deeper"] = "#FFCA28",
            ["Vulnerable"] = "#EC407A",
        };



    /// <summary>
    /// Builds the deck, JSON-first. The built-in bank below is the fallback for
    /// a stripped publish where the file is absent.
    /// </summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        BuildBuiltInCards(players);

    /// <summary>
    /// The compiled card bank. Note that this generates a fresh <c>Guid</c> per
    /// card on every call, so two calls never agree on ids — which is precisely
    /// why the JSON file above is preferred: it pins them. Played-card tracking
    /// across save/resume compares ids, so an unpinned deck re-deals cards the
    /// table has already seen.
    /// </summary>
    private IReadOnlyList<ICard> BuildBuiltInCards(IReadOnlyList<IPlayer> players)
    {
        var parentsOnly = new ParentOnlyRestriction();
        var couplesOnly = new CoupleOnlyRestriction();

        return
        [
            // ════════════════════════════════════════════════════════════════
            // TIER 1 — ICEBREAKER
            // Light, fun, low-stakes. Nobody feels put on the spot.
            // ════════════════════════════════════════════════════════════════

            PromptCard.CreateGenderDirected("First Job",
                maleText:   "What was your first job, and what did you learn from it that had nothing to do with the actual work?",
                femaleText: "What was your first job, and is there anything about it you secretly miss?",
                otherText:  "What was your first job, and how much did it shape how you think about work now?",
                Difficulty.Easy, "Icebreaker"),

            StandardCard.Create("Superpower",
                "If you woke up tomorrow with one completely useless superpower, what would you want it to be and why?",
                Difficulty.Easy, "Icebreaker"),

            PromptCard.CreateGenderDirected("Childhood Obsession",
                maleText:   "What were you completely obsessed with between the ages of 8 and 12 that you are mildly embarrassed about now?",
                femaleText: "What phase did you go through as a kid that your family still brings up?",
                otherText:  "What did you love as a child that most people your age weren't into?",
                Difficulty.Easy, "Icebreaker"),

            StandardCard.Create("Parallel Life",
                "If you had taken a completely different path at 18, what do you think you would be doing now?",
                Difficulty.Easy, "Icebreaker"),

            PromptCard.CreateGenderDirected("Comfort Watch",
                maleText:   "What film, show, or sport do you put on when you just need to switch your brain off?",
                femaleText: "What is the one show or film you have rewatched more times than you would admit?",
                otherText:  "What do you watch or listen to when you need to feel better without thinking about it?",
                Difficulty.Easy, "Icebreaker"),

            StandardCard.Create("Dream Dinner",
                "You can invite three people — living, dead, fictional — to a dinner party. Who and why?",
                Difficulty.Easy, "Icebreaker"),

            PromptCard.CreateGenderDirected("Morning Person",
                maleText:   "Are you actually a morning person, or is that something you tell yourself to feel virtuous?",
                femaleText: "What does your ideal morning look like — and how often does it actually happen?",
                otherText:  "How do you genuinely prefer to start a day, versus how you actually start most days?",
                Difficulty.Easy, "Icebreaker"),

            StandardCard.Create("Skill Swap",
                "What is one skill someone in this room has that you genuinely wish you had?",
                Difficulty.Easy, "Icebreaker"),

            PromptCard.CreateGenderDirected("Hidden Talent",
                maleText:   "What can you do that most people who know you would be surprised by?",
                femaleText: "What is something you are quietly good at that you rarely show people?",
                otherText:  "What talent or ability do you have that almost never comes up in normal conversation?",
                Difficulty.Easy, "Icebreaker"),

            StandardCard.Create("Best Decision",
                "What is a small, almost accidental decision you made that turned out to change everything?",
                Difficulty.Easy, "Icebreaker"),

            StandardCard.Create("Unpopular Comfort",
                "What is a food, song, or place that comforts you that most people would find deeply uncool?",
                Difficulty.Easy, "Icebreaker"),

            PromptCard.CreateGenderDirected("Collecting",
                maleText:   "What have you collected or hoarded at some point in your life — and do you still have any of it?",
                femaleText: "Is there something you keep buying more of even though you objectively have enough?",
                otherText:  "What object or category of thing do you own way too much of, and are you fine with that?",
                Difficulty.Easy, "Icebreaker"),

            // ════════════════════════════════════════════════════════════════
            // TIER 2 — SURFACE
            // Personal but comfortable. Builds familiarity without vulnerability.
            // ════════════════════════════════════════════════════════════════

            PromptCard.CreateGenderDirected("Turning Point",
                maleText:   "What moment in your twenties most changed the direction of your life?",
                femaleText: "What experience do you now see as a turning point, even though it didn't feel that way at the time?",
                otherText:  "Looking back, what changed you more than you expected it to?",
                Difficulty.Medium, "Surface"),

            StandardCard.Create("The Friend You Grew Out Of",
                "Tell us about a friendship that mattered enormously at the time and then just quietly ended. What do you make of it now?",
                Difficulty.Medium, "Surface"),

            PromptCard.CreateGenderDirected("Work Identity",
                maleText:   "How much of your identity is tied up in what you do for work — and is that a problem?",
                femaleText: "Is your job part of who you are, or just something you do? Are you comfortable with that answer?",
                otherText:  "How do you describe yourself when someone asks 'what do you do' — and does that feel accurate?",
                Difficulty.Medium, "Surface"),

            StandardCard.Create("The Thing You Got Wrong",
                "What is an opinion or belief you held confidently in your twenties that you have since completely revised?",
                Difficulty.Medium, "Surface"),

            PromptCard.CreateGenderDirected("Family Pattern",
                maleText:   "What is a pattern from your family that you have consciously tried to break?",
                femaleText: "What did you grow up thinking was normal that you now realise was actually quite specific to your family?",
                otherText:  "What habit, attitude, or way of relating to people did you pick up from your family — for better or worse?",
                Difficulty.Medium, "Surface"),

            StandardCard.Create("Proudest Ordinary Thing",
                "What is something mundane or practical you are quietly proud of getting good at?",
                Difficulty.Medium, "Surface"),

            PromptCard.CreateGenderDirected("Ambition",
                maleText:   "What did you want to be known for at 25, and how does that compare to what you want to be known for now?",
                femaleText: "Has your ambition changed as you've got older — and if so, how do you feel about that?",
                otherText:  "What does success look like to you now, versus what you thought it looked like when you were younger?",
                Difficulty.Medium, "Surface"),

            StandardCard.Create("The Lesson That Took Too Long",
                "What is a lesson you have had to learn more than once because you kept ignoring it the first few times?",
                Difficulty.Medium, "Surface"),

            PromptCard.CreateGenderDirected("Solitude",
                maleText:   "How do you actually feel about spending time alone — honestly?",
                femaleText: "What does time alone feel like for you — recharging, uncomfortable, or something else?",
                otherText:  "When you are alone with no distractions, what usually happens in your head?",
                Difficulty.Medium, "Surface"),

            StandardCard.Create("The Place",
                "Describe a place — a building, a street, a room — that made you feel like yourself. Why that place?",
                Difficulty.Medium, "Surface"),

            PromptCard.CreateGenderDirected("Relationship With Money",
                maleText:   "What attitude to money did you inherit from your parents, and how much has it served you?",
                femaleText: "What is your relationship with money really like — not what you say it is, but what it actually is?",
                otherText:  "What emotion does money most reliably trigger in you, and where do you think that comes from?",
                Difficulty.Medium, "Surface"),

            StandardCard.Create("The Book or Film That Changed You",
                "What piece of writing, film, or music genuinely shifted how you think about something? What changed?",
                Difficulty.Medium, "Surface"),

            StandardCard.Create("Introvert vs Extrovert",
                "Do you think of yourself as an introvert or extrovert — and is there a gap between how you actually are and how you present in public?",
                Difficulty.Medium, "Surface"),

            // ════════════════════════════════════════════════════════════════
            // TIER 3 — DEEPER
            // Requires some trust. Moves toward values, regrets, and real feeling.
            // ════════════════════════════════════════════════════════════════

            PromptCard.CreateGenderDirected("What You Avoid",
                maleText:   "What difficult feeling do you most reliably avoid — and what do you do instead of feeling it?",
                femaleText: "When something upsets you, what is your default strategy for not dealing with it directly?",
                otherText:  "What is the emotional experience you find most difficult to sit with?",
                Difficulty.Hard, "Deeper"),

            StandardCard.Create("The Apology You Owe",
                "Is there someone in your past you have never properly apologised to? You don't have to name them — but what happened?",
                Difficulty.Hard, "Deeper"),

            PromptCard.CreateGenderDirected("Validation",
                maleText:   "Where do you most seek approval from others — and how aware are you of it when it's happening?",
                femaleText: "Whose opinion of you matters more than you think it should?",
                otherText:  "What form of external validation do you find hardest to admit you still need?",
                Difficulty.Hard, "Deeper"),

            StandardCard.Create("The Version of Yourself You Retired",
                "Describe a version of yourself you used to be that you have mostly let go of. Do you miss any part of it?",
                Difficulty.Hard, "Deeper"),

            PromptCard.CreateGenderDirected("Fear of Becoming",
                maleText:   "What kind of person are you most afraid of becoming as you get older?",
                femaleText: "Is there a person in your life — past or present — you are afraid of resembling?",
                otherText:  "What do you worry you are slowly becoming that you don't want to be?",
                Difficulty.Hard, "Deeper"),

            StandardCard.Create("The Sacrifice You Made",
                "What did you give up — a path, a relationship, a version of yourself — that you sometimes still think about?",
                Difficulty.Hard, "Deeper"),

            PromptCard.CreateGenderDirected("Loneliness",
                maleText:   "When do you feel most alone — not just physically, but in the deeper sense?",
                femaleText: "Is there a part of your inner life you have never found anyone to share with?",
                otherText:  "What is something about your experience that you have never quite managed to put into words for another person?",
                Difficulty.Hard, "Deeper"),

            StandardCard.Create("The Story You Keep Telling",
                "What story about your past do you tell most often — and do you think it is the most accurate version of events?",
                Difficulty.Hard, "Deeper"),

            PromptCard.CreateGenderDirected("Anger",
                maleText:   "What is the thing that makes you genuinely angry in a way that surprises even you?",
                femaleText: "How do you handle anger — and is that working for you?",
                otherText:  "What has made you the most unexpectedly angry in the last year, and what do you think it was really about?",
                Difficulty.Hard, "Deeper"),

            StandardCard.Create("The Thing Nobody Fixed",
                "What did you go through that you mostly handled alone — and how did that shape you?",
                Difficulty.Hard, "Deeper"),

            PromptCard.CreateGenderDirected("Success and Guilt",
                maleText:   "Is there something you have achieved that you feel uncomfortable owning fully? Why?",
                femaleText: "Do you find it easier to accept failure or success? Which one makes you more uncomfortable and why?",
                otherText:  "What achievement do you downplay — and what is the reason you think you do that?",
                Difficulty.Hard, "Deeper"),

            StandardCard.Create("The Relationship That Taught You The Most",
                "Which relationship — romantic, family, or friendship — taught you the most about yourself? What did it show you?",
                Difficulty.Hard, "Deeper",
                restriction: new AdultOnlyRestriction()),

            StandardCard.Create("What You Want But Haven't Said",
                "Is there something you want from the people in your life that you have never directly asked for?",
                Difficulty.Hard, "Deeper"),

            // ════════════════════════════════════════════════════════════════
            // TIER 4 — VULNERABLE
            // For a group that trusts each other. Real, tender, honest.
            // ════════════════════════════════════════════════════════════════

            PromptCard.CreateGenderDirected("Worth",
                maleText:   "In what area of your life do you find it hardest to believe you are genuinely enough?",
                femaleText: "Where does your sense of worth most reliably come from — and what happens when that source dries up?",
                otherText:  "Is there a version of yourself you feel you have to become before you are allowed to be happy?",
                Difficulty.Extreme, "Vulnerable"),

            StandardCard.Create("The Thing You Have Never Said Out Loud",
                "What is something true about you or your life that you have never said out loud to another person?",
                Difficulty.Extreme, "Vulnerable"),

            PromptCard.CreateGenderDirected("Being Known",
                maleText:   "Do you feel genuinely known by the people closest to you — and if not, what part of you feels hidden?",
                femaleText: "Is there a version of yourself that you keep very protected? What are you protecting it from?",
                otherText:  "What would it mean to you to be fully known by someone else — and does that feel safe or terrifying?",
                Difficulty.Extreme, "Vulnerable"),

            StandardCard.Create("Your Relationship With Your Own Body",
                "How has your relationship with your body changed over your life — and where are you with it now?",
                Difficulty.Extreme, "Vulnerable"),

            PromptCard.CreateGenderDirected("The Fear Underneath",
                maleText:   "What are you most afraid people would think of you if they saw everything?",
                femaleText: "What do you most fear about being truly seen by someone?",
                otherText:  "What is the fear that sits underneath most of your other fears?",
                Difficulty.Extreme, "Vulnerable"),

            StandardCard.Create("What You Carry",
                "What is something from your past that you carry with you every day, even when you don't talk about it?",
                Difficulty.Extreme, "Vulnerable"),

            PromptCard.CreateGenderDirected("Love",
                maleText:   "What is the most honest thing you can say about how you love people?",
                femaleText: "How do you love — and is there a gap between how you want to love and how you actually do?",
                otherText:  "What does it feel like when you love someone, and how do you know when it's real?",
                Difficulty.Extreme, "Vulnerable",
                restriction: new AdultOnlyRestriction()),

            StandardCard.Create("The Conversation You Are Avoiding",
                "Is there a conversation you need to have with someone in your life that you keep putting off? What is stopping you?",
                Difficulty.Extreme, "Vulnerable"),

            PromptCard.CreateGenderDirected("Grief",
                maleText:   "What loss — a person, a relationship, a version of your life — are you still carrying?",
                femaleText: "What have you lost that you have never quite finished grieving?",
                otherText:  "What grief do you carry that most people in your life don't know about?",
                Difficulty.Extreme, "Vulnerable"),

            StandardCard.Create("The Question You Are Afraid to Answer",
                "What question about your own life are you most afraid to sit with? You don't have to answer it — just name it.",
                Difficulty.Extreme, "Vulnerable"),

            PromptCard.CreateGenderDirected("Enough",
                maleText:   "At what point in your life did you feel most like enough — and what made that possible?",
                femaleText: "Have you ever felt fully enough, exactly as you are? What did that feel like, or what do you think it would feel like?",
                otherText:  "What would your life look like if you genuinely believed you were enough right now?",
                Difficulty.Extreme, "Vulnerable"),

            StandardCard.Create("What You Hope For",
                "What do you still hope for — not for the world, not for other people, but for yourself?",
                Difficulty.Extreme, "Vulnerable"),

            // ── Couple & parent-specific ──────────────────────────────────────

            StandardCard.Create("What We Don't Say",
                "What is something you appreciate about your partner that you rarely or never actually tell them?",
                Difficulty.Hard, "Deeper",
                restriction: new CoupleOnlyRestriction()),

            StandardCard.Create("When It Got Real",
                "When did parenthood shift from something you were doing to something that fundamentally changed who you are?",
                Difficulty.Hard, "Deeper",
                restriction: new ParentOnlyRestriction()),

            StandardCard.Create("The Fight Underneath The Fight",
                "What is an argument you and your partner keep having — and what do you think it is actually about?",
                Difficulty.Extreme, "Vulnerable",
                restriction: new CoupleOnlyRestriction()),
        ];
    }

    private static IEnumerable<string> WrapText(string text, int width)
    {
        var words = text.Split(' ');
        var line = string.Empty;
        foreach (var word in words)
        {
            if ((line + word).Length > width)
            {
                if (line.Length > 0) yield return line.TrimEnd();
                line = string.Empty;
            }
            line += word + " ";
        }
        if (line.Length > 0) yield return line.TrimEnd();
    }
}