#!/usr/bin/env python3
"""
Check that every `async void` method in a MAUI page guards its body, so an
exception cannot escape into a handler with no caller left to catch it.

WHY THIS EXISTS
----------------
An exception escaping an `async void` handler terminates the process on
Android. This codebase already knew that and said so, verbatim, above four
handlers:

    // An exception escaping an async void handler terminates the
    // process on Android; surface it instead.

But the rule was applied by hand, so other handlers never got it. Backlog
item 27 counted six by reading: five identical `OnDoneClicked` one-liners
plus `SettingsPage.OnRoasterClicked`, which was added three lines above one
of those very comments and still went unguarded.

**The real number was eight.** The first run of this script found two more
that reading had missed — `GameSelectionPage.OnAppearing`, the one handler
that runs with no user action at all, and
`PlayerSetupPage.OnSaveRosterClicked`. That gap between a careful manual
count and a mechanical one is the whole argument for this file existing.

The fix that closed item 27 also moved the shared case into
`SafeNavigation.SafePopToRootAsync`, so most handlers now satisfy this by
delegating rather than by repeating a try/catch.

WHAT IT CHECKS
--------------
For each `ui/TableTop.Maui/**/*.xaml.cs` file, every `async void` method must
either:
  * wrap its body in `try` { ... } `catch`, or
  * delegate to a helper on the SAFE_DELEGATES list below, which is itself
    guarded (that is the single-source version of the same rule), or
  * await nothing at all — no `await` means no continuation, so there is no
    unobserved-exception path to protect against.

WHAT IT CANNOT DO
-----------------
Brace-matching and substring tests, not a C# parser. It does not verify the
try actually encloses the awaits (a `try` after the last `await` would pass),
and it does not check that the catch is broad enough to matter. It is aimed
at the failure that actually happened — a handler with no guard at all — not
at a subtly mis-scoped one.

USAGE
-----
    python3 scripts/check-maui-async-void.py
"""

import re
import sys
from pathlib import Path

MAUI_ROOT = "ui/TableTop.Maui"

# Helpers that already own the try/catch, so a handler delegating to one is
# guarded. Keep this list short and make sure every entry really is guarded —
# an unguarded name added here silently disables the check for its callers.
SAFE_DELEGATES = ("SafePopToRootAsync",)

ASYNC_VOID = re.compile(r"\basync\s+void\s+(\w+)\s*\(")


def body_after(text: str, start: int) -> str:
    """Returns the method body following `start`, expression- or block-bodied."""
    arrow, brace = text.find("=>", start), text.find("{", start)

    # Expression-bodied (`=> await Foo();`) when the arrow comes first.
    if arrow != -1 and (brace == -1 or arrow < brace):
        end = text.find(";", arrow)
        return text[arrow:end if end != -1 else len(text)]

    if brace == -1:
        return ""

    depth, i = 0, brace
    while i < len(text):
        if text[i] == "{":
            depth += 1
        elif text[i] == "}":
            depth -= 1
            if depth == 0:
                return text[brace:i + 1]
        i += 1
    return text[brace:]


def is_guarded(body: str) -> bool:
    if "await" not in body:
        return True                                   # nothing to observe
    if any(d in body for d in SAFE_DELEGATES):
        return True
    return "try" in body and "catch" in body


def main() -> int:
    repo = Path(__file__).resolve().parent.parent
    root = repo / MAUI_ROOT
    if not root.exists():
        print("No MAUI head found — nothing to check.")
        return 0

    files = [
        p for p in root.rglob("*.xaml.cs")
        if p.is_file() and "bin" not in p.parts and "obj" not in p.parts
    ]

    problems, checked = [], 0
    for path in sorted(files):
        text = path.read_text(encoding="utf-8", errors="replace")
        for m in ASYNC_VOID.finditer(text):
            # Skip matches inside a comment line — the rule is quoted in prose
            # above several handlers, and that prose is not a declaration.
            line_start = text.rfind("\n", 0, m.start()) + 1
            if text[line_start:m.start()].lstrip().startswith("//"):
                continue

            checked += 1
            if not is_guarded(body_after(text, m.end())):
                line = text.count("\n", 0, m.start()) + 1
                problems.append(
                    f"{path.relative_to(repo).as_posix()}:{line}: "
                    f"async void {m.group(1)} awaits without try/catch - "
                    f"an exception here terminates the process on Android"
                )

    for p in problems:
        print(f"  {p}")

    if problems:
        print(f"\nchecked {checked} async void handler(s): {len(problems)} unguarded")
        return 1

    print(f"checked {checked} async void handler(s) across "
          f"{len(files)} MAUI page(s): all guarded")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
