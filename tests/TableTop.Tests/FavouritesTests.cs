using TableTop.Core.Abstractions.Game;
using TableTop.Hosting.Persistence;

namespace TableTop.Tests;

/// <summary>
/// Starred modes: the in-memory set a picker reads while it renders, and the
/// JSON file behind it.
///
/// <para>
/// The behaviours worth pinning are the ones that fail quietly. A star that is
/// not written survives until the app closes and then is not there. A rollback
/// that does not happen leaves the list showing a star that exists nowhere. And
/// a reorder that is not stable renumbers a console menu under the player's
/// fingers between reading it and typing.
/// </para>
/// </summary>
public sealed class FavouritesTests : IDisposable
{
    private readonly string _dir =
        Path.Combine(Path.GetTempPath(), $"tabletop-favs-{Guid.NewGuid():N}");

    private string FilePath => Path.Combine(_dir, "favourites.json");

    private FavouritesService NewService() =>
        new(new JsonFavouritesRepository(FilePath));

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    private static IGameMode ModeNamed(string name) => new StubMode(name);

    private sealed class StubMode(string name) : IGameMode
    {
        public string Name { get; } = name;
        public string Description => "stub";
    }

    // ── Round trip ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AStarSurvivesARestart()
    {
        var first = NewService();
        await first.LoadAsync();
        await first.ToggleAsync(ModeNamed("Herd"));

        // A brand new service over the same file — this is what "restart" means
        // for a feature whose whole point is persistence.
        var second = NewService();
        await second.LoadAsync();

        second.IsFavourite("Herd").Should().BeTrue();
        second.Count.Should().Be(1);
    }

    [Fact]
    public async Task TogglingTwiceLeavesNothingStarred()
    {
        var service = NewService();
        await service.LoadAsync();

        (await service.ToggleAsync(ModeNamed("Herd"))).Should().BeTrue();
        (await service.ToggleAsync(ModeNamed("Herd"))).Should().BeFalse();

        service.IsFavourite("Herd").Should().BeFalse();

        var reloaded = NewService();
        await reloaded.LoadAsync();
        reloaded.Count.Should().Be(0, "the unstar has to reach the file too");
    }

    [Fact]
    public async Task LoadingWithNoFileYet_IsEmptyRatherThanAnError()
    {
        // First launch. The repository must not treat a missing file as failure.
        var service = NewService();
        await service.LoadAsync();

        service.Count.Should().Be(0);
        File.Exists(FilePath).Should().BeFalse("loading should not create the file");
    }

    [Fact]
    public async Task ACorruptFile_CostsTheStarsAndNotTheLaunch()
    {
        Directory.CreateDirectory(_dir);
        await File.WriteAllTextAsync(FilePath, "{ this is not the array it used to be");

        var service = NewService();
        await service.LoadAsync();

        service.Count.Should().Be(0);
    }

    [Fact]
    public async Task BlankEntriesInAHandEditedFile_AreDropped()
    {
        Directory.CreateDirectory(_dir);
        await File.WriteAllTextAsync(FilePath, "[\"Herd\", \"\", null, \"  \"]");

        var service = NewService();
        await service.LoadAsync();

        service.Names.Should().ContainSingle().Which.Should().Be("Herd");
    }

    // ── Matching ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task NamesMatchCaseInsensitively()
    {
        var service = NewService();
        await service.LoadAsync();
        await service.ToggleAsync("Herd");

        service.IsFavourite("herd").Should().BeTrue();
        service.IsFavourite("HERD").Should().BeTrue();
    }

    [Fact]
    public void AnUnknownNameIsSimplyNotAFavourite()
    {
        NewService().IsFavourite("Nothing By This Name").Should().BeFalse();
    }

    [Fact]
    public async Task ABlankNameIsRejectedRatherThanStarred()
    {
        var service = NewService();
        service.IsFavourite("").Should().BeFalse();

        var act = () => service.ToggleAsync("   ");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ── Ordering ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task FavouritesFirst_KeepsBothGroupsInTheirOriginalOrder()
    {
        // Stability is the point: an unstable sort would move every unstarred
        // row the moment a player stars anything, in a menu they select from by
        // number.
        var service = NewService();
        await service.LoadAsync();
        await service.ToggleAsync("C");

        IGameMode[] modes = [ModeNamed("A"), ModeNamed("B"), ModeNamed("C"), ModeNamed("D")];

        service.FavouritesFirst(modes).Select(m => m.Name)
            .Should().Equal("C", "A", "B", "D");
    }

    [Fact]
    public void FavouritesFirst_WithNothingStarred_ChangesNothing()
    {
        IGameMode[] modes = [ModeNamed("A"), ModeNamed("B")];

        NewService().FavouritesFirst(modes).Select(m => m.Name)
            .Should().Equal("A", "B");
    }

    [Fact]
    public async Task FilterFavourites_DropsAStarredModeThatNoLongerExists()
    {
        // Renaming a mode drops its favourites, exactly as it already drops its
        // saved sessions. Filtering the caller's list rather than resolving
        // names against the registry is what makes that a non-event instead of
        // a lookup failure to handle.
        var service = NewService();
        await service.LoadAsync();
        await service.ToggleAsync("A Mode That Was Renamed");
        await service.ToggleAsync("Herd");

        service.FilterFavourites([ModeNamed("Herd")]).Select(m => m.Name)
            .Should().Equal("Herd");
    }

    // ── Events ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TogglingRaisesAnEventSoAListCanRefresh()
    {
        var service = NewService();
        await service.LoadAsync();

        FavouriteChangedEventArgs? seen = null;
        service.FavouriteChanged += (_, e) => seen = e;

        await service.ToggleAsync("Herd");

        seen.Should().NotBeNull();
        seen!.ModeName.Should().Be("Herd");
        seen.IsFavourite.Should().BeTrue();
    }

    // ── Failure handling ──────────────────────────────────────────────────────

    [Fact]
    public async Task AFailedWriteRollsBackTheInMemoryStar()
    {
        // Otherwise the picker shows a star that survived nowhere — worse than
        // the failure itself, because it looks like it worked.
        var service = new FavouritesService(new ThrowingRepository());

        var act = () => service.ToggleAsync("Herd");

        await act.Should().ThrowAsync<IOException>();
        service.IsFavourite("Herd").Should().BeFalse("the star must not survive a failed save");
    }

    [Fact]
    public async Task ClearRemovesEverything()
    {
        var service = NewService();
        await service.LoadAsync();
        await service.ToggleAsync("Herd");
        await service.ToggleAsync("Claimed!");

        await service.ClearAsync();

        service.Count.Should().Be(0);

        var reloaded = NewService();
        await reloaded.LoadAsync();
        reloaded.Count.Should().Be(0);
    }

    private sealed class ThrowingRepository : IFavouritesRepository
    {
        public Task<IReadOnlyList<string>> LoadAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<string>>([]);

        public Task SaveAsync(IEnumerable<string> modeNames, CancellationToken ct = default) =>
            throw new IOException("disk full");

        public Task ClearAsync(CancellationToken ct = default) =>
            throw new IOException("disk full");
    }
}
