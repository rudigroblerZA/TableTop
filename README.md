# Card Game Engine

A fully UI-agnostic card game engine for couples and party games.

> **Requires the .NET 10 SDK** (LTS, shipped Nov 2025). A `global.json` pins the
> SDK band, so `dotnet` will tell you if you're on an older version. Install from
> <https://dotnet.microsoft.com/download/dotnet/10.0>. The language level is C# 14.

---

## Solution structure

```
TableTop/
├── src/
│   ├── TableTop.Core/         ← Abstractions, domain, deck/rule/scoring engine
│   ├── TableTop.Games/        ← Game mode definitions (98 modes, 3,625 cards)
│   │                             cards live in the in-code banks; see ARCHITECTURE.md
│   ├── TableTop.Hosting/      ← Controllers, events, hints, persistence
│   └── TableTop.Presentation/ ← ViewModels shared by WinUI + MAUI (plain net10.0)
├── tests/
│   ├── TableTop.Tests/        ← 776 tests — engine only, no UI required, any OS
│   └── TableTop.UiTests/      ← ViewModel tests     (Windows — references WinUI)
├── ui/
│   ├── TableTop.Console/      ← Terminal UI         (any OS, no extra installs)
│   ├── TableTop.WinUI/        ← WinUI 3 desktop     (Windows App SDK, x64/x86/ARM64)
│   └── TableTop.Maui/         ← Mobile/desktop      (requires MAUI workload)
├── TableTop.Engine.slnx       ← Engine + Tests + Console (recommended start)
└── TableTop.slnx              ← Full solution — all projects
```

---

## Quick start (any OS)

```bash
dotnet build TableTop.Engine.slnx
dotnet test  TableTop.Engine.slnx
dotnet run --project ui/TableTop.Console
```

Two static checks run in CI and need only Python — worth running before a push,
since they catch XAML faults that produce no exception and no build error:

```bash
python3 scripts/check-maui-xaml.py        # MAUI properties that don't exist on the control
python3 scripts/check-winui-xaml.py       # WinUI properties that don't exist at all
python3 scripts/check-xaml-bindings.py    # bindings that resolve to nothing (render empty)
```

---

## UI-specific setup

### MAUI (iOS / Android / macOS / Windows)

The `Microsoft.Maui.Sdk could not be found` error means the workload is not installed:

```bash
# Install once — choose the platform(s) you need
dotnet workload install maui               # all platforms
dotnet workload install maui-android       # Android only
dotnet workload install maui-ios           # iOS only
dotnet workload install maui-maccatalyst   # macOS only
dotnet workload install maui-windows       # Windows only

# Verify
dotnet workload list

# Then build (pick your target framework)
dotnet build ui/TableTop.Maui/TableTop.Maui.csproj -f net10.0-android
dotnet build ui/TableTop.Maui/TableTop.Maui.csproj -f net10.0-windows10.0.19041.0
```

### WinUI 3 (Windows only)

Declares `x86;x64;ARM64` and no AnyCPU. Name a platform explicitly — this is
what CI does:

```bash
dotnet build ui/TableTop.WinUI/TableTop.WinUI.csproj -c Release -p:Platform=x64
```

A build that names none falls back to `RuntimeIdentifier=win-x64` rather than
failing. `<Platforms>` declares which platforms are *valid*; it does not change
MSBuild's AnyCPU default, and the MSIX packaging targets reject AnyCPU even with
`WindowsPackageType=None`.

---

## Dependency graph

```
Core  ←  Games  ←  Hosting  ←  Console
                            ←  WinUI   ←  UiTests
                            ←  Maui
                            ←  Tests
```

No UI code ever reaches Core, Games, or Hosting. `TableTop.Tests` is
deliberately engine-only and cross-platform; only `TableTop.UiTests` references
a UI head, which is what confines it to Windows.

---

## Default players

Bob (Male, 44) and Alice (Female, 39) are seeded on first Console run. The MAUI
and WinUI apps use the player repository; add players through their setup screens.

---

## Versioning

`VersionPrefix` in `Directory.Build.props` is the single place to bump; every
project inherits it. Currently **1.22.0**. The public API of Core, Games and
Hosting is stable, so a breaking change to it needs a major bump;
`AssemblyVersion` tracks the major only (1.0.0.0 across the whole 1.x line), so
assemblies built against 1.0.0 keep binding without a rebuild.

`api/*.api.txt` records the public surface of Core, Games and Hosting.
`PublicApiSurfaceTests` fails when it moves, so an API change cannot land
silently; regenerate with `TABLETOP_UPDATE_API=1` and commit the diff.

---

## Documentation

Two files, replacing what used to be a 34-file `docs/` folder that had
accumulated a lot of stale project history alongside what was actually
current:

- **[ARCHITECTURE.md](ARCHITECTURE.md)** — how the system is put together,
  right now: the four-assembly engine, the three heads, content and
  controller dispatch, the shared ViewModel layer, and an honest account of
  what "verified" means in an environment without the WinUI or MAUI SDK.
- **[BACKLOG.md](BACKLOG.md)** — genuinely open items only, plus the table
  of static gates and why each one exists.
