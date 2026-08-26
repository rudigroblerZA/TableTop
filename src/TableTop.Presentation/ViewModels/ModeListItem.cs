using TableTop.Core.Abstractions.Game;
using TableTop.Games.Base;

namespace TableTop.Presentation.ViewModels;

/// <summary>
/// Resolves what a game mode should display: JSON title/description override
/// if the deck sets one, else the compiled name.
///
/// <para>
/// Extracted from MAUI's <c>GameModeItem</c>, whose own doc comment explains
/// why this exists: binding <c>IGameMode</c> straight into a template and
/// reading <c>{Binding Name}</c> can never show a JSON override, because the
/// override lives on <c>BaseGameModeDefinition.DisplayName</c> and
/// <c>IGameMode</c> has no such member.
/// </para>
///
/// <para>
/// <b>WinUI's game list had exactly that bug</b> — its XAML bound
/// <c>{Binding Name}</c> directly on the raw <c>IGameMode</c> in
/// <c>GameSelectionViewModel.Modes</c>, so a deck's JSON title never rendered
/// there even though MAUI had already fixed the same problem for itself. The
/// resolution logic now lives here so both heads use it instead of one of them
/// carrying the fix silently.
/// </para>
/// </summary>
public static class ModeDisplayResolver
{
    /// <summary>Resolves a mode's title, description and accent hex, applying JSON overrides.</summary>
    public static (string Title, string Description, string? Accent) Resolve(IGameMode mode)
    {
        var definition = mode as BaseGameModeDefinition;
        return (
            definition?.DisplayName        ?? mode.Name,
            definition?.DisplayDescription ?? mode.Description,
            definition?.Theme?.Accent);
    }
}

/// <summary>
/// One row in a mode list, resolved via <see cref="ModeDisplayResolver"/>.
///
/// Deliberately holds the accent as a hex string rather than a parsed colour:
/// MAUI needs <c>Microsoft.Maui.Graphics.Color</c> and WinUI needs
/// <c>Windows.UI.Color</c>/<c>Brush</c>, and this project cannot reference
/// either. A head that wants the parsed colour wraps this row itself — WinUI's
/// current list has no per-row accent stripe, so it binds <see cref="Title"/>
/// and <see cref="Description"/> directly with no wrapping needed.
/// </summary>
public sealed class ModeListItem
{
    /// <summary>The mode this row represents.</summary>
    public IGameMode Mode { get; }

    /// <summary>Title to show: JSON override if the deck sets one, else the compiled name.</summary>
    public string Title { get; }

    /// <summary>Description to show, JSON override applied.</summary>
    public string Description { get; }

    /// <summary>Accent hex from the deck's theme, or null when it declares none.</summary>
    public string? Accent { get; }

    /// <summary>True when the deck declares a usable accent.</summary>
    public bool HasAccent => !string.IsNullOrWhiteSpace(Accent);

    /// <summary>Wraps a mode for display.</summary>
    public ModeListItem(IGameMode mode)
    {
        ArgumentNullException.ThrowIfNull(mode);
        Mode = mode;
        (Title, Description, Accent) = ModeDisplayResolver.Resolve(mode);
    }
}
