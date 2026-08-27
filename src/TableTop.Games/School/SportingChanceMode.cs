using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Sporting Chance — general-knowledge sport for the classroom: the rules and
/// basics of popular games, the Olympics, and the wider world of sport, as
/// multiple-choice questions dealt one at a time.
///
/// Deliberately kept to timeless general knowledge — how many players, what the
/// pitch is called, which sport uses which equipment, Olympic basics — rather
/// than current results or named players, so it doesn't go stale and stays fair
/// to pupils who don't follow a particular league.
/// </summary>
public sealed class SportingChanceMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Sporting Chance";
    /// <inheritdoc />
    public override string Description =>
        "Sport general knowledge — rules and basics, equipment, the Olympics, and world sport. Multiple choice, four difficulties.";

    /// <summary>Label for a correctly answered card.</summary>
    public override string CompleteLabel => "Correct";
    /// <summary>Label for a passed card.</summary>
    public override string SkipLabel => "Pass";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [SportingChanceCardBank.TheBasicsCategory] = "#66BB6A",
            [SportingChanceCardBank.OnTheFieldCategory] = "#42A5F5",
            [SportingChanceCardBank.TheOlympicsCategory] = "#FFCA28",
            [SportingChanceCardBank.EquipmentCategory] = "#FFA726",
            [SportingChanceCardBank.WorldSportCategory] = "#26A69A",
        };

    /// <summary>Harder questions score more.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in Sporting Chance card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        SportingChanceCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => SportingChanceCardBank.All;
}

/// <summary>Built-in card bank for Sporting Chance.</summary>
public static class SportingChanceCardBank
{
    internal const string TheBasicsCategory = "The Basics";
    internal const string OnTheFieldCategory = "On the Field";
    internal const string TheOlympicsCategory = "The Olympics";
    internal const string EquipmentCategory = "Equipment";
    internal const string WorldSportCategory = "World Sport";

    /// <summary>All cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── THE BASICS ───────────────────────────────────────────────────────
        Q(TheBasicsCategory, "How many players are on a soccer (football) team on the field?", "9", "10", "11", "12", AnswerLabel.C, Difficulty.Easy),
        Q(TheBasicsCategory, "In basketball, how many points is a standard field goal (not a three-pointer or free throw)?", "1", "2", "3", "4", AnswerLabel.B, Difficulty.Easy),
        Q(TheBasicsCategory, "How many players are on a basketball team on the court?", "5", "6", "7", "9", AnswerLabel.A, Difficulty.Medium),
        Q(TheBasicsCategory, "In tennis, what is a score of zero called?", "Nil", "Duck", "Love", "Blank", AnswerLabel.C, Difficulty.Medium),
        Q(TheBasicsCategory, "How many players are on a cricket team?", "9", "10", "11", "12", AnswerLabel.C, Difficulty.Medium),
        Q(TheBasicsCategory, "In which sport might you score a 'try'?", "Rugby", "Hockey", "Golf", "Tennis", AnswerLabel.A, Difficulty.Hard),

        // ── ON THE FIELD — terms and places of play ──────────────────────────
        Q(OnTheFieldCategory, "What is the playing area in tennis called?", "Pitch", "Court", "Rink", "Ring", AnswerLabel.B, Difficulty.Easy),
        Q(OnTheFieldCategory, "Ice hockey is played on a…?", "Court", "Pitch", "Rink", "Track", AnswerLabel.C, Difficulty.Easy),
        Q(OnTheFieldCategory, "In golf, what is the term for one stroke under par on a hole?", "Eagle", "Birdie", "Bogey", "Albatross", AnswerLabel.B, Difficulty.Hard),
        Q(OnTheFieldCategory, "What do you call the person who enforces the rules in soccer?", "Umpire", "Referee", "Judge", "Marshal", AnswerLabel.B, Difficulty.Easy),
        Q(OnTheFieldCategory, "In baseball, how many strikes make an out?", "2", "3", "4", "5", AnswerLabel.B, Difficulty.Medium),
        Q(OnTheFieldCategory, "How many rings are on the Olympic flag?", "4", "5", "6", "7", AnswerLabel.B, Difficulty.Easy),

        // ── THE OLYMPICS ─────────────────────────────────────────────────────
        Q(TheOlympicsCategory, "In which country did the Olympic Games begin in ancient times?", "Rome", "Greece", "Egypt", "China", AnswerLabel.B, Difficulty.Easy),
        Q(TheOlympicsCategory, "How often are the modern Summer Olympic Games held?", "Every year", "Every 2 years", "Every 4 years", "Every 5 years", AnswerLabel.C, Difficulty.Easy),
        Q(TheOlympicsCategory, "What colour medal does an Olympic winner receive?", "Silver", "Bronze", "Gold", "Platinum", AnswerLabel.C, Difficulty.Easy),
        Q(TheOlympicsCategory, "Which of these is a Winter Olympic sport?", "Rowing", "Bobsleigh", "Archery", "Hurdles", AnswerLabel.B, Difficulty.Medium),
        Q(TheOlympicsCategory, "What is the shape formed by the five Olympic rings — they are…?", "Stacked in a line", "Interlocking", "Separate circles", "Inside one another", AnswerLabel.B, Difficulty.Hard),

        // ── EQUIPMENT ────────────────────────────────────────────────────────
        Q(EquipmentCategory, "Which sport uses a shuttlecock?", "Tennis", "Badminton", "Squash", "Table tennis", AnswerLabel.B, Difficulty.Medium),
        Q(EquipmentCategory, "In which sport do you use clubs and a small dimpled ball?", "Hockey", "Golf", "Cricket", "Polo", AnswerLabel.B, Difficulty.Easy),
        Q(EquipmentCategory, "A puck is used in which sport?", "Lacrosse", "Ice hockey", "Curling", "Handball", AnswerLabel.B, Difficulty.Medium),
        Q(EquipmentCategory, "Which sport uses a bat, stumps, and bails?", "Baseball", "Cricket", "Rounders", "Softball", AnswerLabel.B, Difficulty.Medium),
        Q(EquipmentCategory, "Boxing takes place in a…?", "Court", "Cage", "Ring", "Pit", AnswerLabel.C, Difficulty.Easy),

        // ── WORLD SPORT ──────────────────────────────────────────────────────
        Q(WorldSportCategory, "The soccer World Cup is held every how many years?", "2", "3", "4", "5", AnswerLabel.C, Difficulty.Medium),
        Q(WorldSportCategory, "Which sport is often called 'the beautiful game'?", "Cricket", "Soccer", "Rugby", "Tennis", AnswerLabel.B, Difficulty.Medium),
        Q(WorldSportCategory, "The Tour de France is a famous race in which sport?", "Running", "Cycling", "Swimming", "Rowing", AnswerLabel.B, Difficulty.Medium),
        Q(WorldSportCategory, "Sumo wrestling comes from which country?", "China", "Korea", "Japan", "Thailand", AnswerLabel.C, Difficulty.Hard),
        Q(WorldSportCategory, "Which racket sport is played against a wall in an enclosed court?", "Tennis", "Badminton", "Squash", "Table tennis", AnswerLabel.C, Difficulty.Extreme),
    ];

    private static ICard Q(string cat, string question, string a, string b, string c, string d, AnswerLabel correct, Difficulty diff) =>
        MultipleChoiceCard.Create(question, a, b, c, d, correct, diff, cat);
}
