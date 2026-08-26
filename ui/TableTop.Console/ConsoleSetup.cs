using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Progression;
using TableTop.Hosting.Persistence;
using CC = System.ConsoleColor;
using SC = System.Console;

namespace TableTop.Console;

/// <summary>
/// Console player setup with persistence.
/// Loads saved profiles, lets the user select/edit/add players, then saves before starting.
/// </summary>
internal static class ConsolePlayerSetup
{
    public static IReadOnlyList<IPlayer> Run(IPlayerRepository repository)
    {
        var saved = repository.LoadAsync().GetAwaiter().GetResult().ToList();

        var profiles = saved.Count > 0
            ? EditSavedProfiles(saved)
            : CreateNewProfiles();

        // Player-initiated save (same standing as a saved game session, backlog
        // item 19) — reported either way rather than left to an unhandled
        // IOException/UnauthorizedAccessException, which used to take the
        // whole console app down mid-setup on a disk-full or permissions
        // failure instead of just failing this one save.
        try
        {
            repository.SaveAsync(profiles).GetAwaiter().GetResult();
            ConsoleUi.PrintSuccess("Player profiles saved.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ConsoleUi.PrintError("Couldn't save player profiles — check disk space and permissions.");
        }
        SC.WriteLine();

        return profiles.Select(p => (IPlayer)p.ToPlayer()).ToList().AsReadOnly();
    }

    // ── Returning players ─────────────────────────────────────────────────────

    private static List<PlayerProfile> EditSavedProfiles(List<PlayerProfile> saved)
    {
        ConsoleUi.SectionHeader("SAVED PLAYERS");
        PrintProfileList(saved);

        ConsoleUi.PrintMessage("  [1] Play with these players");
        ConsoleUi.PrintMessage("  [2] Add a player");
        ConsoleUi.PrintMessage("  [3] Remove a player");
        ConsoleUi.PrintMessage("  [4] Edit a player");
        ConsoleUi.PrintMessage("  [5] Start fresh");
        SC.WriteLine();

        return ConsoleUi.PromptInt("Choice", 1, 5) switch
        {
            1 => saved,
            2 => AddPlayer(saved),
            3 => RemovePlayer(saved),
            4 => EditPlayer(saved),
            5 => CreateNewProfiles(),
            _ => saved
        };
    }

    private static List<PlayerProfile> AddPlayer(List<PlayerProfile> profiles)
    {
        if (profiles.Count >= 8) { ConsoleUi.PrintError("Maximum 8 players."); return profiles; }
        ConsoleUi.SectionHeader("New Player");
        profiles.Add(CollectProfile());
        return profiles;
    }

    private static List<PlayerProfile> RemovePlayer(List<PlayerProfile> profiles)
    {
        if (profiles.Count <= 2) { ConsoleUi.PrintError("Minimum 2 players."); return profiles; }
        PrintProfileList(profiles);
        var idx = ConsoleUi.PromptInt("Remove player number", 1, profiles.Count) - 1;
        ConsoleUi.PrintSuccess($"{profiles[idx].Name} removed.");
        profiles.RemoveAt(idx);
        return profiles;
    }

    private static List<PlayerProfile> EditPlayer(List<PlayerProfile> profiles)
    {
        PrintProfileList(profiles);
        var idx = ConsoleUi.PromptInt("Edit player number", 1, profiles.Count) - 1;
        ConsoleUi.SectionHeader($"Editing {profiles[idx].Name}");
        ConsoleUi.PrintMessage("(Press ENTER to keep current value)");
        SC.WriteLine();
        profiles[idx] = CollectProfile(profiles[idx]);
        return profiles;
    }

    // ── New players ───────────────────────────────────────────────────────────

    private static List<PlayerProfile> CreateNewProfiles()
    {
        ConsoleUi.SectionHeader("PLAYER SETUP");
        var count = ConsoleUi.PromptInt("How many players?", 2, 8);
        var profiles = new List<PlayerProfile>(count);
        for (var i = 1; i <= count; i++)
        {
            ConsoleUi.SectionHeader($"Player {i}");
            profiles.Add(CollectProfile());
        }
        return profiles;
    }

    private static PlayerProfile CollectProfile(PlayerProfile? existing = null)
    {
        var name = PromptWithDefault("Name", existing?.Name);
        while (string.IsNullOrWhiteSpace(name))
        {
            ConsoleUi.PrintError("Name cannot be empty.");
            name = PromptWithDefault("Name", existing?.Name);
        }

        return new PlayerProfile
        {
            Id = existing?.Id ?? Guid.NewGuid(),
            Name = name.Trim(),
            Gender = PromptGender(existing?.Gender),
            Age = PromptAge(existing?.Age),
            IsParent = ConsoleUi.PromptYesNo($"Parent?{BoolHint(existing?.IsParent)}"),
            IsMarried = ConsoleUi.PromptYesNo($"Married?{BoolHint(existing?.IsMarried)}"),
            IsCoupleMember = ConsoleUi.PromptYesNo($"Partner also playing?{BoolHint(existing?.IsCoupleMember)}"),
        }.Also(p => ConsoleUi.PrintSuccess($"{p.Name} ready."));
    }

    private static string PromptWithDefault(string label, string? def)
    {
        var raw = ConsoleUi.Prompt($"{label}{(def is not null ? $" [{def}]" : "")}:").Trim();
        return string.IsNullOrEmpty(raw) && def is not null ? def : raw;
    }

    private static string PromptGender(string? current)
    {
        while (true)
        {
            var raw = ConsoleUi.Prompt($"Gender (m/f/o){(current is not null ? $" [{current}]" : "")}:")
                .ToLowerInvariant().Trim();
            if (string.IsNullOrEmpty(raw) && current is not null) return current;
            var result = raw switch
            {
                "m" or "male" => "male",
                "f" or "female" => "female",
                "o" or "other" => "other",
                _ => ""
            };
            if (result.Length > 0) return result;
            ConsoleUi.PrintError("Enter m, f, or o.");
        }
    }

    private static int PromptAge(int? current)
    {
        while (true)
        {
            var raw = ConsoleUi.Prompt($"Age{(current.HasValue ? $" [{current}]" : "")}:").Trim();
            if (string.IsNullOrEmpty(raw) && current.HasValue) return current.Value;
            if (int.TryParse(raw, out var age) && age is >= 10 and <= 99) return age;
            ConsoleUi.PrintError("Enter a number between 10 and 99.");
        }
    }

    private static string BoolHint(bool? value) =>
        value.HasValue ? $" (currently {(value.Value ? "yes" : "no")})" : "";

    private static void PrintProfileList(List<PlayerProfile> profiles)
    {
        for (var i = 0; i < profiles.Count; i++)
        {
            var p = profiles[i];
            SC.ForegroundColor = CC.Cyan;
            SC.Write($"  {i + 1}. {p.Name,-16}");
            SC.ForegroundColor = CC.DarkGray;
            var tags = new List<string> { p.Gender, $"age {p.Age}" };
            if (p.IsParent) tags.Add("parent");
            if (p.IsMarried) tags.Add("married");
            if (p.IsCoupleMember) tags.Add("couple");
            SC.WriteLine(string.Join(", ", tags));
        }
        SC.ResetColor();
        SC.WriteLine();
    }
}

/// <summary>Prompts the user to choose a progression strategy.</summary>
internal static class ConsoleGameSetup
{
    public static TableTop.Core.Abstractions.Progression.IProgressionStrategy ChooseProgression()
    {
        ConsoleUi.PrintMessage("Choose card progression:");
        ConsoleUi.PrintMessage("  1  Flow-aware (recommended) — free directional movement");
        ConsoleUi.PrintMessage("  2  Easy → Medium → Hard → Extreme");
        ConsoleUi.PrintMessage("  3  Random");
        ConsoleUi.PrintMessage("  4  Linear (deck order)");
        ConsoleUi.PrintMessage("  5  Score-based");
        ConsoleUi.PrintMessage("  6  Category cycling");

        return ConsoleUi.PromptInt("Selection", 1, 6) switch
        {
            1 => new FlowAwareProgressionStrategy(),
            2 => new DifficultyProgressionStrategy(),
            3 => new RandomProgressionStrategy(),
            4 => new LinearProgressionStrategy(),
            5 => new ScoreBasedProgressionStrategy(),
            6 => new CategoryProgressionStrategy(["Truth", "Dare"]),
            _ => new FlowAwareProgressionStrategy()
        };
    }
}

// ── Extension helper ──────────────────────────────────────────────────────────

internal static class ProfileExtensions
{
    /// <summary>Run an action on a value and return the value. Used for side effects in expressions.</summary>
    public static T Also<T>(this T value, Action<T> action) { action(value); return value; }
}
