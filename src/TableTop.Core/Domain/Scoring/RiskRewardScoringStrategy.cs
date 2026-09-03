using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;

namespace TableTop.Core.Domain.Scoring;

/// <summary>
/// Press-your-luck scoring: attempting a card and failing costs you points,
/// declining it costs nothing.
///
/// <para>
/// <b>The gap this fills.</b> Every other strategy in this file returns zero for
/// <see cref="CardOutcome.Failed"/> — the same as <see cref="CardOutcome.Skipped"/>.
/// That makes attempting a card strictly free: there has never been a reason not
/// to try every card, however far beyond you it is, because a failed attempt and
/// a decline score identically. This is the first strategy in the engine that
/// can return a negative number, and that is the entire mechanic.
/// </para>
///
/// <para>
/// <b>Why a decorator rather than a strategy of its own.</b> The stake has to be
/// whatever the card was already worth, or the mechanic needs its own parallel
/// difficulty table that would drift from the base one. So it asks the wrapped
/// strategy what a <i>completion</i> would have paid and risks that same amount:
/// wrap <see cref="DifficultyBasedScoringStrategy"/> and an Extreme card is worth
/// 5 and costs 5, an Easy one is worth 1 and costs 1. The interesting decision —
/// attempt or decline — falls out of the base strategy's own numbers.
/// </para>
///
/// <para>
/// <b>Skipped is deliberately free.</b> Making a decline cost anything collapses
/// the mechanic back to "attempt everything", because if declining is also
/// punished you may as well take the chance. The choice this strategy creates
/// only exists while one of the two options is genuinely safe. Note that
/// <c>CardTurnController.SkipPenalty</c> is a separate, additive policy — a host
/// that sets both is charging for declines through the back door and has
/// re-broken the mechanic.
/// </para>
///
/// <para>
/// <b>Scores can go negative</b>, and nothing clamps them: <c>ApplyScore</c> adds
/// the delta as given. That is intentional — a floor at zero would make failure
/// free again for anyone already at the bottom, which is precisely the player
/// most tempted to gamble.
/// </para>
///
/// <para>
/// <b>Reachability, stated honestly.</b> Only the Console head currently records
/// <see cref="CardOutcome.Failed"/> at all; WinUI, MAUI and native Android offer
/// Complete and Skip and nothing else, so under those heads this strategy behaves
/// exactly like the strategy it wraps. It degrades to the base rather than
/// misbehaving, but the mechanic is genuinely Console-only until those heads grow
/// a third button. Tracked in BACKLOG.md rather than papered over.
/// </para>
/// </summary>
public sealed class RiskRewardScoringStrategy : IScoringStrategy
{
    private readonly IScoringStrategy _base;
    private readonly double _failurePenaltyRatio;

    /// <summary>
    /// Wraps a base strategy with a failure penalty.
    /// </summary>
    /// <param name="baseStrategy">Supplies both the reward and, via the same call, the stake.</param>
    /// <param name="failurePenaltyRatio">
    /// Share of the would-be reward lost on a failed attempt. 1.0 (the default)
    /// risks exactly what was on offer; 0.5 makes attempting favourable on any
    /// card you are better than even on. Must not be negative — a negative ratio
    /// would pay players for failing, which is not a gentler version of this
    /// mechanic but the inverse of it.
    /// </param>
    public RiskRewardScoringStrategy(
        IScoringStrategy baseStrategy,
        double failurePenaltyRatio = TableTopDefaults.Scoring.FailurePenaltyRatio)
    {
        ArgumentNullException.ThrowIfNull(baseStrategy);
        ArgumentOutOfRangeException.ThrowIfNegative(failurePenaltyRatio);

        _base = baseStrategy;
        _failurePenaltyRatio = failurePenaltyRatio;
    }

    /// <inheritdoc />
    public string Name => $"RiskReward({_base.Name})";

    /// <inheritdoc />
    public int CalculateScore(ICard card, IPlayer player, CardOutcome outcome, TimeSpan? elapsed = null)
    {
        switch (outcome)
        {
            case CardOutcome.Completed:
                return _base.CalculateScore(card, player, CardOutcome.Completed, elapsed);

            case CardOutcome.Skipped:
                // Declining is free. See the class remarks — this is what makes
                // the attempt/decline choice a choice at all.
                return 0;

            case CardOutcome.Failed:
                // Ask the base what winning would have paid, and charge a share
                // of it. Asking with Completed is the point: querying with Failed
                // returns zero from every base strategy in the engine, which is
                // exactly the behaviour being replaced here.
                var atStake = _base.CalculateScore(card, player, CardOutcome.Completed, elapsed);

                // A base that pays nothing for this card risks nothing on it.
                // Guarding here rather than letting the arithmetic produce -0
                // keeps the sign honest for callers that log the delta.
                if (atStake <= 0) return 0;

                // Away-from-zero rounding, so a ratio small enough to round to
                // nothing still costs one point. A penalty that silently becomes
                // free is worse than no penalty, because the card text will have
                // promised one.
                var penalty = (int)Math.Ceiling(atStake * _failurePenaltyRatio);
                return -penalty;

            default:
                return 0;
        }
    }
}
