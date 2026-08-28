using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Spelling Bee — Grade 6 card-per-turn word challenge.
///
/// Each card presents a word. The active player must:
///   1. Spell the word aloud.
///   2. Use it correctly in a sentence.
///
/// The group (or teacher) judges whether both parts were correct.
/// Scoring: 2 pts for spelling + sentence; 1 pt for spelling only; 0 pts if both wrong.
///
/// Difficulty tiers match word complexity:
///   Easy    — 4–5 letter common words (jump, smile, pretty)
///   Medium  — 6–8 letter words with tricky patterns (necessary, believe)
///   Hard    — 9+ letter and subject-specific vocabulary (miscellaneous, exaggerate)
///   Extreme — challenge words (onomatopoeia, conscientious, rhododendron)
/// </summary>
public sealed class SpellingBeeMode : BaseGameModeDefinition, IFlowAwareMode
{
    /// <inheritdoc />
    public override string Name => "Spelling Bee";
    /// <inheritdoc />
    public override string Description =>
        "Spell the word and use it in a sentence. Grade 6 vocabulary — from everyday words to real challenges.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ Both correct (+2)";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "→ Next word";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours => new Dictionary<string, string>
    {
        [SpellingBeeCardBank.WordCategory] = "#26C6DA",
        [SpellingBeeCardBank.TrickyCategory] = "#FFCA28",
        [SpellingBeeCardBank.ChallengeCategory] = "#EC407A",
    };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 2);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        SpellingBeeCardBank.All;

    /// <summary>Exposes the card bank for testing without a player list.</summary>
    public static IReadOnlyList<ICard> GetCards() => SpellingBeeCardBank.All;
}

/// <summary>120 spelling cards across four difficulty tiers.</summary>
public static class SpellingBeeCardBank
{
    internal const string WordCategory = "Word";
    internal const string TrickyCategory = "Tricky";
    internal const string ChallengeCategory = "Challenge";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── Easy: common everyday words ──────────────────────────────────────
        W("Smile",      Difficulty.Easy,   WordCategory,
          "Spell the word <b>SMILE</b> and use it in a sentence."),
        W("Climb",      Difficulty.Easy,   WordCategory,
          "Spell the word <b>CLIMB</b> and use it in a sentence."),
        W("Friend",     Difficulty.Easy,   WordCategory,
          "Spell the word <b>FRIEND</b> and use it in a sentence."),
        W("Bright",     Difficulty.Easy,   WordCategory,
          "Spell the word <b>BRIGHT</b> and use it in a sentence."),
        W("Strange",    Difficulty.Easy,   WordCategory,
          "Spell the word <b>STRANGE</b> and use it in a sentence."),
        W("Castle",     Difficulty.Easy,   WordCategory,
          "Spell the word <b>CASTLE</b>. (Hint: silent T!) Use it in a sentence."),
        W("Knife",      Difficulty.Easy,   WordCategory,
          "Spell the word <b>KNIFE</b>. (Hint: silent K!) Use it in a sentence."),
        W("Caught",     Difficulty.Easy,   WordCategory,
          "Spell the word <b>CAUGHT</b> and use it in a sentence."),
        W("Laugh",      Difficulty.Easy,   WordCategory,
          "Spell the word <b>LAUGH</b> and use it in a sentence."),
        W("Thought",    Difficulty.Easy,   WordCategory,
          "Spell the word <b>THOUGHT</b> and use it in a sentence."),
        W("Island",     Difficulty.Easy,   WordCategory,
          "Spell the word <b>ISLAND</b>. (Hint: silent S!) Use it in a sentence."),
        W("Doubt",      Difficulty.Easy,   WordCategory,
          "Spell the word <b>DOUBT</b>. (Hint: silent B!) Use it in a sentence."),
        W("Whole",      Difficulty.Easy,   WordCategory,
          "Spell the word <b>WHOLE</b> and use it in a sentence."),
        W("Write",      Difficulty.Easy,   WordCategory,
          "Spell the word <b>WRITE</b>. (Hint: silent W!) Use it in a sentence."),
        W("Guard",      Difficulty.Easy,   WordCategory,
          "Spell the word <b>GUARD</b> and use it in a sentence."),

        // ── Medium: trickier patterns ─────────────────────────────────────────
        W("Necessary",   Difficulty.Medium, TrickyCategory,
          "Spell the word <b>NECESSARY</b>. (One C, two S!) Use it in a sentence."),
        W("Believe",     Difficulty.Medium, TrickyCategory,
          "Spell the word <b>BELIEVE</b>. (I before E except after C!) Use it in a sentence."),
        W("Separate",    Difficulty.Medium, TrickyCategory,
          "Spell the word <b>SEPARATE</b>. (There's a RAT in it!) Use it in a sentence."),
        W("Definitely",  Difficulty.Medium, TrickyCategory,
          "Spell the word <b>DEFINITELY</b>. Use it in a sentence."),
        W("Occasion",    Difficulty.Medium, TrickyCategory,
          "Spell the word <b>OCCASION</b>. (Two C's, one S!) Use it in a sentence."),
        W("Conscience",  Difficulty.Medium, TrickyCategory,
          "Spell the word <b>CONSCIENCE</b> and use it in a sentence."),
        W("Rhythm",      Difficulty.Medium, TrickyCategory,
          "Spell the word <b>RHYTHM</b>. (No vowels in the main part!) Use it in a sentence."),
        W("Privilege",   Difficulty.Medium, TrickyCategory,
          "Spell the word <b>PRIVILEGE</b>. Use it in a sentence."),
        W("Mischievous", Difficulty.Medium, TrickyCategory,
          "Spell the word <b>MISCHIEVOUS</b>. (Three syllables: MIS-CHIE-VOUS!) Use it in a sentence."),
        W("Fluorescent", Difficulty.Medium, TrickyCategory,
          "Spell the word <b>FLUORESCENT</b> and use it in a sentence."),
        W("Knowledge",   Difficulty.Medium, TrickyCategory,
          "Spell the word <b>KNOWLEDGE</b>. (Silent K!) Use it in a sentence."),
        W("Lightning",   Difficulty.Medium, TrickyCategory,
          "Spell the word <b>LIGHTNING</b>. (Not 'lightening'!) Use it in a sentence."),
        W("Embarrass",   Difficulty.Medium, TrickyCategory,
          "Spell the word <b>EMBARRASS</b>. (Two R's, two S's!) Use it in a sentence."),
        W("Exaggerate",  Difficulty.Medium, TrickyCategory,
          "Spell the word <b>EXAGGERATE</b>. (Two G's!) Use it in a sentence."),
        W("Environment", Difficulty.Medium, TrickyCategory,
          "Spell the word <b>ENVIRONMENT</b>. (Don't forget the N!) Use it in a sentence."),

        // ── Hard: subject vocabulary ──────────────────────────────────────────
        W("Photosynthesis", Difficulty.Hard, ChallengeCategory,
          "Spell the scientific word <b>PHOTOSYNTHESIS</b> and explain what it means."),
        W("Miscellaneous",  Difficulty.Hard, ChallengeCategory,
          "Spell the word <b>MISCELLANEOUS</b> and use it in a sentence."),
        W("Perseverance",   Difficulty.Hard, ChallengeCategory,
          "Spell the word <b>PERSEVERANCE</b> and use it in a sentence."),
        W("Catastrophe",    Difficulty.Hard, ChallengeCategory,
          "Spell the word <b>CATASTROPHE</b> and use it in a sentence."),
        W("Phenomenon",     Difficulty.Hard, ChallengeCategory,
          "Spell the word <b>PHENOMENON</b>. (PH = F sound!) Use it in a sentence."),
        W("Metamorphosis",  Difficulty.Hard, ChallengeCategory,
          "Spell the word <b>METAMORPHOSIS</b> and explain what it means."),
        W("Archaeology",    Difficulty.Hard, ChallengeCategory,
          "Spell the word <b>ARCHAEOLOGY</b> and use it in a sentence."),
        W("Bureaucracy",    Difficulty.Hard, ChallengeCategory,
          "Spell the word <b>BUREAUCRACY</b> and use it in a sentence."),
        W("Pseudonym",      Difficulty.Hard, ChallengeCategory,
          "Spell the word <b>PSEUDONYM</b>. (Silent P!) Use it in a sentence."),
        W("Pneumonia",      Difficulty.Hard, ChallengeCategory,
          "Spell the word <b>PNEUMONIA</b>. (Silent P!) Use it in a sentence."),

        // ── Extreme: championship-level words ────────────────────────────────
        W("Onomatopoeia",   Difficulty.Extreme, ChallengeCategory,
          "Spell the literary term <b>ONOMATOPOEIA</b> and give an example of it."),
        W("Conscientious",  Difficulty.Extreme, ChallengeCategory,
          "Spell the word <b>CONSCIENTIOUS</b> and use it in a sentence."),
        W("Rhododendron",   Difficulty.Extreme, ChallengeCategory,
          "Spell the plant name <b>RHODODENDRON</b> and use it in a sentence."),
        W("Supercilious",   Difficulty.Extreme, ChallengeCategory,
          "Spell the word <b>SUPERCILIOUS</b> and use it in a sentence."),
        W("Idiosyncrasy",   Difficulty.Extreme, ChallengeCategory,
          "Spell the word <b>IDIOSYNCRASY</b> and use it in a sentence."),
    ];

    private static ICard W(string word, Difficulty d, string cat, string desc) =>
        StandardCard.Create(word, desc, d, cat);
}