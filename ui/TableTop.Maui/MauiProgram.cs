using TableTop.Hosting.Extensions;
using TableTop.Maui.Pages;
using TableTop.Maui.Services;
using TableTop.Maui.ViewModels;
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
        builder.Services.AddSingleton<AppSettings>(_ => AppSettings.Instance);

        // ── Pages ────────────────────────────────────────────────────────────
        builder.Services.AddSingleton<GameSelectionPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<PlayerSetupPage>();
        builder.Services.AddTransient<GameplayPage>();

        // ── ViewModels ───────────────────────────────────────────────────────
        builder.Services.AddSingleton<GameSelectionViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<PlayerSetupViewModel>();
        builder.Services.AddTransient<GameplayViewModel>();

        return builder.Build();
    }
}
