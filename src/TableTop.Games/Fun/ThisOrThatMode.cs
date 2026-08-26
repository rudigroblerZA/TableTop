using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Fun;

/// <summary>
/// This Or That — two options, side by side, pick one and find out what it
/// says about you. The first mode built on <see cref="IThisOrThatCard"/>.
///
/// <para>
/// Every card names two things and asks which you'd take. Once everyone has
/// picked — out loud, at the same time, so nobody drifts toward the popular
/// answer — the detail on each side is read out. The detail isn't a score or
/// a right answer; it's the payoff, and it's usually the part that starts the
/// argument.
/// </para>
///
/// <para>
/// <b>Images are optional by design.</b> Each option carries an
/// <c>ImageKey</c>, a logical asset name a head resolves against its own asset
/// store — but a card with no image is fully playable, and this deck ships
/// keys without requiring any asset to exist yet. A head that finds no asset
/// for a key falls back to the label, which is why every option has one. That
/// means the mode is complete and shippable today, and gains illustration
/// later without a content rewrite.
/// </para>
/// </summary>
public sealed class ThisOrThatMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "This Or That";

    /// <inheritdoc />
    public override string Description =>
        "Two options, side by side. Everyone picks at once — then find out what each choice says about you.";

    /// <inheritdoc />
    public override string CompleteLabel => "Picked";

    /// <inheritdoc />
    public override string SkipLabel => "Can't Choose";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["How To Play"] = "#26A69A",
            ["Everyday"]    = "#42A5F5",
            ["Food"]        = "#FFA726",
            ["Would You"]   = "#AB47BC",
            ["Deep End"]    = "#EF5350",
        };

    /// <summary>The rules card explains simultaneous picking, which the mode depends on.</summary>
    public override IReadOnlyList<string> CategoriesPinnedToStart => ["How To Play"];

    /// <inheritdoc />
    protected override IScoringStrategy BuildScoring() => new FixedScoringStrategy(1);

    /// <inheritdoc />
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        ThisOrThatCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => ThisOrThatCardBank.All;
}

/// <summary>
/// Built-in card bank for This Or That.
///
/// <para>
/// Not authored with <c>CardDeckBuilder</c>: that builder produces
/// <c>StandardCard</c>s, and these need to be <see cref="ThisOrThatCard"/>s.
/// <see cref="ThisOrThatCard.Create"/> uses the same content-derived
/// deterministic id scheme, so the property that mattered — a saved session
/// still resolving its cards after a restart — is preserved.
/// </para>
/// </summary>
public static class ThisOrThatCardBank
{
    private const string Deck = "This Or That";

    /// <summary>All cards, in intended play order.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static ICard Rules() => new StandardCard(
        new Guid(System.Security.Cryptography.MD5.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{Deck}|How To Play|rules"))),
        "How This Works",
        "Every card offers two options.\n\n" +
        "<b>Everyone picks at the same time</b> — count down from three and say it together, or point. " +
        "Picking simultaneously matters: go round the table one at a time and people drift toward whatever the last person said.\n\n" +
        "Then read out what each option says about whoever picked it. It isn't a score and there's no right answer — " +
        "it's just the bit that starts the argument.",
        Difficulty.Easy,
        "How To Play");

    private static ICard C(string category, string title, string question,
        string labelA, string keyA, string detailA,
        string labelB, string keyB, string detailB,
        Difficulty difficulty = Difficulty.Easy) =>
        ThisOrThatCard.Create(Deck, title, question, difficulty, category,
            new ThisOrThatOption(labelA, keyA, detailA),
            new ThisOrThatOption(labelB, keyB, detailB));

    private static IReadOnlyList<ICard> Build() =>
    [
        Rules(),

        // ── EVERYDAY ─────────────────────────────────────────────────────────
        C("Everyday", "Morning", "Which morning would you rather have?",
            "Sunrise walk",  "tot-sunrise",  "You'd rather earn the day than be handed it. People find this either inspiring or unbearable.",
            "Lie-in",        "tot-liein",    "You know rest isn't laziness. You've probably had to defend that at least once."),
        C("Everyday", "The Commute", "Pick your journey.",
            "Empty road",    "tot-road",     "You want the time to think. You'd take longer if it meant being alone with your thoughts.",
            "Packed train",  "tot-train",    "You'd rather be moving with people than moving alone. You read on public transport and mean it."),
        C("Everyday", "Weekend", "Two days off. Which?",
            "Nothing booked","tot-empty",    "You protect unstructured time. Someone in your life finds this frustrating.",
            "Full calendar", "tot-calendar", "You get more rest from doing things than from doing nothing. This confuses the other type."),
        C("Everyday", "The Room", "Which space is yours?",
            "Spotless",      "tot-tidy",     "Your outside matches your inside, or you're using one to manage the other.",
            "Lived in",      "tot-messy",    "You'd rather the room served you than the other way round. You know where everything is."),

        // ── FOOD ─────────────────────────────────────────────────────────────
        C("Food", "The Meal", "Last meal, no consequences.",
            "Something new", "tot-new",      "Novelty beats certainty for you, even at the end. You've been burned by this and did it again.",
            "The old favourite","tot-fav",   "You know what's good and you're not performing for anyone. There's confidence in that."),
        C("Food", "Sweet Or Salt", "One flavour for the rest of your life.",
            "Sweet",         "tot-sweet",    "You go toward pleasure directly. Little patience for the long way round.",
            "Salt",          "tot-salt",     "You like things that make you want more rather than things that satisfy. Read into that what you like."),
        C("Food", "The Table", "Where are you eating?",
            "Street food",   "tot-street",   "You care more about the thing itself than the setting. Hard to impress with a tablecloth.",
            "Long dinner",   "tot-dinner",   "The meal is the excuse; the sitting there is the point. You'd stay for hours."),

        // ── WOULD YOU ────────────────────────────────────────────────────────
        C("Would You", "The Ability", "Pick a power.",
            "Fly",           "tot-fly",      "You want out — of rooms, of situations, of the ground. Escape appeals to you more than most.",
            "Invisible",     "tot-invisible","You want to observe without cost. Consider what you'd actually do with it.",
            Difficulty.Medium),
        C("Would You", "Time", "One trip, one way.",
            "Meet your past self",  "tot-past",   "There's something you'd warn yourself about. You know exactly what it is.",
            "Meet your future self","tot-future", "You want reassurance more than you want to change anything. Or you're just nosy.",
            Difficulty.Medium),
        C("Would You", "The Truth", "One of these, permanently.",
            "Always know when someone's lying", "tot-lie",   "You'd take painful clarity over comfortable doubt. That costs more than people expect.",
            "Always be believed",               "tot-trust", "You want to be taken at your word. Ask yourself whether that's about trust or about winning.",
            Difficulty.Medium),
        C("Would You", "The Audience", "How does the work land?",
            "Loved by a few",  "tot-few",    "Depth over reach. You'd rather matter enormously to a handful of people.",
            "Liked by many",   "tot-many",   "Reach over depth. There's nothing shallow in wanting to be part of something big.",
            Difficulty.Medium),

        // ── DEEP END ─────────────────────────────────────────────────────────
        C("Deep End", "The Regret", "Which would you rather carry?",
            "The thing you did",     "tot-did",    "You'd rather have acted and been wrong. You can live with consequences better than questions.",
            "The thing you didn't",  "tot-didnt",  "You'd rather keep the possibility intact. That's safer and it costs you something.",
            Difficulty.Hard),
        C("Deep End", "Being Known", "Pick one.",
            "Fully known by one person", "tot-one", "You want somewhere to put all of it. That's a lot to ask of one person, and you know it.",
            "Partly known by many",      "tot-many2","You'd rather be widely liked than deeply seen. That's a real choice, not a failure of nerve.",
            Difficulty.Hard),
        C("Deep End", "The Harder Thing", "Which do you actually find harder?",
            "Asking for help",   "tot-ask",   "You'd rather struggle than owe. Worth asking who taught you that.",
            "Being asked",       "tot-asked", "You'd rather be needed than need. Also worth asking about.",
            Difficulty.Hard),
    ];
}
