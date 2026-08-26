using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Restrictions;

namespace TableTop.Core.Domain.Restrictions;

/// <summary>
/// Requires the player to have a specific tag (e.g. "adult", "parent", "couple-member").
/// </summary>
public sealed class TagRestriction : IRestriction
{
    private readonly string _requiredTag;

    /// <summary>Initialises a new <see cref="TagRestriction"/> instance.</summary>
    public TagRestriction(string requiredTag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requiredTag);
        _requiredTag = requiredTag;
    }

    internal string RequiredTag => _requiredTag;

    /// <inheritdoc />
    public string Description => $"Player must have tag '{_requiredTag}'";

    /// <inheritdoc />
    public bool IsSatisfiedBy(IPlayer player, IReadOnlyList<IPlayer> context) =>
        player.Tags.Contains(_requiredTag, StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Requires the player to have a specific attribute value (e.g. gender = "male").
/// Comparisons are case-insensitive.
/// </summary>
public sealed class AttributeRestriction : IRestriction
{
    private readonly string _key;
    private readonly string _value;

    /// <summary>Initialises a new <see cref="AttributeRestriction"/> instance.</summary>
    public AttributeRestriction(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        _key = key;
        _value = value;
    }

    internal string Key   => _key;
    internal string Value => _value;

    /// <inheritdoc />
    public string Description => $"Player attribute '{_key}' must equal '{_value}'";

    /// <inheritdoc />
    public bool IsSatisfiedBy(IPlayer player, IReadOnlyList<IPlayer> context) =>
        player.Attributes.TryGetValue(_key, out var actual) &&
        string.Equals(actual, _value, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Requires the player to be male (Attribute "gender" = "male").
/// </summary>
public sealed class MaleOnlyRestriction : IRestriction
{
    private static readonly AttributeRestriction _inner = new("gender", "male");

    /// <inheritdoc />
    public string Description => "Male players only";

    /// <inheritdoc />
    public bool IsSatisfiedBy(IPlayer player, IReadOnlyList<IPlayer> context) =>
        _inner.IsSatisfiedBy(player, context);
}

/// <summary>
/// Requires the player to be female (Attribute "gender" = "female").
/// </summary>
public sealed class FemaleOnlyRestriction : IRestriction
{
    private static readonly AttributeRestriction _inner = new("gender", "female");

    /// <inheritdoc />
    public string Description => "Female players only";

    /// <inheritdoc />
    public bool IsSatisfiedBy(IPlayer player, IReadOnlyList<IPlayer> context) =>
        _inner.IsSatisfiedBy(player, context);
}

/// <summary>
/// Requires the player to have the "adult" tag.
/// </summary>
public sealed class AdultOnlyRestriction : IRestriction
{
    private static readonly TagRestriction _inner = new("adult");

    /// <inheritdoc />
    public string Description => "Adult players only";

    /// <inheritdoc />
    public bool IsSatisfiedBy(IPlayer player, IReadOnlyList<IPlayer> context) =>
        _inner.IsSatisfiedBy(player, context);
}

/// <summary>
/// Requires the player to have the "parent" tag.
/// </summary>
public sealed class ParentOnlyRestriction : IRestriction
{
    private static readonly TagRestriction _inner = new("parent");

    /// <inheritdoc />
    public string Description => "Parents only";

    /// <inheritdoc />
    public bool IsSatisfiedBy(IPlayer player, IReadOnlyList<IPlayer> context) =>
        _inner.IsSatisfiedBy(player, context);
}

/// <summary>
/// Requires the player to have the "married" tag.
/// </summary>
public sealed class MarriedOnlyRestriction : IRestriction
{
    private static readonly TagRestriction _inner = new("married");

    /// <inheritdoc />
    public string Description => "Married players only";

    /// <inheritdoc />
    public bool IsSatisfiedBy(IPlayer player, IReadOnlyList<IPlayer> context) =>
        _inner.IsSatisfiedBy(player, context);
}

/// <summary>
/// Requires the player to have the "couple-member" tag AND at least one other
/// "couple-member" player to be present in the session context.
/// </summary>
public sealed class CoupleOnlyRestriction : IRestriction
{
    private const string CoupleTag = "couple-member";

    /// <inheritdoc />
    public string Description => "Couple members only (requires partner in session)";

    /// <inheritdoc />
    public bool IsSatisfiedBy(IPlayer player, IReadOnlyList<IPlayer> context)
    {
        if (!player.Tags.Contains(CoupleTag, StringComparer.OrdinalIgnoreCase))
            return false;

        // At least one other active couple-member must be present
        return context.Any(p =>
            p.Id != player.Id &&
            p.Tags.Contains(CoupleTag, StringComparer.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Requires the player's numeric "age" attribute to meet a minimum value.
/// </summary>
public sealed class MinimumAgeRestriction : IRestriction
{
    private readonly int _minimumAge;

    /// <summary>Initialises a new <see cref="MinimumAgeRestriction"/> instance.</summary>
    public MinimumAgeRestriction(int minimumAge)
    {
        if (minimumAge < 0)
            throw new ArgumentOutOfRangeException(nameof(minimumAge));
        _minimumAge = minimumAge;
    }

    internal int MinimumAge => _minimumAge;

    /// <inheritdoc />
    public string Description => $"Minimum age {_minimumAge}";

    /// <inheritdoc />
    public bool IsSatisfiedBy(IPlayer player, IReadOnlyList<IPlayer> context) =>
        player.Attributes.TryGetValue("age", out var raw) &&
        int.TryParse(raw, out var age) &&
        age >= _minimumAge;
}