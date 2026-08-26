namespace TableTop.Core.Abstractions.Players;

/// <summary>
/// Manages the set of players in a game session.
/// Separated from IPlayer to satisfy SRP — player data vs. player lifecycle.
/// </summary>
public interface IPlayerManager
{
    /// <summary>All registered players.</summary>
    IReadOnlyList<IPlayer> Players { get; }

    /// <summary>Players currently eligible to receive a card.</summary>
    IReadOnlyList<IPlayer> ActivePlayers { get; }

    /// <summary>Adds a player to the session.</summary>
    void AddPlayer(IPlayer player);

    /// <summary>Removes a player from the session.</summary>
    void RemovePlayer(Guid playerId);

    /// <summary>Returns the next player in turn order.</summary>
    IPlayer? GetNextPlayer();

    /// <summary>Updates the status of a specific player.</summary>
    void SetStatus(Guid playerId, PlayerStatus status);

    /// <summary>Applies a score delta to a player.</summary>
    void ApplyScore(Guid playerId, int delta);

    /// <summary>
    /// Moves the turn pointer so that <paramref name="playerId"/> is the CURRENT
    /// player — meaning the next call to <see cref="GetNextPlayer"/> returns whoever
    /// follows them. Used when a turn is rewound (undo) so play resumes in the
    /// correct order instead of skipping a player.
    /// </summary>
    void RewindTo(Guid playerId);
}
