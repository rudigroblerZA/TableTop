using Microsoft.Extensions.DependencyInjection;
using TableTop.Console;
using TableTop.Core.Extensions;
using TableTop.Hosting.Extensions;
using SC = System.Console;

// ── Composition root ──────────────────────────────────────────────────────────
// Build the service container once at startup. All services are resolved here;
// nothing else in the Console project uses `new` for engine or hosting types.

var services = new ServiceCollection()
    .AddTableTop()         // Core: IGameFactory, IDeckBuilder, IRuleEvaluator …
    .AddTableTopHosting(); // Hosting: IControllerFactory, IArchetypeRegistry,
                           //          IGamePersistence, IPlayerRepository, IHintEngine

var provider = services.BuildServiceProvider();

try
{
    var launcher = new ConsoleGameLauncher(
        repository:        provider.GetRequiredService<TableTop.Hosting.Persistence.IPlayerRepository>(),
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
