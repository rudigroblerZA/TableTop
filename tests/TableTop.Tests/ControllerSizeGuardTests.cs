namespace TableTop.Tests;

/// <summary>
/// Guards <c>CardTurnController</c> against re-growth.
///
/// Backlog item B.1 has been raised three times (769 → 708 → 866 lines). Five
/// services were extracted along the way and the file still ended up larger
/// than when the item was first opened, because extraction was competing with
/// new features being added to the same file and nothing was keeping score.
/// This test keeps score.
///
/// Two ceilings, because they fail for different reasons:
///
///   • <see cref="MaxCodeLines"/> is the one that matters. It counts only
///     substantive lines — no blanks, no lone braces, no comments — so adding
///     documentation never costs you budget. A rising number here means the
///     controller has taken on more work.
///
///   • <see cref="MaxRawLines"/> is a backstop against the file simply becoming
///     unreadable, whatever the cause.
///
/// <b>When this fails:</b> extract, don't raise the ceiling. The controller's
/// job is the turn loop; anything else belongs in
/// <c>Controllers/Services/</c> alongside <c>SkipPolicy</c>,
/// <c>EffectApplicator</c>, <c>TurnHistoryTracker</c>,
/// <c>SpecialCardCoordinator</c>, <c>FlowCoordinator</c>,
/// <c>PersistenceCoordinator</c>, <c>HintCoordinator</c>,
/// <c>SessionDeckFactory</c>, and <c>UndoCoordinator</c>. If a ceiling genuinely must move, move it in a
/// commit that does nothing else, so the decision is visible in review.
/// </summary>
public sealed class ControllerSizeGuardTests
{
    // Measured at 665 raw / 360 code. B.3 (a second constructor taking a
    // prebuilt deck) pushed this over budget; the guard caught it, and the fix
    // was D.1 (CardTurnControllerOptions) plus extracting UndoCoordinator —
    // not a raised ceiling.
    // Headroom is deliberately thin — a few lines, not a few hundred.
    private const int MaxRawLines = 700;
    private const int MaxCodeLines = 390;

    private const string ControllerPath = "src/TableTop.Hosting/Controllers/CardTurnController.cs";

    [Fact]
    public void CardTurnController_StaysUnderItsLineBudget()
    {
        var lines = File.ReadAllLines(Path.Combine(FindRepositoryRoot(), ControllerPath));
        var code = lines.Count(IsSubstantive);

        code.Should().BeLessThanOrEqualTo(MaxCodeLines,
            $"CardTurnController has {code} lines of code against a budget of {MaxCodeLines}. " +
            "Extract a collaborator into Controllers/Services/ rather than raising this number — " +
            "see the class remarks on this test for why the budget exists.");

        lines.Length.Should().BeLessThanOrEqualTo(MaxRawLines,
            $"CardTurnController is {lines.Length} lines against a backstop of {MaxRawLines}.");
    }

    /// <summary>
    /// The controller should stay a turn-loop orchestrator. Each of these types
    /// owns a responsibility that used to live inline; if one disappears, the
    /// work has probably moved back into the controller.
    /// </summary>
    [Theory]
    [InlineData("SkipPolicy.cs")]
    [InlineData("EffectApplicator.cs")]
    [InlineData("TurnHistoryTracker.cs")]
    [InlineData("SpecialCardCoordinator.cs")]
    [InlineData("FlowCoordinator.cs")]
    [InlineData("PersistenceCoordinator.cs")]
    [InlineData("HintCoordinator.cs")]
    [InlineData("SessionDeckFactory.cs")]
    [InlineData("UndoCoordinator.cs")]
    public void ExtractedService_StillExists(string fileName)
    {
        var path = Path.Combine(
            FindRepositoryRoot(), "src", "TableTop.Hosting", "Controllers", "Services", fileName);

        File.Exists(path).Should().BeTrue(
            $"'{fileName}' was extracted from CardTurnController to keep it focused on the turn loop. " +
            "If it was deliberately merged away, remove its case from this theory in the same commit.");
    }

    private static bool IsSubstantive(string line)
    {
        var t = line.Trim();
        return t.Length > 0
            && t != "{"
            && t != "}"
            && !t.StartsWith("//", StringComparison.Ordinal);
    }

    /// <summary>
    /// Locates the repository root by walking up from the test assembly's output
    /// directory, the same way <see cref="DeckManifestTests"/> finds mode sources.
    /// </summary>
    private static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "src", "TableTop.Hosting")))
                return dir.FullName;

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the repository root by walking up from '{AppContext.BaseDirectory}'. " +
            "This test assumes it runs from within the TableTop repository checkout.");
    }
}
