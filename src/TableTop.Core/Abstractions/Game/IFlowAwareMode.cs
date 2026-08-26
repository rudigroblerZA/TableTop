namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Marker interface implemented by game modes that should be driven by
/// <c>FlowAwareProgressionStrategy</c> rather than the default
/// <c>DifficultyProgressionStrategy</c>.
///
/// Flow-aware progression tracks each player's current difficulty tier and pace
/// and auto-escalates as they succeed. It is best suited for modes where cards
/// form a clear learning scaffold (e.g. school literacy modes) rather than a
/// random entertainment mix.
///
/// Implementing this interface requires no factory changes — the
/// the controller factory dispatches on it automatically.
/// </summary>
public interface IFlowAwareMode { }
