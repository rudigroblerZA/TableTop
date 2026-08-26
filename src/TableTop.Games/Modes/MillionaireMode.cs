using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Domain.Cards;

namespace TableTop.Games;

/// <summary>
/// Who Wants to Be a Millionaire? game mode identity.
///
/// Questions come from the in-code <see cref="MillionaireQuestionBank"/>. They were
/// read from <c>Data/Json/millionaire-questions.deck.json</c> first until 1.19.0,
/// with the bank as a fallback; the deck files and the resolver are both gone.
/// </summary>
public sealed class MillionaireMode : IGameMode, IQuestionBankProvider
{
    /// <inheritdoc />
    public string Name => "Who Wants to Be a Millionaire?";

    /// <inheritdoc />
    public string Description =>
        "Hot-seat quiz. Climb 15 questions to £1,000,000. Three lifelines. One wrong answer ends your run.";

    /// <inheritdoc />
    public IReadOnlyList<MultipleChoiceCard> GetQuestionBank() => MillionaireQuestionBank.All;
}

/// <summary>
/// 60 trivia questions spanning the four difficulty tiers.
/// In production these would be loaded from a JSON file or database.
/// </summary>
public static class MillionaireQuestionBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<MultipleChoiceCard> All { get; } = Build();

    private static IReadOnlyList<MultipleChoiceCard> Build() =>
    [
        // ════════════════════════════════════════════════════════════════════
        // EASY  (Questions 1–5 on the ladder) — 30 questions
        // ════════════════════════════════════════════════════════════════════
        MultipleChoiceCard.Create(
            "What colour is the sky on a clear day?",
            "Green", "Blue", "Red", "Yellow",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "How many days are in a standard week?",
            "5", "6", "7", "8",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which animal is known as man's best friend?",
            "Cat", "Rabbit", "Hamster", "Dog",
            AnswerLabel.D, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What is 2 + 2?",
            "3", "5", "22", "4",
            AnswerLabel.D, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which planet do we live on?",
            "Mars", "Venus", "Earth", "Jupiter",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What is the boiling point of water at sea level in Celsius?",
            "90°C", "110°C", "100°C", "80°C",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "How many sides does a triangle have?",
            "4", "3", "5", "6",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What is the capital city of France?",
            "Berlin", "Madrid", "Rome", "Paris",
            AnswerLabel.D, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which of these is a fruit?",
            "Carrot", "Potato", "Apple", "Broccoli",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What do bees produce?",
            "Milk", "Honey", "Wax only", "Vinegar",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "How many months are in a year?",
            "10", "11", "13", "12",
            AnswerLabel.D, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which ocean is the largest?",
            "Atlantic", "Indian", "Arctic", "Pacific",
            AnswerLabel.D, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What colour are Mickey Mouse's gloves?",
            "Yellow", "Red", "White", "Black",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "How many legs does a spider have?",
            "6", "8", "10", "12",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What is the smallest country in the world?",
            "Monaco", "Luxembourg", "Vatican City", "San Marino",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which sport is played on a diamond?",
            "Cricket", "Baseball", "Lacrosse", "American Football",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What colour is a penguin's back?",
            "White", "Grey", "Black", "Brown",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What do you call a baby dog?",
            "Colt", "Cub", "Puppy", "Foal",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "How many wheels does a bicycle have?",
            "1", "2", "3", "4",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which fruit is yellow on the outside?",
            "Strawberry", "Banana", "Blueberry", "Raspberry",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What do butterflies drink?",
            "Milk", "Water", "Nectar", "Blood",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "How many stars are on the USA flag?",
            "48", "50", "52", "45",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which country makes pasta a main staple?",
            "Spain", "Italy", "Greece", "Portugal",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What is a baby cat called?",
            "Kitten", "Kitty", "Calf", "Kite",
            AnswerLabel.A, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "How many fingers do humans have on both hands?",
            "8", "10", "12", "14",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What colour is a stop sign?",
            "Yellow", "Red", "Blue", "Orange",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "Which is the largest island in the world?",
            "Australia", "Greenland", "Borneo", "Great Britain",
            AnswerLabel.B, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "How many strings does a standard violin have?",
            "4", "5", "6", "8",
            AnswerLabel.A, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "What do you ride to travel on water?",
            "Bike", "Skateboard", "Boat", "Sled",
            AnswerLabel.C, Difficulty.Easy),

        MultipleChoiceCard.Create(
            "How many seconds are in a minute?",
            "30", "60", "90", "120",
            AnswerLabel.B, Difficulty.Easy),

        // ════════════════════════════════════════════════════════════════════
        // MEDIUM  (Questions 6–10 on the ladder) — 45 questions
        // ════════════════════════════════════════════════════════════════════
        MultipleChoiceCard.Create(
            "What is the chemical symbol for gold?",
            "Gd", "Go", "Ag", "Au",
            AnswerLabel.D, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Who painted the Mona Lisa?",
            "Michelangelo", "Raphael", "Leonardo da Vinci", "Caravaggio",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "How many strings does a standard guitar have?",
            "4", "5", "6", "7",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the largest continent by area?",
            "Africa", "North America", "Asia", "Europe",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "In which year did World War II end?",
            "1943", "1944", "1945", "1946",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the speed of light approximately?",
            "150,000 km/s", "300,000 km/s", "450,000 km/s", "600,000 km/s",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which element has the atomic number 1?",
            "Helium", "Oxygen", "Hydrogen", "Carbon",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the largest organ in the human body?",
            "Liver", "Brain", "Heart", "Skin",
            AnswerLabel.D, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "How many players are on a standard football (soccer) team?",
            "9", "10", "11", "12",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which country gifted the Statue of Liberty to the USA?",
            "United Kingdom", "France", "Germany", "Spain",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the square root of 144?",
            "11", "12", "13", "14",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which gas makes up the largest percentage of Earth's atmosphere?",
            "Oxygen", "Carbon dioxide", "Nitrogen", "Argon",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Who wrote 'Romeo and Juliet'?",
            "Charles Dickens", "Jane Austen", "William Shakespeare", "Geoffrey Chaucer",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What currency is used in Japan?",
            "Yuan", "Won", "Baht", "Yen",
            AnswerLabel.D, Difficulty.Medium),

        // ── Medium Pop Culture & Entertainment ──
        MultipleChoiceCard.Create(
            "How many Harry Potter books are there in the main series?",
            "5", "6", "7", "8",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Who directed the first Jurassic Park film?",
            "James Cameron", "Steven Spielberg", "George Lucas", "Peter Jackson",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What year was the first iPhone released?",
            "2005", "2006", "2007", "2008",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "In which country was the musical 'Hamilton' originated?",
            "Canada", "United Kingdom", "United States", "Australia",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "How many Academy Awards did 'Titanic' win?",
            "13", "14", "15", "16",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the most-spoken language in the world?",
            "English", "Spanish", "Mandarin Chinese", "Hindi",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which artist painted The Starry Night?",
            "Pablo Picasso", "Vincent van Gogh", "Salvador Dalí", "Marc Chagall",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "How many sides does a hexagon have?",
            "5", "6", "7", "8",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the capital of Australia?",
            "Sydney", "Melbourne", "Canberra", "Brisbane",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which planet is known as the Red Planet?",
            "Venus", "Mars", "Jupiter", "Saturn",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the currency of the United Kingdom?",
            "Euro", "Pound Sterling", "Crown", "Franc",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Who wrote 'The Great Gatsby'?",
            "Ernest Hemingway", "F. Scott Fitzgerald", "Mark Twain", "Sinclair Lewis",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the smallest bone in the human body?",
            "Stapes", "Malleus", "Incus", "Femur",
            AnswerLabel.A, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "How many continents are there?",
            "5", "6", "7", "8",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What does DNA stand for?",
            "Deoxyribonucleic Acid", "Digital Nucleic Acid", "Deoxyribose Nucleic Acid", "Dynamic Nucleic Acid",
            AnswerLabel.A, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which painting shows a woman with an unusual smile?",
            "Girl with a Pearl Earring", "Mona Lisa", "Nighthawks", "American Gothic",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the most populous country in the world?",
            "India", "China", "USA", "Indonesia",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "How many sides does a Pentagon have?",
            "5", "6", "7", "8",
            AnswerLabel.A, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What type of animal is a dolphin?",
            "Fish", "Amphibian", "Mammal", "Reptile",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which composer wrote 'Fur Elise'?",
            "Mozart", "Beethoven", "Chopin", "Debussy",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the only mammal that can't jump?",
            "Sloth", "Hippopotamus", "Elephant", "Whale",
            AnswerLabel.D, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "How many minutes are in a full day?",
            "1,200", "1,440", "1,680", "1,800",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the deepest ocean trench?",
            "Java Trench", "Mariana Trench", "Tonga Trench", "Philippine Trench",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which country won the FIFA World Cup in 2018?",
            "Germany", "France", "Brazil", "Argentina",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the smallest state in the USA?",
            "Vermont", "Delaware", "Wyoming", "Rhode Island",
            AnswerLabel.D, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "How many rings does Saturn have?",
            "5", "7", "9", "More than 40",
            AnswerLabel.D, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What do you call someone who studies rocks?",
            "Botanist", "Geologist", "Paleontologist", "Meteorologist",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "Which ocean borders Africa to the east?",
            "Atlantic", "Arctic", "Indian", "Southern",
            AnswerLabel.C, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "What is the second-largest planet in our solar system?",
            "Jupiter", "Saturn", "Neptune", "Uranus",
            AnswerLabel.B, Difficulty.Medium),

        MultipleChoiceCard.Create(
            "How many colors are in a rainbow?",
            "5", "6", "7", "8",
            AnswerLabel.C, Difficulty.Medium),

        // ════════════════════════════════════════════════════════════════════
        // HARD  (Questions 11–14 on the ladder) — 35 questions
        // ════════════════════════════════════════════════════════════════════
        MultipleChoiceCard.Create(
            "In which year was the Magna Carta signed?",
            "1215", "1315", "1415", "1515",
            AnswerLabel.A, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the powerhouse of the cell?",
            "Nucleus", "Ribosome", "Mitochondria", "Golgi apparatus",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Which country has the most natural lakes?",
            "Russia", "Brazil", "USA", "Canada",
            AnswerLabel.D, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the hardest natural substance on Earth?",
            "Graphene", "Quartz", "Diamond", "Corundum",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "How many bones are in the adult human body?",
            "196", "206", "216", "226",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Which planet has the most moons?",
            "Jupiter", "Saturn", "Uranus", "Neptune",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the only metal that is liquid at room temperature?",
            "Gallium", "Caesium", "Mercury", "Francium",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "In what year did the Berlin Wall fall?",
            "1987", "1988", "1989", "1990",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the longest river in the world?",
            "Amazon", "Yangtze", "Mississippi", "Nile",
            AnswerLabel.D, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Which mathematician is famous for the incompleteness theorems?",
            "Alan Turing", "Bertrand Russell", "Kurt Gödel", "David Hilbert",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the chemical formula for table salt?",
            "KCl", "NaCl", "MgCl₂", "CaCl₂",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Who was the first woman to win a Nobel Prize?",
            "Rosalind Franklin", "Marie Curie", "Dorothy Hodgkin", "Lise Meitner",
            AnswerLabel.B, Difficulty.Hard),

        // ── Hard Arts & Culture ──
        MultipleChoiceCard.Create(
            "Which novel won the Booker Prize in 2023?",
            "Lessons", "The Heaven & Earth Grocery Store", "The Employees", "Translation State",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the smallest musical interval used in Western music?",
            "Whole tone", "Semitone", "Quarter tone", "Microtone",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "In which city is the Louvre Museum located?",
            "Rome", "London", "Paris", "Berlin",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What year was the internet publically released?",
            "1989", "1991", "1993", "1995",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Which element did Marie Curie discover twice?",
            "Polonium", "Radium", "Thorium", "Uranium",
            AnswerLabel.A, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the most abundant element in the universe?",
            "Helium", "Hydrogen", "Oxygen", "Carbon",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "How many strings does a harp typically have?",
            "22", "47", "46", "23",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "In which century was the printing press invented?",
            "13th", "14th", "15th", "16th",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the capital of Kazakhstan?",
            "Almaty", "Bishkek", "Astana", "Karaganda",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Which physicist won two Nobel Prizes?",
            "Albert Einstein", "Niels Bohr", "Marie Curie", "Max Planck",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What type of animal is a seahorse?",
            "Mammal", "Amphibian", "Fish", "Reptile",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Which nation is the world's largest exporter of coffee?",
            "Colombia", "Vietnam", "Brazil", "Indonesia",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the rarest blood type in humans?",
            "AB-negative", "O-negative", "Rh-null", "B-negative",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "How many chambers does a cow's stomach have?",
            "2", "3", "4", "5",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the most spoken language in Europe by natives?",
            "English", "French", "German", "Russian",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Which ancient wonder of the world still stands?",
            "Hanging Gardens", "Great Pyramid", "Colossus of Rhodes", "Pharos of Alexandria",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the largest living structure on Earth?",
            "Amazon Rainforest", "Great Barrier Reef", "Siberian Taiga", "Congo Rainforest",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "Which country has the most UNESCO World Heritage sites?",
            "France", "China", "Italy", "Germany",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the most dense element on Earth?",
            "Tungsten", "Platinum", "Osmium", "Iridium",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "In which country is the ancient city of Angkor Wat located?",
            "Thailand", "Vietnam", "Cambodia", "Laos",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "How long is the Great Wall of China approximately?",
            "5,000 km", "13,000 km", "21,000 km", "30,000 km",
            AnswerLabel.C, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What year did the Titanic sink?",
            "1911", "1912", "1913", "1914",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "What is the largest artery in the human body?",
            "Pulmonary artery", "Aorta", "Carotid artery", "Femoral artery",
            AnswerLabel.B, Difficulty.Hard),

        MultipleChoiceCard.Create(
            "How many times the size of Earth is the Sun?",
            "99 times", "109 times", "119 times", "129 times",
            AnswerLabel.B, Difficulty.Hard),

        // ════════════════════════════════════════════════════════════════════
        // EXTREME  (Question 15 — the £1,000,000 question) — 20 questions
        // ════════════════════════════════════════════════════════════════════
        MultipleChoiceCard.Create(
            "What is the only number that is both a Fibonacci number and a perfect square greater than 1, other than 144?",
            "34", "55", "89", "There is none",
            AnswerLabel.D, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "Which element was the first to be discovered using spectroscopy?",
            "Helium", "Caesium", "Rubidium", "Thallium",
            AnswerLabel.A, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "In what year was the first programmable electronic computer (Colossus) operational?",
            "1941", "1943", "1944", "1946",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "Which philosopher first used the term 'tabula rasa' in its modern philosophical sense?",
            "Descartes", "Hume", "Locke", "Kant",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the approximate diameter of a hydrogen atom in picometres?",
            "53 pm", "106 pm", "212 pm", "26 pm",
            AnswerLabel.B, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "Which treaty formally ended the Thirty Years' War?",
            "Treaty of Utrecht", "Peace of Westphalia", "Treaty of Vienna", "Peace of Augsburg",
            AnswerLabel.B, Difficulty.Extreme),

        // ── More Extreme Questions ──
        MultipleChoiceCard.Create(
            "What is the Planck length in metres?",
            "1.616 × 10⁻³⁵", "1.616 × 10⁻³⁰", "1.616 × 10⁻⁴⁰", "1.616 × 10⁻²⁵",
            AnswerLabel.A, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "Which artist painted The Garden of Earthly Delights?",
            "Jan van Eyck", "Hieronymus Bosch", "Peter Bruegel", "Dirk Bouts",
            AnswerLabel.B, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the second law of thermodynamics formally called?",
            "Entropy Law", "Law of Entropy Increase", "Second Law of Entropy", "Arrow of Time",
            AnswerLabel.B, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "In which year did Gutenberg invent the printing press?",
            "1440", "1445", "1450", "1455",
            AnswerLabel.A, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the only prime number that is also even?",
            "0", "1", "2", "4",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "Which mathematician developed non-Euclidean geometry?",
            "Gauss", "Lobachevsky", "Riemann", "All of the above",
            AnswerLabel.D, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the Avogadro constant approximately?",
            "6.022 x 10^23", "6.022 x 10^24", "6.022 x 10^22", "6.022 x 10^25",
            AnswerLabel.A, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "In what year did Gödel publish his incompleteness theorems?",
            "1929", "1930", "1931", "1932",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the name of the mathematical constant equal to 2.71828...?",
            "Phi", "Euler's number", "Pi", "Golden ratio",
            AnswerLabel.B, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "Which physicist proposed the concept of black holes?",
            "Karl Schwarzschild", "John Wheeler", "Roger Penrose", "Stephen Hawking",
            AnswerLabel.A, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the mass of an electron approximately in atomic mass units?",
            "0.000549", "1", "9.109 × 10⁻³¹", "0.511 MeV/c²",
            AnswerLabel.A, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "Which ancient Greek mathematician is famous for the principle of buoyancy?",
            "Pythagoras", "Euclid", "Archimedes", "Thales",
            AnswerLabel.C, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the formal name of the mathematical branch studying symmetric patterns?",
            "Topology", "Group Theory", "Symmetry Theory", "Pattern Theory",
            AnswerLabel.B, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "In which century did the Industrial Revolution primarily occur?",
            "17th", "18th", "19th", "20th",
            AnswerLabel.B, Difficulty.Extreme),

        MultipleChoiceCard.Create(
            "What is the speed of sound in air at 20°C approximately?",
            "300 m/s", "340 m/s", "380 m/s", "420 m/s",
            AnswerLabel.B, Difficulty.Extreme),
    ];

}
