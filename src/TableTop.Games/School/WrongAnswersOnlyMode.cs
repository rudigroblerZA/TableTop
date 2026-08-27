using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Wrong Answers Only — the inverted quiz where being right gets you nothing.
///
/// How to play:
///   1. Read the question aloud. The REAL answer is printed at the bottom —
///      the reader shares it first, so everyone starts from the truth.
///   2. Going round the table, each player gives the most creative, confident,
///      committed WRONG answer they can invent.
///   3. The group votes for the best wrong answer: funniest, cleverest, or most
///      convincingly delivered. Winner takes the point.
///   4. Accidentally saying something true is an instant disqualification —
///      and the highest honour of the round.
///
/// Why it belongs in a classroom: inventing a GOOD wrong answer demands real
/// understanding. To claim the moon is held up by scaffolding, you must know
/// it isn't — and roughly why. Inversion is comprehension wearing a disguise,
/// and it hands the class permission to be wrong out loud, which is half the
/// battle of learning anything.
/// </summary>
public sealed class WrongAnswersOnlyMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Wrong Answers Only";
    /// <inheritdoc />
    public override string Description =>
        "The real answer is read first — then everyone competes to invent the best WRONG one. Comprehension in disguise.";

    /// <summary>Label shown on the button that records the round's winner.</summary>
    public override string CompleteLabel => "Best Wrong";
    /// <summary>Label shown on the button that skips the current card.</summary>
    public override string SkipLabel => "Skip";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [WrongAnswersOnlyCardBank.ScienceCategory] = "#26C6DA",
            [WrongAnswersOnlyCardBank.HistoryCategory] = "#FFA726",
            [WrongAnswersOnlyCardBank.GeographyCategory] = "#66BB6A",
            [WrongAnswersOnlyCardBank.NatureCategory] = "#9CCC65",
            [WrongAnswersOnlyCardBank.HowItWorksCategory] = "#AB47BC",
            [WrongAnswersOnlyCardBank.WordOriginsCategory] = "#EC407A",
        };

    /// <summary>One point to the round's voted-best wrong answer.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Returns the built-in wrong-answers card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        WrongAnswersOnlyCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => WrongAnswersOnlyCardBank.All;
}

/// <summary>Built-in card bank for Wrong Answers Only.</summary>
public static class WrongAnswersOnlyCardBank
{
    internal const string ScienceCategory = "Science";
    internal const string HistoryCategory = "History";
    internal const string GeographyCategory = "Geography";
    internal const string NatureCategory = "Nature";
    internal const string HowItWorksCategory = "How It Works";
    internal const string WordOriginsCategory = "Word Origins";

    /// <summary>All wrong-answers cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── SCIENCE ───────────────────────────────────────────────────────────
        W(ScienceCategory, "Why is the sky blue?",
          "Sunlight scatters off air molecules, and blue light scatters most.", Difficulty.Easy),
        W(ScienceCategory, "Why do we have day and night?",
          "The Earth rotates once every 24 hours, turning us toward and away from the Sun.", Difficulty.Easy),
        W(ScienceCategory, "Why does ice float on water?",
          "Frozen water is less dense — its molecules lock into a roomy crystal.", Difficulty.Medium),
        W(ScienceCategory, "Why do helium balloons rise?",
          "Helium is lighter than the surrounding air, so the air pushes the balloon up.", Difficulty.Easy),
        W(ScienceCategory, "What causes thunder?",
          "Lightning superheats the air, which expands explosively — that shockwave is the boom.", Difficulty.Medium),
        W(ScienceCategory, "Why do stars twinkle?",
          "Their light wobbles as it passes through Earth's moving atmosphere.", Difficulty.Medium),
        W(ScienceCategory, "Why is the ocean salty?",
          "Rivers wash tiny amounts of mineral salts off the land into the sea, and it accumulates.", Difficulty.Medium),
        W(ScienceCategory, "Why do we see our breath on cold days?",
          "Warm moist breath hits cold air and condenses into tiny visible droplets.", Difficulty.Easy),

        // ── HISTORY ───────────────────────────────────────────────────────────
        W(HistoryCategory, "Why did ancient Egyptians build the pyramids?",
          "As monumental tombs for their pharaohs.", Difficulty.Easy),
        W(HistoryCategory, "What was the Great Wall of China built for?",
          "To defend the northern borders against raids and invasions.", Difficulty.Easy),
        W(HistoryCategory, "Why did knights wear armour?",
          "Protection from swords, arrows, and other weapons in battle.", Difficulty.Easy),
        W(HistoryCategory, "What did the printing press change?",
          "Books could be copied by machine instead of by hand — ideas spread massively faster.", Difficulty.Medium),
        W(HistoryCategory, "Why were castles built with moats?",
          "Water made walls hard to reach, tunnel under, or roll siege towers against.", Difficulty.Medium),
        W(HistoryCategory, "What were Roman roads for?",
          "Moving armies, trade, and messages quickly across the empire.", Difficulty.Medium),
        W(HistoryCategory, "Why did sailors get scurvy on long voyages?",
          "No fresh fruit or vegetables — months without vitamin C.", Difficulty.Hard),
        W(HistoryCategory, "What was the Silk Road?",
          "A network of trade routes linking China with the Middle East and Europe.", Difficulty.Medium),

        // ── GEOGRAPHY ─────────────────────────────────────────────────────────
        W(GeographyCategory, "Why do rivers flow to the sea?",
          "Gravity — water runs downhill, and the sea is as low as it gets.", Difficulty.Easy),
        W(GeographyCategory, "Why is it hot at the equator?",
          "Sunlight hits it most directly, concentrating the energy.", Difficulty.Easy),
        W(GeographyCategory, "How were mountains like the Himalayas formed?",
          "Continental plates collided and crumpled the land upward over millions of years.", Difficulty.Medium),
        W(GeographyCategory, "Why does it rain more on one side of a mountain range?",
          "Rising air cools and drops its moisture on the windward side, leaving the far side dry.", Difficulty.Hard),
        W(GeographyCategory, "Why are deserts cold at night?",
          "No clouds and dry air — the day's heat radiates straight back into space.", Difficulty.Medium),
        W(GeographyCategory, "What causes ocean tides?",
          "Mostly the Moon's gravity pulling on the oceans as Earth rotates.", Difficulty.Medium),
        W(GeographyCategory, "Why do volcanoes erupt?",
          "Molten rock under pressure finds a weak point in the crust and forces its way out.", Difficulty.Medium),
        W(GeographyCategory, "Why is Antarctica a desert?",
          "Deserts are defined by low precipitation — and it barely ever snows THERE, it's just too cold to melt.", Difficulty.Hard),

        // ── NATURE ────────────────────────────────────────────────────────────
        W(NatureCategory, "Why do birds fly south for the winter?",
          "Following food and warmer weather; their routes are driven by survival, not sightseeing.", Difficulty.Easy),
        W(NatureCategory, "Why do leaves change colour in autumn?",
          "Trees withdraw green chlorophyll, revealing the yellow and orange pigments underneath.", Difficulty.Medium),
        W(NatureCategory, "Why do cats purr?",
          "A vibration made in the voice box — usually contentment, sometimes self-soothing.", Difficulty.Easy),
        W(NatureCategory, "Why do bees dance?",
          "The waggle dance tells hive-mates the direction and distance of food.", Difficulty.Medium),
        W(NatureCategory, "Why do camels have humps?",
          "Fat storage — energy reserves for long stretches without food (not water tanks).", Difficulty.Medium),
        W(NatureCategory, "Why do onions make you cry?",
          "Cutting releases a gas that turns mildly acidic in your eyes; tears flush it out.", Difficulty.Medium),
        W(NatureCategory, "Why do dogs tilt their heads when you talk?",
          "Likely adjusting their ears and sight to locate and read you better.", Difficulty.Easy),
        W(NatureCategory, "Why do flamingos stand on one leg?",
          "It's their most stable, least tiring posture — one-legged standing takes almost no muscle effort.", Difficulty.Hard),

        // ── HOW IT WORKS ──────────────────────────────────────────────────────
        W(HowItWorksCategory, "How does a fridge keep food cold?",
          "It pumps heat OUT of the box using a circulating refrigerant — cold is just heat removed.", Difficulty.Hard),
        W(HowItWorksCategory, "How does a plane stay in the air?",
          "Wings deflect air downward and create pressure differences — the air pushes the plane up.", Difficulty.Medium),
        W(HowItWorksCategory, "How does a microphone work?",
          "Sound vibrates a tiny membrane, and that motion is converted into an electrical signal.", Difficulty.Medium),
        W(HowItWorksCategory, "How does soap clean your hands?",
          "Soap molecules grab grease on one end and water on the other, so grime rinses away.", Difficulty.Medium),
        W(HowItWorksCategory, "How does a compass know where north is?",
          "Its magnetised needle aligns itself with the Earth's magnetic field.", Difficulty.Easy),
        W(HowItWorksCategory, "How do noise-cancelling headphones work?",
          "They play an inverted copy of incoming sound, and the two waves cancel out.", Difficulty.Hard),
        W(HowItWorksCategory, "How does popcorn pop?",
          "Moisture inside the kernel turns to steam until the shell bursts and the starch puffs.", Difficulty.Easy),
        W(HowItWorksCategory, "How does a battery store energy?",
          "As chemical energy — reactions inside push electrons through the circuit when connected.", Difficulty.Hard),

        // ── WORD ORIGINS ──────────────────────────────────────────────────────
        W(WordOriginsCategory, "Where does the word 'sandwich' come from?",
          "The Earl of Sandwich, who wanted meals he could eat without leaving the card table.", Difficulty.Medium),
        W(WordOriginsCategory, "Why is a 'piggy bank' pig-shaped?",
          "Old jars were made of 'pygg' clay — the name stuck and became the animal.", Difficulty.Hard),
        W(WordOriginsCategory, "Where does 'quarantine' come from?",
          "Italian 'quaranta' — forty — the days ships once waited in port during plagues.", Difficulty.Hard),
        W(WordOriginsCategory, "Why do we say 'break a leg' for good luck?",
          "Theatre superstition — wishing luck directly was thought to jinx the performance.", Difficulty.Medium),
        W(WordOriginsCategory, "Where does the word 'robot' come from?",
          "A 1920 Czech play — 'robota' means forced labour.", Difficulty.Hard),
        W(WordOriginsCategory, "Why is it called a 'grandfather' clock?",
          "A popular 1876 song about a grandfather's clock — the name outlived the tune.", Difficulty.Extreme),
        W(WordOriginsCategory, "Where does 'ketchup' come from?",
          "Probably from 'kê-tsiap', a Chinese fermented fish sauce — tomatoes came much later.", Difficulty.Extreme),
        W(WordOriginsCategory, "Why is a marathon 42.195 km?",
          "Legend of a Greek messenger's run, plus a 1908 London tweak so it finished at the royal box.", Difficulty.Extreme),

        // ── EXPANSION: PREMIUM BAIT ───────────────────────────────────────────
        W(ScienceCategory, "Why do we dream?",
          "Likely memory consolidation and emotional processing while the brain does its nightly filing.", Difficulty.Hard),
        W(ScienceCategory, "Why does your voice sound different in recordings?",
          "Live, you also hear it through your skull bones, which adds bass only you receive.", Difficulty.Medium),
        W(ScienceCategory, "Why do mosquito bites itch?",
          "Your immune system reacts to the mosquito's saliva with histamine — the itch is friendly fire.", Difficulty.Medium),
        W(ScienceCategory, "Why can't you tickle yourself?",
          "Your brain predicts your own movements and cancels the surprise — tickling requires surprise.", Difficulty.Hard),
        W(NatureCategory, "Why do wombats have cube-shaped poo?",
          "Their intestines have zones of different elasticity that mould corners. Genuinely. Cubes.", Difficulty.Extreme),
        W(NatureCategory, "Why do goats scream like humans?",
          "Individual variation in bleats — some goats just have unfortunate voices.", Difficulty.Medium),
        W(NatureCategory, "Why don't penguins' feet freeze?",
          "Counter-current blood flow: warm blood pre-heats the cold blood coming back from the feet.", Difficulty.Hard),
        W(HowItWorksCategory, "How does your phone know which way is up?",
          "A tiny accelerometer chip senses gravity's pull direction.", Difficulty.Medium),
        W(HowItWorksCategory, "How do noise-activated sleep apps know you're snoring?",
          "The microphone listens for the frequency pattern of snores — your phone is judging you all night.", Difficulty.Medium),
        W(HowItWorksCategory, "How does bubble wrap get its bubbles?",
          "Two plastic sheets fuse while one is sucked into bubble moulds by vacuum. Popping is destiny.", Difficulty.Easy),
        W(HistoryCategory, "Why do we clink glasses before drinking?",
          "Old ritual of trust and shared celebration — the poison-proof legend is mostly myth.", Difficulty.Medium),
        W(HistoryCategory, "Why do we say 'bless you' after a sneeze?",
          "Ancient habit — one legend ties it to plague-era prayers, another to souls escaping. Habit outlived reasons.", Difficulty.Medium),
        W(HistoryCategory, "Why did pirates wear eye patches?",
          "Likely to keep one eye dark-adapted for going below deck — practical, not just fashion.", Difficulty.Hard),
        W(WordOriginsCategory, "Why is it called a 'nightmare'?",
          "The 'mare' was a demon believed to sit on sleepers' chests. Horses are innocent.", Difficulty.Hard),
        W(WordOriginsCategory, "Where does 'deadline' come from?",
          "A literal line in Civil War prison camps — cross it and be shot. Office life kept the vibe.", Difficulty.Extreme),
        W(WordOriginsCategory, "Why do we 'take the mickey' or 'pull someone's leg'?",
          "Origins are murky slang; leg-pulling may come from tripping victims — mockery has always had technique.", Difficulty.Extreme),
    ];

    private static ICard W(string category, string question, string realAnswer, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>Question:</b> " + question + "\n\n" +
            "<b>Reader: announce the REAL answer first —</b>\n" + realAnswer + "\n\n" +
            "<b>Now, everyone in turn: give your best WRONG answer.</b> " +
            "Confident. Creative. Committed. Group votes for the winner.\n\n" +
            "<i>Say something accidentally true and you're out of the round — with full honours.</i>",
            d, category);
}
