using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Alibi — a very silly crime has been committed, and two of you are suspects.
///
/// How to play:
///   1. Draw a card: it names the crime. The active player plus the player to
///      their left are the SUSPECTS; everyone else is the tribunal.
///   2. Suspects get 30 seconds together to agree an alibi — where they were,
///      what they were doing, why they were together.
///   3. The tribunal then questions them ONE AT A TIME (other suspect covers
///      their ears): three questions each. Stories must match.
///   4. Verdict by vote. Consistent alibi = suspects score. Contradictions =
///      tribunal scores the satisfaction of justice (and the point goes to
///      the sharpest questioner, tribunal's choice).
///
/// The engine of the game: inventing details together is easy — REMEMBERING
/// the same details separately is the hard part, and where the comedy lives.
/// </summary>
public sealed class AlibiMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Alibi";
    /// <inheritdoc />
    public override string Description =>
        "A silly crime, two suspects, one hastily agreed alibi — questioned separately. Stories must match.";

    /// <summary>Label for the button that records an acquittal.</summary>
    public override string CompleteLabel => "Acquitted";
    /// <summary>Label for the button that records a conviction.</summary>
    public override string SkipLabel => "GUILTY";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [AlibiCardBank.DomesticCategory] = "#FFA726",
            [AlibiCardBank.WorkplaceCategory] = "#42A5F5",
            [AlibiCardBank.NeighbourhoodCategory] = "#66BB6A",
            [AlibiCardBank.HistoricCategory] = "#AB47BC",
            [AlibiCardBank.WithATwistCategory] = "#EF5350",
        };

    /// <summary>One point per acquittal (the suspects earned it).</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Returns the built-in alibi card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        AlibiCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => AlibiCardBank.All;
}

/// <summary>Built-in card bank for Alibi.</summary>
public static class AlibiCardBank
{
    internal const string DomesticCategory = "Domestic";
    internal const string WorkplaceCategory = "Workplace";
    internal const string NeighbourhoodCategory = "Neighbourhood";
    internal const string HistoricCategory = "Historic";
    internal const string WithATwistCategory = "With a Twist";

    /// <summary>All alibi cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── DOMESTIC ─────────────────────────────────────────────────────────
        A(DomesticCategory, "Every left sock in the house has vanished. Only the lefts.", Difficulty.Easy),
        A(DomesticCategory, "Someone ate the dessert that was CLEARLY labelled with someone else's name.", Difficulty.Easy),
        A(DomesticCategory, "The good scissors are missing. The GOOD ones.", Difficulty.Easy),
        A(DomesticCategory, "The thermostat was changed. Nobody admits to being cold.", Difficulty.Medium),
        A(DomesticCategory, "The end of the toilet roll was left, un-replaced, like a crime scene.", Difficulty.Easy),
        A(DomesticCategory, "Someone watched the next episode of the shared show. ALONE.", Difficulty.Medium),
        A(DomesticCategory, "The houseplant has been overwatered into the afterlife.", Difficulty.Medium),

        // ── WORKPLACE ────────────────────────────────────────────────────────
        A(WorkplaceCategory, "The office stapler has been jammed with what appears to be malice.", Difficulty.Easy),
        A(WorkplaceCategory, "Someone microwaved fish in the shared kitchen at 11:58 a.m.", Difficulty.Medium),
        A(WorkplaceCategory, "The meeting that could have been an email was scheduled anyway. For 4:30 p.m. Friday.", Difficulty.Medium),
        A(WorkplaceCategory, "Someone replied-all. Twice. To apologise for replying-all.", Difficulty.Hard),
        A(WorkplaceCategory, "The last of the good coffee was taken and the pot left ON, brewing sadness.", Difficulty.Easy),
        A(WorkplaceCategory, "The office birthday card was signed 'so sorry for your loss'.", Difficulty.Hard),

        // ── NEIGHBOURHOOD ────────────────────────────────────────────────────
        A(NeighbourhoodCategory, "Somebody's garden gnome has been rearranged into a dramatic tableau.", Difficulty.Easy),
        A(NeighbourhoodCategory, "The neighbourhood group chat was set to 'admin approval required'. Power has shifted.", Difficulty.Medium),
        A(NeighbourhoodCategory, "A wheelie bin was returned to the wrong house. Possibly on purpose. Possibly a message.", Difficulty.Medium),
        A(NeighbourhoodCategory, "The community noticeboard now advertises 'FREE LLAMA (reason: ask Dave)'. There is no Dave.", Difficulty.Hard),
        A(NeighbourhoodCategory, "Someone has been aggressively winning the unspoken Christmas-lights competition since June.", Difficulty.Medium),

        // ── HISTORIC ─────────────────────────────────────────────────────────
        A(HistoricCategory, "It's 1503 and someone has drawn a moustache on the Mona Lisa. It's an improvement, but still.", Difficulty.Medium),
        A(HistoricCategory, "It's 1969 and one of the moon rocks is missing from the return capsule. Weigh-in was THIS morning.", Difficulty.Hard),
        A(HistoricCategory, "It's ancient Egypt and the pyramid's top block is on backwards. The pharaoh is asking questions.", Difficulty.Medium),
        A(HistoricCategory, "It's 1912 and someone told the Titanic's lookout 'take the night off, what could happen'.", Difficulty.Extreme),
        A(HistoricCategory, "It's the Stone Age and the wheel prototype has been returned with a flat.", Difficulty.Hard),

        // ── WITH A TWIST — the alibi has a handicap ──────────────────────────
        W(WithATwistCategory, "The library's SILENCE sign has been stolen.",
          "Your entire alibi must be delivered in whispers.", Difficulty.Medium),
        W(WithATwistCategory, "Someone released 400 rubber ducks into the town fountain.",
          "Your alibi must involve a boat, and neither of you may say why.", Difficulty.Hard),
        W(WithATwistCategory, "The bakery's award-winning sourdough starter, 'Clint', is missing.",
          "One suspect must claim they were baking at the time. The tribunal may demand technique details.", Difficulty.Hard),
        W(WithATwistCategory, "Every clock in the building is now seven minutes fast.",
          "Any time mentioned in your alibi must be suspiciously precise, to the minute.", Difficulty.Medium),
        W(WithATwistCategory, "The karaoke machine's 'skip song' button has been superglued.",
          "Your alibi must include the title of a real song, and the tribunal may make you perform eight seconds of it.", Difficulty.Extreme),
        W(WithATwistCategory, "A flock of flamingos has appeared on the mayor's lawn overnight.",
          "You may not use the words 'bird', 'pink', or 'lawn' during questioning.", Difficulty.Extreme),
    ];

    private static ICard A(string category, string crime, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>🚨 THE CRIME:</b> " + crime + "\n\n" +
            "The active player + the player to their left are the SUSPECTS. " +
            "30 seconds to agree your alibi — then you're questioned SEPARATELY " +
            "(three questions each, partner's ears covered). Stories must match.\n\n" +
            "<i>Tribunal votes: Acquitted (suspects score) or GUILTY (best questioner scores).</i>",
            d, category);

    private static ICard W(string category, string crime, string twist, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>🚨 THE CRIME:</b> " + crime + "\n\n" +
            "<b>⚖️ SPECIAL CONDITION:</b> " + twist + "\n\n" +
            "The SUSPECTS (active player + left neighbour): 30 seconds to agree the alibi, then you are questioned SEPARATELY, " +
            "three questions each. The condition applies at ALL times.\n\n" +
            "<i>Tribunal votes: Acquitted or GUILTY.</i>",
            d, category);
}
