[CmdletBinding()]
param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
$outputRoot = Join-Path $projectRoot "Package"
$pluginRoot = Join-Path $outputRoot "BepInEx\plugins\SwmarlyValheimPizzaMod"
$projectName = "SwmarlyValheimPizzaMod"
$buildOutput = Join-Path $projectRoot "bin\$Configuration\net48"
$dll = Join-Path $buildOutput "$projectName.dll"

if (-not (Test-Path -LiteralPath $dll)) {
    throw "Build output was not found at $dll. Build the project first with a valid ValheimPath and BepInExPath."
}

New-Item -ItemType Directory -Force -Path $pluginRoot | Out-Null
Copy-Item -LiteralPath $dll -Destination $pluginRoot -Force
$pdb = Join-Path $buildOutput "$projectName.pdb"
if (Test-Path -LiteralPath $pdb) {
    Copy-Item -LiteralPath $pdb -Destination $pluginRoot -Force
}

# Thunderstore users should receive the full usage documentation.
Copy-Item -LiteralPath (Join-Path $projectRoot "README.md") -Destination (Join-Path $outputRoot "README.md") -Force

Write-Host "Package staged at $outputRoot"
