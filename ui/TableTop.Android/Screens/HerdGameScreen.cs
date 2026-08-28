using Android.Content;
using Android.Views;
using Android.Widget;
using TableTop.Droid.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Droid.Screens;

/// <summary>Herd Mentality simultaneous-answer gameplay — drives the shared
/// <see cref="HerdGameViewModel"/>.</summary>
public sealed class HerdGameScreen(HerdGameViewModel vm)
    : GameScreenBase<HerdGameViewModel>(vm)
{
    private TextView _round = null!, _prompt = null!, _category = null!, _scores = null!, _summary = null!;
    private TextView _lastRound = null!;
    private LinearLayout _answers = null!;
    private Button _reveal = null!, _dismiss = null!, _back = null!;

    /// <inheritdoc />
    public override string Title => "Herd Mentality";

    /// <inheritdoc />
    protected override View Build(Context context)
    {
        var column = Ui.Column(context);
        _round = Ui.Label(context, "", 13f);
        _prompt = Ui.Heading(context, "");
        _category = Ui.Label(context, "", 12f);
        _answers = new LinearLayout(context) { Orientation = Orientation.Vertical };
        _reveal = Ui.Button(context, "Reveal").OnClick(Vm.RevealCommand);
        _lastRound = Ui.Label(context, "", 14f);
        _dismiss = Ui.Button(context, "Next round").OnClick(Vm.DismissLastRoundCommand);
        _scores = Ui.Label(context, "", 13f);
        _summary = Ui.Label(context, "", 16f);
        _back = Ui.Button(context, "Back").OnClick(Vm.BackCommand);

        foreach (var v in new View[] { _round, _prompt, _category, _answers, _reveal, _lastRound, _dismiss, _scores, _summary, _back })
            column.AddView(v);
        return Ui.Scroll(context, column);
    }

    /// <inheritdoc />
    protected override void Render()
    {
        if (Vm.HasLoadError)
        {
            _prompt.Text = "Couldn't start";
            _category.Text = Vm.LoadError;
            Hide(_round, _answers, _reveal, _lastRound, _dismiss, _scores);
            return;
        }

        _round.Text = $"Round {Vm.RoundNumber} of {Vm.TotalRounds}";
        _prompt.Text = Vm.Prompt;
        _category.Text = Vm.Category;
        _scores.Text = Vm.Scores;
        _scores.Visibility = Vm.HasScores ? ViewStates.Visible : ViewStates.Gone;
        _summary.Text = Vm.Summary;
        _summary.Visibility = Vm.IsGameOver ? ViewStates.Visible : ViewStates.Gone;

        var showRound = Vm.ShowingLastRound;
        _lastRound.Text = string.Join("\n", new[]
        {
            Vm.LastRoundSummary,
            Vm.HasLastHerdAnswer ? $"Herd: {Vm.LastHerdAnswer}" : null,
            Vm.HasLastLoneVoice ? $"Lone voice: {Vm.LastLoneVoice}" : null,
        }.Where(s => !string.IsNullOrEmpty(s)));
        _lastRound.Visibility = showRound ? ViewStates.Visible : ViewStates.Gone;
        _dismiss.Visibility = showRound ? ViewStates.Visible : ViewStates.Gone;

        var playing = Vm.IsPlaying && !showRound;
        _answers.Visibility = playing ? ViewStates.Visible : ViewStates.Gone;
        _reveal.Visibility = playing ? ViewStates.Visible : ViewStates.Gone;

        if (_answers.ChildCount != Vm.PlayerAnswers.Count)
        {
            _answers.RemoveAllViews();
            foreach (var entry in Vm.PlayerAnswers)
            {
                var captured = entry;
                var field = Ui.Field(_answers.Context!, captured.PlayerName);
                field.Text = captured.Answer;
                field.TextChanged += (_, _) => captured.Answer = field.Text ?? "";
                _answers.AddView(field);
            }
        }
    }

    private static void Hide(params View[] views)
    {
        foreach (var v in views) v.Visibility = ViewStates.Gone;
    }
}
