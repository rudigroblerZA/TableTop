using SC = System.Console;
using CC = System.ConsoleColor;
using CK = System.ConsoleKey;
using TableTop.Core.Abstractions.Game;
using TableTop.Hosting;

namespace TableTop.Console;

/// <summary>
/// Console renderer for the archetype selection flow.
/// Shows the three root archetypes (Classroom / Fun / Couples), drills into
/// sub-archetypes on selection, then presents a final game list.
///
/// Extracted from <c>ConsoleGameLauncher</c> so the archetype UI is independently
/// readable and testable. Zero game logic — only prompts and display.
/// </summary>
internal static class ConsoleArchetypePicker
{
    /// <summary>
    /// Runs the full archetype → sub-archetype → game selection flow.
    /// Returns the chosen <see cref="IGameMode"/>, or null if the user quits.
    /// </summary>
    public static IGameMode? Run(IArchetypeRegistry registry)
    {
        var mode = PickFromArchetype(registry.RootArchetypes, isRoot: true);
        return mode;
    }

    // ── Step 1: root archetype ────────────────────────────────────────────────

    private static IGameMode? PickFromArchetype(
        IReadOnlyList<Archetype> archetypes, bool isRoot)
    {
        while (true)
        {
            ConsoleUi.Clear();
            ConsoleUi.Banner();
            ConsoleUi.SectionHeader(isRoot ? "WHAT KIND OF GAME?" : "CHOOSE A CATEGORY");

            for (var i = 0; i < archetypes.Count; i++)
                PrintArchetypeRow(i + 1, archetypes[i]);

            SC.WriteLine();
            SC.ForegroundColor = CC.DarkGray;
            SC.WriteLine(isRoot ? "  0. Quit" : "  0. Back");
            SC.ResetColor();
            SC.WriteLine();

            var choice = ConsoleUi.PromptInt("Choose", 0, archetypes.Count);
            if (choice == 0) return null;

            var selected = archetypes[choice - 1];

            // Drill into sub-archetypes or go straight to game list
            if (selected.HasSubArchetypes)
            {
                var mode = PickSubArchetype(selected);
                if (mode is not null) return mode;
                // null means user backed out — loop and re-show root list
            }
            else
            {
                var mode = PickGame(selected.AllModes, selected.Name);
                if (mode is not null) return mode;
                // null means user backed out — loop and re-show root list
            }
        }
    }

    // ── Step 2: sub-archetype ─────────────────────────────────────────────────

    private static IGameMode? PickSubArchetype(Archetype parent)
    {
        while (true)
        {
            ConsoleUi.Clear();
            ConsoleUi.Banner();

            // Parent breadcrumb
            SC.ForegroundColor = CC.DarkGray;
            SC.WriteLine($"  ← {parent.Emoji}  {parent.Name}");
            SC.ResetColor();
            SC.WriteLine();

            ConsoleUi.SectionHeader("CHOOSE A CATEGORY");

            var subs = parent.SubArchetypes;
            for (var i = 0; i < subs.Count; i++)
                PrintArchetypeRow(i + 1, subs[i]);

            SC.WriteLine();
            SC.ForegroundColor = CC.DarkGray;
            SC.WriteLine($"  0. All {parent.Name} games");
            SC.ResetColor();
            SC.WriteLine();

            var choice = ConsoleUi.PromptInt("Choose", 0, subs.Count);

            // 0 = play all modes in the parent archetype
            if (choice == 0)
            {
                var mode = PickGame(parent.AllModes, parent.Name);
                if (mode is not null) return mode;
                continue; // backed out → back to sub-archetype list
            }

            var selected = subs[choice - 1];

            // Recurse if this sub has further children
            if (selected.HasSubArchetypes)
            {
                var mode = PickSubArchetype(selected);
                if (mode is not null) return mode;
                continue;
            }
            else
            {
                var mode = PickGame(selected.AllModes, selected.Name);
                if (mode is not null) return mode;
                // backed out → stay in sub-archetype list
            }
        }
    }

    // ── Step 3: specific game ─────────────────────────────────────────────────

    private static IGameMode? PickGame(IReadOnlyList<IGameMode> modes, string categoryName)
    {
        if (modes.Count == 0)
        {
            ConsoleUi.PrintError("No games available in this category.");
            ConsoleUi.PressEnterToContinue();
            return null;
        }

        ConsoleUi.Clear();
        ConsoleUi.Banner();
        ConsoleUi.SectionHeader($"SELECT GAME — {categoryName.ToUpperInvariant()}");

        for (var i = 0; i < modes.Count; i++)
        {
            SC.ForegroundColor = CC.Cyan;
            SC.Write($"  {i + 1}.  {modes[i].Name}");
            SC.ResetColor();
            SC.ForegroundColor = CC.DarkGray;
            SC.WriteLine($"  —  {modes[i].Description}");
            SC.ResetColor();
        }

        SC.WriteLine();
        SC.ForegroundColor = CC.DarkGray;
        SC.WriteLine("  0. Back");
        SC.ResetColor();
        SC.WriteLine();

        var choice = ConsoleUi.PromptInt("Your choice", 0, modes.Count);
        return choice == 0 ? null : modes[choice - 1];
    }

    // ── Display helpers ───────────────────────────────────────────────────────

    private static void PrintArchetypeRow(int number, Archetype archetype)
    {
        // Colour-code by archetype
        var colour = archetype.AgeRating switch
        {
            AgeRating.AllAges => CC.Cyan,
            AgeRating.Teen    => CC.Green,
            AgeRating.Adult   => CC.Magenta,
            _                 => CC.White,
        };

        SC.ForegroundColor = colour;
        SC.Write($"  {number}.  {archetype.Emoji}  {archetype.Name}");
        SC.ResetColor();
        SC.ForegroundColor = CC.DarkGray;
        var pad = Math.Max(1, 22 - archetype.Name.Length);
        SC.Write(new string(' ', pad));
        SC.Write(archetype.Description);

        // Age + game count badge
        var badge = $"  [{archetype.AgeRating.ToLabel()} · {archetype.AllModes.Count} game(s)]";
        SC.ForegroundColor = CC.DarkGray;
        SC.WriteLine(badge);
        SC.ResetColor();
    }
}

internal static class AgeRatingExtensions
{
    public static string ToLabel(this AgeRating rating) => rating switch
    {
        AgeRating.AllAges => "All ages",
        AgeRating.Teen    => "13+",
        AgeRating.Adult   => "18+",
        _                 => string.Empty,
    };
}
