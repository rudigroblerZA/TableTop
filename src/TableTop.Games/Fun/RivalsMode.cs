using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Fun;

/// <summary>
/// Rivals — the first mode built for teams, and built around the one thing
/// team play makes possible that solo play can't: <b>the other side chooses
/// how hard your turn is.</b>
///
/// <para>
/// Every card carries three versions of the same challenge — Easy, Hard and
/// Brutal, worth 1, 3 and 5. The player whose turn it is doesn't pick. The
/// <i>opposing team</i> does, together, out loud, before the card is attempted.
/// </para>
///
/// <para>
/// <b>Why that's a real mechanic and not just a scoring tweak.</b> It makes
/// the opposing team's choice a genuine dilemma rather than an obvious one:
/// stack it Brutal and they probably fail for nothing — but if they land it,
/// that's five points you handed them. Play it safe on Easy and you've
/// conceded a cheap point you could have contested. There's no dominant
/// strategy, so the interesting decision happens on the side of the table
/// that would otherwise just be watching. Every other mode in the catalogue
/// leaves non-active players with nothing to do; here they have the most
/// consequential call of the turn.
/// </para>
///
/// <para>
/// Turn order alternates between teams automatically —
/// <c>TeamAlternatingPlayerManager</c>, selected by
/// <see cref="ITeamMode"/> — so the same side never goes twice running, even
/// if the host typed one whole team in before the other.
/// </para>
///
/// <para>
/// Scoring stays per-player and team totals are summed from members, so the
/// existing scoring strategies work untouched. A player earning 5 for their
/// team is a player scoring 5; nothing had to learn about teams to make that
/// work.
/// </para>
/// </summary>
public sealed class RivalsMode : BaseGameModeDefinition, ITeamMode, ITableShapeMode
{
    /// <summary>Two teams by default — the dilemma is sharpest head to head.</summary>
    public int PreferredTeamCount => 2;

    /// <summary>Four: two teams of two. With fewer, "the opposing team decides" is just one person deciding.</summary>
    public int MinimumPlayersForTeams => 4;

    /// <summary>A group game — needs enough people to make two real sides.</summary>
    public TableShape SuitableFor => TableShape.Group;

    /// <inheritdoc />
    public override string Name => "Rivals";

    /// <inheritdoc />
    public override string Description =>
        "Two teams. Every card comes in Easy, Hard and Brutal — and the other team picks which one you attempt. Go gentle and concede a point, or stack it and risk handing them five.";

    /// <inheritdoc />
    public override int MinimumPlayers => 4;

    /// <inheritdoc />
    public override string CompleteLabel => "Landed It";

    /// <inheritdoc />
    public override string SkipLabel => "Didn't Land";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [RivalsCardBank.HowToPlayCategory] = "#26A69A",
            ["Wordplay"] = "#42A5F5",
            ["Memory"] = "#AB47BC",
            ["Performance"] = "#FFA726",
            ["Knowledge"] = "#66BB6A",
            ["Nerve"] = "#EF5350",
        };

    /// <summary>The rules card explains the mechanic and must come before any card relying on it.</summary>
    public override IReadOnlyList<string> CategoriesPinnedToStart => [RivalsCardBank.HowToPlayCategory];

    /// <inheritdoc />
    protected override IScoringStrategy BuildScoring() => new DifficultyBasedScoringStrategy();

    /// <inheritdoc />
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        RivalsCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => RivalsCardBank.All;
}

/// <summary>
/// Built-in card bank for Rivals, authored with <see cref="CardDeckBuilder"/>.
///
/// Every play card presents the same three-tier shape, because the mechanic
/// depends on the choice being real every single time — a card with only two
/// usable tiers would quietly collapse the decision.
/// </summary>
public static class RivalsCardBank
{
    internal const string HowToPlayCategory = "How To Play";

    /// <summary>All cards, in intended play order.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private const string Footer =
        "\n\n<i>The other team picks — agree out loud before they start.</i>";

    /// <summary>Formats a three-tier challenge. Points are fixed per tier so the trade-off stays legible.</summary>
    private static string Tiers(string setup, string easy, string hard, string brutal) =>
        $"{setup}\n\n" +
        $"<b>EASY — 1 point.</b> {easy}\n" +
        $"<b>HARD — 3 points.</b> {hard}\n" +
        $"<b>BRUTAL — 5 points.</b> {brutal}" +
        Footer;

    private static IReadOnlyList<ICard> Build() => CardDeckBuilder
        .For("Rivals")

        .Category(HowToPlayCategory)
            .Card("How Rivals Works",
                "Two teams, alternating turns — the app handles whose go it is.\n\n" +
                "Every card has three versions: <b>Easy</b> (1 point), <b>Hard</b> (3) and <b>Brutal</b> (5). " +
                "The player up doesn't choose. <b>The other team does</b> — together, out loud, before the attempt starts.\n\n" +
                "That's the whole game. Pick Easy and you've handed them a point. Pick Brutal and they probably fail — " +
                "but if they pull it off, that's five. There's no safe answer, which is the point.\n\n" +
                "Points go to the player, and their team's total is the sum of its members. Highest team total when the deck runs out wins.",
                Difficulty.Easy)

        // ── WORDPLAY ─────────────────────────────────────────────────────────
        .Category("Wordplay")
            .Card("Letters",
                Tiers("Name things starting with a letter the other team picks.",
                    "Five things. Any category. 30 seconds.",
                    "Eight things, all in one category they name. 30 seconds.",
                    "Ten things in a category they name, no two sharing a second letter. 45 seconds."),
                Difficulty.Medium)
            .Card("Without Saying",
                Tiers("Describe something the other team writes down, without using certain words.",
                    "Describe it without saying the word itself.",
                    "Also without three related words they choose.",
                    "Also without gesturing, and in under 20 seconds."),
                Difficulty.Medium)
            .Card("Rhyme Chain",
                Tiers("The other team gives you a word.",
                    "Give three words that rhyme with it.",
                    "Give six, no proper nouns.",
                    "Give a rhyming couplet using it, on the spot, that actually scans."),
                Difficulty.Hard)
            .Card("Backwards",
                Tiers("The other team picks a word or phrase.",
                    "Spell a five-letter word of theirs backwards, out loud.",
                    "Spell an eight-letter word backwards, out loud, no writing.",
                    "Say a whole four-word phrase backwards, word order and letters both."),
                Difficulty.Hard)
            .Card("Alphabet Story",
                Tiers("Tell a story where each sentence starts with the next letter.",
                    "Four sentences, starting from A.",
                    "Six sentences, starting from a letter they choose.",
                    "Eight sentences from their letter, and it has to actually make sense."),
                Difficulty.Hard)

        // ── MEMORY ───────────────────────────────────────────────────────────
        .Category("Memory")
            .Card("The List",
                Tiers("The other team reads you a list of items, once.",
                    "Six items. Repeat them in any order.",
                    "Nine items, in the order given.",
                    "Twelve items, in order, then again backwards."),
                Difficulty.Medium)
            .Card("Who Said It",
                Tiers("Recall things said earlier tonight.",
                    "Name one thing anyone here said in the last ten minutes.",
                    "Quote someone from earlier in the game, near enough word for word.",
                    "Name, in order, the last three cards your team attempted and how each went."),
                Difficulty.Hard)
            .Card("The Room",
                Tiers("Close your eyes. The other team asks about what's around you.",
                    "Name five things you can't currently see in this room.",
                    "Answer three specific questions they ask about the room.",
                    "Describe what everyone here is wearing, one detail each, eyes shut."),
                Difficulty.Hard)
            .Card("Numbers",
                Tiers("The other team reads out a number.",
                    "Six digits. Repeat it.",
                    "Nine digits. Repeat it backwards.",
                    "Nine digits — repeat it backwards, then say their sum."),
                Difficulty.Extreme)

        // ── PERFORMANCE ──────────────────────────────────────────────────────
        .Category("Performance")
            .Card("Act It",
                Tiers("The other team writes down a thing to act out.",
                    "Act it. Your team guesses. No sound.",
                    "Act it with one hand behind your back and no sound.",
                    "Act it sitting completely still — face only."),
                Difficulty.Medium)
            .Card("Accent",
                Tiers("The other team picks an accent.",
                    "Say one sentence in it. Recognisable is enough.",
                    "Tell a 20-second story in it without dropping it.",
                    "Hold it for your whole next turn as well, whatever that turn is."),
                Difficulty.Hard)
            .Card("Sing It",
                Tiers("The other team names a song.",
                    "Hum eight bars. Your team guesses.",
                    "Sing the chorus. Actual words.",
                    "Sing it in a style they name — opera, metal, lullaby, their call."),
                Difficulty.Hard)
            .Card("The Sell",
                Tiers("The other team hands you an ordinary object.",
                    "Sell it to the room in 20 seconds.",
                    "Sell it as something it obviously isn't, 30 seconds, straight face.",
                    "Sell it in a language you don't speak. Commit completely."),
                Difficulty.Extreme)
            .Card("Freeze",
                Tiers("The other team names a pose or expression.",
                    "Hold it for 15 seconds without laughing.",
                    "Hold it for 30 while they actively try to make you laugh.",
                    "Hold it for 30 while answering their questions in character."),
                Difficulty.Extreme)

        // ── KNOWLEDGE ────────────────────────────────────────────────────────
        .Category("Knowledge")
            .Card("Name Them",
                Tiers("The other team picks a category.",
                    "Name three things in it.",
                    "Name seven in 30 seconds.",
                    "Name ten in 45 seconds, no repeats, no hesitating past three seconds."),
                Difficulty.Medium)
            .Card("Put Them In Order",
                Tiers("The other team names some things to sequence.",
                    "Order three by date, size or value — their pick.",
                    "Order five.",
                    "Order seven, and justify any two of them when challenged."),
                Difficulty.Hard)
            .Card("Explain It",
                Tiers("The other team picks something you'd have to explain.",
                    "Explain it in 30 seconds so a ten-year-old gets it.",
                    "Explain it in exactly one sentence.",
                    "Explain it without using the five most obvious words, which they name first."),
                Difficulty.Hard)
            .Card("True Or Not",
                Tiers("You make claims; the other team judges.",
                    "Say three things about yourself — one false. They guess which.",
                    "Five things, two false.",
                    "Five things, and they choose how many are false before you speak."),
                Difficulty.Medium)

        // ── NERVE ────────────────────────────────────────────────────────────
        .Category("Nerve")
            .Card("Say It To Their Face",
                Tiers("Directed at the opposing team.",
                    "Pay one of them a genuine compliment.",
                    "Pay every one of them a genuine, specific, different compliment.",
                    "Do that, and then accept one back without deflecting or joking."),
                Difficulty.Medium)
            .Card("The Confession",
                Tiers("Something true, out loud.",
                    "Admit something mildly embarrassing.",
                    "Answer any one question the other team asks, honestly.",
                    "Answer three, honestly, no passing."),
                Difficulty.Hard)
            .Card("Hand It Over",
                Tiers("Give up some control.",
                    "Let the other team choose your next card's tier in advance.",
                    "Let them assign your next turn to a teammate of their choosing.",
                    "Let them take one point off your team, or add two — their choice, decided now."),
                Difficulty.Extreme)
            .Card("Double Or Nothing",
                Tiers("Only if your team agrees first.",
                    "This card's points count double if you land it, zero if not.",
                    "Double, and a miss costs your team a point.",
                    "Triple, and a miss costs your team three."),
                Difficulty.Extreme)

        .Build();
}
