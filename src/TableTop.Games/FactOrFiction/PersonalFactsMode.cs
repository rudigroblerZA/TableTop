using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.FactOrFiction;

/// <summary>
/// Personal Facts — The ultimate "how well do you know each other" game.
///
/// How to play:
///   1. Each player reads a prompt (e.g., "Tell us three things: two true, one false").
///   2. Player shares their three statements.
///   3. Others vote which one is the lie.
///   4. Correct guessers get points. Player gets bonus if they fool everyone.
///
/// Two variants:
///   Standard: Two truths, one lie (what everyone knows)
///   Expert: Three truths, one lie (harder to spot inconsistencies)
///
/// Perfect for: getting to know people, parties, team building, travel buddies.
/// Reveals personality, weird experiences, and hidden depths.
/// </summary>
public sealed class PersonalFactsMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Personal Facts";
    /// <inheritdoc />
    public override string Description =>
        "Share three personal statements — two true, one false. Others guess which is the lie.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ Guess";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "→ Next Player";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [PersonalFactsCardBank.IcebreakerCategory] = "#42A5F5",
            [PersonalFactsCardBank.ChildhoodCategory] = "#66BB6A",
            [PersonalFactsCardBank.ExperienceCategory] = "#FFCA28",
            [PersonalFactsCardBank.EmbarrassingCategory] = "#EC407A",
            [PersonalFactsCardBank.WeirdTalentCategory] = "#AB47BC",
            [PersonalFactsCardBank.TravelCategory] = "#26C6DA",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        PersonalFactsCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => PersonalFactsCardBank.All;
}

/// <summary>Built-in card bank for PersonalFacts. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class PersonalFactsCardBank
{
    internal const string IcebreakerCategory = "Icebreaker";
    internal const string ChildhoodCategory = "Childhood";
    internal const string ExperienceCategory = "Experience";
    internal const string EmbarrassingCategory = "Embarrassing";
    internal const string WeirdTalentCategory = "Weird Talent";
    internal const string TravelCategory = "Travel";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── ICEBREAKER — Easy, fun, non-threatening ──────────────────────

        P("Tell three things: your most embarrassing childhood moment, your first concert, and something you've never admitted. Two are true.",
          Difficulty.Easy, IcebreakerCategory),

        P("Share three things: a food you hate that everyone loves, a place you'd love to visit, and an instrument you can play. One is false.",
          Difficulty.Easy, IcebreakerCategory),

        P("Tell us: something you're secretly good at, something you wish you were good at, and something you lied about once. Two are honest.",
          Difficulty.Easy, IcebreakerCategory),

        P("Share three things: a hobby nobody knows about, the last film that made you cry, and a skill you're learning. One is made up.",
          Difficulty.Easy, IcebreakerCategory),

        P("Tell three things: the most expensive thing you own, the weirdest compliment you've received, and a show you're obsessed with. Two are true.",
          Difficulty.Easy, IcebreakerCategory),

        // ── CHILDHOOD — Memories and formative experiences ─────────────────

        P("Share three memories: your best birthday ever, the time you got in the most trouble, and a pet you had as a kid. One is fabricated.",
          Difficulty.Medium, ChildhoodCategory),

        P("Tell us: the worst haircut you ever had, your most prized childhood possession, and something you believed in that wasn't real. Two are true.",
          Difficulty.Medium, ChildhoodCategory),

        P("Share three things: a broken bone or major injury from childhood, your first best friend's name, and the school you were most miserable in. One is false.",
          Difficulty.Medium, ChildhoodCategory),

        P("Tell three things: the nickname you hated most as a kid, your first crush's name, and the stupidest thing you did in school. Two are honest.",
          Difficulty.Medium, ChildhoodCategory),

        P("Share three memories: a time you lied to your parents, a talent you had as a kid you've lost, and your worst school subject. One is made up.",
          Difficulty.Medium, ChildhoodCategory),

        // ── EXPERIENCE — Adventures and life events ──────────────────────

        P("Tell three things: the scariest moment of your life, the best decision you ever made, and the biggest risk you've taken. One is invented.",
          Difficulty.Medium, ExperienceCategory),

        P("Share three experiences: a time you were completely lost, a moment you felt truly free, and something you did you immediately regretted. Two are true.",
          Difficulty.Medium, ExperienceCategory),

        P("Tell us: a job you've had that embarrasses you, a time you were fired or quit dramatically, and the most boring trip you've taken. One is false.",
          Difficulty.Medium, ExperienceCategory),

        P("Share three things: the closest you've come to serious injury, a time you helped someone without them knowing, and your wildest night out. Two are real.",
          Difficulty.Medium, ExperienceCategory),

        P("Tell three things: an award or recognition you're proud of, a goal you gave up on, and the nicest thing someone did for you. One is made up.",
          Difficulty.Medium, ExperienceCategory),

        // ── EMBARRASSING — Cringeworthy moments and confessions ───────────

        P("Share three things: the most mortifying social moment, something you've done alone that would horrify you if others knew, and a lie you told to avoid something. Two are true.",
          Difficulty.Hard, EmbarrassingCategory),

        P("Tell us: the most disgusting thing you've ever done, something weird you do in private, and a time you cried at something silly. One is invented.",
          Difficulty.Hard, EmbarrassingCategory),

        P("Share three confessions: something you're ashamed of, something you secretly enjoy that's uncool, and a time you pretended to be sick. Two are honest.",
          Difficulty.Hard, EmbarrassingCategory),

        P("Tell three things: the worst date you've ever had, something you've done that you've never told anyone, and a habit you're disgusted by in yourself. One is false.",
          Difficulty.Hard, EmbarrassingCategory),

        P("Share three things: a time you got caught doing something you shouldn't, the most childish thing you still do, and something you've cried over that seems silly. Two are real.",
          Difficulty.Hard, EmbarrassingCategory),

        // ── WEIRD TALENT — Strange abilities and hidden skills ───────────

        P("Tell three things: something you can do that seems impossible, a weird sound you can make, and a skill you have that surprises people. One is false.",
          Difficulty.Hard, WeirdTalentCategory),

        P("Share three abilities: something you can do with your body that's unusual, a language you speak, and something you're weirdly good at. Two are true.",
          Difficulty.Hard, WeirdTalentCategory),

        P("Tell us: something you can do that nobody would guess, a weird talent you had as a kid, and something you can do that looks like magic. One is invented.",
          Difficulty.Hard, WeirdTalentCategory),

        P("Share three things: the most useless skill you have, something you can do that's actually impressive, and something you've trained yourself to do. Two are real.",
          Difficulty.Hard, WeirdTalentCategory),

        P("Tell three things: a talent you're hiding from most people, something you can do that took you years to learn, and a weird physical ability you have. One is made up.",
          Difficulty.Hard, WeirdTalentCategory),

        // ── TRAVEL — Adventures and destinations ─────────────────────────

        P("Share three travel experiences: the place that changed you most, the worst travel experience, and the most expensive trip you've taken. One is false.",
          Difficulty.Medium, TravelCategory),

        P("Tell us: a country you've been to that you'd never go back to, the most amazing meal you've had, and a place you'd like to visit before you die. Two are true.",
          Difficulty.Medium, TravelCategory),

        P("Share three things: a time you got lost in another country, the place where you felt most alive, and a terrible travel decision you made. One is invented.",
          Difficulty.Medium, TravelCategory),

        P("Tell three things: a country you've never been to but want to, the longest flight you've taken, and a place you visited by accident. Two are honest.",
          Difficulty.Medium, TravelCategory),

        P("Share three experiences: a spontaneous trip you took, the best souvenir you own, and a travel destination that disappointed you. One is made up.",
          Difficulty.Medium, TravelCategory),

        // ── EXPERT — Deeply personal, harder to spot ─────────────────────

        P("Tell three things: something only your closest friend knows, something you've never told anyone, and something that's changed you fundamentally. One is false.",
          Difficulty.Extreme, EmbarrassingCategory),

        P("Share three things: your biggest fear, something you regret deeply, and a moment that shaped who you are. Two are true.",
          Difficulty.Extreme, EmbarrassingCategory),

        P("Tell us: the person you were most jealous of, something you want people to know about you, and a secret you've kept for years. One is invented.",
          Difficulty.Extreme, EmbarrassingCategory),

        P("Share three things: what you'd change about yourself, a moment you felt truly loved, and something nobody would believe about you if they knew. Two are real.",
          Difficulty.Extreme, EmbarrassingCategory),

        P("Tell three deeply personal things: something you're scared of, a dream you're chasing, and something only you know. One is made up.",
          Difficulty.Extreme, EmbarrassingCategory),
    ];

    private static ICard P(string text, Difficulty d, string category) =>
        StandardCard.Create("Personal Facts", text, d, category);
}