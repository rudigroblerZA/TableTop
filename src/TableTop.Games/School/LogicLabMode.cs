using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.School;

/// <summary>
/// Logic Lab — pure reasoning: riddles, mini-deductions, sequences,
/// truth-tellers and liars, and puzzles built to break your assumptions.
///
/// How to play:
///   1. The reader draws a card and reads the puzzle aloud. Everyone thinks —
///      out loud or silently, table's choice.
///   2. For "Lateral" style puzzles the table may interrogate the reader with
///      YES/NO questions (the reader flips early and answers from the back).
///   3. First correct reasoning takes the card. Close-but-incomplete answers:
///      the table decides whether the LOGIC was there. Reasoning scores;
///      lucky guessing is noted with appropriate suspicion.
///   4. Flip for the answer — every back explains WHY, not just what.
///
/// Difficulty is honest here: Easy cards are warm-up riddles; Extreme cards
/// are the ones that make someone stand up and walk a small circle.
/// </summary>
public sealed class LogicLabMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name        => "Logic Lab";
    /// <inheritdoc />
    public override string Description =>
        "Riddles, deductions, sequences, and liars — pure reasoning, with every answer explained on the flip side.";

    /// <summary>Label for the button that records a solved puzzle.</summary>
    public override string CompleteLabel => "Solved It";
    /// <summary>Label for the button that concedes a puzzle.</summary>
    public override string SkipLabel     => "Brain Melted";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Riddles"]      = "#66BB6A",
            ["Deduction"]    = "#42A5F5",
            ["Sequences"]    = "#AB47BC",
            ["Truth & Lies"] = "#FFA726",
            ["Assumptions"]  = "#EF5350",
        };

    /// <summary>Harder puzzles score more — reasoning earns its keep.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new DifficultyBasedScoringStrategy();

    /// <summary>Returns the built-in logic card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        LogicLabCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => LogicLabCardBank.All;
}

/// <summary>Built-in card bank for Logic Lab.</summary>
public static class LogicLabCardBank
{
    /// <summary>All logic cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── RIDDLES — warm-ups with a click ─────────────────────────────────
        P("Riddles", "The Thirsty Towel",
          "What gets wetter the more it dries?",
          "A towel — it soaks up water precisely by doing its job.",
          Difficulty.Easy),
        P("Riddles", "Unbreakable Silence",
          "What breaks the moment you say its name?",
          "Silence. Saying the word is the breaking.",
          Difficulty.Easy),
        P("Riddles", "The Weighty Word",
          "Forward I am heavy; backward I am not. What am I?",
          "A TON — spelled backwards it's NOT.",
          Difficulty.Easy),
        P("Riddles", "Always Ahead",
          "It's always in front of you, but you can never see it. What is it?",
          "The future — always ahead of you, never visible until it becomes the present.",
          Difficulty.Easy),
        P("Riddles", "The Hole Truth",
          "The more you take away from it, the bigger it gets. What is it?",
          "A hole — digging removes material, and every scoop of removal makes it larger.",
          Difficulty.Easy),
        P("Riddles", "Full of Keys",
          "It's full of keys but can't open a single lock. What is it?",
          "A piano. (A keyboard is also accepted — the table decides if computers count as pianos.)",
          Difficulty.Medium),
        P("Riddles", "The Silent Speaker",
          "It has no mouth, yet answers in every language, and only when spoken to. What is it?",
          "An echo — mouthless, it repeats whatever it hears, in any language, only when spoken to.",
          Difficulty.Medium),
        P("Riddles", "The Honest Thief",
          "It runs all day, steals nothing but seconds, and has hands it never uses to take. What is it?",
          "A clock — running all day, stealing seconds as they pass, with hands that only ever point.",
          Difficulty.Medium),

        // ── DEDUCTION — small, airtight, exactly one answer ─────────────────
        P("Deduction", "Three Pets",
          "Ana, Ben and Cal each own one pet: a cat, a dog, or a fish.\n" +
          "1. Ana is allergic to fur.\n" +
          "2. Ben's pet barks at the postman.\n\n" +
          "Who owns the cat?",
          "Cal. Ana's allergy rules out both furry pets, so she has the fish. Ben's pet barks — the dog. The cat is Cal's by elimination.",
          Difficulty.Medium),
        P("Deduction", "Podium Sisters",
          "Ivy, Joy and Kim finished a race 1st, 2nd and 3rd.\n" +
          "1. Ivy didn't win.\n" +
          "2. Kim finished ahead of Ivy.\n" +
          "3. Joy finished ahead of Kim.\n\n" +
          "Name the full podium.",
          "Joy 1st, Kim 2nd, Ivy 3rd. Clue 3 puts Joy above Kim, clue 2 puts Kim above Ivy — one strict order, and clue 1 agrees.",
          Difficulty.Medium),
        P("Deduction", "The Lunch Order",
          "Four friends sit in a row: Dee, Eli, Fay, Gus.\n" +
          "1. Dee sits at one end.\n" +
          "2. Eli sits directly between Fay and Gus.\n" +
          "3. Fay is NOT next to Dee.\n\n" +
          "Who sits where?",
          "Dee, Gus, Eli, Fay (or its mirror image — same seating, viewed from the other side). Eli must be flanked by Fay and Gus, so those three form a block; Dee takes an end. Clue 3 forces Gus, not Fay, beside Dee.",
          Difficulty.Hard),
        P("Deduction", "The Bookshelf",
          "Three books — red, blue, green — sit left to right on a shelf.\n" +
          "1. The red book is not on the left.\n" +
          "2. The green book is IMMEDIATELY to the left of the blue one.\n\n" +
          "What's the order?",
          "Green, Blue, Red. Green-Blue must sit together in that order (clue 2), so the pair occupies either the left two slots or the right two. If they took the right two, Red would be on the left — forbidden by clue 1. So Green-Blue take the left, and Red goes right.",
          Difficulty.Hard),
        P("Deduction", "The Coin Pouches",
          "Three pouches are labelled '10 coins', '20 coins', and '30 coins' — but EVERY label is wrong.\n" +
          "You may count the coins in just ONE pouch. Which do you pick to work out all three?",
          "Any one works — counting one pouch tells you its true amount, and since every label is wrong, the remaining two amounts have only one legal arrangement between the two remaining pouches. Example: open '10', find 20 coins. Then '20' can't hold 20 (found) or 20 (label) — it holds 30 or 10; but '30' can't hold 30, so '30' holds 10, forcing '20' to hold 30.",
          Difficulty.Hard),
        P("Deduction", "Born Together, Not Twins",
          "Two children were born to the same mother, on the same day, in the same year, within the same hour — yet they are NOT twins. How is that possible?",
          "They're two of a set of TRIPLETS (or quadruplets, and so on). Nothing in the puzzle says only two children were born — that's the assumption doing the work.",
          Difficulty.Hard),

        // ── SEQUENCES — what comes next, and why ────────────────────────────
        P("Sequences", "The Calendar Code",
          "What letter comes next?\n\nJ, F, M, A, M, J, J, A, S, O, N, __",
          "D — they're the months: January through December.",
          Difficulty.Easy),
        P("Sequences", "Counting Letters",
          "What letter comes next?\n\nO, T, T, F, F, S, S, E, __",
          "N — the initials of One, Two, Three, Four, Five, Six, Seven, Eight… Nine.",
          Difficulty.Medium),
        P("Sequences", "Double Trouble",
          "What number comes next?\n\n2, 3, 5, 9, 17, __",
          "33. Each term is double the previous, minus one: 2×2−1=3, 3×2−1=5, 5×2−1=9, 9×2−1=17, 17×2−1=33.",
          Difficulty.Medium),
        P("Sequences", "Say What You See",
          "What comes next in this famous sequence?\n\n1, 11, 21, 1211, 111221, __",
          "312211 — each term DESCRIBES the previous one aloud: '1' is 'one 1' (11); '11' is 'two 1s' (21); '21' is 'one 2, one 1' (1211); and 111221 is 'three 1s, two 2s, one 1' → 312211.",
          Difficulty.Hard),
        P("Sequences", "Mirror Math",
          "What number comes next?\n\n61, 52, 63, 94, 46, __",
          "18. They're the square numbers 16, 25, 36, 49, 64, 81 — written backwards.",
          Difficulty.Extreme),
        P("Sequences", "The Shrinking Gap",
          "What number comes next?\n\n100, 96, 88, 76, 60, __",
          "40. The gaps are 4, 8, 12, 16 — growing by four each time — so the next gap is 20.",
          Difficulty.Medium),
        P("Sequences", "Alphabet Jumps",
          "What letter comes next?\n\nA, C, F, J, O, __",
          "U. The jumps grow by one each time: +2 (A→C), +3 (C→F), +4 (F→J), +5 (J→O), +6 (O→U).",
          Difficulty.Hard),

        // ── TRUTH & LIES — knights, knaves, and one impossible pair ─────────
        P("Truth & Lies", "The Self-Accuser",
          "On this island, every person either ALWAYS tells the truth or ALWAYS lies.\n\n" +
          "Ren, standing with a friend, announces: 'We are both liars.'\n\n" +
          "What is Ren, and what is the friend?",
          "Ren is a liar; the friend tells the truth. A truth-teller could never say 'we are both liars' (it would be a lie). So Ren lies — which means the statement is false — meaning they're NOT both liars, so the friend must be truthful.",
          Difficulty.Hard),
        P("Truth & Lies", "The Fork in the Road",
          "One road leads to the village, the other to the swamp. A local stands at the fork — but you don't know if they always lie or always tell the truth.\n\n" +
          "What ONE question finds the village road either way?",
          "Ask: 'If I asked you which road leads to the village, which would you point to?' — then take that road. A truth-teller points true; a liar must lie about the lie they WOULD tell, and the two lies cancel. (Any correctly nested question scores.)",
          Difficulty.Extreme),
        P("Truth & Lies", "The Impossible Pair",
          "On the island of truth-tellers and liars:\n\n" +
          "Pia says: 'Quinn tells the truth.'\n" +
          "Quinn says: 'Pia lies.'\n\n" +
          "Work out what each of them is — or explain why you can't.",
          "You can't — no consistent assignment exists. If Pia is truthful, Quinn is truthful, but then Quinn's claim makes Pia a liar: contradiction. If Pia lies, Quinn is a liar, but then Quinn's claim is false, making Pia truthful: contradiction again. This pair simply cannot live on the island — spotting THAT is the answer.",
          Difficulty.Extreme),
        P("Truth & Lies", "The Cake Culprit",
          "Someone ate the cake. Exactly ONE of these three statements is true:\n\n" +
          "1. Ash says: 'Blair did it.'\n" +
          "2. Blair says: 'I didn't do it.'\n" +
          "3. Cass says: 'I didn't do it.'\n\n" +
          "Who ate the cake?",
          "Cass. Test each suspect: if Blair did it, statements 1 and 3 are both true — too many. If Ash did it, statements 2 and 3 are both true — too many. If Cass did it, only statement 2 is true — exactly one. ✔",
          Difficulty.Hard),
        P("Truth & Lies", "One Door, Two Guards",
          "Two doors: freedom and a broom cupboard. Two guards: one always lies, one always tells the truth — you don't know which is which, and you get ONE question to ONE guard.\n\n" +
          "The classic. Solve it — and explain WHY it works.",
          "Ask either guard: 'Which door would the OTHER guard say leads to freedom?' — then take the opposite door. Truth-teller truthfully reports the liar's lie; liar lies about the truth-teller's truth. Either way you're handed the wrong door, reliably — and a reliable wrong answer is as good as a right one.",
          Difficulty.Extreme),

        // ── ASSUMPTIONS — the puzzle is the thing you didn't question ───────
        P("Assumptions", "The Doctor's Brother",
          "A doctor in London has a brother who is a doctor in York. But the doctor in York has no brother at all. How?",
          "The doctor in London is his SISTER. The puzzle only works while you assume doctors are men — the assumption is the trick.",
          Difficulty.Medium),
        P("Assumptions", "Thirty Pence",
          "Two coins add up to exactly 30 pence. One of them is NOT a 20p coin. What are the two coins?",
          "A 20p and a 10p. ONE of them is not a 20p — the 10p. The OTHER one is. The wording never said neither was.",
          Difficulty.Medium),
        P("Assumptions", "The Hotel Walk",
          "A woman pushes her car along a street and stops beside a hotel. The moment she does, she knows she owes money. Why?",
          "She's playing Monopoly. The 'car' is her token; the hotel means rent is due.",
          Difficulty.Medium),
        P("Assumptions", "The Unlit Room",
          "You enter a pitch-dark room holding a single match. Inside there's an oil lamp, a candle, and a fireplace ready to light. Which do you light first?",
          "The match — nothing else can be lit until it is. The list of options is the misdirection.",
          Difficulty.Easy),
        P("Assumptions", "Half Full",
          "How can you make the number SIX into an ODD number without adding, removing or changing any digits — only by doing something to how it's written?",
          "Write it as a word — SIX — and remove nothing; instead, cover the S: IX is nine in Roman numerals. (Any legitimate reading trick the table accepts scores; 'cover the S' is the classic.)",
          Difficulty.Hard),
        P("Assumptions", "The Elevator Ride",
          "Every morning a man rides the lift from floor 20 down to the ground and goes to work. Every evening he rides it from the ground to floor 12, then climbs the stairs the rest of the way — EXCEPT on rainy days, when he rides all the way to 20. Why?",
          "He's short. He can reach the ground-floor button and the button for 12, but not 20 — except on rainy days, when he has an umbrella to press it with.",
          Difficulty.Hard),
        P("Assumptions", "The Grateful Customer",
          "A man walks into a café and asks for a glass of water. The person behind the counter suddenly SHOUTS at him. The man smiles, says 'thank you', and leaves without drinking anything. Why?\n\n" +
          "<i>Interrogate the reader with yes/no questions — the reader flips early and answers from the back.</i>",
          "The man had HICCUPS. He wanted water to cure them; the shout scared them away instead — problem solved, no water needed. (Classic lateral canon: any fright-based cure the table reasons its way to counts as solved.)",
          Difficulty.Extreme),
    ];

    private static ICard P(string category, string title, string puzzle, string answer, Difficulty d) =>
        StandardCard.Create(
            title,
            "<b>🧠 " + category.ToUpperInvariant() + "</b>\n\n" +
            puzzle + "\n\n" +
            "<i>Reason it out loud — first correct logic takes the card.</i>\n\n" +
            "Answer: " + answer,
            d, category);
}
