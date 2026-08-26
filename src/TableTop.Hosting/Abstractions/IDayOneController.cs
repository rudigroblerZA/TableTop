using TableTop.Hosting.Events;

namespace TableTop.Hosting.Abstractions;

/// <summary>
/// Drives an advent-calendar campaign: exactly one card unlocks per real
/// calendar day. Unlike <see cref="ICardTurnController"/>, there is no
/// shuffling, no rounds, and no single sitting — progress persists across
/// real days via a small self-contained save file (a separate persistence
/// path from <see cref="TableTop.Hosting.Persistence.ISessionRepository"/>, deliberately, so this never
/// risks the existing card-turn save/resume contract).
///
/// Missed days are never lost: if three real days pass without playing,
/// all three become available at once ("catching up"), one at a time,
/// still in original day order.
/// </summary>
public interface IDayOneController : IGameController
{
    /// <summary>Raised when a day's card is ready to play.</summary>
    event EventHandler<DayReadyEvent>? DayReady;

    /// <summary>Raised when every unlocked day has been played.</summary>
    event EventHandler<AllCaughtUpEvent>? AllCaughtUp;

    /// <summary>Raised once, when the final day is completed.</summary>
    event EventHandler<CampaignCompleteEvent>? CampaignComplete;

    /// <summary>
    /// Begins (or resumes) the campaign, raising exactly one of
    /// <see cref="DayReady"/>, <see cref="AllCaughtUp"/>, or
    /// <see cref="CampaignComplete"/> to reflect current state.
    /// </summary>
    void Start();

    /// <summary>Marks today's card complete and advances, raising the next state event.</summary>
    void CompleteToday();

    /// <summary>1-based index of the most recently unlocked day.</summary>
    int DayNumber { get; }

    /// <summary>Total length of the campaign.</summary>
    int TotalDays { get; }

    /// <summary>True when there's an unplayed, unlocked card waiting right now.</summary>
    bool HasPendingCard { get; }
}
