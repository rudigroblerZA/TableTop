using TableTop.Maui.Pages;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Maui;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    /// <summary>
    /// IMPORTANT: inject IServiceProvider here, never a Page.
    ///
    /// A Page constructor-injected into App is built by DI BEFORE this
    /// constructor body runs — i.e. before InitializeComponent() has loaded
    /// App.xaml's ResourceDictionary. Any {StaticResource} in that page then
    /// throws XamlParseException ("StaticResource not found for key …") at
    /// startup. Resolving the page in CreateWindow instead guarantees the
    /// application resources exist first.
    /// </summary>
    public App(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;

        // Apply saved theme before any page renders. Resolved from the
        // container rather than AppSettings.Instance — a custom IAppSettings
        // registered in MauiProgram had no effect on this read before
        // (backlog item 19).
        UserAppTheme = _services.GetRequiredService<IAppSettings>().Theme switch
        {
            "light" => AppTheme.Light,
            "system" => AppTheme.Unspecified,
            _ => AppTheme.Dark,
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Resources are loaded (ctor ran InitializeComponent) — pages may now
        // safely use {StaticResource HeaderLabelStyle} and friends.
        //
        // The root is the FlyoutPage, which owns both the drawer and the
        // NavigationPage that used to be the root here. The walnut bar moved
        // with it: AppFlyoutPage applies it to every detail stack it builds,
        // because swapping the detail replaces that NavigationPage.
        //
        // A FlyoutPage must be the window's root — it cannot be pushed onto a
        // navigation stack — so this is the one place it can be set.
        return new Window(_services.GetRequiredService<AppFlyoutPage>());
    }
}
