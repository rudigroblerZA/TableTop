using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui.Pages;

/// <summary>Screen for the Claimed! area-control mode.</summary>
public partial class ClaimedGamePage : ContentPage
{
    private readonly ClaimedGameViewModel _vm;

    /// <summary>Builds the page for the chosen mode and players.</summary>
    public ClaimedGamePage(IGameMode gameMode, List<IPlayer> players)
    {
        InitializeComponent();
        _vm = ClaimedGameViewModel.Create(new Services.MauiNavigator(this), gameMode, players);
        BindingContext = _vm;
    }

    private void OnChallengeClicked(object sender, EventArgs e)
    {
        if (sender is Button { BindingContext: ClaimedGameViewModel.TerritoryOption territory })
            territory.Invoke();
    }

    private void OnSucceedClicked(object sender, EventArgs e) => _vm.Succeed();
    private void OnFailClicked(object sender, EventArgs e)    => _vm.Fail();

    private async void OnDoneClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Dispose();
    }
}
