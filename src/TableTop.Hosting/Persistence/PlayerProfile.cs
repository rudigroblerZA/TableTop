namespace TableTop.Hosting.Persistence;

/// <summary>
/// A saved player profile. Persisted between sessions.
/// Distinct from <c>IPlayer</c>, which is session-scoped and carries live state
/// (score, status). A profile is the durable identity; a player is the runtime instance.
/// </summary>
public sealed class PlayerProfile
{
    // ── Schema versioning ─────────────────────────────────────────────────────

    /// <summary>Current schema version. Increment when the shape changes.</summary>
    public const int CurrentSchemaVersion = 1;

    /// <summary>
    /// Version written with this record. Loaders check this against
    /// <see cref="CurrentSchemaVersion"/> to detect stale profiles.
    /// </summary>
    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    // ── Profile fields ────────────────────────────────────────────────────────

    /// <summary>Stable identifier — survives across sessions.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Player's display name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Player's gender identity. Used to select gendered card prompts. Defaults to <c>"other"</c>.</summary>
    public string Gender { get; set; } = "other";
    /// <summary>Player's age. Used by the restriction system to gate adult-only content (requires Age &gt;= 18).</summary>
    public int Age { get; set; } = 25;
    /// <summary>True when the player is a parent — used to surface parenting or family content in relevant modes.</summary>
    public bool IsParent { get; set; }
    /// <summary>True when the player is married or in a long-term partnership.</summary>
    public bool IsMarried { get; set; }
    /// <summary>True when the player is participating as part of a couple in couples-only game modes.</summary>
    public bool IsCoupleMember { get; set; }

    /// <summary>Computed — not stored. Derived from <see cref="Age"/> on load.</summary>
    public bool IsAdult => Age >= 18;

    /// <summary>
    /// Converts this profile into a fresh session <c>Player</c> instance.
    /// </summary>
    public Core.Domain.Players.Player ToPlayer()
    {
        var attrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["gender"] = Gender,
            ["age"] = Age.ToString()
        };

        var tags = new List<string>();
        if (IsAdult) tags.Add("adult");
        if (IsParent) tags.Add("parent");
        if (IsMarried) tags.Add("married");
        if (IsCoupleMember) tags.Add("couple-member");

        // Use the profile's stable Id so restriction metadata survives across sessions
        return new Core.Domain.Players.Player(Id, Name.Trim(), attrs, tags);
    }
}
