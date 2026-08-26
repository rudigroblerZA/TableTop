namespace TableTop.Core.Abstractions.Players;

/// <summary>Player participation status within a game session.</summary>
public enum PlayerStatus
{
    /// <summary>The game is running normally.</summary>
    /// <summary>The player is participating in the current round rotation.</summary>
    Active,
    /// <summary>Eliminated.</summary>
    Skipped,
    /// <summary>The player is watching but not participating in the turn rotation.</summary>
    /// <summary>Spectating.</summary>
    Eliminated,
    /// <summary>The player is watching but not participating in the turn rotation.</summary>
    Spectating
}