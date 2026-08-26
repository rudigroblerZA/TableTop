using TableTop.Core.Abstractions.Game;

namespace TableTop.Hosting;

/// <summary>
/// A named grouping of game modes presented on the archetype selection screen.
///
/// Archetypes are the entry point of the UI — the player chooses their context
/// (Classroom, Fun, Couples…) before seeing the mode list. This keeps the game
/// selection focused and age-appropriate by default.
///
/// Archetypes can contain sub-archetypes for deeper categorisation, forming a
/// lightweight tree. Leaf nodes carry the actual <see cref="IGameMode"/> list.
/// </summary>
public sealed class Archetype
{
    /// <summary>Creates an archetype with the given identity and mode set.</summary>
    public Archetype(
        string id,
        string name,
        string description,
        string emoji,
        IReadOnlyList<IGameMode> modes,
        IReadOnlyList<Archetype>? subArchetypes = null,
        AgeRating ageRating = AgeRating.AllAges)
    {
        Id = id;
        Name = name;
        Description = description;
        Emoji = emoji;
        Modes = modes;
        SubArchetypes = subArchetypes ?? [];
        AgeRating = ageRating;
    }

    /// <summary>Stable machine identifier (e.g. "classroom", "couples.intimate").</summary>
    public string Id { get; }

    /// <summary>Display name shown on the archetype card.</summary>
    public string Name { get; }

    /// <summary>One-line description of this archetype's vibe.</summary>
    public string Description { get; }

    /// <summary>Emoji used as the archetype icon in UI.</summary>
    public string Emoji { get; }

    /// <summary>
    /// Modes directly available in this archetype.
    /// Empty when <see cref="SubArchetypes"/> contains all the structure.
    /// </summary>
    public IReadOnlyList<IGameMode> Modes { get; }

    /// <summary>
    /// Child archetypes for further categorisation.
    /// Leaf archetypes have an empty list here.
    /// </summary>
    public IReadOnlyList<Archetype> SubArchetypes { get; }

    /// <summary>Whether this archetype (and its modes) requires adult players.</summary>
    public AgeRating AgeRating { get; }

    /// <summary>
    /// All modes reachable from this archetype — own modes plus all sub-archetype modes,
    /// flattened. Useful for search or "play anything in this category".
    /// </summary>
    public IReadOnlyList<IGameMode> AllModes =>
        Modes.Concat(SubArchetypes.SelectMany(s => s.AllModes))
             .Distinct()
             .ToList()
             .AsReadOnly();

    /// <summary>True when this archetype has sub-archetypes rather than being a leaf.</summary>
    public bool HasSubArchetypes => SubArchetypes.Count > 0;

    // ── Manifest helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Returns the <see cref="ModeManifest"/> for every mode in this archetype,
    /// keyed by mode. Manifests are cached after the first call.
    /// Useful for powering richer selection UI (card counts, difficulty, play time).
    /// </summary>
    public IReadOnlyDictionary<IGameMode, ModeManifest> GetModeManifests() =>
        AllModes.GetManifests();

    /// <summary>
    /// Picks a random mode from this archetype that satisfies the given constraints.
    /// Returns null when no mode matches.
    /// </summary>
    /// <param name="maxCards">Upper bound on card count (null = no limit).</param>
    /// <param name="maxAgeRating">Maximum age rating to allow (null = any).</param>
    /// <param name="requiresAdultContent">
    /// When false (default), adult-only modes are excluded.
    /// When true, adult content is allowed.
    /// </param>
    public IGameMode? SurpriseMe(
        int? maxCards = null,
        AgeRating maxAgeRating = AgeRating.Adult,
        bool requiresAdultContent = false)
    {
        var candidates = AllModes
            .Where(m =>
            {
                var manifest = m.GetManifest();
                if (maxCards.HasValue && manifest.TotalCards > maxCards.Value) return false;
                if (!requiresAdultContent && manifest.HasAdultContent) return false;
                return true;
            })
            .ToList();

        if (candidates.Count == 0) return null;
        return candidates[Random.Shared.Next(candidates.Count)];
    }
}

/// <summary>Age appropriateness of an archetype.</summary>
public enum AgeRating
{
    /// <summary>Suitable for all ages.</summary>
    AllAges,

    /// <summary>Suitable for teenagers and adults (13+).</summary>
    Teen,

    /// <summary>Adults only (18+).</summary>
    Adult,
}
