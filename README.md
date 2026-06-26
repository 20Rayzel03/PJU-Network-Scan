# PJU Network Tester

**PJU Network Tester** ist eine Windows-Desktop-App zur schnellen Netzwerk-Inventarisierung.

## Ziel

Die App scannt IP-Bereiche, pingt Adressen an, versucht Hostnamen, MAC-Adressen und Herstellerinformationen zu ermitteln und zeigt die Ergebnisse in einer übersichtlichen Tabelle an.

## Funktionsumfang Version 1

- Windows Desktop-App mit `Setup.exe` Installer-Grundlage
- dunkles Dashboard-Design angelehnt an `home.jqpollag.de`
- App-Name: **PJU Network Tester**
- App-/Installer-Icon aktuell vorbereitet, echtes Webseiten-Favicon folgt noch
- Sprachen: Deutsch und Englisch
- Sprachwechsel in der App
- lokale Einstellungen als JSON-Datei
- IP-Bereich-Eingabe über:
  - `10.1.5.0/24`
  - `10.1.5.1/24`
  - `10.1.2.1 - 10.1.25.255`
  - einzelne IP, z. B. `10.1.5.20`
- Ping-Scan über den eingegebenen Bereich
- Reverse-DNS/Hostname-Ermittlung
- MAC-Adresse, soweit technisch möglich
- Hersteller/OUI-Erkennung, soweit technisch möglich
- standardmäßig nur Online-Geräte anzeigen
- Einstellung zum Anzeigen von Offline-Adressen
- komplett nicht erreichbare `/24`-Bereiche als zusammengefasste Tabellenzeile anzeigen
- Export der Tabelle als Excel/XLSX

## Bewusst nicht in Version 1

- TCP/UDP-Zwei-PC-Verbindungstest
- Portscanner
- Scan-Historie
- Vergleich alter/neuer Scans
- Server-/Client-Modus

Diese Funktionen können später ergänzt werden, bleiben aber zunächst draußen, damit Version 1 stabil und überschaubar bleibt.

## Technik

- C# / .NET
- Avalonia UI
- Inno Setup für den Windows-Installer
- lokale Einstellungen als JSON-Datei
- keine externe Datenbank

Weitere Details:

- [Implementierungsplan](docs/implementation-plan.md)
- [Windows Publish und Installer](docs/windows-publish-installer.md)
