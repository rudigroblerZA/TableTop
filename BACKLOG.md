# BACKLOG

Open items only, periodized. Closed work lives in `ARCHITECTURE.md`'s
version-by-version log, not here — this file is what is *left*.

Recreated 2026-08-29 after commit `6861545` ("remove backlog") deleted it while
`CLAUDE.md`, `README.md` and `ARCHITECTURE.md` all still linked to it. Item X.5
tracks the resulting broken links.

## How to read this

Four horizons. The boundary is *when the cost of not doing it lands*, not how
hard the item is:

| Horizon | Means | Target |
|---|---|---|
| **Now** | A player hits it, or a gate that exists is lying. Ship next. | 1.35.1 / 1.36.0 |
| **Next** | Real risk with no live symptom yet. Structural, cheap while small. | 1.36–1.38 |
| **Later** | Worth doing; nothing degrades if it waits a quarter. | 1.4x |
| **Someday** | Genuinely open questions. Don't start without deciding the shape first. | unscheduled |

Every item carries **Evidence** (what was actually checked) and **Done when**.

## Verification standard for this pass

Unlike most historical entries, this review ran on a machine with a **real
toolchain**, so nothing below is static reasoning alone:

- `dotnet` 10.0.400 present; WinUI and MAUI workloads both installed.
- `dotnet build TableTop.Engine.slnx -c Release` — clean, **zero warnings**.
- `dotnet test TableTop.Engine.slnx -c Release` — **937/937 passed**.
- `dotnet test tests/TableTop.UiTests -c Release -p:Platform=x64` — **2/2 passed**.
- All eight `scripts/check-*.py` gates pass, including `check-ui-compiles.py`
  (which really compiled both graphical heads).
- Coverage collected for real (see L.1 — this closes a standing "doesn't exist
  here" note in `ARCHITECTURE.md`).

So: the tree is green. Every item below is something green does not catch.

---

## Now

> **All five Now items and X.1 were fixed on 2026-08-29.** They are kept below
> with their resolutions, because each one's *cause* is the useful record — see
> X.2, which is the root N.1, N.2 and X.1 all share and is still open.
>
> Suite went 937 → **961** passing; total coverage 91.7% → **92.0%** line,
> 70.5% → **71.5%** branch; the async-void gate now covers both
> Android-producing heads and was proved to fail before being trusted.
>
> Two things need a human: `v1.35.0` is tagged locally but **not pushed**
> (N.5), and none of this is committed — it wants a branch off `develop` per
> the GitFlow rules in README.

### N.1 — Resume is dead on WinUI and MAUI — **FIXED**

`SavedSessionLookup` takes an optional `IControllerFactory` and falls back to
`new ControllerFactory()` when none is passed
(`src/TableTop.Presentation/Infrastructure/SavedSessionLookup.cs:67`). That
fallback has `_persistence == null`, and `ControllerFactory.LoadSavedSessionAsync`
returns `null` outright when `_persistence` is null. So the lookup can only ever
answer "nothing to resume".

Only the native Android head passes a factory
(`ui/TableTop.Android/Screens/ModePickerScreen.cs:21`). The other two do not:

- `ui/TableTop.WinUI/ViewModels/PickerViewModels.cs:19` — `new SavedSessionLookup()`
- `ui/TableTop.Maui/ViewModels/GameSelectionViewModel.cs:12` — `new SavedSessionLookup()`

Both heads *do* configure real persistence (`%LOCALAPPDATA%\TableTop`,
`FileSystem.AppDataDirectory`) and their gameplay paths *do* resolve the
container's factory, so sessions **are** written. They just can never be read
back. `CanResume` is permanently false, so the Continue button never renders and
nobody sees an error.

The save/load asymmetry is what hides it: `CardTurnController.cs:175` falls back
to `new JsonSessionRepository()` when handed no repository, but
`LoadSavedSessionAsync` has no equivalent fallback. Writes always land somewhere;
reads give up.

This exact defect is described in `SavedSessionLookup`'s own XML docs as the
reason the parameter was added. The parameter landed; two of three call sites
were never updated.

**Evidence:** read end-to-end across all three heads. Not reproduced on device —
WinUI/MAUI runtime behaviour was not exercised, only the call graph.

**Resolved.** Both heads now pass the container's `IControllerFactory`:

- WinUI reaches it through `Navigator.Services`, its existing composition-root
  handle, so `IntroViewModel`'s only call site was unchanged.
- MAUI takes `IControllerFactory` as a **required** constructor parameter on
  `GameSelectionViewModel`. It is DI-resolved (`AddSingleton<GameSelectionViewModel>`),
  so this is also compile-enforced for any future call site.

**A second defect surfaced while fixing this.** WinUI's `ResumeCommand` is an
`AsyncRelayCommand` guarded by `() => CanResume`, and `AsyncRelayCommand`
documents that *"requery is explicit — no ambient CommandManager on any
target."* `LookForSavedSessionAsync` raised `PropertyChanged` for `CanResume`
(which drives the button's `Visibility`) but never `CanExecuteChanged`. So the
lookup fix alone would have produced a Continue button that appears and stays
greyed out. `IntroViewModel` now holds the command at its concrete type and
calls `RaiseCanExecuteChanged()` after the lookup resolves. MAUI is unaffected —
its resume button uses a plain `Clicked` handler, not an `ICommand`.

That second bug was only reachable *because* the first one was fixed, which is
the argument for X.2: two independent failures stacked behind one silent
fallback.

**Not verified on device.** Both heads build clean (WinUI Release/x64 with zero
warnings; MAUI `net10.0-windows10.0.19041.0`), and the behaviour is covered by
the tests added under N.2 — but neither app was launched, so the rendered button
was not observed.

### N.2 — Every `SavedSessionLookup` test pins the broken configuration — **FIXED**

`tests/TableTop.Tests/SavedSessionLookupTests.cs` has five tests. All five
construct `new SavedSessionLookup()` with no factory, and one is named
`RefreshAsync_WithNoPersistenceConfigured_LeavesNothingToResume`. The
injected-factory path — the entire reason the constructor parameter exists — has
**no test at all**, which is why N.1 survived the change that was supposed to fix
it.

Related: `SessionResumer` sits at **12.5% line coverage** despite being a pure
function with four explicit failure branches (null snapshot, no players, mode
gone, roster-vs-snapshot player merge) and being the resume path that
`Directory.Build.props` cites as the motivating bug-report example.

**Evidence:** grep of the test file; coverage run (see L.1).

**Resolved.** `SavedSessionLookupTests` gained six tests on the injected-factory
path — including one asserting the injected factory was *actually consulted*
(`LoadCallCount`), since an ignored factory is externally indistinguishable from
"nothing saved", which is exactly what hid N.1. New `SessionResumerTests` covers
all four refusal branches plus the roster-merge rules that are easy to get
backwards (live player wins on id match; scores deliberately not applied, because
the controller restores them from the same snapshot).

The suite's own docstring was also corrected: it claimed the found path *"genuinely
can't be"* exercised. That was true when written and stopped being true when the
constructor gained the parameter — the docstring outlived the limitation it
described and then justified the gap.

| | Before | After |
|---|---|---|
| `SessionResumer` | 12.5% (42/48 missed) | **100%** |
| `SavedSessionLookup` | — | **100%** |

### N.3 — Eight composition-root tests have never compiled — **FIXED**

`tests/TableTop.Tests/ArchetypeTests.cs:518-596` wraps 8 `[Fact]`s in
`#if HAS_MICROSOFT_DI`. That symbol is defined nowhere — not in the csproj, not
in `Directory.Build.props`. The comment above it says to add
`Microsoft.Extensions.DependencyInjection` to run them; nobody did.

Confirmed by the coverage run: `TableTop.Hosting.Extensions.ServiceCollectionExtensions`
is at **0.0% — 72/72 lines missed**. `AddTableTopHosting` is the composition root
all four heads boot through, and nothing executes it.

The package is already in `Directory.Packages.props` (version 10.0.0), so this is
one `PackageReference` plus deleting the `#if`.

**Evidence:** grep for the symbol across the repo (single occurrence, the `#if`
itself); coverage report.

**Resolved.** `Microsoft.Extensions.DependencyInjection` is now a real
`PackageReference` on `TableTop.Tests` and the `#if` is gone. All 8 tests run and
pass. `AddTableTopHosting` coverage: **0% → 44.4%**.

**Residual, deliberately not done here:** the four convenience overrides in the
same file — `UsePlayerRepository`, `UseHintEngine`, `UseEngineDiagnostics`,
`UseLoggerDiagnostics` — are the bulk of the remaining 40 uncovered lines. They
were never in scope of the 8 unblocked tests. Cheap to add; folded into L.1's
coverage-floor work rather than treated as its own item.

### N.4 — The async-void gate does not cover the Android head — **FIXED**

`scripts/check-maui-async-void.py` scans `ui/TableTop.Maui/**/*.xaml.cs` and
nothing else (`MAUI_ROOT`, line 57). The native Android head — the head that
actually runs on Android, where an escaping `async void` exception is the
process-terminating crash the script was written about — has an
unguarded-by-gate `async void` at
`ui/TableTop.Android/Screens/ModePickerScreen.cs:135`.

It is hand-guarded today. The script's own header explains why that is not
enough: *"which is why this is a script and not a review habit."* The head
shipped in 1.32.0; the gate was never widened.

**Evidence:** read the script's root constant; grepped all `async void` in the
Android head (one, correctly guarded — so this is a gate gap, not a live crash).

**Resolved.** Renamed `scripts/check-maui-async-void.py` →
`scripts/check-async-void.py` (via `git mv`, so history follows) and
restructured it around a `HEADS` table: MAUI's `*.xaml.cs` and the native
head's `*.cs`. `SAFE_DELEGATES` is now per-head rather than global —
`SafePopToRootAsync` is a MAUI type, and a same-named Android helper would not
be the same method, so a global list could have granted a false pass.

Current state: 16 MAUI handlers across 12 files, 1 Android handler across 20
files, all guarded.

**The gate was proved to fail.** A "all guarded" result from a check that has
never gone red is worth nothing — the same argument this script's own header
makes. Injecting a temporary unguarded `async void` into
`ui/TableTop.Android/Screens/SettingsScreen.cs` produced the correct
file:line diagnostic and exit code 1; removing it returned exit 0.

Updated with it: the CI step name and command, `CLAUDE.md`'s script list,
`ARCHITECTURE.md`'s 1.29.0 reference, and `README.md`'s list — which was
missing this gate entirely and said "six static checks" (now seven, plus
`check-ui-compiles.py` as the eighth). That last part overlaps X.5's table;
it was unavoidable while renaming the file, and X.5's remaining rows are
untouched.

### N.5 — 1.35.0 is on `main` untagged — **FIXED**

`Directory.Build.props` says `1.35.0`. `git describe --tags` says `v1.34.0`.
`main` carries the merged Android TV work. README's branching table states every
`main` commit is tagged.

**Evidence:** `git tag`, `git log v1.34.0..HEAD`.

**Resolved.** Annotated tag `v1.35.0` created at `0dbfc8d`. Tagging, not
reverting: `ARCHITECTURE.md` carries a full 1.35.0 changelog entry and the work
reached `main` through PR #38, so it was plainly meant to be released — the tag
is the only thing that was missing.

**The tag is local and has not been pushed.** Publishing it is an outward-facing
step and yours to take: `git push origin v1.35.0`. Undo with `git tag -d v1.35.0`
if you'd rather cut it from a release branch instead.

---

## Next

### X.1 — Five MAUI pages bypass the container's controller factory — **FIXED**

`MonogamyGamePage`, `ClaimedGamePage`, `HerdGamePage`, `MillionaireGamePage` and
`DayOneGamePage` each call `<Family>GameViewModel.CreateAsync(nav, mode, players)`
at line 32, passing no `IControllerFactory` — so each hits
`?? new ControllerFactory()`. Three of them carry an XML doc claiming they build
"through `IControllerFactory`". They do not build through *the app's*
`IControllerFactory`.

No live symptom: only `CardTurnController` consumes persistence, and none of
these five families do. MAUI's CardTurn path (`GameplayViewModel.CreateAsync`)
correctly resolves from `IPlatformApplication.Current.Services`, and WinUI and
Android route every family through the container. So this is MAUI-only and
currently inert — but it is the same bypass as N.1, one interface away from
mattering.

**Evidence:** grepped all five pages; compared against WinUI's
`GameViewModelFactory.CreateAsync` and Android's `GameScreenFactory.CreateAsync`,
both of which resolve correctly.

**Resolved.** All five now pass `controllerFactory:` explicitly. The lookup is
named once, in a new `ui/TableTop.Maui/Services/AppServices.cs`, rather than
repeating `IPlatformApplication.Current!.Services.GetRequiredService<...>()` five
times — the absence of anything obvious to copy is a fair part of why five pages
drifted. It throws with a clear message if touched before `CreateMauiApp` has
run, instead of a bare `NullReferenceException`.

Named argument, not positional: `MonogamyGameViewModel.CreateAsync` takes
`winningTokenCount` before `controllerFactory`, so a positional call would have
bound the factory to the wrong parameter — or, worse for the other four, silently
compiled against a different overload shape later.

`GameplayViewModel.CreateAsync` was left alone. It already resolves correctly and
also needs `IAppSettings`, so folding it into `AppServices` is tidying, not a fix
— and it is the one MAUI path that was never broken.

**Still inert, as diagnosed.** None of these five families consume persistence,
so no behaviour changed. This closes the structural gap; X.2 is what stops it
recurring.

### X.2 — Retire the `?? new ControllerFactory()` idiom — **FIXED**

The root cause behind N.1 and X.1. Seven places in `TableTop.Presentation` accept
`IControllerFactory? = null` and silently substitute a factory with no
persistence, no diagnostics sink and no host configuration:

`SavedSessionLookup.cs:67`, `CardTurnGameViewModel.cs:329`,
`ClaimedGameViewModel.cs:144`, `DayOneGameViewModel.cs:98`,
`HerdGameViewModel.cs:151`, `MillionaireGameViewModel.cs:124`,
`MonogamyGameViewModel.cs:198`.

`CLAUDE.md` already names this bug class (historical item 29): constructing
outside the configured factory "silently drops whatever persistence override,
diagnostics sink, or DI registration a host configured". The optional parameter
is that same bug with a nicer face — it turns a compile error into a silent
behaviour change, which is how N.1 shipped.

Making the parameter required is a `TableTop.Presentation` signature change, and
`api/*.api.txt` does not track Presentation — so no API-snapshot churn, and MINOR
per `Directory.Build.props`.

**Resolved.** Zero `?? new ControllerFactory()` left in `TableTop.Presentation`.
The parameter is required on all six `CreateAsync` methods and on
`SavedSessionLookup`'s constructor, each with `ArgumentNullException.ThrowIfNull`.

Two signatures needed reordering, since a required parameter cannot follow an
optional one: `controllerFactory` moved ahead of `resumeFrom` on
`CardTurnGameViewModel` and ahead of `winningTokenCount` on
`MonogamyGameViewModel`. The MAUI pages already used named arguments and were
unaffected; `GameplayViewModel`'s positional call was updated.

~30 call sites in the test suite now pass `TestFactory.PlainControllerFactory()`,
a named helper rather than a bare `new ControllerFactory()`. That is the point
rather than a workaround: tests genuinely want plain defaults, they just have to
say so. A new test asserts the constructor rejects null.

**A side effect worth recording.** Adding a `<param>` tag triggered CS1573 —
"other parameters do" — on all six methods, which would have failed the lint
gate X.4 had just extended to `TableTop.Presentation`. So every parameter on
those methods is now documented. Two fixes landing in the same change caught
each other, which is the argument for X.4 covering all four assemblies.

### X.3 — UI tests are still commented out in CI — **FIXED**

`.github/workflows/ci.yml` (the `build-windows-heads` job) has the UI-test steps
commented, under a comment that says the blocking bug is fixed and *"the suite
now runs and passes, so the reason for disabling it is gone."* They have stayed
off since.

Verified this pass: `dotnet test tests/TableTop.UiTests -c Release -p:Platform=x64`
passes 2/2 in 361 ms on that job's exact configuration.

Note what "2 tests" means: 359 lines of `DispatchProxy`-based reflection
scaffolding producing two assembly-wide `[Fact]`s. That is a reasonable design —
but it also means a single failure reports as one red test covering every
ViewModel, so the diagnostic value depends entirely on the assertion messages.

**Resolved.** Both steps uncommented. Verified locally twice on this job's
exact configuration (Release, x64): 2/2 in ~300ms.

Worth recording *why* it stayed off: item 23's note said "re-enabled", and only
the comment changed — the steps stayed commented for four more releases. A
comment claiming a thing is enabled is not the thing being enabled.

### X.4 — `lint` checks XML docs for two of four engine assemblies — **FIXED**

The `lint` job runs `TreatWarningsAsErrors` + `GenerateDocumentationFile` against
`TableTop.Core` and `TableTop.Hosting` only. `TableTop.Games` and
`TableTop.Presentation` are never checked, so a missing `<summary>` (CS1591) can
land in either. `TableTop.Presentation` is the shared ViewModel layer three heads
consume — the assembly where undocumented public surface costs the most.

Same shape as the deliberately-deferred `TreatWarningsAsErrors` on the UI heads,
which is also still open. The engine builds warning-clean today, so turning these
on is cheap *now* and gets more expensive with every undocumented member added.

**Resolved,** and it turned up a bigger gap than the item described.

All four engine assemblies are now in the `lint` job. Games and Presentation
were measured clean first, so this turned on with no fixes attached.

The staged `TreatWarningsAsErrors` rollout the CI comment promised is now done
per head, each measured with `--no-incremental` before being switched on:

| Head | Warnings | Now |
|---|---|---|
| WinUI | 0 | on |
| Android (native) | 0 | on |
| Console | 0 | on |
| MAUI | **96** | **off** — see X.6 |

**The gap: `TableTop.Console` was never compiled by CI at all.** It is a
shipping head. `build-and-test` names individual projects rather than building
the solution, and Console was not in `TableTop.Engine.slnx` — even though
README and CLAUDE.md both described that solution as "engine + tests +
console". A `dotnet restore` of the solution does not compile anything. Fixed
both ways: Console added to `Engine.slnx` (matching what the docs already
claimed) and given its own CI build step, with warnings-as-errors from the
start since it builds clean today.

### X.5 — Documentation drift — **FIXED**

Concrete, checkable errors found this pass. Grouped because they are one commit,
not seven.

| Where | Says | Actually |
|---|---|---|
| `README.md` — Versioning | "Currently **1.31.0**" | 1.35.0 |
| `README.md` — Documentation | links `BACKLOG.md` | deleted in `6861545`; this file restores it |
| ~~`README.md` — Quick start~~ | ~~"Six static checks run in CI" + a 6-line list~~ | **fixed under N.4** — now lists all seven, plus `check-ui-compiles.py` as the eighth |
| `README.md` — Branching | "SonarCloud (`sonarcloud` job)" | no such job — Sonar is two steps inside `build-and-test` |
| `README.md` — Documentation | "the three heads" | four |
| `CLAUDE.md` — Architecture | "Console and MAUI both currently [support fewer families]" | all four heads declare all six families; verified against each head's source |
| `ARCHITECTURE.md` ×4, `CLAUDE.md` ×3 | cross-reference `BACKLOG.md` | resolved by this file existing |

`DocumentationAccuracyTests` keeps mode/card/test counts honest, which is why
101 / 3,721 / 937 are all correct. Everything above is outside what it checks —
that is the boundary, and it is the right one; prose is not testable. But the
version number and the gate list are *not* prose, and could be.

**Resolved,** and the list grew before it shrank. `ARCHITECTURE.md` turned out
to carry more drift than the original table recorded — it is only read closely
when someone is changing architecture, and nothing enforces any of it:

| Where | Said | Actually |
|---|---|---|
| `ARCHITECTURE.md` header | "Current as of **1.34.0**" | 1.35.1 |
| `ARCHITECTURE.md` ×2 | "99 modes, 3,657 cards" | 101 / 3,721 — two releases behind |
| `ARCHITECTURE.md` | "MAUI has no AreaControl or SimultaneousAnswer screen, Console has neither plus no Monogamy or DailyCampaign" | **false** — all four heads render all six |
| `CLAUDE.md` | same families claim | same |

The families one is the dangerous kind: it reads as a deliberate architectural
limitation, so a reader would plan around a constraint that no longer exists.
Both copies now state the true position *and* why the mechanism still matters.

Every row from the original table is applied. Two new
`DocumentationAccuracyTests` cases stop the mechanical half recurring:

- `Readme_quoted_version_matches_the_build_props` — README's "Currently
  **N.N.N**" against `VersionPrefix`. This is what let 1.31.0 sit there for
  four releases.
- `Readme_names_every_static_gate_script` — every `scripts/check-*.py` must be
  named somewhere in README. Presence, not count or order: the count is prose
  that may legitimately be reworded, and a gate documented in a sentence is
  still documented.

**The rest stays unenforceable, deliberately.** ARCHITECTURE.md's counts could
be tested the same way, but its prose — which is most of its value — cannot be,
and a test that pins two numbers in a 700-line document invites the belief that
the whole file is checked. The honest fix there is that README is the enforced
copy, which ARCHITECTURE.md now says out loud at the point it repeats them.

### X.6 — MAUI's 96 CS0618 warnings block its warnings-as-errors

The one head still without `TreatWarningsAsErrors`, and the reason X.4 stopped
short of four out of four. All 96 are .NET 10 MAUI deprecations, concentrated in
a handful of APIs: `DisplayAlert`→`DisplayAlertAsync`, `Frame`→`Border`, and
`ScaleXTo`/`FadeTo`/`TranslateTo`→their `*Async` forms.

Not bundled into the CI-configuration change on purpose. Migrating to the async
variants changes behaviour at every call site — several are inside `async void`
handlers that `check-async-void.py` guards, so the guards need re-checking as
the awaits move. That is its own change with its own review, not a line in a
workflow commit.

Worth doing before the next MAUI major, where deprecated becomes removed.

**Done when:** MAUI builds warning-clean and its CI step carries
`-p:TreatWarningsAsErrors=true` like the other three heads.

---

## Later

### L.1 — Coverage floors, now that there is a real number

First real measurement (this pass, `coverage.runsettings`, 937 tests):

| Assembly | Line | Branch |
|---|---|---|
| **Total** | **91.7%** | **70.5%** |
| TableTop.Games | 93.0% | 90.2% |
| TableTop.Presentation | 89.3% | 62.9% |
| TableTop.Core | 87.0% | 76.1% |
| TableTop.Hosting | 86.5% | 67.4% |

After the N.1–N.3 fixes (961 tests): total **92.0% line / 71.5% branch**;
Hosting 89.7% / 69.7%; Presentation 89.6% / 63.1%. Core and Games unchanged.

This closes `ARCHITECTURE.md`'s standing *"A committed, real coverage percentage
— doesn't exist here"* note. Update that section.

Line coverage is healthy. **Branch coverage is the real story**: 70.5% overall,
and 62.9% in `TableTop.Presentation` — the shared ViewModel layer, where the
uncovered branches are error paths and null-guards, exactly the code that only
runs when something has already gone wrong.

Worst offenders by absolute uncovered lines:

| Type | Line cov | Missed |
|---|---|---|
| `Diagnostics.LoggerEngineDiagnostics` | 0.0% | 80/80 |
| `Extensions.ServiceCollectionExtensions` | 0.0% | 72/72 (→ N.3) |
| `Controllers.SerializedCardTurnController` | 53.7% | 50/108 |
| `Controllers.Services.SpecialCardCoordinator` | 60.3% | 58/146 |
| `SessionResumer` | 12.5% | 42/48 (→ N.2) |
| `Controllers.Services.EffectApplicator` | 29.2% | 34/48 |
| `Domain.Players.TeamPlayerManager` | 52.9% | 32/68 |

`SerializedCardTurnController` at 53.7% deserves naming: `ControllerFactory`
wraps **every** card-turn controller in it, so it is on the hot path for most of
the catalogue, and it is the type whose locking semantics were corrected as
recently as 1.34.0.

**Done when:** a floor is enforced in CI. Recommend starting at the measured
numbers minus a point (90% line / 69% branch) so it ratchets rather than blocks,
and raising it deliberately.

### L.2 — Extend static gates to the fourth head

Beyond N.4, the gates predate the native Android head and mostly scope to
MAUI/WinUI:

- `check-mvvm-method-parity.py` — `MAUI_PAGES` only. Android screens bind the
  same shared ViewModels through `ViewModelBinder` with the same
  plain-method-vs-`ICommand` duality.
- `check-shared-usings.py` — `SEARCH_ROOTS = ["ui"]`, so this one *does* cover
  Android already. No action.
- `check-ui-compiles.py` — compiles WinUI and MAUI ("both heads"). CI compiles
  Android separately in `build-android`, so the coverage exists; the script's
  name and output are just misleading.

Lower urgency than N.4 because Android is compiled in CI and C#-level mistakes
fail that build. N.4 is different: `async void` guarding is a *runtime* property
no compiler checks.

**Done when:** `check-mvvm-method-parity.py` covers Android screens.

### L.3 — `ControllerFactory` still repeats the capability chain

`ControllerFamilies.TryFor` is documented as the single source of truth, and
`ModeManifestExtensions.GetManifest` correctly derives from it.
`ControllerFactory.CreateAsync` does not — it re-tests the same seven interfaces
in its own `switch`, in an order that must be kept identical by hand. Both files
say so in comments. The parity test can only check shapes the catalogue actually
contains, and no mode implements two capability interfaces, so a transposition is
invisible — which is precisely how Monogamy and Quiz were once transposed for
real.

Not urgent: they agree today, and the parity tests pass. But "adding a capability
interface means touching both, in the same order" is a standing invariant a human
holds, and the manifest already showed the fix works.

**Done when:** `CreateAsync` dispatches on `ControllerFamilies.TryFor(mode)` and
the duplicate chain is gone, or the coupling is made explicit enough that a
reordering fails a test.

### L.4 — `CardTurnController` headroom

664 raw lines against a 700 backstop. `ControllerSizeGuardTests` calls the
headroom "deliberately thin — a few lines, not a few hundred". It is 36. The
guard's own instruction is *"extract, don't raise the ceiling."* Nine services
already live in `Controllers/Services/`. The next feature touching the turn loop
will trip this, and it will trip mid-feature, which is the worst time to be
designing an extraction.

**Done when:** either a tenth service is extracted, or the item is consciously
closed as "the ceiling is correct and the next author will handle it."

---

## Someday

### S.1 — Two roster models answering one question

`IRosterRepository` / `RosterProfile` / `JsonRosterRepository` in
`TableTop.Hosting` (async, JSON file, used only by Console) and `IRosterStore` /
`SavedRoster` / `SavedPlayer` in `TableTop.Presentation` (sync, per-head
storage — WinUI local JSON, MAUI `Preferences`, Android `SharedPreferences`) both
persist "a named group of players". Different shapes, different lifetimes, no
shared format. A roster saved in Console is invisible in WinUI and vice versa.

`ARCHITECTURE.md`'s 1.34.0 entry calls this out explicitly and deliberately:
*"the two roster shapes answering the same question differently is a real open
question, not a gap."* Agreed — recorded here so it stays a decision rather than
becoming an accident.

Side effect worth knowing: `AddTableTopHosting`'s `rosterFilePath` is passed only
by Console. WinUI/MAUI/Android leave it null, so a `JsonRosterRepository` is
registered against `AppContext.BaseDirectory` — a path the parameter's own XML
doc warns is not writable by an installed app. It is never resolved in those
heads, so it is dead weight rather than a bug. It would stop being dead the
moment someone resolves `IRosterRepository` from a graphical head.

**Decide first:** one model or two? If two, document the split as intentional in
`IRosterStore`'s docs. If one, `SavedRoster` (richer — carries teams) is the
better base.

### S.2 — Xbox controller support

Unchanged: needs `Windows.Gaming.Input` polling, a Windows machine and a physical
controller. Keyboard bindings on three of four gameplay screens remain the
tractable substitute (Millionaire deliberately excluded).

### S.3 — Android TV on real hardware

1.35.0 shipped leanback manifests, a TV banner, overscan insets and D-pad focus
handling. `ARCHITECTURE.md` and the commit message both state plainly it has
never run on a TV device or emulator. Manifest merges were inspected; focus
behaviour was not observed.

**Done when:** someone runs it on an emulator TV image or real hardware and
records what happened.

---

## Deliberately not doing

- **A visual deck editor / any out-of-repo content authoring.** No file format
  exists to author against as of 1.21.0. A new mode is a C# card bank plus a
  rebuild, by design.
- **Publishing the engine as NuGet packages.** Nothing sets `IsPackable`. This is
  load-bearing: `Directory.Build.props`'s carve-out making a public-surface
  *removal* a MINOR bump depends on every consumer being an in-tree
  `ProjectReference`. Publishing reverts that row to a flat MAJOR.

---

## Static gates

Every gate exists because of a specific bug that reached a build. Kept here
because the list of gates is not obvious from the scripts alone.

| Gate | Catches | Written after |
|---|---|---|
| `check-maui-xaml.py` | MAUI properties that don't exist on the control | XAML that parses, binds and resolves, then fails at runtime |
| `check-winui-xaml.py` | WinUI properties that don't exist at all | a shipped `LetterSpacing` — the CSS name; WinUI calls it `CharacterSpacing`. Well-formed, every binding and `StaticResource` resolved |
| `check-xaml-bindings.py` | bindings resolving to nothing → silently empty UI | renamed ViewModel properties leaving blank screens with no error |
| `check-shared-usings.py` | a shared type used without importing its namespace | `check-ui-compiles.py` structurally can't: the compiler stops at 100 errors and an unresolvable framework type masks a first-party one in the same expression |
| `check-mvvm-method-parity.py` | a MAUI page calling a plain method its shared VM doesn't expose | the `ICommand`-vs-plain-method duality (WinUI binds commands, MAUI code-behind calls methods) broke four call sites in one build |
| `check-head-family-coverage.py` | a head's declared family support drifting from its test copy | `HeadFamilyCoverageTests` can't reference the MAUI/WinUI projects, so it reads a hand-typed copy — updating the head alone changed nothing |
| `check-async-void.py` | an unguarded `async void` handler in either Android-producing head | an escaping exception terminates the process on Android. Documented above four handlers by hand, so eight others never got it — and two of the eight survived a careful manual count |
| brace-expansion check (inline in `ci.yml`) | paths containing `{` | five directories literally named `{Base,Modes,Data}` were committed — brace expansion run under `sh`. All empty, one with an unbalanced brace, meaning the setup command was truncated |
| `check-ui-compiles.py` | real compile errors in the graphical heads | needs the .NET SDK + both UI workloads, so it is not in the SDK-free `xaml` job |

The `xaml` job runs first and depends on nothing else, deliberately: these are
pure-Python checks needing no SDK, no workload and no restore, so they still
report when the build itself is red.
