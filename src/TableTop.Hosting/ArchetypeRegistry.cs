using TableTop.Core.Abstractions.Game;
using TableTop.Games;
using TableTop.Games.Couples;
using TableTop.Games.FactOrFiction;
using TableTop.Games.Family;
using TableTop.Games.Fun;
using TableTop.Games.Party;
using TableTop.Games.School;

namespace TableTop.Hosting;

/// <summary>The default <see cref="IArchetypeRegistry"/> implementation. Owns all built-in game archetypes and the modes within them.</summary>
public sealed class ArchetypeRegistry : IArchetypeRegistry
{
    private readonly IReadOnlyList<Archetype> _roots;

    private ArchetypeRegistry(IReadOnlyList<Archetype> roots) => _roots = roots;

    /// <summary>Initialises a new <see cref="Default"/> instance.</summary>
    public static ArchetypeRegistry Default() => Build();

    // WithJsonModes lived here. It took runtime-loaded JsonGameMode instances,
    // bucketed each into exactly one section (school / fun / couples) by its
    // declared category with a prose-tag fallback, and built a registry with
    // "Custom …" nodes for them. It went with JsonGameMode in 1.21.0.
    //
    // The bucketing bug it fixed is worth keeping on record even though the code
    // is gone: the three buckets were once independent Where clauses over the
    // same list, so a mode described as "spicy party fun for two" appeared in
    // both fun and couples at once, and anything matching no clause fell through
    // to fun — the family-facing section. If user content ever returns, one mode
    // must land in exactly one bucket, and the fallback must not be the section
    // shown to families.

    /// <summary>RootArchetypes.</summary>
    public IReadOnlyList<Archetype> RootArchetypes => _roots;

    /// <summary>AllModes.</summary>
    public IReadOnlyList<IGameMode> AllModes =>
        _roots.SelectMany(r => r.AllModes).Distinct().ToList().AsReadOnly();

    /// <inheritdoc />
    public IGameMode? SurpriseMe(
        AgeRating maxAgeRating = AgeRating.Adult,
        bool allowAdultContent = false,
        int? maxCards = null)
    {
        var candidates = AllModes
            .Where(m =>
            {
                var manifest = m.GetManifest();
                if (!allowAdultContent && manifest.HasAdultContent) return false;
                if (maxCards.HasValue && manifest.TotalCards > maxCards.Value) return false;
                return true;
            })
            .ToList();

        return candidates.Count == 0
            ? null
            : candidates[Random.Shared.Next(candidates.Count)];
    }

    /// <inheritdoc />
    public Archetype? FindById(string id)
    {
        foreach (var root in _roots)
        {
            var found = FindIn(root, id);
            if (found is not null) return found;
        }
        return null;
    }

    private static ArchetypeRegistry Build() =>
        new(new List<Archetype>
        {
            BuildClassroom(),
            BuildFun(),
            BuildCouples(),
        }.AsReadOnly());

    // ── Classroom ─────────────────────────────────────────────────────────────

    private static Archetype BuildClassroom()
    {
        var subs = new List<Archetype>
        {
            // ── Curriculum games (age 11–12) ──────────────────────────────
            //
            // Flattened out of a "Grade 6" parent node. That node held these
            // thirteen and no modes of its own, which made the picker one level
            // deeper here than anywhere else — and MAUI needed a recursive mode
            // collector purely so tapping a branch node did not show an empty
            // list. The age targeting that grouping carried now lives in each
            // description instead, where it is visible without a tap.
            //
            // Two ids had to change to avoid colliding with the general-knowledge
            // entries below: grade6.quiz -> schoolmillionaire (Quiz Night already
            // owns classroom.quiz) and grade6.maths -> mentalmaths (Number World
            // already owns classroom.maths).
            new("classroom.schoolmillionaire", "School Millionaire",    "Who Wants to Be a Millionaire? with curriculum questions for age 11–12.",          "🏆", new List<IGameMode> { new SchoolMillionaireMode()    }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.mentalmaths",       "Mental Maths Sprint",   "Rapid-fire mental arithmetic — times tables, fractions, percentages, and BODMAS. Age 11–12.", "🧮", new List<IGameMode> { new MentalMathsSprintMode()   }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.math24",            "Math 24",               "Four numbers, all four used once, any of + − × ÷ — make exactly 24. Worked solution on the flip.", "🔢", new List<IGameMode> { new Math24Mode()             }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.spelling",          "Spelling Bee",          "Spell the word, use it in a sentence. From everyday words to real challenges.",     "🐝", new List<IGameMode> { new SpellingBeeMode()          }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.grammar",           "Grammar Quest",         "Find the error and fix the sentence. Punctuation, tense, pronouns and more.",       "✏️", new List<IGameMode> { new GrammarQuestMode()         }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.vocabulary",        "Vocabulary Builder",    "Define, use, and find synonyms for curriculum vocabulary. Age 11–12.",             "📖", new List<IGameMode> { new VocabularyBuilderMode()    }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.reading",           "Reading Comprehension", "Read the passage and answer — literal, inferential, and vocabulary questions.",     "📚", new List<IGameMode> { new ReadingComprehensionMode() }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.words",             "Word Detectives",       "Break the word into roots, prefixes, and suffixes. Latin and Greek etymology.",     "🔍", new List<IGameMode> { new WordDetectivesMode()       }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.figurative",        "Figurative Language",   "Name the device, explain the effect, write your own. Metaphor to paradox.",         "🌟", new List<IGameMode> { new FigurativeLanguageMode()   }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.stories",           "Story Starters",        "Begin a story, add a twist, or tell for 90 seconds with a constraint.",             "✍️", new List<IGameMode> { new StoryStartersMode()        }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.punctuation",       "Punctuation Wars",      "Place the marks, fix the errors, and explain the rule. From commas to semicolons.", "❗", new List<IGameMode> { new PunctuationWarsMode()      }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.oneword",           "One Word Wonder",       "Describe something in one word — everyone guesses.",                                "🤐", new List<IGameMode> { new OneWordWonderMode()        }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.constraint",        "Constraint Master",     "Write or speak the prompt following this weird rule. 90 seconds.",                 "🎯", new List<IGameMode> { new ConstraintMasterMode()     }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.paradox",           "The Paradox",           "Solve logical impossibilities. Best logic, creativity, or absurdity wins.",         "🤯", new List<IGameMode> { new TheParadoxMode()           }.AsReadOnly(), null, AgeRating.AllAges),

            new("classroom.quiz",       "Quiz Night",  "Who Wants to Be a Millionaire? — general knowledge for all ages.",        "🎓", new List<IGameMode> { new MillionaireMode()   }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.geography",  "World Explorer",    "Geography general knowledge — capitals, countries, landmarks, and natural wonders.",     "🌍", new List<IGameMode> { new WorldExplorerMode()  }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.science",    "Science Sprint",    "Science general knowledge — the body, animals, space, and everyday physics and chemistry.", "🔬", new List<IGameMode> { new ScienceSprintMode()  }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.history",    "Through the Ages",  "History & culture general knowledge — the ancient world, inventions, famous figures, and the arts.", "🏛️", new List<IGameMode> { new ThroughTheAgesMode() }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.sport",      "Sporting Chance",   "Sport general knowledge — rules and basics, equipment, the Olympics, and world sport.",   "⚽", new List<IGameMode> { new SportingChanceMode() }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.animals",    "Animal Kingdom",    "Nature & animals — record-breakers, baby animals, animal groups, habitats, and adaptations.", "🦁", new List<IGameMode> { new AnimalKingdomMode()  }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.music",      "Sound & Song",      "Music general knowledge — instruments, the orchestra, reading music, and music history.",  "🎵", new List<IGameMode> { new SoundAndSongMode()   }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.maths",      "Number World",      "Maths general knowledge — shapes, units, famous numbers, and maths vocabulary. Trivia, not drills.", "🔢", new List<IGameMode> { new NumberWorldMode()    }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.estimation", "Estimation Station", "Everyone secretly guesses the number — closest wins. Reasoning beats knowledge.", "📏", new List<IGameMode> { new EstimationStationMode() }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.wrong",      "Wrong Answers Only", "The real answer is read first — then everyone competes to invent the best WRONG one.", "🙃", new List<IGameMode> { new WrongAnswersOnlyMode() }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.oddoneout",  "Odd One Out",        "Four things, one impostor — everyone points on three, then flip for the rule.",        "🔍", new List<IGameMode> { new OddOneOutMode()        }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.logiclab",   "Logic Lab",          "Riddles, deductions, sequences, and liars — pure reasoning, answers explained on the flip.", "🧠", new List<IGameMode> { new LogicLabMode()         }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.explainitback", "Explain It Back", "Teach the concept in your own words. They answer the Check — you're graded on whether it landed.", "🎓", new List<IGameMode> { new ExplainItBackMode() }.AsReadOnly(), null, AgeRating.AllAges),
            new("classroom.icebreakers","Icebreakers", "Conversation starters and gentle get-to-know-you games.",                 "🤝", new List<IGameMode> { new GetToKnowYouMode(), new WouldYouRatherMode() }.AsReadOnly(), null, AgeRating.AllAges),
        };


        return new("classroom", "Classroom", "Safe for all ages. Curriculum games, general-knowledge quizzes, and icebreakers.", "🏫",
            new List<IGameMode>().AsReadOnly(), subs.AsReadOnly(), AgeRating.AllAges);
    }

    // ── Fun ───────────────────────────────────────────────────────────────────

    private static Archetype BuildFun()
    {
        var subs = new List<Archetype>
        {
            // ── Fact or Fiction ───────────────────────────────────────────────
            new(
                id:          "fun.factorfiction",
                name:        "Fact or Fiction",
                description: "Is it true or made up? Guess statements from mundane to outlandish.",
                emoji:       "🤔",
                modes:       new List<IGameMode>().AsReadOnly(),
                ageRating:   AgeRating.Teen,
                subArchetypes: new List<Archetype>
                {
                    new("fun.factorfiction.basic",  "Fact or Fiction",  "80 wild statements — guess which are true. Easy to Extreme.",                          "📋", new List<IGameMode> { new FactOrFictionMode()        }.AsReadOnly(), null, AgeRating.Teen),
                    new("fun.factorfiction.personal", "Personal Facts",  "Share three personal statements — two true, one false. Others guess the lie.",        "👤", new List<IGameMode> { new PersonalFactsMode()      }.AsReadOnly(), null, AgeRating.Teen),
                    new("fun.factorfiction.expert",  "Expert Facts",    "Topic-specific facts: History, Science, Pop Culture, Sports. Hard to spot the fakes.", "🧠", new List<IGameMode> { new ExpertFactOrFictionMode() }.AsReadOnly(), null, AgeRating.Teen),
                }.AsReadOnly()),

            // ── Family ────────────────────────────────────────────────────────
            new(
                id:          "fun.family",
                name:        "Family",
                description: "Games for all ages — silly dares, quiz, storytelling, and family conversation cards.",
                emoji:       "🏡",
                modes:       new List<IGameMode>().AsReadOnly(),
                ageRating:   AgeRating.AllAges,
                subArchetypes: new List<Archetype>
                {
                    new("fun.family.quiz",     "Family Quiz",    "80 general knowledge questions from ages 6 to adult. Who gets furthest?",                     "🏆", new List<IGameMode> { new FamilyQuizMode()    }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.dares",    "Family Dares",   "Silly, physical, performative dares. No winners — just chaos.",                               "😂", new List<IGameMode> { new FamilyDaresMode()   }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.letterrush","Letter Rush",   "One letter, five categories, 90 seconds — fill them all. Match someone and it's worth nothing.", "🔤", new List<IGameMode> { new LetterRushMode()    }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.actitout", "Act It Out",     "Charades for the family — mime it, no words. First to guess scores. Answer's on the back.",       "🎭", new List<IGameMode> { new ActItOutMode()     }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.drawit",   "Draw It",        "Pictionary for the family — draw it, everyone guesses. First guess scores. Answer's on the back.", "✏️", new List<IGameMode> { new DrawItMode()       }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.blankslate","Blank Slate",   "Fill in the blank, funniest answer wins. Pick from the shortlist or invent your own — the judge decides.", "🃏", new List<IGameMode> { new BlankSlateMode()   }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.stories",  "Family Stories", "Build a story together — one sentence at a time. Openings, twists, and solo challenges.",     "📖", new List<IGameMode> { new FamilyStoriesMode() }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.thisisus", "This Is Us",     "Stories, debates, and reveals about your family specifically. Who knows you best?",           "💛", new List<IGameMode> { new ThisIsUsMode()      }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.laugh",    "Laugh or Groan", "Silly dilemmas, ridiculous scenarios, and hot takes the whole family will argue about.",      "😄", new List<IGameMode> { new LaughOrGroanMode()  }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.caption",  "Caption This",   "Invent the funniest caption, headline, or one-liner for an absurd scene. Quickest wit wins.", "📸", new List<IGameMode> { new CaptionThisMode()   }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.rank",     "Rank This",  "Rank absurd things 1–5. Reveal. Argue about why.",      "⭐", new List<IGameMode> { new RankThisMode()     }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.verdict",  "The Verdict", "Vote on silly statements. See who agrees with chaos.", "🔨", new List<IGameMode> { new TheVerdictMode()   }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.chrono",   "Chronology Challenge", "Put events in order. How wrong were you?",              "📜", new List<IGameMode> { new ChronologyChallengeMode() }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.backwards",  "Backwards Story",    "Read the ending. Write the full story. Vote on which is best.",                "📖", new List<IGameMode> { new BackwardsStoryMode()    }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.emoji",     "Emoji Legends",      "Emoji sequence = film, song, book. Can you guess it?",                         "🧩", new List<IGameMode> { new EmojiLegendsMode()     }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.mono",      "Monologue Madness",  "60 seconds to improvise a speech on an absurd topic. Go.",                   "🎤", new List<IGameMode> { new MonologueMadnessMode() }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.survive",   "Survive This!",      "Rate your survival in ridiculous scenarios. Explain your strategy.",         "🏔️", new List<IGameMode> { new SurviveThisMode()      }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.taste",    "Taste The Colors",   "Cross your senses. What does Tuesday taste like?",                        "🌈", new List<IGameMode> { new TasteTheColorsMode()   }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.rhyme",    "Rhyme Battle",       "Starting word: shout rhymes. 5 seconds. Last one standing wins.",           "🎵", new List<IGameMode> { new RhymeBattleMode()      }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.liar",     "The Liar",           "Three tell a story. One is lying. Can you spot the impostor?",            "🤥", new List<IGameMode> { new TheLiarMode()          }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.predict",  "Predict This",       "Bet on what someone will answer. Correct predictions double the bet.",     "🎲", new List<IGameMode> { new PredictThisMode()      }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.sound",     "Sound Detective",    "Guess sound from abstract clues. Earlier guess = more points.",           "🔊", new List<IGameMode> { new SoundDetectiveMode()   }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.celeb",     "Celebrity Impersonator", "Act like someone famous. Can others guess who you are?",             "🎭", new List<IGameMode> { new CelebrityImpersonatorMode() }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.steal",    "Speed Steal",        "Answer fast. Others challenge. Better answer = steal your point.",        "⚡", new List<IGameMode> { new SpeedStealMode()       }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.blitz",    "Speed Blitz",        "Rapid-fire timed challenges — name things, answer trivia, solve riddles, against the clock.", "⏱️", new List<IGameMode> { new SpeedBlitzMode()       }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.touch",    "Touch & Tell",       "Blindfolded. Feel the object. Guess what it is in 30 seconds.",        "👐", new List<IGameMode> { new TouchAndTellMode()     }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.atlas",    "The Family Atlas",  "Draw one map of your family's world together — mountains you've crossed, the home you keep coming back to, and the places you haven't been yet. Bring paper. Keep the map.", "🗺️", new List<IGameMode> { new FamilyAtlasMode()      }.AsReadOnly(), null, AgeRating.AllAges),
                    new("fun.family.dicenight", "Dice Night",       "Roll two dice — the total picks your category, calm to chaotic. Doubles let you choose.",     "🎲", new List<IGameMode> { new DiceNightMode()    }.AsReadOnly(), null, AgeRating.AllAges),
                }.AsReadOnly()),

            // ── Party ─────────────────────────────────────────────────────────
            new("fun.party",       "Party Classics",    "Truth or Dare and Would You Rather — perfect for any group.",                       "🎉", new List<IGameMode> { new TruthOrDareMode(), new WouldYouRatherMode() }.AsReadOnly(), null, AgeRating.Teen),
            new("fun.questionable","Questionable Choices","Fill in the blank, worst-best answer wins. Dark, rude, and deeply tired — the judge decides.", "🃏", new List<IGameMode> { new QuestionableChoicesMode() }.AsReadOnly(), null, AgeRating.Adult),
            new("fun.lastorders", "Last Orders",         "Pub-night dares for grown-ups. Sips not shots, the soft option always counts the same.", "🍻", new List<IGameMode> { new LastOrdersMode() }.AsReadOnly(), null, AgeRating.Adult),
            new("fun.forbidden",   "Forbidden Words",   "Describe the word without saying the three words you most want to say.",            "🚫", new List<IGameMode> { new ForbiddenWordsMode() }.AsReadOnly(), null, AgeRating.AllAges),
            new("fun.superpowers", "Useless Superpowers", "Draw a terrible power, then pitch why it makes you the greatest hero alive.",       "🦸", new List<IGameMode> { new UselessSuperpowersMode() }.AsReadOnly(), null, AgeRating.AllAges),
            new("fun.onestar",     "One-Star Reviews",    "Deliver a scathing one-star review of something universally beloved.",              "⭐", new List<IGameMode> { new OneStarReviewsMode()     }.AsReadOnly(), null, AgeRating.AllAges),
            new("fun.alibi",       "Alibi",               "A silly crime, two suspects, one hastily agreed alibi — questioned separately.",     "🚨", new List<IGameMode> { new AlibiMode()              }.AsReadOnly(), null, AgeRating.AllAges),
            new("fun.slang",       "Slang Check",         "Hot-seat quiz of internet and dating slang — 15 rungs from 'rizz' to the deep cuts.", "💬", new List<IGameMode> { new SlangCheckMode()         }.AsReadOnly(), null, AgeRating.Teen),
            new("fun.sixtyseconds", "60 Seconds",          "One category, one sixty-second window — name as many as you can before the clock runs out.", "⏱️", new List<IGameMode> { new SixtySecondsMode()       }.AsReadOnly(), null, AgeRating.AllAges),
            new("fun.office",      "Office Safe",       "Get to Know You — professional, inclusive, everyone can join.",                     "🏢", new List<IGameMode> { new GetToKnowYouMode() }.AsReadOnly(), null, AgeRating.AllAges),
            new("fun.together",    "All Together Now",  "The table against the deck. One shared score — clear the target together or the deck takes it.", "🙌", new List<IGameMode> { new AllTogetherNowMode()  }.AsReadOnly(), null, AgeRating.AllAges),
            new("fun.teams",       "Split the Room",    "Two teams, alternating cards. One side performs, the other guesses, judges or races.",           "🎭", new List<IGameMode> { new SplitTheRoomMode()    }.AsReadOnly(), null, AgeRating.AllAges),
            new("fun.claimed",     "Claimed!",          "Challenge open ground to claim it, or raid a rival's territory to steal it. Hold three at once.", "🚩", new List<IGameMode> { new ClaimedMode()         }.AsReadOnly(), null, AgeRating.AllAges),
            new("fun.rollwithit",  "Roll With It",      "Roll two dice — the total picks your category, low-key to wild. Doubles let you choose.", "🎲", new List<IGameMode> { new RollWithItMode()      }.AsReadOnly(), null, AgeRating.Teen),
            new("fun.herd",        "Herd",              "Everyone answers at once. Score for matching the group — or for being the only one who said yours.", "🐑", new List<IGameMode> { new HerdMode()            }.AsReadOnly(), null, AgeRating.AllAges),
            new("fun.bigfive",     "Big Five",          "Fifty statements, five traits, no winner. See the shape you make — and how it reads against everyone else's.", "🧭", new List<IGameMode> { new BigFiveMode()        }.AsReadOnly(), null, AgeRating.AllAges),
            new("fun.thisorthat",  "This Or That",      "Two options side by side. Everyone picks at once — then find out what each choice says about you.", "🔀", new List<IGameMode> { new ThisOrThatMode()      }.AsReadOnly(), null, AgeRating.AllAges),
            new("fun.rivals",      "Rivals",            "Two teams. Every card comes in Easy, Hard and Brutal — and the other team picks which one you attempt.", "⚔️", new List<IGameMode> { new RivalsMode()          }.AsReadOnly(), null, AgeRating.Teen),
            new("fun.standing",    "Last One Standing", "Everyone attempts every card. Fail and you're out — and you become a judge.",                    "🏆", new List<IGameMode> { new LastOneStandingMode() }.AsReadOnly(), null, AgeRating.AllAges),
        };


        return new("fun", "Fun", "Party games, social dares, and crowd-pleasing challenges.", "🎉",
            new List<IGameMode>().AsReadOnly(), subs.AsReadOnly(), AgeRating.Teen);
    }

    // ── Couples ───────────────────────────────────────────────────────────────

    private static Archetype BuildCouples()
    {
        var subs = new List<Archetype>
        {
            // ── Connection ────────────────────────────────────────────────────
            new(
                id:          "couples.connection",
                name:        "Connection",
                description: "Conversation, closeness, and getting to know each other better. For any couple.",
                emoji:       "🌹",
                modes:       new List<IGameMode>().AsReadOnly(),
                ageRating:   AgeRating.Teen,
                subArchetypes: new List<Archetype>
                {
                    new("couples.connection.questions",    "Couples Questions",    "36 structured questions in three sets — goes deeper each time.",                    "💬", new List<IGameMode> { new CouplesQuestionsMode()   }.AsReadOnly(), null, AgeRating.Teen),
                    new("couples.connection.memory",       "Memory Lane",          "One memory, both your versions. How differently did you live the same moment?",     "📷", new List<IGameMode> { new MemoryLaneMode()          }.AsReadOnly(), null, AgeRating.Teen),
                    new("couples.connection.wish",         "Two Truths, One Wish", "Two true things, one wish — partner guesses which is the wish.",                    "✨", new List<IGameMode> { new TwoTruthsOneWishMode()    }.AsReadOnly(), null, AgeRating.Teen),
                    new("couples.connection.gtkY",         "Get to Know You",      "Conversation prompts from light to vulnerable. Four depth tiers.",                  "🗣️", new List<IGameMode> { new GetToKnowYouMode()        }.AsReadOnly(), null, AgeRating.Teen),
                    new("couples.connection.inyourshoes",  "In Your Shoes",        "Answer as your partner would — then they reveal the truth. How well do you really know them?",  "👟", new List<IGameMode> { new InYourShoesMode()         }.AsReadOnly(), null, AgeRating.Teen),
                    new("couples.connection.wyr",          "Would You Rather",     "Gender-directed dilemmas — the group guesses your choice before you reveal it.",    "🎭", new List<IGameMode> { new WouldYouRatherMode()      }.AsReadOnly(), null, AgeRating.Teen),
                    new("couples.connection.would",      "Would You Know?",      "Describe a relationship moment. Can your partner guess what happened?", "🎯", new List<IGameMode> { new WouldYouKnowMode() }.AsReadOnly(), null, AgeRating.Teen),
                    new("couples.connection.prophecy", "The Prophecy",       "Answer a question. Receive a hilariously vague fortune. Laugh.",    "🔮", new List<IGameMode> { new TheProphecyMode()   }.AsReadOnly(), null, AgeRating.Teen),
                    new("couples.connection.mindmeld",     "Mind Meld",            "Both secretly answer the same question — reveal together, score when you match.",   "🧠", new List<IGameMode> { new MindMeldMode()            }.AsReadOnly(), null, AgeRating.Adult),
                    new("couples.connection.parallel",     "Parallel Us",          "Co-write the two of you into other eras, worlds, and what-ifs. All canon.",         "🌌", new List<IGameMode> { new ParallelUsMode()          }.AsReadOnly(), null, AgeRating.Adult),
                    new("couples.connection.heatcheck",    "Heat Check",           "Every card at two temperatures — candle or fire. You choose together, every time.", "🌡️", new List<IGameMode> { new HeatCheckMode()           }.AsReadOnly(), null, AgeRating.Adult),
                    new("couples.connection.slowburn",     "Slow Burn",            "Sealed promises, beautiful almosts, and a pot of IOUs that pays out when the game ends.", "⏳", new List<IGameMode> { new SlowBurnMode()            }.AsReadOnly(), null, AgeRating.Adult),
                    new("couples.connection.allin",        "All In",               "A casino of flirtation — chips are kisses, bluffs get called, and the scoreboard leader claims the jackpot.", "🎰", new List<IGameMode> { new AllInMode()               }.AsReadOnly(), null, AgeRating.Adult),
                    new("couples.connection.modernlove",   "Millionaire: Modern Love", "Hot-seat quiz of dating-app slang — 15 rungs from ghosting to the deep cuts. Winnings convert to kisses.", "💘", new List<IGameMode> { new ModernLoveMillionaireMode() }.AsReadOnly(), null, AgeRating.Adult),
                    new("couples.connection.dayone",       "Day One",              "A 21-day campaign — one card unlocks per real day, Spark to Warmth to Embers. Miss a day and it waits for you.", "📅", new List<IGameMode> { new DayOneMode()              }.AsReadOnly(), null, AgeRating.Adult),
                    new("couples.connection.spyvsspouse",  "Spy vs Spouse",        "An innocent conversation game hiding a second, secret one — silent missions, cover stories, counterintelligence.", "🕵️", new List<IGameMode> { new SpyVsSpouseMode()         }.AsReadOnly(), null, AgeRating.Adult),
                    new("couples.connection.future",       "Future Us",            "A forward-looking conversation — from next year's plans to your wildest shared dreams.",     "🔮", new List<IGameMode> { new FutureUsMode() }.AsReadOnly(), null, AgeRating.Teen),
                    new("couples.connection.longgame",     "The Long Game",        "The quiet deck about noticing and keeping the good things — specific admiration, real thank-yous, honest promises.", "🌳", new List<IGameMode> { new TheLongGameMode()         }.AsReadOnly(), null, AgeRating.Adult),
                    new("couples.connection.lovelanguages", "Love Languages",      "Forty statements on how you each receive affection. No winner — you get a ranking, then you compare.", "💞", new List<IGameMode> { new LoveLanguagesMode() }.AsReadOnly(), null, AgeRating.Teen),
                    new("couples.connection.dynamics",     "Between the Two of You","A self-knowledge quiz on the dynamics of intimacy — lead/follow, give/receive, and more. Find your leans, then grow together.", "🧭", new List<IGameMode> { new BetweenTheTwoOfYouMode() }.AsReadOnly(), null, AgeRating.Adult),
                    new("couples.connection.cartographers","The Cartographers",    "Draw one map of your relationship as a country — terrain, cities, names, and the parts still unexplored. Keep the map.", "🗺️", new List<IGameMode> { new CartographersMode()      }.AsReadOnly(), null, AgeRating.Teen),
                }.AsReadOnly()),

            // ── Dares ─────────────────────────────────────────────────────────
            new(
                id:          "couples.dares",
                name:        "Dares",
                description: "Playful to intimate — dares built for two. Graduated from fun to tender to adult.",
                emoji:       "🔥",
                modes:       new List<IGameMode>().AsReadOnly(),
                ageRating:   AgeRating.Adult,
                subArchetypes: new List<Archetype>
                {
                    new("couples.dares.relationship", "Relationship Dares", "Four zones: playful, honest, tender, intimate. Both of you are in every dare.", "💫", new List<IGameMode> { new RelationshipDaresMode() }.AsReadOnly(), null, AgeRating.Adult),
                    new("couples.dares.truthordare",  "Truth or Dare",      "Classic truths and dares with couple-specific cards mixed in.",                 "😈", new List<IGameMode> { new TruthOrDareMode()        }.AsReadOnly(), null, AgeRating.Adult),
                }.AsReadOnly()),

            // ── Intimate ──────────────────────────────────────────────────────
            new("couples.intimate", "Intimate",
                "Monogamy, Afterglow, Undivided, Out Loud and Both Or Neither — five ways in, from zones of depth to a sealed reveal neither of you has to explain.",
                "💋", new List<IGameMode> { new MonogamyMode(), new AfterglowMode(), new UndividedMode(), new OutLoudMode(), new BothOrNeitherMode() }.AsReadOnly(), null, AgeRating.Adult),
        };


        return new("couples", "Couples", "For two. From gentle connection to full intimacy.", "💕",
            new List<IGameMode>().AsReadOnly(), subs.AsReadOnly(), AgeRating.Adult);
    }


    private static Archetype? FindIn(Archetype node, string id)
    {
        if (node.Id == id) return node;
        foreach (var child in node.SubArchetypes)
        {
            var found = FindIn(child, id);
            if (found is not null) return found;
        }
        return null;
    }
}