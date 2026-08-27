using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Progression;
using TableTop.Core.Abstractions.Scoring;

namespace TableTop.Hosting.Hints;

/// <summary>
/// Analyses recent play history and the player's current flow state to generate
/// a subtle suggestion for where to go next.
///
/// Completely UI-agnostic: produces <see cref="NextTurnHint"/> data records that
/// any renderer (Console, WinUI, MAUI, test assertions) can consume without modification.
/// </summary>
public interface IHintEngine
{
    /// <summary>
    /// Generates a hint for the specified player given their recent history.
    /// Returns null when no meaningful hint can be made (e.g. first card of the session).
    /// </summary>
    NextTurnHint? GenerateHint(
        IPlayer player,
        HintContext context);
}

/// <summary>
/// All information the hint engine needs to produce a suggestion.
/// Pure data — no UI, no side effects.
/// </summary>
/// <param name="RecentOutcomes">Most recent card outcomes for this player, latest first.</param>
/// <param name="RecentDifficulties">Difficulties of the most recently played cards, latest first.</param>
/// <param name="CurrentFlow">Current flow state, if the strategy supports it.</param>
/// <param name="SkipCount">How many times this player has skipped this session.</param>
/// <param name="Round">Current round number.</param>
/// <param name="Standings">All players' scores for relative context.</param>
public sealed record HintContext(
    IReadOnlyList<CardOutcome> RecentOutcomes,
    IReadOnlyList<Difficulty> RecentDifficulties,
    FlowState? CurrentFlow,
    int SkipCount,
    int Round,
    IReadOnlyList<(Guid Id, int Score)> Standings
);

/// <summary>
/// A hint about where to go next. Immutable data record — renderers decide how to display it.
/// </summary>
/// <param name="SuggestedDifficulty">The suggested difficulty for the next card.</param>
/// <param name="SuggestedPaceChange">The suggested pace adjustment; null means no pace change.</param>
/// <param name="NeutralHint">Neutral phrasing of the hint (used when gender is unknown).</param>
/// <param name="HimHint">Hint phrased for a male player. Null if same as <paramref name="NeutralHint"/>.</param>
/// <param name="HerHint">Hint phrased for a female player. Null if same as <paramref name="NeutralHint"/>.</param>
/// <param name="Urgency">How strongly to surface this hint in the UI.</param>
/// <param name="Reason">Machine-readable reason token for filtering and logging.</param>
public sealed record NextTurnHint(
    Difficulty SuggestedDifficulty,
    PaceHint? SuggestedPaceChange,
    string NeutralHint,
    string? HimHint,
    string? HerHint,
    HintUrgency Urgency,
    string Reason
)
{
    /// <summary>
    /// Returns the most appropriate hint text for the given player's gender attribute.
    /// Falls back to <see cref="NeutralHint"/> when no gender-specific variant exists.
    /// </summary>
    public string ForPlayer(IPlayer player)
    {
        var gender = player.Attributes.TryGetValue("gender", out var g) ? g.ToLowerInvariant() : "";
        return gender switch
        {
            "male" => HimHint ?? NeutralHint,
            "female" => HerHint ?? NeutralHint,
            _ => NeutralHint
        };
    }
}

/// <summary>A suggested change in escalation pace.</summary>
public enum PaceHint
{
    /// <summary>Reduce the pace — give the player more cards at their current level.</summary>
    SlowDown,
    /// <summary>Increase the pace — advance the player to harder cards sooner.</summary>
    SpeedUp,
}

/// <summary>How prominently to surface the hint in the UI.</summary>
public enum HintUrgency
{
    /// <summary>Soft suggestion — show subtly, easy to ignore.</summary>
    Gentle,
    /// <summary>Worth showing clearly but not demanding attention.</summary>
    Moderate,
    /// <summary>Player is clearly struggling or excelling — surface prominently.</summary>
    Strong,
}
