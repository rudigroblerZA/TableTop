#!/usr/bin/env python3
"""
Extends the six adult decks that were already internally balanced, so this is
volume rather than gap-filling: relationship-dares, heat-check, the-long-game,
slow-burn, all-in and last-orders.

Each deck has its own voice and its own wrapper, and each wrapper is taken from
that deck's own CardBank helper rather than retyped:

  relationship-dares  plain prose, no markup, restriction "couple"
  heat-check          candle/fire pair, title == category, fixed frame
  the-long-game       <b>emoji CATEGORY</b> + prompt + the "be specific" footer
  slow-burn           per-category header; IOU and Almost carry footers, the
                      House Rule and Cash In cards deliberately do not
  all-in              poker framing, per-category header and footer
  last-orders         social cards ungated; the drink cards carry age:18 AND
                      the sip-not-shot / soft-counts-the-same footer, which is
                      the deck's whole safety posture and is never varied

Difficulties stay inside the ladder each category already uses.
"""
import json, pathlib, uuid, sys
from collections import Counter

ROOT = pathlib.Path(__file__).resolve().parent.parent
JSON = ROOT / "src/TableTop.Games/Data/Json"

# ── wrappers, lifted from each deck's CardBank ────────────────────────────────

LG_FOOT = ('<i>Be specific — the specific thing is the whole gift. If it lands, '
           'either of you can call "Keeper" and write it down.</i>')
LG_EMOJI = {"Noticing": "🌱", "Gratitude": "🕯️", "Weathered": "⚓", "Vows": "💍"}

HC_HEAD = "<b>Choose together before anyone moves — any mismatch means 🕯️:</b>"
HC_FOOT = ("<i>Fire is only fire when it's unanimous and enthusiastic. "
           "Candle is never a loss.</i>")

SB = {
    "IOU": ("<b>✉️ Seal it in the pot:</b>",
            "<i>Fold it. Nobody reads it yet. The pot opens when the game ends — "
            "and either of you can always trade any IOU for a kiss and a rain check.</i>"),
    "Almost": ("<b>🫧 Stop at the best part:</b>",
               "<i>Yes, stopping is the whole card. What you start now, the pot finishes later.</i>"),
    "House Rule": ("<b>📜 New standing rule:</b>", None),
    "Cash In": ("<b>🔓 The pot pays early:</b>", None),
}

AI = {
    "Ante": ("<b>♠ ANTE — play it, score it:</b>", None),
    "Raise": ("<b>♥ THE HAND:</b>",
              "<i>Play the base, then your partner may call the raise: deliver it and "
              "this hand pays double (you also win the next Ante automatically). Fold a "
              "called raise and the chip is theirs — paid with a kiss on the hand.</i>"),
    "Bluff": ("<b>♣ POKER FACE. Deliver this line, then they call TRUE or BLUFF:</b>",
              "<i>Right call: their chip. Wrong call: yours. All debts settle in kisses, "
              "immediately — house rules.</i>"),
    "Jackpot": ("<b>♦ JACKPOT HAND — expensive, unforgettable:</b>",
                "<i>Play it and the chip is yours; fold it and one chip slides across the "
                "table. The house never shames a fold.</i>"),
}

LO_EMOJI = {"Warm Up": "🌤️", "Party Tricks": "🎭", "Confessions": "🎤",
            "Forfeits": "🍻", "Last Round": "🌙"}
LO_SOCIAL_FOOT = "<i>Pass is always free.</i>"
LO_DRINK_FOOT = ("<i>A sip is a sip — never a shot, never the whole glass. Soft "
                 "drinks count the same and score the same. Pass is always free.</i>")


def wrap(head, body, foot=None):
    return f"{head}\n\n{body}" + (f"\n\n{foot}" if foot else "")


# ── content ───────────────────────────────────────────────────────────────────

RELATIONSHIP_DARES = [
    ("Playful", "Easy", "The Tour Guide",
     "Give your partner a two-minute guided tour of the room you're in, as though it's a world heritage site and they have paid a considerable amount for this."),
    ("Playful", "Easy", "Your Greatest Hits",
     "Perform the chorus of a song that means something to the two of you. Choreography is optional but it is being scored."),
    ("Playful", "Medium", "Last Search",
     "Read out the last three things you searched for on your phone. No scrolling ahead, no editing, no explaining until all three are out."),

    ("Honest", "Medium", "The Thing I Nearly Said",
     "Tell them about a time recently when you nearly said something and didn't. Then say it."),
    ("Honest", "Medium", "The Compliment You Don't Believe",
     "Name a compliment they've given you that you've never quite believed. Let them argue for it."),
    ("Honest", "Hard", "What I Get Wrong",
     "Name the thing you know you do that makes life harder for them. No excuse attached and no promise attached — just name it, and let it sit."),

    ("Tender", "Medium", "Hands",
     "Take their hands and describe them out loud, in detail, as though you were memorising them."),
    ("Tender", "Medium", "Where It Started",
     "Tell the story of the moment you first knew — properly told, with the detail in, as though to someone who has never heard it."),
    ("Tender", "Hard", "The Photograph",
     "Find a photo of the two of you from at least a year ago. Each say one thing you remember about that day that the other doesn't know."),

    ("Intimate", "Extreme", "Ask Me Anything",
     "Your partner may ask you three questions about what you want. You answer all three honestly. Nothing asked here gets used against you later."),
    ("Intimate", "Extreme", "The Standing Invitation",
     "Tell your partner one thing you'd like more of — then agree a private signal either of you can use to ask for it on any ordinary day, without needing a conversation first."),
    ("Intimate", "Extreme", "Slow Hands",
     "For ten minutes, one of you does nothing but touch the other exactly how they ask to be touched. Then swap, if you both want to."),
]

HEAT_CHECK = [
    ("Confessions", "Medium",
     "Tell them the compliment you've thought about them today but haven't said.",
     "Tell them the thought you had about them today that you decided not to say out loud. Say it now."),
    ("Confessions", "Hard",
     "Name the moment tonight you'd most like to repeat.",
     "Name the thing you were quietly hoping this game would give you an excuse to do."),
    ("Confessions", "Extreme",
     "Tell them one thing you find attractive that you've never mentioned.",
     "Tell them the one you've never mentioned because saying it felt too revealing — and say why it is."),

    ("Dares", "Medium",
     "Kiss them somewhere you've never deliberately kissed them before.",
     "Same — but they choose the spot, and you take a full slow minute getting there."),
    ("Dares", "Hard",
     "Whisper what you'd like to happen next.",
     "Whisper it, then do the first half of it and stop."),
    ("Dares", "Extreme",
     "Take one minute of their completely undivided attention, however you like.",
     "Take five, and say out loud what you're doing as you do it."),

    ("Scenes", "Medium",
     "You're strangers seated next to each other on a long flight. Make conversation.",
     "Same flight — except one of you has privately decided this is going somewhere. Play it out until the seatbelt sign."),
    ("Scenes", "Hard",
     "You haven't seen each other in six months. Reunite at the arrivals gate.",
     "Same reunion, but you've made it as far as the car park and there is nobody watching."),
    ("Scenes", "Extreme",
     "It's the night you first met, replayed — but this time you both already know how it ends.",
     "The same night, except you skip every part you were too polite to skip the first time."),

    ("Closer", "Hard",
     "Tell them the thing about your life together you're most quietly proud of.",
     "Tell them the thing you want from the next year that you've never said out loud."),
    ("Closer", "Extreme",
     "Forehead to forehead, one minute, breathing in time.",
     "Forehead to forehead until one of you says the thing you're both already thinking."),
    ("Closer", "Extreme",
     "Say what you'd want them to remember about tonight.",
     "Say what you'd want them to remember about you."),
]

THE_LONG_GAME = [
    ("Noticing", "Easy", "The Small Repair",
     "Name one small thing they did this week that quietly fixed something — a job, a mood, a whole day."),
    ("Noticing", "Easy", "In A Room",
     "Name something they do in company that you're proud to stand next to."),
    ("Noticing", "Medium", "The Change",
     "Name one way they've changed for the better since you met — and say what you think it cost them."),

    ("Gratitude", "Medium", "The Unasked Favour",
     "Thank them for something they do that you have never once had to ask for."),
    ("Gratitude", "Hard", "The Cost",
     "Name something in your life that is measurably better and exists only because of them."),
    ("Gratitude", "Extreme", "The Person You Became",
     "Name one way being with them made you a better person — and be specific about how it actually happened."),

    ("Weathered", "Medium", "Carried",
     "Name a stretch when they carried more than their share. Say that you noticed, and say what it looked like from where you stood."),
    ("Weathered", "Hard", "The Night It Turned",
     "Name a night that could have gone badly and didn't. What did one of you do?"),
    ("Weathered", "Extreme", "The Argument We Survived",
     "Name an argument you're glad you had. What did it settle that needed settling?"),

    ("Vows", "Medium", "A Standing Appointment",
     "Promise one recurring thing — weekly or monthly — that belongs to the two of you and nobody else. Name the day out loud."),
    ("Vows", "Hard", "The Thing I'll Stop",
     "Name one thing you'll stop doing, starting now, because you know what it costs them. Say it as a promise, not an intention."),
    ("Vows", "Hard", "In Ten Years",
     "Say one thing you promise will still be true of the two of you in ten years — then say what it'll take to keep it true."),
]

SLOW_BURN = [
    ("IOU", "Medium",
     "Write ONE WORD naming a length of time. Fold it. When the pot opens, that's how long the next kiss lasts. Be generous, or be cruel."),
    ("IOU", "Extreme",
     "Write ONE WORD naming something you want them to say to you. Fold it. When the pot opens they say it — in their own voice, looking at you."),

    ("Almost", "Medium",
     "Take their hand as though you're about to lead them somewhere. Stand up. Then sit back down and say 'not yet.'"),
    ("Almost", "Hard",
     "Tell them the first four words of what you want to happen tonight. Stop at the fourth. The pot is holding the rest."),

    ("House Rule", "Easy",
     "Until the next House Rule: neither of you may say the other's name without touching them somewhere at the same moment."),
    ("House Rule", "Medium",
     "Until the next House Rule: every card is read aloud in a whisper, close enough that it has to be."),

    ("Cash In", "Medium",
     "Draw ONE folded IOU and read it aloud — then fold it and put it back. Now you both know it's coming, and neither of you knows when."),
    ("Cash In", "Extreme",
     "Empty the pot. Read them all in the order they come out, and honour every one before the night is over."),
]

ALL_IN = [
    ("Ante", "Easy", None,
     "Compliment the way they say your name. You had four seconds to think about this and you are already out of time."),
    ("Ante", "Medium", None,
     "Name the item of their clothing you'd remove first, and give exactly one reason. Committee of one. No appeal."),

    ("Raise", "Medium",
     "BASE: hold their hand across the table for a full thirty seconds.",
     "RAISE: the same thirty seconds — eye contact throughout, and neither of you may smile."),
    ("Raise", "Hard",
     "BASE: tell them one thing you want to do later.",
     "RAISE: the same sentence, whispered, close enough that they feel it — and then you don't mention it again all game."),

    ("Bluff", "Medium", None,
     "'I noticed what you were wearing tonight before you'd finished walking into the room.'"),
    ("Bluff", "Hard", None,
     "'There is something I have wanted to do since this game started, and I still haven't done it.'"),

    ("Jackpot", "Hard", None,
     "Re-enact your first kiss with full historical accuracy — same positions, same hesitation, same appalling soundtrack. Then perform the version you'd do now."),
    ("Jackpot", "Extreme", None,
     "Say out loud the one thing about them you've never quite managed to put into words. Take as long as you need. The table waits."),
]

LAST_ORDERS = [
    ("Warm Up", "The Nickname", False,
     "Give everyone at the table a nickname based on the first thing you noticed about them tonight. They're keeping it for the rest of the game."),
    ("Warm Up", "Two Minutes' Notice", False,
     "You've been asked to give a two-minute speech at this table's wedding. You don't know whose. Begin."),

    ("Party Tricks", "Accent Relay", False,
     "Say the same sentence in three different accents. The table picks which one you're keeping for the next round."),
    ("Party Tricks", "Genuinely Useless Talent", False,
     "Demonstrate the most useless skill you possess. It must be genuinely useless and genuinely yours."),

    ("Confessions", "Left On Read", False,
     "What's the message you've left unanswered the longest, and what's the real reason?"),
    ("Confessions", "Worst Money", False,
     "What is the worst thing you have ever spent money on — and would you do it again?"),

    ("Forfeits", "Toast the Absent", True,
     "Take a sip — soft counts, same as ever — and toast someone who isn't here tonight. Say why them."),
    ("Forfeits", "The Round You Owe", True,
     "Take a sip, soft or otherwise, and name the person at this table you'd most like to buy a drink for, and what it would be."),

    ("Last Round", "The Good Bit", False,
     "Everyone names the best moment of the night so far. No repeats, so the slow ones have to think."),
    ("Last Round", "Tomorrow", False,
     "Everyone says one thing they're doing tomorrow. It's a good way to remember there's a tomorrow."),
]


def build():
    out = {}

    out["relationship-dares"] = [
        {"title": t, "description": body, "difficulty": d, "category": c,
         "restriction": "couple"}
        for c, d, t, body in RELATIONSHIP_DARES]

    out["heat-check"] = [
        {"title": c, "category": c, "difficulty": d,
         "description": wrap(HC_HEAD,
                             f"🕯️ <b>Candle:</b> {candle}\n\n🔥 <b>Fire:</b> {fire}",
                             HC_FOOT)}
        for c, d, candle, fire in HEAT_CHECK]

    out["the-long-game"] = [
        {"title": t, "category": c, "difficulty": d,
         "description": wrap(f"<b>{LG_EMOJI[c]} {c.upper()}</b>", body, LG_FOOT)}
        for c, d, t, body in THE_LONG_GAME]

    out["slow-burn"] = [
        {"title": c, "category": c, "difficulty": d,
         "description": wrap(SB[c][0], body, SB[c][1])}
        for c, d, body in SLOW_BURN]

    allin = []
    for row in ALL_IN:
        if row[0] == "Raise":
            c, d, base, raise_ = row
            body = f"{base}\n\n{raise_}"
        else:
            c, d, _, body = row
        allin.append({"title": c, "category": c, "difficulty": d,
                      "description": wrap(AI[c][0], body, AI[c][1])})
    out["all-in"] = allin

    lo = []
    for c, t, gated, body in LAST_ORDERS:
        card = {"title": t, "category": c, "difficulty": "Easy",
                "description": wrap(f"<b>{LO_EMOJI[c]} {c.upper()}</b>", body,
                                    LO_DRINK_FOOT if gated else LO_SOCIAL_FOOT)}
        if gated:
            card["restriction"] = "age:18"
        lo.append(card)
    out["last-orders"] = lo
    return out


def main():
    used = set()
    for f in JSON.glob("*.deck.json"):
        for c in json.loads(f.read_text(encoding="utf-8")).get("cards", []):
            if c.get("id"):
                used.add(c["id"].lower())

    total = 0
    for name, additions in build().items():
        path = JSON / f"{name}.deck.json"
        deck = json.loads(path.read_text(encoding="utf-8"))
        cards = deck["cards"]
        before = len(cards)

        for card in additions:
            if any(c["title"] == card["title"] and c["description"] == card["description"]
                   for c in cards):
                print(f"  skip (exists): {name} / {card['title']}")
                continue
            cid = str(uuid.uuid4())
            while cid.lower() in used:
                cid = str(uuid.uuid4())
            used.add(cid.lower())

            ordered = {"id": cid, "title": card["title"], "description": card["description"],
                       "difficulty": card["difficulty"], "category": card["category"]}
            if "restriction" in card:
                ordered["restriction"] = card["restriction"]

            idx = max((i for i, c in enumerate(cards) if c["category"] == card["category"]),
                      default=len(cards) - 1)
            cards.insert(idx + 1, ordered)

        path.write_text(json.dumps(deck, ensure_ascii=False, indent=2) + "\n",
                        encoding="utf-8")
        cats = Counter(c["category"] for c in cards)
        total += len(cards) - before
        print(f"{name:<22} {before:>3} -> {len(cards):<3}  " +
              "  ".join(f"{k} {v}" for k, v in cats.items()))
    print(f"\ntotal added: {total}")


if __name__ == "__main__":
    sys.exit(main())
