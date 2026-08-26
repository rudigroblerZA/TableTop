using TableTop.Hosting;
using SC = System.Console;
using CC = System.ConsoleColor;
using CK = System.ConsoleKey;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Events;

namespace TableTop.Console;

/// <summary>
/// Renders card-per-turn school mode events to the console.
/// Uses bright, encouraging language appropriate for classroom use.
/// Structurally identical to <see cref="ConsoleCardTurnRenderer"/> but with
/// school-specific display choices — separate class to avoid adding
/// school-specific branching into the general renderer.
/// </summary>
internal sealed class ConsoleSchoolRenderer
{
    private readonly ICardTurnController _controller;
    private readonly string              _gameTitle;
    private CardReadyEvent?              _currentCard;
    private bool                         _waitingForInput;
    private int                          _totalScore;

    public ConsoleSchoolRenderer(ICardTurnController controller, string gameTitle)
    {
        _controller = controller;
        _gameTitle  = gameTitle;

        controller.CardReady          += OnCardReady;
        controller.TurnResult         += OnTurnResult;
        controller.TurnSkipped        += OnTurnSkipped;
        controller.SkipAttempted      += OnSkipAttempted;
        controller.BreakCardDrawn     += OnBreakCardDrawn;
        controller.RewardCardDrawn    += OnRewardCardDrawn;
        controller.InspirationCardDrawn += OnInspirationDrawn;
        controller.NextTurnHint       += OnNextTurnHint;
        controller.GameEnded          += OnGameEnded;
        controller.GamePaused         += OnGamePaused;
        controller.SessionSaved       += OnSessionSaved;
    }

    public void RunBlocking()
    {
        _controller.Start();
        while (_controller.IsRunning || _waitingForInput)
        {
            if (!_waitingForInput) { System.Threading.Thread.Sleep(10); continue; }
            var input = ConsoleUi.Prompt("").Trim().ToLowerInvariant();
            HandleInput(input);
        }
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnCardReady(object? sender, CardReadyEvent e)
    {
        _currentCard = e;

        ConsoleUi.Clear();
        PrintSchoolHeader(e.PlayerName, e.Round);

        // Category colour
        var catColour = e.Category switch
        {
            "Tricky"      or "Agreement" => CC.Yellow,
            "Challenge"   or "Pronouns"  => CC.Magenta,
            "Tense"       or "Inferential" or "Adjective" => CC.Green,
            "Sentences"   or "Academic"  => CC.DarkYellow,
            "Author"                     => CC.Red,
            _                            => CC.Cyan,
        };

        // Category + difficulty badge
        SC.ForegroundColor = catColour;
        SC.Write($"  [{e.Category}]");
        SC.ForegroundColor = CC.DarkGray;
        SC.WriteLine($"  {e.Difficulty}");
        SC.ResetColor();
        SC.WriteLine();

        // Card title
        SC.ForegroundColor = CC.White;
        SC.WriteLine($"  {e.CardTitle}");
        SC.ResetColor();
        SC.WriteLine();

        // Strip every tag, not just <b>. This used to hand-roll a <b>-only
        // replace, which left the 2,490 <i> tags in the decks showing raw.
        var text = CardText.StripHtml(e.CardText);

        // Word-wrap and indent
        foreach (var line in WrapText(text, 62))
        {
            SC.ForegroundColor = CC.Gray;
            SC.WriteLine($"  {line}");
        }
        SC.ResetColor();
        SC.WriteLine();

        PrintSchoolPrompt();
        _waitingForInput = true;
    }

    private void OnTurnResult(object? sender, TurnResultEvent e)
    {
        _waitingForInput = false;

        var (icon, msg, colour) = e.Outcome switch
        {
            CardOutcome.Completed => ("✓", Encourage(), CC.Green),
            CardOutcome.Failed    => ("✗", "Keep going — you'll get it next time!", CC.Red),
            CardOutcome.Skipped   => ("→", "Moving on.", CC.DarkGray),
            _                     => ("·", string.Empty, CC.Gray),
        };

        _totalScore += e.ScoreDelta;

        SC.ForegroundColor = colour;
        SC.WriteLine($"\n  {icon}  {msg}");
        if (e.ScoreDelta != 0)
            SC.WriteLine($"     +{e.ScoreDelta} points");
        SC.ResetColor();
        SC.WriteLine();
    }

    private void OnTurnSkipped(object? sender, TurnSkippedEvent e)
    {
        _waitingForInput = false;
        SC.ForegroundColor = CC.DarkGray;
        SC.WriteLine($"\n  → Skipping {e.PlayerName}'s turn.");
        SC.ResetColor();
    }

    private void OnSkipAttempted(object? sender, SkipAttemptedEvent e)
    {
        if (e.IsFree)
        {
            SC.ForegroundColor = CC.DarkCyan;
            SC.WriteLine($"\n  → {e.PlayerName} skipped (free pass).");
        }
        else
        {
            SC.ForegroundColor = CC.DarkYellow;
            SC.WriteLine($"\n  → {e.PlayerName} skipped  ({e.Penalty} pts).");
        }
        SC.ResetColor();
    }

    private void OnBreakCardDrawn(object? sender, BreakCardDrawnEvent e)
    {
        _waitingForInput = false;
        SC.ForegroundColor = CC.Cyan;
        SC.WriteLine($"\n  ☕  BREAK!  {e.CardTitle}");
        SC.ForegroundColor = CC.Gray;
        SC.WriteLine($"  {CardText.StripHtml(e.CardText)}");
        if (e.DurationMinutes.HasValue)
            SC.WriteLine($"  (about {e.DurationMinutes} minutes)");
        SC.ResetColor();
        ConsoleUi.PressEnterToContinue();
    }

    private void OnRewardCardDrawn(object? sender, RewardCardDrawnEvent e)
    {
        _waitingForInput = false;
        SC.ForegroundColor = CC.Yellow;
        SC.WriteLine($"\n  ★  REWARD!  {e.CardTitle}");
        SC.ForegroundColor = CC.Gray;
        SC.WriteLine($"  {e.EffectDescription}");
        SC.ResetColor();
        ConsoleUi.PressEnterToContinue();
    }

    private void OnInspirationDrawn(object? sender, InspirationCardDrawnEvent e)
    {
        _waitingForInput = false;
        SC.ForegroundColor = CC.DarkCyan;
        SC.WriteLine($"\n  💡  Saved for {e.PlayerName}: {e.InspirationText}");
        SC.ResetColor();
    }

    private void OnNextTurnHint(object? sender, NextTurnHintEvent e)
    {
        var colour = e.Urgency switch
        {
            "Strong"   => CC.Cyan,
            "Moderate" => CC.DarkCyan,
            _          => CC.DarkGray,
        };
        SC.ForegroundColor = colour;
        SC.WriteLine($"\n  💡  {e.HintText}");
        SC.ResetColor();
    }

    private void OnGamePaused(object? sender, GamePausedEvent e)
    {
        _waitingForInput = false;
        if (e.IsPaused)
        {
            SC.ForegroundColor = CC.DarkYellow;
            SC.WriteLine("\n  ⏸  Game paused. Press ENTER to continue.");
            SC.ResetColor();
            SC.ReadLine();
            _controller.TogglePause();
        }
    }

    private void OnSessionSaved(object? sender, SessionSavedEvent e)
    {
        SC.ForegroundColor = CC.DarkGray;
        SC.WriteLine($"  💾  Saved ({e.SavedAt:HH:mm})");
        SC.ResetColor();
    }

    private void OnGameEnded(object? sender, GameEndedEvent e)
    {
        _waitingForInput = false;

        ConsoleUi.Clear();
        ConsoleUi.Banner();
        SC.ForegroundColor = CC.Yellow;
        SC.WriteLine($"\n  🎉  {_gameTitle.ToUpperInvariant()} COMPLETE!\n");

        foreach (var standing in e.FinalStandings)
        {
            SC.ForegroundColor = CC.Cyan;
            SC.Write($"  {standing.Name,-20}");
            SC.ForegroundColor = CC.Yellow;
            SC.WriteLine($"  ⭐ {standing.Score} points");
        }

        SC.ResetColor();
        SC.WriteLine();
        SC.ForegroundColor = CC.Green;
        SC.WriteLine("  Fantastic effort from everyone! Well done! 🌟");
        SC.ResetColor();
        SC.WriteLine();
        ConsoleUi.PressEnterToContinue();
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    private void HandleInput(string input)
    {
        _waitingForInput = false;
        switch (input)
        {
            case "c" or "":
                _controller.RecordOutcome(CardOutcome.Completed);
                break;
            case "s":
                _controller.RecordOutcome(CardOutcome.Skipped);
                break;
            case "f":
                _controller.RecordOutcome(CardOutcome.Failed);
                break;
            case "p":
                _controller.TogglePause();
                break;
            case "+" or "harder":
                if (_controller.SupportsFlow && _currentCard?.Player is not null)
                    _controller.LevelUp(_currentCard.Player.Id);
                _waitingForInput = true;
                break;
            case "-" or "easier":
                if (_controller.SupportsFlow && _currentCard?.Player is not null)
                    _controller.LevelDown(_currentCard.Player.Id);
                _waitingForInput = true;
                break;
            case "r":
                if (_controller.SupportsFlow && _currentCard?.Player is not null)
                    _controller.ResetFlow(_currentCard.Player.Id);
                _waitingForInput = true;
                break;
            case "save":
                _ = _controller.SaveAsync();
                _waitingForInput = true;
                break;
            case "q":
                if (ConsoleUi.PromptYesNo("End the game?"))
                    _controller.Quit();
                else
                {
                    if (_currentCard is not null) PrintSchoolPrompt();
                    _waitingForInput = true;
                }
                break;
            default:
                ConsoleUi.PrintError("Enter: c (correct)  s (skip)  f (failed)  + (harder)  - (easier)  save  q (quit)");
                _waitingForInput = true;
                break;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void PrintSchoolHeader(string playerName, int round)
    {
        SC.ForegroundColor = CC.Cyan;
        SC.Write($"  🎓  {_gameTitle}");
        SC.ForegroundColor = CC.DarkGray;
        SC.Write($"  ·  Round {round}");
        SC.ForegroundColor = CC.Cyan;
        SC.WriteLine($"  ·  {playerName}'s turn");
        SC.ResetColor();
        SC.WriteLine(new string('─', 68));
        SC.WriteLine();
    }

    private void PrintSchoolPrompt()
    {
        SC.ForegroundColor = CC.DarkGray;
        SC.WriteLine("  [ENTER / c] Correct   [s] Skip   [f] Didn't get it   [+/-] Difficulty   [q] Quit");
        SC.ResetColor();
    }

    private static string Encourage()
    {
        var messages = new[]
        {
            "Excellent! 🌟",
            "Brilliant! Well done!",
            "Fantastic! Keep it up! 🎉",
            "Perfect! Great work!",
            "Superb! You nailed it! ⭐",
            "Outstanding! 🏆",
            "Spot on! Great answer!",
            "Wonderful! Full marks! ✓",
        };
        return messages[Random.Shared.Next(messages.Length)];
    }

    private static IEnumerable<string> WrapText(string text, int width)
    {
        foreach (var paragraph in text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(paragraph)) { yield return ""; continue; }
            var words = paragraph.Split(' ');
            var line  = "";
            foreach (var word in words)
            {
                if (line.Length + word.Length + 1 > width && line.Length > 0)
                { yield return line.TrimEnd(); line = ""; }
                line += word + " ";
            }
            if (line.Trim().Length > 0) yield return line.TrimEnd();
        }
    }
}
