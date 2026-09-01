using System.Security.Cryptography;

namespace TableTop.Core.Abstractions;

/// <summary>
/// Abstracts all randomness used by the engine.
///
/// Inject a seeded implementation to make any game session fully reproducible:
/// replaying the same seed with the same player list and mode produces an identical
/// card sequence, shuffle order, and all stochastic outcomes — invaluable for
/// bug reproduction and for "share this game seed" features.
///
/// The default implementation wraps <see cref="RandomNumberGenerator"/> (unseeded,
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
/// Production implementation — wraps <see cref="RandomNumberGenerator"/>, a
/// cryptographically strong source. Thread-safe: every member is either a
/// static call or backed by one.
/// </summary>
public sealed class SharedRandomSource : IRandomSource
{
    /// <summary>Singleton; no state to initialise.</summary>
    public static readonly SharedRandomSource Instance = new();

    private SharedRandomSource() { }

    /// <inheritdoc />
    public int Next(int maxValue) => RandomNumberGenerator.GetInt32(maxValue);
    /// <inheritdoc />
    public int Next(int minValue, int maxValue) => RandomNumberGenerator.GetInt32(minValue, maxValue);
    /// <inheritdoc />
    public int Next() => RandomNumberGenerator.GetInt32(int.MaxValue);
    /// <inheritdoc />
    public double NextDouble()
    {
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        // Top 53 bits give a uniform double in [0, 1), matching the precision
        // System.Random.NextDouble provides.
        var bits = BitConverter.ToUInt64(bytes) >> 11;
        return bits / (double)(1UL << 53);
    }
    /// <inheritdoc />
    public void NextBytes(Span<byte> buffer) => RandomNumberGenerator.Fill(buffer);
}

/// <summary>
/// Seeded implementation — wraps a <see cref="System.Random"/> with a known seed.
/// Not thread-safe; each game session should own its own instance.
///
/// Deliberately non-cryptographic: reproducibility is the entire point of this
/// class ("share this game seed" / bug repro), and a cryptographically strong
/// generator cannot be seeded to reproduce a sequence. Nothing security-sensitive
/// is derived from it — only card order in a party game.
/// </summary>
public sealed class SeededRandomSource : IRandomSource
{
    private readonly Random _rng;

    /// <summary>The seed used to create this source (useful to log for replay).</summary>
    public int Seed { get; }

    /// <summary>Initialises a new <see cref="SeededRandomSource"/> instance.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Security", "S2245:Using pseudorandom number generators (PRNGs) is security-sensitive",
        Justification = "Deliberate. This type exists to make shuffles REPRODUCIBLE from a " +
                        "logged seed so a game can be replayed; a cryptographic RNG cannot be " +
                        "seeded and would defeat its only purpose. No security decision is " +
                        "made from these values.")]
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