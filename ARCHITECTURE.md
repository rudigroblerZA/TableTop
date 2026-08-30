# TableTop — Architecture Review

Current as of **1.35.3**, August 2026. This replaces the accumulated
documentation that used to live in `docs/` — most of it (week-by-week status
reports, a stakeholder presentation, a delivery summary) was stale project
history rather than a description of the system as it stands. This is a
from-scratch account of what's actually here, written to be trusted rather
than archaeologically verified.

## Shape

```
Core ← Games ← Hosting ← Presentation
                              ↑
              Console · Android · WinUI · MAUI
```

Four engine assemblies, four heads. `Core` defines the abstractions (cards,
players, scoring, progression, rules) and has no dependency on anything else
in the solution. `Games` is 101 modes and their decks. `Hosting` is the runtime
— controllers, the archetype registry, persistence, `ControllerFactory`.
`Presentation` is shared ViewModels: plain `net10.0`, no platform SDK
dependency, which is what makes it directly unit-testable without WinUI, MAUI
or the Android workload installed anywhere.

Console, Android, WinUI and MAUI are the four heads. Console is text-only and
builds here. `TableTop.Android` (added 1.32.0) is a native .NET for Android
head — Mono.Android bindings, Activities and view trees built in code, **not**
MAUI — and needs only the `android` workload, which builds on any OS. WinUI and
MAUI need their respective SDKs, which this environment does not have — see
**Verification, honestly** below for what that actually means in practice.

## Content

**101 modes, 3,721 cards, all compiled in.** (README carries the same pair and
is the enforced copy — `DocumentationAccuracyTests` fails when it drifts.
Nothing enforces this line, which is why it sat two releases behind at 99 /
3,657 until backlog X.5. Trust README's numbers over these if they disagree.) Every mode builds its deck from an
in-code card bank, called directly. There is no file lookup, no fallback and no
diagnostic on the path — a mode's deck is a static list in the assembly.

That took two steps. 1.18.0 deleted the 95 `.deck.json` files: of those, only 11
were ever loaded (the rest matched no mode's `DeckResourceName`), and all 11 were
verified card-by-card as identical to their C# bank first, so nothing was lost.
1.19.0 deleted the machinery that had read them — `JsonDeckLoader`,
`JsonDataPaths`, `ModePresentationLoader`, `DeckBulkExporter` — along with the
92 `LoadOrFallback` call sites that had been pointing at files which could no
longer exist. Those had been costing an embedded-resource probe plus a
filesystem walk to the root on every deck build, and firing a "deck file
missing" diagnostic that was by then unconditional and therefore meaningless.
`IEngineDiagnostics.DeckFileMissing` and `DeckLoadFailed` went with them.

**User-supplied content is gone too, as of 1.21.0.** `JsonGameMode`,
`JsonCardProvider`, `TypedDeckLoader`, `CardDeckFile`, `DeckFileValidator`,
`DeckExporter`, the `GameModeFile*` manifest types and `ModeTrust` are all
removed, along with `ArchetypeRegistry.WithJsonModes`, the
`AddJsonGameModes*` DI helpers, and Console's `./modes/` scan. **The catalogue
is now entirely compiled in and the engine reads no content files of any kind.**

`TableTop.Core/Domain/Decks` keeps only what serves compiled modes: `Deck`,
`DeckBuilder`, `InMemoryCardProvider`, `FisherYatesShuffleStrategy`, and
`RestrictionParser` — the last of which survives because `OutLoudMode` parses
restriction strings directly, independent of any file format.

That loose end is closed as of the backlog item 18 fix: `BaseGameModeDefinition`
no longer has a `Presentation` property or any `Resolved*`/`DisplayName`/
`DisplayDescription`/`Theme` member. `ModePresentation` and `ThemePalette` are
deleted from `TableTop.Core` entirely. The three consumers that bound the
pass-throughs (`CardTurnGameViewModel`, MAUI's `GameplayViewModel`,
`ModeDisplayResolver`/`ModeListItem`) now read the compiled-in
`Name`/`Description`/`CompleteLabel`/`SkipLabel`/`CategoryColours` members
directly, and MAUI's `ModeTheme.For` no longer has a palette-overlay branch to
dispatch on.

Archetypes are registered in `ArchetypeRegistry` as a tree; a mode's presence
there is what makes it reachable from a picker at all.

`ModeManifest` is a per-mode summary (`TotalCards`, age rating, etc.). It is
built from whichever deck the mode's controller will actually be handed, and it
finds that deck by asking `ControllerFamilies.TryFor` rather than testing the
capability interfaces itself. **That indirection is the point.** The same
interfaces used to be tested in three places in three different orders — the
factory, `ControllerFamilies.For`, and the manifest — and the manifest's order
put `IGameModeDefinition` first, so Herd (which is a `BaseGameModeDefinition`
*and* an `IHerdDeckProvider`) had its manifest built from the wrong deck and
reported a card it can never deal. Before that, `IClaimedDeckProvider` shipped
with no arm at all and Claimed! reported zero cards for a version, which excluded
it from every capped `SurpriseMe` query.

Adding a capability interface therefore means two edits — the factory arm and the
`TryFor` arm — not four, and the manifest follows automatically.

## Controllers

Most modes run through `CardTurnController`, dispatched by
`ControllerFactory` based on which capability interfaces a mode implements:

| Mode implements | Controller | Progression strategy |
|---|---|---|
| `IGameModeDefinition` only | `CardTurnController` | `DifficultyProgressionStrategy` |
| + `IFlowAwareMode` | `CardTurnController` | `FlowAwareProgressionStrategy` |
| + `IDiceProgressionMode` | `CardTurnController` | `DiceCategoryProgressionStrategy` |
| `IQuestionBankProvider` | `MillionaireController` | bespoke |
| `IMonogamyDeckProvider` | `MonogamyController` | bespoke, dice-driven |
| `IDailyDeckProvider` | `DayOneController` | bespoke, clock-gated |
| `IClaimedDeckProvider` | `ClaimedController` | bespoke, area-control |
| `IHerdDeckProvider` | `HerdController` | bespoke, simultaneous-answer |

`ControllerFamily` names which of the six controller shapes a mode produces,
and is how each head decides which screen to open. Heads declare the families
they render as data (`SupportedFamilies`), and `HeadFamilyCoverageTests`
compares that against the live registry — so a mode no head can play is a
failing test rather than something a player discovers. **All four heads now
render all six families** — this paragraph previously said MAUI had no
AreaControl or SimultaneousAnswer screen and Console lacked four, which was
true when written and had been false for several releases by the time backlog
X.5 caught it. A head is still *allowed* to declare fewer; the point of the
mechanism is that the gap would be a failing test rather than a silent one.

`HerdController` is the first shape with **no active player** — every other
controller asks "whose turn is it?"; this one asks "what did all of you say?".
That's why it's a controller rather than another progression strategy: the
turn-based assumption runs too deep in `CardTurnController` to parameterise
around. Worth knowing when adding a mode: a new capability interface means two
edits, not one — the factory dispatch *and* the `ModeManifestExtensions` arm.
Missing the second is what silently reported zero cards for `Claimed!` and
excluded it from every capped `SurpriseMe` query for a full version.

Cards can carry images. `IThisOrThatCard` presents two options, each with an
optional `ImageKey` — a *logical* asset name, never a path or URL, resolved by
each head against its own asset store. That keeps decks portable across
platforms with incompatible asset conventions, and keeps deck JSON diffable,
which embedded image data would not. An option with no image is fully valid,
so a head that can't render images (Console) degrades to labels rather than
breaking. Adding a card type means touching four places, not one: the DTO
(`CardDeckFile`), the reader (`JsonCardProvider`), the writer (`DeckExporter`)
and the validator's card-type allowlist (`DeckFileValidator`) — miss the
writer and the card silently round-trips as a `StandardCard`.

A mode may additionally implement `ITeamMode`, which switches turn order to
`TeamAlternatingPlayerManager` so the same side never plays twice running.
Team membership lives in `IPlayer.Attributes["team"]` — deliberately not a new
property on `IPlayer`, so no existing implementation, save format or resume
path needed changing. Team scores are derived by summing members rather than
stored, so every existing scoring strategy works untouched. See `Teams`.

Adding a new capability interface means adding a dispatch arm in
`ControllerFactory` — nothing enforces this at compile time, which is why it
has broken silently before (`ModeManifestExtensions` had the identical gap
for `IClaimedDeckProvider`).

## Shared ViewModels — `TableTop.Presentation`

**`TableTop.Presentation` is the intended centre of gravity for UI:**
platform-neutral ViewModels, commands, the settings contract (`IAppSettings`)
and back-navigation (`INavigator`) sit beneath all three heads. Each head then
does the one thing it cannot delegate:

- **WinUI** swaps ViewModels through `Navigator`/`ViewLocator` — no page stack.
- **MAUI** uses a page stack, plus one thin adapter (`GameplayViewModel`) for
  platform `Color`, resolved fonts, and settings that must update live.
- **Android** (`TableTop.Android`, native Mono.Android) swaps `Screen` objects
  over one container with a hand-rolled back-stack, and binds each shared
  ViewModel by hand through a single `render()` callback (`ViewModelBinder`) —
  there is no XAML binding engine. It consumes the shared ViewModels directly,
  with no per-head wrapper.
- **Console** renders controllers directly, with no ViewModel layer at all.

Everything below that line is shared; everything above it is genuinely
platform-shaped. Where a head reaches around the boundary rather than through it
— MAUI's `AppSettings.Instance` singleton, WinUI constructing its own
dependencies — that is a defect, not the design; see `BACKLOG.md`.

Every screen with meaningful logic now has a single, shared implementation,
consumed directly by Monogamy, Millionaire and Day One's pages, and via a
thin per-head wrapper for the two screens that need real platform types:

- `CardTurnGameViewModel` — the main gameplay loop. WinUI consumes it
  directly; MAUI wraps it (`GameplayViewModel`, 321 lines) for platform
  `Color` values, resolved fonts, and settings that must update live.
- `MonogamyGameViewModel`, `MillionaireGameViewModel`, `DayOneGameViewModel`
  — consumed directly by both heads, no wrapper.
- `SettingsViewModel`, `PlayerSetupViewModel` — same, direct consumption.
- `ModeListItem` / `ModeDisplayResolver`, `SavedSessionLookup` — smaller
  shared infrastructure (mode-list display, saved-session lookup on launch).

**This is a completed migration, not an ongoing one.** As of 1.10.4 there is
no meaningful gameplay logic still duplicated between heads. The one thing
that stays deliberately unmerged is the game/mode picker
(`GameSelectionViewModel` / `PickerViewModels.cs`): WinUI drills through four
screens, MAUI cascades three collections on one page. Different enough that
merging them would be a UX redesign, not a refactor.

## Verification, honestly

**Console and the four engine assemblies build and run here.** WinUI and MAUI
do not — no SDK. Anything touching either head is verified by:

1. **Five static gates** (`scripts/check-*.py`): XAML property validity for
   both heads, shared-type usings, XAML binding resolution, and
   `check-mvvm-method-parity.py` — confirms every method a page's code-behind
   calls actually exists on the shared ViewModel it's bound to, rather than
   just a similarly-named `ICommand`. Written after that exact class of bug
   shipped once.
2. **A denylist-based compile check** (`check-ui-compiles.py`) that catches
   first-party ambiguity and duplicate declarations, but cannot detect a
   missing type — the framework itself is unresolvable in this environment
   (no SDK means `UserControl`, `BindableObject`, `Color` all fail to
   resolve), so an unresolved *framework* type in the same expression as a
   first-party error can mask it. When something reports unresolved here,
   the check is: does a definitely-correct, untouched file fail on the exact
   same type? If yes, that's the expected no-SDK limitation, not a real bug.
3. **27 guard tests** (`tests/TableTop.Tests`) — controller size limits, API
   surface snapshots (`api/*.api.txt`), age vocabulary, docs-vs-code accuracy,
   threading, deck title conventions. The deck-content-pipeline guards were
   removed in 1.19.0 with the pipeline they guarded.
4. **A full local test suite** — roughly 900 tests after 1.19.0 removed the
   JSON-deck ones — that compiles here but cannot
   *execute* here — no NuGet access (`NU1301: 403 Forbidden` against
   `api.nuget.org`, confirmed directly, not assumed stale). `dotnet test`
   has been run for real on Windows several times this project's history;
   every one of those runs has found something this environment's static
   checks could not — three real bugs most recently in one pass. **A real
   Windows test run is worth more than every static check here combined.**
   `scripts/measure-coverage.ps1` is the one-command path to a real coverage
   number, mirroring exactly what CI's own coverage step does.

None of this is a substitute for compiling on the real platform. It's the
best available approximation, and its record is mixed on purpose: it has
caught real regressions, and it has also missed real ones that only a
Windows run found. Both facts are worth knowing before trusting a "gates
pass" claim as equivalent to "this works."

## Building without NuGet

`scripts/offline-build.sh` builds all four engine assemblies with no network
access, by referencing the two `Microsoft.Extensions.*` assemblies that already
ship inside the SDK's ASP.NET Core shared framework rather than restoring them
as packages. The 27 guards pass against its output. It covers the engine only —
the full test suite still needs xunit from NuGet, and the UI heads still need
their platform SDKs. Written after a sandbox reset revealed every previous
build had silently depended on a package cache nobody had documented.

## Versioning

Semantic, tracked in `Directory.Build.props`. MAJOR for a breaking change to
Core/Games/Hosting's public surface (guarded by `PublicApiSurfaceTests`
against `api/*.api.txt`); MINOR for new capability; PATCH for fixes.
Assemblies are not published anywhere — a public-surface removal is
therefore MINOR rather than MAJOR, an explicit, narrow carve-out recorded in
`Directory.Build.props` itself, not a silent policy violation.

The last two bumps are both worth reading as worked examples, because both were
initially numbered wrong:

- **1.19.0** removed `JsonDeckLoader`, `JsonDataPaths`, `DeckBulkExporter`,
  `ModePresentationLoader`, `BaseGameModeDefinition.DeckResourceNameForExport`,
  `IEngineDiagnostics.DeckFileMissing`/`DeckLoadFailed`, and
  `ControllerFactory`'s `IEngineDiagnostics` constructor parameter. Removals, so
  MAJOR under a published-package reading — MINOR under the carve-out above,
  since the only consumers are this repo's own heads via `ProjectReference`.
- **1.21.0** removed the entire user-supplied content stack — 15 public types
  across Core and Games, plus `ArchetypeRegistry.WithJsonModes` and the
  `AddJsonGameModes*` helpers. The largest single removal in the 1.x line, and
  MINOR only under the carve-out above; publish a package and this is a 2.0.0.
- **1.20.0** added `ControllerFamilies.TryFor` and changed `For` to throw
  `NotSupportedException` where it used to return `CardTurn`. Added to a static
  class, so MINOR. It was first cut as 1.19.1, which was wrong: a PATCH cannot
  add public surface. `PublicApiSurfaceTests` would have caught the surface
  change but says nothing about which digit moves — that judgement is still
  manual, and it is the step that got skipped.
- **1.22.0** added `TableSuitability` / `TableSuitabilityResult` to Hosting
  (backlog item 17 — a mode is now checked against the table before a session
  is built, instead of starting and silently dealing a restriction-stripped
  deck), and `CartographersMode` / `CartographersCardBank` to Games. Two new
  types in each assembly, no removals and no interface changes, so MINOR on
  both counts of the rule: new capability and a new mode.
- **1.22.1** closed backlog item 12 — WinUI now declares `SupportedFamilies`
  (it never had before), `HeadFamilyCoverageTests` covers it, the tautological
  `NoHeadSilentlyDropsAFamilyItClaimsToSupport` is deleted in favour of
  `scripts/check-head-family-coverage.py` reading the real declarations, and
  `UnsupportedModeViewModel` no longer suggests Console for modes Console also
  cannot play. No new capability and nothing in Core, Games or Hosting's public
  API moved — every change here is a fix, so PATCH under the rule's third case.
- **1.23.0** added `FamilyAtlasMode` / `FamilyAtlasCardBank` to Games, under
  `fun.family` — the family-facing sibling of `CartographersMode`'s
  build-one-shared-page mechanic, minus the `Couple` shape restriction, since
  no card here assumes a headcount or a relationship. Two new types in Games,
  no removals, no interface changes, so MINOR: a new mode, same as 1.22.0's
  precedent for `CartographersMode`.
- **1.24.0** removed `MonogamyCardBankExtended` from Games. It had carried no
  cards of its own since some earlier consolidation — both its members were
  pass-throughs to `MonogamyCardBank.All` — and existed only as a
  backward-compatibility alias for `MonogamyMode.GetDeck()` and Console's
  Monogamy dispatch, which now call `MonogamyCardBank.All` directly. A removal
  that breaks nothing in-tree (its only two callers were fixed in the same
  change), so MINOR under the carve-out above rather than MAJOR.
- **1.25.0** closed backlog item 4 — every head can now play every family in
  the catalogue. `ClaimedGameViewModel` and `HerdGameViewModel` (new,
  Presentation, shared by WinUI and MAUI) plus a View/Page pair per head cover
  AreaControl and SimultaneousAnswer, the two families neither graphical head
  had a screen for; Console gained `ConsoleClaimedRenderer`,
  `ConsoleHerdRenderer` and `ConsoleDayOneRenderer` and now declares all six
  families instead of two. No change to Core, Games or Hosting's public API —
  this is UI-only, new types in Presentation and the three head projects — but
  it's new user-facing capability all the same, so MINOR under the general
  rule rather than left unversioned. `HeadFamilyCoverageTests`' two
  `_CannotYetPlay_` tests are gone with the gap they asserted; see item 4's
  own closure note for the ordering bug the new tests caught in
  `ClaimedGameViewModel` before this shipped.
- **1.25.1** fixed a dead binding in WinUI's Monogamy screen, found while
  reading that screen's pattern to build 1.25.0's new ones. `MonogamyGameView.xaml`
  binds `Command="{Binding SelectCommand}"` on the zone-choice buttons shown
  after a doubles roll; `MonogamyGameViewModel.ZoneOption` never declared a
  `SelectCommand` — only `Zone`, `Display` and a plain `Invoke()` — so the
  binding resolved to nothing and those buttons did nothing on WinUI. MAUI was
  unaffected: its code-behind calls `Invoke()` directly. `check-xaml-bindings.py`
  did not catch it and could not have: it pools every property name declared
  *anywhere* in the codebase into one set rather than resolving per
  DataContext type, and `MillionaireGameViewModel.AnswerOption` declares an
  unrelated `SelectCommand` of its own, which put the name in the pool.
  Fixed by giving `ZoneOption` the same `SelectCommand`/`Invoke()` duality
  `AnswerOption` already has. A PATCH: pure bug fix, no public-API or
  capability change. Confirmed as a real gap, not a false alarm, by removing
  the property and watching the two new regression tests fail to *compile*
  (`ZoneOption` has no `SelectCommand`) — a stronger proof than a runtime
  assertion would have been.
- **1.26.0** formatting and file-organisation cleanup, no behaviour change.
  Added a `.editorconfig` (brace/spacing/naming conventions, closely following
  the .NET runtime's own) and reformatted the codebase to match it — the bulk
  of the diff, and purely mechanical: hand-aligned columns collapsed to a
  single space, brace and spacing style normalised, no logic touched. Verified
  by running the full 862-test suite before and after: identical pass count.
  Alongside that, two deck-builder classes moved out of the miscellaneous
  `Data/` folder and into the mode file that actually uses them —
  `MillionaireQuestionBank` into `Modes/MillionaireMode.cs` and
  `MonogamyCardBank` into what's now `Couples/MonogamyMode.cs` — the same
  Data-folder-namespace mismatch backlog item 9's history already flagged for
  `MillionaireQuestionBank` specifically (`namespace TableTop.Games` despite
  living in `Data/`). `MonogamyMode` itself also moved from `Modes/` to
  `Couples/`, alongside every other couples mode (`CartographersMode`,
  `AfterglowMode`, etc.) instead of sitting apart from them.
  Both moves change `TableTop.Games`'s public namespaces
  (`TableTop.Games.MonogamyMode` → `TableTop.Games.Couples.MonogamyMode`,
  `TableTop.Games.Data.MonogamyCardBank` → `TableTop.Games.Couples.MonogamyCardBank`),
  so `PublicApiSurfaceTests` correctly flagged it; the snapshot update was
  missing from the branch and is included in this merge. A move that breaks
  nothing in-tree (every reference here already compiled against the new
  location) is MINOR under the removal carve-out above, same reasoning as
  1.24.0's `MonogamyCardBankExtended` removal.
- **1.27.0** closed backlog item 5: gave WinUI and MAUI a real composition
  root instead of ignored/unresolvable DI registrations. WinUI had none at
  all — `App.xaml.cs` now builds an `IServiceProvider`
  (`AddTableTopHosting()` plus `IAppSettings` bound to `WinUIAppSettings.Instance`)
  and threads it through `MainWindow` into `Navigator`, which exposes it as
  `Services`; `GameViewModelFactory.CreateAsync` and the
  `PlayerSetupViewModel` construction in `PickerViewModels.cs` now resolve
  `IControllerFactory`/`IAppSettings` from there instead of defaulting to
  `new ControllerFactory()` or reading `WinUIAppSettings.Instance` directly.
  MAUI already had a container, but `MauiProgram.cs` registered
  `PlayerSetupPage`, `GameplayPage`, `PlayerSetupViewModel` and
  `GameplayViewModel` — all four provably unresolvable, since their
  constructors need a per-session `IGameMode`/`List<IPlayer>` nothing
  registers. Those four registrations are gone; `IAppSettings` is registered
  against the existing `AppSettings` singleton instead, and MAUI's
  `GameplayViewModel` now reaches the container through the ambient
  `IPlatformApplication.Current!.Services` handle its call site already sat
  inside, rather than the plan's original sketch of threading a fifth
  `IServiceProvider` constructor parameter through four types — same
  outcome, smaller diff, one fewer thing for every future page in that chain
  to remember to pass along. Both heads' only path into
  `CardTurnGameViewModel.CreateAsync` — the shared seam this whole item
  turned on — now passes a container-resolved `IControllerFactory`, so a
  registration overriding it actually takes effect on a real session for the
  first time. Along the way, closed a stale claim in item 5's own text: the
  `JsonDeckLoader.Diagnostics` static-assignment concern it described was
  already fixed by items 11 and 14, per `ServiceCollectionExtensions.cs`'s
  own comment. MINOR: new capability (a container override now genuinely
  reaches a real session), same reasoning class as 1.25.0's UI-only bump.
  Deliberately out of scope: Millionaire/Monogamy/DayOne/Claimed/Herd's own
  `Create(...)` factories still build their controllers directly, bypassing
  `IControllerFactory` entirely — a separate, pre-existing duplication this
  item didn't touch — and the other `AppSettings.Instance`/
  `WinUIAppSettings.Instance` reads scattered elsewhere are item 19's
  territory, not this one's. This exact behavior — a custom
  `IControllerFactory` being the one actually used — can't be asserted from
  `TableTop.Tests`, since it lives in the two UI-head projects that suite
  deliberately never references; verified by reading the diff and by both
  heads building clean, the same honest gap item 17 recorded for its own
  WinUI/MAUI wiring.

**Backlog item 19, closed.** Persistence failures were handled three
different ways with nothing consistent about any of them: WinUI's settings
save swallowed `IOException` only (a permissions failure threw
`UnauthorizedAccessException` straight through, contradicting its own
"best-effort" comment), MAUI bypassed `IAppSettings` for nine reads across
`App.xaml.cs`, `GameplayViewModel` and `GameSelectionViewModel` so a
container-registered settings implementation had no effect there, a saved
*session* — the one persistence path the player explicitly asks for — was
fire-and-forget (`_ = _controller!.SaveAsync();`) with a write failure
becoming a silently unobserved task exception, and Console's player-profile
save had no catch at all, so the same failure crashed the whole app instead.
Fixed: WinUI's catch now covers both real causes; the nine MAUI reads resolve
`IAppSettings` from the container instead of `AppSettings.Instance`;
`CardTurnGameViewModel.SaveSession` awaits the save and reports a failure
through `FlashText`, the same channel a successful save already used;
Console's two player-profile save sites catch and report instead of
crashing (a first-run seed failure is swallowed, matching item 19's own
"for settings, arguably not \[reported\]" allowance for internal
bookkeeping the player didn't ask for). Verified by a full build of all
three heads, the 861-test suite, and all static gates — including two new
`CardTurnGameViewModelTests` pinning the save-failure report and one
confirming the success path is unchanged.

**Backlog item 20, closed.** Four UI-thread call sites blocked on async
controller construction via `.GetAwaiter().GetResult()`: MAUI's
`GameplayViewModel` constructor and the `DayOneGamePage`/`MillionaireGamePage`
constructors, and WinUI's `IntroViewModel.Resume()`. WinUI converted
`ResumeCommand` from `RelayCommand` to `AsyncRelayCommand` — the same idiom
`PlayerSetupViewModel.StartCommand` already used — so the factory is
genuinely awaited instead of blocked on. MAUI's three pages gained a
two-phase shape: a cheap constructor plus a `Task InitializeAsync()`
(declared on a new `IAsyncInitializablePage` interface in
`ui/TableTop.Maui/Pages`) that does the actual `CreateAsync` await and sets
`BindingContext`; `PlayerSetupPage`'s routing switch and
`GameSelectionPage`'s resume handler now `await page.InitializeAsync();`
between constructing a page and pushing it. `GameplayViewModel` itself
gained the same construct/`CreateAsync` split one level down, since its own
constructor was the thing blocking `GameplayPage`. Monogamy/Claimed/Herd's
pages were untouched — they build through synchronous factories with no
async work to block on, a different shape rather than a template to copy.

- **1.28.0** closed backlog items 2, 13, 15, 16, 18, 19, 20 and 21 — see each
  item's own section in `BACKLOG.md` for the full account; items 18–20 are
  also narrated above. The headline of the batch is item 2:
  `tests/TableTop.UiTests` had apparently never actually run to a real result
  anywhere (its CI job has the WinUI-build and UI-test steps commented out),
  and turned out to be three real bugs deep — a test host that couldn't
  start, a reflection lookup broken by a newer BCL overload, and a null
  default crashing two ViewModel constructors — plus a structural gap where
  the test scanned an assembly (`TableTop.WinUI`) with nothing mutable in it,
  when every settable ViewModel lives in `TableTop.Presentation` instead.
  Alongside the fixes: **Roaster**, a new three-column roster builder
  (templates → configure → save) reachable from MAUI's Settings screen and
  WinUI's Intro screen. `RoasterViewModel`/`RoasterTemplate`/`SavedRoster`/
  `IRosterStore` live in `TableTop.Presentation` — the same call this project
  makes for every screen with no platform-specific values to carry — with
  each head supplying its own `IRosterStore` (MAUI's existing
  Preferences-backed store, and a new `WinUIRosterStore` mirroring
  `WinUIAppSettings`'s local-JSON-file pattern). WinUI's entry point sits on
  Intro rather than Settings: `SettingsViewModel` is shared and holds only
  `INavigator` (`GoBack()` only, by design, so MAUI can still construct it),
  and opening an arbitrary new screen needs the concrete
  `Navigator.Navigate(ViewModelBase)`, which only WinUI-local ViewModels like
  `IntroViewModel` hold. MINOR: new capability (Roaster), plus fixes and a
  test-infrastructure repair — nothing removed from the public surface of
  Core, Games or Hosting.
- **1.29.0** added two game modes and closed backlog item 27.

  **Dice Night** (`fun.family.dicenight`, AllAges) reuses the generic
  `IDiceProgressionMode` mechanic that Roll With It proved, rather than
  inventing a second one: two dice, the total picks one of five categories,
  doubles let the roller choose. It exists because Roll With It sits at a
  Teen floor and `fun.family` had no dice-driven option at all. Implementing
  the interface is the entire integration surface — `ControllerFactory`
  already dispatches it and every head already renders any card-turn mode.

  **Math 24** (`classroom.math24`, AllAges) is four numbers, each used once,
  combined with + − × ÷ and brackets to make 24. Worth recording *how* its
  deck was built, because it is a different standard from every other mode
  here: the puzzles are **machine-verified, not proofread**. An exhaustive
  solver over all parenthesisations, in exact rational arithmetic (floats
  lose `6 ÷ (1 − 3∕4) = 24` to rounding), generated the deck and rejected
  two quads that looked obviously fine while hand-picking — `4 4 6 6` and
  `1 1 8 12` — which have no solution at all. Difficulty tiers are the
  solver's solution count rather than an author's impression, and every
  `Legendary` card is one where *every* route passes through a non-integer
  intermediate. `Math24ModeTests` re-solves all 34 puzzles inside the suite
  using a second, independent solver implementation — a generator checked
  against itself proves nothing — and that check was confirmed non-vacuous
  by planting a known-unsolvable quad and watching it fail by name.

  **Item 27** (unguarded `async void` MAUI handlers) closed, and its own
  count was wrong: writing the proposed gate *before* fixing anything found
  **eight**, not the six counted by reading. `SafeNavigation
  .SafePopToRootAsync` owns the shared guard once rather than five pasted
  try/catch blocks, and `scripts/check-async-void.py` — the seventh gate —
  keeps it closed. (It was `check-maui-async-void.py` and scanned MAUI alone
  until backlog N.4 widened it to the native Android head too.) MINOR: two
  new capabilities, no public-surface removals.
- **1.29.1** SonarCloud code-smell cleanups, no behaviour change:
  `TeamPlayerManager.ApplyScore`'s nested `if` merged into one condition
  (S1066); an unused private `WrapText` helper and an unused `players`
  parameter dropped from `GetToKnowYouMode` and `WouldYouRatherMode`
  (S1144/S1172); `CardTurnController`'s eight extracted-service fields marked
  `readonly` (S2933), each having exactly one assignment.

  Recorded because the branch did not compile when it was picked up, and two
  of the three repairs were the silent-looking kind. The `BuildBuiltInCards`
  call sites had dropped their argument while both signatures still declared
  the parameter (2 × `CS7036`). `CardTurnController._diagnostics` had been
  commented out but is live and referenced eight times (8 × `CS0103`, which
  also broke `TableTop.Console`) — restored. And `_difficultyHistory`, which
  *is* genuinely dead, had been commented out rather than removed;
  commented-out code is itself a Sonar smell (S125), so leaving it that way
  in a Sonar-cleanup branch would trade one finding for another. Deleted
  outright. PATCH: fixes only, no public surface touched.
- **1.29.2** finished the category-literal extraction PR #14 began, on the
  two banks it missed: `ClaimedCardBank` (five territories × six cards) and
  `DayOneCardBank` (three phases × seven days) now name their categories as
  `internal const string` rather than repeating the literal once per card.
  Day One's are `SparkPhase`/`WarmthPhase`/`EmbersPhase` rather than
  `*Category`, matching that mode's own vocabulary.

  Worth more care than a typical extraction because `ClaimedCardBank` derives
  each card's `Guid` by hashing `"claimed|{territory}|{title}|{body}"` — a
  category string off by one character would silently renumber the whole deck
  and make already-seen cards look new across a save/resume, with no error
  anywhere. Every constant holds byte-for-byte the literal it replaced.
  `ScienceSprintMode` and `SoundAndSongMode` were checked at the same time and
  needed nothing: PR #14 had already converted both fully. PATCH: refactor
  only, no behaviour and no public surface change.
- **1.29.3** closed backlog item 23: CI was compiling none of the four heads
  while two jobs — "Build WinUI" and "Build MAUI (Android)" — reported green
  with every real step commented out, the latter installing a MAUI workload
  it then never used. Console's build and the engine `TreatWarningsAsErrors`
  flags had gone the same way across three earlier commits.

  Re-enabling them required fixing a real break first, which is likely why
  they were commented out rather than repaired: **MAUI Android could not
  build at all.** `dotnet build -c Release -f net10.0-android` died with
  `XA1030` — the Android SDK defaults `RunAOTCompilation` to true for
  Release, this project deliberately sets `PublishTrimmed=false` so the
  linker cannot strip card types reached reflectively by `System.Text.Json`,
  and AOT requires trimming. Fixed by setting `RunAOTCompilation=false`
  explicitly, the only value consistent with the trimming decision that
  property group already documents. The UI-test step's own blocker was
  already gone — item 2 fixed the test host in 1.28.0.

  Merged against concurrent work on `main` that had independently restored
  the WinUI build step and added a commented-out `Publish WinUI` placeholder.
  Both intents kept: the UI tests are enabled (their blocker is fixed and
  they pass 2/2), and the packaging step stays commented as work in progress
  rather than being treated as a switched-off step. PATCH: CI configuration
  and one build property, no behaviour and no public surface change.
- **1.30.0** released the four backlog items that had accumulated above 1.29.3
  — 25, 29, 30 and 31 — plus a card-bank refactor that changes nothing at
  runtime.

  **What earns the MINOR.** Item 31 added `SerializedCardTurnController`, a
  new public type in Hosting: `CardTurnController` is documented
  single-threaded and asserts it through `ThreadingGuard`, but `Enabled`
  defaults false in Release — the configuration CI and every release build
  use — so the contract was enforced only where nobody ships. The adapter
  serialises every call through a reentrant lock and `ControllerFactory`'s
  CardTurn arm returns it, so the guarantee holds unconditionally. Item 25 is
  the other half of the case: `PlayerSetupViewModel` gained an optional
  `IRosterStore`, `SavedRosters` and `LoadRoster`, so a saved roster can
  finally start a game instead of only existing on the Roaster screen. New
  capability on both counts.

  **The judgement call, recorded because the rule points two ways.** Item 29
  added an optional `monogamyWinningTokenCount` parameter to
  `IControllerFactory.CreateAsync` so Console's Monogamy flow — which needs
  the table's token target before the controller exists — could stop calling
  `new MonogamyController(...)` and go through the factory like every other
  family. That is a change to an interface member's signature, and the
  INTERFACE row in `Directory.Build.props` reads MAJOR unless the addition
  carries a default implementation; an optional parameter is a default
  *argument*, which helps callers and does nothing for implementers. Taken as
  MINOR anyway, on the same reasoning as the removal carve-out one row above
  it: `ControllerFactory` is the only implementer in the tree, nothing is
  published, no `PackageReference` to these assemblies exists anywhere, and
  every existing caller compiles unchanged because the parameter is optional
  and was inserted before `ct` rather than at the end. The precedent points
  the same way — `ICardTurnController` gained `Players` with a default
  implementation explicitly rejected, and that shipped as 1.27.0, not 2.0.0.
  Publish a package and this reverts to MAJOR, exactly as the removal row
  says of itself. The narrow reading is defensible and was considered; this
  is the deliberate answer, not an oversight.

  **Also in it, neither affecting the digit.** Item 30 fixed two defects in
  four JSON stores: a fixed `.tmp` filename with no synchronisation (now a
  per-call GUID name plus a per-instance gate) and a default write location
  beside the executable rather than app-data (each of the three hosts now
  resolves and passes its own). Item 29's other two thirds routed Monogamy,
  Claimed! and Herd through `IControllerFactory` and gave their MAUI pages the
  two-phase `IAsyncInitializablePage` shape. Item 31 also changed
  `UseSeededRandom` from `AddSingleton` to `AddScoped`, matching the comment
  that had sat above the wrong line since both existed. Four card banks
  (`FactOrFiction`, `SpellingBee`, `CouplesQuestions`, `RelationshipDares`)
  gained default parameters on their private card helpers — 208 call sites
  rewritten, every card's resolved fields verified byte-identical, no public
  surface and no behaviour touched.

  **Verification, honestly.** None of the work above 1.29.3 was built or
  tested: that sandbox had no `dotnet` and no NuGet, so each closed item's
  own note in `BACKLOG.md` says what was checked instead. The seven
  `check-*.py` gates were run and pass; `check-ui-compiles.py` could not be.
- **1.32.0** added a fourth head: **`TableTop.Android`**, a native .NET for
  Android app. Not MAUI — it references the raw Mono.Android bindings and
  builds its screens as Activity + view trees in code, driven by the same
  `TableTop.Presentation` ViewModels WinUI and MAUI already share. A single
  `MainActivity` swaps lightweight `Screen` objects over one `FrameLayout`
  with a hand-rolled back-stack (the analog of WinUI's ViewModel-swapping, no
  page stack); `StackNavigator` implements `INavigator`, `AndroidAppSettings`
  implements `IAppSettings` over `SharedPreferences` (a near-line-for-line port
  of MAUI's `AppSettings`), and `AndroidRosterStore` implements `IRosterStore`.
  `GameScreenFactory` is the Android mirror of WinUI's `GameViewModelFactory`,
  including its `SupportedFamilies` declaration — the head shipped at **full
  parity**, a screen for all six `ControllerFamily` values, so
  `HeadFamilyCoverageTests` gets an `AndroidSupported` array and a
  `_CanNowPlay_` test alongside the other three heads, and
  `scripts/check-head-family-coverage.py` reads a fourth source. The head
  needs only the `android` workload (builds on any OS); its Release config
  carries the same `PublishTrimmed=false` / `RunAOTCompilation=false`
  reflection-safety pair `TableTop.Maui.csproj` documents (backlog item 23).
  MINOR: new capability, no change to Core/Games/Hosting's public surface —
  same reasoning class as the UI-only bumps 1.25.0 and 1.27.0. Built and
  verified here (`-f net10.0-android`, Debug and Release); the engine suite
  and all `check-*.py` gates pass. Not exercised on a device or emulator —
  the same honest gap the other graphical heads carry.
- **1.33.0** closed backlog items 24 and 26. **26** made the Roaster's "Team"
  template real: `SavedPlayer` gained an optional `Team` (5th positional
  param, defaults null — every existing call site and every pre-1.33.0
  saved-roster JSON reads back unchanged), `RoasterViewModel.SaveRoster` deals
  sides through the already-tested `Teams.Deal` when the template asks for it,
  and `PlayerSetupViewModel.LoadRoster` restores those assignments so a saved
  Team roster starts a team mode with sides instead of the unassigned table
  the item described. **24** was pure test omission —
  `RoasterViewModelTests` (18 cases) now covers `RoasterViewModel`, the last
  shared ViewModel without its own test file, using the `FakeRosterStore`
  double that already existed. MINOR: new capability (team-carrying rosters),
  no Core/Games/Hosting public-surface change — `SavedPlayer` lives in
  `TableTop.Presentation`, which `api/*.api.txt` does not track.
- **1.34.0** closed backlog items 28 and 32. **28** gave the non-graphical
  heads a roster: `RosterProfile` / `IRosterRepository` / `JsonRosterRepository`
  in `TableTop.Hosting` — a `PlayerProfile`-shaped sibling of
  `IPlayerRepository` (same `SemaphoreSlim` + unique-temp-file + atomic-replace
  pattern), so it's unit-tested (`RosterRepositoryTests`) and Console needs no
  `TableTop.Presentation` reference. `AddTableTopHosting` gained an optional
  `rosterFilePath`; `ConsoleRoster` is the text-mode build/load/delete flow,
  wired into `ConsolePlayerSetup`. This deliberately does **not** converge with
  the graphical heads' `SavedRoster`/`SavedPlayer` — the two roster shapes
  answering the same question differently is a real open question, not a gap.
  **32** was doc-only: `SerializedCardTurnController`'s docstring claimed the
  lock covers "the full duration of a call", which is untrue of `SaveAsync`
  (the post-`await` `SessionSavedEvent` raise escapes the synchronous
  `Invoke`); the type docstring and a `SaveAsync` remark now state exactly
  what is and isn't gated. MINOR: new public surface in Hosting
  (`api/TableTop.Hosting.api.txt` regenerated in the same commit), same rule as
  item 29's optional-parameter addition in 1.30.0.
- **1.35.0** added Android TV support to both Android-producing heads —
  `TableTop.Android` (native) and `TableTop.Maui`'s Android APK. Each
  `AndroidManifest.xml` now declares `android.software.leanback` and
  `android.hardware.touchscreen` as **not required** (the pair Google Play
  gates TV listing on, and the reason the same APK still installs unchanged on
  phones and tablets) plus an `android:banner` pointing at a new 320x180
  `drawable/banner.xml` — the app-icon hexagon re-centred for 16:9. Each
  `MainActivity` gained a second `[IntentFilter]` carrying the
  `LEANBACK_LAUNCHER` category alongside the `MainLauncher`-generated
  `MAIN`/`LAUNCHER` one, so the app appears on the TV home row without
  gaining a duplicate icon on touch launchers. The native head, which draws
  its own view trees, also does the 10-foot work the platform can't infer:
  `MainActivity` detects a leanback device via
  `PackageManager.HasSystemFeature(FeatureLeanback)` and, only then, insets
  the whole UI by ~27dp against TV overscan, gives the top-bar Back button the
  brass-flip `button_background` (a bare button draws no focus state), and
  calls `RequestFocus()` on each screen's view as it is shown so the D-pad
  has a starting point; `button_background.xml` / `button_text.xml` gained a
  `state_focused` arm (brass fill, parchment ring, ink text) mirroring
  `state_pressed`. The MAUI head needs none of that — its AppCompat views are
  D-pad focusable out of the box. MINOR: new user-facing capability, no
  change to Core/Games/Hosting's public surface — same class as the UI-only
  bumps 1.25.0, 1.27.0 and 1.32.0. Verified by reading the diff and the
  `check-*.py` gates; not exercised on a TV device or emulator, the same
  honest gap every graphical head here carries.
- **1.35.1** closed backlog items N.1–N.5 and X.1, and restored `BACKLOG.md`
  — deleted in `6861545` while three docs still linked to it.

  The headline is **N.1: resume was dead on WinUI and MAUI.**
  `SavedSessionLookup` falls back to `new ControllerFactory()` when handed no
  factory, that fallback has null persistence, and
  `LoadSavedSessionAsync` returns null unconditionally in that case — so
  `CanResume` was permanently false and the Continue button never rendered.
  Both heads *were* writing sessions correctly through the container's
  factory; only the read side was severed, and silently. The class's own XML
  docs describe this exact defect as the reason its `IControllerFactory`
  parameter exists; the parameter landed and two of three call sites were
  never updated. WinUI now reaches the factory through `Navigator.Services`;
  MAUI takes it as a **required** ctor parameter on `GameSelectionViewModel`,
  so it is compile-enforced rather than optional-and-forgettable.

  Fixing it exposed a second defect behind the first: WinUI's `ResumeCommand`
  is an `AsyncRelayCommand`, whose requery is explicit by design, and
  `LookForSavedSessionAsync` raised `PropertyChanged` for `CanResume` (driving
  `Visibility`) but never `CanExecuteChanged`. The lookup fix alone would have
  produced a Continue button that appears and stays greyed out. Two
  independent failures stacked behind one silent fallback — which is the
  argument for the still-open X.2: retiring the
  `?? new ControllerFactory()` idiom from all seven shared-ViewModel sites,
  where an optional parameter turns a compile error into a behaviour change.

  **N.2/N.3 were both tests that existed but did not run.** Every
  `SavedSessionLookup` test used the parameterless constructor — one is named
  `RefreshAsync_WithNoPersistenceConfigured_LeavesNothingToResume` — so the
  suite pinned the broken configuration, and a docstring claiming the found
  path "genuinely can't be" exercised outlived the limitation it described.
  Separately, eight composition-root tests sat behind `#if HAS_MICROSOFT_DI`,
  a symbol nothing ever defined, leaving `AddTableTopHosting` — the root all
  four heads boot through — at 0% coverage while the suite reported green.
  Both fixed: 937 → 961 tests, `SessionResumer` 12.5% → 100%,
  `SavedSessionLookup` → 100%, `AddTableTopHosting` 0% → 44.4%, total
  91.7% → 92.0% line and 70.5% → 71.5% branch. That total is also the first
  real coverage figure this repo has committed, closing the
  "doesn't exist here" note below.

  **N.4** widened `check-maui-async-void.py` to both Android-producing heads
  and renamed it `check-async-void.py`. The native head shipped in 1.32.0
  with an `async void` handler no gate looked at — hand-guarded, which is
  exactly the state MAUI was in when item 27 found eight misses by script
  after six by reading. `SAFE_DELEGATES` is per-head now, since
  `SafePopToRootAsync` is a MAUI type and a same-named Android helper would
  not be the same method. The widened gate was **proved to fail** on an
  injected unguarded handler before being trusted.

  **X.1** pointed MAUI's five specialised game pages at the container's
  factory via a new `Services/AppServices.cs`, rather than each falling
  through to the same `?? new ControllerFactory()` default. Inert today —
  none of those five families consume persistence — so this closes the
  structural gap, not a live bug. **N.5** tagged `v1.35.0`, which had reached
  `main` untagged.

  PATCH: fixes, tests and tooling only. No new user-facing capability and no
  public-surface movement — `PublicApiSurfaceTests` stayed green throughout,
  and `GameSelectionViewModel` lives in the MAUI head, which
  `api/*.api.txt` does not track. Verified with a real SDK: full engine suite,
  both graphical heads built, all eight gates. Not exercised on device — no
  head was launched, so the resume button was never observed rendering.

- **1.35.2** closed backlog X.1-X.5 and opened X.6. CI and documentation
  hardening, plus the structural fix behind 1.35.1's headline bug.

  **There is no `v1.35.2` tag.** This version existed only on `develop`; 1.35.3
  followed before a release was cut, so the work below ships as part of
  `v1.35.3`. Recorded here rather than folded into the 1.35.3 entry, because
  what changed when is the useful history — and because a reader looking for
  the missing tag deserves an answer other than silence.

  **X.2 retired the `?? new ControllerFactory()` idiom.** Seven places in
  `TableTop.Presentation` took an optional `IControllerFactory` and silently
  substituted one carrying no persistence, no diagnostics sink and no DI
  registration. That default is what made 1.35.1's resume bug a behaviour
  change rather than a compile error, and it stacked a second defect behind the
  first. The parameter is now required everywhere, with
  `ArgumentNullException.ThrowIfNull` behind it. Two signatures needed
  reordering, since a required parameter cannot follow an optional one:
  `controllerFactory` moved ahead of `resumeFrom` on `CardTurnGameViewModel`
  and ahead of `winningTokenCount` on `MonogamyGameViewModel`. Roughly thirty
  test call sites now pass `TestFactory.PlainControllerFactory()` — a named
  helper, because tests genuinely do want plain defaults and the point is that
  they say so.

  **The CI work turned up a gap nobody had recorded: `TableTop.Console` was
  never compiled by CI.** `build-and-test` names individual projects rather
  than building the solution, and Console was not in `TableTop.Engine.slnx` at
  all — while README and CLAUDE.md both described that solution as "engine +
  tests + console". A restore compiles nothing. The one head needing no
  workload was the only one with no build coverage. Console is now in the
  solution and has its own CI step.

  Alongside it, the staged `TreatWarningsAsErrors` rollout the workflow had
  promised since the UI jobs were added is done per head — WinUI, native
  Android and Console all measured at zero warnings first. MAUI stays off with
  96 CS0618 deprecations, tracked as X.6 rather than bundled in: migrating to
  the `*Async` variants changes behaviour at every call site, several inside
  `async void` handlers whose guards would need re-checking. The `lint` job now
  covers all four engine assemblies rather than two — Games and Presentation
  were unchecked, Presentation being the shared ViewModel layer three heads
  consume. And the WinUI UI-test steps actually run: item 23 declared them
  "re-enabled" and changed only the comment, leaving them commented for four
  further releases.

  **X.5 found more drift than it had catalogued**, all in `ARCHITECTURE.md` —
  a version header two releases behind, mode/card counts of 99/3,657 against
  the real 101/3,721, and a claim that MAUI and Console rendered fewer
  `ControllerFamily` values than the catalogue, which had been false for
  several releases and read as a deliberate architectural limitation. Two new
  `DocumentationAccuracyTests` pin the mechanical half — README's quoted
  version against `VersionPrefix`, and every `scripts/check-*.py` being named
  in README. The prose stays unenforceable on purpose; README is the enforced
  copy, which this document now says at the point it repeats those numbers.

  PATCH: fixes, tests, CI and documentation. No new user-facing capability and
  no movement in `api/*.api.txt`. The required-parameter change *is* source-
  breaking for `TableTop.Presentation` consumers — that assembly is not tracked
  by the API snapshots and every consumer is an in-tree `ProjectReference`, the
  same reasoning `Directory.Build.props` applies to removals. 964 tests.

- **1.35.3** closed X.2-X.5 and L.1-L.4, and partly closed X.6.

  **L.3 finished a three-year-old consolidation.** `ControllerFactory.CreateAsync`
  now switches on `ControllerFamilies.TryFor(mode)` rather than re-testing the
  seven capability interfaces in a hand-maintained order. All three dispatch
  sites finally agree *by construction* rather than by comment — that ordering
  was wrong once for real, with Monogamy and Quiz transposed while a comment
  claimed they could not disagree, and nothing caught it because no mode in the
  catalogue implements two capability interfaces. The within-family choice moved
  to a private `ProgressionFor`: `IFlowAwareMode` and `IDiceProgressionMode`
  select a progression *strategy*, not a controller type, which is precisely why
  `TryFor` folds all three into `CardTurn`.

  **L.1 turned coverage from a printout into a gate.** CI had collected
  Cobertura and printed a `reportgenerator` summary for a long time, and nothing
  failed when a number went down. `scripts/check-coverage.py` enforces total and
  per-assembly floors set ~1 point under measured. Per-assembly is the half that
  matters: `Games` is ~60% of the tree at 93% and can absorb a sharp fall in
  `Hosting` or `Presentation` — where the logic that breaks lives — while the
  total barely moves.

  **L.2** widened `check-mvvm-method-parity.py` to the native Android head
  (`Vm.Method()` alongside MAUI's `_vm.Method()`), and found a latent defect in
  it while doing so: `vm_type_for` iterated a `set`, and Python randomises
  string hashing per process, so for a file naming two shared ViewModels the
  answer genuinely varied between runs. **L.4** was closed *without* extracting
  a tenth service — 346/390 code lines, and every remaining member is either the
  turn loop itself or a thin delegation to one of the nine coordinators already
  extracted. Inventing a service to satisfy a line count is the failure mode
  those nine avoided.

  **X.6 was partly done, and its own premise was wrong.** The item claimed 96
  CS0618 warnings; that came from grepping the build for one warning prefix, and
  the real figure was **518** once the XAML compiler's share was counted (492
  XC0022 compiled-binding advisories, 18 XC0618 `UseSafeArea`, 8 CS0618
  `Frame`). The 22 deprecated async-API call sites are migrated —
  `DisplayAlert`/`ScaleXTo`/`FadeTo`/`TranslateTo` to their `*Async` forms,
  pure renames, CS0618 down to 8. The rest is split by risk: `Frame`→`Border`
  is a visual change across 10 XAML files and shared styles that nothing here
  can verify, and the `UseSafeArea` replacement could not be confirmed against
  documentation. Both wait for someone who can run the app.

  PATCH: fixes, tests, CI and documentation. No public-surface movement —
  `api/*.api.txt` unchanged. 964 tests; coverage 91.9% line / 70.8% branch.

## What genuinely doesn't exist here

- **A visual deck editor, or any content authoring at all outside the repo.**
  Not resurrected after WPF's removal, and as of 1.21.0 there is no file format
  left to author against. Adding a mode means writing a C# card bank, ideally
  via `CardDeckBuilder`, and rebuilding.
- **Real Xbox controller support.** Needs `Windows.Gaming.Input` polling — a
  genuinely separate input subsystem that cannot be written responsibly
  without a Windows machine and a physical controller. Keyboard bindings
  exist on three of the four gameplay screens (Millionaire deliberately
  excluded — see `BACKLOG.md`) as the tractable substitute.
- ~~**A committed, real coverage percentage.**~~ Closed in 1.35.1: measured on
  a machine with a real SDK at **92.0% line / 71.5% branch** across the four
  engine assemblies. Per-assembly figures and the worst-covered types are in
  `BACKLOG.md` item L.1, which also proposes turning them into a CI floor.
  Reproduce with `scripts/measure-coverage.ps1`, or
  `dotnet test --collect:"XPlat Code Coverage" --settings coverage.runsettings`.
