using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;

namespace TableTop.Hosting.Controllers.Services;

/// <summary>
/// Applies reward and break card effects to game state.
/// Extracted from <c>CardTurnController</c> to keep effect resolution
/// independently testable and the controller focused on orchestration.
/// </summary>
public sealed class EffectApplicator
{
    private readonly IPlayerManager _playerManager;
    private readonly SkipPolicy     _skipPolicy;
    private readonly HashSet<Guid>  _extraCardPlayers = [];

    /// <summary>Initialises a new <see cref="EffectApplicator"/> instance.</summary>
    public EffectApplicator(IPlayerManager playerManager, SkipPolicy skipPolicy)
    {
        _playerManager = playerManager;
        _skipPolicy    = skipPolicy;
    }

    // ── Break effects ─────────────────────────────────────────────────────────

    /// <inheritdoc />
    public void ApplyBreakEffect(IBreakCard card, IPlayer player)
    {
        if (card.Effect is SkipTurnEffect)
            _playerManager.SetStatus(player.Id, PlayerStatus.Skipped);
        // RotatePlayers and GroupBreak: purely event-driven — host decides
    }

    // ── Reward effects ────────────────────────────────────────────────────────

    /// <summary>
    /// Applies the reward effect and returns (scoreDelta, human-readable description).
    /// For effects requiring player choice (StealPoints, SwapCard), scoreDelta is 0;
    /// the host must call <see cref="TableTop.Hosting.Abstractions.ICardTurnController"/> separately.
    /// </summary>
    public (int ScoreDelta, string Description) ApplyRewardEffect(
        RewardEffect effect, IPlayer player)
    {
        switch (effect)
        {
            case ScoreBonusEffect bonus:
                _playerManager.ApplyScore(player.Id, bonus.Points);
                return (bonus.Points, $"+{bonus.Points} pts");

            case ScoreMultiplierEffect mult:
                var delta = (int)(player.Score * mult.Multiplier) - player.Score;
                _playerManager.ApplyScore(player.Id, delta);
                return (delta, $"Score ×{mult.Multiplier}");

            case FreePassEffect:
                _skipPolicy.GrantFreePass(player.Id);
                return (0, "Free pass — next skip is free and skip count reset!");

            case ExtraCardEffect:
                _extraCardPlayers.Add(player.Id);
                return (0, "Draw an extra card!");

            case StealPointsEffect steal:
                return (0, $"Steal {steal.Points} pts from any player");

            case SwapCardEffect:
                return (0, "Swap your next card with any player");

            case DrinkPenaltyEffect drink:
                return (0, $"🍹 {drink.DrinkDescription}");

            case TimedMassageEffect massage:
                return (0, $"💆 {massage.DurationMinutes}-minute massage — {massage.Target} gives it");

            case NarrativeRewardEffect narrative:
                return (0, narrative.Description);

            default:
                return (0, effect.GetType().Name);
        }
    }

    // ── Extra-card tracking ───────────────────────────────────────────────────

    /// <inheritdoc />
    public bool ConsumeExtraCard(Guid playerId) =>
        _extraCardPlayers.Remove(playerId);

    /// <summary>ExtraCardPlayers.</summary>
    public IReadOnlySet<Guid> ExtraCardPlayers => _extraCardPlayers;
}