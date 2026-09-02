using TableTop.Core.Abstractions.Game;
using TableTop.Hosting;

namespace TableTop.WinUI.Infrastructure;

/// <summary>
/// Wraps a real <see cref="IArchetypeRegistry"/> and applies an age-rating
/// floor via <see cref="ArchetypeFilter"/> — the WinUI counterpart to MAUI's
/// filtered <c>GameSelectionViewModel</c>, but done once at the registry
/// level here since WinUI's picker screens are already a chain
/// (<c>ArchetypePickerViewModel</c> → <c>SubArchetypePickerViewModel</c> →
/// <c>GameSelectionViewModel</c>) that all read from one injected registry —
/// filtering it once means none of those three need to know filtering exists.
/// </summary>
public sealed class FilteredArchetypeRegistry : IArchetypeRegistry
{
    private readonly IReadOnlyList<Archetype> _filteredRoots;

    /// <param name="inner">The real registry to filter.</param>
    /// <param name="minAgeRating">Floor — archetypes rated below this are hidden.</param>
    public FilteredArchetypeRegistry(IArchetypeRegistry inner, AgeRating minAgeRating)
    {
        // The wrapped registry is not retained: ArchetypeFilter.Apply returns
        // deep-filtered copies, so _filteredRoots is a complete tree and every
        // member below answers from it. Keeping a field for the unfiltered
        // registry would only offer a way to route around the floor.
        _filteredRoots = new ArchetypeFilter(minAgeRating: minAgeRating, maxAgeRating: AgeRating.Adult)
            .Apply(inner.RootArchetypes);
    }

    /// <inheritdoc />
    public IReadOnlyList<Archetype> RootArchetypes => _filteredRoots;

    /// <inheritdoc />
    public Archetype? FindById(string id) => FindRecursive(_filteredRoots, id);

    private static Archetype? FindRecursive(IReadOnlyList<Archetype> nodes, string id)
    {
        foreach (var n in nodes)
        {
            if (n.Id == id) return n;
            var found = FindRecursive(n.SubArchetypes, id);
            if (found is not null) return found;
        }
        return null;
    }

    /// <inheritdoc />
    public IReadOnlyList<IGameMode> AllModes =>
        _filteredRoots.SelectMany(FlattenModes).ToList();

    private static IEnumerable<IGameMode> FlattenModes(Archetype a) =>
        a.Modes.Concat(a.SubArchetypes.SelectMany(FlattenModes));

    /// <inheritdoc />
    public IGameMode? SurpriseMe(
        AgeRating maxAgeRating = AgeRating.Adult,
        bool allowAdultContent = false,
        int? maxCards = null)
    {
        // Delegating to the wrapped registry's SurpriseMe would only respect
        // the CEILING parameter — it has no concept of this wrapper's floor,
        // so it could still surprise the player with something the floor was
        // set to hide. Picking directly from AllModes (already floor+ceiling
        // filtered by ArchetypeFilter in the constructor) is correct by
        // construction instead.
        //
        // allowAdultContent is deliberately NOT re-checked here: this
        // wrapper's floor already IS the adult-content policy for this
        // registry instance — if the caller configured a Teen or Adult
        // floor, everything in AllModes was already accepted at that
        // maturity level. Re-excluding Adult-rated modes when
        // allowAdultContent defaults to false would make the pool empty by
        // construction the moment someone sets an Adult floor (every
        // surviving mode IS Adult-rated at that point) — the opposite of
        // what the floor was configured to do.
        var candidates = AllModes
            .Where(m => GetAgeRating(m) <= maxAgeRating)
            .Where(m => maxCards is null || m.GetManifest().TotalCards <= maxCards)
            .ToList();

        return candidates.Count == 0 ? null : candidates[Random.Shared.Next(candidates.Count)];
    }

    private AgeRating GetAgeRating(IGameMode mode) =>
        _filteredRoots.SelectMany(FlattenWithRating)
            .FirstOrDefault(t => t.mode == mode).rating;

    private static IEnumerable<(IGameMode mode, AgeRating rating)> FlattenWithRating(Archetype a) =>
        a.Modes.Select(m => (m, a.AgeRating))
            .Concat(a.SubArchetypes.SelectMany(FlattenWithRating));
}
