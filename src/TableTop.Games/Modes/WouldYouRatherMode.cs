using TableTop.Games.Base;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Abstractions.Restrictions;
using TableTop.Core.Domain.Restrictions;
using TableTop.Core.Domain.Scoring;

namespace TableTop.Games;

/// <summary>
/// Would You Rather: each player reads their personalised A/B dilemma and the group
/// must guess which option they chose before they reveal the answer.
///
/// Scoring: +1 for a correct group guess, +1 for the player explaining their reasoning.
/// </summary>
public sealed class WouldYouRatherMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Would You Rather";
    /// <inheritdoc />
    public override string Description =>
        "Gender-directed dilemmas. The group guesses your choice before you reveal it.";

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 2);

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Revealed (+2 pts)";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel     => "Skipped";


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
        var adultsOnly  = new AdultOnlyRestriction();
        var couplesOnly = new CoupleOnlyRestriction();
        var parentsOnly = new ParentOnlyRestriction();

        return
        [
            // ── Easy ──────────────────────────────────────────────────────────
            PromptCard.CreateGenderDirected(
                title: "Social Dilemma",
                maleText:   "Would you rather: be known as the funniest person in every room, OR the most reliable?",
                femaleText: "Would you rather: always say exactly what you think, OR never say the wrong thing?",
                otherText:  "Would you rather: be understood completely by one person, OR partially understood by everyone?",
                Difficulty.Easy, "WouldYouRather"),

            PromptCard.CreateGenderDirected(
                title: "Weekend Preference",
                maleText:   "Would you rather: spend a weekend camping with no phone, OR a weekend in the city with no money?",
                femaleText: "Would you rather: a spontaneous trip with no plan, OR a perfectly planned holiday?",
                otherText:  "Would you rather: explore somewhere completely new alone, OR revisit your favourite place with friends?",
                Difficulty.Easy, "WouldYouRather"),

            PromptCard.CreateGenderDirected(
                title: "Superpower",
                maleText:   "Would you rather: be able to pause time, OR rewind time once per day?",
                femaleText: "Would you rather: know how every story ends, OR never know and always be surprised?",
                otherText:  "Would you rather: be invisible for a day, OR able to fly for a day?",
                Difficulty.Easy, "WouldYouRather"),

            PromptCard.CreateGenderDirected(
                title: "Famous Trade-off",
                maleText:   "Would you rather: be famous for something embarrassing, OR completely anonymous forever?",
                femaleText: "Would you rather: be famous for something trivial, OR respected for something nobody knows about?",
                otherText:  "Would you rather: be famous for 15 minutes, OR quietly influential for a lifetime?",
                Difficulty.Easy, "WouldYouRather"),

            // ── Medium ────────────────────────────────────────────────────────
            PromptCard.CreateGenderDirected(
                title: "Career Crossroads",
                maleText:   "Would you rather: be the highest-paid person at a company you hate, OR the lowest-paid at one you love?",
                femaleText: "Would you rather: work a job you find meaningful but that nobody respects, OR a prestigious job you find pointless?",
                otherText:  "Would you rather: build something that outlasts you with no recognition, OR be celebrated for something you didn't really build?",
                Difficulty.Medium, "WouldYouRather"),

            PromptCard.CreateGenderDirected(
                title: "Social Life",
                maleText:   "Would you rather: have a large group of acquaintances, OR two genuinely close friends?",
                femaleText: "Would you rather: always be the person who organises everything, OR always be the guest?",
                otherText:  "Would you rather: be at the centre of every social event, OR be the person everyone is glad turned up?",
                Difficulty.Medium, "WouldYouRather"),

            PromptCard.CreateGenderDirected(
                title: "Honesty Dilemma",
                maleText:   "Would you rather: always know when someone is lying to you, OR be able to lie without anyone ever knowing?",
                femaleText: "Would you rather: always know what people really think of you, OR remain blissfully unaware?",
                otherText:  "Would you rather: be incapable of lying, OR never be lied to?",
                Difficulty.Medium, "WouldYouRather"),

            PromptCard.CreateGenderDirected(
                title: "Time Machine",
                maleText:   "Would you rather: go back and fix one professional mistake, OR one personal one?",
                femaleText: "Would you rather: revisit your best year for one day, OR skip ahead five years to see where you end up?",
                otherText:  "Would you rather: have ten more minutes in a moment you lost, OR full certainty about one decision you still question?",
                Difficulty.Medium, "WouldYouRather"),

            // ── Medium — Adults ───────────────────────────────────────────────
            PromptCard.CreateGenderDirected(
                title: "Relationship Trade-off",
                maleText:   "Would you rather: be with someone who challenges you constantly, OR someone who supports you unconditionally?",
                femaleText: "Would you rather: feel deeply understood but taken for granted, OR constantly pursued but never quite known?",
                otherText:  "Would you rather: have a relationship that is easy, OR one that makes you a better person?",
                Difficulty.Medium, "WouldYouRather", restriction: adultsOnly),

            // ── Hard ──────────────────────────────────────────────────────────
            PromptCard.CreateGenderDirected(
                title: "Legacy",
                maleText:   "Would you rather: be remembered as great at your job, OR great at being present for the people around you?",
                femaleText: "Would you rather: be someone your children admire, OR someone your friends can't imagine life without?",
                otherText:  "Would you rather: leave a mark on the world that fades in a generation, OR on one person that lasts forever?",
                Difficulty.Hard, "WouldYouRather"),

            PromptCard.CreateGenderDirected(
                title: "The Hard One",
                maleText:   "Would you rather: know exactly when you will die, OR know exactly how?",
                femaleText: "Would you rather: live 20 fewer years but with no regrets, OR a full life knowing every regret in detail?",
                otherText:  "Would you rather: die knowing you played it safe, OR take the risk and never find out how it turned out?",
                Difficulty.Hard, "WouldYouRather"),

            PromptCard.CreateGenderDirected(
                title: "Couples Choice",
                maleText:   "Would you rather: always know what your partner is thinking, OR have them always know what you are thinking?",
                femaleText: "Would you rather: your partner remembers every fight perfectly, OR neither of you can remember them at all?",
                otherText:  "Would you rather: share every thought with your partner, OR keep a completely private inner world?",
                Difficulty.Hard, "WouldYouRather", restriction: couplesOnly),

            PromptCard.CreateGenderDirected(
                title: "Parent's Dilemma",
                maleText:   "Would you rather: your children grow up to be exactly like you, OR exactly like your ideal version of yourself?",
                femaleText: "Would you rather: know every struggle your child will face, OR be completely surprised alongside them?",
                otherText:  "Would you rather: give your children every advantage, OR let them earn everything themselves?",
                Difficulty.Hard, "WouldYouRather", restriction: parentsOnly),

            // ── Extreme ───────────────────────────────────────────────────────
            PromptCard.CreateGenderDirected(
                title: "No Going Back",
                maleText:   "Would you rather: give up all competitive instinct forever, OR never be able to admit you lost?",
                femaleText: "Would you rather: never be able to apologise, OR never need to?",
                otherText:  "Would you rather: always say the true thing, OR always say the kind thing — you can never do both?",
                Difficulty.Extreme, "WouldYouRather", restriction: adultsOnly),

            PromptCard.CreateGenderDirected(
                title: "Absolute Trade-off",
                maleText:   "Would you rather: sacrifice your ambition and gain permanent contentment, OR keep the ambition and the restlessness forever?",
                femaleText: "Would you rather: feel everything deeply but rarely be understood, OR feel less but always be perfectly understood?",
                otherText:  "Would you rather: be exactly who you are in all contexts always, OR shift slightly to fit each room you enter?",
                Difficulty.Extreme, "WouldYouRather", restriction: adultsOnly),
        ];
    }
}