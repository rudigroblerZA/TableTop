using Android.Content;
using Android.Views;
using Android.Widget;
using TableTop.Droid.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Droid.Screens;

/// <summary>Day One daily-campaign gameplay — drives the shared <see cref="DayOneGameViewModel"/>.</summary>
public sealed class DayOneGameScreen(DayOneGameViewModel vm)
    : GameScreenBase<DayOneGameViewModel>(vm)
{
    private TextView _day = null!, _title = null!, _body = null!, _status = null!;
    private Button _complete = null!, _back = null!;

    /// <inheritdoc />
    public override string Title => "Day One";

    /// <inheritdoc />
    protected override View Build(Context context)
    {
        var column = Ui.Column(context);
        _day = Ui.Label(context, "", 16f, bold: true);
        _title = Ui.Heading(context, "");
        _body = Ui.Label(context, "", 18f);
        _status = Ui.Label(context, "", 13f);
        _complete = Ui.Button(context, "Mark today done").OnClick(Vm.CompleteTodayCommand);
        _back = Ui.Button(context, "Back").OnClick(Vm.BackCommand);

        foreach (var v in new View[] { _day, _title, _body, _status, _complete, _back })
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
            _day.Visibility = _status.Visibility = _complete.Visibility = ViewStates.Gone;
            return;
        }

        _day.Text = Vm.DayLabel;
        _title.Text = Vm.CardTitle;
        _body.Text = Vm.CardText;
        _status.Text = Vm.StatusText;
        _title.Visibility = _body.Visibility = Vm.HasCard ? ViewStates.Visible : ViewStates.Gone;
        _complete.Visibility = Vm.HasCard && !Vm.IsDone ? ViewStates.Visible : ViewStates.Gone;
    }
}
