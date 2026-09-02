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
│   ├── TableTop.Games/        ← Game mode definitions (103 modes, 3,811 cards)
│   │                             cards live in the in-code banks; see ARCHITECTURE.md
│   ├── TableTop.Hosting/      ← Controllers, events, hints, persistence
│   └── TableTop.Presentation/ ← ViewModels shared by WinUI + MAUI + Android (plain net10.0)
├── tests/
│   ├── TableTop.Tests/        ← 1074 tests — engine only, no UI required, any OS
│   └── TableTop.UiTests/      ← ViewModel tests     (Windows — references WinUI)
├── ui/
│   ├── TableTop.Console/      ← Terminal UI         (any OS, no extra installs)
│   ├── TableTop.Android/      ← Native .NET for Android (Mono.Android, not MAUI; `android` workload)
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

Seven static checks run in CI and need only **Python 3.12+** and the standard
library — no pip install. Worth running before a push, since they catch faults
that produce no exception and no build error:

```bash
python3 scripts/check-maui-xaml.py                 # MAUI properties that don't exist on the control
python3 scripts/check-winui-xaml.py                # WinUI properties that don't exist at all
python3 scripts/check-xaml-bindings.py             # bindings that resolve to nothing (render empty)
python3 scripts/check-shared-usings.py             # shared type used without importing its namespace
python3 scripts/check-mvvm-method-parity.py        # MAUI page calling a VM method that isn't there
python3 scripts/check-head-family-coverage.py      # a head's declared game support drifted from its test copy
python3 scripts/check-xaml-resources.py      # a {StaticResource} key that isn't defined anywhere
python3 scripts/check-async-void.py                # an async void handler with no try/catch (MAUI + native Android)
python3 scripts/check-settings-defaults.py         # an IAppSettings default that differs between heads
```

**On Windows, use `python` — not `python3`.** Install with
`winget install Python.Python.3.12`, which provides `python.exe` and the `py`
launcher but no `python3.exe`. There is a trap here worth knowing about: Windows
ships alias stubs at `WindowsApps\python.exe` and `python3.exe` that are not an
interpreter — they print "Python was not found" and exit non-zero. With a real
Python installed, `python` resolves to it but `python3` still hits the stub, so
a `python3` command fails in a way that reads like a broken script rather than
a naming mismatch. `py scripts/check-maui-xaml.py` works regardless.

Two further scripts need more than Python, so they aren't in the list above:

- `scripts/check-ui-compiles.py` needs the .NET SDK and both UI workloads; it
  compiles the heads rather than reading them.
- `scripts/check-coverage.py` needs a Cobertura report to read, so it runs in
  CI after the test step. It enforces per-assembly and total coverage floors —
  CI collected and printed coverage for a long time without anything failing
  when a number went down. Run it locally with:

  ```bash
  dotnet test tests/TableTop.Tests --collect:"XPlat Code Coverage" \
      --settings coverage.runsettings --results-directory /tmp/cov
  python3 scripts/check-coverage.py /tmp/cov
  ```

### Dev container

`.devcontainer/devcontainer.json` gives you .NET 10, Python 3.12, a JDK and
Trivy pre-installed — the same toolchain the `xaml`, `build-and-test`, `lint`
and `trivy` CI jobs use, so the Quick Start commands and the seven checks above
all just work, with no host setup and no Windows `python`/`python3` alias trap
(see above). Open the repo in VS Code with the Dev Containers extension, or
in a GitHub Codespace, and it builds itself on first open.

It does **not** cover `build-windows-heads` or `build-maui`: WinUI needs an
actual Windows SDK a Linux container can't provide, and the MAUI Android
workload needs the Android SDK/NDK, heavy enough that it isn't installed by
default. Those two stay native-runner-only, in the container and in CI alike.

---

## UI-specific setup

### Android (native .NET for Android)

`TableTop.Android` is a fourth head: native Mono.Android bindings (Activities +
view trees, no AXML-heavy layouts, no MAUI), consuming the same
`TableTop.Presentation` ViewModels as WinUI and MAUI. It uses its own
`ApplicationId` (`com.tabletop.game.droid`), so it installs alongside the MAUI
Android app rather than replacing it.

```bash
dotnet workload install android                 # one-time
dotnet build ui/TableTop.Android/TableTop.Android.csproj -f net10.0-android
dotnet build ui/TableTop.Android/TableTop.Android.csproj -c Release -f net10.0-android
```

The Release config sets `RunAOTCompilation=false` deliberately — the same
trimming-vs-AOT constraint documented in `TableTop.Maui.csproj` (backlog item
23): card types are reached reflectively by `System.Text.Json`, so the linker
must not trim (`PublishTrimmed=false`), and AOT requires trimming.

**Android TV.** Both the native head and the MAUI Android APK support Android
TV: each manifest declares `leanback` and `touchscreen` as not required and
carries a `LEANBACK_LAUNCHER` entry plus a home-row banner
(`drawable/banner.xml`), so the same APK installs on phones, tablets and TV
boxes and shows up on the TV launcher. The native head additionally applies a
TV-only overscan inset, a visible D-pad focus highlight on its buttons, and an
explicit initial focus per screen; MAUI's AppCompat views are D-pad focusable
without extra work. Neither has been run on a real TV device — see
`ARCHITECTURE.md`'s 1.35.0 note.

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
                            ←  Android
                            ←  WinUI   ←  UiTests
                            ←  Maui
                            ←  Tests
```

No UI code ever reaches Core, Games, or Hosting. `TableTop.Tests` is
deliberately engine-only and cross-platform; only `TableTop.UiTests` references
a UI head, which is what confines it to Windows. `TableTop.Android` is a native
.NET for Android head (Mono.Android bindings, not MAUI) that consumes the same
`TableTop.Presentation` ViewModels as WinUI and MAUI.

---

## Default players

Bob (Male, 44) and Alice (Female, 39) are seeded on first Console run. The MAUI
and WinUI apps use the player repository; add players through their setup screens.

---

## Versioning

`VersionPrefix` in `Directory.Build.props` is the single place to bump; every
project inherits it. Currently **1.39.4**. The public API of Core, Games and
Hosting is stable, so a breaking change to it needs a major bump;
`AssemblyVersion` tracks the major only (1.0.0.0 across the whole 1.x line), so
assemblies built against 1.0.0 keep binding without a rebuild.

`api/*.api.txt` records the public surface of Core, Games and Hosting.
`PublicApiSurfaceTests` fails when it moves, so an API change cannot land
silently; regenerate with `TABLETOP_UPDATE_API=1` and commit the diff.

---

## Branching

GitFlow. Two long-lived branches, and nothing commits directly to either:

| Branch | Holds |
|---|---|
| `main` | Released versions only. Every commit is tagged (`v1.22.0`). |
| `develop` | Integration. The branch you start work from and merge back into. |

Short-lived branches, named for what they carry:

```bash
git switch develop && git switch -c feature/table-suitability
git switch develop && git switch -c bug/millionaire-vm-flake
```

- `feature/*` — new capability, new modes, new decks. Merges to `develop`.
- `bug/*` — fixes. Merges to `develop`.

A release is `develop` → `main`, tagged with the version already set in
`Directory.Build.props`. CI (`.github/workflows/ci.yml`) builds and tests both
long-lived branches and every pull request into them, so a branch that is red
cannot be merged without it being visible.

Two code-quality checks run in CI alongside the build:

- **Trivy** (`trivy` job) — filesystem scan for known-vulnerable NuGet
  packages and committed secrets, reporting to the repo's Security tab.
  Report-only for now (`exit-code: "0"`) until the current findings are
  triaged; see the job's comment in `ci.yml` for turning it into a real gate.
- **SonarCloud** (the `Scanner Begin`/`Scanner End` steps inside
  `build-and-test` — not a job of its own) — static analysis, code smells,
  duplication, PR decoration. Needs a SonarCloud project and a `SONAR_TOKEN`
  repository secret that this repo cannot provision itself; both steps are
  `continue-on-error: true` until those exist — see their comments in
  `ci.yml` for the two setup steps.

Before opening a pull request, the three things that fail loudest if skipped:
the engine suite (`dotnet test TableTop.Engine.slnx`), the API snapshot if you
touched public surface (see **Versioning** above), and the mode/card counts in
this file if you added either — `DocumentationAccuracyTests` enforces the mode
count.

---

## Documentation

Two files, replacing what used to be a 34-file `docs/` folder that had
accumulated a lot of stale project history alongside what was actually
current:

- **[ARCHITECTURE.md](ARCHITECTURE.md)** — how the system is put together,
  right now: the four-assembly engine, the four heads, content and
  controller dispatch, the shared ViewModel layer, and an honest account of
  what "verified" means in an environment without the WinUI or MAUI SDK.
- **[BACKLOG.md](BACKLOG.md)** — genuinely open items only, plus the table
  of static gates and why each one exists.
