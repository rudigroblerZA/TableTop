using SC = System.Console;
using CC = System.ConsoleColor;
using TableTop.Hosting;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Events;

namespace TableTop.Console;

/// <summary>
/// Renders Day One events to the console. Zero game logic.
///
/// <para>
/// Unlike every other renderer here, a session doesn't end when the player
/// stops — the campaign persists across real days on its own. So declining
/// today's card isn't a "quit"; it just leaves it pending for next time, and
/// this renderer returns quietly rather than treating that as an early exit.
/// </para>
/// </summary>
internal sealed class ConsoleDayOneRenderer
{
    private readonly IDayOneController _controller;
    private DayReadyEvent? _dayReady;

    public ConsoleDayOneRenderer(IDayOneController controller)
    {
        _controller = controller;
        controller.DayReady         += (_, e) => _dayReady = e;
        controller.AllCaughtUp      += OnAllCaughtUp;
        controller.CampaignComplete += OnCampaignComplete;
    }

    public void RunBlocking()
    {
        _controller.Start();

        while (_controller.HasPendingCard)
        {
            if (_dayReady is not { } day) break;
            _dayReady = null;

            RenderDay(day);
            if (!ConsoleUi.PromptYesNo("  Mark today's card complete?"))
            {
                ConsoleUi.PrintMessage("  It'll be waiting for you next time.");
                return;
            }

            _controller.CompleteToday();
        }
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    private void RenderDay(DayReadyEvent e)
    {
        ConsoleUi.Clear();
        ConsoleUi.Banner();
        SC.ForegroundColor = CC.Cyan;
        SC.WriteLine($"\n  Day {e.DayNumber} of {e.TotalDays}\n");
        SC.ForegroundColor = CC.Magenta;
        SC.WriteLine($"  {e.Card.Title}");
        SC.ForegroundColor = CC.White;
        SC.WriteLine();
        foreach (var line in WrapText(CardText.StripHtml(e.CardText), 60))
            SC.WriteLine($"  {line}");
        SC.ResetColor();
        SC.WriteLine();
    }

    private void OnAllCaughtUp(object? sender, AllCaughtUpEvent e)
    {
        ConsoleUi.Clear();
        ConsoleUi.Banner();
        SC.ForegroundColor = CC.Yellow;
        SC.WriteLine($"\n  You're all caught up — day {e.DayNumber} of {e.TotalDays}.");
        SC.WriteLine($"  The next day unlocks in {Format(e.TimeUntilNextUnlock)}.\n");
        SC.ResetColor();
        ConsoleUi.PressEnterToContinue();
    }

    private void OnCampaignComplete(object? sender, CampaignCompleteEvent e)
    {
        ConsoleUi.Clear();
        ConsoleUi.Banner();
        SC.ForegroundColor = CC.Yellow;
        SC.WriteLine($"\n  🏆  Campaign complete! All {e.TotalDays} days played, "
                   + $"{(e.CompletedAt - e.StartedAt).Days} calendar days after you started.\n");
        SC.ResetColor();
        ConsoleUi.PressEnterToContinue();
    }

    private static string Format(TimeSpan t) =>
        t.TotalHours >= 1 ? $"{(int)t.TotalHours}h {t.Minutes}m" : $"{t.Minutes}m";

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
