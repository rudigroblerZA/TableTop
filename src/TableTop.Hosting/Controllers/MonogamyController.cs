using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Progression;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Events;

namespace TableTop.Hosting.Controllers;

/// <summary>
/// Drives the Monogamy couples card game.
/// Zero UI code — raises typed events; any renderer subscribes.
/// </summary>
public sealed class MonogamyController : IMonogamyController
{
    private readonly IReadOnlyList<IPlayer> _players;
    private readonly List<MonogamyCard> _deck;
    private readonly Random _rng;
    private readonly int? _winningTokenCount;

    // ── Per-player tracking ───────────────────────────────────────────────────

    private readonly Dictionary<Guid, int> _tokens = [];
    private readonly Dictionary<Guid, Dictionary<string, int>> _byZone = [];
    private readonly Dictionary<Guid, int> _completed = [];
    private readonly Dictionary<Guid, int> _skipped = [];

    private int _currentPlayerIndex;
    private int _round;
    private bool _awaitingZoneChoice;
    private IMonogamyCard? _currentCard;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>DiceRolled.</summary>
    public event EventHandler<DiceRolledEvent>? DiceRolled;
    /// <summary>TimedCardStarted.</summary>
    public event EventHandler<MonogamyTimedCardEvent>? TimedCardStarted;
    /// <summary>DoublesRolled.</summary>
    public event EventHandler<DoublesRolledEvent>? DoublesRolled;
    /// <summary>CardReady.</summary>
    public event EventHandler<MonogamyCardReadyEvent>? CardReady;
    /// <summary>TokensAwarded.</summary>
    public event EventHandler<TokensAwardedEvent>? TokensAwarded;
    /// <summary>GameEnded.</summary>
    public event EventHandler<MonogamyGameEndedEvent>? GameEnded;

    /// <summary>True while the Monogamy game loop is active.</summary>
    public bool IsRunning { get; private set; }

    private readonly List<MonogamyCardHistoryItem> _history = [];
    /// <summary>CardHistory.</summary>
    public IReadOnlyList<MonogamyCardHistoryItem> CardHistory => _history.AsReadOnly();
    /// <inheritdoc />
    public IReadOnlyDictionary<Guid, string> PlayerNames =>
        _players.ToDictionary(p => p.Id, p => p.DisplayName);

    // ── IMonogamyController ───────────────────────────────────────────────────

    /// <summary>Tokens.</summary>
    public IReadOnlyDictionary<Guid, int> Tokens => _tokens;

    /// <summary>TokensByZone.</summary>
    public IReadOnlyDictionary<Guid, Dictionary<string, int>> TokensByZone => _byZone;

    /// <summary>Number of tokens a player must collect to win. Null means play until the deck is exhausted.</summary>
    public int? WinningTokenCount => _winningTokenCount;

    // ── Construction ──────────────────────────────────────────────────────────

    /// <summary>Initialises a new <see cref="MonogamyController"/> instance.</summary>
    public MonogamyController(
        IReadOnlyList<IPlayer> players,
        IReadOnlyList<MonogamyCard> cards,
        int? winningTokenCount = 10,
        Random? rng = null)
    {
        if (players.Count < 2)
            throw new ArgumentException("Monogamy requires at least 2 players.", nameof(players));

        _players = players;
        _winningTokenCount = winningTokenCount;
        _rng = rng ?? Random.Shared;

        // Build and shuffle deck
        _deck = Shuffle(cards.ToList(), rng ?? Random.Shared);

        // Initialise per-player tracking
        foreach (var p in players)
        {
            _tokens[p.Id] = 0;
            _byZone[p.Id] = [];
            _completed[p.Id] = 0;
            _skipped[p.Id] = 0;
        }
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    /// <summary>Starts the Monogamy game loop and raises the first <see cref="TableTop.Hosting.Events.MonogamyCardReadyEvent"/>.</summary>
    public void Start()
    {
        IsRunning = true;
        _round = 1;
        BeginTurn();
    }

    private MonogamyZone? _pendingZone;

    /// <summary>Called after a doubles roll to let the player select their preferred intimacy zone.</summary>
    public void ChooseZone(MonogamyZone zone)
    {
        if (!_awaitingZoneChoice) return;
        _awaitingZoneChoice = false;
        _pendingZone = zone;
        DrawCard();
    }

    /// <summary>Marks the current card as completed (both partners accepted and acted on the prompt).</summary>
    public void CompleteCard() => RecordOutcome(completed: true, negotiated: false);

    /// <summary>Skips the current card with no penalty in Monogamy (any card may be skipped freely).</summary>
    public void SkipCard()
    {
        if (_currentCard is null) return;
        var player = CurrentPlayer;
        _skipped[player.Id]++;
        _currentCard = null;
        AdvanceToNextPlayer();
    }

    /// <summary>Marks the current card as negotiated — played but with modifications agreed by both partners.</summary>
    public void NegotiateCard() => RecordOutcome(completed: false, negotiated: true);

    /// <summary>Ends the Monogamy session immediately.</summary>
    public void Quit()
    {
        IsRunning = false;
        FireGameEnded();
    }

    // ── Private flow ──────────────────────────────────────────────────────────

    private DiceRoll? _lastRoll;

    private void BeginTurn()
    {
        if (!IsRunning) return;
        _round++;

        var player = CurrentPlayer;
        var roll = DiceRoll.Roll(_rng);
        _lastRoll = roll;
        _pendingZone = null;

        DiceRolled?.Invoke(this, new DiceRolledEvent(
            PlayerName: player.DisplayName,
            Die1: roll.Die1,
            Die2: roll.Die2,
            Total: roll.Total,
            IsDouble: roll.IsDouble,
            ResultingZone: roll.ToZone().ToString(),
            Round: _round));

        if (roll.IsDouble)
        {
            _awaitingZoneChoice = true;
            DoublesRolled?.Invoke(this, new DoublesRolledEvent(
                PlayerName: player.DisplayName,
                Die1: roll.Die1,
                Die2: roll.Die2,
                Round: _round));
            // Wait for ChooseZone() call
        }
        else
        {
            DrawCard();
        }
    }

    private void DrawCard()
    {
        var player = CurrentPlayer;
        var partner = PartnerOf(player);

        var zone = _pendingZone ?? _lastRoll?.ToZone() ?? MonogamyZone.Foreplay;
        _pendingZone = null;

        var card = DrawFromZone(zone)
                ?? DrawNearestZone(zone)
                ?? DrawAny();

        if (card is null)
        {
            // Deck exhausted
            IsRunning = false;
            FireGameEnded();
            return;
        }

        _currentCard = card;

        // Resolve gender-directed text for the drawing player
        var text = card is IPromptCard prompt
            ? prompt.ResolvePrompt(player)
            : card.Description;

        CardReady?.Invoke(this, new MonogamyCardReadyEvent(
            PlayerName: player.DisplayName,
            PartnerName: partner?.DisplayName ?? string.Empty,
            CardTitle: card.Title,
            CardText: text,
            Zone: card.Zone.ToString(),
            Target: card.Target.ToString(),
            TokenValue: card.TokenValue,
            DurationMinutes: card.DurationMinutes,
            Round: _round));

        if (card.DurationMinutes.HasValue)
        {
            var activity = "Activity";
            TimedCardStarted?.Invoke(this, new MonogamyTimedCardEvent(
                PlayerName: player.DisplayName,
                CardTitle: card.Title,
                DurationMinutes: card.DurationMinutes.Value,
                ActivityType: activity));
        }
    }

    private void RecordOutcome(bool completed, bool negotiated)
    {
        if (_currentCard is null) return;

        var player = CurrentPlayer;
        var zone = _currentCard.Zone.ToString();
        var tokens = negotiated
            ? Math.Max(0, _currentCard.TokenValue / 2)
            : completed ? _currentCard.TokenValue : 0;

        if (tokens > 0)
        {
            _tokens[player.Id] += tokens;
            _byZone[player.Id].TryGetValue(zone, out var zoneTotal);
            _byZone[player.Id][zone] = zoneTotal + tokens;

            TokensAwarded?.Invoke(this, new TokensAwardedEvent(
                PlayerName: player.DisplayName,
                TokensEarned: tokens,
                TotalTokens: _tokens[player.Id],
                Zone: zone));
        }

        if (completed || negotiated) _completed[player.Id]++;
        else _skipped[player.Id]++;

        // Record history
        _history.Add(new MonogamyCardHistoryItem(
            Zone: (_currentCard?.Zone.ToString() ?? ""),
            Title: (_currentCard?.Title ?? ""),
            WasCompleted: completed,
            WasNegotiated: negotiated));

        _currentCard = null;

        // Check win condition
        if (_winningTokenCount.HasValue
            && _tokens[player.Id] >= _winningTokenCount.Value)
        {
            IsRunning = false;
            FireGameEnded();
            return;
        }

        AdvanceToNextPlayer();
    }

    private void AdvanceToNextPlayer()
    {
        _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        if (IsRunning) BeginTurn();
    }

    private void FireGameEnded()
    {
        var standings = _players
            .OrderByDescending(p => _tokens[p.Id])
            .Select(p => new MonogamyStanding(
                PlayerName: p.DisplayName,
                Tokens: _tokens[p.Id],
                CardsCompleted: _completed[p.Id],
                CardsSkipped: _skipped[p.Id],
                TokensByZone: _byZone[p.Id]))
            .ToList().AsReadOnly();

        var winner = standings[0];

        GameEnded?.Invoke(this, new MonogamyGameEndedEvent(
            FinalStandings: standings,
            WinnerName: winner.PlayerName,
            TotalRounds: _round));
    }

    // ── Deck helpers ──────────────────────────────────────────────────────────

    private IMonogamyCard? DrawFromZone(MonogamyZone zone)
    {
        var idx = _deck.FindIndex(c => c.Zone == zone);
        if (idx < 0) return null;
        var card = _deck[idx];
        _deck.RemoveAt(idx);
        return card;
    }

    private IMonogamyCard? DrawNearestZone(MonogamyZone preferred)
    {
        foreach (var zone in Enum.GetValues<MonogamyZone>()
            .OrderBy(z => Math.Abs((int)z - (int)preferred))
            .Skip(1))
        {
            var card = DrawFromZone(zone);
            if (card is not null) return card;
        }
        return null;
    }

    private IMonogamyCard? DrawAny()
    {
        if (_deck.Count == 0) return null;
        var card = _deck[0];
        _deck.RemoveAt(0);
        return card;
    }

    // ── Player helpers ────────────────────────────────────────────────────────

    private IPlayer CurrentPlayer => _players[_currentPlayerIndex];

    private IPlayer? PartnerOf(IPlayer player) =>
        _players.FirstOrDefault(p => p.Id != player.Id);

    private static List<T> Shuffle<T>(List<T> list, Random rng)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }

    /// <inheritdoc />
    public void Dispose() { /* no managed resources to release */ }
}