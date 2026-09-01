using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui.Pages;

/// <summary>Screen for the Millionaire family of modes.</summary>
public partial class MillionaireGamePage : ContentPage, IAsyncInitializablePage
{
    private readonly IGameMode _gameMode;
    private readonly List<IPlayer> _players;
    private MillionaireGameViewModel _vm = null!;

    /// <summary>
    /// Builds the page for the chosen mode and players.
    ///
    /// Cheap on purpose — backlog item 20. The controller build used to
    /// happen here via a blocking <c>.GetAwaiter().GetResult()</c>; it now
    /// happens in <see cref="InitializeAsync"/>, which a caller must await
    /// before pushing this page.
    /// </summary>
    public MillionaireGamePage(IGameMode gameMode, List<IPlayer> players)
    {
        InitializeComponent();
        _gameMode = gameMode;
        _players = players;
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        _vm = await MillionaireGameViewModel.CreateAsync(
            new Services.MauiNavigator(this), _gameMode, _players,
            controllerFactory: Services.AppServices.ControllerFactory);
        BindingContext = _vm;
    }

    private void OnAnswerClicked(object sender, EventArgs e)
    {
        if (sender is Button { BindingContext: AnswerOption opt })
            opt.Invoke();
    }

    private void OnLifelineClicked(object sender, EventArgs e)
    {
        if (sender is Button { BindingContext: LifelineOption opt })
            opt.Invoke();
    }

    private void OnWalkAwayClicked(object sender, EventArgs e) => _vm.WalkAway();

    private async void OnDoneClicked(object sender, EventArgs e) => await this.SafePopToRootAsync();

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Dispose();
    }
}
