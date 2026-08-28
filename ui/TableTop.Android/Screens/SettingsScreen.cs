using Android.Content;
using Android.Views;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;
using TableTop.Droid.Infrastructure;
using TableTop.Presentation.Infrastructure;
using TableTop.Presentation.ViewModels;

namespace TableTop.Droid.Screens;

/// <summary>Settings screen — every setter writes straight through to
/// <see cref="IAppSettings"/> via the shared <see cref="SettingsViewModel"/>.</summary>
public sealed class SettingsScreen : Screen
{
    private SettingsViewModel _vm = null!;
    private ViewModelBinder? _binder;
    private readonly List<Action> _apply = [];
    private bool _suppress;

    /// <inheritdoc />
    public override string Title => "Settings";

    /// <inheritdoc />
    protected override View OnCreateView(Context context)
    {
        _vm = new SettingsViewModel(Navigator, MainApplication.Services.GetRequiredService<IAppSettings>());

        var column = Ui.Column(context);
        column.AddView(Ui.Heading(context, "Settings"));

        AddSpinner(context, column, "Theme", _vm.ThemeOptions,
            () => _vm.ThemeIndex, v => _vm.ThemeIndex = v);
        AddSpinner(context, column, "Card font size", _vm.FontSizeOptions,
            () => _vm.FontSizeIndex, v => _vm.FontSizeIndex = v);
        AddSwitch(context, column, "Shuffle deck before each game",
            () => _vm.ShuffleCards, v => _vm.ShuffleCards = v);
        AddSpinner(context, column, "Minimum difficulty", _vm.DifficultyOptions,
            () => _vm.MinDifficultyIndex, v => _vm.MinDifficultyIndex = v);
        AddSpinner(context, column, "Maximum difficulty", _vm.DifficultyOptions,
            () => _vm.MaxDifficultyIndex, v => _vm.MaxDifficultyIndex = v);
        AddSpinner(context, column, "Show games rated", _vm.AgeOptions,
            () => _vm.MinAgeRatingIndex, v => _vm.MinAgeRatingIndex = v);
        AddSwitch(context, column, "Auto-advance to next player",
            () => _vm.AutoNextPlayer, v => _vm.AutoNextPlayer = v);
        AddSwitch(context, column, "Per-card countdown timer",
            () => _vm.EnableTimer, v => _vm.EnableTimer = v);
        AddSpinner(context, column, "Timer length", _vm.TimerOptions,
            () => _vm.TimerIndex, v => _vm.TimerIndex = v);
        AddSwitch(context, column, "Show card-count line",
            () => _vm.ShowCardCount, v => _vm.ShowCardCount = v);
        AddSwitch(context, column, "Show difficulty badge",
            () => _vm.ShowDifficultyBadge, v => _vm.ShowDifficultyBadge = v);
        AddSwitch(context, column, "Show category badge",
            () => _vm.ShowCategoryBadge, v => _vm.ShowCategoryBadge = v);

        column.AddView(Ui.Button(context, "Reset to defaults").OnClick(() =>
        {
            new AlertDialog.Builder(context)
                .SetTitle("Reset settings?")!
                .SetMessage("Every setting returns to its default.")!
                .SetPositiveButton("Reset", (_, _) => _vm.ResetToDefaults())!
                .SetNegativeButton("Cancel", (IDialogInterfaceOnClickListener?)null)!
                .Show();
        }));

        _binder = new ViewModelBinder(Host, _vm, Render);
        return Ui.Scroll(context, column);
    }

    private void Render()
    {
        _suppress = true;
        foreach (var apply in _apply) apply();
        _suppress = false;
    }

    private void AddSpinner(
        Context context, LinearLayout parent, string label,
        IReadOnlyList<string> options, Func<int> get, Action<int> set)
    {
        parent.AddView(Ui.Label(context, label, 14f));
        var spinner = Ui.Dropdown(context, options);
        spinner.ItemSelected += (_, e) => { if (!_suppress && e.Position != get()) set(e.Position); };
        parent.AddView(spinner);
        _apply.Add(() => spinner.SetSelection(Math.Clamp(get(), 0, options.Count - 1)));
    }

    private void AddSwitch(
        Context context, LinearLayout parent, string label, Func<bool> get, Action<bool> set)
    {
        var toggle = Ui.Toggle(context, label);
        toggle.CheckedChange += (_, e) => { if (!_suppress && e.IsChecked != get()) set(e.IsChecked); };
        parent.AddView(toggle);
        _apply.Add(() => toggle.Checked = get());
    }

    /// <inheritdoc />
    public override void OnRemoved() => _binder?.Dispose();
}
