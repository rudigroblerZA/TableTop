using System.Windows.Input;

namespace TableTop.Presentation.Infrastructure;

/// <summary>
/// Async <see cref="ICommand"/> that disables itself while running and
/// routes failures to an optional error handler instead of crashing the
/// dispatcher. Requery is explicit — no ambient CommandManager on any target.
/// </summary>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private readonly Action<Exception>? _onError;
    private bool _isRunning;

    /// <summary>Initialises the command with its async action, guard, and error sink.</summary>
    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null, Action<Exception>? onError = null)
    {
        _execute = execute;
        _canExecute = canExecute;
        _onError = onError;
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => !_isRunning && (_canExecute?.Invoke() ?? true);

    /// <inheritdoc />
    public async void Execute(object? parameter)
    {
        _isRunning = true;
        RaiseCanExecuteChanged();
        try { await _execute().ConfigureAwait(true); }
        catch (Exception ex) { _onError?.Invoke(ex); }
        finally
        {
            _isRunning = false;
            RaiseCanExecuteChanged();
        }
    }

    /// <summary>Tells bound controls to re-evaluate <see cref="CanExecute"/>.</summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
