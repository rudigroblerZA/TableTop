namespace TableTop.Hosting.Events;

/// <summary>A territory challenge has been drawn and is waiting for the table to judge it.</summary>
public sealed record TerritoryChallengeReadyEvent(
    string  PlayerName,
    string  TerritoryName,
    string  CardTitle,
    string  CardText,
    string  Difficulty,
    // Null when the territory is unclaimed; the current holder's name otherwise (a raid).
    string? DefenderName);

/// <summary>An unclaimed territory was successfully taken.</summary>
public sealed record TerritoryClaimedEvent(
    string PlayerName,
    string TerritoryName,
    // All territories this player now holds, for the board display.
    IReadOnlyList<string> HeldTerritories);

/// <summary>A rival-held territory was successfully raided and changed hands.</summary>
public sealed record TerritoryStolenEvent(
    string AttackerName,
    string DefenderName,
    string TerritoryName,
    IReadOnlyList<string> AttackerHeldTerritories,
    IReadOnlyList<string> DefenderHeldTerritories);

/// <summary>The challenge failed. The territory's ownership is unchanged.</summary>
public sealed record ChallengeFailedEvent(
    string PlayerName,
    string TerritoryName,
    // True when this was a raid on a held territory rather than a claim on open ground.
    bool   WasRaid);

/// <summary>Why a Claimed! session ended.</summary>
public enum ClaimedEndReason
{
    /// <summary>A player reached the winning territory count simultaneously.</summary>
    ThreeHeld,
    /// <summary>Every territory's deck ran out before anyone reached the target.</summary>
    DeckExhausted,
}

/// <summary>The session is over.</summary>
public sealed record ClaimedGameEndedEvent(
    ClaimedEndReason Reason,
    // Usually one name; more than one on a tied deck-exhaustion ending.
    IReadOnlyList<string> WinnerNames,
    // Every player's final holdings, for the standings screen.
    IReadOnlyDictionary<string, IReadOnlyList<string>> FinalHoldings);
