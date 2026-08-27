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

}
finally {
    Pop-Location
}
