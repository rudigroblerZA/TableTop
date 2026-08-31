namespace TableTop.Core.Abstractions.Analysis;

/// <summary>
/// One dimension a trait instrument measures — the Big Five's "Openness", say.
///
/// <para>
/// <b>Why traits are string-keyed rather than an enum.</b> An enum would pin
/// the engine to one instrument. The whole point of this layer is that the
/// scoring is reusable: Big Five is the first instrument to use it, not the
/// only one it can express, and a five-value enum called <c>Trait</c> would
/// have to grow a member every time a mode wanted a sixth dimension — which is
/// precisely the "adding a capability interface means touching three switches"
/// problem <c>ControllerFamilies</c> exists to prevent. A key plus a
/// <see cref="TraitScale"/> that owns the set keeps the instrument as content,
/// where every other piece of content in this repo already lives.
/// </para>
/// </summary>
public sealed class TraitDefinition
{
    /// <summary>Initialises a new <see cref="TraitDefinition"/>.</summary>
    /// <param name="key">Stable identifier used by item weights and score lookup. Compared case-insensitively.</param>
    /// <param name="name">Display name shown to players.</param>
    /// <param name="lowLabel">What a low score on this dimension means, in plain words.</param>
    /// <param name="highLabel">What a high score means.</param>
    /// <param name="description">One or two sentences describing the dimension.</param>
    public TraitDefinition(
        string key,
        string name,
        string lowLabel,
        string highLabel,
        string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(lowLabel);
        ArgumentException.ThrowIfNullOrWhiteSpace(highLabel);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Key = key;
        Name = name;
        LowLabel = lowLabel;
        HighLabel = highLabel;
        Description = description;
    }

    /// <summary>Stable identifier used by item weights and score lookup.</summary>
    public string Key { get; }

    /// <summary>Display name shown to players.</summary>
    public string Name { get; }

    /// <summary>What a low score on this dimension means, in plain words.</summary>
    public string LowLabel { get; }

    /// <summary>What a high score on this dimension means, in plain words.</summary>
    public string HighLabel { get; }

    /// <summary>One or two sentences describing the dimension.</summary>
    public string Description { get; }
}
