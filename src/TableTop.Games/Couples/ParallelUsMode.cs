using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;
using TableTop.Core.Abstractions.Scoring;
using TableTop.Core.Domain.Cards;
using TableTop.Core.Domain.Scoring;
using TableTop.Games.Base;

namespace TableTop.Games.Couples;

/// <summary>
/// Parallel Us — co-written fiction starring the two of you, in every timeline
/// except this one.
///
/// How to play:
///   1. Draw a card. It drops the two of you into another era, another world,
///      or another version of your own story.
///   2. Tell it TOGETHER, alternating: one of you starts, the other continues,
///      back and forth in short beats until the card's question is answered.
///   3. House rule: "yes, and" — you may escalate your partner's last beat,
///      never erase it.
///   4. There are no points to win, only a shared cinematic universe to build.
///      Recurring characters are encouraged. Continuity errors are canon.
///
/// This isn't reminiscing (Memory Lane) or planning (Future Us) — it's pure
/// invention with your relationship as the raw material. Couples who play it
/// discover the same thing improvisers do: the story you build together is a
/// live demonstration of how you listen to each other.
/// </summary>
public sealed class ParallelUsMode : BaseGameModeDefinition
{
    /// <inheritdoc />
    public override string Name => "Parallel Us";
    /// <inheritdoc />
    public override string Description =>
        "Co-write the two of you into other eras, worlds, and what-ifs — alternating, 'yes-and', no points, all canon.";

    /// <summary>Label shown on the button that records a told story.</summary>
    public override string CompleteLabel => "Canon";
    /// <summary>Label shown on the button that skips the current card.</summary>
    public override string SkipLabel => "Different Timeline";

    /// <summary>Category → hex colour map used by UIs to tint card chrome.</summary>
    public override IReadOnlyDictionary<string, string> CategoryColours =>
        new Dictionary<string, string>
        {
            [ParallelUsCardBank.OtherErasCategory] = "#FFA726",
            [ParallelUsCardBank.OtherWorldsCategory] = "#AB47BC",
            [ParallelUsCardBank.SlidingDoorsCategory] = "#42A5F5",
            [ParallelUsCardBank.GenreSwapCategory] = "#EF5350",
            [ParallelUsCardBank.RemixTheRealCategory] = "#EC407A",
            [ParallelUsCardBank.EpiloguesCategory] = "#66BB6A",
            [ParallelUsCardBank.ForbiddenTimelinesCategory] = "#B71C4A",
            [ParallelUsCardBank.TonightsEpisodeCategory] = "#D97706",
        };

    /// <summary>Storytelling has no score — completion is its own reward.</summary>
    protected override IScoringStrategy BuildScoring() =>
        new FixedScoringStrategy(pointsPerCompletion: 0);

    /// <summary>Returns the built-in parallel-us card bank.</summary>
    protected override IReadOnlyList<ICard> BuildCards(IReadOnlyList<IPlayer> players) =>
        ParallelUsCardBank.All;

    /// <summary>Returns the card collection for this game mode.</summary>
    public static IReadOnlyList<ICard> GetCards() => ParallelUsCardBank.All;
}

/// <summary>Built-in card bank for Parallel Us.</summary>
public static class ParallelUsCardBank
{
    internal const string OtherErasCategory = "Other Eras";
    internal const string OtherWorldsCategory = "Other Worlds";
    internal const string SlidingDoorsCategory = "Sliding Doors";
    internal const string GenreSwapCategory = "Genre Swap";
    internal const string RemixTheRealCategory = "Remix the Real";
    internal const string EpiloguesCategory = "Epilogues";
    internal const string ForbiddenTimelinesCategory = "Forbidden Timelines";
    internal const string TonightsEpisodeCategory = "Tonight's Episode";

    private const string Deck = "Parallel Us";

    /// <summary>All parallel-us cards, ordered by category.</summary>
    public static IReadOnlyList<ICard> All { get; } = Build();

    private static IReadOnlyList<ICard> Build() =>
        CardDeckBuilder.For(Deck)
        // ── OTHER ERAS ────────────────────────────────────────────────────────
            .Category(OtherErasCategory)
            .Card(OtherErasCategory, BodyU("It's 1890 and you're both crew on the same tall ship. Tell the story of how you met on board — and what you were each secretly smuggling."), Difficulty.Easy)
            .Card(OtherErasCategory, BodyU("Ancient Rome. One of you is a chariot mechanic, the other a suspiciously good street poet. How does the empire's strangest partnership begin?"), Difficulty.Medium)
            .Card(OtherErasCategory, BodyU("The 1920s. You run a tiny jazz club together. Describe opening night — including the disaster and how you saved it."), Difficulty.Easy)
            .Card(OtherErasCategory, BodyU("Medieval times. One of you is the castle cook, the other the world's least frightening knight. Tell the tale the bards still sing about you."), Difficulty.Medium)
            .Card(OtherErasCategory, BodyU("The Stone Age. You two just invented something the tribe doesn't understand yet. What is it, and how does the demonstration go?"), Difficulty.Easy)
            .Card(OtherErasCategory, BodyU("1969. You both work at mission control — in the least glamorous department. What do you do, and how do you two quietly save the moon landing?"), Difficulty.Hard)
            .Card(OtherErasCategory, BodyU("Victorian London. You're rival street vendors on the same corner who join forces against a common enemy. Who — or what — is the enemy?"), Difficulty.Medium)
            .Card(OtherErasCategory, BodyU("The far future, year 3024. You're the last two people who remember a piece of 'ancient' 21st-century technology. What is it, and why does it matter now?"), Difficulty.Medium)

        // ── OTHER WORLDS ──────────────────────────────────────────────────────
            .Category(OtherWorldsCategory)
            .Card(OtherWorldsCategory, BodyU("You're both dragons. Describe your shared hoard — what do the two of you collect instead of gold, and who keeps trying to steal it?"), Difficulty.Easy)
            .Card(OtherWorldsCategory, BodyU("A fantasy kingdom. One of you is the monarch, the other the royal advisor who does all the actual work. Narrate one Tuesday."), Difficulty.Medium)
            .Card(OtherWorldsCategory, BodyU("You run the only inn at the crossroads of a magical realm. Tonight, a very strange guest arrives. Take turns describing the evening."), Difficulty.Easy)
            .Card(OtherWorldsCategory, BodyU("You're two ghosts haunting the same house — politely. Describe your haunting division of labour and your opinion of the current residents."), Difficulty.Medium)
            .Card(OtherWorldsCategory, BodyU("Space freighter, two-person crew, long haul. Something small has gone wrong. Tell the story of the repair — and what you talk about while fixing it."), Difficulty.Medium)
            .Card(OtherWorldsCategory, BodyU("You're both wizards, but your magical specialities are ridiculous. Name them, then narrate the emergency only your combined powers could solve."), Difficulty.Hard)
            .Card(OtherWorldsCategory, BodyU("Underwater city. One of you is the mayor; the other runs the last surface-food restaurant. A crisis hits during the lunch rush. Go."), Difficulty.Hard)
            .Card(OtherWorldsCategory, BodyU("You're two characters in a video game who have become self-aware between levels. What game is it, and what do you do with your five minutes of freedom?"), Difficulty.Medium)

        // ── SLIDING DOORS ─────────────────────────────────────────────────────
            .Category(SlidingDoorsCategory)
            .Card(SlidingDoorsCategory, BodyU("Replay the day you actually met — but one of you was five minutes late. Where does the timeline go, and how do you STILL end up meeting?"), Difficulty.Medium)
            .Card(SlidingDoorsCategory, BodyU("In this timeline, you met ten years earlier than you really did. Tell the story of that meeting — would you have liked each other yet?"), Difficulty.Hard)
            .Card(SlidingDoorsCategory, BodyU("You met exactly where you really met — but you'd swapped careers with each other. Replay the first conversation."), Difficulty.Medium)
            .Card(SlidingDoorsCategory, BodyU("The timeline where you became business partners INSTEAD of a couple. What's the company, and what do the employees suspect?"), Difficulty.Medium)
            .Card(SlidingDoorsCategory, BodyU("One small decision from your real first year together goes the other way. Pick it together, then narrate the six months that follow."), Difficulty.Extreme)
            .Card(SlidingDoorsCategory, BodyU("In this timeline you were neighbours for years before speaking. Tell the story of the day the silence finally broke — and what caused it."), Difficulty.Medium)
            .Card(SlidingDoorsCategory, BodyU("You still meet, fall for each other, and build a life — but in the other one's home town. What's different? What's stubbornly the same?"), Difficulty.Hard)

        // ── GENRE SWAP ────────────────────────────────────────────────────────
            .Category(GenreSwapCategory)
            .Card(GenreSwapCategory, BodyU("Your real first date, retold as a heist movie. One of you narrates the setup, the other the twist."), Difficulty.Easy)
            .Card(GenreSwapCategory, BodyU("Your relationship as a nature documentary. Take turns being the hushed narrator describing the other in their natural habitat."), Difficulty.Easy)
            .Card(GenreSwapCategory, BodyU("Retell your most mundane shared routine — the school run, the food shop — as an epic fantasy quest."), Difficulty.Easy)
            .Card(GenreSwapCategory, BodyU("Your last argument, retold as a courtroom drama where you each represent the OTHER side."), Difficulty.Hard)
            .Card(GenreSwapCategory, BodyU("Your relationship as a cooking show. Narrate the 'recipe' for the two of you — ingredients, method, and the bit where it nearly goes wrong."), Difficulty.Medium)
            .Card(GenreSwapCategory, BodyU("A horror movie where the monster is an ordinary household object you both genuinely dislike. Survive the night, together, out loud."), Difficulty.Medium)
            .Card(GenreSwapCategory, BodyU("Your love story as a news bulletin: anchor, field reporter, eyewitness quotes. Swap roles halfway through."), Difficulty.Medium)

        // ── REMIX THE REAL ────────────────────────────────────────────────────
            .Category(RemixTheRealCategory)
            .Card(RemixTheRealCategory, BodyU("Take a real holiday you've been on and add ONE impossible element — a talking animal, a hidden door, a time loop. Retell the trip."), Difficulty.Medium)
            .Card(RemixTheRealCategory, BodyU("Retell how you met, but you're both secretly spies from rival agencies. Which real details suddenly make PERFECT sense?"), Difficulty.Medium)
            .Card(RemixTheRealCategory, BodyU("Your actual home is revealed to have one magical room nobody else can see. Describe finding it together and what's inside."), Difficulty.Easy)
            .Card(RemixTheRealCategory, BodyU("A real gift one of you gave the other turns out to be enchanted. What does it do, and when did you each first notice?"), Difficulty.Medium)
            .Card(RemixTheRealCategory, BodyU("Rewrite a real minor disaster you survived together so that it was, in fact, a test staged by a mysterious organisation. Who are they? Did you pass?"), Difficulty.Hard)
            .Card(RemixTheRealCategory, BodyU("The pet you have (or the pet you'd get) can talk — but only to complain. Narrate its honest review of the two of you."), Difficulty.Easy)
        // ── EXPANSION: DARKER TIMELINES ──────────────────────────────────────
            .Category(GenreSwapCategory)
            .Card(GenreSwapCategory, BodyU("Film noir. Rain, neon, cigarette smoke you both refuse to explain. One of you walks into the other's office with a case. Alternate lines of gravelly narration."), Difficulty.Medium)
            .Card(GenreSwapCategory, BodyU("You're the VILLAIN couple of a superhero franchise — beloved by fans, feared by heroes. Names, matching aesthetic, and your extremely reasonable grievance."), Difficulty.Medium)
            .Card(GenreSwapCategory, BodyU("True-crime documentary — about the disappearance of your shared snacks. You are both suspects AND narrators. Reconstruct the timeline."), Difficulty.Easy)
            .Category(OtherWorldsCategory)
            .Card(OtherWorldsCategory, BodyU("The apocalypse happened and honestly? Your bunker is gorgeous. Give the full house tour, alternating rooms, including the one thing you each refused to leave behind."), Difficulty.Medium)
            .Card(OtherWorldsCategory, BodyU("You two run the underworld's most respected establishment — a laundromat where monsters bring their grievances. Narrate tonight's most delicate negotiation."), Difficulty.Hard)
            .Card(OtherWorldsCategory, BodyU("One of you is a ghost, the other the only person who can see you. It's been three years and frankly the arrangement works. Describe your Tuesday."), Difficulty.Hard)
            .Category(SlidingDoorsCategory)
            .Card(SlidingDoorsCategory, BodyU("The timeline where you became each other's nemesis instead — rival everything, same city, constant headlines. Tell the story of your legendary feud and the night it almost ended."), Difficulty.Hard)
            .Card(SlidingDoorsCategory, BodyU("You met exactly once, years before you actually met — and both forgot. Decide together where it was, then replay the thirty seconds neither of you remembers."), Difficulty.Extreme)
            .Category(RemixTheRealCategory)
            .Card(RemixTheRealCategory, BodyU("Your relationship, but every argument you've ever had was actually a scripted TV moment and the studio audience LOVED it. Re-perform your greatest hit with the laugh track."), Difficulty.Hard)
            .Card(RemixTheRealCategory, BodyU("One of you has secretly been a retired spy the whole relationship. Reveal it now, in-character, and let the other react — then swap and reveal YOUR secret career."), Difficulty.Medium)
            .Category(OtherErasCategory)
            .Card(OtherErasCategory, BodyU("Prohibition. You run a two-person speakeasy behind a flower shop. The password changes nightly and one of you keeps forgetting it. Narrate closing time."), Difficulty.Medium)
            .Card(OtherErasCategory, BodyU("Regency-era scandal: you two keep meeting at balls you both claim to hate. Alternate diary entries after the third 'accidental' encounter."), Difficulty.Hard)

        // ── FORBIDDEN TIMELINES (18+) — tension is the plot ──────────────────
            .Category(ForbiddenTimelinesCategory)
            .Card(ForbiddenTimelinesCategory, BodyU("Hotel bar, another city, and you're both pretending to be strangers. In character, from 'is this seat taken?' — see how long you last before someone breaks."), Difficulty.Medium)
            .Card(ForbiddenTimelinesCategory, BodyU("A masquerade ball. You each know EXACTLY who's behind the other's mask, and you both pretend you don't. Narrate the dance, alternating, staying in the lie."), Difficulty.Hard)
            .Card(ForbiddenTimelinesCategory, BodyU("The timeline where you're exes who meet again at a mutual friend's wedding, both looking infuriatingly good. Alternate inner monologues during the slow song neither of you is sitting out."), Difficulty.Hard)
            .Card(ForbiddenTimelinesCategory, BodyU("You're rival agents ordered to seduce information out of each other. Neither report will ever be filed. Narrate the dinner where you both realise the mission is compromised."), Difficulty.Hard)
            .Card(ForbiddenTimelinesCategory, BodyU("One of you is nobility, the other absolutely is not, and the whole court is watching. Describe the stolen conversation behind the orangery — what's said, what's very much not said."), Difficulty.Medium)
            .Card(ForbiddenTimelinesCategory, BodyU("The immortal and the mortal: one of you has loved the other across four separate lifetimes without ever confessing it. Tonight, in this lifetime, you finally do. Take it in turns."), Difficulty.Extreme)
            .Card(ForbiddenTimelinesCategory, BodyU("Co-stars whose on-screen chemistry has become a genuine industry problem. Narrate the take the director refuses to cut — and the silence in the trailer after 'that's a wrap'."), Difficulty.Hard)
            .Card(ForbiddenTimelinesCategory, BodyU("A storm strands you together in a lighthouse with one blanket, a bottle of something old, and a lot of unresolved history. Alternate paragraphs. The storm lasts as long as you need it to."), Difficulty.Medium)
            .Card(ForbiddenTimelinesCategory, BodyU("You're the villain and the hero, alone after the final battle, and neither of you is fighting anymore. Write the conversation the sequel will pretend never happened."), Difficulty.Extreme)
            .Card(ForbiddenTimelinesCategory, BodyU("Bodyguard timeline: one of you was hired to protect the other, professionalism was maintained for a record eleven days. Narrate day twelve."), Difficulty.Hard)
            .Card(ForbiddenTimelinesCategory, BodyU("The bookshop closes at nine. One of you owns it; the other has been 'browsing' for two hours every Thursday for a month. Tonight the shop stays open late. Alternate lines."), Difficulty.Medium)
            .Card(ForbiddenTimelinesCategory, BodyU("Write the deleted scene from YOUR real story — the moment early on when you both almost made a move and didn't. Then correct the historical record: tell it the way it SHOULD have gone."), Difficulty.Extreme)

        // ── FORBIDDEN TIMELINES, ROUND TWO (18+) ─────────────────────────────
            .Card(ForbiddenTimelinesCategory, BodyU("Rival chefs, adjacent restaurants, years of sabotage — and tonight you're trapped sharing one kitchen after both venues flood. Narrate the service, and what simmers besides the stock."), Difficulty.Medium)
            .Card(ForbiddenTimelinesCategory, BodyU("Painter and life model, final session of a commission that has taken suspiciously many sittings. Alternate: what the painter sees, what the model knows."), Difficulty.Hard)
            .Card(ForbiddenTimelinesCategory, BodyU("The tattoo appointment: one of you is the artist, the other is getting a design placed somewhere that requires... proximity. Three hours in the chair. Narrate the conversation that fills them."), Difficulty.Hard)
            .Card(ForbiddenTimelinesCategory, BodyU("Last train of the night, one compartment, two strangers, a five-hour journey and a shared bottle one of you happened to bring. Alternate lines until the train arrives — or don't let it."), Difficulty.Medium)
            .Card(ForbiddenTimelinesCategory, BodyU("Enemy pirate captains negotiating a truce alone in a cabin, both armed, both flirting, neither admitting either. Parley is such a strong word. Take turns."), Difficulty.Hard)
            .Card(ForbiddenTimelinesCategory, BodyU("The private wine tasting that was supposed to be professional. Six glasses in, the sommelier has stopped describing the wine and started describing the company. Alternate tasting notes."), Difficulty.Medium)
            .Card(ForbiddenTimelinesCategory, BodyU("Dance instructor and the student who booked the final slot of the evening, every week, despite clearly already knowing how to dance. Tonight the instructor calls it out. Mid-tango."), Difficulty.Hard)
            .Card(ForbiddenTimelinesCategory, BodyU("Two novelists snowed into a writers' retreat, deadline rivals, one working fireplace. You start co-writing out of spite. The book becomes a romance without either of you deciding it. Narrate the chapter where you both notice."), Difficulty.Extreme)
            .Card(ForbiddenTimelinesCategory, BodyU("The wrong-number call you kept answering: three months of late-night conversations with a stranger's voice, and tonight you finally agree to meet. Alternate the phone call where you set the place."), Difficulty.Medium)
            .Card(ForbiddenTimelinesCategory, BodyU("You're both understudies, secretly rehearsing the leads' love scene 'for professionalism', in an empty theatre, long after everyone left. Opening night is tomorrow. The rehearsal stops being rehearsal — narrate where exactly."), Difficulty.Extreme)
            .Card(ForbiddenTimelinesCategory, BodyU("Royal decoy and personal guard: your entire job is pretending to be royalty, theirs is never leaving your side. Tonight, on the palace roof, the pretending pauses. Alternate lines, titles optional."), Difficulty.Hard)
            .Card(ForbiddenTimelinesCategory, BodyU("Childhood pen pals who lost touch at fifteen and just recognised each other across a hotel lobby at a conference, twenty years of unwritten letters between you. Write them out loud — alternating, one 'letter' each, until you run out of past and start on the present."), Difficulty.Extreme)


        // ── EPILOGUES ─────────────────────────────────────────────────────────
            .Category(EpiloguesCategory)
            .Card(EpiloguesCategory, BodyU("It's fifty years from tonight, in the BEST timeline. You're telling a young couple how you two made it work. What's the one piece of advice you agree on?"), Difficulty.Hard)
            .Card(EpiloguesCategory, BodyU("A museum opens an exhibit about the two of you. Take turns describing the five items in the display case and the little cards next to them."), Difficulty.Medium)
            .Card(EpiloguesCategory, BodyU("The final scene of the movie about you both. Where is it set, what's the last line of dialogue, and what song plays over the credits?"), Difficulty.Hard)
            .Card(EpiloguesCategory, BodyU("Tell tonight — this exact evening, playing this game — as the opening scene of an adventure that starts the moment you put the phone down."), Difficulty.Extreme)

        // ── TONIGHT'S EPISODE (18+) — this evening, fictionalised LIVE ──────
            .Category(TonightsEpisodeCategory)
            .Card(TonightsEpisodeCategory, BodyU("Narrate the last ten minutes of your actual evening as a prestige-drama 'previously on…' recap — dramatic pauses, meaningful glances the camera 'caught'. End on a cliffhanger about what happens after this card."), Difficulty.Easy)
            .Card(TonightsEpisodeCategory, BodyU("You are being narrated by a nature documentarian RIGHT NOW. Take turns as the hushed voice-over describing the other's current behaviour — 'note how the taller one pretends not to be winning' — through the next two cards."), Difficulty.Medium)
            .Card(TonightsEpisodeCategory, BodyU("Tonight is secretly a first date and you're both trying to play it cool. From THIS SENTENCE onward, stay in character for three cards: the nervous compliments, the accidental hand touches, all of it."), Difficulty.Hard)
            .Card(TonightsEpisodeCategory, BodyU("A tabloid runs tomorrow's front page about tonight. Write the headline, the scandalous subheading, and the 'source close to the couple' quote — each of you contributes one of the three."), Difficulty.Medium)
            .Card(TonightsEpisodeCategory, BodyU("The director yells 'CUT — do the scene again, but this time with tension you could cut with a knife.' Replay the last five minutes of your real evening as that take."), Difficulty.Hard)
            .Card(TonightsEpisodeCategory, BodyU("Alternate writing tonight's episode description for the streaming menu — 45 words max, rated guidance included, and the phrase 'viewers are advised' must appear."), Difficulty.Medium)
            .Card(TonightsEpisodeCategory, BodyU("Your future selves are watching a home movie of tonight, fifty years from now, narrating over it. Voice them: what do they tease you about, and what does old-you whisper to old-them at the end?"), Difficulty.Extreme)
            .Card(TonightsEpisodeCategory, BodyU("The episode needs its final scene, and the writers' room is just you two. Pitch competing endings for tonight — then, per union rules, film the winning pitch."), Difficulty.Extreme)
            .Build();

    private static string BodyU(string prompt) =>
        "<b>📖 " + prompt + "</b>\n\n" +
        "Tell it together, alternating in short beats. \"Yes, and\" — build on each other, never erase.\n\n" +
        "<i>When the story finds its ending, it's canon.</i>";
}
