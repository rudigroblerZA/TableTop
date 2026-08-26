using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Game;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Restrictions;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Restrictions;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Relationship Dares — two-player dares designed for couples.
///
/// Unlike Truth or Dare (which works for any group), these dares are built
/// for two people who know each other well. They range from genuinely playful
/// through tender and emotionally challenging to intimate.
///
/// Four zones:
///   Playful    — fun, physical, lighthearted (Easy)
///   Honest     — emotionally revealing dares; saying things you mean (Medium)
///   Tender     — closeness, touch, presence, gentleness (Hard)
///   Intimate   — adult; requires physical and emotional comfort (Extreme)
///
/// All cards require both players' active participation.
/// Neither player can "lose" a dare — negotiation is always allowed.
/// </summary>
public sealed class RelationshipDaresMode : BaseGameModeDefinition, ITableShapeMode
{
    /// <summary>Two people, together. The consent ritual, the aftercare and the register all assume it.</summary>
    public TableShape SuitableFor => TableShape.Couple;

    /// <inheritdoc />
    public override string Name => "Relationship Dares";
    /// <inheritdoc />
    public override string Description =>
        "Two-player dares: playful, honest, tender, intimate. Both of you are in every one.";

    /// <summary>CompleteLabel.</summary>
    public override string CompleteLabel => "✓ Done it";
    /// <summary>SkipLabel.</summary>
    public override string SkipLabel => "~ Negotiate";

    /// <summary>CategoryColours.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            ["Playful"] = "#26C6DA",
            ["Honest"] = "#66BB6A",
            ["Tender"] = "#FFCA28",
            ["Intimate"] = "#EC407A",
        };

    /// <summary>Initialises a new <see cref="BuildScoring"/> instance.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 1);

    /// <summary>Initialises a new <see cref="BuildCards"/> instance.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        RelationshipDaresCardBank.All;

    /// <summary>Returns the card collection for this game mode, filtered and configured for the given players.</summary>
    public static IReadOnlyList<ICard> GetCards() => RelationshipDaresCardBank.All;
}

/// <summary>Built-in card bank for RelationshipDares. Cards are also available as JSON in <c>Data/Json/</c>.</summary>
public static class RelationshipDaresCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build()
    {
        var couplesOnly = new CoupleOnlyRestriction();
        var adultsOnly = new AdultOnlyRestriction();
        var couples18 = couplesOnly.And(adultsOnly);

        return
        [
            // ════════════════════════════════════════════════════════════════
            // PLAYFUL — fun, physical, low stakes
            // ════════════════════════════════════════════════════════════════

            D("Impression of Me",
              "Do your best impression of your partner — their voice, their mannerisms, the way they walk into a room. Keep going until they say stop.",
              "Playful", Difficulty.Easy, couplesOnly),

            D("Recreate the Photo",
              "Find a photo on your phone of the two of you together. Recreate it right now, as accurately as you can.",
              "Playful", Difficulty.Easy, couplesOnly),

            D("The Text You Never Sent",
              "Find a text draft you never sent your partner — or if you don't have one, type one right now with something you've been meaning to say. Read it aloud. Then decide together whether to send it.",
              "Playful", Difficulty.Easy, couplesOnly),

            D("Their Favourite Thing",
              "Do or say one thing you know your partner loves — their favourite gesture, phrase, or small thing you do. Then ask if you got it right.",
              "Playful", Difficulty.Easy, couplesOnly),

            D("Serenade",
              "Sing at least one full verse of a song your partner loves. No skipping the words. No phone allowed.",
              "Playful", Difficulty.Easy, couplesOnly),

            D("Cook Up a Story",
              "Between you, make up the most improbable story of how you met — if you had met in a completely different way. Five sentences each, alternating.",
              "Playful", Difficulty.Easy, couplesOnly),

            D("Swap Phones for Ten Minutes",
              "Hand over your unlocked phone for ten minutes. No briefing them on what they'll find. No hovering.",
              "Playful", Difficulty.Medium, couplesOnly),

            D("Speak for Them",
              "Answer these questions as if you were your partner — they can only correct you after you've finished all three:\n\n• What am I most afraid of right now?\n• What would I spend £500 on today?\n• What do I secretly wish you did more often?",
              "Playful", Difficulty.Medium, couplesOnly),

            D("The Blind Taste Test",
              "One of you is blindfolded. The other feeds them three things from the kitchen. They have to guess each one.",
              "Playful", Difficulty.Easy, couplesOnly),

            D("Read Their Mind",
              "Your partner thinks of a word — any word. You ask ten yes/no questions to guess it. If you get it wrong, they choose the next dare.",
              "Playful", Difficulty.Easy, couplesOnly),

            D("The Argument",
              "Stage an argument — about something trivial and fake. Both of you fully commit. Thirty seconds each.",
              "Playful", Difficulty.Easy, couplesOnly),

            D("Their Proudest Moment",
              "Tell your partner about the moment you are most proud of them — something they did that you haven't explicitly said made you proud. They listen without interrupting.",
              "Playful", Difficulty.Medium, couplesOnly),

            // ════════════════════════════════════════════════════════════════
            // HONEST — dares that require saying true things
            // ════════════════════════════════════════════════════════════════

            D("The Unsolicited Feedback",
              "Give your partner one piece of honest feedback about something they could do differently — delivered kindly, received without defensiveness. They then do the same for you.",
              "Honest", Difficulty.Medium, couplesOnly),

            D("The Gratitude List",
              "Name five specific things your partner has done in the last month that you are genuinely grateful for. No generalities — five actual things.",
              "Honest", Difficulty.Medium, couplesOnly),

            D("Say the Hard Thing",
              "There is something one of you has been wanting to say but hasn't. Right now: say it. The other listens completely before responding.",
              "Honest", Difficulty.Hard, couplesOnly),

            D("The Apology",
              "Apologise properly — with context, with understanding of what happened, without the word 'but' — for one thing from the last month.",
              "Honest", Difficulty.Hard, couplesOnly),

            D("The Compliment You've Been Holding",
              "Tell your partner the most honest compliment you have been carrying around for them that you have never said. Say it now.",
              "Honest", Difficulty.Medium, couplesOnly),

            D("What I Notice About You",
              "Tell your partner five things you notice about them physically — not their 'best' features, but the specific details you find yourself looking at.",
              "Honest", Difficulty.Medium, couplesOnly),

            D("What I Want More Of",
              "Each of you names one thing you want more of from the other. Not a complaint — a request. Then discuss whether it's possible.",
              "Honest", Difficulty.Hard, couplesOnly),

            D("The Thing I Misjudged",
              "Tell your partner about something you misjudged about them when you first met — something you assumed and turned out to be wrong about.",
              "Honest", Difficulty.Medium, couplesOnly),

            D("Read the Last Year",
              "Name three ways your relationship changed in the last twelve months — one that surprised you, one that you welcomed, one that was hard.",
              "Honest", Difficulty.Hard, couplesOnly),

            D("What I'm Most Afraid Of Losing",
              "Each of you names the one thing about your relationship you are most afraid of losing. Then ask each other: are you protecting it?",
              "Honest", Difficulty.Hard, couplesOnly),

            // ════════════════════════════════════════════════════════════════
            // TENDER — closeness, presence, physical gentleness
            // ════════════════════════════════════════════════════════════════

            D("The Long Look",
              "Sit facing each other. Look at each other without speaking for three minutes. No phones. No explaining it afterwards.",
              "Tender", Difficulty.Hard, couplesOnly),

            D("The Forehead Hold",
              "Press your foreheads together and stay there — no talking, no kissing — for two full minutes.",
              "Tender", Difficulty.Medium, couplesOnly),

            D("Five Things I See",
              "Look at your partner's face for thirty seconds. Then close your eyes and describe five specific things you see — not general, specific.",
              "Tender", Difficulty.Medium, couplesOnly),

            D("Slow Dance",
              "Put on a song you both love. Dance to the whole thing. No irony.",
              "Tender", Difficulty.Medium, couplesOnly),

            D("The Ask",
              "Ask your partner for something you rarely ask for — comfort, closeness, space, a specific kind of touch. They give it.",
              "Tender", Difficulty.Hard, couplesOnly),

            D("The Hold",
              "Hold each other without saying anything for five minutes. No phones. Just stay.",
              "Tender", Difficulty.Medium, couplesOnly),

            D("Read Each Other",
              "Spend two minutes reading your partner's face. Then tell them what you think they're feeling right now — not what you expect them to feel, but what you actually observe.",
              "Tender", Difficulty.Hard, couplesOnly),

            D("The Hands",
              "Hold your partner's hands and look at them for one full minute. Then say one thing the hands make you think about.",
              "Tender", Difficulty.Medium, couplesOnly),

            D("The Thing You Haven't Said Tonight",
              "Each of you says one true thing about how you feel right now — not about the game, not about your day, but about this moment, sitting here.",
              "Tender", Difficulty.Hard, couplesOnly),

            // ════════════════════════════════════════════════════════════════
            // INTIMATE — for couples comfortable with physical and emotional depth
            // ════════════════════════════════════════════════════════════════

            D("Fifteen Minutes",
              "For the next fifteen minutes, your only job is to ask your partner what they want and give it to them. Keep asking until the fifteen minutes is up.",
              "Intimate", Difficulty.Extreme, couples18),

            D("The Specific Compliment",
              "Tell your partner three specific things about their body that you find beautiful — not the things you always say, but things you notice and don't mention.",
              "Intimate", Difficulty.Extreme, couples18),

            D("The Slow Kiss",
              "Kiss your partner — once, slowly. Take as long as they let you. Both of you pay complete attention.",
              "Intimate", Difficulty.Extreme, couples18),

            D("The Wish",
              "Each of you tells the other one intimate thing they wish happened more. No embarrassment. No negotiation required — just say it.",
              "Intimate", Difficulty.Extreme, couples18),

            D("The Touch Inventory",
              "Spend ten minutes exploring your partner's hands, arms, and face — slowly, without intent, just as an act of attention. They do nothing. Then swap.",
              "Intimate", Difficulty.Extreme, couples18),

            D("What You've Never Asked For",
              "Tell your partner something intimate you've wanted but have never asked for. They listen without reacting until you've finished.",
              "Intimate", Difficulty.Extreme, couples18),

            D("The Long Goodbye",
              "Pretend this is the last night you will ever spend together. How do you spend the next hour?",
              "Intimate", Difficulty.Extreme, couples18),
             D("The Tour Guide",
               "Give your partner a two-minute guided tour of the room you're in, as though it's a world heritage site and they have paid a considerable amount for this.",
               "Playful", Difficulty.Easy, couplesOnly),
             D("Your Greatest Hits",
               "Perform the chorus of a song that means something to the two of you. Choreography is optional but it is being scored.",
               "Playful", Difficulty.Easy, couplesOnly),
             D("Last Search",
               "Read out the last three things you searched for on your phone. No scrolling ahead, no editing, no explaining until all three are out.",
               "Playful", Difficulty.Medium, couplesOnly),
             D("The Thing I Nearly Said",
               "Tell them about a time recently when you nearly said something and didn't. Then say it.",
               "Honest", Difficulty.Medium, couplesOnly),
             D("The Compliment You Don't Believe",
               "Name a compliment they've given you that you've never quite believed. Let them argue for it.",
               "Honest", Difficulty.Medium, couplesOnly),
             D("What I Get Wrong",
               "Name the thing you know you do that makes life harder for them. No excuse attached and no promise attached — just name it, and let it sit.",
               "Honest", Difficulty.Hard, couplesOnly),
             D("Hands",
               "Take their hands and describe them out loud, in detail, as though you were memorising them.",
               "Tender", Difficulty.Medium, couplesOnly),
             D("Where It Started",
               "Tell the story of the moment you first knew — properly told, with the detail in, as though to someone who has never heard it.",
               "Tender", Difficulty.Medium, couplesOnly),
             D("The Photograph",
               "Find a photo of the two of you from at least a year ago. Each say one thing you remember about that day that the other doesn't know.",
               "Tender", Difficulty.Hard, couplesOnly),
             D("Ask Me Anything",
               "Your partner may ask you three questions about what you want. You answer all three honestly. Nothing asked here gets used against you later.",
               "Intimate", Difficulty.Extreme, couplesOnly),
             D("The Standing Invitation",
               "Tell your partner one thing you'd like more of — then agree a private signal either of you can use to ask for it on any ordinary day, without needing a conversation first.",
               "Intimate", Difficulty.Extreme, couplesOnly),
             D("Slow Hands",
               "For ten minutes, one of you does nothing but touch the other exactly how they ask to be touched. Then swap, if you both want to.",
               "Intimate", Difficulty.Extreme, couplesOnly),
        ];
    }

    private static ICard D(string title, string text, string category, Difficulty d, IRestriction restriction) =>
        StandardCard.Create(title, text, d, category, restriction: restriction);
}