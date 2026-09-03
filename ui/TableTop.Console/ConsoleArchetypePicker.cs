using TableTop.Core.Abstractions.Game;
using TableTop.Hosting;
using CC = System.ConsoleColor;
using SC = System.Console;

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
    public static IGameMode? Run(IArchetypeRegistry registry, FavouritesService? favourites = null)
    {
        _favourites = favourites;
        var mode = PickFromArchetype(registry.RootArchetypes, isRoot: true);
        return mode;
    }

    /// <summary>
    /// The starred-modes service for this run, or null when the host did not
    /// supply one.
    ///
    /// <para>
    /// Static because the picker is, and the picker is static because it holds
    /// no other state. Nullable rather than a null-object because the whole
    /// favourites row disappears when it is absent — there is a visible
    /// difference between "no favourites yet" and "this host does not do
    /// favourites", and collapsing the two would show players a Favourites
    /// entry that could never fill up.
    /// </para>
    /// </summary>
    private static FavouritesService? _favourites;

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

        // Re-ordered once, outside the loop. Doing it per redraw would renumber
        // the list under the player the instant they starred something — they
        // would type "3" expecting what was on row 3 a second ago.
        var ordered = _favourites?.FavouritesFirst(modes) ?? modes;

        while (true)
        {
            ConsoleUi.Clear();
            ConsoleUi.Banner();
            ConsoleUi.SectionHeader($"SELECT GAME — {categoryName.ToUpperInvariant()}");

            for (var i = 0; i < ordered.Count; i++)
                PrintGameRow(i + 1, ordered[i]);

            SC.WriteLine();
            SC.ForegroundColor = CC.DarkGray;
            SC.WriteLine("  0. Back");
            if (_favourites is not null)
                SC.WriteLine("  f<number>. Star or unstar a game (e.g. f3)");
            SC.ResetColor();
            SC.WriteLine();

            // Not PromptInt: it accepts only a number, and the star command has
            // to share this prompt. The numeric path below reproduces
            // PromptInt's contract exactly, EOF handling included — without it a
            // piped run would spin here forever, which PromptInt's own remarks
            // record as a real bug rather than a hypothetical one.
            var raw = ConsoleUi.Prompt($"Your choice (0-{ordered.Count}):");

            if (ConsoleUi.InputEnded)
            {
                ConsoleUi.PrintError("Input ended; going back.");
                return null;
            }

            if (_favourites is not null &&
                raw.StartsWith('f') &&
                int.TryParse(raw.AsSpan(1), out var star) &&
                star >= 1 && star <= ordered.Count)
            {
                ToggleFavourite(ordered[star - 1]);
                continue;
            }

            if (int.TryParse(raw, out var choice) && choice >= 0 && choice <= ordered.Count)
                return choice == 0 ? null : ordered[choice - 1];

            ConsoleUi.PrintError($"Please enter 0-{ordered.Count}" +
                (_favourites is not null ? ", or f followed by a number to star one." : "."));
            ConsoleUi.PressEnterToContinue();
        }
    }

    /// <summary>Prints one game row, starred ones marked and highlighted.</summary>
    private static void PrintGameRow(int number, IGameMode mode)
    {
        var starred = _favourites?.IsFavourite(mode) == true;

        SC.ForegroundColor = starred ? CC.Yellow : CC.Cyan;
        SC.Write($"  {number}.  {(starred ? "★ " : "")}{mode.Name}");
        SC.ResetColor();
        SC.ForegroundColor = CC.DarkGray;
        SC.WriteLine($"  —  {mode.Description}");
        SC.ResetColor();
    }

    /// <summary>
    /// Stars or unstars a mode, reporting a write failure rather than swallowing
    /// it — <c>FavouritesService</c> rolls the in-memory change back when the
    /// save throws, so staying silent would show the star reverting for no
    /// visible reason.
    /// </summary>
    private static void ToggleFavourite(IGameMode mode)
    {
        try
        {
            var nowFavourite = _favourites!.ToggleAsync(mode).GetAwaiter().GetResult();
            ConsoleUi.PrintMessage(nowFavourite
                ? $"★ {mode.Name} added to favourites."
                : $"{mode.Name} removed from favourites.");
        }
        catch (IOException ex)
        {
            ConsoleUi.PrintError($"Could not save favourites: {ex.Message}");
            ConsoleUi.PressEnterToContinue();
        }
        catch (UnauthorizedAccessException ex)
        {
            ConsoleUi.PrintError($"Could not save favourites: {ex.Message}");
            ConsoleUi.PressEnterToContinue();
        }
    }

    // ── Display helpers ───────────────────────────────────────────────────────

    private static void PrintArchetypeRow(int number, Archetype archetype)
    {
        // Colour-code by archetype
        var colour = archetype.AgeRating switch
        {
            AgeRating.AllAges => CC.Cyan,
            AgeRating.Teen => CC.Green,
            AgeRating.Adult => CC.Magenta,
            _ => CC.White,
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
        AgeRating.Teen => "13+",
        AgeRating.Adult => "18+",
        _ => string.Empty,
    };
}
