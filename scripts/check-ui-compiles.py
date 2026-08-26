#!/usr/bin/env python3
"""
Compile every C# file in both graphical heads against the engine, and report
only the errors that are real.

WHY THIS EXISTS
---------------
WinUI and MAUI cannot be built without Windows and the MAUI workload. That
leaves their C# unchecked, and three bugs reached a real build because of it:

  * a duplicate member in MAUI's GameplayViewModel (CS0102)
  * `ViewModelBase` ambiguous between the WinUI and shared namespaces (CS0104)
  * `ViewModelBase` / `SettingsViewModel` not found in ViewLocator.cs and
    MauiProgram.cs (CS0246) — files an earlier, narrower version of this check
    did not compile at all

The lesson from the third: scoping this to ViewModels/ felt sufficient and was
not. It now compiles EVERYTHING, because the bugs were in the files that
looked too boring to include.

KNOWN BLIND SPOT — READ THIS BEFORE TRUSTING A PASS
---------------------------------------------------
This CANNOT reliably detect a missing using for a first-party type, and a clean
run here does not mean there isn't one. Use `check-shared-usings.py` for that.

Two structural reasons:

  1. The compiler stops after 100 errors. Both heads together produce roughly
     that many unresolvable framework-type errors, so anything past the cap is
     never reported. Compiling per head helps and does not fix (2).

  2. A failing framework type masks a first-party one in the same expression.
     In ViewLocator.cs:

         [typeof(SettingsViewModel)] = () => new SettingsView(),

     `UIElement` cannot resolve without the SDK, so the initializer never binds
     and only that is reported — a missing `SettingsViewModel` is invisible.

This still earns its place for duplicate definitions and ambiguity, which are
declaration-level and reported regardless.

WHAT IT CAN AND CANNOT DO
-------------------------
Framework types (MAUI, WinUI, Android, UIKit) do not resolve here and never
will — those errors are filtered out. What survives the filter is real:

  ambiguous references     CS0104
  duplicate definitions    CS0101 CS0102 CS0111 CS0128
  missing OUR OWN types    CS0246 naming a TableTop type

Platforms/ is excluded: MAUI compiles one platform folder per target, so
compiling them together produces false duplicate AppDelegate/Program errors.

This is not a substitute for building the heads. It is the largest subset of a
compiler available without the SDKs.

USAGE
-----
    python3 scripts/check-ui-compiles.py
"""

import re
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

OWN_TYPES = (
    "ViewModelBase", "RelayCommand", "AsyncRelayCommand", "INavigator",
    "IAppSettings", "SavedPlayer", "SettingsViewModel", "Navigator",
)
REAL_CODES = ("CS0104", "CS0101", "CS0102", "CS0111", "CS0128")


def main() -> int:
    repo = Path(__file__).resolve().parent.parent
    if not (repo / "ui" / "TableTop.WinUI").exists():
        print("No UI heads found — nothing to check.")
        return 0

    work = Path(tempfile.mkdtemp(prefix="uicheck-"))
    try:
        (work / "u.csproj").write_text(f"""<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Library</OutputType><TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable><EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <ImplicitUsings>enable</ImplicitUsings><AssemblyName>u</AssemblyName>
    <NoWarn>CS1591;CS1574</NoWarn>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="{repo}/ui/TableTop.WinUI/**/*.cs" Exclude="{repo}/ui/TableTop.WinUI/obj/**;{repo}/ui/TableTop.WinUI/bin/**" />
    <Compile Include="{repo}/ui/TableTop.Maui/**/*.cs"  Exclude="{repo}/ui/TableTop.Maui/obj/**;{repo}/ui/TableTop.Maui/bin/**;{repo}/ui/TableTop.Maui/Platforms/**" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="{repo}/src/TableTop.Presentation/TableTop.Presentation.csproj" />
    <ProjectReference Include="{repo}/src/TableTop.Games/TableTop.Games.csproj" />
  </ItemGroup>
</Project>
""")
        out = subprocess.run(
            ["dotnet", "build", "-c", "Release"],
            cwd=work, capture_output=True, text=True, timeout=900).stdout

        # A check that cannot run must SAY so, not return success.
        #
        # The first version of this script silently "passed" while compiling
        # nothing: restore failed (no NuGet access), so the compiler never ran
        # and there were no diagnostics to find. Zero errors found is only
        # meaningful once the compiler has actually looked at the files.
        if "error NU" in out or "Failed to restore" in out:
            print("  CANNOT RUN: package restore failed — no compiler output to check.")
            print("  This needs NuGet access. Nothing was verified.")
            for line in out.splitlines():
                if "error NU" in line:
                    print(f"      {line.split('[')[0].strip()}")
                    break
            return 2

        if "error CS" not in out and "Build succeeded" not in out:
            print("  CANNOT RUN: build produced neither compiler errors nor success.")
            print("  Nothing was verified.")
            return 2

        problems = []
        for line in out.splitlines():
            if any(c in line for c in REAL_CODES):
                problems.append(line.split("[")[0].strip())
            elif "CS0246" in line:
                m = re.search(r"name '(\w+)'", line)
                if m and m.group(1) in OWN_TYPES:
                    problems.append(line.split("[")[0].strip())

        for p in sorted(set(problems)):
            print(f"  {p}")
        n = len(set(problems))
        print(f"\ncompiled both heads: {'no real errors' if n == 0 else f'{n} problem(s)'}")
        return 1 if n else 0
    finally:
        shutil.rmtree(work, ignore_errors=True)


if __name__ == "__main__":
    raise SystemExit(main())
