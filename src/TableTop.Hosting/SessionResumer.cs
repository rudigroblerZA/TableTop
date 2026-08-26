using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Players;
using TableTop.Hosting.Persistence;

namespace TableTop.Hosting;

/// <summary>
/// A saved session resolved back into the things needed to restart it: the mode
/// it was playing and the players who were at the table.
/// </summary>
/// <param name="Mode">The mode named in the snapshot.</param>
/// <param name="Players">The roster, with attributes and tags restored.</param>
/// <param name="Snapshot">The snapshot itself, to pass as <c>resumeFrom</c>.</param>
public sealed record ResumableSession(
    IGameMode Mode,
    IReadOnlyList<IPlayer> Players,
    SessionSnapshot Snapshot)
{
    /// <summary>Round the session had reached, for a "continue from round 4?" prompt.</summary>
    public int Round => Snapshot.Round;

    /// <summary>When it was saved, for the same purpose.</summary>
    public DateTimeOffset SavedAt => Snapshot.SavedAt;

    /// <summary>Player names, for the prompt: "Alice and Bob, round 4".</summary>
    public string PlayerSummary => string.Join(", ", Players.Select(p => p.DisplayName));
}

/// <summary>
/// Turns a <see cref="SessionSnapshot"/> back into a mode and a roster.
///
/// This lives in Hosting rather than in each head deliberately. Resume needs a
/// mode looked up by name and players rebuilt from saved state, and both have
/// sharp edges — a mode that no longer exists, a schema-1 snapshot with no
/// attributes, a roster that has since changed. Doing it once here means a head
/// needs only a button and a navigation call, and it can be tested without a UI.
/// </summary>
public static class SessionResumer
{
    /// <summary>
    /// Resolves the saved session, or returns null when there is nothing to
    /// resume or the snapshot can no longer be honoured.
    /// </summary>
    /// <param name="snapshot">The loaded snapshot, or null.</param>
    /// <param name="availableModes">
    /// Everything the app can play — normally <c>IArchetypeRegistry.AllModes</c>.
    /// </param>
    /// <param name="currentRoster">
    /// The live roster, if the app has one. Players are matched by id and
    /// preferred over the snapshot's copy, because the live object is the more
    /// current truth — a name changed since the save should survive the resume.
    /// Players in the snapshot but not the roster are rebuilt from the snapshot.
    /// </param>
    /// <param name="reason">Why null was returned, for logging or a message.</param>
    public static ResumableSession? TryResolve(
        SessionSnapshot? snapshot,
        IReadOnlyList<IGameMode> availableModes,
        IReadOnlyList<IPlayer>? currentRoster,
        out string reason)
    {
        if (snapshot is null)
        {
            reason = "No saved session.";
            return null;
        }

        if (snapshot.Players.Count == 0)
        {
            reason = "The saved session has no players.";
            return null;
        }

        var mode = availableModes.FirstOrDefault(
            m => string.Equals(m.Name, snapshot.ModeName, StringComparison.OrdinalIgnoreCase));

        if (mode is null)
        {
            // A mode removed between save and resume, or a custom JSON mode
            // whose file is gone. Refusing beats dropping the table into a
            // different game than the one they were playing.
            reason = $"'{snapshot.ModeName}' is no longer available.";
            return null;
        }

        var players = snapshot.Players
            .Select(saved => Restore(saved, currentRoster))
            .ToList()
            .AsReadOnly();

        reason = string.Empty;
        return new ResumableSession(mode, players, snapshot);
    }

    /// <summary>
    /// Rebuilds one player, preferring the live roster entry when the id matches.
    ///
    /// Scores are NOT applied here. The controller restores them from the
    /// snapshot when it resumes, and setting them twice would double every
    /// score on screen.
    /// </summary>
    private static IPlayer Restore(PlayerSessionState saved, IReadOnlyList<IPlayer>? roster)
    {
        var live = roster?.FirstOrDefault(p => p.Id == saved.PlayerId);
        if (live is not null) return live;

        // Not in the roster — rebuild from the snapshot. Attributes and tags are
        // empty for a schema-1 snapshot, which is the state resume had before
        // they were persisted; the session still runs, gender-directed cards
        // just fall back to neutral text.
        return new Player(
            saved.PlayerId,
            saved.DisplayName,
            saved.Attributes,
            saved.Tags);
    }
}
