using Microsoft.UI.Xaml;

namespace TableTop.WinUI;

/// <summary>WinUI application entry point.</summary>
public partial class App : Application
{
    private Window? _window;

    /// <summary>Initialises the application.</summary>
    public App() => InitializeComponent();

    /// <inheritdoc />
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
