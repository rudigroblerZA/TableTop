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
            [MindMeldCardBank.UsCategory] = "#EC407A",
            [MindMeldCardBank.FavouritesCategory] = "#AB47BC",
            [MindMeldCardBank.MemoriesCategory] = "#FFA726",
            [MindMeldCardBank.HypotheticalCategory] = "#42A5F5",
            [MindMeldCardBank.SpeedRoundCategory] = "#EF5350",
            [MindMeldCardBank.DeepSyncCategory] = "#66BB6A",
            [MindMeldCardBank.AfterDarkCategory] = "#B71C4A",
            [MindMeldCardBank.DoItNowCategory] = "#D97706",
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
    internal const string UsCategory = "Us";
    internal const string FavouritesCategory = "Favourites";
    internal const string MemoriesCategory = "Memories";
    internal const string HypotheticalCategory = "Hypothetical";
    internal const string SpeedRoundCategory = "Speed Round";
    internal const string DeepSyncCategory = "Deep Sync";
    internal const string AfterDarkCategory = "After Dark";
    internal const string DoItNowCategory = "Do It Now";

    private const string Deck = "Mind Meld";

    /// <summary>All mind-meld cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)
        // ── US ────────────────────────────────────────────────────────────────
            .Category(UsCategory)
            .Card(UsCategory, BodyM("Name the moment you'd call the true beginning of 'us'."), Difficulty.Medium)
            .Card(UsCategory, BodyM("What's OUR song? One answer."), Difficulty.Easy)
            .Card(UsCategory, BodyM("Name the friend couple we spend the most time with."), Difficulty.Easy)
            .Card(UsCategory, BodyM("What's our most-repeated inside joke? Write the punchline."), Difficulty.Medium)
            .Card(UsCategory, BodyM("Which of us is the better cook? Be honest — and identical."), Difficulty.Easy)
            .Card(UsCategory, BodyM("Name the household chore we argue about most."), Difficulty.Easy)
            .Card(UsCategory, BodyM("What's the one thing we ALWAYS forget when we travel?"), Difficulty.Medium)
            .Card(UsCategory, BodyM("Who said 'I love you' first? (There is a fact of the matter.)"), Difficulty.Easy)

        // ── FAVOURITES ────────────────────────────────────────────────────────
            .Category(FavouritesCategory)
            .Card(FavouritesCategory, BodyM("Name a film we BOTH love."), Difficulty.Easy)
            .Card(FavouritesCategory, BodyM("Name the restaurant we'd pick for a no-occasion dinner tonight."), Difficulty.Easy)
            .Card(FavouritesCategory, BodyM("One food we both agree is overrated."), Difficulty.Medium)
            .Card(FavouritesCategory, BodyM("The TV series we'd rewatch together from episode one."), Difficulty.Easy)
            .Card(FavouritesCategory, BodyM("Our ideal holiday: beach, city, or mountains? One word."), Difficulty.Easy)
            .Card(FavouritesCategory, BodyM("Name a song that instantly puts BOTH of us in a good mood."), Difficulty.Medium)
            .Card(FavouritesCategory, BodyM("The board or video game we're most evenly matched at."), Difficulty.Medium)
            .Card(FavouritesCategory, BodyM("Pick the dessert we'd share if we could only order one."), Difficulty.Easy)

        // ── MEMORIES ──────────────────────────────────────────────────────────
            .Category(MemoriesCategory)
            .Card(MemoriesCategory, BodyM("Name the single funniest thing that has ever happened to us together."), Difficulty.Hard)
            .Card(MemoriesCategory, BodyM("Our best holiday so far. One destination."), Difficulty.Easy)
            .Card(MemoriesCategory, BodyM("The meal one of us cooked that we still talk about (good OR disastrous)."), Difficulty.Medium)
            .Card(MemoriesCategory, BodyM("Name the moment one of us was proudest of the other."), Difficulty.Hard)
            .Card(MemoriesCategory, BodyM("The worst weather we've ever been caught in together — where were we?"), Difficulty.Medium)
            .Card(MemoriesCategory, BodyM("Which photo of us would we both choose as THE photo?"), Difficulty.Hard)
            .Card(MemoriesCategory, BodyM("Name a time we cried laughing. Same incident or no point."), Difficulty.Hard)
            .Card(MemoriesCategory, BodyM("The first film we watched together. (Harder than it sounds.)"), Difficulty.Hard)

        // ── HYPOTHETICAL ──────────────────────────────────────────────────────
            .Category(HypotheticalCategory)
            .Card(HypotheticalCategory, BodyM("We win the lottery tonight. What's the FIRST thing we buy?"), Difficulty.Medium)
            .Card(HypotheticalCategory, BodyM("If we opened a small business together, what would it be?"), Difficulty.Medium)
            .Card(HypotheticalCategory, BodyM("Any city in the world for one year, all expenses paid. Which one?"), Difficulty.Medium)
            .Card(HypotheticalCategory, BodyM("If we got a pet tomorrow, what would we name it?"), Difficulty.Hard)
            .Card(HypotheticalCategory, BodyM("One superpower for the two of us to SHARE. Which power?"), Difficulty.Medium)
            .Card(HypotheticalCategory, BodyM("A film gets made about us. Name the genre."), Difficulty.Medium)
            .Card(HypotheticalCategory, BodyM("We can un-invent one modern technology. Which goes?"), Difficulty.Hard)
            .Card(HypotheticalCategory, BodyM("Dinner with any living famous person, together. Who?"), Difficulty.Hard)

        // ── SPEED ROUND ───────────────────────────────────────────────────────
            .Category(SpeedRoundCategory)
            .Card(SpeedRoundCategory, BodyM("SPEED: A colour. First one in your head. Go."), Difficulty.Hard)
            .Card(SpeedRoundCategory, BodyM("SPEED: Pizza topping. Now."), Difficulty.Medium)
            .Card(SpeedRoundCategory, BodyM("SPEED: An animal. Instantly."), Difficulty.Hard)
            .Card(SpeedRoundCategory, BodyM("SPEED: A number between 1 and 10."), Difficulty.Extreme)
            .Card(SpeedRoundCategory, BodyM("SPEED: Breakfast food. Go."), Difficulty.Medium)
            .Card(SpeedRoundCategory, BodyM("SPEED: A country (not this one). Now."), Difficulty.Hard)
            .Card(SpeedRoundCategory, BodyM("SPEED: Something in your kitchen. First thought."), Difficulty.Hard)
            .Card(SpeedRoundCategory, BodyM("SPEED: A word that describes today. Go."), Difficulty.Extreme)

        // ── DEEP SYNC ─────────────────────────────────────────────────────────
            .Category(DeepSyncCategory)
            .Card(DeepSyncCategory, BodyM("Name the value we most want to be known for as a couple. One word."), Difficulty.Hard)
            .Card(DeepSyncCategory, BodyM("What are we better at now than we were a year ago? One answer."), Difficulty.Hard)
            .Card(DeepSyncCategory, BodyM("Name the thing we should do MORE of together. Be specific."), Difficulty.Hard)
            .Card(DeepSyncCategory, BodyM("In one word: what does home mean to us?"), Difficulty.Extreme)
            .Card(DeepSyncCategory, BodyM("Name the next big milestone we're both quietly aiming for."), Difficulty.Hard)
            .Card(DeepSyncCategory, BodyM("The habit we'd both agree to drop, starting tonight."), Difficulty.Hard)
            .Card(DeepSyncCategory, BodyM("One word your partner would use to describe this exact evening."), Difficulty.Extreme)
            .Card(DeepSyncCategory, BodyM("Name the place that feels most 'ours'. One answer."), Difficulty.Hard)

        // ── EXPANSION: AFTER DARK-ISH ─────────────────────────────────────────
            .Category(UsCategory)
            .Card(UsCategory, BodyM("Where was our best kiss? One location."), Difficulty.Medium)
            .Card(UsCategory, BodyM("Who is the better flirt — honestly?"), Difficulty.Easy)
            .Card(UsCategory, BodyM("Name the outfit the other wears that you'd never let them throw away."), Difficulty.Hard)
            .Card(UsCategory, BodyM("What were we doing the last time we lost complete track of time together?"), Difficulty.Hard)
            .Category(FavouritesCategory)
            .Card(FavouritesCategory, BodyM("The song that should NEVER play at our funerals — same answer or scandal."), Difficulty.Hard)
            .Card(FavouritesCategory, BodyM("Our couple's guilty pleasure that we tell no one about. Write it."), Difficulty.Medium)
            .Category(HypotheticalCategory)
            .Card(HypotheticalCategory, BodyM("We have to commit one (legal, minor) act of chaos tonight. What do we do?"), Difficulty.Medium)
            .Card(HypotheticalCategory, BodyM("We're witnesses in a movie heist and must invent our couple alias. Write the SAME fake surname."), Difficulty.Extreme)
            .Card(HypotheticalCategory, BodyM("One of us gets a dramatic villain era. Which of us — and what's the villain name?"), Difficulty.Hard)
            .Category(SpeedRoundCategory)
            .Card(SpeedRoundCategory, BodyM("SPEED: The other one's most attractive feature. Go — no thinking."), Difficulty.Medium)
            .Card(SpeedRoundCategory, BodyM("SPEED: Our relationship as a weather forecast. Two words max."), Difficulty.Hard)
            .Card(SpeedRoundCategory, BodyM("SPEED: What are we doing after this game? First thought. Honest."), Difficulty.Medium)
            .Card(SpeedRoundCategory, BodyM("SPEED: A word we've definitely both said today. Go."), Difficulty.Hard)
            .Category(DeepSyncCategory)
            .Card(DeepSyncCategory, BodyM("The thing we're both slightly scared to bring up — name the TOPIC in one word."), Difficulty.Extreme)
            .Card(DeepSyncCategory, BodyM("Finish the sentence with the same word: 'What we have is ____.'"), Difficulty.Extreme)
            .Card(DeepSyncCategory, BodyM("Which of us fell first? There is a correct answer. Match it."), Difficulty.Hard)

        // ── AFTER DARK (18+) — heat rises, answers still have to MATCH ──────
            .Category(AfterDarkCategory)
            .Card(AfterDarkCategory, BodyM("Where do I most like being kissed? Write the same spot or no point."), Difficulty.Medium)
            .Card(AfterDarkCategory, BodyM("Rate tonight's chances, 1–10. Matching numbers is either romance or telepathy."), Difficulty.Medium)
            .Card(AfterDarkCategory, BodyM("The item of clothing the other owns that should frankly be illegal. Name it."), Difficulty.Hard)
            .Card(AfterDarkCategory, BodyM("One word for the way I look at you when I think you haven't noticed."), Difficulty.Extreme)
            .Card(AfterDarkCategory, BodyM("Name the exact moment this week you found the other most attractive. Same moment = meld."), Difficulty.Extreme)
            .Card(AfterDarkCategory, BodyM("Best kiss of our entire history. Location AND occasion. Both must match."), Difficulty.Hard)
            .Card(AfterDarkCategory, BodyM("The thing I do with my hands that you've never mentioned noticing. Write it."), Difficulty.Extreme)
            .Card(AfterDarkCategory, BodyM("SPEED: A place in this home we have thoroughly... appreciated. First answer. Go."), Difficulty.Hard)
            .Card(AfterDarkCategory, BodyM("SPEED: What am I wearing in your favourite mental picture of me? Go."), Difficulty.Extreme)
            .Card(AfterDarkCategory, BodyM("Slow dance, right now, no music — yes or no? Match your answers, then honour them."), Difficulty.Medium)
            .Card(AfterDarkCategory, BodyM("Name the fictional character the other would absolutely get a hall pass for."), Difficulty.Hard)
            .Card(AfterDarkCategory, BodyM("Finish identically: 'The most underrated part of you is your ____.'"), Difficulty.Extreme)
            .Card(AfterDarkCategory, BodyM("What's the signal — the look, the phrase, the move — that means tonight is ON? Describe the same one."), Difficulty.Hard)
            .Card(AfterDarkCategory, BodyM("The compliment you secretly wish I'd say more often. If I write the SAME one, I clearly already knew."), Difficulty.Extreme)
            .Card(AfterDarkCategory, BodyM("Massage negotiation: who owes whom one, right now? There is a correct answer. Match it."), Difficulty.Easy)
            .Card(AfterDarkCategory, BodyM("Candlelight, hotel room, or nowhere near a bed: pick our ideal setting. One answer."), Difficulty.Medium)
            .Card(AfterDarkCategory, BodyM("The song that should be playing later. Same track = destiny, put it on."), Difficulty.Hard)
            .Card(AfterDarkCategory, BodyM("Write down who's in charge tonight. Matching answers settle it. Non-matching answers ALSO settle it — interestingly."), Difficulty.Extreme)

        // ── AFTER DARK, ROUND TWO (18+) ──────────────────────────────────────
            .Card(AfterDarkCategory, BodyM("Complete identically: 'I could watch you ____ all day.'"), Difficulty.Hard)
            .Card(AfterDarkCategory, BodyM("Name the one place we've never kissed but absolutely should. Same answer = tonight's itinerary."), Difficulty.Hard)
            .Card(AfterDarkCategory, BodyM("The last time we couldn't keep our hands to ourselves in public — where were we? Match the location."), Difficulty.Hard)
            .Card(AfterDarkCategory, BodyM("My best feature, according to YOU — but write what you think I'D say you'd say. Yes, read that twice."), Difficulty.Extreme)
            .Card(AfterDarkCategory, BodyM("What does my voice do when I'm flirting on purpose? Describe it. Matching descriptions earn a live demonstration."), Difficulty.Extreme)
            .Card(AfterDarkCategory, BodyM("SPEED: Lights on or lights off? No thinking. Go."), Difficulty.Medium)
            .Card(AfterDarkCategory, BodyM("SPEED: The exact word I whisper best. Go."), Difficulty.Extreme)
            .Card(AfterDarkCategory, BodyM("SPEED: Kitchen, sofa, staircase — first one in your head. Go."), Difficulty.Hard)
            .Card(AfterDarkCategory, BodyM("Write the time we're actually going to bed tonight — the honest number, not the aspirational one."), Difficulty.Medium)
            .Card(AfterDarkCategory, BodyM("Name the film scene we both privately think of as 'ours'. If you match, you must re-enact the tame first half."), Difficulty.Extreme)
            .Card(AfterDarkCategory, BodyM("One rule for the rest of tonight — write the SAME rule and you both have to keep it."), Difficulty.Extreme)
            .Card(AfterDarkCategory, BodyM("The perfume, cologne, or plain soap-and-skin smell of the other that undoes you. Name it identically."), Difficulty.Hard)
            .Card(AfterDarkCategory, BodyM("Where should my hand be during the boring parts of films? Same answer, obviously."), Difficulty.Medium)
            .Card(AfterDarkCategory, BodyM("Your favourite three seconds of our average day. Be precise. Match them."), Difficulty.Extreme)
            .Card(AfterDarkCategory, BodyM("The nickname I ONLY get in private — write it. If you match, it's officially canon."), Difficulty.Medium)
            .Card(AfterDarkCategory, BodyM("Finish the sentence with the same word: 'Later, I'm going to ____ you senseless.' Keep it printable. Barely."), Difficulty.Extreme)
            .Card(AfterDarkCategory, BodyM("Whose turn is it to make the first move tonight? There's a correct answer and you both know it."), Difficulty.Hard)
            .Card(AfterDarkCategory, BodyM("Describe my 'come here' look in three words. Matching descriptions must be immediately deployed."), Difficulty.Extreme)

        // ── DO IT NOW (18+) — matching answers are self-executing ────────────
            .Category(DoItNowCategory)
            .Card(DoItNowCategory, BodyM("Where should the next kiss land? Write the spot. MATCH = it happens immediately, no discussion."), Difficulty.Medium)
            .Card(DoItNowCategory, BodyM("Pick a number of seconds, 5–60. MATCH = that's the length of the eye-contact staring contest starting NOW. Loser owes a compliment."), Difficulty.Easy)
            .Card(DoItNowCategory, BodyM("Name a song. MATCH = it goes on and you slow dance to it before the next card. NEAR-MISS (same artist) = you dance anyway, smugly."), Difficulty.Hard)
            .Card(DoItNowCategory, BodyM("Write 'left' or 'right'. MATCH = that hand gets held for the next three cards, whatever logistics that requires."), Difficulty.Easy)
            .Card(DoItNowCategory, BodyM("Name the room. MATCH = the rest of this game relocates there right now, cards and all."), Difficulty.Medium)
            .Card(DoItNowCategory, BodyM("Write a number 1–10: how much do you want a massage right now? MATCH = the LOWER scorer gives it. Yes, you read that correctly. Bid carefully."), Difficulty.Extreme)
            .Card(DoItNowCategory, BodyM("Name an item of clothing (yours or theirs). MATCH = it's swapped or shed — owner's choice — for the rest of the game."), Difficulty.Extreme)
            .Card(DoItNowCategory, BodyM("Write one word the other must work naturally into a sentence within the next two cards. MATCH = you both got the SAME word for each other, and the folklore says that's basically telepathy — both forfeit a kiss instead."), Difficulty.Hard)
            .Card(DoItNowCategory, BodyM("Pick: 'lights lower' or 'lights as-is'. MATCH on lower = someone gets up and dims them. The game continues in mood lighting."), Difficulty.Medium)
            .Card(DoItNowCategory, BodyM("Write the exact minute (like 10:47) this game ends tonight. MATCH = spooky, and binding. Within 5 minutes of each other = binding-ish. Wildly apart = negotiate. In writing. With incentives."), Difficulty.Extreme)
            .Build();

    private static string BodyM(string prompt) =>
        "<b>Both of you, in secret:</b>\n\n" + prompt +
        "\n\nWrite your answer where the other can't see. Count down 3-2-1 and reveal together.\n\n" +
        "<i>Same answer (close enough counts — you're the judges): that's a meld. Point for the couple.</i>";
}
