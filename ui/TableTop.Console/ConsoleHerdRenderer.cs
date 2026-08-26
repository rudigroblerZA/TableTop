using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Events;
using CC = System.ConsoleColor;
using SC = System.Console;

namespace TableTop.Console;

/// <summary>
/// Renders Herd events to the console. Zero game logic.
///
/// <para>
/// One shared terminal stands in for "everyone answers at once" — each player
/// types their answer in turn, then every answer is submitted together. Like
/// <see cref="ConsoleClaimedRenderer"/>, there is no background poll loop:
/// <see cref="IHerdController.SubmitAnswers"/> raises
/// <see cref="IHerdController.RoundResolved"/> and then either
/// <see cref="IHerdController.PromptReady"/> or
/// <see cref="IHerdController.GameEnded"/> before returning, so the outcome is
/// always known by the time the call that produced it returns.
/// </para>
/// </summary>
internal sealed class ConsoleHerdRenderer
{
    private readonly IHerdController _controller;
    private HerdPromptReadyEvent? _prompt;

    public ConsoleHerdRenderer(IHerdController controller)
    {
        _controller = controller;
        controller.PromptReady += (_, e) => _prompt = e;
        controller.RoundResolved += OnRoundResolved;
        controller.GameEnded += OnGameEnded;
    }

    public void RunBlocking()
    {
        _controller.Start();
        while (_controller.IsRunning)
        {
            if (_prompt is not { } prompt) break;

            ConsoleUi.Clear();
            ConsoleUi.Banner();
            SC.ForegroundColor = CC.DarkGray;
            SC.WriteLine($"\n  Round {prompt.RoundNumber} / {prompt.TotalRounds}  ·  {prompt.Category}");
            SC.ForegroundColor = CC.White;
            SC.WriteLine($"\n  {prompt.Prompt}\n");
            SC.ResetColor();

            var answers = new Dictionary<string, string>();
            foreach (var name in _controller.Scores.Keys)
                answers[name] = ConsoleUi.Prompt($"  {name}, your answer:");

            _controller.SubmitAnswers(answers);
        }
    }

    // ── Events ────────────────────────────────────────────────────────────────

    private void OnRoundResolved(object? sender, HerdRoundResolvedEvent e)
    {
        SC.WriteLine();
        SC.ForegroundColor = CC.Magenta;
        foreach (var g in e.Groups)
            SC.WriteLine($"  {g.Answer,-24} {string.Join(", ", g.PlayerNames)}");
        SC.ResetColor();

        if (e.HerdAnswer is { Length: > 0 })
        {
            SC.ForegroundColor = CC.Yellow;
            SC.WriteLine($"\n  🐑  The herd said: {e.HerdAnswer}");
        }
        if (e.LoneVoiceName is { Length: > 0 })
        {
            SC.ForegroundColor = CC.Cyan;
            SC.WriteLine($"  🎤  Lone voice: {e.LoneVoiceName}");
        }
        SC.ResetColor();
        SC.WriteLine();
        SC.WriteLine($"  Scores: {string.Join("   ", _controller.Scores.Select(kv => $"{kv.Key} {kv.Value}"))}");
        ConsoleUi.PressEnterToContinue();
    }

    private void OnGameEnded(object? sender, HerdGameEndedEvent e)
    {
        ConsoleUi.Clear();
        ConsoleUi.Banner();
        SC.ForegroundColor = CC.Yellow;
        SC.WriteLine($"\n  🏆  {string.Join(" & ", e.WinnerNames)} wins after {e.RoundsPlayed} rounds!\n");
        foreach (var (name, score) in e.FinalScores)
        {
            SC.ForegroundColor = CC.White;
            SC.Write($"  {name,-20}");
            SC.ForegroundColor = CC.Yellow;
            SC.WriteLine($"  {score}");
        }
        SC.ResetColor();
        ConsoleUi.PressEnterToContinue();
    }
}
