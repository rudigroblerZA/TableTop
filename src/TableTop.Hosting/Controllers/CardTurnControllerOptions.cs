using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Hosting.Hints;
using TableTop.Hosting.Persistence;

namespace TableTop.Hosting.Controllers;

/// <summary>
/// The optional settings for a <see cref="CardTurnController"/> (backlog D.1).
///
/// The controller used to take these as eight trailing optional parameters. By
/// the time backlog B.3 added a second constructor — one accepting a prebuilt
/// deck so <see cref="CardTurnController.CreateAsync"/> could stop blocking —
/// that meant two thirteen-parameter signatures kept in step by hand, and the
/// controller's own line-count guard failed on the duplication. Which is what
/// the guard is for.
///
/// Every member is optional and every default matches what the parameter
/// defaults were, so <c>null</c> and <c>new CardTurnControllerOptions()</c> both
/// mean "as before".
/// </summary>
public sealed record CardTurnControllerOptions
{
    /// <summary>Deck filtering, shuffling and per-player cap. Defaults to <see cref="GameplayOptions.Default"/>.</summary>
    public GameplayOptions? Gameplay { get; init; }

    /// <summary>Where sessions are saved. Defaults to a <see cref="JsonSessionRepository"/>.</summary>
    public IGamePersistence? SessionRepository { get; init; }

    /// <summary>Cards drawn from for reward and break interruptions.</summary>
    public IEnumerable<ICard>? BonusPool { get; init; }

    /// <summary>Inject a bonus card after this many regular cards. 0 disables it.</summary>
    public int RewardChanceInterval { get; init; }

    /// <summary>Score change per skip after the first, which is free.</summary>
    public int SkipPenalty { get; init; } = Core.TableTopDefaults.Scoring.SkipPenalty;

    /// <summary>A snapshot to resume from, or null to start fresh.</summary>
    public SessionSnapshot? ResumeFrom { get; init; }

    /// <summary>
    /// Path of the .gamemode.json this session was loaded from, recorded in the
    /// snapshot so a resume can identify which custom mode to reload. Null for
    /// built-in modes, which are found by name.
    ///
    /// The controller previously held this as a field that nothing ever
    /// assigned, behind a <c>#pragma warning disable CS0649</c> claiming
    /// ControllerFactory populated it. Nothing did, so every snapshot recorded
    /// null. It is a caller-supplied value, so it belongs here.
    /// </summary>
    public string? ModeFilePath { get; init; }

    /// <summary>Hint engine. Defaults to <see cref="DefaultHintEngine"/>.</summary>
    public IHintEngine? HintEngine { get; init; }

    /// <summary>Diagnostics sink. Defaults to a no-op.</summary>
    public Core.Abstractions.IEngineDiagnostics? Diagnostics { get; init; }

    /// <summary>The all-defaults instance, used when a caller passes null.</summary>
    public static CardTurnControllerOptions Default { get; } = new();
}
