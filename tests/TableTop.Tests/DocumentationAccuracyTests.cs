using System.Reflection;
using System.Text.RegularExpressions;
using TableTop.Core.Abstractions.Game;

namespace TableTop.Tests;

/// <summary>
/// Keeps the counts quoted in <c>README.md</c> true (backlog K.3).
///
/// The README has gone stale twice: it claimed 662 tests when there were 704,
/// and 85 modes while the deck count had moved. Numbers typed into prose rot
/// silently, and a README that is wrong about the easy facts is not trusted on
/// the hard ones.
///
/// This checks only figures that can be derived from the tree. Everything else
/// in the README is prose, and prose is not something a test can keep honest.
///
/// <b>When this fails</b>, the README is out of date — update the number. It is
/// not telling you the code is wrong.
/// </summary>
public sealed class DocumentationAccuracyTests
{
    /// <summary>
    /// Counts modes from source. The deck/card half of this test was removed in
    /// 1.18.0 along with the JSON decks.
    ///
    /// <para>
    /// It used to count cards by parsing <c>Data/Json/*.deck.json</c>. With
    /// those gone that sums to zero, which is not "no cards" — it's the wrong
    /// source. Cards now live in the in-code banks, and counting them
    /// statically would mean regex-scraping C# collection initialisers, which
    /// is exactly the kind of brittle proxy this suite exists to avoid.
    /// The real number is asserted at runtime instead, by
    /// <c>ModeManifest</c>-based tests that ask each mode for its actual deck.
    /// </para>
    /// </summary>
    [Fact]
    public void Readme_mode_count_matches_the_tree()
    {
        var root = FindRepositoryRoot();
        var readme = File.ReadAllText(Path.Combine(root, "README.md"));

        // Count what the registry actually holds, not BaseGameModeDefinition
        // subclasses. The subclass count was wrong in two directions at once: it
        // INCLUDED JsonGameMode, which was a runtime loader and never a catalogue
        // entry, and EXCLUDED the seven bespoke provider modes that are —
        // Millionaire, School Millionaire, Modern Love Millionaire, Monogamy,
        // Day One, Claimed! and SlangCheck.
        //
        // Removing JsonGameMode in 1.21.0 fixed half of that by accident and left
        // the other half, which is what made "90" and "97" both defensible and
        // neither correct. A player can reach 97 modes; that is the number the
        // README is telling them. See backlog item 13.
        // DistinctBy(Name), not AllModes.Count: a mode filed under two archetypes
        // is instantiated twice, and AllModes dedupes by reference, so the raw
        // count is 102 for 97 distinct modes. All 97 names are unique.
        var modes = ArchetypeRegistry.Default().AllModes.DistinctBy(m => m.Name).Count();

        var quoted = Regex.Match(readme, @"\((?<modes>[\d,]+) modes, (?<cards>[\d,]+) cards\)");

        quoted.Success.Should().BeTrue(
            "README.md should carry a '(N modes, N cards)' line under src/TableTop.Games — " +
            "if the wording changed, update this test's pattern in the same commit");

        Number(quoted.Groups["modes"]).Should().Be(modes, "README mode count is stale");
    }

    /// <summary>
    /// Counts cards the same way <see cref="ArchetypeRegistry"/>'s own
    /// <c>SurpriseMe</c> filter does — via each mode's <see cref="ModeManifest"/>
    /// — rather than regex-scraping C# collection initialisers (backlog item 13
    /// named that trap explicitly and this avoids it). <c>GetManifest()</c>
    /// derives from whichever deck the controller for that mode's family will
    /// actually be handed, so this cannot disagree with what a player can
    /// actually deal — the same property item 10 fixed for Herd specifically.
    /// </summary>
    [Fact]
    public void Readme_card_count_matches_the_tree()
    {
        var root = FindRepositoryRoot();
        var readme = File.ReadAllText(Path.Combine(root, "README.md"));

        var cards = ArchetypeRegistry.Default().AllModes
            .DistinctBy(m => m.Name)
            .Sum(m => m.GetManifest().TotalCards);

        var quoted = Regex.Match(readme, @"\((?<modes>[\d,]+) modes, (?<cards>[\d,]+) cards\)");

        quoted.Success.Should().BeTrue(
            "README.md should carry a '(N modes, N cards)' line under src/TableTop.Games — " +
            "if the wording changed, update this test's pattern in the same commit");

        Number(quoted.Groups["cards"]).Should().Be(cards, "README card count is stale");
    }

    /// <summary>
    /// Counts test cases the way <c>dotnet test</c> reports them — one per
    /// <c>[Fact]</c> method, one per <c>[InlineData]</c> row on a
    /// <c>[Theory]</c> method — rather than trusting a number typed into prose
    /// (backlog item 13: README said 776 while ARCHITECTURE.md said "roughly
    /// 900" and neither was checked). <see cref="TheoryAttribute"/> derives
    /// from <see cref="FactAttribute"/>, so a Theory method is checked first;
    /// otherwise it would double-count as a Fact too.
    ///
    /// <para>
    /// Only covers <c>[InlineData]</c>. This assembly has no
    /// <c>[MemberData]</c> or <c>[ClassData]</c> today (checked when this test
    /// was written) — if one is added later without updating this method, the
    /// count will silently undercount rather than fail loudly. Worth knowing
    /// before trusting this test blindly on a future assembly shape.
    /// </para>
    /// </summary>
    [Fact]
    public void Readme_test_count_matches_the_assembly()
    {
        var root = FindRepositoryRoot();
        var readme = File.ReadAllText(Path.Combine(root, "README.md"));

        var testCases = typeof(DocumentationAccuracyTests).Assembly
            .GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance |
                                           BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Sum(CountTestCases);

        var quoted = Regex.Match(readme, @"TableTop\.Tests/\s*←\s*(?<count>[\d,]+) tests");

        quoted.Success.Should().BeTrue(
            "README.md should carry a 'TableTop.Tests/ ← N tests' line under tests/ — " +
            "if the wording changed, update this test's pattern in the same commit");

        Number(quoted.Groups["count"]).Should().Be(testCases, "README test count is stale");
    }

    private static int CountTestCases(MethodInfo method)
    {
        if (method.IsDefined(typeof(TheoryAttribute), inherit: false))
            return method.GetCustomAttributes<InlineDataAttribute>(inherit: false).Count();

        return method.IsDefined(typeof(FactAttribute), inherit: false) ? 1 : 0;
    }

    [Fact]
    public void Readme_lists_every_project_in_the_solution()
    {
        var root = FindRepositoryRoot();
        var readme = File.ReadAllText(Path.Combine(root, "README.md"));

        var projects = Directory
            .EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Where(NotBuildOutput)
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => n is not null)
            .Select(n => n!)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        var missing = projects.Where(p => !readme.Contains(p, StringComparison.Ordinal)).ToList();

        missing.Should().BeEmpty(
            "the README's solution structure should name every project — WinUI and UiTests were both " +
            $"absent from it before this test existed. Missing: {string.Join(", ", missing)}");
    }

    private static bool NotBuildOutput(string path) =>
        !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
        !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal);

    private static int Number(Group g) => int.Parse(g.Value.Replace(",", ""));

    private static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "README.md")) &&
                Directory.Exists(Path.Combine(dir.FullName, "src", "TableTop.Core")))
                return dir.FullName;
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the repository root from '{AppContext.BaseDirectory}'.");
    }
}
