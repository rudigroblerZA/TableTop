using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace TableTop.DeckDesigner.Infrastructure;

/// <summary>
/// Converts <c>DeckDesignerViewModel.StatusIsError</c> to a foreground brush
/// for the status line — red on a compile failure, the default text colour
/// otherwise. Builds the brushes directly rather than via a
/// <c>StaticResource</c> lookup, since this tool defines no themed palette
/// of its own to look one up in.
/// </summary>
public sealed class StatusToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush ErrorBrush = new(Colors.Firebrick);
    private static readonly SolidColorBrush NormalBrush = new(Colors.SeaGreen);

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is true ? ErrorBrush : NormalBrush;

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
