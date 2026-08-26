using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Mind Meld — think as one. Both answer the same prompt in secret; match and score.
///
/// How to play:
///   1. Read the prompt. It's the SAME question for both of you.
///   2. Both write your answer in secret (phone notes, paper — no peeking).
///   3. Count down "3, 2, 1" and reveal simultaneously.
///   4. Same answer? Mind meld — a point for the couple. Different answers?
///      No point, but you're about to have an interesting conversation.
///
/// This isn't guessing what your partner would say (that's Would You Know?) —
/// it's converging on the same answer independently, which is a different skill:
/// knowing your shared world. "A film we both love" is easy at year one and
/// telepathic at year ten. Sync cards ask for one answer about your life together;
/// Speed Round cards want your instant first thoughts.
///
/// Score is cooperative: it's the two of you versus the deck.
/// </summary>
public sealed class MindMeldMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Mind Meld";
    /// <inheritdoc />
    public override string Description =>
        "Both secretly answer the same question — reveal together, score when you match. You vs. the deck.";

    /// <summary>Label shown on the button that records a matched round.</summary>
    public override string CompleteLabel => "Matched";
    /// <summary>Label shown on the button that records a miss.</summary>
    public override string SkipLabel => "Missed";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Us"] = "#EC407A",
            ["Favourites"] = "#AB47BC",
            ["Memories"] = "#FFA726",
            ["Hypothetical"] = "#42A5F5",
            ["Speed Round"] = "#EF5350",
            ["Deep Sync"] = "#66BB6A",
            ["After Dark"] = "#B71C4A",
            ["Do It Now"] = "#D97706",
        };

    /// <summary>One shared point per matched answer.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Returns the built-in mind-meld card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        MindMeldCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => MindMeldCardBank.All;
}

/// <summary>Built-in card bank for Mind Meld.</summary>
public static class MindMeldCardBank
{
    /// <summary>All mind-meld cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
    [
        // ── US ────────────────────────────────────────────────────────────────
        M("Us", "Name the moment you'd call the true beginning of 'us'.", Difficulty.Medium),
        M("Us", "What's OUR song? One answer.", Difficulty.Easy),
        M("Us", "Name the friend couple we spend the most time with.", Difficulty.Easy),
        M("Us", "What's our most-repeated inside joke? Write the punchline.", Difficulty.Medium),
        M("Us", "Which of us is the better cook? Be honest — and identical.", Difficulty.Easy),
        M("Us", "Name the household chore we argue about most.", Difficulty.Easy),
        M("Us", "What's the one thing we ALWAYS forget when we travel?", Difficulty.Medium),
        M("Us", "Who said 'I love you' first? (There is a fact of the matter.)", Difficulty.Easy),

        // ── FAVOURITES ────────────────────────────────────────────────────────
        M("Favourites", "Name a film we BOTH love.", Difficulty.Easy),
        M("Favourites", "Name the restaurant we'd pick for a no-occasion dinner tonight.", Difficulty.Easy),
        M("Favourites", "One food we both agree is overrated.", Difficulty.Medium),
        M("Favourites", "The TV series we'd rewatch together from episode one.", Difficulty.Easy),
        M("Favourites", "Our ideal holiday: beach, city, or mountains? One word.", Difficulty.Easy),
        M("Favourites", "Name a song that instantly puts BOTH of us in a good mood.", Difficulty.Medium),
        M("Favourites", "The board or video game we're most evenly matched at.", Difficulty.Medium),
        M("Favourites", "Pick the dessert we'd share if we could only order one.", Difficulty.Easy),

        // ── MEMORIES ──────────────────────────────────────────────────────────
        M("Memories", "Name the single funniest thing that has ever happened to us together.", Difficulty.Hard),
        M("Memories", "Our best holiday so far. One destination.", Difficulty.Easy),
        M("Memories", "The meal one of us cooked that we still talk about (good OR disastrous).", Difficulty.Medium),
        M("Memories", "Name the moment one of us was proudest of the other.", Difficulty.Hard),
        M("Memories", "The worst weather we've ever been caught in together — where were we?", Difficulty.Medium),
        M("Memories", "Which photo of us would we both choose as THE photo?", Difficulty.Hard),
        M("Memories", "Name a time we cried laughing. Same incident or no point.", Difficulty.Hard),
        M("Memories", "The first film we watched together. (Harder than it sounds.)", Difficulty.Hard),

        // ── HYPOTHETICAL ──────────────────────────────────────────────────────
        M("Hypothetical", "We win the lottery tonight. What's the FIRST thing we buy?", Difficulty.Medium),
        M("Hypothetical", "If we opened a small business together, what would it be?", Difficulty.Medium),
        M("Hypothetical", "Any city in the world for one year, all expenses paid. Which one?", Difficulty.Medium),
        M("Hypothetical", "If we got a pet tomorrow, what would we name it?", Difficulty.Hard),
        M("Hypothetical", "One superpower for the two of us to SHARE. Which power?", Difficulty.Medium),
        M("Hypothetical", "A film gets made about us. Name the genre.", Difficulty.Medium),
        M("Hypothetical", "We can un-invent one modern technology. Which goes?", Difficulty.Hard),
        M("Hypothetical", "Dinner with any living famous person, together. Who?", Difficulty.Hard),

        // ── SPEED ROUND ───────────────────────────────────────────────────────
        M("Speed Round", "SPEED: A colour. First one in your head. Go.", Difficulty.Hard),
        M("Speed Round", "SPEED: Pizza topping. Now.", Difficulty.Medium),
        M("Speed Round", "SPEED: An animal. Instantly.", Difficulty.Hard),
        M("Speed Round", "SPEED: A number between 1 and 10.", Difficulty.Extreme),
        M("Speed Round", "SPEED: Breakfast food. Go.", Difficulty.Medium),
        M("Speed Round", "SPEED: A country (not this one). Now.", Difficulty.Hard),
        M("Speed Round", "SPEED: Something in your kitchen. First thought.", Difficulty.Hard),
        M("Speed Round", "SPEED: A word that describes today. Go.", Difficulty.Extreme),

        // ── DEEP SYNC ─────────────────────────────────────────────────────────
        M("Deep Sync", "Name the value we most want to be known for as a couple. One word.", Difficulty.Hard),
        M("Deep Sync", "What are we better at now than we were a year ago? One answer.", Difficulty.Hard),
        M("Deep Sync", "Name the thing we should do MORE of together. Be specific.", Difficulty.Hard),
        M("Deep Sync", "In one word: what does home mean to us?", Difficulty.Extreme),
        M("Deep Sync", "Name the next big milestone we're both quietly aiming for.", Difficulty.Hard),
        M("Deep Sync", "The habit we'd both agree to drop, starting tonight.", Difficulty.Hard),
        M("Deep Sync", "One word your partner would use to describe this exact evening.", Difficulty.Extreme),
        M("Deep Sync", "Name the place that feels most 'ours'. One answer.", Difficulty.Hard),

        // ── EXPANSION: AFTER DARK-ISH ─────────────────────────────────────────
        M("Us", "Where was our best kiss? One location.", Difficulty.Medium),
        M("Us", "Who is the better flirt — honestly?", Difficulty.Easy),
        M("Us", "Name the outfit the other wears that you'd never let them throw away.", Difficulty.Hard),
        M("Us", "What were we doing the last time we lost complete track of time together?", Difficulty.Hard),
        M("Favourites", "The song that should NEVER play at our funerals — same answer or scandal.", Difficulty.Hard),
        M("Favourites", "Our couple's guilty pleasure that we tell no one about. Write it.", Difficulty.Medium),
        M("Hypothetical", "We have to commit one (legal, minor) act of chaos tonight. What do we do?", Difficulty.Medium),
        M("Hypothetical", "We're witnesses in a movie heist and must invent our couple alias. Write the SAME fake surname.", Difficulty.Extreme),
        M("Hypothetical", "One of us gets a dramatic villain era. Which of us — and what's the villain name?", Difficulty.Hard),
        M("Speed Round", "SPEED: The other one's most attractive feature. Go — no thinking.", Difficulty.Medium),
        M("Speed Round", "SPEED: Our relationship as a weather forecast. Two words max.", Difficulty.Hard),
        M("Speed Round", "SPEED: What are we doing after this game? First thought. Honest.", Difficulty.Medium),
        M("Speed Round", "SPEED: A word we've definitely both said today. Go.", Difficulty.Hard),
        M("Deep Sync", "The thing we're both slightly scared to bring up — name the TOPIC in one word.", Difficulty.Extreme),
        M("Deep Sync", "Finish the sentence with the same word: 'What we have is ____.'", Difficulty.Extreme),
        M("Deep Sync", "Which of us fell first? There is a correct answer. Match it.", Difficulty.Hard),

        // ── AFTER DARK (18+) — heat rises, answers still have to MATCH ──────
        M("After Dark", "Where do I most like being kissed? Write the same spot or no point.", Difficulty.Medium),
        M("After Dark", "Rate tonight's chances, 1–10. Matching numbers is either romance or telepathy.", Difficulty.Medium),
        M("After Dark", "The item of clothing the other owns that should frankly be illegal. Name it.", Difficulty.Hard),
        M("After Dark", "One word for the way I look at you when I think you haven't noticed.", Difficulty.Extreme),
        M("After Dark", "Name the exact moment this week you found the other most attractive. Same moment = meld.", Difficulty.Extreme),
        M("After Dark", "Best kiss of our entire history. Location AND occasion. Both must match.", Difficulty.Hard),
        M("After Dark", "The thing I do with my hands that you've never mentioned noticing. Write it.", Difficulty.Extreme),
        M("After Dark", "SPEED: A place in this home we have thoroughly... appreciated. First answer. Go.", Difficulty.Hard),
        M("After Dark", "SPEED: What am I wearing in your favourite mental picture of me? Go.", Difficulty.Extreme),
        M("After Dark", "Slow dance, right now, no music — yes or no? Match your answers, then honour them.", Difficulty.Medium),
        M("After Dark", "Name the fictional character the other would absolutely get a hall pass for.", Difficulty.Hard),
        M("After Dark", "Finish identically: 'The most underrated part of you is your ____.'", Difficulty.Extreme),
        M("After Dark", "What's the signal — the look, the phrase, the move — that means tonight is ON? Describe the same one.", Difficulty.Hard),
        M("After Dark", "The compliment you secretly wish I'd say more often. If I write the SAME one, I clearly already knew.", Difficulty.Extreme),
        M("After Dark", "Massage negotiation: who owes whom one, right now? There is a correct answer. Match it.", Difficulty.Easy),
        M("After Dark", "Candlelight, hotel room, or nowhere near a bed: pick our ideal setting. One answer.", Difficulty.Medium),
        M("After Dark", "The song that should be playing later. Same track = destiny, put it on.", Difficulty.Hard),
        M("After Dark", "Write down who's in charge tonight. Matching answers settle it. Non-matching answers ALSO settle it — interestingly.", Difficulty.Extreme),

        // ── AFTER DARK, ROUND TWO (18+) ──────────────────────────────────────
        M("After Dark", "Complete identically: 'I could watch you ____ all day.'", Difficulty.Hard),
        M("After Dark", "Name the one place we've never kissed but absolutely should. Same answer = tonight's itinerary.", Difficulty.Hard),
        M("After Dark", "The last time we couldn't keep our hands to ourselves in public — where were we? Match the location.", Difficulty.Hard),
        M("After Dark", "My best feature, according to YOU — but write what you think I'D say you'd say. Yes, read that twice.", Difficulty.Extreme),
        M("After Dark", "What does my voice do when I'm flirting on purpose? Describe it. Matching descriptions earn a live demonstration.", Difficulty.Extreme),
        M("After Dark", "SPEED: Lights on or lights off? No thinking. Go.", Difficulty.Medium),
        M("After Dark", "SPEED: The exact word I whisper best. Go.", Difficulty.Extreme),
        M("After Dark", "SPEED: Kitchen, sofa, staircase — first one in your head. Go.", Difficulty.Hard),
        M("After Dark", "Write the time we're actually going to bed tonight — the honest number, not the aspirational one.", Difficulty.Medium),
        M("After Dark", "Name the film scene we both privately think of as 'ours'. If you match, you must re-enact the tame first half.", Difficulty.Extreme),
        M("After Dark", "One rule for the rest of tonight — write the SAME rule and you both have to keep it.", Difficulty.Extreme),
        M("After Dark", "The perfume, cologne, or plain soap-and-skin smell of the other that undoes you. Name it identically.", Difficulty.Hard),
        M("After Dark", "Where should my hand be during the boring parts of films? Same answer, obviously.", Difficulty.Medium),
        M("After Dark", "Your favourite three seconds of our average day. Be precise. Match them.", Difficulty.Extreme),
        M("After Dark", "The nickname I ONLY get in private — write it. If you match, it's officially canon.", Difficulty.Medium),
        M("After Dark", "Finish the sentence with the same word: 'Later, I'm going to ____ you senseless.' Keep it printable. Barely.", Difficulty.Extreme),
        M("After Dark", "Whose turn is it to make the first move tonight? There's a correct answer and you both know it.", Difficulty.Hard),
        M("After Dark", "Describe my 'come here' look in three words. Matching descriptions must be immediately deployed.", Difficulty.Extreme),

        // ── DO IT NOW (18+) — matching answers are self-executing ────────────
        M("Do It Now", "Where should the next kiss land? Write the spot. MATCH = it happens immediately, no discussion.", Difficulty.Medium),
        M("Do It Now", "Pick a number of seconds, 5–60. MATCH = that's the length of the eye-contact staring contest starting NOW. Loser owes a compliment.", Difficulty.Easy),
        M("Do It Now", "Name a song. MATCH = it goes on and you slow dance to it before the next card. NEAR-MISS (same artist) = you dance anyway, smugly.", Difficulty.Hard),
        M("Do It Now", "Write 'left' or 'right'. MATCH = that hand gets held for the next three cards, whatever logistics that requires.", Difficulty.Easy),
        M("Do It Now", "Name the room. MATCH = the rest of this game relocates there right now, cards and all.", Difficulty.Medium),
        M("Do It Now", "Write a number 1–10: how much do you want a massage right now? MATCH = the LOWER scorer gives it. Yes, you read that correctly. Bid carefully.", Difficulty.Extreme),
        M("Do It Now", "Name an item of clothing (yours or theirs). MATCH = it's swapped or shed — owner's choice — for the rest of the game.", Difficulty.Extreme),
        M("Do It Now", "Write one word the other must work naturally into a sentence within the next two cards. MATCH = you both got the SAME word for each other, and the folklore says that's basically telepathy — both forfeit a kiss instead.", Difficulty.Hard),
        M("Do It Now", "Pick: 'lights lower' or 'lights as-is'. MATCH on lower = someone gets up and dims them. The game continues in mood lighting.", Difficulty.Medium),
        M("Do It Now", "Write the exact minute (like 10:47) this game ends tonight. MATCH = spooky, and binding. Within 5 minutes of each other = binding-ish. Wildly apart = negotiate. In writing. With incentives.", Difficulty.Extreme),
    ];

    private static ICard M(string category, string prompt, Difficulty d) =>
        StandardCard.Create(
            category,
            "<b>Both of you, in secret:</b>\n\n" + prompt +
            "\n\nWrite your answer where the other can't see. Count down 3-2-1 and reveal together.\n\n" +
            "<i>Same answer (close enough counts — you're the judges): that's a meld. Point for the couple.</i>",
            d, category);
}
