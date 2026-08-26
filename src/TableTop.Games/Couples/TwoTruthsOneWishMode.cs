using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Abstractions.Restrictions;
using TableTop.Core.Domain.Restrictions;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Two Truths, One Wish — a couples variant of the classic game.
///
/// Standard Two Truths One Lie replaces the lie with a <b>wish</b>.
/// This is more interesting for couples: both truths AND the wish reveal
/// something real about the person and what they want.
///
/// PLAY: The active player reads the card prompt and gives:
///   • <b>Two true things</b> about themselves (or the relationship) matching the prompt.
///   • <b>One wish</b> — something true about what they want or hope for.
///
/// Their partner guesses which of the three is the wish.
/// Then they explain all three.
///
/// Scoring: Partner gets 1 pt for correct guess. Player gets 1 pt for a wish that
/// surprises their partner.
///
/// Cards are organised by category:
///   About Me       — individual: past, habits, desires, fears
///   About Us       — the relationship itself
///   About the Future — what each person wants
///   Big Questions  — the hard stuff
/// </summary>
public sealed class TwoTruthsOneWishMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Two Truths, One Wish";
    /// <inheritdoc />
    public override string Description =>
        "Two true things, one wish — partner guesses which is the wish. No lies, only hopes.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ Revealed (+1)";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel     => "→ Next card";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["About Me"]      = "#42A5F5",
            ["About Us"]      = "#66BB6A",
            ["The Future"]    = "#FFCA28",
            ["Big Questions"] = "#EC407A",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        TwoTruthsOneWishCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => TwoTruthsOneWishCardBank.All;
}

/// <summary>Built-in card bank for TwoTruthsOneWish. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class TwoTruthsOneWishCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build()
    {
        var couplesOnly = new CoupleOnlyRestriction();
        var couples18   = couplesOnly.And(new AdultOnlyRestriction());

        return
        [
            // ════════════════════════════════════════════════════════════════
            // ABOUT ME — individual truths + personal wish
            // ════════════════════════════════════════════════════════════════

            W("What I'm Proud Of",
              "Give two things you are genuinely proud of about yourself right now, and one thing you wish you could be proud of — something you haven't achieved yet but want to.",
              "About Me", Difficulty.Easy, couplesOnly),

            W("What I'm Afraid Of",
              "Give two real fears — not the easy ones you always say, but ones that actually keep you up — and one fear you <i>wish</i> you had instead (because it would mean something important to you).",
              "About Me", Difficulty.Medium, couplesOnly),

            W("What I Want for Myself",
              "Give two things you genuinely want for your own life in the next five years, and one thing you wish you wanted — something you think you <i>should</i> want but aren't sure you do.",
              "About Me", Difficulty.Medium, couplesOnly),

            W("How I Show Up",
              "Give two true things about how you behave in relationships — habits, tendencies, patterns you know about yourself. And one thing you wish you could change about how you show up.",
              "About Me", Difficulty.Hard, couplesOnly),

            W("What I'm Good At",
              "Give two things you know you are genuinely good at — not modestly, actually good. And one thing you wish you were good at.",
              "About Me", Difficulty.Easy, couplesOnly),

            W("What I Miss",
              "Give two things from your past life — places, people, habits, a version of yourself — that you genuinely miss. And one thing you wish you missed more than you actually do.",
              "About Me", Difficulty.Medium, couplesOnly),

            W("How I Handle Difficulty",
              "Give two true things about how you cope when life is hard — strategies you actually use. And one thing you wish you did instead.",
              "About Me", Difficulty.Hard, couplesOnly),

            W("My Relationship With My Body",
              "Give two true things about how you relate to your body — things you accept, things that are complicated. And one thing you wish were different.",
              "About Me", Difficulty.Hard, couplesOnly),

            W("What I Believe",
              "Give two things you believe genuinely and deeply — not politically, but about life or people. And one thing you wish you believed.",
              "About Me", Difficulty.Medium, couplesOnly),

            W("What I Need",
              "Give two things you know you need in life to function well. And one thing you wish you needed less.",
              "About Me", Difficulty.Hard, couplesOnly),

            // ════════════════════════════════════════════════════════════════
            // ABOUT US — the relationship itself
            // ════════════════════════════════════════════════════════════════

            W("What I Love About Us",
              "Give two specific things you love about your relationship — not your partner in general, but things about what you two are together. And one thing you wish were true of us that isn't yet.",
              "About Us", Difficulty.Easy, couplesOnly),

            W("What We're Good At Together",
              "Give two things you do well as a couple — actual strengths. And one thing you wish we were better at.",
              "About Us", Difficulty.Medium, couplesOnly),

            W("How We Fight",
              "Give two true things about how arguments go between you — patterns you've noticed, things that happen. And one thing you wish were different about how you fight.",
              "About Us", Difficulty.Hard, couplesOnly),

            W("What I Take for Granted",
              "Give two things about your partner or your relationship that you probably take for granted — things you rely on without always acknowledging. And one thing you wish your partner took for granted (that they don't).",
              "About Us", Difficulty.Hard, couplesOnly),

            W("What We Don't Talk About",
              "Give two things you don't talk about enough as a couple — topics, feelings, or parts of life that go mostly unaddressed. And one thing you wish we talked about more.",
              "About Us", Difficulty.Hard, couplesOnly),

            W("How We Show Love",
              "Give two specific ways you know you show love to your partner — things you actually do, not just intend. And one way you wish you showed it more.",
              "About Us", Difficulty.Medium, couplesOnly),

            W("Our Best Period",
              "Give two true things about what our best period together felt like — specific qualities. And one thing you wish could come back from that time.",
              "About Us", Difficulty.Medium, couplesOnly),

            W("What I Think We Need",
              "Give two things you genuinely think your relationship needs more of. And one thing you wish you wanted for us that you're not sure you do.",
              "About Us", Difficulty.Hard, couplesOnly),

            // ════════════════════════════════════════════════════════════════
            // THE FUTURE — what each person wants
            // ════════════════════════════════════════════════════════════════

            W("Where I Want to Be in Ten Years",
              "Give two true things about where you want to be in ten years — specific enough to be meaningful. And one wish for us in ten years that feels vulnerable to say.",
              "The Future", Difficulty.Medium, couplesOnly),

            W("What I Want Our Life to Look Like",
              "Give two true things about what you want your everyday life to look like — not dreams, but actual preferences for how life should feel. And one thing you wish you wanted that would make our lives easier to align.",
              "The Future", Difficulty.Medium, couplesOnly),

            W("What Scares Me About the Future",
              "Give two real things about the future that genuinely worry you — specific, not vague. And one thing you wish you were more afraid of (because being afraid of it would mean you cared about it more).",
              "The Future", Difficulty.Hard, couplesOnly),

            W("What I Want to Have Done",
              "Give two things you want to have done — lived, experienced, achieved — before you die. And one thing you wish mattered more to you than it currently does.",
              "The Future", Difficulty.Medium, couplesOnly),

            W("What I Hope For Us",
              "Give two specific hopes for your relationship — not vague ones, but something concrete. And one hope you've never said out loud.",
              "The Future", Difficulty.Extreme, couplesOnly),

            // ════════════════════════════════════════════════════════════════
            // BIG QUESTIONS — the difficult ones
            // ════════════════════════════════════════════════════════════════

            W("What I Know About Love",
              "Give two true things you know about love — things experience has taught you. And one thing you wish you knew.",
              "Big Questions", Difficulty.Hard, couplesOnly),

            W("What I Think About Death",
              "Give two honest things about how you think about death — how often, in what terms. And one thing you wish you could believe about it.",
              "Big Questions", Difficulty.Hard, couplesOnly),

            W("What I Think Happiness Is",
              "Give two true things about what happiness actually is for you — not what it's supposed to be. And one thing you wish made you happier than it does.",
              "Big Questions", Difficulty.Hard, couplesOnly),

            W("What I Believe About Us",
              "Give two things you genuinely believe about your relationship — true beliefs, held now. And one thing you wish you could believe that you're not sure you do.",
              "Big Questions", Difficulty.Extreme, couplesOnly),

            W("What I Think About Regret",
              "Give two specific things you regret — genuinely, not performatively. And one thing you wish you regretted more than you do.",
              "Big Questions", Difficulty.Extreme, couplesOnly),

            W("What I Want You to Know",
              "Give two true things you want your partner to know about you — things you're not sure they fully understand. And one thing you wish you wanted them to know that you're still holding back.",
              "Big Questions", Difficulty.Extreme, couplesOnly),
        ];
    }

    private static ICard W(string title, string text, string category, Difficulty d, IRestriction restriction) =>
        StandardCard.Create(title, text, d, category, restriction: restriction);
}