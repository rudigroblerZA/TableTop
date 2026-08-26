using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Maui.ViewModels;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui.Pages;

/// <summary>Screen for the Monogamy couples mode.</summary>
public partial class MonogamyGamePage : ContentPage
{
    private readonly MonogamyGameViewModel _vm;

    /// <summary>Builds the page for the chosen mode and players.</summary>
    public MonogamyGamePage(IGameMode gameMode, List<IPlayer> players)
    {
        InitializeComponent();
        _vm = MonogamyGameViewModel.Create(new Services.MauiNavigator(this), gameMode, players);
        BindingContext = _vm;
    }

    private void OnZoneClicked(object sender, EventArgs e)
    {
        if (sender is Button { BindingContext: MonogamyGameViewModel.ZoneOption opt })
            opt.Invoke();
    }

    private void OnCompleteClicked(object sender, EventArgs e)  => _vm.Complete();
    private void OnNegotiateClicked(object sender, EventArgs e) => _vm.Negotiate();
    private void OnSkipClicked(object sender, EventArgs e)      => _vm.Skip();

    private async void OnDoneClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Dispose();
    }
}
