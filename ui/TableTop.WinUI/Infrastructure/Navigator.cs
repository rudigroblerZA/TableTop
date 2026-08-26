using TableTop.Presentation.Infrastructure;
namespace TableTop.WinUI.Infrastructure;

/// <summary>
/// Application-level navigation service.
/// Screens push/pop via Navigate() and GoBack(). The MainViewModel binds to CurrentViewModel.
///
/// <para>
/// Also WinUI's composition-root handle (backlog item 5). <c>Navigator</c> is
/// already threaded through every ViewModel in the app as <c>_navigator</c>,
/// so giving it <see cref="Services"/> is what makes the container reachable
/// from anywhere in that existing chain without adding a parameter to every
/// single ViewModel constructor along the way.
/// </para>
/// </summary>
public sealed class Navigator : ViewModelBase, TableTop.Presentation.Infrastructure.INavigator
{
    private readonly Stack<ViewModelBase> _stack = new();

    private ViewModelBase? _current;

    /// <summary>The app's composition root. Built once in App.xaml.cs.</summary>
    public IServiceProvider Services { get; }

    /// <summary>Initialises the navigator with the app's service provider.</summary>
    public Navigator(IServiceProvider services) => Services = services;

    public ViewModelBase? CurrentViewModel
    {
        get => _current;
        private set => SetField(ref _current, value);
    }

    public void Navigate(ViewModelBase viewModel)
    {
        if (_current is not null)
            _stack.Push(_current);
        CurrentViewModel = viewModel;
    }

    public void GoBack()
    {
        if (_stack.Count > 0)
            CurrentViewModel = _stack.Pop();
    }

    public void NavigateRoot(ViewModelBase viewModel)
    {
        _stack.Clear();
        CurrentViewModel = viewModel;
    }
}
