using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;
using TableTop.Presentation.Infrastructure;
using TableTop.Presentation.ViewModels;
using TableTop.WinUI.Infrastructure;

namespace TableTop.WinUI.ViewModels;

/// <summary>
/// Builds the right ViewModel for a chosen mode — the WinUI mirror of the
/// WPF <c>GameViewModelFactory</c>, including the same two behaviours that
/// were hard-won there: GameplayOptions built from Settings so the deal is
/// genuinely shaped by them, and a graceful fallback for controller types
/// this UI has no screen for yet (Millionaire, Monogamy, Day One) instead
/// of a crash.
/// </summary>
public static class GameViewModelFactory
{
    /// <summary>
    /// The controller families this app has a screen for — the switch arms in
    /// <see cref="CreateAsync"/> that aren't <see cref="Fallback"/>.
    ///
    /// <para>
    /// Declared as data, matching the pattern MAUI's <c>PlayerSetupPage</c> and
    /// Console's <c>ConsoleGameLauncher</c> already use, so the gap is
    /// inspectable rather than something only readable in the switch below.
    /// Backlog item 12: this is the flagship head, and until this property
    /// existed it was the one head with no declaration and no test coverage at
    /// all — the exact "implicit in the routing switch" state the mechanism
    /// was built to replace.
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
        ControllerFamily.TraitProfile,
    ];

    /// <summary>Creates the ViewModel that will drive <paramref name="mode"/>.</summary>
    /// <summary>
    /// Builds the ViewModel for a mode. Pass <paramref name="resumeFrom"/> to
    /// continue a saved session instead of starting fresh (backlog L.1); use
    /// <c>ControllerFactory.LoadSavedSessionAsync</c> to obtain one.
    /// </summary>
    public static async Task<ViewModelBase> CreateAsync(
        Navigator navigator, IGameMode mode, IReadOnlyList<IPlayer> players,
        Hosting.Persistence.SessionSnapshot? resumeFrom = null,
        Hosting.Abstractions.IControllerFactory? controllerFactory = null)
    {
        var s = WinUIAppSettings.Instance;
        var gameplayOptions = new GameplayOptions
        {
            ShuffleDeck = s.ShuffleCards,
            MinDifficulty = (Core.Abstractions.Cards.Difficulty)(s.MinDifficulty + 1),
            MaxDifficulty = (Core.Abstractions.Cards.Difficulty)(s.MaxDifficulty + 1),
            CardsPerPlayer = s.CardsPerPlayer > 0 ? s.CardsPerPlayer : null,
        };

        // Backlog item 5: a controllerFactory registered in the app's
        // container (Navigator.Services) now actually reaches here — this
        // used to default straight to `new ControllerFactory()` regardless of
        // anything the composition root had registered, since nothing built
        // one to pass in the first place.
        var factory = controllerFactory ?? navigator.Services.GetRequiredService<IControllerFactory>();
        var controller = await factory.CreateAsync(
            mode, players, maxRounds: 10, gameplayOptions: gameplayOptions, resumeFrom: resumeFrom);

        // Route the built controller to the ViewModel that drives its family.
        // Every family the catalogue can produce has a screen here now
        // (backlog item 4) — Fallback stays as a safety net for a future
        // family that ships a controller before its screen, not because any
        // mode currently reaches it.
        return controller switch
        {
            ICardTurnController ctc => new CardTurnGameViewModel(
                navigator, mode, ctc, s.EnableTimer, s.TimerSeconds, s.ShowCardCount),
            IMillionaireController mc => new MillionaireGameViewModel(navigator, mc),
            IMonogamyController mo => new MonogamyGameViewModel(navigator, mo),
            IDayOneController dc => new DayOneGameViewModel(navigator, dc),
            IClaimedController cc => new ClaimedGameViewModel(navigator, cc),
            IHerdController hc => new HerdGameViewModel(navigator, hc),
            ITraitProfileController tp => new TraitProfileGameViewModel(navigator, tp),
            _ => Fallback(navigator, mode, controller),
        };
    }

    private static ViewModelBase Fallback(Navigator navigator, IGameMode mode, IGameController controller)
    {
        controller.Dispose();
        return new UnsupportedModeViewModel(navigator, mode.Name);
    }
}

// CardTurnGameViewModel now lives in TableTop.Presentation and is shared with
// MAUI — backlog item 1, the last real duplication: 733 lines on MAUI, 404
// here, both driving the same ICardTurnController. See its doc comment in
// TableTop.Presentation/ViewModels/CardTurnGameViewModel.cs for the full
// union-of-both-heads account: WinUI gained the timer, three-tier hint
// urgency, and constructor error handling (a failed controller build used to
// take the whole app down — the same class of bug already found and fixed for
// Monogamy, Millionaire and Day One when each of those screens was merged).

/// <summary>Friendly fallback for modes whose controller has no WinUI screen yet.</summary>
public sealed class UnsupportedModeViewModel : ViewModelBase
{
    /// <summary>The mode that was attempted.</summary>
    public string ModeName { get; }
    /// <summary>
    /// Explanation shown to the player.
    ///
    /// <para>
    /// Used to say "try it in Console" unconditionally, back when Claimed!
    /// (AreaControl) and Herd (SimultaneousAnswer) were the only two modes
    /// that ever reached this screen and Console couldn't play them either
    /// — backlog item 12. Both families have real screens on every head now
    /// (item 4), so no mode in the catalogue reaches this fallback today; it
    /// stays as a safety net for a future family that ships a controller
    /// before its screen.
    /// </para>
    /// </summary>
    public string Message =>
        $"'{ModeName}' isn't playable on this screen yet — check back soon.";
    /// <summary>Returns to game selection.</summary>
    public ICommand GoBackCommand { get; }

    /// <summary>Initialises the fallback for <paramref name="modeName"/>.</summary>
    public UnsupportedModeViewModel(Navigator navigator, string modeName)
    {
        ModeName = modeName;
        GoBackCommand = new RelayCommand(() => navigator.GoBack());
    }
}
