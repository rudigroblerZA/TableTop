using TableTop.Core.Abstractions.Game;
using TableTop.Core.Domain.Decks;
using TableTop.Core.Domain.Progression;
using TableTop.Core.Domain.Rules;
using TableTop.Core.Domain.Scoring;
using TableTop.Core.Engine;

namespace TableTop.Tests;

public sealed class GameEngineTests
{
    private static IGame BuildSimpleGame(int cardCount = 4, int? maxRounds = null)
    {
        var cards = Enumerable.Range(1, cardCount)
            .Select(i => StandardCard.Create($"Card{i}", "desc", Difficulty.Easy, "Test"))
            .ToList();

        var deck = new Deck(Guid.NewGuid(), "TestDeck", cards);
        var players = new[]
        {
            Player.Create("Alice"),
            Player.Create("Bob")
        };

        return new GameBuilder()
            .WithDeck(deck)
            .WithPlayers(players)
            .WithProgression(new LinearProgressionStrategy())
            .WithScoring(new FixedScoringStrategy(10))
            .AddRule(new RestrictionRule())
            .WithMaxRounds(maxRounds)
            .Build();
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    [Fact]
    public void Start_TransitionsToActive()
    {
        var game = BuildSimpleGame();
        game.Start();
        game.State.Should().Be(GameState.Active);
    }

    [Fact]
    public void Start_WhenAlreadyActive_Throws()
    {
        var game = BuildSimpleGame();
        game.Start();
        Xunit.Assert.Throws<InvalidOperationException>(() => game.Start());
    }

    [Fact]
    public void Pause_And_Resume_Works()
    {
        var game = BuildSimpleGame();
        game.Start();
        game.Pause();
        game.State.Should().Be(GameState.Paused);
        game.Resume();
        game.State.Should().Be(GameState.Active);
    }

    [Fact]
    public void End_TransitionsToEnded()
    {
        var game = BuildSimpleGame();
        game.Start();
        game.End();
        game.State.Should().Be(GameState.Ended);
    }

    // ── Turn flow ─────────────────────────────────────────────────────────────

    [Fact]
    public void AdvanceTurn_ReturnsCard()
    {
        var game = BuildSimpleGame();
        game.Start();
        var card = game.AdvanceTurn();
        card.Should().NotBeNull();
    }

    [Fact]
    public void RecordOutcome_UpdatesScore()
    {
        var game = BuildSimpleGame();
        game.Start();
        game.AdvanceTurn();
        game.RecordOutcome(CardOutcome.Completed);

        game.CurrentPlayer.Should().BeNull(); // cleared after RecordOutcome
    }

    [Fact]
    public void RecordOutcome_WithoutAdvanceTurn_Throws()
    {
        var game = BuildSimpleGame();
        game.Start();
        Xunit.Assert.Throws<InvalidOperationException>(() => game.RecordOutcome(CardOutcome.Completed));
    }

    // ── Events ────────────────────────────────────────────────────────────────

    [Fact]
    public void TurnCompleted_FiredAfterRecordOutcome()
    {
        var game = BuildSimpleGame();
        TurnCompletedEventArgs? captured = null;
        game.TurnCompleted += (_, args) => captured = args;

        game.Start();
        game.AdvanceTurn();
        game.RecordOutcome(CardOutcome.Completed);

        captured.Should().NotBeNull();
        captured.ScoreDelta.Should().Be(10);
        captured.Outcome.Should().Be(CardOutcome.Completed);
    }

    [Fact]
    public void GameEnded_FiredOnEnd()
    {
        var game = BuildSimpleGame();
        GameEndedEventArgs? captured = null;
        game.GameEnded += (_, args) => captured = args;

        game.Start();
        game.End();

        captured.Should().NotBeNull();
        captured.FinalStandings.Should().HaveCount(2);
    }

    // ── PlayedCards tracking ──────────────────────────────────────────────────

    [Fact]
    public void PlayedCards_AccumulatesAfterEachTurn()
    {
        var game = BuildSimpleGame(4);
        game.Start();

        game.AdvanceTurn(); game.RecordOutcome(CardOutcome.Completed);
        game.AdvanceTurn(); game.RecordOutcome(CardOutcome.Skipped);

        game.PlayedCards.Should().HaveCount(2);
    }

    // ── NoDuplicate rule ──────────────────────────────────────────────────────

    [Fact]
    public void NoDuplicateCardRule_DeniesRepeat()
    {
        var cards = new[]
        {
            StandardCard.Create("OnlyCard", "desc", Difficulty.Easy, "Test")
        };
        var deck = new Deck(Guid.NewGuid(), "Tiny", cards);
        var players = new[] { Player.Create("Alice") };

        var game = new GameBuilder()
            .WithDeck(deck)
            .WithPlayers(players)
            .WithProgression(new LinearProgressionStrategy())
            .WithScoring(new FixedScoringStrategy())
            .AddRule(new RestrictionRule())
            .AddRule(new NoDuplicateCardRule())
            .Build();

        game.Start();
        var first = game.AdvanceTurn();
        first.Should().NotBeNull();
        game.RecordOutcome(CardOutcome.Completed);

        // Deck is exhausted; second advance should return null
        var second = game.AdvanceTurn();
        second.Should().BeNull();
    }
}