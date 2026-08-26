using Microsoft.UI.Xaml;

namespace TableTop.Maui.WinUI;

/// <summary>
/// WinUI bootstrapper. <see cref="MauiWinUIApplication"/> supplies the generated
/// Main entry point for the net10.0-windows target — without this class the
/// build fails with CS5001 "no static 'Main' method".
/// </summary>
public partial class App : MauiWinUIApplication
{
    public App() => InitializeComponent();

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
