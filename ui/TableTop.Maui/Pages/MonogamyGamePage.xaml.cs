using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui.Pages;

/// <summary>Screen for the Monogamy couples mode.</summary>
public partial class MonogamyGamePage : ContentPage, IAsyncInitializablePage
{
    private readonly IGameMode _gameMode;
    private readonly List<IPlayer> _players;
    private MonogamyGameViewModel _vm = null!;

    /// <summary>
    /// Builds the page for the chosen mode and players.
    ///
    /// Cheap on purpose — same two-phase shape as <c>DayOneGamePage</c> and
    /// <c>MillionaireGamePage</c> (backlog item 20). The controller now
    /// builds through <c>IControllerFactory</c> in <see cref="InitializeAsync"/>,
    /// which a caller must await before pushing this page.
    /// </summary>
    public MonogamyGamePage(IGameMode gameMode, List<IPlayer> players)
    {
        InitializeComponent();
        _gameMode = gameMode;
        _players = players;
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        _vm = await MonogamyGameViewModel.CreateAsync(new Services.MauiNavigator(this), _gameMode, _players);
        BindingContext = _vm;
    }

    private void OnZoneClicked(object sender, EventArgs e)
    {
        if (sender is Button { BindingContext: MonogamyGameViewModel.ZoneOption opt })
            opt.Invoke();
    }

    private void OnCompleteClicked(object sender, EventArgs e) => _vm.Complete();
    private void OnNegotiateClicked(object sender, EventArgs e) => _vm.Negotiate();
    private void OnSkipClicked(object sender, EventArgs e) => _vm.Skip();

    private async void OnDoneClicked(object sender, EventArgs e) => await this.SafePopToRootAsync();

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Dispose();
    }
}
