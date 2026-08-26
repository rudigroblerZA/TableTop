#!/usr/bin/env python3
"""
Check WinUI 3 XAML for properties that don't exist in WinUI.

WHY THIS EXISTS
---------------
`check-maui-xaml.py` covers MAUI and deliberately refuses to run against WinUI,
because the two control surfaces differ. That left WinUI's XAML with no property
check at all — and it cost us: a pip strip shipped with

    <TextBlock CharacterSpacing="..." LetterSpacing="60" />

`LetterSpacing` is a CSS and Avalonia name. WinUI calls it `CharacterSpacing`
(units of 1/1000 em). The file was perfectly well-formed XML, every binding
resolved, every StaticResource resolved — and none of those checks can tell you
a property does not exist.

WHAT IT CATCHES
---------------
Two failure modes, both from writing one framework's habits in another's file:

  1. Properties that exist in no XAML framework here, or that belong to CSS,
     Avalonia or WPF and have a different name in WinUI.
  2. MAUI property names appearing in WinUI files. The two heads sit side by
     side in this repository and share a design language, so copying a block
     across is the obvious thing to do — and the names quietly differ.

It is deliberately conservative: it only lists names that are certainly wrong in
WinUI, so a hit is always a real bug rather than something to argue about. A
noisy gate gets switched off.

USAGE
-----
    python3 scripts/check-winui-xaml.py            # checks ui/TableTop.WinUI
    python3 scripts/check-winui-xaml.py <dir>

Exits non-zero if anything is found, so it can gate CI.
"""

import re
import sys
from pathlib import Path

# name -> what to use instead in WinUI
WRONG_IN_WINUI = {
    # CSS / Avalonia habits
    "LetterSpacing":     "CharacterSpacing (1/1000 em)",
    "FontWeightStyle":   "FontWeight",
    "TextAlignmentMode": "TextAlignment",

    # MAUI names. The two heads share a palette and a layout language, so blocks
    # get copied between them and these are what differ.
    "HorizontalOptions": "HorizontalAlignment",
    "VerticalOptions":   "VerticalAlignment",
    "WidthRequest":      "Width",
    "HeightRequest":     "Height",
    "MinimumWidthRequest":  "MinWidth",
    "MinimumHeightRequest": "MinHeight",
    "TextColor":         "Foreground",
    "BackgroundColor":   "Background",
    "FontAttributes":    "FontWeight / FontStyle",
    "IsVisible":         "Visibility (with a BoolToVisibility converter)",
    "LineBreakMode":     "TextWrapping / TextTrimming",
    "StrokeShape":       "CornerRadius on Border",
    "HasShadow":         "Shadow / ThemeShadow",
    "Clicked":           "Click",
    "Detail":            "not a WinUI concept",

    # WPF names WinUI does not have
    "SnapsToDevicePixels":  "UseLayoutRounding",
    "TextOptions":          "no equivalent — remove",
    "ToolTip":              "ToolTipService.ToolTip",
    "ContextMenu":          "ContextFlyout",
    "Visibility.Collapsed": "Visibility=\"Collapsed\"",
}

# Attribute occurrences, ignoring anything inside a comment.
ATTR = re.compile(r"\b([A-Za-z][\w.]*)\s*=\s*\"")
COMMENT = re.compile(r"<!--.*?-->", re.S)


def main() -> int:
    repo = Path(__file__).resolve().parent.parent
    target = Path(sys.argv[1]) if len(sys.argv) > 1 else repo / "ui" / "TableTop.WinUI"

    if not target.exists():
        print(f"no such directory: {target}")
        return 2

    # WinUI ONLY. MAUI's surface differs — IsVisible and TextColor are correct
    # there — so pointing this at MAUI would report every file as broken.
    if any(part == "TableTop.Maui" for part in target.parts):
        print(f"{target} is a MAUI project — use check-maui-xaml.py. Nothing checked.")
        return 2

    problems = []
    files = [f for f in sorted(target.rglob("*.xaml")) if "obj" not in f.parts and "bin" not in f.parts]

    for path in files:
        text = COMMENT.sub("", path.read_text(encoding="utf-8", errors="ignore"))
        for m in ATTR.finditer(text):
            name = m.group(1)
            if name in WRONG_IN_WINUI:
                line = text[: m.start()].count("\n") + 1
                problems.append(
                    (path.relative_to(repo).as_posix(), line, name, WRONG_IN_WINUI[name]))

    for rel, line, name, fix in problems:
        print(f"{rel}:{line}  '{name}' does not exist in WinUI — use {fix}")

    print(f"\nchecked {len(files)} XAML files: "
          + ("no invalid properties" if not problems else f"{len(problems)} problem(s)"))
    return 1 if problems else 0


if __name__ == "__main__":
    raise SystemExit(main())
