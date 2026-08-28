using System.ComponentModel;
using Android.App;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Droid.Infrastructure;

/// <summary>
/// Bridges a shared <see cref="ViewModelBase"/> to an Android view tree.
///
/// <para>
/// There is no XAML binding engine here, so each screen supplies one
/// <c>render</c> callback that reads every bound property off the ViewModel and
/// pushes it into its views. The binder calls <c>render</c> once immediately and
/// again — marshalled onto the UI thread — every time the ViewModel raises
/// <see cref="INotifyPropertyChanged.PropertyChanged"/>. A null/empty property
/// name (the "everything changed" signal) is handled the same way: re-render.
/// </para>
/// </summary>
public sealed class ViewModelBinder : IDisposable
{
    private readonly Activity _activity;
    private readonly ViewModelBase _viewModel;
    private readonly Action _render;

    /// <summary>Subscribes to <paramref name="viewModel"/> and runs <paramref name="render"/> once.</summary>
    public ViewModelBinder(Activity activity, ViewModelBase viewModel, Action render)
    {
        _activity = activity;
        _viewModel = viewModel;
        _render = render;
        _viewModel.PropertyChanged += OnPropertyChanged;
        _render();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        _activity.RunOnUiThread(_render);

    /// <inheritdoc />
    public void Dispose() => _viewModel.PropertyChanged -= OnPropertyChanged;
}
