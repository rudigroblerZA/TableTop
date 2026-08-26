using TableTop.Hosting.Events;

namespace TableTop.Hosting.Abstractions;

/// <summary>
/// Controls a "Claimed!" area-control session: players take turns challenging
/// territories, either claiming open ground or raiding a rival's holding.
/// </summary>
public interface IClaimedController : IGameController
{
    /// <summary>TerritoryChallengeReady.</summary>
    event EventHandler<TerritoryChallengeReadyEvent> TerritoryChallengeReady;
    /// <summary>TerritoryClaimed.</summary>
    event EventHandler<TerritoryClaimedEvent>         TerritoryClaimed;
    /// <summary>TerritoryStolen.</summary>
    event EventHandler<TerritoryStolenEvent>          TerritoryStolen;
    /// <summary>ChallengeFailed.</summary>
    event EventHandler<ChallengeFailedEvent>          ChallengeFailed;
    /// <summary>GameEnded.</summary>
    event EventHandler<ClaimedGameEndedEvent>         GameEnded;

    /// <summary>Starts the session with the first player's turn.</summary>
    void Start();

    /// <summary>Name of the player whose turn it currently is.</summary>
    string CurrentPlayerName { get; }

    /// <summary>
    /// Every territory not already held by the current player and not yet
    /// exhausted of cards — the legal choices for this turn.
    /// </summary>
    IReadOnlyList<string> ChallengeableTerritories { get; }

    /// <summary>
    /// Every territory and who holds it, or null for open ground — the full
    /// board, for a UI that wants to render it between turns.
    /// </summary>
    IReadOnlyDictionary<string, string?> TerritoryHolders { get; }

    /// <summary>
    /// Draws the next card for <paramref name="territoryName"/> and raises
    /// <see cref="TerritoryChallengeReady"/>. Must be one of
    /// <see cref="ChallengeableTerritories"/>.
    /// </summary>
    void ChallengeTerritory(string territoryName);

    /// <summary>
    /// The table's judgment on the card just presented. Claims, steals, or
    /// does nothing, checks the win condition, then advances the turn.
    /// </summary>
    void ResolveChallenge(bool succeeded);
}
