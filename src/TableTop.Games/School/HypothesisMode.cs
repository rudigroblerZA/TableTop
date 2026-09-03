using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Hypothesis! — say what will happen <i>before</i> you find out, then say why.
///
/// How to play:
///   1. The reader reads the setup and stops. Nobody flips yet.
///   2. Everyone commits to a prediction out loud, all at once on "three".
///      Committing is the whole game — a quiet "I knew that" after the reveal
///      is worth nothing here.
///   3. Flip. A right prediction scores. Explaining the mechanism correctly
///      scores again, and a wrong prediction with the right reasoning still
///      earns the second point (table's verdict).
///
/// <para>
/// <b>Why this is not Science Sprint.</b> The other classroom science mode asks
/// what you remember; this one asks what you'd bet on. Several cards here are
/// deliberately ones where the popular answer is wrong — the sealed candle, the
/// melting ice cube, the sky — so recall alone actively costs you. The reveal
/// always names the mechanism rather than just the outcome, because the
/// mechanism is the part that transfers to the next card.
/// </para>
/// </summary>
public sealed class HypothesisMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Hypothesis!";

    /// <inheritdoc />
    public override string Description =>
        "Predict what happens before the flip, then explain why. Getting it right scores — knowing the mechanism scores again.";

    /// <summary>Label for the button that records a correct prediction.</summary>
    public override string CompleteLabel => "Called It";

    /// <summary>Label for the button that passes on a card.</summary>
    public override string SkipLabel => "Got It Wrong";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [HypothesisCardBank.ForcesCategory] = "#EF5350",
            [HypothesisCardBank.LivingWorldCategory] = "#66BB6A",
            [HypothesisCardBank.MatterCategory] = "#26C6DA",
            [HypothesisCardBank.EarthAndSpaceCategory] = "#5C6BC0",
            [HypothesisCardBank.EverydayCategory] = "#FFA726",
        };

    /// <summary>
    /// Difficulty-based, not flat: the counter-intuitive cards are the ones
    /// worth backing yourself on, and a flat point would pay the same for
    /// "the ice cube melts" as for guessing where a tree's mass comes from.
    /// </summary>
    protected override IScoringStrategy BuildScoring() => new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in Hypothesis! card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        HypothesisCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => HypothesisCardBank.All;
}

/// <summary>
/// Built-in card bank for Hypothesis!. Authored with <see cref="CardDeckBuilder"/>
/// so ids derive from card content and stay stable across restarts.
/// </summary>
public static class HypothesisCardBank
{
    internal const string ForcesCategory = "Forces & Motion";
    internal const string LivingWorldCategory = "Living World";
    internal const string MatterCategory = "Matter";
    internal const string EarthAndSpaceCategory = "Earth & Space";
    internal const string EverydayCategory = "Everyday";

    /// <summary>All Hypothesis! cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    /// <summary>
    /// Formats a card. The prediction instruction is repeated on every card on
    /// purpose: the one rule that makes this mode work is "commit before you
    /// flip", and a rule stated once at the start is a rule nobody follows by
    /// card nine.
    /// </summary>
    private static string Predict(string setup, string reveal) =>
        "<b>🧪 Predict first. Everyone commits out loud on three — then flip.</b>\n\n" +
        setup + "\n\n" +
        "<i>Right prediction = a point. The correct mechanism = a point too, even if you " +
        "predicted wrong. Table decides.</i>\n\n" +
        "Answer: " + reveal;

    private static IReadOnlyList<ICard> Build() => CardDeckBuilder
        .For("Hypothesis!")

        // ── FORCES & MOTION ──────────────────────────────────────────────────
        .Category(ForcesCategory)
            .Card("The Hammer and the Feather",
                Predict(
                    "An astronaut stands on the Moon holding a geological hammer in one hand and a falcon feather in the other, at the same height. She lets go of both at the same instant. Which hits the ground first?",
                    "They land together. Gravity accelerates everything at the same rate; on Earth it's air resistance that slows the feather, and the Moon has no air. David Scott did exactly this on Apollo 15 in 1971, on camera, to prove the point."),
                Difficulty.Easy)
            .Card("Two Balls, One Drop",
                Predict(
                    "Back on Earth. You hold a bowling ball and a tennis ball at shoulder height and release them together. Same time, or does the heavy one win?",
                    "Essentially the same time. Heavier means more gravitational force but also more inertia to move, and the two cancel exactly. Over two metres air resistance barely gets a say — drop the tennis ball from a tower and it would start to lag."),
                Difficulty.Medium)
            .Card("The Spinning Skater",
                Predict(
                    "A skater is spinning slowly with both arms stretched out wide. She pulls her arms in tight to her chest. What happens to how fast she spins — and to her energy?",
                    "She speeds up sharply. Angular momentum is conserved, so pulling mass closer to the axis must raise the rate. The twist: her rotational energy goes UP, not sideways — she paid for it with the muscle work of dragging her arms inwards."),
                Difficulty.Medium)
            .Card("The Tablecloth",
                Predict(
                    "A full dinner service sits on a tablecloth. Someone yanks the cloth horizontally, very fast. Do the plates come with it?",
                    "Barely — done fast enough they stay put. Friction from the cloth acts on the plates for only a few hundredths of a second, which is not long enough to accelerate them noticeably. Pull slowly and the same friction has all the time it needs, and everything goes on the floor."),
                Difficulty.Medium)
            .Card("The Bus Stops",
                Predict(
                    "You're standing on a bus with no handrail. The driver brakes hard. You lurch forward. What pushed you?",
                    "Nothing did. You were already moving with the bus and simply kept moving; the bus slowed underneath you. Every 'force' you feel in a braking or turning vehicle is your own inertia meeting a floor that changed its mind."),
                Difficulty.Easy)
            .Card("Hoop Versus Disc",
                Predict(
                    "A metal hoop and a solid disc — same mass, same diameter — are released together at the top of a ramp and roll down without slipping. Which reaches the bottom first?",
                    "The solid disc, every time. Both convert the same height into the same total energy, but the hoop carries all its mass at the rim, so a far bigger share of that energy has to go into spinning rather than travelling. Mass and size are red herrings: only where the mass sits matters."),
                Difficulty.Hard)

        // ── LIVING WORLD ─────────────────────────────────────────────────────
        .Category(LivingWorldCategory)
            .Card("The Plant in the Box",
                Predict(
                    "A seedling is sealed in a cardboard box with one small hole cut in the side. It's watered through a tube and left for two weeks. What does it look like when the box comes off?",
                    "Long, pale and bent hard towards the hole. Growth hormone (auxin) gathers on the shaded side and makes those cells stretch longer, which tips the stem towards the light — and starved of light it goes leggy and yellow, spending everything on reaching rather than on leaves."),
                Difficulty.Easy)
            .Card("Where a Tree Comes From",
                Predict(
                    "An oak weighs two tonnes. The soil in the hole it grew from weighs almost exactly what it did when the acorn went in. So where did the two tonnes come from?",
                    "Mostly out of the air. The bulk of a tree's dry mass is carbon pulled from carbon dioxide during photosynthesis, plus water. Soil supplies minerals in tiny quantities. Van Helmont weighed the pot to check this in the 1600s and still guessed wrong — he credited the water."),
                Difficulty.Hard)
            .Card("Yeast, Sugar, Balloon",
                Predict(
                    "Warm water, a spoon of sugar and a sachet of yeast go into a bottle. A balloon is stretched over the neck. Thirty minutes later?",
                    "The balloon stands up, inflated. The yeast is respiring the sugar and giving off carbon dioxide — the same process that raises bread and puts the fizz in beer. Use cold water instead and almost nothing happens; the yeast stays dormant."),
                Difficulty.Easy)
            .Card("Celery and Blue Water",
                Predict(
                    "A stick of celery with leaves is stood in water dyed bright blue and left overnight. What's blue in the morning, and what isn't?",
                    "The stringy tubes running up the stalk go blue, and so do the veins in the leaves — but the flesh between them stays pale. Those tubes are xylem, and water climbs them as leaves lose water to the air and pull the column up behind it."),
                Difficulty.Easy)
            .Card("The Sideways Seedling",
                Predict(
                    "A sprouted bean is planted on its side, root pointing left, shoot pointing right. It's kept in total darkness. A week later — which way is each one pointing?",
                    "The root has curved downwards and the shoot upwards, with no light involved at all. They're sensing gravity, not the sun: heavy starch grains settle to the bottom of certain cells and the plant grows accordingly. Put the pot on a slow turntable and the confused seedling grows outwards."),
                Difficulty.Medium)
            .Card("Why You Shiver",
                Predict(
                    "You step out into the cold and start to shiver. Is your body doing something useful, or just reacting?",
                    "It's a heater. Shivering is your muscles contracting and relaxing rapidly on purpose, and because muscle is inefficient, most of that work comes out as heat — which is exactly the point. It can raise heat production several times over resting level, at the cost of a lot of energy."),
                Difficulty.Easy)

        // ── MATTER ───────────────────────────────────────────────────────────
        .Category(MatterCategory)
            .Card("The Ice Cube at the Brim",
                Predict(
                    "A glass is filled with water to the absolute brim, and a big ice cube floats in it, sticking well above the rim. The ice melts completely. Does it overflow?",
                    "No — the level doesn't change. Floating ice already displaces exactly its own mass of water, and when it melts it becomes exactly that much water. This is why melting sea ice doesn't raise sea level, while ice sitting on land absolutely does."),
                Difficulty.Hard)
            .Card("Salt on Ice",
                Predict(
                    "You drop a thermometer into a bowl of ice at 0 °C, then stir in a large handful of salt. Does the temperature go up, down, or stay put?",
                    "It falls — well below zero, to −10 °C or lower. Salt lowers the freezing point, so the ice starts melting, and melting absorbs heat from whatever's nearby, including the remaining mixture. Old ice-cream makers ran on exactly this."),
                Difficulty.Medium)
            .Card("Hot Water, Cold Water",
                Predict(
                    "Two identical trays go into the same freezer at the same moment — one filled with hot water, one with cold. Which freezes first?",
                    "Usually the cold one. But under the right conditions hot water genuinely wins, which is the Mpemba effect, named after a schoolboy whose teacher told him he was mistaken. Evaporation, dissolved gas, convection and frost under the tray have all been proposed; there is still no agreed explanation, and some careful experiments fail to reproduce it at all. 'It depends, and nobody's sure why' is the honest answer here."),
                Difficulty.Extreme)
            .Card("Bicarb in a Bag",
                Predict(
                    "Two spoons of bicarbonate of soda and a splash of vinegar go into a freezer bag, and the bag is sealed fast. Then what?",
                    "It swells and, given enough of both, bursts. The acid and the carbonate react to make carbon dioxide gas, which needs vastly more room than the liquids it came from. Feel the bag while it happens — it goes cold, because this reaction takes heat in rather than giving it out."),
                Difficulty.Easy)
            .Card("The Candle in the Sealed Jar",
                Predict(
                    "A burning candle is sealed inside a glass jar standing on a very accurate balance. It burns for a while and goes out. Does the reading change?",
                    "Not at all. Nothing left the jar — the wax combined with oxygen from the trapped air to make carbon dioxide and water vapour, and every atom is still in there. Mass isn't lost by burning, only rearranged. Do the same with the lid off and it appears to lose mass, because the products walk out."),
                Difficulty.Hard)
            .Card("Oil, Water, Washing-Up Liquid",
                Predict(
                    "Oil floats in a jar of water in a clear layer. You shake it — it mixes, then separates again. Now add a drop of washing-up liquid and shake. What changes?",
                    "It stays mixed, cloudy, for far longer. Detergent molecules have a water-loving end and an oil-loving end, so they park at the boundary and coat the oil droplets, stopping them merging back together. That is also the entire mechanism by which soap cleans a greasy plate."),
                Difficulty.Medium)

        // ── EARTH & SPACE ────────────────────────────────────────────────────
        .Category(EarthAndSpaceCategory)
            .Card("Why the Sky Is Blue",
                Predict(
                    "Sunlight is white — a mix of every colour. So why does the sky read as blue, and why does the same sky go orange at sunset?",
                    "Air scatters short wavelengths far more strongly than long ones, so blue light gets bounced around the sky and reaches you from every direction. At sunset the light travels through much more atmosphere, the blue is scattered away entirely before it arrives, and what's left is the red and orange end."),
                Difficulty.Medium)
            .Card("Summer Is Not Closer",
                Predict(
                    "It's July, it's hot in London, and the Earth's orbit isn't a perfect circle. Is the Earth nearer the Sun than usual right now?",
                    "It's actually at its farthest, in early July. Seasons have nothing to do with distance — the Earth is tilted about 23.5°, and in summer your hemisphere leans towards the Sun, so the light strikes more steeply and the days run longer. That's also why the southern hemisphere has Christmas on the beach."),
                Difficulty.Hard)
            .Card("The Dark Side of the Moon",
                Predict(
                    "We only ever see one face of the Moon from Earth. Is the other face permanently dark?",
                    "No — it gets just as much sunlight as the near side, roughly two weeks on and two weeks off. It's the FAR side, not the dark side. We see one face because the Moon turns exactly once per orbit, locked there by Earth's gravity over billions of years."),
                Difficulty.Medium)
            .Card("On the Moon You Weigh Less",
                Predict(
                    "An astronaut and her toolkit travel to the Moon. What changes: her mass, her weight, both, or neither?",
                    "Weight only — to about a sixth. Mass is how much stuff there is and it travels unchanged; weight is the pull of gravity on that stuff. She'd still need the same shove to get a heavy toolkit moving sideways, which is why astronauts move so carefully rather than flinging things about."),
                Difficulty.Easy)
            .Card("Which Way Does the Drain Spin?",
                Predict(
                    "Does the water in a bathroom sink really spin one way in the northern hemisphere and the other way in the southern?",
                    "No. The Coriolis effect is real and it does steer hurricanes, but at the scale of a sink it is thousands of times weaker than the shape of the basin, the angle of the tap and whatever swirl the water already had. Fill a sink perfectly still in a controlled lab and you can just about measure it — nowhere else."),
                Difficulty.Hard)
            .Card("Stars Twinkle, Planets Don't",
                Predict(
                    "On a clear night stars flicker but the bright 'stars' that turn out to be planets shine steadily. Why the difference?",
                    "Distance. A star is so far away it arrives as a single point of light, so one pocket of wobbling air can shift or dim the whole thing. A planet is close enough to be a tiny disc — many points at once — and their flickers average out. Astronomers put telescopes on mountains and in orbit to escape exactly this."),
                Difficulty.Hard)

        // ── EVERYDAY ─────────────────────────────────────────────────────────
        .Category(EverydayCategory)
            .Card("The Cold Metal Table Leg",
                Predict(
                    "A table has a metal leg and a wooden leg, in the same room all night. The metal feels much colder. Which one IS colder?",
                    "Neither — they're the same temperature, and a thermometer says so. Metal conducts heat away from your hand far faster than wood, so what you're feeling is the rate you're losing heat, not the temperature you're touching. Your skin is a heat-flow meter that thinks it's a thermometer."),
                Difficulty.Medium)
            .Card("The Fogged Mirror",
                Predict(
                    "You run a hot shower and the bathroom mirror fogs over. Where did the water on the glass come from, and why the mirror in particular?",
                    "From the air. Warm air holds a lot of water vapour, and the mirror is one of the coldest surfaces in the room, so air touching it cools past the point where it can hold that vapour and dumps it as droplets. Warm the glass first — with a hairdryer — and it stays clear."),
                Difficulty.Easy)
            .Card("The Kernel That Wouldn't Pop",
                Predict(
                    "Popcorn pops. A kernel with a cracked hull, in the same pan, does not. What's the mechanism, and why does the crack ruin it?",
                    "Each kernel holds a little water. Heat turns it to steam, and the tough hull traps that steam until the pressure blows the kernel inside out. A cracked hull lets the steam leak away gently, so pressure never builds — the same reason very old, dried-out kernels fail."),
                Difficulty.Easy)
            .Card("The Onion That Fights Back",
                Predict(
                    "Cutting an onion stings your eyes. Cutting it under running water, or after chilling it, stings much less. Why?",
                    "Damaging the cells releases sulphur compounds that react into a volatile gas, and that gas reacts with the water on your eye to make a mild acid — your eyes water to flush it out. Cold slows the reaction right down and water washes the gas away before it reaches you."),
                Difficulty.Medium)
            .Card("The Bent Straw",
                Predict(
                    "A straight straw in a glass of water looks broken at the surface. Is anything actually bending?",
                    "The light is. Light slows down entering water and changes direction at the boundary, so rays from the submerged part reach your eye at a different angle than you'd expect — and your brain, which assumes light travels in straight lines, places that part somewhere it isn't. Lenses, rainbows and spectacles are all the same effect put to work."),
                Difficulty.Easy)
            .Card("Shouting Underwater",
                Predict(
                    "Sound needs a material to travel through. Does it travel faster through air or through water — and what about steel?",
                    "Water beats air by about four times, and steel is faster still, roughly fifteen times air. Sound is a squeeze passed from particle to particle, so the closer and stiffer they are, the faster it moves. It also explains the classic: in the vacuum of space, no particles, no sound at all."),
                Difficulty.Medium)

        .Build();
}
