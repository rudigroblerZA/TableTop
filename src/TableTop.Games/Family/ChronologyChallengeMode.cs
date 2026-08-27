using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Chronology Challenge — a sequencing game about order and history.
///
/// How to play:
///   1. Read out a set of four historical events, discoveries, or facts.
///   2. Players write them down and arrange them in chronological order.
///   3. Reveal the answer — whoever got it right scores.
///   4. Discuss the surprising ones (spoiler: humans are much older than you thought).
///
/// Cards cover real history (invention of the wheel, first aeroplane), pop culture
/// (when was the first emoji?), famous disasters, scientific breakthroughs, and more.
/// Some are obvious, some are shockingly recent, some make you realise how long ago
/// things happened.
///
/// Great for all ages. Teaches history naturally and generates funny arguments
/// ("Wait, the internet is THAT old?"). No material memorisation required — just
/// a sense of when things happened.
/// </summary>
public sealed class ChronologyChallengeMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Chronology Challenge";
    /// <inheritdoc />
    public override string Description =>
        "Put four events in order. Did they happen closer together than you think — or further apart?";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Ordered";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [ChronologyChallengeCardBank.HistoryCategory] = "#42A5F5",
            [ChronologyChallengeCardBank.InventionCategory] = "#66BB6A",
            [ChronologyChallengeCardBank.PopCultureCategory] = "#EC407A",
            [ChronologyChallengeCardBank.ScienceCategory] = "#AB47BC",
            [ChronologyChallengeCardBank.ModernCategory] = "#FFCA28",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        ChronologyChallengeCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => ChronologyChallengeCardBank.All;
}

/// <summary>Built-in card bank for Chronology Challenge. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class ChronologyChallengeCardBank
{
    internal const string HistoryCategory = "History";
    internal const string InventionCategory = "Invention";
    internal const string PopCultureCategory = "Pop Culture";
    internal const string ScienceCategory = "Science";
    internal const string ModernCategory = "Modern";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── HISTORY ──────────────────────────────────────────────────────────
        C(HistoryCategory,
            "1) The Great Wall of China was completed\n" +
            "2) The Roman Empire fell\n" +
            "3) The Egyptian pyramids were built\n" +
            "4) The printing press was invented",
            "3 (2560 BC) → 2 (476 AD) → 4 (1440 AD) → 1 (1644 AD)\n" +
            "Wait, the pyramids were first? Yes. By a huge margin.",
            Difficulty.Hard),

        C(HistoryCategory,
            "1) Shakespeare was born\n" +
            "2) The American Declaration of Independence\n" +
            "3) The fall of the Berlin Wall\n" +
            "4) The first Moon landing",
            "1 (1564) → 2 (1776) → 4 (1969) → 3 (1989)\n" +
            "Shakespeare lived closer to us than to the Moon landing.",
            Difficulty.Hard),

        C(HistoryCategory,
            "1) Julius Caesar was assassinated\n" +
            "2) London was founded\n" +
            "3) Cleopatra ruled Egypt\n" +
            "4) The Vikings were sailing",
            "3 (51 BC) → 1 (44 BC) → 2 (43 AD) → 4 (793 AD)\n" +
            "Cleopatra lived closer to our time than to the pyramids.",
            Difficulty.Hard),

        // ── INVENTION ────────────────────────────────────────────────────────
        C(InventionCategory,
            "1) The telephone was invented\n" +
            "2) The electric light bulb was invented\n" +
            "3) The aeroplane first flew\n" +
            "4) The first computer was built",
            "1 (1876) → 2 (1879) → 3 (1903) → 4 (1946)\n" +
            "That's only 30 years from first phone to first plane!",
            Difficulty.Medium),

        C(InventionCategory,
            "1) The wheel was invented\n" +
            "2) Writing was invented\n" +
            "3) Iron was first smelted\n" +
            "4) Beer was first brewed",
            "4 (7000 BC) → 1 (3500 BC) → 2 (3200 BC) → 3 (1200 BC)\n" +
            "Humans figured out beer before writing. Says something.",
            Difficulty.Hard),

        C(InventionCategory,
            "1) The steam engine was invented\n" +
            "2) The cotton gin was invented\n" +
            "3) The microscope was invented\n" +
            "4) The telescope was invented",
            "4 (1608) → 3 (1609) → 2 (1793) → 1 (1769)\n" +
            "We could see space before we could see germs.",
            Difficulty.Hard),

        // ── POP CULTURE ──────────────────────────────────────────────────────
        C(PopCultureCategory,
            "1) The first Star Wars film was released\n" +
            "2) The first iPhone was released\n" +
            "3) The Beatles broke up\n" +
            "4) The first Harry Potter book was published",
            "3 (1970) → 1 (1977) → 4 (1997) → 2 (2007)\n" +
            "The iPhone is more recent than Harry Potter!",
            Difficulty.Medium),

        C(PopCultureCategory,
            "1) Elvis Presley was born\n" +
            "2) Michael Jackson released 'Thriller'\n" +
            "3) The first MTV video aired\n" +
            "4) Taylor Swift was born",
            "1 (1935) → 3 (1981) → 2 (1982) → 4 (1989)\n" +
            "From Elvis to MTV was 46 years. MTV to Taylor was 8.",
            Difficulty.Hard),

        C(PopCultureCategory,
            "1) The first Pokémon game was released\n" +
            "2) The first emoji was created\n" +
            "3) The first text message was sent\n" +
            "4) Instagram was founded",
            "3 (1992) → 2 (1999) → 1 (1996) → 4 (2010)\n" +
            "Texts came before emoji came before Pokémon!",
            Difficulty.Hard),

        // ── SCIENCE ──────────────────────────────────────────────────────────
        C(ScienceCategory,
            "1) Gravity was explained by Newton\n" +
            "2) DNA structure was discovered\n" +
            "3) The atom was split\n" +
            "4) Evolution was proposed by Darwin",
            "1 (1687) → 4 (1859) → 3 (1938) → 2 (1953)\n" +
            "Only 6 years between splitting the atom and understanding DNA.",
            Difficulty.Hard),

        C(ScienceCategory,
            "1) Penicillin was discovered\n" +
            "2) Vaccines were developed\n" +
            "3) Microbes were discovered\n" +
            "4) Surgery was first performed",
            "4 (Ancient Egypt) → 2 (1796) → 3 (1670s) → 1 (1928)\n" +
            "We had vaccines 200 years before we discovered microbes.",
            Difficulty.Hard),

        // ── MODERN ───────────────────────────────────────────────────────────
        C(ModernCategory,
            "1) The World Wide Web was created\n" +
            "2) The first text message was sent\n" +
            "3) The first webcam was used\n" +
            "4) Email was first sent",
            "4 (1971) → 2 (1992) → 3 (1993) → 1 (1989)\n" +
            "Email is older than the web!",
            Difficulty.Hard),

        C(ModernCategory,
            "1) Facebook was founded\n" +
            "2) YouTube was founded\n" +
            "3) Twitter was founded\n" +
            "4) Snapchat was founded",
            "1 (2004) → 3 (2006) → 2 (2005) → 4 (2011)\n" +
            "Wait, Twitter came after YouTube? Yes.",
            Difficulty.Medium),

        C(ModernCategory,
            "1) The first smartphone was released\n" +
            "2) The first tablet was released\n" +
            "3) Smartphones became mainstream\n" +
            "4) The first smartwatch was released",
            "1 (2007, iPhone) → 2 (2010, iPad) → 4 (2015, Apple Watch) → 3 (ongoing)\n" +
            "Smartwatches came way after smartphones and tablets.",
            Difficulty.Medium),
    ];

    private static ICard C(string category, string events, string answer, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>Put these in chronological order (oldest to newest):</b>\n\n" +
            events +
            "\n\n<b>Write down your order:</b> 1-2-3-4 (or however you think it goes)\n\n" +
            "<b>Answer:</b> " + answer,
            d, category);
}
