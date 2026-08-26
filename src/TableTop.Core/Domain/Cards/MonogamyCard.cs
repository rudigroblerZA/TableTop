using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Restrictions;
using TableTop.Core.Domain.Restrictions;

namespace TableTop.Core.Domain.Cards;

/// <summary>
/// A Monogamy game card. Extends <see cref="PromptCard"/> so partner-specific
/// text is resolved at draw time based on the drawing player's gender,
/// and also implements <see cref="IMonogamyCard"/> for zone/target metadata.
/// </summary>
public sealed class MonogamyCard : BaseCard, IMonogamyCard, IPromptCard
{
    private readonly ICardTextResolver _resolver;

    /// <summary>Initialises a new <see cref="MonogamyCard"/> instance.</summary>
    public MonogamyCard(
        Guid              id,
        string            title,
        string            baseDescription,
        ICardTextResolver resolver,
        MonogamyZone      zone,
        CardTarget        target,
        int               tokenValue     = 1,
        int?              durationMinutes = null,
        IEnumerable<string>? tags        = null,
        IRestriction?     restriction    = null)
        : base(id, title, baseDescription,
               ZoneToDifficulty(zone), zone.ToString(), tags, restriction)
    {
        _resolver       = resolver ?? throw new ArgumentNullException(nameof(resolver));
        Zone            = zone;
        Target          = target;
        TokenValue      = tokenValue;
        DurationMinutes = durationMinutes;
    }

    // ── IMonogamyCard ─────────────────────────────────────────────────────────

    /// <inheritdoc />
    public MonogamyZone Zone            { get; }

    /// <inheritdoc />
    public CardTarget   Target          { get; }

    /// <inheritdoc />
    public int          TokenValue      { get; }

    /// <inheritdoc />
    public int?         DurationMinutes { get; }

    // ── IPromptCard ───────────────────────────────────────────────────────────

    /// <inheritdoc />
    public string ResolvePrompt(IPlayer player) => _resolver.Resolve(player);

    // ── Factories ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a Monogamy card with separate text for the drawer and their partner.
    /// The resolver picks the text based on the drawing player's gender attribute.
    /// </summary>
    public static MonogamyCard Create(
        string       title,
        string       forHimText,
        string       forHerText,
        string       neutralText,
        MonogamyZone zone,
        CardTarget   target,
        int          tokenValue      = 1,
        int?         durationMinutes = null,
        IEnumerable<string>? tags   = null,
        IRestriction? restriction   = null) =>
        Create(Guid.NewGuid(), title, forHimText, forHerText, neutralText,
               zone, target, tokenValue, durationMinutes, tags, restriction);

    /// <summary>Overload that accepts a stable <see cref="Guid"/> (for JSON loading).</summary>
    public static MonogamyCard Create(
        Guid         id,
        string       title,
        string       forHimText,
        string       forHerText,
        string       neutralText,
        MonogamyZone zone,
        CardTarget   target,
        int          tokenValue      = 1,
        int?         durationMinutes = null,
        IEnumerable<string>? tags   = null,
        IRestriction? restriction   = null)
    {
        var resolver = new GenderDirectedTextResolver(
            defaultText: neutralText,
            variantsByGender: new Dictionary<string, string>
            {
                ["male"]   = forHimText,
                ["female"] = forHerText,
                ["other"]  = neutralText,
            });

        return new MonogamyCard(
            id, title, neutralText, resolver,
            zone, target, tokenValue, durationMinutes, tags,
            restriction ?? new AdultOnlyRestriction());
    }

    /// <summary>
    /// Creates a Monogamy card with a single text shown to all players.
    /// </summary>
    public static MonogamyCard CreateNeutral(
        string       title,
        string       text,
        MonogamyZone zone,
        CardTarget   target,
        int          tokenValue      = 1,
        int?         durationMinutes = null,
        IEnumerable<string>? tags   = null,
        IRestriction? restriction   = null) =>
        Create(Guid.NewGuid(), title, text, text, text, zone, target,
               tokenValue, durationMinutes, tags, restriction);

    /// <summary>Overload that accepts a stable <see cref="Guid"/> (for JSON loading).</summary>
    public static MonogamyCard CreateNeutral(
        Guid         id,
        string       title,
        string       text,
        MonogamyZone zone,
        CardTarget   target,
        int          tokenValue      = 1,
        int?         durationMinutes = null,
        IEnumerable<string>? tags   = null,
        IRestriction? restriction   = null) =>
        Create(id, title, text, text, text, zone, target,
               tokenValue, durationMinutes, tags, restriction);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Difficulty ZoneToDifficulty(MonogamyZone zone) => zone switch
    {
        MonogamyZone.Foreplay => Difficulty.Easy,
        MonogamyZone.Sensual  => Difficulty.Medium,
        MonogamyZone.Steamy   => Difficulty.Hard,
        MonogamyZone.Wild     => Difficulty.Extreme,
        _                     => Difficulty.Easy,
    };
}