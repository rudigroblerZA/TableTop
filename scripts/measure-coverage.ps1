$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot

dotnet tool restore
dotnet test tests/TableTop.Tests/TableTop.Tests.csproj `
    -c Release `
    --collect:"XPlat Code Coverage" `
    --settings coverage.runsettings `
    --results-directory tests/TableTop.Tests/TestResults
$reportsGlob = "tests/TableTop.Tests/TestResults/**/coverage.cobertura.xml"
dotnet reportgenerator `
    "-reports:$reportsGlob" `
    "-targetdir:coverage-report" `
    "-reporttypes:TextSummary;Html"
Pop-Location
