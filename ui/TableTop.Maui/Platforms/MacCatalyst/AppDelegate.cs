using Foundation;

namespace TableTop.Maui;

/// <summary>Mac Catalyst application delegate — creates the MAUI app.</summary>
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
