using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Restrictions;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Restrictions;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Couples Questions — 36 structured conversation cards for romantic partners.
///
/// Structured in three sets of 12, inspired by psychological research into
/// interpersonal closeness. Each set goes deeper than the last.
///
/// SET 1 — Shared Ground:  lighter questions establishing shared history, values,
///          preferences, and how you see each other now.
/// SET 2 — Honest Ground:  questions about what you want, what you fear, how you
///          hurt and how you're hurt, and what you carry alone.
/// SET 3 — Open Ground:    the most vulnerable questions — about the relationship
///          itself, what you've never said, what you hope for, what you need.
///
/// Both players answer every question. No scoring — the point is the conversation.
/// After answering, look at each other in silence for four minutes (final card).
///
/// For two players in an established romantic relationship.
/// </summary>
public sealed class CouplesQuestionsMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Couples Questions";
    /// <inheritdoc />
    public override string Description =>
        "36 questions that go deeper each set. Both of you answer every one. For two.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "→ Next question";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "⤳ Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [CouplesQuestionsCardBank.Set1Category] = "#26C6DA",
            [CouplesQuestionsCardBank.Set2Category] = "#FFCA28",
            [CouplesQuestionsCardBank.Set3Category] = "#EC407A",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0); // no scoring — just conversation

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        CouplesQuestionsCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => CouplesQuestionsCardBank.All;
}

/// <summary>Built-in card bank for CouplesQuestions. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class CouplesQuestionsCardBank
{
    internal const string Set1Category = "Set 1";
    internal const string Set2Category = "Set 2";
    internal const string Set3Category = "Set 3";

    // Every card here is couples-only, so the restriction is a default on Q
    // rather than an argument repeated 36 times. Still one shared instance, as
    // the local it replaces was — CoupleOnlyRestriction holds no state.
    //
    // Declared before All, and that order matters: static field initialisers run
    // in declaration order, so All = Build() reading this field from above it
    // would read null and hand every card a null restriction.
    private static readonly CoupleOnlyRestriction CouplesOnly = new();

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ════════════════════════════════════════════════════════════════
        // SET 1 — SHARED GROUND
        // Light enough to warm up. Still specific enough to reveal something.
        // ════════════════════════════════════════════════════════════════

        Q("1.  Your Perfect Day",
          "Describe your ideal day — from the moment you wake up to the moment you fall asleep. What does it include, and what does it not include?",
          Set1Category, Difficulty.Easy),

        Q("2.  What Would Make You Famous",
          "If you could be famous for anything, what would you choose — and why that specifically?",
          Set1Category, Difficulty.Easy),

        Q("3.  The Phone Call You'd Rehearse",
          "Before making an important phone call, do you rehearse what you're going to say? Which calls make you do this?",
          Set1Category, Difficulty.Easy),

        Q("4.  What Your Name Means to You",
          "What does your name mean to you? Do you like it? Does it feel like you?",
          Set1Category, Difficulty.Easy),

        Q("5.  Last Time You Sang to Yourself",
          "When did you last sing to yourself — or to someone else? What were you singing?",
          Set1Category, Difficulty.Easy),

        Q("6.  If You Could Live to 90",
          "If you could choose between keeping the body or the mind of a 30-year-old for the rest of your life after 90, which would you pick and why?",
          Set1Category, Difficulty.Easy),

        Q("7.  How You Think We'll Die",
          "Do you have a sense — not morbid, just honest — of how you might die? How much does it cross your mind?",
          Set1Category, Difficulty.Easy),

        Q("8.  Three Things We Have in Common",
          "Name three things you think we have in common that you haven't told me before. They can be big or completely trivial.",
          Set1Category, Difficulty.Easy),

        Q("9.  What You're Most Grateful For",
          "What are you most grateful for in your life right now? Be specific — not a general category but a specific thing.",
          Set1Category, Difficulty.Easy),

        Q("10.  If You Could Change One Thing About How You Were Raised",
          "If you could change one thing about how you were raised, what would it be?",
          Set1Category, Difficulty.Easy),

        Q("11.  Tell Me Your Life Story",
          "Tell me your life story in four minutes — as much detail as you can. Go.",
          Set1Category, Difficulty.Easy),

        Q("12.  The Thing You'd Most Want to Learn",
          "If you could wake up tomorrow with one new skill or ability fully mastered, what would you choose?",
          Set1Category, Difficulty.Easy),

        // ════════════════════════════════════════════════════════════════
        // SET 2 — HONEST GROUND
        // Requires more trust. Moves into values, fears, and the relationship.
        // ════════════════════════════════════════════════════════════════

        Q("13.  What You Like Most About Yourself",
          "What do you like most about yourself? Not your qualities in general — specifically today, right now.",
          Set2Category, Difficulty.Medium),

        Q("14.  Your Most Treasured Memory",
          "What is your most treasured memory?",
          Set2Category, Difficulty.Medium),

        Q("15.  Your Worst Memory",
          "What is your worst memory? You don't have to explain it fully — just name it.",
          Set2Category, Difficulty.Medium),

        Q("16.  What Would Change If You Knew You Had One Year",
          "If you knew you would die in exactly one year, what would change about how you are living now?",
          Set2Category, Difficulty.Medium),

        Q("17.  What Friendship Means to You",
          "What does friendship mean to you? What do you actually want from a close friend?",
          Set2Category, Difficulty.Medium),

        Q("18.  The Role Love Has Played",
          "What role has love played in your life so far? Not just romantic love — all of it.",
          Set2Category, Difficulty.Medium),

        Q("19.  What You Share With No One",
          "Share something about yourself that you normally keep from people in general — something you consider a weakness or a source of shame.",
          Set2Category, Difficulty.Hard),

        Q("20.  If You Were Going to Die",
          "If you were going to die tonight and couldn't contact anyone, what would you most regret not having told someone? What has stopped you?",
          Set2Category, Difficulty.Hard),

        Q("21.  Our Friendship",
          "What do you value most about our relationship — not as a couple necessarily, but as two people who know each other?",
          Set2Category, Difficulty.Hard),

        Q("22.  The Best Memory With Family",
          "What is your most cherished memory of your family? It can be childhood or much more recent.",
          Set2Category, Difficulty.Medium),

        Q("23.  How Close Is Your Family",
          "How close is your family? How does that compare with what you see in other families?",
          Set2Category, Difficulty.Medium),

        Q("24.  Your Relationship With Your Mother",
          "How do you feel about your relationship with your mother? How has it changed as you've both got older?",
          Set2Category, Difficulty.Medium),

        // ════════════════════════════════════════════════════════════════
        // SET 3 — OPEN GROUND
        // The most vulnerable questions. About us, about what you carry, about what you want.
        // ════════════════════════════════════════════════════════════════

        Q("25.  Three True Sentences",
          "Complete the following sentences three times each:\n\n\"I wish I had someone to...\"\n\"I would never tell this to...\"\n\"I hope that...\"\n\nThen listen while I do the same.",
          Set3Category, Difficulty.Hard),

        Q("26.  What You'd Tell Your Friend About Me",
          "If we were close friends rather than partners, what would you tell your other friends about me? What's the version of me they'd hear?",
          Set3Category, Difficulty.Hard),

        Q("27.  What You Want Me to Know About You",
          "Tell me something it's important for me to know about you as a person — something you don't think I fully understand yet.",
          Set3Category, Difficulty.Hard),

        Q("28.  The Thing You Love Most About Our Life",
          "What do you love most about our life together — not the big things, but something specific and ordinary that you would miss if it were gone?",
          Set3Category, Difficulty.Hard),

        Q("29.  What I Do That Moves You",
          "Tell me the last time I did something that moved you — something small or specific, not a grand gesture.",
          Set3Category, Difficulty.Hard),

        Q("30.  How You Handle Embarrassment",
          "Tell me something embarrassing that happened to you recently. How did you feel in the moment? Is there a pattern to when you get embarrassed?",
          Set3Category, Difficulty.Hard),

        Q("31.  When Did You Last Cry",
          "When did you last cry alone? When did you last cry with another person? What made the difference?",
          Set3Category, Difficulty.Hard),

        Q("32.  What You Already Like About Me",
          "Tell me something about me that you already like — one specific thing that, if I changed it, would actually make me less me.",
          Set3Category, Difficulty.Hard),

        Q("33.  What Is Too Serious for Jokes",
          "Is there anything in your life that feels too serious to joke about? Has that always been true?",
          Set3Category, Difficulty.Hard),

        Q("34.  If You Were Going to Die",
          "If you were going to die tonight — right now — and you had three things left to say to three different people, who and what?",
          Set3Category, Difficulty.Extreme),

        Q("35.  What We Need to Say",
          "What is something we need to talk about that we keep not talking about? Name it — you don't have to solve it tonight.",
          Set3Category, Difficulty.Extreme),

        Q("36.  Four Minutes",
          "This is the last card.\n\nPut down your phones.\nTurn to face each other.\nLook at each other in silence for four minutes.\n\nNothing else.",
          Set3Category, Difficulty.Extreme),
    ];

    private static ICard Q(string title, string text, string category, Difficulty d,
                           IRestriction? restriction = null) =>
        StandardCard.Create(title, text, d, category, restriction: restriction ?? CouplesOnly);
}