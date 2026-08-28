# TableTop — Backlog

Current as of **1.29.3**, August 2026. Open items only; git history has the rest.

Items 1–8 predate the 1.18.0 review and keep their numbers — rewriting a
numbered item in place is how item 7 vanished once (see its note). Items 9–16
came from that review; 1.19.0 closed three of them and half of a fourth, and
those are marked **CLOSED** in place rather than deleted, so the next reader can
see what was done and why. Items 17–18 came out of actually building and running
1.19.0 — the step every previous entry here had to be written without. Items
23–28 came from a review of 1.28.0, the first pass run with a full toolchain
*and* a green suite behind it.

---

## Priority

Item numbers are chronological, not ordered — they exist so an item can be
referred to without being renumbered (item 7 explains what happens otherwise).
This is the ordering to work in. It comes from a UI-architecture review that cut
across the existing numbering rather than adding to it.

| | Item | Why it matters |
|---|---|---|
| **P2** | **24** — `RoasterViewModel` is the only shared ViewModel with no tests | Nine of the ten in `TableTop.Presentation` have a test file. This one has none, and it is the newest and least exercised. |
| **P2** | **25** — saved rosters are a closed loop | You can build and persist a roster, and then never play with it. Nothing outside the Roaster screen reads a `SavedRoster`. |
| **P2** | **26** — the "Team" roster template promises sides it never assigns | Teams are a real first-class concept (`ITeamMode`, `AssignTeams`). The template's description says "split into sides" and does nothing of the kind. |
| **P3** | **28** — Console has no Roaster | Third head, no roster builder and no `IRosterStore`. Smallest of the parity gaps but the one now written down. |

**Items 23 and 27 are closed** and have dropped out of this table. Item 27
turned out to be eight unguarded handlers rather than the six it counted by
reading — the two extras found by the gate the item proposed, written before
the fix for exactly that reason. `check-maui-async-void.py` now enforces it
in CI. Item 23 was closed by re-enabling every disabled step, which first
required fixing two genuine build failures: MAUI Android could not build at
all (`XA1030`, AOT-versus-trimming), and the UI-test host could not start
until item 2 fixed it. CI now compiles all four heads again.

Items 23–28 came from a review of the tree at 1.28.0 — the first review run
on a machine with a full toolchain *and* a green suite, so unlike earlier
passes nothing below is "not verified by a build". Every claim was checked
by running something.

**Items 2, 4, 5, 12, 18, 19, 20 and 21 are all closed** and have dropped out
of this table entirely. Item 12 closed the coverage *mechanism* — the
declarations and the gate that keeps them honest. Item 4 closed the actual
gap that mechanism was reporting: real screens on every head for every
family in the catalogue. Item 5 gave both heads a real composition root, so
a container-registered `IControllerFactory`/`IAppSettings` now actually
reaches the one path that was silently ignoring it. Item 18 deleted the
inert `Presentation`/`Resolved*` layer rather than resurrecting it. Item 19
finished what item 5 started — MAUI's remaining nine `AppSettings.Instance`
reads now go through the injected `IAppSettings` — and fixed the one
persistence path that most needed it: a saved session failing silently. Item
20 gave the three blocking MAUI pages a two-phase construct-then-initialise
shape and converted WinUI's `ResumeCommand` to the async-command idiom the
rest of the codebase already used. Item 21 removed the surviving comments
that described the WinUI files they sit in as WPF, and fixed the one that
was outright wrong about MAUI's save behaviour. Item 2 turned out to be
three real bugs deep — a test host that couldn't start, a reflection lookup
broken by a newer BCL overload, and a null default that crashed two
constructors — plus a structural gap where the test scanned an assembly
with nothing mutable in it; `tests/TableTop.UiTests` now runs and passes for
what appears to be the first time anywhere. See each item's own section
below.

Items 3 and 8 sit below this — real, but neither is between a player and a
working game, and each needs either a human decision or hardware this
project doesn't have in CI. Items 13 and 15 are now also closed in full —
item 15 by extraction rather than by raising its guard's ceiling.

**1.21.0 removed a guard without replacing it, and that is worth a decision
rather than a shrug.** `AgeVocabularyTests` checked that
`GameModeFileValidator.ValidRatings` and the `AgeRating` enum described the same
set — two vocabularies in assemblies that cannot reference each other, on a
field governing what content reaches which audience. The validator is gone with
the manifest format, so the test went too. Nothing about `AgeRating` itself
changed, and it is still consumed by `ArchetypeFilter` and the pickers; there is
simply no second vocabulary left to drift from. If a rating is ever added,
re-check that claim before trusting it.

---

**How the 1.21.0 review was done, so you know what to trust.** The five static
gates were run and all pass. Nothing was compiled or executed: that sandbox had
no `dotnet` at all, so `check-ui-compiles.py`, `offline-build.sh` and the test
suite were all unavailable. Everything below it is from reading source against
docs. Item 9 is the one place that review asserted a test was *red* rather than
describing a design gap — the reasoning is spelled out so it can be checked in
about a minute on a machine with the SDK.

**That constraint no longer applies, and several items below were written under
it.** As of 1.22.0 the working machine has the .NET 10 SDK, NuGet access and
Python 3.12: the full solution builds, the suite runs, and all six scripts in
`scripts/` execute. Anything below marked "not verified by a build" was true
when written and is now simply checkable — check it rather than trusting it.

---

### 1. Coverage is a static approximation, not a number — **CLOSED**

CI collects coverage (`coverage.runsettings`) and `scripts/measure-coverage.ps1`
turns it into a percentage. Neither had run to completion when this item was
written — no NuGet access (`NU1301: 403`, re-confirmed each time, never
assumed).

`scripts/offline-build.sh` (item 8) did **not** unblock this: coverage needs
the real test suite, which needs xunit and coverlet from NuGet. The engine
building offline is not the same as the tests running.

**Closed by running the ask.** `./scripts/measure-coverage.ps1` on a machine
with NuGet, against the 862-test suite (all passing):

| Metric | Result |
|---|---|
| Line coverage | **91.8%** (17,527 / 19,080 coverable lines) |
| Branch coverage | 67.9% (1,307 / 1,924) |
| Method coverage | 70.9% (1,504 / 2,121); fully covered 59.8% (1,270 / 2,121) |
| Assemblies / Classes / Files | 4 / 349 / 214 |

Per-assembly: `TableTop.Core` 85.6%, `TableTop.Hosting` 88.9%,
`TableTop.Presentation` 88.7%. (`TableTop.Games` isn't broken out separately
by the tool — mode-sweeping tests exercise it heavily but don't name types,
the same effect that made the old static-reach proxy call it falsely low.)

The line/method split is the honest read: line coverage alone would say this
suite is thorough, and it mostly is, but branch coverage sitting almost 24
points lower means a meaningful fraction of conditional paths inside covered
methods are untested — a line can be "hit" while only one side of an `if`
ever runs. Full-method coverage (59.8%) being well below line coverage
(91.8%) says the same thing from a different angle: many methods have *some*
covered line but aren't exercised end-to-end.

Named zero-coverage classes worth a look before trusting any future refactor
near them: `TableTop.Hosting.Controllers.JsonGamePersistence`,
`TableTop.Hosting.Diagnostics.LoggerEngineDiagnostics`,
`TableTop.Hosting.Extensions.ServiceCollectionExtensions`,
`TableTop.Hosting.ResumableSession`, and `TableTop.Hosting.SessionResumer` at
12.5%. None of these came up in this pass — recorded here so the next person
touching one knows to write the test first, not assume one exists.

Confirms the static-reach proxy this item always distrusted was wrong in
both directions, same as previously found: it once flagged
`TableTop.Presentation` low before it had any tests, and called
`TableTop.Games` low because mode-sweeping tests don't name types — a real
run settles it instead of guessing.

### 2. `tests/TableTop.UiTests` can't reach the shared ViewModels — **CLOSED**

It references `TableTop.WinUI`, which needs the WinUI SDK. `TableTop.Tests`
(SDK-free) got a direct `TableTop.Presentation` reference instead, so real
ViewModel tests got written without fixing this — a sidestep, not a fix.
Whatever `UiTests` existed for is still blocked.

Decide: give it a reason to exist that doesn't need the SDK, or accept it
needs Windows and say so plainly.

**Closed on a machine with the WinUI SDK — and what actually surfaced once
it ran is worth recording in full, because none of it was visible before.**
This project's own CI job (`build-windows-heads` in `ci.yml`) has its WinUI
build and UI-test steps commented out, so as far as can be determined this
suite had never actually executed and reported a result anywhere before
now — every prior backlog entry that says "not verified — no `dotnet`-driven
UI smoke test exists" was correct, but for a stronger reason than "no SDK
was available": even on a machine that had one, the suite couldn't run.

Three real, independent bugs, found in this order:

1. **The test host couldn't start at all.** `dotnet test` threw
   `FileNotFoundException` on `Microsoft.TestPlatform.CoreUtilities` before a
   single test ran. That assembly is a plain compile+runtime asset in
   `Microsoft.TestPlatform.ObjectModel`'s `lib/net8.0` folder — nothing
   RID-specific — but with `RuntimeIdentifiers` set (required for
   `UseWinUI`), the SDK stops copying framework-dependent package assets to
   the build output on the assumption a publish step will resolve them via
   the RID graph, and `dotnet test` never publishes. `TableTop.Tests` (no
   `RuntimeIdentifiers`, no `UseWinUI`) never hits this. Fixed with
   `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>` on the
   `UiTests` project.
2. **`StubProxy.For` threw `AmbiguousMatchException`.** It located
   `DispatchProxy.Create` via `Type.GetMethod(name, bindingFlags)`, which
   requires exactly one match — true when this was written, false once the
   BCL added the non-generic `DispatchProxy.Create(Type, Type)` overload
   alongside the original `Create<T, TProxy>()`. The new overload is exactly
   what this helper needs (a runtime-known `Type`, not a compile-time
   generic argument), so the fix is to call it directly instead of
   reflecting for the ambiguous one. `TryConstruct` swallowed the exception
   as "this dependency can't be satisfied", so **every ViewModel with an
   interface-typed constructor parameter silently dropped out of both
   tests' coverage** — which in this codebase is nearly all of them.
3. **`DefaultFor` returned `null` for `Archetype`**, which has no
   parameterless constructor. `ArchetypePickerViewModel`,
   `SubArchetypePickerViewModel` and `GameSelectionViewModel` all
   dereference their `Archetype` parameter in the constructor body
   (`parent.SubArchetypes`, `node.Modes`), so the null default threw a
   `NullReferenceException` that `TryConstruct` again swallowed as a benign
   "can't build this" — silently dropping three more ViewModels, including
   the ones a fourth finding (below) made the only interesting targets left.
   Fixed with a stub `Archetype` instance for `DefaultFor` to hand back.

**A fourth finding, structural rather than a bug in any one place: even
with the three fixes above, `Every_settable_property_raises_PropertyChanged`
still failed — "found 0", the same failure as before, for a completely
different reason.** Every ViewModel actually declared in `TableTop.WinUI` —
the whole picker chain plus `UnsupportedModeViewModel` — is fully immutable:
get-only properties, commands assigned once in the constructor, nothing to
mutate. Every settable, `PropertyChanged`-raising ViewModel
(`SettingsViewModel`, `CardTurnGameViewModel`, `MillionaireGameViewModel`,
`PlayerSetupViewModel`, …) lives in `TableTop.Presentation`, which the
test's `UiAssembly` anchor never scanned. The test could not have passed
non-vacuously as originally written — not because anything was broken, but
because the one assembly it looked at has nothing mutable in it. Fixed by
scanning `TableTop.Presentation` too (anchored on `SettingsViewModel`,
alongside the existing `Navigator` anchor for `TableTop.WinUI`) — an
assembly `TableTop.WinUI` already references and which is therefore already
loaded in the test host, so this costs nothing extra.

Both tests now pass with real, non-vacuous coverage across both assemblies.
Verified: the 862-test engine suite is unaffected, WinUI still builds clean,
and `check-shared-usings.py`/`check-mvvm-method-parity.py`/
`check-head-family-coverage.py` all still pass.

**Not touched, and worth a decision rather than a silent re-enable:** the
CI job's WinUI-build and UI-test steps are commented out by a deliberate
commit (`91ffc27`, "Disable WinUI build and UI tests in CI") with no stated
reason recorded. Given the suite genuinely runs and passes now, whether to
uncomment those steps is a call for whoever disabled them, not something to
flip back on the strength of one local run.

### 3. Xbox controller — designed, not implemented

Needs `Windows.Gaming.Input` polling; can't be written responsibly without
hardware to test button feel and repeat rate. Keyboard bindings on three of
four gameplay screens are the shipped substitute; the path for a controller to
reuse the same `ICommand`s is designed but unbuilt.

**Closed by design, not a gap:** Millionaire has no accelerators. Its answers
and lifelines are template-generated in an `ItemsControl` (a static `Key="A"`
in a `DataTemplate` binds to every instance), and `WalkAway` is irreversible —
the same reasoning that excludes `Quit` on the main screen.

### 4. Head routing — CLOSED

**Was live and broken.** Six controller shapes existed with nothing connecting
them to what a head could render. MAUI's router fell through `_ =>` to its
card-turn page, which rejects non-`ICardTurnController`; Console's `switch` had
no default. Claimed! and Herd were unplayable in MAUI and did nothing in
Console — and a sweep found Console silently dropping **four** modes, including
Monogamy and Day One, for several versions.

**Fixed.** `ControllerFamily` names the shape a mode produces, dispatching on
the same interfaces in the same order as `ControllerFactory`, with a test
proving they agree across all 97 modes — a descriptor that lies is worse than
none. Both heads route on family, declare supported families as data, and say
plainly when a mode needs a screen they lack.

**Open:** MAUI has no AreaControl or SimultaneousAnswer page; Console lacks
those plus Monogamy/DailyCampaign. Asserted in `HeadFamilyCoverageTests`, so
writing them fails the test until the declaration is updated. Four to six
screens is real work — choose it deliberately.

*Two claims in the paragraph above are now known to be false — see items 10 and
12. Left standing here rather than quietly edited, because the difference
between what this item claimed and what's true is the point.*

**A third nuance, found while closing item 12 and worth the same treatment.**
"Console lacks... Monogamy" isn't quite right either. `ConsoleGameLauncher.RunMode`
special-cases `mode is MonogamyMode` before the family switch runs, and plays it
— but `SupportedFamilies` still declares `[CardTurn, Quiz]`, with no `Monogamy`.
So Console can play *the one built-in Monogamy mode* through a bespoke path that
sits outside the family/coverage system entirely, while its own declared support
list says it can't play Monogamy at all. Not a safety problem — the permissive
direction this backlog generally prefers, same as `ArchetypeFilter`'s default —
but it means the declaration undersells what Console actually does, which is
its own small honesty gap.

**Closed.** Real screens on every head, for every family the catalogue can
produce:

- **WinUI and MAUI** each gained `ClaimedGameViewModel` and `HerdGameViewModel`
  (shared, in `TableTop.Presentation`, same controller-injected-constructor +
  mode-and-players-`Create`-factory shape as `MonogamyGameViewModel`), plus a
  View/Page pair per head, wired into each head's routing switch and
  `SupportedFamilies`. Both new screens use the plain board-game felt palette
  already shared by the default card-turn screen — Claimed! and Herd are "Fun"
  archetype, not couples- or quiz-specific, so no new palette was invented.
- **Console** stopped special-casing Monogamy by concrete type
  (`mode is MonogamyMode` → `mode is IMonogamyDeckProvider`, matching how
  `ControllerFactory` and `ControllerFamilies` already dispatch), and gained
  `ConsoleClaimedRenderer`, `ConsoleHerdRenderer` and `ConsoleDayOneRenderer` —
  the last of which had *zero* prior coverage. `SupportedFamilies` now honestly
  declares all six families, closing the honesty gap named above the same way.
- `HeadFamilyCoverageTests`' two `_CannotYetPlay_` tests — which asserted the
  gap this item existed to close — are gone, replaced with
  `_CanNowPlayEveryModeInTheCatalogue` assertions that the unsupported set is
  empty. A test asserting a known gap has to leave when the gap does, the same
  way `NoHeadSilentlyDropsAFamilyItClaimsToSupport` and `DeckManifestTests`
  did when *their* subjects went.
- **A real bug found by the new `HerdGameViewModel`/`ClaimedGameViewModel`
  tests, not by inspection.** `IHerdController.SubmitAnswers` and
  `IClaimedController.ResolveChallenge` both raise their "what happened" event
  and then advance internal state (next prompt, or the next player's turn)
  *before returning* — the same synchronous-cascade shape that broke WinUI's
  old Monogamy screen (see item 9's history and
  `MonogamyGameViewModel.Submit`'s doc comment). The first cut of
  `ClaimedGameViewModel` read `CurrentPlayerName` from inside the
  `TerritoryClaimed`/`TerritoryStolen`/`ChallengeFailed` handlers — before the
  controller had actually advanced the turn — so the board briefly showed the
  wrong player. `HerdGameViewModel` was written with the hazard in mind from
  the start (its own doc comment names it) and needed no fix; `ClaimedGameViewModel`
  needed one, found by `Challenge_ThenSucceed_ClaimsTheTerritoryAndAdvancesTheTurn`
  failing on the very first run. Worth remembering: knowing about a footgun in
  one screen doesn't mean the next screen written in the same sitting avoids
  it automatically.
- **Not verified against the graphical heads' UI directly** — same limitation
  item 17 recorded for its own WinUI/MAUI wiring: no `dotnet`-driven UI smoke
  test exists for either head. Console's new renderers could not be exercised
  interactively either, for an unrelated reason: `ConsoleUi.Clear()` throws
  `"The handle is invalid"` the moment standard output isn't a real console
  handle, which is every non-interactive way of driving this app (redirected
  to a file, piped through another process). That is a pre-existing property
  of every console renderer, not something this change introduced — the very
  first `Clear()` call in `ConsoleGameLauncher.Run()` hits it before any
  mode-specific code runs. Verified instead by the full engine build under
  `-p:TreatWarningsAsErrors=true`, the 860-test suite (11 of them new,
  including the ordering bug above), and all seven static gates.

### 5. Composition-root DI — CLOSED

All six `new ControllerFactory()` sites accept an injected `IControllerFactory`,
falling back only when none is passed. Proved with a recording spy that
injection takes effect, not just that it compiles.

Also corrected: this backlog once called `SavedSessionLookup`'s "session found"
path *structurally untestable*. It wasn't — it was this bypass, misdiagnosed.

**Was open: heads didn't pass anything.** MAUI registered services but pages
constructed ViewModels directly; WinUI had no composition root. Plumbing in,
wiring out — production took the default path regardless of what either
container had registered.

**Sharpened by review — the MAUI registrations were not merely unused, they
were unresolvable.** `MauiProgram` registered `PlayerSetupPage`, `GameplayPage`,
`PlayerSetupViewModel` and `GameplayViewModel`, and every one of those
constructors took a runtime value the container had no registration for:
`PlayerSetupPage(IGameMode)`, `GameplayPage(IGameMode, List<IPlayer>, …)`.
Resolving any of them threw. So "wire the heads up" needed a
parameterised-resolution seam for the per-session values first, not a bigger
container.

**One diagnosis in this item's own text turned out to be stale, caught while
closing it.** The paragraph that used to sit here — "`AddTableTopHosting`
registers `IControllerFactory` transient... its constructor assigns the
process-wide `JsonDeckLoader.Diagnostics` static unconditionally" — described
pre-1.19.0 code. `ServiceCollectionExtensions.cs`'s own comment at that
registration already says so: *"Until 1.19.0 that comment was false... The
JSON deck path and that static are both gone."* Items 11 and 14 closed it as a
side effect; this backlog paragraph just never caught up. Left here as a
record of the drift, same reason item 4's now-false claims were left standing
rather than quietly edited.

**Closed.** Both heads now have a real composition root reaching the one seam
that actually mattered:

- `CardTurnGameViewModel.CreateAsync` (`TableTop.Presentation`) has always
  taken `IControllerFactory? controllerFactory = null`, falling back to
  `new ControllerFactory()`. Neither head's only call site into it ever passed
  one — a custom `IControllerFactory` registered in either container had
  **zero effect** on a real game session. Same shape for `IAppSettings`:
  `WinUIAppSettings.Instance` / `AppSettings.Instance` were read as hand-picked
  singletons at the same call sites, instead of the interface each constructor
  already declared.
- **WinUI**: `App.xaml.cs` now builds a real `IServiceProvider`
  (`AddTableTopHosting()` + `IAppSettings`) and hands it to `MainWindow`,
  which gives it to `Navigator`. `Navigator` is already threaded through every
  ViewModel as `_navigator`, so `Navigator.Services` is what makes the
  container reachable from the existing chain — no other ViewModel
  constructor needed to change. `GameViewModelFactory.CreateAsync` and the
  `PlayerSetupViewModel` construction site now resolve from it instead of
  defaulting silently.
- **MAUI**: `MauiProgram.cs` now also registers `IAppSettings` (previously
  only the concrete `AppSettings` type was registered), and drops the four
  registrations confirmed unresolvable above rather than leaving them as
  decoration. `GameplayViewModel`'s constructor — the one place MAUI's
  CardTurn path actually builds a controller — resolves `IControllerFactory`
  and `IAppSettings` via `IPlatformApplication.Current!.Services`, the same
  ambient-container idiom `GameSelectionPage.xaml.cs` already used to reach
  `SettingsPage`. No page constructor needed threading a service provider
  through by hand; the idiom was already sitting in the codebase.
- **Deliberately not touched**: Millionaire/Monogamy/DayOne/Claimed/Herd's own
  `Create(...)` factories on both heads construct their concrete controllers
  directly (`new MonogamyController(...)`, `new ClaimedController(...)`, …)
  and never went through `IControllerFactory` at all — a separate,
  pre-existing duplication of `ControllerFactory`'s own dispatch logic, not
  the ignored-seam bug this item closes. Threading a service provider into
  those five page types would have no consumer waiting for it today.
- **Not verified by a passing assertion** — "a container-registered
  `IControllerFactory` is the one actually used" can't be unit-tested in
  `TableTop.Tests`, because `GameViewModelFactory` and MAUI's
  `GameplayViewModel` live in the two UI-head projects that project
  deliberately cannot reference (WinUI needs the Windows SDK, MAUI needs its
  workload — same reasoning `HeadFamilyCoverageTests` documents for keeping
  hand-typed copies instead). Verified by reading the diff and by both heads
  building clean under the full solution build; the same honest gap item 17
  recorded for its own WinUI/MAUI wiring.

### 6. Content loading is fallback-first, not integrity-first — **CLOSED in 1.21.0**

`JsonDeckLoader.LoadOrFallback` swallows any exception and serves compiled
content, so a malformed deck means players silently run stale cards.
`JsonDeckLoader.Diagnostics` is global mutable state any later
`ControllerFactory` can null out process-wide.

Wanted: a typed load result; fail or disable only the affected mode when
supplied content is invalid; a scoped content/diagnostic policy.

**CLOSED in 1.19.0, by removal rather than repair.** `JsonDeckLoader` is gone,
so there is no fallback-first path left to make integrity-first: a built-in
mode's deck is a compiled-in list that cannot be missing or malformed. The
`JsonDeckLoader.Diagnostics` static went with it, and so did the
`ControllerFactory` constructor parameter whose only job was to assign it.

**Fully closed in 1.21.0.** The remaining half was about *user-supplied* content —
a player's own deck file genuinely can be malformed, and `JsonCardProvider`'s
`throwOnValidationError: false` path dropped bad cards silently. That whole
stack is gone. There is no content loading left in the engine, so there is no
fallback-first path to make integrity-first.

Worth being clear that this closes the item by deleting the feature, not by
solving the problem. If content loading ever returns, **the typed load result is
the design decision to make first**, before any loader exists to retrofit it
into. That was the mistake the original path made.

### 7. `GameplayViewModel`'s wrapper pattern has one consumer

MAUI's `GameplayViewModel` (321 lines) is the only per-head wrapper left; every
other merged screen binds the shared class directly. Right call for the
documented reasons (platform `Color`, fonts, live settings), but unvalidated by
comparison. If a second screen ever needs the same shape, check whether the
pattern generalises or whether this one's specifics (WCAG-contrast strip
colours especially) were coincidentally easy to lift.

Not an action item — a note for whoever hits the second case. *(Deleted by
accident in 1.15.0 when the routing item was written into its slot and the edit
spliced to the next `---`. Restored 1.16.1. Rewriting a numbered item in place
is how an unrelated one vanishes.)*

### 8. Build environment — offline path solved, packaging not

The sandbox reset mid-session and came back empty. Source survived only because
it had been shipped as an archive, and recovery revealed that **every build in
this project's history had depended on a NuGet cache nobody had recorded** —
with nuget.org, its CDN and Azure blob all `403 host_not_allowed`, even
`TableTop.Core` couldn't restore its two `Microsoft.Extensions.*` references.

**Solved: `scripts/offline-build.sh`.** Those two assemblies already ship
inside the SDK's ASP.NET Core shared framework, so referencing them by path
removes the restore step entirely. All four engine assemblies build with no
network, and the 27 guards pass against the result. Two traps are documented in
the script because both cost real time:

- Use `shared/…`, not `packs/…/ref/…` — reference assemblies compile fine and
  then fail at runtime with `0x80131058`, so everything looks healthy until a
  test actually loads one.
- Run harnesses from **inside** the repo tree; several guards walk up looking
  for `src/TableTop.Games` and otherwise fail with a misleading
  "could not locate repository root".

**Still open:** the script covers the engine only. `TableTop.Tests` proper
still needs xunit/FluentAssertions/Moq from NuGet, so the ~950-test suite
remains unrunnable offline — the 27 guards run via a stub harness instead. The
UI heads still need their platform SDKs. Vendoring the handful of test packages
would close the rest.

**New floor, worth recording:** the sandbox this review ran in had no `dotnet`
binary at all. `offline-build.sh` assumes an SDK is installed and only the
network is missing. That assumption has now failed once. Nothing needs fixing —
but "offline" and "no toolchain" are different failure modes and only one of
them is covered.

---

## From the 1.18.0 review, and from building 1.19.0

Items 9–16 came from reading the tree at 1.18.0. Items 17–18 came from compiling
and running 1.19.0, and are the first entries here written with a real build
behind them.

### 9. `DeckManifestTests` should be failing on main — 1.18.0 deleted what it guards — **CLOSED in 1.19.0**

The finding: `Every_referenced_deck_file_exists_on_disk` regex-scanned the mode
sources for `LoadOrFallback("x.deck.json", …)`, listed `Data/Json/*.deck.json`,
and asserted every referenced file was present. 1.18.0 deleted all 95 files and
kept all ~94 references, so `missing` was every one of them and the assertion
could not pass. CI runs `dotnet test` on main, so this should have been red. It
went unnoticed because the environment 1.18.0 was authored in cannot execute the
suite — exactly the risk `ARCHITECTURE.md` already names under **Verification,
honestly**, arriving on schedule.

**Resolved by deleting the references**, the first of the three options this item
laid out. 92 `LoadOrFallback` call sites now read their C# bank directly, the
three older `JsonDataPaths.Resolve` sites (Monogamy, Millionaire, School
Millionaire) with them, and `DeckManifestTests` is gone along with the pipeline
it guarded. `DeckContentPipelineTests` and `DeckBulkExporterTests` went too —
both were iterating an empty directory and passing vacuously, which is worse
than red.

**Then it was built and run, which is what actually closed it.** The change was
authored with no `dotnet` binary available, so the five static gates and a grep
sweep were the only checks possible — and, as predicted here, they were not
enough. A real build and test run found six things:

- **One compile error.** `MonogamyMode.GetDeck()` returns
  `MonogamyCardBankExtended.FullDeck`, which lives in `TableTop.Games.Data`,
  but the cleanup script stripped `using TableTop.Games.Data;` from all three
  `JsonDataPaths.Resolve` modes without checking what each still needed. CS0103,
  plus CS1574 on the `<see cref>` in the same doc comment — a warning normally,
  an error under `TreatWarningsAsErrors`. The other two modes survived by luck
  of namespacing: `MillionaireQuestionBank` is declared `namespace TableTop.Games`
  despite sitting in `Data/`, and `Grade6QuestionBank` is in `TableTop.Games.School`.
- **Three test failures of my own making**, all in the rewritten presentation
  tests — see the note under item 18.
- **Two test failures that were not mine**, and are the more interesting half —
  now item 17.

All six are fixed. **The lesson is the one `ARCHITECTURE.md` already states and
this item now demonstrates twice over: a real build run is worth more than every
static check here combined.** Note also that three of the six were logic errors
in test expectations, which a compiler would never have caught either — the
build and the test run are two separate levels of verification, and neither
substitutes for the other.

### 10. Herd's manifest counted a card it never deals — three dispatch orders — **CLOSED in 1.20.0**

The finding: the same capability interfaces were tested in three places in three
different orders — `ControllerFactory.CreateAsync`, `ControllerFamilies.For` and
`ModeManifestExtensions.GetManifest` — with only one pair asserted, and a comment
on `For` claiming the pair "deliberately" matched when Monogamy and Quiz were
transposed. The manifest's order was the one that was live: it tested
`IGameModeDefinition` first, and `HerdMode` is
`BaseGameModeDefinition, IHerdDeckProvider`, so it matched that arm and never
reached the Herd arm below it. Herd's `TotalCards` counted the `"How To Play"`
card that `GetHerdDeck()` strips on purpose, and `SurpriseMe` filters on
`TotalCards`.

**Fixed by collapsing three orders into one.** `ControllerFamilies.TryFor` is now
the single source of truth, its order matches the factory arm for arm, and
`GetManifest` dispatches on its result instead of repeating the chain — so the
manifest can no longer disagree with the factory about which deck a mode plays.
That is structural, not merely tested for.

`For`'s dishonest `_ => CardTurn` fallback is also gone. `TryFor` returns null
for a mode satisfying nothing, and `For` throws `NotSupportedException` with the
same message shape the factory uses, so the two agree on every input rather than
on every input the catalogue happens to contain. `UnsupportedIn` uses `TryFor`
and lists such a mode instead of throwing out of a query whose job is to report
what a head cannot play.

**Four tests pin it**, and two of them supply inputs the catalogue cannot:

- `Herd_ManifestCountsTheDeckItPlays_NotTheCatalogueBehindIt` — the specific bug,
  including an assertion that the two counts genuinely differ so the test can't
  pass vacuously if `GetHerdDeck` ever stops stripping.
- `EveryMode_ManifestTotal_MatchesTheDeckItsFamilyPlays` — the general form,
  across every registered mode.
- `AModeWithTwoCapabilities_ResolvesTheSameWayInAllThreePlaces` — a synthetic mode
  implementing both `IMonogamyDeckProvider` and `IQuestionBankProvider`. **This is
  the input whose absence hid the bug**: the old parity test passed on every mode
  that exists, because none implements two capability interfaces, so a transposed
  order was unfalsifiable. Worth remembering as a pattern — a parity test over a
  catalogue proves parity for the catalogue, not for the rule.
- `TryFor_ReturnsNull_ForAModeNoFactoryCanBuild` and its `UnsupportedIn` sibling.

**Not verified by a build.** Written in the same no-`dotnet` environment as
1.19.0 and 1.20.0. The one place I'd look first is the target-typed switch in
`GetManifest`:
its arms mix `List<ICard>` with `IReadOnlyList<ICard>` and an empty collection
expression, which is why the local is explicitly typed rather than `var`.

### 11. 94 dead deck references — the loader now always missed — **CLOSED in 1.19.0**

The finding: 1.18.0 removed the files and kept every call site, so
`LoadOrFallback` ran across the catalogue and missed every time. Each miss cost
an embedded-resource probe plus a filesystem walk to the root, and fired
`Diagnostics?.DeckFileMissing(fileName)` — a signal whose whole point was that a
mode "can silently run on its C# fallback forever otherwise", now permanently on
for every deck-backed mode and therefore incapable of distinguishing anything.

**All of it removed.** 92 call sites rewritten to call their banks directly;
`JsonDeckLoader`, `JsonDataPaths`, `DeckBulkExporter` and `ModePresentationLoader`
deleted; `IEngineDiagnostics.DeckFileMissing` and `DeckLoadFailed` deleted with
them, since nothing could raise them any more. The two secondary problems this
item named went too: `DeckContentPipelineTests.Every_mode_is_backed_by_a_json_deck`,
which was enforcing that all 94 dead references stay put, and
`AdultJsonDeckTests`' docstring, which claimed players saw the JSON rather than
the bank. Those tests are kept — `GetCards` is still the right path to assert
consent structure against, because filtering, ordering and pinning still happen
there — with the rationale corrected to say why.

**One thing removed here was not in this item's scope, and is worth naming.**
`ControllerFactory`'s `IEngineDiagnostics` constructor parameter existed only to
assign the `JsonDeckLoader.Diagnostics` static, so it went with the static. That
closes half of item 14 as a side effect — see there.

### 12. WinUI is outside head-family coverage, and one coverage test is a tautology — **CLOSED**

`HeadFamilyCoverageTests` covers MAUI and Console. **WinUI has no
`SupportedFamilies` declaration and is not asserted anywhere.** It's the
flagship head. Its routing switch handles CardTurn, Quiz, Monogamy and
DailyCampaign and falls through to `UnsupportedModeViewModel` — the same four
families MAUI declares, discoverable only by reading the switch. That is
precisely the "implicit in the routing switch" state the mechanism was built to
replace, still true for the head that matters most.

`NoHeadSilentlyDropsAFamilyItClaimsToSupport` cannot fail. It filters modes to
those whose family is in `supported`, then asserts those modes' families are in
`supported`:

```csharp
var claimed = AllModes().Where(m => supported.Contains(ControllerFamilies.For(m)));
claimed.Should().OnlyContain(m => supported.Contains(ControllerFamilies.For(m)), …);
```

The predicate is the filter. Its docstring calls it "the actual invariant worth
protecting." Delete it or make it real — a check that reads as the load-bearing
one and is structurally incapable of failing is worse than no check, because it
buys confidence.

Related, and user-visible: `UnsupportedModeViewModel.Message` reads
*"isn't playable on this screen yet — try it in Console."* The only two modes
that reach it are Claimed! (AreaControl) and Herd (SimultaneousAnswer), and
Console declares `[CardTurn, Quiz]`. Every player who sees that message is
being sent to a head that also can't play it. Derive the suggestion from
`SupportedFamilies` or drop the second clause.

Also worth pinning while you're here: the `MauiSupported` / `ConsoleSupported`
arrays in the test are hand-copied from the heads with nothing checking they
still match. Backlog item 4 claims writing a missing page "fails the test until
the declaration is updated" — but the test reads its own copy, so updating the
head alone changes nothing. A `check-*.py` gate parsing the two declarations
out of the head sources would close it, and fits the existing pattern exactly.

**All four findings closed, each the way this item's own text pointed to.**

- **WinUI now declares `SupportedFamilies`** on `GameViewModelFactory` —
  `[CardTurn, Quiz, Monogamy, DailyCampaign]`, the same four its switch always
  handled. `HeadFamilyCoverageTests` gained `WinUiSupported` and
  `WinUi_CannotYetPlay_AreaControlOrSimultaneousAnswer`, mirroring the MAUI
  pair exactly. The flagship head is no longer the one with the least
  coverage.

- **`NoHeadSilentlyDropsAFamilyItClaimsToSupport` is deleted, not repaired.**
  Rewriting it cannot make it real: the "invariant" it wanted to check —
  a head's `SupportedFamilies` actually matches what its routing switch
  handles — needs the head's own source, and `TableTop.Tests` cannot reference
  either graphical head (both need SDKs it doesn't have). Comparing the test's
  copy to itself is tautological no matter how it's phrased, because there is
  no independent ground truth reachable from C# in that project. The removal
  is recorded in place, same as `DeckManifestTests` was when its subject went
  in 1.19.0.

- **The real check is `scripts/check-head-family-coverage.py`**, which is what
  makes the paragraph above true rather than aspirational. It parses the
  `SupportedFamilies` literal out of each head's own source file and diffs it
  against the test's copy — the actual invariant the deleted test's docstring
  claimed to protect, checked where it's actually checkable. Verified it
  catches drift, not just that it passes: adding a family to WinUI's
  declaration without updating the test's copy made it fail with a message
  naming exactly which head and which family; reverting made it pass again.
  Wired into CI's `xaml` job alongside the other five.

- **`UnsupportedModeViewModel.Message` no longer suggests Console.** It read
  *"isn't playable on this screen yet — try it in Console, or check back
  soon"* unconditionally. The only two modes that ever reach this screen are
  Claimed! and Herd, and Console cannot play either — so the suggestion was
  false every time a player saw it. Took the "drop the second clause" option
  this item offered, since deriving the suggestion from Console's declared
  families would need a project reference WinUI has no other reason to carry.
  **Not verified by a running test** — `TableTop.UiTests` is the project that
  could exercise this and doesn't cover `UnsupportedModeViewModel` yet; the
  change is a string-literal removal, reviewed by reading the diff rather than
  by a passing assertion.

**Found while fixing the WinUI message, flagged separately, now also closed.**
`ConsoleGameLauncher`'s own fallback path said *"Try this mode in the Windows
or mobile app instead"* for any mode outside its two supported families — true
for Day One (MAUI plays it), false for Claimed! and Herd (nobody does, yet).
Same shape as the WinUI message, same fix: dropped the second line rather than
deriving it from MAUI's declared families, since that would need a project
reference Console has no other reason to carry.

### 13. The doc counts disagree — **CLOSED**

The finding: README said 91 modes, ARCHITECTURE said 91 in one place and 97 in
another, this backlog said 97, and the tree had 97 registered.
`DocumentationAccuracyTests` counted `class X : … BaseGameModeDefinition` and got
91 — a number wrong in two directions at once. It **included** `JsonGameMode`,
which was the runtime loader and never a catalogue entry, and **excluded** the
seven bespoke provider modes that are: `MillionaireMode`,
`SchoolMillionaireMode`, `ModernLoveMillionaireMode`, `MonogamyMode`,
`DayOneMode`, `ClaimedMode`, `SlangCheckMode`.

**Closed for modes.** Removing `JsonGameMode` in 1.21.0 fixed half of it by
accident — the subclass count fell to 90 and the guard started failing, which is
what forced the rest. The guard now counts what the registry actually holds:

```csharp
ArchetypeRegistry.Default().AllModes.DistinctBy(m => m.Name).Count()
```

`DistinctBy(Name)` matters and is not decoration. A mode filed under two
archetypes is instantiated twice and `AllModes` dedupes by reference, so the raw
count is 102 for 97 distinct modes. All 97 display names are unique — checked,
not assumed. README now says **97 modes**, which is the number a player can
actually reach.

**Closed for cards.** `Readme_card_count_matches_the_tree` sums
`ModeManifest.TotalCards` across `ArchetypeRegistry.Default().AllModes`
(deduped by name, same as the mode guard) rather than regex-scraping C#
initialisers — the brittle proxy the original card guard was removed for in
1.18.0. `GetManifest()` derives from whichever deck the controller for that
mode's family actually gets handed, so this cannot disagree with what a
player can actually deal, the same property item 10 fixed for Herd
specifically.

Turned up a real drift while adding the guard, just not in README: it was
already correct. `ARCHITECTURE.md` said **97 modes, 3,591 cards** — both
stale, since the mode count moved to 99 sometime after item 13 was first
closed for modes and neither document's card figure had ever been checked
by anything. README's **99 modes, 3,657 cards** is what the guard confirms
the tree actually holds; `ARCHITECTURE.md` is corrected to match.

**Now closed for the test count too.** `Readme_test_count_matches_the_assembly`
counts test cases the way `dotnet test` actually reports them — reflecting over
the test assembly for `[Fact]` methods (1 each) and `[Theory]` methods'
`[InlineData]` rows (one per row), rather than trusting a number typed into
prose. `TheoryAttribute` derives from `FactAttribute`, so a Theory method is
checked first or it would double-count as a Fact too. README's stale **776**
is corrected to **863** — the real number, confirmed by the guard itself
failing on CI's actual `dotnet test` run.

Worth recording exactly how, since it's the point of writing the guard as
reflection rather than trusting a hand count. Authored with no local `dotnet`
in this sandbox, so the README figure going into the PR was a static grep-based
estimate of attribute usage in source — 871, arrived at by counting `[Fact]`/
`[Theory]`/`[InlineData]` occurrences and excluding the false positives that
came from this very guard's own doc comment, which mentions those same
attribute names in prose and had inflated a first, naive pass of the count.
CI's real run disagreed anyway — the assertion failure named the actual figure
directly (863, a difference of 8 from the estimate) — and README was corrected
to match in a follow-up push. The eight-count gap between careful static
counting and the real reflected total is itself the argument for why this
guard reflects over the assembly instead of grep-counting attributes: even a
careful static count missed something a real `Type.GetMethods` pass didn't.

Only covers `[InlineData]`: this assembly has no `[MemberData]` or `[ClassData]`
today (checked when the guard was written). If one is added later without
updating the guard, it will silently undercount rather than fail loudly — the
same kind of scope note the mode and card guards above already carry for their
own blind spots.

### 14. Process-wide statics vs. parallel test classes — **half closed in 1.19.0**

`ThreadingAndDiagnosticsTests` and `ControllerThreadingTests` share
`[Collection("ThreadingGuard")]` with a good docstring: `ThreadingGuard.Enabled`
is process-wide, xunit parallelises across test classes by default, and an
overlapping window makes a call one test expects to be silent throw. The same
reasoning applied to two more statics and neither was protected.

**Both are now gone rather than serialised around**, which is the better fix and
was free:

- `JsonDataPaths.DataDirectoryOverride` — was set and restored by five test
  sites across four classes. Save/restore is correct within a class and
  worthless across concurrent ones. The type is deleted; all five sites went
  with the tests that used them.
- `JsonDeckLoader.Diagnostics` — was assigned by **every** `ControllerFactory`
  constructor, including to null when no sink was passed, on a type registered
  transient. Any resolution anywhere could silently clear a sink another host
  had set, and controller factories are constructed in dozens of tests across
  many classes. Both the static and the constructor parameter that fed it are
  deleted.

**Still open:** `ThreadingGuard.Enabled` itself, which is what the existing
`[Collection]` exists for. That one is load-bearing and cannot simply be
deleted — it is a real runtime guard with a real global switch. Nothing about it
changed; it just no longer has company.

If a "flaky on Windows, green locally" report predates 1.19.0, the two deleted
statics are now a plausible past cause rather than a present one.

### 15. `CardTurnController` has 15 lines of raw headroom, and documenting it costs budget — **CLOSED**

Measured then: **685 raw / 700**, **360 code / 390**. The raw backstop would
have bound first, at less than half the headroom.

That inverted the guard's own design. Its docstring says `MaxCodeLines` "is
the one that matters" and that `IsSubstantive` skips comments so "adding
documentation never costs you budget." `IsSubstantive` excludes `//`-prefixed
lines, which catches `///` too — so doc comments are free against *code*, and
counted in full against *raw*. In a file this close to the raw ceiling, writing
the XML docs this codebase rightly insists on is what trips the guard, and it
trips with a message about readability that won't match what the author did.

Two ways to close this: exclude doc comments from the raw count too (a guard
semantics change), or extract now (a code change). **Took the second** — it's
what the guard's own docstring already recommends ("extract, don't raise the
ceiling"), the same pattern that closed this item's two prior occurrences, and
it doesn't touch what "raw" means for every other file this guard could ever
watch.

**The extraction:** `EmitCard`'s three-way `if (card is IBreakCard) … if (card
is IRewardCard) … if (card is IInspirationCard)` block — each arm identical in
shape (call a `SpecialCardCoordinator` handler, record `CardOutcome.Completed`,
return) — collapsed into one `SpecialCardCoordinator.TryHandleSpecialCard`
call. `SpecialCardCoordinator` already owned all three handlers (`HandleBreakCard`,
`HandleRewardCard`, `HandleInspirationCard`) and already dispatched on the same
three types for bonus-card injection (`TryInjectBonus`); `TryHandleSpecialCard`
is the same dispatch, exposed once instead of duplicated as an inline `if`
chain in the controller. Behaviour is unchanged — same three types checked in
the same order, same handlers called, same event raised — only where the
`is` checks live moved.

**Result: 683 raw / 358 code → 668 raw / 349 code.** Real headroom restored
(32 raw, 41 code) rather than headroom borrowed from redefining what counts.

Not verified by a local build (no `dotnet` in this sandbox) — verified by
a brace/paren parity check on both edited files, a grep confirming no other
call site referenced the three handler methods directly (all internal-class
access, no test reaches into `SpecialCardCoordinator` itself), and
`scripts/check-shared-usings.py` passing after adding the `Scoring` using
that `TryHandleSpecialCard`'s `<see cref="CardOutcome.Completed">` doc
comment needed. If CI disagrees, same close-the-loop pattern as the rest of
this backlog.

### 16. `check-ui-compiles.py` dies with a traceback when `dotnet` is absent — **CLOSED**

It called `subprocess.run(["dotnet", …])` unguarded and raised
`FileNotFoundError` with a full Python traceback. Every other gate degrades or
reports cleanly. Its sibling scripts are pure-Python and run in the XAML CI job
precisely because they need no SDK; this one needs one and didn't say so.

1.19.0 raised the stakes slightly: the compile error in item 9 is exactly what
this check exists to catch, and it stayed hidden partly because the check died
noisily enough to be mistaken for the environment rather than the code. This is
a check whose entire purpose is to report a nuanced result honestly — see its
known limitation about unresolvable framework types masking first-party
errors — and it could not distinguish "no toolchain" from "your code is
broken."

**Closed.** A `shutil.which("dotnet") is None` guard at the top of `main()`
now prints `SKIPPED: no .NET SDK on PATH — nothing was verified.` and returns
2 — the same exit code the restore-failure and ambiguous-output branches
already used for "could not verify anything," rather than a bare traceback.
Confirmed the old code actually crashed this way (`subprocess.run(["dotnet",
…])` raising `FileNotFoundError` when `dotnet` isn't on `PATH`) before fixing
it, on a machine with no SDK installed at all — exactly the environment this
item describes, not a hypothetical.

### 17. A player who isn't tagged gets a quietly stripped deck, not an explanation — **CLOSED**

Found by two `CardTurnGameViewModel` tests failing in the 1.19.0 run, but the
tests were the symptom.

`BetweenTheTwoOfYouMode` declares `SuitableFor => TableShape.Couple`, and every
question card in it carries a `CoupleOnlyRestriction`. `TableShape.Of` only
reports `Couple` when **both** players carry the `couple-member` tag. Set that
mode up with two untagged players and nothing objects: the restriction filters
every question card out before the controller sees it, and the game deals the
unrestricted remainder — which for this mode is the handful of "Results" cards.
The player gets a game that starts normally, has no questions in it, and ends
with a summary tallying nothing. No error, no warning, no explanation.

**`SuitableFor` has exactly one consumer: `ArchetypeFilter`**, which filters the
*picker*. Nothing consults it on the way into a game. So the data needed to say
"this one needs you both marked as a couple — want to fix that first?" is
already there, computed, and thrown away at the only moment it would help.

This is not one mode. Ten-plus modes declare `TableShape.Couple`, and the adult
decks additionally gate on `AdultOnlyRestriction`; `Family`, `Team` and `Group`
modes have their own shapes. Any of them can be entered at the wrong shape.

Worth deciding as product, not just as code:

- **Block it** — refuse to start and say why. Safest for the adult decks, where
  an unrestricted card reaching the wrong table is a consent problem, not a UX one.
- **Warn and continue** — say what will be missing, let them play anyway.
- **Offer the fix inline** — the setup screen already knows the players; tagging
  them is one tap.

Whichever, an empty-feeling deck with no explanation is the one option nobody
would choose deliberately. A guard test asserting that a mode entered at a shape
it doesn't suit either refuses or reports would pin it.

**Why the tests only failed now.** They drove the mode with `Player.Create(name)`,
which carries no tags, and asserted a choice card would appear. Whether they ever
passed depends on the era: for as long as the deck came from a `.deck.json`, what
they exercised was that file's content rather than the bank's. The verification
recorded in `TableTop.Games.csproj` before the files were deleted compared *cards*
between the two copies and found no content unique to either side — it did not
compare *restrictions*. That gap is why this surfaced as a test failure two
versions later instead of at the point of deletion, and it is worth remembering
the next time a "verified identical" claim gets written down: name what you
compared, not just that you compared.

The tests now use couple-tagged players, which is what the mode actually
requires. The fix is correct independent of the history.

**Closed by taking the "block it" option.** `TableSuitability.Check(mode, players)`
in `TableTop.Hosting` reads the same two pieces of data this item names as
already existing and already thrown away — `SuitableFor` and
`TableComposition.From` — and returns a result the caller can act on before a
session is built, rather than after it has quietly played nothing.

"Block it" over the other two options because it is the one the item itself
argues for: safest for the adult and couple decks, where an unrestricted card
reaching the wrong table is a consent problem. "Offer the fix inline" would
have been better UX but is a larger, per-head change (each setup screen would
need to let a player retag themselves as part of starting); blocking with a
clear reason was the change that fit the size of the gap.

**Wired into all three heads, not just the shared code**, because the shared
code turned out not to cover all three:

- Console — checked in `ConsoleGameLauncher.Run`, right after mode selection.
  Prints the explanation and loops back to setup.
- WinUI — checked in `PlayerSetupViewModel.StartAsync` (`TableTop.Presentation`,
  shared by both graphical heads), surfaced through the `Error` binding the
  screen already had for roster validation.
- MAUI — **could not** be closed by the shared-VM change alone.
  `PlayerSetupPage.OnStartGameClicked` never calls `StartAsync`; it is the
  code-behind item 5 already flags as bypassing the shared flow to build its
  own navigation. Checked again there, directly, surfaced via `DisplayAlert`.
  Worth remembering next time a fix is aimed at the shared ViewModel: confirm
  every head is actually calling the method being fixed, not just the two that
  usually are.

Four tests pin it in `TableShapeTests.cs`: an unsuitable table is reported
rather than silently accepted, a suitable one has no explanation, a mode that
declares no shape still always suits (the same permissive default
`ArchetypeFilter` uses, checked again here since it's a separate code path),
and a mode wanting more than one shape names all of them in the explanation,
not just the first.

**Not verified against the graphical heads' UI directly** — no `dotnet`-driven
UI smoke test exists for either, and this was authored without one. Console's
path was read end-to-end; WinUI and MAUI's wiring compiles and follows the
existing validation pattern each screen already had, but nobody has clicked
"Start game" with an untagged pair at a Couple-only mode and watched the
message appear.

### 18. `Presentation` is a permanent `None`, and `Resolved*` is a pass-through — **CLOSED**

`ARCHITECTURE.md` points at this item by name; here it is.

`BaseGameModeDefinition.Presentation` now returns `ModePresentation.None`
unconditionally, so `DisplayName`, `DisplayDescription`, `ResolvedCompleteLabel`,
`ResolvedSkipLabel`, `ResolvedMinimumPlayers`, `ResolvedCategoryColours`,
`ResolvedCategoriesPinnedToStart`, `ResolvedCategoriesPinnedToEnd` and `Theme`
all do nothing but return the compiled-in member they were written to override.
Nine members and a whole abstraction, load-bearing for nothing.

They were kept rather than deleted because the API snapshot pins them and three
files bind them: `CardTurnGameViewModel`, MAUI's `GameplayViewModel`, and
`ModeListItem`/`ModeDisplayResolver`. That is a small blast radius — this is a
tractable change, deliberately deferred rather than hard.

`Theme` is the one with a visible consequence: it is now always null, so
`ModeListItem.HasAccent` is false for every mode in the catalogue, and MAUI's
`ModeTheme` always takes its fallback branch. Accent colours that used to come
from deck files are simply gone from the UI. If that is a regression rather than
an acceptable simplification, the fix is to move those palettes into C# — not to
resurrect the loader.

**Three tests exist solely to pin this**, and they are the honest cost of leaving
it half-done: `EveryMode_ResolvesToItsCompiledInValues`,
`SlowBurn_TheModeThatNeverHadAPresentationBlock_IsUnchanged`, and
`HasAccent_IsFalse_ForABaseGameModeDefinition_BecauseNothingCanSupplyAnAccent`.
All three assert that a feature does nothing. Collapsing the members deletes all
three along with the abstraction.

*The three self-inflicted test failures in the 1.19.0 run were all here, and both
are worth recording as failure modes rather than typos.* Two tests in the same
rewritten file asserted opposite things about `UndividedMode` — one that its
theme was null, one that it wasn't — because the old test was carried over
wholesale while a new one was written from the new behaviour; neither could pass.
And `EveryMode_ResolvesToItsCompiledInValues` used `BeSameAs` to compare
`ResolvedCategoryColours` against `CategoryColours`, which fails despite the
member being a literal pass-through: `CategoryColours` is expression-bodied and
allocates a fresh dictionary per call, so no two reads are ever the same
instance. Reference equality is the wrong assertion for a property that computes.

**Closed by deleting the abstraction, not restoring it.** No content source
remains that could ever populate `Presentation`, so there was nothing to make
the pass-throughs a real feature again short of inventing a new one — moving
the palettes into C# was the other option this item offered, but that is
genuinely new work (a per-mode palette registry, threading it through both
heads' theming), not a bug fix. Deleting the dead layer is the change that
fits this item's scope.

`BaseGameModeDefinition.Presentation` and its nine dependants
(`DisplayName`, `DisplayDescription`, `ResolvedCompleteLabel`,
`ResolvedSkipLabel`, `ResolvedMinimumPlayers`, `ResolvedCategoryColours`,
`ResolvedCategoriesPinnedToStart`, `ResolvedCategoriesPinnedToEnd`, `Theme`)
are gone. `ModePresentation` and `ThemePalette` are deleted from
`TableTop.Core` entirely — nothing else referenced either type. The three
bound consumers now read the compiled-in members directly:
`CardTurnGameViewModel` reads `CompleteLabel`/`SkipLabel`/`Name`; MAUI's
`GameplayViewModel` reads `CategoryColours`; `ModeDisplayResolver` (used by
both `ModeListItem` and MAUI's `GameModeItem`) reads `Name`/`Description`.

**A second inert layer came with it, one level downstream and not named in
this item's original text.** `Theme` was the only source `ModeDisplayResolver`
had for an accent hex, so `ModeListItem.Accent`/`HasAccent` and MAUI's
`GameModeItem.Accent`/`AccentColor`/`HasAccent` were already permanently
null/false before this change — the same "kept as a pass-through that can
never fire" shape this item exists to clean up, just one hop further from
`BaseGameModeDefinition`. Removed rather than left as a now-hardcoded `null`:
`ModeListItem`/`GameModeItem` lost their accent members, and
`GameSelectionPage.xaml`'s leading accent stripe (`IsVisible="{Binding
HasAccent}"`, permanently false) is deleted along with the `Grid` column that
held it, rather than kept as permanently-invisible chrome.

MAUI's `ModeTheme.For` no longer pattern-matches on `Theme` — the
palette-overlay branch was unreachable once `Theme` could never be non-null —
so `OverlaidWith` and its private `Background`/`Solid`/`Hex` helpers are
deleted too; `For` is now just the concrete-type fallback switch it always
resolved to in practice.

Two tests deleted outright (`EveryMode_ResolvesToItsCompiledInValues`,
`SlowBurn_TheModeThatNeverHadAPresentationBlock_IsUnchanged`) along with a
third and fourth covering the downstream accent dead-end
(`HasAccent_IsFalse_ForABaseGameModeDefinition_BecauseNothingCanSupplyAnAccent`
and `Constructor_WrapsTheSameResolutionAsModeDisplayResolver`'s accent
assertion, trimmed rather than the whole test deleted). The public API
snapshots (`api/TableTop.Core.api.txt`, `api/TableTop.Games.api.txt`) are
regenerated via `TABLETOP_UPDATE_API=1 dotnet test … --filter
PublicApiSurfaceTests` and committed with this change, per
`PublicApiSurfaceTests`' own instruction.

**Verified by a full build and test run**, not just static analysis: all four
engine assemblies, Console, WinUI (`-p:Platform=x64`) and MAUI (Windows target)
all build with zero errors; the 858-test suite passes in full, including the
regenerated API snapshots; and all seven static gates
(`check-maui-xaml.py`, `check-winui-xaml.py`, `check-xaml-bindings.py`,
`check-mvvm-method-parity.py`, `check-shared-usings.py`,
`check-head-family-coverage.py`, `check-ui-compiles.py`) pass. **Not
verified:** `tests/TableTop.UiTests` could not run in this environment — the
WinUI test host failed to load `Microsoft.TestPlatform.CoreUtilities`
independent of any change here (a `FileNotFoundException` before any test
executes) — so the `GameSelectionPage.xaml` layout change is confirmed to
build and bind correctly (`check-xaml-bindings.py`, `check-maui-xaml.py`) but
not confirmed by a running UI test or by eyes on the actual screen.

### 19. Persistence failures are handled three different ways, none of them visibly — **CLOSED**

**P2.** Verified in the tree:

- `WinUIAppSettings.Save` writes to a temp file, moves it over the target, and
  wraps the pair in `catch (IOException) { /* best-effort — a failed save
  shouldn't crash the app */ }`. It then raises `Changed` regardless. The
  comment is right that crashing is wrong; the problem is what happens instead.
  The UI updates, the in-memory value is correct for the rest of the session,
  and the setting silently reverts on next launch. A disk-full or
  permissions failure is indistinguishable from success.
- **MAUI never goes through `IAppSettings` at all** in the places that matter.
  `App.xaml.cs`, `GameplayViewModel` and `GameSelectionViewModel` all reach for
  the `AppSettings.Instance` singleton directly — nine call sites — so the
  interface that exists precisely to be the boundary is bypassed by the head
  that most needs it. MAUI `Preferences` has its own failure semantics, and
  nothing in the shared layer can observe or test them.
- Console writes player profiles through `_repository` with no failure path
  visible at the call sites.

Two things to decide, and they are separable. First, whether a failed save is
reported to the player at all — for settings, arguably not; for a saved *session*
the player asked to keep, certainly yes, and that path deserves checking
separately. Second, route MAUI's settings reads through the injected
`IAppSettings` rather than the singleton, which is the same change item 5 needs
and probably wants doing with it.

Note the singleton is also why `GameplayViewModel` is hard to test: item 7 calls
that wrapper "unvalidated by comparison", and this is a large part of why.

**Closed, addressing both decisions this item posed rather than picking one.**

- **The saved-session path — "certainly yes" — is fixed.**
  `CardTurnGameViewModel.SaveSession()` was fire-and-forget
  (`_ = _controller!.SaveAsync();`); a write failure became an unobserved
  task exception and the player who explicitly asked to save got no feedback
  at all, worse than any of the three behaviors this item catalogued. It now
  awaits the save and reports a failure through `FlashText` — the same
  channel a successful save already used (`"Session saved"`) — so both
  outcomes go through one visible path instead of only one of them doing so.
- **MAUI's nine `AppSettings.Instance` reads are gone.** `App.xaml.cs`
  resolves `IAppSettings` from its `IServiceProvider`; `GameplayViewModel`
  keeps the `IAppSettings` it already resolved (it was resolving one and
  then reaching past it for the singleton anyway) as a field instead of
  re-fetching the singleton at each of six call sites;
  `GameSelectionViewModel` gained an `IAppSettings` constructor parameter,
  supplied automatically since it's container-registered
  (`AddSingleton<GameSelectionViewModel>()` in `MauiProgram.cs`), which also
  meant de-static-ing the private helper that read the old singleton. A
  custom `IAppSettings` registered in the container now actually reaches
  every read these three files make, not just the one CardTurn path item 5
  already fixed.
- **WinUI's swallow is broadened to match what it already claimed to do.**
  `catch (IOException)` didn't catch `UnauthorizedAccessException` — so a
  permissions failure, the second cause this item names by name, was never
  actually swallowed; it threw straight through the comment claiming
  best-effort handling. Both are now caught. `Changed` still fires
  unconditionally either way, per this item's own "for settings, arguably
  not \[reported\]" — a setting's in-memory value is genuinely correct for
  the rest of the session regardless of whether the write landed, so there
  is nothing false in what `Changed` tells a subscriber.
- **Console's player-profile saves no longer crash the app.** The
  player-initiated save in `ConsolePlayerSetup.Run` now reports success or
  failure via `ConsoleUi`, the same standing as the saved-session fix above.
  The first-run seed write in `ConsoleGameLauncher.SeedDefaultsIfEmpty` is
  internal bookkeeping the player never asked for — same category as a
  settings save — so it's caught and swallowed rather than reported;
  worst case a fresh machine falls through to `CreateNewProfiles()` instead
  of offering Bob and Alice, same as it always does with no repository at
  all.

**Deliberately not touched:** MAUI's `AppSettings` (the `Preferences`-backed
concrete class) gained no new exception handling. `Preferences.Set` is not
file I/O and this item's own text names disk-full and permissions as the
failure modes worth normalising — both are WinUI/Console problems, not ones
this class has. Two more `AppSettings.Instance` sites this item's "nine"
count didn't include — `SettingsPage.xaml.cs` and `PlayerSetupPage.xaml.cs`,
both passing the singleton into a constructor that declares `IAppSettings` —
are left alone; they already hand a real `IAppSettings`-typed instance to a
properly-typed parameter, which is a smaller and different gap than the nine
this item measured.

**Verified with a full build and test run.** All four engine assemblies,
Console, WinUI (`-p:Platform=x64`) and MAUI (Windows target) build with zero
errors; the 861-test suite passes in full (three new
`CardTurnGameViewModelTests`: an `IOException` and an
`UnauthorizedAccessException` from a throwing `IGamePersistence` fake both
report failure through `FlashText` without throwing, and a success case
confirms `"Session saved"` still fires unchanged); all seven static gates
pass, including `check-mvvm-method-parity.py` and
`check-head-family-coverage.py` after the `GameSelectionViewModel`
constructor change. **Not verified:** `tests/TableTop.UiTests` — same
pre-existing WinUI test-host `FileNotFoundException` recorded under item 18,
unrelated to this change.

### 20. Async work is run synchronously on UI threads — **CLOSED**

**P2.** Twelve `.GetAwaiter().GetResult()` call sites, of which these are on or
near a UI thread:

- `ui/TableTop.Maui/ViewModels/GameplayViewModel.cs:285` — controller creation
- `ui/TableTop.Maui/Pages/DayOneGamePage.xaml.cs:18` and
  `MillionaireGamePage.xaml.cs:18` — in page constructors
- `ui/TableTop.WinUI/ViewModels/PickerViewModels.cs:67`

Console's five are fine — it has no synchronisation context and blocking is what
a terminal loop wants. `SessionDeckFactory` and `ServiceCollectionExtensions`
are engine-side and worth a separate look, and `CardTurnController:286` already
carries a comment acknowledging one.

Two costs. The visible one is responsiveness: deck construction and session
resume run on the UI thread, so a large mode janks the transition into a game.
The latent one is worse — blocking on a task that resumes onto a captured
synchronisation context is the classic deadlock, and it hasn't bitten only
because nothing in these paths currently resumes that way. That is a property of
today's implementation, not a guarantee, and it will not announce itself when it
changes.

The fix is async factories and async page initialisation. On MAUI that means
constructors cannot do this work, which is why it is filed as work rather than a
patch: pages need a two-phase construct-then-initialise shape, and that touches
navigation.

**Closed. Line numbers had drifted** (items 18 and 19 both touched
`GameplayViewModel.cs` in the meantime) — re-verified against the current tree
before fixing: `GameplayViewModel.cs:295`, `DayOneGamePage.xaml.cs:17`,
`MillionaireGamePage.xaml.cs:17`, `PickerViewModels.cs:66`. Also re-counted the
"twelve": only **ten** real call sites exist now, not twelve.
`ServiceCollectionExtensions` has none — the blocking DI-resolution-time load
this item pointed at was itself removed in 1.21.0 along with `JsonGameMode`.
`CardTurnController:284`'s comment is now purely historical: it documents a
blocking pattern `CreateAsync` used to have and no longer does (fixed under a
prior item), not a live occurrence. `SessionDeckFactory.cs:63` is real but
reachable only from `CardTurnController`'s public **synchronous** constructor
— no UI code calls it, so it's out of this item's scope (a UI-thread problem)
though still worth a look if that constructor is ever called from a UI
thread. The five Console sites are unchanged and still fine, same reasoning
as before.

- **WinUI, the small fix.** `IntroViewModel.ResumeCommand` was a plain
  `RelayCommand` calling a synchronous `Resume()` that blocked on
  `GameViewModelFactory.CreateAsync`. Converted to `AsyncRelayCommand` — the
  same idiom `PlayerSetupViewModel.StartCommand` already uses for its own
  async build — so `ResumeAsync` genuinely awaits and the command disables
  itself for the duration instead of the dispatcher blocking. No
  `INavigator`/`Navigator` interface change needed: `Navigator.Navigate`
  already takes an already-built `ViewModelBase`, so awaiting the factory
  first and calling `Navigate` with the result was already the pattern
  `GameSelectionViewModel.SelectCommand` used — `IntroViewModel` was simply
  the one place that hadn't adopted it.

- **MAUI, the real work.** Page constructors can't be async and
  `Navigation.PushAsync` never awaits construction, so each of the three
  pages (`GameplayPage`, `DayOneGamePage`, `MillionaireGamePage`) moved to a
  two-phase shape: a cheap constructor that only stores its arguments, plus
  a new `Task InitializeAsync()` (declared on a new
  `IAsyncInitializablePage` interface) that does the actual
  `XxxViewModel.CreateAsync(...)` await and sets `BindingContext`. Callers —
  `PlayerSetupPage`'s family-routing switch and `GameSelectionPage`'s resume
  handler — now do `await page.InitializeAsync();` between constructing the
  page and `Navigation.PushAsync(page)`; both call sites were already inside
  `async void` handlers, so this added no new async boundary, just moved
  where the awaiting happens. `GameplayViewModel` itself gained the same
  shape one level down — a private constructor taking an already-built
  `CardTurnGameViewModel` plus a static `CreateAsync` that does the
  `IAppSettings`/`IControllerFactory` resolution and the actual
  `CardTurnGameViewModel.CreateAsync` await — since `GameplayViewModel`'s own
  constructor was the thing blocking, one level above `GameplayPage`'s.

**Deliberately not touched:** `MonogamyGamePage`/`ClaimedGamePage`/
`HerdGamePage` build their controllers through synchronous factories
(`MonogamyGameViewModel.Create`, etc.) with no `IControllerFactory.CreateAsync`
involved at all — there's no blocking call to remove because there's no async
work being blocked on. Not a template the three fixed pages could have
copied; a genuinely different, simpler shape for modes whose deck comes from
a synchronous provider.

**Verified with a full build and test run.** All four engine assemblies,
Console, WinUI (`-p:Platform=x64`) and MAUI (Windows target) build with zero
errors; the 861-test suite passes unchanged (`GameplayViewModel` and the
three MAUI pages have no existing test coverage to preserve or extend —
confirmed by search before starting); all seven static gates pass,
including `check-mvvm-method-parity.py` and `check-ui-compiles.py` against
the new `IAsyncInitializablePage` interface and the changed constructor
shapes. **Not verified:** the actual UI thread staying responsive during a
controller build — no `dotnet`-driven UI smoke test exists for either
graphical head (the same limitation recorded under items 4, 17 and 18), so
this is confirmed by reading the diff (no `.GetAwaiter().GetResult()`
remains in any of the four call sites, and every caller now awaits
correctly) rather than by measuring a frame that no longer drops.

### 21. Comments describe a head that no longer exists — **CLOSED**

**P2, and the cheapest item here.** Nineteen references to WPF survive its
removal, and they are not all harmless historical notes:

- `ui/TableTop.WinUI/Infrastructure/FilteredArchetypeRegistry.cs:8,10` calls
  itself "the WPF counterpart" and explains a design choice in terms of "WPF's
  picker screens". It **is** the WinUI implementation — the comment describes
  the file it sits in as something else.
- `WinUIAppSettings.cs:18,59` says "WPF has no equivalent of MAUI's…" and "WPF
  currently only ships dark" about WinUI's own behaviour.
- `ViewLocator.cs:11` and `BoolToVisibilityConverter.cs:8` contrast WinUI
  against WPF, which is fine and genuinely explanatory — keep these.

The engine-side ones (`IGameController`, `IHintEngine`, `CardText`,
`BaseGameModeDefinition`) mostly list WPF as an example host; harmless but stale.
`RelayCommand.cs:8` explains why `CommandManager` isn't used and is worth keeping
verbatim.

Separately, `WinUIAppSettings.cs:169` says MAUI "auto-saves at game start", and
`SettingsPage.xaml:265` tells the player "All settings are saved automatically."
Check both against what MAUI's roster actually does now before editing either —
if the comment is wrong the label may be too, and the label is user-facing.

**Rule for this pass:** delete a comment that misdescribes the file it is in;
keep one that contrasts against WPF to explain a decision. The distinction is
whether WPF is the subject or the foil.

**Closed.** `FilteredArchetypeRegistry.cs` no longer calls itself "the WPF
counterpart"; `WinUIAppSettings.cs` no longer describes its own behaviour as
WPF's. The four engine-side files (`IGameController`, `IHintEngine`,
`CardText`, `BaseGameModeDefinition`) had WPF replaced with Console/WinUI/MAUI
in their example-host lists. `CardText.cs`'s claim that "WPF's HtmlTextBlock"
renders the card banks' HTML markup natively was also wrong on its own terms —
traced the actual behaviour and found `CardTurnGameViewModel` strips that
markup unconditionally before either graphical head ever sees it, so no head
renders it; rewritten to say so instead of naming a substitute control that
doesn't exist.

The separately-flagged wrong claim is fixed too: `WinUIAppSettings.cs` said
MAUI "auto-saves at game start" — checked against `PlayerSetupViewModel`
(shared by both heads since the composition-root work), which documents
`SaveRosterAsDefault` as explicit-only, starting a game does not do this in
either head. The comment was simply false; corrected to match, and
`SettingsPage.xaml`'s "All settings are saved automatically" label was checked
against the same behaviour and left alone since it describes settings, not the
roster.

Kept, per this item's own rule, as genuine contrast rather than
self-misdescription: `ViewLocator.cs`, `BoolToVisibilityConverter.cs`,
`RelayCommand.cs`, and the remaining engine-side mentions that list WPF/MAUI/
Console as example UI hosts rather than describing the file's own identity.

### 22. `MillionaireGameViewModelTests` is flaky — it drives a shuffled real deck — **CLOSED**

**P2, and it was genuinely intermittent — measured, not suspected.** Three of the
class's tests answered `vm.Answers[0]` and asserted `vm.IsAnswered` becomes true.
`RealController()` built from `new MillionaireMode().GetQuestionBank()`, the
live bank, and `MillionaireController.BuildQuestionPool` orders by difficulty
then by `Random.Shared.Next()`, so which question reached rung one changed
every run.

Measured on the 1.21.0 tree while adding an unrelated mode: the full suite
failed 2 runs out of 3, and the *failing test within the class differed between
runs* (`AnswerOption_SelectCommand_…` one time, `AnswerOption_Invoke_…` the
next). Run the class alone and it passed 5 for 5 — which is exactly the profile
that gets a failure dismissed as "just a flake" and then hides a real one later.

**The mechanism was one step past where this entry originally stopped, and the
extra step is the interesting part.** "The question changes per run" is true but
not sufficient — it only matters because a *correct* answer does not leave the
question settled. `SubmitAnswer` raises `AnswerCorrect`, whose handler sets
`IsAnswered = true`, and then calls `LoadNextQuestion`, whose `QuestionReady`
handler sets `IsAnswered = false` again for the newly-loaded question. So the
assertion failed precisely when the shuffle happened to put the correct answer
at `Answers[0]`. Measured directly before fixing: the first option was correct
in **22 of 400** constructions, which compounds across three tests and repeated
runs into the observed one-in-three suite failure.

**Fixed with a fixed fixture bank** whose correct answer is always D, so
`Answers[0]` (label A) is reliably wrong. The tests now name the option they
click — `WrongLabel` to settle a question, `CorrectLabel` to advance the ladder
— rather than trusting an index, since `Answers[0]` reading as "the one that
ends the round" was the assumption that broke. `Answer_CorrectOrWrong_SetsIsAnswered`
was split into two honestly-named tests: a wrong answer settles, and a correct
answer advances and *reopens* interaction. That second behaviour was always
intended and was previously only ever observed by accident, in the runs that
failed.

**Verified by repetition, which is the only proof that means anything here:**
25 consecutive runs of the class and 15 consecutive full-suite runs, zero
failures. Before the fix the class failed roughly one run in three.

This was never the parallel-statics problem item 14 describes; both statics
named there are gone. It was a test reaching for real, shuffled content when it
wanted a fixed fixture.

---

## From the 1.28.0 review

Items 23–28 came from reading and running the tree at 1.28.0. Unlike the
1.18.0 and 1.21.0 passes, this one had the full toolchain and a green suite,
so every claim below was checked by executing something rather than inferred
from source. Items 24–26 and half of 27 are faults in code added *during*
1.28.0 — recorded here rather than quietly fixed, because a feature that
shipped with a known gap is worth more as a written-down gap than as a
silent one.

### 23. Two CI jobs report green while building nothing — **CLOSED**

**P1, and the most serious thing in this review.** `build-windows-heads`
("Build WinUI") and `build-maui` ("Build MAUI (Android)") both still exist,
still run on every push, and still report green — with every step that does
real work commented out. What remains:

- **`build-windows-heads`**: checkout, `setup-dotnet`, and then nothing. The
  WinUI build, the `TableTop.UiTests` run and the results upload are all
  commented out.
- **`build-maui`**: checkout, `setup-dotnet`, `dotnet workload install
  maui-android` — and then nothing. It installs a workload it never uses,
  which is the tell: the job takes minutes, looks busy, and compiles no code.

Two more removals in the same commit (`91ffc27`), both easy to miss:

- **Console's build is gone** from `build-and-test` — so of the four
  buildable heads, CI now compiles **none**.
- **`TreatWarningsAsErrors` is gone** from Build Core / Games / Hosting.
  Those steps used to pass `/p:TreatWarningsAsErrors=true
  /p:WarningsAsErrors="1591,1573,1572"`; now they are bare `-c Release`. The
  `lint` job still enforces XML docs on Core and Hosting, so that half
  survives — but Games has no warnings gate at all any more, and nothing
  fails on a new warning anywhere.

`TableTop.Presentation` is also never built by name in any job. It compiles
transitively via `dotnet test`, so this one is covered in practice — noted
only so the next person doesn't "fix" it and think they've achieved
something.

**Why this is worse than having no job at all.** These jobs are branch-visible
green checks that assert "Build WinUI ✓" and "Build MAUI (Android) ✓" while
asserting nothing. That is precisely the shape this backlog already condemns
twice — `NoHeadSilentlyDropsAFamilyItClaimsToSupport`, a test whose predicate
was its own filter, and the `ModeManifestExtensions` dispatch that had
acquired the bug it was written to prevent. Both got the same verdict: *a
check that reads as load-bearing and is structurally incapable of failing is
worse than no check, because it buys confidence.* Same verdict here.

The stakes are not hypothetical. Every head-facing change in 1.28.0 — items
18, 19 and 20, and the whole Roaster feature across both graphical heads —
would have sailed through CI without one line of head code being compiled.
They were verified locally on a Windows machine with the MAUI workload, which
is exactly the environment this repo cannot assume a contributor has, and the
reason these jobs were written in the first place. `ci.yml`'s own surviving
comment still says, of these heads: *"That cost is the point: these heads are
the product."* The file now contradicts itself.

**Decide, don't drift.** The commit that disabled them (`91ffc27`, "Disable
WinUI build and UI tests in CI") records no reason, so the cost that motivated
it is unknown — plausibly runner minutes, plausibly a red build someone needed
to get past. Either is a legitimate reason to turn a job off; neither is a
reason to leave a green no-op standing in its place. Three honest options:

1. **Re-enable them.** `TableTop.UiTests` genuinely runs and passes now
   (item 2, closed in 1.28.0) — the blocker that plausibly justified
   disabling the UI-test step is gone.
2. **Delete the jobs.** If the runner cost is not worth it, say so in
   `ci.yml` and remove them, so nothing claims a guarantee it isn't giving.
3. **Keep them, gated.** `if:` on a label or a path filter, so they run when
   head code changes and are visibly skipped — not falsely green — otherwise.

Anything but the current state, which is option 2's cost with option 1's
appearance.

**Closed by taking option 1 — and re-enabling turned out to require fixing
two real build failures, which is almost certainly why the steps were
commented out in the first place.**

The history matters, because it reads as triage rather than a decision. Four
separate commits turned things off one at a time — `219d44e` ("Simplify build
commands by removing warning options"), `ed1ec93` (Console), `9265df4`
(MAUI Android), `91ffc27` (WinUI + UI tests) — none recording a reason. Each
step was verified locally before being restored, rather than switched back on
and hoped for:

- **MAUI Android could not build at all.** `dotnet build -c Release -f
  net10.0-android` failed with `XA1030: The 'RunAOTCompilation' MSBuild
  property is only supported when trimming is enabled`. The Android SDK
  defaults `RunAOTCompilation` to true for Release; this project
  deliberately sets `PublishTrimmed=false` (the documented "Android release
  safety" block, so the linker cannot strip card types reached reflectively
  by `System.Text.Json`); AOT requires trimming, so the two are mutually
  exclusive and the build dies. Fixed by setting `RunAOTCompilation=false`
  explicitly in `TableTop.Maui.csproj` — the only value consistent with the
  trimming decision already made there. **A job that cannot pass is a job
  someone disables**, so this was very likely the actual cause rather than
  runner cost.
- **The UI-test step's blocker was already gone.** It could not start at all
  (`FileNotFoundException` on `Microsoft.TestPlatform.CoreUtilities` before
  any test ran) until item 2 fixed it in 1.28.0. It now runs and passes 2/2.
- **`TreatWarningsAsErrors` restored** on Core, Games, Hosting and Console.
  All four still build clean under it, so nothing was being hidden — the
  flags had simply been dropped along with everything else.

Every job now does real work, verified locally against the exact commands
CI runs: engine builds with warnings-as-errors, Console with them, the
893-test suite, WinUI `-p:Platform=x64`, `TableTop.UiTests` (2/2), MAUI
Android Release, and the `lint` job's XML-doc completeness checks on Core
and Hosting. `ci.yml` parses as valid YAML with **zero commented-out steps
remaining**, and its own comment — *"That cost is the point: these heads are
the product"* — is true again rather than self-contradictory.

**Not verified, and worth saying plainly:** these ran on one Windows
developer machine with the MAUI workload already installed, not on GitHub's
runners. The Android job in particular installs `maui-android` fresh each
time, and this machine has `android` + `maui-windows` instead — so the
workload-install step itself is the one part of the chain still unproven.
The first CI run after this merge is the real check.

### 24. `RoasterViewModel` is the only shared ViewModel with no tests

**P2.** `TableTop.Presentation/ViewModels` holds ten files. Nine of the ten
type names are referenced by at least one file under `tests/TableTop.Tests` —
`PlayerSetupViewModel` in four, `MillionaireGameViewModel` in two, and even
`ModeListItem` (which is a row type, not strictly a ViewModel) in one. The
tenth, `RoasterViewModel`, appears in none.

It is also the newest (added in 1.28.0) and the one with the most untested
logic per line: template selection resetting the in-progress roster,
`CanAddPlayer` gating on a template's `MaxPlayers`, `SaveBlockedReason`'s
message text, `SaveRoster`'s no-op guard, the `IRosterStore` round-trip. None
of that is exercised anywhere.

`TableTop.Tests` references `TableTop.Presentation` directly and needs no
SDK, so — unlike the WinUI/MAUI wrappers — there is no structural obstacle
here at all. This is a gap of omission, nothing more. Worth pairing with a
fake `IRosterStore` (trivial: two methods) so the save/load path is covered
without touching real storage.

Worth noting what *does* cover it, so the gap isn't overstated:
`TableTop.UiTests`' two reflection-driven tests now sweep
`TableTop.Presentation`, so `RoasterViewModel`'s commands are asserted
non-null and its settable properties asserted to raise `PropertyChanged`.
That is real coverage and it caught nothing — but it is generic, and it
knows nothing about what any of the behaviour above is *supposed* to do.

### 25. Saved rosters are a closed loop — you can build one and never play it — **CLOSED**

**P2, and a product gap rather than a code fault.** `SavedRoster` is written
by the Roaster screen and read by the Roaster screen. A repo-wide search for
consumers finds the ViewModel, the two `IRosterStore` implementations, and
the two views' delete handlers — and nothing else. In particular
`PlayerSetupViewModel`, which is where a game actually gets its roster, has
never heard of it.

So the whole feature terminates in itself: a player picks a template, enters
everyone's name, gender and age, saves it, sees it in the third column — and
then, to actually play, goes to the picker and types all those names in
again. The persistence works; it just isn't wired to the one place it would
save anyone effort.

This was flagged when the feature landed and deliberately not built, because
"load a roster into player setup" is a real navigation and UX decision, not a
detail. It needs deciding:

- **Load from setup** — a "use a saved roster" control on `PlayerSetupPage`
  / `PlayerSetupView` that fills `Players` from a chosen `SavedRoster`. Most
  useful, most work; both heads need it, and `PlayerSetupViewModel` would
  gain an `IRosterStore` dependency.
- **Play from the roster screen** — a "play with this" action per saved
  roster, jumping into mode selection with the roster pre-loaded. Fewer
  touches, but inverts the app's existing mode-then-players flow.
- **Accept it as a planning tool** — decide the feature is for *composing*
  groups, not launching them, and say so on the screen. Cheapest, and
  honest, but then the third column needs to stop looking like a launcher.

Note the adjacent duplication while deciding: `IAppSettings.RecentPlayers`
already remembers the last roster and already pre-fills player setup. A saved
roster and the recent-players list are now two overlapping answers to "don't
make them retype everyone", and only the older one is actually wired up.

**Closed with "Load from setup".** `PlayerSetupViewModel` gained an optional
`IRosterStore` dependency (null-safe — a caller that doesn't supply one, a
test included, gets the exact pre-existing behaviour: no roster picker,
nothing else different) and two new members: `SavedRosters`, populated from
`rosterStore.Load()` at construction, and `LoadRoster(SavedRoster)`, which
replaces `Players` wholesale from the chosen roster — same replace-not-append
shape as `ClearPlayers` and the `RecentPlayers` prefill, so picking a roster
gives a predictable result regardless of what was already typed in. Each
saved roster is wrapped in a new `SavedRosterOption` (`Name`, `Subtitle`,
`LoadCommand` for WinUI's binding, `Invoke()` for MAUI's code-behind) — the
same duality every other per-item option class here already carries
(`MonogamyGameViewModel.ZoneOption`, `ClaimedGameViewModel.TerritoryOption`).
Both heads wire their own `IRosterStore` in at the one place they already
construct `PlayerSetupViewModel` (`GameSelectionViewModel` for WinUI,
`PlayerSetupPage` for MAUI) — no new DI registration, matching how each head
already constructs its own `WinUIRosterStore`/`RosterStore.Instance` for the
Roaster screen itself. MAUI's `PlayerSetupPage.xaml` and WinUI's
`PlayerSetupView.xaml` each gained a row of roster buttons above the
name-entry field, hidden entirely (`HasSavedRosters`) when nothing is saved.

The adjacent duplication noted above is **not** resolved by this — a saved
roster and `RecentPlayers` remain two separate answers to the same problem,
now both reachable from the same screen rather than one of them being
reachable from nowhere. Worth its own item if it becomes confusing in
practice; out of scope for closing the "you can never play it" gap.

Not verified by a local build (no `dotnet`/NuGet access in this sandbox) —
checked by grepping every `new PlayerSetupViewModel(` call site (two: WinUI,
MAUI, both updated) and every XAML file for well-formedness, and by the new
`PlayerSetupViewModelTests` coverage: the store-absent default, the
store-present load, replace-not-append, error/status clearing, team-clearing
on replace, and both halves of the `SavedRosterOption` duality.

### 26. The "Team" roster template promises sides it never assigns

**P2, an honesty gap of exactly the kind this file keeps catching.** The
Roaster's four templates differ in real ways — `MinPlayers`, `MaxPlayers`,
and `TagAsCouple`, which genuinely tags players `couple-member` and is what
makes `TableSuitability` accept a Couple-only mode. Three of the four are
truthful. "Team" is not:

```
new() { Name = "Team", Description = "Two or more players split into sides", MinPlayers = 4 },
```

Nothing splits anyone into sides. `SavedPlayer` carries `Name`, `Gender`,
`Age` and `IsCoupleMember` — there is no team field for a roster to populate.
So "Team" differs from "Friends" by exactly one integer (`MinPlayers` 4 vs 2)
while its description claims a mechanic it does not have.

Teams are not a missing concept elsewhere — they are first-class and real:
`ITeamMode`, `PreferredTeamCount`, `PlayerSetupViewModel.AssignTeams()`,
`Teams.Deal`, `TeamAlternatingPlayerManager`. That is what makes this a gap
rather than a wording quibble: a player who reads "split into sides",
saves a Team roster, and then starts a team mode gets an unassigned table and
no explanation, exactly as if they had picked Friends.

Either give `SavedPlayer` an optional team and have the Team template deal
them (`Teams.Deal` already exists and is tested), or reword the description
to what the template actually does — a bigger group with a higher floor. The
first is better and is entangled with item 25: a roster that carries team
assignments is only worth building if something downstream can consume it.

### 27. Six MAUI `async void` handlers are unguarded — **CLOSED (it was eight)**

**P3.** This codebase already knows the hazard and writes it down in four
places. Three say it verbatim — *"An exception escaping an async void handler
terminates the process on Android; surface it instead"* — above
`GameplayPage.OnEndGameClicked`, `PlayerSetupPage.OnStartGameClicked` and
`SettingsPage.OnResetClicked`, each of which wraps its body in try/catch and
shows a `DisplayAlert`. The fourth, on `GameSelectionPage`'s `_navigating`
field, states the same hazard and adds the trigger: *"Two PushAsync calls in
flight — trivially caused by an impatient double-tap — throw, and an exception
escaping an async void handler terminates the process on Android rather than
being caught anywhere useful."* `GameSelectionPage` and `PlayerSetupPage` both
carry that re-entrancy flag as well as the try/catch.

Six handlers have neither guard:

- `ClaimedGamePage.OnDoneClicked`, `DayOneGamePage.OnDoneClicked`,
  `HerdGamePage.OnDoneClicked`, `MillionaireGamePage.OnDoneClicked`,
  `MonogamyGamePage.OnDoneClicked` — five identical one-liners,
  `async void … => await Navigation.PopToRootAsync();`
- `SettingsPage.OnRoasterClicked` — `async void … => await
  Navigation.PushAsync(new RoasterPage());`, added in 1.28.0, sitting three
  lines above `OnResetClicked`'s try/catch and its explanatory comment.

Honest severity, since P3 is deliberate: `PopToRootAsync` on an already-rooted
stack is the realistic double-tap case and MAUI tolerates it, so the five
`OnDoneClicked` handlers are unlikely to fire in practice. `OnRoasterClicked`
is the one that matches the shape `GameSelectionPage`'s comment was written
about — a `PushAsync` reachable by double-tap, with no `_navigating` flag —
and it is the newest of the six. The reason to fix all six together is not
that each is dangerous; it is that the rule is already written down, already
followed in four places, and a rule followed 4-of-10 times is one nobody can
rely on.

A `check-*.py` gate would fit the existing pattern exactly: flag any `async
void` event handler in a MAUI page whose body is not wrapped in a try/catch.
That is a purely syntactic check, which is what the other six gates are.

**Closed — and the count in this item's own title was wrong.** Writing the
proposed gate first, before fixing anything, is what exposed that: it found
**eight** unguarded handlers, not six. The two the manual read had missed:

- **`GameSelectionPage.OnAppearing`** — the only handler in the app that runs
  with no user action at all, on every appearance of the landing page. Low
  real risk, because `SavedSessionLookup.RefreshAsync` catches everything
  internally, so nothing can currently reach the handler — but that is a
  property of today's implementation rather than a guarantee, the same
  distinction item 20 drew about its deadlock. Guarded, and deliberately
  silent: a resume offer that can't be built is not worth an alert on a
  screen the player just opened.
- **`PlayerSetupPage.OnSaveRosterClicked`** — genuinely exposed.
  `SaveRosterAsDefault` writes through `IAppSettings`, which on MAUI is
  `Preferences`-backed and catches nothing (item 19 left that alone on
  purpose — `Preferences` is not file I/O), and the `DisplayAlert` that
  follows can throw in its own right.

**The five identical `OnDoneClicked` one-liners were fixed by extraction, not
by five try/catch blocks.** `SafeNavigation.SafePopToRootAsync` (a `Page`
extension in `ui/TableTop.Maui/Pages`) owns the guard once and each page
delegates in a single line. Copy-pasting the same ten lines five times would
have restated the rule five times — precisely the "a guard that duplicates
the thing it's checking is the shape that rots" failure this file's own
Guards section closes on. `SettingsPage.OnRoasterClicked` got the fuller
treatment instead, because it is a `PushAsync` reachable by double-tap: a
`_navigating` re-entrancy flag *and* a try/catch, matching
`GameSelectionPage` exactly.

**`scripts/check-maui-async-void.py` is what makes this stay closed**, and it
is wired into CI's `xaml` job alongside the other six. Verified it catches
drift, not merely that it passes: reverting one `OnDoneClicked` to the bare
`await Navigation.PopToRootAsync()` made it fail naming that exact file and
line; restoring it made it pass again. All 16 `async void` handlers across 12
MAUI pages now report guarded.

The lesson generalises past this item, and is the reason the gate was written
before the fix rather than after: **a rule enforced by review gets applied at
the rate people remember it — here, 4 of 12 — and a careful manual audit of
the remainder still missed a quarter of what was left.** The gate found in one
run what two passes of reading did not.

### 28. Console has no Roaster

**P3, the smallest gap here, recorded so it stops being invisible.** The
Roaster shipped in 1.28.0 to two heads. Console has no roster builder, and no
`IRosterStore` implementation — the interface has exactly two, MAUI's
`RosterStore` and `WinUIRosterStore`.

Deliberate in the moment: the parity pass that added WinUI was scoped to the
graphical heads, and Console genuinely is a different surface — it already
has its own player-setup flow in `ConsolePlayerSetup`, backed by
`IPlayerRepository` and `PlayerProfile` rather than `SavedPlayer`. A Console
Roaster is therefore not a port of the shared ViewModel; it is a third
storage shape and a text-mode flow.

Worth doing only if item 25 resolves toward rosters being genuinely useful.
If a saved roster stays a thing you build and never play, adding a third head
that can also build-and-never-play it is not progress. Sequence this after 25,
not before.

### 29. Three controller families bypassed `IControllerFactory` — **CLOSED**

**Critical.** `MonogamyController`, `ClaimedController` and `HerdController`
were each constructed directly with `new` in production code, in four places:
`MonogamyGameViewModel.Create`, `ClaimedGameViewModel.Create`,
`HerdGameViewModel.Create` — all three called from a MAUI page's constructor
— and `ConsoleGameLauncher.RunMode`'s early Monogamy arm. Item 20 recorded the
three `.Create` call sites in passing ("**Deliberately not touched**") for a
different reason — there was no blocking call to remove — without flagging
that they were also the last production paths skipping the factory entirely.
Every controller built this way carried none of what `IControllerFactory`
gives every other family: the `IGamePersistence` a host registered, any
diagnostics sink, and whatever policy a future `ControllerFactory` change
adds. WinUI never had this problem — `GameViewModelFactory` already resolved
every family through the injected `IControllerFactory` and passed the built
controller into these same ViewModels' constructor-injected overload; only
MAUI's `.Create` and Console's Monogamy special case skipped it.

(The finding that prompted this said "five controller families." Only three
actually bypass the factory — Millionaire and Day One already went through
`IControllerFactory.CreateAsync` in their own `CreateAsync` methods, verified
by grep before touching anything. Fixed what the code actually showed rather
than chasing a count the code doesn't support.)

**The fix, in three parts:**

- `IControllerFactory.CreateAsync` gained one new optional parameter,
  `monogamyWinningTokenCount`. Console's Monogamy arm needs the table's
  chosen token target *before* the controller exists, which the interface had
  no way to carry through; every other family ignores it, the same
  honest-scope-boundary treatment `gameplayOptions` already gets from the
  families that don't need it. `ControllerFactory`'s Monogamy arm now reads
  `monogamyWinningTokenCount ?? monogamy.WinningTokenCount` — falling back to
  the mode's own default exactly as the other four capability branches do for
  their own settings.
- `MonogamyGameViewModel.Create` / `ClaimedGameViewModel.Create` /
  `HerdGameViewModel.Create` became `CreateAsync`, matching the shape
  `MillionaireGameViewModel.CreateAsync` and `DayOneGameViewModel.CreateAsync`
  already used: resolve `controllerFactory ?? new ControllerFactory()`, await
  `CreateAsync`, pattern-match the result to the family's controller
  interface, dispose and throw `NotSupportedException` on a mismatch, catch
  that (and the factory's own `NotSupportedException` for a mode with no
  matching capability interface) into the existing `LoadError` path. A side
  effect worth naming: `MonogamyGameViewModel.Create` used to default the
  token target to a literal `10` regardless of what the mode itself declared;
  going through the factory means it now defers to
  `IMonogamyDeckProvider.WinningTokenCount` like WinUI's path always did — a
  latent MAUI/WinUI inconsistency this incidentally closes.
- `MonogamyGamePage` / `ClaimedGamePage` / `HerdGamePage` moved to the same
  two-phase `IAsyncInitializablePage` shape item 20 already established for
  `GameplayPage`/`DayOneGamePage`/`MillionaireGamePage`: a cheap constructor
  that stores the mode and players, and an `InitializeAsync()` that awaits the
  ViewModel's new `CreateAsync` and sets `BindingContext`. `PlayerSetupPage`'s
  routing switch needed no change — it already calls
  `InitializeAsync()` on any page implementing the interface, generically.

Five test call sites (`MonogamyGameViewModelTests`,
`ClaimedGameViewModelTests`, `HerdGameViewModelTests`) moved from `Create` to
`await CreateAsync`; the tests that construct a real controller directly to
drive a ViewModel in isolation (`RealController` helpers in the same files)
are unaffected — that is a test fixture reaching for a known concrete type on
purpose, not a production path guessing at one.

**Not verified by a local build** (no `dotnet` in this sandbox, consistent
with every other item in this file authored the same way) — checked instead
by grepping for every remaining `new MonogamyController(`/`new
ClaimedController(`/`new HerdController(` outside `ControllerFactory.cs` and
test files (none left), confirming no call site anywhere passes
`ControllerFactory.CreateAsync`'s new parameter positionally (every existing
caller either omits it or names `maxRounds`/`gameplayOptions`/`resumeFrom`
explicitly, so inserting a parameter before `ct` cannot have shifted one),
and a brace/paren parity check on every changed file.

### 30. JSON persistence shared a temp filename with no locking, and wrote beside the executable — **CLOSED**

**Critical.** Two separate defects in the same four stores
(`JsonSessionRepository`, `JsonPlayerRepository`, and their WinUI-only
siblings `WinUIAppSettings`, `WinUIRosterStore`, which turned out to share
both bugs verbatim rather than just the first).

**1. Shared `.tmp` filename, no synchronisation.** Every one of the four
wrote to a fixed `{file}.tmp` path — `session.json.tmp`, `players.json.tmp`,
etc. — with nothing stopping two calls from overlapping. Two concurrent
`SaveAsync`s on the same instance (a manual save racing an autosave, two
settings changed in quick succession from different threads) could have the
second call's `File.Create` truncate the first's still-open stream, and
whichever `File.Move` ran second throw because the source it expected had
already been consumed by the first rename. Fixed with two changes together —
either alone is insufficient:

  - A **unique temp filename per call** (`{file}.{Guid.NewGuid():N}.tmp`),
    so two overlapping writes can never target the same path.
  - A **per-instance gate** — `SemaphoreSlim` for the two async repositories,
    a plain `lock` for the two synchronous WinUI stores, since a unique name
    alone still leaves two renames racing for "whichever finishes last wins"
    in an unpredictable order. The gate serialises every call (save, load,
    *and* delete — not just save, so a save's in-flight write is never read
    half-committed by a concurrent caller sharing the same instance) rather
    than only the write path.

  A leftover temp file from a genuine failure (disk full, permissions) is
  now cleaned up in a `catch`/before returning — safe because its name is
  unique to that one call and cannot collide with a future save's.

**2. Beside the executable, not app-data.** All four defaulted to
`AppContext.BaseDirectory` — the install directory. `AddTableTopHosting`'s
own doc comment claimed `%AppData%/TableTop/...` "(or platform equivalent)"
and never did that; nothing passed `sessionFilePath`/`playerFilePath` from
any of the three real hosts, so every one of them silently got the
beside-the-executable default. That default is not writable by a standard
user for an app installed to `Program Files` (or the equivalent), and even
where a location happens to be writable, an app update that replaces the
install directory's contents takes the player's data with it.

`TableTop.Hosting` cannot resolve a real app-data directory itself — it has
no dependency on any platform storage API, which is what keeps it usable
from a console app, a test host, or a future head this project doesn't have
yet. So each host now resolves its own, and passes it explicitly:

  - **Console** (`Program.cs`) — `Environment.SpecialFolder.ApplicationData`
    plus a `TableTop` subfolder, the portable choice for a plain .NET
    console app on any OS.
  - **WinUI** (`App.xaml.cs`) — a new `WinUIAppPaths.DataDirectory`,
    `%LOCALAPPDATA%\TableTop`. Not the WinRT `ApplicationData.Current` API:
    this app ships unpackaged (`WindowsPackageType=None`), and that API
    requires package identity and throws without it.
    `WinUIAppSettings`/`WinUIRosterStore` now default to the same directory
    instead of their own independent `AppContext.BaseDirectory` reads —
    three call sites that used to each answer "where does WinUI's data
    live?" slightly differently now answer it once.
  - **MAUI** (`MauiProgram.cs`) — `FileSystem.AppDataDirectory`, MAUI's own
    sandboxed, always-writable per-platform location. MAUI's `AppSettings`
    and `RosterStore` were never affected by this half of the bug — both
    already used `Preferences`, which handles its own storage — only the
    `IGamePersistence`/`IPlayerRepository` registrations `AddTableTopHosting`
    creates needed a path.

`AddTableTopHosting`'s doc comment is corrected to say what actually happens
now: its own fallback is still beside-the-executable (unchanged, and now
honestly documented as such, since removing it would be a bigger, riskier
change than this item's scope), and every real host overrides it.

Not verified by a local build (no `dotnet`/NuGet access in this sandbox) —
checked by a new `JsonPersistenceConcurrencyTests` (twenty concurrent saves
against a real temp file, on both repositories: no exception, the file left
behind is always exactly one complete write, and no orphaned temp file
survives a clean run), by re-reading each of the three hosts' composition
roots to confirm every one now passes an explicit path, and by a brace/paren
parity check on every changed file.

---

## Guards that must not rot

Each exists because a specific bug shipped. If one fails for a reason unrelated
to what it catches, fix the false positive — don't delete the check.

| Check | Catches | Written after |
|---|---|---|
| `check-maui-xaml.py` | properties that don't exist on a MAUI control | MAUI `Border` has no `CornerRadius` |
| `check-winui-xaml.py` | properties that don't exist in WinUI | `LetterSpacing` shipped; WinUI calls it `CharacterSpacing` |
| `check-shared-usings.py` | shared type used without importing it | same missing-using broke a build three times |
| `check-xaml-bindings.py` | bindings resolving to nothing — silently empty UI | a dropped `StartCommand` binding |
| `check-mvvm-method-parity.py` | MAUI page calling a method a shared VM only exposes as `ICommand` | four call sites broke in one build |
| `check-head-family-coverage.py` | a head's declared `SupportedFamilies` drifting from `HeadFamilyCoverageTests`' copy of it | WinUI shipped with no declaration at all, and the coverage test that should have caught that read only its own copy |
| `check-maui-async-void.py` | an `async void` MAUI handler that awaits without a try/catch — on Android an escaping exception kills the process | the rule was documented above four handlers and applied by hand; eight others never got it, two of which a careful manual count still missed |
| `PublicApiSurfaceTests` | unnoticed breaking change to public surface | written proactively |
| `ModeManifestExtensions` dispatch | a mode's manifest reporting zero cards | `Claimed!` excluded from capped `SurpriseMe` for a version |

All seven `check-*.py` gates pass, verified by actually running them on Windows
with Python 3.12 (matching CI's pin). `check-ui-compiles.py` passes too, on a
machine that has both Python and the .NET SDK — the first time that has been
confirmed rather than assumed.

**Two of them had to be fixed before they could run at all, and the reason is
worth keeping.** `check-maui-xaml.py` and `check-xaml-bindings.py` both did
`rglob("*.xaml")` filtering only `obj`, not `bin`. A local MAUI build leaves a
*directory* named `Microsoft.UI.Xaml` under `bin/`, Windows path matching is
case-insensitive, and `rglob` yields directories as readily as files — so both
gates tried to open a directory and died with a `PermissionError` traceback.
`check-winui-xaml.py` filtered both and was fine; `check-xaml-bindings.py`
filtered both for its `.cs` walk and only `obj` for its XAML walk, in the same
file.

CI never saw any of it, because a fresh checkout has no `bin/`. That is the
general shape to watch for: **a gate that only ever runs on clean CI can carry a
bug that fires for every developer who runs it locally** — and a gate that
crashes on a developer's machine is one they stop running. Both now filter
`bin` and `obj` and require `is_file()`, which also makes them immune to any
other directory that happens to end in `.xaml`.

`DeckManifestTests` used to be in this table. It was removed in 1.19.0 with the
JSON deck pipeline — the correct end for a guard whose subject no longer exists,
as distinct from deleting one that still has a job.

**Two entries in this table used to not be doing their job. Both are fixed now,
and both are worth remembering for the next one.**

`NoHeadSilentlyDropsAFamilyItClaimsToSupport` could not fail — its assertion
filtered by the same predicate it then asserted, so it passed on every input by
construction. Closed in item 12 by deleting it rather than rewriting it: no
phrasing makes a C# test compare a copy to itself and get a real answer. The
`check-head-family-coverage.py` row above is what actually does the job the
deleted test's docstring claimed to.

`ModeManifestExtensions` dispatch was the other. It had acquired the exact bug it
was written to prevent, one interface over — fixed in 1.20.0 by removing the
dispatch rather than repairing it: the manifest now derives from
`ControllerFamilies.TryFor` and has no order of its own to get wrong. That is the
better shape of fix for this table generally. A guard that re-implements a
decision made elsewhere can drift from it; one that reads that decision cannot.

Both fixes land on the same lesson from different angles: a guard that
duplicates the thing it's checking, rather than reading it from a single
source, is the shape that rots. `check-head-family-coverage.py` reads the
head's own declaration instead of trusting a copy; the manifest reads
`ControllerFamilies.TryFor` instead of repeating its dispatch chain.

The pattern is the same each time: the guard was written against the specific
bug that shipped, and the next instance arrived one position along. Worth
holding in mind when writing the fixes — a check pinned to `IClaimedDeckProvider`
by name does not survive `IHerdDeckProvider`.
