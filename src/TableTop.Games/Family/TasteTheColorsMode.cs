using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Taste The Colors — a synesthesia game where senses get crossed.
///
/// How to play:
///   1. Read the abstract question aloud.
///   2. Everyone writes their sensory answer: "What does Tuesday taste like?"
///   3. Read answers aloud — the weirdest, most poetic, or most hilarious wins.
///   4. Vote on best answer. Points go to the most creative.
///
/// Questions deliberately mix senses: taste colors, see sounds, smell emotions,
/// touch concepts. There's no wrong answer — only more or less creative. Some people
/// get poetic ("Jealousy tastes like burnt caramel"), some get silly ("Monday smells
/// like old cheese"), some go abstract ("Nostalgia feels like the colour that doesn't
/// exist yet").
///
/// Great for creative minds, writers, artists, and anyone who thinks differently.
/// Embraces that everyone's brain makes different connections. No gatekeeping — just
/// celebrating weird and wonderful thinking.
/// </summary>
public sealed class TasteTheColorsMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name        => "Taste The Colors";
    /// <inheritdoc />
    public override string Description =>
        "What does Tuesday taste like? Cross your senses. Be creative. Be weird.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Answered";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel     => "Pass";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Time"]       = "#42A5F5",
            ["Emotion"]    = "#EC407A",
            ["Concept"]    = "#AB47BC",
            ["Abstract"]   = "#FFA726",
            ["Sensory"]    = "#66BB6A",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        TasteTheColorsCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => TasteTheColorsCardBank.All;
}

/// <summary>Built-in card bank for Taste The Colors. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class TasteTheColorsCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── TIME ──────────────────────────────────────────────────────────────
        T("Time", "What does Monday taste like?", Difficulty.Easy),
        T("Time", "What colour is Thursday?", Difficulty.Easy),
        T("Time", "What does midnight sound like?", Difficulty.Medium),
        T("Time", "If you could touch tomorrow, what texture would it be?", Difficulty.Hard),
        T("Time", "What does the last day of summer smell like?", Difficulty.Medium),
        T("Time", "What does waiting taste like?", Difficulty.Medium),

        // ── EMOTION ───────────────────────────────────────────────────────────
        T("Emotion", "What colour is jealousy?", Difficulty.Easy),
        T("Emotion", "What does happiness taste like?", Difficulty.Easy),
        T("Emotion", "What does anxiety sound like?", Difficulty.Medium),
        T("Emotion", "If you could taste loneliness, what would it be?", Difficulty.Hard),
        T("Emotion", "What does love smell like?", Difficulty.Medium),
        T("Emotion", "What texture is nostalgia?", Difficulty.Hard),
        T("Emotion", "What does embarrassment taste like?", Difficulty.Medium),

        // ── CONCEPT ───────────────────────────────────────────────────────────
        T("Concept", "What does success sound like?", Difficulty.Medium),
        T("Concept", "What colour is freedom?", Difficulty.Medium),
        T("Concept", "If you could taste knowledge, what would it be?", Difficulty.Hard),
        T("Concept", "What does chaos smell like?", Difficulty.Medium),
        T("Concept", "What texture is an idea?", Difficulty.Hard),
        T("Concept", "What does gravity taste like?", Difficulty.Hard),

        // ── ABSTRACT ──────────────────────────────────────────────────────────
        T("Abstract", "What colour is the number 7?", Difficulty.Hard),
        T("Abstract", "If you could touch a conversation, what would it feel like?", Difficulty.Hard),
        T("Abstract", "What does the letter 'Q' taste like?", Difficulty.Hard),
        T("Abstract", "What colour is a paradox?", Difficulty.Hard),
        T("Abstract", "If silence had a sound, what would it be?", Difficulty.Hard),
        T("Abstract", "What does the colour of an echo taste like?", Difficulty.Hard),

        // ── SENSORY ───────────────────────────────────────────────────────────
        T("Sensory", "What does sunlight taste like?", Difficulty.Easy),
        T("Sensory", "What colour is rain?", Difficulty.Easy),
        T("Sensory", "If you could smell music, what would it smell like?", Difficulty.Medium),
        T("Sensory", "What does the ocean sound like if you could taste it?", Difficulty.Hard),
        T("Sensory", "What texture is the colour blue?", Difficulty.Medium),
        T("Sensory", "If you could see sound, what would it look like?", Difficulty.Hard),
    ];

    private static ICard T(string category, string question, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>Cross your senses:</b>\n\n" +
            question + "\n\n" +
            "<b>Write your answer.</b> Be poetic, be weird, be creative.\n\n" +
            "Everyone reads theirs aloud. Vote on the most creative, funniest, or most beautiful.",
            d, category);
}
