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
/// shared the fix.
/// </summary>
public sealed class GameModeItem
{
    /// <summary>The mode this row represents.</summary>
    public IGameMode Mode { get; }

    /// <summary>Title to show: JSON override if the deck sets one, else the compiled name.</summary>
    public string Title { get; }

    /// <summary>Description to show, JSON override applied.</summary>
    public string Description { get; }

    /// <summary>Wraps a mode for display.</summary>
    public GameModeItem(IGameMode mode)
    {
        ArgumentNullException.ThrowIfNull(mode);
        Mode = mode;

        (Title, Description) = ModeDisplayResolver.Resolve(mode);
    }
}
