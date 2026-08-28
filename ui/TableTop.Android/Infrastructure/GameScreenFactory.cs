using Microsoft.Extensions.DependencyInjection;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Droid.Screens;
using TableTop.Hosting;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;
using TableTop.Hosting.Persistence;
using TableTop.Presentation.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Droid.Infrastructure;

/// <summary>
/// Builds the right gameplay <see cref="Screen"/> for a chosen mode — the
/// Android mirror of WinUI's <c>GameViewModelFactory</c> and MAUI's
/// <c>PlayerSetupPage</c> routing. Same two behaviours those carry:
/// <see cref="GameplayOptions"/> built from <see cref="IAppSettings"/> so the
/// deal is genuinely shaped by settings, and a graceful fallback for a family
/// with no screen yet rather than a crash.
/// </summary>
public static class GameScreenFactory
{
    /// <summary>
    /// The controller families this head has a screen for. Declared as data,
    /// matching the pattern Console's <c>ConsoleGameLauncher</c> and WinUI's
    /// <c>GameViewModelFactory</c> already use; <c>HeadFamilyCoverageTests</c>
    /// and <c>scripts/check-head-family-coverage.py</c> keep this honest.
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

    /// <summary>Creates the screen that will drive <paramref name="mode"/> for these players.</summary>
    public static async Task<Screen> CreateAsync(
        INavigator navigator,
        IGameMode mode,
        IReadOnlyList<IPlayer> players,
        IAppSettings settings,
        SessionSnapshot? resumeFrom = null)
    {
        var factory = MainApplication.Services.GetRequiredService<IControllerFactory>();

        var options = new GameplayOptions
        {
            ShuffleDeck = settings.ShuffleCards,
            MinDifficulty = (Difficulty)(settings.MinDifficulty + 1),
            MaxDifficulty = (Difficulty)(settings.MaxDifficulty + 1),
            CardsPerPlayer = settings.CardsPerPlayer > 0 ? settings.CardsPerPlayer : null,
        };

        IGameController controller;
        try
        {
            controller = await factory.CreateAsync(
                mode, players, maxRounds: 10, gameplayOptions: options, resumeFrom: resumeFrom);
        }
        catch (Exception ex)
        {
            return new MessageScreen(mode.Name, ex.Message);
        }

        return controller switch
        {
            ICardTurnController ctc => new CardTurnGameScreen(new CardTurnGameViewModel(
                navigator, mode, ctc, settings.EnableTimer, settings.TimerSeconds, settings.ShowCardCount)),
            IMillionaireController mc => new MillionaireGameScreen(new MillionaireGameViewModel(navigator, mc)),
            IMonogamyController mo => new MonogamyGameScreen(new MonogamyGameViewModel(navigator, mo)),
            IDayOneController dc => new DayOneGameScreen(new DayOneGameViewModel(navigator, dc)),
            IClaimedController cc => new ClaimedGameScreen(new ClaimedGameViewModel(navigator, cc)),
            IHerdController hc => new HerdGameScreen(new HerdGameViewModel(navigator, hc)),
            _ => Fallback(mode, controller),
        };
    }

    private static Screen Fallback(IGameMode mode, IGameController controller)
    {
        controller.Dispose();
        return new MessageScreen(
            mode.Name,
            $"'{mode.Name}' needs a {ControllerFamilies.For(mode)} screen, which this app doesn't have yet.");
    }
}
