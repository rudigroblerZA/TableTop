using TableTop.Core.Abstractions.Game;
using TableTop.Games.Base;

namespace TableTop.Presentation.ViewModels;

/// <summary>
/// Resolves what a game mode should display: the compiled name and description.
///
/// <para>
/// Extracted from MAUI's <c>GameModeItem</c>, whose own doc comment explains
/// why this exists: binding <c>IGameMode</c> straight into a template and
/// reading <c>{Binding Name}</c> can never show a <c>BaseGameModeDefinition</c>
/// subclass's own override, because <c>IGameMode</c> has no such member.
/// </para>
///
/// <para>
/// <b>WinUI's game list had exactly that bug</b> — its XAML bound
/// <c>{Binding Name}</c> directly on the raw <c>IGameMode</c> in
/// <c>GameSelectionViewModel.Modes</c>, so the resolved name never rendered
/// there even though MAUI had already fixed the same problem for itself. The
/// resolution logic now lives here so both heads use it instead of one of them
/// carrying the fix silently.
/// </para>
/// </summary>
public static class ModeDisplayResolver
{
    /// <summary>Resolves a mode's title and description.</summary>
    public static (string Title, string Description) Resolve(IGameMode mode)
    {
        var definition = mode as BaseGameModeDefinition;
        return (
            definition?.Name ?? mode.Name,
            definition?.Description ?? mode.Description);
    }
}

/// <summary>
/// One row in a mode list, resolved via <see cref="ModeDisplayResolver"/>.
/// </summary>
public sealed class ModeListItem
{
    /// <summary>The mode this row represents.</summary>
    public IGameMode Mode { get; }

    /// <summary>Title to show.</summary>
    public string Title { get; }

    /// <summary>Description to show.</summary>
    public string Description { get; }

    /// <summary>Wraps a mode for display.</summary>
    public ModeListItem(IGameMode mode)
    {
        ArgumentNullException.ThrowIfNull(mode);
        Mode = mode;
        (Title, Description) = ModeDisplayResolver.Resolve(mode);
    }
}
