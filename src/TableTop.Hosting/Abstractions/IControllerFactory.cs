using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;

namespace TableTop.Hosting.Abstractions;

/// <summary>
/// Creates the correct controller for a given game mode.
///
/// The mode→controller mapping previously lived in two places:
/// <c>GameViewModelFactory</c> (WPF) and <c>ConsoleGameLauncher</c> (Console).
/// Centralising it here means adding a new mode only requires updating one place
/// (OCP), and both UIs stay thin.
///
/// UIs receive an <see cref="IGameController"/> and are responsible only for
/// mapping the controller type to the correct ViewModel or renderer.
/// </summary>
public interface IControllerFactory
{
    /// <summary>
    /// Creates and returns the appropriate <see cref="IGameController"/> for
    /// <paramref name="mode"/>, wired up and ready to start.
    /// </summary>
    /// <param name="mode">The game mode to run.</param>
    /// <param name="players">Players registered for the session.</param>
    /// <param name="maxRounds">Maximum rounds before the game ends naturally.</param>
    /// <param name="gameplayOptions">
    /// Optional shuffle/difficulty-range/session-length preferences. Null
    /// (the default) preserves original behaviour: full deck, always
    /// shuffled. See <see cref="GameplayOptions"/>.
    /// </param>
    /// <param name="resumeFrom">
    /// A snapshot to resume from, or null to start fresh (backlog L.1). The
    /// engine has supported resume for a long time; until this parameter
    /// existed there was no way for a host to ask for it through the factory,
    /// which is why no graphical head could offer it.
    /// </param>
    /// <param name="monogamyWinningTokenCount">
    /// Overrides the mode's own token target for a Monogamy-family mode (see
    /// <see cref="IMonogamyDeckProvider.WinningTokenCount"/>). Null (the
    /// default) uses the mode's own value. Ignored by every other family —
    /// same honest-scope-boundary treatment as <paramref name="gameplayOptions"/>.
    /// Exists so a host that lets the table pick their own target (Console
    /// prompts for one) can still go through this factory rather than
    /// constructing <c>MonogamyController</c> itself.
    /// </param>
    /// <param name="ct">Optional cancellation token for async deck building.</param>
    Task<IGameController> CreateAsync(
        IGameMode mode,
        IReadOnlyList<IPlayer> players,
        int maxRounds = Core.TableTopDefaults.Session.MaxRounds,
        GameplayOptions? gameplayOptions = null,
        Persistence.SessionSnapshot? resumeFrom = null,
        int? monogamyWinningTokenCount = null,
        CancellationToken ct = default);

    /// <summary>
    /// The saved session, or null if there isn't one. A host calls this at
    /// launch to decide whether to offer "resume" — one call, rather than each
    /// head hand-rolling repository access.
    /// </summary>
    Task<Persistence.SessionSnapshot?> LoadSavedSessionAsync(CancellationToken ct = default);
}
