using TableTop.Core.Abstractions.Analysis;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Analysis;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Love Languages — forty statements about how you actually receive affection,
/// and what happens when you hold your answers up against your partner's.
///
/// <para>
/// The second mode on the trait-analysis layer, and the one that shows why the
/// layer was worth building: it is the same scoring, the same controller and the
/// same Console renderer as Big Five, with nothing but a different
/// <see cref="TraitScale"/> and a different item bank. The whole mode is content.
/// </para>
///
/// <para>
/// <b>What matters here is the ranking, not the level.</b> Big Five asks "how
/// open are you"; this asks "which of these five lands hardest for you". Two
/// people can both score 70 on Physical Touch and the interesting fact is still
/// whether it is each of their *highest*. <see cref="TraitProfile.Strongest"/>
/// is what a results screen should lead with for this mode, and
/// <see cref="TraitProfileComparison.GreatestDivergence"/> is the conversation
/// the couple actually came for: the language one of you leans on hardest and
/// the other reads least.
/// </para>
///
/// <para>
/// <b>Likert, where the well-known version is forced-choice.</b> The popular
/// questionnaire makes you pick between two statements thirty times, which
/// produces ipsative scores — they are ranks, they sum to a constant, and
/// scoring high on one *necessarily* costs another. That is a poor fit here for
/// two reasons: it cannot express "all five matter to me a lot", which is a real
/// and common answer, and ipsative scores are famously unsafe to compare between
/// people, which is exactly what this mode does at the end. Agreeing
/// independently with each statement keeps the comparison honest and still
/// produces a clear ranking.
/// </para>
///
/// <para>
/// <b>Provenance.</b> The five categories are the widely-used popular ones and
/// are named descriptively. The statements are original to this repo — none is
/// taken from any published questionnaire, and this is not that assessment nor
/// affiliated with it. Same standard as <c>BigFiveItemBank</c>: content here is
/// compiled in and shipped, and "it was probably fine to copy" is not a licence.
/// </para>
/// </summary>
public sealed class LoveLanguagesMode : BaseGameModeDefinition, ITraitAssessmentProvider, ITableShapeMode
{
    /// <summary>Built for two, and works for a family reading each other.</summary>
    public TableShape SuitableFor => TableShape.Couple | TableShape.Family;

    /// <inheritdoc />
    public override string Name => "Love Languages";

    /// <inheritdoc />
    public override string Description =>
        "Forty statements about how you actually receive affection. No winner — you each get a ranking, and then you compare: what lands hardest for you, and what your partner has been reading least.";

    /// <summary>One person can profile themselves; the comparison is the point of a second.</summary>
    public override int MinimumPlayers => 1;

    /// <inheritdoc />
    public override string CompleteLabel => "Answer";

    /// <inheritdoc />
    public override string SkipLabel => "Skip This One";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [LoveLanguages.WordsKey] = "#EC407A",
            [LoveLanguages.ServiceKey] = "#26A69A",
            [LoveLanguages.GiftsKey] = "#FFA726",
            [LoveLanguages.TimeKey] = "#7E57C2",
            [LoveLanguages.TouchKey] = "#EF5350",
        };

    /// <inheritdoc />
    public TraitScale GetTraitScale() => LoveLanguages.Scale;

    /// <inheritdoc />
    public IReadOnlyList<TraitItemCard> GetItemBank() => LoveLanguagesItemBank.All;

    /// <inheritdoc />
    /// <remarks>The same items the assessment plays, so browsing and playing cannot disagree.</remarks>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        LoveLanguagesItemBank.All;

    /// <summary>Nobody wins Love Languages, so nothing scores. See <c>BigFiveMode</c> for why zero rather than one.</summary>
    protected override IScoringStrategy BuildScoring() => new FixedScoringStrategy(0);

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => LoveLanguagesItemBank.All;
}

/// <summary>
/// The five languages, their keys and the words shown to players.
/// </summary>
public static class LoveLanguages
{
    /// <summary>Key for the Words of Affirmation dimension.</summary>
    public const string WordsKey = "WordsOfAffirmation";
    /// <summary>Key for the Acts of Service dimension.</summary>
    public const string ServiceKey = "ActsOfService";
    /// <summary>Key for the Receiving Gifts dimension.</summary>
    public const string GiftsKey = "Gifts";
    /// <summary>Key for the Quality Time dimension.</summary>
    public const string TimeKey = "QualityTime";
    /// <summary>Key for the Physical Touch dimension.</summary>
    public const string TouchKey = "PhysicalTouch";

    /// <summary>
    /// The five languages as a <see cref="TraitScale"/>.
    ///
    /// <para>
    /// The low/high labels are phrased as "reads it less" / "reads it most",
    /// not "bad at" / "good at". Every dimension here is a way of receiving
    /// affection and none of them is the correct one — a low score is
    /// information for a partner, not a deficiency, and the words a results
    /// screen borrows from these labels are what make that land.
    /// </para>
    /// </summary>
    public static TraitScale Scale { get; } = new("Love Languages",
    [
        new TraitDefinition(WordsKey, "Words of Affirmation",
            "Reads words less", "Hears it most in words",
            "Being told, out loud and specifically, what someone values about you."),

        new TraitDefinition(ServiceKey, "Acts of Service",
            "Reads doing less", "Feels it most when it's done",
            "Someone lightening your load — the dreaded job quietly handled."),

        new TraitDefinition(GiftsKey, "Receiving Gifts",
            "Reads objects less", "Feels it most in the thought",
            "Being given something chosen because it made them think of you."),

        new TraitDefinition(TimeKey, "Quality Time",
            "Reads time less", "Feels it most in attention",
            "Unhurried, undistracted attention — the phones down and the door shut."),

        new TraitDefinition(TouchKey, "Physical Touch",
            "Reads touch less", "Feels it most in contact",
            "Everyday closeness — a hand on your back, sitting near, a proper hug."),
    ]);
}
