using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Progression;
using TableTop.Core.Abstractions.Rules;
using TableTop.Core.Domain.Progression;
using TableTop.Core.Domain.Rules;
using TableTop.Core.Domain.Scoring;
using TableTop.Hosting.Abstractions;
using TableTop.Hosting.Controllers;
using TableTop.Hosting.Persistence;

namespace TableTop.Tests.Helpers;

/// <summary>
/// Minimal <see cref="IGameModeDefinition"/> backed by an explicit card list.
/// Use in tests wherever a real game mode definition is needed without
/// pulling in any UI or mode-specific logic.
/// </summary>
public sealed class InlineModeDef : IGameMode, IGameModeDefinition
{
    private readonly IReadOnlyList<ICard> _cards;
    private readonly IScoringStrategy _scoring;
    private readonly IEnumerable<IRule> _rules;

    public string Name => "InlineMode";
    public string Description => "Test mode";

    public InlineModeDef(
        IReadOnlyList<ICard> cards,
        IScoringStrategy? scoring = null,
        IEnumerable<IRule>? rules = null)
    {
        _cards = cards;
        _scoring = scoring ?? new FixedScoringStrategy(1);
        _rules = rules ?? [new RestrictionRule(), new NoDuplicateCardRule()];
    }

    public IReadOnlyList<ICard> GetCards(IReadOnlyList<IPlayer> players) => _cards;
    public IScoringStrategy GetScoring() => _scoring;
    public IEnumerable<IRule> GetRules() => _rules;
}

/// <summary>
/// Factory helpers used across all test fixtures.
/// </summary>
public static class TestFactory
{
    /// <summary>
    /// A plain <see cref="ControllerFactory"/> with no persistence — the
    /// defaults a ViewModel test almost always wants.
    ///
    /// <para>
    /// Exists because backlog X.2 made <c>IControllerFactory</c> a required
    /// argument on every shared-ViewModel <c>CreateAsync</c>. It used to
    /// default to exactly this value, which read as convenience and was
    /// actually a trap: a head that forgot the argument lost its configured
    /// persistence silently, and that is how resume shipped broken on WinUI
    /// and MAUI. Tests genuinely do want the plain factory — they just have to
    /// say so now, which is the whole point of the change. Naming it here
    /// keeps that explicit without repeating <c>new ControllerFactory()</c> at
    /// thirty call sites.
    /// </para>
    /// </summary>
    public static IControllerFactory PlainControllerFactory() => new ControllerFactory();

    /// <summary>Creates N Easy standard cards for filler use in tests.</summary>
    public static IReadOnlyList<ICard> MakeCards(int n, Difficulty difficulty = Difficulty.Easy) =>
        Enumerable.Range(1, n)
            .Select(i => (ICard)StandardCard.Create(
                $"Card{i}", "Test card.", difficulty, "Test"))
            .ToList().AsReadOnly();

    /// <summary>Creates a mixed deck with cards spread across all difficulty tiers.</summary>
    public static IReadOnlyList<ICard> MakeMixedCards(int perTier = 4) =>
        Enum.GetValues<Difficulty>()
            .SelectMany(d => Enumerable.Range(1, perTier)
                .Select(i => (ICard)StandardCard.Create($"{d}{i}", "Test.", d, "Test")))
            .ToList().AsReadOnly();

    /// <summary>Creates a named player with optional gender and tags.</summary>
    public static Player MakePlayer(
        string name,
        string gender = "other",
        int age = 25,
        bool isAdult = true,
        IEnumerable<string>? extraTags = null)
    {
        var attrs = new Dictionary<string, string> { ["gender"] = gender, ["age"] = age.ToString() };
        var tags = new List<string>();
        if (isAdult) tags.Add("adult");
        if (extraTags is not null) tags.AddRange(extraTags);
        return Player.Create(name, attrs, tags);
    }

    /// <summary>
    /// Builds a <see cref="CardTurnController"/> from an explicit card list.
    /// All parameters are optional — sensible defaults used for quick test setup.
    /// </summary>
    public static CardTurnController BuildController(
        IReadOnlyList<ICard> cards,
        IReadOnlyList<IPlayer>? players = null,
        int maxRounds = 20,
        int skipPenalty = -1,
        IProgressionStrategy? progression = null,
        IGamePersistence? sessionRepository = null,
        IEnumerable<ICard>? bonusPool = null,
        int rewardInterval = 0,
        Hosting.Hints.IHintEngine? hintEngine = null,
        SessionSnapshot? resumeFrom = null,
        TableTop.Core.Abstractions.IEngineDiagnostics? diagnostics = null)
    {
        var playerList = players ?? [MakePlayer("Alice"), MakePlayer("Bob")];
        var strat = progression ?? new LinearProgressionStrategy();
        var def = new InlineModeDef(cards);

        return new CardTurnController(
            definition: def,
            players: playerList,
            modeName: "Test",
            maxRounds: maxRounds,
            progression: strat,
            options: new TableTop.Hosting.Controllers.CardTurnControllerOptions
            {
                SessionRepository = sessionRepository,
                BonusPool = bonusPool,
                RewardChanceInterval = rewardInterval,
                SkipPenalty = skipPenalty,
                ResumeFrom = resumeFrom,
                HintEngine = hintEngine,
                Diagnostics = diagnostics,
            });
    }

    /// <summary>
    /// Creates a minimal IRuleContext for use in rule evaluator tests.
    /// Uses an empty deck and no players — sufficient for restriction and
    /// difficulty-score rule tests that only need the card and player.
    /// </summary>
    public static TableTop.Core.Abstractions.Rules.IRuleContext MakeRuleContext(int round = 1)
    {
        var deck = new TableTop.Core.Domain.Decks.Deck(
            Guid.NewGuid(), "TestDeck", Array.Empty<TableTop.Core.Abstractions.Cards.ICard>());
        return new TableTop.Core.Domain.Rules.RuleContext(
            round,
            Array.Empty<TableTop.Core.Abstractions.Players.IPlayer>(),
            deck);
    }
}
