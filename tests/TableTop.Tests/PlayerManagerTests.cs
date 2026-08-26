using TableTop.Core.Abstractions.Players;

namespace TableTop.Tests;

public sealed class PlayerManagerTests
{
    private static Player MakePlayer(string name) => Player.Create(name);

    [Fact]
    public void AddPlayer_IncreasesCount()
    {
        var manager = new RoundRobinPlayerManager();
        manager.AddPlayer(MakePlayer("Alice"));
        manager.Players.Should().HaveCount(1);
    }

    [Fact]
    public void AddPlayer_DuplicateId_Throws()
    {
        var manager = new RoundRobinPlayerManager();
        var player = MakePlayer("Alice");
        manager.AddPlayer(player);
        Xunit.Assert.Throws<InvalidOperationException>(() => manager.AddPlayer(player));
    }

    [Fact]
    public void GetNextPlayer_CyclesThroughActive()
    {
        var manager = new RoundRobinPlayerManager();
        var alice = MakePlayer("Alice");
        var bob = MakePlayer("Bob");
        manager.AddPlayer(alice);
        manager.AddPlayer(bob);

        manager.GetNextPlayer()!.DisplayName.Should().Be("Alice");
        manager.GetNextPlayer()!.DisplayName.Should().Be("Bob");
        manager.GetNextPlayer()!.DisplayName.Should().Be("Alice");
    }

    [Fact]
    public void GetNextPlayer_SkipsInactivePlayers()
    {
        var manager = new RoundRobinPlayerManager();
        var alice = MakePlayer("Alice");
        var bob = MakePlayer("Bob");
        manager.AddPlayer(alice);
        manager.AddPlayer(bob);
        manager.SetStatus(alice.Id, PlayerStatus.Skipped);

        manager.GetNextPlayer()!.DisplayName.Should().Be("Bob");
        manager.GetNextPlayer()!.DisplayName.Should().Be("Bob");
    }

    [Fact]
    public void ApplyScore_AccumulatesCorrectly()
    {
        var manager = new RoundRobinPlayerManager();
        var alice = MakePlayer("Alice");
        manager.AddPlayer(alice);

        manager.ApplyScore(alice.Id, 5);
        manager.ApplyScore(alice.Id, 3);

        manager.Players.First(p => p.Id == alice.Id).Score.Should().Be(8);
    }

    [Fact]
    public void RemovePlayer_ReducesCount()
    {
        var manager = new RoundRobinPlayerManager();
        var alice = MakePlayer("Alice");
        manager.AddPlayer(alice);
        manager.RemovePlayer(alice.Id);
        manager.Players.Should().BeEmpty();
    }

    [Fact]
    public void RemovePlayer_UnknownId_Throws()
    {
        var manager = new RoundRobinPlayerManager();
        Xunit.Assert.Throws<KeyNotFoundException>(() => manager.RemovePlayer(Guid.NewGuid()));
    }
}