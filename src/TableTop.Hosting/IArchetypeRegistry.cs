using TableTop.Core.Abstractions.Game;

namespace TableTop.Hosting;

/// <summary>
/// Builds and owns the archetype tree.
/// </summary>
public interface IArchetypeRegistry
{
    /// <summary>Top-level archetypes shown on the selection screen.</summary>
    IReadOnlyList<Archetype> RootArchetypes { get; }

    /// <summary>Finds an archetype by stable id, searching the full tree. Returns null when not found.</summary>
    Archetype? FindById(string id);

    /// <summary>All modes in the registry, flattened across the full tree. Useful for search and bulk manifest inspection.</summary>
    IReadOnlyList<IGameMode> AllModes { get; }

    /// <summary>
    /// Picks a random mode from the entire registry.
    /// Returns null when no mode satisfies the constraints.
    /// </summary>
    IGameMode? SurpriseMe(
        AgeRating maxAgeRating      = AgeRating.Adult,
        bool      allowAdultContent = false,
        int?      maxCards          = null);
}
