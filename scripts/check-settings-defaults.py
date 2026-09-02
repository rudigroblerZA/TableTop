#!/usr/bin/env python3
"""
Check that the three IAppSettings implementations ship the same defaults.

WHY THIS EXISTS
---------------
MAUI, WinUI and the native Android head each implement IAppSettings over a
different store — MAUI over `Preferences`, Android over `ISharedPreferences`,
WinUI over its own settings.json — and each therefore spells its default value
in a different place:

    MAUI     get => Preferences.Get(KeyAutoNextPlayer, false);
    Android  get => _prefs.GetBoolean(KeyAutoNextPlayer, false);
    WinUI    public bool AutoNextPlayer { get; set; } = true;   <-- in SettingsData

Nothing tied those three numbers together. IAppSettings' own remarks record
that the interface was "not designed so much as discovered": the heads had
converged on the same properties independently, and the shared SettingsViewModel
now drives all three. But an interface constrains the *shape*, not the value a
head hands back when the user has never touched the setting — so first-run
behaviour was free to differ per head with nothing to catch it.

It did. `AutoNextPlayer` was ported into WinUI precisely because "the same
product behaved differently depending on which head you opened", and the port
defaulted it to `true` while MAUI and Android both said `false`. A fresh install
auto-advanced to the next player on WinUI and waited on the other two. Every
other one of the shared defaults agreed across all three heads; that one did
not, and no test could see it — TableTop.Tests cannot reference a UI head, and
the test doubles in PresentationTestDoubles.cs pick their own values, so they
assert nothing about what ships.

WHAT IT CHECKS
--------------
Parses each head's default for every IAppSettings property it can find, and
diffs the three as a table. Any property whose heads disagree is reported with
the value each head gives.

WHAT IT CANNOT DO
-----------------
Textual matching, like every other check in scripts/. It reads the literal in
the source; it does not run the heads, and it cannot tell whether a divergence
is deliberate. A deliberate one belongs in ALLOWED_DIVERGENCES below, with the
reason — which is the point: the difference becomes a decision someone wrote
down rather than an accident nobody saw.

USAGE
-----
    python3 scripts/check-settings-defaults.py
"""

import re
import sys
from pathlib import Path

INTERFACE = "src/TableTop.Presentation/Infrastructure/IAppSettings.cs"

# Properties whose defaults are genuinely not comparable across heads. Add an
# entry only with a reason; an unexplained difference is what this gate is for.
ALLOWED_DIVERGENCES = {
    # WinUI stores the roster as real JSON objects in settings.json (a List<T>,
    # defaulting to empty); MAUI and Android store it as a JSON *string* under a
    # single Preferences key, so their "default" is the empty string. Same empty
    # roster, two representations — comparing the literals is meaningless.
    "RecentPlayers": "different representations: List<SavedPlayer> vs a JSON string",
}


def normalise(value: str) -> str:
    """Trims a C# literal to a comparable form."""
    return value.strip().rstrip(";").strip()


def maui_defaults(text: str) -> dict[str, str]:
    """MAUI: `public bool Foo { get => Preferences.Get(KeyFoo, false); ... }`"""
    out = {}
    for name, default in re.findall(
            r"public\s+[\w<>\[\],\.\?]+\s+(\w+)\s*\{\s*get\s*=>\s*Preferences\.Get\("
            r"\s*\w+\s*,\s*([^)]*?)\s*\)", text):
        out[name] = normalise(default)
    return out


def android_defaults(text: str) -> dict[str, str]:
    """Android: `public bool Foo { get => _prefs.GetBoolean(KeyFoo, false); ... }`"""
    out = {}
    for name, default in re.findall(
            r"public\s+[\w<>\[\],\.\?]+\s+(\w+)\s*\{\s*get\s*=>\s*_prefs\.Get\w+\("
            r"\s*\w+\s*,\s*([^)]*?)\s*\)", text):
        out[name] = normalise(default)
    return out


def winui_defaults(text: str) -> dict[str, str]:
    """
    WinUI: the property forwards to `_data.Foo`, and the default lives on the
    private SettingsData class as a property initialiser.
    """
    block = re.search(r"private sealed class SettingsData\s*\{(.*?)\n    \}", text, re.S)
    if block is None:
        return {}
    out = {}
    for name, default in re.findall(
            r"public\s+[\w<>\[\],\.\?]+\s+(\w+)\s*\{\s*get;\s*set;\s*\}\s*=\s*([^;]+);",
            block.group(1)):
        out[name] = normalise(default)
    return out


HEADS = [
    ("MAUI",    "ui/TableTop.Maui/Services/AppSettings.cs",                 maui_defaults),
    ("Android", "ui/TableTop.Android/Infrastructure/AndroidAppSettings.cs", android_defaults),
    ("WinUI",   "ui/TableTop.WinUI/Infrastructure/WinUIAppSettings.cs",     winui_defaults),
]


def interface_properties(repo: Path) -> set[str]:
    path = repo / INTERFACE
    if not path.exists():
        return set()
    text = path.read_text(encoding="utf-8", errors="ignore")
    # Interface members: `bool AutoNextPlayer { get; set; }`
    return set(re.findall(r"^\s*[\w<>\[\],\.\?]+\s+(\w+)\s*\{\s*get;\s*set;\s*\}", text, re.M))


def main() -> int:
    repo = Path(__file__).resolve().parent.parent
    problems: list[str] = []

    parsed: dict[str, dict[str, str]] = {}
    for label, rel, parser in HEADS:
        path = repo / rel
        if not path.exists():
            problems.append(f"{rel} not found — cannot verify {label}'s defaults")
            continue
        found = parser(path.read_text(encoding="utf-8", errors="ignore"))
        if not found:
            problems.append(f"{rel}: could not parse any settings defaults for {label}")
            continue
        parsed[label] = found

    if len(parsed) < 2:
        for p in problems:
            print(p)
        print("\nfewer than two heads parsed — nothing to compare")
        return 1

    props = interface_properties(repo)
    if not props:
        # Fall back to whatever the heads agree exists, so a moved interface
        # file degrades to a weaker check rather than a silent pass.
        props = set().union(*(set(d) for d in parsed.values()))

    compared = 0
    for prop in sorted(props):
        if prop in ALLOWED_DIVERGENCES:
            continue
        values = {label: d[prop] for label, d in parsed.items() if prop in d}
        if len(values) < 2:
            continue
        compared += 1
        if len(set(values.values())) > 1:
            spelled = ", ".join(f"{label}={value}" for label, value in sorted(values.items()))
            problems.append(
                f"{prop}: heads disagree on the default — {spelled}. A fresh "
                f"install behaves differently depending on which head you open; "
                f"make them agree, or record the reason in ALLOWED_DIVERGENCES.")

    for p in problems:
        print(p)

    print(f"\ncompared {compared} setting default(s) across {len(parsed)} head(s): "
          + ("all agree" if not problems else f"{len(problems)} problem(s)"))
    return 1 if problems else 0


if __name__ == "__main__":
    raise SystemExit(main())
