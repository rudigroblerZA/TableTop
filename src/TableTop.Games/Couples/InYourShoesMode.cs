using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// In Your Shoes — how well do you actually know how your partner thinks?
///
/// The twist that makes this its own game: you don't answer as yourself. You
/// answer <em>as your partner</em> — predicting what THEY would say — and then
/// they reveal their real answer. It's the opposite move from Mind Meld (where
/// you both answer the same prompt as yourselves and hope to match); here one
/// of you plays the other, out loud, and gets marked on how close they landed.
///
/// How to play:
///   1. Draw a card. One partner is "the guesser" this round.
///   2. The guesser answers the prompt AS their partner would — first person,
///      in character: "I'd pick the window seat, because…"
///   3. The partner then reveals their real answer.
///   4. Score by how well the guess landed — the back of every card is a
///      three-tier guide (Nailed it / Close / Missed) so the table judges
///      consistently rather than arguing. Then swap who guesses.
///
/// Three depths: Everyday (harmless preferences), Inner (values, fears, hopes),
/// and Us (how your partner sees the relationship — the vulnerable tier, where
/// a good guess means the most and a miss is the most worth talking about).
/// </summary>
public sealed class InYourShoesMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name        => "In Your Shoes";
    /// <inheritdoc />
    public override string Description =>
        "Answer as your partner would — then they reveal the truth. How well do you really know them?";

    /// <summary>Label for a guessed-and-revealed round.</summary>
    public override string CompleteLabel => "Revealed";
    /// <summary>Label for skipping a card.</summary>
    public override string SkipLabel     => "Pass";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Everyday"] = "#42A5F5",
            ["Inner"]    = "#AB47BC",
            ["Us"]       = "#EC407A",
        };

    /// <summary>A closer read of your partner (harder tier) is worth more.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in In Your Shoes card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        InYourShoesCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => InYourShoesCardBank.All;
}

/// <summary>Built-in card bank for In Your Shoes.</summary>
public static class InYourShoesCardBank
{
    /// <summary>All cards, ordered by depth.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── EVERYDAY — low stakes, fast, funny ───────────────────────────────
        S("Everyday", "The Last Slice",
          "There's one slice of pizza left. What does your partner do — take it, offer it, or split it?",
          "Nailed it: you named their move AND their reasoning. Close: right move, wrong reason. Missed: they'd do the opposite.",
          Difficulty.Easy),
        S("Everyday", "Free Saturday",
          "A totally empty Saturday appears. How does your partner most want to spend it?",
          "Nailed it: you described their ideal day and they'd change nothing. Close: right vibe, wrong details. Missed: that's YOUR Saturday, not theirs.",
          Difficulty.Easy),
        S("Everyday", "The Grocery Run",
          "Your partner walks into a shop for one thing. What do they actually walk out with?",
          "Nailed it: you know their weakness aisle. Close: right shop, wrong treat. Missed: not even close.",
          Difficulty.Easy),
        S("Everyday", "Road Trip Radio",
          "Who controls the music on a long drive, and what's on? Answer as your partner would.",
          "Nailed it: you could DJ for them. Close: right genre, wrong artist. Missed: they'd skip every song.",
          Difficulty.Easy),
        S("Everyday", "The Ideal Night In",
          "Describe your partner's perfect stay-at-home evening, in their words.",
          "Nailed it: that's their exact night. Close: right shape, missing one favourite thing. Missed: you built your own night again.",
          Difficulty.Medium),
        S("Everyday", "Pet Peeve",
          "What's the small everyday thing that most reliably annoys your partner?",
          "Nailed it: you named the exact trigger. Close: right area, wrong specifics. Missed: news to them.",
          Difficulty.Medium),
        S("Everyday", "Comfort Order",
          "Your partner's had a rough day and orders comfort food. What is it?",
          "Nailed it: their exact order. Close: right craving, wrong dish. Missed: they'd never.",
          Difficulty.Easy),
        S("Everyday", "The Group Chat",
          "In a group chat, is your partner the planner, the lurker, the joker, or the one who leaves on read? Answer as them.",
          "Nailed it: dead on. Close: two of those on a good day. Missed: wrong person entirely.",
          Difficulty.Medium),

        // ── INNER — values, fears, hopes ─────────────────────────────────────
        S("Inner", "The Proud Moment",
          "What accomplishment is your partner secretly (or openly) most proud of? Answer as them.",
          "Nailed it: you named the moment that matters most to them. Close: a real source of pride, not THE one. Missed: you guessed what YOU'RE proud of them for.",
          Difficulty.Medium),
        S("Inner", "The Quiet Worry",
          "What's something your partner worries about more than they let on?",
          "Nailed it: you saw the worry under the surface. Close: adjacent worry. Missed: they're not worried about that at all.",
          Difficulty.Hard),
        S("Inner", "What They'd Change",
          "If your partner could change one thing about their daily life, what would it be?",
          "Nailed it: exactly what they'd fix first. Close: on their list, not the top. Missed: they like that thing.",
          Difficulty.Hard),
        S("Inner", "The Recharge",
          "After a draining week, what genuinely refills your partner — people, quiet, movement, or making something?",
          "Nailed it: you know how they recharge. Close: works sometimes for them. Missed: that would drain them more.",
          Difficulty.Medium),
        S("Inner", "The Compliment That Lands",
          "What kind of compliment actually means something to your partner — about their looks, their mind, their effort, or their heart?",
          "Nailed it: that's the one that gets through. Close: they'll take it, but it's not THE one. Missed: that one bounces off.",
          Difficulty.Hard),
        S("Inner", "Five Years",
          "Answer as your partner: 'In five years, the thing I most hope is true about my life is…'",
          "Nailed it: you spoke their hope out loud. Close: right direction, wrong destination. Missed: that's your dream, not theirs.",
          Difficulty.Hard),
        S("Inner", "The Childhood Thread",
          "What's one thing from your partner's childhood that still shapes how they act today?",
          "Nailed it: you connected the thread. Close: right era, wrong thread. Missed: unconnected.",
          Difficulty.Extreme),
        S("Inner", "The Unspoken No",
          "What's something your partner quietly wishes they could say 'no' to more often?",
          "Nailed it: you named the thing they overcommit to. Close: a real one, not the biggest. Missed: they're fine with that.",
          Difficulty.Extreme),

        // ── US — how your partner sees the relationship ──────────────────────
        S("Us", "Their Favourite 'Us'",
          "Answer as your partner: 'My favourite thing about us is…'",
          "Nailed it: you named what they treasure most. Close: something they love, not the top. Missed: that's YOUR favourite thing about us.",
          Difficulty.Medium),
        S("Us", "The Thing I Do",
          "What's one small thing YOU do that your partner secretly loves — said in their voice?",
          "Nailed it: you know your own good magic. Close: they like it, but there's a better one. Missed: modesty or a total miss.",
          Difficulty.Hard),
        S("Us", "When They Felt Closest",
          "Answer as your partner: 'The moment I felt closest to you was…'",
          "Nailed it: you both remember the same moment. Close: same season of the relationship. Missed: different memories entirely — worth talking about.",
          Difficulty.Hard),
        S("Us", "What They'd Protect",
          "If your partner could protect one ritual or habit the two of you share, which would they guard?",
          "Nailed it: you named their sacred one. Close: something they'd keep, not first. Missed: they'd trade that one away.",
          Difficulty.Hard),
        S("Us", "The Reassurance",
          "Answer as your partner: 'The thing I most need to hear from you when I'm struggling is…'",
          "Nailed it: you know exactly what steadies them. Close: it helps, but there's a truer one. Missed: that's what YOU need to hear.",
          Difficulty.Extreme),
        S("Us", "How They'd Describe Us",
          "If your partner described your relationship to a stranger in one sentence, what would they say?",
          "Nailed it: you'd both write the same sentence. Close: same meaning, different words. Missed: two different relationships on paper.",
          Difficulty.Extreme),
        S("Us", "The Small Fear",
          "Answer as your partner: 'One small thing I sometimes worry about with us is…'",
          "Nailed it: you met a real, gentle worry with honesty. Close: adjacent. Missed: they don't carry that — but now you're talking, which is the point.",
          Difficulty.Extreme),
    ];

    private static ICard S(string category, string title, string prompt, string scoringGuide, Difficulty d) =>
        StandardCard.Create(
            title,
            "<b>👟 " + category.ToUpperInvariant() + "</b>\n\n" +
            "<i>Answer as your partner would — in their voice, first person. Then they reveal the truth.</i>\n\n" +
            prompt + "\n\n" +
            "The reading: " + scoringGuide,
            d, category);
}
