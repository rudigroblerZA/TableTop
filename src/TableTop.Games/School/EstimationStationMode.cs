using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Estimation Station — the closest-guess numbers game.
///
/// How to play:
///   1. Read the question aloud. Nobody may look anything up.
///   2. Everyone secretly writes down a number.
///   3. Reveal together. The card shows the real answer — closest guess wins the point.
///   4. Ties share the point. Wildly wrong answers earn affectionate mockery.
///
/// Why it works: estimation ("Fermi problems") is real mathematical thinking —
/// breaking a big unknown into small known chunks. How many litres fill a bathtub?
/// Well, a bucket is about 10 litres, and a bath looks like maybe 15 buckets…
/// Kids learn that a sensible method beats a lucky guess, and adults discover
/// they have no idea how heavy a cloud is.
///
/// No knowledge required — only reasoning. That levels the field between ages,
/// which makes it a genuine all-ages classroom or family game.
/// </summary>
public sealed class EstimationStationMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Estimation Station";
    /// <inheritdoc />
    public override string Description =>
        "Everyone secretly guesses the number — closest wins. Reasoning beats knowledge.";

    /// <summary>Label shown on the button that records a completed round.</summary>
    public override string CompleteLabel => "Closest";
    /// <summary>Label shown on the button that skips the current card.</summary>
    public override string SkipLabel => "Skip";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [EstimationStationCardBank.MeasurementCategory] = "#42A5F5",
            [EstimationStationCardBank.NatureCategory] = "#66BB6A",
            [EstimationStationCardBank.EverydayCategory] = "#FFA726",
            [EstimationStationCardBank.SpaceCategory] = "#AB47BC",
            [EstimationStationCardBank.BodyCategory] = "#EC407A",
            [EstimationStationCardBank.SpeedTimeCategory] = "#26C6DA",
        };

    /// <summary>One point to whoever guessed closest each round.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Returns the built-in estimation card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        EstimationStationCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => EstimationStationCardBank.All;
}

/// <summary>Built-in card bank for Estimation Station.</summary>
public static class EstimationStationCardBank
{
    internal const string MeasurementCategory = "Measurement";
    internal const string NatureCategory = "Nature";
    internal const string EverydayCategory = "Everyday";
    internal const string SpaceCategory = "Space";
    internal const string BodyCategory = "Body";
    internal const string SpeedTimeCategory = "Speed & Time";

    private const string Deck = "Estimation Station";

    /// <summary>All estimation cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)

            // ── MEASUREMENT ───────────────────────────────────────────────────────
            .Category(MeasurementCategory)
            .Card(MeasurementCategory, Body("How many litres of water fill a standard bathtub?",
                "About 150 litres (a bucket is ~10 L — picture 15 buckets)."), Difficulty.Easy)
            .Card(MeasurementCategory, Body("How tall is an adult giraffe, in metres?",
                "About 5 metres — roughly a two-storey house."), Difficulty.Easy)
            .Card(MeasurementCategory, Body("How many centimetres long is a standard school ruler?",
                "30 cm. (If someone gets this wrong, discuss.)"), Difficulty.Easy)
            .Card(MeasurementCategory, Body("How much does an adult elephant weigh, in kilograms?",
                "About 5,000 kg (5 tonnes) for an African elephant."), Difficulty.Medium)
            .Card(MeasurementCategory, Body("How long is a football (soccer) pitch, in metres?",
                "About 105 metres, goal line to goal line."), Difficulty.Medium)
            .Card(MeasurementCategory, Body("How many millilitres are in a typical can of fizzy drink?",
                "330 ml in most of the world; 355 ml in North America."), Difficulty.Easy)
            .Card(MeasurementCategory, Body("How heavy is a typical cloud (a fair-weather cumulus), in kilograms?",
                "Around 500,000 kg — the weight of about 100 elephants, floating."), Difficulty.Hard)
            .Card(MeasurementCategory, Body("How deep is the deepest point of the ocean, in metres?",
                "About 11,000 m (the Mariana Trench). Everest would sink without a trace."), Difficulty.Medium)

            // ── NATURE ────────────────────────────────────────────────────────────
            .Category(NatureCategory)
            .Card(NatureCategory, Body("How many legs does a typical millipede actually have?",
                "Most have 100–400. The record holder has 1,306 — but 'milli' (1,000) is marketing."), Difficulty.Medium)
            .Card(NatureCategory, Body("How many years can a giant tortoise live?",
                "Over 150 years. Some alive today hatched in the 1800s."), Difficulty.Easy)
            .Card(NatureCategory, Body("How many bees live in a typical honeybee hive in summer?",
                "About 50,000."), Difficulty.Medium)
            .Card(NatureCategory, Body("How fast can a cheetah run at top speed, in km/h?",
                "About 110 km/h — motorway speed, on paws."), Difficulty.Easy)
            .Card(NatureCategory, Body("How many hearts does an octopus have?",
                "Three. Two pump blood to the gills, one to the body."), Difficulty.Easy)
            .Card(NatureCategory, Body("How tall is the tallest tree on Earth, in metres?",
                "About 116 m (a coast redwood named Hyperion) — taller than a 30-storey building."), Difficulty.Medium)
            .Card(NatureCategory, Body("How many ants are estimated to live on Earth, in trillions?",
                "About 20,000 trillion (20 quadrillion). Roughly 2.5 million ants per human."), Difficulty.Hard)
            .Card(NatureCategory, Body("How long can a snail sleep, in years?",
                "Up to 3 years in one stretch, waiting for wet weather."), Difficulty.Hard)

            // ── EVERYDAY ──────────────────────────────────────────────────────────
            .Category(EverydayCategory)
            .Card(EverydayCategory, Body("How many times does a person blink in one day?",
                "About 15,000–20,000 times."), Difficulty.Medium)
            .Card(EverydayCategory, Body("How many sheets of paper are in a standard ream?",
                "500 sheets."), Difficulty.Easy)
            .Card(EverydayCategory, Body("How many steps does an average person take in a day?",
                "About 4,000–5,000 for most people (the famous 10,000 was a marketing slogan)."), Difficulty.Easy)
            .Card(EverydayCategory, Body("How many words does an average person speak per day?",
                "Roughly 16,000 — men and women almost identical, despite the myth."), Difficulty.Medium)
            .Card(EverydayCategory, Body("How many hours will the average person spend asleep by age 75?",
                "About 220,000 hours — roughly 25 YEARS of sleeping."), Difficulty.Hard)
            .Card(EverydayCategory, Body("How many grapes does it take to make one bottle of wine?",
                "About 600–800 grapes."), Difficulty.Hard)
            .Card(EverydayCategory, Body("How many keys are on a full-size piano?",
                "88 — 52 white, 36 black."), Difficulty.Easy)
            .Card(EverydayCategory, Body("How many litres of milk does a dairy cow produce per day?",
                "About 25–30 litres."), Difficulty.Medium)

            // ── SPACE ─────────────────────────────────────────────────────────────
            .Category(SpaceCategory)
            .Card(SpaceCategory, Body("How long does light from the Sun take to reach Earth, in minutes?",
                "About 8 minutes 20 seconds."), Difficulty.Easy)
            .Card(SpaceCategory, Body("How many Earths would fit inside the Sun?",
                "About 1.3 million."), Difficulty.Medium)
            .Card(SpaceCategory, Body("How far away is the Moon, in kilometres?",
                "About 384,000 km — you could line up all the other planets in the gap."), Difficulty.Medium)
            .Card(SpaceCategory, Body("How many days does Mercury take to orbit the Sun?",
                "88 days. A Mercury 'year' is shorter than a school term."), Difficulty.Hard)
            .Card(SpaceCategory, Body("How fast is the International Space Station moving, in km/h?",
                "About 28,000 km/h — it laps Earth every 90 minutes."), Difficulty.Hard)
            .Card(SpaceCategory, Body("What temperature is the surface of the Sun, in degrees Celsius?",
                "About 5,500 °C. (The core is 15 million.)"), Difficulty.Medium)

            // ── BODY ──────────────────────────────────────────────────────────────
            .Category(BodyCategory)
            .Card(BodyCategory, Body("How many bones does an adult human have?",
                "206. Babies start with about 300 — many fuse as they grow."), Difficulty.Easy)
            .Card(BodyCategory, Body("How many times does your heart beat in one day?",
                "About 100,000 times."), Difficulty.Medium)
            .Card(BodyCategory, Body("How long are all the blood vessels in your body if laid end to end, in kilometres?",
                "Roughly 100,000 km — two and a half times around the Earth."), Difficulty.Hard)
            .Card(BodyCategory, Body("How many litres of saliva does a person produce in a year?",
                "About 400–500 litres. You're welcome."), Difficulty.Hard)
            .Card(BodyCategory, Body("How many muscles does it take to smile?",
                "About 12 — and around 11 to frown, so the old saying is backwards."), Difficulty.Medium)
            .Card(BodyCategory, Body("How fast does a sneeze travel, in km/h?",
                "About 60–70 km/h (the '160 km/h' figure is a myth, but it's still fast)."), Difficulty.Medium)

            // ── SPEED & TIME ──────────────────────────────────────────────────────
            .Category(SpeedTimeCategory)
            .Card(SpeedTimeCategory, Body("How many seconds are in one day?",
                "86,400."), Difficulty.Easy)
            .Card(SpeedTimeCategory, Body("How long would it take to walk around the Earth's equator, walking 8 hours a day?",
                "About 3 years (40,000 km at ~5 km/h, 8 h/day ≈ 1,000 days) — ignoring the oceans."), Difficulty.Hard)
            .Card(SpeedTimeCategory, Body("How fast does sound travel through air, in metres per second?",
                "About 343 m/s. Count seconds between lightning and thunder, divide by 3, get kilometres."), Difficulty.Medium)
            .Card(SpeedTimeCategory, Body("How many minutes are in a week?",
                "10,080."), Difficulty.Medium)
            .Card(SpeedTimeCategory, Body("How old is the Earth, in billions of years?",
                "About 4.5 billion years."), Difficulty.Easy)
            .Card(SpeedTimeCategory, Body("If you counted one number per second, nonstop, how many DAYS to reach a million?",
                "About 11.5 days. (A billion would take 32 years.)"), Difficulty.Hard)

            // ── EXPANSION: WEIRD BUT TRUE ─────────────────────────────────────────
            .Category(BodyCategory)
            .Card(BodyCategory, Body("How many skin cells do you shed per day?",
                "Around 500 million. A lot of household dust used to be you."), Difficulty.Hard)
            .Card(BodyCategory, Body("How many dreams does a person have per night?",
                "About 4–6. You forget nearly all of them within minutes."), Difficulty.Medium)
            .Card(BodyCategory, Body("How many times will you laugh today, on average?",
                "Adults: about 15–20 times. Children: closer to 300. Discuss."), Difficulty.Medium)
            .Category(NatureCategory)
            .Card(NatureCategory, Body("How many times does a hummingbird's heart beat per minute?",
                "Up to 1,200 while flying."), Difficulty.Hard)
            .Card(NatureCategory, Body("How loud is a blue whale's call, in decibels?",
                "About 188 dB — louder than a jet engine; audible across hundreds of kilometres of ocean."), Difficulty.Hard)
            .Card(NatureCategory, Body("How many eggs does a queen bee lay per day in summer?",
                "Up to 2,000 — more than her own body weight."), Difficulty.Medium)
            .Category(EverydayCategory)
            .Card(EverydayCategory, Body("How many times does the average person check their phone per day?",
                "Around 100–150 times. Yes, including during this game."), Difficulty.Easy)
            .Card(EverydayCategory, Body("How many years of their life does the average person spend queueing?",
                "Roughly 6 months to a year, depending on country. Feels longer."), Difficulty.Hard)
            .Card(EverydayCategory, Body("How many words are in the longest official place name in the world?",
                "The Welsh village name has 58 letters; the Māori hill name has 85. One word each."), Difficulty.Extreme)
            .Category(SpaceCategory)
            .Card(SpaceCategory, Body("How many pieces of space junk larger than 10 cm orbit Earth?",
                "Around 35,000 tracked pieces — humanity litters everywhere it goes."), Difficulty.Hard)
            .Card(SpaceCategory, Body("How long is one day on Venus, in Earth days?",
                "About 243 Earth days — longer than its year (225). Venus is not okay."), Difficulty.Extreme)
            .Category(MeasurementCategory)
            .Card(MeasurementCategory, Body("How much does the internet weigh (all its moving electrons), roughly in grams?",
                "Estimates put it near 50 grams — a strawberry. Everything ever posted: one strawberry."), Difficulty.Extreme)

            .Build();

    private static string Body(string question, string answer) =>
        "<b>Everyone: secretly write down your guess.</b>\n\n" + question +
        "\n\n<i>Reveal together, then tap to see the answer…</i>\n\n" +
        "<b>Answer:</b> " + answer +
        "\n\nClosest guess takes the point. Ties share it.";
}
