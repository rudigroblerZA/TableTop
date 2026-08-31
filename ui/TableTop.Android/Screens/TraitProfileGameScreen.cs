using Android.Content;
using Android.Views;
using Android.Widget;
using TableTop.Droid.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Droid.Screens;

/// <summary>
/// Trait-assessment gameplay (Big Five, Love Languages) — drives the shared
/// <see cref="TraitProfileGameViewModel"/>.
///
/// <para>
/// Structurally the closest sibling to <see cref="HerdGameScreen"/>: everyone
/// answers one prompt at once. The two differences are the response row — five
/// fixed buttons rather than a text field — and the ending, which renders a
/// profile per player instead of a scoreboard.
/// </para>
/// </summary>
public sealed class TraitProfileGameScreen(TraitProfileGameViewModel vm)
    : GameScreenBase<TraitProfileGameViewModel>(vm)
{
    private TextView _progress = null!, _statement = null!, _category = null!, _scale = null!;
    private TextView _summary = null!, _comparison = null!, _caveat = null!;
    private LinearLayout _responses = null!, _results = null!;
    private Button _next = null!, _skip = null!, _back = null!;

    /// <inheritdoc />
    public override string Title => "Profile";

    /// <inheritdoc />
    protected override View Build(Context context)
    {
        var column = Ui.Column(context);

        _progress = Ui.Label(context, "", 13f);
        _statement = Ui.Heading(context, "");
        _category = Ui.Label(context, "", 12f);
        _scale = Ui.Label(context, "1 strongly disagree  ·  5 strongly agree", 12f);
        _responses = new LinearLayout(context) { Orientation = Orientation.Vertical };
        _next = Ui.Button(context, "Next").OnClick(Vm.SubmitCommand);
        _skip = Ui.Button(context, "Skip").OnClick(Vm.SkipCommand);

        _summary = Ui.Label(context, "", 16f);
        _results = new LinearLayout(context) { Orientation = Orientation.Vertical };
        _comparison = Ui.Label(context, "", 14f);
        _caveat = Ui.Label(context, Vm.ResultsCaveat, 12f);
        _back = Ui.Button(context, "Back").OnClick(Vm.BackCommand);

        foreach (var v in new View[]
                 {
                     _progress, _statement, _category, _scale, _responses, _next, _skip,
                     _summary, _results, _comparison, _caveat, _back,
                 })
            column.AddView(v);

        return Ui.Scroll(context, column);
    }

    /// <inheritdoc />
    protected override void Render()
    {
        if (Vm.HasLoadError)
        {
            _statement.Text = "Couldn't start";
            _category.Text = Vm.LoadError;
            Hide(_progress, _scale, _responses, _next, _skip, _summary, _results, _comparison, _caveat);
            return;
        }

        var playing = Vm.IsPlaying;
        var complete = Vm.IsComplete;

        _progress.Text = Vm.ProgressLabel;
        _statement.Text = playing ? Vm.Statement : "Results";
        _category.Text = playing ? Vm.Category : "";

        Show(playing, _progress, _scale, _responses, _next, _skip);
        Show(complete, _summary, _results, _caveat);

        _summary.Text = Vm.Summary;
        _comparison.Text = Vm.ComparisonSummary;
        _comparison.Visibility = complete && Vm.HasComparison ? ViewStates.Visible : ViewStates.Gone;

        // Rebuild only when the roster changes shape. Rebuilding every render
        // would drop the button row the player is mid-tap on, and Render runs on
        // every property change.
        if (playing && _responses.ChildCount != Vm.PlayerResponses.Count)
            BuildResponseRows();

        if (complete && _results.ChildCount != Vm.Profiles.Count)
            BuildResults();
    }

    private void BuildResponseRows()
    {
        var context = _responses.Context!;
        _responses.RemoveAllViews();

        foreach (var entry in Vm.PlayerResponses)
        {
            var captured = entry;

            var name = Ui.Label(context, captured.PlayerName, 14f, bold: true);
            var row = Ui.Row(context);

            for (var value = 1; value <= 5; value++)
            {
                // Captured per iteration: without this every button would send
                // 6, the value the loop variable holds once it exits.
                var choice = value;
                row.AddView(Ui.Button(context, choice.ToString())
                    .OnClick(() => captured.Pick(choice)));
            }

            _responses.AddView(name);
            _responses.AddView(row);
        }
    }

    private void BuildResults()
    {
        var context = _results.Context!;
        _results.RemoveAllViews();

        foreach (var profile in Vm.Profiles)
        {
            _results.AddView(Ui.Label(context, profile.PlayerName, 17f, bold: true));

            if (profile.HasTopTrait)
                _results.AddView(Ui.Label(context, $"Strongest: {profile.TopTrait}", 13f));

            foreach (var score in profile.Scores)
            {
                var line = score.HasData
                    ? $"{score.TraitName}  —  {score.Rounded} · {score.BandLabel}"
                    : $"{score.TraitName}  —  no answers";
                _results.AddView(Ui.Label(context, line, 14f));
            }
        }
    }

    private static void Show(bool visible, params View[] views)
    {
        foreach (var v in views)
            v.Visibility = visible ? ViewStates.Visible : ViewStates.Gone;
    }

    private static void Hide(params View[] views) => Show(false, views);
}
