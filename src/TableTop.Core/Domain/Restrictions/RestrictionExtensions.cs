using TableTop.Core.Abstractions.Restrictions;
namespace TableTop.Core.Domain.Restrictions;

/// <summary>
/// Fluent extension methods for composing restrictions.
/// </summary>
public static class RestrictionExtensions
{
    /// <summary>Returns a restriction that requires both <paramref name="left"/> and <paramref name="right"/>.</summary>
    public static IRestriction And(this IRestriction left, IRestriction right) =>
        new AndRestriction(left, right);

    /// <summary>Returns a restriction that requires either <paramref name="left"/> or <paramref name="right"/>.</summary>
    public static IRestriction Or(this IRestriction left, IRestriction right) =>
        new OrRestriction(left, right);

    /// <summary>Returns the logical negation of this restriction.</summary>
    public static IRestriction Not(this IRestriction restriction) =>
        new NotRestriction(restriction);
}
