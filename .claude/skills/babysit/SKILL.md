---
name: babysit
description: Repo-specific conventions for driving a TableTop pull request to a green, mergeable state — this repo's CI shape, known quirks, and what "done" means here. Read before acting on a CI failure or review comment on a PR you opened or were asked to drive.
---

# Babysitting a TableTop PR

This supplements (does not replace) the standing PR-driving rules: merge
conflicts first, then red CI, then review comments; never skip, disable, or
quarantine a test; never rewrite history on someone else's branch; never
widen a PR with unrelated changes to get it green. Those "never"s hold no
matter what's written below.

## Posture

- **Never punt.** A red check or an open thread is work now, not something to
  leave for the next wake. If you can't fix it this turn, say exactly what's
  blocking in one PR comment — don't go quiet.
- **Address every unresolved thread**, not just the ones that look load-bearing.
  A nit sitting next to a design question is still a nit to fix.
- **A failing test is never "just a flake" by assertion.** This repo has one
  documented real flake pattern (below) — everything else gets root-caused or
  explained, not waved off.

## What "done" looks like here

Green on every job in `.github/workflows/ci.yml` that runs for the PR's
changes, no merge conflict with the base branch, and every review thread
either resolved or answered. This repo runs no "Claude Approvals" check —
don't wait on one that doesn't exist.

## This repo's CI shape

Seven jobs, three tiers:

1. **`xaml`** (Ubuntu, no SDK) — pure-Python static gates in `scripts/`, run
   first because they need no restore and still report when the build itself
   is broken. If one of these fails, the fix is almost always in the XAML or
   C# the gate names, not in the gate script itself — read the script's own
   docstring (`python3 scripts/<name>.py --help`-style header comment) before
   assuming it's wrong.
2. **`trivy`** (Ubuntu) — dependency/secret scan, report-only (`exit-code: "0"`)
   for now. A finding here is worth a look but is not currently a hard gate.
3. **`build-and-test`, `build-windows-heads`, `build-maui`, `build-android`,
   `lint`** — real builds. `build-and-test` covers Core/Games/Hosting/Console
   plus the full `TableTop.Tests` suite with coverage floors
   (`scripts/check-coverage.py`); the other three build the graphical heads
   and need their respective SDK/workload. `lint` re-builds all four engine
   assemblies with `GenerateDocumentationFile` to catch missing XML doc
   comments (CS1591) as errors.

### Warnings-as-errors status (check before assuming a warning should fail the build)

`TreatWarningsAsErrors=true` is ON for: Core, Games, Hosting (via `lint`),
Console, WinUI, MAUI (both `net10.0-windows...` and `net10.0-android`, with
one narrow exemption — `-p:WarningsNotAsErrors=XA1006`, tracked as backlog
X.7, delete when that lands), and the native Android head. If a build fails
on a warning in one of these, that's real — fix it, don't ask for the flag to
be turned off. A project that does NOT yet have it on says so in its own
`ci.yml` step comment (e.g. a brand-new tool build not yet measured with
`--no-incremental`) — don't flip it on as a side effect of an unrelated fix;
that's its own change with its own "measure clean first" step.

### The one documented real flake

`build-windows-heads`' UI-test step (`TableTop.UiTests`) has hung on the
runner before — not in the tests themselves (they run in ~361ms locally) but
in host startup/shutdown on a runner with no interactive desktop session. It
now runs with `--blame-hang --blame-hang-timeout 5m --blame-hang-dump-type
full`, so a real hang produces a `Sequence*.xml` / `.dmp` artifact instead of
burning the 30-minute job timeout. If this step times out: check the uploaded
`ui-test-results` artifact first. A hang with no artifact, or a dump pointing
inside `TableTop.WinUI` app code rather than host machinery, is a real
regression — treat it as one, don't just re-run.

Nothing else in this pipeline has an established flake pattern. A red
`build-and-test`, `build-maui`, or `build-android` is this PR's until proven
otherwise (reproduces identically on a clean re-run, or fails the same way on
the base branch).

## Repo-specific things to check before pushing a fix

- **Public API discipline.** If the diff touches `src/TableTop.Core`,
  `src/TableTop.Games`, or `src/TableTop.Hosting`'s public surface,
  `PublicApiSurfaceTests` will fail until `api/TableTop.{Core,Games,Hosting}.api.txt`
  is regenerated: `TABLETOP_UPDATE_API=1 dotnet test tests/TableTop.Tests --filter PublicApiSurfaceTests`.
  Read the diff before committing it — a regenerated-without-reading snapshot
  defeats the point of having one.
- **Versioning.** A MINOR/PATCH-worthy change needs `VersionPrefix` bumped in
  `Directory.Build.props` — see that file's own comment block for the
  MAJOR/MINOR/PATCH decision tree (and the narrow carve-out for why most
  removals here are MINOR, not MAJOR). If you bump it, also update
  `README.md`'s `Currently **N.N.N**` line in the same push —
  `DocumentationAccuracyTests.Readme_quoted_version_matches_the_build_props`
  checks the two agree.
- **README self-checks.** `DocumentationAccuracyTests` also asserts the
  README's solution-structure tree names every `*.csproj` in the repo, and
  its list of static gates names every `scripts/check-*.py` file. Adding a
  project or a script without a matching README mention fails these — and
  they're checked by literal substring, so `TableTop.Foo.Tests` needs that
  exact string somewhere in the README, not just `Foo.Tests`.
- **`BACKLOG.md`.** Check it before treating something as a new finding — it
  may already be tracked, closed, or deliberately deferred with a reason
  recorded there.

## No `dotnet` locally, but CI has it

Per `CLAUDE.md`'s "Environment reality check," a Claude session working in
this repo often has no local `.NET` SDK and has to verify changes by reading.
That constraint does not apply to GitHub Actions — the runners have the real
SDK (and, on the Windows jobs, the real workloads). When re-checking a CI
failure or validating a fix for one, prefer reproducing it for real against
CI's own commands (copy the exact `dotnet build`/`dotnet test` invocation out
of the failing job in `ci.yml`) over re-deriving the same conclusion by
static reasoning a second time.
