using Android.Content;
using Android.Views;
using TableTop.Droid.Infrastructure;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Droid.Screens;

/// <summary>
/// Shared plumbing for the gameplay screens: holds the shared ViewModel, wires a
/// <see cref="ViewModelBinder"/> after the view is built, and disposes both when
/// the screen is popped.
/// </summary>
public abstract class GameScreenBase<TViewModel>(TViewModel viewModel) : Screen
    where TViewModel : ViewModelBase
{
    private ViewModelBinder? _binder;

    /// <summary>The shared ViewModel driving this screen.</summary>
    protected TViewModel Vm { get; } = viewModel;

    /// <inheritdoc />
    protected sealed override View OnCreateView(Context context)
    {
        var view = Build(context);
        _binder = new ViewModelBinder(Host, Vm, Render);
        return view;
    }

    /// <summary>Builds the static view tree. Dynamic state is pushed in by <see cref="Render"/>.</summary>
    protected abstract View Build(Context context);

    /// <summary>Reads the ViewModel and updates the views. Called on the UI thread.</summary>
    protected abstract void Render();

    /// <inheritdoc />
    public override void OnRemoved()
    {
        _binder?.Dispose();
        (Vm as IDisposable)?.Dispose();
    }
}
