using TableTop.Maui.Services;
using TableTop.Presentation.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui.Pages;

/// <summary>
/// Three-column roster builder, reached from Settings: templates on the
/// left, the roster being configured in the middle, saved rosters on the
/// right.
///
/// Thin wrapper — <see cref="RoasterViewModel"/> is shared with WinUI. This
/// page's whole job is supplying the MAUI-specific <see cref="RosterStore"/>.
/// </summary>
public partial class RoasterPage : ContentPage
{
    private readonly RoasterViewModel _vm;

    public RoasterPage()
    {
        InitializeComponent();
        _vm = new RoasterViewModel(new Services.MauiNavigator(this), RosterStore.Instance);
        BindingContext = _vm;
    }

    // Selection handled in code-behind rather than a bound command, same
    // reasoning as GameSelectionPage: a Frame inside a CollectionView
    // DataTemplate eats the tap on Android before the CollectionView sees
    // it, and an {x:Reference} command binding doesn't resolve reliably out
    // of a DataTemplate's own namescope. Both the tap and SelectionChanged
    // route to the same setter, and the setter is idempotent, so having both
    // fire is harmless.

    private static T? ItemFrom<T>(object? sender) where T : class =>
        (sender as BindableObject)?.BindingContext as T;

    private void OnTemplateTapped(object? sender, TappedEventArgs e)
    {
        if (ItemFrom<RoasterTemplate>(sender) is { } template)
            _vm.SelectedTemplate = template;
    }

    private void OnTemplateSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is RoasterTemplate template)
            _vm.SelectedTemplate = template;
    }

    private void OnAddPlayerClicked(object sender, EventArgs e) => _vm.AddPlayer();

    private void OnRemovePlayerClicked(object sender, EventArgs e)
    {
        if (ItemFrom<SavedPlayer>(sender) is { } player)
            _vm.RemovePlayer(player);
    }

    private void OnSaveRosterClicked(object sender, EventArgs e) => _vm.SaveRoster();

    private void OnDeleteRosterClicked(object sender, EventArgs e)
    {
        if (ItemFrom<SavedRoster>(sender) is { } roster)
            _vm.DeleteRoster(roster);
    }
}
