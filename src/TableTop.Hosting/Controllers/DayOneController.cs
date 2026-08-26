using System.Text.Json;
using System.Text.Json.Serialization;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Events;

namespace TableTop.Hosting.Controllers;

/// <summary>Tiny persisted state for a Day One campaign — deliberately separate
/// from <see cref="TableTop.Hosting.Persistence.SessionSnapshot"/>; a linear
/// day-count campaign needs none of that shape's round/score/deck-shuffle
/// fields, and piggybacking on it would risk the existing save/resume tests.</summary>
internal sealed class DayOneSnapshot
{
    public DateTimeOffset StartedAtUtc { get; set; }
    public int CompletedCount { get; set; }
}

/// <inheritdoc cref="IDayOneController"/>
public sealed class DayOneController : IDayOneController
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IReadOnlyList<ICard> _deck;
    private readonly IReadOnlyList<IPlayer> _players;
    private readonly IClock _clock;
    private readonly string _filePath;

    private DayOneSnapshot _state = new();

    /// <inheritdoc />
    public event EventHandler<DayReadyEvent>? DayReady;
    /// <inheritdoc />
    public event EventHandler<AllCaughtUpEvent>? AllCaughtUp;
    /// <inheritdoc />
    public event EventHandler<CampaignCompleteEvent>? CampaignComplete;

    /// <inheritdoc />
    public bool IsRunning { get; private set; }
    /// <inheritdoc />
    public int DayNumber { get; private set; }
    /// <inheritdoc />
    public int TotalDays => _deck.Count;
    /// <inheritdoc />
    public bool HasPendingCard { get; private set; }

    /// <summary>Initialises a new campaign controller.</summary>
    /// <param name="deck">The strictly-ordered daily deck (index 0 = Day 1).</param>
    /// <param name="players">Players in this campaign (for prompt-card resolution).</param>
    /// <param name="modeName">Used to derive a stable per-campaign save file.</param>
    /// <param name="clock">Time source; defaults to the real system clock.</param>
    /// <param name="filePath">Explicit save-file override, mainly for tests.</param>
    public DayOneController(
        IReadOnlyList<ICard> deck,
        IReadOnlyList<IPlayer> players,
        string modeName,
        IClock? clock = null,
        string? filePath = null)
    {
        if (deck.Count == 0)
            throw new ArgumentException("A Day One campaign needs at least one day.", nameof(deck));

        _deck = deck;
        _players = players;
        _clock = clock ?? new SystemClock();
        var slug = new string(modeName.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        _filePath = filePath ?? Path.Combine(AppContext.BaseDirectory, $"dayone-{slug}.json");
    }

    /// <inheritdoc />
    public void Start()
    {
        var existed = File.Exists(_filePath);
        _state = LoadOrCreate();

        // A brand-new campaign must persist StartedAtUtc immediately — not
        // just after the first CompleteToday(). Otherwise a second Start()
        // call (app reopened before playing Day 1) would silently re-derive
        // "now" as the start date instead of preserving the true one, since
        // there'd be nothing on disk yet to load.
        if (!existed) Persist();

        IsRunning = true;
        Evaluate();
    }

    /// <inheritdoc />
    public void CompleteToday()
    {
        if (!IsRunning || !HasPendingCard) return;
        _state.CompletedCount++;
        Persist();
        Evaluate();
    }

    /// <summary>
    /// Recomputes state from (StartedAtUtc, CompletedCount, clock.UtcNow) and
    /// raises exactly one event. Pure function of persisted state + the
    /// clock — no hidden mutable timers, which is what makes this testable
    /// by just moving a fake clock forward.
    /// </summary>
    private void Evaluate()
    {
        var elapsedWholeDays = (int)(_clock.UtcNow - _state.StartedAtUtc).TotalDays;
        var unlockedCount = Math.Min(TotalDays, elapsedWholeDays + 1);   // Day 1 unlocks immediately
        var pendingIndex = _state.CompletedCount;                      // 0-based, next card due

        if (pendingIndex >= TotalDays)
        {
            DayNumber = TotalDays;
            HasPendingCard = false;
            CampaignComplete?.Invoke(this,
                new CampaignCompleteEvent(TotalDays, _state.StartedAtUtc, _clock.UtcNow));
            return;
        }

        if (pendingIndex < unlockedCount)
        {
            DayNumber = pendingIndex + 1;
            HasPendingCard = true;
            var card = _deck[pendingIndex];
            var text = card is IPromptCard prompt
                ? prompt.ResolvePrompt(_players.Count > 0 ? _players[0] : null!)
                : card.Description;
            DayReady?.Invoke(this, new DayReadyEvent(card, text, DayNumber, TotalDays));
            return;
        }

        // Caught up: played everything unlocked so far, next day isn't here yet.
        DayNumber = pendingIndex;   // most recent day actually played
        HasPendingCard = false;
        var nextUnlockAt = _state.StartedAtUtc + TimeSpan.FromDays(unlockedCount);
        AllCaughtUp?.Invoke(this,
            new AllCaughtUpEvent(DayNumber, TotalDays, nextUnlockAt - _clock.UtcNow));
    }

    private DayOneSnapshot LoadOrCreate()
    {
        if (File.Exists(_filePath))
        {
            try
            {
                var json = File.ReadAllText(_filePath);
                var loaded = JsonSerializer.Deserialize<DayOneSnapshot>(json, JsonOptions);
                if (loaded is not null) return loaded;
            }
            catch (JsonException) { /* corrupt save — start fresh rather than crash */ }
        }
        return new DayOneSnapshot { StartedAtUtc = _clock.UtcNow, CompletedCount = 0 };
    }

    private void Persist()
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        // Same crash-safe write-then-move pattern as JsonSessionRepository.
        var tmp = _filePath + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(_state, JsonOptions));
        File.Move(tmp, _filePath, overwrite: true);
    }

    /// <inheritdoc />
    public void Dispose() => IsRunning = false;
}
