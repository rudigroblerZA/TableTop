using TableTop.Core.Abstractions;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Players;
using TableTop.Core.Domain.Rules;

namespace TableTop.Core.Engine;

/// <summary>
/// Creates fully wired <see cref="Game"/> instances from an <see cref="IGameConfiguration"/>.
/// Consumers depend on <see cref="IGameFactory"/>, not on this class (DIP).
/// </summary>
public sealed class GameFactory : IGameFactory
{
    private readonly IEngineDiagnostics _diagnostics;
    private readonly bool _teamPlay;

    /// <summary>Initialises a new <see cref="GameFactory"/> instance.</summary>
    /// <param name="diagnostics">Optional diagnostics sink.</param>
    /// <param name="teamPlay">
    /// When true, turn order alternates between teams
    /// (<see cref="TeamAlternatingPlayerManager"/>) rather than running
    /// straight round-robin.
    ///
    /// <para>
    /// This is a constructor flag rather than something read off
    /// <see cref="IGameConfiguration"/> because that interface carries no
    /// reference to the mode, and widening it would touch every
    /// implementation and test double for the sake of one boolean.
    /// <c>ControllerFactory</c> already knows the mode and can detect
    /// <c>ITeamMode</c>, so the knowledge is passed down from where it
    /// already exists instead of being rediscovered here.
    /// </para>
    /// </param>
    public GameFactory(IEngineDiagnostics? diagnostics = null, bool teamPlay = false)
    {
        _diagnostics = diagnostics ?? NullEngineDiagnostics.Instance;
        _teamPlay    = teamPlay;
    }

    /// <inheritdoc />
    public IGame Create(IGameConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        IPlayerManager playerManager = _teamPlay
            ? new TeamAlternatingPlayerManager()
            : new RoundRobinPlayerManager();

        var ruleEvaluator = new RuleEvaluator(configuration.Rules, _diagnostics);

        return new Game(configuration, playerManager, ruleEvaluator);
    }
}