using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Constraint Master — creative writing and speaking under arbitrary rules.
///
/// How to play:
///   1. Read the constraint and the prompt aloud.
///   2. Everyone has 90 seconds to write (or speak) their response following the constraint.
///   3. Read aloud. Vote on best: funniest, most creative, most impressive.
///   4. Points go to the winner each round.
///
/// Constraints are ridiculous: "Describe your week using only 3-word sentences", "Tell a story
/// where every word starts with the same letter", "Write dialogue using no vowels except 'e'".
/// This is NOT easy. It's a puzzle wrapped in a writing exercise wrapped in a laugh.
///
/// Great for word nerds, writers, and anyone who likes a challenge. Teaches creative problem-solving
/// by forcing you to say normal things in abnormal ways. No right answer — just difficulty levels
/// of "how bad is your constraint going to make this?"
/// </summary>
public sealed class ConstraintMasterMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Constraint Master";
    /// <inheritdoc />
    public override string Description =>
        "Write/speak the prompt following this weird rule. 90 seconds. Go.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Finished";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [ConstraintMasterCardBank.GrammarCategory] = "#42A5F5",
            [ConstraintMasterCardBank.LetterCategory] = "#66BB6A",
            [ConstraintMasterCardBank.SoundCategory] = "#EC407A",
            [ConstraintMasterCardBank.StructureCategory] = "#AB47BC",
            [ConstraintMasterCardBank.ImpossibleCategory] = "#EF5350",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        ConstraintMasterCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => ConstraintMasterCardBank.All;
}

/// <summary>
/// Built-in card bank for Constraint Master. Cards are also available as JSON
/// in <c>Data/Json/</c>.
///
/// Migrated to <see cref="CardDeckBuilder"/> as the demonstration case for that
/// builder — chosen because this mode is JSON-first
/// (<c>constraint-master.deck.json</c> exists), so the change here only
/// affects the fallback path used when that file is missing. It also fixes a
/// real gap the fallback had: the old local <c>C(...)</c> helper called
/// <see cref="StandardCard.Create"/>, which assigns a random id every process
/// start, so a session resumed after JSON went missing could not find the
/// cards it had saved. The builder's ids are deterministic instead.
///
/// Card titles are each card's category name, unchanged from the original —
/// this migration is a faithful port of existing content, not a content edit.
/// </summary>
public static class ConstraintMasterCardBank
{
    internal const string GrammarCategory = "Grammar";
    internal const string LetterCategory = "Letter";
    internal const string SoundCategory = "Sound";
    internal const string StructureCategory = "Structure";
    internal const string ImpossibleCategory = "Impossible";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build()
    {
        var b = CardDeckBuilder.For("Constraint Master");

        b.Category(GrammarCategory)
            .Card(GrammarCategory, Prompt("Every sentence must be exactly 3 words.", "Describe your week."), Difficulty.Medium)
            .Card(GrammarCategory, Prompt("No word can repeat (use each word only once).", "Tell a story about a grocery trip."), Difficulty.Hard)
            .Card(GrammarCategory, Prompt("Write only using nouns and verbs (no adjectives, adverbs, or 'the').", "Describe your morning."), Difficulty.Medium)
            .Card(GrammarCategory, Prompt("Every sentence must be a question.", "Explain what happened at the party."), Difficulty.Medium);

        b.Category(LetterCategory)
            .Card(LetterCategory, Prompt("Every word must start with the letter 'S'.", "Tell a story."), Difficulty.Hard)
            .Card(LetterCategory, Prompt("Write a sentence where each word starts with consecutive letters (A, B, C, D, etc).", "Make it funny."), Difficulty.Hard)
            .Card(LetterCategory, Prompt("No word can contain the letter 'E'.", "Describe your favourite food."), Difficulty.Hard)
            .Card(LetterCategory, Prompt("Every word must be a palindrome or have a repeated letter pattern.", "Say anything at all."), Difficulty.Hard);

        b.Category(SoundCategory)
            .Card(SoundCategory, Prompt("Every word must rhyme with the previous word.", "Describe a typical day."), Difficulty.Hard)
            .Card(SoundCategory, Prompt("Use only words that start with 'Sh' and 'Ch' sounds.", "Tell a story."), Difficulty.Hard)
            .Card(SoundCategory, Prompt("Alternate between short words and long words (1 syllable, 4+ syllables, 1, 4+).", "Explain something."), Difficulty.Medium);

        b.Category(StructureCategory)
            .Card(StructureCategory, Prompt("Write backwards — last sentence first, first sentence last.", "Tell a short story."), Difficulty.Hard)
            .Card(StructureCategory, Prompt("Your entire response must be one single sentence.", "Describe a full day."), Difficulty.Medium)
            .Card(StructureCategory, Prompt("Write your response as a list of exactly 5 items.", "Explain how to survive anything."), Difficulty.Easy)
            .Card(StructureCategory, Prompt("First word of each sentence uses the next letter of the alphabet.", "Tell a story."), Difficulty.Hard);

        b.Category(ImpossibleCategory)
            .Card(ImpossibleCategory, Prompt("Use only vowels (A, E, I, O, U) — no consonants at all.", "Say anything coherent."), Difficulty.Hard)
            .Card(ImpossibleCategory, Prompt("Every word must be longer than 5 letters AND shorter than 3 letters.", "Describe a moment."), Difficulty.Hard)
            .Card(ImpossibleCategory, Prompt("Write about love using only angry words.", "Make it work somehow."), Difficulty.Hard)
            .Card(ImpossibleCategory, Prompt("Describe someone without using any descriptive words.", "Make it understandable anyway."), Difficulty.Hard);

        return b.Build();
    }

    private static string Prompt(string constraint, string prompt) =>
        "<b>90-SECOND CONSTRAINT CHALLENGE</b>\n\n" +
        $"Constraint: {constraint}\n\nPrompt: {prompt}\n\n" +
        "Write or speak your response. Follow the constraint exactly.\n\n" +
        "Vote on best: funniest, most creative, or most impressive.";
}
