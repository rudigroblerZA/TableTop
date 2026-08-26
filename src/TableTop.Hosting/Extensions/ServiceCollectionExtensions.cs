using Microsoft.Extensions.DependencyInjection;
using TableTop.Core.Abstractions;
using TableTop.Core.Domain.Decks;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;
using TableTop.Hosting.Diagnostics;
using TableTop.Hosting.Hints;
using TableTop.Hosting.Persistence;

namespace TableTop.Hosting.Extensions;

/// <summary>
/// Extension methods for registering all hosting-layer services with Microsoft DI.
///
/// Call <see cref="AddTableTopHosting"/> once at application startup.  Every
/// service that UIs previously <c>new</c>-ed by hand is now resolved from the
/// container, which means implementations can be swapped (e.g. cloud persistence)
/// in a single registration and all consumers update automatically.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the full TableTop hosting stack:
    /// <list type="bullet">
    ///   <item><see cref="TableTop.Hosting.Abstractions.IControllerFactory"/> → <see cref="ControllerFactory"/> (transient)</item>
    ///   <item><see cref="IArchetypeRegistry"/> → <see cref="ArchetypeRegistry"/> default (singleton)</item>
    ///   <item><see cref="IGamePersistence"/> → <see cref="JsonGamePersistence"/> (singleton)</item>
    ///   <item><see cref="IPlayerRepository"/> → <see cref="JsonPlayerRepository"/> (singleton)</item>
    ///   <item><see cref="IHintEngine"/> → <see cref="DefaultHintEngine"/> (singleton)</item>
    /// </list>
    ///
    /// Override individual registrations after this call to swap any implementation:
    /// <code>
    /// services.AddTableTopHosting()
    ///         .AddSingleton&lt;IGamePersistence, CloudGamePersistence&gt;();
    /// </code>
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="sessionFilePath">
    /// Optional custom path for the JSON session file.
    /// Defaults to <c>%AppData%/TableTop/session.json</c> (or platform equivalent).
    /// </param>
    /// <param name="playerFilePath">
    /// Optional custom path for the JSON player-profile file.
    /// Defaults to <c>%AppData%/TableTop/players.json</c> (or platform equivalent).
    /// </param>
    public static IServiceCollection AddTableTopHosting(
        this IServiceCollection services,
        string?                 sessionFilePath = null,
        string?                 playerFilePath  = null)
    {
        // ── Controller factory ───────────────────────────────────────────────
        // Transient: a new factory is cheap and now genuinely carries no state of
        // its own. Until 1.19.0 that comment was false — the constructor assigned
        // the process-wide JsonDeckLoader.Diagnostics static, so every resolution
        // reassigned it. The JSON deck path and that static are both gone.
        services.AddTransient<IControllerFactory>(sp =>
            new ControllerFactory(sp.GetService<IGamePersistence>()));

        // ── Archetype registry ───────────────────────────────────────────────
        // Singleton: the tree is built once — it is read-only after construction.
        services.AddSingleton<IArchetypeRegistry>(_ => ArchetypeRegistry.Default());

        // ── Persistence ──────────────────────────────────────────────────────
        // Singleton: one file per application instance; sequential access only.
        services.AddSingleton<IGamePersistence>(
            _ => new JsonGamePersistence(sessionFilePath));

        services.AddSingleton<IPlayerRepository>(
            _ => new JsonPlayerRepository(playerFilePath));

        // ── Hint engine ──────────────────────────────────────────────────────
        // Singleton: stateless — holds no mutable data, safe to share.
        services.AddSingleton<IHintEngine, DefaultHintEngine>();

        return services;
    }

    // ── Convenience overrides ─────────────────────────────────────────────────

    /// <summary>
    /// Replaces the default <see cref="IGamePersistence"/> with a custom implementation.
    /// Call after <see cref="AddTableTopHosting"/>.
    /// </summary>
    public static IServiceCollection UseGamePersistence<T>(this IServiceCollection services)
        where T : class, IGamePersistence
    {
        // Remove the default registration then add the override so the last
        // registration wins cleanly rather than accumulating duplicates.
        var descriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IGamePersistence));
        if (descriptor is not null) services.Remove(descriptor);

        services.AddSingleton<IGamePersistence, T>();
        return services;
    }

    /// <summary>
    /// Replaces the default <see cref="IPlayerRepository"/> with a custom implementation.
    /// Call after <see cref="AddTableTopHosting"/>.
    /// </summary>
    public static IServiceCollection UsePlayerRepository<T>(this IServiceCollection services)
        where T : class, IPlayerRepository
    {
        var descriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IPlayerRepository));
        if (descriptor is not null) services.Remove(descriptor);

        services.AddSingleton<IPlayerRepository, T>();
        return services;
    }

    /// <summary>
    /// Replaces the default <see cref="IHintEngine"/> with a custom implementation.
    /// Call after <see cref="AddTableTopHosting"/>.
    /// </summary>
    public static IServiceCollection UseHintEngine<T>(this IServiceCollection services)
        where T : class, IHintEngine
    {
        var descriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IHintEngine));
        if (descriptor is not null) services.Remove(descriptor);

        services.AddSingleton<IHintEngine, T>();
        return services;
    }

    /// <summary>
    /// Registers a custom <see cref="IEngineDiagnostics"/> implementation.
    /// The sink receives rule denials, card selections, turn recordings and
    /// game lifecycle events — useful for debugging rule chains in the field.
    ///
    /// For hosts that use <c>Microsoft.Extensions.Logging</c>, prefer
    /// <see cref="UseLoggerDiagnostics"/> which wires in <see cref="LoggerEngineDiagnostics"/>
    /// automatically.
    /// </summary>
    public static IServiceCollection UseEngineDiagnostics<T>(this IServiceCollection services)
        where T : class, IEngineDiagnostics
    {
        var descriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IEngineDiagnostics));
        if (descriptor is not null) services.Remove(descriptor);

        services.AddSingleton<IEngineDiagnostics, T>();
        return services;
    }

    /// <summary>
    /// Wires <see cref="LoggerEngineDiagnostics"/> as the <see cref="IEngineDiagnostics"/> sink.
    /// Requires <c>Microsoft.Extensions.Logging</c> to be configured in the host.
    ///
    /// Log levels:
    /// <list type="bullet">
    ///   <item>Trace — rule evaluations (verbose; enable only when debugging a skip loop)</item>
    ///   <item>Debug — card selected, turn recorded, game start/end</item>
    ///   <item>Warning — no card available after exhausting candidates</item>
    /// </list>
    ///
    /// Usage:
    /// <code>
    /// services.AddTableTopHosting()
    ///         .UseLoggerDiagnostics();
    /// // In appsettings.json set:
    /// //   "Logging": { "LogLevel": { "TableTop": "Debug" } }
    /// </code>
    /// </summary>
    public static IServiceCollection UseLoggerDiagnostics(this IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IEngineDiagnostics));
        if (descriptor is not null) services.Remove(descriptor);

        services.AddSingleton<IEngineDiagnostics, LoggerEngineDiagnostics>();
        return services;
    }

    // AddJsonGameModes, AddJsonGameModesAsync and AddLoadedGameModes lived here.
    // They loaded JsonGameMode instances from disk and swapped in an
    // ArchetypeRegistry that included them. All three went with JsonGameMode in
    // 1.21.0; the registry is now always ArchetypeRegistry.Default().
    //
    // One note worth keeping, because it took a real diagnosis: the load used to
    // happen inside the singleton factory, so it ran on whatever thread first
    // resolved IArchetypeRegistry — blocking on I/O during DI resolution,
    // potentially on a UI thread. It was moved to registration time so the
    // blocking sat at a controlled point in startup and a bad path failed
    // immediately rather than on first resolve. Any future registration that
    // does I/O should do the same.
}
