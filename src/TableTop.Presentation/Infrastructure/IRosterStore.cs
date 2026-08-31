namespace TableTop.Presentation.Infrastructure;

/// <summary>A named group of players, saved for reuse.</summary>
public sealed class SavedRoster
{
    /// <summary>The name the player gave this roster, or the template's name if they didn't.</summary>
    public required string Name { get; init; }

    /// <summary>The template this roster was built from.</summary>
    public required string TemplateName { get; init; }

    /// <summary>The players configured into this roster, in entry order.</summary>
    public required IReadOnlyList<SavedPlayer> Players { get; init; }

    /// <summary>"Team · 3 players", for the saved-rosters list. Not persisted — computed from <see cref="Players"/>.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string Subtitle => $"{TemplateName} · {Players.Count} player{(Players.Count == 1 ? "" : "s")}";
}

/// <summary>
/// Persists saved rosters. Each head keeps its own storage — MAUI through
/// <c>Microsoft.Maui.Storage.Preferences</c>, WinUI through a local JSON
/// file — the same split <see cref="IAppSettings"/> already draws, and for
/// the same reason: only the shape (and the ViewModel that builds it) is
/// worth sharing.
///
/// <para>
/// <b>This is deliberately not the only roster model in the repo, and that is a
/// decision rather than an accident (backlog S.1, settled 1.39.0).</b>
/// <c>TableTop.Hosting.Persistence.IRosterRepository</c> / <c>RosterProfile</c>
/// persists the same idea — "a named group of players" — in a different
/// vocabulary, and Console uses that one. Four reasons the two stay separate:
/// </para>
///
/// <list type="number">
///   <item>
///     <b>Neither is a superset.</b> <see cref="SavedPlayer"/> carries
///     <c>Team</c>, which <c>PlayerProfile</c> has no concept of;
///     <c>PlayerProfile</c> carries a stable <c>Id</c>, <c>IsParent</c>,
///     <c>IsMarried</c> and a <c>SchemaVersion</c>, which this shape has no use
///     for. Merging produces a union type where every consumer ignores half the
///     fields. (An earlier note claimed <c>SavedRoster</c> was simply "richer"
///     and the better base — it is not; it is richer in one direction only.)
///   </item>
///   <item>
///     <b>The nullability difference is semantic.</b> <c>Gender</c> and
///     <c>Age</c> are nullable here because this shape models <i>setup input
///     part-way through being entered</i>; they are non-null with defaults on
///     <c>PlayerProfile</c> because that shape models a <i>durable profile</i>.
///     Same words, different lifecycle stage.
///   </item>
///   <item>
///     <b>The dependency direction forbids the cheap merge.</b> Console
///     deliberately does not reference <c>TableTop.Presentation</c> (backlog
///     item 28), and <c>Presentation</c> sits above <c>Hosting</c>. One shared
///     type in <c>Hosting</c> drags <c>Team</c> — a presentation concept — into
///     the engine; one shared type here forces Console to depend on the
///     ViewModel layer. Neither is acceptable.
///   </item>
///   <item>
///     <b>Sync versus async is not cosmetic.</b> This interface is synchronous
///     because per-head key-value storage is; <c>IRosterRepository</c> is
///     asynchronous because it is file I/O. Unifying makes one of them lie
///     about what it does.
///   </item>
/// </list>
///
/// <para>
/// <b>The accepted cost:</b> a roster saved in Console is invisible to the
/// graphical heads and vice versa, even on the same machine. That is a real
/// limitation and is accepted rather than explained away — the two flows do not
/// share a storage location today, and making them would mean picking one of
/// the two shapes above, which is the trade this note exists to refuse.
/// </para>
/// </summary>
public interface IRosterStore
{
    /// <summary>Loads every saved roster, or an empty list if none exist or the stored data is unreadable.</summary>
    IReadOnlyList<SavedRoster> Load();

    /// <summary>Persists the full current list of saved rosters, replacing whatever was there before.</summary>
    void Save(IReadOnlyList<SavedRoster> rosters);
}
