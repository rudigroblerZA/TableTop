using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// A self-contained game mode that knows how to configure and run itself.
/// Register new modes without touching the launcher (OCP).
/// </summary>
public interface IGameMode
{
    /// <summary>Short identifier shown in the selection menu.</summary>
    string Name { get; }

    /// <summary>One-line description shown beneath the mode name.</summary>
    string Description { get; }
}
