using Microsoft.UI.Xaml.Controls;
using TableTop.WinUI.ViewModels;

namespace TableTop.WinUI.Views;

/// <summary>Interaction logic for <see cref="ArchetypePickerView"/>.</summary>
public sealed partial class ArchetypePickerView : UserControl
{
    /// <summary>Initialises the view.</summary>
    public ArchetypePickerView() => InitializeComponent();

    private void OnItemClick(object sender, ItemClickEventArgs e)
    {
        // ItemClick + explicit command invocation is the reliable WinUI
        // pattern; ElementName bindings into DataTemplates are not.
        if (DataContext is ArchetypePickerViewModel vm)
            vm.SelectCommand.Execute(e.ClickedItem);
    }
}
