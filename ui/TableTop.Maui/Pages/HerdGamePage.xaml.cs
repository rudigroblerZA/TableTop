using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui.Pages;

/// <summary>Screen for the Herd simultaneous-answer mode.</summary>
public partial class HerdGamePage : ContentPage
{
    private readonly HerdGameViewModel _vm;

    /// <summary>Builds the page for the chosen mode and players.</summary>
    public HerdGamePage(IGameMode gameMode, List<IPlayer> players)
    {
        InitializeComponent();
        _vm = HerdGameViewModel.Create(new Services.MauiNavigator(this), gameMode, players);
        BindingContext = _vm;
    }

    private void OnRevealClicked(object sender, EventArgs e) => _vm.Reveal();
    private void OnDismissLastRoundClicked(object sender, EventArgs e) => _vm.DismissLastRound();

    private async void OnDoneClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Dispose();
    }
}
