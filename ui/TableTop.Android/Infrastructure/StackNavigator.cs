using TableTop.Presentation.Infrastructure;

namespace TableTop.Droid.Infrastructure;

/// <summary>
/// <see cref="INavigator"/> over <see cref="MainActivity"/>'s screen stack.
///
/// The interface is deliberately just "go back" — the only navigation concept
/// every head can honestly express (see the interface's own doc comment).
/// Forward navigation is head-specific and done through <see cref="MainActivity.Push"/>
/// directly at the call sites that know where they are going.
/// </summary>
public sealed class StackNavigator(MainActivity activity) : INavigator
{
    /// <inheritdoc />
    public void GoBack() => activity.RunOnUiThread(activity.Pop);
}
