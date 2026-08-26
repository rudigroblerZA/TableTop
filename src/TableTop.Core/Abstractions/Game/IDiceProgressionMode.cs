namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Marker interface implemented by game modes that want dice-driven category
/// selection instead of the default <c>DifficultyProgressionStrategy</c>.
///
/// Each turn, two dice are rolled and the total picks which category the next
/// card comes from — doubles offer the drawing player a free choice. See
/// <c>DiceCategoryProgressionStrategy</c> for the mechanic itself.
///
/// Implementing this interface requires no factory changes beyond the one
/// dispatch arm already added for it — the controller factory recognises it
/// automatically, the same way <see cref="IFlowAwareMode"/> does.
/// </summary>
public interface IDiceProgressionMode
{
    /// <summary>
    /// Every category this mode uses, in "distance" order — adjacent entries
    /// are what a rolled-but-empty category falls back to first, and doubles
    /// offer a choice among these.
    /// </summary>
    IReadOnlyList<string> CategoriesInOrder { get; }

    /// <summary>Maps a dice total (2–12) to one of <see cref="CategoriesInOrder"/>.</summary>
    string CategoryForTotal(int diceTotal);
}
