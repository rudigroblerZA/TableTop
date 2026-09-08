using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace TableTop.DeckDesigner.Infrastructure;

/// <summary>
/// Converts a truthy value (<see cref="bool"/>, a non-null object, a
/// non-empty string) to <see cref="Visibility"/>. WinUI ships no built-in
/// BooleanToVisibilityConverter. Used both for "is a card selected"
/// (bound directly to the <c>CardRowViewModel</c> object) and for
/// "is this a this-or-that card" (bound to the <c>bool</c> flag).
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var flag = value switch
        {
            bool b => b,
            string s => !string.IsNullOrWhiteSpace(s),
            null => false,
            _ => true,
        };
        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        value is Visibility v && v == Visibility.Visible;
}
