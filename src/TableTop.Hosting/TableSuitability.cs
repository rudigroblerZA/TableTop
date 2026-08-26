using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;

namespace TableTop.Hosting;

/// <summary>
/// The result of checking a mode against the table about to play it. See
/// <see cref="TableSuitability.Check"/>.
/// </summary>
/// <param name="Suits">
/// True when the table's shape overlaps the mode's <see cref="ITableShapeMode.SuitableFor"/>
/// (or the mode declares no shape at all, in which case everything suits it).
/// </param>
/// <param name="ModeName">The mode that was checked.</param>
/// <param name="Required">
/// The shape(s) the mode declares. <see cref="TableShape.None"/> when the mode
/// doesn't implement <see cref="ITableShapeMode"/> — there is nothing to be
/// unsuitable for.
/// </param>
/// <param name="Actual">The shape <see cref="TableComposition.From"/> inferred for the players given.</param>
public sealed record TableSuitabilityResult(bool Suits, string ModeName, TableShape Required, TableShape Actual)
{
    /// <summary>
    /// A player-facing explanation of the mismatch, or null when <see cref="Suits"/>
    /// is true. Says what the mode needs, not what the table happens to be missing —
    /// the fix (tag players as a couple, add players) lives with the caller, which
    /// knows the UI it's writing this into.
    /// </summary>
    public string? Explanation => Suits
        ? null
        : $"{ModeName} needs a {Describe(Required)} table, and this one doesn't look like one yet.";

    private static string Describe(TableShape shape) =>
        string.Join(" or ", Enum.GetValues<TableShape>()
            .Where(s => s != TableShape.None && shape.HasFlag(s))
            .Select(s => s.ToString()));
}

/// <summary>
/// Checks a mode against a table <b>before</b> a session starts — the check
/// <see cref="ArchetypeFilter"/> cannot do, because it filters the picker, not
/// the launch.
///
/// <see cref="ITableShapeMode.SuitableFor"/> already exists and
/// <see cref="TableComposition.From"/> already infers a table's shape from its
/// players; until this type, nothing on the path into a game read either of
/// them. A mode could be started at an unsuitable table, have its per-card
/// restrictions strip most or all of its deck, and hand back a session that
/// starts normally and plays nothing — no error, no warning. Worst for the
/// adult and couple content, where an unrestricted card reaching the wrong
/// table is a consent problem, not a UX one.
///
/// This does not decide what a UI does with an unsuitable result — block,
/// warn, or offer to fix the players inline are all legitimate and the right
/// choice differs by head. It only makes the fact checkable, which is the
/// part that was missing.
/// </summary>
public static class TableSuitability
{
    /// <summary>
    /// Checks whether <paramref name="mode"/> suits a table made up of
    /// <paramref name="players"/>. A mode that doesn't implement
    /// <see cref="ITableShapeMode"/> always suits — same permissive default
    /// <see cref="ArchetypeFilter"/> uses, for the same reason: most of the
    /// catalogue has no real constraint.
    /// </summary>
    public static TableSuitabilityResult Check(IGameMode mode, IReadOnlyList<IPlayer> players)
    {
        ArgumentNullException.ThrowIfNull(mode);
        ArgumentNullException.ThrowIfNull(players);

        if (mode is not ITableShapeMode shaped)
            return new TableSuitabilityResult(true, mode.Name, TableShape.None, TableShape.None);

        var actual = TableComposition.From(players).Shape;
        var suits  = shaped.SuitableFor.Suits(actual);
        return new TableSuitabilityResult(suits, mode.Name, shaped.SuitableFor, actual);
    }
}
