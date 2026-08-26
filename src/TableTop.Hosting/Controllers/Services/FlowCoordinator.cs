using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Progression;
using TableTop.Hosting.Events;

namespace TableTop.Hosting.Controllers.Services;

/// <summary>
/// Handles all flow-control commands (LevelUp, LevelDown, SpeedUp, SlowDown,
/// JumpTo, ResetFlow) and raises <see cref="FlowChangedEvent"/>, extracted from
/// <see cref="CardTurnController"/> to keep the main controller focused on the
/// turn loop.
/// </summary>
internal sealed class FlowCoordinator
{
    private readonly IFlowAwareProgressionStrategy? _flowStrategy;
    private readonly IReadOnlyList<IPlayer> _players;
    private readonly Action<FlowChangedEvent> _onFlowChanged;
    private readonly Func<int> _getRound;

    /// <inheritdoc />
    public bool SupportsFlow => _flowStrategy is not null;

    public FlowCoordinator(
        IFlowAwareProgressionStrategy? flowStrategy,
        IReadOnlyList<IPlayer> players,
        Action<FlowChangedEvent> onFlowChanged,
        Func<int> getRound)
    {
        _flowStrategy = flowStrategy;
        _players = players;
        _onFlowChanged = onFlowChanged;
        _getRound = getRound;
    }

    /// <inheritdoc />
    public FlowState? GetFlowState(Guid playerId) =>
        _flowStrategy?.GetFlowState(playerId);

    /// <inheritdoc />
    public void LevelUp(Guid playerId) => Apply(playerId, "LevelUp", s => s.LevelUp());
    /// <inheritdoc />
    public void LevelDown(Guid playerId) => Apply(playerId, "LevelDown", s => s.LevelDown());
    /// <inheritdoc />
    public void SpeedUp(Guid playerId) => Apply(playerId, "SpeedUp", s => s.SpeedUp());
    /// <inheritdoc />
    public void SlowDown(Guid playerId) => Apply(playerId, "SlowDown", s => s.SlowDown());

    /// <inheritdoc />
    public void JumpTo(Guid playerId, Difficulty difficulty)
    {
        if (_flowStrategy is null) return;
        var state = _flowStrategy.GetFlowState(playerId);
        if (state is null) return;
        state.SetDifficulty(difficulty);
        state.ResetLevelCounter();
        Raise(playerId, "JumpTo", state);
    }

    /// <inheritdoc />
    public void ResetFlow(Guid playerId)
    {
        if (_flowStrategy is null) return;
        var state = _flowStrategy.GetFlowState(playerId);
        if (state is null) return;
        state.SetDifficulty(Difficulty.Easy);
        state.SetPace(FlowPace.Normal);
        state.ResetLevelCounter();
        Raise(playerId, "Reset", state);
    }

    private void Apply(Guid playerId, string changeName, Func<FlowState, bool> mutation)
    {
        if (_flowStrategy is null) return;
        var state = _flowStrategy.GetFlowState(playerId);
        if (state is null) return;
        var changed = mutation(state);
        if (changed) Raise(playerId, changeName, state);
    }

    private void Raise(Guid playerId, string change, FlowState state)
    {
        var player = _players.FirstOrDefault(p => p.Id == playerId);
        if (player is null) return;
        _onFlowChanged(new FlowChangedEvent(
            PlayerName: player.DisplayName,
            Change: change,
            NewDifficulty: state.CurrentDifficulty.ToString(),
            NewPace: state.CurrentPace.ToString(),
            CardsBeforeEscalation: state.CardsBeforeEscalation,
            Round: _getRound()));
    }
}