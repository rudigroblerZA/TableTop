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

    private const string Deck = "Spelling Bee";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)

            // ── Easy: common everyday words ──────────────────────────────────────
            .Category(WordCategory)
            .Card("Smile", Prompt("Smile", null), Difficulty.Easy)
            .Card("Climb", Prompt("Climb", null), Difficulty.Easy)
            .Card("Friend", Prompt("Friend", null), Difficulty.Easy)
            .Card("Bright", Prompt("Bright", null), Difficulty.Easy)
            .Card("Strange", Prompt("Strange", null), Difficulty.Easy)
            .Card("Castle", Prompt("Castle", "Hint: silent T!"), Difficulty.Easy)
            .Card("Knife", Prompt("Knife", "Hint: silent K!"), Difficulty.Easy)
            .Card("Caught", Prompt("Caught", null), Difficulty.Easy)
            .Card("Laugh", Prompt("Laugh", null), Difficulty.Easy)
            .Card("Thought", Prompt("Thought", null), Difficulty.Easy)
            .Card("Island", Prompt("Island", "Hint: silent S!"), Difficulty.Easy)
            .Card("Doubt", Prompt("Doubt", "Hint: silent B!"), Difficulty.Easy)
            .Card("Whole", Prompt("Whole", null), Difficulty.Easy)
            .Card("Write", Prompt("Write", "Hint: silent W!"), Difficulty.Easy)
            .Card("Guard", Prompt("Guard", null), Difficulty.Easy)

            // ── Medium: trickier patterns ─────────────────────────────────────────
            .Category(TrickyCategory)
            .Card("Necessary", Prompt("Necessary", "One C, two S!"), Difficulty.Medium)
            .Card("Believe", Prompt("Believe", "I before E except after C!"), Difficulty.Medium)
            .Card("Separate", Prompt("Separate", "There's a RAT in it!"), Difficulty.Medium)
            .Card("Definitely", "Spell the word <b>DEFINITELY</b>. Use it in a sentence.", Difficulty.Medium)
            .Card("Occasion", Prompt("Occasion", "Two C's, one S!"), Difficulty.Medium)
            .Card("Conscience", Prompt("Conscience", null), Difficulty.Medium)
            .Card("Rhythm", Prompt("Rhythm", "No vowels in the main part!"), Difficulty.Medium)
            .Card("Privilege", "Spell the word <b>PRIVILEGE</b>. Use it in a sentence.", Difficulty.Medium)
            .Card("Mischievous", Prompt("Mischievous", "Three syllables: MIS-CHIE-VOUS!"), Difficulty.Medium)
            .Card("Fluorescent", Prompt("Fluorescent", null), Difficulty.Medium)
            .Card("Knowledge", Prompt("Knowledge", "Silent K!"), Difficulty.Medium)
            .Card("Lightning", Prompt("Lightning", "Not 'lightening'!"), Difficulty.Medium)
            .Card("Embarrass", Prompt("Embarrass", "Two R's, two S's!"), Difficulty.Medium)
            .Card("Exaggerate", Prompt("Exaggerate", "Two G's!"), Difficulty.Medium)
            .Card("Environment", Prompt("Environment", "Don't forget the N!"), Difficulty.Medium)

            // ── Hard: subject vocabulary ──────────────────────────────────────────
            .Category(ChallengeCategory)
            .Card("Photosynthesis", "Spell the scientific word <b>PHOTOSYNTHESIS</b> and explain what it means.", Difficulty.Hard)
            .Card("Miscellaneous", Prompt("Miscellaneous", null), Difficulty.Hard)
            .Card("Perseverance", Prompt("Perseverance", null), Difficulty.Hard)
            .Card("Catastrophe", Prompt("Catastrophe", null), Difficulty.Hard)
            .Card("Phenomenon", Prompt("Phenomenon", "PH = F sound!"), Difficulty.Hard)
            .Card("Metamorphosis", "Spell the word <b>METAMORPHOSIS</b> and explain what it means.", Difficulty.Hard)
            .Card("Archaeology", Prompt("Archaeology", null), Difficulty.Hard)
            .Card("Bureaucracy", Prompt("Bureaucracy", null), Difficulty.Hard)
            .Card("Pseudonym", Prompt("Pseudonym", "Silent P!"), Difficulty.Hard)
            .Card("Pneumonia", Prompt("Pneumonia", "Silent P!"), Difficulty.Hard)

            // ── Extreme: championship-level words ────────────────────────────────
            .Card("Onomatopoeia", "Spell the literary term <b>ONOMATOPOEIA</b> and give an example of it.", Difficulty.Extreme)
            .Card("Conscientious", Prompt("Conscientious", null), Difficulty.Extreme)
            .Card("Rhododendron", "Spell the plant name <b>RHODODENDRON</b> and use it in a sentence.", Difficulty.Extreme)
            .Card("Supercilious", Prompt("Supercilious", null), Difficulty.Extreme)
            .Card("Idiosyncrasy", Prompt("Idiosyncrasy", null), Difficulty.Extreme)

            .Build();

    // Hard and Extreme share the Challenge category, so the fluent chain sets
    // it once and both tiers fall under it. `Prompt` builds the standard
    // spell-and-use body; the six cards that ask for something else pass their
    // description literally.
    private static string Prompt(string word, string? hint) =>
        hint is null
            ? $"Spell the word <b>{word.ToUpperInvariant()}</b> and use it in a sentence."
            : $"Spell the word <b>{word.ToUpperInvariant()}</b>. ({hint}) Use it in a sentence.";
}