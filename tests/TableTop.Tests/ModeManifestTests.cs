using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Restrictions;
using TableTop.Core.Domain.Game;
using TableTop.Core.Domain.Restrictions;
using TableTop.Games;

namespace TableTop.Tests;

/// <summary>
/// Tests for enhancement 2.3 — ModeManifest discovery metadata.
///
/// Verifies that:
///   - ModeManifestBuilder computes accurate metadata from a card list.
///   - ModeManifestExtensions.GetManifest() dispatches correctly to each mode shape.
///   - IModeManifestProvider fast path works.
///   - ArchetypeRegistry.AllModes and SurpriseMe() work correctly.
///   - Play-time estimates are non-zero and reasonable.
/// </summary>
public sealed class ModeManifestTests
{
    // ── ModeManifestBuilder ───────────────────────────────────────────────────

    [Fact]
    public void Build_EmptyList_ReturnsTotalCardsZero()
    {
        var manifest = ModeManifestBuilder.Build([]);
        manifest.TotalCards.Should().Be(0);
    }

    [Fact]
    public void Build_CountsCardsByDifficulty()
    {
        var cards = new List<ICard>
        {
            MakeCard(Difficulty.Easy),
            MakeCard(Difficulty.Easy),
            MakeCard(Difficulty.Hard),
        };

        var manifest = ModeManifestBuilder.Build(cards);

        manifest.TotalCards.Should().Be(3);
        manifest.CardsByDifficulty[Difficulty.Easy].Should().Be(2);
        manifest.CardsByDifficulty[Difficulty.Hard].Should().Be(1);
        manifest.CardsByDifficulty.Should().NotContainKey(Difficulty.Medium);
    }

    [Fact]
    public void Build_CountsCardsByCategory()
    {
        var cards = new List<ICard>
        {
            MakeCard(Difficulty.Easy, category: "Truth"),
            MakeCard(Difficulty.Easy, category: "Truth"),
            MakeCard(Difficulty.Easy, category: "Dare"),
        };

        var manifest = ModeManifestBuilder.Build(cards);

        manifest.CardsByCategory["Truth"].Should().Be(2);
        manifest.CardsByCategory["Dare"].Should().Be(1);
        manifest.Categories.Should().Contain("Dare");
        manifest.Categories.Should().Contain("Truth");
    }

    [Fact]
    public void Build_EstimatesNonZeroPlayTime()
    {
        var cards = Enumerable.Range(0, 20).Select(_ => MakeCard(Difficulty.Easy)).ToList<ICard>();
        var manifest = ModeManifestBuilder.Build(cards);

        manifest.EstimatedMinPlayTime.TotalMinutes.Should().BeGreaterThan(0);
        manifest.EstimatedMaxPlayTime.Should().BeGreaterThan(manifest.EstimatedMinPlayTime);
    }

    [Fact]
    public void Build_DetectsAdultContent_FromRestriction()
    {
        var cards = new List<ICard>
        {
            MakeCard(Difficulty.Easy),
            MakeCard(Difficulty.Easy, restriction: new AdultOnlyRestriction()),
        };

        var manifest = ModeManifestBuilder.Build(cards);
        manifest.HasAdultContent.Should().BeTrue();
    }

    [Fact]
    public void Build_NoAdultContent_WhenClean()
    {
        var cards = new List<ICard>
        {
            MakeCard(Difficulty.Easy),
            MakeCard(Difficulty.Medium),
        };

        var manifest = ModeManifestBuilder.Build(cards);
        manifest.HasAdultContent.Should().BeFalse();
    }

    [Fact]
    public void Build_CollectsDistinctTags()
    {
        var cards = new List<ICard>
        {
            MakeCard(Difficulty.Easy, tags: ["fun", "party"]),
            MakeCard(Difficulty.Easy, tags: ["fun", "social"]),
        };

        var manifest = ModeManifestBuilder.Build(cards);

        manifest.AllTags.Should().Contain("fun");
        manifest.AllTags.Should().Contain("party");
        manifest.AllTags.Should().Contain("social");
        manifest.AllTags.Where(t => t == "fun").Should().HaveCount(1); // deduplicated
    }

    // ── DifficultyDisplay ─────────────────────────────────────────────────────

    [Fact]
    public void DifficultyDisplay_SingleTier_ShowsName()
    {
        var cards = Enumerable.Range(0, 5).Select(_ => MakeCard(Difficulty.Easy)).ToList<ICard>();
        var manifest = ModeManifestBuilder.Build(cards);
        manifest.DifficultyDisplay.Should().Be("Easy");
    }

    [Fact]
    public void DifficultyDisplay_AllFourTiers_ShowsMixed()
    {
        var cards = new List<ICard>
        {
            MakeCard(Difficulty.Easy),
            MakeCard(Difficulty.Medium),
            MakeCard(Difficulty.Hard),
            MakeCard(Difficulty.Extreme),
        };
        var manifest = ModeManifestBuilder.Build(cards);
        manifest.DifficultyDisplay.Should().Be("Mixed");
    }

    [Fact]
    public void DominantDifficulty_ReturnsMostFrequent()
    {
        var cards = new List<ICard>
        {
            MakeCard(Difficulty.Easy),
            MakeCard(Difficulty.Easy),
            MakeCard(Difficulty.Easy),
            MakeCard(Difficulty.Hard),
        };
        var manifest = ModeManifestBuilder.Build(cards);
        manifest.DominantDifficulty.Should().Be(Difficulty.Easy);
    }

    // ── ModeManifestExtensions.GetManifest ────────────────────────────────────

    [Fact]
    public void GetManifest_IGameModeDefinition_BuildsFromCards()
    {
        IGameMode mode = new InlineModeDef(Enumerable.Range(0, 10)
            .Select(_ => MakeCard(Difficulty.Medium))
            .ToList<ICard>());

        var manifest = mode.GetManifest();

        manifest.TotalCards.Should().Be(10);
        manifest.CardsByDifficulty[Difficulty.Medium].Should().Be(10);
    }

    [Fact]
    public void GetManifest_IsMemoised_ReturnsSameInstance()
    {
        IGameMode mode = new InlineModeDef(new List<ICard> { MakeCard(Difficulty.Easy) });

        var first = mode.GetManifest();
        var second = mode.GetManifest();

        object.ReferenceEquals(first, second).Should().BeTrue(
            "manifest is cached after first computation");
    }

    [Fact]
    public void GetManifest_IModeManifestProvider_UsesFastPath()
    {
        var mode = new FastManifestMode();
        var manifest = mode.GetManifest();

        // FastManifestMode returns a pre-built manifest with TotalCards=999
        manifest.TotalCards.Should().Be(999,
            "IModeManifestProvider.GetManifest() must be used directly, not recomputed");
    }

    [Fact]
    public void GetManifest_IQuestionBankProvider_ComputesFromBank()
    {
        var manifest = new MillionaireMode().GetManifest();

        manifest.TotalCards.Should().BeGreaterThan(0,
            "MillionaireMode has a non-empty question bank");
        manifest.CardsByDifficulty.Should().ContainKey(Difficulty.Easy);
    }

    [Fact]
    public void GetManifest_UnknownModeShape_ReturnsEmptyManifest()
    {
        var mode = new StubGameMode("Unnamed", "No cards");
        var manifest = mode.GetManifest();

        manifest.TotalCards.Should().Be(0);
    }

    // ── PlayTimeDisplay ───────────────────────────────────────────────────────

    [Fact]
    public void PlayTimeDisplay_ZeroCards_ReturnsVaries()
    {
        var manifest = ModeManifestBuilder.Build([]);
        manifest.PlayTimeDisplay.Should().Be("varies");
    }

    [Fact]
    public void PlayTimeDisplay_HasCorrectFormat()
    {
        var cards = Enumerable.Range(0, 20).Select(_ => MakeCard(Difficulty.Easy)).ToList<ICard>();
        var manifest = ModeManifestBuilder.Build(cards);

        // Should look like "X–Y min"
        manifest.PlayTimeDisplay.Should().MatchRegex(@"^\d+–\d+ min$");
    }

    // ── ArchetypeRegistry.AllModes ────────────────────────────────────────────

    [Fact]
    public void Registry_AllModes_IsNotEmpty()
    {
        var registry = ArchetypeRegistry.Default();
        registry.AllModes.Should().NotBeEmpty();
    }

    [Fact]
    public void Registry_NoModeInstanceAppearsInTwoNodes()
    {
        // This used to assert AllModes.Count == AllModes.Distinct().Count().
        // AllModes *already* calls .Distinct() internally, so that compared a
        // deduplicated list against itself deduplicated again — always equal,
        // whatever the tree looked like. It could not fail, and so proved
        // nothing.
        //
        // The real invariant is on the raw tree: one mode instance registered
        // under two archetype nodes shows up twice in the picker, and dedup on
        // the way out hides that rather than preventing it.
        var registry = ArchetypeRegistry.Default();

        static IEnumerable<Archetype> Flatten(Archetype a) =>
            new[] { a }.Concat(a.SubArchetypes.SelectMany(Flatten));

        var placements = registry.RootArchetypes
            .SelectMany(Flatten)
            .SelectMany(node => node.Modes.Select(mode => (mode, node)))
            .ToList();

        var shared = placements
            .GroupBy(x => x.mode)
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key.Name} in [{string.Join(", ", g.Select(x => x.node.Id))}]")
            .ToList();

        shared.Should().BeEmpty(
            "a mode instance under two nodes appears twice in the picker");
    }

    [Fact]
    public void Registry_SameModeTypeInSeveralNodes_UsesSeparateInstances()
    {
        // Some games legitimately appear under more than one archetype — Would
        // You Rather sits in Fun, Family and Couples. That is fine as long as
        // each placement is its own instance: modes carry per-session state, so
        // a shared instance would leak between two entries in the picker.
        var registry = ArchetypeRegistry.Default();

        static IEnumerable<Archetype> Flatten(Archetype a) =>
            new[] { a }.Concat(a.SubArchetypes.SelectMany(Flatten));

        var byType = registry.RootArchetypes
            .SelectMany(Flatten)
            .SelectMany(n => n.Modes)
            .GroupBy(m => m.GetType())
            .Where(g => g.Count() > 1);

        foreach (var group in byType)
        {
            group.Distinct().Count().Should().Be(group.Count(),
                $"'{group.Key.Name}' is registered {group.Count()} times and each " +
                "placement must be a separate instance");
        }
    }

    [Fact]
    public void Registry_AllModes_EachHasNonZeroManifest()
    {
        var registry = ArchetypeRegistry.Default();
        foreach (var mode in registry.AllModes)
        {
            var manifest = mode.GetManifest();
            manifest.TotalCards.Should().BeGreaterThan(0,
                $"Mode '{mode.Name}' should have at least one card in its manifest");
        }
    }

    // ── ArchetypeRegistry.SurpriseMe ──────────────────────────────────────────

    [Fact]
    public void SurpriseMe_ReturnsAMode()
    {
        var registry = ArchetypeRegistry.Default();
        var mode = registry.SurpriseMe();
        mode.Should().NotBeNull("there are modes in the registry");
    }

    [Fact]
    public void SurpriseMe_AllowAdultFalse_ExcludesAdultModes()
    {
        var registry = ArchetypeRegistry.Default();

        // Run many times to reduce false-negative probability
        for (int i = 0; i < 50; i++)
        {
            var mode = registry.SurpriseMe(allowAdultContent: false);
            if (mode is null) continue;
            mode.GetManifest().HasAdultContent.Should().BeFalse(
                $"SurpriseMe(allowAdultContent:false) must never return adult mode '{mode.Name}'");
        }
    }

    [Fact]
    public void SurpriseMe_MaxCards_FiltersLargeDecks()
    {
        var registry = ArchetypeRegistry.Default();

        // Pick a very small max that most modes exceed
        const int maxCards = 5;
        var mode = registry.SurpriseMe(maxCards: maxCards);

        if (mode is not null)
        {
            mode.GetManifest().TotalCards.Should().BeLessThanOrEqualTo(maxCards,
                $"SurpriseMe(maxCards:{maxCards}) returned a mode with too many cards");
        }
        // null is valid when no mode fits — not an assertion failure
    }

    [Fact]
    public void Archetype_SurpriseMe_ReturnsOnlyFromItsOwnModes()
    {
        var registry = ArchetypeRegistry.Default();
        var classroom = registry.FindById("classroom");
        classroom.Should().NotBeNull();

        var allClassroomModes = classroom.AllModes;
        var mode = classroom.SurpriseMe();

        if (mode is not null)
        {
            allClassroomModes.Should().Contain(mode,
                "SurpriseMe on an archetype must return one of its own modes");
        }
    }

    // ── GetModeManifests (bulk) ───────────────────────────────────────────────

    [Fact]
    public void GetModeManifests_ReturnsEntryPerMode()
    {
        var registry = ArchetypeRegistry.Default();
        var classroom = registry.FindById("classroom")!;
        var manifests = classroom.GetModeManifests();

        manifests.Count.Should().Be(classroom.AllModes.Count);
        foreach (var mode in classroom.AllModes)
            manifests.Should().ContainKey(mode);
    }

    // ── the manifest must describe the deck the controller is handed ──────────

    [Fact]
    public void Herd_ManifestCountsTheDeckItPlays_NotTheCatalogueBehindIt()
    {
        // The bug this pins. HerdMode is `BaseGameModeDefinition, IHerdDeckProvider`,
        // and GetManifest used to test IGameModeDefinition FIRST — so it matched
        // that arm and built from GetCards([]) without ever reaching the Herd arm
        // sitting below it.
        //
        // GetHerdDeck() is GetCards([]) minus the "How To Play" category, stripped
        // on purpose: the deck feeds the controller as prompts, and round one would
        // otherwise ask the whole table to simultaneously answer a page of
        // instructions. So the manifest counted at least one card the mode can
        // never deal, and ArchetypeRegistry.SurpriseMe filters on TotalCards.
        var mode = new TableTop.Games.Fun.HerdMode();

        var played = mode.GetHerdDeck().Count;
        var catalogue = mode.GetCards([]).Count;

        played.Should().BeLessThan(catalogue,
            "GetHerdDeck strips How To Play — if this is ever equal the test below proves nothing");
        mode.GetManifest().TotalCards.Should().Be(played);
    }

    [Fact]
    public void EveryMode_ManifestTotal_MatchesTheDeckItsFamilyPlays()
    {
        // The general form. Herd was the live case, but the failure mode is
        // structural: any mode deriving from BaseGameModeDefinition AND
        // implementing a capability interface hit the same wrong arm.
        var mismatches = new List<string>();

        foreach (var mode in AllRegisteredModes())
        {
            if (mode is IModeManifestProvider) continue;   // legitimately overrides

            var expected = ControllerFamilies.TryFor(mode) switch
            {
                ControllerFamily.Monogamy => ((IMonogamyDeckProvider)mode).GetDeck().Count,
                ControllerFamily.Quiz => ((IQuestionBankProvider)mode).GetQuestionBank().Count,
                ControllerFamily.SimultaneousAnswer => ((IHerdDeckProvider)mode).GetHerdDeck().Count,
                ControllerFamily.AreaControl => ((IClaimedDeckProvider)mode).GetClaimedDeck().Count,
                ControllerFamily.DailyCampaign => ((IDailyDeckProvider)mode).GetDailyDeck().Count,
                ControllerFamily.TraitProfile => ((ITraitAssessmentProvider)mode).GetItemBank().Count,
                ControllerFamily.CardTurn => ((IGameModeDefinition)mode).GetCards([]).Count,
                _ => 0,
            };

            var actual = mode.GetManifest().TotalCards;
            if (actual != expected)
                mismatches.Add($"{mode.Name}: manifest says {actual}, its family plays {expected}");
        }

        mismatches.Should().BeEmpty();
    }

    private static IReadOnlyList<IGameMode> AllRegisteredModes()
    {
        var modes = new List<IGameMode>();
        void Walk(IEnumerable<Archetype> nodes)
        {
            foreach (var n in nodes) { modes.AddRange(n.Modes); Walk(n.SubArchetypes); }
        }
        Walk(ArchetypeRegistry.Default().RootArchetypes);
        return modes.DistinctBy(m => m.Name).ToList();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ICard MakeCard(
        Difficulty difficulty = Difficulty.Easy,
        string category = "Test",
        IRestriction? restriction = null,
        IEnumerable<string>? tags = null) =>
        StandardCard.Create(
            $"Card-{Guid.NewGuid():N}",
            "desc",
            difficulty,
            category,
            tags: tags?.ToList() ?? [],
            restriction: restriction);
}

// ── Test doubles ──────────────────────────────────────────────────────────────

/// <summary>
/// A mode that implements IModeManifestProvider to test the O(1) fast path.
/// Returns a manifest with TotalCards=999 as a sentinel value.
/// </summary>
internal sealed class FastManifestMode : IGameMode, IModeManifestProvider
{
    public string Name => "Fast Manifest";
    public string Description => "Tests the IModeManifestProvider fast path.";

    public ModeManifest GetManifest() =>
        new() { TotalCards = 999 };
}
