param(
    [string]$InnoSetupCompiler = "iscc",
    [switch]$SkipPublish
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$publishScript = Join-Path $repoRoot "scripts/publish-windows.ps1"
$installerScript = Join-Path $repoRoot "installer/PjuNetworkTester.iss"
$publishOutput = Join-Path $repoRoot "artifacts/win-x64"

if (!$SkipPublish) {
    & $publishScript
}

$exePath = Join-Path $publishOutput "PjuNetworkTester.exe"
if (!(Test-Path $exePath)) {
    throw "Windows publish output is missing. Expected: $exePath"
}

Write-Host "Building installer with Inno Setup ..."
& $InnoSetupCompiler $installerScript

$setupPath = Join-Path $repoRoot "installer/output/PJU-Network-Tester-Setup.exe"
if (!(Test-Path $setupPath)) {
    throw "Installer build finished, but expected setup file was not found: $setupPath"
}

Write-Host "Installer complete: $setupPath"
