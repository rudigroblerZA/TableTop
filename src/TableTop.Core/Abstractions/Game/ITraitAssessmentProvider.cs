using TableTop.Core.Abstractions.Analysis;
using TableTop.Core.Domain.Analysis;

namespace TableTop.Core.Abstractions.Game;

/// <summary>
/// Implemented by game modes that profile players across trait dimensions
/// rather than scoring them toward a win.
///
/// <para>
/// Follows the same rule as every other capability interface here: the mode
/// supplies its own content, so <c>Hosting</c> never reaches into a specific
/// static card-bank class and stays ignorant of game content (DIP, OCP).
/// Implementing this is what makes a mode resolve to
/// <see cref="ControllerFamily.TraitProfile"/>.
/// </para>
///
/// <para>
/// <b>Adding this interface meant two edits, not one.</b>
/// <c>ControllerFamilies.TryFor</c> and <c>ControllerFactory.CreateAsync</c>
/// both switch on the capability set, in the same order, and
/// <c>ModeManifestExtensions</c> derives from the first of those. That is the
/// standing hazard recorded in CLAUDE.md and in <c>ControllerFamily</c>'s own
/// docs — the ordering was wrong once for real, with Monogamy and Quiz
/// transposed. This interface is unambiguous against the existing set (no mode
/// implements two), but the arms were still added in matching positions.
/// </para>
/// </summary>
public interface ITraitAssessmentProvider
{
    /// <summary>
    /// The instrument this mode reports on — the dimensions and their labels.
    /// </summary>
    TraitScale GetTraitScale();

    /// <summary>
    /// The item bank, in the order the mode wants it presented. The controller
    /// is responsible for any shuffling or truncation.
    /// </summary>
    IReadOnlyList<TraitItemCard> GetItemBank();
}
