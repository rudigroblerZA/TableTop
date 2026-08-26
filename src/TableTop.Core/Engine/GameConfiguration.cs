using TableTop.Core.Abstractions.Decks;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Progression;
using TableTop.Core.Abstractions.Rules;
using TableTop.Core.Abstractions.Scoring;

namespace TableTop.Core.Engine;

/// <summary>
/// Concrete configuration carrying all dependencies needed to create a game session.
/// </summary>
public sealed class GameConfiguration : IGameConfiguration
{
    /// <summary>Initialises a new <see cref="GameConfiguration"/> instance.</summary>
    public GameConfiguration(
        IDeck deck,
        IEnumerable<IPlayer> players,
        IProgressionStrategy progressionStrategy,
        IScoringStrategy scoringStrategy,
        IEnumerable<IRule>? rules = null,
        int? maxRounds = null,
        TableTop.Core.Abstractions.Game.SpecialCardScoringPolicy specialCardScoringPolicy
            = TableTop.Core.Abstractions.Game.SpecialCardScoringPolicy.NoScore,
        int specialCardBonusScore = 0,
        IEnumerable<string>? deferredCategories = null)
    {
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        DeferredCategories = deferredCategories?.ToList().AsReadOnly()
            ?? (IReadOnlyList<string>)Array.Empty<string>();
        Players = players?.ToList().AsReadOnly()
            ?? throw new ArgumentNullException(nameof(players));
        ProgressionStrategy = progressionStrategy
            ?? throw new ArgumentNullException(nameof(progressionStrategy));
        ScoringStrategy = scoringStrategy
            ?? throw new ArgumentNullException(nameof(scoringStrategy));
        Rules = (rules ?? Enumerable.Empty<IRule>()).ToList().AsReadOnly();
        MaxRounds                = maxRounds;
        SpecialCardScoringPolicy = specialCardScoringPolicy;
        SpecialCardBonusScore    = specialCardBonusScore;
    }

    /// <inheritdoc />
    public IDeck Deck { get; }

    /// <inheritdoc />
    public IReadOnlyList<IPlayer> Players { get; }

    /// <inheritdoc />
    public IProgressionStrategy ProgressionStrategy { get; }

    /// <inheritdoc />
    public IScoringStrategy ScoringStrategy { get; }

    /// <inheritdoc />
    public IReadOnlyList<IRule> Rules { get; }

    /// <inheritdoc />
    public int? MaxRounds { get; }
    /// <summary>The policy that controls how special cards (break/reward) affect scoring.</summary>
    public TableTop.Core.Abstractions.Game.SpecialCardScoringPolicy SpecialCardScoringPolicy { get; }
    /// <summary>Bonus score awarded when a special card is drawn.</summary>
    public int SpecialCardBonusScore { get; }

    /// <summary>
    /// Categories held back until every other card has been played.
    ///
    /// Deck ORDER alone isn't enough to keep a card last: progression
    /// strategies choose candidates by peeking at the whole deck (by
    /// difficulty, for instance), so an easy card sitting at the end can still
    /// be picked early. Cards in these categories are excluded from candidate
    /// selection while anything else remains — which is what actually keeps a
    /// quiz results key, or an aftercare card, at the end.
    /// </summary>
    public IReadOnlyList<string> DeferredCategories { get; }
}