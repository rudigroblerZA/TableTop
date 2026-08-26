using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Restrictions;

namespace TableTop.Core.Domain.Cards;

/// <summary>
/// A card whose displayed prompt is resolved at draw time via an <see cref="ICardTextResolver"/>.
/// Drop-in replacement for <see cref="StandardCard"/> anywhere a gender-directed or
/// attribute-directed prompt is needed — consumers that only know <see cref="ICard"/>
/// see the base description; consumers that detect <see cref="IPromptCard"/> get the
/// resolved, player-specific text (LSP).
/// </summary>
public sealed class PromptCard : BaseCard, IPromptCard
{
    private readonly ICardTextResolver _resolver;

    /// <param name="id">Unique card identifier.</param>
    /// <param name="title">Display title (gender-neutral).</param>
    /// <param name="baseDescription">
    /// Fallback description shown to consumers unaware of <see cref="IPromptCard"/>.
    /// Should be a gender-neutral summary of what the card does.
    /// </param>
    /// <param name="resolver">
    /// Strategy that produces the player-specific prompt text at draw time.
    /// </param>
    /// <param name="difficulty">Card difficulty tier.</param>
    /// <param name="category">Thematic category (e.g. "Prompt", "Dare").</param>
    /// <param name="tags">Optional tags for filtering.</param>
    /// <param name="restriction">Optional eligibility restriction.</param>
    public PromptCard(
        Guid id,
        string title,
        string baseDescription,
        ICardTextResolver resolver,
        Difficulty difficulty,
        string category,
        IEnumerable<string>? tags = null,
        IRestriction? restriction = null)
        : base(id, title, baseDescription, difficulty, category, tags, restriction)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    /// <inheritdoc />
    public string ResolvePrompt(IPlayer player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return _resolver.Resolve(player);
    }

    // ── Convenience factories ─────────────────────────────────────────────────

    /// <summary>Creates a gender-directed prompt card with separate male/female/other texts.</summary>
    /// <param name="title">Short display title for the card.</param>
    /// <param name="maleText">Prompt text shown to players whose gender attribute is male.</param>
    /// <param name="femaleText">Prompt text shown to players whose gender attribute is female.</param>
    /// <param name="otherText">Prompt text for everyone else; also the card's base description.</param>
    /// <param name="difficulty">Difficulty band used by progression and filtering.</param>
    /// <param name="category">Category name used by scoring, pinning, and filtering.</param>
    /// <param name="tags">Optional free-form tags.</param>
    /// <param name="restriction">Optional restriction gating which players may draw the card.</param>
    /// <param name="id">
    /// Stable identifier. Omit (or pass null) to generate a fresh one — correct for
    /// cards declared in C# card banks, which are rebuilt identically on every launch.
    /// Loaders that have an authored id MUST pass it: a card whose id changes between
    /// runs defeats the played-card tracking that persistence relies on, so an
    /// already-seen prompt card reappears after resume.
    /// </param>
    public static PromptCard CreateGenderDirected(
        string title,
        string maleText,
        string femaleText,
        string otherText,
        Difficulty difficulty,
        string category,
        IEnumerable<string>? tags = null,
        IRestriction? restriction = null,
        Guid? id = null)
    {
        var resolver = new GenderDirectedTextResolver(
            defaultText: otherText,
            variantsByGender: new Dictionary<string, string>
            {
                ["male"] = maleText,
                ["female"] = femaleText,
                ["other"] = otherText,
            });

        // Base description is the "other" text so plain ICard consumers still get useful text.
        return new PromptCard(
            id ?? Guid.NewGuid(), title, otherText, resolver,
            difficulty, category, tags, restriction);
    }

    /// <summary>Creates a prompt card with a single text resolved by an arbitrary attribute.</summary>
    /// <param name="title">Short display title for the card.</param>
    /// <param name="attributeKey">Player attribute key whose value selects a variant.</param>
    /// <param name="defaultText">Prompt text used when no variant matches; also the card's base description.</param>
    /// <param name="variants">Prompt text per attribute value.</param>
    /// <param name="difficulty">Difficulty band used by progression and filtering.</param>
    /// <param name="category">Category name used by scoring, pinning, and filtering.</param>
    /// <param name="tags">Optional free-form tags.</param>
    /// <param name="restriction">Optional restriction gating which players may draw the card.</param>
    /// <param name="id">Stable identifier; omit to generate one. See <see cref="CreateGenderDirected"/>.</param>
    public static PromptCard CreateAttributeDirected(
        string title,
        string attributeKey,
        string defaultText,
        IDictionary<string, string> variants,
        Difficulty difficulty,
        string category,
        IEnumerable<string>? tags = null,
        IRestriction? restriction = null,
        Guid? id = null)
    {
        var resolver = new AttributeDirectedTextResolver(attributeKey, defaultText, variants);
        return new PromptCard(
            id ?? Guid.NewGuid(), title, defaultText, resolver,
            difficulty, category, tags, restriction);
    }
}
