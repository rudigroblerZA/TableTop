using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Useless Superpowers — the hero draft nobody asked for.
///
/// How to play:
///   1. The active player draws a card and receives a magnificently useless
///      superpower. It is theirs now. No refunds.
///   2. They have 60 seconds to PITCH it to the group: why this power actually
///      makes them the most valuable hero alive. The card includes the power's
///      one crushing limitation — the pitch must work around it.
///   3. Some cards are SHOWDOWNS: two players draw imaginary swords and argue
///      whose (previously won) power beats the other's in the given crisis.
///   4. The group votes. Best pitch takes the point.
///
/// The comedy engine: real persuasion skills aimed at an indefensible position.
/// The best players stop apologising for the power and start weaponising the
/// limitation — which is, quietly, how every good pitch in the real world works.
/// </summary>
public sealed class UselessSuperpowersMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Useless Superpowers";
    /// <inheritdoc />
    public override string Description =>
        "Draw a terrible power, then pitch the group on why it makes you the greatest hero alive.";

    /// <summary>Label shown on the button that records the round's winning pitch.</summary>
    public override string CompleteLabel => "Best Pitch";
    /// <summary>Label shown on the button that skips the current card.</summary>
    public override string SkipLabel => "Recast";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Barely Super"] = "#42A5F5",
            ["Cursed Timing"] = "#FFA726",
            ["Tiny Scale"] = "#66BB6A",
            ["Wrong Target"] = "#AB47BC",
            ["Showdown"] = "#EF5350",
            ["Origin Story"] = "#EC407A",
        };

    /// <summary>One point to the round's voted-best pitch.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Returns the built-in useless-superpowers card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        UselessSuperpowersCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => UselessSuperpowersCardBank.All;
}

/// <summary>Built-in card bank for Useless Superpowers.</summary>
public static class UselessSuperpowersCardBank
{
    /// <summary>All useless-superpower cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── BARELY SUPER ──────────────────────────────────────────────────────
        P("Barely Super", "You can fly", "…at walking pace, 30 centimetres off the ground.", Difficulty.Easy),
        P("Barely Super", "You are invisible", "…but only while nobody is looking at you.", Difficulty.Medium),
        P("Barely Super", "Super strength", "…in your left little finger only.", Difficulty.Easy),
        P("Barely Super", "You can read minds", "…but only of people who are currently thinking about sandwiches.", Difficulty.Easy),
        P("Barely Super", "You can teleport", "…exactly one metre, once per day, with a loud honk.", Difficulty.Medium),
        P("Barely Super", "You can breathe underwater", "…but only in water shallower than your knees.", Difficulty.Medium),
        P("Barely Super", "Laser vision", "…at the temperature of a warm cup of tea.", Difficulty.Easy),
        P("Barely Super", "You can stop time", "…for exactly one second, and you're frozen too.", Difficulty.Hard),

        // ── CURSED TIMING ─────────────────────────────────────────────────────
        P("Cursed Timing", "You can predict the future", "…exactly four seconds ahead, and only while sneezing.", Difficulty.Medium),
        P("Cursed Timing", "You become incredibly persuasive", "…between 3:00 and 3:07 a.m.", Difficulty.Medium),
        P("Cursed Timing", "You can run at the speed of sound", "…but only when you urgently need the toilet.", Difficulty.Easy),
        P("Cursed Timing", "You gain photographic memory", "…of the previous Tuesday only, refreshed weekly.", Difficulty.Hard),
        P("Cursed Timing", "You can talk to animals", "…but only ones that are asleep.", Difficulty.Medium),
        P("Cursed Timing", "You are immune to all damage", "…during your birthday.", Difficulty.Medium),
        P("Cursed Timing", "You can duplicate yourself", "…but the copy shows up 45 minutes late.", Difficulty.Hard),
        P("Cursed Timing", "You can rewind time by ten seconds", "…but everyone remembers both versions.", Difficulty.Extreme),

        // ── TINY SCALE ────────────────────────────────────────────────────────
        P("Tiny Scale", "You control the weather", "…inside a one-metre bubble around you.", Difficulty.Medium),
        P("Tiny Scale", "You can move objects with your mind", "…up to the weight of a single grape.", Difficulty.Easy),
        P("Tiny Scale", "You can heal any wound", "…paper cuts.", Difficulty.Easy),
        P("Tiny Scale", "You can shape-shift", "…into a slightly shorter version of yourself.", Difficulty.Medium),
        P("Tiny Scale", "You can speak every language", "…one word per language.", Difficulty.Hard),
        P("Tiny Scale", "You can turn invisible any object", "…smaller than a coin, for five seconds.", Difficulty.Medium),
        P("Tiny Scale", "You can summon any animal", "…snails. You can summon snails.", Difficulty.Easy),
        P("Tiny Scale", "You generate electricity", "…enough to charge a phone by 1% per hour of vigorous dancing.", Difficulty.Medium),

        // ── WRONG TARGET ──────────────────────────────────────────────────────
        P("Wrong Target", "You can make anyone fall asleep instantly", "…it only works on yourself.", Difficulty.Easy),
        P("Wrong Target", "You can find anything that's lost", "…that belongs to strangers.", Difficulty.Medium),
        P("Wrong Target", "You always know when someone is lying", "…about cheese.", Difficulty.Easy),
        P("Wrong Target", "You can grant wishes", "…exclusively wishes people mutter sarcastically.", Difficulty.Hard),
        P("Wrong Target", "You can erase memories", "…only your own, only embarrassing ones, only at random.", Difficulty.Medium),
        P("Wrong Target", "You can control machines with your mind", "…printers. Only printers. They still jam.", Difficulty.Medium),
        P("Wrong Target", "Everything you touch turns to gold", "…for four seconds, then back, slightly stickier.", Difficulty.Hard),
        P("Wrong Target", "You can talk to plants", "…they are extremely boring.", Difficulty.Easy),
        // ── EXPANSION: MONKEY'S PAW EDITION ──────────────────────────────────
        P("Barely Super", "You can read minds", "…but you read them out loud, in full, immediately.", Difficulty.Hard),
        P("Barely Super", "You never need to sleep", "…but you are exhausted the entire time.", Difficulty.Medium),
        P("Barely Super", "You can pause any conversation", "…everyone remembers exactly where it stopped, including the question you were avoiding.", Difficulty.Hard),
        P("Barely Super", "You know everyone's secrets", "…and they instantly know that you know.", Difficulty.Extreme),
        P("Cursed Timing", "You can time travel", "…only to moments you embarrassed yourself. As a spectator. Front row.", Difficulty.Medium),
        P("Cursed Timing", "You always win arguments", "…three days after the argument has ended.", Difficulty.Medium),
        P("Cursed Timing", "You can become famous instantly", "…for the least impressive thing you did this week.", Difficulty.Hard),
        P("Cursed Timing", "Your wishes come true", "…in the order you made them, starting from age four.", Difficulty.Extreme),
        P("Wrong Target", "You can silence any room", "…by starting to explain your hobbies.", Difficulty.Easy),
        P("Wrong Target", "You can smell lies", "…and they smell like your least favourite food, and lingering.", Difficulty.Medium),
        P("Wrong Target", "You can delete one memory per day", "…from your pet. Your pet has very few memories.", Difficulty.Medium),
        P("Wrong Target", "Everyone tells you the truth", "…about your driving.", Difficulty.Hard),
        P("Tiny Scale", "You can fly through time", "…at one second per second, forward only. So: living.", Difficulty.Hard),
        P("Tiny Scale", "You can make anything disappear", "…into your other hand.", Difficulty.Easy),
        P("Tiny Scale", "You have a sixth sense", "…for when bread is about to go stale. 45 seconds' warning.", Difficulty.Medium),
        P("Tiny Scale", "You are fluent in sarcasm", "…written sarcasm. Only when reading tax documents.", Difficulty.Hard),


        // ── SHOWDOWN ──────────────────────────────────────────────────────────
        S("Showdown", "A cat is stuck in a tree.",
          "Two players: argue whose previously-drawn power handles this crisis better. Group votes.", Difficulty.Medium),
        S("Showdown", "The city's bridge is collapsing in ten minutes.",
          "Two players: argue whose previously-drawn power saves more people. Group votes.", Difficulty.Medium),
        S("Showdown", "An alien delegation lands and demands to meet Earth's mightiest hero.",
          "Two players: argue why the aliens should pick YOU. Group votes.", Difficulty.Hard),
        S("Showdown", "A supervillain has stolen every left shoe in the country.",
          "Two players: argue whose power cracks the case. Group votes.", Difficulty.Medium),
        S("Showdown", "The world's coffee supply will run out by Friday.",
          "Two players: argue whose power averts the crisis. Group votes.", Difficulty.Hard),
        S("Showdown", "A toddler's birthday party has descended into chaos.",
          "Two players: argue whose power restores order. Group votes.", Difficulty.Easy),

        // ── ORIGIN STORY ──────────────────────────────────────────────────────
        O("Origin Story", "Tell the tragic origin story of how you got your most recently drawn power.", Difficulty.Medium),
        O("Origin Story", "Deliver your hero catchphrase and pose. It must reference your power's limitation.", Difficulty.Easy),
        O("Origin Story", "Describe your arch-nemesis — the one villain your useless power is PERFECTLY suited to defeat.", Difficulty.Hard),
        O("Origin Story", "Pitch the blockbuster movie of your hero. Title, tagline, and the trailer voice-over.", Difficulty.Hard),
        O("Origin Story", "Explain why the hero union rejected your application — and why they were wrong.", Difficulty.Medium),
        O("Origin Story", "Design your hero costume out loud. Every element must be justified by your power.", Difficulty.Medium),
    ];

    private static ICard P(string category, string power, string limitation, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>Your new power: " + power + "</b>\n\n" +
            "<b>The catch:</b> " + limitation + "\n\n" +
            "You have 60 seconds. Pitch the group: why does THIS power make you the most " +
            "valuable hero alive? Own the catch — don't apologise for it.\n\n" +
            "<i>Group votes. Best pitch takes the point.</i>",
            d, category);

    private static ICard S(string category, string crisis, string instructions, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>⚔️ SHOWDOWN — Crisis:</b> " + crisis + "\n\n" + instructions + "\n\n" +
            "<i>No previously drawn powers yet? Both players draw the next card each and battle with those.</i>",
            d, category);

    private static ICard O(string category, string prompt, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>🎬 " + prompt + "</b>\n\n" +
            "<i>Group votes on commitment, drama, and shamelessness. Winner takes the point.</i>",
            d, category);
}
