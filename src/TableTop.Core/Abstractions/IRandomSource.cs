namespace TableTop.Core.Abstractions;

/// <summary>
/// Abstracts all randomness used by the engine.
///
/// Inject a seeded implementation to make any game session fully reproducible:
/// replaying the same seed with the same player list and mode produces an identical
/// card sequence, shuffle order, and all stochastic outcomes — invaluable for
/// bug reproduction and for "share this game seed" features.
///
/// The default implementation wraps <see cref="System.Random.Shared"/> (unseeded,
/// non-deterministic) and is appropriate for all production scenarios where
/// reproducibility isn't required.
/// </summary>
public interface IRandomSource
{
    /// <summary>Returns a non-negative random integer less than <paramref name="maxValue"/>.</summary>
    int Next(int maxValue);

    /// <summary>Returns a random integer in the range [<paramref name="minValue"/>, <paramref name="maxValue"/>).</summary>
    int Next(int minValue, int maxValue);

    /// <summary>Returns a non-negative random integer.</summary>
    int Next();

    /// <summary>Returns a random double in the range [0.0, 1.0).</summary>
    double NextDouble();

    /// <summary>Fills the provided span with random bytes.</summary>
    void NextBytes(Span<byte> buffer);
}

/// <summary>
/// Production implementation — wraps <see cref="System.Random.Shared"/>.
/// Thread-safe via <c>Random.Shared</c> semantics.
/// </summary>
public sealed class SharedRandomSource : IRandomSource
{
    /// <summary>Singleton; no state to initialise.</summary>
    public static readonly SharedRandomSource Instance = new();

    private SharedRandomSource() { }

    /// <inheritdoc />
    public int Next(int maxValue) => Random.Shared.Next(maxValue);
    /// <inheritdoc />
    public int Next(int minValue, int maxValue) => Random.Shared.Next(minValue, maxValue);
    /// <inheritdoc />
    public int Next() => Random.Shared.Next();
    /// <inheritdoc />
    public double NextDouble() => Random.Shared.NextDouble();
    /// <inheritdoc />
    public void NextBytes(Span<byte> buffer) => Random.Shared.NextBytes(buffer);
}

/// <summary>
/// Seeded implementation — wraps a <see cref="System.Random"/> with a known seed.
/// Not thread-safe; each game session should own its own instance.
/// </summary>
public sealed class SeededRandomSource : IRandomSource
{
    private readonly Random _rng;

    /// <summary>The seed used to create this source (useful to log for replay).</summary>
    public int Seed { get; }

    /// <summary>Initialises a new <see cref="SeededRandomSource"/> instance.</summary>
    public SeededRandomSource(int seed)
    {
        Seed = seed;
        _rng = new Random(seed);
    }

    /// <inheritdoc />
    public int Next(int maxValue) => _rng.Next(maxValue);
    /// <inheritdoc />
    public int Next(int minValue, int maxValue) => _rng.Next(minValue, maxValue);
    /// <inheritdoc />
    public int Next() => _rng.Next();
    /// <inheritdoc />
    public double NextDouble() => _rng.NextDouble();
    /// <inheritdoc />
    public void NextBytes(Span<byte> buffer) => _rng.NextBytes(buffer);
}