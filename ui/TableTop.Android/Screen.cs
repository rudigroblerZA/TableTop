using Android.Content;
using Android.Views;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Droid;

/// <summary>
/// One screen in the head. This is the Android equivalent of a WinUI View +
/// ViewModel pair: a lightweight object that builds a <see cref="View"/> tree
/// and owns whatever shared ViewModel drives it.
///
/// <para>
/// <see cref="MainActivity"/> keeps a stack of these and swaps their views into
/// a single container — no <c>Fragment</c>, no AndroidX. The view is built once
/// and cached, so returning to a screen after a pop does not rebuild its
/// ViewModel (which for a gameplay screen would restart the game).
/// </para>
/// </summary>
public abstract class Screen
{
    private View? _view;

    /// <summary>The hosting activity. Set by <see cref="MainActivity.Push"/> before the view is built.</summary>
    protected MainActivity Host { get; private set; } = null!;

    /// <summary>A back-only navigator scoped to this screen.</summary>
    protected INavigator Navigator { get; private set; } = null!;

    /// <summary>Title shown in the top bar.</summary>
    public abstract string Title { get; }

    internal void Attach(MainActivity host, INavigator navigator)
    {
        Host = host;
        Navigator = navigator;
    }

    internal View GetView(Context context) => _view ??= OnCreateView(context);

    /// <summary>Builds this screen's view tree. Called once; the result is cached.</summary>
    protected abstract View OnCreateView(Context context);

    /// <summary>Called when the screen is popped off the stack. Override to dispose ViewModels and timers.</summary>
    public virtual void OnRemoved()
    {
    }
}
