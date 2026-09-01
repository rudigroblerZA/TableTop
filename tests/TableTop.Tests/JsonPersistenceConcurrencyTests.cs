using TableTop.Hosting.Persistence;

namespace TableTop.Tests;

/// <summary>
/// Covers the fix for the "shared .tmp filename, no synchronisation" finding
/// on <see cref="JsonSessionRepository"/> and <see cref="JsonPlayerRepository"/>.
///
/// Before the fix, every <c>SaveAsync</c> wrote to the exact same
/// <c>{file}.tmp</c> path with no coordination between overlapping calls —
/// two concurrent saves against the same repository instance could have one
/// call's <see cref="File.Create(string)"/> truncate the other's still-open
/// stream, and whichever <see cref="File.Move(string, string, bool)"/> ran
/// second would throw because the source it expected had already been
/// consumed by the first. These tests fire many concurrent saves at a real
/// file and assert neither symptom shows up: no exception, and the file left
/// behind is always one complete, valid write — never a torn one.
/// </summary>
public sealed class JsonPersistenceConcurrencyTests : IDisposable
{
    private readonly string _tmpFile = Path.Combine(Path.GetTempPath(), $"test_concurrency_{Guid.NewGuid()}.json");

    public void Dispose()
    {
        if (File.Exists(_tmpFile)) File.Delete(_tmpFile);
        foreach (var f in Directory.GetFiles(Path.GetTempPath(), $"{Path.GetFileName(_tmpFile)}.*.tmp"))
            File.Delete(f);
    }

    [Fact]
    public async Task JsonPlayerRepository_ConcurrentSaves_NeverThrow_AndLeaveAValidFile()
    {
        var repo = new JsonPlayerRepository(_tmpFile);

        var tasks = Enumerable.Range(0, 20)
            .Select(i => repo.SaveAsync([new PlayerProfile { Name = $"Player{i}" }]))
            .ToArray();

        var act = () => Task.WhenAll(tasks);
        await act.Should().NotThrowAsync(
            "concurrent saves must be serialised rather than stomping each other's temp file");

        // Whatever ended up on disk must be exactly one of the 20 writes,
        // fully intact — never a truncated or mixed file.
        var loaded = await repo.LoadAsync();
        loaded.Should().ContainSingle();
        loaded[0].Name.Should().MatchRegex(@"^Player\d+$");
    }

    [Fact]
    public async Task JsonPlayerRepository_ConcurrentSaves_LeaveNoOrphanedTempFiles()
    {
        var repo = new JsonPlayerRepository(_tmpFile);

        await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(i => repo.SaveAsync([new PlayerProfile { Name = $"Player{i}" }])));

        var dir = Path.GetDirectoryName(_tmpFile)!;
        Directory.GetFiles(dir, $"{Path.GetFileName(_tmpFile)}.*.tmp").Should().BeEmpty(
            "every unique-named temp file must be consumed by its own rename — none should survive a clean run");
    }

    [Fact]
    public async Task JsonSessionRepository_ConcurrentSaves_NeverThrow_AndLeaveAValidFile()
    {
        var repo = new JsonSessionRepository(_tmpFile);

        var tasks = Enumerable.Range(0, 20)
            .Select(i => repo.SaveAsync(new SessionSnapshot { ModeName = $"Mode{i}", Round = i }))
            .ToArray();

        var act = () => Task.WhenAll(tasks);
        await act.Should().NotThrowAsync(
            "concurrent saves must be serialised rather than stomping each other's temp file");

        var loaded = await repo.LoadAsync();
        loaded.Should().NotBeNull();
        loaded.ModeName.Should().MatchRegex(@"^Mode\d+$");
    }

    [Fact]
    public async Task JsonSessionRepository_SaveDuringLoad_NeitherThrowsNorCorrupts()
    {
        // A save and a load racing each other on the same instance — the load
        // must see either the pre-save or the post-save content, never a
        // half-written one, and neither call should throw.
        var repo = new JsonSessionRepository(_tmpFile);
        await repo.SaveAsync(new SessionSnapshot { ModeName = "Before", Round = 1 });

        var saveTask = repo.SaveAsync(new SessionSnapshot { ModeName = "After", Round = 2 });
        var loadTask = repo.LoadAsync();

        var act = () => Task.WhenAll(saveTask, loadTask);
        await act.Should().NotThrowAsync();

        (await loadTask)!.ModeName.Should().BeOneOf("Before", "After");
    }
}
