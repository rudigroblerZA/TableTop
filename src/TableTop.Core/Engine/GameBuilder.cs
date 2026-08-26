using TableTop.Core.Abstractions;
using TableTop.Core.Abstractions.Decks;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Progression;
using TableTop.Core.Abstractions.Rules;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Progression;
using TableTop.Core.Domain.Scoring;

namespace TableTop.Core.Engine;

/// <summary>
/// Fluent builder for assembling and launching a game session.
/// <example>
/// <code>
/// var game = new GameBuilder()
///     .WithDeck(deck)
///     .WithPlayers(players)
///     .WithProgression(new DifficultyProgressionStrategy())
///     .WithScoring(new DifficultyBasedScoringStrategy())
///     .AddRule(new RestrictionRule())
///     .AddRule(new NoDuplicateCardRule())
///     .WithMaxRounds(10)
///     .Build();
///
/// game.Start();
/// </code>
/// </example>
/// </summary>
public sealed class GameBuilder
{
    private IDeck? _deck;
    private readonly List<IPlayer> _players = [];
    private IProgressionStrategy _progression = new LinearProgressionStrategy();
    private IScoringStrategy _scoring = new FixedScoringStrategy();
    private readonly List<IRule> _rules = [];
    private int? _maxRounds;
    private List<string>? _deferredCategories;
    private IGameFactory? _explicitFactory;
    private IEngineDiagnostics? _diagnostics;
    private bool _teamPlay;

    /// <summary>
    /// Plugs a diagnostics sink into the rule evaluator.
    /// Rule denials will be emitted to <paramref name="diagnostics"/> so you can
    /// trace why a card was skipped without attaching a debugger.
    /// </summary>
    public GameBuilder WithDiagnostics(IEngineDiagnostics diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        _diagnostics = diagnostics;
        return this;
    }

    /// <summary>
    /// Alternates turn order between teams rather than running straight
    /// round-robin. See <c>TeamAlternatingPlayerManager</c>; team membership
    /// itself lives in <c>IPlayer.Attributes["team"]</c>.
    /// </summary>
    public GameBuilder WithTeamPlay(bool teamPlay = true)
    {
        _teamPlay = teamPlay;
        return this;
    }

    /// <summary>Initialises a new <see cref="WithDeck"/> instance.</summary>
    public GameBuilder WithDeck(IDeck deck)
    {
        _deck = deck ?? throw new ArgumentNullException(nameof(deck));
        return this;
    }

    /// <summary>Initialises a new <see cref="WithPlayers"/> instance.</summary>
    public GameBuilder WithPlayers(IEnumerable<IPlayer> players)
    {
        ArgumentNullException.ThrowIfNull(players);
        _players.AddRange(players);
        return this;
    }

    /// <summary>Initialises a new <see cref="AddPlayer"/> instance.</summary>
    public GameBuilder AddPlayer(IPlayer player)
    {
        ArgumentNullException.ThrowIfNull(player);
        _players.Add(player);
        return this;
    }

    /// <summary>Initialises a new <see cref="WithProgression"/> instance.</summary>
    public GameBuilder WithProgression(IProgressionStrategy strategy)
    {
        _progression = strategy ?? throw new ArgumentNullException(nameof(strategy));
        return this;
    }

    /// <summary>Initialises a new <see cref="WithScoring"/> instance.</summary>
    public GameBuilder WithScoring(IScoringStrategy strategy)
    {
        _scoring = strategy ?? throw new ArgumentNullException(nameof(strategy));
        return this;
    }

    /// <summary>Initialises a new <see cref="WithRules"/> instance.</summary>
    public GameBuilder WithRules(IEnumerable<IRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        foreach (var r in rules) _rules.Add(r);
        return this;
    }

    /// <summary>Initialises a new <see cref="AddRule"/> instance.</summary>
    public GameBuilder AddRule(IRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        _rules.Add(rule);
        return this;
    }

    /// <summary>Initialises a new <see cref="WithMaxRounds"/> instance.</summary>
    /// <summary>
    /// Holds the given card categories back until everything else has been
    /// played. See <see cref="GameConfiguration.DeferredCategories"/>.
    /// </summary>
    public GameBuilder WithDeferredCategories(IEnumerable<string>? categories)
    {
        _deferredCategories = categories?.ToList();
        return this;
    }

    /// <inheritdoc />
    public GameBuilder WithMaxRounds(int? maxRounds)
    {
        if (maxRounds < 1) throw new ArgumentOutOfRangeException(nameof(maxRounds));
        _maxRounds = maxRounds;
        return this;
    }

    /// <summary>
    /// Supplies a factory outright. Overrides <see cref="WithDiagnostics"/> and
    /// <see cref="WithTeamPlay"/> entirely — a caller handing over a whole
    /// factory owns how it was built.
    /// </summary>
    public GameBuilder WithFactory(IGameFactory factory)
    {
        _explicitFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        return this;
    }

    private TableTop.Core.Abstractions.Game.SpecialCardScoringPolicy _specialCardPolicy
        = TableTop.Core.Abstractions.Game.SpecialCardScoringPolicy.NoScore;
    private int _specialCardBonus;

    /// <summary>Initialises a new <see cref="WithSpecialCardPolicy"/> instance.</summary>
    public GameBuilder WithSpecialCardPolicy(
        TableTop.Core.Abstractions.Game.SpecialCardScoringPolicy policy, int bonus = 0)
    {
        _specialCardPolicy = policy;
        _specialCardBonus = bonus;
        return this;
    }
    /// <summary>Constructs and returns the configured <see cref="IGame"/> instance.</summary>

    public IGame Build()
    {
        if (_deck is null)
            throw new InvalidOperationException("A deck must be provided via WithDeck().");
        if (_players.Count == 0)
            throw new InvalidOperationException("At least one player must be added via AddPlayer() or WithPlayers().");

        var config = new GameConfiguration(
            _deck, _players, _progression, _scoring, _rules, _maxRounds,
            _specialCardPolicy, _specialCardBonus,
            deferredCategories: _deferredCategories);

        // Built here rather than in each With* method so the two settings
        // that feed it can't clobber each other: WithDiagnostics() used to
        // replace the whole factory, so calling it after WithTeamPlay() would
        // have silently discarded the team manager depending purely on call
        // order. Resolving once, at the end, makes the builder order-independent.
        var factory = _explicitFactory ?? new GameFactory(_diagnostics, _teamPlay);

        return factory.Create(config);
    }
}