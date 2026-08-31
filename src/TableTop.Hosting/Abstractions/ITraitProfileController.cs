using TableTop.Core.Abstractions.Analysis;
using TableTop.Hosting.Events;

namespace TableTop.Hosting.Abstractions;

/// <summary>
/// Controls a trait-assessment session: everyone answers the same statement,
/// and the session ends in a profile per player rather than a winner.
///
/// <para>
/// Like <see cref="IHerdController"/> and unlike everything else here, there is
/// no current player, so no <c>CurrentPlayerName</c>. A head collects each
/// player's response however suits it and submits them together via
/// <see cref="SubmitResponses"/>.
/// </para>
///
/// <para>
/// <b>Item-major, not player-major.</b> The session walks the item bank once
/// with everyone answering each statement, rather than handing the device to
/// one player for all fifty. On a shared device the alternative means every
/// other player waits out a full inventory before their turn starts, and the
/// comparison — the reason two people play this — arrives only after the last
/// person finishes.
/// </para>
/// </summary>
public interface ITraitProfileController : IGameController
{
    /// <summary>Raised when a statement is ready for everyone to answer.</summary>
    event EventHandler<TraitItemReadyEvent> ItemReady;

    /// <summary>Raised once a statement's responses have been recorded.</summary>
    event EventHandler<TraitItemRecordedEvent> ItemRecorded;

    /// <summary>Raised when the session ends, carrying every profile and comparison.</summary>
    event EventHandler<TraitAssessmentCompletedEvent> AssessmentCompleted;

    /// <summary>Starts the session and raises the first statement.</summary>
    void Start();

    /// <summary>The current statement number, 1-based. Zero before <see cref="Start"/>.</summary>
    int ItemNumber { get; }

    /// <summary>How many statements this session will play.</summary>
    int TotalItems { get; }

    /// <summary>The instrument being scored against.</summary>
    TraitScale Scale { get; }

    /// <summary>
    /// The roster, in setup order.
    ///
    /// <para>
    /// Present because a head has to know who to collect responses from, and
    /// this family has no score dictionary to read the names out of the way
    /// <see cref="IHerdController"/> does. Deriving the roster from a scoreboard
    /// was always incidental; a mode with no score has to state it.
    /// </para>
    /// </summary>
    IReadOnlyList<string> PlayerNames { get; }

    /// <summary>
    /// Records every player's response to the current statement, then advances.
    ///
    /// <para>
    /// A player absent from <paramref name="responses"/> has skipped this
    /// statement: nothing is recorded for them and the item does not count
    /// toward their totals in either direction. Skipping is not the same as
    /// answering <see cref="LikertResponse.Neutral"/> — neutral is an opinion
    /// and widens the score's denominator, a skip is an absence and does not.
    /// </para>
    /// </summary>
    /// <param name="responses">Response by player name.</param>
    void SubmitResponses(IReadOnlyDictionary<string, LikertResponse> responses);

    /// <summary>Skips the current statement for everyone and advances.</summary>
    void Skip();

    /// <summary>
    /// Ends the session early, reporting profiles built from whatever has been
    /// answered so far. A partly-finished assessment is still a real result —
    /// <c>TraitScore.ItemCount</c> says how thin it is.
    /// </summary>
    void Quit();
}
