using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Draw It — Pictionary for the family. One person draws (no words, no letters,
/// no numbers), everyone else races to guess. Distinct from Useless
/// Superpowers, where you draw a thing and then pitch it — here the whole point
/// is that the others GUESS what you drew.
///
/// How to play:
///   1. Grab any paper and a pen. The drawer reads a card silently.
///   2. Flip it face-down and draw on "go" — no letters, numbers, words, or
///      talking. Gestures at your own drawing are allowed and encouraged.
///   3. Everyone shouts guesses. First correct guess scores for the guesser
///      AND the drawer. 60 seconds a card.
///   4. Pass the pen. The answer is on the back, so no arguing.
///
/// Objects for the little ones, actions and idioms for the challenge — an
/// idiom like "raining cats and dogs" is where the family art gets gloriously
/// bad.
/// </summary>
public sealed class DrawItMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Draw It";
    /// <inheritdoc />
    public override string Description =>
        "Pictionary for the family — draw it, no words or letters, everyone guesses. First guess scores. Answer's on the back.";

    /// <summary>Label for a guessed card.</summary>
    public override string CompleteLabel => "Guessed!";
    /// <summary>Label for a card nobody got.</summary>
    public override string SkipLabel => "Nobody got it";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [DrawItCardBank.ObjectsCategory] = "#66BB6A",
            [DrawItCardBank.AnimalsNatureCategory] = "#26A69A",
            [DrawItCardBank.ActionsCategory] = "#42A5F5",
            [DrawItCardBank.PlacesCategory] = "#FFA726",
            [DrawItCardBank.IdiomsCategory] = "#EF5350",
        };

    /// <summary>Harder things to draw score more.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in Draw It card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        DrawItCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => DrawItCardBank.All;
}

/// <summary>Built-in card bank for Draw It.</summary>
public static class DrawItCardBank
{
    internal const string ObjectsCategory = "Objects";
    internal const string AnimalsNatureCategory = "Animals & Nature";
    internal const string ActionsCategory = "Actions";
    internal const string PlacesCategory = "Places";
    internal const string IdiomsCategory = "Idioms";

    /// <summary>All cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── OBJECTS — easiest, for the youngest artists ──────────────────────
        D(ObjectsCategory, "A house", Difficulty.Easy),
        D(ObjectsCategory, "An umbrella", Difficulty.Easy),
        D(ObjectsCategory, "A birthday cake", Difficulty.Easy),
        D(ObjectsCategory, "A pair of glasses", Difficulty.Medium),
        D(ObjectsCategory, "A washing machine", Difficulty.Medium),

        // ── ANIMALS & NATURE ─────────────────────────────────────────────────
        D(AnimalsNatureCategory, "A cat", Difficulty.Easy),
        D(AnimalsNatureCategory, "A rainbow", Difficulty.Easy),
        D(AnimalsNatureCategory, "An octopus", Difficulty.Medium),
        D(AnimalsNatureCategory, "A volcano erupting", Difficulty.Medium),
        D(AnimalsNatureCategory, "A hedgehog", Difficulty.Hard),

        // ── ACTIONS — harder to draw without words ───────────────────────────
        D(ActionsCategory, "Sleeping", Difficulty.Medium),
        D(ActionsCategory, "Juggling", Difficulty.Medium),
        D(ActionsCategory, "Sneezing", Difficulty.Hard),
        D(ActionsCategory, "Winning a race", Difficulty.Hard),

        // ── PLACES ───────────────────────────────────────────────────────────
        D(PlacesCategory, "The beach", Difficulty.Easy),
        D(PlacesCategory, "A farm", Difficulty.Medium),
        D(PlacesCategory, "An airport", Difficulty.Hard),
        D(PlacesCategory, "A haunted house", Difficulty.Hard),

        // ── IDIOMS — the gloriously bad ones ─────────────────────────────────
        D(IdiomsCategory, "Raining cats and dogs", Difficulty.Hard),
        D(IdiomsCategory, "A piece of cake", Difficulty.Hard),
        D(IdiomsCategory, "Butterflies in your stomach", Difficulty.Extreme),
        D(IdiomsCategory, "The elephant in the room", Difficulty.Extreme),
        D(IdiomsCategory, "Barking up the wrong tree", Difficulty.Extreme),
        D(IdiomsCategory, "When pigs fly", Difficulty.Extreme),
    ];

    private static ICard D(string category, string answer, Difficulty d) =>
        StandardCard.Create(
            category + " sketch",
            "<b>✏️ DRAW IT — " + category.ToUpperInvariant() + "</b>\n\n" +
            "<i>Read silently, then draw — no words, letters, or numbers. 60 seconds. First to guess scores with you.</i>\n\n" +
            "Answer: " + answer,
            d, category);
}
