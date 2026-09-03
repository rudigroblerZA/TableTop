using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// House Rules — the deck about the logistics of sharing a life, played as a
/// game rather than survived as an argument.
///
/// <para>
/// <b>The gap this fills.</b> Every other Couples mode is about feeling:
/// memory, desire, admiration, the future as a dream. Nothing in the catalogue
/// touches the things two people actually negotiate — the thermostat, the
/// spending threshold, whose family gets Christmas, what "I need an hour"
/// means. Those conversations happen anyway; they just tend to happen at 11pm
/// while one person is already annoyed. This deck moves them to a Tuesday
/// evening with a pen, which is the only real innovation on offer here.
/// </para>
///
/// <para>
/// <b>How it works.</b> Each card names one domain. You both answer, then agree
/// <i>one</i> concrete rule and write it on a shared list. The rule has to be
/// specific enough to break — "we tell each other before spending over a
/// hundred pounds" counts, "we communicate better about money" does not.
/// </para>
///
/// <para>
/// <b>Park It is a real move, not a forfeit.</b> The skip label says so, and the
/// opening cards say so twice. A deck that turned every disagreement into a
/// stalemate you must resolve tonight would push couples into agreeing to
/// things they do not mean — which is worse than no rule at all, because now
/// it is written down. Parking a card is the deck working.
/// </para>
///
/// Gentle by design and safe for any established couple; nothing here is
/// explicit, so it sits at Teen alongside Future Us rather than with the
/// Intimate decks.
/// </summary>
public sealed class HouseRulesMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <summary>Two people who share, or intend to share, the decisions on these cards.</summary>
    public TableShape SuitableFor => TableShape.Couple;

    /// <inheritdoc />
    public override string Name => "House Rules";

    /// <inheritdoc />
    public override string Description =>
        "The practical deck — money, mess, time, families and the future. Both answer, then write down one rule you would both actually keep. Parking a card is always allowed.";

    /// <summary>Label for a card that produced a written rule.</summary>
    public override string CompleteLabel => "Agreed — Written Down";

    /// <summary>Label for a card you are not settling tonight. Deliberately not "Skipped".</summary>
    public override string SkipLabel => "Park It";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [HouseRulesCardBank.SetupCategory] = "#26A69A",
            [HouseRulesCardBank.HomeCategory] = "#66BB6A",
            [HouseRulesCardBank.MoneyCategory] = "#FFA726",
            [HouseRulesCardBank.TimeCategory] = "#42A5F5",
            [HouseRulesCardBank.PeopleCategory] = "#AB47BC",
            [HouseRulesCardBank.FutureCategory] = "#5C6BC0",
            [HouseRulesCardBank.PactCategory] = "#EC407A",
        };

    /// <summary>
    /// The setup cards explain the list and the right to park, and both have to
    /// land before the first real card or the deck reads as an interrogation.
    /// The closing pair is what turns a pile of answers into something you keep.
    /// </summary>
    public override IReadOnlyList<string> CategoriesPinnedToStart => [HouseRulesCardBank.SetupCategory];

    /// <inheritdoc cref="CategoriesPinnedToStart" />
    public override IReadOnlyList<string> CategoriesPinnedToEnd => [HouseRulesCardBank.PactCategory];

    /// <summary>
    /// Flat scoring, and the "score" is just a count of rules you wrote. Paying
    /// more for the harder cards would quietly price the difficult
    /// conversations — exactly the ones where parking should cost nothing.
    /// </summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Returns the built-in House Rules card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        HouseRulesCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => HouseRulesCardBank.All;
}

/// <summary>
/// Built-in card bank for House Rules. Authored with <see cref="CardDeckBuilder"/>
/// so ids derive from card content and stay stable across restarts.
///
/// Ordered setup first, pact last, with the five domains rising in weight
/// between them — the thermostat before the ageing parent, deliberately.
/// </summary>
public static class HouseRulesCardBank
{
    internal const string SetupCategory = "Before You Start";
    internal const string HomeCategory = "Home";
    internal const string MoneyCategory = "Money";
    internal const string TimeCategory = "Time";
    internal const string PeopleCategory = "People";
    internal const string FutureCategory = "The Future";
    internal const string PactCategory = "The Pact";

    /// <summary>All House Rules cards, in intended play order.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    /// <summary>
    /// Formats a domain card. The footer repeats on every card on purpose:
    /// "specific enough to break" and "parking is free" are the two rules that
    /// keep this from turning into a row, and neither survives being stated
    /// once at the start.
    /// </summary>
    private static string Rule(string setup, string ask) =>
        setup + "\n\n" +
        "<b>Both answer:</b> " + ask + "\n\n" +
        "<i>Then agree ONE rule and write it on the list — specific enough that you would both " +
        "know if it was broken. Cannot agree? Park it. Parking costs nothing and beats a rule " +
        "neither of you means.</i>";

    private static IReadOnlyList<ICard> Build() => CardDeckBuilder
        .For("House Rules")

        // ── BEFORE YOU START ─────────────────────────────────────────────────
        .Category(SetupCategory)
            .Card("Before You Start — Get a Pen",
                "This deck needs paper. Find some now, before the first card, and title it however you like — you are going to be adding to it all evening.\n\n" +
                "Each card ends the same way: you both answer, then you agree ONE rule and write it down. The writing is not decoration. A rule you said out loud is a nice moment; a rule on a list is something either of you can point at in three weeks without it becoming an argument about what was agreed.\n\n" +
                "Make them specific. \"We will be better about the kitchen\" is not a rule. \"Whoever cooks does not wash up\" is.",
                Difficulty.Easy)
            .Card("Before You Start — Parking Is Free",
                "One thing to settle before you settle anything else.\n\n" +
                "Either of you can say \"park it\" on any card, at any point, and the card is over. No reason owed, no follow-up question, no sulking. It goes on a second list — things to come back to — and you move on.\n\n" +
                "This matters more than it sounds. A deck that forces a decision tonight produces agreements people do not mean, and a rule nobody means is worse than no rule, because now it is in writing and somebody is going to be held to it. If you park half of these, the evening still worked.",
                Difficulty.Easy)

        // ── HOME ─────────────────────────────────────────────────────────────
        .Category(HomeCategory)
            .Card("The Dishes",
                Rule("Every household has one person with a lower tolerance for mess, and they do more of the cleaning while wondering why they have to.",
                     "What does \"the kitchen is done\" mean to you, exactly? Describe the state of the sink, the hob and the worktops."),
                Difficulty.Easy)
            .Card("The Thermostat",
                Rule("One of you is cold. The other is baffled by this. It has probably come up before.",
                     "What temperature do you actually want the house at, and what do you do when you are uncomfortable — say something, or quietly go and change it?"),
                Difficulty.Easy)
            .Card("Whose Pile Is That",
                Rule("There is a surface in your home that has become someone's. There always is.",
                     "Name the surface. Then, honestly: whose is it, and how long does something get to live there before the other person may move it?"),
                Difficulty.Medium)
            .Card("The Closed Door",
                Rule("Sometimes one of you needs to not be spoken to for an hour, and the other reads that as something being wrong.",
                     "How do you signal \"I need to be left alone and it is not about you\" — and how would you like that signal to be received?"),
                Difficulty.Medium)
            .Card("Guests",
                Rule("Someone is staying the night. One of you is thrilled, the other is doing sums about the bathroom.",
                     "How much notice do you need before someone stays, and how long is too long?"),
                Difficulty.Medium)
            .Card("The Shopping",
                Rule("The milk situation is a recurring theme.",
                     "Who notices when things run out, who actually replaces them, and is that the same person?"),
                Difficulty.Easy)

        // ── MONEY ────────────────────────────────────────────────────────────
        .Category(MoneyCategory)
            .Card("The Number",
                Rule("Most money arguments are not about money. They are about finding out afterwards.",
                     "What is the amount above which you would want to hear about a purchase before it happens? Say your number out loud at the same time."),
                Difficulty.Medium)
            .Card("Yours, Mine, Ours",
                Rule("There are a dozen workable ways to split money between two people and no natural one.",
                     "Describe how you would like money to be organised between you, and say plainly whether that is what is happening now."),
                Difficulty.Hard)
            .Card("Spending On Each Other",
                Rule("Two people can have completely different ideas about what a normal present costs, and never discover it except by disappointing each other.",
                     "What is a normal amount to spend on each other for a birthday? And is there anything you would rather have than a gift?"),
                Difficulty.Easy)
            .Card("Lending to Family",
                Rule("Someone's relative is going to ask, eventually.",
                     "If a family member asked one of you for a significant loan, what would you want to happen — and would it be a joint decision?"),
                Difficulty.Hard)
            .Card("What Counts as an Emergency",
                Rule("You may have a rainy-day fund. You almost certainly have not agreed what rain is.",
                     "Name two things you would consider a genuine emergency, and one thing the other person might call an emergency that you would not."),
                Difficulty.Medium)
            .Card("The Big Purchase",
                Rule("The car, the sofa, the thing that takes a month to research.",
                     "For something large: do you want to decide together from the start, or would you rather one of you did the work and came back with a recommendation?"),
                Difficulty.Medium)

        // ── TIME ─────────────────────────────────────────────────────────────
        .Category(TimeCategory)
            .Card("The Weeknight",
                Rule("A normal Wednesday is most of your life together, and nobody plans it.",
                     "Describe your ideal ordinary weeknight together. Then say how often the real ones look like that."),
                Difficulty.Easy)
            .Card("Alone Time",
                Rule("Wanting an evening to yourself is not a comment on the relationship, but it can land like one.",
                     "How much time on your own do you actually need in a week — and how do you usually get it?"),
                Difficulty.Medium)
            .Card("Phones",
                Rule("You already know the answer to this one. That is not the same as having agreed it.",
                     "Where and when should phones be away entirely? Be specific: which room, which meal, which hour."),
                Difficulty.Easy)
            .Card("Who Owns the Calendar",
                Rule("In most couples one person carries the diary in their head, and it is invisible work until they stop.",
                     "Who currently keeps track of what is happening this month? How did that get decided?"),
                Difficulty.Medium)
            .Card("Saying No For Two",
                Rule("An invitation arrives addressed to both of you and one of you has to answer it.",
                     "Can either of you decline something on behalf of both — or does every invitation come back for a second opinion first?"),
                Difficulty.Medium)
            .Card("The Sunday Reset",
                Rule("The week runs better or worse depending on what happened on Sunday evening.",
                     "What has to be done before Monday morning for the week to start well — and who has been doing it?"),
                Difficulty.Easy)

        // ── PEOPLE ───────────────────────────────────────────────────────────
        .Category(PeopleCategory)
            .Card("Christmas, and Whose",
                Rule("Two families, one set of dates, and a decision that gets made by default if you do not make it.",
                     "How should the big family dates be divided? Say what you would want if there were no expectations at all."),
                Difficulty.Hard)
            .Card("The Friend",
                Rule("Everyone has a partner's friend they tolerate. This is normal and rarely discussed.",
                     "Without naming them unkindly: how much of each other's social life do you want to be part of, and how much would you rather not?"),
                Difficulty.Hard)
            .Card("What Is Ours to Tell",
                Rule("You each have someone you talk to about your relationship. That is healthy, right up until it is not.",
                     "What is shareable outside the two of you, and what should never leave the house?"),
                Difficulty.Hard)
            .Card("The Group Chat",
                Rule("Photos, plans, screenshots and jokes at each other's expense.",
                     "What are you happy for the other to post, forward or repeat — and what would you rather they asked about first?"),
                Difficulty.Medium)
            .Card("Arriving and Leaving",
                Rule("One of you wants to go home. The other is talking to someone in the kitchen.",
                     "How do you signal that you want to leave a party — and what happens if only one of you is ready?"),
                Difficulty.Easy)
            .Card("When Someone Is Rude To You",
                Rule("A relative, a friend, a stranger, and your partner is standing right there.",
                     "Do you want your partner to step in, back you quietly, or stay out of it — and does the answer change depending on who it is?"),
                Difficulty.Medium)

        // ── THE FUTURE ───────────────────────────────────────────────────────
        .Category(FutureCategory)
            .Card("Where We Live",
                Rule("Most people have a private answer to this and assume the other person shares it.",
                     "Say where you would genuinely like to be living in five years, and how firmly you mean it."),
                Difficulty.Medium)
            .Card("The Job Offer",
                Rule("One of you is offered something very good, two hundred miles away.",
                     "What would you want the process to look like — not the answer, the process. Who decides, and how?"),
                Difficulty.Hard)
            .Card("Children, Or Not",
                Rule("If you have settled this, use the card to check it is still settled for both of you.",
                     "Say where you honestly stand today. \"The same as last time we talked\" is a complete answer."),
                Difficulty.Hard)
            .Card("When A Parent Needs Us",
                Rule("This arrives without warning and reorganises everything, usually for years.",
                     "If one of your parents needed real care, what would you want to happen — and what could you not do?"),
                Difficulty.Hard)
            .Card("The Money We Have Not Got Yet",
                Rule("It is easier to agree about a windfall before there is one.",
                     "An unexpected sum arrives — enough to matter, not enough to retire. What happens to it?"),
                Difficulty.Medium)
            .Card("The Five-Year Question",
                Rule("Last one, and the only one with no logistics in it.",
                     "What is one thing you would like to be true about the two of you in five years that is not quite true yet?"),
                Difficulty.Medium)

        // ── THE PACT ─────────────────────────────────────────────────────────
        .Category(PactCategory)
            .Card("The Signing",
                "Stop. Read the list aloud, top to bottom, one rule at a time.\n\n" +
                "As each one is read, either of you can strike it out — no argument, no defence required. A rule that sounded fine an hour ago and does not now was never going to survive contact with a Tuesday, and crossing it out here costs nothing. What is left is what you both still mean.\n\n" +
                "Then sign it, both of you, at the bottom. Yes, actually sign it. It is a bit silly and it works: a list with two signatures on it gets referred to, and a list without them gets lost.",
                Difficulty.Easy)
            .Card("The Diary Date",
                "One last thing, and it is the one that decides whether tonight mattered.\n\n" +
                "Pick a date roughly three months out and put it in both your calendars now, before you put the pen down. That evening you get the list out, read it, and ask two questions of every rule: did we keep it, and do we still want it?\n\n" +
                "Rules that got ignored are not failures — they are information, usually that the rule was wrong rather than that you were. Rewrite those. And bring the parked list too: some of what you could not settle tonight will settle itself in three months, and the rest will be easier for having waited.",
                Difficulty.Easy)

        .Build();
}
