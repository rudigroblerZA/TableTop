using Microsoft.UI.Xaml;
using TableTop.WinUI.Infrastructure;
using TableTop.WinUI.ViewModels;
using TableTop.WinUI.Views;

namespace TableTop.WinUI;

/// <summary>Root window: hosts whichever view the Navigator's current ViewModel maps to.</summary>
public sealed partial class MainWindow : Window
{
    private readonly Navigator _navigator = new();

    /// <summary>Initialises the window and navigates to the intro screen.</summary>
    public MainWindow()
    {
        InitializeComponent();
        Title = "TableTop";
        _navigator.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Navigator.CurrentViewModel))
                Host.Content = ViewLocator.Resolve(_navigator.CurrentViewModel);
        };
        _navigator.Navigate(new IntroViewModel(_navigator));
    }
}
