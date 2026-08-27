using TableTop.Core.Abstractions.Game;
using TableTop.Hosting;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui.Pages;

public partial class PlayerSetupPage : ContentPage
{
    /// <summary>
    /// The controller families this app has a screen for.
    ///
    /// <para>
    /// Declared as data rather than left implicit in the routing switch so a
    /// test can compare it against the live registry and fail when a mode
    /// exists that MAUI cannot play. That check is what was missing when
    /// Claimed! and Herd shipped unroutable — nothing anywhere knew the router
    /// had a hole in it, and the only way to find out was to pick the mode.
    /// </para>
    /// </summary>
    public static IReadOnlyList<ControllerFamily> SupportedFamilies { get; } =
    [
        ControllerFamily.CardTurn,
        ControllerFamily.Quiz,
        ControllerFamily.Monogamy,
        ControllerFamily.DailyCampaign,
        ControllerFamily.AreaControl,
        ControllerFamily.SimultaneousAnswer,
    ];

    private readonly PlayerSetupViewModel _vm;

    public PlayerSetupPage(IGameMode gameMode)
    {
        InitializeComponent();
        _vm = new PlayerSetupViewModel(new Services.MauiNavigator(this), gameMode, Services.AppSettings.Instance);
        BindingContext = _vm;
    }

    private void OnAddPlayerClicked(object sender, EventArgs e)
    {
        _vm.AddPlayer();
        PlayerNameEntry.Focus();
    }

    private void OnRemovePlayerClicked(object sender, EventArgs e)
    {
        if (sender is Button { BindingContext: PlayerSetupViewModel.PlayerEntry player })
            _vm.RemovePlayer(player);
    }

    private async void OnSaveRosterClicked(object sender, EventArgs e)
    {
        _vm.SaveRosterAsDefault();
        var n = _vm.Players.Count;
        await DisplayAlert(
            "Roster saved",
            n == 0 ? "Saved roster cleared."
                   : $"Saved {n} player{(n == 1 ? "" : "s")} for next time.",
            "OK");
    }

    private void OnClearPlayersClicked(object sender, EventArgs e)
    {
        _vm.ClearPlayers();
        PlayerNameEntry.Focus();
    }

    private async void OnStartGameClicked(object sender, EventArgs e)
    {
        // An exception escaping an async void handler terminates the
        // process on Android; surface it instead.
        try
        {
            if (!_vm.CanStartGame)
            {
                var need = _vm.MinimumPlayers;
                await DisplayAlert("Need Players",
                    need == 1 ? "Please add a player." : $"Please add at least {need} players.", "OK");
                return;
            }

            // Backlog J.1, resolved: starting a game no longer implicitly saves
            // the roster. It used to, alongside the explicit "Save roster"
            // button above — so a one-off group tried for a single game would
            // silently overwrite whoever was saved before, with no undo. WinUI
            // has only ever been explicit-only, for exactly that reason; this
            // brings MAUI in line rather than leaving the two heads disagreeing
            // about whether starting a game is implicit consent to remember it.
            //
            // Nothing else changes: OnSaveRosterClicked above still saves
            // on request, same as it always has.

            // Materialise into engine players, carrying gender/age attributes for
            // any player who set them.
            var players = _vm.BuildPlayers().ToList();

            // Backlog item 17: an incompatible roster (e.g. two untagged
            // players for a Couple-only mode) used to start normally and have
            // its per-card restrictions strip most or all of the deck, with no
            // error and no explanation. Checked here, not just in the shared
            // StartAsync, because this handler builds its own navigation
            // instead of calling it.
            var suitability = TableSuitability.Check(_vm.Mode, players);
            if (!suitability.Suits)
            {
                await DisplayAlert("Not quite the right table", suitability.Explanation, "OK");
                return;
            }

            // Route by controller family rather than by capability interface.
            //
            // This used to be a `_ =>` fall-through to GameplayPage, with a
            // comment claiming it let MAUI "play the WHOLE catalogue". That
            // stopped being true the moment ClaimedController and
            // HerdController were added: both fell into the fall-through, and
            // GameplayPage rejects anything that isn't an ICardTurnController
            // — so picking either mode surfaced a raw cast error dressed up as
            // "Couldn't start the game".
            //
            // Switching on ControllerFamilies.For makes every family an
            // explicit arm. A family MAUI has no page for now says so in
            // plain language instead of failing halfway into a screen, and
            // MauiSupportedFamilies below is asserted against the live
            // registry in tests, so the next new family is caught before a
            // player finds it.
            var family = ControllerFamilies.For(_vm.Mode);

            Page? next = family switch
            {
                ControllerFamily.Quiz => new MillionaireGamePage(_vm.Mode, players),
                ControllerFamily.Monogamy => new MonogamyGamePage(_vm.Mode, players),
                ControllerFamily.DailyCampaign => new DayOneGamePage(_vm.Mode, players),
                ControllerFamily.CardTurn => new GameplayPage(_vm.Mode, players),
                ControllerFamily.AreaControl => new ClaimedGamePage(_vm.Mode, players),
                ControllerFamily.SimultaneousAnswer => new HerdGamePage(_vm.Mode, players),
                _ => null,
            };

            if (next is null)
            {
                await DisplayAlert(
                    "Not available here yet",
                    $"'{_vm.Mode.Name}' needs a {family} screen, which this app doesn't have yet.",
                    "OK");
                return;
            }

            // Backlog item 20: GameplayPage/MillionaireGamePage/DayOneGamePage
            // build their controller asynchronously now rather than blocking
            // the UI thread in their constructors — this is where that async
            // step actually runs, awaited before the page is ever shown.
            if (next is IAsyncInitializablePage initializable)
                await initializable.InitializeAsync();

            await Navigation.PushAsync(next);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Couldn't start the game", ex.Message, "OK");
        }
    }
}
