using System.Windows.Input;

namespace TableTop.Presentation.Infrastructure;

/// <summary>
/// Synchronous <see cref="ICommand"/>. Requery is explicit via
/// <see cref="RaiseCanExecuteChanged"/> rather than an ambient
/// <c>CommandManager</c> — that type is WPF-only and exists on no platform this
/// project targets, which is exactly why this class is shareable.
/// </summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    /// <summary>Initialises the command with its action and optional guard.</summary>
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute    = execute;
        _canExecute = canExecute;
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    /// <inheritdoc />
    public void Execute(object? parameter) => _execute();

    /// <summary>Tells bound controls to re-evaluate <see cref="CanExecute"/>.</summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
