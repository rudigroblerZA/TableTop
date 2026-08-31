namespace TableTop.Core.Abstractions.Analysis;

/// <summary>
/// A named instrument: the complete, ordered set of dimensions a trait
/// assessment reports on.
///
/// <para>
/// <b>Uniqueness is enforced in the constructor, not assumed.</b> Two
/// definitions sharing a key would give a profile two scores under one name,
/// and whichever one a lookup returned would depend on dictionary insertion
/// order — a bug that reads as a display glitch and is actually a scoring
/// error. Throwing at construction makes a malformed instrument impossible to
/// build rather than something a mode discovers at the results screen.
/// </para>
/// </summary>
public sealed class TraitScale
{
    private readonly Dictionary<string, TraitDefinition> _byKey;

    /// <summary>Initialises a new <see cref="TraitScale"/>.</summary>
    /// <param name="name">Display name of the instrument (e.g. "Big Five").</param>
    /// <param name="traits">The dimensions it reports on. Keys must be unique, case-insensitively.</param>
    /// <exception cref="ArgumentException">A key is duplicated, or the set is empty.</exception>
    public TraitScale(string name, IEnumerable<TraitDefinition> traits)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(traits);

        var ordered = traits.ToList();
        if (ordered.Count == 0)
            throw new ArgumentException("A trait scale needs at least one dimension.", nameof(traits));

        _byKey = new Dictionary<string, TraitDefinition>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in ordered)
        {
            ArgumentNullException.ThrowIfNull(t);
            if (!_byKey.TryAdd(t.Key, t))
                throw new ArgumentException(
                    $"Duplicate trait key '{t.Key}' in scale '{name}'. Keys are compared " +
                    "case-insensitively and must be unique — two dimensions sharing a key " +
                    "would silently merge into one score.", nameof(traits));
        }

        Name = name;
        Traits = ordered.AsReadOnly();
    }

    /// <summary>Display name of the instrument.</summary>
    public string Name { get; }

    /// <summary>The dimensions, in the order the instrument reports them.</summary>
    public IReadOnlyList<TraitDefinition> Traits { get; }

    /// <summary>True when <paramref name="key"/> names a dimension in this scale.</summary>
    public bool Contains(string key) =>
        !string.IsNullOrWhiteSpace(key) && _byKey.ContainsKey(key);

    /// <summary>
    /// The definition for <paramref name="key"/>, or <c>null</c> when this scale
    /// has no such dimension. Returns null rather than throwing so a renderer
    /// can skip an unknown key instead of taking the results screen down.
    /// </summary>
    public TraitDefinition? Find(string key) =>
        string.IsNullOrWhiteSpace(key) ? null : _byKey.GetValueOrDefault(key);
}
