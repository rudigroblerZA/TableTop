using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Emoji Legends — guess films, songs, and books from emoji sequences.
///
/// How to play:
///   1. The emoji sequence is revealed.
///   2. Everyone writes down what they think it represents (film, song, book, or TV show).
///   3. Reveal the answer.
///   4. Points for correct guesses.
///
/// Some are obvious: 🧊👑 = Frozen. Some are clever: 🔥🐉⚔️ = Game of Thrones.
/// Some are absurd: ⚙️🧠💔 = Heartless (the emoji version, not the song).
///
/// Works for all ages. Bridges the gap between pop culture and visual puzzle solving.
/// Great for testing who actually knows their films, songs, and books versus who just
/// thinks they do.
/// </summary>
public sealed class EmojiLegendsMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Emoji Legends";
    /// <inheritdoc />
    public override string Description =>
        "Emoji sequence = film, song, or book. Can you guess it?";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Guessed";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [EmojiLegendsCardBank.FilmCategory] = "#42A5F5",
            [EmojiLegendsCardBank.SongCategory] = "#EC407A",
            [EmojiLegendsCardBank.BookCategory] = "#66BB6A",
            [EmojiLegendsCardBank.TVShowCategory] = "#FFCA28",
            [EmojiLegendsCardBank.MixedCategory] = "#AB47BC",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        EmojiLegendsCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => EmojiLegendsCardBank.All;
}

/// <summary>Built-in card bank for Emoji Legends. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class EmojiLegendsCardBank
{
    internal const string FilmCategory = "Film";
    internal const string SongCategory = "Song";
    internal const string BookCategory = "Book";
    internal const string TVShowCategory = "TV Show";
    internal const string MixedCategory = "Mixed";

    private const string Deck = "Emoji Legends";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)
        // ── FILM ──────────────────────────────────────────────────────────────
            .Category(FilmCategory)
            .Card(FilmCategory, Body("🧊👑", "Frozen"), Difficulty.Easy)
            .Card(FilmCategory, Body("🚗💨⚡", "Cars"), Difficulty.Easy)
            .Card(FilmCategory, Body("🦁👑🌍", "The Lion King"), Difficulty.Easy)
            .Card(FilmCategory, Body("🎬📽️🎞️", "The Truman Show"), Difficulty.Medium)
            .Card(FilmCategory, Body("🏴\u200D☠️💀🌊", "Pirates of the Caribbean"), Difficulty.Easy)
            .Card(FilmCategory, Body("🔥🐉⚔️", "Game of Thrones"), Difficulty.Easy)
            .Card(FilmCategory, Body("🎒🌍✈️", "Up"), Difficulty.Easy)
            .Card(FilmCategory, Body("👶💼🎩", "Boss Baby"), Difficulty.Easy)
            .Card(FilmCategory, Body("🕷️🦸\u200D♂️💫", "Spider-Man"), Difficulty.Easy)
            .Card(FilmCategory, Body("🧙\u200D♂️⚡🪄", "Harry Potter"), Difficulty.Easy)
            .Card(FilmCategory, Body("🌊🧜\u200D♀️👑", "The Little Mermaid"), Difficulty.Easy)
            .Card(FilmCategory, Body("🐘👂💔", "Dumbo"), Difficulty.Medium)
            .Card(FilmCategory, Body("💍⚔️🗻", "The Lord of the Rings"), Difficulty.Easy)

        // ── SONG ──────────────────────────────────────────────────────────────
            .Category(SongCategory)
            .Card(SongCategory, Body("🎵🐝🐦", "Let It Be (Beatles)"), Difficulty.Medium)
            .Card(SongCategory, Body("🌧️☂️👨", "Singin' in the Rain"), Difficulty.Medium)
            .Card(SongCategory, Body("🚀🌙⭐", "Rocket Man (Elton John)"), Difficulty.Hard)
            .Card(SongCategory, Body("💔🎵", "Someone Like You (Adele)"), Difficulty.Medium)
            .Card(SongCategory, Body("🐕🎵", "Hound Dog (Elvis)"), Difficulty.Hard)
            .Card(SongCategory, Body("⛰️💬", "The Hills (The Weeknd)"), Difficulty.Medium)
            .Card(SongCategory, Body("👸💎", "Royals (Lorde)"), Difficulty.Medium)
            .Card(SongCategory, Body("💔🎹🔥", "Rolling in the Deep"), Difficulty.Medium)

        // ── BOOK ──────────────────────────────────────────────────────────────
            .Category(BookCategory)
            .Card(BookCategory, Body("📚❄️🏔️", "The Hobbit"), Difficulty.Medium)
            .Card(BookCategory, Body("🔮🔬", "Invisible Woman"), Difficulty.Hard)
            .Card(BookCategory, Body("🧛💔", "Twilight"), Difficulty.Easy)
            .Card(BookCategory, Body("💀💀💀📚", "Macbeth (Shakespeare)"), Difficulty.Hard)
            .Card(BookCategory, Body("🏃\u200D♂️🏃\u200D♀️💫", "The Hunger Games"), Difficulty.Easy)
            .Card(BookCategory, Body("🎪🎡🌙", "The Phantom of the Opera"), Difficulty.Medium)
            .Card(BookCategory, Body("🐅📗", "The Tiger That Came to Tea"), Difficulty.Medium)

        // ── TV SHOW ───────────────────────────────────────────────────────────
            .Category(TVShowCategory)
            .Card(TVShowCategory, Body("👨\u200D👩\u200D👧\u200D👦🏘️☕", "Friends"), Difficulty.Easy)
            .Card(TVShowCategory, Body("🧛🏰🩸", "The Vampire Diaries"), Difficulty.Medium)
            .Card(TVShowCategory, Body("🚗🏃\u200D♂️", "Breaking Bad"), Difficulty.Easy)
            .Card(TVShowCategory, Body("🖤💀🔮", "Wednesday"), Difficulty.Easy)
            .Card(TVShowCategory, Body("🎬🎭📺", "Curb Your Enthusiasm"), Difficulty.Hard)
            .Card(TVShowCategory, Body("🧋🧟", "Squid Game"), Difficulty.Easy)

        // ── MIXED ────────────────────────────────────────────────────────────
            .Category(MixedCategory)
            .Card(MixedCategory, Body("🍎👩\u200D🦱", "Snow White"), Difficulty.Easy)
            .Card(MixedCategory, Body("🐢🐢🐢🍕", "Teenage Mutant Ninja Turtles"), Difficulty.Easy)
            .Card(MixedCategory, Body("🌹👸🐺", "Beauty and the Beast"), Difficulty.Easy)
            .Card(MixedCategory, Body("⚡🪄✨", "Harry Potter (franchise)"), Difficulty.Easy)
            .Card(MixedCategory, Body("🦸\u200D♂️🛡️💫", "Captain America"), Difficulty.Easy)
            .Card(MixedCategory, Body("🌳🗿🌍", "Lord of the Rings (extended)"), Difficulty.Medium)

            .Build();

    private static string Body(string emojis, string answer) =>
        "<b>What film, song, or book is this?</b>\n\n" +
        emojis + "\n\n" +
        "<b>Write your guess.</b> Film, song, book, or TV show?\n\n" +
        "<b>Answer:</b> " + answer;
}
