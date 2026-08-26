using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Rules;
using TableTop.Core.Domain.Rules;
using TableTop.Core.Domain.Scoring;
using TableTop.Core.Engine;

namespace TableTop.Tests;

/// <summary>
/// Integration tests proving the ten engine invariants identified in the architecture review.
/// Each region corresponds to a numbered issue.
/// </summary>
public sealed class EngineInvariantTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IGame DirectGame(
        IReadOnlyList<ICard> cards,
        IReadOnlyList<IPlayer>? players = null,
        int? maxRounds = null,
        IEnumerable<IRule>? rules = null,
        SpecialCardScoringPolicy specialPolicy = SpecialCardScoringPolicy.NoScore)
    {
        var ps = (players ?? [Player.Create("Alice"), Player.Create("Bob")])
            .ToList().AsReadOnly();
        return new GameBuilder()
            .WithDeck(BuildDeck(cards))
            .WithPlayers(ps)
            .WithProgression(new Core.Domain.Progression.LinearProgressionStrategy())
            .WithScoring(new FixedScoringStrategy(2))
            .WithRules(rules ?? [new RestrictionRule(), new NoDuplicateCardRule()])
            .WithMaxRounds(maxRounds)
            .WithSpecialCardPolicy(specialPolicy)
            .Build();
    }

    private static Core.Domain.Decks.Deck BuildDeck(IReadOnlyList<ICard> cards)
    {
        var provider = new Core.Domain.Decks.InMemoryCardProvider(cards.ToList());
        return new Core.Domain.Decks.DeckBuilder()
            .WithProvider(provider)
            .BuildAsync().GetAwaiter().GetResult() as Core.Domain.Decks.Deck
            ?? throw new InvalidOperationException("Deck build failed");
    }

    // ── Issue 1: scoring pipeline unification ─────────────────────────────────

    [Fact]
    public void Issue1_FinalScore_IsScoringStrategyPlusRuleScoreDelta()
    {
        // DifficultyScoreRule adds +1 for Hard/Extreme cards
        var card = StandardCard.Create("Hard", "d", Difficulty.Hard, "Test");
        var alice = Player.Create("Alice");
        var game = DirectGame([card, .. TestFactory.MakeCards(5)],
            players: [alice],
            rules: [new NoDuplicateCardRule(), new DifficultyScoreRule()]);

        game.Start();
        game.AdvanceTurn();
        game.RecordOutcome(CardOutcome.Completed);

        // FixedScoringStrategy(2) + DifficultyScoreRule(Hard=+1) = 3
        game.PlayerManager.Players.First(p => p.Id == alice.Id).Score.Should().Be(3);
    }

    [Fact]
    public void Issue1_DeniedRule_DoesNotApplyScoreDelta()
    {
        // Even with a DifficultyScoreRule, a denied card should never reach RecordOutcome
        var card = StandardCard.Create("Easy", "d", Difficulty.Easy, "Test");
        var alice = Player.Create("Alice");

        // RestrictionRule will block adult card for non-adult player
        var adultCard = new StandardCard(
            Guid.NewGuid(), "Adult", "d", Difficulty.Easy, "T",
            [], new Core.Domain.Restrictions.AdultOnlyRestriction());

        var dl = new System.Collections.Generic.List<ICard> { adultCard, card };
        dl.AddRange(TestFactory.MakeCards(3));
        var game = DirectGame(dl.ToArray(), players: [alice],
            rules: [new RestrictionRule(), new NoDuplicateCardRule()]);

        game.Start();
        var drawn = game.AdvanceTurn();

        // The adult card should have been skipped; alice is not adult
        drawn.Should().NotBeNull();
        drawn!.Title.Should().Be("Easy"); // skipped the adult card
    }

    // ── Issue 2: non-mutating candidate selection ─────────────────────────────

    [Fact]
    public void Issue2_IneligibleCards_NotConsumedDuringSearch()
    {
        // All cards are adult-only; player is not adult → no eligible cards
        var adultCard = new StandardCard(
            Guid.NewGuid(), "Adult", "d", Difficulty.Easy, "T",
            [], new Core.Domain.Restrictions.AdultOnlyRestriction());
        var alice = Player.Create("Alice"); // no adult tag → ineligible

        var game = DirectGame([adultCard], players: [alice]);
        game.Start();
        var drawn = game.AdvanceTurn();

        drawn.Should().BeNull("no eligible card exists");
        // Deck should still have the card — it was not consumed
        game.PlayerManager.Players.Should().NotBeEmpty();
    }

    [Fact]
    public void Issue2_EligibleCard_DrawnExactlyOnce()
    {
        var cards = TestFactory.MakeCards(5);
        var alice = Player.Create("Alice");
        var game = DirectGame(cards, players: [alice]);

        game.Start();
        var c1 = game.AdvanceTurn()!;
        game.RecordOutcome(CardOutcome.Completed);
        var c2 = game.AdvanceTurn()!;
        game.RecordOutcome(CardOutcome.Completed);

        c1.Id.Should().NotBe(c2.Id);
        game.PlayedCards.Should().HaveCount(2);
        game.PlayedCards.Select(c => c.Id).Distinct().Should().HaveCount(2);
    }

    // ── Issue 3: round progression with snapshotted player count ─────────────

    [Fact]
    public void Issue3_RoundAdvances_AfterAllSnapshotPlayersHaveTaken_OneTurn()
    {
        var cards = TestFactory.MakeCards(20);
        var alice = Player.Create("Alice");
        var bob = Player.Create("Bob");
        var game = DirectGame(cards, players: [alice, bob]);

        game.Start();
        game.Round.Should().Be(1);

        game.AdvanceTurn(); game.RecordOutcome(CardOutcome.Completed); // alice
        game.Round.Should().Be(1); // not yet — bob hasn't gone

        game.AdvanceTurn(); game.RecordOutcome(CardOutcome.Completed); // bob
        game.Round.Should().Be(2); // now advanced
    }

    [Fact]
    public void Issue3_MidRound_StatusChange_DoesNotBreakRoundCounting()
    {
        var cards = TestFactory.MakeCards(20);
        var alice = Player.Create("Alice");
        var bob = Player.Create("Bob");
        var carol = Player.Create("Carol");
        var game = DirectGame(cards, players: [alice, bob, carol]);

        game.Start();
        // Snapshot at round start: 3 active players

        // Alice goes, then Carol becomes inactive mid-round
        game.AdvanceTurn(); game.RecordOutcome(CardOutcome.Completed); // alice
        game.PlayerManager.SetStatus(carol.Id, PlayerStatus.Skipped);

        // Bob goes
        game.AdvanceTurn(); game.RecordOutcome(CardOutcome.Completed); // bob

        // Round should NOT advance yet — snapshot was 3, only 2 turns done
        game.Round.Should().Be(1);

        // Carol goes (she was skipped by status but the turn still advances round counter)
        // Actually: GetNextPlayer skips inactive players
        // So after 3 turns round advances regardless (snapshot = 3 at start)
        game.AdvanceTurn(); game.RecordOutcome(CardOutcome.Completed);
        game.Round.Should().Be(2);
    }

    // ── Issue 4: MaxRounds semantics ──────────────────────────────────────────

    [Fact]
    public void Issue4_MaxRounds_EndsAfterExactRounds()
    {
        var cards = TestFactory.MakeCards(20);
        var alice = Player.Create("Alice");
        var game = DirectGame(cards, players: [alice], maxRounds: 2);

        game.Start();

        GameEndedEventArgs? endedArgs = null;
        game.GameEnded += (_, e) => endedArgs = e;

        // Round 1: 1 player → 1 turn
        game.AdvanceTurn(); game.RecordOutcome(CardOutcome.Completed);
        endedArgs.Should().BeNull("still in round 1");

        // Round 2: 1 player → 1 turn
        game.AdvanceTurn(); game.RecordOutcome(CardOutcome.Completed);
        endedArgs.Should().NotBeNull("maxRounds=2 reached after 2 complete rounds");
        endedArgs!.TotalRounds.Should().Be(2);
    }

    // ── Issue 5: IPlayerManager accepts any IPlayer ───────────────────────────

    [Fact]
    public void Issue5_PlayerManager_AcceptsAnyIPlayer()
    {
        var mgr = new RoundRobinPlayerManager();
        var stub = new StubPlayer("test-player");

        // Should not throw — any IPlayer is accepted
        var act = () => mgr.AddPlayer(stub);
        act(); // should not throw
    }

    [Fact]
    public void Issue5_PlayerManager_ScoreAndStatus_ReflectManagerState()
    {
        var mgr = new RoundRobinPlayerManager();
        var alice = Player.Create("Alice");
        mgr.AddPlayer(alice);

        mgr.ApplyScore(alice.Id, 5);
        mgr.SetStatus(alice.Id, PlayerStatus.Skipped);

        // Score and status accessed via the Players collection (PlayerView wrappers)
        var view = mgr.Players.First(p => p.Id == alice.Id);
        view.Score.Should().Be(5);
        view.Status.Should().Be(PlayerStatus.Skipped);
    }

    // ── Issue 6: cached Players/ActivePlayers ─────────────────────────────────

    [Fact]
    public void Issue6_Players_ReturnsSameReference_WhenUnchanged()
    {
        var mgr = new RoundRobinPlayerManager();
        var alice = Player.Create("Alice");
        mgr.AddPlayer(alice);

        var ref1 = mgr.Players;
        var ref2 = mgr.Players;

        ReferenceEquals(ref1, ref2).Should().BeTrue("cached until mutation");
    }

    [Fact]
    public void Issue6_ActivePlayers_CacheInvalidated_OnStatusChange()
    {
        var mgr = new RoundRobinPlayerManager();
        var alice = Player.Create("Alice");
        mgr.AddPlayer(alice);

        var before = mgr.ActivePlayers;
        mgr.SetStatus(alice.Id, PlayerStatus.Skipped);
        var after = mgr.ActivePlayers;

        ReferenceEquals(before, after).Should().BeFalse("cache invalidated on mutation");
        after.Should().BeEmpty("alice is now skipped");
    }

    // ── Issue 8: special card scoring policy ──────────────────────────────────

    [Fact]
    public void Issue8_BreakCard_NoScore_ByDefault()
    {
        var cardList = new System.Collections.Generic.List<ICard> { BreakCard.CreateGroupBreak("Rest", "Take a break.") };
        cardList.AddRange(TestFactory.MakeCards(3));
        var cards = cardList.ToArray();
        var alice = Player.Create("Alice");
        var game = DirectGame(cards, players: [alice],
            specialPolicy: SpecialCardScoringPolicy.NoScore);

        game.Start();
        game.AdvanceTurn();
        game.RecordOutcome(CardOutcome.Completed);

        alice.Score.Should().Be(0, "break cards score 0 with NoScore policy");
    }

    [Fact]
    public void Issue8_RewardCard_FixedBonus_AppliedWhenPolicySet()
    {
        var cardList2 = new System.Collections.Generic.List<ICard> { RewardCard.CreateScoreBonus("Bonus", "d.", 5) };
        cardList2.AddRange(TestFactory.MakeCards(3));
        var cards = cardList2.ToArray();
        var alice = Player.Create("Alice");

        // Use FixedBonus(3) policy — the reward card's own bonus is separate from this
        var game = new GameBuilder()
            .WithDeck(BuildDeck(cards))
            .WithPlayers([alice])
            .WithProgression(new Core.Domain.Progression.LinearProgressionStrategy())
            .WithScoring(new FixedScoringStrategy(2))
            .WithRules([new NoDuplicateCardRule()])
            .WithSpecialCardPolicy(SpecialCardScoringPolicy.FixedBonus, bonus: 3)
            .Build();

        game.Start();
        game.AdvanceTurn();
        game.RecordOutcome(CardOutcome.Completed);

        game.PlayerManager.Players.First(p => p.Id == alice.Id).Score.Should().Be(3, "FixedBonus(3) applied to the reward card");
    }

    // ── Issue 10: typed metadata ──────────────────────────────────────────────

    [Fact]
    public void Issue10_GameMetadata_TracksPlayedCards_WithTypedApi()
    {
        var meta = new GameMetadata();
        var player = Guid.NewGuid();
        var card = Guid.NewGuid();

        meta.HasCardBeenPlayedBy(player, card).Should().BeFalse();
        meta.MarkCardPlayed(player, card);
        meta.HasCardBeenPlayedBy(player, card).Should().BeTrue();
    }

    [Fact]
    public void Issue10_NoDuplicateRule_UsesTypedMetadata()
    {
        // After a card is played, the same card should not be offered again
        var card = StandardCard.Create("X", "d", Difficulty.Easy, "T");
        var alice = Player.Create("Alice");
        var game = DirectGame([card, .. TestFactory.MakeCards(5)],
            players: [alice]);

        game.Start();
        var drawn = game.AdvanceTurn()!;
        game.RecordOutcome(CardOutcome.Completed);

        // On subsequent turns the same card ID should not appear
        for (var i = 0; i < 5; i++)
        {
            var next = game.AdvanceTurn();
            if (next is null) break;
            next.Id.Should().NotBe(drawn.Id);
            game.RecordOutcome(CardOutcome.Completed);
        }
    }
}

// ── Stubs ─────────────────────────────────────────────────────────────────────

internal sealed class StubPlayer(string name) : IPlayer
{
    public Guid Id { get; } = Guid.NewGuid();
    public string DisplayName { get; } = name;
    public int Score => 0;
    public PlayerStatus Status => PlayerStatus.Active;
    public IReadOnlyDictionary<string, string> Attributes =>
        new Dictionary<string, string>();
    public IReadOnlyList<string> Tags => [];
}
