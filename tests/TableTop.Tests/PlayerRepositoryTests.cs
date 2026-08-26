using TableTop.Hosting.Persistence;

namespace TableTop.Tests;

public sealed class PlayerRepositoryTests : IDisposable
{
    private readonly string _tmpFile = Path.Combine(Path.GetTempPath(), $"test_players_{Guid.NewGuid()}.json");
    private JsonPlayerRepository Repo() => new(_tmpFile);

    public void Dispose()
    {
        if (File.Exists(_tmpFile)) File.Delete(_tmpFile);
        if (File.Exists(_tmpFile + ".tmp")) File.Delete(_tmpFile + ".tmp");
    }

    [Fact]
    public async Task SaveAndLoad_RoundTripsAllFields()
    {
        var profile = new PlayerProfile
        {
            Id = Guid.NewGuid(),
            Name = "Alice",
            Gender = "female",
            Age = 30,
            IsParent = true,
            IsMarried = true,
            IsCoupleMember = false,
        };

        var repo = Repo();
        await repo.SaveAsync([profile]);
        var loaded = await repo.LoadAsync();

        loaded.Should().HaveCount(1);
        var l = loaded[0];
        l.Id.Should().Be(profile.Id);
        l.Name.Should().Be("Alice");
        l.Gender.Should().Be("female");
        l.Age.Should().Be(30);
        l.IsParent.Should().BeTrue();
        l.IsMarried.Should().BeTrue();
        l.IsCoupleMember.Should().BeFalse();
    }

    [Fact]
    public async Task Load_WhenFileAbsent_ReturnsEmpty()
    {
        var loaded = await Repo().LoadAsync();
        loaded.Should().BeEmpty();
    }

    [Fact]
    public async Task Save_MultiplePlayers_PreservesOrder()
    {
        var profiles = new[]
        {
            new PlayerProfile { Name = "Alpha", Gender = "male",   Age = 20 },
            new PlayerProfile { Name = "Beta",  Gender = "female", Age = 25 },
            new PlayerProfile { Name = "Gamma", Gender = "other",  Age = 30 },
        };

        var repo = Repo();
        await repo.SaveAsync(profiles);
        var loaded = await repo.LoadAsync();

        loaded.Select(p => p.Name).First().Should().Be("Alpha");
    }

    [Fact]
    public async Task Save_OverwritesPreviousSave()
    {
        var repo = Repo();
        await repo.SaveAsync([new PlayerProfile { Name = "Old" }]);
        await repo.SaveAsync([new PlayerProfile { Name = "New" }]);

        var loaded = await repo.LoadAsync();
        loaded.Should().HaveCount(1);
        loaded[0].Name.Should().Be("New");
    }

    [Fact]
    public async Task Clear_RemovesFile()
    {
        var repo = Repo();
        await repo.SaveAsync([new PlayerProfile { Name = "Alice" }]);
        await repo.ClearAsync();

        var loaded = await repo.LoadAsync();
        loaded.Should().BeEmpty();
    }

    [Fact]
    public async Task Load_CorruptJson_ReturnsEmpty()
    {
        await File.WriteAllTextAsync(_tmpFile, "{ this is not valid json }}}");
        var loaded = await Repo().LoadAsync();
        loaded.Should().BeEmpty();
    }

    [Fact]
    public async Task ToPlayer_SetsTagsFromProfile()
    {
        var profile = new PlayerProfile
        {
            Name = "Bob",
            Gender = "male",
            Age = 40,
            IsParent = true,
            IsMarried = false,
            IsCoupleMember = true,
        };

        var player = profile.ToPlayer();

        player.DisplayName.Should().Be("Bob");
        player.Tags.Should().Contain(x => x == "adult");
        player.Tags.Should().Contain(x => x == "parent");
        player.Tags.Should().Contain(x => x == "couple-member");
        player.Tags.Should().NotContain(x => x == "married");
        player.Attributes["gender"].Should().Be("male");
    }

    [Fact]
    public async Task ToPlayer_PreservesProfileId()
    {
        var id = Guid.NewGuid();
        var profile = new PlayerProfile { Id = id, Name = "Carol", Age = 22 };
        var player = profile.ToPlayer();
        player.Id.Should().Be(id);
    }

    [Fact]
    public async Task IsAdult_DerivedFromAge()
    {
        new PlayerProfile { Age = 17 }.IsAdult.Should().BeFalse();
        new PlayerProfile { Age = 18 }.IsAdult.Should().BeTrue();
        new PlayerProfile { Age = 25 }.IsAdult.Should().BeTrue();
    }
}