using TableTop.Hosting.Persistence;
using CC = System.ConsoleColor;
using SC = System.Console;

namespace TableTop.Console;

/// <summary>
/// Text-mode roster builder (backlog item 28). Console had a player-setup flow
/// but no way to save a whole group and start a later game from it — the
/// Roaster the graphical heads got in 1.28.0 never reached this head.
///
/// <para>
/// Deliberately its own small flow over <see cref="PlayerProfile"/> /
/// <see cref="IRosterRepository"/>, not the shared <c>RoasterViewModel</c>:
/// Console does not reference <c>TableTop.Presentation</c>, and its people are
/// already <c>PlayerProfile</c>s from <c>IPlayerRepository</c> rather than the
/// lighter record the graphical heads' Roaster builds.
/// </para>
/// </summary>
internal static class ConsoleRoster
{
    /// <summary>
    /// Offers the saved rosters for selection. Returns a fresh copy of the
    /// chosen roster's players, or null when the user picks "none" — in which
    /// case the caller falls back to the normal setup flow. Also handles
    /// deleting a roster in place.
    /// </summary>
    public static List<PlayerProfile>? Choose(IRosterRepository repository, IReadOnlyList<RosterProfile> rosters)
    {
        var working = rosters.ToList();

        while (true)
        {
            ConsoleUi.SectionHeader("SAVED ROSTERS");
            for (var i = 0; i < working.Count; i++)
            {
                SC.ForegroundColor = CC.Cyan;
                SC.Write($"  {i + 1}. {working[i].Name,-20}");
                SC.ForegroundColor = CC.DarkGray;
                SC.WriteLine(working[i].Summary);
            }
            SC.ResetColor();
            SC.WriteLine();
            ConsoleUi.PrintMessage("  [0] None — set players up normally");
            ConsoleUi.PrintMessage("  [d] Delete a roster");
            SC.WriteLine();

            var raw = ConsoleUi.Prompt("Choice:").Trim().ToLowerInvariant();

            if (raw is "0" or "")
                return null;

            if (raw == "d")
            {
                if (working.Count == 0) { ConsoleUi.PrintError("Nothing to delete."); continue; }
                var del = ConsoleUi.PromptInt("Delete roster number", 1, working.Count) - 1;
                ConsoleUi.PrintSuccess($"Deleted \"{working[del].Name}\".");
                working.RemoveAt(del);
                repository.SaveAsync(working).GetAwaiter().GetResult();
                if (working.Count == 0) return null;
                continue;
            }

            if (int.TryParse(raw, out var n) && n >= 1 && n <= working.Count)
            {
                // Clone the profiles so editing them in setup can't mutate the
                // saved roster in memory before the player decides to re-save.
                return working[n - 1].Players.Select(Clone).ToList();
            }

            ConsoleUi.PrintError($"Enter 0–{working.Count}, or d.");
        }
    }

    /// <summary>
    /// Prompts for a name and appends the current group to the saved rosters,
    /// persisting the whole list. A blank name or a disk failure just skips the
    /// save with a message — same standing as a player-profile save (item 19),
    /// not something to crash setup over.
    /// </summary>
    public static void SaveCurrent(
        IRosterRepository repository, IReadOnlyList<RosterProfile> existing, IReadOnlyList<PlayerProfile> players)
    {
        var name = ConsoleUi.Prompt("Roster name:").Trim();
        if (name.Length == 0)
        {
            ConsoleUi.PrintMessage("No name given — roster not saved.");
            return;
        }

        var updated = existing.ToList();
        updated.RemoveAll(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
        updated.Add(new RosterProfile { Name = name, Players = players.Select(Clone).ToList() });

        try
        {
            repository.SaveAsync(updated).GetAwaiter().GetResult();
            ConsoleUi.PrintSuccess($"Saved roster \"{name}\".");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ConsoleUi.PrintError("Couldn't save the roster — check disk space and permissions.");
        }
    }

    private static PlayerProfile Clone(PlayerProfile p) => new()
    {
        SchemaVersion = p.SchemaVersion,
        Id = p.Id,
        Name = p.Name,
        Gender = p.Gender,
        Age = p.Age,
        IsParent = p.IsParent,
        IsMarried = p.IsMarried,
        IsCoupleMember = p.IsCoupleMember,
    };
}
