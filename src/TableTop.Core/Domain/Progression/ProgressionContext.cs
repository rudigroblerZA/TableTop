using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Progression;

namespace TableTop.Core.Domain.Progression;

/// <summary>
/// Immutable snapshot of game state passed to <see cref="IProgressionStrategy"/> implementations.
/// </summary>
public sealed class ProgressionContext : IProgressionContext
{
    /// <summary>Initialises a new <see cref="ProgressionContext"/> instance.</summary>
    public ProgressionContext(
        int round,
        IEnumerable<ICard> playedCards,
        IEnumerable<IPlayer> players,
        TableTop.Core.Abstractions.Game.GameMetadata? metadata = null)
    {
        Round = round;
        PlayedCards = playedCards.ToList().AsReadOnly();
        Players = players.ToList().AsReadOnly();
        Metadata = metadata ?? new TableTop.Core.Abstractions.Game.GameMetadata();
    }

    /// <inheritdoc />
    public int Round { get; }

    /// <inheritdoc />
    public IReadOnlyList<ICard> PlayedCards { get; }

    /// <inheritdoc />
    public IReadOnlyList<IPlayer> Players { get; }

    /// <inheritdoc />
    public TableTop.Core.Abstractions.Game.GameMetadata Metadata { get; }
}