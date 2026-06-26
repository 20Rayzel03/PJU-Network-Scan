# Windows Publish und Installer

Dieses Dokument beschreibt den aktuellen Release-/Installer-Stand für **PJU Network Tester**.

## 1. Windows-Publish erzeugen

Auf Windows mit PowerShell:

```powershell
./scripts/publish-windows.ps1
```

Auf Linux/macOS mit Bash, z. B. für CI oder Cross-Publish:

```bash
./scripts/publish-windows.sh
```

Das erzeugt den Windows-Release-Publish hier:

```text
artifacts/win-x64/
```

Die erwartete Hauptdatei ist:

```text
artifacts/win-x64/PjuNetworkTester.exe
```

Aktuell wird veröffentlicht als:

- Runtime: `win-x64`
- Configuration: `Release`
- self-contained: ja
- single-file: ja

Dadurch braucht der Ziel-PC normalerweise keine separate .NET-Installation.

## 2. Setup.exe mit Inno Setup bauen

Voraussetzung auf dem Windows-Build-PC:

- Inno Setup 6.x
- `iscc.exe` im PATH oder Pfad per Parameter angeben

Build:

```powershell
./scripts/build-installer.ps1
```

Falls der Publish bereits vorhanden ist:

```powershell
./scripts/build-installer.ps1 -SkipPublish
```

Falls `iscc.exe` nicht im PATH ist:

```powershell
./scripts/build-installer.ps1 -InnoSetupCompiler "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
```

Ergebnis:

```text
installer/output/PJU-Network-Tester-Setup.exe
```

## 3. Installer-Verhalten

Der Installer ist als Benutzer-Installation vorbereitet:

```text
%LOCALAPPDATA%\Programs\PJU Network Tester
```

Dadurch sind keine Adminrechte nötig.

Unterstützte Installer-Sprachen:

- Deutsch
- Englisch

Zusätzlich kann ein Desktop-Icon optional ausgewählt werden.

## 4. Noch offen

- echtes PJU/Icon/Favicon statt Avalonia-Standardicon einbauen
- Installer später auf einem Windows-System real kompilieren und testen
- optional GitHub Actions Release-Workflow ergänzen
