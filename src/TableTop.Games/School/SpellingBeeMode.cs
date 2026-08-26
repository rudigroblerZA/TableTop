using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Rules;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Rules;
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
    public override string SkipLabel     => "→ Next word";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours => new Dictionary<string, string>
    {
        ["Word"]       = "#26C6DA",
        ["Tricky"]     = "#FFCA28",
        ["Challenge"]  = "#EC407A",
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
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── Easy: common everyday words ──────────────────────────────────────
        W("Smile",      "Smile",       Difficulty.Easy,   "Word",
          "Spell the word <b>SMILE</b> and use it in a sentence."),
        W("Climb",      "Climb",       Difficulty.Easy,   "Word",
          "Spell the word <b>CLIMB</b> and use it in a sentence."),
        W("Friend",     "Friend",      Difficulty.Easy,   "Word",
          "Spell the word <b>FRIEND</b> and use it in a sentence."),
        W("Bright",     "Bright",      Difficulty.Easy,   "Word",
          "Spell the word <b>BRIGHT</b> and use it in a sentence."),
        W("Strange",    "Strange",     Difficulty.Easy,   "Word",
          "Spell the word <b>STRANGE</b> and use it in a sentence."),
        W("Castle",     "Castle",      Difficulty.Easy,   "Word",
          "Spell the word <b>CASTLE</b>. (Hint: silent T!) Use it in a sentence."),
        W("Knife",      "Knife",       Difficulty.Easy,   "Word",
          "Spell the word <b>KNIFE</b>. (Hint: silent K!) Use it in a sentence."),
        W("Caught",     "Caught",      Difficulty.Easy,   "Word",
          "Spell the word <b>CAUGHT</b> and use it in a sentence."),
        W("Laugh",      "Laugh",       Difficulty.Easy,   "Word",
          "Spell the word <b>LAUGH</b> and use it in a sentence."),
        W("Thought",    "Thought",     Difficulty.Easy,   "Word",
          "Spell the word <b>THOUGHT</b> and use it in a sentence."),
        W("Island",     "Island",      Difficulty.Easy,   "Word",
          "Spell the word <b>ISLAND</b>. (Hint: silent S!) Use it in a sentence."),
        W("Doubt",      "Doubt",       Difficulty.Easy,   "Word",
          "Spell the word <b>DOUBT</b>. (Hint: silent B!) Use it in a sentence."),
        W("Whole",      "Whole",       Difficulty.Easy,   "Word",
          "Spell the word <b>WHOLE</b> and use it in a sentence."),
        W("Write",      "Write",       Difficulty.Easy,   "Word",
          "Spell the word <b>WRITE</b>. (Hint: silent W!) Use it in a sentence."),
        W("Guard",      "Guard",       Difficulty.Easy,   "Word",
          "Spell the word <b>GUARD</b> and use it in a sentence."),

        // ── Medium: trickier patterns ─────────────────────────────────────────
        W("Necessary",  "Necessary",   Difficulty.Medium, "Tricky",
          "Spell the word <b>NECESSARY</b>. (One C, two S!) Use it in a sentence."),
        W("Believe",    "Believe",     Difficulty.Medium, "Tricky",
          "Spell the word <b>BELIEVE</b>. (I before E except after C!) Use it in a sentence."),
        W("Separate",   "Separate",    Difficulty.Medium, "Tricky",
          "Spell the word <b>SEPARATE</b>. (There's a RAT in it!) Use it in a sentence."),
        W("Definitely",  "Definitely",  Difficulty.Medium, "Tricky",
          "Spell the word <b>DEFINITELY</b>. Use it in a sentence."),
        W("Occasion",   "Occasion",    Difficulty.Medium, "Tricky",
          "Spell the word <b>OCCASION</b>. (Two C's, one S!) Use it in a sentence."),
        W("Conscience",  "Conscience",  Difficulty.Medium, "Tricky",
          "Spell the word <b>CONSCIENCE</b> and use it in a sentence."),
        W("Rhythm",     "Rhythm",      Difficulty.Medium, "Tricky",
          "Spell the word <b>RHYTHM</b>. (No vowels in the main part!) Use it in a sentence."),
        W("Privilege",  "Privilege",   Difficulty.Medium, "Tricky",
          "Spell the word <b>PRIVILEGE</b>. Use it in a sentence."),
        W("Mischievous","Mischievous", Difficulty.Medium, "Tricky",
          "Spell the word <b>MISCHIEVOUS</b>. (Three syllables: MIS-CHIE-VOUS!) Use it in a sentence."),
        W("Fluorescent","Fluorescent", Difficulty.Medium, "Tricky",
          "Spell the word <b>FLUORESCENT</b> and use it in a sentence."),
        W("Knowledge",  "Knowledge",   Difficulty.Medium, "Tricky",
          "Spell the word <b>KNOWLEDGE</b>. (Silent K!) Use it in a sentence."),
        W("Lightning",  "Lightning",   Difficulty.Medium, "Tricky",
          "Spell the word <b>LIGHTNING</b>. (Not 'lightening'!) Use it in a sentence."),
        W("Embarrass",  "Embarrass",   Difficulty.Medium, "Tricky",
          "Spell the word <b>EMBARRASS</b>. (Two R's, two S's!) Use it in a sentence."),
        W("Exaggerate", "Exaggerate",  Difficulty.Medium, "Tricky",
          "Spell the word <b>EXAGGERATE</b>. (Two G's!) Use it in a sentence."),
        W("Environment","Environment", Difficulty.Medium, "Tricky",
          "Spell the word <b>ENVIRONMENT</b>. (Don't forget the N!) Use it in a sentence."),

        // ── Hard: subject vocabulary ──────────────────────────────────────────
        W("Photosynthesis","Photosynthesis",Difficulty.Hard,"Challenge",
          "Spell the scientific word <b>PHOTOSYNTHESIS</b> and explain what it means."),
        W("Miscellaneous","Miscellaneous",Difficulty.Hard,"Challenge",
          "Spell the word <b>MISCELLANEOUS</b> and use it in a sentence."),
        W("Perseverance","Perseverance", Difficulty.Hard, "Challenge",
          "Spell the word <b>PERSEVERANCE</b> and use it in a sentence."),
        W("Catastrophe","Catastrophe",  Difficulty.Hard, "Challenge",
          "Spell the word <b>CATASTROPHE</b> and use it in a sentence."),
        W("Phenomenon", "Phenomenon",   Difficulty.Hard, "Challenge",
          "Spell the word <b>PHENOMENON</b>. (PH = F sound!) Use it in a sentence."),
        W("Metamorphosis","Metamorphosis",Difficulty.Hard,"Challenge",
          "Spell the word <b>METAMORPHOSIS</b> and explain what it means."),
        W("Archaeology","Archaeology",  Difficulty.Hard, "Challenge",
          "Spell the word <b>ARCHAEOLOGY</b> and use it in a sentence."),
        W("Bureaucracy","Bureaucracy",  Difficulty.Hard, "Challenge",
          "Spell the word <b>BUREAUCRACY</b> and use it in a sentence."),
        W("Pseudonym",  "Pseudonym",    Difficulty.Hard, "Challenge",
          "Spell the word <b>PSEUDONYM</b>. (Silent P!) Use it in a sentence."),
        W("Pneumonia",  "Pneumonia",    Difficulty.Hard, "Challenge",
          "Spell the word <b>PNEUMONIA</b>. (Silent P!) Use it in a sentence."),

        // ── Extreme: championship-level words ────────────────────────────────
        W("Onomatopoeia","Onomatopoeia",Difficulty.Extreme,"Challenge",
          "Spell the literary term <b>ONOMATOPOEIA</b> and give an example of it."),
        W("Conscientious","Conscientious",Difficulty.Extreme,"Challenge",
          "Spell the word <b>CONSCIENTIOUS</b> and use it in a sentence."),
        W("Rhododendron","Rhododendron",Difficulty.Extreme,"Challenge",
          "Spell the plant name <b>RHODODENDRON</b> and use it in a sentence."),
        W("Supercilious","Supercilious",Difficulty.Extreme,"Challenge",
          "Spell the word <b>SUPERCILIOUS</b> and use it in a sentence."),
        W("Idiosyncrasy","Idiosyncrasy",Difficulty.Extreme,"Challenge",
          "Spell the word <b>IDIOSYNCRASY</b> and use it in a sentence."),
    ];

    private static ICard W(string id, string word, Difficulty d, string cat, string desc) =>
        StandardCard.Create(word, desc, d, cat);
}