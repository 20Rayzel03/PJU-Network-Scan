# PJU Network Tester Implementation Plan

> **For Hermes:** Use subagent-driven-development skill to implement this plan task-by-task.

**Goal:** Build a Windows desktop app with installer that scans IP ranges, enriches reachable hosts with DNS/MAC/vendor data, summarizes unreachable /24 blocks, and exports the visible table as XLSX.

**Architecture:** Use a .NET solution with an Avalonia UI desktop app, separated core scanning services, localization resources, and installer packaging. Keep Version 1 focused: range parsing, ping/reverse-DNS/MAC/OUI enrichment, table display, settings, and XLSX export.

**Tech Stack:** C#/.NET, Avalonia UI, ClosedXML for XLSX export, Inno Setup for Setup.exe packaging, JSON settings file.

---

## Confirmed Requirements

### Product

- App name: **PJU Network Tester**
- Repository: `20Rayzel03/PJU-Network-Scan`
- Target: Windows Desktop-App with `Setup.exe` installer
- Design: dark, clean dashboard look inspired by `https://home.jqpollag.de/`
- Icon: use website favicon as app and installer icon where possible

### Scan Input Formats

The scan input field must accept:

- Single IP: `10.1.5.20`
- Explicit range: `10.1.2.1 - 10.1.25.255`
- CIDR network: `10.1.5.0/24`
- CIDR host address: `10.1.5.1/24`, interpreted as the complete `10.1.5.0/24` network

### Scan Result Columns

- Status / online indicator
- IP address or summarized subnet
- Hostname / reverse DNS name
- MAC address, if technically visible
- Manufacturer/vendor from OUI data, if available
- Ping time in ms
- Note / remark

### Display Rules

- Default: show only online devices.
- Setting: allow showing offline addresses.
- If no address in a `/24` responds, show one summarized row such as:
  - `10.1.4.0/24` → `Adressbereich nicht erreichbar oder existiert nicht im Netzwerk.`
- Do not spam the table with 255 individual offline rows for completely unreachable `/24` segments.

### Export

- Export as Excel/XLSX.
- CSV is not required for Version 1.

### Localization

- Supported languages: German and English.
- Installer should allow language selection.
- App settings should allow changing language after installation.

### Out of Scope for Version 1

- TCP/UDP two-PC connection test
- Port scanning
- Scan history
- Compare old/new scans
- Server/client mode

---

## Implementation Tasks

### Task 1: Create .NET solution skeleton

**Objective:** Create the initial project structure.

**Files:**
- Create: `src/PjuNetworkTester/PjuNetworkTester.csproj`
- Create: `src/PjuNetworkTester.Core/PjuNetworkTester.Core.csproj`
- Create: `tests/PjuNetworkTester.Core.Tests/PjuNetworkTester.Core.Tests.csproj`
- Create: `PJU-Network-Scan.sln`

**Steps:**
1. Install or verify .NET SDK availability.
2. Create Avalonia app project.
3. Create core class library.
4. Create xUnit test project.
5. Add references from app/tests to core.
6. Run `dotnet build`.
7. Commit: `chore: create initial .NET solution`.

### Task 2: Add IP range parser with tests

**Objective:** Parse supported input forms into normalized IP ranges.

**Files:**
- Create: `src/PjuNetworkTester.Core/Networking/IpRangeParser.cs`
- Create: `src/PjuNetworkTester.Core/Networking/IpRange.cs`
- Test: `tests/PjuNetworkTester.Core.Tests/IpRangeParserTests.cs`

**Test Cases:**
- `10.1.5.20` → start/end `10.1.5.20`
- `10.1.2.1 - 10.1.25.255` → exact start/end
- `10.1.5.0/24` → `10.1.5.0 - 10.1.5.255`
- `10.1.5.1/24` → `10.1.5.0 - 10.1.5.255`
- invalid input returns validation error

**Verification:**
- Run `dotnet test`.
- Commit: `feat: add IP range parser`.

### Task 3: Add scan result model and /24 grouping logic

**Objective:** Represent scan results and summarize fully unreachable `/24` blocks.

**Files:**
- Create: `src/PjuNetworkTester.Core/Scanning/ScanResult.cs`
- Create: `src/PjuNetworkTester.Core/Scanning/SubnetSummaryService.cs`
- Test: `tests/PjuNetworkTester.Core.Tests/SubnetSummaryServiceTests.cs`

**Rules:**
- If an entire `/24` has zero online hosts, produce one summary row.
- If a `/24` contains at least one online host, keep host-level rows.
- Summary row note German default: `Adressbereich nicht erreichbar oder existiert nicht im Netzwerk.`

**Verification:**
- Run `dotnet test`.
- Commit: `feat: summarize unreachable subnet blocks`.

### Task 4: Implement ping and reverse DNS scanner

**Objective:** Scan IPs concurrently with timeout and reverse DNS lookup.

**Files:**
- Create: `src/PjuNetworkTester.Core/Scanning/NetworkScanner.cs`
- Create: `src/PjuNetworkTester.Core/Scanning/ScanOptions.cs`
- Test: `tests/PjuNetworkTester.Core.Tests/NetworkScannerTests.cs`

**Notes:**
- Use bounded concurrency to avoid overwhelming the network.
- Default timeout should be conservative.
- Cancellation support is required for Stop Scan.

**Verification:**
- Unit tests for scanner behavior where mockable.
- Manual test with `127.0.0.1`.
- Commit: `feat: add ping and reverse DNS scanner`.

### Task 5: Implement MAC and vendor lookup

**Objective:** Try to resolve MAC addresses for reachable local devices and map OUI prefixes to vendors.

**Files:**
- Create: `src/PjuNetworkTester.Core/Networking/ArpTableReader.cs`
- Create: `src/PjuNetworkTester.Core/Networking/OuiVendorLookup.cs`
- Create: `src/PjuNetworkTester.Core/Data/oui-vendors.json`
- Test: `tests/PjuNetworkTester.Core.Tests/OuiVendorLookupTests.cs`

**Notes:**
- MAC detection is only reliable in the same Layer-2 network segment.
- Missing MAC is not an error; show empty/unknown.

**Verification:**
- Run `dotnet test`.
- Manual ARP lookup test on Windows during later verification.
- Commit: `feat: add MAC and vendor detection`.

### Task 6: Build main Avalonia UI

**Objective:** Create the main window with input, scan controls, progress, settings link, table, and export button.

**Files:**
- Modify: `src/PjuNetworkTester/MainWindow.axaml`
- Modify: `src/PjuNetworkTester/MainWindow.axaml.cs`
- Create: `src/PjuNetworkTester/ViewModels/MainWindowViewModel.cs`

**UI Elements:**
- App title: `PJU Network Tester`
- Input field
- Start/Stop Scan button
- Progress bar/status text
- Results table
- XLSX export button
- Settings button

**Verification:**
- Run app locally.
- Confirm UI starts and accepts input.
- Commit: `feat: build main scanner UI`.

### Task 7: Add settings and localization

**Objective:** Add German/English text resources and user settings.

**Files:**
- Create: `src/PjuNetworkTester/Localization/Strings.de.resx`
- Create: `src/PjuNetworkTester/Localization/Strings.en.resx`
- Create: `src/PjuNetworkTester/Settings/AppSettings.cs`
- Create: `src/PjuNetworkTester/Settings/AppSettingsService.cs`

**Settings:**
- Language: German/English
- Show offline addresses
- Scan timeout
- Max concurrency

**Verification:**
- Switch language in app settings.
- Restart app and confirm setting persists.
- Commit: `feat: add settings and localization`.

### Task 8: Add XLSX export

**Objective:** Export current table results to Excel/XLSX.

**Files:**
- Create: `src/PjuNetworkTester.Core/Export/XlsxExporter.cs`
- Test: `tests/PjuNetworkTester.Core.Tests/XlsxExporterTests.cs`

**Verification:**
- Export sample data.
- Open/inspect generated XLSX structure in tests.
- Commit: `feat: add XLSX export`.

### Task 9: Apply visual design and icon

**Objective:** Apply the dark dashboard style and app icon.

**Files:**
- Modify: Avalonia style files
- Add: `src/PjuNetworkTester/Assets/app-icon.ico`
- Add: `src/PjuNetworkTester/Assets/app-icon.png`

**Verification:**
- App window uses dark theme.
- Icon appears in app window/taskbar where supported.
- Commit: `style: apply PJU dashboard design`.

### Task 10: Add Windows installer packaging

**Objective:** Build a `Setup.exe` installer.

**Files:**
- Create: `installer/PjuNetworkTester.iss`
- Create: `scripts/build-installer.ps1`

**Verification:**
- Publish Windows build.
- Compile installer with Inno Setup.
- Test install/uninstall on Windows.
- Commit: `build: add Windows installer packaging`.

### Task 11: Add CI build workflow

**Objective:** Build and test the project on GitHub Actions.

**Files:**
- Create: `.github/workflows/ci.yml`

**Verification:**
- CI runs `dotnet restore`, `dotnet build`, and `dotnet test`.
- Commit: `ci: add .NET build and test workflow`.

---

## Verification Checklist

- [ ] `dotnet build` succeeds
- [ ] `dotnet test` succeeds
- [ ] app starts on Windows
- [ ] `10.1.5.0/24` parses correctly
- [ ] `10.1.5.1/24` parses as complete `/24`
- [ ] `10.1.2.1 - 10.1.25.255` parses correctly
- [ ] online-only default view works
- [ ] offline-address setting works
- [ ] unreachable `/24` summary row works
- [ ] XLSX export works
- [ ] German/English switching works
- [ ] Setup.exe installs and uninstalls cleanly
