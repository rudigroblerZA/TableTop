using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace TableTop.WinUI.Infrastructure;

/// <summary>
/// Converts a <see cref="bool"/> to <see cref="Visibility"/>. WinUI, unlike
/// WPF, ships no built-in BooleanToVisibilityConverter, so this is the
/// standard hand-rolled replacement. Pass ConverterParameter="invert" to
/// flip the sense (true → Collapsed).
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // Truthiness across the types actually bound in the views: a real
        // bool, a non-empty string (e.g. the error line), or a non-zero
        // count. Anything else is falsey.
        var flag = value switch
        {
            bool b       => b,
            string s2    => !string.IsNullOrWhiteSpace(s2),
            int i        => i != 0,
            null         => false,
            _            => true,
        };
        if (parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase))
            flag = !flag;
        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        var visible = value is Visibility v && v == Visibility.Visible;
        if (parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase))
            visible = !visible;
        return visible;
    }
}
