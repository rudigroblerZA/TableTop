namespace TableTop.Hosting.Persistence;

/// <summary>
/// A named, saved group of <see cref="PlayerProfile"/>s — the durable form of
/// "the people who play together", so a table doesn't re-enter the same roster
/// every session.
///
/// <para>
/// This is the <see cref="PlayerProfile"/>-shaped counterpart to
/// <c>TableTop.Presentation.Infrastructure.SavedRoster</c>, which the graphical
/// heads' Roaster screen builds. The two answer the same question with
/// different vocabularies — <c>SavedPlayer</c> (name/gender/age/couple) versus
/// the fuller <see cref="PlayerProfile"/> (schema version, parent/married,
/// stable <see cref="PlayerProfile.Id"/>) that Console's own
/// <c>IPlayerRepository</c> flow already uses. Console reuses its existing
/// shape rather than taking a dependency on <c>TableTop.Presentation</c>, which
/// it deliberately does not reference (backlog item 28).
/// </para>
///
/// <para>
/// <b>The two shapes stay separate on purpose (backlog S.1, settled 1.39.0).</b>
/// Neither is a superset — this one has the stable <c>Id</c>, <c>IsParent</c>,
/// <c>IsMarried</c> and schema versioning; <c>SavedPlayer</c> has <c>Team</c>,
/// which this one does not model. The full reasoning, including why the
/// dependency direction rules out a single shared type, is on
/// <c>IRosterStore</c>'s doc comment, which is where a reader of the other half
/// will look.
/// </para>
/// </summary>
public sealed class RosterProfile
{
    /// <summary>Current schema version. Increment when the shape changes.</summary>
    public const int CurrentSchemaVersion = 1;

    /// <summary>Version written with this record, for stale-format detection on load.</summary>
    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    /// <summary>The name the table gave this roster, e.g. "Friday Regulars".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The players in this roster, in entry order.</summary>
    public IReadOnlyList<PlayerProfile> Players { get; set; } = [];

    /// <summary>"3 players: Alice, Bob, Cara" — computed, not stored.</summary>
    public string Summary =>
        Players.Count == 0
            ? "empty"
            : $"{Players.Count} player{(Players.Count == 1 ? "" : "s")}: {string.Join(", ", Players.Select(p => p.Name))}";
}
