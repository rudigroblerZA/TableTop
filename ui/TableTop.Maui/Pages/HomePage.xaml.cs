using TableTop.Maui.ViewModels;

namespace TableTop.Maui.Pages;

/// <summary>
/// The landing screen behind the flyout — Continue, Play, Settings, Roaster.
///
/// <para>
/// <b>It shares <see cref="GameSelectionViewModel"/> with the picker rather
/// than owning a second one.</b> That view model already holds the saved-session
/// lookup, and it is registered as a singleton, so both screens read one answer
/// to "is there a game to continue?". A second lookup would hit the same file
/// again and could disagree with the picker after a game ends.
/// </para>
/// </summary>
public partial class HomePage : ContentPage
{
    private readonly GameSelectionViewModel _vm;

    /// <summary>DI constructor.</summary>
    public HomePage(GameSelectionViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    // One navigation at a time. Two PushAsync calls in flight — trivially
    // caused by an impatient double-tap — throw, and an exception escaping an
    // async void handler terminates the process on Android.
    private bool _navigating;

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Re-checked on every appearance so finishing a game removes the stale
        // offer without a restart, and so returning here from the drawer shows
        // a current answer. Silent on failure: a resume offer that cannot be
        // built is not worth an alert on a screen the player just opened.
        try
        {
            await _vm.LookForSavedSessionAsync();
        }
        catch
        {
            // Leave the offer hidden; every other button still works.
        }
    }

    private async void OnResumeClicked(object? sender, EventArgs e)
    {
        if (_navigating) return;
        _navigating = true;
        try
        {
            if (_vm.Resumable is not { } r) return;

            // Straight to gameplay — the picker and player setup would only ask
            // again for what the snapshot already records.
            var page = new GameplayPage(r.Mode, r.Players.ToList(), r.Snapshot);
            await page.InitializeAsync();
            await Navigation.PushAsync(page);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Couldn't resume", ex.Message, "OK");
        }
        finally { _navigating = false; }
    }

    private async void OnPlayClicked(object? sender, EventArgs e)
    {
        if (_navigating) return;
        _navigating = true;
        try
        {
            var page = IPlatformApplication.Current!.Services.GetRequiredService<GameSelectionPage>();
            await Navigation.PushAsync(page);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Couldn't open the game picker", ex.Message, "OK");
        }
        finally { _navigating = false; }
    }

    private async void OnSettingsClicked(object? sender, EventArgs e)
    {
        if (_navigating) return;
        _navigating = true;
        try
        {
            var page = IPlatformApplication.Current!.Services.GetRequiredService<SettingsPage>();
            await Navigation.PushAsync(page);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Couldn't open settings", ex.Message, "OK");
        }
        finally { _navigating = false; }
    }

    private async void OnRoasterClicked(object? sender, EventArgs e)
    {
        if (_navigating) return;
        _navigating = true;
        try
        {
            await Navigation.PushAsync(new RoasterPage());
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Couldn't open the roaster", ex.Message, "OK");
        }
        finally { _navigating = false; }
    }
}
