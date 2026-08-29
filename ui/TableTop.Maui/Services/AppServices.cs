using Microsoft.Extensions.DependencyInjection;
using TableTop.Hosting.Abstractions;

namespace TableTop.Maui.Services;

/// <summary>
/// Reaches the app's container from code MAUI did not construct through DI.
///
/// <para>
/// <b>Why this exists.</b> Pages that need per-session runtime arguments (an
/// <c>IGameMode</c>, a player list) can't be container-resolved — see
/// <c>MauiProgram</c>'s note on why they're built with <c>new</c> — so they
/// have no injected <c>IServiceProvider</c> to hand to the shared ViewModels.
/// <c>GameplayViewModel.CreateAsync</c> already solved this by reading
/// <see cref="IPlatformApplication.Current"/>; five other pages didn't, and
/// each silently fell through to <c>ViewModel.CreateAsync</c>'s
/// <c>?? new ControllerFactory()</c> default instead (backlog X.1).
/// </para>
///
/// <para>
/// That default is the bug class <c>CLAUDE.md</c> names: a factory built by
/// hand carries no persistence override, no diagnostics sink and no DI
/// registration a host configured, and swapping it in costs a behaviour change
/// rather than a compile error. Naming the lookup once means the next page has
/// something obvious to copy — the absence of that is what let five of them
/// drift.
/// </para>
/// </summary>
internal static class AppServices
{
    /// <summary>The app's composition root, built in <c>MauiProgram.CreateMauiApp</c>.</summary>
    private static IServiceProvider Current =>
        IPlatformApplication.Current?.Services
        ?? throw new InvalidOperationException(
            "The MAUI app's service provider is not available yet. AppServices must not " +
            "be touched before MauiProgram.CreateMauiApp has run.");

    /// <summary>
    /// The container's controller factory, carrying the persistence
    /// <c>MauiProgram</c> configured under <c>FileSystem.AppDataDirectory</c>.
    /// </summary>
    public static IControllerFactory ControllerFactory =>
        Current.GetRequiredService<IControllerFactory>();
}
