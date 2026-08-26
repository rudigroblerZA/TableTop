using TableTop.Core.Abstractions.Cards;

namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Implemented by game modes that supply their own territory-challenge deck for
/// a "Claimed!" style area-control game. Cards group into territories by their
/// own <see cref="ICard.Category"/> — no separate territory format, so any
/// existing multi-category deck already qualifies.
///
/// Mirrors <see cref="IQuestionBankProvider"/>: the hosting factory obtains
/// content from the mode itself rather than reaching into a specific card-bank
/// class (DIP, OCP).
/// </summary>
public interface IClaimedDeckProvider
{
    /// <summary>
    /// The challenge deck. Each distinct <see cref="ICard.Category"/> becomes
    /// one territory; the controller shuffles within each territory but never
    /// across them, so claiming one territory says nothing about the
    /// difficulty of another.
    /// </summary>
    IReadOnlyList<ICard> GetClaimedDeck();

    /// <summary>
    /// Territories a player must hold <i>simultaneously</i> to win outright.
    /// Typically 3 of 5–6 — enough that holding it all isn't required, but
    /// enough that a single early claim isn't a runaway lead.
    /// </summary>
    int WinningTerritoryCount { get; }
}
