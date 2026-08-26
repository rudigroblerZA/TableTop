using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Restrictions;

namespace TableTop.Core.Domain.Cards;

/// <summary>
/// Concrete <see cref="IBreakCard"/> implementation.
/// </summary>
public sealed class BreakCard : BaseCard, IBreakCard
{
    /// <summary>Initialises a new <see cref="BreakCard"/> instance.</summary>
    public BreakCard(
        Guid id,
        string title,
        string description,
        BreakScope scope,
        BreakEffect? effect           = null,
        BreakActivity? activity       = null,
        int? durationMinutes          = null,
        IEnumerable<string>? tags     = null,
        IRestriction? restriction     = null)
        : base(id, title, description, Difficulty.Easy, "Break", tags, restriction)
    {
        Scope           = scope;
        Effect          = effect;
        Activity        = activity;
        DurationMinutes = durationMinutes;
    }

    /// <inheritdoc />
    public BreakScope    Scope           { get; }
    /// <inheritdoc />
    public BreakEffect?  Effect          { get; }
    /// <inheritdoc />
    public BreakActivity? Activity       { get; }
    /// <inheritdoc />
    public int?          DurationMinutes { get; }

    // ── Factories ─────────────────────────────────────────────────────────────

    /// <summary>Initialises a new <see cref="CreateGroupBreak"/> instance.</summary>
    public static BreakCard CreateGroupBreak(
        string title, string description,
        BreakActivity? activity   = BreakActivity.GroupPause,
        int? durationMinutes      = null,
        string? activityText      = null,
        IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), title, description, BreakScope.AllPlayers,
            activityText is not null ? new GroupBreakEffect(activityText) : null,
            activity, durationMinutes, tags);

    /// <summary>Initialises a new <see cref="CreateBath"/> instance.</summary>
    public static BreakCard CreateBath(string title, string description,
        int durationMinutes = 20, IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), title, description, BreakScope.CurrentPlayer,
            new GroupBreakEffect("bath"), BreakActivity.Bath, durationMinutes, tags);

    /// <summary>Initialises a new <see cref="CreateShower"/> instance.</summary>
    public static BreakCard CreateShower(string title, string description,
        int durationMinutes = 10, IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), title, description, BreakScope.CurrentPlayer,
            new GroupBreakEffect("shower"), BreakActivity.Shower, durationMinutes, tags);

    /// <summary>Initialises a new <see cref="CreateMassage"/> instance.</summary>
    public static BreakCard CreateMassage(string title, string description,
        int durationMinutes = 15, IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), title, description, BreakScope.AllPlayers,
            new GroupBreakEffect("massage"), BreakActivity.Massage, durationMinutes, tags);

    /// <summary>Initialises a new <see cref="CreateEat"/> instance.</summary>
    public static BreakCard CreateEat(string title, string description,
        int durationMinutes = 15, IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), title, description, BreakScope.AllPlayers,
            new GroupBreakEffect("eat"), BreakActivity.Eat, durationMinutes, tags);

    /// <summary>Initialises a new <see cref="CreateDrink"/> instance.</summary>
    public static BreakCard CreateDrink(string title, string description,
        int? durationMinutes = null, IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), title, description, BreakScope.AllPlayers,
            new GroupBreakEffect("drink"), BreakActivity.Drink, durationMinutes, tags);

    /// <summary>Initialises a new <see cref="CreateRest"/> instance.</summary>
    public static BreakCard CreateRest(string title, string description,
        int durationMinutes = 10, IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), title, description, BreakScope.AllPlayers,
            new GroupBreakEffect("rest"), BreakActivity.Rest, durationMinutes, tags);

    /// <summary>Initialises a new <see cref="CreateExercise"/> instance.</summary>
    public static BreakCard CreateExercise(string title, string description,
        int durationMinutes = 5, IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), title, description, BreakScope.AllPlayers,
            new GroupBreakEffect("exercise"), BreakActivity.Exercise, durationMinutes, tags);

    /// <summary>Initialises a new <see cref="CreateSkipTurn"/> instance.</summary>
    public static BreakCard CreateSkipTurn(string title, string description,
        IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), title, description, BreakScope.CurrentPlayer,
            new SkipTurnEffect(), null, null, tags);

    /// <summary>Initialises a new <see cref="CreateRotate"/> instance.</summary>
    public static BreakCard CreateRotate(string title, string description,
        IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), title, description, BreakScope.AllPlayers,
            new RotatePlayersEffect(), BreakActivity.Rotate, null, tags);
}

/// <summary>
/// Concrete <see cref="IRewardCard"/> implementation.
/// </summary>
public sealed class RewardCard : BaseCard, IRewardCard
{
    /// <summary>Initialises a new <see cref="RewardCard"/> instance.</summary>
    public RewardCard(
        Guid id,
        string title,
        string description,
        RewardEffect effect,
        Difficulty difficulty        = Difficulty.Easy,
        IEnumerable<string>? tags   = null,
        IRestriction? restriction   = null)
        : base(id, title, description, difficulty, "Reward", tags, restriction)
    {
        Effect = effect ?? throw new ArgumentNullException(nameof(effect));
    }

    /// <inheritdoc />
    public RewardEffect Effect { get; }

    /// <summary>Initialises a new <see cref="CreateScoreBonus"/> instance.</summary>
    public static RewardCard CreateScoreBonus(string title, string description, int points,
        Difficulty difficulty = Difficulty.Easy, IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), title, description, new ScoreBonusEffect(points), difficulty, tags);

    /// <summary>Initialises a new <see cref="CreateStealPoints"/> instance.</summary>
    public static RewardCard CreateStealPoints(string title, string description, int points,
        Difficulty difficulty = Difficulty.Medium, IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), title, description, new StealPointsEffect(points), difficulty, tags);

    /// <summary>Initialises a new <see cref="CreateFreePass"/> instance.</summary>
    public static RewardCard CreateFreePass(string title, string description,
        IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), title, description, new FreePassEffect(), Difficulty.Easy, tags);

    /// <summary>Initialises a new <see cref="CreateExtraCard"/> instance.</summary>
    public static RewardCard CreateExtraCard(string title, string description,
        IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), title, description, new ExtraCardEffect(), Difficulty.Easy, tags);

    /// <summary>Initialises a new <see cref="CreateNarrative"/> instance.</summary>
    public static RewardCard CreateNarrative(string title, string description,
        Difficulty difficulty = Difficulty.Easy, IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), title, description, new NarrativeRewardEffect(description), difficulty, tags);
}

/// <summary>
/// Concrete <see cref="IInspirationCard"/> implementation.
/// </summary>
public sealed class InspirationCard : BaseCard, IInspirationCard
{
    /// <summary>Initialises a new <see cref="InspirationCard"/> instance.</summary>
    public InspirationCard(
        Guid id,
        string title,
        string description,
        string inspirationText,
        string? inspirationCategory  = null,
        Difficulty difficulty        = Difficulty.Easy,
        IEnumerable<string>? tags   = null,
        IRestriction? restriction   = null)
        : base(id, title, description, difficulty, "Inspiration", tags, restriction)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inspirationText);
        InspirationText     = inspirationText;
        InspirationCategory = inspirationCategory;
    }

    /// <inheritdoc />
    public string  InspirationText     { get; }
    /// <inheritdoc />
    public string? InspirationCategory { get; }

    /// <summary>Initialises a new <see cref="Create"/> instance.</summary>
    public static InspirationCard Create(
        string title, string description, string inspirationText,
        string? category = null, Difficulty difficulty = Difficulty.Easy,
        IEnumerable<string>? tags = null) =>
        new(Guid.NewGuid(), title, description, inspirationText, category, difficulty, tags);
}