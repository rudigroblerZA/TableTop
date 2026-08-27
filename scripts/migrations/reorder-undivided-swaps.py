#!/usr/bin/env python3
"""
Redistributes Undivided's Swap cards, in the JSON and the C# bank together.

Appending the two new Swap cards after the last existing one left three Swaps
back-to-back at positions 17-19. This deck is authored to be played in order
(consent first, aftercare last — the bank's own doc comment says so), and Swap
is the card that trades Giver and Receiver, so three in a row means swapping
three times with nothing happening in between. That's not a cosmetic ordering
nit; it's the mechanic misfiring.

Target: Swap acts as a rotation point between movements, never adjacent to
another Swap and never stranded immediately before the aftercare landing.

    Consent x3
    Attention x3  Swap  Attention x3  Swap
    Devotion x6   Swap
    Worship x3    Swap  Worship x3
    Aftercare x5

The C# bank is reordered to match by moving whole helper-call blocks, so the
two representations stay card-for-card identical in content AND in order.
"""
import json, pathlib, re, sys

ROOT = pathlib.Path(__file__).resolve().parent.parent


def resolve_within(base: pathlib.Path, candidate: pathlib.Path) -> pathlib.Path:
    """Resolve `candidate` and refuse it if it would land outside `base`.

    DECK and BANK below are fixed, hardcoded literals, never external input —
    but the guard makes that guarantee explicit and load-bearing rather than
    assumed.
    """
    base = base.resolve()
    resolved = candidate.resolve()
    if resolved != base and base not in resolved.parents:
        raise ValueError(f"refusing to touch a path outside {base}: {resolved}")
    return resolved


DECK = resolve_within(ROOT, ROOT / "src/TableTop.Games/Data/Json/undivided.deck.json")
BANK = resolve_within(ROOT, ROOT / "src/TableTop.Games/Couples/UndividedMode.cs")

PLAN = [("Consent", 3), ("Attention", 3), ("Swap", 1), ("Attention", 3), ("Swap", 1),
        ("Devotion", 6), ("Swap", 1), ("Worship", 3), ("Swap", 1), ("Worship", 3),
        ("Aftercare", 5)]


def main():
    deck = json.loads(DECK.read_text(encoding="utf-8"))
    pools = {}
    for c in deck["cards"]:
        pools.setdefault(c["category"], []).append(c)

    ordered = []
    for category, count in PLAN:
        available = pools.get(category, [])
        if len(available) < count:
            print(f"  !! not enough {category}: need {count}, have {len(available)}")
            return 1
        ordered += [available.pop(0) for _ in range(count)]

    leftover = {k: len(v) for k, v in pools.items() if v}
    if leftover:
        print(f"  !! cards left over, plan doesn't cover the deck: {leftover}")
        return 1

    deck["cards"] = ordered
    DECK.write_text(json.dumps(deck, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("JSON reordered:", " ".join(c["category"][:4] for c in ordered))

    # ── Reorder the C# bank to the same sequence ──────────────────────────────
    text = BANK.read_text(encoding="utf-8")
    start = text.index("private static IReadOnlyList<ICard> Build() =>")
    open_br = text.index("[", start)
    close_br = text.index("\n    ];", open_br)
    array = text[open_br + 1:close_br]

    # Split the array into blocks, one per helper call, keeping any comment
    # lines attached to the block that follows them.
    blocks, current = [], []
    for line in array.split("\n"):
        if re.match(r'^\s+(C|R|SW|A)\("', line) and current and any(
                re.match(r'^\s+(C|R|SW|A)\("', l) for l in current):
            blocks.append("\n".join(current))
            current = [line]
        else:
            current.append(line)
    if current:
        blocks.append("\n".join(current))

    def title_of(block):
        m = re.search(r'(C|R|SW|A)\("([^"]*)"(?:,\s*"([^"]*)")?', block)
        if not m:
            return None
        return m.group(3) if m.group(1) == "R" else m.group(2)

    by_title = {}
    for b in blocks:
        t = title_of(b)
        if t:
            by_title[t] = re.sub(r"^\s*\n", "", b)   # drop section comments

    missing = [c["title"] for c in ordered if c["title"] not in by_title]
    if missing:
        print(f"  !! bank is missing {len(missing)} cards, not reordering: {missing[:3]}")
        return 1

    rebuilt = "\n" + "\n".join(by_title[c["title"]].rstrip() for c in ordered)
    BANK.write_text(text[:open_br + 1] + rebuilt + text[close_br:], encoding="utf-8")
    print(f"bank reordered: {len(ordered)} blocks")
    return 0


if __name__ == "__main__":
    sys.exit(main())
