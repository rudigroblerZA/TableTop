using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Restrictions;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Restrictions;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// This Is Us — family conversation cards about shared life, memories, and traditions.
///
/// Unlike generic icebreakers, these cards are about <b>your family specifically</b>.
/// Cards prompt stories, debates, comparisons, and honest answers about family life.
///
/// Three types:
///   Memory     — recall a shared moment ("tell the story of...")
///   Debate     — the family votes on something ("who is most likely to...")
///   Reveal     — a question about family knowledge ("does anyone know why...")
///
/// Some cards direct to a specific person; others are for the group.
/// Parent-specific cards are tagged accordingly.
///
/// No scoring by default — the point is the conversation and the laughter.
/// </summary>
public sealed class ThisIsUsMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "This Is Us";
    /// <inheritdoc />
    public override string Description =>
        "Stories, debates, and reveals about your family specifically. Who knows you best?";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "→ Next";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "⤳ Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [ThisIsUsCardBank.MemoryCategory] = "#42A5F5",
            [ThisIsUsCardBank.DebateCategory] = "#FFCA28",
            [ThisIsUsCardBank.RevealCategory] = "#EC407A",
            [ThisIsUsCardBank.StoriesCategory] = "#66BB6A",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        ThisIsUsCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => ThisIsUsCardBank.All;
}

/// <summary>Built-in card bank for ThisIsUs. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class ThisIsUsCardBank
{
    internal const string MemoryCategory = "Memory";
    internal const string DebateCategory = "Debate";
    internal const string RevealCategory = "Reveal";
    internal const string StoriesCategory = "Stories";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build()
    {
        var parentsOnly = new ParentOnlyRestriction();
        var adultsOnly = new AdultOnlyRestriction();

        return
        [
            // ── MEMORY — recall shared family moments ─────────────────────────

            T("The Holiday Story",
              "Tell the story of the best family holiday or trip you've ever taken. Everyone who was there adds at least one detail the others forgot.",
              MemoryCategory, Difficulty.Easy),

            T("The Most Embarrassing Moment",
              "Name the most embarrassing thing that has ever happened to this family in public. Agree on the definitive version.",
              MemoryCategory, Difficulty.Easy),

            T("The Meal That Went Wrong",
              "Who can tell the story of a family meal that completely fell apart? Burnt, forgotten, or served to the wrong people.",
              MemoryCategory, Difficulty.Easy),

            T("The Car Journey",
              "Describe the worst long car journey this family has ever taken. Everyone contributes what they remember.",
              MemoryCategory, Difficulty.Easy),

            T("The Best Christmas",
              "Each person names their single best Christmas or holiday memory. Then vote on whose was objectively the best.",
              MemoryCategory, Difficulty.Easy),

            T("The Rule That Got Changed",
              "Name a family rule that existed when you were younger that no longer exists — and why it got dropped.",
              MemoryCategory, Difficulty.Medium),

            T("The First Pet",
              "Tell the story of the first family pet. What happened? How did it end? Who was most affected?",
              MemoryCategory, Difficulty.Easy),

            T("Before I Was Born",
              "Each parent tells one story about their life before the children were born — something the children have never heard.",
              MemoryCategory, Difficulty.Medium, parentsOnly),

            T("The Great Misunderstanding",
              "Name a time when a family communication went completely wrong — a misheard instruction, a forgotten message, a mix-up that caused chaos.",
              MemoryCategory, Difficulty.Easy),

            T("The Unexpected Skill",
              "Go around the table: each person names something they can do that the family doesn't know about — or didn't know until now.",
              MemoryCategory, Difficulty.Easy),

            T("The Worst Idea We Ever Had",
              "Name the worst collective family decision you can all remember. Who proposed it? Who agreed? How did it end?",
              MemoryCategory, Difficulty.Medium),

            T("The Time Someone Got Lost",
              "Has anyone in this family ever got properly lost — in a shop, a city, anywhere? Tell the whole story.",
              MemoryCategory, Difficulty.Easy),

            T("When the Plan Completely Changed",
              "Name a time when a family plan — a trip, a meal, a day out — changed completely at the last minute. What happened instead? Was it better or worse?",
              MemoryCategory, Difficulty.Medium),

            // ── DEBATE — the family votes ─────────────────────────────────────

            T("Most Likely to Run a Marathon",
              "<b>Vote:</b> who in this family is most likely to one day run a marathon — whether or not they've expressed any desire to?\n\nEveryone votes simultaneously. Majority wins.",
              DebateCategory, Difficulty.Easy),

            T("Most Stubborn",
              "<b>Vote:</b> who is the most stubborn person in this family?\n\nEveryone votes. The winner defends themselves with one sentence.",
              DebateCategory, Difficulty.Easy),

            T("Most Likely to Move Abroad",
              "<b>Vote:</b> who is most likely to move to a different country at some point in their life?\n\nVote, then each person says where they'd go if they had to move tomorrow.",
              DebateCategory, Difficulty.Easy),

            T("Best Cook",
              "<b>Vote:</b> who is the best cook in the family?\n\nEveryone votes. The person who gets the fewest votes must now defend their cooking.",
              DebateCategory, Difficulty.Easy),

            T("Most Dramatic in a Crisis",
              "<b>Vote:</b> who is most likely to dramatically overreact to a small inconvenience?\n\nVote simultaneously. The winner may not protest — that would prove the point.",
              DebateCategory, Difficulty.Easy),

            T("Who Changed the Most",
              "<b>Vote:</b> who has changed the most since five years ago?\n\nAfter voting, each person says one way they think they've changed.",
              DebateCategory, Difficulty.Medium),

            T("Best at Keeping Secrets",
              "<b>Vote:</b> who in the family is best at keeping a secret?\n\nVote, then the winner names a secret they have successfully kept. If they can't think of one, they obviously lost.",
              DebateCategory, Difficulty.Easy),

            T("Most Likely to Become Famous",
              "<b>Vote:</b> who is most likely to become famous — and for what?\n\nVote, then each person states what they'd be famous for.",
              DebateCategory, Difficulty.Easy),

            T("Best Under Pressure",
              "<b>Vote:</b> who is best at staying calm when things go wrong?\n\nVote, then the person with the fewest votes names a time they were not calm.",
              DebateCategory, Difficulty.Medium),

            T("The Unofficial Family Motto",
              "<b>Debate:</b> what should the family motto be? Each person proposes one sentence. Group votes on the best one.",
              DebateCategory, Difficulty.Medium),

            T("Most Likely to Have a Secret Hobby",
              "<b>Vote:</b> who has a hobby or interest that the rest of the family doesn't know about?\n\nAfter voting, everyone must confess at least one thing they do alone that they haven't mentioned.",
              DebateCategory, Difficulty.Medium),

            T("The Family Superpower",
              "<b>Debate:</b> if the whole family were one superhero team, what would each person's power be — and who would be the hero, the sidekick, and the one who accidentally causes the problem?",
              DebateCategory, Difficulty.Easy),

            // ── REVEAL — knowledge about each other ───────────────────────────

            T("Do You Know Everyone's Middle Name?",
              "Without checking: does everyone know every other person's middle name?\n\nTest it. Anyone who can't answer loses a point (invent your own consequence).",
              RevealCategory, Difficulty.Easy),

            T("What Were You Afraid of as a Kid?",
              "Each person names something they were genuinely afraid of as a child — that they have never told the rest of the family before.",
              RevealCategory, Difficulty.Easy),

            T("First Memory",
              "Each person shares their earliest memory. Group votes on who has the oldest genuine memory.",
              RevealCategory, Difficulty.Easy),

            T("What Did They Want to Be?",
              "Each adult names what they wanted to be when they grew up — aged 8. Then ages 15. Compare.",
              RevealCategory, Difficulty.Medium),

            T("What Does Your Name Mean?",
              "Does everyone know what their name means or why they were given it?\n\nParents explain the story behind each name — if there is one.",
              RevealCategory, Difficulty.Easy, parentsOnly),

            T("The Thing They Don't Know About You",
              "Each person tells the rest of the family one true thing about themselves they're fairly confident the others don't know.",
              RevealCategory, Difficulty.Medium),

            T("First Impression",
              "Go around the table. Each person says what they first thought of each other person — in one sentence. Only kind truths allowed.",
              RevealCategory, Difficulty.Medium),

            T("The Worry They Carry",
              "Each person names something they've been quietly worried about lately — doesn't have to be big. Group listens without offering solutions until everyone has spoken.",
              RevealCategory, Difficulty.Hard, adultsOnly),

            T("What Do You Want This Year?",
              "Each person says one thing they genuinely want for themselves in the next twelve months. Specific enough to be measurable.",
              RevealCategory, Difficulty.Medium),

            // ── STORIES — collaborative family storytelling ────────────────────

            T("The Family Legend",
              "Tell the most embellished version of a family story — the version that has grown over the years. Who knows the most details? Who added the most exaggerations?",
              StoriesCategory, Difficulty.Easy),

            T("The Ideal Family Holiday",
              "Each person describes their ideal family holiday in complete detail. Group votes on whose they'd most want to go on.",
              StoriesCategory, Difficulty.Easy),

            T("If We Were a TV Show",
              "If your family were a TV show, what genre would it be? Who would be the main character? Name the show, the cast, and the episode you're currently in.",
              StoriesCategory, Difficulty.Easy),

            T("One Year from Now",
              "Each person describes in one paragraph exactly where they hope to be in one year. Read them all out. Group votes on most likely to come true.",
              StoriesCategory, Difficulty.Medium),

            T("The Ancestor Story",
              "Does anyone know a story about an ancestor — a grandparent, great-grandparent, or further back — that the whole group may not know? Tell it.",
              StoriesCategory, Difficulty.Medium),

            T("The Time We Laughed Until It Hurt",
              "Each person tells the story of the time this family laughed the hardest. Vote on the best one.",
              StoriesCategory, Difficulty.Easy),
        ];
    }

    private static ICard T(string title, string text, string category, Difficulty d,
        IRestriction? restriction = null) =>
        StandardCard.Create(title, text, d, category, restriction: restriction);
}