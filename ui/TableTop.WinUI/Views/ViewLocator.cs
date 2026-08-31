using Microsoft.UI.Xaml;
using TableTop.Presentation.Infrastructure;
using TableTop.Presentation.ViewModels;
using TableTop.WinUI.ViewModels;

namespace TableTop.WinUI.Views;

/// <summary>
/// Maps ViewModel types to View instances. WinUI has no WPF-style implicit
/// DataTemplate-by-DataType resolution, so this explicit registry is the
/// idiomatic replacement: the shell asks it for a view whenever the
/// Navigator's current ViewModel changes.
/// </summary>
public static class ViewLocator
{
    private static readonly Dictionary<Type, Func<UIElement>> Map = new()
    {
        [typeof(IntroViewModel)] = () => new IntroView(),
        [typeof(ArchetypePickerViewModel)] = () => new ArchetypePickerView(),
        [typeof(SubArchetypePickerViewModel)] = () => new SubArchetypePickerView(),
        [typeof(GameSelectionViewModel)] = () => new GameSelectionView(),
        [typeof(PlayerSetupViewModel)] = () => new PlayerSetupView(),
        [typeof(CardTurnGameViewModel)] = () => new CardTurnGameView(),
        [typeof(MillionaireGameViewModel)] = () => new MillionaireGameView(),
        [typeof(MonogamyGameViewModel)] = () => new MonogamyGameView(),
        [typeof(DayOneGameViewModel)] = () => new DayOneGameView(),
        [typeof(ClaimedGameViewModel)] = () => new ClaimedGameView(),
        [typeof(HerdGameViewModel)] = () => new HerdGameView(),
        [typeof(TraitProfileGameViewModel)] = () => new TraitProfileGameView(),
        [typeof(SettingsViewModel)] = () => new SettingsView(),
        [typeof(RoasterViewModel)] = () => new RoasterView(),
        [typeof(UnsupportedModeViewModel)] = () => new UnsupportedModeView(),
    };

    /// <summary>Builds the view for <paramref name="viewModel"/> with its DataContext set.</summary>
    public static UIElement? Resolve(ViewModelBase? viewModel)
    {
        if (viewModel is null) return null;
        if (!Map.TryGetValue(viewModel.GetType(), out var make))
            throw new InvalidOperationException($"No view registered for {viewModel.GetType().Name}.");
        var view = make();
        if (view is FrameworkElement fe) fe.DataContext = viewModel;
        return view;
    }
}
