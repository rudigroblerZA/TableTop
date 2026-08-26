using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Domain.Players;

/// <summary>
/// Mutable player entity used during a game session.
/// </summary>
public sealed class Player : IPlayer
{
    private readonly Dictionary<string, string> _attributes;
    private readonly List<string> _tags;

    /// <summary>Initialises a new <see cref="Player"/> instance.</summary>
    public Player(
        Guid id,
        string displayName,
        IDictionary<string, string>? attributes = null,
        IEnumerable<string>? tags = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        Id = id;
        DisplayName = displayName;
        _attributes = new Dictionary<string, string>(
            attributes ?? new Dictionary<string, string>(),
            StringComparer.OrdinalIgnoreCase);
        _tags = (tags ?? Enumerable.Empty<string>()).ToList();
    }

    /// <inheritdoc />
    public Guid Id { get; }

    /// <inheritdoc />
    public string DisplayName { get; }

    /// <inheritdoc />
    public PlayerStatus Status { get; internal set; } = PlayerStatus.Active;

    /// <inheritdoc />
    public int Score { get; internal set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Attributes => _attributes;

    /// <inheritdoc />
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();

    /// <summary>Convenience factory with a new random identifier.</summary>
    public static Player Create(
        string displayName,
        IDictionary<string, string>? attributes = null,
        IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), displayName, attributes, tags);
}