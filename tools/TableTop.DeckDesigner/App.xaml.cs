using Microsoft.UI.Xaml;

namespace TableTop.DeckDesigner;

/// <summary>Application entry point. Single window, no composition root — this tool is one screen.</summary>
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
