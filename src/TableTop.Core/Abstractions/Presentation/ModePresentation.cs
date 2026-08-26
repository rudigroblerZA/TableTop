namespace TableTop.Core.Abstractions.Presentation;

/// <summary>
/// Colours and fonts for one mode, sourced from its deck JSON.
///
/// Every member is optional. A renderer takes what is present and keeps its own
/// value for the rest, so a deck can restyle one thing — a background, an accent
/// — without restating a whole palette and without a deck author needing to know
/// what the other fourteen slots even are.
///
/// Colours are hex strings exactly as the existing <c>CategoryColours</c> hint
/// already uses ("#42A5F5"), because that convention is established and renderers
/// already parse it. Validation is the renderer's job: this layer carries the
/// value, it does not construct platform colour objects, so Core stays free of
/// any UI framework.
/// </summary>
public sealed record ThemePalette
{
    /// <summary>Human label for the palette, e.g. "After Dark". Diagnostic only.</summary>
    public string? Name { get; init; }

    /// <summary>Page background. May be a single hex colour or a gradient (see <see cref="BackgroundGradient"/>).</summary>
    public string? Background { get; init; }

    /// <summary>
    /// Optional gradient stops for the background, darkest-to-lightest as the
    /// renderer sees fit. When present, renderers that support gradients should
    /// prefer this and fall back to <see cref="Background"/> when they don't.
    /// </summary>
    public IReadOnlyList<string>? BackgroundGradient { get; init; }

    /// <summary>Primary accent colour.</summary>
    public string? Accent { get; init; }

    /// <summary>Muted variant of the accent, for secondary text and rules.</summary>
    public string? AccentSoft { get; init; }

    /// <summary>Background of panels sitting on the page.</summary>
    public string? PanelBackground { get; init; }

    /// <summary>Panel border colour.</summary>
    public string? PanelBorder { get; init; }

    /// <summary>Face colour of an unflipped card.</summary>
    public string? CardStock { get; init; }

    /// <summary>Face colour of a flipped card.</summary>
    public string? CardStockFlipped { get; init; }

    /// <summary>Card heading text colour.</summary>
    public string? CardInk { get; init; }

    /// <summary>Card body text colour.</summary>
    public string? CardBodyInk { get; init; }

    /// <summary>Muted card text — captions, pips, counters.</summary>
    public string? CardInkSubtle { get; init; }

    /// <summary>Primary action button fill.</summary>
    public string? PrimaryButton { get; init; }

    /// <summary>Text on the primary action button.</summary>
    public string? PrimaryButtonText { get; init; }

    /// <summary>Secondary action button fill.</summary>
    public string? SecondaryButton { get; init; }

    /// <summary>Text on the secondary action button.</summary>
    public string? SecondaryButtonText { get; init; }

    /// <summary>Progress indicator colour.</summary>
    public string? Progress { get; init; }

    /// <summary>Font family for headings and card titles.</summary>
    public string? DisplayFont { get; init; }

    /// <summary>Font family for body text.</summary>
    public string? BodyFont { get; init; }

    /// <summary>Font family for counters, labels and other utility text.</summary>
    public string? UtilityFont { get; init; }

    /// <summary>True when the palette carries nothing at all.</summary>
    public bool IsEmpty =>
        Name is null && Background is null && BackgroundGradient is null && Accent is null
        && AccentSoft is null && PanelBackground is null && PanelBorder is null
        && CardStock is null && CardStockFlipped is null && CardInk is null
        && CardBodyInk is null && CardInkSubtle is null && PrimaryButton is null
        && PrimaryButtonText is null && SecondaryButton is null && SecondaryButtonText is null
        && Progress is null && DisplayFont is null && BodyFont is null && UtilityFont is null;
}

/// <summary>
/// Everything about a mode that is presentation or tuning rather than rules:
/// what it's called, what its buttons say, how many can play, and how it looks.
///
/// WHY THIS EXISTS
/// ───────────────
/// All of it previously lived in C# — <c>Name</c> and <c>Description</c> as
/// abstract properties, <c>CompleteLabel</c>, <c>SkipLabel</c>,
/// <c>CategoryColours</c>, <c>MinimumPlayers</c> and the pinned-category lists as
/// virtuals, and the colour palettes hardcoded per concrete type in the MAUI
/// theme. Changing a title, retuning a button label or restyling a mode meant a
/// recompile and a store release, which is the wrong shape for content.
///
/// Every member is nullable and means "not specified". The mode's existing C#
/// value stands wherever JSON is silent, so all 92 decks — none of which carry
/// any of this yet — behave exactly as before.
/// </summary>
public sealed record ModePresentation
{
    /// <summary>Overrides the mode's displayed name.</summary>
    public string? Title { get; init; }

    /// <summary>Overrides the mode's one-line description.</summary>
    public string? Description { get; init; }

    /// <summary>Primary action button text, e.g. "Reveal", "Done".</summary>
    public string? CompleteLabel { get; init; }

    /// <summary>Secondary action button text, e.g. "Pass".</summary>
    public string? SkipLabel { get; init; }

    /// <summary>Smallest workable table size.</summary>
    public int? MinimumPlayers { get; init; }

    /// <summary>Categories dealt first regardless of shuffle, e.g. a consent ritual.</summary>
    public IReadOnlyList<string>? CategoriesPinnedToStart { get; init; }

    /// <summary>Categories dealt last, e.g. aftercare or a results key.</summary>
    public IReadOnlyList<string>? CategoriesPinnedToEnd { get; init; }

    /// <summary>Per-category colour hints, keyed by category name.</summary>
    public IReadOnlyDictionary<string, string>? CategoryColours { get; init; }

    /// <summary>Colours and fonts.</summary>
    public ThemePalette? Theme { get; init; }

    /// <summary>An entirely unspecified presentation — the default for every existing deck.</summary>
    public static ModePresentation None { get; } = new();

    /// <summary>True when nothing at all is specified.</summary>
    public bool IsEmpty =>
        Title is null && Description is null && CompleteLabel is null && SkipLabel is null
        && MinimumPlayers is null && CategoriesPinnedToStart is null
        && CategoriesPinnedToEnd is null && CategoryColours is null
        && (Theme is null || Theme.IsEmpty);
}
