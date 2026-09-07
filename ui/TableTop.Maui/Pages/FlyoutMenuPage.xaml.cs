using TableTop.Maui.ViewModels;

namespace TableTop.Maui.Pages;

/// <summary>
/// The flyout drawer: header image, then Home, the archetypes, Roaster and
/// Settings.
///
/// <para>
/// This page raises <see cref="ItemSelected"/> rather than navigating itself.
/// The drawer does not own the detail pane — <see cref="AppFlyoutPage"/> does,
/// and it is the only thing that can both swap the detail and close the drawer.
/// Handing it the tapped row keeps that decision in one place instead of
/// splitting it across two pages.
/// </para>
/// </summary>
public partial class FlyoutMenuPage : ContentPage
{
    /// <summary>Raised when a row is tapped, with the row that was tapped.</summary>
    public event EventHandler<FlyoutMenuItem>? ItemSelected;

    /// <summary>DI constructor — the menu's rows come from the shared settings.</summary>
    public FlyoutMenuPage(FlyoutMenuViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    // Not async, so it cannot be an async void handler — there is nothing to
    // await here and nothing that can throw past the null check. The work this
    // would have awaited lives in AppFlyoutPage, which does guard it.
    private void OnItemTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is FlyoutMenuItem item)
            ItemSelected?.Invoke(this, item);
    }
}
