using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui.Pages;

/// <summary>Screen for the trait-assessment modes (Big Five, Love Languages).</summary>
public partial class TraitProfileGamePage : ContentPage, IAsyncInitializablePage
{
    private readonly IGameMode _gameMode;
    private readonly List<IPlayer> _players;
    private TraitProfileGameViewModel _vm = null!;

    /// <summary>
    /// Builds the page for the chosen mode and players.
    ///
    /// Cheap on purpose — the same two-phase shape as <c>HerdGamePage</c> and
    /// <c>DayOneGamePage</c> (backlog item 20). MAUI never awaits page
    /// construction, so the controller builds in <see cref="InitializeAsync"/>,
    /// which a caller must await before pushing this page.
    /// </summary>
    public TraitProfileGamePage(IGameMode gameMode, List<IPlayer> players)
    {
        InitializeComponent();
        _gameMode = gameMode;
        _players = players;
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        _vm = await TraitProfileGameViewModel.CreateAsync(
            new Services.MauiNavigator(this), _gameMode, _players,
            controllerFactory: Services.AppServices.ControllerFactory);
        BindingContext = _vm;

        // The mode names the screen — two modes share this page, so a fixed
        // title would say "Profile" while the player is answering Love
        // Languages. Set after the VM exists so a failed build still shows a
        // sensible header rather than an empty one.
        Title = _gameMode.Name;
    }

    private void OnNextClicked(object sender, EventArgs e) => _vm.Submit();
    private void OnSkipClicked(object sender, EventArgs e) => _vm.Skip();

    private async void OnQuitClicked(object sender, EventArgs e) => await this.SafePopToRootAsync();
    private async void OnDoneClicked(object sender, EventArgs e) => await this.SafePopToRootAsync();

    /// <inheritdoc />
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Dispose();
    }
}
