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

        _services = new ServiceCollection()
            .AddTableTopHosting()
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
