using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Rules;
using TableTop.Core.Abstractions.Scoring;

namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Exposes the card catalogue, rules, and scoring of a game mode for non-console hosts
/// (WPF, MAUI, ASP.NET, etc.) that drive the engine loop themselves rather than
/// delegating to <see cref="IGame"/>.
///
/// All <see cref="IGameMode"/> implementations should also implement this interface
/// so any host can reuse the same definitions without duplication (OCP, DIP).
/// </summary>
public interface IGameModeDefinition
{
    /// <summary>Returns all playable cards for this mode, optionally filtered by player context.</summary>
    IReadOnlyList<ICard> GetCards(IReadOnlyList<IPlayer> players);

    /// <summary>Returns the scoring strategy used by this mode.</summary>
    IScoringStrategy GetScoring();

    /// <summary>Returns the rules pipeline used by this mode.</summary>
    IEnumerable<IRule> GetRules();

    /// <summary>
    /// Card categories that must be dealt FIRST, whatever the shuffle setting.
    ///
    /// Shuffling defaults to on, which is right for almost every deck but wrong
    /// for a few where order carries meaning — a consent ritual has to come
    /// before the cards it governs, not somewhere in the middle.
    /// </summary>
    IReadOnlyList<string> CategoriesPinnedToStart => [];

    /// <summary>
    /// Card categories that must be dealt LAST, whatever the shuffle setting.
    ///
    /// Used for results keys (a quiz answer key shuffled into the middle both
    /// spoils the quiz and arrives before there is anything to interpret) and
    /// for aftercare, which only makes sense at the end.
    /// </summary>
    IReadOnlyList<string> CategoriesPinnedToEnd => [];

    /// <summary>
    /// Fewest players this mode needs.
    ///
    /// Defaults to 2, which is right for the party and couples decks that make
    /// up most of the catalogue. Personality quizzes override it to 1: they are
    /// self-assessments, and requiring a second person to take one is an
    /// arbitrary barrier — the UIs were hardcoding a minimum of two rather than
    /// asking the mode what it actually needed.
    /// </summary>
    int MinimumPlayers => 2;
}
