using TableTop.Hosting;

namespace TableTop.Maui.ViewModels;

/// <summary>What a flyout row does when it is tapped.</summary>
public enum FlyoutDestination
{
    /// <summary>The landing screen — Play, Settings, Roaster, and Continue.</summary>
    Home,

    /// <summary>
    /// A top-level archetype. <see cref="FlyoutMenuItem.Archetype"/> carries
    /// which one, and is non-null for exactly this destination.
    /// </summary>
    Archetype,

    /// <summary>The settings screen.</summary>
    Settings,

    /// <summary>The roster builder.</summary>
    Roaster,
}

/// <summary>
/// One row in the flyout drawer.
///
/// <para>
/// A plain immutable row rather than a <c>BindableObject</c>: nothing about a
/// menu entry changes once the menu is built. The list itself is rebuilt from
/// scratch when the age-rating setting changes, which is the only thing that
/// can alter it — see <see cref="FlyoutMenuViewModel"/>.
/// </para>
///
/// <para>
/// <b>Bindable by name.</b> <see cref="Glyph"/>, <see cref="Title"/> and
/// <see cref="Detail"/> are instance properties because a <c>{Binding}</c> in
/// the row's <c>DataTemplate</c> resolves against the instance — a static one
/// would render empty, which is the failure
/// <c>scripts/check-xaml-bindings.py</c> exists to catch.
/// </para>
/// </summary>
public sealed class FlyoutMenuItem
{
    /// <summary>Creates a row.</summary>
    public FlyoutMenuItem(FlyoutDestination destination, string glyph, string title,
                          string detail, Archetype? archetype = null)
    {
        Destination = destination;
        Glyph = glyph;
        Title = title;
        Detail = detail;
        Archetype = archetype;
    }

    /// <summary>Where tapping this row goes.</summary>
    public FlyoutDestination Destination { get; }

    /// <summary>Emoji shown at the leading edge of the row.</summary>
    public string Glyph { get; }

    /// <summary>The row's label.</summary>
    public string Title { get; }

    /// <summary>One line under the label. Empty renders as a blank line, so it is never null.</summary>
    public string Detail { get; }

    /// <summary>
    /// The archetype this row opens, or null for every non-archetype row.
    /// Non-null exactly when <see cref="Destination"/> is
    /// <see cref="FlyoutDestination.Archetype"/>.
    /// </summary>
    public Archetype? Archetype { get; }
}
