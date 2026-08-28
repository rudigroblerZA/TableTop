using Android.Content;
using Android.Views;
using TableTop.Droid.Infrastructure;

namespace TableTop.Droid.Screens;

/// <summary>A dead-end screen that just shows a heading and a line of text — used
/// when a mode can't be started or has no screen for its family.</summary>
public sealed class MessageScreen(string title, string message) : Screen
{
    /// <inheritdoc />
    public override string Title => title;

    /// <inheritdoc />
    protected override View OnCreateView(Context context)
    {
        var column = Ui.Column(context);
        column.AddView(Ui.Heading(context, title));
        column.AddView(Ui.Label(context, message));

        var back = Ui.Button(context, "Back").OnClick(() => Navigator.GoBack());
        column.AddView(back);
        return column;
    }
}
