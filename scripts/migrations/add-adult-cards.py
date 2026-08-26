#!/usr/bin/env python3
"""
One-shot content addition for the adult (couples) decks.

Targets the three decks whose categories were measurably uneven, so the deal
felt lopsided: Afterglow's "Undone" had 2 cards against 5 for its siblings,
Undivided's "Swap" had 2 against 4, and Between the Two of You had three of its
five quiz axes carrying 2 questions where the other two carried 3.

Style is copied from the decks themselves, not invented:
  * exact category header and footer strings are lifted from an existing card
    in the same deck/category, so no drift is possible
  * difficulty stays inside the ladder each category already uses
  * quiz answers preserve each axis's A/B/C/D meaning, because the Results
    cards score by counting letters — a new question with a different mapping
    would silently corrupt every result.
"""
import json, pathlib, uuid, sys
from collections import Counter

ROOT = pathlib.Path(__file__).resolve().parent.parent
JSON = ROOT / "src/TableTop.Games/Data/Json"

AG_FOOT = ('<i>An invitation, never an order. Take it, soften it, trade it, or '
           'pass — pass is always free. Enthusiasm is the only yes; call '
           '"colour?" anytime.</i>')
UD_FOOT = ('<i>An invitation, never an order. The receiver holds every yes; '
           'pass is always free, and enthusiasm is the only green light. Call '
           '"colour?" anytime.</i>')


def ag(header, body, foot=True):
    return f"<b>{header}</b>\n\n{body}" + (f"\n\n{AG_FOOT}" if foot else "")


def ud(header, body, foot=True):
    return f"<b>{header}</b>\n\n{body}" + (f"\n\n{UD_FOOT}" if foot else "")


WARM, TURN, HEAT, UNDONE = "🌤️ WARM UP", "🔥 TURN UP", "🌶️ HEAT", "💥 UNDONE"
AGCARE = "💜 AFTERCARE"
ATT = "🌤️ ATTENTION  ·  the receiver steers"
SWAP = "🔄 SWAP"
DEV = "🔥 DEVOTION  ·  the receiver steers"
WOR = "🌶️ WORSHIP  ·  the receiver steers"
UDCARE = "💜 AFTERCARE"

AFTERGLOW = [
    ("Warm Up", "Easy", "Ask First", ag(WARM,
        "Ask your partner one question — what would you like more of tonight? "
        "Listen to the whole answer before you touch them at all. Then begin there.")),
    ("Warm Up", "Medium", "One Thing", ag(WARM,
        "Remove one item of your partner's clothing, slowly, and only that one. "
        "Then go back to kissing as though nothing had happened.")),

    ("Turn Up", "Medium", "Somewhere Unobvious", ag(TURN,
        "Kiss somewhere your partner wouldn't have guessed — the inside of an elbow, "
        "the back of a knee, the base of the spine. Stay there longer than seems necessary.")),
    ("Turn Up", "Hard", "Hands Away", ag(TURN,
        "One of you: hands behind your back, no touching allowed. The other: two minutes, "
        "do as you like. The one who can't touch says when the two minutes are up.")),

    ("Heat", "Hard", "Say It While It Happens", ag(HEAT,
        "One partner: keep your hands or mouth busy. The other: say out loud what you want "
        "next, as it occurs to you. Instructions get followed exactly.")),
    ("Heat", "Extreme", "Half Speed", ag(HEAT,
        "Whatever is happening, halve the speed of it. Stay at half speed for as long as "
        "you can both stand — then check in before you change anything.")),

    ("Undone", "Extreme", "The Unsaid Thing", ag(UNDONE,
        "Tell your partner the thing you've been thinking about all evening and haven't said. "
        "Then decide together, out loud, whether tonight is the night for it. Either answer is a good one.")),
    ("Undone", "Extreme", "Nothing New", ag(UNDONE,
        "No new ideas on this card. Do the thing that has always worked for the two of you — "
        "the old reliable — and give it your whole attention, as though it were the first time.")),
    ("Undone", "Extreme", "Stay Here", ag(UNDONE,
        "Don't move on to anything else. Whatever this is, stay in it — no escalating, no "
        "switching — until one of you says otherwise.")),

    ("Aftercare", "Medium", "One Thing That Landed", ag(AGCARE,
        "Tell each other one specific moment from tonight you'll still be thinking about "
        "tomorrow. Be precise — the exact moment, not the general idea.", foot=False)),
]

UNDIVIDED = [
    ("Attention", "Easy", "Ask, Then Begin", ud(ATT,
        "Giver: ask your Receiver where they'd like to be touched first. Do exactly that, "
        "and only that, for a slow minute.")),
    ("Attention", "Medium", "Read the Breath", ud(ATT,
        "Giver: touch your Receiver slowly and watch nothing but their breathing. Wherever it "
        "changes, stay there. Let their body do the talking.")),

    ("Swap", "Easy", "Swap On Their Word", ud(SWAP,
        "The Receiver decides when. Whenever they say swap, you swap — mid-anything. "
        "Until they say it, nothing changes.", foot=False)),
    ("Swap", "Easy", "Swap Halfway", ud(SWAP,
        "Stop at the halfway point of whatever's happening and trade roles. The new Giver picks "
        "up exactly where the last one left off — same pace, same place.", foot=False)),

    ("Devotion", "Medium", "Nothing Back", ud(DEV,
        "Giver: this whole card is your hands only. Receiver: you're not allowed to reciprocate — "
        "your only job is to receive it and say what's working.")),
    ("Devotion", "Hard", "Past The Point", ud(DEV,
        "Receiver: name the one thing you like most. Giver: do it, and keep doing it well past "
        "the point you'd normally move on. Only the Receiver decides when it's done.")),

    ("Worship", "Hard", "Slower Than They Want", ud(WOR,
        "Giver: use your mouth on your Receiver, and go slower than they'd like. Check \"colour?\" "
        "as you go, and stay only on green.")),
    ("Worship", "Extreme", "Until They Say", ud(WOR,
        "Giver: this ends when the Receiver says it ends — not before, not after. Receiver: say it "
        "out loud when you're ready, and take as long as you want getting there.")),

    ("Aftercare", "Easy", "Say What They Gave You", ud(UDCARE,
        "Whoever received: tell your Giver one specific thing they did that you'll remember. "
        "Whoever gave: tell them what you liked about giving it.", foot=False)),
]

# Quiz. Letter meanings per axis, taken from the Results cards:
#   Plan & Spark   A planned      B spontaneous  C a mix         D no strong lean
#   Words & Touch  A words        B touch        C both equally  D effort/actions
#   Bold & Cosy    A bold         B cosy         C bold w/ trust D curious, cautious
BETWEEN = [
    ("Plan & Spark", "Medium", "The Free Evening",
     "An unexpected free evening turns up in both your diaries. You'd rather:\n"
     "A) Make a plan for it now and enjoy the wait.\n"
     "B) Leave it completely open and see what happens.\n"
     "C) Have a loose idea, with room to change it.\n"
     "D) Not think about it either way until you're in it."),

    ("Words & Touch", "Medium", "In the Middle of the Day",
     "It's an ordinary Tuesday afternoon. What reaches you most?\n"
     "A) A message spelling out exactly what they're thinking about.\n"
     "B) Coming home to them reaching for you before either of you speaks.\n"
     "C) Both — the message, and then the hands.\n"
     "D) Finding they've quietly cleared the evening so you have it together."),

    ("Bold & Cosy", "Medium", "Somewhere That Isn't Here",
     "The idea of being together somewhere other than your usual place:\n"
     "A) Exciting — you're already thinking of where.\n"
     "B) Not for you; your own space is where you actually relax.\n"
     "C) Good, as long as you'd talked it through and felt safe first.\n"
     "D) Interesting, but you'd want to sit with the idea a while."),
]

DECKS = {
    "afterglow": AFTERGLOW,
    "undivided": UNDIVIDED,
    "between-the-two-of-you": BETWEEN,
}


def main():
    # Collect every id in every deck so new ones can't collide.
    used = set()
    for f in JSON.glob("*.deck.json"):
        for c in json.loads(f.read_text(encoding="utf-8")).get("cards", []):
            if c.get("id"):
                used.add(c["id"].lower())

    for name, additions in DECKS.items():
        path = JSON / f"{name}.deck.json"
        deck = json.loads(path.read_text(encoding="utf-8"))
        cards = deck["cards"]
        before = Counter(c["category"] for c in cards)

        for category, difficulty, title, description in additions:
            if any(c["title"] == title and c["category"] == category for c in cards):
                print(f"  skip (exists): {name} / {title}")
                continue
            new_id = str(uuid.uuid4())
            while new_id.lower() in used:
                new_id = str(uuid.uuid4())
            used.add(new_id.lower())

            card = {"id": new_id, "title": title, "description": description,
                    "difficulty": difficulty, "category": category}

            # Insert after the last existing card of the same category so the
            # file stays grouped the way it was authored.
            idx = max((i for i, c in enumerate(cards) if c["category"] == category),
                      default=len(cards) - 1)
            cards.insert(idx + 1, card)

        after = Counter(c["category"] for c in cards)
        path.write_text(json.dumps(deck, ensure_ascii=False, indent=2) + "\n",
                        encoding="utf-8")
        print(f"\n{name}: {sum(before.values())} -> {sum(after.values())} cards")
        for cat in after:
            mark = "  +%d" % (after[cat] - before.get(cat, 0)) if after[cat] != before.get(cat, 0) else ""
            print(f"   {cat:<16} {before.get(cat,0):>2} -> {after[cat]:<2}{mark}")


if __name__ == "__main__":
    sys.exit(main())
