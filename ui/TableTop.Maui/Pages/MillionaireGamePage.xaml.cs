using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui.Pages;

/// <summary>Screen for the Millionaire family of modes.</summary>
public partial class MillionaireGamePage : ContentPage
{
    private readonly MillionaireGameViewModel _vm;

    /// <summary>Builds the page for the chosen mode and players.</summary>
    public MillionaireGamePage(IGameMode gameMode, List<IPlayer> players)
    {
        InitializeComponent();
        _vm = MillionaireGameViewModel.CreateAsync(new Services.MauiNavigator(this), gameMode, players)
            .GetAwaiter().GetResult();
        BindingContext = _vm;
    }

    private void OnAnswerClicked(object sender, EventArgs e)
    {
        if (sender is Button { BindingContext: MillionaireGameViewModel.AnswerOption opt })
            opt.Invoke();
    }

    private void OnLifelineClicked(object sender, EventArgs e)
    {
        if (sender is Button { BindingContext: MillionaireGameViewModel.LifelineOption opt })
            opt.Invoke();
    }

    private void OnWalkAwayClicked(object sender, EventArgs e) => _vm.WalkAway();

    private async void OnDoneClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Dispose();
    }
}
