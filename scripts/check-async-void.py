#!/usr/bin/env python3
"""
Check that every `async void` method in an Android-producing head guards its
body, so an exception cannot escape into a handler with no caller left to
catch it.

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
`SafeNavigation.SafePopToRootAsync`, so most MAUI handlers now satisfy this
by delegating rather than by repeating a try/catch.

WHY IT COVERS TWO HEADS
------------------------
It was `check-maui-async-void.py` and scanned `ui/TableTop.Maui` alone, which
left the gap backlog item N.4 recorded: the *native* Android head — the one
where the crash this script is named after actually happens — shipped in
1.32.0 with an `async void` handler no gate looked at. It was hand-guarded,
which is exactly the state MAUI was in when item 27 found eight misses. The
argument above ("a script and not a review habit") applies to whichever head
runs on Android, not to whichever head happened to exist when the script was
written.

MAUI's Windows/iOS/macOS targets are scanned too, because the same source
files build for Android and the failure is a property of the code, not of the
target you happen to be debugging on.

WHAT IT CHECKS
--------------
For every file matched by HEADS below, each `async void` method must either:
  * wrap its body in `try` { ... } `catch`, or
  * delegate to a helper on that head's SAFE_DELEGATES list, which is itself
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
    python3 scripts/check-async-void.py
"""

import re
import sys
from pathlib import Path


class Head:
    """One head to scan: where it lives, which files, and its guarded helpers."""

    def __init__(self, label, root, pattern, safe_delegates=()):
        self.label = label
        self.root = root
        self.pattern = pattern
        # Helpers that already own the try/catch, so a handler delegating to
        # one is guarded. Keep these lists short and make sure every entry
        # really is guarded — an unguarded name here silently disables the
        # check for its callers. Per-head on purpose: SafePopToRootAsync is a
        # MAUI type, and a same-named Android helper would not be the same
        # method.
        self.safe_delegates = safe_delegates


HEADS = (
    # MAUI: only code-behind has handlers; ViewModels are plain classes with
    # no event wiring, and a `*.xaml.cs` glob is what the original check used.
    Head("MAUI", "ui/TableTop.Maui", "*.xaml.cs", safe_delegates=("SafePopToRootAsync",)),

    # Native Android draws its own view trees, so handlers live in ordinary
    # .cs files (Screens/, MainActivity) with no XAML counterpart. Scan all of
    # them rather than guessing at a directory convention that could change.
    Head("Android (native)", "ui/TableTop.Android", "*.cs"),
)

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


def is_guarded(body: str, safe_delegates) -> bool:
    if "await" not in body:
        return True                                   # nothing to observe
    if any(d in body for d in safe_delegates):
        return True
    return "try" in body and "catch" in body


def scan(repo: Path, head: Head):
    """Returns (problems, handlers_checked, files_scanned) for one head."""
    root = repo / head.root
    if not root.exists():
        return [], 0, 0

    files = [
        p for p in root.rglob(head.pattern)
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
            if not is_guarded(body_after(text, m.end()), head.safe_delegates):
                line = text.count("\n", 0, m.start()) + 1
                problems.append(
                    f"{path.relative_to(repo).as_posix()}:{line}: "
                    f"async void {m.group(1)} awaits without try/catch - "
                    f"an exception here terminates the process on Android"
                )

    return problems, checked, len(files)


def main() -> int:
    repo = Path(__file__).resolve().parent.parent

    all_problems, summaries, scanned_any = [], [], False
    for head in HEADS:
        problems, checked, files = scan(repo, head)
        if files == 0:
            summaries.append(f"{head.label}: not present - skipped")
            continue

        scanned_any = True
        all_problems.extend(problems)
        summaries.append(
            f"{head.label}: {checked} async void handler(s) across {files} file(s)")

    if not scanned_any:
        print("No Android-producing head found - nothing to check.")
        return 0

    for p in all_problems:
        print(f"  {p}")

    for s in summaries:
        print(f"  {s}")

    if all_problems:
        print(f"\n{len(all_problems)} unguarded async void handler(s)")
        return 1

    print("\nall async void handlers guarded")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
