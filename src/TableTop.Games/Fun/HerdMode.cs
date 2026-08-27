using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Fun;

/// <summary>
/// Herd — everyone answers the same question at the same time, and you score
/// for <b>agreeing</b>, not for being right.
///
/// <para>
/// The first mode in the catalogue with no active player. Every other mode
/// puts one person on the spot while the rest watch; here all of you write an
/// answer at once, reveal together, and the scoreboard is decided by what the
/// group happened to have in common.
/// </para>
///
/// <para>
/// <b>Two ways to score, deliberately unequal.</b> Match the biggest group and
/// everyone in it takes 3. Be the <i>only</i> person to say your answer and
/// you take 2. The lone-voice points exist because without them the game has
/// one dominant strategy — always name the most obvious thing — which solves
/// it after two rounds and punishes exactly the players who think of something
/// better. Being worth less than the herd keeps it a gamble rather than a
/// superior line, so both choices stay live on every card.
/// </para>
///
/// <para>
/// A round where everyone says something different scores nothing at all: no
/// herd, and nobody uniquely alone. That's a real outcome rather than an edge
/// case — the table just proved the question was too open, and the scoreboard
/// says so.
/// </para>
///
/// <para>
/// Needs at least three players to be worth playing. With two, "matching the
/// herd" and "agreeing with the other person" are the same thing, and the
/// mechanic has nothing left in it. That's declared via
/// <see cref="MinimumPlayers"/> and enforced by the setup screen —
/// <c>HerdController</c> deliberately does <b>not</b> throw on a short roster,
/// matching every other mode: a two-player session simply scores nothing every
/// round, which tells the table what it needs to know without crashing a
/// resumed session whose roster shrank.
/// </para>
/// </summary>
public sealed class HerdMode : BaseGameModeDefinition, IHerdDeckProvider, ITableShapeMode
{
    /// <summary>Friends, a party, any group of adults who chose to be there.</summary>
    public TableShape SuitableFor => TableShape.Group | TableShape.Family;

    /// <inheritdoc />
    public override string Name => "Herd";

    /// <inheritdoc />
    public override string Description =>
        "Everyone answers at once. Match the group and you all score — or be the only one who says yours and score anyway. Saying something nobody else did is worth less than agreeing, but not nothing.";

    /// <summary>Three: with two players, matching the herd and simply agreeing are identical.</summary>
    public override int MinimumPlayers => 3;

    /// <inheritdoc />
    public override string CompleteLabel => "Reveal";

    /// <inheritdoc />
    public override string SkipLabel => "Pass This One";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [HerdCardBank.HowToPlayCategory] = "#26A69A",
            [HerdCardBank.ObviousCategory] = "#42A5F5",
            [HerdCardBank.AwkwardCategory] = "#FFA726",
            [HerdCardBank.ThisTableCategory] = "#AB47BC",
            [HerdCardBank.SplitCategory] = "#EF5350",
        };

    /// <summary>The rules card explains simultaneous answering, which the mode depends on.</summary>
    public override IReadOnlyList<string> CategoriesPinnedToStart => [HerdCardBank.HowToPlayCategory];

    /// <inheritdoc />
    protected override IScoringStrategy BuildScoring() => new FixedScoringStrategy(1);

    /// <inheritdoc />
    /// <remarks>
    /// Excludes the HerdCardBank.HowToPlayCategory card. Every other mode's rules card is a
    /// normal card the players turn past, but here the deck feeds
    /// <c>HerdController</c> directly as prompts — so leaving it in would make
    /// round 1 ask the table to simultaneously answer a page of instructions.
    /// Caught by playing a real session rather than by reading the code.
    /// The card stays in <see cref="GetCards"/> so the rules still render
    /// wherever a mode's deck is browsed.
    /// </remarks>
    public IReadOnlyList<ICard> GetHerdDeck() =>
        GetCards([]).Where(c => c.Category != HerdCardBank.HowToPlayCategory).ToList();

    /// <inheritdoc />
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        HerdCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => HerdCardBank.All;
}

/// <summary>
/// Built-in card bank for Herd.
///
/// <para>
/// Prompts are tuned so there's a plausible obvious answer <i>and</i> real room
/// to go your own way — a question with only one sane answer makes the whole
/// table score every time and removes the decision, and one with a thousand
/// answers means nobody ever matches. Each category leans a different way on
/// purpose: Obvious has a strong favourite, Split deliberately doesn't.
/// </para>
/// </summary>
public static class HerdCardBank
{
    internal const string HowToPlayCategory = "How To Play";
    internal const string ObviousCategory = "Obvious";
    internal const string AwkwardCategory = "Awkward";
    internal const string ThisTableCategory = "This Table";
    internal const string SplitCategory = "Split";

    private const string Deck = "Herd";

    /// <summary>All cards, in intended play order.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static ICard C(string category, string title, string prompt, Difficulty difficulty = Difficulty.Easy)
    {
        var seed = $"{Deck}|{category}|{title}|{prompt}";
        var digest = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(seed));
        return new StandardCard(new Guid(digest[..16]), title, prompt, difficulty, category);
    }

    private static IReadOnlyList<ICard> Build() =>
    [
        C(HowToPlayCategory, "How Herd Works",
            "Everyone answers <b>at the same time</b>. Write it down, or type it and turn the screen over — " +
            "no saying it out loud first, because the moment one person speaks everyone else drifts toward them.\n\n" +
            "Reveal together.\n\n" +
            "<b>Matched the biggest group?</b> Everyone in it scores 3.\n" +
            "<b>Only person who said yours?</b> You score 2.\n" +
            "<b>Everyone said something different?</b> Nobody scores. The question was too open — move on.\n\n" +
            "So you can play safe and go with the room, or back yourself to be the only one. " +
            "The safe move is worth more, which is exactly why the other one is interesting."),

        // ── OBVIOUS — a strong favourite exists; matching should be easy ─────
        C(ObviousCategory, "Cereal",        "Name a breakfast cereal."),
        C(ObviousCategory, "Primary Colour","Name a primary colour."),
        C(ObviousCategory, "Card Game",     "Name a card game."),
        C(ObviousCategory, "Big Cat",       "Name a big cat."),
        C(ObviousCategory, "Pizza Topping", "Name a pizza topping."),
        C(ObviousCategory, "Planet",        "Name a planet."),
        C(ObviousCategory, "Board Game",    "Name a board game."),

        // ── AWKWARD — obvious answer exists, but saying it costs something ───
        C(AwkwardCategory, "Bad Habit",      "Name a bad habit most people here probably have.", Difficulty.Medium),
        C(AwkwardCategory, "Overrated",      "Name something widely loved that you find overrated.", Difficulty.Medium),
        C(AwkwardCategory, "Lied About",     "Name something almost everyone has lied about at least once.", Difficulty.Medium),
        C(AwkwardCategory, "Never Finished", "Name something loads of people start and never finish.", Difficulty.Medium),
        C(AwkwardCategory, "Too Old For",    "Name something people claim to be too old for and still do.", Difficulty.Medium),

        // ── THIS TABLE — about the people actually in the room ──────────────
        C(ThisTableCategory, "Most Likely To Be Late",   "Who here is most likely to be late? Write a name.", Difficulty.Medium),
        C(ThisTableCategory, "Best Cook",                "Who here is the best cook? Write a name.", Difficulty.Medium),
        C(ThisTableCategory, "Would Survive Longest",    "Who here would survive longest with no phone? Write a name.", Difficulty.Medium),
        C(ThisTableCategory, "Tells The Best Stories",   "Who here tells the best stories? Write a name.", Difficulty.Medium),
        C(ThisTableCategory, "Hardest To Buy For",       "Who here is hardest to buy a present for? Write a name.", Difficulty.Medium),

        // ── SPLIT — deliberately no favourite; matching here is genuine luck ─
        C(SplitCategory, "A Number",        "Write down a number between 1 and 20.", Difficulty.Hard),
        C(SplitCategory, "Any Animal",      "Name any animal at all.", Difficulty.Hard),
        C(SplitCategory, "A Word",          "Write down any word you like.", Difficulty.Hard),
        C(SplitCategory, "Somewhere To Go", "Name anywhere in the world you'd go tomorrow.", Difficulty.Hard),
        C(SplitCategory, "A Year",          "Write down any year.", Difficulty.Hard),
    ];
}
