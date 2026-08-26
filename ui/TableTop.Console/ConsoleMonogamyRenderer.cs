using TableTop.Hosting;
using SC = System.Console;
using CC = System.ConsoleColor;
using CK = System.ConsoleKey;
using TableTop.Core.Abstractions.Cards;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Events;

namespace TableTop.Console;

/// <summary>
/// Renders Monogamy events to the console. Zero game logic.
/// </summary>
internal sealed class ConsoleMonogamyRenderer
{
    private readonly IMonogamyController _controller;
    private bool                         _waitingForInput;
    private bool                         _awaitingZoneChoice;

    public ConsoleMonogamyRenderer(IMonogamyController controller)
    {
        _controller = controller;
        controller.DiceRolled    += OnDiceRolled;
        controller.DoublesRolled += OnDoublesRolled;
        controller.CardReady     += OnCardReady;
        controller.TokensAwarded += OnTokensAwarded;
        controller.GameEnded     += OnGameEnded;
    }

    public void RunBlocking()
    {
        _controller.Start();
        while (_controller.IsRunning || _waitingForInput)
        {
            if (!_waitingForInput) { System.Threading.Thread.Sleep(10); continue; }
            var input = ConsoleUi.Prompt(">").Trim().ToLowerInvariant();
            HandleInput(input);
        }
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnDiceRolled(object? sender, DiceRolledEvent e)
    {
        ConsoleUi.Clear();
        ConsoleUi.Banner();
        SC.ForegroundColor = CC.Cyan;
        SC.WriteLine($"\n  {e.PlayerName}'s turn");
        SC.ForegroundColor = CC.White;
        SC.WriteLine($"\n  🎲  {e.Die1}  +  {e.Die2}  =  {e.Total}");
        SC.ForegroundColor = CC.DarkGray;
        SC.WriteLine(e.IsDouble
            ? $"  Doubles! Choose your zone."
            : $"  Zone: {e.ResultingZone}");
        SC.ResetColor();
        SC.WriteLine();
    }

    private void OnDoublesRolled(object? sender, DoublesRolledEvent e)
    {
        _awaitingZoneChoice = true;
        ConsoleUi.PrintMessage("  Choose your zone:");
        ConsoleUi.PrintMessage("  [1] Foreplay   [2] Sensual   [3] Steamy   [4] Wild");
        _waitingForInput = true;
    }

    private void OnCardReady(object? sender, MonogamyCardReadyEvent e)
    {
        SC.ForegroundColor = CC.Magenta;
        SC.WriteLine($"  [{e.Zone}]  {e.CardTitle}  ★{e.TokenValue}");
        if (e.DurationMinutes.HasValue)
        {
            SC.ForegroundColor = CC.DarkGray;
            SC.WriteLine($"  ⏱ {e.DurationMinutes} minutes");
        }
        SC.ForegroundColor = CC.White;
        SC.WriteLine();
        foreach (var line in WrapText(CardText.StripHtml(e.CardText), 60))
            SC.WriteLine($"  {line}");
        SC.ResetColor();
        SC.WriteLine();
        ConsoleUi.PrintMessage("  [c] Complete   [n] Negotiate   [s] Skip   [q] Quit");
        _waitingForInput = true;
    }

    private void OnTokensAwarded(object? sender, TokensAwardedEvent e)
    {
        _waitingForInput = false;
        SC.ForegroundColor = CC.Yellow;
        SC.WriteLine($"\n  ★  {e.PlayerName} +{e.TokensEarned} token{(e.TokensEarned == 1 ? "" : "s")}  (total: {e.TotalTokens})");
        SC.ResetColor();
        ConsoleUi.PressEnterToContinue();
    }

    private void OnGameEnded(object? sender, MonogamyGameEndedEvent e)
    {
        _waitingForInput = false;
        ConsoleUi.Clear();
        ConsoleUi.Banner();
        SC.ForegroundColor = CC.Yellow;
        SC.WriteLine($"\n  🏆  {e.WinnerName} wins!\n");
        foreach (var s in e.FinalStandings)
        {
            SC.ForegroundColor = CC.White;
            SC.Write($"  {s.PlayerName,-20}");
            SC.ForegroundColor = CC.Yellow;
            SC.WriteLine($"  {s.Tokens} tokens  ({s.CardsCompleted} completed)");
        }
        SC.ResetColor();
        ConsoleUi.PressEnterToContinue();
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    private void HandleInput(string input)
    {
        _waitingForInput = false;

        if (_awaitingZoneChoice)
        {
            _awaitingZoneChoice = false;
            var zone = input switch
            {
                "1" or "foreplay" => MonogamyZone.Foreplay,
                "2" or "sensual"  => MonogamyZone.Sensual,
                "3" or "steamy"   => MonogamyZone.Steamy,
                "4" or "wild"     => MonogamyZone.Wild,
                _                 => (MonogamyZone?)null,
            };
            if (zone is null)
            {
                ConsoleUi.PrintError("Enter 1, 2, 3, or 4.");
                _awaitingZoneChoice = true;
                _waitingForInput    = true;
                return;
            }
            _controller.ChooseZone(zone.Value);
            return;
        }

        switch (input)
        {
            case "c": _controller.CompleteCard();  break;
            case "n": _controller.NegotiateCard(); break;
            case "s": _controller.SkipCard();      break;
            case "q":
                if (ConsoleUi.PromptYesNo("Quit Monogamy?"))
                    _controller.Quit();
                else
                    _waitingForInput = true;
                break;
            default:
                ConsoleUi.PrintError("Enter c, n, s, or q.");
                _waitingForInput = true;
                break;
        }
    }

    private static IEnumerable<string> WrapText(string text, int width)
    {
        // Line breaks in card text are meaningful — see ConsoleUi.WrapText.
        foreach (var paragraph in text.Replace("\r\n", "\n").Split('\n'))
        {
            if (paragraph.Length == 0) { yield return string.Empty; continue; }

            var line = string.Empty;
            foreach (var word in paragraph.Split(' '))
            {
                if ((line + word).Length > width)
                {
                    if (line.Length > 0) yield return line.TrimEnd();
                    line = string.Empty;
                }
                line += word + " ";
            }
            if (line.Length > 0) yield return line.TrimEnd();
        }
    }
}
