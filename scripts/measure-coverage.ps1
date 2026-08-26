<#
.SYNOPSIS
    Runs the test suite with coverage and prints a real number.

.DESCRIPTION
    Backlog item 3 ("real test coverage is unknown") exists because the
    assistant's own environment cannot reach NuGet, so it has never been able
    to run this. That is not a reason to leave the item as a guess forever —
    it is a reason to make running it, on a machine that CAN, as close to
    zero-friction as possible. This script is that: one command, a real
    Cobertura report, and a readable summary instead of an XML file you have
    to go interpret yourself.

    Mirrors exactly what CI now does (.github/workflows/ci.yml, "Run tests" /
    "Generate coverage summary" steps) — same settings file, same tool, same
    report — so a local run and a CI run are never quietly measuring
    different things.

.EXAMPLE
    ./scripts/measure-coverage.ps1
#>

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot

try {
    Write-Host "Restoring local tools (reportgenerator)..." -ForegroundColor Cyan
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw "dotnet tool restore failed." }

    Write-Host "`nRunning tests with coverage..." -ForegroundColor Cyan
    dotnet test tests/TableTop.Tests/TableTop.Tests.csproj `
        -c Release `
        --collect:"XPlat Code Coverage" `
        --settings coverage.runsettings `
        --results-directory tests/TableTop.Tests/TestResults
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`nTests failed — see output above. Coverage was not measured." -ForegroundColor Yellow
        exit $LASTEXITCODE
    }

    Write-Host "`nGenerating summary..." -ForegroundColor Cyan
    $reportsGlob = "tests/TableTop.Tests/TestResults/**/coverage.cobertura.xml"
    dotnet reportgenerator `
        "-reports:$reportsGlob" `
        "-targetdir:coverage-report" `
        "-reporttypes:TextSummary;Html"
    if ($LASTEXITCODE -ne 0) { throw "reportgenerator failed." }

    Write-Host "`n──────────────────────────────────────────────" -ForegroundColor Green
    Get-Content "coverage-report/Summary.txt"
    Write-Host "──────────────────────────────────────────────" -ForegroundColor Green
    Write-Host "`nFull HTML report: coverage-report/index.html"
    Write-Host "Paste the summary above back to update BACKLOG.md item 1."
}
finally {
    Pop-Location
}
