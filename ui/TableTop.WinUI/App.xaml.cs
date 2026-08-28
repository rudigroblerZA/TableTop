using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using TableTop.Hosting.Extensions;
using TableTop.Presentation.Infrastructure;
using TableTop.WinUI.Infrastructure;

namespace TableTop.WinUI;

/// <summary>WinUI application entry point.</summary>
public partial class App : Application
{
    private Window? _window;

    /// <summary>
    /// The app's composition root (backlog item 5). Built once, here, rather
    /// than every screen hand-`new`-ing its own dependencies — see
    /// <see cref="Infrastructure.Navigator"/>, the one place downstream code
    /// actually reaches it.
    /// </summary>
    private readonly IServiceProvider _services;

    /// <summary>Initialises the application.</summary>
    public App()
    {
        InitializeComponent();

        // Explicit paths in WinUIAppPaths.DataDirectory (%LOCALAPPDATA%\TableTop) —
        // AddTableTopHosting's own defaults fall back to AppContext.BaseDirectory
        // (beside the executable), which an installed, unpackaged app cannot
        // reliably write to and which an update can wipe. See WinUIAppPaths.
        _services = new ServiceCollection()
            .AddTableTopHosting(
                sessionFilePath: Path.Combine(WinUIAppPaths.DataDirectory, "session.json"),
                playerFilePath: Path.Combine(WinUIAppPaths.DataDirectory, "players.json"))
            .AddSingleton<IAppSettings>(_ => WinUIAppSettings.Instance)
            .BuildServiceProvider();
    }

    /// <inheritdoc />
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow(_services);
        _window.Activate();
    }
}
