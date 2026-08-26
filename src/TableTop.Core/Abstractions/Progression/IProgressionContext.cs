using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Abstractions.Progression;

/// <summary>
/// Read-only game state available to progression strategies.
/// </summary>
public interface IProgressionContext
{
    /// <summary>Current round number.</summary>
    int Round { get; }

    /// <summary>Cards already played this session, in order.</summary>
    IReadOnlyList<ICard> PlayedCards { get; }

    /// <summary>All active players.</summary>
    IReadOnlyList<IPlayer> Players { get; }

    /// <summary>Arbitrary metadata (e.g. timer state, mode flags).</summary>
    TableTop.Core.Abstractions.Game.GameMetadata Metadata { get; }
}
