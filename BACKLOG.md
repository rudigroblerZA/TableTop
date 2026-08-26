# TableTop — Backlog

Current as of **1.22.0**, August 2026. Open items only; git history has the rest.

Items 1–8 predate the 1.18.0 review and keep their numbers — rewriting a
numbered item in place is how item 7 vanished once (see its note). Items 9–16
came from that review; 1.19.0 closed three of them and half of a fourth, and
those are marked **CLOSED** in place rather than deleted, so the next reader can
see what was done and why. Items 17–18 came out of actually building and running
1.19.0 — the step every previous entry here had to be written without.

---

## Priority

Item numbers are chronological, not ordered — they exist so an item can be
referred to without being renumbered (item 7 explains what happens otherwise).
This is the ordering to work in. It comes from a UI-architecture review that cut
across the existing numbering rather than adding to it.

| | Item | Why it matters |
|---|---|---|
| **P0** | **4** + **12** — controller-family coverage | MAUI has no AreaControl or SimultaneousAnswer screen; Console additionally lacks Monogamy and DailyCampaign. WinUI declares no supported families at all, and its unsupported-mode message sends players to Console for the two modes Console also cannot play. |
| **P0** | **5** — real composition roots | MAUI registers pages and ViewModels DI cannot resolve, then constructs them by hand; WinUI has no composition root. Injected hosting services and test doubles are plumbed in but never used in production. |
| **P1** | **18** — remove or restore the inert presentation layer | `ModePresentation` and every `Resolved*` member pass compiled values straight through. Delete the abstraction, or move the palettes into C# so MAUI theming means something again. |
| **P1** | **12** (second half) — consolidate routing metadata | Not a repeat of the P0 row above — that's the missing screens; this is the mechanism that lets them go missing unnoticed. MAUI and Console support lists are hand-copied into tests; WinUI has none, and `NoHeadSilentlyDropsAFamilyItClaimsToSupport` cannot fail (its predicate is its own filter). A coverage gate that can't fail reads as protection and isn't — raised from P2 because that's a false sense of safety, not just missing tidiness. |
| **P2** | **19** — normalise persistence failure handling | WinUI settings swallow `IOException` and raise `Changed` anyway, so a failed save is indistinguishable from a good one. MAUI bypasses `IAppSettings` entirely for nine reads, going straight to a singleton. |
| **P2** | **20** — reduce UI-thread blocking | Four UI call sites run async controller and session creation through `.GetAwaiter().GetResult()`, two of them in page constructors. Costs responsiveness now; risks a synchronisation-context deadlock later. |
| **P2** | **21** — stale UI comments | Nineteen surviving WPF references; several describe the WinUI file they sit in as WPF. One user-facing settings label may be wrong too. |

Items 1, 2, 3, 8, 13, 15 and 16 sit below this — real, but none of them is
between a player and a working game.

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

**How the review was done, so you know what to trust.** The five static gates
were run and all pass. Nothing was compiled or executed: this sandbox has no
`dotnet` at all, so `check-ui-compiles.py`, `offline-build.sh` and the test
suite were all unavailable. Everything below is from reading source against
docs. Item 9 is the one place I'm asserting a test is *red* rather than
describing a design gap — the reasoning is spelled out so you can check it in
about a minute on a machine with the SDK.

---

### 1. Coverage is a static approximation, not a number

CI collects coverage (`coverage.runsettings`) and `scripts/measure-coverage.ps1`
turns it into a percentage. Neither has run to completion here — no NuGet
access (`NU1301: 403`, re-confirmed each time, never assumed).

`scripts/offline-build.sh` (item 8) does **not** unblock this: coverage needs
the real test suite, which needs xunit and coverlet from NuGet. The engine
building offline is not the same as the tests running.

**Ask: run `./scripts/measure-coverage.ps1` on a machine with NuGet.** The
1.19.0 run proved `dotnet test` is reachable on someone's machine, so this is
now one command away rather than blocked. Static reach analysis is a poor proxy — it was wrong in both directions the once it
was checked: flagged `TableTop.Presentation` low before it had any tests, and
called `TableTop.Games` low because mode-sweeping tests don't name types.

### 2. `tests/TableTop.UiTests` can't reach the shared ViewModels

It references `TableTop.WinUI`, which needs the WinUI SDK. `TableTop.Tests`
(SDK-free) got a direct `TableTop.Presentation` reference instead, so real
ViewModel tests got written without fixing this — a sidestep, not a fix.
Whatever `UiTests` existed for is still blocked.

Decide: give it a reason to exist that doesn't need the SDK, or accept it
needs Windows and say so plainly.

### 3. Xbox controller — designed, not implemented

Needs `Windows.Gaming.Input` polling; can't be written responsibly without
hardware to test button feel and repeat rate. Keyboard bindings on three of
four gameplay screens are the shipped substitute; the path for a controller to
reuse the same `ICommand`s is designed but unbuilt.

**Closed by design, not a gap:** Millionaire has no accelerators. Its answers
and lifelines are template-generated in an `ItemsControl` (a static `Key="A"`
in a `DataTemplate` binds to every instance), and `WalkAway` is irreversible —
the same reasoning that excludes `Quit` on the main screen.

### 4. Head routing — partially closed

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

### 5. Composition-root DI — bypass closed, wiring not

All six `new ControllerFactory()` sites accept an injected `IControllerFactory`,
falling back only when none is passed. Proved with a recording spy that
injection takes effect, not just that it compiles.

Also corrected: this backlog once called `SavedSessionLookup`'s "session found"
path *structurally untestable*. It wasn't — it was this bypass, misdiagnosed.

**Open:** heads don't pass anything yet. MAUI registers services but pages
construct ViewModels directly; WinUI has no composition root. Plumbing in,
wiring out — production still takes the default path.

**Sharpened by review — the MAUI registrations are not merely unused, they are
unresolvable.** `MauiProgram` registers `PlayerSetupPage`, `GameplayPage`,
`PlayerSetupViewModel` and `GameplayViewModel`, and every one of those
constructors takes a runtime value the container has no registration for:
`PlayerSetupPage(IGameMode)`, `GameplayPage(IGameMode, List<IPlayer>, …)`.
Resolving any of them throws. So "wire the heads up" is not a small change —
it needs a factory or parameterised-resolution seam for the per-session values
first. Budget for that rather than discovering it mid-change.

Also: `AddTableTopHosting` registers `IControllerFactory` transient with the
comment *"a new factory is cheap and carries no state of its own."* Its
constructor assigns the process-wide `JsonDeckLoader.Diagnostics` static
unconditionally, including to null. It carries global state, and transient
means every resolution reassigns it. See items 11 and 14.

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

### 12. WinUI is outside head-family coverage, and one coverage test is a tautology

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

### 13. The doc counts disagree — **mode count CLOSED in 1.21.0; card and test counts still open**

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

**Still open — the card count.** README says **3,591 cards** and nothing checks
it. The card half of the guard was removed in 1.18.0 with sound reasoning
(regex-scraping C# initialisers is brittle), and the obvious replacement is
summed `ModeManifest.TotalCards` — which is now trustworthy, since item 10 fixed
the one mode that was reporting the wrong deck. That makes this a tractable
one-line addition rather than the blocked item it was.

**Still open — the test count.** README says 776; ARCHITECTURE says roughly 900.
Neither is checked and the real figure moved again with 1.21.0's removals. Lower
value than the card count: nobody makes a decision on it.

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

### 15. `CardTurnController` has 15 lines of raw headroom, and documenting it costs budget

Measured now: **685 raw / 700**, **360 code / 390**. The raw backstop will bind
first, at less than half the headroom.

That inverts the guard's own design. Its docstring says `MaxCodeLines` "is the
one that matters" and that `IsSubstantive` skips comments so "adding
documentation never costs you budget." `IsSubstantive` excludes `//`-prefixed
lines, which catches `///` too — so doc comments are free against *code*, and
counted in full against *raw*. In a file this close to the raw ceiling, writing
the XML docs this codebase rightly insists on is what trips the guard, and it
trips with a message about readability that won't match what the author did.

Not a reason to raise a ceiling — the guard's advice to extract rather than
raise is sound and has been vindicated twice. But decide deliberately whether
raw should exclude doc comments too, or whether the next extraction happens
now, before someone hits this while doing something unrelated and reads the
failure as noise.

### 16. `check-ui-compiles.py` dies with a traceback when `dotnet` is absent

It calls `subprocess.run(["dotnet", …])` unguarded and raises
`FileNotFoundError` with a full Python traceback. Every other gate degrades or
reports cleanly. Its sibling scripts are pure-Python and run in the XAML CI job
precisely because they need no SDK; this one needs one and doesn't say so.

Small fix, real value: a `shutil.which("dotnet")` check with an explicit
"skipped, no SDK" message and a chosen exit code. 1.19.0 raised the stakes
slightly: the compile error in item 9 is exactly what this check exists to
catch, and it stayed hidden partly because the check dies noisily enough to be
mistaken for the environment rather than the code. This is a check whose entire
purpose is to report a nuanced result honestly — see its known limitation about
unresolvable framework types masking first-party errors — and it currently
cannot distinguish "no toolchain" from "your code is broken."

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

### 18. `Presentation` is a permanent `None`, and `Resolved*` is a pass-through

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

### 19. Persistence failures are handled three different ways, none of them visibly

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

### 20. Async work is run synchronously on UI threads

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

### 21. Comments describe a head that no longer exists

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

### 22. `MillionaireGameViewModelTests` is flaky — it drives a shuffled real deck

**P2, and it is genuinely intermittent — measured, not suspected.** Three of the
class's tests answer `vm.Answers[0]` and assert `vm.IsAnswered` becomes true.
`RealController()` builds from `new MillionaireMode().GetQuestionBank()`, the
live bank, and the question order is shuffled, so which question is on screen
and what `Answers[0]` refers to changes per run.

Measured on the 1.21.0 tree while adding an unrelated mode: the full suite
failed 2 runs out of 3, and the *failing test within the class differed between
runs* (`AnswerOption_SelectCommand_…` one time, `AnswerOption_Invoke_…` the
next). Run the class alone and it passed 5 for 5 — which is exactly the profile
that gets a failure dismissed as "just a flake" and then hides a real one later.

This is not the parallel-statics problem item 14 describes; both statics named
there are gone. It is a test reaching for real, shuffled content when it wanted
a fixed fixture. The fix is a stub question bank with known answers, so the
three tests assert the routing they were written to assert
(`Invoke()` and `SelectCommand` reaching the same path) rather than
incidentally depending on which question the shuffle dealt.

Worth doing before the next person debugs it: a suite that fails a third of the
time trains everyone to re-run rather than read.

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
| `PublicApiSurfaceTests` | unnoticed breaking change to public surface | written proactively |
| `ModeManifestExtensions` dispatch | a mode's manifest reporting zero cards | `Claimed!` excluded from capped `SurpriseMe` for a version |

All five `check-*.py` gates pass as of 1.19.0.

`DeckManifestTests` used to be in this table. It was removed in 1.19.0 with the
JSON deck pipeline — the correct end for a guard whose subject no longer exists,
as distinct from deleting one that still has a job.

**One entry in this table is still not doing its job:**

- `NoHeadSilentlyDropsAFamilyItClaimsToSupport` cannot fail (item 12).

`ModeManifestExtensions` dispatch was the other. It had acquired the exact bug it
was written to prevent, one interface over — fixed in 1.20.0 by removing the
dispatch rather than repairing it: the manifest now derives from
`ControllerFamilies.TryFor` and has no order of its own to get wrong. That is the
better shape of fix for this table generally. A guard that re-implements a
decision made elsewhere can drift from it; one that reads that decision cannot.

The pattern is the same each time: the guard was written against the specific
bug that shipped, and the next instance arrived one position along. Worth
holding in mind when writing the fixes — a check pinned to `IClaimedDeckProvider`
by name does not survive `IHerdDeckProvider`.
