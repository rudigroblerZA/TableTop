using TableTop.Core.Abstractions.Decks;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Rules;

namespace TableTop.Core.Domain.Rules;

/// <summary>
/// Immutable snapshot of game state passed to <see cref="IRule"/> evaluators.
/// </summary>
public sealed class RuleContext : IRuleContext
{
    /// <summary>Initialises a new <see cref="RuleContext"/> instance.</summary>
    public RuleContext(
        int round,
        IEnumerable<IPlayer> players,
        IDeck deck,
        TableTop.Core.Abstractions.Game.GameMetadata? metadata = null)
    {
        Round = round;
        Players = players.ToList().AsReadOnly();
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        Metadata = metadata ?? new TableTop.Core.Abstractions.Game.GameMetadata();
    }

    /// <inheritdoc />
    public int Round { get; }

    /// <inheritdoc />
    public IReadOnlyList<IPlayer> Players { get; }

    /// <inheritdoc />
    public IDeck Deck { get; }

    /// <inheritdoc />
    public TableTop.Core.Abstractions.Game.GameMetadata Metadata { get; }
}