namespace TableTop.Core.Abstractions.Progression;

/// <summary>
/// A progression strategy that is aware of and driven by a mutable <see cref="FlowState"/>.
/// Provides free-directional movement — level up, level down, speed up, slow down — without
/// rewiring the engine. The strategy reads the player's current <see cref="FlowState"/> on
/// every call to <see cref="IProgressionStrategy.SelectCandidate"/> so changes take effect immediately.
/// </summary>
public interface IFlowAwareProgressionStrategy : IProgressionStrategy
{
    /// <summary>
    /// The flow state for a specific player.
    /// Created automatically on first access; callers can also inject a pre-built state.
    /// </summary>
    FlowState GetFlowState(Guid playerId);

    /// <summary>
    /// Replaces the flow state for a player entirely.
    /// Useful for session restore or programmatic setup.
    /// </summary>
    void SetFlowState(Guid playerId, FlowState state);
}
