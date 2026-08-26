using TableTop.Maui.Pages;
using TableTop.Maui.Services;

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

        // Apply saved theme before any page renders
        UserAppTheme = AppSettings.Instance.Theme switch
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
        var root = _services.GetRequiredService<GameSelectionPage>();

        return new Window(new NavigationPage(root)
        {
            // Walnut bar over the felt table, matching the framed-table shell
            // the WPF and WinUI heads use. This shows on every page, so it was
            // the most visible thing still wearing the pre-mock palette.
            BarBackgroundColor = Color.FromArgb("#4A2E1D"),
            BarTextColor = Color.FromArgb("#E3C67F"),
        });
    }
}
