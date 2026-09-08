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

    private const string Deck = "Draw It";

    /// <summary>All cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)
        // ── OBJECTS — easiest, for the youngest artists ──────────────────────
            .Category(ObjectsCategory)
            .Card(ObjectsCategory + " sketch", Body(ObjectsCategory, "A house"), Difficulty.Easy)
            .Card(ObjectsCategory + " sketch", Body(ObjectsCategory, "An umbrella"), Difficulty.Easy)
            .Card(ObjectsCategory + " sketch", Body(ObjectsCategory, "A birthday cake"), Difficulty.Easy)
            .Card(ObjectsCategory + " sketch", Body(ObjectsCategory, "A pair of glasses"), Difficulty.Medium)
            .Card(ObjectsCategory + " sketch", Body(ObjectsCategory, "A washing machine"), Difficulty.Medium)

        // ── ANIMALS & NATURE ─────────────────────────────────────────────────
            .Category(AnimalsNatureCategory)
            .Card(AnimalsNatureCategory + " sketch", Body(AnimalsNatureCategory, "A cat"), Difficulty.Easy)
            .Card(AnimalsNatureCategory + " sketch", Body(AnimalsNatureCategory, "A rainbow"), Difficulty.Easy)
            .Card(AnimalsNatureCategory + " sketch", Body(AnimalsNatureCategory, "An octopus"), Difficulty.Medium)
            .Card(AnimalsNatureCategory + " sketch", Body(AnimalsNatureCategory, "A volcano erupting"), Difficulty.Medium)
            .Card(AnimalsNatureCategory + " sketch", Body(AnimalsNatureCategory, "A hedgehog"), Difficulty.Hard)

        // ── ACTIONS — harder to draw without words ───────────────────────────
            .Category(ActionsCategory)
            .Card(ActionsCategory + " sketch", Body(ActionsCategory, "Sleeping"), Difficulty.Medium)
            .Card(ActionsCategory + " sketch", Body(ActionsCategory, "Juggling"), Difficulty.Medium)
            .Card(ActionsCategory + " sketch", Body(ActionsCategory, "Sneezing"), Difficulty.Hard)
            .Card(ActionsCategory + " sketch", Body(ActionsCategory, "Winning a race"), Difficulty.Hard)

        // ── PLACES ───────────────────────────────────────────────────────────
            .Category(PlacesCategory)
            .Card(PlacesCategory + " sketch", Body(PlacesCategory, "The beach"), Difficulty.Easy)
            .Card(PlacesCategory + " sketch", Body(PlacesCategory, "A farm"), Difficulty.Medium)
            .Card(PlacesCategory + " sketch", Body(PlacesCategory, "An airport"), Difficulty.Hard)
            .Card(PlacesCategory + " sketch", Body(PlacesCategory, "A haunted house"), Difficulty.Hard)

        // ── IDIOMS — the gloriously bad ones ─────────────────────────────────
            .Category(IdiomsCategory)
            .Card(IdiomsCategory + " sketch", Body(IdiomsCategory, "Raining cats and dogs"), Difficulty.Hard)
            .Card(IdiomsCategory + " sketch", Body(IdiomsCategory, "A piece of cake"), Difficulty.Hard)
            .Card(IdiomsCategory + " sketch", Body(IdiomsCategory, "Butterflies in your stomach"), Difficulty.Extreme)
            .Card(IdiomsCategory + " sketch", Body(IdiomsCategory, "The elephant in the room"), Difficulty.Extreme)
            .Card(IdiomsCategory + " sketch", Body(IdiomsCategory, "Barking up the wrong tree"), Difficulty.Extreme)
            .Card(IdiomsCategory + " sketch", Body(IdiomsCategory, "When pigs fly"), Difficulty.Extreme)

            .Build();

    private static string Body(string category, string answer) =>
        "<b>✏️ DRAW IT — " + category.ToUpperInvariant() + "</b>\n\n" +
        "<i>Read silently, then draw — no words, letters, or numbers. 60 seconds. First to guess scores with you.</i>\n\n" +
        "Answer: " + answer;
}
