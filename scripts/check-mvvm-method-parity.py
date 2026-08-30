#!/usr/bin/env python3
"""
Check that every `_vm.Method(...)` / `Vm.Method(...)` call in a head's screen
code resolves to a real public method on the shared ViewModel it's bound to.

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

WHY IT COVERS TWO HEADS
------------------------
The native Android head has the same duality and was out of scope until backlog
L.2. Its screens call `Vm.Method()` — a protected property on
`GameScreenBase<T>` — where MAUI's code-behind calls `_vm.Method()`. Same plain-
method dependency on the same shared ViewModels, same way to break.

Android is compiled in CI, so a break there fails `build-android` rather than
reaching a user; that is why L.2 was filed as "Later" and not urgent. It is
still worth checking here, because this gate names the missing member *and* the
ViewModel it should be on, where a CS1061 several hundred lines into a build log
does not.

WHAT IT CHECKS
--------------
For each file matched by HEADS below:
  1. Find the shared ViewModel type it's built around (from the field
     declaration, constructor call, or generic base).
  2. Find every `_vm.Method(...)` / `Vm.Method(...)` call in the file.
  3. Confirm a public method of that name exists on the ViewModel — not just
     an `ICommand` property of a similar name.

WHAT IT CANNOT DO
-----------------
Textual matching, not real overload resolution — it does not check argument
counts or types (that gap is exactly what `check-ui-compiles.py` is for, on
the files it can see). It also only covers the ViewModels already in
TableTop.Presentation; a screen using a head-local ViewModel is out of scope
by design.

USAGE
-----
    python3 scripts/check-mvvm-method-parity.py
"""

import re
import sys
from pathlib import Path

SHARED_VM_DIR = "src/TableTop.Presentation/ViewModels"

# (label, directory, glob). Both heads consume the same shared ViewModels and
# both call plain methods on them; only the accessor name differs.
HEADS = (
    ("MAUI", "ui/TableTop.Maui/Pages", "*.xaml.cs"),
    ("Android (native)", "ui/TableTop.Android/Screens", "*.cs"),
)

# `_vm` in MAUI code-behind, `Vm` on Android's GameScreenBase<T>. The leading
# word boundary keeps `SomeOtherVm.Foo()` from matching on its `Vm.` tail.
VM_CALL = re.compile(r"\b(?:_vm|Vm)\.(\w+)\s*\(")

# Members that are never the ViewModel's own API.
IGNORED = {"GetAwaiter", "ToString", "Equals"}

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


def vm_type_for(text: str, known) -> str | None:
    """
    The shared ViewModel a file is built around.

    Picks the one whose name appears EARLIEST in the file, tie-breaking on the
    longer name. This used to iterate a `set` and return whichever matched
    first — and Python randomises string hashing per process, so for a file
    mentioning two shared ViewModels the answer genuinely varied between runs.
    A gate that can pick a different ViewModel on a re-run is worse than no
    gate: it fails intermittently and teaches people to re-run it.
    """
    best = None
    for cls in sorted(known):
        m = re.search(rf"\b{re.escape(cls)}\b", text)
        if m is None:
            continue
        candidate = (m.start(), -len(cls), cls)
        if best is None or candidate < best:
            best = candidate
    return best[2] if best else None


def scan(repo: Path, directory: str, glob: str, known: dict[str, set[str]]):
    """Returns (problems, files_bound), or (None, 0) when the head is absent."""
    head_dir = repo / directory
    if not head_dir.exists():
        return None, 0

    problems: list[str] = []
    checked = 0

    for path in sorted(head_dir.glob(glob)):
        if "bin" in path.parts or "obj" in path.parts:
            continue

        text = path.read_text(encoding="utf-8", errors="ignore")
        cls = vm_type_for(text, known)
        if cls is None:
            continue
        checked += 1

        for m in VM_CALL.finditer(text):
            name = m.group(1)
            if name in IGNORED:
                continue
            if name not in known[cls]:
                line = text[: m.start()].count("\n") + 1
                accessor = m.group(0).rsplit(".", 1)[0]
                problems.append(
                    f"{path.relative_to(repo).as_posix()}:{line}  "
                    f"calls {accessor}.{name}() but {cls} has no public method '{name}'")

    return problems, checked


def main() -> int:
    repo = Path(__file__).resolve().parent.parent
    known = shared_vm_members(repo)

    all_problems: list[str] = []
    summaries: list[str] = []
    scanned_any = False

    for label, directory, glob in HEADS:
        problems, checked = scan(repo, directory, glob, known)
        if problems is None:
            summaries.append(f"{label}: not present - skipped")
            continue

        scanned_any = True
        all_problems.extend(problems)
        summaries.append(f"{label}: {checked} file(s) bound to a shared ViewModel")

    if not scanned_any:
        print("No head found - nothing to check.")
        return 0

    for p in all_problems:
        print(p)

    for s in summaries:
        print(f"  {s}")

    if all_problems:
        print(f"\n{len(all_problems)} missing method(s)")
        return 1

    print("\nall method calls resolve")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
