using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Progression;
using TableTop.Hosting.Events;
using TableTop.Hosting.Hints;

namespace TableTop.Hosting.Controllers.Services;

/// <summary>
/// Assembles a <see cref="HintContext"/> from live session state, asks the
/// <see cref="IHintEngine"/> for a hint, and raises
/// <see cref="NextTurnHintEvent"/>.
///
/// Extracted from <see cref="CardTurnController"/> (backlog B.1). Hint
/// generation is advisory: a failure here must never interrupt the turn loop,
/// so <see cref="Emit"/> swallows exceptions exactly as the inline version did.
/// </summary>
internal sealed class HintCoordinator
{
    private readonly IHintEngine                    _hintEngine;
    private readonly TurnHistoryTracker             _history;
    private readonly SkipPolicy                     _skipPolicy;
    private readonly IFlowAwareProgressionStrategy? _flowStrategy;
    private readonly IReadOnlyList<IPlayer>         _players;
    private readonly Func<int>                      _getRound;
    private readonly Action<NextTurnHintEvent>      _onHint;

    public HintCoordinator(
        IHintEngine                    hintEngine,
        TurnHistoryTracker             history,
        SkipPolicy                     skipPolicy,
        IFlowAwareProgressionStrategy? flowStrategy,
        IReadOnlyList<IPlayer>         players,
        Func<int>                      getRound,
        Action<NextTurnHintEvent>      onHint)
    {
        _hintEngine   = hintEngine;
        _history      = history;
        _skipPolicy   = skipPolicy;
        _flowStrategy = flowStrategy;
        _players      = players;
        _getRound     = getRound;
        _onHint       = onHint;
    }

    /// <summary>
    /// Generates and raises a hint for <paramref name="player"/>, or does
    /// nothing if the engine has no suggestion.
    /// </summary>
    public void Emit(IPlayer player)
    {
        try
        {
            var ctx = new HintContext(
                RecentOutcomes:     _history.GetOutcomes(player.Id),
                RecentDifficulties: _history.GetDifficulties(player.Id),
                CurrentFlow:        _flowStrategy?.GetFlowState(player.Id),
                // Previously hardcoded to 0 in the controller, which made
                // DefaultHintEngine's `SkipCount >= 3` branch unreachable in a
                // real session — the engine's own unit test passed because it
                // constructs the context directly. Now sourced from SkipPolicy,
                // which is the component that actually counts skips.
                SkipCount:          _skipPolicy.GetSkipCount(player.Id),
                Round:              _getRound(),
                Standings:          _players
                                        .Select(p => (p.Id, p.Score))
                                        .ToList().AsReadOnly());

            var hint = _hintEngine.GenerateHint(player, ctx);
            if (hint is null) return;

            _onHint(new NextTurnHintEvent(
                PlayerName:          player.DisplayName,
                HintText:            hint.ForPlayer(player),
                SuggestedDifficulty: hint.SuggestedDifficulty.ToString(),
                SuggestedPaceChange: hint.SuggestedPaceChange?.ToString(),
                Urgency:             hint.Urgency.ToString(),
                Reason:              hint.Reason));
        }
        catch
        {
            // Hint generation must never crash the game loop
        }
    }
}
