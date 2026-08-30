using TableTop.Hosting;
using TableTop.Maui.ViewModels;

namespace TableTop.Maui.Pages;

public partial class GameSelectionPage : ContentPage
{
    private readonly GameSelectionViewModel _vm;

    // DI constructor — MAUI injects GameSelectionViewModel from the service container
    public GameSelectionPage(GameSelectionViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    // ── Selection ────────────────────────────────────────────────────────────
    //
    // Selection is handled here in code-behind rather than through bindings,
    // deliberately, because two separate things were breaking it:
    //
    //   1. A Frame inside a CollectionView DataTemplate consumes the touch on
    //      Android, so the CollectionView never raises a selection at all.
    //   2. The obvious fix — a TapGestureRecognizer bound to a command on the
    //      page's ViewModel via {x:Reference} — does NOT resolve reliably from
    //      inside a DataTemplate, because the template has its own namescope.
    //      The binding fails silently, so the command simply never runs, which
    //      looks exactly like "tapping does nothing".
    //
    // Reading BindingContext off the tapped element sidesteps both problems:
    // no namescope, no binding resolution, nothing to fail quietly. Both the
    // tap and the CollectionView's own SelectionChanged route to the same
    // setters, and those setters are idempotent, so the pair firing together
    // is harmless.

    private static T? ItemFrom<T>(object? sender) where T : class =>
        (sender as BindableObject)?.BindingContext as T;

    private void OnArchetypeTapped(object? sender, TappedEventArgs e)
    {
        if (ItemFrom<Archetype>(sender) is { } archetype)
            _vm.SelectedArchetype = archetype;
    }

    private void OnSubArchetypeTapped(object? sender, TappedEventArgs e)
    {
        if (ItemFrom<Archetype>(sender) is { } sub)
            _vm.SelectedSubArchetype = sub;
    }

    private void OnGameModeTapped(object? sender, TappedEventArgs e)
    {
        // Rows are GameModeItem now, not IGameMode — unwrap before assigning.
        if (ItemFrom<ViewModels.GameModeItem>(sender) is { } item)
            _vm.SelectedGameMode = item.Mode;
    }

    private void OnArchetypeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_vm.IsRebuilding) return;   // VM-driven change, not a user tap
        if (e.CurrentSelection.FirstOrDefault() is Archetype archetype)
            _vm.SelectedArchetype = archetype;
    }

    private void OnSubArchetypeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_vm.IsRebuilding) return;
        if (e.CurrentSelection.FirstOrDefault() is Archetype sub)
            _vm.SelectedSubArchetype = sub;
    }

    private void OnGameModeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_vm.IsRebuilding) return;
        if (e.CurrentSelection.FirstOrDefault() is ViewModels.GameModeItem item)
            _vm.SelectedGameMode = item.Mode;
    }

    // ── Navigation ───────────────────────────────────────────────────────────

    // One navigation at a time. Two PushAsync calls in flight — trivially
    // caused by an impatient double-tap — throw, and an exception escaping an
    // async void handler terminates the process on Android rather than being
    // caught anywhere useful.
    private bool _navigating;

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        if (_navigating) return;
        _navigating = true;
        try
        {
            var page = IPlatformApplication.Current!.Services.GetRequiredService<SettingsPage>();
            await Navigation.PushAsync(page);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Couldn't open settings", ex.Message, "OK");
        }
        finally { _navigating = false; }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Re-checked every time this page shows, so finishing a game removes the
        // stale offer without a restart.
        //
        // Guarded even though the shared saved-session lookup already catches
        // everything inside its own RefreshAsync, so nothing can reach here
        // today. That is a property of today's implementation rather than a
        // guarantee — the same reasoning backlog item 20 applied to its
        // deadlock risk — and this is the one handler that runs with no user
        // action at all, on every appearance. Silent is the right failure
        // here: a resume offer that can't be built is not worth an alert on a
        // screen the player just opened.
        try
        {
            await _vm.LookForSavedSessionAsync();
        }
        catch
        {
            // Leave the resume offer hidden; the picker below still works.
        }
    }

    private async void OnResumeClicked(object sender, EventArgs e)
    {
        if (_navigating) return;
        _navigating = true;
        try
        {
            if (_vm.Resumable is not { } r) return;

            // Straight to gameplay — the picker and player setup would only ask
            // again for what the snapshot already records.
            var page = new GameplayPage(r.Mode, r.Players.ToList(), r.Snapshot);
            await page.InitializeAsync();  // backlog item 20 — async build, awaited before showing
            await Navigation.PushAsync(page);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Couldn't resume", ex.Message, "OK");
        }
        finally { _navigating = false; }
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        if (_navigating) return;
        _navigating = true;
        try
        {
            if (_vm.SelectedGameMode is null)
            {
                await DisplayAlertAsync("Select Game", "Please choose a game to play.", "OK");
                return;
            }

            var page = new PlayerSetupPage(_vm.SelectedGameMode);
            await Navigation.PushAsync(page);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Couldn't start the game", ex.Message, "OK");
        }
        finally { _navigating = false; }
    }
}
