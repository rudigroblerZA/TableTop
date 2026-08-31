using TableTop.Core.Abstractions.Analysis;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Events;
using CC = System.ConsoleColor;
using SC = System.Console;

namespace TableTop.Console;

/// <summary>
/// Renders a trait-assessment session to the console. Zero game logic.
///
/// <para>
/// One shared terminal stands in for "everyone answers the same statement" —
/// each player types a 1-5 in turn, then the whole set is submitted together.
/// Like <see cref="ConsoleHerdRenderer"/> there is no background poll loop:
/// <see cref="ITraitProfileController.SubmitResponses"/> raises
/// <see cref="ITraitProfileController.ItemRecorded"/> and then either the next
/// <see cref="ITraitProfileController.ItemReady"/> or
/// <see cref="ITraitProfileController.AssessmentCompleted"/> before returning.
/// </para>
///
/// <para>
/// Fifty statements times a table of players is a lot of typing, so a blank
/// line skips the statement for that player rather than forcing a number. That
/// is a real skip in the engine — the item is left out of that player's
/// denominator entirely, not recorded as a neutral answer.
/// </para>
/// </summary>
internal sealed class ConsoleTraitProfileRenderer
{
    private const int BarWidth = 24;

    private readonly ITraitProfileController _controller;
    private TraitItemReadyEvent? _item;

    public ConsoleTraitProfileRenderer(ITraitProfileController controller)
    {
        _controller = controller;
        controller.ItemReady += (_, e) => _item = e;
        controller.AssessmentCompleted += OnCompleted;
    }

    public void RunBlocking()
    {
        _controller.Start();

        while (_controller.IsRunning)
        {
            if (_item is not { } item) break;

            ConsoleUi.Clear();
            ConsoleUi.Banner();
            SC.ForegroundColor = CC.DarkGray;
            SC.WriteLine($"\n  {item.ItemNumber} / {item.TotalItems}  ·  {item.Category}");
            SC.ForegroundColor = CC.White;
            SC.WriteLine($"\n  {item.Statement}\n");
            SC.ForegroundColor = CC.DarkGray;
            SC.WriteLine("  1 strongly disagree   2 disagree   3 neutral   4 agree   5 strongly agree");
            SC.WriteLine("  (blank to skip)\n");
            SC.ResetColor();

            var responses = new Dictionary<string, LikertResponse>(StringComparer.OrdinalIgnoreCase);

            foreach (var name in _controller.PlayerNames)
            {
                var raw = ConsoleUi.Prompt($"  {name} (1-5):");

                // EOF mid-assessment: stop asking rather than looping on empty
                // reads for every remaining player and item. Quit ends the
                // session on what has actually been answered.
                if (ConsoleUi.InputEnded) { _controller.Quit(); return; }

                if (int.TryParse(raw, out var value) && value is >= 1 and <= 5)
                    responses[name] = (LikertResponse)value;
            }

            _controller.SubmitResponses(responses);
        }
    }

    // ── Events ────────────────────────────────────────────────────────────────

    private void OnCompleted(object? sender, TraitAssessmentCompletedEvent e)
    {
        ConsoleUi.Clear();
        ConsoleUi.Banner();

        if (e.Profiles.Count == 0)
        {
            SC.ForegroundColor = CC.Yellow;
            SC.WriteLine("\n  Nobody answered anything, so there is nothing to report.\n");
            SC.ResetColor();
            ConsoleUi.PressEnterToContinue();
            return;
        }

        foreach (var profile in e.Profiles)
        {
            SC.ForegroundColor = CC.Yellow;
            SC.WriteLine($"\n  {profile.PlayerName}  ·  {profile.AnsweredItems} answered");
            SC.ResetColor();

            foreach (var score in profile.Scores)
            {
                SC.ForegroundColor = CC.White;
                SC.Write($"  {score.Trait.Name,-18}");

                if (!score.HasData)
                {
                    SC.ForegroundColor = CC.DarkGray;
                    SC.WriteLine("  no answers");
                    SC.ResetColor();
                    continue;
                }

                SC.ForegroundColor = CC.Cyan;
                SC.Write($"  {Bar(score.Normalized)}");
                SC.ForegroundColor = CC.DarkGray;
                SC.WriteLine($"  {score.Normalized,5:0.0}  {Describe(score)}");
                SC.ResetColor();
            }
        }

        if (e.MostAlike is { } alike)
        {
            SC.ForegroundColor = CC.Magenta;
            SC.WriteLine($"\n  Closest: {alike.Left.PlayerName} & {alike.Right.PlayerName}"
                       + $"  ({alike.Similarity:0.0}% alike across {alike.ComparedDimensions} traits)");
            SC.ResetColor();

            if (alike.GreatestDivergence is { } gap)
                SC.WriteLine($"    Furthest apart on {gap.Trait.Name}: "
                           + $"{gap.Left.Normalized:0} vs {gap.Right.Normalized:0}");
            if (alike.ClosestAlignment is { } same)
                SC.WriteLine($"    Most aligned on {same.Trait.Name}: "
                           + $"{same.Left.Normalized:0} vs {same.Right.Normalized:0}");
        }

        if (e.MostDifferent is { } apart && !ReferenceEquals(apart, e.MostAlike))
        {
            SC.ForegroundColor = CC.DarkYellow;
            SC.WriteLine($"\n  Furthest: {apart.Left.PlayerName} & {apart.Right.PlayerName}"
                       + $"  ({apart.Similarity:0.0}% alike)");
            SC.ResetColor();
        }

        SC.WriteLine();
        SC.ForegroundColor = CC.DarkGray;
        SC.WriteLine("  Scores show where your answers landed on this quiz's own range —");
        SC.WriteLine("  they are not percentiles, and this is not a personality test.");
        SC.ResetColor();
        SC.WriteLine();

        ConsoleUi.PressEnterToContinue();
    }

    /// <summary>A filled/unfilled bar for a 0-100 score.</summary>
    private static string Bar(double normalized)
    {
        var filled = (int)Math.Round(normalized / 100d * BarWidth, MidpointRounding.AwayFromZero);
        filled = Math.Clamp(filled, 0, BarWidth);
        return new string('█', filled) + new string('·', BarWidth - filled);
    }

    /// <summary>The band, in the dimension's own words rather than "VeryHigh".</summary>
    private static string Describe(TraitScore score) => score.Band switch
    {
        TraitBand.VeryLow => score.Trait.LowLabel,
        TraitBand.Low => score.Trait.LowLabel,
        TraitBand.High => score.Trait.HighLabel,
        TraitBand.VeryHigh => score.Trait.HighLabel,
        _ => "in the middle",
    };
}
