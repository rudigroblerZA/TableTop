namespace TableTop.Hosting.Events;

/// <summary>
/// Raised when the dice have been rolled at the start of a Monogamy turn.
/// The UI should display the dice result and — if doubles — ask the player
/// to choose their zone before calling <c>ChooseZoneForDoubles</c>.
/// </summary>
public sealed record DiceRolledEvent(
    string PlayerName,
    int Die1,
    int Die2,
    int Total,
    bool IsDouble,
    string ResultingZone,   // "Foreplay", "Sensual", "Steamy", "Wild", "Fantasy"
    int Round
);

/// <summary>
/// Raised when a Monogamy card is ready.
/// Extends the normal card-ready pattern with zone, target, token, and duration metadata.
/// </summary>
public sealed record MonogamyCardReadyEvent(
    string PlayerName,
    string PartnerName,     // the other player in a two-player game, or empty
    string CardTitle,
    string CardText,        // gender-resolved prompt
    string Zone,            // "Foreplay", "Sensual", "Steamy", "Wild", "Fantasy"
    string Target,          // "ForDrawer", "ForPartner", "ForBoth", "PlayerChoice"
    int TokenValue,
    int? DurationMinutes,
    int Round
);

/// <summary>
/// Raised when a player completes a Monogamy challenge and earns tokens.
/// </summary>
public sealed record TokensAwardedEvent(
    string PlayerName,
    int TokensEarned,
    int TotalTokens,
    string Zone
);

/// <summary>
/// Raised when the active player rolls doubles and must choose their zone.
/// The UI should present the four zone choices and call
/// <c>IMonogamyController.ChooseZone</c> with the player's selection.
/// </summary>
public sealed record DoublesRolledEvent(
    string PlayerName,
    int Die1,
    int Die2,
    int Round
);

/// <summary>
/// Raised when the Monogamy game ends.
/// </summary>
public sealed record MonogamyGameEndedEvent(
    IReadOnlyList<MonogamyStanding> FinalStandings,
    string WinnerName,
    int TotalRounds
);

/// <summary>
/// Raised when a timed card (massage, bath, etc.) starts so the UI
/// can start a countdown timer.
/// </summary>
public sealed record MonogamyTimedCardEvent(
    string PlayerName,
    string CardTitle,
    int DurationMinutes,
    string ActivityType    // "Massage", "Bath", "Shower", etc.
);

/// <summary>
/// A summary of one completed card for the session history sidebar.
/// </summary>
public sealed record MonogamyCardHistoryItem(
    string Zone,
    string Title,
    bool WasCompleted,
    bool WasNegotiated
);

/// <summary>Final standing for one player in a Monogamy session.</summary>
public sealed record MonogamyStanding(
    string PlayerName,
    int Tokens,
    int CardsCompleted,
    int CardsSkipped,
    IReadOnlyDictionary<string, int> TokensByZone  // zone name → tokens earned
);
