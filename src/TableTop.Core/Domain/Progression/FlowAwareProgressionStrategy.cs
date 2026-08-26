using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Decks;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Progression;

namespace TableTop.Core.Domain.Progression;

/// <summary>
/// Selects cards according to each player's personal <see cref="FlowState"/>.
///
/// Behaviour per turn:
/// <list type="number">
///   <item>Look at the current player's difficulty level.</item>
///   <item>Try to find an unplayed card at that exact difficulty.</item>
///   <item>If none, widen the search one tier at a time (up then down).</item>
///   <item>After <see cref="FlowState.CardsBeforeEscalation"/> cards, auto-advance difficulty one step.</item>
///   <item>If already at Extreme, stay there (no wrap-around unless opt-in).</item>
/// </list>
///
/// Any call to <c>LevelUp</c>, <c>LevelDown</c>, <c>SpeedUp</c>, etc. on the player's
/// <see cref="FlowState"/> takes effect on the very next card draw — no restart needed.
/// </summary>
public sealed class FlowAwareProgressionStrategy : IFlowAwareProgressionStrategy
{
    private readonly Dictionary<Guid, FlowState> _states = [];
    private readonly Difficulty _initialDifficulty;
    private readonly FlowPace _initialPace;

    /// <summary>Initialises a new <see cref="FlowAwareProgressionStrategy"/> instance.</summary>
    public FlowAwareProgressionStrategy(
        Difficulty initialDifficulty = Difficulty.Easy,
        FlowPace initialPace = FlowPace.Normal)
    {
        _initialDifficulty = initialDifficulty;
        _initialPace = initialPace;
    }

    /// <inheritdoc />
    public string Name => "FlowAware";

    // ── IFlowAwareProgressionStrategy ────────────────────────────────────────

    /// <inheritdoc />
    public FlowState GetFlowState(Guid playerId)
    {
        if (!_states.TryGetValue(playerId, out var state))
        {
            state = new FlowState(_initialDifficulty, _initialPace);
            _states[playerId] = state;
        }
        return state;
    }

    /// <inheritdoc />
    public void SetFlowState(Guid playerId, FlowState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        _states[playerId] = state;
    }

    // ── IProgressionStrategy ─────────────────────────────────────────────────

    /// <inheritdoc />
    public Guid? SelectCandidate(IPlayer player, IDeck deck, IProgressionContext context)
    {
        var flow = GetFlowState(player.Id);
        var preferred = flow.CurrentDifficulty;

        // Peek only — no deck mutation
        var candidate = deck.Peek(c => c.Difficulty == preferred)
                     ?? PeekNearDifficulty(deck, preferred)
                     ?? deck.Peek();

        if (candidate is null) return null;

        // Auto-escalation: after N cards at this level, move up one tier
        if (flow.RecordCardPlayed())
        {
            var changed = flow.LevelUp();
            if (changed) flow.ResetLevelCounter();
        }

        return candidate.Id;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ICard? PeekNearDifficulty(IDeck deck, Difficulty preferred)
    {
        var all = Enum.GetValues<Difficulty>()
            .OrderBy(d => Math.Abs((int)d - (int)preferred))
            .Skip(1); // already tried preferred

        foreach (var d in all)
        {
            var card = deck.Peek(c => c.Difficulty == d);
            if (card is not null) return card;
        }
        return null;
    }
}