#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
app_project="$repo_root/src/PjuNetworkTester/PjuNetworkTester.csproj"
output_path="$repo_root/artifacts/win-x64"

rm -rf "$output_path"
mkdir -p "$output_path"

echo "Publishing PJU Network Tester for win-x64 (Release) ..."

dotnet publish "$app_project" \
  --configuration Release \
  --runtime win-x64 \
  --self-contained true \
  --output "$output_path" \
  /p:PublishSingleFile=true \
  /p:IncludeNativeLibrariesForSelfExtract=true \
  /p:DebugType=None \
  /p:DebugSymbols=false

exe_path="$output_path/PjuNetworkTester.exe"
if [[ ! -f "$exe_path" ]]; then
  echo "Publish finished, but expected executable was not found: $exe_path" >&2
  exit 1
fi

echo "Publish complete: $output_path"
echo "Executable: $exe_path"
