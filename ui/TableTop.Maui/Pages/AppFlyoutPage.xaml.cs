using TableTop.Maui.ViewModels;

namespace TableTop.Maui.Pages;

/// <summary>
/// The head's root: a <see cref="FlyoutPage"/> whose drawer is
/// <see cref="FlyoutMenuPage"/> and whose detail is a
/// <see cref="NavigationPage"/>.
///
/// <para>
/// <b>The detail is always a NavigationPage, and that is load-bearing.</b>
/// Every existing page in this head navigates with
/// <c>Navigation.PushAsync</c> — player setup, gameplay, settings, the roster —
/// and <c>Navigation</c> resolves to the enclosing navigation stack. Put a bare
/// <c>ContentPage</c> in the detail and every one of those calls silently does
/// nothing. It is also what draws the hamburger: without a NavigationPage
/// toolbar there is no affordance to open the drawer on Android, and the only
/// way in is an edge swipe most players never try.
/// </para>
///
/// <para>
/// <b>Swapping the detail replaces the whole stack.</b> Choosing a category
/// from the drawer is a lateral move, not a step forward, so the previous
/// screen's back stack is discarded rather than accumulated. Returning Home
/// from three levels into a game should not walk back out through them.
/// </para>
/// </summary>
public partial class AppFlyoutPage : FlyoutPage
{
    private readonly IServiceProvider _services;

    /// <summary>DI constructor. Builds the drawer and opens on Home.</summary>
    public AppFlyoutPage(IServiceProvider services, FlyoutMenuPage menu)
    {
        InitializeComponent();
        _services = services;

        menu.ItemSelected += OnMenuItemSelected;
        Flyout = menu;

        ShowDetail(_services.GetRequiredService<HomePage>());
    }

    /// <summary>
    /// Puts <paramref name="page"/> in the detail pane, wrapped in a fresh
    /// navigation stack wearing the walnut bar the rest of the head uses.
    /// </summary>
    private void ShowDetail(Page page) =>
        Detail = new NavigationPage(page)
        {
            // Walnut over the felt, matching the framed-table shell the WinUI
            // head uses. This bar is on every screen, so it is the most visible
            // piece of chrome in the app.
            BarBackgroundColor = Color.FromArgb("#4A2E1D"),
            BarTextColor = Color.FromArgb("#E3C67F"),
        };

    // Not async: every branch below is synchronous. Making it async void to
    // match the other handlers would add a way for an exception to escape and
    // terminate the process on Android, for no benefit — there is nothing to
    // await. The try/catch is still here because resolving a page from the
    // container can throw, and this handler has no caller to catch it either.
    private void OnMenuItemSelected(object? sender, FlyoutMenuItem item)
    {
        try
        {
            switch (item.Destination)
            {
                case FlyoutDestination.Home:
                    ShowDetail(_services.GetRequiredService<HomePage>());
                    break;

                case FlyoutDestination.Archetype:
                    ShowArchetype(item.Archetype);
                    break;

                case FlyoutDestination.Roaster:
                    ShowDetail(new RoasterPage());
                    break;

                case FlyoutDestination.Settings:
                    ShowDetail(_services.GetRequiredService<SettingsPage>());
                    break;
            }
        }
        catch
        {
            // A destination that cannot be built must not take the app with it.
            // The drawer still closes below, leaving the current screen up.
        }
        finally
        {
            // Close the drawer whatever happened. On a phone it covers the
            // detail, so leaving it open after a tap reads as the tap having
            // been ignored.
            IsPresented = false;
        }
    }

    /// <summary>
    /// Opens the game picker with <paramref name="archetype"/> already chosen.
    ///
    /// <para>
    /// The picker is reused rather than replaced: setting
    /// <see cref="GameSelectionViewModel.SelectedArchetype"/> cascades into the
    /// variant and game lists exactly as tapping the category would, so the
    /// drawer becomes another way into the same flow instead of a second one
    /// to keep in step. The picker is a singleton, so the selection survives
    /// leaving and coming back.
    /// </para>
    /// </summary>
    private void ShowArchetype(TableTop.Hosting.Archetype? archetype)
    {
        var picker = _services.GetRequiredService<GameSelectionPage>();

        if (archetype is not null)
            _services.GetRequiredService<GameSelectionViewModel>().SelectedArchetype = archetype;

        ShowDetail(picker);
    }
}
