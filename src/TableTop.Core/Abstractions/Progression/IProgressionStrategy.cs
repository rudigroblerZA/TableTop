using TableTop.Core.Abstractions.Decks;
using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Abstractions.Progression;

/// <summary>
/// Determines which card to present next based on game state.
/// Swappable without changing engine internals (OCP, DIP).
///
/// CONTRACT — selection is separated from consumption:
/// <see cref="SelectCandidate"/> peeks at the deck without mutating it.
/// The engine validates the candidate against rules, then calls
/// <see cref="IDeck.DrawById"/> exactly once. This prevents cards from
/// being silently consumed during failed search attempts.
/// </summary>
public interface IProgressionStrategy
{
    /// <summary>Display name of this progression strategy.</summary>
    string Name { get; }

    /// <summary>
    /// Returns the ID of the card this strategy wants to play next,
    /// without removing it from the deck.
    /// Returns null when no suitable candidate is available.
    /// </summary>
    Guid? SelectCandidate(IPlayer player, IDeck deck, IProgressionContext context);
}
