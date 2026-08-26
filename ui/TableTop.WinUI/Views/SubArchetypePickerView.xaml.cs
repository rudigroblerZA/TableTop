using Microsoft.UI.Xaml.Controls;
using TableTop.WinUI.ViewModels;

namespace TableTop.WinUI.Views;

/// <summary>Interaction logic for <see cref="SubArchetypePickerView"/>.</summary>
public sealed partial class SubArchetypePickerView : UserControl
{
    /// <summary>Initialises the view.</summary>
    public SubArchetypePickerView() => InitializeComponent();

    private void OnItemClick(object sender, ItemClickEventArgs e)
    {
        if (DataContext is SubArchetypePickerViewModel vm)
            vm.SelectCommand.Execute(e.ClickedItem);
    }
}
