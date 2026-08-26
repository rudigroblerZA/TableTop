using TableTop.Core.Abstractions.Decks;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Progression;
using TableTop.Core.Abstractions.Rules;
using TableTop.Core.Abstractions.Scoring;

namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Creates configured game instances.
/// The factory depends on abstractions, not concrete types (DIP).
/// </summary>
public interface IGameFactory
{
    /// <summary>Creates a new game session from the supplied configuration.</summary>
    IGame Create(IGameConfiguration configuration);
}

/// <summary>
/// Carries all settings needed to bootstrap a game session.
/// </summary>
public interface IGameConfiguration
{
    /// <summary>The deck to play with.</summary>
    IDeck Deck { get; }

    /// <summary>Players registered for this session.</summary>
    IReadOnlyList<IPlayer> Players { get; }

    /// <summary>Strategy that determines card selection order.</summary>
    IProgressionStrategy ProgressionStrategy { get; }

    /// <summary>Strategy that calculates scores after each turn.</summary>
    IScoringStrategy ScoringStrategy { get; }

    /// <summary>Rules applied during card selection and turn evaluation.</summary>
    IReadOnlyList<IRule> Rules { get; }

    /// <summary>Maximum rounds before the game ends automatically. Null means unlimited.</summary>
    /// <summary>
    /// Number of completed playable rounds after which the game ends.
    /// A "round" is defined as every active player (snapshotted at round start) taking one turn.
    /// Null means play until the deck is exhausted.
    /// </summary>
    int? MaxRounds { get; }

    /// <summary>
    /// How to score break, reward, and inspiration cards (special cards that auto-complete).
    /// Default: <see cref="SpecialCardScoringPolicy.NoScore"/>.
    /// </summary>
    SpecialCardScoringPolicy SpecialCardScoringPolicy { get; }

    /// <summary>
    /// Fixed bonus score applied to special cards when policy is <see cref="SpecialCardScoringPolicy.FixedBonus"/>.
    /// </summary>
    int SpecialCardBonusScore { get; }
    /// <summary>
    /// Categories held back until every other card has been played.
    ///
    /// Defaults to empty, so existing implementations are unaffected. Deck
    /// order alone cannot keep a card last, because progression strategies peek
    /// across the whole deck and may pick one early.
    /// </summary>
    IReadOnlyList<string> DeferredCategories => [];

}

/// <summary>
/// Governs how break, reward, and inspiration cards contribute to player scores.
/// </summary>
public enum SpecialCardScoringPolicy
{
    /// <summary>Special cards award no score regardless of scoring strategy. Default.</summary>
    NoScore,

    /// <summary>Special cards award a fixed bonus defined by <see cref="IGameConfiguration.SpecialCardBonusScore"/>.</summary>
    FixedBonus,

    /// <summary>Special cards are scored through the mode's normal scoring strategy.</summary>
    ModeDefined,
}
