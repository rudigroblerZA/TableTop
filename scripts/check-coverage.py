#!/usr/bin/env python3
"""
Enforce a coverage floor against a Cobertura report, so coverage can only
ratchet upward.

WHY THIS EXISTS
----------------
`ARCHITECTURE.md` carried "a committed, real coverage percentage" under **What
genuinely doesn't exist here** for a long time, because measuring it needed
NuGet access the usual sandbox lacked. It was finally measured in 1.35.1 —
92.0% line / 71.5% branch — and CI has been collecting Cobertura and printing a
`reportgenerator` summary all along. Printing it is not the same as enforcing
it: nothing failed when a number went down, so the summary was an artifact to
download rather than a gate (backlog L.1).

WHAT IT CHECKS
--------------
Total line and branch rate against `--min-line` / `--min-branch`, and each
assembly against its own floor in ASSEMBLY_FLOORS below.

Per-assembly floors matter more than the total here. `TableTop.Games` is ~60% of
the tree by line count and sits at 93%, so it can mask a sharp fall in
`TableTop.Hosting` or `TableTop.Presentation` while the total barely moves —
and those two are where the logic that breaks actually lives.

**Floors are set a point or so BELOW the measured value, deliberately.** The
point is to catch a real regression, not to fail a build because a refactor
moved twenty lines between files. Raise them when a change earns it; that is a
one-line edit and a visible decision, which is the same argument
`ControllerSizeGuardTests` makes about its own ceilings.

BRANCH COVERAGE IS THE INTERESTING NUMBER. Line coverage here is comfortable
and stable; branch coverage is lower and is where the untested error paths and
null guards are — the code that only runs once something has already gone
wrong.

USAGE
-----
    python3 scripts/check-coverage.py <path-to-coverage.cobertura.xml>
    python3 scripts/check-coverage.py <dir>          # finds the report under it
    python3 scripts/check-coverage.py <dir> --min-line 90 --min-branch 69
"""

import argparse
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

# Measured 2026-08-30 at: Core 87.0/76.0, Games 93.0/90.2, Hosting 88.5/68.7,
# Presentation 89.7/61.9, total 91.9/70.8. Floors sit ~1 point under each.
DEFAULT_MIN_LINE = 90.0
DEFAULT_MIN_BRANCH = 69.0

ASSEMBLY_FLOORS = {
    #                     line, branch
    "TableTop.Core":         (86.0, 75.0),
    "TableTop.Games":        (92.0, 89.0),
    "TableTop.Hosting":      (87.0, 67.0),
    "TableTop.Presentation": (88.0, 60.0),
}


def find_report(target: Path) -> Path | None:
    if target.is_file():
        return target
    if not target.is_dir():
        return None
    reports = sorted(target.rglob("coverage.cobertura.xml"))
    return reports[-1] if reports else None


def pct(node, attr: str) -> float:
    raw = node.get(attr)
    return round(float(raw) * 100, 1) if raw is not None else 0.0


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("path", help="coverage.cobertura.xml, or a directory containing one")
    ap.add_argument("--min-line", type=float, default=DEFAULT_MIN_LINE)
    ap.add_argument("--min-branch", type=float, default=DEFAULT_MIN_BRANCH)
    args = ap.parse_args()

    report = find_report(Path(args.path))
    if report is None:
        # Loud, not silent. A missing report means the test step changed shape
        # or failed; treating that as "nothing to check" would quietly disable
        # this gate, which is the failure mode it exists to prevent elsewhere.
        print(f"No coverage.cobertura.xml found at or under '{args.path}'.")
        return 1

    root = ET.parse(report).getroot()

    failures: list[str] = []

    total_line, total_branch = pct(root, "line-rate"), pct(root, "branch-rate")
    print(f"  {'TOTAL':<24} line {total_line:5.1f}%  branch {total_branch:5.1f}%"
          f"   (floor {args.min_line}/{args.min_branch})")
    if total_line < args.min_line:
        failures.append(f"total line coverage {total_line}% is below the {args.min_line}% floor")
    if total_branch < args.min_branch:
        failures.append(f"total branch coverage {total_branch}% is below the {args.min_branch}% floor")

    seen = set()
    for pkg in root.iter("package"):
        name = pkg.get("name") or "?"
        seen.add(name)
        line, branch = pct(pkg, "line-rate"), pct(pkg, "branch-rate")
        floor = ASSEMBLY_FLOORS.get(name)

        if floor is None:
            print(f"  {name:<24} line {line:5.1f}%  branch {branch:5.1f}%   (no floor set)")
            continue

        print(f"  {name:<24} line {line:5.1f}%  branch {branch:5.1f}%"
              f"   (floor {floor[0]}/{floor[1]})")
        if line < floor[0]:
            failures.append(f"{name} line coverage {line}% is below its {floor[0]}% floor")
        if branch < floor[1]:
            failures.append(f"{name} branch coverage {branch}% is below its {floor[1]}% floor")

    # An assembly that has a floor but produced no data is a silent hole: the
    # report would look clean while a whole project went unmeasured.
    for name in sorted(set(ASSEMBLY_FLOORS) - seen):
        failures.append(f"{name} has a coverage floor but no data in the report")

    if failures:
        print()
        for f in failures:
            print(f"  FAIL: {f}")
        print("\nCoverage dropped below its floor. Add tests, or move the floor in a "
              "commit that does nothing else so the decision is visible in review.")
        return 1

    print("\nall coverage floors met")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
