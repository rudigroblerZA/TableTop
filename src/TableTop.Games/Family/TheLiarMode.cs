using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// The Liar — collaborative storytelling where one person is lying.
///
/// How to play:
///   1. Read the scenario aloud.
///   2. Three volunteers: each one tells a short part of the story (their character's perspective).
///   3. One storyteller is COMPLETELY LYING. The other two are truthful.
///   4. Everyone votes on who they think is lying.
///   5. If you correctly identify the liar, you get points. If the liar fools everyone, THEY get points.
///
/// The Liar must commit fully and be convincing. The truth-tellers must be consistent but
/// also not obviously synchronized. It's social deduction meets storytelling meets improv.
/// Some liars lean into chaos; some build elaborate fake details. Some are caught instantly.
///
/// Great for groups that love debate and catching each other in lies. Different every time
/// because the lies are improvised and people have wildly different strategies for both
/// lying and truth-telling.
/// </summary>
public sealed class TheLiarMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "The Liar";
    /// <inheritdoc />
    public override string Description =>
        "Three people tell the same story. One is lying. Can you spot the impostor?";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Guessed";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "Skip";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Everyday"] = "#42A5F5",
            ["Adventure"] = "#66BB6A",
            ["Workplace"] = "#FFA726",
            ["Relationship"] = "#EC407A",
            ["Disaster"] = "#EF5350",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        TheLiarCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => TheLiarCardBank.All;
}

/// <summary>Built-in card bank for The Liar. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class TheLiarCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── EVERYDAY ──────────────────────────────────────────────────────────
        L("Everyday",
            "Three roommates tell the story of whose turn it was to do the dishes — and the disaster that followed.",
            "One roommate is completely lying about their part. Two are truthful."),
        L("Everyday",
            "Three coworkers recount what happened at last night's team happy hour.",
            "One person's story about what they were doing is totally made up."),
        L("Everyday",
            "Three friends describe the night they lost someone's wallet.",
            "One of them didn't actually lose it — they're making their part up entirely."),

        // ── ADVENTURE ────────────────────────────────────────────────────────
        L("Adventure",
            "Three hikers describe getting lost on a trail and how they found their way back.",
            "One hiker is inventing their part of the story completely. What actually happened vs what they're claiming?"),
        L("Adventure",
            "Three people tell the story of a road trip that had one major breakdown.",
            "One person's version of events is completely fabricated. Two are accurate."),
        L("Adventure",
            "Three vacationers recount an unexpected encounter with wildlife.",
            "One person's story about the animal is pure fiction. The others are telling the truth."),

        // ── WORKPLACE ────────────────────────────────────────────────────────
        L("Workplace",
            "Three employees describe a meeting that went catastrophically wrong.",
            "One person is lying about what was said or what happened. The others are truthful."),
        L("Workplace",
            "Three people recount the day someone accidentally deleted an important file.",
            "One of them is lying about their involvement. Two are telling the truth."),
        L("Workplace",
            "Three coworkers describe a client interaction that became awkward.",
            "One person's version of events is completely invented. Two are accurate."),

        // ── RELATIONSHIP ──────────────────────────────────────────────────────
        L("Relationship",
            "Three people describe the argument that led to someone sleeping on the couch.",
            "One person is completely lying about what they said. Two are truthful."),
        L("Relationship",
            "Three friends recount the night one of them got caught in an embarrassing situation.",
            "One friend is fabricating their part of the story entirely."),
        L("Relationship",
            "Three people tell the story of someone meeting their partner's parents for the first time.",
            "One person's account of what happened is completely made up. Two are accurate."),

        // ── DISASTER ──────────────────────────────────────────────────────────
        L("Disaster",
            "Three people describe the day someone's phone was stolen from a restaurant.",
            "One person is completely lying about where they were or what they saw."),
        L("Disaster",
            "Three witnesses recount a car accident at an intersection.",
            "One witness is inventing their version of events entirely. Two saw what actually happened."),
        L("Disaster",
            "Three people describe the house fire that destroyed a neighbor's home.",
            "One person is lying about the circumstances completely. Two are truthful."),
    ];

    private static ICard L(string category, string scenario, string detail) =>
        StandardCard.Create(
            category,
            "<b>THE LIAR SETUP</b>\n\n" +
            scenario + "\n\n" +
            detail + "\n\n" +
            "<b>HOW TO PLAY:</b>\n" +
            "• Ask 3 volunteers to leave the room\n" +
            "• Tell them the scenario (one knows they're the liar)\n" +
            "• Each tells their part (30 seconds each)\n" +
            "• Group votes: who's lying?\n" +
            "• Correct votes = 1 point. Liar fools everyone = 3 points.",
            Difficulty.Medium, category);
}
