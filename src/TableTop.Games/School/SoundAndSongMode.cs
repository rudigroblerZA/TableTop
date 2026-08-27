using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Sound &amp; Song — general-knowledge music for the classroom: instruments and
/// the families of the orchestra, the basics of how music is written and
/// counted, and broadly-known music history, as multiple-choice questions
/// dealt one at a time.
///
/// Kept to timeless, curriculum-friendly ground — instrument families, note
/// names, tempo words, long-established composers — and deliberately avoids
/// song lyrics or current chart acts, so it stays evergreen and copyright-safe.
/// </summary>
public sealed class SoundAndSongMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Sound & Song";
    /// <inheritdoc />
    public override string Description =>
        "Music general knowledge — instruments, the orchestra, reading and counting music, and music history. Multiple choice.";

    /// <summary>Label for a correctly answered card.</summary>
    public override string CompleteLabel => "Correct";
    /// <summary>Label for a passed card.</summary>
    public override string SkipLabel => "Pass";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [SoundAndSongCardBank.InstrumentsCategory] = "#42A5F5",
            [SoundAndSongCardBank.TheOrchestraCategory] = "#AB47BC",
            [SoundAndSongCardBank.ReadingMusicCategory] = "#26A69A",
            [SoundAndSongCardBank.TempoTermsCategory] = "#FFA726",
            [SoundAndSongCardBank.MusicHistoryCategory] = "#EC407A",
        };

    /// <summary>Harder questions score more.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in Sound &amp; Song card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        SoundAndSongCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => SoundAndSongCardBank.All;
}

/// <summary>Built-in card bank for Sound &amp; Song.</summary>
public static class SoundAndSongCardBank
{
    internal const string InstrumentsCategory = "Instruments";
    internal const string TheOrchestraCategory = "The Orchestra";
    internal const string ReadingMusicCategory = "Reading Music";
    internal const string TempoTermsCategory = "Tempo & Terms";
    internal const string MusicHistoryCategory = "Music History";

    /// <summary>All cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── INSTRUMENTS ──────────────────────────────────────────────────────
        Q(InstrumentsCategory, "How many strings does a standard guitar have?", "4", "5", "6", "8", AnswerLabel.C, Difficulty.Easy),
        Q(InstrumentsCategory, "Which instrument has black and white keys?", "Violin", "Piano", "Flute", "Drum", AnswerLabel.B, Difficulty.Easy),
        Q(InstrumentsCategory, "Which of these is a percussion instrument?", "Trumpet", "Cello", "Drum", "Flute", AnswerLabel.C, Difficulty.Easy),
        Q(InstrumentsCategory, "You play a flute by doing what?", "Plucking", "Blowing", "Striking", "Bowing", AnswerLabel.B, Difficulty.Medium),
        Q(InstrumentsCategory, "How many strings does a standard violin have?", "4", "5", "6", "7", AnswerLabel.A, Difficulty.Medium),
        Q(InstrumentsCategory, "Which instrument is the largest of the string family, played sitting between the knees?", "Violin", "Viola", "Cello", "Harp", AnswerLabel.C, Difficulty.Hard),

        // ── THE ORCHESTRA ────────────────────────────────────────────────────
        Q(TheOrchestraCategory, "The violin, viola, and cello belong to which orchestra family?", "Brass", "Woodwind", "Strings", "Percussion", AnswerLabel.C, Difficulty.Medium),
        Q(TheOrchestraCategory, "The trumpet and trombone belong to which family?", "Strings", "Brass", "Woodwind", "Percussion", AnswerLabel.B, Difficulty.Medium),
        Q(TheOrchestraCategory, "The flute, clarinet, and oboe belong to which family?", "Brass", "Strings", "Woodwind", "Percussion", AnswerLabel.C, Difficulty.Hard),
        Q(TheOrchestraCategory, "Who stands at the front and leads an orchestra?", "Soloist", "Conductor", "Composer", "Captain", AnswerLabel.B, Difficulty.Easy),
        Q(TheOrchestraCategory, "Which family does the drum and cymbal belong to?", "Percussion", "Brass", "Strings", "Woodwind", AnswerLabel.A, Difficulty.Easy),

        // ── READING MUSIC ────────────────────────────────────────────────────
        Q(ReadingMusicCategory, "How many musical notes are in one octave before the pattern repeats (A to G)?", "5", "6", "7", "8", AnswerLabel.C, Difficulty.Medium),
        Q(ReadingMusicCategory, "The symbol at the start of a music staff that sets the pitch is the…?", "Note", "Clef", "Bar", "Rest", AnswerLabel.B, Difficulty.Hard),
        Q(ReadingMusicCategory, "A symbol showing a moment of silence in music is called a…?", "Rest", "Pause", "Gap", "Break", AnswerLabel.A, Difficulty.Medium),
        Q(ReadingMusicCategory, "The lines and spaces that music is written on are called the…?", "Grid", "Staff (stave)", "Ladder", "Chart", AnswerLabel.B, Difficulty.Hard),
        Q(ReadingMusicCategory, "Which of these note values lasts the longest?", "Quarter note", "Half note", "Whole note", "Eighth note", AnswerLabel.C, Difficulty.Extreme),

        // ── TEMPO & TERMS ────────────────────────────────────────────────────
        Q(TempoTermsCategory, "In music, what does 'tempo' describe?", "How loud", "How fast", "How high", "How long", AnswerLabel.B, Difficulty.Medium),
        Q(TempoTermsCategory, "The word 'forte' in music means to play…?", "Softly", "Loudly", "Slowly", "Quickly", AnswerLabel.B, Difficulty.Hard),
        Q(TempoTermsCategory, "The word 'piano' as a music instruction means to play…?", "Loudly", "Softly", "Fast", "Slow", AnswerLabel.B, Difficulty.Hard),
        Q(TempoTermsCategory, "A group of singers performing together is called a…?", "Band", "Choir", "Troupe", "Cast", AnswerLabel.B, Difficulty.Easy),
        Q(TempoTermsCategory, "A piece of music written for one performer alone is a…?", "Duet", "Solo", "Trio", "Chorus", AnswerLabel.B, Difficulty.Medium),

        // ── MUSIC HISTORY — long-established, safe ────────────────────────────
        Q(MusicHistoryCategory, "Which composer wrote 'Für Elise' and continued composing after going deaf?", "Mozart", "Beethoven", "Bach", "Chopin", AnswerLabel.B, Difficulty.Hard),
        Q(MusicHistoryCategory, "Mozart was a famous composer from which century era of music?", "Baroque", "Classical", "Romantic", "Modern", AnswerLabel.B, Difficulty.Extreme),
        Q(MusicHistoryCategory, "Which family of instruments is the oldest, likely the very first?", "Brass", "Strings", "Percussion", "Woodwind", AnswerLabel.C, Difficulty.Hard),
        Q(MusicHistoryCategory, "Jazz music is widely said to have begun in which country?", "France", "USA", "Brazil", "UK", AnswerLabel.B, Difficulty.Medium),
        Q(MusicHistoryCategory, "An opera is a play in which the story is mostly…?", "Spoken", "Sung", "Danced", "Mimed", AnswerLabel.B, Difficulty.Medium),
    ];

    private static ICard Q(string cat, string question, string a, string b, string c, string d, AnswerLabel correct, Difficulty diff) =>
        MultipleChoiceCard.Create(question, a, b, c, d, correct, diff, cat);
}
