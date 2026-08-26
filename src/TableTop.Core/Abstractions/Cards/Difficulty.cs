namespace TableTop.Core.Abstractions.Cards;

/// <summary>
/// Represents the relative difficulty of a card, used by progression strategies.
/// </summary>
public enum Difficulty
{
    /// <summary>The easiest difficulty tier; accessible to all players.</summary>
    Easy = 1,
    /// <summary>Moderate challenge; requires more thought or willingness.</summary>
    Medium = 2,
    /// <summary>Challenging; pushes players outside their comfort zone.</summary>
    Hard = 3,
    /// <summary>Maximum challenge; reserved for players who want the full experience.</summary>
    Extreme = 4
}