using TableTop.Core.Abstractions.Game;

namespace TableTop.Core.Domain.Game;

/// <summary>
/// The classic 15-rung prize ladder with safe havens at questions 5 and 10.
/// </summary>
public sealed class PrizeLadder : IPrizeLadder
{
    private static readonly IReadOnlyList<PrizeLadderRung> DefaultRungs =
    [
        new(1,         100,  false),
        new(2,         200,  false),
        new(3,         300,  false),
        new(4,         500,  false),
        new(5,       1_000,  true),   // ← safe haven
        new(6,       2_000,  false),
        new(7,       4_000,  false),
        new(8,       8_000,  false),
        new(9,      16_000,  false),
        new(10,     32_000,  true),   // ← safe haven
        new(11,     64_000,  false),
        new(12,    125_000,  false),
        new(13,    250_000,  false),
        new(14,    500_000,  false),
        new(15, 1_000_000,  false),
    ];

    private int _currentIndex;

    /// <summary>Initialises a new <see cref="PrizeLadder"/> instance.</summary>
    public PrizeLadder() : this(DefaultRungs) { }

    /// <summary>Initialises a new <see cref="PrizeLadder"/> instance.</summary>
    public PrizeLadder(IEnumerable<PrizeLadderRung> rungs)
    {
        Rungs = rungs.ToList().AsReadOnly();
        _currentIndex = 0;
    }

    /// <inheritdoc />
    public IReadOnlyList<PrizeLadderRung> Rungs { get; }

    /// <inheritdoc />
    public int CurrentRungIndex => _currentIndex;

    /// <inheritdoc />
    public PrizeLadderRung CurrentRung => Rungs[_currentIndex];

    /// <inheritdoc />
    public long GuaranteedPrize
    {
        get
        {
            // Walk backwards from current position to find the last safe haven reached
            for (var i = _currentIndex - 1; i >= 0; i--)
                if (Rungs[i].IsSafeHaven) return Rungs[i].PrizeAmount;
            return 0;
        }
    }

    /// <inheritdoc />
    public bool IsComplete => _currentIndex >= Rungs.Count;

    /// <inheritdoc />
    public void Advance()
    {
        if (IsComplete)
            throw new InvalidOperationException("Ladder is already complete.");
        _currentIndex++;
    }
}