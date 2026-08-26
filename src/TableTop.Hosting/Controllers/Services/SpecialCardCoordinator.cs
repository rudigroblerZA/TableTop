using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Events;
using TableTop.Hosting.Persistence;

namespace TableTop.Hosting.Controllers.Services;

/// <summary>
/// Handles all special-card types (break, reward, inspiration) and bonus-card
/// injection, extracted from <see cref="CardTurnController"/> to keep the main
/// controller focused on the turn loop.
///
/// The coordinator is given a delegate bundle at construction time rather than
/// a reference to the controller, keeping it decoupled.
/// </summary>
internal sealed class SpecialCardCoordinator
{
    private readonly Func<IReadOnlyList<ScoreEntry>> _buildScores;
    private readonly Action<BreakCardDrawnEvent> _onBreakCard;
    private readonly Action<RewardCardDrawnEvent> _onRewardCard;
    private readonly Action<InspirationCardDrawnEvent> _onInspirationCard;
    private readonly Action<CardReadyEvent> _onCardReady;
    private readonly EffectApplicator _effectApplicator;
    private readonly Dictionary<Guid, List<SavedInspiration>> _playerInspirations;
    private readonly List<ICard> _bonusPool;

    /// <inheritdoc />
    public int RewardChanceInterval { get; }
    /// <inheritdoc />
    public int RegularCardsSinceBonus { get; private set; }

    public SpecialCardCoordinator(
        EffectApplicator effectApplicator,
        Dictionary<Guid, List<SavedInspiration>> playerInspirations,
        List<ICard> bonusPool,
        int rewardChanceInterval,
        Func<IReadOnlyList<ScoreEntry>> buildScores,
        Action<BreakCardDrawnEvent> onBreakCard,
        Action<RewardCardDrawnEvent> onRewardCard,
        Action<InspirationCardDrawnEvent> onInspirationCard,
        Action<CardReadyEvent> onCardReady)
    {
        _effectApplicator = effectApplicator;
        _playerInspirations = playerInspirations;
        _bonusPool = bonusPool;
        RewardChanceInterval = rewardChanceInterval;
        _buildScores = buildScores;
        _onBreakCard = onBreakCard;
        _onRewardCard = onRewardCard;
        _onInspirationCard = onInspirationCard;
        _onCardReady = onCardReady;
    }

    /// <summary>
    /// Returns true when the interval has been reached and a bonus card was injected.
    /// The caller should return after this without presenting the normal card.
    /// </summary>
    public bool TryInjectBonus(Func<ICard?> advanceTurn, Func<IPlayer?> currentPlayer, int round)
    {
        if (RewardChanceInterval <= 0 || _bonusPool.Count == 0) return false;
        if (RegularCardsSinceBonus < RewardChanceInterval) return false;

        RegularCardsSinceBonus = 0;
        _ = advanceTurn();
        var player = currentPlayer();
        if (player is null) return true;

        var card = _bonusPool[Random.Shared.Next(_bonusPool.Count)];
        if (card is IBreakCard bc) { HandleBreakCard(bc, player, round); return true; }
        if (card is IRewardCard rc) { HandleRewardCard(rc, player, round); return true; }

        // Bonus is a regular card — present normally
        var text = card is IPromptCard p ? p.ResolvePrompt(player) : card.Description;
        _onCardReady(new CardReadyEvent(
            Player: player, PlayerName: player.DisplayName, Card: card,
            CardTitle: card.Title, CardText: text, Category: card.Category,
            Difficulty: card.Difficulty.ToString(), Restriction: card.Restriction?.Description,
            Round: round));
        return true;
    }

    /// <summary>Increments the regular-card counter used for bonus injection.</summary>
    public void IncrementRegularCard() => RegularCardsSinceBonus++;

    // ── Special card handlers ──────────────────────────────────────────────────

    /// <inheritdoc />
    public void HandleBreakCard(IBreakCard card, IPlayer player, int round)
    {
        _effectApplicator.ApplyBreakEffect(card, player);

        _onBreakCard(new BreakCardDrawnEvent(
            PlayerName: player.DisplayName,
            CardTitle: card.Title,
            CardText: card.Description,
            Scope: card.Scope.ToString(),
            EffectType: card.Effect?.GetType().Name.Replace("Effect", "") ?? string.Empty,
            Round: round,
            Activity: card.Activity?.ToString() ?? string.Empty,
            DurationMinutes: card.DurationMinutes));
    }

    /// <inheritdoc />
    public void HandleRewardCard(IRewardCard card, IPlayer player, int round)
    {
        var (scoreDelta, description) = _effectApplicator.ApplyRewardEffect(card.Effect, player);

        _onRewardCard(new RewardCardDrawnEvent(
            PlayerName: player.DisplayName,
            CardTitle: card.Title,
            CardText: card.Description,
            EffectType: card.Effect.GetType().Name.Replace("Effect", ""),
            EffectDescription: description,
            ScoreDelta: scoreDelta,
            Round: round,
            CurrentScores: _buildScores()));
    }

    /// <inheritdoc />
    public void HandleInspirationCard(IInspirationCard card, IPlayer player, int round)
    {
        var saved = new SavedInspiration
        {
            CardId = card.Id,
            Title = card.Title,
            InspirationText = card.InspirationText,
            InspirationCategory = card.InspirationCategory,
            SavedAt = DateTimeOffset.UtcNow,
        };

        if (!_playerInspirations.ContainsKey(player.Id))
            _playerInspirations[player.Id] = [];
        _playerInspirations[player.Id].Add(saved);

        _onInspirationCard(new InspirationCardDrawnEvent(
            PlayerName: player.DisplayName,
            CardTitle: card.Title,
            InspirationText: card.InspirationText,
            InspirationCategory: card.InspirationCategory ?? string.Empty,
            Round: round));
    }
}