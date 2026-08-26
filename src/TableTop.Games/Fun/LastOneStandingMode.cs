using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Fun;

/// <summary>
/// Last One Standing — everyone attempts the same card, and whoever fails is out.
///
/// The catalogue's first elimination format. Every other mode accumulates points,
/// which means a player who falls behind early spends the rest of the game
/// knowing they've lost. Elimination has the opposite shape: it's tense the whole
/// way, and when you're out you become an audience with opinions, which is a
/// better job than trailing by nine points.
///
/// How to play:
///   1. Everyone plays every card. The card says what counts as failing.
///   2. Anyone who fails is out. Out players adjudicate — that's a real job, and
///      the reason elimination doesn't leave anyone bored.
///   3. Down to two, skip to the Final cards. Down to one, that's your winner.
///   4. If a card would eliminate everybody at once, nobody goes out. Somebody
///      has to survive.
///
/// ON NOT LEAVING PEOPLE OUT FOR AN HOUR
/// ─────────────────────────────────────
/// Elimination's known failure is the person knocked out in round two watching
/// forty minutes of somebody else's game. Two things here work against that: out
/// players are the judges, with the standing right to overrule, and the Revival
/// cards let the eliminated back in. A deck this size runs about fifteen minutes,
/// which is the other half of the answer — it's designed to be replayed, not
/// endured.
/// </summary>
public sealed class LastOneStandingMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <inheritdoc />
    public override string Name        => "Last One Standing";
    /// <inheritdoc />
    public override string Description =>
        "Everyone attempts every card. Fail and you're out — and you become a judge. Last player left wins, and it takes about fifteen minutes.";

    /// <summary>Three is the floor: with two, the first elimination ends it.</summary>
    public override int MinimumPlayers => 3;

    /// <summary>A group game. A couple would run out of players immediately.</summary>
    public TableShape SuitableFor => TableShape.Family | TableShape.Team | TableShape.Group;

    /// <summary>Records surviving the card.</summary>
    public override string CompleteLabel => "Survived";
    /// <summary>Records being knocked out.</summary>
    public override string SkipLabel     => "Out";

    /// <summary>Opens on the rules, because elimination needs its terms agreed first.</summary>
    public override IReadOnlyList<string> CategoriesPinnedToStart => ["Rules"];

    /// <summary>The two-player endgame comes last.</summary>
    public override IReadOnlyList<string> CategoriesPinnedToEnd => ["Final"];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Rules"]    = "#5AC8B0",
            ["Round"]    = "#42A5F5",
            ["Pressure"] = "#FFA726",
            ["Revival"]  = "#66BB6A",
            ["Final"]    = "#B71C4A",
        };

    /// <summary>
    /// Surviving a harder card counts for more, which only matters as a tiebreak
    /// — the winner is whoever is left, not whoever scored most.
    ///
    /// This was <c>new StreakScoringStrategy()</c>, which does not compile:
    /// <c>StreakScoringStrategy</c> is a decorator and has no parameterless
    /// constructor. Wrapping this strategy in it would compile, but would not do
    /// anything — the multiplier fires on a <c>streak:{n}</c> player tag and
    /// nothing anywhere in the codebase ever writes one, so it can only ever
    /// return the base score. Plain difficulty scoring is the honest version of
    /// what that line was reaching for.
    /// </summary>
    protected override IScoringStrategy BuildScoring() => new DifficultyBasedScoringStrategy();

    /// <inheritdoc />
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        LastOneStandingCardBank.All;

    /// <summary>Returns the built-in card bank.</summary>
    public static IReadOnlyList<ICard> GetCards() => LastOneStandingCardBank.All;
}

/// <summary>Built-in card bank for Last One Standing.</summary>
public static class LastOneStandingCardBank
{
    /// <summary>All cards, in authored order.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static ICard C(string category, string title, string body, Difficulty difficulty) =>
        StandardCard.Create(title, $"<b>{Emoji(category)} {category.ToUpperInvariant()}</b>\n\n{body}",
            difficulty, category);

    private static string Emoji(string category) => category switch
    {
        "Rules"    => "📋",
        "Round"    => "🎯",
        "Pressure" => "🔥",
        "Revival"  => "🌱",
        "Final"    => "🏆",
        _          => "🎲",
    };

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── RULES ─────────────────────────────────────────────────────────────
        C("Rules", "How You Go Out",
          "Every card names what counts as failing. Fail it and you're out — but read the next two cards " +
          "before anyone does, because being out is not the same as being finished.", Difficulty.Easy),
        C("Rules", "The Out Players Judge",
          "Anyone eliminated becomes a judge, and the judges have the final word on every borderline call for " +
          "the rest of the game. This is a real job and it is more fun than it sounds.", Difficulty.Easy),
        C("Rules", "Nobody Goes Out Alone",
          "Two standing rules. If a card would eliminate everyone at once, nobody goes out — somebody has to " +
          "survive. And if a card asks something a player physically can't do, the judges substitute a fair " +
          "equivalent rather than eliminating them for it.", Difficulty.Easy),

        // ── ROUND — the standard eliminators ──────────────────────────────────
        C("Round", "Name One, Quickly",
          "A category is named. Round the table, one item each, no repeats, three seconds each. First to " +
          "hesitate or repeat is out.", Difficulty.Easy),
        C("Round", "Don't Say It",
          "The judges pick a common word. Everyone converses normally for two minutes. Say the word and you're " +
          "out. The judges may ask you leading questions and absolutely will.", Difficulty.Medium),
        C("Round", "Keep A Straight Face",
          "Everyone must hold a completely straight face for sixty seconds. The judges may do anything except " +
          "touch you or leave their seats. First to break is out.", Difficulty.Medium),
        C("Round", "Finish The Sentence",
          "A judge starts a sentence. Round the table, each player must finish it differently and plausibly. " +
          "Repeat an idea or stall and you're out.", Difficulty.Medium),
        C("Round", "One Hand Only",
          "Until the next Round card, everyone plays with one hand behind their back. Use the wrong hand for " +
          "anything at all and you're out. The judges are watching for exactly this.", Difficulty.Hard),
        C("Round", "The Rhyme",
          "A judge names a word. Round the table, each player rhymes it. No repeats, no non-words, no " +
          "hesitating. Last to survive the loop stays; the one who breaks is out.", Difficulty.Hard),

        // ── PRESSURE — harder, and they eliminate faster ───────────────────────
        C("Pressure", "Twenty Seconds Each",
          "A category, and twenty seconds per player to name five things in it. Fall short and you're out. The " +
          "judges count out loud, unhelpfully.", Difficulty.Hard),
        C("Pressure", "Two Things At Once",
          "Each player must recite the alphabet backwards from J while clapping a steady beat. Lose the beat or " +
          "the alphabet and you're out.", Difficulty.Hard),
        C("Pressure", "Answer In Questions",
          "The judges interrogate each player in turn for thirty seconds. Every answer must itself be a " +
          "question. Give a straight answer and you're out.", Difficulty.Extreme),
        C("Pressure", "Nobody Blinks",
          "Pair up — judges assign the pairs. Hold eye contact. First of each pair to blink, laugh or look away " +
          "is out. Odd number left over survives automatically.", Difficulty.Medium),
        C("Pressure", "The Impossible Category",
          "The judges name a category so narrow they doubt anyone can fill it. Everyone must produce one valid " +
          "item. Anyone who can't is out — but if nobody can, the judges lose the card and everyone survives.",
          Difficulty.Extreme),

        // ── REVIVAL — the eliminated get back in ──────────────────────────────
        C("Revival", "One Way Back",
          "All eliminated players compete on a single challenge of the judges' choosing — and the judges here " +
          "are the players still standing. Whoever wins it rejoins the game.", Difficulty.Medium),
        C("Revival", "Bought Back",
          "Any player still standing may bring one eliminated player back in. It costs them nothing and helps " +
          "them not at all, which is exactly why it's interesting.", Difficulty.Easy),
        C("Revival", "Everybody Back",
          "Everyone eliminated rejoins immediately. Yes, everyone. The game is probably about to get much " +
          "louder and this card knows what it's doing.", Difficulty.Easy),

        // ── FINAL — for the last two ───────────────────────────────────────────
        C("Final", "Head To Head",
          "Down to two. A category, alternating answers, no repeats, no hesitation. First to falter loses. The " +
          "judges — everyone else — call it, and their call is final.", Difficulty.Extreme),
        C("Final", "Three Rounds, Best Of",
          "Down to two. Best of three: one round of naming, one of acting, one of holding a straight face. The " +
          "judges pick who goes first in each.", Difficulty.Extreme),
        C("Final", "The Last Question",
          "Down to two. The judges agree one question that both finalists answer. The better answer wins the " +
          "game, decided by open vote among everyone who is out. That's the deck.", Difficulty.Extreme),
    ];
}
