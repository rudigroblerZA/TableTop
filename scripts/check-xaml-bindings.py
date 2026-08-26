#!/usr/bin/env python3
"""
Cross-check every XAML binding path against the properties that actually exist.

WHY THIS EXISTS
---------------
check-maui-xaml.py catches invalid *properties on controls*, and only for MAUI —
its rules describe MAUI's control surface, which is why it refuses to run against
WPF and WinUI. That leaves 34 of the project's 43 XAML files with no check at all.

This script covers the other failure, in every head, framework-agnostically:

    <TextBlock Text="{Binding TotalScore}" />

where `TotalScore` is missing, misspelled, or — the nastier case — declared
`static`. A {Binding} resolves against an *instance*, so a static property binds
to nothing and renders EMPTY. No exception, no warning, no build error. The
control just shows blank, and it looks like a data problem rather than a binding
problem, which is why it has cost this project real debugging time before.

WHAT IT REPORTS
---------------
  STATIC-ONLY  a bound name exists, but only as a static property  -> renders empty
  MISSING      a bound name matches no property anywhere           -> renders empty
  TEXT+HTML    one TextBlock binds both Text and HtmlTextBlock.Html -> one wins, unpredictably

Both are errors. Exits non-zero so it can gate CI.

WHAT IT DELIBERATELY IGNORES
----------------------------
Nested paths (`A.B.C`) are checked on their first segment only, indexers are
ignored, and anything resolvable on a type in src/ counts as found. The bar is
"a hit is always a real bug" — a noisy gate gets switched off, which is worse
than no gate.

USAGE
-----
    python3 scripts/check-xaml-bindings.py            # all heads under ui/
    python3 scripts/check-xaml-bindings.py <dir>      # one head
"""

import re
import sys
from pathlib import Path

# Names XAML resolves itself — never ViewModel properties.
XAML_INTRINSICS = {
    "DataContext", "RelativeSource", "TemplatedParent", "Self", "Source",
    "ElementName", "Path", "Mode", "Converter", "ConverterParameter",
    "StringFormat", "FallbackValue", "TargetNullValue", "UpdateSourceTrigger",
    "AncestorType", "AncestorLevel", "IsAsync", "Delay", "BindsDirectlyToSource",
    "XmlNamespaceManager", "PresentationTraceSources",
}

# Property declarations. Two groups matter: `static` (the silent-empty trap)
# and everything else.
PROP = re.compile(
    r"^\s*(?:public|internal|protected)\s+(?P<static>static\s+)?"
    r"(?:readonly\s+|virtual\s+|override\s+|sealed\s+|new\s+|required\s+|partial\s+)*"
    r"[\w<>\[\],\?\.\(\) ]+?\s+(?P<name>\w+)\s*(?:\{|=>)",
    re.M,
)

# CommunityToolkit.Mvvm source generators: the *generated* member is what XAML
# binds to, and it never appears in the source text.
OBSERVABLE = re.compile(
    r"\[ObservableProperty\][\s\S]{0,200}?\b(?:private|protected|internal)\s+"
    r"[\w<>\[\],\?\.]+\s+_?(?P<name>\w+)\s*[;=]"
)
RELAY = re.compile(
    r"\[RelayCommand[^\]]*\][\s\S]{0,300}?\b(?:private|public|protected|internal)\s+"
    r"(?:async\s+)?[\w<>\[\],\?\.]+\s+(?P<name>\w+)\s*\("
)

# Positional record parameters are properties too:  record Foo(int Bar, string Baz)
RECORD = re.compile(r"\brecord\s+(?:struct\s+|class\s+)?\w+\s*\((?P<params>[^)]*)\)", re.S)
RECORD_PARAM = re.compile(r"(?:^|,)\s*(?:\[[^\]]*\]\s*)?[\w<>\[\],\?\.]+\s+(?P<name>\w+)\s*(?:=|,|$)")

# {Binding Foo} and {x:Bind Foo}. The whole extension body is captured so a
# binding that redirects its source can be skipped.
#
# {TemplateBinding Foo} is NOT matched, and neither is any binding carrying
# RelativeSource / ElementName / Source: all of those resolve against something
# other than the DataContext — usually the templated control, whose Padding,
# BorderBrush and BorderThickness are real framework properties. Checking them
# against ViewModels reports every themed ControlTemplate in the project as
# broken, which is exactly the noise that gets a gate switched off.
BINDING = re.compile(
    r"\{(?:x:Bind|Binding)\s+(?:Path\s*=\s*)?(?P<path>[A-Za-z_][\w\.]*)(?P<rest>[^}]*)"
)

REDIRECTED = re.compile(r"\b(?:RelativeSource|ElementName|Source)\s*=")

# A <TextBlock> that sets both Text and HtmlTextBlock.Html. TextBlock.Text and
# TextBlock.Inlines are mutually exclusive in WPF — assigning Text discards the
# inlines the attached property just built, so on every update whichever binding
# applies second wins. It rendered correctly often enough to look fine and
# showed raw <b> tags the rest of the time.
TEXTBLOCK = re.compile(r"<TextBlock\b((?:[^<>\"]|\"[^\"]*\")*?)/?>", re.S)


def collect_properties(roots):
    """Return (instance_names, static_names) declared anywhere under `roots`."""
    instance, static = set(), set()
    for root in roots:
        for cs in root.rglob("*.cs"):
            if "obj" in cs.parts or "bin" in cs.parts:
                continue
            text = cs.read_text(encoding="utf-8", errors="ignore")
            for m in PROP.finditer(text):
                (static if m.group("static") else instance).add(m.group("name"))
            for m in OBSERVABLE.finditer(text):
                n = m.group("name")
                instance.add(n[0].upper() + n[1:])
            for m in RELAY.finditer(text):
                n = m.group("name")
                base = n[0].upper() + n[1:]
                if base.endswith("Async"):
                    base = base[: -len("Async")]
                instance.add(base + "Command")
            for m in RECORD.finditer(text):
                for p in RECORD_PARAM.finditer(m.group("params")):
                    instance.add(p.group("name"))
    return instance, static


def conflicting_text_bindings(path: Path):
    """Yield (line, snippet) for TextBlocks that set both Text and Html."""
    text = path.read_text(encoding="utf-8", errors="ignore")
    for m in TEXTBLOCK.finditer(text):
        attrs = m.group(1)
        if re.search(r"\bText\s*=", attrs) and re.search(r"HtmlTextBlock\.Html\s*=", attrs):
            yield text[: m.start()].count("\n") + 1


def bindings_in(path: Path):
    """Yield (line, first_path_segment, full_path) for each binding."""
    text = path.read_text(encoding="utf-8", errors="ignore")
    for m in BINDING.finditer(text):
        if REDIRECTED.search(m.group("rest")):
            continue
        full = m.group("path")
        head = full.split(".")[0].split("[")[0]
        yield text[: m.start()].count("\n") + 1, head, full


def main() -> int:
    repo = Path(__file__).resolve().parent.parent
    target = Path(sys.argv[1]) if len(sys.argv) > 1 else repo / "ui"
    if not target.exists():
        print(f"no such directory: {target}")
        return 2

    heads = [target] if (target / "obj").exists() or any(target.glob("*.csproj")) \
        else sorted(d for d in target.iterdir() if d.is_dir())

    # Engine types are bindable too (a DataTemplate over an engine model), so
    # their properties count as declared.
    engine = [repo / "src"]

    total_files = 0
    problems = []

    for head in heads:
        # Skip build output and non-files. The .cs walk at the top of
        # collect_properties() already excluded both bin and obj; this line
        # excluded only obj, so a local MAUI build — whose bin/ contains a
        # DIRECTORY named `Microsoft.UI.Xaml`, matched by this case-insensitive
        # glob — crashed the gate with a PermissionError traceback. CI never saw
        # it because a fresh checkout has no bin/.
        xamls = [
            f for f in sorted(head.rglob("*.xaml"))
            if f.is_file() and not {"bin", "obj"} & set(f.parts)
        ]
        if not xamls:
            continue
        instance, static = collect_properties([head] + engine)
        total_files += len(xamls)

        for xf in xamls:
            rel = xf.relative_to(repo).as_posix()

            for line in conflicting_text_bindings(xf):
                problems.append(
                    (rel, line, "TEXT+HTML", "<TextBlock>",
                     "sets both Text and HtmlTextBlock.Html. TextBlock.Text and TextBlock.Inlines "
                     "are mutually exclusive — assigning Text discards the inlines, so whichever "
                     "binding applies second wins. Bind Html alone."))

            for line, head_name, full in bindings_in(xf):
                if head_name in XAML_INTRINSICS or head_name in instance:
                    continue
                if head_name in static:
                    problems.append(
                        (rel, line, "STATIC-ONLY", full,
                         f"'{head_name}' is declared static; {{Binding}} resolves against "
                         f"an instance, so this renders EMPTY. Make it an instance property."))
                else:
                    problems.append(
                        (rel, line, "MISSING", full,
                         f"no property named '{head_name}' found on this head or in src/. "
                         f"Misspelled or removed — renders EMPTY."))

    for rel, line, kind, full, msg in problems:
        print(f"{rel}:{line}  [{kind}]  {{Binding {full}}}\n      {msg}")

    print(f"\nchecked {total_files} XAML files across {len(heads)} head(s): "
          + ("no unresolved bindings" if not problems else f"{len(problems)} problem(s)"))
    return 1 if problems else 0


if __name__ == "__main__":
    raise SystemExit(main())
