using System.Collections.ObjectModel;
using System.Windows.Input;
using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;
using TableTop.Hosting.Events;
using TableTop.Presentation.Infrastructure;

namespace TableTop.Presentation.ViewModels;

/// <summary>
/// The Monogamy screen, shared by every head.
///
/// <para>
/// Merging the two implementations turned up a live bug in WinUI. MAUI had
/// already found and fixed it; WinUI never received the fix, because there was
/// no mechanism by which it could. See <see cref="Submit"/> for the detail —
/// in short, WinUI's commands read
/// <c>if (HasCard) { _controller.CompleteCard(); HasCard = false; }</c>, and
/// that trailing assignment wipes the state the *next* card's
/// <c>CardReady</c> has already set, because the controller is fully
/// synchronous. Every action is gated on <c>HasCard</c>, so the screen froze
/// on the first non-doubles turn — five rolls in six.
/// </para>
///
/// <para>
/// The surface is the union of both heads: WinUI's <see cref="ICommand"/> shape
/// (MAUI binds commands perfectly well and gains a cleaner page), plus MAUI's
/// <see cref="Scores"/>, <see cref="LoadError"/> and <see cref="HasLoadError"/>,
/// which WinUI never had. <c>Zone</c> is <see cref="MonogamyZone"/> rather than
/// MAUI's <c>string</c> — the drift that made this pair worth merging first.
/// </para>
/// </summary>
public sealed class MonogamyGameViewModel : ViewModelBase, IDisposable
{
    private readonly IMonogamyController? _controller;
    private readonly Dictionary<string, int> _tokenTotals = [];

    /// <summary>Zones offered after doubles; empty otherwise.</summary>
    public ObservableCollection<ZoneOption> ZoneChoices { get; } = [];

    /// <summary>Returns to the previous screen.</summary>
    public ICommand BackCommand      { get; }
    /// <summary>Completes the current card (both partners acted).</summary>
    public ICommand CompleteCommand  { get; }
    /// <summary>Skips the current card — free in Monogamy.</summary>
    public ICommand SkipCommand      { get; }
    /// <summary>Marks the card negotiated (played with modifications).</summary>
    public ICommand NegotiateCommand { get; }

    private string _playerName = "", _diceText = "", _cardTitle = "", _cardText = "";
    private string _tokenText = "", _flash = "", _scores = "", _summary = "";
    private readonly string _loadError = "";
    private MonogamyZone _zone;
    private bool _awaitingZone, _hasCard, _isGameOver;

    /// <summary>Whose turn it is.</summary>
    public string PlayerName { get => _playerName; private set => SetField(ref _playerName, value); }
    /// <summary>The last dice roll, rendered for display.</summary>
    public string DiceText   { get => _diceText;   private set => SetField(ref _diceText, value); }
    /// <summary>Title of the current card.</summary>
    public string CardTitle  { get => _cardTitle;  private set => SetField(ref _cardTitle, value); }
    /// <summary>Body text of the current card, resolved for the current player.</summary>
    public string CardText   { get => _cardText;   private set => SetField(ref _cardText, value); }
    /// <summary>Token standings, rendered for display.</summary>
    public string TokenText  { get => _tokenText;  private set => SetField(ref _tokenText, value); }
    /// <summary>Transient feedback after an action.</summary>
    public string Flash      { get => _flash;      private set => SetField(ref _flash, value); }
    /// <summary>End-of-game summary.</summary>
    public string Summary    { get => _summary;    private set => SetField(ref _summary, value); }

    /// <summary>
    /// Per-player token totals. Was MAUI-only; WinUI showed nothing equivalent.
    /// </summary>
    public string Scores
    {
        get => _scores;
        private set { SetField(ref _scores, value); OnPropertyChanged(nameof(HasScores)); }
    }

    /// <summary>True once any tokens have been awarded.</summary>
    public bool HasScores => _scores.Length > 0;

    /// <summary>
    /// The current zone, typed. WinUI exposed <see cref="MonogamyZone"/> and MAUI
    /// exposed a string, so a page binding against MAUI's version was comparing
    /// display text — the drift this merge exists to remove.
    ///
    /// The controller's events carry the zone as a string, so it is parsed here,
    /// once, rather than in each head.
    /// </summary>
    public MonogamyZone Zone { get => _zone; private set => SetField(ref _zone, value); }

    /// <summary>The zone as text, for heads that bind a label directly.</summary>
    public string ZoneName => _zone.ToString();

    private void SetZone(string raw)
    {
        Zone = Enum.TryParse<MonogamyZone>(raw, ignoreCase: true, out var z) ? z : MonogamyZone.Foreplay;
        OnPropertyChanged(nameof(ZoneName));
    }

    /// <summary>True while the table is choosing a zone after doubles.</summary>
    public bool AwaitingZone { get => _awaitingZone; private set => SetField(ref _awaitingZone, value); }

    /// <summary>
    /// True while a card is face up and actionable.
    ///
    /// Owned exclusively by <see cref="OnCardReady"/>. Nothing else may write it
    /// — see <see cref="Submit"/>.
    /// </summary>
    public bool HasCard { get => _hasCard; private set => SetField(ref _hasCard, value); }

    /// <summary>True once the session has ended.</summary>
    public bool IsGameOver
    {
        get => _isGameOver;
        private set { SetField(ref _isGameOver, value); OnPropertyChanged(nameof(IsPlaying)); }
    }

    /// <summary>True while the session is live and loadable.</summary>
    public bool IsPlaying => !IsGameOver && !HasLoadError;

    /// <summary>Deck-load failure message, or empty. Was MAUI-only.</summary>
    public string LoadError => _loadError;

    /// <summary>True when the deck could not be loaded.</summary>
    public bool HasLoadError => !string.IsNullOrEmpty(_loadError);

    /// <summary>
    /// Builds the screen around an already-created controller.
    ///
    /// Controller-injected rather than mode-and-players, because WinUI's
    /// factory already builds the controller centrally and passing it in keeps
    /// that single construction path. MAUI built its own inside the ViewModel;
    /// <see cref="Create"/> preserves that entry point without duplicating the
    /// screen.
    /// </summary>
    /// <param name="navigator">Used to leave the screen.</param>
    /// <param name="controller">A started or unstarted Monogamy controller.</param>
    public MonogamyGameViewModel(INavigator navigator, IMonogamyController controller)
    {
        _controller = controller;

        BackCommand      = new RelayCommand(() => { _controller?.Quit(); navigator.GoBack(); });
        CompleteCommand  = new RelayCommand(() => Submit(() => _controller?.CompleteCard()),  () => HasCard);
        SkipCommand      = new RelayCommand(() => Submit(() => _controller?.SkipCard()),      () => HasCard);
        NegotiateCommand = new RelayCommand(() => Submit(() => _controller?.NegotiateCard()), () => HasCard);

        _controller.DiceRolled    += OnDiceRolled;
        _controller.DoublesRolled += OnDoublesRolled;
        _controller.CardReady     += OnCardReady;
        _controller.TokensAwarded += OnTokensAwarded;
        _controller.GameEnded     += OnGameEnded;

        if (!_controller.IsRunning) _controller.Start();
    }

    /// <summary>
    /// Error-state constructor: no controller, just a message.
    /// </summary>
    private MonogamyGameViewModel(INavigator navigator, string loadError)
    {
        _loadError       = loadError;
        BackCommand      = new RelayCommand(navigator.GoBack);
        CompleteCommand  = new RelayCommand(() => { }, () => false);
        SkipCommand      = new RelayCommand(() => { }, () => false);
        NegotiateCommand = new RelayCommand(() => { }, () => false);
    }

    /// <summary>
    /// Builds the controller from a mode, surfacing a deck-load failure as a
    /// message rather than an exception.
    ///
    /// MAUI did this and WinUI did not: a missing or malformed deck took the
    /// WinUI app down instead of showing why. Shared, so both behave the same.
    /// </summary>
    public static MonogamyGameViewModel Create(
        INavigator             navigator,
        IGameMode              mode,
        IReadOnlyList<IPlayer> players,
        int                    winningTokenCount = 10)
    {
        try
        {
            var deck = mode is IMonogamyDeckProvider p
                ? p.GetDeck()
                : throw new NotSupportedException($"'{mode.Name}' provides no Monogamy deck.");

            return new MonogamyGameViewModel(
                navigator, new MonogamyController(players, deck, winningTokenCount));
        }
        catch (Exception ex)
        {
            return new MonogamyGameViewModel(navigator, ex.Message);
        }
    }

    // ── Controller events ─────────────────────────────────────────────────────

    private void OnDiceRolled(object? sender, DiceRolledEvent e)
    {
        PlayerName = e.PlayerName;
        DiceText   = $"{e.Die1} + {e.Die2} = {e.Total}";
        SetZone(e.ResultingZone);
    }

    private void OnDoublesRolled(object? sender, DoublesRolledEvent e)
    {
        AwaitingZone = true;
        ZoneChoices.Clear();
        // Enum.GetValues, not a hardcoded list — a hardcoded one in both heads is
        // what left the Fantasy zone missing from the picker when it was added.
        foreach (var z in Enum.GetValues<MonogamyZone>())
            ZoneChoices.Add(new ZoneOption(z, this));
        Flash = $"Doubles! {e.PlayerName} chooses the zone.";
    }

    private void OnCardReady(object? sender, MonogamyCardReadyEvent e)
    {
        AwaitingZone = false;
        ZoneChoices.Clear();
        CardTitle = e.CardTitle;
        CardText  = e.CardText;
        SetZone(e.Zone);
        HasCard   = true;
        RaiseActionCommands();
    }

    private void OnTokensAwarded(object? sender, TokensAwardedEvent e)
    {
        _tokenTotals[e.PlayerName] = e.TotalTokens;
        TokenText = $"{e.PlayerName}: {e.TotalTokens} tokens";
        Flash     = e.TokensEarned > 0 ? $"+{e.TokensEarned} to {e.PlayerName}" : "";
        Scores    = string.Join("   ·   ", _tokenTotals.Select(kv => $"{kv.Key} {kv.Value}"));
    }

    private void OnGameEnded(object? sender, MonogamyGameEndedEvent e)
    {
        IsGameOver = true;
        HasCard    = false;
        Summary    = e.WinnerName is { Length: > 0 }
            ? $"{e.WinnerName} wins after {e.TotalRounds} rounds."
            : "Session ended.";
        RaiseActionCommands();
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    /// <summary>Chooses a zone after doubles. Ignored unless a choice is pending.</summary>
    public void ChooseZone(MonogamyZone zone)
    {
        if (!AwaitingZone) return;
        _controller?.ChooseZone(zone);
    }

    private bool _submitting;

    /// <summary>
    /// Runs an action once, and does NOT touch <see cref="HasCard"/> afterwards.
    ///
    /// <para>
    /// This is the whole reason the two implementations differed in behaviour.
    /// The obvious shape — <c>if (HasCard) { controller.Complete(); HasCard =
    /// false; }</c>, which is what WinUI still had — looks like a submit-once
    /// guard and is actually a bug: <see cref="IMonogamyController"/> is fully
    /// synchronous, so by the time <c>CompleteCard</c> returns it has already
    /// run RecordOutcome → AdvanceToNextPlayer → BeginTurn → DiceRolled →
    /// DrawCard → CardReady, and <see cref="OnCardReady"/> has already set
    /// <c>HasCard = true</c> for the NEXT card. The trailing assignment then
    /// wipes it.
    /// </para>
    ///
    /// <para>
    /// Effect: on the first non-doubles turn — five rolls in six — the card and
    /// its buttons vanish and never return, because every action is gated on
    /// <c>HasCard</c>. Millionaire and Day One call their controllers and let
    /// the events own the state, which is why only this mode was affected.
    /// </para>
    /// </summary>
    private void Submit(Action action)
    {
        if (!HasCard || _submitting) return;
        _submitting = true;
        try     { action(); }
        finally { _submitting = false; RaiseActionCommands(); }
    }

    private void RaiseActionCommands()
    {
        (CompleteCommand  as RelayCommand)?.RaiseCanExecuteChanged();
        (SkipCommand      as RelayCommand)?.RaiseCanExecuteChanged();
        (NegotiateCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    // Plain methods alongside the ICommands above, same duality as
    // Millionaire's AnswerOption/LifelineOption: WinUI binds the ICommands,
    // MAUI's code-behind (MonogamyGamePage.xaml.cs) calls these directly. Both
    // route through the same Submit() gate, so neither path can double-submit
    // or skip the re-entrancy guard the other has.

    /// <summary>Completes the current card. No-op when nothing is waiting.</summary>
    public void Complete() => Submit(() => _controller?.CompleteCard());

    /// <summary>Skips the current card. No-op when nothing is waiting.</summary>
    public void Skip() => Submit(() => _controller?.SkipCard());

    /// <summary>Marks the card negotiated. No-op when nothing is waiting.</summary>
    public void Negotiate() => Submit(() => _controller?.NegotiateCard());

    /// <inheritdoc />
    public void Dispose() => _controller?.Dispose();

    /// <summary>One selectable intimacy zone.</summary>
    public sealed class ZoneOption
    {
        private readonly MonogamyGameViewModel _owner;

        /// <summary>The zone value.</summary>
        public MonogamyZone Zone { get; }

        /// <summary>Display name.</summary>
        public string Display => Zone.ToString();

        internal ZoneOption(MonogamyZone zone, MonogamyGameViewModel owner)
        {
            Zone = zone; _owner = owner;
        }

        /// <summary>Selects this zone.</summary>
        public void Invoke() => _owner.ChooseZone(Zone);
    }
}
