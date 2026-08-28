using TableTop.Hosting.Persistence;

namespace TableTop.Tests;

/// <summary>
/// Backlog item 28: Console gained a roster builder, backed by
/// <see cref="JsonRosterRepository"/> in Hosting (rather than a head-local
/// store) precisely so the persistence is covered here. Mirrors
/// <see cref="PlayerRepositoryTests"/> — the two are line-for-line siblings.
/// </summary>
public sealed class RosterRepositoryTests : IDisposable
{
    private readonly string _tmpFile = Path.Combine(Path.GetTempPath(), $"test_rosters_{Guid.NewGuid()}.json");
    private JsonRosterRepository Repo() => new(_tmpFile);

    public void Dispose()
    {
        if (File.Exists(_tmpFile)) File.Delete(_tmpFile);
        foreach (var stray in Directory.EnumerateFiles(
                     Path.GetDirectoryName(_tmpFile)!, Path.GetFileName(_tmpFile) + ".*"))
            File.Delete(stray);
    }

    private static RosterProfile Roster(string name, params string[] playerNames) => new()
    {
        Name = name,
        Players = playerNames.Select(n => new PlayerProfile { Name = n, Gender = "other", Age = 25 }).ToList(),
    };

    [Fact]
    public async Task SaveAndLoad_RoundTripsNameAndPlayers()
    {
        var repo = Repo();
        await repo.SaveAsync([Roster("Friday Regulars", "Alice", "Bob", "Cara")]);

        var loaded = await repo.LoadAsync();

        var r = loaded.Should().ContainSingle().Subject;
        r.Name.Should().Be("Friday Regulars");
        r.SchemaVersion.Should().Be(RosterProfile.CurrentSchemaVersion);
        r.Players.Select(p => p.Name).Should().Equal("Alice", "Bob", "Cara");
        r.Summary.Should().Be("3 players: Alice, Bob, Cara");
    }

    [Fact]
    public async Task Load_WhenFileAbsent_ReturnsEmpty()
    {
        (await Repo().LoadAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Load_WhenFileIsCorrupt_ReturnsEmpty_RatherThanThrowing()
    {
        await File.WriteAllTextAsync(_tmpFile, "{ not json");

        (await Repo().LoadAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Save_ReplacesTheWholeList()
    {
        var repo = Repo();
        await repo.SaveAsync([Roster("A", "x"), Roster("B", "y")]);
        await repo.SaveAsync([Roster("C", "z")]);

        var loaded = await repo.LoadAsync();
        loaded.Select(r => r.Name).Should().Equal("C");
    }

    [Fact]
    public async Task Save_LeavesNoTempFilesBehind()
    {
        var repo = Repo();
        await repo.SaveAsync([Roster("A", "x")]);

        Directory.EnumerateFiles(Path.GetDirectoryName(_tmpFile)!, Path.GetFileName(_tmpFile) + ".*.tmp")
            .Should().BeEmpty();
    }

    [Fact]
    public async Task Clear_RemovesEverything()
    {
        var repo = Repo();
        await repo.SaveAsync([Roster("A", "x")]);

        await repo.ClearAsync();

        (await repo.LoadAsync()).Should().BeEmpty();
        File.Exists(_tmpFile).Should().BeFalse();
    }
}
