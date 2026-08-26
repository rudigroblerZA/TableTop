using System.Reflection;
using System.Text;

namespace TableTop.Tests;

/// <summary>
/// Pins the public API surface of the three engine assemblies (backlog E.2).
///
/// The usual tool for this is <c>Microsoft.CodeAnalysis.PublicApiAnalyzers</c>.
/// This does the same job by reflection instead, for two reasons: it adds no
/// package dependency, and it runs in environments where NuGet isn't reachable.
///
/// The point is not to prevent API changes — this engine is young and they are
/// expected. It is to stop them happening <i>silently</i>. The most recent
/// example: backlog D.1 replaced <c>CardTurnController</c>'s eight trailing
/// optional parameters with a <c>CardTurnControllerOptions</c> record. That is a
/// breaking change for every caller outside this repository, and nothing in the
/// build would have said so. Now the diff turns up in review.
///
/// <b>When this fails</b>, read the diff. If the change was intended, regenerate
/// the snapshot and commit it alongside the change:
///
/// <code>
///     TABLETOP_UPDATE_API=1 dotnet test tests/TableTop.Tests --filter PublicApiSurfaceTests
/// </code>
///
/// Committing a regenerated snapshot is the record that someone looked at the
/// change and accepted it. Regenerating without reading the diff defeats the
/// whole exercise.
/// </summary>
public sealed class PublicApiSurfaceTests
{
    private const string UpdateEnvironmentVariable = "TABLETOP_UPDATE_API";

    [Theory]
    [InlineData("TableTop.Core")]
    [InlineData("TableTop.Games")]
    [InlineData("TableTop.Hosting")]
    public void Public_surface_matches_the_committed_snapshot(string assemblyName)
    {
        var assembly = AssemblyFor(assemblyName);
        var actual = Describe(assembly);
        var path = Path.Combine(FindRepositoryRoot(), "api", $"{assemblyName}.api.txt");

        if (Environment.GetEnvironmentVariable(UpdateEnvironmentVariable) == "1")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, actual);
            return;
        }

        File.Exists(path).Should().BeTrue(
            $"'{path}' is the committed public API snapshot for {assemblyName}. " +
            $"Generate it with {UpdateEnvironmentVariable}=1.");

        var expectedLines = File.ReadAllLines(path);
        var actualLines = actual.Split('\n').Select(l => l.TrimEnd('\r')).ToArray();

        // Ordered, positional comparison — NOT a set difference.
        //
        // This originally used Except(), which compares distinct line VALUES.
        // That silently missed any change whose text already appeared elsewhere
        // in the file, and member signatures repeat constantly: `.ctor()`,
        // `Int32 GetHashCode()`, `String Name { get; set; }`. It let a real
        // change through — adding `ModeFilePath` to CardTurnControllerOptions
        // was invisible because SessionSnapshot already declared a line reading
        // exactly `    String ModeFilePath { get; set; }`.
        //
        // Walking both sequences in order and pairing on the type header keeps
        // a member attributed to the type it belongs to.
        var added = new List<string>();
        var removed = new List<string>();

        var expectedByType = GroupByType(expectedLines);
        var actualByType = GroupByType(actualLines);

        foreach (var (type, members) in actualByType)
        {
            if (!expectedByType.TryGetValue(type, out var was))
            {
                added.Add(type);
                added.AddRange(members.Select(m => $"{type} :: {m}"));
                continue;
            }
            added.AddRange(members.Where(m => !was.Remove(m)).Select(m => $"{type} :: {m}"));
        }

        foreach (var (type, members) in expectedByType)
        {
            if (!actualByType.ContainsKey(type))
            {
                removed.Add(type);
                removed.AddRange(members.Select(m => $"{type} :: {m}"));
            }
            else
            {
                removed.AddRange(members.Select(m => $"{type} :: {m}"));
            }
        }

        var report = new StringBuilder();
        if (removed.Count > 0)
        {
            report.AppendLine($"REMOVED from {assemblyName} — breaking for existing callers:");
            foreach (var l in removed.Take(25)) report.AppendLine($"  - {l}");
            if (removed.Count > 25) report.AppendLine($"  … and {removed.Count - 25} more");
        }
        if (added.Count > 0)
        {
            report.AppendLine($"ADDED to {assemblyName}:");
            foreach (var l in added.Take(25)) report.AppendLine($"  + {l}");
            if (added.Count > 25) report.AppendLine($"  … and {added.Count - 25} more");
        }

        (added.Count + removed.Count).Should().Be(0,
            $"the public surface of {assemblyName} changed.\n{report}\n" +
            $"If that was intended, regenerate with {UpdateEnvironmentVariable}=1 and commit the snapshot " +
            "in the same change, so the diff is visible in review.");
    }

    /// <summary>
    /// Splits a snapshot into type header → list of its member lines. Members
    /// stay a list rather than a set, so two identical signatures on one type
    /// are two entries and dropping one is detected.
    /// </summary>
    private static Dictionary<string, List<string>> GroupByType(IEnumerable<string> lines)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var current = "";

        foreach (var line in lines)
        {
            if (line.Length == 0) continue;

            if (line[0] != ' ')
            {
                current = line;
                if (!result.ContainsKey(current)) result[current] = [];
            }
            else if (current.Length > 0)
            {
                result[current].Add(line.Trim());
            }
        }

        return result;
    }

    // ── Description ───────────────────────────────────────────────────────────
    //
    // Kept deliberately simple and ordered, because the output is a diff target:
    // anything unstable between runs (hash codes, reflection ordering) would
    // produce noise and get the test disabled.

    private static Assembly AssemblyFor(string name) => name switch
    {
        "TableTop.Core" => typeof(Core.Abstractions.Cards.ICard).Assembly,
        "TableTop.Games" => typeof(Games.WouldYouRatherMode).Assembly,
        "TableTop.Hosting" => typeof(Hosting.Controllers.CardTurnController).Assembly,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "unknown assembly"),
    };

    internal static string Describe(Assembly assembly)
    {
        var sb = new StringBuilder();
        foreach (var t in assembly.GetExportedTypes().OrderBy(t => t.FullName, StringComparer.Ordinal))
        {
            sb.AppendLine(TypeHeader(t));
            foreach (var m in Members(t)) sb.AppendLine("    " + m);
        }
        return sb.ToString();
    }

    private static string TypeHeader(Type t)
    {
        var kind = t.IsEnum ? "enum" : t.IsInterface ? "interface" : t.IsValueType ? "struct" : "class";
        var mods = t is { IsAbstract: true, IsSealed: true } ? "static "
                 : t.IsAbstract ? "abstract "
                 : t.IsSealed ? "sealed " : "";
        return $"{mods}{kind} {t.FullName}";
    }

    private static IEnumerable<string> Members(Type t)
    {
        const BindingFlags Flags =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        if (t.IsEnum)
        {
            return Enum.GetNames(t)
                       .OrderBy(n => n, StringComparer.Ordinal)
                       .Select(n => $"{n} = {Convert.ToInt64(Enum.Parse(t, n))}");
        }

        var lines = new List<string>();

        foreach (var c in t.GetConstructors(Flags))
            lines.Add($".ctor({Parameters(c.GetParameters())})");
        foreach (var p in t.GetProperties(Flags))
            lines.Add($"{ShortName(p.PropertyType)} {p.Name} {{ {(p.CanRead ? "get; " : "")}{(p.CanWrite ? "set; " : "")}}}");
        foreach (var e in t.GetEvents(Flags))
            lines.Add($"event {ShortName(e.EventHandlerType!)} {e.Name}");
        foreach (var f in t.GetFields(Flags).Where(f => !f.IsSpecialName))
            lines.Add($"{ShortName(f.FieldType)} {f.Name}");
        foreach (var m in t.GetMethods(Flags).Where(m => !m.IsSpecialName))
            lines.Add($"{ShortName(m.ReturnType)} {m.Name}({Parameters(m.GetParameters())})");

        return lines.OrderBy(l => l, StringComparer.Ordinal);
    }

    private static string Parameters(ParameterInfo[] parameters) =>
        string.Join(", ", parameters.Select(p =>
            $"{ShortName(p.ParameterType)} {p.Name}{(p.HasDefaultValue ? " = default" : "")}"));

    private static string ShortName(Type t) =>
        t.IsGenericType
            ? $"{t.Name[..t.Name.IndexOf('`')]}<{string.Join(", ", t.GetGenericArguments().Select(ShortName))}>"
            : t.Name;

    private static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "src", "TableTop.Core")))
                return dir.FullName;
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the repository root from '{AppContext.BaseDirectory}'.");
    }
}
