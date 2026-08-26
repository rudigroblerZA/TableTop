using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Science Sprint — general-knowledge science for the classroom: the human
/// body, animals and plants, space, and everyday physics and chemistry, as
/// multiple-choice questions dealt one at a time.
///
/// A browsable subject deck (pick science, deal, discuss, reveal) rather than a
/// single hot-seat ladder. Four difficulties so it works from "how many legs
/// does an insect have" up to the questions that catch out the grown-ups.
/// </summary>
public sealed class ScienceSprintMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name        => "Science Sprint";
    /// <inheritdoc />
    public override string Description =>
        "Science general knowledge — the body, animals, space, and everyday physics and chemistry. Multiple choice, four difficulties.";

    /// <summary>Label for a correctly answered card.</summary>
    public override string CompleteLabel => "Correct";
    /// <summary>Label for a passed card.</summary>
    public override string SkipLabel     => "Pass";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["The Body"]   = "#EF5350",
            ["Living World"] = "#66BB6A",
            ["Space"]      = "#5C6BC0",
            ["Everyday Science"] = "#42A5F5",
            ["Elements"]   = "#FFA726",
        };

    /// <summary>Harder questions score more.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in Science Sprint card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        ScienceSprintCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => ScienceSprintCardBank.All;
}

/// <summary>Built-in card bank for Science Sprint.</summary>
public static class ScienceSprintCardBank
{
    /// <summary>All cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── THE BODY ─────────────────────────────────────────────────────────
        Q("The Body", "Which organ pumps blood around your body?", "Lungs", "Heart", "Liver", "Brain", AnswerLabel.B, Difficulty.Easy),
        Q("The Body", "How many bones does an adult human have?", "106", "186", "206", "306", AnswerLabel.C, Difficulty.Hard),
        Q("The Body", "Which organ do you use to breathe?", "Kidneys", "Lungs", "Stomach", "Heart", AnswerLabel.B, Difficulty.Easy),
        Q("The Body", "What part of blood carries oxygen?", "White cells", "Platelets", "Red cells", "Plasma", AnswerLabel.C, Difficulty.Medium),
        Q("The Body", "Which sense organ detects light?", "Ear", "Nose", "Skin", "Eye", AnswerLabel.D, Difficulty.Easy),
        Q("The Body", "What is the largest organ of the human body?", "Liver", "Skin", "Brain", "Lungs", AnswerLabel.B, Difficulty.Medium),

        // ── LIVING WORLD — animals & plants ──────────────────────────────────
        Q("Living World", "How many legs does an insect have?", "4", "6", "8", "10", AnswerLabel.B, Difficulty.Easy),
        Q("Living World", "Which gas do plants take in to make food?", "Oxygen", "Nitrogen", "Carbon dioxide", "Helium", AnswerLabel.C, Difficulty.Medium),
        Q("Living World", "What do we call an animal that eats only plants?", "Carnivore", "Herbivore", "Omnivore", "Predator", AnswerLabel.B, Difficulty.Easy),
        Q("Living World", "Which is the largest living animal?", "African elephant", "Blue whale", "Giraffe", "Great white shark", AnswerLabel.B, Difficulty.Easy),
        Q("Living World", "What process do plants use to make food from sunlight?", "Respiration", "Digestion", "Photosynthesis", "Evaporation", AnswerLabel.C, Difficulty.Medium),
        Q("Living World", "A tadpole grows up to become a…?", "Fish", "Frog", "Lizard", "Snail", AnswerLabel.B, Difficulty.Easy),
        Q("Living World", "Which of these is a mammal?", "Shark", "Penguin", "Dolphin", "Crocodile", AnswerLabel.C, Difficulty.Medium),

        // ── SPACE ────────────────────────────────────────────────────────────
        Q("Space", "Which planet do we live on?", "Mars", "Venus", "Earth", "Jupiter", AnswerLabel.C, Difficulty.Easy),
        Q("Space", "What is at the centre of our solar system?", "The Moon", "The Sun", "Earth", "Jupiter", AnswerLabel.B, Difficulty.Easy),
        Q("Space", "Which planet is known as the Red Planet?", "Venus", "Mars", "Mercury", "Saturn", AnswerLabel.B, Difficulty.Easy),
        Q("Space", "Which is the largest planet in our solar system?", "Saturn", "Neptune", "Jupiter", "Earth", AnswerLabel.C, Difficulty.Medium),
        Q("Space", "What do we call a rock that lands on Earth from space?", "Comet", "Asteroid", "Meteorite", "Satellite", AnswerLabel.C, Difficulty.Hard),
        Q("Space", "Which planet is closest to the Sun?", "Venus", "Mercury", "Mars", "Earth", AnswerLabel.B, Difficulty.Medium),

        // ── EVERYDAY SCIENCE — physics you can feel ──────────────────────────
        Q("Everyday Science", "What force pulls objects toward the ground?", "Magnetism", "Friction", "Gravity", "Tension", AnswerLabel.C, Difficulty.Easy),
        Q("Everyday Science", "At what temperature does water freeze (Celsius)?", "0°C", "10°C", "32°C", "100°C", AnswerLabel.A, Difficulty.Easy),
        Q("Everyday Science", "At what temperature does water boil at sea level (Celsius)?", "50°C", "90°C", "100°C", "120°C", AnswerLabel.C, Difficulty.Easy),
        Q("Everyday Science", "What do we call water in its gas form?", "Ice", "Steam", "Frost", "Dew", AnswerLabel.B, Difficulty.Easy),
        Q("Everyday Science", "Which travels faster?", "Sound", "Light", "They're equal", "Neither moves", AnswerLabel.B, Difficulty.Medium),
        Q("Everyday Science", "What simple machine is a see-saw an example of?", "Pulley", "Lever", "Wheel", "Screw", AnswerLabel.B, Difficulty.Hard),

        // ── ELEMENTS — basic chemistry ───────────────────────────────────────
        Q("Elements", "What gas do humans need to breathe to live?", "Carbon dioxide", "Oxygen", "Hydrogen", "Nitrogen", AnswerLabel.B, Difficulty.Easy),
        Q("Elements", "Water is made of hydrogen and which other element?", "Carbon", "Oxygen", "Nitrogen", "Helium", AnswerLabel.B, Difficulty.Medium),
        Q("Elements", "What is the chemical symbol for gold?", "Go", "Gd", "Au", "Ag", AnswerLabel.C, Difficulty.Hard),
        Q("Elements", "Which gas makes up most of the air we breathe?", "Oxygen", "Nitrogen", "Carbon dioxide", "Argon", AnswerLabel.B, Difficulty.Extreme),
        Q("Elements", "Diamond and pencil 'lead' are both made mostly of which element?", "Silicon", "Iron", "Carbon", "Calcium", AnswerLabel.C, Difficulty.Hard),
    ];

    private static ICard Q(string cat, string question, string a, string b, string c, string d, AnswerLabel correct, Difficulty diff) =>
        MultipleChoiceCard.Create(question, a, b, c, d, correct, diff, cat);
}
