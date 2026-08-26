#!/usr/bin/env python3
"""
Delete files that a release removed but an unzip-over-the-old-folder left behind.

WHY THIS EXISTS
---------------
Releases are delivered as a zip. Extracting a zip over an existing directory
adds and overwrites, but never deletes — so any file removed in a release
survives locally and keeps compiling.

That is not theoretical. 1.5.0 moved `ViewModelBase`, `RelayCommand` and
`AsyncRelayCommand` out of `TableTop.WinUI.Infrastructure` into the shared
`TableTop.Presentation.Infrastructure`. The zip contained only the new copies,
but the old ones stayed on disk, and every WinUI ViewModel then failed with:

    CS0104: 'ViewModelBase' is an ambiguous reference between
            'TableTop.WinUI.Infrastructure.ViewModelBase' and
            'TableTop.Presentation.Infrastructure.ViewModelBase'

26 of them. The zip was correct; the leftover file was the problem.

USAGE
-----
    python3 scripts/remove-stale-files.py            # report only
    python3 scripts/remove-stale-files.py --delete   # actually remove

The cleanest alternative is extracting each release into a fresh directory,
which makes this script unnecessary.
"""

import sys
from pathlib import Path

# Paths that must NOT exist at or after the stated release.
STALE = [
    ("1.5.0", "ui/TableTop.WinUI/Infrastructure/ViewModelBase.cs",
              "moved to src/TableTop.Presentation/Infrastructure/"),
    ("1.5.0", "ui/TableTop.WinUI/Infrastructure/RelayCommand.cs",
              "moved to src/TableTop.Presentation/Infrastructure/"),
    ("1.5.0", "ui/TableTop.WinUI/Infrastructure/AsyncRelayCommand.cs",
              "moved to src/TableTop.Presentation/Infrastructure/"),
    ("1.5.0", "ui/TableTop.Maui/ViewModels/SettingsViewModel.cs",
              "replaced by the shared SettingsViewModel"),
    ("1.5.0", "ui/TableTop.Maui/Services/CastService.cs",
              "Cast to TV removed — was MAUI-only"),
    ("1.6.0", "ui/TableTop.Maui/ViewModels/MonogamyGameViewModel.cs",   "shared into TableTop.Presentation"),
    ("1.6.0", "ui/TableTop.Maui/ViewModels/PlayerSetupViewModel.cs",    "shared into TableTop.Presentation"),
    ("1.6.0", "ui/TableTop.Maui/ViewModels/MillionaireGameViewModel.cs","shared into TableTop.Presentation"),
    ("1.6.0", "ui/TableTop.Maui/ViewModels/DayOneGameViewModel.cs",     "shared into TableTop.Presentation"),
    ("1.6.0", "ui/TableTop.WinUI/ViewModels/SpecialisedGameViewModels.cs",
              "all three classes shared into TableTop.Presentation; file emptied"),
    ("1.7.0", "src/TableTop.Games/Family/TimelineMode.cs",
              "duplicate of Chronology Challenge; its 27 cards were merged into that deck"),
    ("1.7.0", "src/TableTop.Games/Data/Json/timeline.deck.json",
              "cards merged into chronology-challenge.deck.json"),
    ("1.10.6", "docs",
              "whole folder removed — replaced by ARCHITECTURE.md and BACKLOG.md "
              "at the repo root; api/*.api.txt moved to api/ at the root"),
    ("1.0.0", "ui/TableTop.Wpf",
              "the WPF head was removed"),
]


def main() -> int:
    root = Path(__file__).resolve().parent.parent
    delete = "--delete" in sys.argv
    found = []

    for version, rel, why in STALE:
        p = root / rel
        if p.exists():
            found.append((version, rel, why, p))

    if not found:
        print("No stale files. Nothing to do.")
        return 0

    print(f"{len(found)} stale path(s) found:\n")
    for version, rel, why, p in found:
        print(f"  {rel}")
        print(f"      removed in {version} — {why}")
        if delete:
            if p.is_dir():
                import shutil
                shutil.rmtree(p)
            else:
                p.unlink()
            print("      DELETED")
    print()

    if not delete:
        print("Report only. Re-run with --delete to remove them.")
        return 1

    print("Done. Rebuild.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
