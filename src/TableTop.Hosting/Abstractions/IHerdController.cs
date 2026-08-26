using TableTop.Hosting.Events;

namespace TableTop.Hosting.Abstractions;

/// <summary>
/// Controls a simultaneous-answer session: every player answers the same
/// prompt at once, and scoring turns on agreement rather than correctness.
///
/// <para>
/// Unlike every other controller here, there is no current player — so this
/// interface has no <c>CurrentPlayerName</c>. A head collects all the answers
/// however suits it (one shared device passed around, or each player typing on
/// their own) and submits them together via <see cref="SubmitAnswers"/>.
/// </para>
/// </summary>
public interface IHerdController : IGameController
{
    /// <summary>Raised when a new prompt is ready for everyone to answer.</summary>
    event EventHandler<HerdPromptReadyEvent> PromptReady;

    /// <summary>Raised once a round's answers have been scored.</summary>
    event EventHandler<HerdRoundResolvedEvent> RoundResolved;

    /// <summary>Raised when the session ends.</summary>
    event EventHandler<HerdGameEndedEvent> GameEnded;

    /// <summary>Starts the session and raises the first prompt.</summary>
    void Start();

    /// <summary>The current round number, 1-based. Zero before <see cref="Start"/>.</summary>
    int RoundNumber { get; }

    /// <summary>How many prompts this session will play.</summary>
    int TotalRounds { get; }

    /// <summary>Live scores by player name.</summary>
    IReadOnlyDictionary<string, int> Scores { get; }

    /// <summary>
    /// Submits every player's answer for the current round, scores it, raises
    /// <see cref="RoundResolved"/>, then either raises the next
    /// <see cref="PromptReady"/> or ends the session.
    /// </summary>
    /// <param name="answers">Answer by player name. A blank answer counts as no answer and scores nothing.</param>
    void SubmitAnswers(IReadOnlyDictionary<string, string> answers);

    /// <summary>Ends the session early, reporting standings as they stand.</summary>
    void Quit();
}
