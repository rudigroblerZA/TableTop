namespace TableTop.Hosting.Abstractions;

/// <summary>
/// Base contract for all game controllers.
/// A controller owns the game loop state machine and raises typed events.
/// It has no knowledge of any UI — Console and WinUI both subscribe to the same events.
///
/// <para>
/// <strong>Lifecycle:</strong> call <see cref="IDisposable.Dispose"/> when the game ends
/// (naturally or via Quit) to unsubscribe internal event handlers and allow the
/// controller to be collected. UIs that subscribe to controller events should also
/// unsubscribe in their teardown before or after disposing.
/// </para>
/// </summary>
public interface IGameController : IDisposable
{
    /// <summary>True while the controller is running a game loop.</summary>
    bool IsRunning { get; }
}
