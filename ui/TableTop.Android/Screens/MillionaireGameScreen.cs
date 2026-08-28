using Android.Content;
using Android.Views;
using Android.Widget;
using TableTop.Droid.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Droid.Screens;

/// <summary>Quiz-ladder gameplay — drives the shared <see cref="MillionaireGameViewModel"/>.</summary>
public sealed class MillionaireGameScreen(MillionaireGameViewModel vm)
    : GameScreenBase<MillionaireGameViewModel>(vm)
{
    private TextView _player = null!, _prize = null!, _guaranteed = null!, _question = null!;
    private TextView _flash = null!, _summary = null!;
    private LinearLayout _answers = null!, _lifelines = null!;
    private Button _walkAway = null!, _back = null!;

    /// <inheritdoc />
    public override string Title => "Quiz";

    /// <inheritdoc />
    protected override View Build(Context context)
    {
        var column = Ui.Column(context);
        _player = Ui.Label(context, "", 16f, bold: true);
        _prize = Ui.Label(context, "", 14f);
        _guaranteed = Ui.Label(context, "", 12f);
        _question = Ui.Heading(context, "");
        _flash = Ui.Label(context, "", 14f);
        _summary = Ui.Label(context, "", 16f);
        _answers = new LinearLayout(context) { Orientation = Orientation.Vertical };
        _lifelines = Ui.Row(context);
        _walkAway = Ui.Button(context, "Walk away").OnClick(Vm.WalkAwayCommand);
        _back = Ui.Button(context, "Back").OnClick(Vm.BackCommand);

        foreach (var v in new View[] { _player, _prize, _guaranteed, _question, _flash, _answers, _lifelines, _summary, _walkAway, _back })
            column.AddView(v);
        return Ui.Scroll(context, column);
    }

    /// <inheritdoc />
    protected override void Render()
    {
        if (Vm.HasLoadError)
        {
            _question.Text = "Couldn't start";
            _flash.Text = Vm.LoadError;
            Hide(_player, _prize, _guaranteed, _answers, _lifelines, _walkAway);
            return;
        }

        _player.Text = Vm.PlayerName;
        _prize.Text = Vm.PrizeText;
        _guaranteed.Text = Vm.GuaranteedText;
        _question.Text = Vm.QuestionText;
        _flash.Text = Vm.Flash;
        _summary.Text = Vm.Summary;
        _summary.Visibility = Vm.IsGameOver ? ViewStates.Visible : ViewStates.Gone;
        _walkAway.Visibility = Vm.CanInteract ? ViewStates.Visible : ViewStates.Gone;

        _answers.RemoveAllViews();
        foreach (var a in Vm.Answers)
        {
            var captured = a;
            var b = Ui.Button(_answers.Context!, captured.Display).OnClick(() => captured.Invoke());
            b.Enabled = Vm.CanInteract;
            _answers.AddView(b);
        }

        _lifelines.RemoveAllViews();
        foreach (var l in Vm.Lifelines)
        {
            var captured = l;
            var b = Ui.Button(_lifelines.Context!, captured.Name).OnClick(() => captured.Invoke());
            b.Enabled = captured.IsAvailable && Vm.CanInteract;
            _lifelines.AddView(b);
        }
    }

    private static void Hide(params View[] views)
    {
        foreach (var v in views) v.Visibility = ViewStates.Gone;
    }
}
