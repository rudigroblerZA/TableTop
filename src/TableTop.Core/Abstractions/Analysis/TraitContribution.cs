namespace TableTop.Core.Abstractions.Analysis;

/// <summary>
/// What one response added to one trait, and the range it could have added.
///
/// <para>
/// The bounds travel with the value rather than being recomputed later because
/// only the strategy knows them. A caller that tried to derive the range would
/// have to re-implement the keying and loading rules to do it, which is the
/// duplication that lets a normalisation drift out of step with the scoring it
/// is meant to normalise.
/// </para>
/// </summary>
public sealed class TraitContribution
{
    /// <summary>Initialises a new <see cref="TraitContribution"/>.</summary>
    /// <param name="value">What this response actually contributed.</param>
    /// <param name="minimum">The least it could have contributed, over every possible response.</param>
    /// <param name="maximum">The most it could have contributed.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maximum"/> is below <paramref name="minimum"/>.</exception>
    public TraitContribution(double value, double minimum, double maximum)
    {
        if (maximum < minimum)
            throw new ArgumentOutOfRangeException(nameof(maximum),
                $"maximum ({maximum}) is below minimum ({minimum}).");

        Value = value;
        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>What this response actually contributed.</summary>
    public double Value { get; }

    /// <summary>The least this item could have contributed to this trait.</summary>
    public double Minimum { get; }

    /// <summary>The most this item could have contributed to this trait.</summary>
    public double Maximum { get; }
}
