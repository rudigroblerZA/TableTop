#!/usr/bin/env python3
"""
Check MAUI XAML for properties that don't exist on the control they're set on.

WHY THIS EXISTS
---------------
XML well-formedness says nothing about whether a property is real. A file can
be perfectly valid XML and still contain

    <Entry Padding="12" CornerRadius="8" />

which MAUI rejects, because Entry supports neither property. If the XAML isn't
compiled, that surfaces only when the page is NAVIGATED TO — a crash in the
user's hands reading "Position 43:17. Cannot assign property Padding", rather
than a build error on the developer's machine.

The project now sets [assembly: XamlCompilation(XamlCompilationOptions.Compile)]
so the real build catches this. This script exists for environments that can't
run the MAUI build at all (no workload, no Android SDK), where it's the only
way to catch the mistake before it ships.

USAGE
-----
    python3 scripts/check-maui-xaml.py            # checks ui/TableTop.Maui
    python3 scripts/check-maui-xaml.py <dir>

Exits non-zero if anything is found, so it can gate CI.
"""

import re
import sys
from pathlib import Path

# Properties these controls do NOT have. Deliberately conservative — it only
# lists combinations that are certainly wrong, so a hit is always a real bug
# rather than something to argue about.
#
# The recurring trap is layout/appearance properties on input controls: in MAUI
# Padding, CornerRadius and the Border* family live on layouts, Border, Frame
# and Button — not on Entry, Editor, Picker, CheckBox, Switch or Slider. The
# fix is to wrap the control in a Border that carries them.
INVALID_PROPERTIES = {
    "Entry":    {"Padding", "CornerRadius", "BorderColor", "BorderWidth", "HasShadow", "Orientation"},
    "Editor":   {"CornerRadius", "BorderColor", "BorderWidth", "HasShadow", "Orientation"},
    "Picker":   {"Padding", "CornerRadius", "BorderColor", "BorderWidth", "HasShadow"},
    "DatePicker": {"Padding", "CornerRadius", "BorderColor", "BorderWidth", "HasShadow"},
    "TimePicker": {"Padding", "CornerRadius", "BorderColor", "BorderWidth", "HasShadow"},
    "Label":    {"CornerRadius", "BorderColor", "BorderWidth", "HasShadow", "Spacing"},
    "CheckBox": {"Padding", "CornerRadius", "BorderColor", "BorderWidth", "Text"},
    "Switch":   {"Padding", "CornerRadius", "BorderColor", "BorderWidth", "Text"},
    "Slider":   {"Padding", "CornerRadius", "BorderColor", "BorderWidth", "Text"},
    "Image":    {"Padding", "BorderColor", "BorderWidth", "Text"},
    "ActivityIndicator": {"Padding", "CornerRadius", "BorderColor", "Text"},
    # Frame is legacy but still present; it has no Stroke (that's Border).
    "Frame":    {"Stroke", "StrokeThickness", "StrokeShape"},
    # Border is the modern replacement; it has no BorderColor/CornerRadius
    # directly — those are Stroke and StrokeShape.
    "Border":   {"BorderColor", "BorderWidth", "CornerRadius", "HasShadow"},
}

ELEMENT = re.compile(r'<(\w+)\b((?:[^<>"]|"[^"]*")*?)/?>', re.S)


def check(path: Path):
    """Yield (file, line, tag, property) for every invalid combination."""
    text = path.read_text(encoding="utf-8")
    for match in ELEMENT.finditer(text):
        tag, attrs = match.group(1), match.group(2)
        banned = INVALID_PROPERTIES.get(tag)
        if not banned:
            continue
        element_line = text[: match.start()].count("\n") + 1
        for prop in sorted(banned):
            found = re.search(rf"\b{prop}\s*=", attrs)
            if found:
                line = element_line + attrs[: found.start()].count("\n")
                yield path.name, line, tag, prop


def main() -> int:
    root = Path(sys.argv[1] if len(sys.argv) > 1 else "ui/TableTop.Maui")
    if not root.exists():
        print(f"no such directory: {root}")
        return 2

    # MAUI ONLY. These rules describe MAUI's control surface, which differs from
    # WinUI's — WinUI genuinely does have Border.CornerRadius, for instance, so
    # running this against it reports dozens of false positives. Refuse rather
    # than mislead. (check-xaml-bindings.py is the framework-agnostic check and
    # covers every head.)
    if any(part in {"TableTop.WinUI"} for part in root.parts):
        print(f"{root} is not a MAUI project — these rules are MAUI-specific "
              f"(WinUI has a different control surface). Nothing checked.")
        return 2

    # Skip build output, and skip anything that isn't a regular file.
    #
    # Both halves are load-bearing on a developer machine and neither is on CI,
    # which is why this went unnoticed: a fresh checkout has no bin/ or obj/ at
    # all. Locally, `bin/` holds a DIRECTORY named `Microsoft.UI.Xaml` — and
    # rglob("*.xaml") matches it, because Windows path matching is
    # case-insensitive and rglob yields directories as readily as files. Opening
    # it raised PermissionError and took the whole gate down with a traceback.
    #
    # check-winui-xaml.py already filtered both bin and obj; this one filtered
    # only obj. is_file() is the belt-and-braces half: it makes the check immune
    # to any other directory that happens to end in .xaml, wherever it appears.
    files = sorted(
        f for f in root.rglob("*.xaml")
        if f.is_file() and not {"bin", "obj"} & set(f.parts)
    )
    problems = [p for f in files for p in check(f)]

    hints = {
        "Border": "use Stroke / StrokeThickness / StrokeShape instead",
        "Frame":  "use BorderColor / CornerRadius instead",
    }
    for name, line, tag, prop in problems:
        hint = hints.get(tag, "wrap it in a Border that carries that property instead")
        print(f"{name}:{line}  <{tag}> has no '{prop}' — {hint}")

    print(f"\nchecked {len(files)} XAML files: "
          f"{'no invalid control properties' if not problems else f'{len(problems)} problem(s)'}")
    return 1 if problems else 0


if __name__ == "__main__":
    raise SystemExit(main())
