namespace TableTop.Core.Abstractions.Players;

/// <summary>
/// Represents a participant in a game session.
/// </summary>
public interface IPlayer
{
    /// <summary>Unique identifier for this player.</summary>
    Guid Id { get; }

    /// <summary>Name shown to other players.</summary>
    string DisplayName { get; }

    /// <summary>Current status in the active game.</summary>
    PlayerStatus Status { get; }

    /// <summary>Accumulated score in the current session.</summary>
    int Score { get; }

    /// <summary>
    /// Arbitrary key/value attributes (e.g. "gender", "age", "role").
    /// Used by restriction and rule evaluators.
    /// </summary>
    IReadOnlyDictionary<string, string> Attributes { get; }

    /// <summary>
    /// Audience tags (e.g. "adult", "parent", "couple-member").
    /// Used for restriction matching.
    /// </summary>
    IReadOnlyList<string> Tags { get; }
}
