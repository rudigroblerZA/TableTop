using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Maui.ViewModels;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui.Pages;

/// <summary>Screen for the Day One couples campaign.</summary>
public partial class DayOneGamePage : ContentPage
{
    private readonly DayOneGameViewModel _vm;

    /// <summary>Builds the page for the chosen mode and players.</summary>
    public DayOneGamePage(IGameMode gameMode, List<IPlayer> players)
    {
        InitializeComponent();
        _vm = DayOneGameViewModel.CreateAsync(new Services.MauiNavigator(this), gameMode, players)
            .GetAwaiter().GetResult();
        BindingContext = _vm;
    }

    private void OnCompleteClicked(object sender, EventArgs e) => _vm.CompleteToday();

    private async void OnDoneClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Dispose();
    }
}
