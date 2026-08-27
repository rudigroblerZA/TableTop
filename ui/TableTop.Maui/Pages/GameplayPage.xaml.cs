using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Maui.ViewModels;

namespace TableTop.Maui.Pages;

public partial class GameplayPage : ContentPage, IAsyncInitializablePage
{
    private readonly IGameMode _gameMode;
    private readonly List<IPlayer> _players;
    private readonly TableTop.Hosting.Persistence.SessionSnapshot? _resumeFrom;
    private GameplayViewModel _vm = null!;

    /// <summary>
    /// Opens a game. Pass <paramref name="resumeFrom"/> to continue a saved
    /// session rather than start fresh.
    ///
    /// Cheap on purpose — backlog item 20. The actual controller build used
    /// to happen here, blocking the UI thread via
    /// <c>GameplayViewModel(...).GetAwaiter().GetResult()</c> internally. It
    /// now happens in <see cref="InitializeAsync"/>, which every caller must
    /// await before pushing this page.
    /// </summary>
    public GameplayPage(IGameMode gameMode, List<IPlayer> players,
                        TableTop.Hosting.Persistence.SessionSnapshot? resumeFrom = null)
    {
        InitializeComponent();
        _gameMode = gameMode;
        _players = players;
        _resumeFrom = resumeFrom;
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        _vm = await GameplayViewModel.CreateAsync(
            new Services.MauiNavigator(this), _gameMode, _players, _resumeFrom);
        BindingContext = _vm;

        // The engine announces the end (deck out, rounds done, or Quit) —
        // show final standings (plus quiz styles) once, then leave.
        //
        // CardTitle, not CurrentCard: the shared CardTurnGameViewModel
        // exposes a flattened string rather than the raw ICard the old
        // per-head implementation held. Confirmed CardTitle changes exactly
        // once per genuinely new card and never on a flip (Flip() only
        // raises CardBodyText and FlipButtonText) — the same property this
        // exact reasoning already established for WinUI's deal-in trigger.
        _vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GameplayViewModel.CardTitle))
                AnimateDealIn();
        };

        _vm.GameOver += async summary =>
        {
            if (_closing) return;
            _closing = true;
            await DisplayAlert("Game over", summary, "OK");
            await Navigation.PopToRootAsync();
        };
    }

    private bool _closing;

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Dispose();
    }

    private void OnUndoClicked(object sender, EventArgs e) => _vm.UndoLastTurn();
    private void OnLevelUpClicked(object sender, EventArgs e) => _vm.LevelUp();
    private void OnLevelDownClicked(object sender, EventArgs e) => _vm.LevelDown();
    private void OnSaveClicked(object sender, EventArgs e) => _vm.SaveSession();
    private void OnSkipClicked(object sender, EventArgs e) => _vm.Skip();
    private void OnCompleteClicked(object sender, EventArgs e) => _vm.Complete();

    /// <summary>
    /// The tabletop flip: squash the card to its edge, swap the face at the
    /// midpoint, expand.
    /// </summary>
    private async void OnFlipClicked(object sender, EventArgs e)
    {
        if (_animating || !_vm.HasBack) return;
        _animating = true;
        try
        {
            await CardFrame.ScaleXTo(0.02, 130);
            _vm.FlipCard();
            await CardFrame.ScaleXTo(1.0, 130);
        }
        catch { _vm.FlipCard(); }   // animation unavailable → still flip
        finally { _animating = false; }
    }

    private bool _animating;

    /// <summary>Deal-in: each new card fades and slides up onto the table.</summary>
    private async void AnimateDealIn()
    {
        try
        {
            CardFrame.Opacity = 0;
            CardFrame.TranslationY = 26;
            await Task.WhenAll(
                CardFrame.FadeTo(1, 200),
                CardFrame.TranslateTo(0, 0, 200));
        }
        catch { CardFrame.Opacity = 1; CardFrame.TranslationY = 0; }
    }
    private void OnChoiceAClicked(object sender, EventArgs e) => _vm.RecordChoice('A');
    private void OnChoiceBClicked(object sender, EventArgs e) => _vm.RecordChoice('B');
    private void OnChoiceCClicked(object sender, EventArgs e) => _vm.RecordChoice('C');
    private void OnChoiceDClicked(object sender, EventArgs e) => _vm.RecordChoice('D');

    private async void OnEndGameClicked(object sender, EventArgs e)
    {
        // An exception escaping an async void handler terminates the
        // process on Android; surface it instead.
        try
        {
            bool confirm = await DisplayAlert("End Game", "Are you sure you want to end this game?", "Yes", "No");
            if (!confirm) return;

            // The engine fires GameEnded → the GameOver handler above shows the
            // standings (including quiz styles) and pops. If the engine never
            // started (load error), just leave.
            if (!_closing)
            {
                _vm.Quit();
                if (!_closing) await Navigation.PopToRootAsync();   // load-error path
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Couldn't end the game", ex.Message, "OK");
        }
    }
}
