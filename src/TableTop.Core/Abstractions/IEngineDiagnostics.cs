using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Rules;
using TableTop.Core.Abstractions.Scoring;

namespace TableTop.Core.Abstractions;

/// <summary>
/// Optional diagnostic sink threaded through the engine.
/// Implement this to receive structured events from the rule evaluator, turn loop,
/// and card selection without a dependency on a specific logging framework.
///
/// A no-op null implementation is used by default so no allocation occurs unless
/// diagnostics are enabled.
///
/// Register via DI by calling <c>services.AddSingleton&lt;IEngineDiagnostics, MyDiagnostics&gt;()</c>
/// or pass directly to <see cref="TableTop.Core.Domain.Rules.RuleEvaluator"/>.
///
/// For hosts that already use <c>Microsoft.Extensions.Logging</c>, wrap an
/// <c>ILogger</c> inside a class that implements this interface.
/// </summary>
public interface IEngineDiagnostics
{
    // ── Rule evaluation ───────────────────────────────────────────────────────

    /// <summary>Called for each rule that allows a card (score delta may be non-zero).</summary>
    void RuleAllowed(IRule rule, ICard card, IPlayer player, int scoreDelta) { }

    /// <summary>
    /// Called when a rule denies a card to a player.
    /// This is the key diagnostic: when a card keeps getting denied, this reveals which
    /// rule is responsible and why.
    /// </summary>
    void RuleDenied(IRule rule, ICard card, IPlayer player, string reason) { }

    // ── Card selection ────────────────────────────────────────────────────────

    /// <summary>Called when the engine selects a card for a player's turn.</summary>
    void CardSelected(ICard card, IPlayer player, int round) { }

    /// <summary>
    /// Called when the engine exhausted all available candidates without finding
    /// an eligible card (all were denied by rules or the deck is empty).
    /// </summary>
    void NoCardAvailable(IPlayer player, int candidatesExhausted, int round) { }

    // ── Turn lifecycle ────────────────────────────────────────────────────────

    /// <summary>Called when a turn outcome is recorded and scored.</summary>
    void TurnRecorded(IPlayer player, ICard card, CardOutcome outcome, int scoreDelta, int round) { }

    /// <summary>Called when UndoLastTurn() reverses a completed turn.</summary>
    void TurnUndone(IPlayer player, ICard card, CardOutcome reversed, int scoreRestored) { }

    // ── Session ───────────────────────────────────────────────────────────────

    /// <summary>Called when the game session starts.</summary>
    void GameStarted(string modeName, int playerCount) { }

    /// <summary>Called when the game session ends (naturally or via Quit).</summary>
    void GameEnded(string modeName, int totalRounds, int totalTurns) { }

}

/// <summary>
/// Default no-op implementation — all methods are empty default interface implementations
/// so this allocates nothing and all calls optimise away at JIT time.
/// </summary>
public sealed class NullEngineDiagnostics : IEngineDiagnostics
{
    /// <summary>Shared singleton — no state, safe to use everywhere.</summary>
    public static readonly NullEngineDiagnostics Instance = new();

    private NullEngineDiagnostics() { }
}
