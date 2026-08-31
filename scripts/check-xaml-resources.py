#!/usr/bin/env python3
"""
Check that every {StaticResource X} / {DynamicResource X} in a XAML head
resolves to an x:Key="X" defined somewhere in that same head.

WHY THIS EXISTS
---------------
A missing resource key is not a compile error on either XAML head. It is a
crash the moment the page is NAVIGATED TO, reading something like

    Cannot find resource named 'SecondaryButtonStyle'

which reaches the player rather than the developer. The failure mode is
identical to the one check-maui-xaml.py was written for — valid XML, valid
XAML, wrong at runtime — and it is easy to hit for exactly the reason it was
hit here: a new page copied the shape of an existing one and guessed at a
style name that sounded right. MAUI's secondary button style is called
QuietButtonStyle; "SecondaryButtonStyle" had never existed.

That mistake was caught by grepping the resource dictionary by hand while
writing the trait-profile screens. Nothing would have caught it otherwise:
the XAML parses, every binding resolves, and all seven other gates pass.

WHAT IT CHECKS
--------------
Per head, as a closed set: collect every x:Key in the head's XAML, collect
every {StaticResource}/{DynamicResource} reference, and report references
with no definition. Both heads resolve entirely within themselves today —
neither references a resource shipped by the MAUI or WinUI SDK — which is
what makes the closed-set check sound rather than noisy. If a head ever does
need an SDK resource, this will report it, and the honest fix is an
explicitly named allowlist here rather than loosening the check.

WHAT IT CANNOT DO
-----------------
Textual matching, like every other check-*.py. It does not verify that a
resolved resource is of a usable *type* — a Style keyed for Button applied
to a Label still fails at runtime, which is what check-maui-xaml.py and
check-winui-xaml.py cover from the other direction. It also does not detect
an unused key, which is harmless.

USAGE
-----
    python3 scripts/check-xaml-resources.py            # checks both XAML heads
    python3 scripts/check-xaml-resources.py <dir>

Exits non-zero if anything is found, so it can gate CI.
"""

import re
import sys
from pathlib import Path

REFERENCE = re.compile(r"\{(?:Static|Dynamic)Resource\s+([A-Za-z_][\w.]*)\s*\}")
KEY = re.compile(r'x:Key="([^"]+)"')

DEFAULT_HEADS = ["ui/TableTop.Maui", "ui/TableTop.WinUI"]


def xaml_files(root: Path):
    """Every real .xaml file under root, skipping build output.

    is_file() and the bin/obj filter are both load-bearing for the reason
    check-maui-xaml.py records: on Windows a build leaves a *directory* named
    Microsoft.UI.Xaml under bin, and rglob happily yields directories.
    """
    return sorted(
        f for f in root.rglob("*.xaml")
        if f.is_file() and not {"bin", "obj"} & set(f.parts)
    )


def check(root: Path):
    """Yield (file, line, key) for every reference that resolves to nothing."""
    files = xaml_files(root)

    defined = set()
    for f in files:
        defined |= set(KEY.findall(f.read_text(encoding="utf-8", errors="ignore")))

    for f in files:
        for number, line in enumerate(f.read_text(encoding="utf-8", errors="ignore").split("\n"), 1):
            for name in REFERENCE.findall(line):
                if name not in defined:
                    yield f.name, number, name

    return files


def main() -> int:
    repo = Path(__file__).resolve().parent.parent
    roots = [Path(sys.argv[1])] if len(sys.argv) > 1 else [repo / h for h in DEFAULT_HEADS]

    problems = []
    checked = 0

    for root in roots:
        if not root.exists():
            print(f"no such directory: {root}")
            return 2

        files = xaml_files(root)
        checked += len(files)
        for name, line, key in check(root):
            problems.append(f"{name}:{line}  {{StaticResource {key}}} is not defined in {root.name}")

    for p in problems:
        print(p)

    print(f"\nchecked {checked} XAML files across {len(roots)} head(s): "
          + ("every resource reference resolves" if not problems
             else f"{len(problems)} unresolved reference(s)"))
    return 1 if problems else 0


if __name__ == "__main__":
    raise SystemExit(main())
