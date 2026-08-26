#!/usr/bin/env python3
"""
Mirrors the newly-added JSON cards into their C# fallback banks.

Every other adult deck keeps its .deck.json and its CardBank in exact step
(truth-or-dare 35/35, heat-check 36/36, relationship-dares 38/38, the-long-game
25/25, slow-burn 32/32, all-in 32/32). Parity is the convention here, so adding
to the JSON alone would leave Afterglow and Undivided as the only decks that
deal different content when the JSON fails to load.

The bank helpers (M/A/R/SW/Q) apply the category header and the invitation
footer themselves, so only the middle body text is passed — this script strips
the <b> header and <i> footer back off the JSON description to recover it,
rather than re-typing the body and risking a mismatch.
"""
import json, pathlib, re, sys

ROOT = pathlib.Path(__file__).resolve().parent.parent
JSON = ROOT / "src/TableTop.Games/Data/Json"
SRC = ROOT / "src/TableTop.Games/Couples"

NEW = {
    "afterglow": ["Ask First", "One Thing", "Somewhere Unobvious", "Hands Away",
                  "Say It While It Happens", "Half Speed", "The Unsaid Thing",
                  "Nothing New", "Stay Here", "One Thing That Landed"],
    "undivided": ["Ask, Then Begin", "Read the Breath", "Swap On Their Word",
                  "Swap Halfway", "Nothing Back", "Past The Point",
                  "Slower Than They Want", "Until They Say",
                  "Say What They Gave You"],
    "between-the-two-of-you": ["The Free Evening", "In the Middle of the Day",
                               "Somewhere That Isn't Here"],
}


def body_of(description: str) -> str:
    """Strip the <b> header line and the <i> footer that the bank helper adds."""
    t = re.sub(r"^<b>.*?</b>\n\n", "", description, flags=re.S)
    t = re.sub(r"\n\n<i>.*?</i>\s*$", "", t, flags=re.S)
    return t.strip()


def cs_literal(body: str, indent: int) -> str:
    """Render a body as C# string-concat lines matching the banks' house style."""
    pad = " " * indent
    lines = body.split("\n")
    out = []
    for i, ln in enumerate(lines):
        esc = ln.replace("\\", "\\\\").replace('"', '\\"')
        suffix = "\\n" if i < len(lines) - 1 else ""
        out.append(f'{pad}"{esc}{suffix}"')
    return " +\n".join(out)


def call_for(deck, card):
    cat, title, diff = card["category"], card["title"], card["difficulty"]
    body = cs_literal(body_of(card["description"]), 14)
    t = title.replace('"', '\\"')
    if deck == "afterglow":
        if cat == "Aftercare":
            return f'        A("{t}",\n{body},\n          Difficulty.{diff}),'
        return f'        M("{cat}", "{t}",\n{body},\n          Difficulty.{diff}),'
    if deck == "undivided":
        if cat == "Swap":
            return f'        SW("{t}",\n{body}),'
        if cat == "Aftercare":
            return f'        A("{t}",\n{body},\n          Difficulty.{diff}),'
        return f'        R("{cat}", "{t}",\n{body},\n          Difficulty.{diff}),'
    return f'            Q("{cat}", "{t}",\n{body},\n              Difficulty.{diff}),'


FILES = {
    "afterglow": SRC / "AfterglowMode.cs",
    "undivided": SRC / "UndividedMode.cs",
    "between-the-two-of-you": SRC / "BetweenTheTwoOfYouMode.cs",
}


def main():
    for deck, titles in NEW.items():
        cards = {c["title"]: c for c in
                 json.loads((JSON / f"{deck}.deck.json").read_text(encoding="utf-8"))["cards"]}
        path = FILES[deck]
        text = path.read_text(encoding="utf-8")
        added = 0

        for title in titles:
            card = cards[title]
            if f'"{title}"' in text:
                print(f"  skip (already in bank): {deck} / {title}")
                continue
            call = call_for(deck, card)

            # Anchor: insert after the last existing call of the same category,
            # so the bank stays grouped exactly like the JSON.
            cat = card["category"]
            anchors = [m.start() for m in re.finditer(
                rf'(?m)^\s+(?:M|R|SW|A|Q)\("{re.escape(cat)}", ', text)]
            if not anchors and cat in ("Aftercare", "Swap"):
                # A( and SW( don't take a category argument — anchor on the
                # helper name instead.
                helper = "A" if cat == "Aftercare" else "SW"
                anchors = [m.start() for m in re.finditer(rf'(?m)^\s+{helper}\("', text)]
            if not anchors:
                print(f"  !! no anchor for {deck} / {title} ({cat})")
                continue

            last = anchors[-1]
            end = text.index("),\n", last) + len("),\n")
            text = text[:end] + call + "\n" + text[end:]
            added += 1

        path.write_text(text, encoding="utf-8")
        print(f"{deck}: +{added} into {path.name}")


if __name__ == "__main__":
    sys.exit(main())
