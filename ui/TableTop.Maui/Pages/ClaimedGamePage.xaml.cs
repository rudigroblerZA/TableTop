using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui.Pages;

/// <summary>Screen for the Claimed! area-control mode.</summary>
public partial class ClaimedGamePage : ContentPage, IAsyncInitializablePage
{
    private readonly IGameMode _gameMode;
    private readonly List<IPlayer> _players;
    private ClaimedGameViewModel _vm = null!;

    /// <summary>
    /// Builds the page for the chosen mode and players.
    ///
    /// Cheap on purpose — same two-phase shape as <c>DayOneGamePage</c> and
    /// <c>MillionaireGamePage</c> (backlog item 20). The controller now
    /// builds through <c>IControllerFactory</c> in <see cref="InitializeAsync"/>,
    /// which a caller must await before pushing this page.
    /// </summary>
    public ClaimedGamePage(IGameMode gameMode, List<IPlayer> players)
    {
        InitializeComponent();
        _gameMode = gameMode;
        _players = players;
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        _vm = await ClaimedGameViewModel.CreateAsync(new Services.MauiNavigator(this), _gameMode, _players);
        BindingContext = _vm;
    }

    private void OnChallengeClicked(object sender, EventArgs e)
    {
        if (sender is Button { BindingContext: ClaimedGameViewModel.TerritoryOption territory })
            territory.Invoke();
    }

    private void OnSucceedClicked(object sender, EventArgs e) => _vm.Succeed();
    private void OnFailClicked(object sender, EventArgs e) => _vm.Fail();

    private async void OnDoneClicked(object sender, EventArgs e) => await this.SafePopToRootAsync();

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Dispose();
    }
}
