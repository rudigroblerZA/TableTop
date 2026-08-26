using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Animal Kingdom — general-knowledge nature for the classroom, focused on the
/// animal world: records and superlatives, baby animals and animal groups,
/// habitats, and clever adaptations, as multiple-choice questions dealt one at
/// a time.
///
/// Kept distinct from Science Sprint's "Living World" (which teaches the
/// biology basics — mammals vs not, photosynthesis): this deck is the
/// crowd-pleasing animal-facts kind — what a group of lions is called, which
/// animal is fastest, what a baby kangaroo is — the trivia kids love.
/// </summary>
public sealed class AnimalKingdomMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name        => "Animal Kingdom";
    /// <inheritdoc />
    public override string Description =>
        "Nature & animals general knowledge — record-breakers, baby animals, animal groups, habitats, and adaptations. Multiple choice.";

    /// <summary>Label for a correctly answered card.</summary>
    public override string CompleteLabel => "Correct";
    /// <summary>Label for a passed card.</summary>
    public override string SkipLabel     => "Pass";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Record Breakers"] = "#EF5350",
            ["Baby Animals"]    = "#FFB74D",
            ["Animal Groups"]   = "#AB47BC",
            ["Habitats"]        = "#66BB6A",
            ["Adaptations"]     = "#26A69A",
        };

    /// <summary>Harder questions score more.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in Animal Kingdom card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        AnimalKingdomCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => AnimalKingdomCardBank.All;
}

/// <summary>Built-in card bank for Animal Kingdom.</summary>
public static class AnimalKingdomCardBank
{
    /// <summary>All cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── RECORD BREAKERS ──────────────────────────────────────────────────
        Q("Record Breakers", "What is the fastest land animal?", "Lion", "Cheetah", "Horse", "Greyhound", AnswerLabel.B, Difficulty.Easy),
        Q("Record Breakers", "What is the tallest animal in the world?", "Elephant", "Giraffe", "Ostrich", "Camel", AnswerLabel.B, Difficulty.Easy),
        Q("Record Breakers", "What is the largest animal that has ever lived?", "African elephant", "Blue whale", "Dinosaur", "Great white shark", AnswerLabel.B, Difficulty.Medium),
        Q("Record Breakers", "Which bird is the largest in the world?", "Eagle", "Albatross", "Ostrich", "Emu", AnswerLabel.C, Difficulty.Medium),
        Q("Record Breakers", "Which is the smallest bird in the world?", "Sparrow", "Hummingbird", "Wren", "Finch", AnswerLabel.B, Difficulty.Hard),
        Q("Record Breakers", "Which animal has the longest lifespan, sometimes over 150 years?", "Elephant", "Giant tortoise", "Parrot", "Whale", AnswerLabel.B, Difficulty.Hard),

        // ── BABY ANIMALS ─────────────────────────────────────────────────────
        Q("Baby Animals", "What is a baby dog called?", "Kit", "Puppy", "Cub", "Calf", AnswerLabel.B, Difficulty.Easy),
        Q("Baby Animals", "What is a baby cat called?", "Cub", "Kit", "Kitten", "Pup", AnswerLabel.C, Difficulty.Easy),
        Q("Baby Animals", "What is a baby kangaroo called?", "Joey", "Calf", "Cub", "Fawn", AnswerLabel.A, Difficulty.Medium),
        Q("Baby Animals", "What is a baby cow called?", "Colt", "Calf", "Foal", "Kid", AnswerLabel.B, Difficulty.Easy),
        Q("Baby Animals", "What is a baby horse called?", "Calf", "Kid", "Foal", "Lamb", AnswerLabel.C, Difficulty.Medium),
        Q("Baby Animals", "What is a baby frog called before it grows legs?", "Fry", "Tadpole", "Nymph", "Larva", AnswerLabel.B, Difficulty.Easy),

        // ── ANIMAL GROUPS ────────────────────────────────────────────────────
        Q("Animal Groups", "What do you call a group of lions?", "Pack", "Pride", "Herd", "Flock", AnswerLabel.B, Difficulty.Medium),
        Q("Animal Groups", "What do you call a group of wolves?", "Pride", "Pack", "School", "Colony", AnswerLabel.B, Difficulty.Medium),
        Q("Animal Groups", "What do you call a group of fish?", "Flock", "Herd", "School", "Pack", AnswerLabel.C, Difficulty.Easy),
        Q("Animal Groups", "What do you call a group of cows?", "Herd", "Pack", "Pride", "Swarm", AnswerLabel.A, Difficulty.Easy),
        Q("Animal Groups", "What do you call a group of crows?", "A murder", "A gaggle", "A parade", "A pod", AnswerLabel.A, Difficulty.Hard),
        Q("Animal Groups", "What do you call a group of whales?", "Herd", "Pod", "School", "Colony", AnswerLabel.B, Difficulty.Hard),

        // ── HABITATS ─────────────────────────────────────────────────────────
        Q("Habitats", "Which animal lives in the Arctic and has thick white fur?", "Brown bear", "Polar bear", "Panda", "Sloth bear", AnswerLabel.B, Difficulty.Easy),
        Q("Habitats", "Camels are especially suited to living in the…?", "Rainforest", "Ocean", "Desert", "Mountains", AnswerLabel.C, Difficulty.Easy),
        Q("Habitats", "In which habitat would you naturally find a clownfish?", "River", "Coral reef", "Cave", "Desert", AnswerLabel.B, Difficulty.Medium),
        Q("Habitats", "Which of these animals lives in the ocean its whole life?", "Turtle", "Penguin", "Dolphin", "Seal", AnswerLabel.C, Difficulty.Medium),
        Q("Habitats", "Giant pandas naturally live in the wild in which country?", "Japan", "China", "India", "Nepal", AnswerLabel.B, Difficulty.Medium),

        // ── ADAPTATIONS ──────────────────────────────────────────────────────
        Q("Adaptations", "Which animal can change the colour of its skin to blend in?", "Chameleon", "Zebra", "Tiger", "Frog", AnswerLabel.A, Difficulty.Medium),
        Q("Adaptations", "What is the main purpose of a camel's hump?", "Storing water", "Storing fat for energy", "Balance", "Keeping cool", AnswerLabel.B, Difficulty.Hard),
        Q("Adaptations", "How does an octopus escape from danger?", "Squirting ink", "Playing dead", "Flying", "Digging", AnswerLabel.A, Difficulty.Medium),
        Q("Adaptations", "Bats find their way in the dark mainly using…?", "Night vision", "Smell", "Echolocation (sound)", "Whiskers", AnswerLabel.C, Difficulty.Hard),
        Q("Adaptations", "Why do many Arctic animals have white fur?", "To stay warm", "For camouflage in snow", "To attract mates", "To scare predators", AnswerLabel.B, Difficulty.Extreme),
    ];

    private static ICard Q(string cat, string question, string a, string b, string c, string d, AnswerLabel correct, Difficulty diff) =>
        MultipleChoiceCard.Create(question, a, b, c, d, correct, diff, cat);
}
