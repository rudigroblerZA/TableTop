using TableTop.Core.Abstractions.Game;
namespace TableTop.Hosting;

/// <summary>
/// Filters the archetype tree for game-selection settings — the other half
/// of "comprehensive settings": <see cref="TableTop.Core.Abstractions.Game.GameplayOptions"/> shapes how a
/// session plays once you're in it, this shapes what's even offered on the
/// selection screen in the first place.
///
/// One filter today (age-rating ceiling), written as a class rather than a
/// static method so the reasonable next filters (hide specific categories,
/// favourites-only, hide modes with fewer than N cards) have a natural home
/// without changing every call site.
/// </summary>
public sealed class ArchetypeFilter
{
    private readonly AgeRating _minAgeRating;
    private readonly AgeRating _maxAgeRating;
    private readonly TableShape _tableShape;

    /// <param name="minAgeRating">
    /// The lowest age rating to include — content rated below this is
    /// hidden. Most callers leave this at <see cref="AgeRating.AllAges"/>
    /// (no floor); it exists because at least one real settings screen
    /// ("hide games below the selected rating") genuinely wants a floor,
    /// not a ceiling.
    /// </param>
    /// <param name="maxAgeRating">
    /// The highest age rating to include — a parental-style ceiling.
    /// <see cref="AgeRating.AllAges"/> shows only all-ages content;
    /// <see cref="AgeRating.Adult"/> shows everything at or above the floor.
    /// </param>
    /// <param name="tableShape">
    /// Who is at the table. Modes that declare an incompatible shape are
    /// dropped; modes that declare nothing survive, because most of the
    /// catalogue has no real constraint and hiding all of it would look like a
    /// broken filter rather than missing annotations. Leave at
    /// <see cref="TableShapes.Any"/> to filter on age alone.
    /// </param>
    public ArchetypeFilter(
        AgeRating minAgeRating = AgeRating.AllAges,
        AgeRating maxAgeRating = AgeRating.Adult,
        TableShape tableShape = TableShapes.Any)
    {
        _minAgeRating = minAgeRating;
        _maxAgeRating = maxAgeRating;
        _tableShape = tableShape;
    }

    /// <summary>Filters for a specific table, at a given rating ceiling.</summary>
    public ArchetypeFilter(TableComposition table, AgeRating maxAgeRating = AgeRating.Adult)
        : this(AgeRating.AllAges, maxAgeRating, table.Shape) { }

    /// <summary>Convenience constructor for the common ceiling-only case.</summary>
    public ArchetypeFilter(AgeRating maxAgeRating) : this(AgeRating.AllAges, maxAgeRating) { }

    /// <summary>An unfiltered pass-through — every archetype, every mode.</summary>
    public static ArchetypeFilter ShowEverything { get; } = new();

    /// <summary>
    /// Returns a filtered copy of <paramref name="archetypes"/>: nodes whose
    /// own <see cref="Archetype.AgeRating"/> falls outside [min, max] are
    /// dropped entirely (including their modes and children — a Teen-rated
    /// parent with an Adult-rated child still hides that child). Parents
    /// that would end up with no modes and no surviving children are
    /// dropped too, so the tree never shows an empty category.
    /// </summary>
    public IReadOnlyList<Archetype> Apply(IReadOnlyList<Archetype> archetypes) =>
        archetypes
            .Where(a => a.AgeRating >= _minAgeRating && a.AgeRating <= _maxAgeRating)
            .Select(FilterNode)
            .Where(a => a.Modes.Count > 0 || a.SubArchetypes.Count > 0)
            .ToList();

    private Archetype FilterNode(Archetype a)
    {
        var children = Apply(a.SubArchetypes);
        var modes = a.Modes.Where(SuitsTable).ToList();
        return new Archetype(a.Id, a.Name, a.Description, a.Emoji, modes, children, a.AgeRating);
    }

    /// <summary>
    /// True when a mode is playable at the configured table.
    ///
    /// A mode that does not implement <see cref="ITableShapeMode"/> always
    /// passes — see the interface docs for why the default is permissive.
    /// </summary>
    private bool SuitsTable(IGameMode mode) =>
        mode is not ITableShapeMode shaped || shaped.SuitableFor.Suits(_tableShape);

    /// <summary>
    /// Convenience: counts how many playable modes survive the filter across
    /// the whole tree — handy for a settings screen showing "42 of 74 games
    /// available at this rating".
    /// </summary>
    public int CountSurvivingModes(IReadOnlyList<Archetype> archetypes)
    {
        var filtered = Apply(archetypes);
        return filtered.Sum(a => a.Modes.Count + CountSurvivingModes(a.SubArchetypes));
    }
}
