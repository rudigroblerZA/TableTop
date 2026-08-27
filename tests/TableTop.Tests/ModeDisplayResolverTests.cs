using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;
using TableTop.Games.Family;
using TableTop.Games.Fun;
using TableTop.Presentation.ViewModels;

namespace TableTop.Tests;

/// <summary>
/// Zero test references before this. Exists to guard the exact bug both
/// classes were extracted to fix: WinUI's game list bound
/// <c>{Binding Name}</c> directly on the raw <c>IGameMode</c>, so a
/// <see cref="BaseGameModeDefinition"/> subclass's own name never rendered
/// there, even after MAUI had already fixed the same problem for itself.
/// </summary>
public sealed class ModeDisplayResolverTests
{
    /// <summary>A bare <see cref="IGameMode"/>, not a <see cref="BaseGameModeDefinition"/> — no display fields exist at all.</summary>
    private sealed class RawMode(string name, string description) : IGameMode
    {
        public string Name => name;
        public string Description => description;
    }

    private sealed class CompiledMode : BaseGameModeDefinition
    {
        public override string Name => "Compiled Name";
        public override string Description => "Compiled description";
        protected override IScoringStrategy BuildScoring() => new FixedScoringStrategy(1);
        protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) => [];
    }

    [Fact]
    public void Resolve_WithABareIGameMode_UsesItsNameAndDescriptionDirectly()
    {
        var mode = new RawMode("Raw Name", "Raw description");

        var (title, description) = ModeDisplayResolver.Resolve(mode);

        title.Should().Be("Raw Name");
        description.Should().Be("Raw description");
    }

    [Fact]
    public void Resolve_WithABaseGameModeDefinition_UsesTheCompiledName()
    {
        var mode = new CompiledMode();

        var (title, description) = ModeDisplayResolver.Resolve(mode);

        title.Should().Be("Compiled Name");
        description.Should().Be("Compiled description");
    }

    [Fact]
    public void Resolve_DelegatesExactlyToNameAndDescription_AcrossRealModes()
    {
        foreach (var mode in new BaseGameModeDefinition[]
        {
            new OneStarReviewsMode(), new AllTogetherNowMode(), new SplitTheRoomMode(),
        })
        {
            var (title, description) = ModeDisplayResolver.Resolve(mode);
            title.Should().Be(mode.Name);
            description.Should().Be(mode.Description);
        }
    }
}

/// <summary>Zero test references before this.</summary>
public sealed class ModeListItemTests
{
    [Fact]
    public void Constructor_ThrowsOnNullMode()
    {
        var act = () => new ModeListItem(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WrapsTheSameResolutionAsModeDisplayResolver()
    {
        var mode = new OneStarReviewsMode();
        var item = new ModeListItem(mode);
        var (title, description) = ModeDisplayResolver.Resolve(mode);

        item.Mode.Should().BeSameAs(mode);
        item.Title.Should().Be(title);
        item.Description.Should().Be(description);
    }
}
