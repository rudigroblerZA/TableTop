#!/usr/bin/env bash
# Builds the four engine assemblies with NO NuGet access.
#
# WHY THIS EXISTS
# ---------------
# Every build in this project's history depended on a NuGet package cache that
# happened to be present in the dev container. Nothing recorded that. When the
# sandbox was reset mid-session the cache vanished, nuget.org returned
# `403 host_not_allowed`, and the entire solution became unbuildable — even
# TableTop.Core, which needs only two Microsoft.Extensions.* references.
#
# The way out: those two assemblies ALREADY SHIP with the .NET SDK, inside the
# ASP.NET Core shared framework. Referencing them by path instead of by
# PackageReference removes the restore step entirely.
#
# Two things that cost time and are worth not rediscovering:
#
#   1. Use `shared/Microsoft.AspNetCore.App/<ver>`, NOT
#      `packs/Microsoft.AspNetCore.App.Ref/<ver>/ref/net10.0`. The latter are
#      REFERENCE assemblies: they compile fine and then fail at runtime with
#      "Reference assemblies cannot be loaded for execution (0x80131058)".
#      Everything builds; only tests that actually load the assembly break.
#
#   2. Run test harnesses from INSIDE the repo tree. Several guards walk up
#      from the assembly directory looking for `src/TableTop.Games`, so a
#      harness in /tmp or a sibling directory fails with a confusing
#      "could not locate repository root" that looks like a code fault.
#
# This is a fallback for a restricted environment, not a replacement for the
# real solution build. It covers the engine only — the UI heads still need
# their platform SDKs.

set -euo pipefail

REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUT="${1:-/tmp/tabletop-offline}"

# Highest installed ASP.NET Core shared framework.
FW_ROOT="/usr/lib/dotnet/shared/Microsoft.AspNetCore.App"
if [[ ! -d "$FW_ROOT" ]]; then
  echo "error: $FW_ROOT not found — install the .NET SDK (apt-get install -y dotnet-sdk-10.0)" >&2
  exit 1
fi
FW="$FW_ROOT/$(ls "$FW_ROOT" | sort -V | tail -1)"
echo "using framework assemblies from: $FW"

mkdir -p "$OUT"
rm -rf "${OUT:?}"/*

emit() {                      # emit <name> <dep1> <dep2> ...
  local name="$1"; shift
  local refs=""
  for d in "$@"; do
    refs+="    <ProjectReference Include=\"$OUT/$(echo "$d" | tr '[:upper:]' '[:lower:]').csproj\" />"$'\n'
  done
  cat > "$OUT/$(echo "$name" | tr '[:upper:]' '[:lower:]').csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AssemblyName>TableTop.$name</AssemblyName>
    <RootNamespace>TableTop.$name</RootNamespace>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <GenerateDocumentationFile>false</GenerateDocumentationFile>
    <NoWarn>CS1591;CS1574</NoWarn>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="$REPO/src/TableTop.$name/**/*.cs"
             Exclude="$REPO/src/TableTop.$name/obj/**;$REPO/src/TableTop.$name/bin/**" />
  </ItemGroup>
  <ItemGroup>
$refs  </ItemGroup>
  <ItemGroup>
    <Reference Include="Microsoft.Extensions.DependencyInjection.Abstractions">
      <HintPath>$FW/Microsoft.Extensions.DependencyInjection.Abstractions.dll</HintPath>
    </Reference>
    <Reference Include="Microsoft.Extensions.Logging.Abstractions">
      <HintPath>$FW/Microsoft.Extensions.Logging.Abstractions.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
EOF
}

emit Core
emit Games        Core
emit Hosting      Core Games
emit Presentation Core Games Hosting

echo "building…"
dotnet build "$OUT/presentation.csproj" -c Release "${@:2}"

echo
echo "engine built offline: $OUT"
echo "reference these .csproj files from a harness placed inside the repo tree"
echo "(see note 2 above) to run guards or ad-hoc verification."
