using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.FactOrFiction;

/// <summary>
/// Expert Fact or Fiction — 60 topic-specific statements for players who want a challenge.
///
/// Topics: History, Science, Pop Culture, Sports, Geography, Literature, Urban Legends.
/// Mix of plausible-sounding lies and unbelievable truths.
///
/// Perfect for: trivia nights, pub quizzes, expert-level parties, themed nights.
/// Scoring: 1 point per correct guess, 2 points if you fool everyone with a false statement.
/// </summary>
public sealed class ExpertFactOrFictionMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Expert Fact or Fiction";
    /// <inheritdoc />
    public override string Description =>
        "60 topic-specific facts: History, Science, Pop Culture, Sports. Hard to spot the fakes.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ Guess";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "→ Next";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [ExpertFactOrFictionCardBank.HistoryCategory] = "#8B4513",
            [ExpertFactOrFictionCardBank.ScienceCategory] = "#1E90FF",
            [ExpertFactOrFictionCardBank.PopCultureCategory] = "#FF69B4",
            [ExpertFactOrFictionCardBank.SportsCategory] = "#228B22",
            [ExpertFactOrFictionCardBank.GeographyCategory] = "#FF8C00",
            [ExpertFactOrFictionCardBank.LiteratureCategory] = "#4B0082",
            [ExpertFactOrFictionCardBank.UrbanLegendCategory] = "#DC143C",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        ExpertFactOrFictionCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => ExpertFactOrFictionCardBank.All;
}

/// <summary>Built-in card bank for ExpertFactOrFiction. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class ExpertFactOrFictionCardBank
{
    internal const string HistoryCategory = "History";
    internal const string ScienceCategory = "Science";
    internal const string PopCultureCategory = "Pop Culture";
    internal const string SportsCategory = "Sports";
    internal const string GeographyCategory = "Geography";
    internal const string LiteratureCategory = "Literature";
    internal const string UrbanLegendCategory = "Urban Legend";

    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── HISTORY ──────────────────────────────────────────────────────

        E("Leonardo da Vinci was left-handed and wrote backwards to keep his work secret.",
          true, HistoryCategory, Difficulty.Medium),

        E("The Great Pyramid of Giza was the tallest building in the world until 1889.",
          true, HistoryCategory, Difficulty.Medium),

        E("Ancient Romans had a specific word for the space between your eyebrows.",
          true, HistoryCategory, Difficulty.Medium),

        E("Queen Victoria was the first person photographed.", false, HistoryCategory, Difficulty.Hard),

        E("The Ottoman Empire lasted over 600 years.",
          true, HistoryCategory, Difficulty.Medium),

        E("Medieval knights actually weighed over 300 pounds due to their armour.",
          false, HistoryCategory, Difficulty.Hard),

        E("Julius Caesar was never actually crowned Emperor of Rome.",
          true, HistoryCategory, Difficulty.Hard),

        E("The Library of Alexandria contained over 700,000 volumes.",
          true, HistoryCategory, Difficulty.Hard),

        // ── SCIENCE ──────────────────────────────────────────────────────

        E("A single bolt of lightning is hotter than the surface of the sun.",
          true, ScienceCategory, Difficulty.Medium),

        E("Gold is the only metal that doesn't rust.",
          false, ScienceCategory, Difficulty.Hard),

        E("Neutron stars are so dense that a teaspoon of their material would weigh as much as a mountain.",
          true, ScienceCategory, Difficulty.Medium),

        E("The human brain generates more electricity than a 9-volt battery.",
          false, ScienceCategory, Difficulty.Hard),

        E("Quantum entanglement allows information to travel faster than light.",
          false, ScienceCategory, Difficulty.Extreme),

        E("A type of jellyfish called Turritopsis dohrnii is biologically immortal.",
          true, ScienceCategory, Difficulty.Hard),

        E("The vacuum of space smells like burnt steak according to astronauts.",
          true, ScienceCategory, Difficulty.Hard),

        E("DNA was discovered by Watson and Crick without seeing its structure.",
          false, ScienceCategory, Difficulty.Extreme),

        // ── POP CULTURE ──────────────────────────────────────────────────

        E("The Beatles were not allowed to perform in Singapore because of their hair.",
          false, PopCultureCategory, Difficulty.Medium),

        E("Stanley Kubrick was so perfectionist he filmed 'The Shining' for 247 days.",
          true, PopCultureCategory, Difficulty.Medium),

        E("The Lord of the Rings trilogy was filmed over 8 years.",
          false, PopCultureCategory, Difficulty.Hard),

        E("'Game of Thrones' author George R.R. Martin writes on a 1980s DOS computer.",
          true, PopCultureCategory, Difficulty.Medium),

        E("The Wizard of Oz was filmed in just 2 weeks.",
          false, PopCultureCategory, Difficulty.Medium),

        E("Michael Jackson actually owned the rights to the South Park theme song.",
          true, PopCultureCategory, Difficulty.Hard),

        E("The Jaws theme was originally composed without any string instruments.",
          false, PopCultureCategory, Difficulty.Hard),

        E("Marilyn Monroe's shoe size was size 15 (US men's).",
          false, PopCultureCategory, Difficulty.Hard),

        // ── SPORTS ───────────────────────────────────────────────────────

        E("A regulation basketball has 39,000 pebbles on its surface.",
          true, SportsCategory, Difficulty.Hard),

        E("Serena Williams has won more Grand Slam titles than any other female player in history.",
          true, SportsCategory, Difficulty.Medium),

        E("Baseball was banned in England for 20 years.",
          false, SportsCategory, Difficulty.Hard),

        E("Olympic gold medals are only 1.34% gold.",
          true, SportsCategory, Difficulty.Hard),

        E("Muhammad Ali knocked out Sonny Liston so hard he was found unconscious in the dressing room.",
          false, SportsCategory, Difficulty.Hard),

        E("A tennis ball travels at speeds up to 120 mph in professional play.",
          true, SportsCategory, Difficulty.Medium),

        E("The FIFA World Cup trophy must be held by all players who won it.",
          false, SportsCategory, Difficulty.Hard),

        // ── GEOGRAPHY ────────────────────────────────────────────────────

        E("Russia spans 11 time zones.",
          true, GeographyCategory, Difficulty.Medium),

        E("The Dead Sea is the lowest point on Earth's surface.",
          true, GeographyCategory, Difficulty.Medium),

        E("Africa is the second-smallest continent.",
          false, GeographyCategory, Difficulty.Hard),

        E("Mount Everest is the tallest mountain from sea level, but not from the centre of the Earth.",
          true, GeographyCategory, Difficulty.Hard),

        E("The Sahara Desert grows larger every year.",
          true, GeographyCategory, Difficulty.Hard),

        E("Australia is wider than the distance from Earth to the Moon.",
          false, GeographyCategory, Difficulty.Extreme),

        E("Iceland is getting larger due to tectonic activity.",
          true, GeographyCategory, Difficulty.Hard),

        E("The Mariana Trench is so deep that Mount Everest could fit inside it underwater.",
          true, GeographyCategory, Difficulty.Hard),

        // ── LITERATURE ───────────────────────────────────────────────────

        E("Jane Austen wrote 'Pride and Prejudice' when she was 21.",
          false, LiteratureCategory, Difficulty.Hard),

        E("Charles Dickens wrote 'A Christmas Carol' in just 6 weeks.",
          true, LiteratureCategory, Difficulty.Hard),

        E("Stephen King wrote 'The Shining' in a single hotel room.",
          true, LiteratureCategory, Difficulty.Medium),

        E("J.K. Rowling wrote the final Harry Potter book at the top of the Eiffel Tower.",
          false, LiteratureCategory, Difficulty.Hard),

        E("Bram Stoker had never actually been to Transylvania when he wrote Dracula.",
          true, LiteratureCategory, Difficulty.Hard),

        E("The word 'robot' was invented by Shakespeare.",
          false, LiteratureCategory, Difficulty.Hard),

        E("Dr. Seuss wrote 'Green Eggs and Ham' using only 50 different words.",
          true, LiteratureCategory, Difficulty.Medium),

        // ── URBAN LEGENDS & WEIRD ────────────────────────────────────────

        E("There is a town in France called Fucking (now officially Fugging).",
          true, UrbanLegendCategory, Difficulty.Medium),

        E("The phrase '5G causes COVID' originated in a lab in China.",
          false, UrbanLegendCategory, Difficulty.Medium),

        E("Area 51 is actually a NASA facility, not a government secret base.",
          false, UrbanLegendCategory, Difficulty.Hard),

        E("Sloths only come down from trees once a week to defecate, and can lose 30% of their body weight doing so.",
          true, UrbanLegendCategory, Difficulty.Hard),

        E("A group of flamingos is called a 'flamboyance' according to the Oxford Dictionary.",
          true, UrbanLegendCategory, Difficulty.Hard),

        E("The phrase 'rule of three' in filmmaking means every joke or action repeated three times is funny.",
          true, UrbanLegendCategory, Difficulty.Medium),

        E("Chewing gum stays in your stomach for 7 years if swallowed.",
          false, UrbanLegendCategory, Difficulty.Easy),

        E("Humans use 10% of their brain (the remaining 90% is unused).",
          false, UrbanLegendCategory, Difficulty.Easy),

        E("If you flush a toilet while the bathroom door is closed, bacteria spreads everywhere.",
          false, UrbanLegendCategory, Difficulty.Medium),

        E("Red cars are pulled over more by police.",
          false, UrbanLegendCategory, Difficulty.Medium),

        E("You swallow spiders in your sleep.",
          false, UrbanLegendCategory, Difficulty.Easy),

        E("Shaving makes your hair grow back thicker.",
          false, UrbanLegendCategory, Difficulty.Easy),

        // ── BONUS: WILDLY UNBELIEVABLE TRUTHS ────────────────────────────

        E("Bananas are berries but strawberries are technically not berries.",
          true, ScienceCategory, Difficulty.Medium),

        E("A group of porcupines is called a 'prickle'.",
          true, UrbanLegendCategory, Difficulty.Easy),

        E("The inventor of the Pringles tube died and was cremated inside one.",
          true, UrbanLegendCategory, Difficulty.Hard),

        E("Cleopatra lived closer in time to the moon landing than to the Great Pyramid's construction.",
          true, HistoryCategory, Difficulty.Hard),

        E("Oxford University is older than the Aztec Empire.",
          true, HistoryCategory, Difficulty.Hard),

        E("Honey is the only food that doesn't expire.",
          true, ScienceCategory, Difficulty.Easy),

        E("A jiffy is an actual unit of time used by physicists.",
          true, ScienceCategory, Difficulty.Hard),

        E("A group of crows is called a 'murder' and they can hold grudges.",
          true, UrbanLegendCategory, Difficulty.Medium),
    ];

    private static ICard E(string text, bool isFact, string category, Difficulty d) =>
        StandardCard.Create(
            title: "Fact… or Fiction?",
            description:
                "<b>Statement:</b>\n\n" + text +
                "\n\n<b>Everyone vote:</b> fact or fiction? Hands up together — no discussion until all votes are in." +
                "\n\n<i>Reader: keep the next line to yourself until then…</i>\n\n" +
                "<b>Answer: " + (isFact ? "✅ FACT" : "❌ FICTION") + "</b>",
            difficulty: d,
            category: category);
}