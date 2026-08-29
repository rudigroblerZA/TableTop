using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui.Pages;

/// <summary>Screen for the Day One couples campaign.</summary>
public partial class DayOneGamePage : ContentPage, IAsyncInitializablePage
{
    private readonly IGameMode _gameMode;
    private readonly List<IPlayer> _players;
    private DayOneGameViewModel _vm = null!;

    /// <summary>
    /// Builds the page for the chosen mode and players.
    ///
    /// Cheap on purpose — backlog item 20. The controller build used to
    /// happen here via a blocking <c>.GetAwaiter().GetResult()</c>; it now
    /// happens in <see cref="InitializeAsync"/>, which a caller must await
    /// before pushing this page.
    /// </summary>
    public DayOneGamePage(IGameMode gameMode, List<IPlayer> players)
    {
        InitializeComponent();
        _gameMode = gameMode;
        _players = players;
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        _vm = await DayOneGameViewModel.CreateAsync(
            new Services.MauiNavigator(this), _gameMode, _players,
            controllerFactory: Services.AppServices.ControllerFactory);
        BindingContext = _vm;
    }

    private void OnCompleteClicked(object sender, EventArgs e) => _vm.CompleteToday();

    private async void OnDoneClicked(object sender, EventArgs e) => await this.SafePopToRootAsync();

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Dispose();
    }
}
