using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Who is round the table. Flags rather than a single value because most modes
/// genuinely suit more than one shape — a quiz works for a family, a work team
/// and a group of friends alike, and forcing a single answer would mean picking
/// one arbitrarily and hiding it from the other two.
/// </summary>
[Flags]
public enum TableShape
{
    /// <summary>No shape declared. Never used as an answer; see <see cref="TableShapes.Any"/>.</summary>
    None = 0,

    /// <summary>Exactly two people in a relationship with each other.</summary>
    Couple = 1,

    /// <summary>A household across more than one generation — mixed ages, parents present.</summary>
    Family = 2,

    /// <summary>Colleagues. Shares the mixed-familiarity problem with Group, but has a professional floor.</summary>
    Team = 4,

    /// <summary>Friends, a party, any assortment of adults who chose to be there.</summary>
    Group = 8,
}

/// <summary>Helpers over <see cref="TableShape"/>.</summary>
public static class TableShapes
{
    /// <summary>Every shape. The default for a mode that hasn't declared one.</summary>
    public const TableShape Any = TableShape.Couple | TableShape.Family | TableShape.Team | TableShape.Group;

    /// <summary>True when <paramref name="declared"/> covers <paramref name="wanted"/> at all.</summary>
    ///
    /// Overlap rather than equality: a mode declaring Family|Group suits a table
    /// asking for Group. A table asking for several shapes at once is asking for
    /// anything that fits any of them.
    public static bool Suits(this TableShape declared, TableShape wanted) =>
        wanted == TableShape.None || (declared & wanted) != TableShape.None;
}

/// <summary>
/// Implemented by a mode that only makes sense for particular table shapes.
///
/// OPT-IN, AND THE DEFAULT IS DELIBERATE
/// ─────────────────────────────────────
/// A mode that doesn't implement this is treated as suiting
/// <see cref="TableShapes.Any"/>, not as suiting nothing. There are ~90 modes in
/// the catalogue and only a handful have a real constraint; defaulting to hidden
/// would empty the selection screen for anyone who set a shape filter, and the
/// failure would look like a filter bug rather than missing annotations.
///
/// Declare a shape only where playing outside it would actually be wrong —
/// Monogamy at a family table, say — not merely where another shape is a better
/// fit. "Suits" is a lower bar than "is ideal for".
/// </summary>
public interface ITableShapeMode
{
    /// <summary>The shapes this mode is genuinely playable at.</summary>
    TableShape SuitableFor { get; }
}

/// <summary>
/// The table actually in front of you: how many people, and what shape they are.
/// </summary>
/// <param name="PlayerCount">How many are playing.</param>
/// <param name="Shape">
/// The shape(s) this table satisfies. More than one is normal — three colleagues
/// who are also friends are both a Team and a Group.
/// </param>
public sealed record TableComposition(int PlayerCount, TableShape Shape)
{
    /// <summary>A table of unknown shape — matches everything.</summary>
    public static TableComposition Unknown { get; } = new(0, TableShapes.Any);

    /// <summary>
    /// Infers a composition from the players present, using the tags the
    /// restriction system already relies on so there is one vocabulary rather
    /// than two.
    ///
    /// Inference is deliberately generous: it adds shapes it can justify and
    /// never subtracts. A table it can't read comes back as
    /// <see cref="TableShapes.Any"/> rather than as nothing, because a wrong
    /// guess that hides content is worse than one that shows a little too much —
    /// the per-card restrictions are still the real gate either way.
    /// </summary>
    /// <param name="players">Who is playing.</param>
    public static TableComposition From(IReadOnlyList<IPlayer> players)
    {
        ArgumentNullException.ThrowIfNull(players);
        if (players.Count == 0) return Unknown;

        var shape = TableShape.None;

        bool Tagged(string tag) =>
            players.Any(p => p.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));

        // Exactly two people who are each other's partner. The couple-member tag
        // is the same one BuiltInRestrictions gates couple-only cards on.
        if (players.Count == 2 &&
            players.All(p => p.Tags.Contains("couple-member", StringComparer.OrdinalIgnoreCase)))
            shape |= TableShape.Couple;

        // A family needs the generational mix, not merely a parent present —
        // two parents on a night out are not a family table.
        if (Tagged("parent") && (Tagged("child") || Tagged("teen")))
            shape |= TableShape.Family;

        if (Tagged("colleague")) shape |= TableShape.Team;

        // Three or more is playable as a group — but NOT when this is a family
        // table. A family of four would otherwise pick up Group from the head
        // count alone and see everything written for adults out together, which
        // is how the pub drinking game would end up in front of the children.
        // A two-person family (one parent, one teen) escaped this only by being
        // too small to trip the count rule, which is luck rather than design.
        if (!shape.HasFlag(TableShape.Family) && (players.Count >= 3 || shape == TableShape.None))
            shape |= TableShape.Group;

        return new TableComposition(players.Count, shape);
    }
}
