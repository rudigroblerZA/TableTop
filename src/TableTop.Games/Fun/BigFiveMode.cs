using TableTop.Core.Abstractions.Analysis;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Analysis;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Fun;

/// <summary>
/// Big Five — fifty statements, five dimensions, and a profile per player
/// instead of a winner.
///
/// <para>
/// The first mode in the catalogue that does not produce a score anyone can
/// win. Everyone answers the same statements on a five-point agree/disagree
/// scale, and the result is a shape rather than a number: where each player
/// landed on Openness, Conscientiousness, Extraversion, Agreeableness and
/// Sensitivity, plus — the part a couples game is actually for — how two
/// profiles read against each other.
/// </para>
///
/// <para>
/// <b>What this is not.</b> It is not a psychometric instrument and the results
/// are not percentiles. A real Big Five inventory reports where you sit against
/// a normed population sample; this reports where your answers sat on the range
/// these fifty items could produce. That distinction is written into
/// <see cref="TraitScore.Normalized"/> and <see cref="TraitBand"/> rather than
/// left to a disclaimer nobody reads, because "you scored 82 on Openness" reads
/// as a percentile to almost everyone who sees it.
/// </para>
///
/// <para>
/// <b>The item bank is balanced five-and-five per dimension, and that is a
/// correctness property rather than a stylistic one.</b> Each trait gets five
/// forward-keyed and five reverse-keyed items, so a player who agrees with
/// every single statement scores exactly the midpoint on every dimension
/// instead of maximum on all five. Acquiescence bias — the tendency to agree
/// with whatever you are shown — is the easiest way to make a personality
/// result meaningless, and an all-positive bank measures nothing but how
/// agreeable the player is feeling toward the quiz.
/// <c>BigFiveItemBank.IsBalanced</c> exposes this so a test can assert it
/// rather than trusting that someone counted correctly.
/// </para>
///
/// <para>
/// <b>On the fifth dimension's name.</b> Its key is <c>Neuroticism</c>, because
/// that is what the dimension is called and a key that says otherwise would
/// make the mode's output unidentifiable to anyone who knows the model. It is
/// <i>displayed</i> as "Sensitivity". Telling a player at a party that they
/// scored high on neuroticism is a clinical-sounding verdict on a night out;
/// the dimension it names is real and worth reporting, the word is not worth
/// the damage.
/// </para>
/// </summary>
public sealed class BigFiveMode : BaseGameModeDefinition, ITraitAssessmentProvider, ITableShapeMode
{
    /// <summary>Works for one person, a couple, or a table taking turns.</summary>
    public TableShape SuitableFor => TableShape.Couple | TableShape.Group | TableShape.Family;

    /// <inheritdoc />
    public override string Name => "Big Five";

    /// <inheritdoc />
    public override string Description =>
        "Fifty statements, five traits, no winner. Answer how much each one sounds like you — then see the shape you make, and how it compares to everyone else's.";

    /// <summary>One player can profile themselves; the comparison needs two.</summary>
    public override int MinimumPlayers => 1;

    /// <inheritdoc />
    public override string CompleteLabel => "Answer";

    /// <inheritdoc />
    public override string SkipLabel => "Skip This One";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [BigFiveTraits.OpennessKey] = "#7E57C2",
            [BigFiveTraits.ConscientiousnessKey] = "#26A69A",
            [BigFiveTraits.ExtraversionKey] = "#FFA726",
            [BigFiveTraits.AgreeablenessKey] = "#66BB6A",
            [BigFiveTraits.NeuroticismKey] = "#42A5F5",
        };

    /// <inheritdoc />
    public TraitScale GetTraitScale() => BigFiveTraits.Scale;

    /// <inheritdoc />
    public IReadOnlyList<TraitItemCard> GetItemBank() => BigFiveItemBank.All;

    /// <inheritdoc />
    /// <remarks>
    /// The same items the assessment plays, so browsing the mode's deck and
    /// playing it cannot disagree — the property backlog item 10 fixed for Herd,
    /// where the manifest counted a deck the controller never dealt.
    /// </remarks>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        BigFiveItemBank.All;

    /// <summary>
    /// Nobody wins Big Five, so nothing scores. Zero rather than one: a mode
    /// with no winner should not quietly populate a scoreboard that a head might
    /// then render as a result.
    /// </summary>
    protected override IScoringStrategy BuildScoring() => new FixedScoringStrategy(0);

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => BigFiveItemBank.All;
}

/// <summary>
/// The five dimensions, their keys and the words shown to players.
/// </summary>
public static class BigFiveTraits
{
    /// <summary>Key for the Openness dimension.</summary>
    public const string OpennessKey = "Openness";
    /// <summary>Key for the Conscientiousness dimension.</summary>
    public const string ConscientiousnessKey = "Conscientiousness";
    /// <summary>Key for the Extraversion dimension.</summary>
    public const string ExtraversionKey = "Extraversion";
    /// <summary>Key for the Agreeableness dimension.</summary>
    public const string AgreeablenessKey = "Agreeableness";
    /// <summary>Key for the Neuroticism dimension, displayed as "Sensitivity".</summary>
    public const string NeuroticismKey = "Neuroticism";

    /// <summary>The Big Five as a <see cref="TraitScale"/>, in conventional OCEAN order.</summary>
    public static TraitScale Scale { get; } = new("Big Five",
    [
        new TraitDefinition(OpennessKey, "Openness",
            "Prefers the familiar", "Chases the new",
            "How much you're pulled toward novelty, ideas and art versus what you already know works."),

        new TraitDefinition(ConscientiousnessKey, "Conscientiousness",
            "Plays it by ear", "Plans it out",
            "How much you organise, follow through and keep track, versus deciding as you go."),

        new TraitDefinition(ExtraversionKey, "Extraversion",
            "Recharges alone", "Recharges with people",
            "Where your energy comes from, and how readily you take up space in a room."),

        new TraitDefinition(AgreeablenessKey, "Agreeableness",
            "Holds their line", "Keeps the peace",
            "How far you'll bend to keep things warm, and how readily you assume the best of people."),

        // Displayed as "Sensitivity" — see the note on BigFiveMode for why the
        // key and the label deliberately differ.
        new TraitDefinition(NeuroticismKey, "Sensitivity",
            "Hard to rattle", "Feels things sharply",
            "How strongly things land and how long they stay — the Big Five's Neuroticism dimension, under a kinder name."),
    ]);
}
