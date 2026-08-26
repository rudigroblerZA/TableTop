namespace TableTop.Core.Abstractions.Lifelines;

/// <summary>
/// A one-use aid that a player may activate during a game to get help
/// with a multiple-choice question. Each lifeline has its own activation logic
/// and returns a <see cref="LifelineResult"/> describing the outcome.
/// </summary>
public interface ILifeline
{
    /// <summary>Display name shown to the player (e.g. "Phone a Friend").</summary>
    string Name { get; }

    /// <summary>Brief description of what the lifeline does.</summary>
    string Description { get; }

    /// <summary>Whether this lifeline has not yet been used this session.</summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Activates the lifeline against the current question.
    /// Sets <see cref="IsAvailable"/> to false and returns the outcome narrative.
    /// </summary>
    /// <param name="card">The question currently on the hot seat.</param>
    /// <param name="player">The player activating the lifeline.</param>
    /// <param name="allPlayers">All players in the session (needed by Ask the Audience).</param>
    LifelineResult Activate(
        TableTop.Core.Abstractions.Cards.IMultipleChoiceCard card,
        TableTop.Core.Abstractions.Players.IPlayer player,
        System.Collections.Generic.IReadOnlyList<TableTop.Core.Abstractions.Players.IPlayer> allPlayers);
}

/// <summary>
/// The outcome of activating a lifeline.
/// </summary>
/// <param name="Narrative">Human-readable text to display (e.g. simulated phone call).</param>
/// <param name="RemainingOptions">Answer labels still viable after the lifeline ran. All four if no elimination occurred.</param>
/// <param name="Suggestion">The lifeline's best-guess answer, or null if it only eliminates options.</param>
public sealed record LifelineResult(
    string Narrative,
    System.Collections.Generic.IReadOnlyList<TableTop.Core.Abstractions.Cards.AnswerLabel> RemainingOptions,
    TableTop.Core.Abstractions.Cards.AnswerLabel? Suggestion = null
);
