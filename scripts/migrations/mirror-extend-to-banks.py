#!/usr/bin/env python3
"""
Mirrors the 62 newly-added cards into their C# fallback banks.

Each bank has its own helper with its own signature, and each helper applies
that deck's wrapper itself — so what gets passed is the inner body only, which
this script recovers by stripping the header and footer back off the JSON
description rather than re-typing it.

  relationship-dares  D(title, text, category, Difficulty.X, couplesOnly)
  heat-check          H(category, candle, fire, Difficulty.X)
  the-long-game       N|G|W|V(category, title, prompt, Difficulty.X)
  slow-burn           I|A|R|C(text, Difficulty.X)
  all-in              N(text,d) | S(base,raise,d) | B(line,d) | J(text,d)
  last-orders         S(category, title, body)  /  D(category, title, body, drinkingAge)
"""
import json, pathlib, re, sys

ROOT = pathlib.Path(__file__).resolve().parent.parent
JSON = ROOT / "src/TableTop.Games/Data/Json"


def resolve_within(base: pathlib.Path, candidate: pathlib.Path) -> pathlib.Path:
    """Resolve `candidate` and refuse it if it would land outside `base`.

    Both paths here are built from a fixed, hardcoded deck/file list (see
    DECKS below), never from external input — but the guard makes that
    guarantee explicit and load-bearing rather than assumed.
    """
    base = base.resolve()
    resolved = candidate.resolve()
    if resolved != base and base not in resolved.parents:
        raise ValueError(f"refusing to touch a path outside {base}: {resolved}")
    return resolved

DECKS = {
    "relationship-dares": ROOT / "src/TableTop.Games/Couples/RelationshipDaresMode.cs",
    "heat-check":         ROOT / "src/TableTop.Games/Couples/HeatCheckMode.cs",
    "the-long-game":      ROOT / "src/TableTop.Games/Couples/TheLongGameMode.cs",
    "slow-burn":          ROOT / "src/TableTop.Games/Couples/SlowBurnMode.cs",
    "all-in":             ROOT / "src/TableTop.Games/Couples/AllInMode.cs",
    "last-orders":        ROOT / "src/TableTop.Games/Party/LastOrdersMode.cs",
}

NEW_TITLES = {
    "relationship-dares": ["The Tour Guide", "Your Greatest Hits", "Last Search",
                           "The Thing I Nearly Said", "The Compliment You Don't Believe",
                           "What I Get Wrong", "Hands", "Where It Started",
                           "The Photograph", "Ask Me Anything",
                           "The Standing Invitation", "Slow Hands"],
    "the-long-game": ["The Small Repair", "In A Room", "The Change",
                      "The Unasked Favour", "The Cost", "The Person You Became",
                      "Carried", "The Night It Turned", "The Argument We Survived",
                      "A Standing Appointment", "The Thing I'll Stop", "In Ten Years"],
    "last-orders": ["The Nickname", "Two Minutes' Notice", "Accent Relay",
                    "Genuinely Useless Talent", "Left On Read", "Worst Money",
                    "Toast the Absent", "The Round You Owe", "The Good Bit", "Tomorrow"],
}

LG_HELPER = {"Noticing": "N", "Gratitude": "G", "Weathered": "W", "Vows": "V"}
SB_HELPER = {"IOU": "I", "Almost": "A", "House Rule": "R", "Cash In": "C"}
AI_HELPER = {"Ante": "N", "Raise": "S", "Bluff": "B", "Jackpot": "J"}


def inner(desc):
    """Strip the <b> header line and any <i> footer the helper re-adds."""
    t = re.sub(r"^<b>.*?</b>\n\n", "", desc, flags=re.S)
    t = re.sub(r"\n\n<i>.*?</i>\s*$", "", t, flags=re.S)
    return t.strip()


def lit(body, indent):
    """C# string-concat lines in the banks' house style."""
    pad = " " * indent
    parts = body.split("\n")
    return " +\n".join(
        f'{pad}"{p.replace(chr(92), chr(92)*2).replace(chr(34), chr(92)+chr(34))}'
        f'{"\\n" if i < len(parts)-1 else ""}"'
        for i, p in enumerate(parts))


def call_for(deck, card, ind):
    cat, title, diff = card["category"], card["title"], card["difficulty"]
    body = inner(card["description"])
    t = title.replace('"', '\\"')
    pad = " " * ind

    if deck == "relationship-dares":
        return f'{pad}D("{t}",\n{lit(body, ind+2)},\n{pad}  "{cat}", Difficulty.{diff}, couplesOnly),'

    if deck == "heat-check":
        m = re.search(r"🕯️ <b>Candle:</b> (.*?)\n\n🔥 <b>Fire:</b> (.*)", body, re.S)
        candle, fire = m.group(1).strip(), m.group(2).strip()
        return (f'{pad}H("{cat}",\n{lit(candle, ind+2)},\n'
                f'{lit(fire, ind+2)},\n{pad}  Difficulty.{diff}),')

    if deck == "the-long-game":
        h = LG_HELPER[cat]
        return f'{pad}{h}("{cat}", "{t}",\n{lit(body, ind+2)},\n{pad}  Difficulty.{diff}),'

    if deck == "slow-burn":
        h = SB_HELPER[cat]
        return f'{pad}{h}(\n{lit(body, ind+2)},\n{pad}  Difficulty.{diff}),'

    if deck == "all-in":
        h = AI_HELPER[cat]
        if h == "S":
            base, raise_ = body.split("\n\n", 1)
            return (f'{pad}S(\n{lit(base.strip(), ind+2)},\n'
                    f'{lit(raise_.strip(), ind+2)},\n{pad}  Difficulty.{diff}),')
        return f'{pad}{h}(\n{lit(body, ind+2)},\n{pad}  Difficulty.{diff}),'

    # last-orders
    if card.get("restriction") == "age:18":
        return f'{pad}D("{cat}", "{t}",\n{lit(body, ind+2)},\n{pad}  drinkingAge),'
    return f'{pad}S("{cat}", "{t}",\n{lit(body, ind+2)}),'


def main():
    for deck, raw_path in DECKS.items():
        deck_json = resolve_within(JSON, JSON / f"{deck}.deck.json")
        cards = json.loads(deck_json.read_text(encoding="utf-8"))["cards"]
        path = resolve_within(ROOT, raw_path)
        text = path.read_text(encoding="utf-8")

        # Which cards are new? For decks with unique titles, by title. For the
        # ones whose titles repeat (heat-check, slow-burn, all-in all title every
        # card after its category), by whether the body text appears in the file.
        if deck in NEW_TITLES:
            new = [c for c in cards if c["title"] in NEW_TITLES[deck]]
        else:
            new = [c for c in cards
                   if inner(c["description"]).split("\n")[0][:40]
                   .replace('"', '\\"') not in text]

        added = 0
        for card in new:
            probe = inner(card["description"]).split("\n")[0][:40].replace('"', '\\"')
            if probe in text:
                continue

            # Indentation and anchor: match the last existing call in the same
            # category so the bank stays grouped as authored.
            cat = card["category"]
            pat = {
                "relationship-dares": rf'(?m)^(\s+)D\("[^"]*",\n(?:.*\n)*?\s+"{re.escape(cat)}", Difficulty',
                "heat-check":         rf'(?m)^(\s+)H\("{re.escape(cat)}",',
                "the-long-game":      rf'(?m)^(\s+){LG_HELPER.get(cat,"N")}\("{re.escape(cat)}",',
                "slow-burn":          rf'(?m)^(\s+){SB_HELPER.get(cat,"I")}\(',
                "all-in":             rf'(?m)^(\s+){AI_HELPER.get(cat,"N")}\(',
                "last-orders":        rf'(?m)^(\s+)[SD]\("{re.escape(cat)}",',
            }[deck]
            hits = list(re.finditer(pat, text))
            if not hits:
                print(f"  !! no anchor: {deck} / {card['title']} ({cat})")
                continue
            last = hits[-1]
            ind = len(last.group(1))
            end = text.index("),\n", last.end()) + len("),\n")
            text = text[:end] + call_for(deck, card, ind) + "\n" + text[end:]
            added += 1

        path.write_text(text, encoding="utf-8")
        print(f"{deck:<22} +{added} into {path.name}")


if __name__ == "__main__":
    sys.exit(main())
