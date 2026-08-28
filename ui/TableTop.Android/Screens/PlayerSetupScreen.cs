using Android.Content;
using Android.Views;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;
using TableTop.Core.Abstractions.Game;
using TableTop.Droid.Infrastructure;
using TableTop.Presentation.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Droid.Screens;

/// <summary>Player setup for a chosen mode. Drives the shared
/// <see cref="PlayerSetupViewModel"/>; the <c>onStart</c> callback builds and
/// pushes the gameplay screen once the roster validates.</summary>
public sealed class PlayerSetupScreen(IGameMode mode) : Screen
{
    private PlayerSetupViewModel _vm = null!;
    private ViewModelBinder? _binder;

    private EditText _name = null!;
    private Spinner _gender = null!;
    private EditText _age = null!;
    private Switch _couple = null!;
    private LinearLayout _playerList = null!;
    private LinearLayout _rosterList = null!;
    private TextView _error = null!;
    private TextView _status = null!;

    /// <inheritdoc />
    public override string Title => $"Set up — {mode.Name}";

    /// <inheritdoc />
    protected override View OnCreateView(Context context)
    {
        var settings = MainApplication.Services.GetRequiredService<IAppSettings>();
        var rosters = MainApplication.Services.GetRequiredService<IRosterStore>();

        _vm = new PlayerSetupViewModel(
            Navigator, mode, settings,
            onStart: async players =>
            {
                var screen = await GameScreenFactory.CreateAsync(
                    new StackNavigator(Host), mode, players, settings);
                Host.RunOnUiThread(() => Host.Push(screen));
            },
            rosterStore: rosters);

        var column = Ui.Column(context);
        column.AddView(Ui.Heading(context, mode.Name));

        _name = Ui.Field(context, "Name");
        _name.TextChanged += (_, _) => _vm.NewName = _name.Text ?? "";
        column.AddView(_name);

        _gender = Ui.Dropdown(context, _vm.GenderOptions.Select(g => g.Length == 0 ? "(unspecified)" : g).ToList());
        _gender.ItemSelected += (_, e) => _vm.SelectedGender = _vm.GenderOptions[e.Position];
        column.AddView(_gender);

        _age = Ui.Field(context, "Age (optional)");
        _age.InputType = Android.Text.InputTypes.ClassNumber;
        _age.TextChanged += (_, _) => _vm.NewAge = _age.Text ?? "";
        column.AddView(_age);

        _couple = Ui.Toggle(context, "Part of the couple");
        _couple.CheckedChange += (_, e) => _vm.NewIsCouple = e.IsChecked;
        column.AddView(_couple);

        column.AddView(Ui.Button(context, "Add player").OnClick(_vm.AddPlayerCommand));

        _playerList = new LinearLayout(context) { Orientation = Orientation.Vertical };
        column.AddView(_playerList);

        _error = Ui.Label(context, "");
        _error.SetTextColor(Android.Graphics.Color.ParseColor("#EF4444"));
        column.AddView(_error);

        _status = Ui.Label(context, "", 13f);
        column.AddView(_status);

        column.AddView(Ui.Button(context, "Save as default roster").OnClick(_vm.SaveRosterCommand));

        column.AddView(Ui.Label(context, "Saved rosters", 14f));
        _rosterList = new LinearLayout(context) { Orientation = Orientation.Vertical };
        column.AddView(_rosterList);

        column.AddView(Ui.Button(context, "Start game").OnClick(_vm.StartCommand));

        _binder = new ViewModelBinder(Host, _vm, () => Render(context));
        return Ui.Scroll(context, column);
    }

    private void Render(Context context)
    {
        if (_name.Text != _vm.NewName) _name.Text = _vm.NewName;
        if (_age.Text != _vm.NewAge) _age.Text = _vm.NewAge;
        _couple.Checked = _vm.NewIsCouple;
        _error.Text = _vm.Error;
        _error.Visibility = _vm.HasError ? ViewStates.Visible : ViewStates.Gone;
        _status.Text = _vm.RosterStatus;

        _playerList.RemoveAllViews();
        foreach (var p in _vm.Players)
        {
            var row = Ui.Row(context);
            var label = Ui.Label(context, p.HasDetail ? $"{p.Name}  ({p.Detail})" : p.Name);
            label.LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1f);
            row.AddView(label);
            var remove = new Android.Widget.Button(context) { Text = "✕" };
            var captured = p;
            remove.Click += (_, _) => _vm.RemovePlayer(captured);
            row.AddView(remove);
            _playerList.AddView(row);
        }

        _rosterList.RemoveAllViews();
        foreach (var r in _vm.SavedRosters)
        {
            var captured = r;
            _rosterList.AddView(Ui.Button(context, $"{r.Name} — {r.Subtitle}").OnClick(() => captured.Invoke()));
        }
        _rosterList.Visibility = _vm.HasSavedRosters ? ViewStates.Visible : ViewStates.Gone;
    }

    /// <inheritdoc />
    public override void OnRemoved() => _binder?.Dispose();
}
