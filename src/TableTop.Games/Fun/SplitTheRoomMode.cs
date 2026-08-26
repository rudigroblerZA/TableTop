using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Fun;

/// <summary>
/// Split the Room — two teams, one card at a time, alternating.
///
/// The catalogue's first team mode. Everything else is either individual or
/// whole-table, so a group that naturally arrives as two sides — two families,
/// two departments, the people who arrived early and the people who didn't — had
/// nothing built for them.
///
/// How to play:
///   1. Split into two teams. Uneven is fine; the smaller team goes first.
///   2. Draw a card. It says which team acts and what the other team does —
///      usually guess, sometimes judge, occasionally race.
///   3. Score it, then the other team takes the next card. Keep going until the
///      deck runs out or someone has to leave.
///
/// TEAM SCORE IS KEPT BY THE TABLE
/// ───────────────────────────────
/// The engine has no team abstraction — it scores per player, and adding one for
/// a single mode would be a large change to <c>CardTurnController</c>, which the
/// architecture backlog already flags as oversized. So the score lives on paper,
/// exactly as the sixty-second window in 60 Seconds and the thirty seconds in
/// Alibi live on the card rather than in the engine. Every card states what it is
/// worth. If team play grows past one mode, real team support is the answer; one
/// mode is not enough to justify it.
/// </summary>
public sealed class SplitTheRoomMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <inheritdoc />
    public override string Name        => "Split the Room";
    /// <inheritdoc />
    public override string Description =>
        "Two teams, alternating cards. One side performs, the other guesses, judges or races — score on paper, loser buys the next round of something.";

    /// <summary>Two teams need four bodies to be worth the name.</summary>
    public override int MinimumPlayers => 4;

    /// <summary>Made for groups and work socials; a family works too if it splits sensibly.</summary>
    public TableShape SuitableFor => TableShape.Family | TableShape.Team | TableShape.Group;

    /// <summary>Records the acting team taking the points.</summary>
    public override string CompleteLabel => "Point Scored";
    /// <summary>Records the acting team missing.</summary>
    public override string SkipLabel     => "No Point";

    /// <summary>Opens on the setup card so teams and scoring get agreed.</summary>
    public override IReadOnlyList<string> CategoriesPinnedToStart => ["Setup"];

    /// <summary>Closes on the decider, for tables that end up level.</summary>
    public override IReadOnlyList<string> CategoriesPinnedToEnd => ["Decider"];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Setup"]     = "#5AC8B0",
            ["Describe"]  = "#42A5F5",
            ["Perform"]   = "#AB47BC",
            ["Guess Us"]  = "#FFA726",
            ["Race"]      = "#EF5350",
            ["Decider"]   = "#B71C4A",
        };

    /// <summary>Harder cards are worth more points, and the cards say so.</summary>
    protected override IScoringStrategy BuildScoring() => new DifficultyBasedScoringStrategy();

    /// <inheritdoc />
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        SplitTheRoomCardBank.All;

    /// <summary>Returns the built-in card bank.</summary>
    public static IReadOnlyList<ICard> GetCards() => SplitTheRoomCardBank.All;
}

/// <summary>Built-in card bank for Split the Room.</summary>
public static class SplitTheRoomCardBank
{
    /// <summary>All cards, in authored order.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static ICard C(string category, string title, string body, Difficulty difficulty, int points) =>
        StandardCard.Create(title,
            $"<b>{Emoji(category)} {category.ToUpperInvariant()}  ·  {points} point{(points == 1 ? "" : "s")}</b>\n\n{body}",
            difficulty, category);

    private static string Emoji(string category) => category switch
    {
        "Setup"    => "📋",
        "Describe" => "🗣️",
        "Perform"  => "🎭",
        "Guess Us" => "🤔",
        "Race"     => "⏱️",
        "Decider"  => "🏆",
        _          => "🎲",
    };

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── SETUP ─────────────────────────────────────────────────────────────
        C("Setup", "Pick Your Sides",
          "Split into two teams. Uneven is fine — the smaller team goes first every time, which is the whole " +
          "handicap system and it works. Name your teams. Bad names are traditional.", Difficulty.Easy, 0),
        C("Setup", "Who's Keeping Score",
          "One person, from either team, keeps score on paper where everyone can see it. Each card says what " +
          "it's worth. Arguments about the score are settled by whoever is holding the pen.", Difficulty.Easy, 0),

        // ── DESCRIBE — talk, without the obvious words ────────────────────────
        C("Describe", "Without The Word",
          "Your team picks a person in the room. Describe them to the other team without naming any part of " +
          "their appearance. They get three guesses.", Difficulty.Medium, 2),
        C("Describe", "Explain Your Job Badly",
          "One of you explains their job — or their day — as unhelpfully as possible while staying strictly " +
          "truthful. Other team guesses what it is.", Difficulty.Easy, 1),
        C("Describe", "Three Clues Only",
          "Think of a film, book or song. You get exactly three clues, one word each. Other team gets one " +
          "guess. Land it and take three points.", Difficulty.Hard, 3),
        C("Describe", "The Long Way Round",
          "Describe a common object without using its name or what it's for. Other team guesses. If they get " +
          "it in under ten seconds, they take the point instead.", Difficulty.Medium, 2),

        // ── PERFORM — act, sing, draw ─────────────────────────────────────────
        C("Perform", "One Of You Acts",
          "Nominate an actor. They get thirty seconds to act out something the other team has to name. No " +
          "sounds, no words, no pointing at objects in the room.", Difficulty.Medium, 2),
        C("Perform", "Two Of You, One Thing",
          "Two of your team act out a single thing together — a machine, an animal, a situation. Other team " +
          "names it. Worth double because coordinating this is genuinely hard.", Difficulty.Hard, 3),
        C("Perform", "Hum It",
          "One of you hums a tune. Nothing else. Other team names it. If they can't, someone from their team " +
          "may hum their guess back for a consolation point.", Difficulty.Easy, 1),
        C("Perform", "Draw It Blind",
          "One of you draws something with your eyes shut while your team calls instructions. Other team has " +
          "to name it from the drawing alone once you stop.", Difficulty.Hard, 3),
        C("Perform", "The Reenactment",
          "Reenact something that actually happened to your team tonight — or on the way here. Other team " +
          "guesses what it was.", Difficulty.Medium, 2),

        // ── GUESS US — the other team predicts your team ───────────────────────
        C("Guess Us", "How Would We Answer",
          "The other team reads out a question. Every one of your team answers privately in writing. The other " +
          "team then predicts how many of you gave the same answer — exact match, they score.", Difficulty.Medium, 2),
        C("Guess Us", "Rank Us",
          "Other team ranks your team on something harmless — most likely to be late, worst at directions, " +
          "first to cry at a film. Your team privately agrees the real order. Points for how close they got.", Difficulty.Medium, 2),
        C("Guess Us", "Who Said It",
          "Each of your team writes down an opinion nobody would guess was theirs. Read them out shuffled. " +
          "Other team matches statements to people. One point each correct.", Difficulty.Hard, 3),
        C("Guess Us", "Odd One Out",
          "Your team states three facts about itself; two true of everyone on the team, one true of only one " +
          "person. Other team finds the odd one and names who.", Difficulty.Hard, 3),

        // ── RACE — both teams at once ─────────────────────────────────────────
        C("Race", "Both Teams, Same Category",
          "Someone neutral names a category. Both teams write as many items as they can in sixty seconds. " +
          "Longest valid list takes two points; anything on both lists gets struck off first.", Difficulty.Medium, 2),
        C("Race", "First To Find It",
          "Someone names an object that plausibly exists in this room or in somebody's bag. First team to " +
          "produce it takes the point. No breaking anything and no going outside.", Difficulty.Easy, 1),
        C("Race", "Twenty Questions, Both Sides",
          "One team thinks of something. The other asks yes/no questions and gets twenty. Guess it and they " +
          "take three; run out and the thinking team takes two.", Difficulty.Hard, 3),
        C("Race", "Alphabet Sprint",
          "A category, and both teams race to name something for every letter A to J. First team to finish " +
          "reads their list; anything the table rejects hands the round to the other side.", Difficulty.Extreme, 4),

        // ── DECIDER — for a level score ───────────────────────────────────────
        C("Decider", "Sudden Death",
          "Level scores only. One representative from each team. A category is named; you alternate answers " +
          "with no repeats and no hesitation. First to falter loses the whole game.", Difficulty.Extreme, 5),
        C("Decider", "One Question Each",
          "Level scores only. Each team writes one question they believe the other cannot answer about them. " +
          "Swap. Answer correctly and you win; both right or both wrong and you go again.", Difficulty.Extreme, 5),
        C("Decider", "Call It A Draw",
          "If you're level and nobody wants sudden death, this card ends it as a draw and says so out loud, " +
          "which is a better ending than an argument. That's the deck.", Difficulty.Easy, 0),
    ];
}
