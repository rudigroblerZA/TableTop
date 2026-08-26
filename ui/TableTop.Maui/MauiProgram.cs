using TableTop.Hosting.Extensions;
using TableTop.Maui.Pages;
using TableTop.Maui.Services;
using TableTop.Maui.ViewModels;
using TableTop.Presentation.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // ── Engine ───────────────────────────────────────────────────────────
        // Registers IControllerFactory, IArchetypeRegistry, IGamePersistence,
        // IPlayerRepository, and IHintEngine with their default implementations.
        builder.Services.AddTableTopHosting();

        // ── App Settings ─────────────────────────────────────────────────────
        // Registered under both the concrete type (existing consumers still
        // resolve it directly) and IAppSettings — backlog item 5: the shared
        // ViewModels declare IAppSettings as a dependency, and until this
        // registration existed nothing in the container could actually supply
        // one, so every construction site read AppSettings.Instance by hand.
        builder.Services.AddSingleton<AppSettings>(_ => AppSettings.Instance);
        builder.Services.AddSingleton<IAppSettings>(sp => sp.GetRequiredService<AppSettings>());

        // ── Pages ────────────────────────────────────────────────────────────
        // PlayerSetupPage and GameplayPage are deliberately NOT registered
        // here (backlog item 5) — their constructors need a per-session
        // IGameMode/List<IPlayer> the container has no registration for, so a
        // resolve would throw. They're built with `new`, at the point their
        // runtime arguments are known, in GameSelectionPage/PlayerSetupPage;
        // IServiceProvider is threaded to those call sites instead so they can
        // still reach IControllerFactory/IAppSettings for the one path that
        // needs them (GameplayPage's CardTurn family).
        builder.Services.AddSingleton<GameSelectionPage>();
        builder.Services.AddTransient<SettingsPage>();

        // ── ViewModels ───────────────────────────────────────────────────────
        // Same reasoning as the Pages above: PlayerSetupViewModel and
        // GameplayViewModel need per-session runtime values no registration
        // can supply, so they aren't registered — they're constructed
        // directly, with IServiceProvider passed in for the services they do
        // need from the container.
        builder.Services.AddSingleton<GameSelectionViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        return builder.Build();
    }
}
