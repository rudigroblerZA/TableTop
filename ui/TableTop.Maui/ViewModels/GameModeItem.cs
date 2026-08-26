using TableTop.Core.Abstractions.Game;
using TableTop.Presentation.ViewModels;

namespace TableTop.Maui.ViewModels;

/// <summary>
/// One row in the game list.
///
/// WHY THE LIST NO LONGER BINDS DOMAIN OBJECTS DIRECTLY
/// ───────────────────────────────────────────────────
/// It used to bind <c>IGameMode</c> straight into the template and read
/// <c>{Binding Name}</c>. That could never show a JSON title override, because
/// the override lives on <c>BaseGameModeDefinition.DisplayName</c> and
/// <c>IGameMode</c> has no such member.
///
/// The fallback-chain resolution now lives in the shared
/// <see cref="ModeDisplayResolver"/> rather than here — WinUI's list had the
/// exact same bug this class was written to fix, undetected because nothing
/// shared the fix. This class now wraps the shared resolution and adds only
/// what is genuinely MAUI-specific: parsing the accent hex into a real
/// <see cref="Color"/>, since <c>TableTop.Presentation</c> cannot reference
/// <c>Microsoft.Maui.Graphics</c>.
/// </summary>
public sealed class GameModeItem
{
    /// <summary>The mode this row represents.</summary>
    public IGameMode Mode { get; }

    /// <summary>Title to show: JSON override if the deck sets one, else the compiled name.</summary>
    public string Title { get; }

    /// <summary>Description to show, JSON override applied.</summary>
    public string Description { get; }

    /// <summary>
    /// Accent colour for the row's leading stripe, taken from the deck's theme
    /// when it declares one. Null when it doesn't, and the template falls back to
    /// the app accent — a mode without a palette should look deliberate, not
    /// broken.
    /// </summary>
    public string? Accent { get; }

    /// <summary>
    /// The same accent as a parsed <see cref="Color"/>.
    ///
    /// Parsed here rather than left to the binding: MAUI's string-to-Color
    /// conversion inside a <c>{Binding}</c> is not reliable the way it is for a
    /// literal attribute value, and a malformed hex in a deck should cost the
    /// stripe, not throw during layout.
    /// </summary>
    public Color? AccentColor { get; }

    /// <summary>True when the deck declares a usable palette, so the stripe means something.</summary>
    public bool HasAccent => AccentColor is not null;

    /// <summary>Wraps a mode for display.</summary>
    public GameModeItem(IGameMode mode)
    {
        ArgumentNullException.ThrowIfNull(mode);
        Mode = mode;

        (Title, Description, Accent) = ModeDisplayResolver.Resolve(mode);
        AccentColor = Parse(Accent);
    }

    private static Color? Parse(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return null;
        try { return Color.FromArgb(hex); }
        catch { return null; }
    }
}
