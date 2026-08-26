using TableTop.Core.Abstractions.Game;

namespace TableTop.Maui.Theming;

/// <summary>
/// A visual skin for the shared <c>GameplayPage</c>.
///
/// WHY THIS EXISTS RATHER THAN A PAGE PER MODE
/// ───────────────────────────────────────────
/// Millionaire, Monogamy and Day One each have their own page because each
/// drives a *different controller* and genuinely needs different chrome — a
/// money ladder, a dice/zone track, a campaign calendar. Truth or Dare does
/// not: it is an ordinary card-turn mode, so a dedicated page would be a
/// copy of <c>GameplayPage</c>'s ~200 lines of XAML differing only in colour.
/// Every future fix to the gameplay screen would then have to be made twice,
/// and the copy would drift — exactly what happened between the two heads'
/// ViewModels before they were shared into <c>TableTop.Presentation</c>.
///
/// So the page stays single and the *colours* become data. Themes bind
/// through the view model, which means a new skin is one entry in
/// <see cref="For"/> and no XAML at all.
///
/// The palettes here are deliberately the same registers as the static
/// resources in <c>App.xaml</c> (baize card room, wine velvet, TV studio) so
/// the app still reads as one product. <c>App.xaml</c> keeps the static
/// definitions for pages that never change skin; this type covers the one
/// page that does.
/// </summary>
public sealed record ModeTheme
{
    /// <summary>Human-readable skin name. Diagnostics and tests only.</summary>
    public required string Name { get; init; }

    /// <summary>The page's backdrop — the "table" the cards sit on.</summary>
    public required Brush PageBackground { get; init; }

    /// <summary>Headline colour: current player, scores, anything shouting.</summary>
    public required Color Accent { get; init; }

    /// <summary>Quiet colour: captions, counters, round pips.</summary>
    public required Color AccentSoft { get; init; }

    /// <summary>Fill for the raised panels that frame status rows.</summary>
    public required Color PanelBackground { get; init; }

    /// <summary>Hairline around those panels.</summary>
    public required Color PanelBorder { get; init; }

    /// <summary>Face of the playing card, question side.</summary>
    public required Color CardStock { get; init; }

    /// <summary>Face of the playing card once flipped to the answer.</summary>
    public required Color CardStockFlipped { get; init; }

    /// <summary>Ink for the card title.</summary>
    public required Color CardInk { get; init; }

    /// <summary>Ink for the card body — slightly lighter than the title.</summary>
    public required Color CardBodyInk { get; init; }

    /// <summary>Ink for the round pip and other card-corner marks.</summary>
    public required Color CardInkSubtle { get; init; }

    /// <summary>
    /// The inset rule printed just inside the card edge.
    /// A Brush, not a Color: <c>Border.Stroke</c> is typed as Brush, and a
    /// binding that delivers a Color to it does not reliably convert at runtime.
    /// </summary>
    public required Brush CardRule { get; init; }

    /// <summary>Fill of the affirmative action ("Done", "Did It").</summary>
    public required Brush PrimaryButton { get; init; }

    /// <summary>Text on the affirmative action. Must clear 4.5:1 on the fill.</summary>
    public required Color PrimaryButtonText { get; init; }

    /// <summary>Fill of the declining action ("Skip", "Chickened Out").</summary>
    public required Color SecondaryButton { get; init; }

    /// <summary>Text on the declining action.</summary>
    public required Color SecondaryButtonText { get; init; }

    /// <summary>Hairline around the declining action.</summary>
    public required Color SecondaryButtonBorder { get; init; }

    /// <summary>Progress-bar fill.</summary>
    public required Color Progress { get; init; }

    // ── Fonts ─────────────────────────────────────────────────────────────────
    //
    // Nullable, and null means "whatever App.xaml resolves for this platform".
    // The app-level fonts are OnPlatform values — serif on Android, Georgia on
    // iOS, Constantia on WinUI — so a C# theme cannot hardcode a default without
    // imposing one platform's answer on all of them. Resolution therefore lives
    // in the view model, which can look the app resource up.

    /// <summary>Heading and card-title font family, or null for the app default.</summary>
    public string? DisplayFont { get; init; }

    /// <summary>Body font family, or null for the app default.</summary>
    public string? BodyFont { get; init; }

    /// <summary>Counter and label font family, or null for the app default.</summary>
    public string? UtilityFont { get; init; }

    // ── Lookup ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the skin for <paramref name="mode"/>, or <see cref="Baize"/> when
    /// the mode has no dedicated one — which is the great majority of the
    /// catalogue and stays looking exactly as it did.
    ///
    /// Matched on concrete type rather than <c>mode.Name</c>: the display name
    /// is player-facing copy and may be reworded, and a skin silently reverting
    /// to baize because someone retitled a mode is the kind of failure nobody
    /// notices for months.
    /// </summary>
    public static ModeTheme For(IGameMode? mode) => mode switch
    {
        TableTop.Games.TruthOrDareMode => AfterDark,
        _ => Baize,
    };

    // ── Palettes ──────────────────────────────────────────────────────────────

    /// <summary>
    /// The house skin: green baize in a walnut frame, gold and cream card
    /// stock. Mirrors the static resources in <c>App.xaml</c> exactly, so the
    /// ~88 modes that use it are pixel-identical to before this type existed.
    /// </summary>
    public static ModeTheme Baize { get; } = new()
    {
        Name = "Baize",
        PageBackground = Radial("#245546", "#174034", "#0E2A22", centerY: 0.28),
        Accent = Color.FromArgb("#E3C67F"),
        AccentSoft = Color.FromArgb("#8A7A55"),
        PanelBackground = Color.FromArgb("#16382E"),
        PanelBorder = Color.FromArgb("#4DC49E4C"),
        CardStock = Color.FromArgb("#FAF8F2"),
        CardStockFlipped = Color.FromArgb("#FFF4DD"),
        CardInk = Color.FromArgb("#1F2126"),
        CardBodyInk = Color.FromArgb("#3B3A38"),
        CardInkSubtle = Color.FromArgb("#7C7466"),
        CardRule = new SolidColorBrush(Color.FromArgb("#8CC49E4C")),
        PrimaryButton = Vertical("#E3C67F", "#C49E4C"),
        PrimaryButtonText = Color.FromArgb("#22160A"),
        SecondaryButton = Color.FromArgb("#4D000000"),
        SecondaryButtonText = Color.FromArgb("#E3C67F"),
        SecondaryButtonBorder = Color.FromArgb("#73C49E4C"),
        Progress = Color.FromArgb("#C49E4C"),
    };

    /// <summary>
    /// Truth or Dare — after dark.
    ///
    /// The other three registers are all *places*: a card room, a bedroom, a
    /// TV studio. This one is a time of night. Truth or Dare is played in a
    /// circle, late, with the lights down and one bright phone in the middle
    /// of it, so the page is an indigo room with a pool of light where the
    /// card sits, and the card stock is the brightest thing on screen.
    ///
    /// The two accents carry the game's actual mechanic. Every card holds a
    /// truth AND a dare and you commit blind, so the palette runs cool-to-hot:
    /// cyan for the confessional half, magenta for the reckless one. The
    /// affirmative button is magenta — in this game "Did It" is the brave
    /// answer, and it should look like the fun one.
    ///
    /// CONTRAST. Every foreground/background pair here clears WCAG AA for the
    /// size it is actually rendered at (measured, not eyeballed):
    ///
    ///   card title on stock            16.9:1
    ///   card body on stock             12.3:1
    ///   card ink on flipped stock      17.1:1
    ///   round pip on stock              5.7:1   (11px — needs the full 4.5)
    ///   "Did It" on the button           5.0:1   at the DARKEST end of the
    ///                                            gradient; 9.3:1 at the top
    ///   player name on panel            6.9:1
    ///   caption on panel                5.5:1
    ///   "Chickened Out" on its fill    12.9:1
    ///
    /// The pip and the button label both failed on the first pass (4.1 and
    /// 4.2). This palette is darker than baize overall, and a hot magenta is
    /// a much lighter fill than brass, so neither could be carried over by eye
    /// from the existing theme — both needed measuring.
    /// </summary>
    public static ModeTheme AfterDark { get; } = new()
    {
        Name = "After Dark",
        PageBackground = Radial("#2B1B4A", "#1A1030", "#0B0718", centerY: 0.30),
        Accent = Color.FromArgb("#FF7AB0"),
        AccentSoft = Color.FromArgb("#9A8CC4"),
        PanelBackground = Color.FromArgb("#241640"),
        PanelBorder = Color.FromArgb("#59FF4D8D"),

        // Cooler and brighter than baize cream. Against an indigo room this
        // reads as lit paper rather than the warm ivory of a card table.
        CardStock = Color.FromArgb("#F7F4FF"),
        CardStockFlipped = Color.FromArgb("#E8FBF8"),   // cyan-tinted answer face
        CardInk = Color.FromArgb("#17102B"),
        CardBodyInk = Color.FromArgb("#332A4A"),
        CardInkSubtle = Color.FromArgb("#655A88"),
        CardRule = new SolidColorBrush(Color.FromArgb("#8CFF4D8D")),

        PrimaryButton = Vertical("#FF8FBC", "#E63D80"),
        PrimaryButtonText = Color.FromArgb("#1A0410"),

        // "Chickened Out" is a legitimate move in this game, not a failure —
        // it has its own forfeit and its own comedy. Cyan-outlined and quiet,
        // readable but clearly the cooler of the two choices.
        SecondaryButton = Color.FromArgb("#59000000"),
        SecondaryButtonText = Color.FromArgb("#5FE8DC"),
        SecondaryButtonBorder = Color.FromArgb("#7335E0D2"),

        Progress = Color.FromArgb("#FF4D8D"),
    };

    // ── Brush helpers ─────────────────────────────────────────────────────────

    private static RadialGradientBrush Radial(string lit, string mid, string deep, double centerY)
    {
        var brush = new RadialGradientBrush
        {
            Center = new Point(0.5, centerY),
            Radius = 0.95,
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(lit), 0.0f));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(mid), 0.55f));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(deep), 1.0f));
        return brush;
    }

    private static LinearGradientBrush Vertical(string top, string bottom)
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1),
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(top), 0.0f));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(bottom), 1.0f));
        return brush;
    }
}
