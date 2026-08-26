using SC = System.Console;
using CC = System.ConsoleColor;
using CK = System.ConsoleKey;
using TableTop.Core.Abstractions.Cards;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Events;

namespace TableTop.Console;

/// <summary>
/// Renders Millionaire events to the console. Zero game logic.
/// </summary>
internal sealed class ConsoleMillionaireRenderer
{
    private readonly IMillionaireController _controller;
    private QuestionReadyEvent?             _currentQuestion;
    private bool                            _awaitingConfirm;
    private AnswerLabel?                    _pendingAnswer;
    private bool                            _waitingForInput;

    public ConsoleMillionaireRenderer(IMillionaireController controller)
    {
        _controller = controller;

        controller.HotSeatBegan   += OnHotSeatBegan;
        controller.QuestionReady  += OnQuestionReady;
        controller.LifelineUsed   += OnLifelineUsed;
        controller.AnswerCorrect  += OnAnswerCorrect;
        controller.AnswerWrong    += OnAnswerWrong;
        controller.WalkedAway     += OnWalkedAway;
        controller.MillionaireWon += OnMillionaireWon;
        controller.GameEnded      += OnGameEnded;
    }

    public void RunBlocking()
    {
        _controller.Start();
        while (_controller.IsRunning || _waitingForInput)
        {
            if (!_waitingForInput) { System.Threading.Thread.Sleep(10); continue; }
            var raw = SC.ReadLine();
            if (raw is null) { ConsoleUi.NoteInputEnded(); return; }
            var input = raw.Trim().ToUpperInvariant();
            HandleInput(input);
        }
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnHotSeatBegan(object? sender, HotSeatBeganEvent e)
    {
        ConsoleUi.Clear();
        ConsoleUi.Banner();
        SC.ForegroundColor = CC.Yellow;
        SC.WriteLine($"\n  ★  {e.PlayerName.ToUpperInvariant()}, come on down!");
        SC.WriteLine($"     Player {e.PlayerIndex + 1} of {e.TotalPlayers}");
        SC.ResetColor();
        ConsoleUi.PressEnterToContinue();
    }

    private void OnQuestionReady(object? sender, QuestionReadyEvent e)
    {
        _currentQuestion = e;
        _awaitingConfirm = false;
        _pendingAnswer   = null;

        ConsoleUi.Clear();
        ConsoleUi.Banner();
        PrintMillionaireSidebar(e);
        ConsoleUi.PrintMessage("  [A/B/C/D] Answer   [1] 50:50   [2] Phone   [3] Audience   [W] Walk away");
        ConsoleUi.PrintPromptMarker(">");  // backlog: was silently eating one real input line
        _waitingForInput = true;
    }

    private void OnLifelineUsed(object? sender, LifelineUsedEvent e)
    {
        SC.ForegroundColor = CC.Magenta;
        foreach (var line in e.Narrative.Split('\n'))
            SC.WriteLine($"  {line}");
        SC.ResetColor();
        SC.WriteLine();
    }

    private void OnAnswerCorrect(object? sender, AnswerCorrectEvent e)
    {
        _waitingForInput = false;
        SC.ForegroundColor = CC.Green;
        SC.WriteLine($"\n  ✓ CORRECT!  Won £{e.PrizeWon:N0}!");
        if (e.SafeHavenReached)
            SC.WriteLine($"  ✦ Safe haven — £{e.GuaranteedPrize:N0} guaranteed.");
        SC.ResetColor();
        ConsoleUi.PressEnterToContinue();
    }

    private void OnAnswerWrong(object? sender, AnswerWrongEvent e)
    {
        _waitingForInput = false;
        SC.ForegroundColor = CC.Red;
        SC.WriteLine($"\n  ✗ WRONG!  Answer was {e.CorrectLabel}: {e.CorrectText}");
        SC.ForegroundColor = CC.Yellow;
        SC.WriteLine($"  You leave with £{e.GuaranteedPrize:N0}.");
        SC.ResetColor();
        ConsoleUi.PressEnterToContinue();
    }

    private void OnWalkedAway(object? sender, WalkedAwayEvent e)
    {
        _waitingForInput = false;
        SC.ForegroundColor = CC.Yellow;
        SC.WriteLine($"\n  You walked away with £{e.Prize:N0}!");
        SC.ResetColor();
        ConsoleUi.PressEnterToContinue();
    }

    private void OnMillionaireWon(object? sender, MillionaireWonEvent e)
    {
        _waitingForInput = false;
        SC.ForegroundColor = CC.Yellow;
        SC.WriteLine($"\n  🎉  {e.PlayerName} IS A MILLIONAIRE!");
        SC.ResetColor();
        ConsoleUi.PressEnterToContinue();
    }

    private void OnGameEnded(object? sender, MillionaireGameEndedEvent e)
    {
        _waitingForInput = false;
        ConsoleUi.SectionHeader("FINAL RESULTS");
        foreach (var r in e.Results)
        {
            SC.ForegroundColor = CC.Yellow;
            SC.WriteLine($"  {r.PlayerName,-20} £{r.Prize:N0}");
        }
        SC.ResetColor();
        ConsoleUi.PressEnterToContinue();
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    private void HandleInput(string input)
    {
        _waitingForInput = false;

        if (_awaitingConfirm)
        {
            if (input is "Y" or "YES") { _controller.SubmitAnswer(_pendingAnswer!.Value); return; }
            if (input is "N" or "NO")  { _awaitingConfirm = false; _pendingAnswer = null; PrintCurrentQuestion(); return; }
        }

        switch (input)
        {
            case "A": AttemptAnswer(AnswerLabel.A); break;
            case "B": AttemptAnswer(AnswerLabel.B); break;
            case "C": AttemptAnswer(AnswerLabel.C); break;
            case "D": AttemptAnswer(AnswerLabel.D); break;
            case "1": _controller.UseLifeline(0); _waitingForInput = true; break;
            case "2": _controller.UseLifeline(1); _waitingForInput = true; break;
            case "3": _controller.UseLifeline(2); _waitingForInput = true; break;
            case "W": _controller.WalkAway(); break;
            default:
                ConsoleUi.PrintError("Invalid input.");
                _waitingForInput = true;
                break;
        }
    }

    private void AttemptAnswer(AnswerLabel label)
    {
        if (!_controller.AvailableOptions.Contains(label))
        {
            ConsoleUi.PrintError("That answer was eliminated.");
            _waitingForInput = true;
            return;
        }
        _pendingAnswer   = label;
        _awaitingConfirm = true;
        SC.Write($"  Final answer — {label}: {_currentQuestion?.Answers[label]}? (y/n): ");
        _waitingForInput = true;
    }

    private void PrintCurrentQuestion()
    {
        if (_currentQuestion is null) return;
        ConsoleUi.Clear();
        ConsoleUi.Banner();
        PrintMillionaireSidebar(_currentQuestion);
        ConsoleUi.PrintPromptMarker(">");  // backlog: was silently eating one real input line
        _waitingForInput = true;
    }

    private static void PrintMillionaireSidebar(QuestionReadyEvent e)
    {
        SC.ForegroundColor = CC.DarkYellow;
        SC.WriteLine($"\n  Q — £{e.Ladder.Rungs[e.Ladder.CurrentIndex].PrizeAmount:N0}  |  Guaranteed: £{e.Ladder.GuaranteedPrize:N0}");
        SC.ForegroundColor = CC.White;
        SC.WriteLine($"\n  {e.QuestionText}\n");
        foreach (var (label, text) in e.Answers.OrderBy(kv => kv.Key))
        {
            var available = e.AvailableOptions.Contains(label);
            SC.ForegroundColor = available ? CC.Cyan : CC.DarkGray;
            SC.WriteLine($"    {label}: {(available ? text : "——")}");
        }
        SC.ForegroundColor = CC.DarkGray;
        SC.WriteLine();
        foreach (var l in e.Lifelines)
        {
            SC.ForegroundColor = l.IsAvailable ? CC.Green : CC.DarkGray;
            SC.Write($"  [{l.Name}{(l.IsAvailable ? "" : " ✗")}]");
        }
        SC.ResetColor();
        SC.WriteLine("\n");
    }
}
