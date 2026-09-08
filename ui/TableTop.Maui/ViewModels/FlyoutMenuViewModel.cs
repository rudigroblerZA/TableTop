using System.Collections.ObjectModel;
using TableTop.Core.Abstractions.Game;
using TableTop.Hosting;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Maui.ViewModels;

/// <summary>
/// Builds the flyout drawer's rows: Home, one row per top-level archetype, then
/// Settings and Roaster.
///
/// <para>
/// <b>The archetype rows are the point of the flyout.</b> Before this, the only
/// way to reach a category was the horizontal strip at the top of
/// <c>GameSelectionPage</c> — three cards competing for width on a phone, above
/// two more pickers. Promoting them to the drawer gives each one a full-width
/// row with room for its description, and leaves the detail pane to the game
/// list alone.
/// </para>
///
/// <para>
/// <b>Age filtering is applied here, not in the view.</b> The same
/// <see cref="ArchetypeFilter"/> the game picker uses, read from the same
/// setting, so the drawer cannot offer a category the picker would then refuse
/// to show. <c>MinAgeRating</c> is genuinely a floor in this codebase — the
/// Settings page's own copy says "Hides games BELOW the selected rating" — and
/// this honours that rather than second-guessing it.
/// </para>
/// </summary>
public sealed class FlyoutMenuViewModel : BindableObject
{
    private readonly IAppSettings _settings;

    /// <summary>The drawer's rows, in display order.</summary>
    public ObservableCollection<FlyoutMenuItem> Items { get; } = [];

    /// <summary>Creates the menu and fills it for the current age setting.</summary>
    public FlyoutMenuViewModel(IAppSettings settings)
    {
        _settings = settings;
        Rebuild();

        // The drawer is built once and lives for the life of the app, so it
        // would otherwise keep showing categories the player has just filtered
        // out until a restart.
        _settings.Changed += OnSettingsChanged;
    }

    private void OnSettingsChanged(object? sender, string key)
    {
        if (key != nameof(IAppSettings.MinAgeRating) && key != "*") return;
        Rebuild();
    }

    private void Rebuild()
    {
        Items.Clear();

        Items.Add(new FlyoutMenuItem(
            FlyoutDestination.Home, "🃏", "Home", "Start a game, or pick up where you left off"));

        foreach (var archetype in FilteredArchetypes())
            Items.Add(new FlyoutMenuItem(
                FlyoutDestination.Archetype, archetype.Emoji, archetype.Name,
                archetype.Description, archetype));

        Items.Add(new FlyoutMenuItem(
            FlyoutDestination.Roaster, "👥", "Roaster", "Build and save the tables you play with"));
        Items.Add(new FlyoutMenuItem(
            FlyoutDestination.Settings, "⚙", "Settings", "Theme, age rating, and gameplay options"));
    }

    private IReadOnlyList<Archetype> FilteredArchetypes()
    {
        var floor = (AgeRating)_settings.MinAgeRating;
        return new ArchetypeFilter(minAgeRating: floor, maxAgeRating: AgeRating.Adult)
            .Apply(ArchetypeRegistry.Default().RootArchetypes);
    }
}
