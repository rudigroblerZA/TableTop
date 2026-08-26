namespace TableTop.Hosting.Persistence;

/// <summary>
/// A serialisable snapshot of a running game session.
/// Saved to disk so the session can be resumed later.
/// </summary>
public sealed class SessionSnapshot
{
    // ── Schema versioning ─────────────────────────────────────────────────────

    /// <summary>
    /// Current schema version. Increment this constant whenever the shape of
    /// <see cref="TableTop.Hosting.Persistence.SessionSnapshot"/> or any of its nested types changes in a
    /// way that would make an old save file unreadable or produce incorrect
    /// state when loaded.
    /// </summary>
    /// <summary>
    /// 2 — schema 1 did not persist player attributes or tags. A schema-1
    /// snapshot still loads; its players simply come back without them, which
    /// is exactly the state resume had before this.
    /// </summary>
    public const int CurrentSchemaVersion = 2;

    /// <summary>
    /// Version of the schema used when this snapshot was written.
    /// Defaults to 1 (the initial versioned format).
    /// Loaders compare this against <see cref="CurrentSchemaVersion"/> and can
    /// invoke a migration path rather than failing silently.
    /// </summary>
    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    /// <summary>
    /// Returns true when the snapshot was written with an older schema and may
    /// need migration before use.
    /// </summary>
    public bool RequiresMigration => SchemaVersion < CurrentSchemaVersion;

    // ── Identity ──────────────────────────────────────────────────────────────

    /// <summary>Unique identifier of this session (matches the IGame.Id).</summary>
    public Guid SessionId { get; set; } = Guid.NewGuid();

    /// <summary>When this snapshot was taken.</summary>
    public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Name of the game mode that was running.</summary>
    public string ModeName { get; set; } = string.Empty;

    /// <summary>
    /// Path to the .gamemode.json file, if the mode was loaded from disk.
    /// Null for built-in modes.
    /// </summary>
    public string? ModeFilePath { get; set; }

    /// <summary>Current round number at the time of save.</summary>
    public int Round { get; set; }

    /// <summary>Player states at the time of save.</summary>
    public List<PlayerSessionState> Players { get; set; } = [];

    /// <summary>IDs of all cards played so far, in order.</summary>
    public List<Guid> PlayedCardIds { get; set; } = [];

    /// <summary>IDs of players who currently have a free pass.</summary>
    public List<Guid> FreePassPlayerIds { get; set; } = [];

    /// <summary>IDs of players who have an extra card draw pending.</summary>
    public List<Guid> ExtraCardPlayerIds { get; set; } = [];

    /// <summary>
    /// Number of times each player has skipped, keyed by player ID string.
    /// First skip is free; subsequent skips apply a penalty.
    /// </summary>
    public Dictionary<string, int> SkipCounts { get; set; } = [];

    /// <summary>Inspiration cards saved per player during this session.</summary>
    public Dictionary<string, List<SavedInspiration>> PlayerInspirations { get; set; } = [];

    /// <summary>
    /// Serialised flow states per player (keyed by player ID string).
    /// Only present when the session used a flow-aware progression strategy.
    /// </summary>
    public Dictionary<string, FlowStateSnapshot>? FlowStates { get; set; }
}

/// <summary>Serialisable snapshot of a FlowState.</summary>
public sealed class FlowStateSnapshot
{
    /// <summary>The difficulty tier at which this player's flow was sitting when the session was saved.</summary>
    public string Difficulty           { get; set; } = "Easy";
    /// <summary>The escalation pace at which this player's flow was sitting when the session was saved.</summary>
    public string Pace                 { get; set; } = "Normal";
    /// <summary>Cards played at the saved difficulty level, used to continue auto-escalation counting after resume.</summary>
    public int    CardsPlayedAtLevel   { get; set; }
}

/// <summary>Per-player state at save time.</summary>
public sealed class PlayerSessionState
{
    /// <summary>The player's stable identity GUID.</summary>
    public Guid   PlayerId    { get; set; }
    /// <summary>The player's display name at save time.</summary>
    public string DisplayName { get; set; } = string.Empty;
    /// <summary>The player's score at save time.</summary>
    public int    Score       { get; set; }
    /// <summary>The player's status (<c>Active</c> or <c>Inactive</c>) at save time.</summary>
    public string Status      { get; set; } = "Active";

    /// <summary>
    /// The player's attributes at save time — gender above all.
    ///
    /// Added in schema 2. Without it a resumed session rebuilt players with no
    /// attributes at all, so every gender-directed card resolved to its neutral
    /// text and the couples modes quietly stopped addressing anyone. A schema-1
    /// snapshot has none of this; see the note on <c>Tags</c>.
    /// </summary>
    public Dictionary<string, string> Attributes { get; set; } = [];

    /// <summary>
    /// The player's tags at save time (<c>couple-member</c>, <c>adult</c>, and
    /// so on). Added in schema 2 alongside <see cref="Attributes"/>: tags gate
    /// restrictions, so a resumed player without them fails eligibility checks
    /// that passed a moment earlier.
    /// </summary>
    public List<string> Tags { get; set; } = [];
}

/// <summary>An inspiration card saved to a player's list during a session.</summary>
public sealed class SavedInspiration
{
    /// <summary>The saved inspiration card's ID.</summary>
    public Guid    CardId             { get; set; }
    /// <summary>The saved inspiration card's title.</summary>
    public string  Title              { get; set; } = string.Empty;
    /// <summary>The inspiration prompt text that was saved.</summary>
    public string  InspirationText    { get; set; } = string.Empty;
    /// <summary>The category of this inspiration card, if one was set.</summary>
    public string? InspirationCategory{ get; set; }
    /// <summary>SavedAt.</summary>
    public DateTimeOffset SavedAt     { get; set; } = DateTimeOffset.UtcNow;

}