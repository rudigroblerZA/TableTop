using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Events;

namespace TableTop.Hosting.Controllers.Services;

/// <summary>
/// Encapsulates the skip policy: the first skip per player per session is free;
/// subsequent skips apply a configurable score penalty.
/// A <see cref="TableTop.Core.Abstractions.Cards.RewardEffect"/> reward resets the counter and grants another free skip.
/// </summary>
public sealed class SkipPolicy
{
    private readonly int                    _penalty;
    private readonly Dictionary<Guid, int>  _skipCounts    = [];
    private readonly HashSet<Guid>          _freePassPlayers = [];

    /// <summary>Initialises a new <see cref="SkipPolicy"/> instance.</summary>
    public SkipPolicy(int penalty = -1) => _penalty = penalty;

    /// <inheritdoc />
    public void Initialise(IReadOnlyList<IPlayer> players)
    {
        foreach (var p in players)
            _skipCounts[p.Id] = 0;
    }

    /// <summary>Grants the player a free pass and resets their skip counter.</summary>
    public void GrantFreePass(Guid playerId)
    {
        _freePassPlayers.Add(playerId);
        _skipCounts[playerId] = 0;
    }

    /// <summary>
    /// Processes a skip attempt. Returns the penalty applied (0 when free).
    /// Mutates the skip counter for the player.
    /// </summary>
    public SkipAttemptedEvent ProcessSkip(
        IPlayer player, int round, IReadOnlyList<ScoreEntry> scores)
    {
        // Free-pass token redemption
        if (_freePassPlayers.Remove(player.Id))
        {
            _skipCounts[player.Id]++;
            return new SkipAttemptedEvent(
                player.DisplayName, IsFree: true,
                SkipCount: _skipCounts[player.Id], Penalty: 0,
                Round: round, CurrentScores: scores);
        }

        _skipCounts.TryGetValue(player.Id, out var count);
        _skipCounts[player.Id] = count + 1;

        var isFree  = count == 0;
        var penalty = isFree ? 0 : _penalty;

        return new SkipAttemptedEvent(
            player.DisplayName, IsFree: isFree,
            SkipCount: count + 1, Penalty: penalty,
            Round: round, CurrentScores: scores);
    }

    /// <inheritdoc />
    public int GetSkipCount(Guid playerId) =>
        _skipCounts.TryGetValue(playerId, out var c) ? c : 0;

    /// <inheritdoc />
    public Dictionary<string, int> Snapshot() =>
        _skipCounts.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);

    /// <inheritdoc />
    public void Restore(Dictionary<string, int> snapshot)
    {
        foreach (var (key, count) in snapshot)
            if (Guid.TryParse(key, out var id))
                _skipCounts[id] = count;
    }
}