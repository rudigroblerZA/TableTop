using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.FactOrFiction;

/// <summary>
/// Fact or Fiction — 80 statements ranging from mundane to surprising.
///
/// How to play:
///   1. One player reads a statement aloud.
///   2. Other players vote: Fact or Fiction? (simultaneously, no discussion yet)
///   3. Reveal the answer.
///   4. One point for correct guess; bonus if you fooled everyone.
///
/// Perfect for: parties, trivia nights, getting to know people better.
/// Difficulty ranges from everyday facts (Easy) to surprisingly obscure truths (Extreme).
/// </summary>
public sealed class FactOrFictionMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Fact or Fiction";
    /// <inheritdoc />
    public override string Description =>
        "80 wild statements — guess which are true and which are made up. Easy to Extreme.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ Guess";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "→ Next";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [FactOrFictionCardBank.EverydayCategory] = "#66BB6A",
            [FactOrFictionCardBank.SurprisingCategory] = "#42A5F5",
            [FactOrFictionCardBank.OutlandishCategory] = "#FFCA28",
            [FactOrFictionCardBank.WildCategory] = "#EC407A",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        FactOrFictionCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => FactOrFictionCardBank.All;
}

/// <summary>Built-in card bank for FactOrFiction. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class FactOrFictionCardBank
{
    internal const string EverydayCategory = "Everyday";
    internal const string SurprisingCategory = "Surprising";
    internal const string OutlandishCategory = "Outlandish";
    internal const string WildCategory = "Wild";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── EVERYDAY — Easy facts most people don't know ──────────────────

        F("A group of flamingos is called a 'flamboyance'.",
          Difficulty.Easy),

        F("Honey never spoils. Archaeologists have found 3000-year-old honey in Egyptian tombs that was still edible.",
          Difficulty.Easy),

        F("Bananas are berries, but strawberries are not.",
          Difficulty.Easy),

        F("A shark must keep moving or it will sink.",
          Difficulty.Easy),

        F("Octopuses have three hearts.",
          Difficulty.Easy),

        F("Wombats produce cube-shaped droppings.",
          Difficulty.Easy),

        F("A penguin's knees are inside its body.",
          Difficulty.Easy),

        F("Cats have a third eyelid called a nictitating membrane.",
          Difficulty.Easy),

        F("Koalas sleep 22 hours a day.",
          Difficulty.Easy),

        F("Sloths only defecate once a week.",
          Difficulty.Easy),

        F("A group of crows is called a 'murder'.",
          Difficulty.Easy),

        F("Horses can't vomit.",
          Difficulty.Easy),

        F("Snakes can dislocate their jaws to swallow things larger than their head.",
          Difficulty.Easy),

        F("Butterflies taste with their feet.",
          Difficulty.Easy),

        F("A giraffe's tongue is 20 inches long.",
          Difficulty.Easy),

        F("Dolphins sleep with one eye open.",
          Difficulty.Easy),

        // ── SURPRISING — Weird but true facts ────────────────────────────

        F("Cleopatra lived closer to the invention of the iPhone than to the building of the Great Pyramid.",
          Difficulty.Medium),

        F("The shortest war in history lasted 38 minutes (between Britain and Zanzibar in 1896).",
          Difficulty.Medium),

        F("A day on Venus is longer than a year on Venus.",
          Difficulty.Medium),

        F("Honey is the only food that doesn't rot and can last thousands of years.",
          Difficulty.Medium),

        F("Scotland's national animal is a unicorn.",
          Difficulty.Medium),

        F("There are more stars in the universe than grains of sand on all Earth's beaches.",
          Difficulty.Medium),

        F("Octopuses have blue blood.",
          Difficulty.Medium),

        F("A group of zebras is called a 'zeal'.",
          Difficulty.Medium),

        F("The fingerprints of koalas are so similar to humans that they could confuse crime scene investigators.",
          Difficulty.Medium),

        F("Almonds are technically not nuts — they're seeds.",
          Difficulty.Medium),

        F("Peanuts are legumes, not nuts.",
          Difficulty.Medium),

        F("A cockroach can live for a week without its head.",
          Difficulty.Medium),

        F("Tardigrades (water bears) can survive in space.",
          Difficulty.Medium),

        F("A mantis shrimp can punch with the force of a .22 caliber bullet.",
          Difficulty.Medium),

        F("Platypuses glow under ultraviolet light.",
          Difficulty.Medium),

        F("The oldest known recipe is for beer, dating back 4000 years.",
          Difficulty.Medium),

        F("Bananas are radioactive due to potassium-40.",
          Difficulty.Medium),

        F("Carrots were originally purple, not orange.",
          Difficulty.Medium),

        // ── OUTLANDISH — These seem impossible but are true ──────────────

        F("A narwhal's tusk is actually a giant tooth with 10 million sensory receptors.",
          Difficulty.Hard),

        F("Axolotls can regenerate their brains.",
          Difficulty.Hard),

        F("Jellyfish are older than dinosaurs.",
          Difficulty.Hard),

        F("Some flatworms are hermaphrodites that engage in 'penis fencing'.",
          Difficulty.Hard),

        F("The Anglerfish female is 40 times larger than the male, and they fuse together permanently.",
          Difficulty.Hard),

        F("A blue whale's heart is as big as a car.",
          Difficulty.Hard),

        F("Swordfish can heat their eyes and brains to improve vision in cold water.",
          Difficulty.Hard),

        F("Some sea cucumbers expel their organs as a defence mechanism.",
          Difficulty.Hard),

        F("The blobfish looks normal in the deep ocean but becomes blob-like when brought to the surface due to pressure change.",
          Difficulty.Hard),

        F("A parrot fish can change its sex.",
          Difficulty.Hard),

        F("Dolphins have names for each other.",
          Difficulty.Hard),

        F("Cuttlefish can change colour and pattern while sleeping.",
          Difficulty.Hard),

        F("A mantis shrimp can see 16 types of colour receptors (humans see 3).",
          Difficulty.Hard),

        F("Some whales sing songs that get remixed by other whales.",
          Difficulty.Hard),

        F("The Greenland shark is the longest-living vertebrate, living over 400 years.",
          Difficulty.Hard),

        // ── WILD — Unbelievable facts that are actually true ──────────────

        F("A snail can have over 25,000 teeth.",
          Difficulty.Extreme),

        F("The smell of petrichor (rain on dry earth) comes from bacteria called actinomycetes.",
          Difficulty.Extreme),

        F("T-Rex couldn't bend their arms enough to touch their own mouth.",
          Difficulty.Extreme),

        F("A flea can jump 150 times its body length.",
          Difficulty.Extreme),

        F("Clownfish are all born female, and the dominant female becomes male if needed.",
          Difficulty.Extreme),

        F("A cat's purr vibrates at the same frequency that promotes bone healing.",
          Difficulty.Extreme),

        F("The Earth's magnetic poles swap positions every 200,000-300,000 years.",
          Difficulty.Extreme),

        F("Glass frogs have transparent skin and you can see their eggs through their belly.",
          Difficulty.Extreme),

        F("A hummingbird's heart can beat up to 1,260 times per minute.",
          Difficulty.Extreme),

        F("Elephants are afraid of bees.",
          Difficulty.Extreme),

        F("Ants don't sleep.",
          Difficulty.Extreme),

        F("A cockroach has been alive for 300 million years without changing much.",
          Difficulty.Extreme),

        F("Some jellyfish are technically immortal — they can revert to their juvenile form.",
          Difficulty.Extreme),

        F("A giraffe can clean its own ears with its tongue.",
          Difficulty.Extreme),

        F("Porcupines float naturally due to their quills.",
          Difficulty.Extreme),

        F("The mantis shrimp sees colours that don't exist in human perception.",
          Difficulty.Extreme),

        // ── FICTION — Made-up statements to trick people ─────────────────

        F("An ostrich buries its head in the sand when scared.",
          Difficulty.Easy, isFact: false),

        F("Goldfish have a 3-second memory.",
          Difficulty.Easy, isFact: false),

        F("Glass is a liquid.",
          Difficulty.Medium, isFact: false),

        F("Napoleon was very short.",
          Difficulty.Medium, isFact: false),

        F("We only use 10% of our brains.",
          Difficulty.Medium, isFact: false),

        F("Sharks never get cancer.",
          Difficulty.Medium, isFact: false),

        F("Cracking your knuckles causes arthritis.",
          Difficulty.Medium, isFact: false),

        F("Hot water freezes faster than cold water (without any specific conditions).",
          Difficulty.Hard, isFact: false),

        F("Humans shed their entire outer skin layer weekly.",
          Difficulty.Hard, isFact: false),

        F("A person can sneeze with their eyes open without them popping out.",
          Difficulty.Hard, isFact: false),

        F("Dinosaurs and humans coexisted.",
          Difficulty.Extreme, isFact: false),

        F("The Great Wall of China is visible from space with the naked eye.",
          Difficulty.Extreme, isFact: false),
    ];

    // Two defaults, for the two things every card was repeating. The four
    // difficulty tiers and the four categories are the same four bands — every
    // card in the bank pairs them the same way — so `category` derives from `d`
    // unless a card passes its own. `isFact` defaults to true because the
    // fiction cards are the minority (12 of 77), and a bank where the exception
    // is the thing spelled out is easier to scan than one where both are.
    private static ICard F(string text, Difficulty d, bool isFact = true, string? category = null) =>
        StandardCard.Create(
            title: "Fact… or Fiction?",
            description:
                "<b>Statement:</b>\n\n" + text +
                "\n\n<b>Everyone vote:</b> fact or fiction? Hands up together — no discussion until all votes are in." +
                "\n\n<i>Reader: keep the next line to yourself until then…</i>\n\n" +
                "<b>Answer: " + (isFact ? "✅ FACT" : "❌ FICTION") + "</b>",
            difficulty: d,
            category: category ?? CategoryFor(d));

    private static string CategoryFor(Difficulty d) => d switch
    {
        Difficulty.Easy => EverydayCategory,
        Difficulty.Medium => SurprisingCategory,
        Difficulty.Hard => OutlandishCategory,
        _ => WildCategory,
    };
}