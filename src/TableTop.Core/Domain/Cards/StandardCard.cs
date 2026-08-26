using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Restrictions;

namespace TableTop.Core.Domain.Cards;

/// <summary>
/// A general-purpose card suitable for most game types.
/// Create domain-specific card types by subclassing <see cref="BaseCard"/> instead of modifying this class.
/// </summary>
public sealed class StandardCard : BaseCard
{
    /// <summary>Initialises a new <see cref="StandardCard"/> instance.</summary>
    public StandardCard(
        Guid id,
        string title,
        string description,
        Difficulty difficulty,
        string category,
        IEnumerable<string>? tags = null,
        IRestriction? restriction = null)
        : base(id, title, description, difficulty, category, tags, restriction)
    {
    }

    /// <summary>Convenience factory using a new random <see cref="Guid"/>.</summary>
    public static StandardCard Create(
        string title,
        string description,
        Difficulty difficulty,
        string category,
        IEnumerable<string>? tags = null,
        IRestriction? restriction = null) =>
        new(Guid.NewGuid(), title, description, difficulty, category, tags, restriction);
}