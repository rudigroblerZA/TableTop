#!/usr/bin/env python3
"""
Check that every `_vm.Method(...)` call in a MAUI page's code-behind resolves
to a real public method on the shared ViewModel it's bound to.

WHY THIS EXISTS
----------------
MAUI's code-behind calls plain methods directly (`_vm.Complete()`); WinUI binds
`ICommand` properties (`{Binding CompleteCommand}`). When Monogamy and Settings
were merged into TableTop.Presentation, the WinUI side's ICommand shape carried
over cleanly — and the plain methods MAUI's existing pages already called
(`Complete()`, `Skip()`, `Negotiate()`, `ResetToDefaults()`) were never added to
the merged classes. Four CS1061/CS1501s reached a real build.

Millionaire's merge got this right — `AnswerOption`/`LifelineOption` carry both
an `ICommand` *and* an `Invoke()` method, deliberately, for exactly this
duality. Monogamy and Settings didn't, because nothing checked for the
asymmetry. This does.

WHAT IT CHECKS
--------------
For each `ui/TableTop.Maui/Pages/*.xaml.cs` file:
  1. Find the shared ViewModel type it's built around (from the field
     declaration or constructor call).
  2. Find every `_vm.Method(...)` call in the file.
  3. Confirm a public method of that name exists on the ViewModel — not just
     an `ICommand` property of a similar name.

WHAT IT CANNOT DO
-----------------
Textual matching, not real overload resolution — it does not check argument
counts or types (that gap is exactly what `check-ui-compiles.py` is for, on
the files it can see). It also only covers the five ViewModels already in
TableTop.Presentation; a screen using a MAUI-only or WinUI-only ViewModel
is out of scope by design.

USAGE
-----
    python3 scripts/check-mvvm-method-parity.py
"""

import re
import sys
from pathlib import Path

SHARED_VM_DIR = "src/TableTop.Presentation/ViewModels"
MAUI_PAGES = "ui/TableTop.Maui/Pages"

# A member counts as present if it's a public method OR the ICommand-suffixed
# version exists — some calls are legitimately `.Execute()` etc. on a command,
# which this does not attempt to model; those show up as false positives if
# ever used, which is the safe direction to be wrong in.
PUBLIC_METHOD = re.compile(
    r"public\s+(?:async\s+)?[\w<>\[\],?]+(?:\s+[\w<>\[\],?]+)*\s+(\w+)\s*\("
)


def shared_vm_members(repo: Path) -> dict[str, set[str]]:
    members: dict[str, set[str]] = {}
    for path in (repo / SHARED_VM_DIR).glob("*.cs"):
        text = path.read_text(encoding="utf-8", errors="ignore")
        cls = path.stem
        members[cls] = set(PUBLIC_METHOD.findall(text))
    return members


def vm_type_for(text: str, known: set[str]) -> str | None:
    for cls in known:
        if re.search(rf"\b{re.escape(cls)}\b", text):
            return cls
    return None


def main() -> int:
    repo = Path(__file__).resolve().parent.parent
    pages_dir = repo / MAUI_PAGES
    if not pages_dir.exists():
        print("No MAUI pages found — nothing to check.")
        return 0

    known = shared_vm_members(repo)
    problems: list[str] = []
    checked = 0

    for path in sorted(pages_dir.glob("*.xaml.cs")):
        text = path.read_text(encoding="utf-8", errors="ignore")
        cls = vm_type_for(text, set(known))
        if cls is None:
            continue
        checked += 1

        for m in re.finditer(r"_vm\.(\w+)\s*\(", text):
            name = m.group(1)
            if name in {"GetAwaiter", "ToString", "Equals"}:
                continue
            if name not in known[cls]:
                line = text[: m.start()].count("\n") + 1
                problems.append(
                    f"{path.relative_to(repo).as_posix()}:{line}  "
                    f"calls _vm.{name}() but {cls} has no public method '{name}'")

    for p in problems:
        print(p)

    print(f"\nchecked {checked} MAUI page(s) bound to a shared ViewModel: "
          + ("all method calls resolve" if not problems
             else f"{len(problems)} missing method(s)"))
    return 1 if problems else 0


if __name__ == "__main__":
    raise SystemExit(main())
