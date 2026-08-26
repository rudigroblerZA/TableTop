using TableTop.Hosting.Controllers;
using TableTop.Hosting.Events;

namespace TableTop.Tests;

/// <summary>
/// <see cref="ClaimedController"/> had zero test references — verified once, at
/// write time, by a harness playthrough, which is not a regression test.
/// Nothing would have caught a break. This closes that gap.
///
/// Mirrors <see cref="MonogamyTests"/>'s shape: a small player-builder helper,
/// a <c>BuildController</c> factory with sensible defaults, then one test per
/// behaviour rather than one giant scenario — so a failure names the specific
/// thing that broke.
/// </summary>
public sealed class ClaimedControllerTests
{
    private static Player Alice(string name = "Alice") => Player.Create(name);
    private static Player Bob(string name = "Bob") => Player.Create(name);

    private static ICard Card(string territory, string title = "T", Difficulty difficulty = Difficulty.Easy) =>
        new StandardCard(Guid.NewGuid(), title, "body", difficulty, territory);

    /// <summary>A deck with <paramref name="territoryCount"/> territories, <paramref name="cardsPerTerritory"/> cards each.</summary>
    private static List<ICard> Deck(int territoryCount, int cardsPerTerritory = 6)
    {
        var deck = new List<ICard>();
        for (var t = 0; t < territoryCount; t++)
        {
            var name = ((char)('A' + t)).ToString();
            for (var c = 0; c < cardsPerTerritory; c++)
                deck.Add(Card(name, $"{name}{c}"));
        }
        return deck;
    }

    private static ClaimedController BuildController(
        IReadOnlyList<Player>? players = null,
        IReadOnlyList<ICard>? deck = null,
        int winningTerritoryCount = 3)
    {
        var p = players ?? [Alice(), Bob()];
        var d = deck ?? Deck(territoryCount: 5);
        return new ClaimedController(
            p.Cast<Core.Abstractions.Players.IPlayer>().ToList().AsReadOnly(),
            d, winningTerritoryCount);
    }

    // ── Construction guards ──────────────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsWithFewerThanTwoPlayers()
    {
        var act = () => new ClaimedController(
            [Alice()], Deck(5), winningTerritoryCount: 3);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*at least two players*");
    }

    [Fact]
    public void Constructor_ThrowsWhenNoOneCouldEverWin()
    {
        // 2 territories, but 3 needed to win — the deck's own categories make
        // the target unreachable, so this must be caught at construction, not
        // discovered mid-game.
        var act = () => BuildController(deck: Deck(territoryCount: 2), winningTerritoryCount: 3);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*territories*winningTerritoryCount*");
    }

    [Fact]
    public void Constructor_AcceptsExactlyEnoughTerritoriesToWin()
    {
        var act = () => BuildController(deck: Deck(territoryCount: 3), winningTerritoryCount: 3);
        act.Should().NotThrow();
    }

    // ── Start / turn order ───────────────────────────────────────────────────

    [Fact]
    public void Start_SetsIsRunning()
    {
        var c = BuildController();
        c.IsRunning.Should().BeFalse("not started yet");
        c.Start();
        c.IsRunning.Should().BeTrue();
    }

    [Fact]
    public void CurrentPlayerName_IsFirstPlayerInListOrder()
    {
        var alice = Alice();
        var bob = Bob();
        var c = BuildController(players: [alice, bob]);
        c.Start();

        c.CurrentPlayerName.Should().Be(alice.DisplayName);
    }

    [Fact]
    public void ChallengeableTerritories_ExcludesTerritoriesTheCurrentPlayerAlreadyHolds()
    {
        var c = BuildController(deck: Deck(territoryCount: 5));
        c.Start();

        var first = c.ChallengeableTerritories.First();
        c.ChallengeTerritory(first);
        c.ResolveChallenge(succeeded: true);          // Alice claims it, turn passes to Bob

        // Bob's turn: Alice's territory is a legal RAID target for him, so it
        // still appears in his list — the exclusion is specifically about the
        // current player's own holdings, not anyone's. Fail Bob's move so the
        // turn cycles back to Alice without changing the board, then check
        // that SHE no longer sees the territory she holds.
        c.ChallengeTerritory(c.ChallengeableTerritories.First());
        c.ResolveChallenge(succeeded: false);

        c.CurrentPlayerName.Should().Be("Alice");
        c.ChallengeableTerritories.Should().NotContain(first,
            "the current player never sees their own held territory as challengeable");
    }

    // ── Claiming open ground ─────────────────────────────────────────────────

    [Fact]
    public void ChallengeTerritory_RaisesTerritoryChallengeReady_WithNoDefenderOnOpenGround()
    {
        var c = BuildController();
        c.Start();
        TerritoryChallengeReadyEvent? ev = null;
        c.TerritoryChallengeReady += (_, e) => ev = e;

        c.ChallengeTerritory(c.ChallengeableTerritories.First());

        ev.Should().NotBeNull();
        ev!.DefenderName.Should().BeNull("no one holds open ground");
    }

    [Fact]
    public void ResolveChallenge_Success_OnOpenGround_RaisesTerritoryClaimed_NotStolen()
    {
        var c = BuildController();
        c.Start();
        var claimed = false;
        var stolen = false;
        c.TerritoryClaimed += (_, _) => claimed = true;
        c.TerritoryStolen += (_, _) => stolen = true;

        c.ChallengeTerritory(c.ChallengeableTerritories.First());
        c.ResolveChallenge(succeeded: true);

        claimed.Should().BeTrue();
        stolen.Should().BeFalse();
    }

    [Fact]
    public void ResolveChallenge_Success_UpdatesTerritoryHolders()
    {
        var alice = Alice();
        var c = BuildController(players: [alice, Bob()]);
        c.Start();

        var territory = c.ChallengeableTerritories.First();
        c.ChallengeTerritory(territory);
        c.ResolveChallenge(succeeded: true);

        c.TerritoryHolders[territory].Should().Be(alice.DisplayName);
    }

    [Fact]
    public void ResolveChallenge_Failure_LeavesTerritoryOpen_AndRaisesChallengeFailed()
    {
        var c = BuildController();
        c.Start();
        ChallengeFailedEvent? ev = null;
        c.ChallengeFailed += (_, e) => ev = e;

        var territory = c.ChallengeableTerritories.First();
        c.ChallengeTerritory(territory);
        c.ResolveChallenge(succeeded: false);

        ev.Should().NotBeNull();
        ev!.WasRaid.Should().BeFalse();
        c.TerritoryHolders[territory].Should().BeNull("a failed claim on open ground changes nothing");
    }

    // ── The steal path ───────────────────────────────────────────────────────

    [Fact]
    public void ResolveChallenge_Success_OnRivalHeldTerritory_StealsIt()
    {
        var alice = Alice();
        var bob = Bob();
        var c = BuildController(players: [alice, bob], deck: Deck(territoryCount: 5, cardsPerTerritory: 3));
        c.Start();

        // Alice claims A.
        var territory = c.ChallengeableTerritories.First();
        c.ChallengeTerritory(territory);
        c.ResolveChallenge(succeeded: true);
        c.TerritoryHolders[territory].Should().Be(alice.DisplayName);

        // Bob's turn: raid Alice's territory specifically.
        c.CurrentPlayerName.Should().Be(bob.DisplayName);
        c.ChallengeableTerritories.Should().Contain(territory,
            "a rival-held territory is a legal raid target");

        TerritoryStolenEvent? stolen = null;
        c.TerritoryStolen += (_, e) => stolen = e;
        c.ChallengeTerritory(territory);
        c.ResolveChallenge(succeeded: true);

        c.TerritoryHolders[territory].Should().Be(bob.DisplayName, "the raid succeeded");
        stolen.Should().NotBeNull();
        stolen!.AttackerName.Should().Be(bob.DisplayName);
        stolen.DefenderName.Should().Be(alice.DisplayName);
        stolen.TerritoryName.Should().Be(territory);
    }

    [Fact]
    public void ChallengeTerritory_OnRivalHeldTerritory_SetsDefenderNameOnTheReadyEvent()
    {
        var alice = Alice();
        var c = BuildController(players: [alice, Bob()], deck: Deck(territoryCount: 5, cardsPerTerritory: 3));
        c.Start();

        var territory = c.ChallengeableTerritories.First();
        c.ChallengeTerritory(territory);
        c.ResolveChallenge(succeeded: true);   // Alice claims it

        TerritoryChallengeReadyEvent? ev = null;
        c.TerritoryChallengeReady += (_, e) => ev = e;
        c.ChallengeTerritory(territory);       // Bob raids it

        ev!.DefenderName.Should().Be(alice.DisplayName);
    }

    [Fact]
    public void ResolveChallenge_FailedRaid_LeavesTerritoryWithTheDefender()
    {
        var alice = Alice();
        var c = BuildController(players: [alice, Bob()], deck: Deck(territoryCount: 5, cardsPerTerritory: 3));
        c.Start();

        var territory = c.ChallengeableTerritories.First();
        c.ChallengeTerritory(territory);
        c.ResolveChallenge(succeeded: true);   // Alice claims it

        ChallengeFailedEvent? ev = null;
        c.ChallengeFailed += (_, e) => ev = e;
        c.ChallengeTerritory(territory);       // Bob raids it
        c.ResolveChallenge(succeeded: false);  // ...and fails

        ev!.WasRaid.Should().BeTrue();
        c.TerritoryHolders[territory].Should().Be(alice.DisplayName, "a failed raid changes nothing");
    }

    // ── Win condition: ThreeHeld ─────────────────────────────────────────────

    [Fact]
    public void Win_ByHoldingTarget_EndsGame_WithCorrectWinnerAndReason()
    {
        // Bob's move is always resolved as failed, so it can never change the
        // board regardless of which territory he happens to target — safe
        // against dictionary iteration order, which the fix in
        // Win_FinalHoldings below exists because of.
        var alice = Alice();
        var c = BuildController(players: [alice, Bob()], deck: Deck(territoryCount: 3, cardsPerTerritory: 2),
                                 winningTerritoryCount: 2);
        c.Start();

        ClaimedGameEndedEvent? ended = null;
        c.GameEnded += (_, e) => ended = e;

        // Alice claims #1.
        c.ChallengeTerritory(c.ChallengeableTerritories.First());
        c.ResolveChallenge(true);
        // Bob fails on purpose so the board stays simple.
        c.ChallengeTerritory(c.ChallengeableTerritories.First());
        c.ResolveChallenge(false);
        // Alice claims #2 -> hits winningTerritoryCount.
        c.ChallengeTerritory(c.ChallengeableTerritories.First());
        c.ResolveChallenge(true);

        ended.Should().NotBeNull();
        ended!.Reason.Should().Be(ClaimedEndReason.ThreeHeld);
        ended.WinnerNames.Should().ContainSingle().Which.Should().Be(alice.DisplayName);
        c.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void Win_FinalHoldings_ReportEveryPlayersActualTerritories()
    {
        // One card per territory, deliberately: with more than one card, a
        // territory a player just claimed can still be re-targeted as a raid,
        // and .First() picking that raid instead of new open ground breaks the
        // "Alice=2, Bob=1" outcome this test asserts. Verified empirically —
        // with cardsPerTerritory > 1 here, Bob's very next move raids Alice's
        // fresh claim instead of taking separate ground, and the game never
        // reaches the state below. One card per territory removes the
        // ambiguity: a claimed territory's pool is empty immediately, so it
        // drops out of ChallengeableTerritories entirely rather than lingering
        // as a raid option.
        var alice = Alice();
        var bob = Bob();
        var c = BuildController(players: [alice, bob], deck: Deck(territoryCount: 3, cardsPerTerritory: 1),
                                 winningTerritoryCount: 2);
        c.Start();

        ClaimedGameEndedEvent? ended = null;
        c.GameEnded += (_, e) => ended = e;

        c.ChallengeTerritory(c.ChallengeableTerritories.First()); c.ResolveChallenge(true);  // Alice #1
        c.ChallengeTerritory(c.ChallengeableTerritories.First()); c.ResolveChallenge(true);   // Bob #1 (forced onto new ground)
        c.ChallengeTerritory(c.ChallengeableTerritories.First()); c.ResolveChallenge(true);   // Alice #2 -> wins

        ended.Should().NotBeNull();
        ended!.FinalHoldings[alice.DisplayName].Should().HaveCount(2);
        ended.FinalHoldings[bob.DisplayName].Should().HaveCount(1);
    }

    // ── Win condition: DeckExhausted, including the tie ──────────────────────

    [Fact]
    public void DeckExhausted_WithNoOneAtTarget_EndsGame_WithMostHeldWinning()
    {
        // 2 territories, 1 card each, target of 2 — unreachable by either
        // player once each has claimed one, so this always ends by exhaustion
        // rather than a target hit. With two players and two single-card
        // territories this necessarily splits 1-1, which is the tie case,
        // covered on its own terms in the next test — this one's job is just
        // confirming the end reason is DeckExhausted.
        var alice = Alice();
        var bob = Bob();
        var c = new ClaimedController(
            [alice, bob], Deck(territoryCount: 2, cardsPerTerritory: 1), winningTerritoryCount: 2);
        c.Start();

        ClaimedGameEndedEvent? ended = null;
        c.GameEnded += (_, e) => ended = e;

        c.ChallengeTerritory(c.ChallengeableTerritories.First()); c.ResolveChallenge(true);  // Alice claims A
        c.ChallengeTerritory(c.ChallengeableTerritories.First()); c.ResolveChallenge(true);   // Bob claims B

        ended.Should().NotBeNull();
        ended!.Reason.Should().Be(ClaimedEndReason.DeckExhausted);
        c.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void DeckExhausted_Tie_ReportsBothPlayersAsWinners_NotOneArbitrarily()
    {
        var alice = Alice();
        var bob = Bob();
        var c = new ClaimedController(
            [alice, bob], Deck(territoryCount: 2, cardsPerTerritory: 1), winningTerritoryCount: 2);
        c.Start();

        ClaimedGameEndedEvent? ended = null;
        c.GameEnded += (_, e) => ended = e;

        c.ChallengeTerritory(c.ChallengeableTerritories.First()); c.ResolveChallenge(true);  // Alice: A
        c.ChallengeTerritory(c.ChallengeableTerritories.First()); c.ResolveChallenge(true);   // Bob: B

        ended!.WinnerNames.Should().HaveCount(2,
            "a 1-1 split is a genuine tie and both names must be reported");
        ended.WinnerNames.Should().Contain(alice.DisplayName);
        ended.WinnerNames.Should().Contain(bob.DisplayName);
    }

    [Fact]
    public void DeckExhausted_TerritoryDroppedFromChallengeable_OnceItsPoolIsEmpty()
    {
        // A single-card territory should vanish from ChallengeableTerritories
        // once that card is drawn, whether or not the challenge succeeds — the
        // pool, not the outcome, gates availability.
        //
        // winningTerritoryCount must be explicit here: BuildController defaults
        // to 3, but this deck only has 2 territories, and the constructor
        // guard (Constructor_ThrowsWhenNoOneCouldEverWin, above) rejects that
        // combination before Start() is ever reached. Caught by a real
        // Windows test run — this test never got as far as exercising the
        // pool-exhaustion behaviour it's named for.
        var c = BuildController(deck: Deck(territoryCount: 2, cardsPerTerritory: 1), winningTerritoryCount: 2);
        c.Start();

        var first = c.ChallengeableTerritories.First();
        c.ChallengeTerritory(first);
        c.ResolveChallenge(succeeded: false);   // fails, but the card is still spent

        c.ChallengeableTerritories.Should().NotContain(first,
            "the pool is empty regardless of whether the challenge succeeded");
    }

    // ── Guards on out-of-turn / malformed calls ──────────────────────────────

    [Fact]
    public void ChallengeTerritory_WithAnIllegalName_IsSilentlyIgnored()
    {
        var c = BuildController();
        c.Start();
        var before = c.ChallengeableTerritories.Count;

        c.ChallengeTerritory("NotARealTerritory");

        c.ChallengeableTerritories.Count.Should().Be(before,
            "an unrecognised territory name must not change any state");
    }

    [Fact]
    public void ResolveChallenge_WithNothingPending_IsSilentlyIgnored()
    {
        var c = BuildController();
        c.Start();

        var act = () => c.ResolveChallenge(succeeded: true);

        act.Should().NotThrow("resolving with nothing pending is a no-op, not an error");
    }

    [Fact]
    public void ChallengeTerritory_BeforeStart_IsSilentlyIgnored()
    {
        var c = BuildController();
        // Deliberately no Start().

        var act = () => c.ChallengeTerritory(c.ChallengeableTerritories.First());

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var c = BuildController();
        c.Start();
        var act = () => c.Dispose();
        act.Should().NotThrow();
    }
}
