using TableTop.Core.Domain.Cards;

namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Implemented by game modes that supply their own deck of <see cref="MonogamyCard"/>s.
/// Lets the hosting factory obtain the deck — and any win condition — <i>from the
/// mode</i> rather than referencing a specific static card-bank class (DIP, OCP).
/// </summary>
public interface IMonogamyDeckProvider
{
    /// <summary>The full Monogamy deck this mode plays with (pre-shuffle).</summary>
    IReadOnlyList<MonogamyCard> GetDeck();

    /// <summary>
    /// Number of tokens required to win, or null for an open-ended session.
    /// Defaults to 10 when not overridden.
    /// </summary>
    int? WinningTokenCount => 10;
}
