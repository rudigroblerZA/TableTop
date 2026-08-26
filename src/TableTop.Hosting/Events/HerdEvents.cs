namespace TableTop.Hosting.Events;

/// <summary>
/// A prompt everyone answers at the same time.
/// </summary>
/// <param name="RoundNumber">1-based round.</param>
/// <param name="TotalRounds">How many prompts the session will play.</param>
/// <param name="Prompt">The question itself, e.g. "name a breakfast cereal".</param>
/// <param name="Category">The prompt's category, for chrome.</param>
public sealed record HerdPromptReadyEvent(
    int RoundNumber,
    int TotalRounds,
    string Prompt,
    string Category);

/// <summary>
/// The result of a round, once every answer is in.
/// </summary>
/// <param name="Prompt">The prompt that was answered.</param>
/// <param name="Groups">
/// Answers grouped by what was said, largest group first. The key is the
/// answer as normalised for comparison; <see cref="AnswerGroup.Answer"/>
/// carries a version fit to show.
/// </param>
/// <param name="HerdAnswer">
/// The single most-given answer, or null when nothing was said more than once
/// — a genuine outcome, not an error: a round where everyone said something
/// different has no herd, and scores accordingly.
/// </param>
/// <param name="Scores">Points awarded this round, by player name.</param>
/// <param name="LoneVoiceName">
/// The player who was the only one to give their answer, when exactly one
/// player was — null otherwise. Scored separately from the herd; see
/// <c>HerdController</c> for why both are worth points.
/// </param>
public sealed record HerdRoundResolvedEvent(
    string Prompt,
    IReadOnlyList<AnswerGroup> Groups,
    string? HerdAnswer,
    IReadOnlyDictionary<string, int> Scores,
    string? LoneVoiceName);

/// <summary>A set of players who gave the same answer.</summary>
/// <param name="Answer">The answer, as first written, fit to display.</param>
/// <param name="PlayerNames">Everyone who gave it.</param>
public sealed record AnswerGroup(string Answer, IReadOnlyList<string> PlayerNames);

/// <summary>The session is over.</summary>
/// <param name="WinnerNames">Usually one name; more on a tie.</param>
/// <param name="FinalScores">Every player's total, highest first.</param>
/// <param name="RoundsPlayed">How many prompts were actually answered.</param>
public sealed record HerdGameEndedEvent(
    IReadOnlyList<string> WinnerNames,
    IReadOnlyList<KeyValuePair<string, int>> FinalScores,
    int RoundsPlayed);
