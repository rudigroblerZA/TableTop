# TableTop — Architecture Review

Current as of **1.24.0**, August 2026. This replaces the accumulated
documentation that used to live in `docs/` — most of it (week-by-week status
reports, a stakeholder presentation, a delivery summary) was stale project
history rather than a description of the system as it stands. This is a
from-scratch account of what's actually here, written to be trusted rather
than archaeologically verified.

## Shape

```
Core ← Games ← Hosting ← Presentation
                              ↑
                    Console · WinUI · MAUI
```

Four engine assemblies, three heads. `Core` defines the abstractions (cards,
players, scoring, progression, rules) and has no dependency on anything else
in the solution. `Games` is 97 modes and their decks. `Hosting` is the runtime
— controllers, the archetype registry, persistence, `ControllerFactory`.
`Presentation` is shared ViewModels: plain `net10.0`, no platform SDK
dependency, which is what makes it directly unit-testable without WinUI or
MAUI installed anywhere.

Console, WinUI and MAUI are the three heads. Console is text-only and builds
here. WinUI and MAUI need their respective SDKs, which this environment does
not have — see **Verification, honestly** below for what that actually means
in practice.

## Content

**97 modes, 3,591 cards, all compiled in.** Every mode builds its deck from an
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

One deliberate loose end: `BaseGameModeDefinition.Presentation` now always
returns `ModePresentation.None`, so every `Resolved*` member (`DisplayName`,
`ResolvedCompleteLabel`, `ResolvedCategoryColours`, `Theme`, …) is a
pass-through to the compiled-in value. They were kept rather than deleted
because both heads, the shared ViewModels and the public API snapshot bind to
them. Collapsing them is a head-facing change and belongs in its own commit —
see `BACKLOG.md`.

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
failing test rather than something a player discovers. Heads currently render
fewer families than exist: MAUI has no AreaControl or SimultaneousAnswer
screen, Console has neither plus no Monogamy or DailyCampaign. Those are
declared gaps, not silent ones.

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
- **A committed, real coverage percentage.** Static reach analysis exists as
  an approximation; the actual number needs one command
  (`scripts/measure-coverage.ps1`) on a machine with NuGet access.
