using System.Globalization;
using System.Windows.Input;

namespace TableTop.Presentation.Infrastructure;

/// <summary>
/// An <see cref="ICommand"/> that takes the parameter the control passes it.
///
/// <para>
/// <see cref="RelayCommand"/> covers the overwhelming majority of this app's
/// actions, which are "do the one thing this button does". A Likert row is the
/// case it cannot express: five buttons that differ only in which value they
/// send, per player, per statement. Five separate commands per entry would work
/// and would be indefensible.
/// </para>
///
/// <para>
/// <b>The parameter is <see cref="object"/>, not a generic <c>T</c>, and that is
/// deliberate.</b> XAML passes <c>CommandParameter="3"</c> as a <b>string</b> on
/// both WinUI and MAUI — a <c>RelayCommand&lt;int&gt;</c> would compile, bind,
/// and then silently fail to execute at runtime because the string will not cast
/// to int. The native Android head passes a real <see cref="int"/> from code.
/// Accepting object and converting once, here, is what makes one command work
/// from all three call sites. <see cref="AsInt"/> is that conversion, exposed so
/// a caller can use the same lenient rule rather than reinventing it.
/// </para>
/// </summary>
public sealed class ParameterRelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    /// <summary>Initialises the command with its action and optional guard.</summary>
    public ParameterRelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute);
        _execute = execute;
        _canExecute = canExecute;
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    /// <inheritdoc />
    public void Execute(object? parameter) => _execute(parameter);

    /// <summary>Tells bound controls to re-evaluate <see cref="CanExecute"/>.</summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Reads a command parameter as an int, accepting the boxed int the Android
    /// head sends and the string XAML sends. Returns <paramref name="fallback"/>
    /// for anything else, so a malformed parameter is an ignored tap rather than
    /// a crash inside a bound command.
    /// </summary>
    public static int AsInt(object? parameter, int fallback = 0) => parameter switch
    {
        int i => i,
        string s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) => parsed,
        IConvertible c => TryConvert(c, fallback),
        _ => fallback,
    };

    private static int TryConvert(IConvertible value, int fallback)
    {
        try { return value.ToInt32(CultureInfo.InvariantCulture); }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
        {
            return fallback;
        }
    }
}
