using Foundation;

namespace TableTop.Maui;

/// <summary>iOS application delegate — creates the MAUI app.</summary>
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
