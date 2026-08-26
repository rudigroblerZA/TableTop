# One-shot content migrations

These are **migrations, not tools**. Each mutates deck JSON or C# card banks in
place, and each has already been applied — their output is committed.

Backlog H.5: they used to sit in `scripts/` alongside `check-maui-xaml.py` and
`check-xaml-bindings.py`, which are standing checks meant to be run on every
push. Nothing distinguished the two, and nothing recorded whether a migration had
been run. Re-running one is not obviously idempotent — `extend-adult-decks.py`
appends cards, so a second run would duplicate them.

| Script | Applied | What it did |
|---|---|---|
| `add-adult-cards.py` | ✅ before 2026-08-05 | Added adult-rated cards to the couples decks |
| `extend-adult-decks.py` | ✅ before 2026-08-05 | Extended those decks further. **Appends — do not re-run** |
| `mirror-adult-cards-to-banks.py` | ✅ before 2026-08-05 | Mirrored the JSON additions into the C# card banks |
| `mirror-extend-to-banks.py` | ✅ before 2026-08-05 | Same, for the extension pass |
| `reorder-undivided-swaps.py` | ✅ before 2026-08-05 | Reordered swap cards in `undivided` so turn-taking reads correctly |

"Before 2026-08-05" is as precise as the record goes — these predate the review
that noticed the problem, and no log was kept. Dates for anything added from here
should be exact.

## If you add one

1. Put it here, not in `scripts/`.
2. Name it with a date prefix: `2026-08-06-thing-it-does.py`.
3. Add a row above the day it is applied.
4. Say in its docstring whether re-running is safe.

## Note on the C# banks

Four of these mirror JSON changes into C# card banks. Since backlog H.1 every
mode loads JSON first and falls back to its bank only when the file is missing,
so the banks matter far less than they did — a future content change probably
does not need a mirror step at all.
