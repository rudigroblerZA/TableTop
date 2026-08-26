using TableTop.Core.Abstractions.Cards;
using TableTop.Hosting.Events;

namespace TableTop.Hosting.Abstractions;

/// <summary>
/// Controls the Monogamy couples card game loop.
///
/// Turn flow:
/// <list type="number">
///   <item>Controller auto-rolls dice and raises <see cref="DiceRolled"/>.</item>
///   <item>If doubles: raises <see cref="DoublesRolled"/>, waits for <see cref="ChooseZone"/>.</item>
///   <item>Raises <see cref="CardReady"/> with the drawn card.</item>
///   <item>UI calls <see cref="CompleteCard"/>, <see cref="SkipCard"/>, or <see cref="NegotiateCard"/>.</item>
///   <item>Tokens are awarded, next turn begins.</item>
/// </list>
/// </summary>
public interface IMonogamyController : IGameController
{
    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>DiceRolled.</summary>
    event EventHandler<DiceRolledEvent> DiceRolled;
    /// <summary>DoublesRolled.</summary>
    event EventHandler<DoublesRolledEvent> DoublesRolled;
    /// <summary>CardReady.</summary>
    event EventHandler<MonogamyCardReadyEvent> CardReady;
    /// <summary>TokensAwarded.</summary>
    event EventHandler<TokensAwardedEvent> TokensAwarded;
    /// <summary>GameEnded.</summary>
    event EventHandler<MonogamyGameEndedEvent> GameEnded;
    /// <summary>TimedCardStarted.</summary>
    event EventHandler<MonogamyTimedCardEvent> TimedCardStarted;

    /// <summary>Running history of cards played this session.</summary>
    IReadOnlyList<MonogamyCardHistoryItem> CardHistory { get; }

    /// <summary>Player names keyed by ID (for display).</summary>
    IReadOnlyDictionary<Guid, string> PlayerNames { get; }

    // ── Read-only state ───────────────────────────────────────────────────────

    /// <summary>Current token totals, keyed by player ID.</summary>
    IReadOnlyDictionary<Guid, int> Tokens { get; }

    /// <summary>Tokens earned per zone per player, keyed by player ID then zone name.</summary>
    IReadOnlyDictionary<Guid, Dictionary<string, int>> TokensByZone { get; }

    /// <summary>Target token count to win. Null means play until deck exhausted.</summary>
    int? WinningTokenCount { get; }

    // ── Commands ──────────────────────────────────────────────────────────────

    /// <summary>Starts the game, rolls dice for the first player.</summary>
    void Start();

    /// <summary>
    /// When the last roll was doubles, call this to choose which zone to draw from.
    /// Ignored if the last roll was not doubles.
    /// </summary>
    void ChooseZone(MonogamyZone zone);

    /// <summary>
    /// The current player completed the challenge. Tokens are awarded and the
    /// next turn begins automatically.
    /// </summary>
    void CompleteCard();

    /// <summary>
    /// The current player skips the challenge. No tokens awarded.
    /// Configurable penalty can be applied.
    /// </summary>
    void SkipCard();

    /// <summary>
    /// The couple negotiates the card — agrees to a modified version.
    /// Awards a partial token (half, rounded down, minimum 0).
    /// </summary>
    void NegotiateCard();

    /// <summary>Ends the game immediately.</summary>
    void Quit();
}