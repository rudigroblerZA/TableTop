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
/// <c>{Binding Name}</c> directly on the raw <c>IGameMode</c>, so a deck's
/// JSON title override never rendered there, even after MAUI had already
/// fixed the same problem for itself.
///
/// A real sweep across every registered mode found <c>DisplayName == Name</c>
/// for all of them — no shipped deck's JSON title has ever actually diverged
/// from its compiled name. Worth stating plainly: that is very likely *why*
/// the WinUI bug went unnoticed for as long as it did, and it means the
/// delegation itself has to be proven with a controlled fake deck below, not
/// found lying around in real content.
/// </summary>
public sealed class ModeDisplayResolverTests
{
    /// <summary>A bare <see cref="IGameMode"/>, not a <see cref="BaseGameModeDefinition"/> — no display fields exist at all.</summary>
    private sealed class RawMode(string name, string description) : IGameMode
    {
        public string Name => name;
        public string Description => description;
    }

    /// <summary>
    /// A plain <see cref="BaseGameModeDefinition"/>. It used to take a deck
    /// resource name so a test could feed it a JSON presentation block; with the
    /// JSON deck path gone there is nothing to point it at, and its
    /// <c>Presentation</c> is always None.
    /// </summary>
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

        var (title, description, accent) = ModeDisplayResolver.Resolve(mode);

        title.Should().Be("Raw Name");
        description.Should().Be("Raw description");
        accent.Should().BeNull();
    }

    [Fact]
    public void Resolve_WithABaseGameModeDefinition_UsesTheCompiledName()
    {
        var mode = new CompiledMode();

        var (title, description, accent) = ModeDisplayResolver.Resolve(mode);

        title.Should().Be("Compiled Name");
        description.Should().Be("Compiled description");
        accent.Should().BeNull();
    }

    // Resolve_WithAJsonTitleOverride_UsesTheOverride_NotTheCompiledName lived
    // here and was the positive half of this class: it wrote a deck file with a
    // presentation block, pointed the resolver at it, and proved the JSON title
    // beat the compiled Name. There is no JSON title to beat it with any more.
    //
    // The bug this class exists for is unaffected and still covered above —
    // WinUI bound {Binding Name} on the raw IGameMode instead of going through
    // ModeDisplayResolver. That is a routing mistake, not a content one, and it
    // would reappear identically with or without a JSON override to miss.

    [Fact]
    public void Resolve_DelegatesExactlyToDisplayNameAndDisplayDescription_AcrossRealModes()
    {
        foreach (var mode in new BaseGameModeDefinition[]
        {
            new OneStarReviewsMode(), new AllTogetherNowMode(), new SplitTheRoomMode(),
        })
        {
            var (title, description, accent) = ModeDisplayResolver.Resolve(mode);
            title.Should().Be(mode.DisplayName);
            description.Should().Be(mode.DisplayDescription);
            accent.Should().Be(mode.Theme?.Accent);
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
        var (title, description, accent) = ModeDisplayResolver.Resolve(mode);

        item.Mode.Should().BeSameAs(mode);
        item.Title.Should().Be(title);
        item.Description.Should().Be(description);
        item.Accent.Should().Be(accent);
    }

    [Fact]
    public void HasAccent_IsFalse_ForAModeThatBypassesTheBaseClass()
    {
        // Millionaire implements IGameMode directly, so Resolve's cast to
        // BaseGameModeDefinition yields null and Accent stays null. That branch
        // is the reason Accent is nullable at all.
        var item = new ModeListItem(new TableTop.Games.MillionaireMode());
        item.HasAccent.Should().BeFalse();
    }

    [Fact]
    public void HasAccent_IsFalse_ForABaseGameModeDefinition_BecauseNothingCanSupplyAnAccent()
    {
        // This was HasAccent_IsTrue_WhenTheDeckDeclaresOne, asserted of
        // UndividedMode because its .deck.json carried a theme with an accent.
        // Theme resolves through Presentation, Presentation is now always None,
        // and no deck files remain — so no mode in the catalogue can declare an
        // accent and HasAccent is false everywhere.
        //
        // Kept as the negative rather than deleted: HasAccent is bound in both
        // heads, and if some future content source makes it true again, this is
        // the test that should be reconsidered rather than silently satisfied.
        var undivided = new TableTop.Games.Couples.UndividedMode();
        var item = new ModeListItem(undivided);

        undivided.Theme.Should().BeNull("nothing can populate Presentation.Theme any more");
        item.Accent.Should().BeNull();
        item.HasAccent.Should().BeFalse();
    }
}
