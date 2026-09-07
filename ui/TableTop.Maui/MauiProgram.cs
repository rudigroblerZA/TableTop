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
        //
        // Explicit paths under FileSystem.AppDataDirectory — MAUI's own
        // sandboxed, always-writable per-platform app-data location (Android's
        // internal files dir, iOS/macOS's Library, Windows's LocalState).
        // AddTableTopHosting's own defaults fall back to AppContext.BaseDirectory
        // (beside the executable/app bundle), which several MAUI targets don't
        // allow writing to at all.
        builder.Services.AddTableTopHosting(
            sessionFilePath: Path.Combine(FileSystem.AppDataDirectory, "session.json"),
            playerFilePath: Path.Combine(FileSystem.AppDataDirectory, "players.json"));

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
        // TRANSIENT, not singleton, and the flyout is why. A Page may have only
        // one parent: putting one instance into a second NavigationPage — which
        // is exactly what "open the picker from the drawer, go Home, open it
        // again" does — throws. It was a singleton when it was the fixed root
        // page and had a single consumer; it now has three. Nothing is lost by
        // rebuilding it, because the selection lives in the singleton
        // GameSelectionViewModel below, not in the page.
        builder.Services.AddTransient<GameSelectionPage>();
        builder.Services.AddTransient<SettingsPage>();

        // The flyout shell and its two fixed screens. AppFlyoutPage is resolved
        // once, by App.CreateWindow; HomePage is transient for the same
        // re-parenting reason as the picker.
        builder.Services.AddTransient<AppFlyoutPage>();
        builder.Services.AddTransient<FlyoutMenuPage>();
        builder.Services.AddTransient<HomePage>();

        // ── ViewModels ───────────────────────────────────────────────────────
        // Same reasoning as the Pages above: PlayerSetupViewModel and
        // GameplayViewModel need per-session runtime values no registration
        // can supply, so they aren't registered — they're constructed
        // directly, with IServiceProvider passed in for the services they do
        // need from the container.
        builder.Services.AddSingleton<GameSelectionViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Singleton so it subscribes to IAppSettings.Changed exactly once. The
        // drawer is built once and lives for the life of the app, and a second
        // subscription would rebuild the menu twice per settings change.
        builder.Services.AddSingleton<FlyoutMenuViewModel>();

        return builder.Build();
    }
}
