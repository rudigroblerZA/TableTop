using TableTop.Core.Abstractions.Cards;
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
            [ThisOrThatCardBank.HowToPlayCategory] = "#26A69A",
            [ThisOrThatCardBank.EverydayCategory] = "#42A5F5",
            [ThisOrThatCardBank.FoodCategory] = "#FFA726",
            [ThisOrThatCardBank.WouldYouCategory] = "#AB47BC",
            [ThisOrThatCardBank.DeepEndCategory] = "#EF5350",
        };

    /// <summary>The rules card explains simultaneous picking, which the mode depends on.</summary>
    public override IReadOnlyList<string> CategoriesPinnedToStart => [ThisOrThatCardBank.HowToPlayCategory];

    /// <inheritdoc />
    protected override IScoringStrategy BuildScoring() => new FixedScoringStrategy(1);

    /// <inheritdoc />
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        ThisOrThatCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => ThisOrThatCardBank.All;
}

/// <summary>
/// Built-in card bank for This Or That, authored with
/// <see cref="CardDeckBuilder"/> — the rules card via <c>Card</c>, the
/// comparisons via its <c>ThisOrThatCard</c> method, the same fluent DSL the
/// StandardCard decks use. The content-derived deterministic id scheme is
/// preserved, so a saved session still resolves its cards after a restart.
/// </summary>
public static class ThisOrThatCardBank
{
    internal const string HowToPlayCategory = "How To Play";
    internal const string EverydayCategory = "Everyday";
    internal const string FoodCategory = "Food";
    internal const string WouldYouCategory = "Would You";
    internal const string DeepEndCategory = "Deep End";

    private const string Deck = "This Or That";

    /// <summary>All cards, in intended play order.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)

            .Category(HowToPlayCategory)
            .Card("How This Works",
                "Every card offers two options.\n\n" +
                "<b>Everyone picks at the same time</b> — count down from three and say it together, or point. " +
                "Picking simultaneously matters: go round the table one at a time and people drift toward whatever the last person said.\n\n" +
                "Then read out what each option says about whoever picked it. It isn't a score and there's no right answer — " +
                "it's just the bit that starts the argument.",
                Difficulty.Easy)

            // ── EVERYDAY ─────────────────────────────────────────────────────────
            .Category(EverydayCategory)
            .ThisOrThatCard("Morning", "Which morning would you rather have?",
                new("Sunrise walk",  "tot-sunrise",  "You'd rather earn the day than be handed it. People find this either inspiring or unbearable."),
                new("Lie-in",        "tot-liein",    "You know rest isn't laziness. You've probably had to defend that at least once."))
            .ThisOrThatCard("The Commute", "Pick your journey.",
                new("Empty road",    "tot-road",     "You want the time to think. You'd take longer if it meant being alone with your thoughts."),
                new("Packed train",  "tot-train",    "You'd rather be moving with people than moving alone. You read on public transport and mean it."))
            .ThisOrThatCard("Weekend", "Two days off. Which?",
                new("Nothing booked","tot-empty",    "You protect unstructured time. Someone in your life finds this frustrating."),
                new("Full calendar", "tot-calendar", "You get more rest from doing things than from doing nothing. This confuses the other type."))
            .ThisOrThatCard("The Room", "Which space is yours?",
                new("Spotless",      "tot-tidy",     "Your outside matches your inside, or you're using one to manage the other."),
                new("Lived in",      "tot-messy",    "You'd rather the room served you than the other way round. You know where everything is."))
            .ThisOrThatCard("The Notification", "How do you take bad news by text?",
                new("Rip it off fast",   "tot-fast",   "You'd rather know now and deal with it now. Waiting is worse than the news itself, to you."),
                new("Let it sit unread", "tot-unread", "You want a moment before the moment. Other people find this maddening; you find it necessary."))

            // ── FOOD ─────────────────────────────────────────────────────────────
            .Category(FoodCategory)
            .ThisOrThatCard("The Meal", "Last meal, no consequences.",
                new("Something new", "tot-new",      "Novelty beats certainty for you, even at the end. You've been burned by this and did it again."),
                new("The old favourite","tot-fav",   "You know what's good and you're not performing for anyone. There's confidence in that."))
            .ThisOrThatCard("Sweet Or Salt", "One flavour for the rest of your life.",
                new("Sweet",         "tot-sweet",    "You go toward pleasure directly. Little patience for the long way round."),
                new("Salt",          "tot-salt",     "You like things that make you want more rather than things that satisfy. Read into that what you like."))
            .ThisOrThatCard("The Table", "Where are you eating?",
                new("Street food",   "tot-street",   "You care more about the thing itself than the setting. Hard to impress with a tablecloth."),
                new("Long dinner",   "tot-dinner",   "The meal is the excuse; the sitting there is the point. You'd stay for hours."))
            .ThisOrThatCard("The Leftovers", "One rule for the rest of your life.",
                new("Never waste a scrap", "tot-noWaste", "You treat food as a small moral obligation. Somebody in your life has been quietly grateful for this."),
                new("Always cook too much", "tot-toomuch", "Abundance is the point for you, even when it's wasteful. You'd rather over-provide than run short."))

            // ── WOULD YOU ────────────────────────────────────────────────────────
            .Category(WouldYouCategory)
            .ThisOrThatCard("The Ability", "Pick a power.",
                new("Fly",           "tot-fly",      "You want out — of rooms, of situations, of the ground. Escape appeals to you more than most."),
                new("Invisible",     "tot-invisible","You want to observe without cost. Consider what you'd actually do with it."),
                Difficulty.Medium)
            .ThisOrThatCard("Time", "One trip, one way.",
                new("Meet your past self",  "tot-past",   "There's something you'd warn yourself about. You know exactly what it is."),
                new("Meet your future self","tot-future", "You want reassurance more than you want to change anything. Or you're just nosy."),
                Difficulty.Medium)
            .ThisOrThatCard("The Truth", "One of these, permanently.",
                new("Always know when someone's lying", "tot-lie",   "You'd take painful clarity over comfortable doubt. That costs more than people expect."),
                new("Always be believed",               "tot-trust", "You want to be taken at your word. Ask yourself whether that's about trust or about winning."),
                Difficulty.Medium)
            .ThisOrThatCard("The Audience", "How does the work land?",
                new("Loved by a few",  "tot-few",    "Depth over reach. You'd rather matter enormously to a handful of people."),
                new("Liked by many",   "tot-many",   "Reach over depth. There's nothing shallow in wanting to be part of something big."),
                Difficulty.Medium)
            .ThisOrThatCard("The Memory", "One of these, permanently.",
                new("Forget one bad memory entirely",   "tot-forget",   "You'd trade the lesson for the peace. That's not weakness, it's a real trade."),
                new("Keep every memory, sharp forever",  "tot-keepall",  "You want the whole record, painful parts included. You'd rather feel it than lose it."),
                Difficulty.Medium)

            // ── DEEP END ─────────────────────────────────────────────────────────
            .Category(DeepEndCategory)
            .ThisOrThatCard("The Regret", "Which would you rather carry?",
                new("The thing you did",     "tot-did",    "You'd rather have acted and been wrong. You can live with consequences better than questions."),
                new("The thing you didn't",  "tot-didnt",  "You'd rather keep the possibility intact. That's safer and it costs you something."),
                Difficulty.Hard)
            .ThisOrThatCard("Being Known", "Pick one.",
                new("Fully known by one person", "tot-one", "You want somewhere to put all of it. That's a lot to ask of one person, and you know it."),
                new("Partly known by many",      "tot-many2","You'd rather be widely liked than deeply seen. That's a real choice, not a failure of nerve."),
                Difficulty.Hard)
            .ThisOrThatCard("The Harder Thing", "Which do you actually find harder?",
                new("Asking for help",   "tot-ask",   "You'd rather struggle than owe. Worth asking who taught you that."),
                new("Being asked",       "tot-asked", "You'd rather be needed than need. Also worth asking about."),
                Difficulty.Hard)
            .ThisOrThatCard("The Ending", "Pick how it goes.",
                new("Know exactly when it's coming",  "tot-know",    "You want to prepare, say the things, close the loop. Certainty is worth the dread to you."),
                new("Never see it coming at all",      "tot-noidea",  "You'd rather live without the countdown, even at the cost of goodbye. That's its own kind of brave."),
                Difficulty.Hard)

            .Build();
}
