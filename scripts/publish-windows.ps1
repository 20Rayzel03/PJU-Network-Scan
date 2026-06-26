param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Output = "artifacts/win-x64"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$appProject = Join-Path $repoRoot "src/PjuNetworkTester/PjuNetworkTester.csproj"
$outputPath = Join-Path $repoRoot $Output

if (Test-Path $outputPath) {
    Remove-Item $outputPath -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $outputPath | Out-Null

Write-Host "Publishing PJU Network Tester for $Runtime ($Configuration) ..."

dotnet publish $appProject `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained true `
    --output $outputPath `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:DebugType=None `
    /p:DebugSymbols=false

$exePath = Join-Path $outputPath "PjuNetworkTester.exe"
if (!(Test-Path $exePath)) {
    throw "Publish finished, but expected executable was not found: $exePath"
}

Write-Host "Publish complete: $outputPath"
Write-Host "Executable: $exePath"
