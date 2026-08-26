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
    public override string Name        => "Estimation Station";
    /// <inheritdoc />
    public override string Description =>
        "Everyone secretly guesses the number — closest wins. Reasoning beats knowledge.";

    /// <summary>Label shown on the button that records a completed round.</summary>
    public override string CompleteLabel => "Closest";
    /// <summary>Label shown on the button that skips the current card.</summary>
    public override string SkipLabel     => "Skip";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Measurement"] = "#42A5F5",
            ["Nature"]      = "#66BB6A",
            ["Everyday"]    = "#FFA726",
            ["Space"]       = "#AB47BC",
            ["Body"]        = "#EC407A",
            ["Speed & Time"]= "#26C6DA",
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
    /// <summary>All estimation cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── MEASUREMENT ───────────────────────────────────────────────────────
        E("Measurement", "How many litres of water fill a standard bathtub?",
          "About 150 litres (a bucket is ~10 L — picture 15 buckets).", Difficulty.Easy),
        E("Measurement", "How tall is an adult giraffe, in metres?",
          "About 5 metres — roughly a two-storey house.", Difficulty.Easy),
        E("Measurement", "How many centimetres long is a standard school ruler?",
          "30 cm. (If someone gets this wrong, discuss.)", Difficulty.Easy),
        E("Measurement", "How much does an adult elephant weigh, in kilograms?",
          "About 5,000 kg (5 tonnes) for an African elephant.", Difficulty.Medium),
        E("Measurement", "How long is a football (soccer) pitch, in metres?",
          "About 105 metres, goal line to goal line.", Difficulty.Medium),
        E("Measurement", "How many millilitres are in a typical can of fizzy drink?",
          "330 ml in most of the world; 355 ml in North America.", Difficulty.Easy),
        E("Measurement", "How heavy is a typical cloud (a fair-weather cumulus), in kilograms?",
          "Around 500,000 kg — the weight of about 100 elephants, floating.", Difficulty.Hard),
        E("Measurement", "How deep is the deepest point of the ocean, in metres?",
          "About 11,000 m (the Mariana Trench). Everest would sink without a trace.", Difficulty.Medium),

        // ── NATURE ────────────────────────────────────────────────────────────
        E("Nature", "How many legs does a typical millipede actually have?",
          "Most have 100–400. The record holder has 1,306 — but 'milli' (1,000) is marketing.", Difficulty.Medium),
        E("Nature", "How many years can a giant tortoise live?",
          "Over 150 years. Some alive today hatched in the 1800s.", Difficulty.Easy),
        E("Nature", "How many bees live in a typical honeybee hive in summer?",
          "About 50,000.", Difficulty.Medium),
        E("Nature", "How fast can a cheetah run at top speed, in km/h?",
          "About 110 km/h — motorway speed, on paws.", Difficulty.Easy),
        E("Nature", "How many hearts does an octopus have?",
          "Three. Two pump blood to the gills, one to the body.", Difficulty.Easy),
        E("Nature", "How tall is the tallest tree on Earth, in metres?",
          "About 116 m (a coast redwood named Hyperion) — taller than a 30-storey building.", Difficulty.Medium),
        E("Nature", "How many ants are estimated to live on Earth, in trillions?",
          "About 20,000 trillion (20 quadrillion). Roughly 2.5 million ants per human.", Difficulty.Hard),
        E("Nature", "How long can a snail sleep, in years?",
          "Up to 3 years in one stretch, waiting for wet weather.", Difficulty.Hard),

        // ── EVERYDAY ──────────────────────────────────────────────────────────
        E("Everyday", "How many times does a person blink in one day?",
          "About 15,000–20,000 times.", Difficulty.Medium),
        E("Everyday", "How many sheets of paper are in a standard ream?",
          "500 sheets.", Difficulty.Easy),
        E("Everyday", "How many steps does an average person take in a day?",
          "About 4,000–5,000 for most people (the famous 10,000 was a marketing slogan).", Difficulty.Easy),
        E("Everyday", "How many words does an average person speak per day?",
          "Roughly 16,000 — men and women almost identical, despite the myth.", Difficulty.Medium),
        E("Everyday", "How many hours will the average person spend asleep by age 75?",
          "About 220,000 hours — roughly 25 YEARS of sleeping.", Difficulty.Hard),
        E("Everyday", "How many grapes does it take to make one bottle of wine?",
          "About 600–800 grapes.", Difficulty.Hard),
        E("Everyday", "How many keys are on a full-size piano?",
          "88 — 52 white, 36 black.", Difficulty.Easy),
        E("Everyday", "How many litres of milk does a dairy cow produce per day?",
          "About 25–30 litres.", Difficulty.Medium),

        // ── SPACE ─────────────────────────────────────────────────────────────
        E("Space", "How long does light from the Sun take to reach Earth, in minutes?",
          "About 8 minutes 20 seconds.", Difficulty.Easy),
        E("Space", "How many Earths would fit inside the Sun?",
          "About 1.3 million.", Difficulty.Medium),
        E("Space", "How far away is the Moon, in kilometres?",
          "About 384,000 km — you could line up all the other planets in the gap.", Difficulty.Medium),
        E("Space", "How many days does Mercury take to orbit the Sun?",
          "88 days. A Mercury 'year' is shorter than a school term.", Difficulty.Hard),
        E("Space", "How fast is the International Space Station moving, in km/h?",
          "About 28,000 km/h — it laps Earth every 90 minutes.", Difficulty.Hard),
        E("Space", "What temperature is the surface of the Sun, in degrees Celsius?",
          "About 5,500 °C. (The core is 15 million.)", Difficulty.Medium),

        // ── BODY ──────────────────────────────────────────────────────────────
        E("Body", "How many bones does an adult human have?",
          "206. Babies start with about 300 — many fuse as they grow.", Difficulty.Easy),
        E("Body", "How many times does your heart beat in one day?",
          "About 100,000 times.", Difficulty.Medium),
        E("Body", "How long are all the blood vessels in your body if laid end to end, in kilometres?",
          "Roughly 100,000 km — two and a half times around the Earth.", Difficulty.Hard),
        E("Body", "How many litres of saliva does a person produce in a year?",
          "About 400–500 litres. You're welcome.", Difficulty.Hard),
        E("Body", "How many muscles does it take to smile?",
          "About 12 — and around 11 to frown, so the old saying is backwards.", Difficulty.Medium),
        E("Body", "How fast does a sneeze travel, in km/h?",
          "About 60–70 km/h (the '160 km/h' figure is a myth, but it's still fast).", Difficulty.Medium),

        // ── SPEED & TIME ──────────────────────────────────────────────────────
        E("Speed & Time", "How many seconds are in one day?",
          "86,400.", Difficulty.Easy),
        E("Speed & Time", "How long would it take to walk around the Earth's equator, walking 8 hours a day?",
          "About 3 years (40,000 km at ~5 km/h, 8 h/day ≈ 1,000 days) — ignoring the oceans.", Difficulty.Hard),
        E("Speed & Time", "How fast does sound travel through air, in metres per second?",
          "About 343 m/s. Count seconds between lightning and thunder, divide by 3, get kilometres.", Difficulty.Medium),
        E("Speed & Time", "How many minutes are in a week?",
          "10,080.", Difficulty.Medium),
        E("Speed & Time", "How old is the Earth, in billions of years?",
          "About 4.5 billion years.", Difficulty.Easy),
        E("Speed & Time", "If you counted one number per second, nonstop, how many DAYS to reach a million?",
          "About 11.5 days. (A billion would take 32 years.)", Difficulty.Hard),

        // ── EXPANSION: WEIRD BUT TRUE ─────────────────────────────────────────
        E("Body", "How many skin cells do you shed per day?",
          "Around 500 million. A lot of household dust used to be you.", Difficulty.Hard),
        E("Body", "How many dreams does a person have per night?",
          "About 4–6. You forget nearly all of them within minutes.", Difficulty.Medium),
        E("Body", "How many times will you laugh today, on average?",
          "Adults: about 15–20 times. Children: closer to 300. Discuss.", Difficulty.Medium),
        E("Nature", "How many times does a hummingbird's heart beat per minute?",
          "Up to 1,200 while flying.", Difficulty.Hard),
        E("Nature", "How loud is a blue whale's call, in decibels?",
          "About 188 dB — louder than a jet engine; audible across hundreds of kilometres of ocean.", Difficulty.Hard),
        E("Nature", "How many eggs does a queen bee lay per day in summer?",
          "Up to 2,000 — more than her own body weight.", Difficulty.Medium),
        E("Everyday", "How many times does the average person check their phone per day?",
          "Around 100–150 times. Yes, including during this game.", Difficulty.Easy),
        E("Everyday", "How many years of their life does the average person spend queueing?",
          "Roughly 6 months to a year, depending on country. Feels longer.", Difficulty.Hard),
        E("Everyday", "How many words are in the longest official place name in the world?",
          "The Welsh village name has 58 letters; the Māori hill name has 85. One word each.", Difficulty.Extreme),
        E("Space", "How many pieces of space junk larger than 10 cm orbit Earth?",
          "Around 35,000 tracked pieces — humanity litters everywhere it goes.", Difficulty.Hard),
        E("Space", "How long is one day on Venus, in Earth days?",
          "About 243 Earth days — longer than its year (225). Venus is not okay.", Difficulty.Extreme),
        E("Measurement", "How much does the internet weigh (all its moving electrons), roughly in grams?",
          "Estimates put it near 50 grams — a strawberry. Everything ever posted: one strawberry.", Difficulty.Extreme),
    ];

    private static ICard E(string category, string question, string answer, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>Everyone: secretly write down your guess.</b>\n\n" + question +
            "\n\n<i>Reveal together, then tap to see the answer…</i>\n\n" +
            "<b>Answer:</b> " + answer +
            "\n\nClosest guess takes the point. Ties share it.",
            d, category);
}
