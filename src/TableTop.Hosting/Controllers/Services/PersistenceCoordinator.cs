using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Progression;
using TableTop.Hosting.Events;
using TableTop.Hosting.Persistence;

namespace TableTop.Hosting.Controllers.Services;

/// <summary>
/// Owns the save/resume half of <see cref="CardTurnController"/>: building a
/// <see cref="SessionSnapshot"/> from live session state, writing it through an
/// <see cref="IGamePersistence"/>, restoring one on resume, and clearing the
/// saved session when a game ends naturally.
///
/// Extracted from the controller (backlog B.1) alongside
/// <see cref="HintCoordinator"/>. The controller keeps the public
/// <c>SaveAsync</c> surface and delegates here; nothing about the on-disk
/// snapshot format changes.
/// </summary>
internal sealed class PersistenceCoordinator
{
    private readonly IGame                                     _game;
    private readonly IGamePersistence                          _repository;
    private readonly IReadOnlyList<IPlayer>                    _players;
    private readonly SkipPolicy                                _skipPolicy;
    private readonly IFlowAwareProgressionStrategy?            _flowStrategy;
    private readonly Dictionary<Guid, List<SavedInspiration>>  _playerInspirations;
    private readonly string                                    _modeName;
    private readonly string?                                   _modeFilePath;
    private readonly Action<SessionSavedEvent>                 _onSessionSaved;

    public PersistenceCoordinator(
        IGame                                    game,
        IGamePersistence                         repository,
        IReadOnlyList<IPlayer>                   players,
        SkipPolicy                               skipPolicy,
        IFlowAwareProgressionStrategy?           flowStrategy,
        Dictionary<Guid, List<SavedInspiration>> playerInspirations,
        string                                   modeName,
        string?                                  modeFilePath,
        Action<SessionSavedEvent>                onSessionSaved)
    {
        _game               = game;
        _repository         = repository;
        _players            = players;
        _skipPolicy         = skipPolicy;
        _flowStrategy       = flowStrategy;
        _playerInspirations = playerInspirations;
        _modeName           = modeName;
        _modeFilePath       = modeFilePath;
        _onSessionSaved     = onSessionSaved;
    }

    /// <summary>
    /// Builds a snapshot, persists it, and raises <see cref="SessionSavedEvent"/>.
    /// </summary>
    public async Task SaveAsync(CancellationToken ct = default)
    {
        var snapshot = BuildSnapshot();
        await _repository.SaveAsync(snapshot, ct).ConfigureAwait(false);
        _onSessionSaved(new SessionSavedEvent(
            _repository is JsonSessionRepository jsr ? jsr.FilePath : "session",
            snapshot.SavedAt));
    }

    /// <summary>
    /// Clears the saved session. Called when a game ends naturally, so a
    /// finished session is never offered for resume.
    /// </summary>
    public Task DeleteAsync() => _repository.DeleteAsync();

    /// <summary>
    /// Captures the current session as a <see cref="SessionSnapshot"/>.
    /// </summary>
    public SessionSnapshot BuildSnapshot()
    {
        var snap = new SessionSnapshot
        {
            ModeName          = _modeName,
            ModeFilePath      = _modeFilePath,
            Round             = _game.Round,
            Players           = _players.Select(p => new PlayerSessionState
            {
                PlayerId    = p.Id,
                DisplayName = p.DisplayName,
                Score       = p.Score,
                Status      = p.Status.ToString(),
                // Schema 2. Without these a resumed player came back with no
                // gender and no tags, so gender-directed cards fell back to
                // neutral text and tag-gated restrictions started failing.
                Attributes  = p.Attributes.ToDictionary(kv => kv.Key, kv => kv.Value),
                Tags        = p.Tags.ToList(),
            }).ToList(),
            // Resolved cards PLUS the one currently face-up: a revealed card
            // is spent (the table has read it — answers included), so resuming
            // must never deal it again. This was the source of an intermittent
            // save/resume duplicate before it was included.
            PlayedCardIds      = _game.PlayedCards.Select(c => c.Id)
                                     .Concat(_game.CurrentCard is { } cur ? new[] { cur.Id } : Array.Empty<Guid>())
                                     .Distinct()
                                     .ToList(),
            FreePassPlayerIds  = new List<Guid>(),  // managed by SkipPolicy
            ExtraCardPlayerIds = new List<Guid>(),  // managed by EffectApplicator
            SkipCounts         = _skipPolicy.Snapshot(),
            PlayerInspirations = _playerInspirations.ToDictionary(
                kv => kv.Key.ToString(),
                kv => kv.Value),
        };

        // Persist flow states if strategy supports it
        if (_flowStrategy is not null)
        {
            snap.FlowStates = _players.ToDictionary(
                p => p.Id.ToString(),
                p =>
                {
                    var fs = _flowStrategy.GetFlowState(p.Id);
                    return new FlowStateSnapshot
                    {
                        Difficulty         = fs.CurrentDifficulty.ToString(),
                        Pace               = fs.CurrentPace.ToString(),
                        CardsPlayedAtLevel = fs.CardsPlayedAtCurrentLevel,
                    };
                });
        }

        return snap;
    }

    /// <summary>
    /// Restores mid-session state from <paramref name="snap"/>. Called once
    /// during construction when the controller is resuming.
    /// </summary>
    public void Restore(SessionSnapshot snap)
    {
        // Restore scores
        foreach (var ps in snap.Players)
        {
            var delta = ps.Score - (_players.FirstOrDefault(p => p.Id == ps.PlayerId)?.Score ?? 0);
            if (delta != 0) _game.PlayerManager.ApplyScore(ps.PlayerId, delta);
        }

        // Restore played-card history so NoDuplicateCardRule excludes already-played cards.
        // We mark every played card for every player (conservative but correct — the snapshot
        // records card IDs but not which player played which card).
        if (snap.PlayedCardIds.Count > 0)
            _game.Metadata.SeedFromSnapshot(
                _players.Select(p => p.Id),
                snap.PlayedCardIds);

        // Free-pass / extra-card sets are managed by services (restored via SkipPolicy)

        if (snap.SkipCounts.Count > 0) _skipPolicy.Restore(snap.SkipCounts);

        // Restore inspirations
        foreach (var (key, list) in snap.PlayerInspirations)
            if (Guid.TryParse(key, out var id)) _playerInspirations[id] = list;

        // Restore flow states
        if (_flowStrategy is not null && snap.FlowStates is not null)
        {
            foreach (var (key, fss) in snap.FlowStates)
            {
                if (!Guid.TryParse(key, out var id)) continue;
                var diff  = Enum.TryParse<Difficulty>(fss.Difficulty, out var d) ? d : Difficulty.Easy;
                var pace  = Enum.TryParse<FlowPace>(fss.Pace, out var pa) ? pa : FlowPace.Normal;
                var state = new FlowState(diff, pace);
                for (var i = 0; i < fss.CardsPlayedAtLevel; i++) state.RecordCardPlayed();
                _flowStrategy.SetFlowState(id, state);
            }
        }
    }
}
