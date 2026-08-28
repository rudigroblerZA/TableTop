using Android.Content;
using Android.Views;
using Android.Widget;
using TableTop.Droid.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Droid.Screens;

/// <summary>The main gameplay loop — drives the shared <see cref="CardTurnGameViewModel"/>,
/// the same class WinUI and MAUI use.</summary>
public sealed class CardTurnGameScreen(CardTurnGameViewModel vm)
    : GameScreenBase<CardTurnGameViewModel>(vm)
{
    private TextView _player = null!, _count = null!, _title = null!, _meta = null!, _body = null!;
    private TextView _scores = null!, _flash = null!, _hint = null!, _timer = null!, _summary = null!;
    private LinearLayout _choiceRow = null!, _actionRow = null!, _flowRow = null!;
    private Button _flip = null!, _complete = null!, _skip = null!, _undo = null!, _save = null!, _quit = null!;

    /// <inheritdoc />
    public override string Title => Vm.ModeTitle;

    /// <inheritdoc />
    protected override View Build(Context context)
    {
        var column = Ui.Column(context);

        _player = Ui.Label(context, "", 16f, bold: true);
        _count = Ui.Label(context, "", 13f);
        _title = Ui.Heading(context, "");
        _meta = Ui.Label(context, "", 12f);
        _body = Ui.Label(context, "", 18f);
        _flash = Ui.Label(context, "", 14f);
        _hint = Ui.Label(context, "", 13f);
        _timer = Ui.Label(context, "", 20f, bold: true);
        _scores = Ui.Label(context, "", 13f);
        _summary = Ui.Label(context, "", 16f);

        _flip = Ui.Button(context, "Reveal answer").OnClick(() => Vm.FlipCard());
        _choiceRow = new LinearLayout(context) { Orientation = Orientation.Vertical };

        _complete = Ui.Button(context, "Completed").OnClick(() => Vm.Complete());
        _skip = Ui.Button(context, "Skip").OnClick(() => Vm.Skip());
        _actionRow = Ui.Row(context);
        _actionRow.AddView(_complete);
        _actionRow.AddView(_skip);

        _undo = Ui.Button(context, "Undo").OnClick(Vm.UndoCommand);
        _save = Ui.Button(context, "Save").OnClick(Vm.SaveCommand);
        _quit = Ui.Button(context, "Quit").OnClick(() => Vm.Quit());

        _flowRow = Ui.Row(context);
        _flowRow.AddView(Ui.Button(context, "Level −").OnClick(Vm.LevelDownCommand));
        _flowRow.AddView(Ui.Button(context, "Level +").OnClick(Vm.LevelUpCommand));
        _flowRow.AddView(Ui.Button(context, "Slower").OnClick(Vm.SlowDownCommand));
        _flowRow.AddView(Ui.Button(context, "Faster").OnClick(Vm.SpeedUpCommand));

        foreach (var v in new View[]
                 {
                     _player, _count, _title, _meta, _body, _timer, _flash, _hint,
                     _flip, _choiceRow, _actionRow, _flowRow, _scores, _summary,
                     _undo, _save, _quit,
                 })
            column.AddView(v);

        return Ui.Scroll(context, column);
    }

    /// <inheritdoc />
    protected override void Render()
    {
        if (Vm.HasLoadError)
        {
            _title.Text = "Couldn't start";
            _body.Text = Vm.LoadError;
            Hide(_player, _count, _meta, _timer, _flash, _hint, _flip, _choiceRow,
                _actionRow, _flowRow, _scores, _undo, _save);
            _quit.Text = "Back";
            return;
        }

        _player.Text = Vm.PlayerName;
        _count.Text = Vm.CardCountText;
        _count.Visibility = Vm.ShowCardCount && !Vm.IsGameOver ? ViewStates.Visible : ViewStates.Gone;
        _title.Text = Vm.CardTitle;
        _meta.Text = string.Join("   ·   ", new[] { Vm.CardCategory, Vm.CardDifficulty }.Where(s => s.Length > 0));
        _body.Text = Vm.CardBodyText;

        _flash.Text = Vm.FlashText;
        _flash.Visibility = Vm.HasFlash ? ViewStates.Visible : ViewStates.Gone;

        _hint.Text = Vm.HintText;
        _hint.Visibility = Vm.HasHint ? ViewStates.Visible : ViewStates.Gone;
        _hint.SetTextColor(Android.Graphics.Color.ParseColor(Vm.HintColor));

        _timer.Text = Vm.TimerDisplay;
        _timer.Visibility = Vm.TimerEnabled && !Vm.IsGameOver ? ViewStates.Visible : ViewStates.Gone;

        _flip.Visibility = Vm.HasBack && !Vm.IsGameOver ? ViewStates.Visible : ViewStates.Gone;
        _flip.Text = Vm.FlipButtonText;

        RenderChoices();

        _actionRow.Visibility = Vm.IsPlaying && !Vm.HasChoices ? ViewStates.Visible : ViewStates.Gone;
        _complete.Text = Vm.CompleteLabel;
        _skip.Text = Vm.SkipLabel;

        _flowRow.Visibility = Vm.SupportsFlow && Vm.IsPlaying ? ViewStates.Visible : ViewStates.Gone;

        _scores.Text = Vm.ScoresText;
        _scores.Visibility = Vm.HasScores ? ViewStates.Visible : ViewStates.Gone;

        _summary.Text = Vm.SummaryText;
        _summary.Visibility = Vm.IsGameOver ? ViewStates.Visible : ViewStates.Gone;

        _undo.Visibility = _save.Visibility = Vm.IsPlaying ? ViewStates.Visible : ViewStates.Gone;
        _undo.Enabled = Vm.CanUndo;
        _save.Enabled = Vm.CanSave;
        _quit.Text = Vm.IsGameOver ? "Back" : "Quit";
    }

    private void RenderChoices()
    {
        _choiceRow.RemoveAllViews();
        if (!Vm.HasChoices || Vm.IsGameOver)
        {
            _choiceRow.Visibility = ViewStates.Gone;
            return;
        }

        _choiceRow.Visibility = ViewStates.Visible;
        foreach (var choice in Vm.Choices)
        {
            var captured = choice;
            _choiceRow.AddView(Ui.Button(_choiceRow.Context!, captured.Display).OnClick(() => captured.Invoke()));
        }
    }

    private static void Hide(params View[] views)
    {
        foreach (var v in views) v.Visibility = ViewStates.Gone;
    }
}
