using Android.Content;
using Android.Views;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;
using TableTop.Core.Abstractions.Game;
using TableTop.Droid.Infrastructure;
using TableTop.Hosting;
using TableTop.Presentation.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Droid.Screens;

/// <summary>
/// The landing screen: pick an archetype, then a sub-archetype, then a mode.
/// Applies the age-rating floor from <see cref="IAppSettings.MinAgeRating"/> the
/// same way MAUI's <c>GameSelectionViewModel</c> does, and offers a saved
/// session when one is resumable.
/// </summary>
public sealed class ModePickerScreen : Screen
{
    private readonly SavedSessionLookup _savedSession =
        new(MainApplication.Services.GetRequiredService<TableTop.Hosting.Abstractions.IControllerFactory>());

    private IReadOnlyList<Archetype> _archetypes = [];
    private Spinner _rootSpinner = null!;
    private Spinner _subSpinner = null!;
    private LinearLayout _modeList = null!;
    private Button _resumeButton = null!;

    /// <inheritdoc />
    public override string Title => "Choose a game";

    /// <inheritdoc />
    protected override View OnCreateView(Context context)
    {
        var registry = MainApplication.Services.GetRequiredService<IArchetypeRegistry>();
        var settings = MainApplication.Services.GetRequiredService<IAppSettings>();

        _archetypes = new ArchetypeFilter(minAgeRating: (AgeRating)settings.MinAgeRating)
            .Apply(registry.RootArchetypes);

        var column = Ui.Column(context);
        column.AddView(Ui.Heading(context, "TableTop"));

        _resumeButton = Ui.Button(context, "Continue saved game");
        _resumeButton.Visibility = ViewStates.Gone;
        _resumeButton.OnClick(ResumeAsync);
        column.AddView(_resumeButton);

        column.AddView(Ui.Button(context, "Settings").OnClick(() => Host.Push(new SettingsScreen())));

        column.AddView(Ui.Label(context, "Category", 14f));
        _rootSpinner = Ui.Dropdown(context, _archetypes.Select(a => $"{a.Emoji} {a.Name}").ToList());
        _rootSpinner.ItemSelected += (_, _) => RebuildSubSpinner(context);
        column.AddView(_rootSpinner);

        column.AddView(Ui.Label(context, "Sub-category", 14f));
        _subSpinner = Ui.Dropdown(context, new List<string> { "" });
        _subSpinner.ItemSelected += (_, _) => RebuildModeList(context);
        column.AddView(_subSpinner);

        _modeList = new LinearLayout(context) { Orientation = Orientation.Vertical };
        column.AddView(_modeList);

        if (_archetypes.Count > 0) RebuildSubSpinner(context);

        _ = RefreshSavedSessionAsync();

        return Ui.Scroll(context, column);
    }

    private Archetype? SelectedRoot =>
        _rootSpinner.SelectedItemPosition >= 0 && _rootSpinner.SelectedItemPosition < _archetypes.Count
            ? _archetypes[_rootSpinner.SelectedItemPosition]
            : null;

    private void RebuildSubSpinner(Context context)
    {
        var root = SelectedRoot;
        var subs = root?.SubArchetypes ?? [];

        var labels = subs.Count > 0
            ? subs.Select(s => s.Name).ToList()
            : new List<string> { "All modes" };
        var adapter = new ArrayAdapter<string>(
            context, Android.Resource.Layout.SimpleSpinnerItem, labels);
        adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
        _subSpinner.Adapter = adapter;
        _subSpinner.Visibility = subs.Count > 0 ? ViewStates.Visible : ViewStates.Gone;

        RebuildModeList(context);
    }

    private void RebuildModeList(Context context)
    {
        _modeList.RemoveAllViews();

        var root = SelectedRoot;
        if (root is null) return;

        var subs = root.SubArchetypes;
        var modes = subs.Count > 0
            ? (_subSpinner.SelectedItemPosition >= 0 && _subSpinner.SelectedItemPosition < subs.Count
                ? subs[_subSpinner.SelectedItemPosition].Modes
                : [])
            : root.Modes;

        if (modes.Count == 0)
        {
            _modeList.AddView(Ui.Label(context, "No modes here."));
            return;
        }

        foreach (var mode in modes)
        {
            var item = new ModeListItem(mode);
            var button = Ui.Button(context, item.Title);
            button.OnClick(() => Host.Push(new PlayerSetupScreen(mode)));
            _modeList.AddView(button);
        }
    }

    private async Task RefreshSavedSessionAsync()
    {
        await _savedSession.RefreshAsync();
        Host.RunOnUiThread(() =>
        {
            _resumeButton.Text = _savedSession.CanResume ? _savedSession.ResumeText : "Continue saved game";
            _resumeButton.Visibility = _savedSession.CanResume ? ViewStates.Visible : ViewStates.Gone;
        });
    }

    // async void: an unguarded exception here terminates the process on Android,
    // so the whole body is wrapped and a failure lands on a MessageScreen instead.
    private async void ResumeAsync()
    {
        try
        {
            if (_savedSession.Resumable is not { } r) return;
            var settings = MainApplication.Services.GetRequiredService<IAppSettings>();
            var screen = await GameScreenFactory.CreateAsync(
                new StackNavigator(Host), r.Mode, r.Players, settings, resumeFrom: r.Snapshot);
            Host.Push(screen);
        }
        catch (Exception ex)
        {
            Host.Push(new MessageScreen("Couldn't resume", ex.Message));
        }
    }
}
