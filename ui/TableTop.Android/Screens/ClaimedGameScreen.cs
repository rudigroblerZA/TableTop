using Android.Content;
using Android.Views;
using Android.Widget;
using TableTop.Droid.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Droid.Screens;

/// <summary>Claimed! area-control gameplay — drives the shared <see cref="ClaimedGameViewModel"/>.</summary>
public sealed class ClaimedGameScreen(ClaimedGameViewModel vm)
    : GameScreenBase<ClaimedGameViewModel>(vm)
{
    private TextView _current = null!, _flash = null!, _summary = null!;
    private TextView _pendingTitle = null!, _pendingBody = null!, _pendingMeta = null!;
    private LinearLayout _territories = null!, _resolveRow = null!;
    private Button _back = null!;

    /// <inheritdoc />
    public override string Title => "Claimed!";

    /// <inheritdoc />
    protected override View Build(Context context)
    {
        var column = Ui.Column(context);
        _current = Ui.Label(context, "", 16f, bold: true);
        _territories = new LinearLayout(context) { Orientation = Orientation.Vertical };
        _pendingTitle = Ui.Heading(context, "");
        _pendingBody = Ui.Label(context, "", 18f);
        _pendingMeta = Ui.Label(context, "", 12f);
        _resolveRow = Ui.Row(context);
        _resolveRow.AddView(Ui.Button(context, "Succeeded").OnClick(Vm.SucceedCommand));
        _resolveRow.AddView(Ui.Button(context, "Failed").OnClick(Vm.FailCommand));
        _flash = Ui.Label(context, "", 14f);
        _summary = Ui.Label(context, "", 16f);
        _back = Ui.Button(context, "Back").OnClick(Vm.BackCommand);

        foreach (var v in new View[] { _current, _territories, _pendingTitle, _pendingBody, _pendingMeta, _resolveRow, _flash, _summary, _back })
            column.AddView(v);
        return Ui.Scroll(context, column);
    }

    /// <inheritdoc />
    protected override void Render()
    {
        if (Vm.HasLoadError)
        {
            _pendingTitle.Text = "Couldn't start";
            _pendingBody.Text = Vm.LoadError;
            _current.Visibility = _territories.Visibility = _pendingMeta.Visibility = _resolveRow.Visibility = ViewStates.Gone;
            return;
        }

        _current.Text = Vm.IsGameOver ? "" : $"{Vm.CurrentPlayerName}'s turn";
        _flash.Text = Vm.Flash;
        _summary.Text = Vm.Summary;
        _summary.Visibility = Vm.IsGameOver ? ViewStates.Visible : ViewStates.Gone;

        var pending = Vm.HasPendingChallenge;
        _pendingTitle.Text = pending ? Vm.PendingCardTitle : "";
        _pendingBody.Text = pending ? Vm.PendingCardText : "";
        _pendingMeta.Text = pending
            ? string.Join("   ·   ", new[]
            {
                Vm.PendingDifficulty,
                Vm.IsRaid ? "Raid" : null,
                Vm.PendingDefenderName is { } d ? $"vs {d}" : null,
            }.Where(s => !string.IsNullOrEmpty(s)))
            : "";
        _pendingTitle.Visibility = _pendingBody.Visibility = _pendingMeta.Visibility =
            pending ? ViewStates.Visible : ViewStates.Gone;
        _resolveRow.Visibility = pending ? ViewStates.Visible : ViewStates.Gone;

        _territories.RemoveAllViews();
        _territories.Visibility = Vm.IsPlaying && !pending ? ViewStates.Visible : ViewStates.Gone;
        if (Vm.IsPlaying && !pending)
        {
            foreach (var t in Vm.Territories)
            {
                var captured = t;
                var b = Ui.Button(_territories.Context!, $"{t.Name} — {t.HolderDisplay}")
                    .OnClick(() => captured.Invoke());
                b.Enabled = captured.IsChallengeable;
                _territories.AddView(b);
            }
        }
    }
}
