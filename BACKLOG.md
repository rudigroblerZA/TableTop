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
> **A second pass on 2026-08-30** closed X.2-X.5 and L.1-L.4, and partly
> closed X.6. Suite 937 → **964**.
>
> **A third pass on 2026-08-30** (1.35.4) closed **X.6b** — the documentation
> that item was blocked on turned out to be reachable, and the migration it
> describes is exact.
>
> **1.36.0** added the trait-analysis layer and Big Five, **1.37.0** added Love
> Languages on top of it, and **1.38.0** closed **N.6** by giving the three
> graphical heads a `TraitProfile` screen (see `ARCHITECTURE.md`). What is left
> is **N.7** (CI's WinUI job hangs — now contained with a timeout and
> `--blame-hang` diagnostics, but not yet root-caused), **X.6c** and S.2-S.3.
>
> **A fourth pass on 2026-08-31** closed **X.6a** (`Frame` → `Border`, grounded
> in Microsoft's published migration table the way X.6b was) and settled **S.1**
> (two roster models, kept deliberately). It also found that **X.6c is blocked**
> on a structural problem the item did not know about — 11 of 16 `DataTemplate`
> scopes bind nested types XAML cannot name — so what is genuinely left is X.6c's
> prerequisite refactor, plus S.2-S.3, both of which need hardware.
>
> Two things need a human: tags `v1.35.0`/`v1.35.1` are local and **not
> pushed**, and `develop` is ahead of `origin`.

### N.7 — CI's WinUI job never finishes: "Run UI tests" hangs — **CONTAINED, not root-caused**

**No CI run on this repository has completed since 2026-08-30.** The `Build
WinUI` job reaches its last step and stays there until the run is cancelled —
the two `develop` runs on 2026-08-30 (`33299660962`, `33303279916`) each sat for
roughly six hours before being cancelled, and every run since has the same job
stuck `in_progress`.

**It is not the build.** Reading the job's steps rather than its status makes
that clear:

| step | outcome |
|---|---|
| Checkout, Setup .NET 10 | success |
| **Build WinUI** | **success, ~90s** |
| **Run UI tests** | **`in_progress`, indefinitely** |
| Upload UI test results | never reached |

So WinUI compiles fine. `dotnet test tests/TableTop.UiTests` is what hangs
(`.github/workflows/ci.yml:353`).

**The timeline points at one change.** The last CI run to complete was
`33271411188` on 2026-08-29. The first to hang was 2026-08-30 07:37 — the merge
that brought in 1.35.2, whose own `ARCHITECTURE.md` entry records: *"the WinUI
UI-test steps actually run: item 23 declared them 're-enabled' and changed only
the comment, leaving them commented for four further releases."* X.3 genuinely
enabled a step that had never actually executed in CI, and it does not
terminate. The step was reported as fixed; what it did was surface a hang.

**Where to look.** `ViewModelBindingTests` has two facts, both of which
reflect over every ViewModel type, construct each one, and then — in
`Every_settable_property_raises_PropertyChanged` — **set every public settable
property on it**. That is a lot of behaviour to invoke blind: a setter that
starts a timer, blocks on I/O, or waits on a task would hang the run and look
exactly like this. Constructing rather than setting is the cheaper suspect to
rule out first.

**Consequences while it stands.** No PR can ever report all-green, so "CI is
green" cannot currently be a merge criterion — the two PRs merged today
(#41, #42) both went in with this job still running. And the WinUI head's UI
tests are providing no signal at all despite appearing to run.

**Evidence:** job `99496277158` of run `33394673779` (2026-08-31, commit
`2fc41c3`) shows the step breakdown above. Run `33271411188` (2026-08-29,
success) versus `33299660962` (2026-08-30, cancelled at ~6h) brackets the
change. Not reproduced locally — `TableTop.UiTests` is Windows-only and needs
the Windows App SDK, neither of which this environment has.

**Done when:** the WinUI job completes *and* the tests actually run to a result.

---

**Done 2026-08-31 — containment and instrumentation. The root cause is still
unknown, and that is stated rather than glossed.**

**The decisive evidence is already in the workflow's own comment:** the step's
history records these tests running *"green twice locally on this job's exact
configuration (Release, x64): 2/2 in 361ms"*. Two `[Fact]`s that finish in a
third of a second cannot plausibly be looping. Combined with the step
breakdown — `Build WinUI` succeeds in ~90s, `Run UI tests` never returns — the
fault is almost certainly in the **test host**, not the test bodies: discovery
or shutdown of a host that references a WinUI *app* assembly on a runner with
no interactive desktop session. Reading the ViewModels supports that; the two
plausible in-code culprits were checked and cleared:

- No `.Result`, `.Wait()` or `GetAwaiter().GetResult()` anywhere in
  `TableTop.Presentation` or `TableTop.WinUI`, so no blocking-async deadlock
  under xUnit's synchronisation context.
- `CardTurnGameViewModel`'s timer loop is not reachable: it starts only from
  `StartTimerAsync`, gated on `TimerEnabled`, which is get-only and false under
  the stubbed constructor arguments `TryConstruct` supplies.

**What changed (`.github/workflows/ci.yml`):**

1. **`timeout-minutes: 30` on the job.** It was inheriting GitHub's 6-hour
   default, which is exactly what the 2026-08-30 runs spent. ~20x the healthy
   duration.
2. **`--blame-hang --blame-hang-timeout 5m --blame-hang-dump-type full`.** This
   is the diagnosis, and it is why the job can now answer the question it has
   been refusing to. On hang the host is killed and a sequence file is written
   naming the test that was executing — **or naming none, which is itself the
   answer**, because it places the hang outside the test bodies. The dump
   carries thread stacks. 5m is ~800x the healthy run, so a genuine regression
   in the tests still fails on its own assertion long before this trips.
3. **The artefact upload now captures the dumps and sequence files**, not just
   the `.trx`, and keeps `if: always()`. Without that a hang uploaded nothing —
   which is how this went a full day undiagnosed.

**Explicitly NOT done, and deliberately so:** neither test is skipped, disabled
or quarantined. Both still run and both still have to pass. The job now ends
**red with evidence** rather than stalling invisibly, which is a different thing
from being green.

**Not verified:** this could not be reproduced here — `TableTop.UiTests` is
Windows-only and needs the Windows App SDK, and there is no `dotnet` in this
environment at all. The change is a workflow edit validated by parsing the YAML
and checking the folded `run` command; whether `--blame-hang` names a test or
names nothing is the finding the next run will produce.

**Still open:** the root cause. The next red run's `Sequence*.xml` is the thing
to read, and it should decide between "a specific test hangs" and "the host
never starts or never exits".

---

### N.6 — Three heads cannot play the trait-assessment modes — **FIXED**

`ControllerFamily.TraitProfile` shipped in 1.36.0 with a Console renderer only.
WinUI, MAUI and native Android declare six of seven families, so the
trait-assessment modes are the ones they cannot open. **1.37.0 made that two:**
`BigFiveMode` and `LoveLanguagesMode`. The cost of not having this screen now
grows with every mode built on the layer, which is the argument for doing it
before a third arrives.

**This is a stated gap, not a silent one**, which is the whole reason
`SupportedFamilies` exists. All three heads already route unknown families to a
fallback that says "'Big Five' needs a TraitProfile screen, which this app
doesn't have yet" rather than crashing or falling through to a card-turn screen
— the failure backlog item 4 was written after, when MAUI's router silently
mishandled Herd and Claimed! and Console had no default arm at all.

`ControllerFamilyTests.GraphicalHeads_PlayEveryModeExceptTheTraitProfileOnes`
asserts the gap **by name**. A head that gains the screen fails that test until
its mirror array is updated, and `TheOnlyUnsupportedFamilyAnywhere_IsTraitProfile`
fails if a *second* unsupported family ever appears rather than letting it hide
behind this one. This worked in practice rather than in theory: adding Love
Languages failed the by-name test immediately, which is exactly the prompt to
come back here and widen the item.

**What a screen needs.** Less than the family count suggests — the shape is
close to Herd's, which all three already render: one prompt, everyone answers,
submit together. The differences are a fixed five-button response row instead of
free text, and a results screen that draws five bars per player plus the
pairwise comparison, where Herd draws a scoreboard. There is no shared
`TableTop.Presentation` ViewModel for it yet; writing one is the first step, and
it is what would let WinUI and MAUI share the work rather than doing it twice.

**Evidence:** the four heads' `SupportedFamilies` declarations, their fallback
arms (`PlayerSetupPage.xaml.cs:157`, `GameViewModels.cs:90`,
`GameScreenFactory.cs:79`), and the tests named above. Not exercised on any
graphical head — no `dotnet` in the pass that added this, and three of the four
heads cannot be built here at all.

**Done when:** the three heads declare `TraitProfile`, their mirror arrays in
`ControllerFamilyTests` match, and the by-name gap test is deleted rather than
edited to expect a smaller gap.

**Resolved in 1.38.0, in that order.** `TraitProfileGameViewModel` landed in
`TableTop.Presentation` first, so the three heads are views over one state
machine rather than three implementations of it — which is what the note above
predicted would make this cheap. WinUI got a `UserControl` and a `ViewLocator`
entry, MAUI a `ContentPage` on the `IAsyncInitializablePage` two-phase pattern,
and native Android a code-built screen. The by-name test is gone;
`EveryHead_CanPlayEveryModeInTheCatalogue` replaces it as a Theory over all four
mirrors, joined by `EveryHead_DeclaresEveryFamilyTheCatalogueProduces` reading
the same invariant from the families end.

Two things fell out of doing it:

- **`ParameterRelayCommand`**, because a Likert row is five buttons per player
  differing only in the value they send. Its parameter is `object` rather than a
  generic `T` because XAML passes `CommandParameter="3"` as a **string** on both
  WinUI and MAUI — a `RelayCommand<int>` binds and then silently never executes.
- **`check-xaml-resources.py`**, after this work reached for a
  `SecondaryButtonStyle` that has never existed. A missing resource key is a
  navigation-time crash on both XAML heads and no gate caught it. Proved to fail
  on the real bug before being trusted.

**Still not exercised on a device.** No `dotnet` in the pass that did it, and
three of the four heads cannot be built here regardless. The ViewModel has real
tests; no XAML was compiled and no screen has been rendered. That is the same
standing caveat S.3 carries for Android TV, and `check-ui-compiles.py` is the
gate that would close it.

---

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

### X.6 — MAUI deprecations — **PARTLY FIXED** (a and c open), and the original count was wrong

**Correction first.** This item said "96 CS0618 warnings". That was measured by
grepping the build for `warning CS0618` — the C# compiler's share only. Turning
`TreatWarningsAsErrors` on to test the theory surfaced a second warning source
the normal build output had buried: the **XAML compiler**. The real figure was
**518**, not 96:

| Code | Count | What |
|---|---|---|
| XC0022 | 492 | binding could be compiled if `x:DataType` were specified |
| XC0618 | 18 | `UseSafeArea` deprecated, on 9 pages |
| CS0618 | 8 | `Frame` |

Worth recording how that got past me: I trusted a grep for one warning prefix
instead of counting every `warning` line. The number in a backlog item is only
as good as the command that produced it.

**Done: the 22 deprecated async-API call sites.** `DisplayAlert` →
`DisplayAlertAsync` (18), `ScaleXTo`/`FadeTo`/`TranslateTo` → their `*Async`
forms (4), across `GameplayPage`, `GameSelectionPage`, `PlayerSetupPage`,
`SettingsPage` and `SafeNavigation`. Pure renames — both forms already return
`Task` — so the `async void` guards `check-async-void.py` covers were unaffected
(re-run to confirm). **CS0618: 96 → 8.** Both MAUI targets build.

**Not done, and split out by risk rather than deferred as one lump:**

- **X.6a — `Frame` → `Border` (8 CS0618) — FIXED 2026-08-31.** 35 elements
  across 11 XAML files, plus the shared `CardStyle`/`PlayingCardStyle` that
  targeted `Frame`.

  This item called the change unverifiable, and it was right to be wary — but
  the same thing that unblocked X.6b applies: Microsoft publishes the mapping.
  "What's new in .NET MAUI 9 → Deprecated APIs → Frame" states it directly:
  `Frame.BorderColor` becomes `Border.Stroke`, `Frame.CornerRadius` becomes part
  of `Border.StrokeShape`, and it warns that padding may need restating.

  **That padding warning is the whole risk, and an audit retired it.** `Frame`
  carries an implicit default padding; `Border` does not. Every one of the 35
  elements was checked: each either sets `Padding` explicitly or takes a style
  that does, so nothing depended on the implicit default. Three further defaults
  were confirmed from the API docs rather than assumed — `Border.Stroke` is
  `null` (so the two elements that set no `BorderColor` gain no stroke),
  `StrokeShape` is `Rectangle`, `StrokeThickness` is 1.0.

  Two elements carried `HasShadow="True"` (Claimed's pending card, Monogamy's
  active card). `Border` has no `HasShadow`, and dropping it would have silently
  flattened both. They now carry an explicit `Shadow` — the exact values
  `PlayingCardStyle` already uses for card stock, reused rather than invented.
  `HasShadow="False"` simply went, being `Border`'s default.

  Two happy findings: the MAUI head **already contained hand-written `Border`s**
  using `Stroke`/`StrokeShape` (PlayerSetupPage, GameplayPage), so the result
  matches an idiom the file already had; and `check-maui-xaml.py` independently
  verifies the direction, since it knows `Border` has no
  `BorderColor`/`CornerRadius`/`HasShadow` and `Frame` has no `Stroke*`.

  **Evidence:** all eight gates pass, every MAUI XAML file parses, and no
  `Frame` remains. `x:Name="CardFrame"` is kept — it is a name, and the
  code-behind animations on it (`ScaleXToAsync`, `FadeToAsync`,
  `TranslateToAsync`, `Opacity`, `TranslationY`) are all `VisualElement`
  members that carry over unchanged. Still no build and no device: the visual
  result has not been seen.
- **X.6b — `UseSafeArea` (18 XC0618) — FIXED 2026-08-30.** The blocker was
  documentation access, not risk, and the documentation was reachable this
  pass. Microsoft Learn's safe-area page carries an explicit migration table:
  `ios:Page.UseSafeArea="True"` becomes `SafeAreaEdges="Container"` on
  `ContentPage`. That is a stated first-party equivalence, not an inference,
  which is what made this safe to do without a device.

  Applied to all 9 pages. Each loses two lines (the attribute and the
  `xmlns:ios` declaration — nothing else in the head used that namespace) and
  gains one. `SafeAreaEdges` is a plain `ContentPage` property, so the
  replacement is cross-platform where the thing it replaced was iOS-only.

  **It was hiding a live Android bug.** .NET 10 changed `ContentPage`'s Android
  default from container-safe to `None` (edge-to-edge); .NET 9 behaved like
  `Container`. The old attribute was iOS-only, so nothing was protecting
  Android — every one of these pages had silently begun rendering under the
  status and navigation bars on a head this project ships as a first-class
  target. `Container` is precisely the value Microsoft names for restoring
  .NET 9 behaviour, so one change clears the deprecation and the regression
  together. Worth noting as a pattern: this is the second time in three
  releases that a "deprecation warning" turned out to be reporting a defect.

  `RoasterPage` was swept in as well. It never carried the deprecated
  attribute, so no warning pointed at it — but it is the tenth `ContentPage`
  in the head and hits the same new Android default. It now carries
  `SafeAreaEdges="Container"` like the other nine.

  **Evidence:** all ten files parse as XML; the seven runnable
  `scripts/check-*.py` gates pass. No `dotnet` in this environment, so no
  build and no XC0618 recount. Note that `check-maui-xaml.py` proves nothing
  here — it is a denylist keyed on tag name and has no `ContentPage` rules, so
  `SafeAreaEdges` is trusted from the API docs, not from the gate. Safe-area
  behaviour has still never been observed on hardware.
- **X.6c — compiled bindings (492 XC0022) — BLOCKED, and this item's premise
  was wrong.** It said "mechanical but wide". It is neither: it is blocked on a
  structural problem that has to be fixed first.

  **XAML cannot name a nested type.** There is no `Outer+Inner` syntax XamlC
  accepts, and **11 of the 16 `DataTemplate` scopes in this head bind to nested
  classes**:

  | ViewModel | nested item type | template |
  |---|---|---|
  | `ClaimedGameViewModel` | `TerritoryOption` | ClaimedGamePage |
  | `HerdGameViewModel` | `PlayerAnswerEntry` | HerdGamePage |
  | `MillionaireGameViewModel` | `AnswerOption`, `LifelineOption` | MillionaireGamePage ×2 |
  | `MonogamyGameViewModel` | `ZoneOption` | MonogamyGamePage |
  | `PlayerSetupViewModel` | `SavedRosterOption`, `PlayerEntry` | PlayerSetupPage ×2 |
  | `TraitProfileGameViewModel` | `PlayerResponseEntry`, `PlayerProfileView`, `TraitScoreView` | TraitProfileGamePage ×3 |

  Only `GameSelectionViewModel` (`Archetype`, `GameModeItem`) and
  `RoasterViewModel` (`RoasterTemplate`, `SavedPlayer`, `SavedRoster`) expose
  top-level item types.

  **And it is not partially doable per page.** A `DataTemplate` without its own
  `x:DataType` inherits the enclosing scope's, so annotating a page root makes
  every un-annotated template inside it resolve its bindings against the *page's*
  ViewModel. That is not a silent empty binding — with compiled bindings it is a
  **build error**. So a page can be annotated only when all of its templates can
  be, which rules out 8 of the 11 pages.

  **The prerequisite is a real refactor:** promote those 11 nested classes to
  top-level types in `TableTop.Presentation.ViewModels`. That changes the shared
  layer's public shape for all four heads and touches their tests. It is a
  sound change — arguably better design regardless — but it is its own item, and
  it wants a machine that can build MAUI, because the failure mode of getting a
  single binding path wrong flips from "renders empty" to "does not compile".

  **Not attempted in the pass that found this.** No `dotnet`, so an unverifiable
  public refactor of the shared ViewModel layer whose failure mode is a broken
  build is exactly the trade the rest of this backlog refuses to make.

**Done when:** all three land and MAUI's CI step carries
`-p:TreatWarningsAsErrors=true` like the other three heads. **X.6a and X.6b are
done.** X.6c is blocked on promoting 11 nested ViewModel classes to top-level
types — see above; that is the next piece of work here, and unlike the other
two it genuinely wants a machine that can build MAUI.

---

## Later

### L.1 — Coverage floors, now that there is a real number — **FIXED**

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

**Resolved.** New `scripts/check-coverage.py` parses the Cobertura report CI
already produces and fails below a floor. Wired into `build-and-test` right
after the existing summary step — which had been *printing* coverage all along
while nothing failed when a number went down.

Floors are ~1 point under measured, per assembly as well as total:

| | measured | floor |
|---|---|---|
| **Total** | 91.9 / 70.8 | 90.0 / 69.0 |
| Core | 87.0 / 76.0 | 86.0 / 75.0 |
| Games | 93.0 / 90.2 | 92.0 / 89.0 |
| Hosting | 88.5 / 68.7 | 87.0 / 67.0 |
| Presentation | 89.7 / 61.9 | 88.0 / 60.0 |

Per-assembly floors are the ones that matter: `Games` is ~60% of the tree at
93%, so it can absorb a sharp fall in `Hosting` or `Presentation` — where the
logic that breaks actually lives — while the total barely moves.

Two design calls worth stating. A **missing report fails** rather than passing
quietly, since "no data" is how a gate silently switches itself off. And the
step is deliberately **not** `if: always()`, unlike the reporting steps above
it: a failed test run produces partial coverage, and failing this too would
bury the real cause under a second, misleading error.

Verified in both directions — passes at current numbers, exits 1 against an
impossible floor, exits 1 on a missing report.

### L.2 — Extend static gates to the fourth head — **FIXED**

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

**Resolved.** `check-mvvm-method-parity.py` now scans both heads via a `HEADS`
table — MAUI's `Pages/*.xaml.cs` and the native head's `Screens/*.cs` — matching
`_vm.Method()` and Android's `Vm.Method()` (a protected property on
`GameScreenBase<T>`). Now covers 9 files per head.

Proved by injecting a `Vm.CompleteTypo()` into `CardTurnGameScreen`: correct
file:line diagnostic and exit 1, exit 0 once reverted.

**Found a latent bug while widening it.** `vm_type_for` iterated a `set` and
returned the first match — and Python randomises string hashing per process, so
for any file mentioning two shared ViewModels the answer genuinely varied
between runs. No file trips it today, which is why it never showed. It now
picks the earliest occurrence, tie-breaking on the longer name. A gate that can
answer differently on a re-run is worse than no gate: it fails intermittently
and teaches people to re-run it.

`check-shared-usings.py` already covered Android (`SEARCH_ROOTS = ["ui"]`), and
`check-ui-compiles.py`'s scope is fine — CI compiles Android separately. No
change needed to either.

### L.3 — `ControllerFactory` still repeats the capability chain — **FIXED**

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

**Resolved.** `ControllerFactory.CreateAsync` now switches on
`ControllerFamilies.TryFor(mode)` instead of re-testing the seven capability
interfaces in a hand-maintained order. All three dispatch sites finally agree by
construction rather than by comment: `TryFor` decides, `ModeManifestExtensions`
already followed it, and the factory does now too.

The within-family choice moved to a private `ProgressionFor(mode)`.
`IFlowAwareMode` and `IDiceProgressionMode` select a *progression strategy*, not
a controller type — which is exactly why `TryFor` folds all three into
`CardTurn` — so keeping that decision separate is the point rather than an
oversight. Adding a progression flavour is now a one-method edit; adding a
controller shape is still a new family plus an arm.

No public-surface change (`ProgressionFor` is private; `api/*.api.txt`
unchanged). 964 tests pass, including the family-parity suite that guards this
exact invariant.

### L.4 — `CardTurnController` headroom — **CLOSED, no change**

664 raw lines against a 700 backstop. `ControllerSizeGuardTests` calls the
headroom "deliberately thin — a few lines, not a few hundred". It is 36. The
guard's own instruction is *"extract, don't raise the ceiling."* Nine services
already live in `Controllers/Services/`. The next feature touching the turn loop
will trip this, and it will trip mid-feature, which is the worst time to be
designing an extraction.

**Closed — consciously, without extracting.** Taking the second option the item
offered, because the first would have made the code worse.

The framing was slightly off. 664/700 *raw* is the backstop; the metric the
guard itself calls "the one that matters" is **code** lines, and that reads
**346/390 — 44 lines of headroom**, not 36. Comments and blank lines don't cost
budget, which is the whole reason the guard counts two ways.

More to the point, I read every member looking for a responsibility still
sitting inline. There isn't one. What's left is the turn loop and thin
delegations to the nine coordinators already extracted — `LevelUp`/`SpeedUp`/
`JumpTo` forward to `FlowCoordinator`, `SaveAsync` to `PersistenceCoordinator`,
`UndoLastTurn` to `UndoCoordinator`, `HandleSkipPolicy` to `SkipPolicy` — plus
`Start`/`AdvanceTurn`/`EmitCard`/`OnTurnCompleted`/`OnGameEnded`, which *are*
the turn loop and belong here. The nearest candidate, `BuildScores`, is ~18
lines of result formatting; giving it a file to buy 18 lines would be a service
that exists to move lines rather than to own a concept, which is the failure
mode the nine real extractions avoided.

So the ceiling stays where it is and no tenth service is invented to satisfy a
number. The guard is doing its job: it keeps score, and the score is currently
fine. Reopen this when a feature actually trips it — at which point there will
be a real new responsibility to name, which is a better basis for a boundary
than a line count.

---

## Someday

### S.1 — Two roster models answering one question — **DECIDED: keep both**

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

**Answer: two, deliberately. Documented in `IRosterStore`'s doc comment, with a
cross-reference from `RosterProfile` so a reader arriving at either half finds
the reasoning.**

**This item's own framing was wrong, which is why the answer went the other
way.** `SavedRoster` is not "richer". Neither shape is a superset:

| | `SavedPlayer` (Presentation) | `PlayerProfile` (Hosting) |
|---|---|---|
| `Team` | ✅ | ❌ |
| stable `Id` | ❌ | ✅ |
| `IsParent` / `IsMarried` | ❌ | ✅ |
| `SchemaVersion` | ❌ | ✅ |
| `Gender` / `Age` | nullable | non-null, defaulted |

Picking either as "the base" silently drops fields the other half depends on;
merging them produces a union type where every consumer ignores half of it.

Three further reasons, all found while deciding:

- **The nullability split is semantic, not sloppy.** `SavedPlayer` allows null
  `Gender`/`Age` because it models setup input *part-way through being entered*.
  `PlayerProfile` defaults them because it models a *durable profile*. Same
  words, different lifecycle stage.
- **The dependency direction rules out the cheap merge.** Console deliberately
  does not reference `TableTop.Presentation` (item 28), and `Presentation` sits
  above `Hosting`. A shared type in `Hosting` drags `Team` — a presentation
  concept — into the engine; a shared type in `Presentation` forces Console onto
  the ViewModel layer.
- **Sync versus async is load-bearing.** `IRosterStore` is synchronous because
  per-head key-value storage is; `IRosterRepository` is asynchronous because it
  is file I/O. Unifying makes one of them lie.

**The accepted cost is stated rather than explained away:** a roster saved in
Console is invisible to the graphical heads and vice versa, even on one machine.

**The side effect is now flagged where it can bite.** `AddTableTopHosting`'s
`rosterFilePath` registration is lazy and never resolved by a graphical head, so
it stays dead weight rather than a bug — but its doc now says plainly that a head
which *starts* resolving `IRosterRepository` would be writing to
`AppContext.BaseDirectory`, which an installed app cannot write to, and must pass
a real path.

**Done when:** ✅ decided and documented. Reopen only if a head needs to read the
other's rosters, which is the one requirement that would force convergence.

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
