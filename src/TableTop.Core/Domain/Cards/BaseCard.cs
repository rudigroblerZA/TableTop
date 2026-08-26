using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Restrictions;

namespace TableTop.Core.Domain.Cards;

/// <summary>
/// Base implementation of <see cref="ICard"/> providing common card properties.
/// Extend this class to create specialised card types without modifying the base (OCP).
/// </summary>
public abstract class BaseCard : ICard
{
    /// <summary>Initialises a new <see cref="BaseCard"/> instance.</summary>
    protected BaseCard(
        Guid id,
        string title,
        string description,
        Difficulty difficulty,
        string category,
        IEnumerable<string>? tags = null,
        IRestriction? restriction = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        Id = id;
        Title = title;
        Description = description;
        Difficulty = difficulty;
        Category = category;
        Tags = (tags ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
        Restriction = restriction;
    }

    /// <inheritdoc />
    public Guid Id { get; }

    /// <inheritdoc />
    public string Title { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public Difficulty Difficulty { get; }

    /// <inheritdoc />
    public string Category { get; }

    /// <inheritdoc />
    public IReadOnlyList<string> Tags { get; }

    /// <inheritdoc />
    public IRestriction? Restriction { get; }
}