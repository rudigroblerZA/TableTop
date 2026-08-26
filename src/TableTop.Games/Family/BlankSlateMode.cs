using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Family;

/// <summary>
/// Blank Slate — the fill-in-the-blank judged party game, family edition.
///
/// The classic format: a prompt with a hole in it, everyone offers an answer,
/// and one player judges which is funniest. All prompts and answers here are
/// original writing.
///
/// ADAPTED FOR ONE SCREEN. The tabletop version of this format deals each
/// player a private hand of answer cards. On a single shared device that
/// doesn't work, so every prompt card carries its own numbered shortlist of
/// candidate answers — players secretly pick a number, or invent something
/// better of their own. Same game, no printing required.
///
/// How to play:
///   1. The active player is the JUDGE. They read the prompt aloud.
///   2. Everyone else secretly picks a number from the shortlist — or makes up
///      their own answer, which is always allowed and usually funnier.
///   3. Go round, read the answers out with the prompt, filling in the blank.
///   4. The judge picks the one that made them laugh most. That player wins
///      the round; hit "🏆 Funniest" to award the point.
///   5. Pass the judging on.
///
/// Deliberately silly and safe for mixed ages — the humour is absurdity, not
/// insult. Nothing here needs a grown-up to vet it first.
/// </summary>
public sealed class BlankSlateMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Blank Slate";
    /// <inheritdoc />
    public override string Description =>
        "Fill in the blank, funniest answer wins. Pick from the shortlist or invent your own — the judge decides.";

    /// <summary>Label for awarding the round.</summary>
    public override string CompleteLabel => "🏆 Funniest";
    /// <summary>Label for skipping a prompt.</summary>
    public override string SkipLabel => "⤳ Skip";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Everyday"] = "#42A5F5",
            ["School"] = "#66BB6A",
            ["Creatures"] = "#FFA726",
            ["Absurd"] = "#AB47BC",
        };

    /// <summary>One point to whoever wins the round.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Returns the built-in Blank Slate card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        BlankSlateCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => BlankSlateCardBank.All;
}

/// <summary>Built-in card bank for Blank Slate. All prompts and answers are original.</summary>
public static class BlankSlateCardBank
{
    /// <summary>All cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── EVERYDAY ─────────────────────────────────────────────────────────
        P("Everyday", "The real reason the washing machine keeps eating socks is ______.",
          ["a very small, very hungry monster", "the socks are simply tired of us",
           "an escape tunnel behind the drum", "Dad", "a portal to a sock dimension",
           "they were never in there to begin with", "the cat's retirement fund",
           "science has no answer"], Difficulty.Easy),
        P("Everyday", "Nothing ruins a family road trip faster than ______.",
          ["the same song for four hours", "someone needing the toilet immediately",
           "a map read upside down", "the snacks running out in the first ten minutes",
           "an argument about the thermostat", "a wasp in the car",
           "'I think we passed it'", "everyone singing different words"], Difficulty.Easy),
        P("Everyday", "I would tidy my room, but ______.",
          ["the floor is lava", "I've made a system and you wouldn't understand it",
           "there's a very comfortable pile", "my socks are load-bearing",
           "I'm saving it for a rainy decade", "archaeologists will want this untouched",
           "it's tidy in a way you can't see", "I have simply chosen peace"], Difficulty.Easy),
        P("Everyday", "The worst possible thing to find at the bottom of your school bag is ______.",
          ["a banana from a previous era", "last term's permission slip",
           "something damp and unexplained", "forty-one pens, none working",
           "a sandwich that has become a civilisation", "your missing homework, now historical",
           "a small amount of sand", "someone else's entire lunch"], Difficulty.Medium),
        P("Everyday", "You can tell a grown-up is properly tired when they ______.",
          ["put the milk in the cupboard", "say 'in a minute' for two hours",
           "sigh while standing up", "call you by the pet's name",
           "watch a programme with their eyes closed", "announce they're 'just resting'",
           "lose the phone they are holding", "start a sentence and give up"], Difficulty.Medium),

        // ── SCHOOL ───────────────────────────────────────────────────────────
        P("School", "The most terrifying words a teacher can say are ______.",
          ["'Let's swap and mark each other's'", "'I've moved the seating plan'",
           "'Quick test — no notes'", "'Get into pairs'",
           "'I'll wait'", "'Read out what you wrote'",
           "'This will count towards your report'", "'Where's your homework?'"], Difficulty.Easy),
        P("School", "My homework is late because ______.",
          ["time is a construct", "I did it perfectly in a dream",
           "it was too good and I panicked", "the printer sensed my fear",
           "I have been extremely busy thinking", "a bird was involved",
           "it's still loading", "I finished it and then it left"], Difficulty.Easy),
        P("School", "The school trip was ruined by ______.",
          ["a single unsupervised goose", "the coach driver's music taste",
           "someone's packed lunch exploding", "rain, immediately, all day",
           "a headcount that never worked", "the gift shop",
           "one child who wandered off with confidence", "a very long queue for one small thing"], Difficulty.Medium),
        P("School", "You know the lesson has gone off track when ______.",
          ["everyone is now debating pizza", "the smartboard has surrendered",
           "someone asks a question that breaks the teacher", "a wasp enters the room",
           "the video won't play and never will", "you're twenty minutes into a story about the teacher's dog",
           "the class starts marking each other's handwriting", "somebody found the pencil sharpener"], Difficulty.Medium),

        // ── CREATURES ────────────────────────────────────────────────────────
        P("Creatures", "If my pet could talk, the first thing it would say is ______.",
          ["'We need to discuss the food situation'", "'I've been lying to you'",
           "'That was me. All of it was me.'", "'Who is a good boy? Be specific.'",
           "'I have been awake since four'", "'The postman and I have history'",
           "'You sit in my chair'", "'I would like to renegotiate walks'"], Difficulty.Easy),
        P("Creatures", "The animal least suited to running a restaurant is ______.",
          ["a seagull, for obvious reasons", "a sloth, on timing grounds",
           "a goldfish with no memory of the order", "an octopus doing eight jobs badly",
           "a raccoon who eats the stock", "a very loud parrot on front of house",
           "a bear who samples everything", "a snail, in the delivery role"], Difficulty.Medium),
        P("Creatures", "Scientists have discovered that dolphins are secretly ______.",
          ["extremely judgemental", "running a very successful business",
           "listening to all of it", "better at maths than us",
           "just wearing very good costumes", "in charge already",
           "keeping detailed notes", "planning something for Tuesday"], Difficulty.Medium),
        P("Creatures", "The worst superpower for an animal to have is ______.",
          ["invisibility, for a very large horse", "flight, for something already smug",
           "telepathy, for a cat", "super speed, but only backwards",
           "the ability to open doors", "understanding money",
           "immortality, for a wasp", "the power to send emails"], Difficulty.Hard),

        // ── ABSURD ───────────────────────────────────────────────────────────
        P("Absurd", "The next big Olympic sport will be ______.",
          ["competitive napping", "extreme umbrella wrestling",
           "synchronised sighing", "long-distance staring",
           "carrying too many bags in one trip", "professional queueing",
           "getting the duvet into the cover, for time", "advanced tripping over nothing"], Difficulty.Medium),
        P("Absurd", "I've invented a machine that finally ______.",
          ["finds the other sock", "explains what the noise was",
           "tells you if you already washed your hair", "removes the last bit of stubborn packaging",
           "makes toast that is actually the right shade", "answers 'what do you want for dinner'",
           "stops the plastic bag drawer", "puts things back where you found them"], Difficulty.Medium),
        P("Absurd", "The moon landing was almost cancelled because of ______.",
          ["a bee in the rocket", "someone forgetting the snacks",
           "a very long argument about the playlist", "one loose screw and a lot of denial",
           "a printer that would not connect", "a booking clash with a wedding",
           "somebody's mum saying no", "the sheer amount of paperwork"], Difficulty.Hard),
        P("Absurd", "You should never trust a person who ______.",
          ["claps when the plane lands, personally", "puts the milk in first, aggressively",
           "enjoys folding fitted sheets", "has zero photos on their phone",
           "says 'I'll be honest with you' too often", "reads the instructions all the way through",
           "actually finishes the pot of yoghurt neatly", "has never once lost a pen"], Difficulty.Hard),
        P("Absurd", "The instruction manual clearly said ______.",
          ["'Do not do the obvious thing'", "'Part C does not exist. Good luck.'",
           "'You will need a second person and a better attitude'",
           "'Congratulations on your purchase. We're sorry.'",
           "'Tighten until it feels wrong'", "'This step is impossible. Continue.'",
           "'If you hear a crack, that's normal'", "'Steps 4 to 9 have been removed'"], Difficulty.Extreme),
    ];

    private static ICard P(string category, string prompt, string[] answers, Difficulty d) =>
        StandardCard.Create(
            prompt.Length > 42 ? prompt[..39].TrimEnd() + "…" : prompt,
            "<b>🃏 " + category.ToUpperInvariant() + "</b>\n\n" +
            "<b>" + prompt + "</b>\n\n" +
            "<i>Judge reads it out. Everyone else secretly picks a number — or invents a better answer of their own.</i>\n\n" +
            string.Join("\n", answers.Select((a, i) => $"{i + 1}. {a}")),
            d, category);
}
