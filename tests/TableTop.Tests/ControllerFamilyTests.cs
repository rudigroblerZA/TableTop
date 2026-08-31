using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
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
        IMonogamyController => ControllerFamily.Monogamy,
        IHerdController => ControllerFamily.SimultaneousAnswer,
        IClaimedController => ControllerFamily.AreaControl,
        IDayOneController => ControllerFamily.DailyCampaign,
        ITraitProfileController => ControllerFamily.TraitProfile,
        ICardTurnController => ControllerFamily.CardTurn,
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
                var actual = FamilyOfController(controller);
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

        var declared = ControllerFamilies.For(mode);
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
        ControllerFamily.AreaControl,
        ControllerFamily.SimultaneousAnswer,
    ];

    /// <summary>Mirrors <c>ConsoleGameLauncher.SupportedFamilies</c>.</summary>
    private static readonly ControllerFamily[] ConsoleSupported =
    [
        ControllerFamily.CardTurn,
        ControllerFamily.Quiz,
        ControllerFamily.Monogamy,
        ControllerFamily.DailyCampaign,
        ControllerFamily.AreaControl,
        ControllerFamily.SimultaneousAnswer,
        ControllerFamily.TraitProfile,
    ];

    /// <summary>
    /// Mirrors <c>GameViewModelFactory.SupportedFamilies</c>. WinUI needs the
    /// Windows App SDK, so — same reasoning as <see cref="MauiSupported"/> —
    /// this project cannot reference it and reads a copy instead.
    ///
    /// Until backlog item 12 closed this, WinUI had no declaration at all: it
    /// was "the flagship head" with less coverage than either of the other two.
    /// Its routing switch has always handled these same four families; only the
    /// declaration was missing.
    /// </summary>
    private static readonly ControllerFamily[] WinUiSupported =
    [
        ControllerFamily.CardTurn,
        ControllerFamily.Quiz,
        ControllerFamily.Monogamy,
        ControllerFamily.DailyCampaign,
        ControllerFamily.AreaControl,
        ControllerFamily.SimultaneousAnswer,
    ];

    /// <summary>
    /// Mirrors <c>GameScreenFactory.SupportedFamilies</c> in the native
    /// .NET for Android head. That project needs the <c>android</c> workload,
    /// so — same reasoning as <see cref="MauiSupported"/> and
    /// <see cref="WinUiSupported"/> — this project cannot reference it and reads
    /// a copy instead, kept honest by
    /// <c>scripts/check-head-family-coverage.py</c>. The head shipped at full
    /// parity: a screen for every family.
    /// </summary>
    private static readonly ControllerFamily[] AndroidSupported =
    [
        ControllerFamily.CardTurn,
        ControllerFamily.Quiz,
        ControllerFamily.Monogamy,
        ControllerFamily.DailyCampaign,
        ControllerFamily.AreaControl,
        ControllerFamily.SimultaneousAnswer,
    ];

    [Fact]
    public void EveryModeInTheRegistry_MapsToAKnownFamily()
    {
        AllModes().Should().OnlyContain(m => ControllerFamilies.All.Contains(ControllerFamilies.For(m)));
    }

    /// <summary>
    /// The three graphical heads have no <see cref="ControllerFamily.TraitProfile"/>
    /// screen, so Big Five is the one mode they cannot play.
    ///
    /// <para>
    /// <b>This is the mechanism working, not a regression.</b> These three tests
    /// asserted an empty set from backlog item 4 until Big Five landed, and it
    /// would have been easy to keep them green by declaring the family in each
    /// head and letting its router fall through — which is exactly the failure
    /// item 4 was written after, when MAUI's router fell through to its
    /// card-turn page for Herd and Claimed! and Console's switch had no default
    /// arm at all. Four modes silently did nothing, two of them for several
    /// versions.
    /// </para>
    ///
    /// <para>
    /// Asserting the gap by name is the honest alternative: a head that gains
    /// the screen fails this test and must say so, and a <i>second</i>
    /// unsupported mode appearing fails it too rather than hiding behind the
    /// first. <c>ControllerFamily</c>'s own docs describe this as the point of
    /// declaring families — "what makes a future gap a failing test rather than
    /// a mode that silently does nothing".
    /// </para>
    /// </summary>
    private const string TraitProfileModeName = "Big Five";

    [Theory]
    [InlineData("MAUI")]
    [InlineData("WinUI")]
    [InlineData("Android")]
    public void GraphicalHeads_PlayEveryModeExceptBigFive(string head)
    {
        var supported = head switch
        {
            "MAUI" => MauiSupported,
            "WinUI" => WinUiSupported,
            "Android" => AndroidSupported,
            _ => throw new ArgumentOutOfRangeException(nameof(head), head, "unknown head"),
        };

        ControllerFamilies.UnsupportedIn(AllModes(), supported)
            .Select(m => m.Name)
            .Should().BeEquivalentTo(new[] { TraitProfileModeName },
                $"{head} has no TraitProfile screen yet, so Big Five is its only gap. " +
                "If a screen was added, declare the family in the head, update its " +
                "mirror array above, and change this test — do not let a head claim " +
                "a family its router cannot actually render.");
    }

    [Fact]
    public void Console_CanPlayEveryModeInTheCatalogue()
    {
        // Console was "deliberately the thinnest head" until backlog item 4
        // gave it real renderers for Monogamy, Day One, Claimed! and Herd. It
        // is now the head with the *most* coverage: it shipped the
        // TraitProfile renderer with the family, so it is the only one that can
        // play Big Five.
        ControllerFamilies.UnsupportedIn(AllModes(), ConsoleSupported).Should().BeEmpty();
    }

    [Fact]
    public void TheOnlyUnsupportedFamilyAnywhere_IsTraitProfile()
    {
        // Guards the claim the test above makes by name. If a future mode
        // introduces a second family the graphical heads lack, this fails
        // and names it, rather than being absorbed into the Big Five gap.
        var families = AllModes()
            .Select(ControllerFamilies.TryFor)
            .Where(f => f is not null && !MauiSupported.Contains(f.Value))
            .Select(f => f!.Value)
            .Distinct();

        families.Should().BeEquivalentTo(new[] { ControllerFamily.TraitProfile });
    }

    // REMOVED: NoHeadSilentlyDropsAFamilyItClaimsToSupport
    //
    // Backlog item 12. It filtered AllModes() to those whose family is in
    // `supported`, then asserted those same modes' families are in `supported`
    // — the predicate was the filter, so it could not fail for any input. Its
    // docstring called it "the actual invariant worth protecting," which made
    // the false confidence worse than having no test at all.
    //
    // The real invariant — a head's SupportedFamilies array actually matches
    // what its routing switch handles — cannot be checked here. MauiSupported
    // and WinUiSupported above are hand-typed copies of properties that live in
    // projects this one cannot reference (they need the MAUI and WinUI SDKs).
    // No amount of rewriting this test changes that: comparing a copy to itself
    // is tautological regardless of phrasing, because there is no independent
    // ground truth reachable from C# in this project.
    //
    // scripts/check-head-family-coverage.py is the real fix, and it fits the
    // pattern the five existing check-*.py gates already use: it reads the
    // SupportedFamilies literal out of each head's own source file — not a
    // copy — and diffs it against the arrays above. That catches exactly what
    // this test's docstring claimed to catch, and unlike this test, it can
    // actually fail.
}
