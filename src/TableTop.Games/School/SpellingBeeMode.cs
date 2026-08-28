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

/// <summary>45 spelling cards across four difficulty tiers.</summary>
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
        W("Smile", Difficulty.Easy),
        W("Climb", Difficulty.Easy),
        W("Friend", Difficulty.Easy),
        W("Bright", Difficulty.Easy),
        W("Strange", Difficulty.Easy),
        W("Castle", Difficulty.Easy, hint: "Hint: silent T!"),
        W("Knife", Difficulty.Easy, hint: "Hint: silent K!"),
        W("Caught", Difficulty.Easy),
        W("Laugh", Difficulty.Easy),
        W("Thought", Difficulty.Easy),
        W("Island", Difficulty.Easy, hint: "Hint: silent S!"),
        W("Doubt", Difficulty.Easy, hint: "Hint: silent B!"),
        W("Whole", Difficulty.Easy),
        W("Write", Difficulty.Easy, hint: "Hint: silent W!"),
        W("Guard", Difficulty.Easy),

        // ── Medium: trickier patterns ─────────────────────────────────────────
        W("Necessary", Difficulty.Medium, hint: "One C, two S!"),
        W("Believe", Difficulty.Medium, hint: "I before E except after C!"),
        W("Separate", Difficulty.Medium, hint: "There's a RAT in it!"),
        W("Definitely", Difficulty.Medium,
          desc: "Spell the word <b>DEFINITELY</b>. Use it in a sentence."),
        W("Occasion", Difficulty.Medium, hint: "Two C's, one S!"),
        W("Conscience", Difficulty.Medium),
        W("Rhythm", Difficulty.Medium, hint: "No vowels in the main part!"),
        W("Privilege", Difficulty.Medium,
          desc: "Spell the word <b>PRIVILEGE</b>. Use it in a sentence."),
        W("Mischievous", Difficulty.Medium, hint: "Three syllables: MIS-CHIE-VOUS!"),
        W("Fluorescent", Difficulty.Medium),
        W("Knowledge", Difficulty.Medium, hint: "Silent K!"),
        W("Lightning", Difficulty.Medium, hint: "Not 'lightening'!"),
        W("Embarrass", Difficulty.Medium, hint: "Two R's, two S's!"),
        W("Exaggerate", Difficulty.Medium, hint: "Two G's!"),
        W("Environment", Difficulty.Medium, hint: "Don't forget the N!"),

        // ── Hard: subject vocabulary ──────────────────────────────────────────
        W("Photosynthesis", Difficulty.Hard,
          desc: "Spell the scientific word <b>PHOTOSYNTHESIS</b> and explain what it means."),
        W("Miscellaneous", Difficulty.Hard),
        W("Perseverance", Difficulty.Hard),
        W("Catastrophe", Difficulty.Hard),
        W("Phenomenon", Difficulty.Hard, hint: "PH = F sound!"),
        W("Metamorphosis", Difficulty.Hard,
          desc: "Spell the word <b>METAMORPHOSIS</b> and explain what it means."),
        W("Archaeology", Difficulty.Hard),
        W("Bureaucracy", Difficulty.Hard),
        W("Pseudonym", Difficulty.Hard, hint: "Silent P!"),
        W("Pneumonia", Difficulty.Hard, hint: "Silent P!"),

        // ── Extreme: championship-level words ────────────────────────────────
        W("Onomatopoeia", Difficulty.Extreme,
          desc: "Spell the literary term <b>ONOMATOPOEIA</b> and give an example of it."),
        W("Conscientious", Difficulty.Extreme),
        W("Rhododendron", Difficulty.Extreme,
          desc: "Spell the plant name <b>RHODODENDRON</b> and use it in a sentence."),
        W("Supercilious", Difficulty.Extreme),
        W("Idiosyncrasy", Difficulty.Extreme),
    ];

    // Three defaults, for the three things every card was retyping.
    //   cat  — each difficulty tier is a category (Hard and Extreme share
    //          Challenge), so it derives from `d` unless a card passes its own.
    //   hint — the parenthesised aside in "…</b>. (Silent K!) Use it in a
    //          sentence." Pass the aside; the sentence around it is built here.
    //   desc — the escape hatch for the six cards that ask for something other
    //          than spell-and-use-in-a-sentence. Wins over `hint` when both are
    //          given, since it replaces the whole prompt rather than filling a
    //          slot in it.
    private static ICard W(string word, Difficulty d, string? hint = null,
                           string? cat = null, string? desc = null) =>
        StandardCard.Create(word, desc ?? Prompt(word, hint), d, cat ?? CategoryFor(d));

    private static string Prompt(string word, string? hint) =>
        hint is null
            ? $"Spell the word <b>{word.ToUpperInvariant()}</b> and use it in a sentence."
            : $"Spell the word <b>{word.ToUpperInvariant()}</b>. ({hint}) Use it in a sentence.";

    private static string CategoryFor(Difficulty d) => d switch
    {
        Difficulty.Easy => WordCategory,
        Difficulty.Medium => TrickyCategory,
        _ => ChallengeCategory,
    };
}