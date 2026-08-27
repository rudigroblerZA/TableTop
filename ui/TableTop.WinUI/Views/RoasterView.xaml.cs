using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using TableTop.Presentation.Infrastructure;
using TableTop.Presentation.ViewModels;
using Windows.System;

namespace TableTop.WinUI.Views;

/// <summary>Interaction logic for <see cref="RoasterView"/>.</summary>
public sealed partial class RoasterView : UserControl
{
    /// <summary>Initialises the view.</summary>
    public RoasterView() => InitializeComponent();

    // ItemClick + explicit command invocation is the reliable WinUI pattern
    // for a list of tappable tiles — same reasoning ArchetypePickerView
    // already documents. SelectedTemplate is a plain settable property, so
    // no dedicated select command is needed the way ArchetypePickerViewModel
    // needs one for its two-deep navigation.
    private void OnTemplateItemClick(object sender, ItemClickEventArgs e)
    {
        if (DataContext is RoasterViewModel vm && e.ClickedItem is RoasterTemplate t)
            vm.SelectedTemplate = t;
    }

    private void OnPlayerEntryKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && DataContext is RoasterViewModel vm)
            vm.AddPlayerCommand.Execute(null);
    }

    // Delete buttons live inside a DataTemplate, so a bound command would
    // resolve against the item's own DataContext (a SavedPlayer/SavedRoster),
    // not the page's RoasterViewModel — the same ElementName/namescope
    // problem ArchetypePickerView's own comment names. Reading the sender's
    // DataContext directly sidesteps it, same as PlayerSetupView's roster
    // list would need to if it ever grew a per-row action.

    private void OnRemovePlayerClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: SavedPlayer player } &&
            DataContext is RoasterViewModel vm)
            vm.RemovePlayer(player);
    }

    private void OnDeleteRosterClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: SavedRoster roster } &&
            DataContext is RoasterViewModel vm)
            vm.DeleteRoster(roster);
    }
}
