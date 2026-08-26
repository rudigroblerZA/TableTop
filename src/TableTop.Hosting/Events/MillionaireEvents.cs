using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Lifelines;

namespace TableTop.Hosting.Events;

/// <summary>Raised when a new hot-seat player begins their run.</summary>
public sealed record HotSeatBeganEvent(
    string PlayerName,
    int    PlayerIndex,
    int    TotalPlayers
);

/// <summary>Raised when a new question is ready for the hot-seat player.</summary>
public sealed record QuestionReadyEvent(
    string                                   QuestionText,
    IReadOnlyDictionary<AnswerLabel, string> Answers,
    IReadOnlyList<AnswerLabel>               AvailableOptions,
    PrizeLadderSnapshot                      Ladder,
    IReadOnlyList<LifelineSnapshot>          Lifelines
);

/// <summary>Raised when a lifeline result is available.</summary>
public sealed record LifelineUsedEvent(
    string                     LifelineName,
    string                     Narrative,
    IReadOnlyList<AnswerLabel>  RemainingOptions,
    AnswerLabel?                Suggestion
);

/// <summary>Raised when the player answers correctly.</summary>
public sealed record AnswerCorrectEvent(
    long PrizeWon,
    bool SafeHavenReached,
    long GuaranteedPrize,
    PrizeLadderSnapshot Ladder
);

/// <summary>Raised when the player answers incorrectly.</summary>
public sealed record AnswerWrongEvent(
    AnswerLabel CorrectLabel,
    string      CorrectText,
    long        GuaranteedPrize
);

/// <summary>Raised when the player walks away.</summary>
public sealed record WalkedAwayEvent(long Prize);

/// <summary>Raised when the player wins £1,000,000.</summary>
public sealed record MillionaireWonEvent(string PlayerName);

/// <summary>Raised when all players have completed their hot-seat run.</summary>
public sealed record MillionaireGameEndedEvent(IReadOnlyList<HotSeatResult> Results);

/// <summary>Records the outcome of a hot-seat run — the player name and the prize they walked away with.</summary>
public sealed record HotSeatResult(string PlayerName, long Prize);

/// <summary>Immutable snapshot of the prize ladder for UI rendering.</summary>
public sealed record PrizeLadderSnapshot(
    IReadOnlyList<LadderRungSnapshot> Rungs,
    int                               CurrentIndex,
    long                              GuaranteedPrize,
    bool                              IsComplete
);

/// <summary>A snapshot of one rung on the prize ladder, used in the game-ended standings.</summary>
public sealed record LadderRungSnapshot(
    int    QuestionNumber,
    long   PrizeAmount,
    bool   IsSafeHaven,
    bool   IsCurrent,
    bool   IsPassed
);

/// <summary>The name and remaining-use status of a single lifeline.</summary>
public sealed record LifelineSnapshot(string Name, bool IsAvailable);
