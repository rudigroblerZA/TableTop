#!/usr/bin/env python3
"""
Check that each head's declared ControllerFamily support matches the copy of
it hand-typed into ControllerFamilyTests.cs.

WHY THIS EXISTS
---------------
MAUI, WinUI and Console each declare a `SupportedFamilies` property naming
which ControllerFamily values they have a screen or renderer for (backlog
item 4). HeadFamilyCoverageTests asserts against those families — but it
cannot reference MAUI or WinUI directly, because both need SDKs this project
does not have, so it keeps a hand-typed copy of each head's array instead
(MauiSupported, WinUiSupported, ConsoleSupported).

Backlog item 12 named the resulting gap precisely: "the test reads its own
copy, so updating the head alone changes nothing." Someone adds a screen,
updates the head's SupportedFamilies, and the test — reading a copy that
nobody touched — keeps passing on stale data. The coverage tests would then
be asserting a shortfall that no longer exists, or missing one that does.

WHAT IT CHECKS
--------------
For each of the three (head source, test array name) pairs below: parses the
ControllerFamily.* literal list out of both, and diffs them as sets. A
mismatch in either direction is reported — the head declaring something the
test doesn't know about, or the test believing in something the head no
longer declares.

WHAT IT CANNOT DO
------------------
Textual matching, like every other check in this file — it does not verify
that a declared family is actually reachable at runtime (that's what
HeadFamilyCoverageTests and ControllerFamilyTests exercise once the two
sides agree on what's being tested). It only keeps the two declarations from
silently drifting apart, which is the specific failure mode item 12 found.

USAGE
-----
    python3 scripts/check-head-family-coverage.py
"""

import re
import sys
from pathlib import Path

TEST_FILE = "tests/TableTop.Tests/ControllerFamilyTests.cs"

# (label, head source file, property name in the head, array name in the test)
SOURCES = [
    ("MAUI",    "ui/TableTop.Maui/Pages/PlayerSetupPage.xaml.cs",            "SupportedFamilies", "MauiSupported"),
    ("WinUI",   "ui/TableTop.WinUI/ViewModels/GameViewModels.cs",            "SupportedFamilies", "WinUiSupported"),
    ("Console", "ui/TableTop.Console/ConsoleGameLauncher.cs",                "SupportedFamilies", "ConsoleSupported"),
    ("Android", "ui/TableTop.Android/Infrastructure/GameScreenFactory.cs",   "SupportedFamilies", "AndroidSupported"),
]

FAMILY = re.compile(r"ControllerFamily\.(\w+)")


def families_after(text: str, declaration_name: str) -> set[str] | None:
    """
    Finds `<declaration_name> ... = [ ... ];` — a property or field
    initialised with a collection-expression list of ControllerFamily.X
    values — and returns the set of family names inside the brackets.

    Matches an IReadOnlyList<ControllerFamily> property (`{ get; } = [...]`)
    or a ControllerFamily[] field (`= [...]`) with the same regex, since both
    heads and the test use the shape that suits them and this check cares
    only about the values, not the declared type.
    """
    m = re.search(rf"\b{re.escape(declaration_name)}\b[^=]*=\s*\[(.*?)\]\s*;", text, re.S)
    if m is None:
        return None
    return set(FAMILY.findall(m.group(1)))


def main() -> int:
    repo = Path(__file__).resolve().parent.parent
    test_path = repo / TEST_FILE
    if not test_path.exists():
        print(f"{TEST_FILE} not found — nothing to check.")
        return 0
    test_text = test_path.read_text(encoding="utf-8", errors="ignore")

    problems: list[str] = []
    checked = 0

    for label, head_rel, head_prop, test_array in SOURCES:
        head_path = repo / head_rel
        if not head_path.exists():
            problems.append(f"{head_rel} not found — cannot verify {label}'s declared families")
            continue

        head_text = head_path.read_text(encoding="utf-8", errors="ignore")
        head_families = families_after(head_text, head_prop)
        test_families = families_after(test_text, test_array)

        if head_families is None:
            problems.append(f"{head_rel}: could not find a '{head_prop}' declaration to parse")
            continue
        if test_families is None:
            problems.append(f"{TEST_FILE}: could not find a '{test_array}' declaration to parse")
            continue

        checked += 1
        missing_from_test = head_families - test_families
        missing_from_head = test_families - head_families

        if missing_from_test:
            problems.append(
                f"{label}: {head_rel} declares "
                f"{', '.join(sorted(missing_from_test))} that {TEST_FILE}'s "
                f"{test_array} does not know about — coverage tests are running "
                f"against a stale copy")
        if missing_from_head:
            problems.append(
                f"{label}: {TEST_FILE}'s {test_array} claims "
                f"{', '.join(sorted(missing_from_head))}, which {head_rel} no "
                f"longer declares — coverage tests are asserting a shortfall "
                f"that isn't real, or missing one that is")

    for p in problems:
        print(p)

    print(f"\nchecked {checked} head declaration(s) against their test copy: "
          + ("all match" if not problems
             else f"{len(problems)} mismatch(es)"))
    return 1 if problems else 0


if __name__ == "__main__":
    raise SystemExit(main())
