using TableTop.Core.Abstractions.Cards;

namespace TableTop.Hosting.Events;

/// <summary>Today's card is ready to play.</summary>
/// <param name="Card">The card for this day (already resolved if it's a prompt card).</param>
/// <param name="CardText">Prompt-resolved, display-ready text.</param>
/// <param name="DayNumber">1-based day index (Day 1, Day 2, …).</param>
/// <param name="TotalDays">Length of the whole campaign.</param>
public sealed record DayReadyEvent(ICard Card, string CardText, int DayNumber, int TotalDays);

/// <summary>
/// Every unlocked day has been played; the next one isn't available yet.
/// Not an error state — this is what "you're all caught up" looks like.
/// </summary>
/// <param name="DayNumber">The most recent day played.</param>
/// <param name="TotalDays">Length of the whole campaign.</param>
/// <param name="TimeUntilNextUnlock">How long until tomorrow's card unlocks.</param>
public sealed record AllCaughtUpEvent(int DayNumber, int TotalDays, TimeSpan TimeUntilNextUnlock);

/// <summary>The full campaign has been completed — every day played.</summary>
/// <param name="TotalDays">Length of the whole campaign.</param>
/// <param name="StartedAt">When Day 1 was first unlocked.</param>
/// <param name="CompletedAt">When the final day was completed.</param>
public sealed record CampaignCompleteEvent(int TotalDays, DateTimeOffset StartedAt, DateTimeOffset CompletedAt);
