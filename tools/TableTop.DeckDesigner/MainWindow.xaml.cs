using Microsoft.UI.Xaml;
using TableTop.DeckDesigner.ViewModels;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace TableTop.DeckDesigner;

/// <summary>The tool's only window. One screen, no navigation.</summary>
public sealed partial class MainWindow : Window
{
    /// <summary>The window's ViewModel, bound onto <c>RootGrid</c> since WinUI's <see cref="Window"/> itself has no DataContext.</summary>
    public DeckDesignerViewModel ViewModel { get; } = new();

    /// <summary>Initialises the window.</summary>
    public MainWindow()
    {
        InitializeComponent();
        Title = "TableTop Deck Designer";
        RootGrid.DataContext = ViewModel;
    }

    /// <summary>
    /// Saves the generated C# to a file the user picks. Kept in code-behind
    /// rather than a bound command: <see cref="FileSavePicker"/> needs this
    /// window's HWND (<see cref="WinRT.Interop.WindowNative.GetWindowHandle"/>),
    /// which only the View can supply without leaking a WinUI window type
    /// into the ViewModel.
    /// </summary>
    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.HasGeneratedCode)
            return;

        var picker = new FileSavePicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeChoices.Add("C# source", new List<string> { ".cs" });
        picker.SuggestedFileName = ViewModel.SuggestedFileName;

        var file = await picker.PickSaveFileAsync();
        if (file is null)
            return;

        await FileIO.WriteTextAsync(file, ViewModel.GeneratedCode);
    }
}
