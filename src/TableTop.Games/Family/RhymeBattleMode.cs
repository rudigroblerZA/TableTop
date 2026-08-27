using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Rhyme Battle — rapid-fire rhyming competition.
///
/// How to play:
///   1. Read the starting word aloud.
///   2. Everyone has 5 seconds to shout out a word that rhymes.
///   3. Cannot repeat any word that's been said before in the round.
///   4. Last person still rhyming wins the point. Next round.
///
/// It's simple, frantic, and hilarious. "Orange" becomes everyone desperately
/// trying to rhyme (spoiler: you can't), "Brat" becomes 20 words flying at once,
/// "Syzygy" sends everyone home in shame.
///
/// Works for all ages. No vocabulary gatekeeping — slant rhymes count, made-up
/// words count, anything that sounds close enough. The real game is speed and
/// not caring that you sound ridiculous yelling "FLOOR-DOOR-SORE-LORE!" while
/// someone else is yelling "SCHMORE!"
/// </summary>
public sealed class RhymeBattleMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Rhyme Battle";
    /// <inheritdoc />
    public override string Description =>
        "Starting word is given. Shout rhymes. Can't repeat. Last one standing wins.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Survived";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Forfeit";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [RhymeBattleCardBank.EasyCategory] = "#66BB6A",
            [RhymeBattleCardBank.MediumCategory] = "#FFCA28",
            [RhymeBattleCardBank.HardCategory] = "#EF5350",
            [RhymeBattleCardBank.ImpossibleCategory] = "#AB47BC",
            [RhymeBattleCardBank.ChaosCategory] = "#EC407A",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        RhymeBattleCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => RhymeBattleCardBank.All;
}

/// <summary>Built-in card bank for Rhyme Battle. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class RhymeBattleCardBank
{
    internal const string EasyCategory = "Easy";
    internal const string MediumCategory = "Medium";
    internal const string HardCategory = "Hard";
    internal const string ImpossibleCategory = "Impossible";
    internal const string ChaosCategory = "Chaos";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── EASY ──────────────────────────────────────────────────────────────
        R(EasyCategory, "CAT", "(mat, hat, sat, bat, fat, rat, splat, combat...)", Difficulty.Easy),
        R(EasyCategory, "LIGHT", "(night, sight, bright, fight, might, flight...)", Difficulty.Easy),
        R(EasyCategory, "TREE", "(free, bee, sea, key, spree, decree...)", Difficulty.Easy),
        R(EasyCategory, "SONG", "(long, strong, wrong, along, belong...)", Difficulty.Easy),
        R(EasyCategory, "BLUE", "(true, shoe, flew, new, drew, crew...)", Difficulty.Easy),
        R(EasyCategory, "RING", "(sing, wing, thing, spring, sting, bring...)", Difficulty.Easy),

        // ── MEDIUM ────────────────────────────────────────────────────────────
        R(MediumCategory, "SILVER", "(quiver, shiver, deliver, river...)", Difficulty.Medium),
        R(MediumCategory, "ORANGE", "(no good rhymes exist; people suggest 'door-hinge'...)", Difficulty.Hard),
        R(MediumCategory, "HEART", "(part, start, smart, art, chart, dart...)", Difficulty.Medium),
        R(MediumCategory, "DANCE", "(chance, glance, prance, trance, romance...)", Difficulty.Medium),
        R(MediumCategory, "DRAGON", "(wagon, flagon... that's about it)", Difficulty.Medium),
        R(MediumCategory, "CIRCLE", "(purple rhymes loosely; mostly just pain)", Difficulty.Hard),

        // ── HARD ──────────────────────────────────────────────────────────────
        R(HardCategory, "MONTH", "(nope)", Difficulty.Hard),
        R(HardCategory, "PURPLE", "(circle sort of? nurple? this is brutal)", Difficulty.Hard),
        R(HardCategory, "PINT", "(hint, tint, mint, stint, squint...)", Difficulty.Hard),
        R(HardCategory, "ORANGE", "(still nothing)", Difficulty.Hard),
        R(HardCategory, "STRENGTH", "(length, if you cheat)", Difficulty.Hard),

        // ── IMPOSSIBLE ────────────────────────────────────────────────────────
        R(ImpossibleCategory, "SYZYGY", "(good luck)", Difficulty.Hard),
        R(ImpossibleCategory, "SIXTH", "(...seriously?)", Difficulty.Hard),
        R(ImpossibleCategory, "RHYTHM", "(absolutely not)", Difficulty.Hard),
        R(ImpossibleCategory, "WORCESTERSHIRE", "(this is psychological warfare)", Difficulty.Hard),

        // ── CHAOS ────────────────────────────────────────────────────────────
        R(ChaosCategory, "DOOR", "(four, floor, more, core, score, whore, store...)", Difficulty.Medium),
        R(ChaosCategory, "HAND", "(band, stand, land, grand, brand, strand...)", Difficulty.Easy),
        R(ChaosCategory, "POWER", "(flower, tower, hour, shower, sour...)", Difficulty.Medium),
        R(ChaosCategory, "MONEY", "(honey, sunny, funny, runny...)", Difficulty.Easy),
        R(ChaosCategory, "LOVE", "(dove, above, shove, glove, thereof...)", Difficulty.Medium),
    ];

    private static ICard R(string category, string word, string helpfulHint, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>5-SECOND RHYME BATTLE</b>\n\n" +
            "Starting word: <b>" + word + "</b>\n\n" +
            "SHOUT rhyming words. You have 5 seconds. Cannot repeat words already said.\n\n" +
            "Last person to say a valid rhyme wins the round. Everyone else is out.\n\n" +
            helpfulHint,
            d, category);
}
