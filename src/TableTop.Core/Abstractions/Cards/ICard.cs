using TableTop.Core.Abstractions.Restrictions;

namespace TableTop.Core.Abstractions.Cards;

/// <summary>
/// Defines the contract for a playable card within the engine.
/// Implementations must be substitutable without special consumer handling (LSP).
/// </summary>
public interface ICard
{
    /// <summary>Globally unique identifier for this card.</summary>
    Guid Id { get; }

    /// <summary>Display title shown to players.</summary>
    string Title { get; }

    /// <summary>Full description or prompt text for the card.</summary>
    string Description { get; }

    /// <summary>Difficulty rating used in progression logic.</summary>
    Difficulty Difficulty { get; }

    /// <summary>Arbitrary tags used for filtering and rule evaluation.</summary>
    IReadOnlyList<string> Tags { get; }

    /// <summary>Thematic or mechanical category (e.g. "Dare", "Question", "Action").</summary>
    string Category { get; }

    /// <summary>
    /// Composite restriction that controls which players or groups are eligible for this card.
    /// Null means no restrictions (everyone is eligible).
    /// </summary>
    IRestriction? Restriction { get; }
}
