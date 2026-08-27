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

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── FILM ──────────────────────────────────────────────────────────────
        E(FilmCategory, "🧊👑", "Frozen", Difficulty.Easy),
        E(FilmCategory, "🚗💨⚡", "Cars", Difficulty.Easy),
        E(FilmCategory, "🦁👑🌍", "The Lion King", Difficulty.Easy),
        E(FilmCategory, "🎬📽️🎞️", "The Truman Show", Difficulty.Medium),
        E(FilmCategory, "🏴\u200D☠️💀🌊", "Pirates of the Caribbean", Difficulty.Easy),
        E(FilmCategory, "🔥🐉⚔️", "Game of Thrones", Difficulty.Easy),
        E(FilmCategory, "🎒🌍✈️", "Up", Difficulty.Easy),
        E(FilmCategory, "👶💼🎩", "Boss Baby", Difficulty.Easy),
        E(FilmCategory, "🕷️🦸\u200D♂️💫", "Spider-Man", Difficulty.Easy),
        E(FilmCategory, "🧙\u200D♂️⚡🪄", "Harry Potter", Difficulty.Easy),
        E(FilmCategory, "🌊🧜\u200D♀️👑", "The Little Mermaid", Difficulty.Easy),
        E(FilmCategory, "🐘👂💔", "Dumbo", Difficulty.Medium),
        E(FilmCategory, "💍⚔️🗻", "The Lord of the Rings", Difficulty.Easy),

        // ── SONG ──────────────────────────────────────────────────────────────
        E(SongCategory, "🎵🐝🐦", "Let It Be (Beatles)", Difficulty.Medium),
        E(SongCategory, "🌧️☂️👨", "Singin' in the Rain", Difficulty.Medium),
        E(SongCategory, "🚀🌙⭐", "Rocket Man (Elton John)", Difficulty.Hard),
        E(SongCategory, "💔🎵", "Someone Like You (Adele)", Difficulty.Medium),
        E(SongCategory, "🐕🎵", "Hound Dog (Elvis)", Difficulty.Hard),
        E(SongCategory, "⛰️💬", "The Hills (The Weeknd)", Difficulty.Medium),
        E(SongCategory, "👸💎", "Royals (Lorde)", Difficulty.Medium),
        E(SongCategory, "💔🎹🔥", "Rolling in the Deep", Difficulty.Medium),

        // ── BOOK ──────────────────────────────────────────────────────────────
        E(BookCategory, "📚❄️🏔️", "The Hobbit", Difficulty.Medium),
        E(BookCategory, "🔮🔬", "Invisible Woman", Difficulty.Hard),
        E(BookCategory, "🧛💔", "Twilight", Difficulty.Easy),
        E(BookCategory, "💀💀💀📚", "Macbeth (Shakespeare)", Difficulty.Hard),
        E(BookCategory, "🏃\u200D♂️🏃\u200D♀️💫", "The Hunger Games", Difficulty.Easy),
        E(BookCategory, "🎪🎡🌙", "The Phantom of the Opera", Difficulty.Medium),
        E(BookCategory, "🐅📗", "The Tiger That Came to Tea", Difficulty.Medium),

        // ── TV SHOW ───────────────────────────────────────────────────────────
        E(TVShowCategory, "👨\u200D👩\u200D👧\u200D👦🏘️☕", "Friends", Difficulty.Easy),
        E(TVShowCategory, "🧛🏰🩸", "The Vampire Diaries", Difficulty.Medium),
        E(TVShowCategory, "🚗🏃\u200D♂️", "Breaking Bad", Difficulty.Easy),
        E(TVShowCategory, "🖤💀🔮", "Wednesday", Difficulty.Easy),
        E(TVShowCategory, "🎬🎭📺", "Curb Your Enthusiasm", Difficulty.Hard),
        E(TVShowCategory, "🧋🧟", "Squid Game", Difficulty.Easy),

        // ── MIXED ────────────────────────────────────────────────────────────
        E(MixedCategory, "🍎👩\u200D🦱", "Snow White", Difficulty.Easy),
        E(MixedCategory, "🐢🐢🐢🍕", "Teenage Mutant Ninja Turtles", Difficulty.Easy),
        E(MixedCategory, "🌹👸🐺", "Beauty and the Beast", Difficulty.Easy),
        E(MixedCategory, "⚡🪄✨", "Harry Potter (franchise)", Difficulty.Easy),
        E(MixedCategory, "🦸\u200D♂️🛡️💫", "Captain America", Difficulty.Easy),
        E(MixedCategory, "🌳🗿🌍", "Lord of the Rings (extended)", Difficulty.Medium),
    ];

    private static ICard E(string category, string emojis, string answer, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>What film, song, or book is this?</b>\n\n" +
            emojis + "\n\n" +
            "<b>Write your guess.</b> Film, song, book, or TV show?\n\n" +
            "<b>Answer:</b> " + answer,
            d, category);
}
