using TableTop.Core.Abstractions.Game;
using TableTop.Games;
using TableTop.Games.Couples;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;
using TableTop.Hosting.Persistence;

namespace TableTop.Tests;

public sealed class ArchetypeTests
{
    // ── Default registry ──────────────────────────────────────────────────────

    [Fact]
    public void Default_HasThreeRootArchetypes()
    {
        // Three, not four. The registry builds Classroom, Fun and Couples;
        // personality routes into Fun rather than having a root of its own
        // (see the "personality" case in ArchetypeRegistry). This test asserted
        // four and had been failing since that change — the engine's behaviour
        // is deliberate and documented, so the test was the stale half.
        var registry = ArchetypeRegistry.Default();
        registry.RootArchetypes.Should().HaveCount(3);
    }

    [Fact]
    public void Default_RootArchetypes_AreClassroomFunCouples()
    {
        var registry = ArchetypeRegistry.Default();
        registry.RootArchetypes.Select(a => a.Id)
            .Should().Contain(x => x == "classroom");
    }

    [Fact]
    public void Default_EachRootHasSubArchetypes()
    {
        var registry = ArchetypeRegistry.Default();
        registry.RootArchetypes.Should().OnlyContain(a => a.HasSubArchetypes);
    }

    [Fact]
    public void Default_ClassroomIsAllAges()
    {
        var classroom = ArchetypeRegistry.Default().FindById("classroom");
        classroom!.AgeRating.Should().Be(AgeRating.AllAges);
    }

    [Fact]
    public void Default_CouplesIsAdult()
    {
        var couples = ArchetypeRegistry.Default().FindById("couples");
        couples!.AgeRating.Should().Be(AgeRating.Adult);
    }

    // ── Sub-archetype lookup ──────────────────────────────────────────────────

    [Fact]
    public void FindById_ReturnsCorrectNode_AtAnyDepth()
    {
        var registry = ArchetypeRegistry.Default();

        registry.FindById("classroom.quiz")!.Name.Should().Be("Quiz Night");
        registry.FindById("fun.party")!.Name.Should().Be("Party Classics");
        registry.FindById("couples.intimate")!.Name.Should().Be("Intimate");
    }

    [Fact]
    public void FindById_ReturnsNull_WhenNotFound()
    {
        var registry = ArchetypeRegistry.Default();
        registry.FindById("nonexistent.id").Should().BeNull();
    }

    // ── Mode assignment ───────────────────────────────────────────────────────

    [Fact]
    public void ClassroomQuiz_ContainsMillionnaireMode()
    {
        var node = ArchetypeRegistry.Default().FindById("classroom.quiz");
        node!.Modes.Should().Contain(m => m.Name.Contains("Millionaire"));
    }

    [Fact]
    public void FunParty_ContainsTruthOrDare()
    {
        var node = ArchetypeRegistry.Default().FindById("fun.party");
        node!.Modes.Should().Contain(m => m.Name.Contains("Truth"));
    }

    [Fact]
    public void CouplesIntimate_ContainsMonogamy()
    {
        var node = ArchetypeRegistry.Default().FindById("couples.intimate");
        node!.Modes.Should().Contain(m => m.Name == "Monogamy");
    }

    // ── AllModes flattening ───────────────────────────────────────────────────

    [Fact]
    public void AllModes_IncludesModesFromAllSubArchetypes()
    {
        var classroom = ArchetypeRegistry.Default().FindById("classroom");
        var allModes = classroom!.AllModes;

        allModes.Should().Contain(m => m.Name.Contains("Millionaire"),
            "Quiz Night lives under Classroom");
        allModes.Should().Contain(m => m.Name.Contains("Get to Know"),
            "Icebreakers lives under Classroom");
    }

    [Fact]
    public void AllModes_NoDuplicates()
    {
        var registry = ArchetypeRegistry.Default();
        foreach (var root in registry.RootArchetypes)
        {
            var all = root.AllModes;
            all.Select(m => m.Name).Distinct().Count().Should().Be(all.Count,
                $"Archetype '{root.Name}' should have no duplicate modes");
        }
    }

    // ── JSON mode injection ───────────────────────────────────────────────────

    // Four WithJsonModes_* tests lived here, pinning the bucketing of
    // runtime-loaded modes: school-tagged to classroom, couples-tagged to
    // couples, untagged to fun, and no custom nodes when there is nothing to
    // bucket. They went with JsonGameMode in 1.21.0.
    //
    // They existed because the bucketing was once three independent Where
    // clauses, so a mode matching two heuristics appeared twice and one matching
    // none fell through to fun — the family-facing section. If user content
    // returns, that is the bug to write the tests against first.


    [Fact]
    public void EachArchetype_HasNonEmptyNameDescriptionEmoji()
    {
        var registry = ArchetypeRegistry.Default();
        foreach (var root in registry.RootArchetypes)
        {
            root.Name.Should().NotBeNullOrWhiteSpace();
            root.Description.Should().NotBeNullOrWhiteSpace();
            root.Emoji.Should().NotBeNullOrWhiteSpace();
            foreach (var sub in root.SubArchetypes)
            {
                sub.Name.Should().NotBeNullOrWhiteSpace();
                sub.Description.Should().NotBeNullOrWhiteSpace();
                sub.Emoji.Should().NotBeNullOrWhiteSpace();
            }
        }
    }
}

// ── New school language game tests ────────────────────────────────────────────

public sealed class NewSchoolModeTests
{
    [Theory]
    // Ids lost their "grade6" segment when that tier was flattened into
    // Classroom — the node held thirteen games and no modes of its own, which
    // made this branch one level deeper than any other.
    [InlineData("classroom.words", "Word Detectives")]
    [InlineData("classroom.figurative", "Figurative Language")]
    [InlineData("classroom.stories", "Story Starters")]
    [InlineData("classroom.punctuation", "Punctuation Wars")]
    public void CurriculumModes_RegisteredInArchetype(string id, string modeName)
    {
        var node = ArchetypeRegistry.Default().FindById(id);
        node.Should().NotBeNull($"archetype '{id}' should exist");
        node!.Modes.Should().Contain(m => m.Name.Contains(modeName));
    }

    [Fact]
    public void Classroom_IsFlat_WithEveryNodeCarryingItsOwnModes()
    {
        // Was Grade6_NowHasThirteenModes, asserting on "classroom.grade6".
        // That node is gone, so the old test dereferenced null and threw rather
        // than failing with a useful message. What is worth protecting is the
        // reason it was flattened: a branch node with no modes of its own made
        // the picker show an empty list, which reads as broken selection.
        var classroom = ArchetypeRegistry.Default().FindById("classroom")!;

        classroom.SubArchetypes.Should().NotBeEmpty();
        classroom.SubArchetypes.Should().OnlyContain(
            n => n.SubArchetypes.Count == 0,
            "Classroom is flat — a nested node here is what the flatten removed");
        classroom.SubArchetypes.Should().OnlyContain(
            n => n.Modes.Count > 0,
            "every node must carry modes, or tapping it shows an empty game list");
    }

    [Fact]
    public void MentalMathsSprint_RegisteredInClassroom()
    {
        // "classroom.maths" was already taken by Number World, so flattening
        // renamed this one rather than colliding.
        var node = ArchetypeRegistry.Default().FindById("classroom.mentalmaths");
        node.Should().NotBeNull();
        node!.Modes.Should().Contain(m => m.Name.Contains("Mental Maths"));
    }

    [Fact]
    public void WordDetectives_HasAllDifficulties()
    {
        var cards = TableTop.Games.School.WordDetectivesMode.GetCards();
        cards.Should().Contain(c => c.Difficulty == Difficulty.Easy);
        cards.Should().Contain(c => c.Difficulty == Difficulty.Hard);
        cards.Should().Contain(c => c.Difficulty == Difficulty.Extreme);
        cards.Count.Should().BeGreaterThan(25);
    }

    [Fact]
    public void FigurativeLanguage_HasMultipleCategories()
    {
        var cards = TableTop.Games.School.FigurativeLanguageMode.GetCards();
        var cats = cards.Select(c => c.Category).Distinct().ToList();
        cats.Count.Should().BeGreaterThan(3);
        cards.Count.Should().BeGreaterThan(20);
    }

    [Fact]
    public void StoryStarters_HasStarterConstraintAndTwistCards()
    {
        var cards = TableTop.Games.School.StoryStartersMode.GetCards();
        cards.Should().Contain(c => c.Category == "Starter");
        cards.Should().Contain(c => c.Category == "Constraint");
        cards.Should().Contain(c => c.Category == "Twist");
    }

    [Fact]
    public void PunctuationWars_HasApostropheAndCommaCards()
    {
        var cards = TableTop.Games.School.PunctuationWarsMode.GetCards();
        cards.Should().Contain(c => c.Category == "Apostrophe");
        cards.Should().Contain(c => c.Category == "Comma");
    }

    [Fact]
    public void AllNewModes_HaveNonEmptyNamesAndDescriptions()
    {
        TableTop.Core.Abstractions.Game.IGameMode[] modes =
        [
            new TableTop.Games.School.WordDetectivesMode(),
            new TableTop.Games.School.FigurativeLanguageMode(),
            new TableTop.Games.School.StoryStartersMode(),
            new TableTop.Games.School.PunctuationWarsMode(),
        ];
        foreach (var m in modes)
        {
            m.Name.Should().NotBeNullOrWhiteSpace();
            m.Description.Should().NotBeNullOrWhiteSpace();
        }
    }
}

// ── Couples game tests ─────────────────────────────────────────────────────────

public sealed class CouplesGameTests
{
    [Theory]
    [InlineData("couples.connection.questions", "Couples Questions")]
    [InlineData("couples.connection.memory", "Memory Lane")]
    [InlineData("couples.connection.wish", "Two Truths, One Wish")]
    [InlineData("couples.dares.relationship", "Relationship Dares")]
    public void NewCouplesModes_RegisteredInArchetype(string id, string modeName)
    {
        var node = ArchetypeRegistry.Default().FindById(id);
        node.Should().NotBeNull($"archetype '{id}' should exist");
        node!.Modes.Should().Contain(m => m.Name.Contains(modeName));
    }

    [Fact]
    public void Couples_HasConnectionAndDaresAndIntimate()
    {
        var couples = ArchetypeRegistry.Default().FindById("couples")!;
        couples.SubArchetypes.Should().Contain(s => s.Id == "couples.connection");
        couples.SubArchetypes.Should().Contain(s => s.Id == "couples.dares");
        couples.SubArchetypes.Should().Contain(s => s.Id == "couples.intimate");
    }

    [Fact]
    public void CouplesQuestions_Has36Cards()
    {
        var cards = TableTop.Games.Couples.CouplesQuestionsMode.GetCards();
        cards.Should().HaveCount(36);
    }

    [Fact]
    public void CouplesQuestions_HasThreeSets()
    {
        var cards = TableTop.Games.Couples.CouplesQuestionsMode.GetCards();
        cards.Should().Contain(c => c.Category == "Set 1");
        cards.Should().Contain(c => c.Category == "Set 2");
        cards.Should().Contain(c => c.Category == "Set 3");
    }

    [Fact]
    public void RelationshipDares_HasAllFourZones()
    {
        var cards = TableTop.Games.Couples.RelationshipDaresMode.GetCards();
        cards.Should().Contain(c => c.Category == "Playful");
        cards.Should().Contain(c => c.Category == "Honest");
        cards.Should().Contain(c => c.Category == "Tender");
        cards.Should().Contain(c => c.Category == "Intimate");
    }

    [Fact]
    public void MemoryLane_HasFirstTimesAndMilestonesAndHiddenViews()
    {
        var cards = TableTop.Games.Couples.MemoryLaneMode.GetCards();
        cards.Should().Contain(c => c.Category == "First Times");
        cards.Should().Contain(c => c.Category == "Milestones");
        cards.Should().Contain(c => c.Category == "Hidden Views");
        cards.Count.Should().BeGreaterThan(28);
    }

    [Fact]
    public void TwoTruthsOneWish_HasAboutMeAndAboutUsAndFuture()
    {
        var cards = TableTop.Games.Couples.TwoTruthsOneWishMode.GetCards();
        cards.Should().Contain(c => c.Category == "About Me");
        cards.Should().Contain(c => c.Category == "About Us");
        cards.Should().Contain(c => c.Category == "The Future");
        cards.Should().Contain(c => c.Category == "Big Questions");
    }

    [Fact]
    public void AllCouplesCards_HaveCoupleOnlyRestriction()
    {
        var modes = new[]
        {
            TableTop.Games.Couples.CouplesQuestionsMode.GetCards(),
            TableTop.Games.Couples.MemoryLaneMode.GetCards(),
            TableTop.Games.Couples.RelationshipDaresMode.GetCards(),
            TableTop.Games.Couples.TwoTruthsOneWishMode.GetCards(),
        };
        foreach (var bank in modes)
        {
            // Every card in a couples game should have at least some restriction
            bank.Should().OnlyContain(
                c => c.Restriction != null,
                "all couples cards should require couple-member players");
        }
    }

    [Fact]
    public void AllNewCouplesModes_HaveNonEmptyNamesAndDescriptions()
    {
        TableTop.Core.Abstractions.Game.IGameMode[] modes =
        [
            new TableTop.Games.Couples.CouplesQuestionsMode(),
            new TableTop.Games.Couples.RelationshipDaresMode(),
            new TableTop.Games.Couples.MemoryLaneMode(),
            new TableTop.Games.Couples.TwoTruthsOneWishMode(),
        ];
        foreach (var m in modes)
        {
            m.Name.Should().NotBeNullOrWhiteSpace();
            m.Description.Should().NotBeNullOrWhiteSpace();
        }
    }
}

// ── Family game tests ──────────────────────────────────────────────────────────

public sealed class FamilyGameTests
{
    [Fact]
    public void Fun_HasFamilySubArchetype()
    {
        var fun = ArchetypeRegistry.Default().FindById("fun")!;
        fun.SubArchetypes.Should().Contain(s => s.Id == "fun.family");
    }

    [Theory]
    [InlineData("fun.family.quiz", "Family Quiz")]
    [InlineData("fun.family.dares", "Family Dares")]
    [InlineData("fun.family.stories", "Family Stories")]
    [InlineData("fun.family.thisisus", "This Is Us")]
    [InlineData("fun.family.laugh", "Laugh or Groan")]
    public void FamilyModes_RegisteredInArchetype(string id, string modeName)
    {
        var node = ArchetypeRegistry.Default().FindById(id);
        node.Should().NotBeNull($"archetype '{id}' should exist");
        node!.Modes.Should().Contain(m => m.Name.Contains(modeName));
    }

    [Fact]
    public void FamilyQuiz_Has80Questions()
    {
        var cards = TableTop.Games.Family.FamilyQuizMode.GetCards();
        cards.Should().HaveCount(80);
    }

    [Fact]
    public void FamilyQuiz_HasAllDifficultyTiers()
    {
        var cards = TableTop.Games.Family.FamilyQuizMode.GetCards();
        cards.Should().Contain(c => c.Difficulty == Difficulty.Easy);
        cards.Should().Contain(c => c.Difficulty == Difficulty.Medium);
        cards.Should().Contain(c => c.Difficulty == Difficulty.Hard);
        cards.Should().Contain(c => c.Difficulty == Difficulty.Extreme);
    }

    [Fact]
    public void FamilyDares_HasAllTiers()
    {
        var cards = TableTop.Games.Family.FamilyDaresMode.GetCards();
        cards.Should().Contain(c => c.Difficulty == Difficulty.Easy);
        cards.Should().Contain(c => c.Difficulty == Difficulty.Hard);
        cards.Should().Contain(c => c.Difficulty == Difficulty.Extreme);
        cards.Count.Should().BeGreaterThan(30);
    }

    [Fact]
    public void FamilyStories_HasOpeningConstraintAndTwist()
    {
        var cards = TableTop.Games.Family.FamilyStoriesMode.GetCards();
        cards.Should().Contain(c => c.Category == "Opening");
        cards.Should().Contain(c => c.Category == "Constraint");
        cards.Should().Contain(c => c.Category == "Twist");
        cards.Should().Contain(c => c.Category == "Solo");
    }

    [Fact]
    public void ThisIsUs_HasMemoryDebateAndReveal()
    {
        var cards = TableTop.Games.Family.ThisIsUsMode.GetCards();
        cards.Should().Contain(c => c.Category == "Memory");
        cards.Should().Contain(c => c.Category == "Debate");
        cards.Should().Contain(c => c.Category == "Reveal");
    }

    [Fact]
    public void LaughOrGroan_HasAllThreeCardTypes()
    {
        var cards = TableTop.Games.Family.LaughOrGroanMode.GetCards();
        cards.Should().Contain(c => c.Category == "Would You Rather");
        cards.Should().Contain(c => c.Category == "Scenario");
        cards.Should().Contain(c => c.Category == "Hot Take");
    }

    [Fact]
    public void AllFamilyModes_AreAllAges()
    {
        var family = ArchetypeRegistry.Default().FindById("fun.family")!;
        family.AgeRating.Should().Be(AgeRating.AllAges);
        foreach (var sub in family.SubArchetypes)
            sub.AgeRating.Should().Be(AgeRating.AllAges);
    }

    [Fact]
    public void AllFamilyModes_HaveNonEmptyNamesAndDescriptions()
    {
        TableTop.Core.Abstractions.Game.IGameMode[] modes =
        [
            new TableTop.Games.Family.FamilyQuizMode(),
            new TableTop.Games.Family.FamilyDaresMode(),
            new TableTop.Games.Family.FamilyStoriesMode(),
            new TableTop.Games.Family.ThisIsUsMode(),
            new TableTop.Games.Family.LaughOrGroanMode(),
        ];
        foreach (var m in modes)
        {
            m.Name.Should().NotBeNullOrWhiteSpace();
            m.Description.Should().NotBeNullOrWhiteSpace();
        }
    }
}

// ── Architecture / factory tests ────────────────────────────────────────────────

public sealed class ArchitectureTests
{
    [Fact]
    public void IGameMode_HasNoRunMethod()
    {
        var method = typeof(IGameMode).GetMethod("Run");
        method.Should().BeNull("Run() was dead code and has been removed");
    }

    [Fact]
    public void IGamePersistence_IsDistinctFromISessionRepository()
    {
        typeof(IGamePersistence).Should().NotBe(typeof(ISessionRepository));
        typeof(ISessionRepository).IsAssignableTo(typeof(IGamePersistence))
            .Should().BeTrue("ISessionRepository should extend IGamePersistence");
    }

    [Fact]
    public void JsonSessionRepository_ImplementsIGamePersistence()
    {
        typeof(JsonSessionRepository)
            .IsAssignableTo(typeof(IGamePersistence))
            .Should().BeTrue();
    }

    [Fact]
    public void ControllerFactory_IsRegisteredInHostingAssembly()
    {
        typeof(ControllerFactory).Assembly.GetName().Name
            .Should().Be("TableTop.Hosting");
    }

    [Fact]
    public void IControllerFactory_IsInHostingAssembly()
    {
        typeof(IControllerFactory).Assembly.GetName().Name
            .Should().Be("TableTop.Hosting");
    }

    // ── Composition root (enhancement 1.3) ───────────────────────────────────
    // These tests require the Microsoft.Extensions.DependencyInjection package.
    // Add it to TableTop.Tests.csproj to run them:
    //   <PackageReference Include="Microsoft.Extensions.DependencyInjection" />

#if HAS_MICROSOFT_DI

    [Fact]
    public void AddTableTopHosting_RegistersIControllerFactory()
    {
        var sp = BuildHostingProvider();
        sp.GetService<IControllerFactory>().Should().NotBeNull();
    }

    [Fact]
    public void AddTableTopHosting_RegistersIArchetypeRegistry()
    {
        var sp = BuildHostingProvider();
        sp.GetService<TableTop.Hosting.IArchetypeRegistry>().Should().NotBeNull();
    }

    [Fact]
    public void AddTableTopHosting_RegistersIGamePersistence()
    {
        var sp = BuildHostingProvider();
        sp.GetService<IGamePersistence>().Should().NotBeNull();
    }

    [Fact]
    public void AddTableTopHosting_RegistersIPlayerRepository()
    {
        var sp = BuildHostingProvider();
        sp.GetService<TableTop.Hosting.Persistence.IPlayerRepository>().Should().NotBeNull();
    }

    [Fact]
    public void AddTableTopHosting_RegistersIHintEngine()
    {
        var sp = BuildHostingProvider();
        sp.GetService<TableTop.Hosting.Hints.IHintEngine>().Should().NotBeNull();
    }

    [Fact]
    public void AddTableTopHosting_IControllerFactory_IsTransient()
    {
        var sp = BuildHostingProvider();
        var a  = sp.GetRequiredService<IControllerFactory>();
        var b  = sp.GetRequiredService<IControllerFactory>();
        object.ReferenceEquals(a, b).Should().BeFalse(
            "IControllerFactory is transient — each resolution gets a fresh instance");
    }

    [Fact]
    public void AddTableTopHosting_IArchetypeRegistry_IsSingleton()
    {
        var sp = BuildHostingProvider();
        var a  = sp.GetRequiredService<TableTop.Hosting.IArchetypeRegistry>();
        var b  = sp.GetRequiredService<TableTop.Hosting.IArchetypeRegistry>();
        object.ReferenceEquals(a, b).Should().BeTrue(
            "IArchetypeRegistry is singleton — the tree is built once");
    }

    [Fact]
    public void UseGamePersistence_ReplacesDefaultRegistration()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddTableTopHosting()
                .UseGamePersistence<InMemoryPersistence>();

        var sp     = services.BuildServiceProvider();
        var result = sp.GetRequiredService<IGamePersistence>();

        result.Should().BeAssignableTo<InMemoryPersistence>(
            "UseGamePersistence<T> should replace the default JsonGamePersistence");
    }

    private static System.IServiceProvider BuildHostingProvider()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddTableTopHosting();
        return services.BuildServiceProvider();
    }

#endif

    [Fact]
    public async Task ControllerFactory_CreatesCardTurnController_ForGenericMode()
    {
        var players = new[]
        {
            TestFactory.MakePlayer("Alice"),
            TestFactory.MakePlayer("Bob")
        };

        var factory = new ControllerFactory();
        var controller = await factory.CreateAsync(
            new TableTop.Games.Couples.CouplesQuestionsMode(), players, maxRounds: 5);

        controller.Should().BeAssignableTo<ICardTurnController>();
    }

    [Fact]
    public async Task ControllerFactory_CreatesMillionaireController_ForMillionaireMode()
    {
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };
        var factory = new ControllerFactory();
        var controller = await factory.CreateAsync(new MillionaireMode(), players);

        controller.Should().BeAssignableTo<IMillionaireController>();
    }

    [Fact]
    public async Task ControllerFactory_CreatesMonogamyController_ForMonogamyMode()
    {
        var players = new[]
        {
            TestFactory.MakePlayer("Alice", gender: "female", extraTags: new[] { "couple-member", "married" }),
            TestFactory.MakePlayer("Bob",   gender: "male",   extraTags: new[] { "couple-member", "married" })
        };
        var factory = new ControllerFactory();
        var controller = await factory.CreateAsync(new MonogamyMode(), players);

        controller.Should().BeAssignableTo<IMonogamyController>();
    }

    [Fact]
    public async Task CardTurnController_CreateAsync_BuildsDeckWithoutSyncOverAsync()
    {
        var cards = TestFactory.MakeCards(5);
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };
        var def = new InlineModeDef(cards);

        // CreateAsync should complete without throwing
        var controller = await CardTurnController.CreateAsync(
            def, players, "TestMode", 10,
            new TableTop.Core.Domain.Progression.LinearProgressionStrategy());

        controller.Should().NotBeNull();
        controller.IsRunning.Should().BeFalse();
    }

    // ── Capability-interface dispatch (enhancement 1.2) ───────────────────────

    [Fact]
    public void MillionaireMode_SuppliesItsOwnQuestionBank()
    {
        var bank = new MillionaireMode().GetQuestionBank();
        bank.Should().NotBeEmpty("the mode must own its content, not the factory");
    }

    [Fact]
    public void SchoolMillionaireMode_SuppliesGrade6Bank()
    {
        var bank = new TableTop.Games.School.SchoolMillionaireMode().GetQuestionBank();
        bank.Should().NotBeEmpty();
    }

    [Fact]
    public void FamilyQuizMode_SuppliesItsOwnQuestionBank()
    {
        var bank = new TableTop.Games.Family.FamilyQuizMode().GetQuestionBank();
        bank.Should().NotBeEmpty();
    }

    [Fact]
    public void MonogamyMode_SuppliesItsOwnDeckAndWinCondition()
    {
        var mode = new MonogamyMode();
        mode.GetDeck().Should().NotBeEmpty();
        mode.WinningTokenCount.Should().Be(10);
    }

    // The "JSON card bank loading" block that lived here (nine tests) went with
    // the JSON deck path in 1.19.0. It asserted that shipped .deck.json files
    // resolved on disk and that each mode's loaded deck matched the file it came
    // from — both meaningless once modes read their C# banks directly. The
    // user-supplied content path that replaced it is covered by DeckFileTests
    // and the JSON mode tests, all removed in 1.21.0.

    [Fact]
    public async Task ControllerFactory_DispatchesOnQuestionBankCapability_NotConcreteType()
    {
        // A brand-new mode the factory has never heard of, only implementing the
        // capability interface, must still route to the Millionaire controller.
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };
        var factory = new ControllerFactory();
        var controller = await factory.CreateAsync(new CustomQuizMode(), players);

        controller.Should().BeAssignableTo<IMillionaireController>(
            "dispatch is driven by IQuestionBankProvider, not a concrete mode type");
    }

    [Fact]
    public async Task ControllerFactory_DispatchesOnMonogamyDeckCapability_NotConcreteType()
    {
        var players = new[]
        {
            TestFactory.MakePlayer("Alice", gender: "female", extraTags: new[] { "couple-member" }),
            TestFactory.MakePlayer("Bob",   gender: "male",   extraTags: new[] { "couple-member" })
        };
        var factory = new ControllerFactory();
        var controller = await factory.CreateAsync(new CustomMonogamyMode(), players);

        controller.Should().BeAssignableTo<IMonogamyController>(
            "dispatch is driven by IMonogamyDeckProvider, not a concrete mode type");
    }

    // ── IFlowAwareMode dispatch (enhancement 1.1) ─────────────────────────────

    [Fact]
    public void SpellingBeeMode_ImplementsIFlowAwareMode()
    {
        typeof(TableTop.Games.School.SpellingBeeMode)
            .IsAssignableTo(typeof(TableTop.Core.Abstractions.Game.IFlowAwareMode))
            .Should().BeTrue();
    }

    [Fact]
    public void GrammarQuestMode_ImplementsIFlowAwareMode()
    {
        typeof(TableTop.Games.School.GrammarQuestMode)
            .IsAssignableTo(typeof(TableTop.Core.Abstractions.Game.IFlowAwareMode))
            .Should().BeTrue();
    }

    [Fact]
    public void VocabularyBuilderMode_ImplementsIFlowAwareMode()
    {
        typeof(TableTop.Games.School.VocabularyBuilderMode)
            .IsAssignableTo(typeof(TableTop.Core.Abstractions.Game.IFlowAwareMode))
            .Should().BeTrue();
    }

    [Fact]
    public void ReadingComprehensionMode_ImplementsIFlowAwareMode()
    {
        typeof(TableTop.Games.School.ReadingComprehensionMode)
            .IsAssignableTo(typeof(TableTop.Core.Abstractions.Game.IFlowAwareMode))
            .Should().BeTrue();
    }

    [Fact]
    public async Task ControllerFactory_DispatchesOnFlowAwareCapability_NotConcreteType()
    {
        // A brand-new mode the factory has never seen, only IFlowAwareMode + IGameModeDefinition,
        // must still route to a CardTurnController (not throw or fall through wrongly).
        var players = new[] { TestFactory.MakePlayer("Alice"), TestFactory.MakePlayer("Bob") };
        var factory = new ControllerFactory();
        var controller = await factory.CreateAsync(new CustomFlowAwareMode(), players);

        controller.Should().BeAssignableTo<ICardTurnController>(
            "dispatch is driven by IFlowAwareMode + IGameModeDefinition, not a concrete mode type");
    }

    [Fact]
    public void ControllerFactory_HasNoConcreteGameModeReferences()
    {
        // ControllerFactory must not reference any concrete game mode type.
        // Hosting may reference the Games assembly (for ArchetypeRegistry) but
        // the factory class itself must only reference capability interfaces.
        var factoryType = typeof(ControllerFactory);
        var gamesAssembly = typeof(TableTop.Games.MillionaireMode).Assembly;

        // Collect every type directly referenced in ControllerFactory's methods
        // via IL metadata. We check that no concrete IGameMode from the Games
        // assembly appears — capability interfaces (from Core) are fine.
        var referencedTypes = factoryType.GetMethods(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public)
            .SelectMany(m => m.GetMethodBody()?.LocalVariables
                              .Select(v => v.LocalType) ?? [])
            .Where(t => t is not null)
            .ToList();

        var concreteGameModes = referencedTypes
            .Where(t => t!.Assembly == gamesAssembly &&
                        typeof(TableTop.Core.Abstractions.Game.IGameMode).IsAssignableFrom(t))
            .ToList();

        concreteGameModes.Should().BeEmpty(
            "ControllerFactory must dispatch on capability interfaces, " +
            "never reference concrete game mode types");
    }
}

// ── Stub ──────────────────────────────────────────────────────────────────────

internal sealed class StubGameMode(string name, string description)
    : TableTop.Core.Abstractions.Game.IGameMode
{
    public string Name => name;
    public string Description => description;
}

/// <summary>
/// In-memory persistence double. Used in DI override test (enhancement 1.3).
/// </summary>
internal sealed class InMemoryPersistence : IGamePersistence
{
    private SessionSnapshot? _saved;
    public bool HasSavedSession => _saved is not null;
    public Task SaveAsync(SessionSnapshot s, CancellationToken ct = default) { _saved = s; return Task.CompletedTask; }
    public Task<SessionSnapshot?> LoadAsync(CancellationToken ct = default) => Task.FromResult(_saved);
    public Task DeleteAsync(CancellationToken ct = default) { _saved = null; return Task.CompletedTask; }
}

/// <summary>
/// A quiz mode the factory has never heard of. Proves dispatch is driven by the
/// <see cref="IQuestionBankProvider"/> capability, not a known concrete type.
/// </summary>
internal sealed class CustomQuizMode
    : TableTop.Core.Abstractions.Game.IGameMode,
      TableTop.Core.Abstractions.Game.IQuestionBankProvider
{
    public string Name => "Custom Quiz";
    public string Description => "A bespoke quiz mode for testing capability dispatch.";

    public IReadOnlyList<TableTop.Core.Domain.Cards.MultipleChoiceCard> GetQuestionBank() =>
    [
        TableTop.Core.Domain.Cards.MultipleChoiceCard.Create(
            "What is 2 + 2?", "3", "4", "5", "6",
            TableTop.Core.Abstractions.Cards.AnswerLabel.B,
            TableTop.Core.Abstractions.Cards.Difficulty.Easy),
    ];
}

/// <summary>
/// A flow-aware card-turn mode the factory has never heard of. Proves dispatch
/// is driven by <see cref="IFlowAwareMode"/> + <see cref="IGameModeDefinition"/>.
/// </summary>
internal sealed class CustomFlowAwareMode
    : TableTop.Games.Base.BaseGameModeDefinition,
      TableTop.Core.Abstractions.Game.IFlowAwareMode
{
    public override string Name => "Custom Flow Mode";
    public override string Description => "Testing flow-aware capability dispatch.";

    protected override TableTop.Core.Abstractions.Scoring.IScoringStrategy BuildScoring() =>
        new TableTop.Core.Domain.Scoring.FixedScoringStrategy(1);

    protected override IReadOnlyList<TableTop.Core.Abstractions.Cards.ICard> BuildCards(
        IReadOnlyList<TableTop.Core.Abstractions.Players.IPlayer> players) =>
        TestFactory.MakeCards(5);
}
internal sealed class CustomMonogamyMode
    : TableTop.Core.Abstractions.Game.IGameMode,
      TableTop.Core.Abstractions.Game.IMonogamyDeckProvider
{
    public string Name => "Custom Monogamy";
    public string Description => "A bespoke intimacy deck for testing capability dispatch.";

    public IReadOnlyList<TableTop.Core.Domain.Cards.MonogamyCard> GetDeck() =>
    [
        TableTop.Core.Domain.Cards.MonogamyCard.Create(
            "Test Card",
            "for him", "for her", "neutral",
            TableTop.Core.Abstractions.Cards.MonogamyZone.Foreplay,
            TableTop.Core.Abstractions.Cards.CardTarget.ForBoth),
    ];

    public int? WinningTokenCount => 5;
}
