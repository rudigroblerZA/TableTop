using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;

namespace TableTop.Core.Abstractions.Game;

/// <summary>Lifecycle states of a game session.</summary>
public enum GameState
{
    /// <summary>The game has been created but <c>Start()</c> has not been called.</summary>
    Pending,
    /// <summary>The game is running; cards are being drawn and outcomes recorded.</summary>
    Active,
    /// <summary>The game loop is suspended; no new cards until resumed.</summary>
    Paused,
    /// <summary>All rounds have been played or <c>Quit()</c> was called.</summary>
    Ended,
}

/// <summary>Payload raised when a turn completes inside the engine.</summary>
public sealed class TurnCompletedEventArgs : EventArgs
{
    /// <summary>The player whose turn just ended.</summary>
    public required IPlayer Player { get; init; }
    /// <summary>The card that was drawn for this turn.</summary>
    public required ICard Card { get; init; }
    /// <summary>The outcome recorded by the player.</summary>
    public required CardOutcome Outcome { get; init; }
    /// <summary>Score change resulting from this turn.</summary>
    public required int ScoreDelta { get; init; }
    /// <summary>Round number when this turn occurred.</summary>
    public required int Round { get; init; }
}

/// <summary>Payload raised when the game session ends.</summary>
public sealed class GameEndedEventArgs : EventArgs
{
    /// <summary>Players ranked by final score, descending.</summary>
    public required IReadOnlyList<IPlayer> FinalStandings { get; init; }
    /// <summary>Total number of rounds played.</summary>
    public required int TotalRounds { get; init; }
}
