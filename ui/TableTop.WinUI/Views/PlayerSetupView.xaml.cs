using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using TableTop.Presentation.ViewModels;
using Windows.System;

namespace TableTop.WinUI.Views;

/// <summary>Interaction logic for <see cref="PlayerSetupView"/>.</summary>
public sealed partial class PlayerSetupView : UserControl
{
    /// <summary>Initialises the view.</summary>
    public PlayerSetupView() => InitializeComponent();

    private void OnNameKeyDown(object sender, KeyRoutedEventArgs e)
    {
        // Enter adds the typed name — the expected keyboard flow on desktop.
        if (e.Key == VirtualKey.Enter && DataContext is PlayerSetupViewModel vm)
            vm.AddPlayerCommand.Execute(null);
    }
}
