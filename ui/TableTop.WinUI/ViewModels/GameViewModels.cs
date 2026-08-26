using System.Collections.ObjectModel;
using System.Windows.Input;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Hosting;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;
using TableTop.WinUI.Infrastructure;
using TableTop.Presentation.Infrastructure;
using TableTop.Presentation.ViewModels;

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
            ShuffleDeck    = s.ShuffleCards,
            MinDifficulty  = (Core.Abstractions.Cards.Difficulty)(s.MinDifficulty + 1),
            MaxDifficulty  = (Core.Abstractions.Cards.Difficulty)(s.MaxDifficulty + 1),
            CardsPerPlayer = s.CardsPerPlayer > 0 ? s.CardsPerPlayer : null,
        };

        var controller = await (controllerFactory ?? new ControllerFactory()).CreateAsync(
            mode, players, maxRounds: 10, gameplayOptions: gameplayOptions, resumeFrom: resumeFrom);

        // Route the built controller to the ViewModel that drives its family.
        //
        // This comment used to claim every controller type had a real WinUI
        // screen, so the fallback was unreachable. That stopped being true
        // when ClaimedController and HerdController were added — both land on
        // Fallback today. WinUI at least degrades to a readable
        // "unsupported mode" screen rather than throwing, which is more than
        // MAUI or Console managed; see ControllerFamily and
        // HeadFamilyCoverageTests for the gap stated as data.
        return controller switch
        {
            ICardTurnController ctc   => new CardTurnGameViewModel(
                navigator, mode, ctc, s.EnableTimer, s.TimerSeconds, s.ShowCardCount),
            IMillionaireController mc => new MillionaireGameViewModel(navigator, mc),
            IMonogamyController mo    => new MonogamyGameViewModel(navigator, mo),
            IDayOneController dc      => new DayOneGameViewModel(navigator, dc),
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
    /// Used to say "try it in Console" unconditionally. The only two modes
    /// that ever reach this screen are Claimed! (AreaControl) and Herd
    /// (SimultaneousAnswer), and Console declares
    /// <c>[CardTurn, Quiz]</c> — so every player who saw that suggestion was
    /// being sent to a head that also cannot play the mode. Backlog item 12.
    /// </para>
    /// </summary>
    public string Message =>
        $"'{ModeName}' isn't playable on this screen yet — check back soon.";
    /// <summary>Returns to game selection.</summary>
    public ICommand GoBackCommand { get; }

    /// <summary>Initialises the fallback for <paramref name="modeName"/>.</summary>
    public UnsupportedModeViewModel(Navigator navigator, string modeName)
    {
        ModeName      = modeName;
        GoBackCommand = new RelayCommand(() => navigator.GoBack());
    }
}
