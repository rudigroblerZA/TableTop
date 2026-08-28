using Android.Content;
using Android.Views;
using Android.Widget;
using TableTop.Droid.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Droid.Screens;

/// <summary>Monogamy gameplay — drives the shared <see cref="MonogamyGameViewModel"/>.</summary>
public sealed class MonogamyGameScreen(MonogamyGameViewModel vm)
    : GameScreenBase<MonogamyGameViewModel>(vm)
{
    private TextView _player = null!, _dice = null!, _zone = null!, _tokens = null!;
    private TextView _title = null!, _body = null!, _flash = null!, _scores = null!, _summary = null!;
    private LinearLayout _zoneChoices = null!, _cardActions = null!;
    private Button _back = null!;

    /// <inheritdoc />
    public override string Title => "Monogamy";

    /// <inheritdoc />
    protected override View Build(Context context)
    {
        var column = Ui.Column(context);
        _player = Ui.Label(context, "", 16f, bold: true);
        _dice = Ui.Label(context, "", 20f, bold: true);
        _zone = Ui.Label(context, "", 14f);
        _tokens = Ui.Label(context, "", 13f);
        _title = Ui.Heading(context, "");
        _body = Ui.Label(context, "", 18f);
        _flash = Ui.Label(context, "", 14f);
        _scores = Ui.Label(context, "", 13f);
        _summary = Ui.Label(context, "", 16f);
        _zoneChoices = new LinearLayout(context) { Orientation = Orientation.Vertical };

        _cardActions = Ui.Row(context);
        _cardActions.AddView(Ui.Button(context, "Complete").OnClick(Vm.CompleteCommand));
        _cardActions.AddView(Ui.Button(context, "Negotiate").OnClick(Vm.NegotiateCommand));
        _cardActions.AddView(Ui.Button(context, "Skip").OnClick(Vm.SkipCommand));

        _back = Ui.Button(context, "Back").OnClick(Vm.BackCommand);

        foreach (var v in new View[] { _player, _dice, _zone, _tokens, _title, _body, _zoneChoices, _cardActions, _flash, _scores, _summary, _back })
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
            Hide(_player, _dice, _zone, _tokens, _zoneChoices, _cardActions, _scores);
            return;
        }

        _player.Text = Vm.PlayerName;
        _dice.Text = Vm.DiceText;
        _zone.Text = Vm.ZoneName;
        _tokens.Text = Vm.TokenText;
        _title.Text = Vm.CardTitle;
        _body.Text = Vm.CardText;
        _flash.Text = Vm.Flash;
        _scores.Text = Vm.Scores;
        _scores.Visibility = Vm.HasScores ? ViewStates.Visible : ViewStates.Gone;
        _summary.Text = Vm.Summary;
        _summary.Visibility = Vm.IsGameOver ? ViewStates.Visible : ViewStates.Gone;

        _cardActions.Visibility = Vm.HasCard && Vm.IsPlaying ? ViewStates.Visible : ViewStates.Gone;

        _zoneChoices.RemoveAllViews();
        _zoneChoices.Visibility = Vm.AwaitingZone ? ViewStates.Visible : ViewStates.Gone;
        if (Vm.AwaitingZone)
        {
            _zoneChoices.AddView(Ui.Label(_zoneChoices.Context!, "Doubles! Choose a zone:", 14f));
            foreach (var z in Vm.ZoneChoices)
            {
                var captured = z;
                _zoneChoices.AddView(Ui.Button(_zoneChoices.Context!, captured.Display).OnClick(() => captured.Invoke()));
            }
        }
    }

    private static void Hide(params View[] views)
    {
        foreach (var v in views) v.Visibility = ViewStates.Gone;
    }
}
