using TableTop.Hosting;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Events;
using CC = System.ConsoleColor;
using SC = System.Console;

namespace TableTop.Console;

/// <summary>
/// Renders Claimed! events to the console. Zero game logic.
///
/// <para>
/// Unlike Monogamy's dice-driven turns, nothing here happens on its own —
/// every event is a direct, synchronous consequence of a call this renderer
/// just made (<see cref="IClaimedController.ChallengeTerritory"/> raises
/// <see cref="IClaimedController.TerritoryChallengeReady"/> before returning;
/// <see cref="IClaimedController.ResolveChallenge"/> raises the outcome event
/// and possibly <see cref="IClaimedController.GameEnded"/> before returning).
/// So there is no background poll loop — the renderer drives the sequence
/// directly and reads the result each call just produced.
/// </para>
/// </summary>
internal sealed class ConsoleClaimedRenderer
{
    private readonly IClaimedController _controller;
    private TerritoryChallengeReadyEvent? _pending;

    public ConsoleClaimedRenderer(IClaimedController controller)
    {
        _controller = controller;
        controller.TerritoryChallengeReady += (_, e) => _pending = e;
        controller.TerritoryClaimed += OnClaimed;
        controller.TerritoryStolen += OnStolen;
        controller.ChallengeFailed += OnFailed;
        controller.GameEnded += OnGameEnded;
    }

    public void RunBlocking()
    {
        _controller.Start();
        while (_controller.IsRunning)
        {
            RenderBoard();
            var choice = PromptTerritory();
            if (choice is null) continue;

            _controller.ChallengeTerritory(choice);
            if (_pending is not { } challenge) continue;
            _pending = null;

            RenderChallenge(challenge);
            var succeeded = ConsoleUi.PromptYesNo("  Did they succeed?");
            _controller.ResolveChallenge(succeeded);
        }
    }

    // ── Board and prompts ────────────────────────────────────────────────────

    private void RenderBoard()
    {
        ConsoleUi.Clear();
        ConsoleUi.Banner();
        SC.ForegroundColor = CC.Cyan;
        SC.WriteLine($"\n  {_controller.CurrentPlayerName}'s turn\n");
        SC.ForegroundColor = CC.White;
        foreach (var (territory, holder) in _controller.TerritoryHolders)
        {
            SC.ForegroundColor = holder is null ? CC.DarkGray : CC.White;
            SC.WriteLine($"  {territory,-20}  {holder ?? "Open"}");
        }
        SC.ResetColor();
        SC.WriteLine();
    }

    private string? PromptTerritory()
    {
        var options = _controller.ChallengeableTerritories;
        SC.ForegroundColor = CC.DarkGray;
        for (var i = 0; i < options.Count; i++)
            SC.WriteLine($"  [{i + 1}] {options[i]}");
        SC.ResetColor();

        var input = ConsoleUi.Prompt("Challenge which territory?").Trim();
        if (int.TryParse(input, out var index) && index >= 1 && index <= options.Count)
            return options[index - 1];

        var match = options.FirstOrDefault(o => string.Equals(o, input, StringComparison.OrdinalIgnoreCase));
        if (match is not null) return match;

        // Same guard as ConsoleUi.PromptInt/PromptYesNo: without it, a piped
        // run whose input ran dry would re-prompt a dead stream forever —
        // ReadLine returns null instantly on a closed stream, so this isn't a
        // slow retry, it's a tight busy-loop.
        if (ConsoleUi.InputEnded)
        {
            ConsoleUi.PrintError($"Input ended; using {options[0]}.");
            return options[0];
        }

        ConsoleUi.PrintError("Enter a number from the list, or the territory's name.");
        return null;
    }

    private void RenderChallenge(TerritoryChallengeReadyEvent e)
    {
        SC.ForegroundColor = CC.Magenta;
        SC.WriteLine($"\n  {(e.DefenderName is null ? "Claiming" : $"Raiding {e.DefenderName}'s")} {e.TerritoryName}");
        SC.WriteLine($"  {e.CardTitle}  [{e.Difficulty}]");
        SC.ForegroundColor = CC.White;
        SC.WriteLine();
        foreach (var line in WrapText(CardText.StripHtml(e.CardText), 60))
            SC.WriteLine($"  {line}");
        SC.ResetColor();
        SC.WriteLine();
    }

    // ── Outcome events ────────────────────────────────────────────────────────

    private void OnClaimed(object? sender, TerritoryClaimedEvent e)
    {
        ConsoleUi.PrintSuccess($"{e.PlayerName} claims {e.TerritoryName}! ({e.HeldTerritories.Count} held)");
        ConsoleUi.PressEnterToContinue();
    }

    private void OnStolen(object? sender, TerritoryStolenEvent e)
    {
        ConsoleUi.PrintSuccess($"{e.AttackerName} raids {e.TerritoryName} from {e.DefenderName}!");
        ConsoleUi.PressEnterToContinue();
    }

    private void OnFailed(object? sender, ChallengeFailedEvent e)
    {
        ConsoleUi.PrintError(e.WasRaid
            ? $"{e.PlayerName}'s raid on {e.TerritoryName} fails."
            : $"{e.PlayerName} fails to claim {e.TerritoryName}.");
        ConsoleUi.PressEnterToContinue();
    }

    private void OnGameEnded(object? sender, ClaimedGameEndedEvent e)
    {
        ConsoleUi.Clear();
        ConsoleUi.Banner();
        SC.ForegroundColor = CC.Yellow;
        SC.WriteLine(e.Reason == ClaimedEndReason.ThreeHeld
            ? $"\n  🏆  {string.Join(" & ", e.WinnerNames)} wins by holding {e.WinnerNames.Count} territories!\n"
            : $"\n  🏆  The decks ran dry — {string.Join(" & ", e.WinnerNames)} held the most.\n");
        foreach (var (name, held) in e.FinalHoldings)
        {
            SC.ForegroundColor = CC.White;
            SC.Write($"  {name,-20}");
            SC.ForegroundColor = CC.Yellow;
            SC.WriteLine($"  {held.Count} held  ({string.Join(", ", held)})");
        }
        SC.ResetColor();
        ConsoleUi.PressEnterToContinue();
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
