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
            ["Film"] = "#42A5F5",
            ["Song"] = "#EC407A",
            ["Book"] = "#66BB6A",
            ["TV Show"] = "#FFCA28",
            ["Mixed"] = "#AB47BC",
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
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── FILM ──────────────────────────────────────────────────────────────
        E("Film", "🧊👑", "Frozen", Difficulty.Easy),
        E("Film", "🚗💨⚡", "Cars", Difficulty.Easy),
        E("Film", "🦁👑🌍", "The Lion King", Difficulty.Easy),
        E("Film", "🎬📽️🎞️", "The Truman Show", Difficulty.Medium),
        E("Film", "🏴\u200D☠️💀🌊", "Pirates of the Caribbean", Difficulty.Easy),
        E("Film", "🔥🐉⚔️", "Game of Thrones", Difficulty.Easy),
        E("Film", "🎒🌍✈️", "Up", Difficulty.Easy),
        E("Film", "👶💼🎩", "Boss Baby", Difficulty.Easy),
        E("Film", "🕷️🦸\u200D♂️💫", "Spider-Man", Difficulty.Easy),
        E("Film", "🧙\u200D♂️⚡🪄", "Harry Potter", Difficulty.Easy),
        E("Film", "🌊🧜\u200D♀️👑", "The Little Mermaid", Difficulty.Easy),
        E("Film", "🐘👂💔", "Dumbo", Difficulty.Medium),
        E("Film", "💍⚔️🗻", "The Lord of the Rings", Difficulty.Easy),

        // ── SONG ──────────────────────────────────────────────────────────────
        E("Song", "🎵🐝🐦", "Let It Be (Beatles)", Difficulty.Medium),
        E("Song", "🌧️☂️👨", "Singin' in the Rain", Difficulty.Medium),
        E("Song", "🚀🌙⭐", "Rocket Man (Elton John)", Difficulty.Hard),
        E("Song", "💔🎵", "Someone Like You (Adele)", Difficulty.Medium),
        E("Song", "🐕🎵", "Hound Dog (Elvis)", Difficulty.Hard),
        E("Song", "⛰️💬", "The Hills (The Weeknd)", Difficulty.Medium),
        E("Song", "👸💎", "Royals (Lorde)", Difficulty.Medium),
        E("Song", "💔🎹🔥", "Rolling in the Deep", Difficulty.Medium),

        // ── BOOK ──────────────────────────────────────────────────────────────
        E("Book", "📚❄️🏔️", "The Hobbit", Difficulty.Medium),
        E("Book", "🔮🔬", "Invisible Woman", Difficulty.Hard),
        E("Book", "🧛💔", "Twilight", Difficulty.Easy),
        E("Book", "💀💀💀📚", "Macbeth (Shakespeare)", Difficulty.Hard),
        E("Book", "🏃\u200D♂️🏃\u200D♀️💫", "The Hunger Games", Difficulty.Easy),
        E("Book", "🎪🎡🌙", "The Phantom of the Opera", Difficulty.Medium),
        E("Book", "🐅📗", "The Tiger That Came to Tea", Difficulty.Medium),

        // ── TV SHOW ───────────────────────────────────────────────────────────
        E("TV Show", "👨\u200D👩\u200D👧\u200D👦🏘️☕", "Friends", Difficulty.Easy),
        E("TV Show", "🧛🏰🩸", "The Vampire Diaries", Difficulty.Medium),
        E("TV Show", "🚗🏃\u200D♂️", "Breaking Bad", Difficulty.Easy),
        E("TV Show", "🖤💀🔮", "Wednesday", Difficulty.Easy),
        E("TV Show", "🎬🎭📺", "Curb Your Enthusiasm", Difficulty.Hard),
        E("TV Show", "🧋🧟", "Squid Game", Difficulty.Easy),

        // ── MIXED ────────────────────────────────────────────────────────────
        E("Mixed", "🍎👩\u200D🦱", "Snow White", Difficulty.Easy),
        E("Mixed", "🐢🐢🐢🍕", "Teenage Mutant Ninja Turtles", Difficulty.Easy),
        E("Mixed", "🌹👸🐺", "Beauty and the Beast", Difficulty.Easy),
        E("Mixed", "⚡🪄✨", "Harry Potter (franchise)", Difficulty.Easy),
        E("Mixed", "🦸\u200D♂️🛡️💫", "Captain America", Difficulty.Easy),
        E("Mixed", "🌳🗿🌍", "Lord of the Rings (extended)", Difficulty.Medium),
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
