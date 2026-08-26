using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Fun;

/// <summary>
/// All Together Now — the table against the deck.
///
/// The first co-operative mode in the catalogue. Every other mode either scores
/// players individually or passes a turn round the table; here there is one
/// shared score and either everyone clears the target or nobody does. That
/// changes the social shape of the evening rather than just the content: nobody
/// is knocked out, nobody is behind, and the quiet player at the end of the sofa
/// is a resource rather than a rival.
///
/// How to play:
///   1. Agree a target before you start. Twelve cleared cards is a good first
///      game; fifteen is hard; eighteen is a boast.
///   2. Draw a card. Everybody attempts it together — the card says how.
///   3. Cleared it? That's one toward the target. Failed? Draw the next one;
///      you have as many cards as the deck holds and no more.
///   4. Run out of deck before the target and the deck wins. It is allowed to
///      win. That is what makes clearing it mean something.
///
/// SHARED SCORE IS A TABLE CONVENTION, NOT AN ENGINE FEATURE
/// ────────────────────────────────────────────────────────
/// The engine scores per player, and there is no concept of a shared or team
/// total. Rather than bolt one on for a single mode, this follows the convention
/// already used for timed play — 60 Seconds bakes its sixty-second window into
/// the card text rather than the engine, as do Alibi and One-Star Reviews. So
/// the shared tally lives on the card and in the rules above, and the per-player
/// score the engine keeps is simply ignored. If co-op grows past one mode, a
/// real shared-score abstraction is the right answer; one mode does not justify
/// it.
/// </summary>
public sealed class AllTogetherNowMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <inheritdoc />
    public override string Name => "All Together Now";
    /// <inheritdoc />
    public override string Description =>
        "The table against the deck. One shared score, no winners and no losers — either you all clear the target or the deck takes it.";

    /// <summary>Works for any group; a pair can play it but it wants a crowd.</summary>
    public override int MinimumPlayers => 2;

    /// <summary>Co-op is a group activity; a couple's night is not the audience.</summary>
    public TableShape SuitableFor => TableShape.Family | TableShape.Team | TableShape.Group;

    /// <summary>Records a card the table cleared together.</summary>
    public override string CompleteLabel => "Cleared It";
    /// <summary>Records a card that beat the table.</summary>
    public override string SkipLabel => "Beat Us";

    /// <summary>Opens on the shared brief so the target gets agreed out loud.</summary>
    public override IReadOnlyList<string> CategoriesPinnedToStart => ["Brief"];

    /// <summary>Closes on the debrief, whichever way it went.</summary>
    public override IReadOnlyList<string> CategoriesPinnedToEnd => ["Debrief"];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Brief"] = "#5AC8B0",
            ["Everyone In"] = "#66BB6A",
            ["Relay"] = "#42A5F5",
            ["In Silence"] = "#7E57C2",
            ["Against It"] = "#EF5350",
            ["Debrief"] = "#A78BD0",
        };

    /// <summary>Harder cards are worth more, for tables that want a points target.</summary>
    protected override IScoringStrategy BuildScoring() => new DifficultyBasedScoringStrategy();

    /// <inheritdoc />
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        AllTogetherNowCardBank.All;

    /// <summary>Returns the built-in card bank.</summary>
    public static IReadOnlyList<ICard> GetCards() => AllTogetherNowCardBank.All;
}

/// <summary>Built-in card bank for All Together Now.</summary>
public static class AllTogetherNowCardBank
{
    /// <summary>All cards, in authored order.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static ICard C(string category, string title, string body, Difficulty difficulty) =>
        StandardCard.Create(title, $"<b>{Emoji(category)} {category.ToUpperInvariant()}</b>\n\n{body}",
            difficulty, category);

    private static string Emoji(string category) => category switch
    {
        "Brief" => "📋",
        "Everyone In" => "🙌",
        "Relay" => "🔗",
        "In Silence" => "🤫",
        "Against It" => "🔥",
        "Debrief" => "💜",
        _ => "🎲",
    };

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── BRIEF — agree the terms out loud before anything starts ───────────
        C("Brief", "Pick Your Number",
          "Agree your target now, out loud, before anyone sees a card. Twelve cleared is a good first game. " +
          "Fifteen is hard. Eighteen and you get to tell people about it.", Difficulty.Easy),
        C("Brief", "Nobody Sits Out",
          "One rule for the whole game: every card needs everyone. If someone can't do the thing a card asks — " +
          "can't stand, can't see it, can't hear it — the table adapts the card. Adapting is not cheating; " +
          "leaving someone out is losing.", Difficulty.Easy),
        C("Brief", "The Deck Can Win",
          "Say this part out loud: the deck is allowed to beat you. If it couldn't, clearing it wouldn't mean " +
          "anything. Failed cards get set aside, not re-tried.", Difficulty.Easy),

        // ── EVERYONE IN — the whole table does the same thing at once ─────────
        C("Everyone In", "Same Word",
          "On three, everyone says one word that means 'happy'. If any two of you say the same word, the card " +
          "is cleared. No conferring, no lip-reading.", Difficulty.Easy),
        C("Everyone In", "Count To Ten",
          "As a table, count aloud to ten. One voice per number, no order agreed in advance, and nobody may " +
          "say two numbers in a row. Two people speak at once and you start again.", Difficulty.Medium),
        C("Everyone In", "One Sentence, One Word Each",
          "Build a single sentence, one word per person, going round until somebody ends it. It has to make " +
          "grammatical sense when read back. The table decides whether it does.", Difficulty.Medium),
        C("Everyone In", "Everybody Hum",
          "Everyone hums a different song at the same time for ten seconds. Afterwards, name every song you " +
          "heard. Get them all and it's cleared.", Difficulty.Hard),
        C("Everyone In", "Unanimous Or Nothing",
          "Someone names a category. Everyone writes one answer privately. Cleared only if all of you wrote " +
          "something different.", Difficulty.Medium),

        // ── RELAY — it passes round and one break costs the card ──────────────
        C("Relay", "No Repeats",
          "Someone names a category. Go round naming items in it, no repeats, no pauses longer than three " +
          "seconds. Get all the way round the table twice and it's cleared.", Difficulty.Easy),
        C("Relay", "Alphabet Round",
          "Round the table in A-to-Z order — first person names something starting with A, next with B, on you " +
          "go. Reach the person who started A again and you've cleared it.", Difficulty.Medium),
        C("Relay", "Last Letter First",
          "Each answer must start with the last letter of the previous one. Same category throughout. Ten in a " +
          "row clears it.", Difficulty.Medium),
        C("Relay", "Story With A Rule",
          "Tell a story one sentence each. Before you start, the table picks a word nobody may say. Get twice " +
          "round without anyone saying it — and without the story collapsing — and it's cleared.", Difficulty.Hard),
        C("Relay", "Countdown Under Pressure",
          "Count backwards from fifty as a table, one number each, going round. Anyone who hesitates or " +
          "misspeaks and you start from fifty again. Three attempts, then it's the deck's card.", Difficulty.Hard),

        // ── IN SILENCE — no talking, which is much harder than it sounds ──────
        C("In Silence", "Line Up By Birthday",
          "Without speaking or writing, arrange yourselves in order of birthday — January at one end, December " +
          "at the other. Check it out loud only when everyone has stopped moving.", Difficulty.Medium),
        C("In Silence", "Pass The Face",
          "First person makes an expression. It goes round, each person copying the one before as exactly as " +
          "they can. If the last face still resembles the first, cleared.", Difficulty.Easy),
        C("In Silence", "Silent Agreement",
          "Without a word, the table must all point at the same person on the count of three. Cleared only if " +
          "it's unanimous — and nobody may point at themselves.", Difficulty.Hard),
        C("In Silence", "Order Without Words",
          "Someone picks a category with an obvious order — height, alphabetical first names, distance from " +
          "home. Get yourselves into that order in silence.", Difficulty.Medium),

        // ── AGAINST IT — the hard ones, worth the most ────────────────────────
        C("Against It", "Everyone Knows Something",
          "Every person at the table must state one fact nobody else here knew about them. All of them have to " +
          "land — if the table already knew one, the card isn't cleared.", Difficulty.Hard),
        C("Against It", "Sixty Seconds, Twenty Things",
          "One minute. As a table, name twenty things you can see from where you're sitting. Anyone may speak " +
          "at any time; no repeats.", Difficulty.Hard),
        C("Against It", "Name Everyone's Everything",
          "Go round: each person has to name something every single other person at the table likes. Not " +
          "guesses — things they can be corrected on. One wrong and the card stands.", Difficulty.Extreme),
        C("Against It", "The Long Sentence",
          "Build one sentence, one word each, that runs at least thirty words and still makes sense at the end. " +
          "Somebody count. Somebody else adjudicate.", Difficulty.Extreme),
        C("Against It", "All Of You, One Voice",
          "Pick a song everyone knows. Sing one line together, in time, in unison, from a standing start with " +
          "no count-in. The table decides whether that was one voice or several.", Difficulty.Extreme),

        // ── DEBRIEF — it closes properly whichever way it went ────────────────
        C("Debrief", "Where It Turned",
          "Whether you cleared it or not: agree on the single card where the game turned. There's usually one, " +
          "and you usually all know which.", Difficulty.Easy),
        C("Debrief", "Who Carried It",
          "Name the person who did the most to get you through — and say the specific thing they did. Not " +
          "everyone gets named and that's fine; the point is that someone hears it.", Difficulty.Easy),
        C("Debrief", "One For Next Time",
          "Agree one rule to bring to your next game. Write it down if you're the sort of table that keeps " +
          "things. That's the end of the deck.", Difficulty.Easy),
    ];
}
