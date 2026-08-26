using TableTop.Presentation.Infrastructure;
namespace TableTop.WinUI.Infrastructure;

/// <summary>
/// Application-level navigation service.
/// Screens push/pop via Navigate() and GoBack(). The MainViewModel binds to CurrentViewModel.
/// </summary>
public sealed class Navigator : ViewModelBase, TableTop.Presentation.Infrastructure.INavigator
{
    private readonly Stack<ViewModelBase> _stack = new();

    private ViewModelBase? _current;

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
