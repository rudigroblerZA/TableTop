using Microsoft.Extensions.DependencyInjection;
using TableTop.Console;
using TableTop.Core.Extensions;
using TableTop.Hosting.Extensions;
using SC = System.Console;

// ── Composition root ──────────────────────────────────────────────────────────
// Build the service container once at startup. All services are resolved here;
// nothing else in the Console project uses `new` for engine or hosting types.

// AddTableTopHosting's own defaults fall back to AppContext.BaseDirectory
// (beside the executable) when no path is given — writable for a build
// output folder, not guaranteed for wherever this gets installed to. The
// per-user app-data directory is the portable, always-writable choice every
// platform .NET runs on provides through the same API.
var appDataDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TableTop");
Directory.CreateDirectory(appDataDir);

var services = new ServiceCollection()
    .AddTableTop()         // Core: IGameFactory, IDeckBuilder, IRuleEvaluator …
    .AddTableTopHosting(   // Hosting: IControllerFactory, IArchetypeRegistry,
                           //          IGamePersistence, IPlayerRepository, IHintEngine
        sessionFilePath: Path.Combine(appDataDir, "session.json"),
        playerFilePath: Path.Combine(appDataDir, "players.json"));

var provider = services.BuildServiceProvider();

try
{
    var launcher = new ConsoleGameLauncher(
        repository: provider.GetRequiredService<TableTop.Hosting.Persistence.IPlayerRepository>(),
        controllerFactory: provider.GetRequiredService<TableTop.Hosting.Abstractions.IControllerFactory>());

    launcher.Run();
}
catch (Exception ex)
{
    ConsoleUi.PrintError($"Unexpected error: {ex.Message}");
    ConsoleUi.PrintMessage("Press ENTER to exit.");
    SC.ReadLine();
}
finally
{
    // Dispose the container cleanly so any IDisposable singletons are released.
    if (provider is IDisposable d) d.Dispose();
}
