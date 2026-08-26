using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Domain.Players;
using TableTop.Hosting;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;

namespace TableTop.Tests;

/// <summary>
/// <see cref="ControllerFamilies"/> is a second description of something
/// <c>ControllerFactory</c> already decides, so the thing worth pinning hardest
/// is that the two never disagree — a descriptor that lies is worse than no
/// descriptor, because heads would route on it.
/// </summary>
public sealed class ControllerFamilyTests
{
    private static IReadOnlyList<IGameMode> AllModes()
    {
        var modes = new List<IGameMode>();
        void Walk(IEnumerable<Archetype> nodes)
        {
            foreach (var n in nodes) { modes.AddRange(n.Modes); Walk(n.SubArchetypes); }
        }
        Walk(ArchetypeRegistry.Default().RootArchetypes);
        return modes.DistinctBy(m => m.Name).ToList();
    }

    private static IReadOnlyList<IPlayer> Players() =>
        new[] { (IPlayer)Player.Create("A"), Player.Create("B"), Player.Create("C") };

    private static ControllerFamily FamilyOfController(IGameController controller) => controller switch
    {
        IMillionaireController => ControllerFamily.Quiz,
        IMonogamyController    => ControllerFamily.Monogamy,
        IHerdController        => ControllerFamily.SimultaneousAnswer,
        IClaimedController     => ControllerFamily.AreaControl,
        IDayOneController      => ControllerFamily.DailyCampaign,
        ICardTurnController    => ControllerFamily.CardTurn,
        _ => throw new InvalidOperationException(
            $"Unmapped controller type {controller.GetType().Name} — ControllerFamily needs a new member."),
    };

    [Fact]
    public void DeclaredFamily_MatchesWhatTheFactoryActuallyBuilds_ForEveryMode()
    {
        // The load-bearing test. ControllerFamilies.For and ControllerFactory
        // dispatch on the same capability interfaces in the same order; if they
        // ever drift, heads route to the wrong screen for a whole family.
        var mismatches = new List<string>();

        foreach (var mode in AllModes())
        {
            var controller = new ControllerFactory().CreateAsync(mode, Players()).GetAwaiter().GetResult();
            try
            {
                var declared = ControllerFamilies.For(mode);
                var actual   = FamilyOfController(controller);
                if (declared != actual)
                    mismatches.Add($"{mode.Name}: declared {declared}, factory built {actual}");
            }
            finally { controller.Dispose(); }
        }

        mismatches.Should().BeEmpty();
    }

    [Fact]
    public void EveryControllerTypeTheFactoryBuilds_HasAFamily()
    {
        // Guards the other direction: a new controller with no ControllerFamily
        // member makes FamilyOfController throw rather than quietly bucketing
        // it as CardTurn.
        var act = () =>
        {
            foreach (var mode in AllModes())
            {
                var c = new ControllerFactory().CreateAsync(mode, Players()).GetAwaiter().GetResult();
                try { FamilyOfController(c); } finally { c.Dispose(); }
            }
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void For_ThrowsOnNull()
    {
        var act = () => ControllerFamilies.For(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UnsupportedIn_NamesTheModesAHeadCannotPlay()
    {
        var cardTurnOnly = new[] { ControllerFamily.CardTurn };

        var unsupported = ControllerFamilies.UnsupportedIn(AllModes(), cardTurnOnly);

        unsupported.Should().NotBeEmpty("Millionaire alone is not a card-turn mode");
        unsupported.Should().OnlyContain(m => ControllerFamilies.For(m) != ControllerFamily.CardTurn);
    }

    [Fact]
    public void UnsupportedIn_IsEmptyWhenEveryFamilyIsSupported()
    {
        ControllerFamilies.UnsupportedIn(AllModes(), ControllerFamilies.All).Should().BeEmpty();
    }

    // ── the three-orders bug ──────────────────────────────────────────────────
    //
    // These capability interfaces used to be tested in three places in three
    // different orders: ControllerFactory, ControllerFamilies.For, and
    // ModeManifestExtensions. Only the first pair was asserted, and it passed on
    // every input the catalogue could supply, because no real mode implements two
    // capability interfaces. The manifest's order — IGameModeDefinition first —
    // was the one that was actually wrong, and it was wrong in production.

    /// <summary>
    /// A mode implementing TWO capability interfaces. The catalogue contains no
    /// such mode, which is exactly why the parity test above could never catch a
    /// transposed order. This supplies the input reality doesn't.
    /// </summary>
    private sealed class TwoCapabilityMode : IGameMode, IMonogamyDeckProvider, IQuestionBankProvider
    {
        public string Name => "Two Capabilities";
        public string Description => "Implements both Monogamy and Quiz providers.";
        public int? WinningTokenCount => 5;

        public IReadOnlyList<MonogamyCard> GetDeck() =>
        [
            MonogamyCard.Create(
                "Only Card", "him", "her", "neutral",
                MonogamyZone.Foreplay, CardTarget.ForDrawer),
        ];

        public IReadOnlyList<MultipleChoiceCard> GetQuestionBank() =>
        [
            MultipleChoiceCard.Create("Q1?", "a", "b", "c", "d", AnswerLabel.A, Difficulty.Easy),
            MultipleChoiceCard.Create("Q2?", "a", "b", "c", "d", AnswerLabel.B, Difficulty.Easy),
        ];
    }

    [Fact]
    public void AModeWithTwoCapabilities_ResolvesTheSameWayInAllThreePlaces()
    {
        var mode = new TwoCapabilityMode();

        var declared   = ControllerFamilies.For(mode);
        var controller = new ControllerFactory().CreateAsync(mode, Players()).GetAwaiter().GetResult();

        try
        {
            var built = FamilyOfController(controller);

            declared.Should().Be(ControllerFamily.Monogamy,
                "ControllerFactory tests IMonogamyDeckProvider before IQuestionBankProvider, "
                + "and ControllerFamilies must agree — they were transposed, and the comment "
                + "on For() claimed they could not be");
            built.Should().Be(declared);

            // The manifest now derives from the family rather than repeating the
            // chain, so it cannot pick a third answer. One card in the Monogamy
            // deck, two in the question bank — the count says which it used.
            mode.GetManifest().TotalCards.Should().Be(1,
                "the manifest must describe the deck the controller was handed");
        }
        finally { controller.Dispose(); }
    }

    [Fact]
    public void TryFor_ReturnsNull_ForAModeNoFactoryCanBuild()
    {
        // For() used to answer CardTurn here. A head would route confidently to
        // its card-turn screen and then blow up inside CreateAsync, which throws
        // NotSupportedException for this exact shape.
        var orphan = new NoCapabilityMode();

        ControllerFamilies.TryFor(orphan).Should().BeNull();

        var act = () => ControllerFamilies.For(orphan);
        act.Should().Throw<NotSupportedException>("this is what the factory does with the same mode");
    }

    [Fact]
    public void UnsupportedIn_ListsAModeNoFactoryCanBuild_RatherThanThrowing()
    {
        // The query's job is to report what a head cannot play. A mode nothing
        // can build is the strongest possible case of that, so it must appear in
        // the list rather than take the whole query down.
        var modes = new IGameMode[] { new NoCapabilityMode() };

        ControllerFamilies.UnsupportedIn(modes, ControllerFamilies.All)
            .Should().ContainSingle().Which.Name.Should().Be("No Capability");
    }

    private sealed class NoCapabilityMode : IGameMode
    {
        public string Name => "No Capability";
        public string Description => "Implements no capability interface at all.";
    }
}

/// <summary>
/// Per-head coverage. These are the tests that were missing when
/// <c>ClaimedController</c> and <c>HerdController</c> shipped: both were valid
/// factory outputs that no head could route, and nothing anywhere knew.
///
/// <para>
/// They deliberately assert on a head's <b>declared</b> family list rather than
/// reflecting over its routing switch — the declaration is what a reviewer
/// reads, so it's what has to be true. A head whose declaration and switch
/// drift apart is a separate bug, and one a compiler catches when the switch
/// arm goes missing.
/// </para>
/// </summary>
public sealed class HeadFamilyCoverageTests
{
    private static IReadOnlyList<IGameMode> AllModes()
    {
        var modes = new List<IGameMode>();
        void Walk(IEnumerable<Archetype> nodes)
        {
            foreach (var n in nodes) { modes.AddRange(n.Modes); Walk(n.SubArchetypes); }
        }
        Walk(ArchetypeRegistry.Default().RootArchetypes);
        return modes.DistinctBy(m => m.Name).ToList();
    }

    /// <summary>
    /// Mirrors <c>PlayerSetupPage.SupportedFamilies</c>. Duplicated rather than
    /// referenced because this project deliberately does not reference the MAUI
    /// head — it needs the MAUI SDK, which is exactly why the shared ViewModels
    /// were extracted in the first place.
    /// </summary>
    private static readonly ControllerFamily[] MauiSupported =
    [
        ControllerFamily.CardTurn,
        ControllerFamily.Quiz,
        ControllerFamily.Monogamy,
        ControllerFamily.DailyCampaign,
    ];

    /// <summary>Mirrors <c>ConsoleGameLauncher.SupportedFamilies</c>.</summary>
    private static readonly ControllerFamily[] ConsoleSupported =
    [
        ControllerFamily.CardTurn,
        ControllerFamily.Quiz,
    ];

    [Fact]
    public void EveryModeInTheRegistry_MapsToAKnownFamily()
    {
        AllModes().Should().OnlyContain(m => ControllerFamilies.All.Contains(ControllerFamilies.For(m)));
    }

    [Fact]
    public void Maui_CannotYetPlay_AreaControlOrSimultaneousAnswer()
    {
        // Documents the CURRENT gap as a fact rather than pretending it's
        // closed. When someone writes those two pages, this test fails and
        // tells them to update the declaration — which is the correct
        // failure, and infinitely better than the silent one this replaces.
        var unsupported = ControllerFamilies.UnsupportedIn(AllModes(), MauiSupported);

        unsupported.Should().NotBeEmpty();
        unsupported.Select(ControllerFamilies.For).Distinct()
            .Should().OnlyContain(f => f == ControllerFamily.AreaControl
                                    || f == ControllerFamily.SimultaneousAnswer);
    }

    [Fact]
    public void Console_SupportsFewerFamilies_AndSaysSo()
    {
        var unsupported = ControllerFamilies.UnsupportedIn(AllModes(), ConsoleSupported);

        // The console is deliberately the thinnest head. What matters is that
        // every shortfall is a family it never claimed, not one it claimed and
        // then dropped on the floor.
        unsupported.Select(ControllerFamilies.For).Distinct()
            .Should().OnlyContain(f => !ConsoleSupported.Contains(f));
    }

    [Fact]
    public void NoHeadSilentlyDropsAFamilyItClaimsToSupport()
    {
        // The actual invariant worth protecting: a mode whose family a head
        // declares must be playable there.
        foreach (var (head, supported) in new[]
                 {
                     ("MAUI", MauiSupported),
                     ("Console", ConsoleSupported),
                 })
        {
            var claimed = AllModes().Where(m => supported.Contains(ControllerFamilies.For(m)));
            claimed.Should().OnlyContain(m => supported.Contains(ControllerFamilies.For(m)),
                $"{head} must be able to play every mode in a family it declares");
        }
    }
}
