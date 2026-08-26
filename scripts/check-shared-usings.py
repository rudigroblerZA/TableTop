#!/usr/bin/env python3
"""
Check that every file using a shared type imports the namespace it lives in.

WHY THIS EXISTS
---------------
This has now broken a real build three separate times, each after moving a type
into TableTop.Presentation:

  * ViewModelBase moved  -> ViewLocator.cs, PlayerSetupViewModel.cs, MauiProgram.cs
  * the ViewModels moved -> PickerViewModels.cs, ViewLocator.cs, PlayerSetupView.xaml.cs

`check-ui-compiles.py` was written to catch exactly this and CANNOT. Two reasons,
both structural rather than fixable:

  1. **The compiler stops at 100 errors.** WinUI and MAUI together produce ~100
     unresolvable framework-type errors before reaching anything real, so
     first-party failures past that point are never reported at all.

  2. **Framework failures mask first-party ones in the same expression.** In
     ViewLocator.cs the line is

         [typeof(SettingsViewModel)] = () => new SettingsView(),

     `UIElement` cannot resolve without the WinUI SDK, so the whole initializer
     fails to bind and the compiler reports only that. The missing
     `SettingsViewModel` is never mentioned. Splitting the compilation per head
     fixes (1) and does nothing for (2).

So this checks the thing directly instead of hoping a compiler will: if a file
names a shared type, it must import that type's namespace. No compilation, no
SDKs, no masking.

WHAT IT CANNOT DO
-----------------
It matches identifiers textually, so a type named in a comment or a string counts
as a use. That errs toward a false positive — an extra using — which is
harmless, unlike the false negative it replaces.

Fully-qualified uses (`TableTop.Presentation.ViewModels.SettingsViewModel`) are
recognised and need no using.

USAGE
-----
    python3 scripts/check-shared-usings.py
"""

import re
import sys
from pathlib import Path

# namespace -> the types that live in it
SHARED = {
    "TableTop.Presentation.Infrastructure": [
        "ViewModelBase", "RelayCommand", "AsyncRelayCommand",
        "INavigator", "IAppSettings", "SavedPlayer", "SavedSessionLookup",
    ],
    "TableTop.Presentation.ViewModels": [
        "SettingsViewModel", "PlayerSetupViewModel", "MonogamyGameViewModel",
        "MillionaireGameViewModel", "DayOneGameViewModel",
        "ModeListItem", "ModeDisplayResolver",
    ],
}

SEARCH_ROOTS = ["ui"]


def main() -> int:
    repo = Path(__file__).resolve().parent.parent
    problems = []
    checked = 0

    for root in SEARCH_ROOTS:
        base = repo / root
        if not base.exists():
            continue
        for path in sorted(base.rglob("*.cs")):
            if "obj" in path.parts or "bin" in path.parts:
                continue
            text = path.read_text(encoding="utf-8", errors="ignore")
            checked += 1

            for namespace, types in SHARED.items():
                # Already imported, or the file IS that namespace? Nothing to do.
                if re.search(rf"^\s*using\s+{re.escape(namespace)}\s*;", text, re.M):
                    continue
                if re.search(rf"^\s*namespace\s+{re.escape(namespace)}\b", text, re.M):
                    continue

                for t in types:
                    # Fully-qualified uses carry their own namespace.
                    if re.search(rf"\b{re.escape(namespace)}\.{t}\b", text):
                        continue
                    if re.search(rf"\b{t}\b", text):
                        rel = path.relative_to(repo).as_posix()
                        problems.append((rel, t, namespace))
                        break   # one report per file per namespace is enough

    for rel, t, ns in problems:
        print(f"{rel}")
        print(f"    uses '{t}' but does not import '{ns}'")

    print(f"\nchecked {checked} C# files: "
          + ("all shared types imported" if not problems
             else f"{len(problems)} missing using(s)"))
    return 1 if problems else 0


if __name__ == "__main__":
    raise SystemExit(main())
