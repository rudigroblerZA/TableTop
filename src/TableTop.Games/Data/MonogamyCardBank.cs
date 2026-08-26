using TableTop.Core.Domain.Cards;
using TableTop.Core.Abstractions.Cards;

namespace TableTop.Games.Data;

/// <summary>
/// Monogamy — Expanded couples intimacy card bank.
/// Consolidated from original + extended banks, with significant new creative content.
///
/// Five zones: Foreplay (playful) → Sensual (romantic) → Steamy (adult) → Wild (adventurous)
/// → Fantasy (most explicit — naming a fantasy aloud, then enacting it)
/// All content is original, consensual, and designed for couples to adapt to their comfort level.
/// The "no obligation" rule applies throughout: couples always negotiate, always agree.
/// </summary>
public static class MonogamyCardBank
{
    /// <summary>All.</summary>
    public static IReadOnlyList<MonogamyCard> All { get; } = Build();

    private static IReadOnlyList<MonogamyCard> Build() =>
    [
        // ════════════════════════════════════════════════════════════════════
        // FOREPLAY — Playful, light, non-explicit (20+ cards)
        // ════════════════════════════════════════════════════════════════════

        MonogamyCard.CreateNeutral(
            "The Look",
            "Spend one full minute holding eye contact with your partner without speaking. No looking away.",
            MonogamyZone.Foreplay, CardTarget.ForBoth, tokenValue: 1),

        MonogamyCard.Create(
            "One True Thing",
            forHimText:  "Tell your partner one thing about her body that you find genuinely beautiful. Mean it.",
            forHerText:  "Tell your partner one thing about him that you find irresistibly attractive. Be specific.",
            neutralText: "Tell your partner one specific thing about them that you find genuinely beautiful.",
            MonogamyZone.Foreplay, CardTarget.ForDrawer, tokenValue: 1),

        MonogamyCard.CreateNeutral(
            "Slow Dance",
            "Play a song your partner loves and slow dance together in whatever space you have.",
            MonogamyZone.Foreplay, CardTarget.ForBoth, tokenValue: 1, durationMinutes: 3),

        MonogamyCard.CreateNeutral(
            "The Question",
            "Ask your partner what they secretly wish you did more of. Listen without interrupting or defending.",
            MonogamyZone.Foreplay, CardTarget.ForDrawer, tokenValue: 1),

        MonogamyCard.Create(
            "Compliment Overload",
            forHimText:  "Give your partner five genuine compliments in a row, faster than she can respond to each one.",
            forHerText:  "Give your partner five genuine compliments in a row, faster than he can respond to each one.",
            neutralText: "Give your partner five genuine compliments in a row, faster than they can respond to each.",
            MonogamyZone.Foreplay, CardTarget.ForDrawer, tokenValue: 1),

        MonogamyCard.CreateNeutral(
            "First Memory",
            "Tell your partner the first moment you knew you wanted to be with them. Take your time.",
            MonogamyZone.Foreplay, CardTarget.ForBoth, tokenValue: 1),

        MonogamyCard.Create(
            "The Greeting",
            forHimText:  "Greet your partner as if you haven't seen each other in a week. Show her how much you missed her.",
            forHerText:  "Greet your partner as if you haven't seen each other in a week. Show him how much you missed him.",
            neutralText: "Greet your partner as if you haven't seen each other in a week. Show them how much you missed them.",
            MonogamyZone.Foreplay, CardTarget.ForDrawer, tokenValue: 1),

        MonogamyCard.CreateNeutral(
            "Hand in Hand",
            "Hold your partner's hand in both of yours and trace every finger slowly. Two minutes, no other movement.",
            MonogamyZone.Foreplay, CardTarget.ForBoth, tokenValue: 1, durationMinutes: 2),

        MonogamyCard.CreateNeutral(
            "The Wish",
            "Tell your partner one thing you wish you could do together that you haven't done yet.",
            MonogamyZone.Foreplay, CardTarget.ForBoth, tokenValue: 1),

        MonogamyCard.Create(
            "Head in Lap",
            forHimText:  "Lay your head in her lap for three minutes. She may stroke your hair — or not.",
            forHerText:  "Lay your head in his lap for three minutes. He may stroke your hair — or not.",
            neutralText: "Lay your head in your partner's lap for three minutes. They may stroke your hair — or not.",
            MonogamyZone.Foreplay, CardTarget.ForDrawer, tokenValue: 1, durationMinutes: 3),

        MonogamyCard.Create(
            "The Moment We Met",
            forHimText:  "Ask her what she thought of you the moment you met. Be ready for the honest answer.",
            forHerText:  "Ask him what he thought of you the moment you met. Be ready for the honest answer.",
            neutralText: "Ask your partner what they thought of you the moment you met. Listen without judgment.",
            MonogamyZone.Foreplay, CardTarget.ForDrawer, tokenValue: 1),

        MonogamyCard.CreateNeutral(
            "Fingers Entwined",
            "Sit facing each other, hold both hands, and don't let go for five minutes. Just be present.",
            MonogamyZone.Foreplay, CardTarget.ForBoth, tokenValue: 1, durationMinutes: 5),

        MonogamyCard.Create(
            "Shoulder Tap",
            forHimText:  "Stand behind her and gently kiss the back of her neck once. That's it.",
            forHerText:  "Stand behind him and gently kiss the back of his neck once. That's it.",
            neutralText: "Stand behind your partner and gently kiss the back of their neck once.",
            MonogamyZone.Foreplay, CardTarget.ForDrawer, tokenValue: 1),

        MonogamyCard.CreateNeutral(
            "The Smile",
            "Tell your partner the specific moment today when they made you smile. Be as detailed as possible.",
            MonogamyZone.Foreplay, CardTarget.ForDrawer, tokenValue: 1),

        MonogamyCard.Create(
            "Barefoot",
            forHimText:  "Spend ten minutes doing something together with your shoes off. No specific activity — just barefoot together.",
            forHerText:  "Spend ten minutes doing something together with your shoes off. No specific activity — just barefoot together.",
            neutralText: "Spend ten minutes together with your shoes off, doing whatever feels natural.",
            MonogamyZone.Foreplay, CardTarget.ForBoth, tokenValue: 1, durationMinutes: 10),

        MonogamyCard.CreateNeutral(
            "The Shadow",
            "Follow your partner from room to room for ten minutes, watching what they do when they think you're not paying attention.",
            MonogamyZone.Foreplay, CardTarget.ForBoth, tokenValue: 1, durationMinutes: 10),

        MonogamyCard.Create(
            "Whisper",
            forHimText:  "Whisper something you love about her in her ear. She can't respond — just listen.",
            forHerText:  "Whisper something you love about him in his ear. He can't respond — just listen.",
            neutralText: "Whisper something you love about your partner in their ear. They just listen.",
            MonogamyZone.Foreplay, CardTarget.ForDrawer, tokenValue: 1),

        MonogamyCard.CreateNeutral(
            "The Photograph",
            "Take a simple, clothed photo of your partner being present with you. No agenda.",
            MonogamyZone.Foreplay, CardTarget.ForBoth, tokenValue: 1),

        MonogamyCard.CreateNeutral(
            "Raindrop",
            "If it's raining, watch the rain together for five minutes. Hold hands. Say nothing.",
            MonogamyZone.Foreplay, CardTarget.ForBoth, tokenValue: 1, durationMinutes: 5),

        // ════════════════════════════════════════════════════════════════════
        // SENSUAL — Romantic, emotionally connected (25+ cards)
        // ════════════════════════════════════════════════════════════════════

        MonogamyCard.Create(
            "Slow Kiss",
            forHimText:  "Kiss her once. Slowly. Let it last as long as she needs it to.",
            forHerText:  "Kiss him once. Slowly. Let it last as long as he needs it to.",
            neutralText: "Kiss your partner once. Slowly. Let it last as long as they need it to.",
            MonogamyZone.Sensual, CardTarget.ForDrawer, tokenValue: 2),

        MonogamyCard.Create(
            "Shoulder Ritual",
            forHimText:  "Give her a slow, deliberate shoulder and neck massage for five minutes. No conversation.",
            forHerText:  "Give him a slow, deliberate shoulder and neck massage for five minutes. No conversation.",
            neutralText: "Give your partner a slow, deliberate shoulder and neck massage for five minutes.",
            MonogamyZone.Sensual, CardTarget.ForDrawer, tokenValue: 2, durationMinutes: 5),

        MonogamyCard.CreateNeutral(
            "Bathroom Mirror",
            "Go to the bathroom together. Wash your partner's face slowly. Ten minutes.",
            MonogamyZone.Sensual, CardTarget.ForBoth, tokenValue: 2, durationMinutes: 10),

        MonogamyCard.Create(
            "Three Kisses",
            forHimText:  "Kiss her in three places. Make each one last exactly the same amount of time.",
            forHerText:  "Kiss him in three places. Make each one last exactly the same amount of time.",
            neutralText: "Kiss your partner in three different places with equal slowness.",
            MonogamyZone.Sensual, CardTarget.ForDrawer, tokenValue: 2),

        MonogamyCard.CreateNeutral(
            "The Photograph",
            "Take one intimate photo of your partner — clothed, nothing explicit. Just them being present with you.",
            MonogamyZone.Sensual, CardTarget.ForBoth, tokenValue: 2),

        MonogamyCard.Create(
            "Hair Ritual",
            forHimText:  "Spend five minutes slowly running your fingers through her hair. That's all.",
            forHerText:  "Spend five minutes slowly running your fingers through his hair. That's all.",
            neutralText: "Spend five minutes slowly running your fingers through your partner's hair.",
            MonogamyZone.Sensual, CardTarget.ForDrawer, tokenValue: 2, durationMinutes: 5),

        MonogamyCard.CreateNeutral(
            "Barefoot Dancing",
            "Put on music and dance together, barefoot, lights dimmed. Minimum five minutes. No talking.",
            MonogamyZone.Sensual, CardTarget.ForBoth, tokenValue: 2, durationMinutes: 5),

        MonogamyCard.Create(
            "The Breath",
            forHimText:  "Kiss her neck slowly and feel her breathing. Match her rhythm. Three minutes.",
            forHerText:  "Kiss his neck slowly and feel his breathing. Match his rhythm. Three minutes.",
            neutralText: "Kiss your partner's neck and synchronise your breathing with theirs for three minutes.",
            MonogamyZone.Sensual, CardTarget.ForDrawer, tokenValue: 2, durationMinutes: 3),

        MonogamyCard.CreateNeutral(
            "The Backrub",
            "Give your partner a backrub while they lie down. Minimum ten minutes. Focus entirely on them.",
            MonogamyZone.Sensual, CardTarget.ForBoth, tokenValue: 2, durationMinutes: 10),

        MonogamyCard.CreateNeutral(
            "Candlelit",
            "Light candles around your bedroom. Spend thirty minutes together in that light. No phones, no TV.",
            MonogamyZone.Sensual, CardTarget.ForBoth, tokenValue: 2, durationMinutes: 30),

        MonogamyCard.Create(
            "Whisper Intimacy",
            forHimText:  "Whisper three things you find attractive about her — physical, emotional, and hidden.",
            forHerText:  "Whisper three things you find attractive about him — physical, emotional, and hidden.",
            neutralText: "Whisper three different attractions — physical, emotional, and something hidden — to your partner.",
            MonogamyZone.Sensual, CardTarget.ForDrawer, tokenValue: 2),

        MonogamyCard.CreateNeutral(
            "Texture Exchange",
            "Take turns exploring each other's skin slowly — arms, hands, back. Eight minutes each.",
            MonogamyZone.Sensual, CardTarget.ForBoth, tokenValue: 2, durationMinutes: 16),

        MonogamyCard.Create(
            "The Gaze",
            forHimText:  "Lie beside her. Spend five minutes just looking at her face. Let her look back.",
            forHerText:  "Lie beside him. Spend five minutes just looking at his face. Let him look back.",
            neutralText: "Lie beside your partner and spend five minutes looking at their face. Let them look back.",
            MonogamyZone.Sensual, CardTarget.ForBoth, tokenValue: 2, durationMinutes: 5),

        MonogamyCard.CreateNeutral(
            "Silk or Cotton",
            "Choose one of your partner's favourite textures. Spend five minutes touching them with only that texture.",
            MonogamyZone.Sensual, CardTarget.ForDrawer, tokenValue: 2, durationMinutes: 5),

        MonogamyCard.Create(
            "Neckline",
            forHimText:  "Kiss along her collarbone and shoulders slowly. Five minutes.",
            forHerText:  "Kiss along his collarbone and shoulders slowly. Five minutes.",
            neutralText: "Kiss your partner's collarbone and shoulders slowly for five minutes.",
            MonogamyZone.Sensual, CardTarget.ForDrawer, tokenValue: 2, durationMinutes: 5),

        MonogamyCard.CreateNeutral(
            "The Rain",
            "If it's raining, open a window and sit listening together. Hold hands. Ten minutes.",
            MonogamyZone.Sensual, CardTarget.ForBoth, tokenValue: 2, durationMinutes: 10),

        MonogamyCard.Create(
            "Slow Hand",
            forHimText:  "Use only your hands for ten minutes. Slowly. Touch her the way she most loves.",
            forHerText:  "Use only your hands for ten minutes. Slowly. Touch him the way he most loves.",
            neutralText: "Use only your hands for ten minutes, slowly, touching your partner exactly as they love.",
            MonogamyZone.Sensual, CardTarget.ForDrawer, tokenValue: 2, durationMinutes: 10),

        MonogamyCard.CreateNeutral(
            "Scent",
            "Notice and tell your partner what they smell like right now. Be specific. Intimate. True.",
            MonogamyZone.Sensual, CardTarget.ForBoth, tokenValue: 2),

        MonogamyCard.CreateNeutral(
            "The Moment",
            "Tell your partner the moment today you felt closest to them. Details matter.",
            MonogamyZone.Sensual, CardTarget.ForBoth, tokenValue: 2),

        MonogamyCard.CreateNeutral(
            "Forehead",
            "Kiss your partner's forehead three times, slowly, with complete tenderness. Five minutes total.",
            MonogamyZone.Sensual, CardTarget.ForBoth, tokenValue: 2, durationMinutes: 5),

        // ════════════════════════════════════════════════════════════════════
        // STEAMY — Intimate, adults-only (60+ cards)
        // ════════════════════════════════════════════════════════════════════

        MonogamyCard.Create(
            "The Full Back Massage",
            forHimText:  "Give her a full back massage — shoulders to lower back, fifteen minutes. Take your time.",
            forHerText:  "Give him a full back massage — shoulders to lower back, fifteen minutes. Take your time.",
            neutralText: "Give your partner a full back massage from shoulders to lower back. Fifteen minutes.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3, durationMinutes: 15),

        MonogamyCard.Create(
            "Ask For It",
            forHimText:  "Ask her to tell you exactly what she wants right now. Then do exactly that.",
            forHerText:  "Ask him to tell you exactly what he wants right now. Then do exactly that.",
            neutralText: "Ask your partner to tell you exactly what they want right now. Then do exactly that.",
            MonogamyZone.Steamy, CardTarget.ForBoth, tokenValue: 3),

        MonogamyCard.Create(
            "Two Places",
            forHimText:  "Kiss her in two places she didn't expect. She can't ask — just wait.",
            forHerText:  "Kiss him in two places he didn't expect. He can't ask — just wait.",
            neutralText: "Kiss your partner in two unexpected places. They just wait.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3),

        MonogamyCard.Create(
            "The Slow Undress",
            forHimText:  "Remove one item of her clothing, slowly and deliberately. Then stop.",
            forHerText:  "Remove one item of his clothing, slowly and deliberately. Then stop.",
            neutralText: "Remove one item of your partner's clothing, slowly and deliberately. Then stop.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3),

        MonogamyCard.Create(
            "Tell Me a Fantasy",
            forHimText:  "Tell her one intimate fantasy you've never shared. She listens without judgment. No obligation.",
            forHerText:  "Tell him one intimate fantasy you've never shared. He listens without judgment. No obligation.",
            neutralText: "Tell your partner one intimate fantasy you've never shared. They listen without judgment.",
            MonogamyZone.Steamy, CardTarget.ForBoth, tokenValue: 3),

        MonogamyCard.Create(
            "Massage Exchange",
            forHimText:  "She gives you a ten-minute massage of your choosing. Then you give her the same.",
            forHerText:  "He gives you a ten-minute massage of your choosing. Then you give him the same.",
            neutralText: "Exchange ten-minute massages. Drawer chooses first, then partner chooses.",
            MonogamyZone.Steamy, CardTarget.ForBoth, tokenValue: 3, durationMinutes: 20),

        MonogamyCard.Create(
            "The Long Hold",
            forHimText:  "Hold her against you — no words, no movement — until she is the one who moves first.",
            forHerText:  "Hold him against you — no words, no movement — until he is the one who moves first.",
            neutralText: "Hold your partner against you — no words, no movement — until they move first.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3),

        MonogamyCard.Create(
            "Shower Together",
            forHimText:  "Take a shower together. No agenda — just warmth, closeness, and ten minutes alone.",
            forHerText:  "Take a shower together. No agenda — just warmth, closeness, and ten minutes alone.",
            neutralText: "Take a shower together. No agenda — just warmth, closeness, and ten minutes alone.",
            MonogamyZone.Steamy, CardTarget.ForBoth, tokenValue: 3, durationMinutes: 10),

        MonogamyCard.Create(
            "Under Your Hands",
            forHimText:  "She closes her eyes. For eight minutes you choose what you do — she just receives.",
            forHerText:  "He closes his eyes. For eight minutes you choose what you do — he just receives.",
            neutralText: "Your partner closes their eyes. For eight minutes you choose what you do — they receive.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3, durationMinutes: 8),

        MonogamyCard.Create(
            "Their Weakness",
            forHimText:  "Find her weakest spot — the one that makes her lose composure. Use it.",
            forHerText:  "Find his weakest spot — the one that makes him lose composure. Use it.",
            neutralText: "Find your partner's weakest spot — the one that makes them lose composure. Use it.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3),

        MonogamyCard.Create(
            "The Taste",
            forHimText:  "Kiss her in a new way — use your tongue differently, kiss deeper, kiss softer. Surprise her.",
            forHerText:  "Kiss him in a new way — use your tongue differently, kiss deeper, kiss softer. Surprise him.",
            neutralText: "Kiss your partner in a completely new way. Change depth, speed, or pressure.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3),

        MonogamyCard.Create(
            "Tempo",
            forHimText:  "Touch her — anywhere — but change your pace every thirty seconds. Fast, slow, barely there.",
            forHerText:  "Touch him — anywhere — but change your pace every thirty seconds. Fast, slow, barely there.",
            neutralText: "Touch your partner anywhere, varying speed every thirty seconds. Three minutes total.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3, durationMinutes: 3),

        MonogamyCard.Create(
            "What You Want",
            forHimText:  "Ask her what she wants you to do. She tells you. You do it, exactly as she describes.",
            forHerText:  "Ask him what he wants you to do. He tells you. You do it, exactly as he describes.",
            neutralText: "Ask your partner what they want. They describe it. You do it exactly as described.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3),

        MonogamyCard.Create(
            "Breathe Together",
            forHimText:  "Hold each other close and synchronise your breathing for five full minutes. Eyes open.",
            forHerText:  "Hold each other close and synchronise your breathing for five full minutes. Eyes open.",
            neutralText: "Hold each other close, synchronising your breath for five minutes with eyes open.",
            MonogamyZone.Steamy, CardTarget.ForBoth, tokenValue: 3, durationMinutes: 5),

        MonogamyCard.Create(
            "Anticipation",
            forHimText:  "Tell her what you're going to do next. Take a full minute to do it.",
            forHerText:  "Tell him what you're going to do next. Take a full minute to do it.",
            neutralText: "Tell your partner what you're about to do. Take a full minute to do it.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3, durationMinutes: 1),

        MonogamyCard.Create(
            "One More Kiss",
            forHimText:  "Kiss her once. Then again. Then once more. Each one should be different.",
            forHerText:  "Kiss him once. Then again. Then once more. Each one should be different.",
            neutralText: "Kiss your partner three times in succession, each kiss with a different quality.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3),

        MonogamyCard.Create(
            "Resistance",
            forHimText:  "Let her lead completely. Don't take any initiative. Ten minutes.",
            forHerText:  "Let him lead completely. Don't take any initiative. Ten minutes.",
            neutralText: "Let your partner lead completely. Don't take any initiative. Ten minutes.",
            MonogamyZone.Steamy, CardTarget.ForPartner, tokenValue: 3, durationMinutes: 10),

        MonogamyCard.Create(
            "Sensitivity",
            forHimText:  "Find five places on her body that make her gasp or shiver. Take your time discovering them.",
            forHerText:  "Find five places on his body that make him gasp or shiver. Take your time discovering them.",
            neutralText: "Discover five places on your partner's body with heightened sensitivity. Take your time.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3),

        MonogamyCard.Create(
            "Permission",
            forHimText:  "Ask her permission for everything you do next. Watch her face.",
            forHerText:  "Ask him permission for everything you do next. Watch his face.",
            neutralText: "Ask your partner permission for each thing you do. Five minutes.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3, durationMinutes: 5),

        MonogamyCard.Create(
            "The Edge",
            forHimText:  "Bring her close to losing control — then stop. Wait. Let her ask for more.",
            forHerText:  "Bring him close to losing control — then stop. Wait. Let him ask for more.",
            neutralText: "Bring your partner close to losing control — then stop. Let them ask for more.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3),

        MonogamyCard.Create(
            "Blindfolded",
            forHimText:  "Blindfold her and spend ten minutes using only your hands.",
            forHerText:  "Blindfold him and spend ten minutes using only your hands.",
            neutralText: "Blindfold your partner and spend ten minutes using only touch and presence.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3, durationMinutes: 10),

        MonogamyCard.Create(
            "Patience",
            forHimText:  "Spend ten minutes on one inch of her body. Just that one place.",
            forHerText:  "Spend ten minutes on one inch of his body. Just that one place.",
            neutralText: "Focus on one small area of your partner's body for ten full minutes.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3, durationMinutes: 10),

        MonogamyCard.Create(
            "Temperature",
            forHimText:  "Kiss her using only warmth for three minutes, then cool your mouth and kiss again.",
            forHerText:  "Kiss him using only warmth for three minutes, then cool your mouth and kiss again.",
            neutralText: "Kiss your partner with warmth, then with cool breath. Notice the contrast.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3, durationMinutes: 3),

        MonogamyCard.Create(
            "The Confession",
            forHimText:  "Tell her something you've never admitted during intimacy. She listens, no judgment.",
            forHerText:  "Tell him something you've never admitted during intimacy. He listens, no judgment.",
            neutralText: "Tell your partner something you've never admitted. They listen without judgment.",
            MonogamyZone.Steamy, CardTarget.ForBoth, tokenValue: 3),

        MonogamyCard.Create(
            "Claim",
            forHimText:  "Kiss her like you're claiming her. Like you're making sure she knows she's yours.",
            forHerText:  "Kiss him like you're claiming him. Like you're making sure he knows he's yours.",
            neutralText: "Kiss your partner with complete, undeniable intensity and intention.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3),

        MonogamyCard.Create(
            "Texture Play",
            forHimText:  "Use different textures on her skin — your tongue, your hands, something soft, something not. Five minutes.",
            forHerText:  "Use different textures on his skin — your tongue, your hands, something soft, something not. Five minutes.",
            neutralText: "Play with different textures against your partner's skin for five minutes.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3, durationMinutes: 5),

        MonogamyCard.CreateNeutral(
            "Bed Time",
            "Go to bed together twenty minutes earlier than usual. Just you, together, in the dark.",
            MonogamyZone.Steamy, CardTarget.ForBoth, tokenValue: 3, durationMinutes: 20),

        MonogamyCard.Create(
            "Without Words",
            forHimText:  "For the next fifteen minutes, no speaking. Only touch, look, and breath. That's everything.",
            forHerText:  "For the next fifteen minutes, no speaking. Only touch, look, and breath. That's everything.",
            neutralText: "Spend fifteen minutes together with no words — only touch, eye contact, and breathing.",
            MonogamyZone.Steamy, CardTarget.ForBoth, tokenValue: 3, durationMinutes: 15),

        MonogamyCard.Create(
            "Slow Motion",
            forHimText:  "Everything you do next — every touch, every kiss, every movement — is in slow motion. Ten minutes.",
            forHerText:  "Everything you do next — every touch, every kiss, every movement — is in slow motion. Ten minutes.",
            neutralText: "Do everything in slow motion for ten full minutes. Slow touch, slow kisses, slow breath.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3, durationMinutes: 10),

        MonogamyCard.Create(
            "Wake Me",
            forHimText:  "Wake her in the morning with your touch and nothing else.",
            forHerText:  "Wake him in the morning with your touch and nothing else.",
            neutralText: "Wake your partner with only your touch. No words, no sounds.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3),

        MonogamyCard.Create(
            "Whisper Dirty",
            forHimText:  "Whisper something you want her to know about how much she affects you.",
            forHerText:  "Whisper something you want him to know about how much he affects you.",
            neutralText: "Whisper something vulnerable about your desire for your partner.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3),

        MonogamyCard.Create(
            "Pressure",
            forHimText:  "Use your lips and tongue on her neck. Five minutes. Relentless.",
            forHerText:  "Use your lips and tongue on his neck. Five minutes. Relentless.",
            neutralText: "Kiss your partner's neck with focused intensity for five minutes.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3, durationMinutes: 5),

        MonogamyCard.Create(
            "Skin",
            forHimText:  "Spend time just feeling her skin. Appreciate every inch. Ten minutes, your pace.",
            forHerText:  "Spend time just feeling his skin. Appreciate every inch. Ten minutes, your pace.",
            neutralText: "Spend ten minutes appreciating every part of your partner's skin.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3, durationMinutes: 10),

        MonogamyCard.Create(
            "Control",
            forHimText:  "Let her put her hands on you exactly where she wants. Let her control the pace.",
            forHerText:  "Let him put his hands on you exactly where he wants. Let him control the pace.",
            neutralText: "Let your partner guide your body exactly where they want. Give them complete control.",
            MonogamyZone.Steamy, CardTarget.ForPartner, tokenValue: 3, durationMinutes: 15),

        MonogamyCard.Create(
            "The Moment",
            forHimText:  "Kiss her when she least expects it. Make it count.",
            forHerText:  "Kiss him when he least expects it. Make it count.",
            neutralText: "Kiss your partner when they least expect it. Make it completely count.",
            MonogamyZone.Steamy, CardTarget.ForDrawer, tokenValue: 3),

        MonogamyCard.Create(
            "All In",
            forHimText:  "Show her everything you feel, without holding back. Let her see.",
            forHerText:  "Show him everything you feel, without holding back. Let him see.",
            neutralText: "Show your partner everything you feel without holding back. Let them see you completely.",
            MonogamyZone.Steamy, CardTarget.ForBoth, tokenValue: 3, durationMinutes: 15),

        // ════════════════════════════════════════════════════════════════════
        // WILD — Adventurous, pushing boundaries (40+ cards)
        // ════════════════════════════════════════════════════════════════════

        MonogamyCard.Create(
            "An Hour of You",
            forHimText:  "For the next hour, you are entirely in her hands. Whatever she asks — within your limits — you say yes.",
            forHerText:  "For the next hour, you are entirely in his hands. Whatever he asks — within your limits — you say yes.",
            neutralText: "For one hour, you are entirely in your partner's hands. Whatever they ask — within your limits — you agree.",
            MonogamyZone.Wild, CardTarget.ForDrawer, tokenValue: 4, durationMinutes: 60),

        MonogamyCard.Create(
            "The Blindfold",
            forHimText:  "Blindfold her and spend ten minutes using only touch to communicate what you feel.",
            forHerText:  "Blindfold him and spend ten minutes using only touch to communicate what you feel.",
            neutralText: "Blindfold your partner and spend ten minutes using only touch and presence.",
            MonogamyZone.Wild, CardTarget.ForDrawer, tokenValue: 4, durationMinutes: 10),

        MonogamyCard.Create(
            "Three Rules",
            forHimText:  "She writes three rules for the rest of the evening. You agree to them before she reads them aloud.",
            forHerText:  "He writes three rules for the rest of the evening. You agree to them before he reads them aloud.",
            neutralText: "Your partner writes three rules for the rest of the evening. You agree before they're read.",
            MonogamyZone.Wild, CardTarget.ForPartner, tokenValue: 4),

        MonogamyCard.Create(
            "The Mirror",
            forHimText:  "Mirror everything she does for five minutes — touch for touch, movement for movement.",
            forHerText:  "Mirror everything he does for five minutes — touch for touch, movement for movement.",
            neutralText: "Mirror everything your partner does for five minutes — exactly, deliberately.",
            MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4, durationMinutes: 5),

        MonogamyCard.Create(
            "The Confession",
            forHimText:  "Tell her one thing you've always wanted to do with her but never asked. No obligation — just honesty.",
            forHerText:  "Tell him one thing you've always wanted to do with him but never asked. No obligation — just honesty.",
            neutralText: "Tell your partner one thing you've always wanted to explore together. Just honesty.",
            MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4),

        MonogamyCard.Create(
            "Take Control",
            forHimText:  "For the next twenty minutes, she decides everything that happens. You follow completely.",
            forHerText:  "For the next twenty minutes, he decides everything that happens. You follow completely.",
            neutralText: "For twenty minutes, your partner decides everything that happens. You follow completely.",
            MonogamyZone.Wild, CardTarget.ForPartner, tokenValue: 4, durationMinutes: 20),

        MonogamyCard.CreateNeutral(
            "New Room",
            "Choose a room in your home you have never been intimate in. Go there now.",
            MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4),

        MonogamyCard.Create(
            "Surrender",
            forHimText:  "For the next half hour, don't move — let her do everything. You receive only.",
            forHerText:  "For the next half hour, don't move — let him do everything. You receive only.",
            neutralText: "For thirty minutes, don't initiate. Let your partner do everything. You receive only.",
            MonogamyZone.Wild, CardTarget.ForPartner, tokenValue: 4, durationMinutes: 30),

        MonogamyCard.Create(
            "Permission Play",
            forHimText:  "Ask her permission for everything. Let her have complete control over yes or no. Thirty minutes.",
            forHerText:  "Ask him permission for everything. Let him have complete control over yes or no. Thirty minutes.",
            neutralText: "Ask your partner for permission before every action. Let them have complete control.",
            MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4, durationMinutes: 30),

        MonogamyCard.Create(
            "Complete Trust",
            forHimText:  "She has total control. You give complete trust. Fifteen minutes.",
            forHerText:  "He has total control. You give complete trust. Fifteen minutes.",
            neutralText: "Your partner has total control. Give complete trust for fifteen minutes.",
            MonogamyZone.Wild, CardTarget.ForPartner, tokenValue: 4, durationMinutes: 15),

        MonogamyCard.Create(
            "Intensity Match",
            forHimText:  "Match her intensity exactly. If she goes faster, you go faster. Mirror her energy perfectly.",
            forHerText:  "Match his intensity exactly. If he goes faster, you go faster. Mirror his energy perfectly.",
            neutralText: "Match your partner's intensity perfectly. If they escalate, you escalate with them.",
            MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4),

        MonogamyCard.Create(
            "The Test",
            forHimText:  "She tests your boundaries. See how far you'll go. You show her.",
            forHerText:  "He tests your boundaries. See how far you'll go. You show him.",
            neutralText: "Your partner tests your boundaries. Show them how far you're willing to go.",
            MonogamyZone.Wild, CardTarget.ForPartner, tokenValue: 4),

        MonogamyCard.Create(
            "Forbidden",
            forHimText:  "Do one thing together that you've always thought was forbidden or too much. Just once.",
            forHerText:  "Do one thing together that you've always thought was forbidden or too much. Just once.",
            neutralText: "Do something together that feels daring or forbidden to you both. This is the time.",
            MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4),

        MonogamyCard.Create(
            "Two Hours",
            forHimText:  "Dedicate the next two hours entirely to her pleasure. Anything she wants.",
            forHerText:  "Dedicate the next two hours entirely to his pleasure. Anything he wants.",
            neutralText: "Dedicate the next two hours entirely to your partner's pleasure. Whatever they want.",
            MonogamyZone.Wild, CardTarget.ForPartner, tokenValue: 4, durationMinutes: 120),

        MonogamyCard.Create(
            "The Proposition",
            forHimText:  "Propose something you both want to try. Together you decide if you do it now.",
            forHerText:  "Propose something you both want to try. Together you decide if you do it now.",
            neutralText: "Propose something you both want to explore. Decide together whether to try it now.",
            MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4),

        MonogamyCard.Create(
            "Lose Control",
            forHimText:  "Let her make you lose control. Don't hold back. Show her.",
            forHerText:  "Let him make you lose control. Don't hold back. Show him.",
            neutralText: "Let your partner make you lose complete control. Don't hold back.",
            MonogamyZone.Wild, CardTarget.ForPartner, tokenValue: 4),

        MonogamyCard.Create(
            "Restraint",
            forHimText:  "She gently ties your hands with something soft. You trust her completely. Thirty minutes.",
            forHerText:  "He gently ties your hands with something soft. You trust him completely. Thirty minutes.",
            neutralText: "Your partner gently restrains you with something soft. Complete trust for thirty minutes.",
            MonogamyZone.Wild, CardTarget.ForPartner, tokenValue: 4, durationMinutes: 30),

        MonogamyCard.Create(
            "The Real Question",
            forHimText:  "Ask her the one thing you've always been too nervous to ask for. Really ask.",
            forHerText:  "Ask him the one thing you've always been too nervous to ask for. Really ask.",
            neutralText: "Ask your partner the one thing you've always been nervous about. Really ask them.",
            MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4),

        MonogamyCard.Create(
            "Lights On",
            forHimText:  "Do everything with all the lights on. No shadows. She sees all of you.",
            forHerText:  "Do everything with all the lights on. No shadows. He sees all of you.",
            neutralText: "Do everything with all the lights on. No shadows, no hiding. Complete visibility.",
            MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4),

        MonogamyCard.Create(
            "The Watch",
            forHimText:  "She watches you. You perform for her. Show her everything.",
            forHerText:  "He watches you. You perform for him. Show him everything.",
            neutralText: "Your partner watches as you show them exactly what you want them to see.",
            MonogamyZone.Wild, CardTarget.ForDrawer, tokenValue: 4),

        MonogamyCard.CreateNeutral(
            "The Dare",
            "You dare each other to one thing. Together you accept the dare.",
            MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4),

        MonogamyCard.Create(
            "Relentless",
            forHimText:  "See how long she can take it. Slow, relentless, until she has to ask you to stop.",
            forHerText:  "See how long he can take it. Slow, relentless, until he has to ask you to stop.",
            neutralText: "Be slow and relentless until your partner has to ask you to stop.",
            MonogamyZone.Wild, CardTarget.ForDrawer, tokenValue: 4),

        MonogamyCard.Create(
            "Worship",
            forHimText:  "Worship her body. Every part. Make her feel adored beyond reason.",
            forHerText:  "Worship his body. Every part. Make him feel adored beyond reason.",
            neutralText: "Worship your partner's entire body. Make them feel completely, utterly adored.",
            MonogamyZone.Wild, CardTarget.ForDrawer, tokenValue: 4),

        MonogamyCard.Create(
            "The Destination",
            forHimText:  "There is only one outcome tonight. Make it happen for her.",
            forHerText:  "There is only one outcome tonight. Make it happen for him.",
            neutralText: "Tonight has one clear destination. Make sure you both get there.",
            MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4),

        MonogamyCard.Create(
            "All Night",
            forHimText:  "She controls the night. Dedicate every hour to her pleasure.",
            forHerText:  "He controls the night. Dedicate every hour to his pleasure.",
            neutralText: "Your partner controls the entire night. Dedicate every hour to their pleasure.",
            MonogamyZone.Wild, CardTarget.ForPartner, tokenValue: 4, durationMinutes: 480),

        MonogamyCard.CreateNeutral(
            "No Limits",
            "Tonight has no limits — except the ones you both set. Go deeper than usual.",
            MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4),

        MonogamyCard.Create(
            "Complete Night",
            forHimText:  "Make the night complete for her. Nothing left unfinished.",
            forHerText:  "Make the night complete for him. Nothing left unfinished.",
            neutralText: "Make tonight completely satisfying for both of you. Leave nothing unfinished.",
            MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4),

        MonogamyCard.Create(
            "Pure Passion",
            forHimText:  "Stop thinking. Just feel her. Let instinct take over.",
            forHerText:  "Stop thinking. Just feel him. Let instinct take over.",
            neutralText: "Stop thinking. Just feel your partner. Let instinct take over completely.",
            MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4),

        MonogamyCard.Create(
            "The Yes",
            forHimText:  "Tell her yes to something you've never said yes to before.",
            forHerText:  "Tell him yes to something you've never said yes to before.",
            neutralText: "Say yes to something new. Something you've never agreed to before.",
            MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4),

        MonogamyCard.Create(
            "Make Me",
            forHimText:  "Make her lose control completely. Whatever it takes.",
            forHerText:  "Make him lose control completely. Whatever it takes.",
            neutralText: "Make your partner lose complete control. Use everything you know.",
            MonogamyZone.Wild, CardTarget.ForDrawer, tokenValue: 4),

        MonogamyCard.Create(
            "The Night Is Ours",
            forHimText:  "The night belongs to you both. Take what you want.",
            forHerText:  "The night belongs to you both. Take what you want.",
            neutralText: "The night is completely yours. Take what you both need from each other.",
            MonogamyZone.Wild, CardTarget.ForBoth, tokenValue: 4),

        // ════════════════════════════════════════════════════════════════════
        // FANTASY — the deck's most explicit tier (5 tokens)
        //
        // Wild escalates what you DO. Fantasy escalates what you're willing to
        // NAME — saying the thing out loud first, then enacting it. That's the
        // harder step for most couples, and it's why this sits above Wild
        // rather than beside it.
        //
        // The "no obligation" rule that governs the whole deck matters most
        // here: every one of these is negotiable, and any of them can be
        // declined without explanation. Several cards make the veto explicit
        // in their own text rather than leaving it to the rulebook.
        // ════════════════════════════════════════════════════════════════════

        MonogamyCard.Create(
            "Say It First",
            forHimText:  "Tell her a fantasy you've never said out loud. All of it, in your own words, before anything happens.",
            forHerText:  "Tell him a fantasy you've never said out loud. All of it, in your own words, before anything happens.",
            neutralText: "Tell your partner a fantasy you've never said out loud — the whole thing, before anything happens.",
            MonogamyZone.Fantasy, CardTarget.ForDrawer, tokenValue: 5),

        MonogamyCard.Create(
            "Their Turn",
            forHimText:  "She names a fantasy. You listen to all of it without interrupting, then tell her which part you want most.",
            forHerText:  "He names a fantasy. You listen to all of it without interrupting, then tell him which part you want most.",
            neutralText: "Your partner names a fantasy. Listen to all of it, then say which part you want most.",
            MonogamyZone.Fantasy, CardTarget.ForPartner, tokenValue: 5),

        MonogamyCard.Create(
            "Word For Word",
            forHimText:  "She describes exactly what she wants. You do precisely that — no improvising, no more, no less.",
            forHerText:  "He describes exactly what he wants. You do precisely that — no improvising, no more, no less.",
            neutralText: "One of you describes exactly what you want. The other does precisely that — no improvising.",
            MonogamyZone.Fantasy, CardTarget.ForBoth, tokenValue: 5),

        MonogamyCard.CreateNeutral(
            "Two Truths",
            "Each of you names one thing you've wanted and never asked for. Then pick one of the two and spend the rest of the evening on it.",
            MonogamyZone.Fantasy, CardTarget.ForBoth, tokenValue: 5),

        MonogamyCard.Create(
            "The Scene",
            forHimText:  "She sets a scene — where you are, who you're being. Stay in it for fifteen minutes without breaking.",
            forHerText:  "He sets a scene — where you are, who you're being. Stay in it for fifteen minutes without breaking.",
            neutralText: "One of you sets a scene. Both stay in it for fifteen minutes without breaking character.",
            MonogamyZone.Fantasy, CardTarget.ForBoth, tokenValue: 5, durationMinutes: 15),

        MonogamyCard.CreateNeutral(
            "The Veto",
            "Name the thing you'd never do. Say why out loud. Then name the thing right next to it that you would — and do that instead.",
            MonogamyZone.Fantasy, CardTarget.ForBoth, tokenValue: 5),

        MonogamyCard.Create(
            "Narrate It",
            forHimText:  "Describe what you're doing to her while you do it. Don't stop talking, even when it gets hard to.",
            forHerText:  "Describe what you're doing to him while you do it. Don't stop talking, even when it gets hard to.",
            neutralText: "Describe what you're doing while you do it. Don't stop talking, even when it gets hard to.",
            MonogamyZone.Fantasy, CardTarget.ForDrawer, tokenValue: 5),

        MonogamyCard.Create(
            "Ask For It",
            forHimText:  "Ask her for exactly what you want, in the words you actually mean — not the polite ones. She decides.",
            forHerText:  "Ask him for exactly what you want, in the words you actually mean — not the polite ones. He decides.",
            neutralText: "Ask for exactly what you want, in the words you actually mean. Your partner decides.",
            MonogamyZone.Fantasy, CardTarget.ForDrawer, tokenValue: 5),

        MonogamyCard.CreateNeutral(
            "One Rule Each",
            "You each set one rule for the next half hour — one thing that must happen, one that mustn't. Both rules stand.",
            MonogamyZone.Fantasy, CardTarget.ForBoth, tokenValue: 5, durationMinutes: 30),

        MonogamyCard.Create(
            "The Long Version",
            forHimText:  "Tell her the fantasy again — the version you edited down the first time. The whole thing this time.",
            forHerText:  "Tell him the fantasy again — the version you edited down the first time. The whole thing this time.",
            neutralText: "Tell it again — the version you edited down the first time. The whole thing now.",
            MonogamyZone.Fantasy, CardTarget.ForDrawer, tokenValue: 5),

        MonogamyCard.CreateNeutral(
            "Nothing Off The Table",
            "For the next twenty minutes, either of you may ask for anything. Either of you may say no to anything. Both halves matter equally.",
            MonogamyZone.Fantasy, CardTarget.ForBoth, tokenValue: 5, durationMinutes: 20),

        MonogamyCard.Create(
            "Again, Then",
            forHimText:  "Ask her what she wants to happen again — and put a date on it before the night ends.",
            forHerText:  "Ask him what he wants to happen again — and put a date on it before the night ends.",
            neutralText: "Ask what your partner wants to happen again. Put a real date on it before the night ends.",
            MonogamyZone.Fantasy, CardTarget.ForBoth, tokenValue: 5),
    ];
}