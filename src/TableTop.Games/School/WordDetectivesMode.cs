using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Word Detectives — etymology and morphology game for Grade 6.
///
/// Each card shows a word and asks the player to:
///   1. Break it into its component parts (prefix / root / suffix).
///   2. Give the meaning of at least one component.
///   3. Bonus: name another English word that shares the same root.
///
/// Scoring: 1 pt each for parts + meaning (max 2), +1 bonus for related word.
/// Group adjudicates. Discussion is encouraged — "is that the same root?"
///
/// Roots are drawn from Latin and Greek, matching the Grade 6 curriculum.
/// Categories: Greek Roots, Latin Roots, Prefixes, Suffixes, Mixed.
/// </summary>
public sealed class WordDetectivesMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Word Detectives";
    /// <inheritdoc />
    public override string Description =>
        "Break the word into its parts, find the root, earn a bonus for a related word. Grade 6 etymology.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ Identified (+2)";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "→ Next word";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Greek Root"] = "#42A5F5",
            ["Latin Root"] = "#66BB6A",
            ["Prefix"] = "#FFCA28",
            ["Suffix"] = "#AB47BC",
            ["Mixed"] = "#EC407A",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 2);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        WordDetectivesCardBank.All;

    /// <summary>Exposes the card bank for testing.</summary>
    public static IReadOnlyList<ICard> GetCards() => WordDetectivesCardBank.All;
}

/// <summary>Built-in card bank for WordDetectives. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class WordDetectivesCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── EASY: single obvious root, common English words ───────────────────

        W("Telescope",   "Greek Root", Difficulty.Easy,
          "🔍 <b>TELESCOPE</b>\n\n" +
          "Break it down: <b>tele-</b> + <b>-scope</b>\n" +
          "What does each part mean?\n\n" +
          "🌟 Bonus: Name another word with <b>tele-</b>."),

        W("Bicycle",     "Greek Root", Difficulty.Easy,
          "🔍 <b>BICYCLE</b>\n\n" +
          "Break it down: <b>bi-</b> + <b>-cycle</b>\n" +
          "What does each part mean?\n\n" +
          "🌟 Bonus: Name another word with <b>bi-</b>."),

        W("Photograph",  "Greek Root", Difficulty.Easy,
          "🔍 <b>PHOTOGRAPH</b>\n\n" +
          "Break it down: <b>photo-</b> + <b>-graph</b>\n" +
          "What does each part mean?\n\n" +
          "🌟 Bonus: Name another word with <b>photo-</b>."),

        W("Dictionary",  "Latin Root", Difficulty.Easy,
          "🔍 <b>DICTIONARY</b>\n\n" +
          "The root is <b>dict-</b> (Latin: to say/speak).\n" +
          "Name the root and give its meaning.\n\n" +
          "🌟 Bonus: Name two other words with <b>dict-</b>."),

        W("Microphone",  "Greek Root", Difficulty.Easy,
          "🔍 <b>MICROPHONE</b>\n\n" +
          "Break it down: <b>micro-</b> + <b>-phone</b>\n" +
          "What does each part mean?\n\n" +
          "🌟 Bonus: Name a word with <b>micro-</b> AND one with <b>-phone</b>."),

        W("Submarine",   "Latin Root", Difficulty.Easy,
          "🔍 <b>SUBMARINE</b>\n\n" +
          "Break it down: <b>sub-</b> + <b>marine</b>\n" +
          "What does each part mean?\n\n" +
          "🌟 Bonus: Name two other words with <b>sub-</b>."),

        W("Triangle",    "Latin Root", Difficulty.Easy,
          "🔍 <b>TRIANGLE</b>\n\n" +
          "Break it down: <b>tri-</b> + <b>angle</b>\n" +
          "What does <b>tri-</b> mean? Name two more <b>tri-</b> words.\n\n" +
          "🌟 Bonus: What number root follows tri in the sequence?"),

        W("Unhappy",     "Prefix", Difficulty.Easy,
          "🔍 <b>UNHAPPY</b>\n\n" +
          "The prefix is <b>un-</b>. What does it do to the base word?\n\n" +
          "Name the base word, then name <b>three</b> other <b>un-</b> words.\n\n" +
          "🌟 Bonus: Name a different prefix that means the same thing as <b>un-</b>."),

        W("Homeless",    "Suffix",    Difficulty.Easy,
          "🔍 <b>HOMELESS</b>\n\n" +
          "The suffix is <b>-less</b>. What does it mean?\n" +
          "Name the base word, then name three other <b>-less</b> words.\n\n" +
          "🌟 Bonus: What suffix means the <b>opposite</b> of <b>-less</b>?"),

        W("Happiness",   "Suffix",    Difficulty.Easy,
          "🔍 <b>HAPPINESS</b>\n\n" +
          "The suffix <b>-ness</b> turns adjectives into nouns.\n" +
          "What is the base adjective here?\n\n" +
          "🌟 Bonus: Give two more adjective → noun conversions using <b>-ness</b>."),

        W("Rewrite",     "Prefix",    Difficulty.Easy,
          "🔍 <b>REWRITE</b>\n\n" +
          "The prefix is <b>re-</b>. What does it mean?\n" +
          "Name the base word.\n\n" +
          "🌟 Bonus: Name four other <b>re-</b> words from school life."),

        W("Autobiography","Mixed",   Difficulty.Easy,
          "🔍 <b>AUTOBIOGRAPHY</b>\n\n" +
          "Three parts: <b>auto-</b> + <b>bio-</b> + <b>-graphy</b>\n" +
          "What does each part mean?\n\n" +
          "🌟 Bonus: How is an autobiography different from a biography?"),

        // ── MEDIUM: less obvious, two-root words, subject vocabulary ──────────

        W("Geology",     "Greek Root", Difficulty.Medium,
          "🔍 <b>GEOLOGY</b>\n\n" +
          "Break it down: <b>geo-</b> + <b>-logy</b>\n" +
          "What does each part mean?\n\n" +
          "🌟 Bonus: Name <b>four</b> other <b>-logy</b> words (fields of study)."),

        W("Democracy",   "Greek Root", Difficulty.Medium,
          "🔍 <b>DEMOCRACY</b>\n\n" +
          "From Greek: <b>demos</b> (people) + <b>kratos</b> (power/rule).\n" +
          "Explain the connection between the root meanings and what democracy means.\n\n" +
          "🌟 Bonus: Name another <b>-cracy</b> word."),

        W("Thermometer", "Greek Root", Difficulty.Medium,
          "🔍 <b>THERMOMETER</b>\n\n" +
          "Break it down: <b>thermo-</b> + <b>-meter</b>\n" +
          "What does each part mean?\n\n" +
          "🌟 Bonus: Name three other <b>-meter</b> words."),

        W("Biodegradable","Mixed",    Difficulty.Medium,
          "🔍 <b>BIODEGRADABLE</b>\n\n" +
          "Parts: <b>bio-</b> + <b>de-</b> + <b>grade</b> + <b>-able</b>\n" +
          "What does each part add to the overall meaning?\n\n" +
          "🌟 Bonus: What does <b>-able</b> do to any root word?"),

        W("Prehistoric",  "Mixed",    Difficulty.Medium,
          "🔍 <b>PREHISTORIC</b>\n\n" +
          "Break it down: <b>pre-</b> + <b>histor-</b> + <b>-ic</b>\n" +
          "What does each part mean?\n\n" +
          "🌟 Bonus: Name three other <b>pre-</b> words used in history."),

        W("Contradict",   "Latin Root",Difficulty.Medium,
          "🔍 <b>CONTRADICT</b>\n\n" +
          "Parts: <b>contra-</b> (against) + <b>dict</b> (to say)\n" +
          "Explain what it literally means to contradict someone.\n\n" +
          "🌟 Bonus: Name two other <b>contra-</b> words."),

        W("Transparent",  "Latin Root",Difficulty.Medium,
          "🔍 <b>TRANSPARENT</b>\n\n" +
          "Parts: <b>trans-</b> (across/through) + <b>par-</b> (show/appear) + <b>-ent</b>\n" +
          "What does the word literally mean from its roots?\n\n" +
          "🌟 Bonus: Name three other <b>trans-</b> words."),

        W("Sympathise",   "Greek Root",Difficulty.Medium,
          "🔍 <b>SYMPATHISE</b>\n\n" +
          "From Greek: <b>sym-</b> (together/with) + <b>pathos</b> (feeling/suffering)\n" +
          "How do the roots explain the meaning of the word?\n\n" +
          "🌟 Bonus: Name the difference between sympathise and empathise."),

        W("Submarine",    "Latin Root",Difficulty.Medium,
          "🔍 <b>INTERSTELLAR</b>\n\n" +
          "Parts: <b>inter-</b> (between) + <b>stella</b> (star) + <b>-ar</b>\n" +
          "What does the word literally mean?\n\n" +
          "🌟 Bonus: Name three other <b>inter-</b> words."),

        W("Manuscript",   "Latin Root",Difficulty.Medium,
          "🔍 <b>MANUSCRIPT</b>\n\n" +
          "Parts: <b>manu-</b> (hand) + <b>scrib/script</b> (write)\n" +
          "Explain what this literally means and why old documents were called manuscripts.\n\n" +
          "🌟 Bonus: Name three other <b>scrib-/script-</b> words."),

        W("Circumnavigate","Latin Root",Difficulty.Medium,
          "🔍 <b>CIRCUMNAVIGATE</b>\n\n" +
          "Parts: <b>circum-</b> (around) + <b>navig-</b> (sail/steer) + <b>-ate</b>\n" +
          "What does it mean to circumnavigate the globe?\n\n" +
          "🌟 Bonus: Name two other <b>circum-</b> words."),

        // ── HARD: multiple roots, abstract meaning, subject-specific ──────────

        W("Philanthropy",  "Greek Root",Difficulty.Hard,
          "🔍 <b>PHILANTHROPY</b>\n\n" +
          "From Greek: <b>philan-</b> (loving) + <b>thrōpos</b> (human being)\n" +
          "Explain the etymology AND what a philanthropist does today.\n\n" +
          "🌟 Bonus: Name the two Greek words that give us <b>philosophy</b>."),

        W("Omnivore",      "Latin Root",Difficulty.Hard,
          "🔍 <b>OMNIVORE</b>\n\n" +
          "Parts: <b>omni-</b> (all/every) + <b>vore</b> (to eat)\n" +
          "Name the difference between an omnivore, herbivore, and carnivore — using the roots to explain.\n\n" +
          "🌟 Bonus: What does <b>omni-</b> mean in the word omniscient?"),

        W("Synchronise",   "Greek Root",Difficulty.Hard,
          "🔍 <b>SYNCHRONISE</b>\n\n" +
          "From Greek: <b>syn-</b> (together) + <b>chronos</b> (time)\n" +
          "Explain the meaning from the roots, then give a real example of synchronisation.\n\n" +
          "🌟 Bonus: Name three other <b>chron-</b> words."),

        W("Malevolent",    "Latin Root",Difficulty.Hard,
          "🔍 <b>MALEVOLENT</b>\n\n" +
          "Parts: <b>male-</b> (bad/evil) + <b>vol-</b> (wish/will) + <b>-ent</b>\n" +
          "What does it literally mean to 'have evil wishes'?\n" +
          "What is its antonym using a different Latin prefix?\n\n" +
          "🌟 Bonus: Name two other <b>male-/mal-</b> words."),

        W("Anachronism",   "Greek Root",Difficulty.Hard,
          "🔍 <b>ANACHRONISM</b>\n\n" +
          "Parts: <b>ana-</b> (against/back) + <b>chronos</b> (time) + <b>-ism</b>\n" +
          "Explain what an anachronism is and give a real example from film or history.\n\n" +
          "🌟 Bonus: Name the literary term for placing a modern thing in an ancient setting."),

        W("Anthropology",  "Greek Root",Difficulty.Hard,
          "🔍 <b>ANTHROPOLOGY</b>\n\n" +
          "Parts: <b>anthrop-</b> (human) + <b>-logy</b> (study of)\n" +
          "Name the difference between anthropology and archaeology.\n\n" +
          "🌟 Bonus: What are the three other major <b>-logy</b> subjects studied at GCSE?"),

        // ── EXTREME: rare roots, connotation, etymology chain ─────────────────

        W("Sesquipedalian","Latin Root",Difficulty.Extreme,
          "🔍 <b>SESQUIPEDALIAN</b>\n\n" +
          "From Latin: <b>sesqui-</b> (one and a half) + <b>peda-</b> (foot) + <b>-ian</b>\n" +
          "This is a self-referential word — explain why, using the etymology.\n\n" +
          "🌟 Bonus: What does it mean to call someone a sesquipedalian?"),

        W("Perspicacious",  "Latin Root",Difficulty.Extreme,
          "🔍 <b>PERSPICACIOUS</b>\n\n" +
          "From Latin: <b>per-</b> (through) + <b>spic-</b> (see/look) + <b>-acious</b>\n" +
          "What does it mean to be perspicacious?\n" +
          "Name the common English word that shares the <b>spic/spec</b> root.\n\n" +
          "🌟 Bonus: Name three more <b>spec-/spic-</b> words."),

        W("Magnanimous",    "Latin Root",Difficulty.Extreme,
          "🔍 <b>MAGNANIMOUS</b>\n\n" +
          "Parts: <b>magn-</b> (great) + <b>animus</b> (spirit/mind/soul)\n" +
          "Explain what a magnanimous person is like, using the roots to justify.\n\n" +
          "🌟 Bonus: Name the word for someone with the OPPOSITE disposition, using a Latin root."),

        W("Loquacious",     "Latin Root",Difficulty.Extreme,
          "🔍 <b>LOQUACIOUS</b>\n\n" +
          "From Latin: <b>loqu-</b> (speak) + <b>-acious</b> (tending to)\n" +
          "Define loquacious and use it correctly in a sentence.\n" +
          "Name one other <b>-acious</b> word and explain what <b>-acious</b> adds.\n\n" +
          "🌟 Bonus: Name another <b>loqu-/locut-</b> word."),
    ];

    private static ICard W(string title, string category, Difficulty d, string desc) =>
        StandardCard.Create(title, desc, d, category);
}