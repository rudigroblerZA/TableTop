using TableTop.Core.Abstractions.Progression;

namespace TableTop.Games;

/// <summary>
/// Settings chosen by the host before a game session begins.
/// </summary>
public sealed record GameOptions(
    IProgressionStrategy Progression,
    int MaxRounds,
    bool ShowScoreAfterEachTurn);
