# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A fully UI-agnostic card game engine for couples and party games (101 modes,
3,721 cards as of the last README update — `DocumentationAccuracyTests`
keeps that figure honest, so trust README.md's count over any other doc's if
they ever disagree). All content is compiled in: no content files, no
runtime deck loading. .NET 10 / C# 14. Four engine assemblies plus four UI
heads (Console, WinUI, MAUI, and native Android) sharing one ViewModel layer.
The two Android-producing heads (native `TableTop.Android` and MAUI's Android
APK) both support Android TV — leanback launcher entry, TV banner, and (native
head only) D-pad focus handling.

For anything beyond this file, read in this order:
- **[README.md](README.md)** — setup, workload installs, versioning/branching rules.
- **[ARCHITECTURE.md](ARCHITECTURE.md)** — how the system is put together, plus a
  version-by-version log of every non-trivial change and why it was sized the
  way it was. Read before making an architectural change; it usually records
  why the current shape was chosen.
- **[BACKLOG.md](BACKLOG.md)** — open items and the full table of static
  gates with the specific bug each one was written after. Check here before
  reporting something as a new finding — it may already be tracked, closed,
  or deliberately deferred with a reason.

## Environment reality check

**This sandbox has no `dotnet` and no NuGet access.** Confirm with `which
dotnet` before assuming otherwise — it varies by environment. When it's
missing: don't claim a build or test run succeeded. Verify changes by reading
the diff carefully, checking brace/paren balance, and grepping for every call
site of anything you changed (signature, name, positional-vs-named args).
This is the same discipline every entry in `BACKLOG.md` was written under —
read a few of its "Not verified by a local build" notes for the pattern.

When a real .NET SDK **is** available, prefer it over static reasoning —
`ARCHITECTURE.md`'s "Verification, honestly" section explains why: a real
test run has found bugs every static check here missed.

## Commands

```bash
# Build/test the engine (any OS, no UI workload needed)
dotnet build TableTop.Engine.slnx
dotnet test  TableTop.Engine.slnx

# Single test class or method
dotnet test tests/TableTop.Tests --filter FullyQualifiedName~ControllerFamilyTests
dotnet test tests/TableTop.Tests --filter FullyQualifiedName~ClaimedGameViewModelTests.ReachingTheWinningCount_EndsTheGame

# Run the Console head
dotnet run --project ui/TableTop.Console

# Regenerate a public-API snapshot after an intentional signature change to
# Core/Games/Hosting (see "Public API discipline" below) — read the diff
# before committing it, then:
TABLETOP_UPDATE_API=1 dotnet test tests/TableTop.Tests --filter PublicApiSurfaceTests
```

Static gates (Python 3.12+ stdlib only, no pip install; use `py` instead of
`python3` on Windows — see README for why):

```bash
python3 scripts/check-maui-xaml.py             # MAUI properties that don't exist on the control
python3 scripts/check-winui-xaml.py            # WinUI properties that don't exist at all
python3 scripts/check-xaml-bindings.py         # bindings resolving to nothing (silently empty UI)
python3 scripts/check-shared-usings.py         # shared type used without importing its namespace
python3 scripts/check-mvvm-method-parity.py    # MAUI page calling a method its shared VM doesn't expose
python3 scripts/check-head-family-coverage.py  # a head's declared game support drifted from its test copy
python3 scripts/check-async-void.py             # an async void handler with no try/catch (MAUI + native Android)
python3 scripts/check-ui-compiles.py           # needs the .NET SDK + both UI workloads
```

WinUI and MAUI need their own SDK/workload to build — see README.md's
"UI-specific setup" for exact `dotnet workload install` and `-p:Platform`/`-f`
invocations; don't guess at these, they're finicky (WinUI rejects AnyCPU,
MAUI needs a target framework moniker per platform).

## Architecture — the parts that span files

**Dependency direction is one-way, enforced by actual `ProjectReference`s, not
just convention:** `Core ← Games ← Hosting`, and every other project builds
on that. `Console` references `Hosting`/`Games` directly and skips
`Presentation` entirely — it renders controllers straight to the terminal
with no ViewModel layer at all. `WinUI` and `MAUI` both reference
`Core`/`Games`/`Hosting`/`Presentation`. `TableTop.Tests` references all four
engine assemblies and nothing UI-side, which is what keeps it cross-platform;
`TableTop.UiTests` is the only project that references a UI head (WinUI),
which is why it's Windows-only. No UI code — not even `Presentation` — is
ever reachable from `Core`, `Games`, or `Hosting`.

**`IControllerFactory` is the sole controller-creation boundary.** A mode
implements one or more capability interfaces (`IGameModeDefinition`,
`IQuestionBankProvider`, `IMonogamyDeckProvider`, `IDailyDeckProvider`,
`IClaimedDeckProvider`, `IHerdDeckProvider`); `ControllerFactory.CreateAsync`
dispatches on those to build the right controller
(`CardTurnController`/`MillionaireController`/`MonogamyController`/
`DayOneController`/`ClaimedController`/`HerdController`). Every UI head must
go through it — constructing a controller with `new` anywhere outside
`ControllerFactory` was a recurring bug class here (see BACKLOG item 29):
it silently drops whatever persistence override, diagnostics sink, or DI
registration a host configured. If you add a mode that needs a new kind of
controller, that's a new capability interface and a new `ControllerFactory`
dispatch arm — not a UI-side workaround.

**The same capability-interface set gets tested in more than one place, and
history shows they drift.** `ControllerFactory.CreateAsync`,
`ControllerFamilies.TryFor` (which of six `ControllerFamily` values a mode
produces — drives which screen a head opens) and `ModeManifestExtensions`
(per-mode card-count summaries) all switch on the same interfaces.
`ControllerFamilies.TryFor` is the intended single source of truth now —
`ModeManifest` derives from it instead of re-testing the interfaces — but
`ControllerFactory` still repeats the chain, so adding a capability interface
means touching both, in the same order, or the two can (and have) disagreed
silently.

**Each head declares the `ControllerFamily` values it can render**
(`SupportedFamilies`), and `HeadFamilyCoverageTests` / `check-head-family-coverage.py`
check that declaration against the live registry — the enforcement is against
each head's own stated claim, not a hardcoded expectation, because a head is
allowed to support fewer families than the catalogue has. None currently
does: all four heads declare all six families. The mechanism still matters —
it is what makes a future gap a failing test rather than a mode that
silently does nothing.

**Shared ViewModels live in `TableTop.Presentation`**, plain `net10.0` with
no platform SDK dependency — that's what makes them unit-testable without
WinUI or MAUI installed. WinUI swaps ViewModels via `Navigator`/`ViewLocator`
(no page stack); MAUI uses a page stack and needs one thin per-screen adapter
only where platform `Color`/fonts/live-updating settings are unavoidable
(`GameplayViewModel` wraps `CardTurnGameViewModel` for exactly this). A MAUI
page whose ViewModel needs an async build step implements
`IAsyncInitializablePage` (constructor stores args only; `InitializeAsync()`
does the real work) because MAUI never awaits page construction — see any of
`GameplayPage`/`DayOneGamePage`/`MillionaireGamePage`/`MonogamyGamePage`/
`ClaimedGamePage`/`HerdGamePage` for the pattern before adding a new one.

**Public API discipline:** `api/TableTop.{Core,Games,Hosting}.api.txt` are
committed reflection-based snapshots of each assembly's public surface.
`PublicApiSurfaceTests` fails the moment that surface moves — read the diff
before regenerating, since a regenerated-without-reading snapshot defeats the
whole point. `Directory.Build.props` has the full MAJOR/MINOR/PATCH decision
tree in its comments, including the narrow carve-out for why a removal here
is usually MINOR rather than MAJOR (nothing in this repo is published as a
NuGet package — every consumer is an in-tree `ProjectReference`).

**Content is entirely compiled-in.** There is no deck file format, no
runtime mode loading, and no user-supplied content path (all removed by
1.21.0) — a new mode means a new C# card bank (ideally via `CardDeckBuilder`)
and a rebuild, full stop.
