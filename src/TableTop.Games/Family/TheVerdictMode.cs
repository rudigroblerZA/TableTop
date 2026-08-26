using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// The Verdict — a voting and debate game for silly (and not-so-silly) arguments.
///
/// How to play:
///   1. Read a statement or scenario aloud.
///   2. Everyone votes privately: Agree or Disagree.
///   3. Reveal the split and debate for 30 seconds.
///   4. Points awarded for being in the majority — and bonus points if you change someone's mind.
///
/// Cards range from obvious jokes ("Is it okay to eat cereal with orange juice?") to
/// genuinely debatable questions ("Should honesty always come before kindness?"). Great
/// for getting to know how people think, not just what they think.
///
/// Works for any age and any group size. Creates natural conversation. Plus there's always
/// one person who votes "yes" to something absurd and has to defend it.
/// </summary>
public sealed class TheVerdictMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name        => "The Verdict";
    /// <inheritdoc />
    public override string Description =>
        "Vote on silly statements. Debate. Find out who's reasonable and who's chaos.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "Voted";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel     => "Pass";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Silly"]       = "#EC407A",
            ["Food"]        = "#FFCA28",
            ["Manners"]     = "#42A5F5",
            ["Morality"]    = "#66BB6A",
            ["Weird"]       = "#AB47BC",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        TheVerdictCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => TheVerdictCardBank.All;
}

/// <summary>Built-in card bank for The Verdict. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class TheVerdictCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── SILLY ────────────────────────────────────────────────────────────
        V("Silly", "It is morally acceptable to eat cereal with orange juice instead of milk.", Difficulty.Easy),
        V("Silly", "Socks with sandals is a respectable fashion choice.", Difficulty.Easy),
        V("Silly", "Hot dogs are a type of sandwich.", Difficulty.Easy),
        V("Silly", "You should be allowed to wear pyjamas to work if you work from home.", Difficulty.Easy),
        V("Silly", "It's acceptable to go to the pub in your gym clothes without changing.", Difficulty.Easy),
        V("Silly", "Eating pizza with a fork and knife is more civilised than with your hands.", Difficulty.Easy),
        V("Silly", "Cereal is a soup.", Difficulty.Medium),
        V("Silly", "Wearing matching outfits with your partner is cute, not embarrassing.", Difficulty.Easy),
        V("Silly", "It's acceptable to wear a onesie to run errands.", Difficulty.Easy),
        V("Silly", "Bouncy castles should be a permanent fixture in every town square.", Difficulty.Medium),

        // ── FOOD ─────────────────────────────────────────────────────────────
        V("Food", "Pineapple belongs on pizza.", Difficulty.Easy),
        V("Food", "Ketchup on a hot dog is disgusting.", Difficulty.Easy),
        V("Food", "Chocolate and salt go together perfectly.", Difficulty.Easy),
        V("Food", "You should eat dessert before the main course.", Difficulty.Medium),
        V("Food", "Crunchy peanut butter is superior to smooth.", Difficulty.Easy),
        V("Food", "Breakfast is the most important meal of the day.", Difficulty.Easy),
        V("Food", "It's acceptable to eat ice cream right out of the tub.", Difficulty.Easy),
        V("Food", "You should always finish your plate, even if you're full.", Difficulty.Medium),
        V("Food", "Beans belong in chilli.", Difficulty.Easy),
        V("Food", "Olives are either delicious or disgusting — no middle ground.", Difficulty.Easy),

        // ── MANNERS ──────────────────────────────────────────────────────────
        V("Manners", "It's okay to be on your phone while someone is talking to you.", Difficulty.Easy),
        V("Manners", "You should always RSVP to events, even if you're not going.", Difficulty.Medium),
        V("Manners", "It's rude to ask someone their age or how much they earn.", Difficulty.Easy),
        V("Manners", "You should take off your shoes when you enter someone's home.", Difficulty.Easy),
        V("Manners", "It's acceptable to be late if you text ahead.", Difficulty.Easy),
        V("Manners", "Chewing with your mouth open is one of the worst things.", Difficulty.Easy),
        V("Manners", "You should always say 'please' and 'thank you', even with family.", Difficulty.Medium),
        V("Manners", "It's okay to ask for something even if it might be inconvenient for someone.", Difficulty.Medium),

        // ── MORALITY ─────────────────────────────────────────────────────────
        V("Morality", "It is sometimes okay to tell a white lie to spare someone's feelings.", Difficulty.Hard),
        V("Morality", "You should always stand up for what you believe in, even at great personal cost.", Difficulty.Hard),
        V("Morality", "Everyone deserves a second chance, no matter what they did.", Difficulty.Hard),
        V("Morality", "It's more important to be honest than to be kind.", Difficulty.Hard),
        V("Morality", "You have a responsibility to help others, even if it's not convenient.", Difficulty.Hard),
        V("Morality", "Forgiveness is always the right choice.", Difficulty.Hard),
        V("Morality", "It's acceptable to ignore a friend's bad behaviour if confronting them would hurt the friendship.", Difficulty.Hard),
        V("Morality", "You should always do what your gut tells you, even if logic says otherwise.", Difficulty.Hard),

        // ── WEIRD ────────────────────────────────────────────────────────────
        V("Weird", "Pigeons are just tiny dinosaurs.", Difficulty.Easy),
        V("Weird", "The plural of octopus should be 'octopi', not 'octopuses'.", Difficulty.Easy),
        V("Weird", "Birds aren't real — they're government drones.", Difficulty.Easy),
        V("Weird", "Water has a taste.", Difficulty.Easy),
        V("Weird", "Your birthday cake should always be chocolate.", Difficulty.Easy),
        V("Weird", "You can be tickled by your own hands.", Difficulty.Easy),
        V("Weird", "Tomatoes are technically fruits, not vegetables.", Difficulty.Easy),
        V("Weird", "Everyone experiences colours the same way in their mind.", Difficulty.Hard),
    ];

    private static ICard V(string category, string verdict, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>Do you agree or disagree?</b>\n\n" + verdict +
            "\n\n<b>Vote silently:</b> Agree = thumbs up, Disagree = thumbs down.\n\n" +
            "<b>Reveal:</b> Show your votes and debate for 30 seconds. The majority is right... probably.",
            d, category);
}
