using TableTop.Core.Abstractions.Scoring;
using TableTop.Hosting;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Events;
using CC = System.ConsoleColor;
using SC = System.Console;

namespace TableTop.Console;

/// <summary>
/// Subscribes to <see cref="ICardTurnController"/> events and renders them to the console.
/// Contains zero game logic — it only reads events and prints.
/// </summary>
internal sealed class ConsoleCardTurnRenderer
{
    private readonly ICardTurnController _controller;
    private readonly string _gameTitle;
    private CardReadyEvent? _currentCard;
    private bool _waitingForInput;

    public ConsoleCardTurnRenderer(ICardTurnController controller, string gameTitle)
    {
        _controller = controller;
        _gameTitle = gameTitle;

        controller.CardReady += OnCardReady;
        controller.FlowChanged += OnFlowChanged;
        controller.NextTurnHint += OnNextTurnHint;
        controller.TurnResult += OnTurnResult;
        controller.TurnSkipped += OnTurnSkipped;
        controller.TurnUndone += OnTurnUndone;
        controller.GameEnded += OnGameEnded;
        controller.GamePaused += OnGamePaused;
    }

    /// <summary>Starts the controller and blocks until the game ends.</summary>
    public void RunBlocking()
    {
        _controller.Start();

        // Block on input loop until the game is over
        while (_controller.IsRunning)
        {
            if (!_waitingForInput)
            {
                System.Threading.Thread.Sleep(10);
                continue;
            }

            var raw = SC.ReadLine();
            if (raw is null)
            {
                // EOF. Without this the loop treated a dead stream as an
                // unrecognised command and re-prompted forever, so the app
                // could not be piped input or smoke-tested in CI.
                ConsoleUi.NoteInputEnded();
                _controller.Quit();
                return;
            }
            var input = raw.Trim().ToLowerInvariant();
            HandleInput(input);
        }
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnCardReady(object? sender, CardReadyEvent e)
    {
        _currentCard = e;
        _playerIds = _playerIds.Length == 0 && e.Player is not null
            ? [e.Player.Id]
            : _playerIds;

        ConsoleUi.Clear();
        ConsoleUi.Banner();
        ConsoleUi.PrintRoundHeader(e.Round, e.PlayerName);
        // Card text carries <b>/<i> markup for the graphical heads. The console
        // has no rich text, and leaving the tags in also breaks the card box:
        // the border is padded to a fixed width from the string length.
        ConsoleUi.PrintCard(
            e.Category, e.CardTitle, CardText.StripHtml(e.CardText), e.Difficulty, e.Restriction);
        SC.WriteLine();
        PrintOutcomePrompt();
        ConsoleUi.PrintPromptMarker(">");  // backlog: was silently eating one real input line
        _waitingForInput = true;
    }

    private void OnTurnResult(object? sender, TurnResultEvent e)
    {
        _waitingForInput = false;
        ConsoleUi.PrintTurnResult(e.PlayerName, e.Outcome.ToString(), e.ScoreDelta);
        ConsoleUi.PressEnterToContinue();
    }

    private static void OnTurnSkipped(object? sender, TurnSkippedEvent e)
    {
        ConsoleUi.PrintSkippedTurn(e.PlayerName, e.Reason);
    }

    private static void OnTurnUndone(object? sender, TurnUndoneEvent e)
    {
        // TurnUndone re-presents the reversed card via a fresh CardReady, which
        // sets _waitingForInput itself — this handler only prints the outcome.
        ConsoleUi.PrintTurnUndone(e.PlayerName, e.CardTitle, e.ScoreRestored);
    }

    private void OnGameEnded(object? sender, GameEndedEvent e)
    {
        _waitingForInput = false;
        ConsoleUi.PrintMessage($"{_gameTitle} complete!");
        ConsoleUi.PrintFinalStandings(
            e.FinalStandings.Select(s => (s.Name, s.Score)),
            e.TotalRounds);
    }

    private void FlowAll(Action<Guid> action, string label)
    {
        if (!_controller.SupportsFlow)
        {
            ConsoleUi.PrintError("Flow control not available with this progression strategy.");
            _waitingForInput = true;
            return;
        }
        foreach (var id in _playerIds)
            action(id);
        ConsoleUi.PrintMessage($"{label} applied to all {_playerIds.Length} player(s).");
        _waitingForInput = true;
    }

    private Guid[] _playerIds = [];

    private void OnGamePaused(object? sender, GamePausedEvent e)
    {
        if (e.IsPaused)
        {
            ConsoleUi.PrintMessage("Paused — press ENTER to resume.");
            SC.ReadLine();
            _controller.TogglePause();
        }
    }

    private static void OnFlowChanged(object? sender, TableTop.Hosting.Events.FlowChangedEvent e)
    {
        SC.ForegroundColor = CC.DarkCyan;
        SC.WriteLine($"\n  ⇄  {e.PlayerName}: {e.Change} → {e.NewDifficulty} · {e.NewPace}");
        SC.ResetColor();
    }

    private static void OnNextTurnHint(object? sender, TableTop.Hosting.Events.NextTurnHintEvent e)
    {
        SC.ForegroundColor = e.Urgency switch
        {
            "Strong" => CC.Cyan,
            "Moderate" => CC.DarkCyan,
            _ => CC.DarkGray,
        };
        SC.WriteLine($"\n  💡  {e.HintText}");
        SC.ResetColor();
    }

    // ── Input dispatch ────────────────────────────────────────────────────────

    private void HandleInput(string input)
    {
        _waitingForInput = false;
        switch (input)
        {
            case "c": _controller.RecordOutcome(CardOutcome.Completed); break;
            case "s": _controller.RecordOutcome(CardOutcome.Skipped); break;
            case "f": _controller.RecordOutcome(CardOutcome.Failed); break;
            case "p": _controller.TogglePause(); break;
            case "u":
                // UndoLastTurn returns false rather than throwing when there is
                // nothing to reverse, so a stray 'u' at the start of a session
                // is a quiet no-op — same as WinUI and MAUI, whose Undo button
                // is simply disabled until a turn exists.
                if (!_controller.UndoLastTurn())
                {
                    ConsoleUi.PrintError("Nothing to undo yet.");
                    _waitingForInput = true;
                }
                break;
            case "q":
                if (ConsoleUi.PromptYesNo("Quit?"))
                    _controller.Quit();
                else
                {
                    if (_currentCard is not null) PrintOutcomePrompt();
                    _waitingForInput = true;
                }
                break;

            // ── Flow commands (only active when strategy supports flow) ───────
            case "+":
            case "lu":
                FlowAll(id => _controller.LevelUp(id), "Level Up");
                break;
            case "-":
            case "ld":
                FlowAll(id => _controller.LevelDown(id), "Level Down");
                break;
            case ">":
            case "su":
                FlowAll(id => _controller.SpeedUp(id), "Speed Up");
                break;
            case "<":
            case "sd":
                FlowAll(id => _controller.SlowDown(id), "Slow Down");
                break;
            case "r":
                FlowAll(id => _controller.ResetFlow(id), "Reset Flow");
                break;
            case "save":
                _ = _controller.SaveAsync();
                break;

            default:
                ConsoleUi.PrintError("Enter c, s, f, u (undo), p, q, save, +/- (level), >/<(pace), r (reset).");
                _waitingForInput = true;
                break;
        }
    }

    private void PrintOutcomePrompt()
    {
        ConsoleUi.PrintMessage("  [c] Completed   [s] Skipped   [f] Failed   [u] Undo   [p] Pause   [q] Quit   [save] Save");
        if (_controller.SupportsFlow)
            ConsoleUi.PrintMessage("  [+] Level Up   [-] Level Down   [>] Faster   [<] Slower   [r] Reset");
    }
}
