using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Domain.Cards;

namespace TableTop.Games.School;

/// <summary>
/// 80 curriculum-aligned multiple-choice questions for Grade 6 (age 11–12).
/// Covers: English Language Arts, Mathematics, Science, History/Geography,
/// and General Knowledge appropriate for a classroom setting.
///
/// Difficulty mapping to prize ladder:
///   Easy    → Q1–Q5   (£100–£1,000)     — KS2 recall and basic understanding
///   Medium  → Q6–Q10  (£2,000–£32,000)  — KS3 application and comprehension
///   Hard    → Q11–Q14 (£64,000–£500,000) — reasoning and multi-step problems
///   Extreme → Q15     (£1,000,000)       — stretch/challenge questions
/// </summary>
public static class Grade6QuestionBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<MultipleChoiceCard> All { get; } = Build();

    private static IReadOnlyList<MultipleChoiceCard> Build() =>
    [
        // ════════════════════════════════════════════════════════════════════
        // EASY — Recall and basic understanding (Grade 5–6 level)
        // ════════════════════════════════════════════════════════════════════

        // English
        MultipleChoiceCard.Create(
            "Which of these is a noun?",
            "Run", "Quickly", "Happiness", "Under",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What does an adjective do?",
            "Describes a verb", "Names a person or place",
            "Describes a noun", "Shows an action",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which sentence uses a comma correctly?",
            "I like apples, and bananas.",
            "I like, apples and bananas.",
            "I, like apples and bananas.",
            "I like apples and, bananas.",
            AnswerLabel.A, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What is the plural of 'leaf'?",
            "Leafs", "Leaves", "Leafe", "Leavs",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which word is a synonym for 'happy'?",
            "Sad", "Joyful", "Angry", "Tired",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which is a complete sentence?",
            "Running fast through the park.",
            "The dog barked loudly.",
            "After the long and sunny day.",
            "Because she was hungry.",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What does the prefix 'un-' mean?",
            "Again", "Before", "Not", "With",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which punctuation mark ends a question?",
            "Full stop", "Exclamation mark", "Comma", "Question mark",
            AnswerLabel.D, Difficulty.Easy),

        // Maths
        MultipleChoiceCard.Create(
            "What is 15% of 200?",
            "25", "30", "35", "40",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which of these is a prime number?",
            "9", "15", "17", "21",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What is the area of a rectangle 8cm wide and 5cm tall?",
            "26 cm²", "40 cm²", "13 cm²", "30 cm²",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What is ½ + ¼?",
            "¾", "⅔", "⅗", "1",
            AnswerLabel.A, Difficulty.Easy),

        // Science
        MultipleChoiceCard.Create(
            "What gas do plants take in during photosynthesis?",
            "Oxygen", "Nitrogen", "Carbon dioxide", "Hydrogen",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What is the boiling point of water at sea level?",
            "90°C", "95°C", "100°C", "110°C",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What force pulls objects toward Earth?",
            "Magnetism", "Friction", "Gravity", "Tension",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "How many bones are in the adult human body?",
            "106", "156", "206", "256",
            AnswerLabel.C, Difficulty.Easy),

        // History/Geography
        MultipleChoiceCard.Create(
            "What is the capital of France?",
            "Lyon", "Marseille", "Paris", "Nice",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "On which continent is the Amazon rainforest found?",
            "Africa", "Asia", "Australia", "South America",
            AnswerLabel.D, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Who was the first President of the United States?",
            "Abraham Lincoln", "George Washington",
            "Thomas Jefferson", "John Adams",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What is the largest ocean on Earth?",
            "Atlantic", "Indian", "Pacific", "Arctic",
            AnswerLabel.C, Difficulty.Easy),

        // ════════════════════════════════════════════════════════════════════
        // MEDIUM — Application and comprehension
        // ════════════════════════════════════════════════════════════════════

        // English
        MultipleChoiceCard.Create(
            "What literary device is used in 'The wind whispered through the trees'?",
            "Simile", "Metaphor", "Personification", "Alliteration",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which sentence is written in the past tense?",
            "She runs every morning.",
            "She will run tomorrow.",
            "She ran five miles yesterday.",
            "She is running right now.",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the main purpose of a topic sentence?",
            "To end a paragraph", "To introduce the main idea of a paragraph",
            "To give an example", "To summarise the whole essay",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which word is an antonym of 'ancient'?",
            "Old", "Historic", "Modern", "Traditional",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the correct spelling?",
            "Recieve", "Receive", "Receeve", "Recive",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What does the suffix '-tion' typically create?",
            "An adjective", "An adverb", "A noun", "A verb",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "In the sentence 'She ran quickly', what part of speech is 'quickly'?",
            "Noun", "Adjective", "Verb", "Adverb",
            AnswerLabel.D, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which of these is a compound sentence?",
            "I went to school.",
            "I went to school because I wanted to learn.",
            "I went to school, and I learned a lot.",
            "Although I was tired, I went to school.",
            AnswerLabel.C, Difficulty.Medium),

        // Maths
        MultipleChoiceCard.Create(
            "What is the LCM of 4 and 6?",
            "2", "8", "12", "24",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "A bag contains 3 red, 5 blue, and 2 green balls. What is the probability of picking a blue ball?",
            "1/2", "5/10", "1/5", "3/10",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is 3.6 × 100?",
            "36", "360", "3600", "0.036",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Solve for x: 3x + 7 = 22",
            "x = 3", "x = 5", "x = 7", "x = 9",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the volume of a cube with sides of 4cm?",
            "16 cm³", "48 cm³", "64 cm³", "96 cm³",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which fraction is equivalent to 0.75?",
            "1/4", "2/3", "3/4", "4/5",
            AnswerLabel.C, Difficulty.Medium),

        // Science
        MultipleChoiceCard.Create(
            "What is the chemical symbol for gold?",
            "Go", "Gd", "Au", "Ag",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What type of rock is formed from cooled lava?",
            "Sedimentary", "Metamorphic", "Igneous", "Limestone",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which organ in the human body produces insulin?",
            "Liver", "Kidney", "Pancreas", "Stomach",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the speed of light approximately?",
            "100,000 km/s", "200,000 km/s",
            "300,000 km/s", "400,000 km/s",
            AnswerLabel.C, Difficulty.Medium),

        // History/Geography
        MultipleChoiceCard.Create(
            "In which year did World War I begin?",
            "1912", "1914", "1916", "1918",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the longest river in the world?",
            "Amazon", "Mississippi", "Nile", "Yangtze",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What mountain range separates Europe from Asia?",
            "The Alps", "The Himalayas", "The Andes", "The Urals",
            AnswerLabel.D, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Who wrote 'Romeo and Juliet'?",
            "Charles Dickens", "William Shakespeare",
            "Jane Austen", "Geoffrey Chaucer",
            AnswerLabel.B, Difficulty.Medium),

        // ════════════════════════════════════════════════════════════════════
        // HARD — Reasoning and multi-step problems
        // ════════════════════════════════════════════════════════════════════

        // English
        MultipleChoiceCard.Create(
            "What is the difference between a simile and a metaphor?",
            "A simile compares using 'like' or 'as'; a metaphor says one thing IS another",
            "A metaphor uses 'like' or 'as'; a simile is a direct comparison",
            "Both are exactly the same figure of speech",
            "A simile is a type of metaphor",
            AnswerLabel.A, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Which of these sentences contains a subordinate clause?",
            "I like dogs and cats.",
            "She went to the shop.",
            "Although it was raining, we played outside.",
            "The tall, old tree fell down.",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the correct use of a semicolon?",
            "To separate items in a simple list",
            "To join two closely related independent clauses",
            "To introduce a quotation",
            "To show ownership",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Which of these words contains a silent letter?",
            "Bring", "Clock", "Knight", "Step",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is an oxymoron?",
            "A word that sounds like what it means",
            "Two contradictory terms placed together",
            "A comparison using 'like' or 'as'",
            "A word with the same spelling but different meanings",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "In 'The Great Gatsby', what does the green light symbolise?",
            "Money and wealth",
            "Gatsby's jealousy",
            "Daisy and the unattainable dream",
            "The American natural landscape",
            AnswerLabel.C, Difficulty.Hard),

        // Maths
        MultipleChoiceCard.Create(
            "What is the sum of the interior angles of a hexagon?",
            "540°", "720°", "900°", "1080°",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "A train travels at 80 km/h. How long does it take to travel 240 km?",
            "2 hours", "2.5 hours", "3 hours", "3.5 hours",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the square root of 169?",
            "11", "12", "13", "14",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "If a shirt costs £45 after a 25% discount, what was the original price?",
            "£55", "£56.25", "£60", "£67.50",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the HCF of 36 and 48?",
            "6", "8", "12", "18",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "A circle has a radius of 7cm. What is its area? (π ≈ 3.14)",
            "43.96 cm²", "87.92 cm²", "153.86 cm²", "44 cm²",
            AnswerLabel.C, Difficulty.Hard),

        // Science
        MultipleChoiceCard.Create(
            "What is the function of mitochondria in a cell?",
            "To store genetic material",
            "To produce proteins",
            "To produce energy (ATP)",
            "To control what enters and leaves the cell",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Which of Newton's laws states that every action has an equal and opposite reaction?",
            "First law", "Second law", "Third law", "Fourth law",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What happens during meiosis that doesn't happen during mitosis?",
            "DNA replication",
            "Cell division",
            "Chromosome number is halved",
            "Nucleus divides",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the atomic number of carbon?",
            "2", "4", "6", "8",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What type of wave is sound?",
            "Transverse", "Longitudinal", "Electromagnetic", "Seismic",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the process by which plants lose water through their leaves?",
            "Osmosis", "Diffusion", "Transpiration", "Evaporation",
            AnswerLabel.C, Difficulty.Hard),

        // ════════════════════════════════════════════════════════════════════
        // EXTREME — Stretch/challenge questions
        // ════════════════════════════════════════════════════════════════════

        MultipleChoiceCard.Create(
            "What is the value of Pi to 5 decimal places?",
            "3.14159", "3.14256", "3.14128", "3.14193",
            AnswerLabel.A, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "In grammar, what is a 'dangling modifier'?",
            "A clause that modifies the wrong noun",
            "A modifier placed after the word it modifies",
            "An unnecessary word in a sentence",
            "A modifier that has no clear subject to modify",
            AnswerLabel.D, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the chemical formula for glucose?",
            "C6H12O6", "C12H22O11", "CH4", "H2O",
            AnswerLabel.A, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "Who proposed the heliocentric model of the solar system?",
            "Galileo Galilei", "Isaac Newton",
            "Nicolaus Copernicus", "Johannes Kepler",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the sum of angles in a triangle that has one angle of 90° and another of 37°?",
            "180°", "The third angle is 53°",
            "The third angle is 43°", "The third angle is 63°",
            AnswerLabel.B, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "Which of these is NOT a prime number?",
            "97", "89", "91", "83",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What literary term describes when a narrator addresses the reader directly as 'you'?",
            "Third person", "First person", "Second person", "Omniscient narrator",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the Pythagorean theorem?",
            "a² + b² = c²", "a + b = c",
            "a² × b² = c²", "a² − b² = c",
            AnswerLabel.A, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What organ produces bile to aid digestion?",
            "Stomach", "Pancreas", "Liver", "Small intestine",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "In which century did the Renaissance begin in Italy?",
            "12th", "13th", "14th", "15th",
            AnswerLabel.C, Difficulty.Extreme),
    ];
}