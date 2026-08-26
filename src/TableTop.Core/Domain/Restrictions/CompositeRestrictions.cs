using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Restrictions;

namespace TableTop.Core.Domain.Restrictions;

/// <summary>
/// Combines two restrictions with a logical AND.
/// Both must be satisfied for the card to be eligible.
/// </summary>
public sealed class AndRestriction : IRestriction
{
    private readonly IRestriction _left;
    private readonly IRestriction _right;

    internal IRestriction Left  => _left;
    internal IRestriction Right => _right;

    /// <summary>Initialises a new <see cref="AndRestriction"/> instance.</summary>
    public AndRestriction(IRestriction left, IRestriction right)
    {
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = right ?? throw new ArgumentNullException(nameof(right));
    }

    /// <inheritdoc />
    public string Description => $"({_left.Description} AND {_right.Description})";

    /// <inheritdoc />
    public bool IsSatisfiedBy(IPlayer player, IReadOnlyList<IPlayer> context) =>
        _left.IsSatisfiedBy(player, context) && _right.IsSatisfiedBy(player, context);
}

/// <summary>
/// Combines two restrictions with a logical OR.
/// Either must be satisfied for the card to be eligible.
/// </summary>
public sealed class OrRestriction : IRestriction
{
    private readonly IRestriction _left;
    private readonly IRestriction _right;

    internal IRestriction Left  => _left;
    internal IRestriction Right => _right;

    /// <summary>Initialises a new <see cref="OrRestriction"/> instance.</summary>
    public OrRestriction(IRestriction left, IRestriction right)
    {
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = right ?? throw new ArgumentNullException(nameof(right));
    }

    /// <inheritdoc />
    public string Description => $"({_left.Description} OR {_right.Description})";

    /// <inheritdoc />
    public bool IsSatisfiedBy(IPlayer player, IReadOnlyList<IPlayer> context) =>
        _left.IsSatisfiedBy(player, context) || _right.IsSatisfiedBy(player, context);
}

/// <summary>
/// Negates an existing restriction.
/// </summary>
public sealed class NotRestriction : IRestriction
{
    private readonly IRestriction _inner;

    internal IRestriction Inner => _inner;

    /// <summary>Initialises a new <see cref="NotRestriction"/> instance.</summary>
    public NotRestriction(IRestriction inner) =>
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    /// <inheritdoc />
    public string Description => $"NOT ({_inner.Description})";

    /// <inheritdoc />
    public bool IsSatisfiedBy(IPlayer player, IReadOnlyList<IPlayer> context) =>
        !_inner.IsSatisfiedBy(player, context);
}