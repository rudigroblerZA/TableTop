using TableTop.Maui.Services;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _vm;

    public SettingsPage()
    {
        InitializeComponent();
        _vm = new SettingsViewModel(new MauiNavigator(this), Services.AppSettings.Instance);
        BindingContext = _vm;

        // Apply theme change immediately when ThemeIndex changes
        AppSettings.Instance.Changed += OnSettingChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        AppSettings.Instance.Changed -= OnSettingChanged;
    }

    private void OnSettingChanged(object? sender, string key)
    {
        if (key is nameof(AppSettings.Theme) or "*")
            ApplyTheme();
    }

    private void ApplyTheme()
    {
        Application.Current!.UserAppTheme = AppSettings.Instance.Theme switch
        {
            "light" => AppTheme.Light,
            "system" => AppTheme.Unspecified,
            _ => AppTheme.Dark,
        };
    }

    // One navigation at a time, same as GameSelectionPage. Two PushAsync
    // calls in flight — trivially caused by an impatient double-tap — throw,
    // and an exception escaping an async void handler terminates the process
    // on Android rather than being caught anywhere useful.
    private bool _navigating;

    private async void OnRoasterClicked(object sender, EventArgs e)
    {
        if (_navigating) return;
        _navigating = true;
        try
        {
            await Navigation.PushAsync(new RoasterPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Couldn't open the roster builder", ex.Message, "OK");
        }
        finally { _navigating = false; }
    }

    private async void OnResetClicked(object sender, EventArgs e)
    {
        // An exception escaping an async void handler terminates the
        // process on Android; surface it instead.
        try
        {
            bool confirmed = await DisplayAlert(
                "Reset Settings",
                "This will reset all settings to their defaults. Are you sure?",
                "Reset",
                "Cancel");

            if (!confirmed)
                return;

            _vm.ResetToDefaults();
            ApplyTheme();

            await DisplayAlert("Done", "Settings have been reset to defaults.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Couldn't reset settings", ex.Message, "OK");
        }
    }
}
