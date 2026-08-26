using TableTop.Core.Abstractions.Cards;

namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Player-configurable options that genuinely change how a session plays —
/// as opposed to display-only settings (font size, badge visibility) which
/// stay entirely in each UI's own preferences store.
///
/// This is the missing link that was flagged when MAUI's gameplay screen was
/// rewired to the real engine: settings like difficulty range and shuffle
/// existed in the UI's preferences but had no way to reach the controller
/// that actually builds the deck. Passing a <see cref="GameplayOptions"/> to
/// <c>IControllerFactory.CreateAsync</c>
/// closes that gap for every UI at once, since they all go through the same
/// factory.
///
/// Entirely optional: omitting it (or passing null) preserves the original
/// behaviour — full deck, always shuffled — so no existing caller breaks.
/// </summary>
public sealed record GameplayOptions
{
    /// <summary>Shuffle the deck before dealing. Default true.</summary>
    public bool ShuffleDeck { get; init; } = true;

    /// <summary>Lowest difficulty tier to include. Default <see cref="Difficulty.Easy"/>.</summary>
    public Difficulty MinDifficulty { get; init; } = Difficulty.Easy;

    /// <summary>Highest difficulty tier to include. Default <see cref="Difficulty.Extreme"/>.</summary>
    public Difficulty MaxDifficulty { get; init; } = Difficulty.Extreme;

    /// <summary>
    /// Caps the session to roughly this many cards per player (applied to the
    /// shuffled/filtered pool, so it's a random sample, not just "the first
    /// N"). Null means no cap — deal the whole filtered deck.
    /// </summary>
    public int? CardsPerPlayer { get; init; }

    /// <summary>The unrestricted default: full deck, shuffled, no cap.</summary>
    public static GameplayOptions Default { get; } = new();

    /// <summary>
    /// True when every filter is at its widest setting — lets callers skip
    /// building a filter predicate entirely for the common case.
    /// </summary>
    public bool IsUnrestricted =>
        MinDifficulty == Difficulty.Easy && MaxDifficulty == Difficulty.Extreme && CardsPerPlayer is null;
}
