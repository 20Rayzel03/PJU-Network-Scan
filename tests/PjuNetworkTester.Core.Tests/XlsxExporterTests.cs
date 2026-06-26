using ClosedXML.Excel;
using PjuNetworkTester.Core.Export;
using PjuNetworkTester.Core.Scanning;

namespace PjuNetworkTester.Core.Tests;

public sealed class XlsxExporterTests
{
    [Fact]
    public async Task ExportAsync_writes_scan_rows_with_expected_headers_and_values()
    {
        var rows = new[]
        {
            new ScanDisplayRow(
                AddressOrRange: "10.1.5.1",
                Status: ScanDisplayStatus.Online,
                Hostname: "router.local",
                MacAddress: "AA:BB:CC:11:22:33",
                Vendor: "PJU Test Vendor",
                RoundtripTimeMs: 2,
                Note: ""),
            new ScanDisplayRow(
                AddressOrRange: "10.1.4.0/24",
                Status: ScanDisplayStatus.UnreachableSubnet,
                Hostname: null,
                MacAddress: null,
                Vendor: null,
                RoundtripTimeMs: null,
                Note: "Adressbereich nicht erreichbar oder existiert nicht im Netzwerk.")
        };
        var exportPath = Path.Combine(Path.GetTempPath(), $"pju-network-scan-{Guid.NewGuid():N}.xlsx");

        try
        {
            await XlsxExporter.ExportAsync(rows, exportPath, CancellationToken.None);

            using var workbook = new XLWorkbook(exportPath);
            var worksheet = workbook.Worksheet("Scan Ergebnisse");

            Assert.Equal("Status", worksheet.Cell(1, 1).GetString());
            Assert.Equal("IP / Bereich", worksheet.Cell(1, 2).GetString());
            Assert.Equal("Hostname", worksheet.Cell(1, 3).GetString());
            Assert.Equal("MAC-Adresse", worksheet.Cell(1, 4).GetString());
            Assert.Equal("Hersteller", worksheet.Cell(1, 5).GetString());
            Assert.Equal("Ping", worksheet.Cell(1, 6).GetString());
            Assert.Equal("Bemerkung", worksheet.Cell(1, 7).GetString());

            Assert.Equal("Online", worksheet.Cell(2, 1).GetString());
            Assert.Equal("10.1.5.1", worksheet.Cell(2, 2).GetString());
            Assert.Equal("router.local", worksheet.Cell(2, 3).GetString());
            Assert.Equal("AA:BB:CC:11:22:33", worksheet.Cell(2, 4).GetString());
            Assert.Equal("PJU Test Vendor", worksheet.Cell(2, 5).GetString());
            Assert.Equal("2 ms", worksheet.Cell(2, 6).GetString());

            Assert.Equal("Nicht erreichbarer Bereich", worksheet.Cell(3, 1).GetString());
            Assert.Equal("10.1.4.0/24", worksheet.Cell(3, 2).GetString());
            Assert.Equal("Adressbereich nicht erreichbar oder existiert nicht im Netzwerk.", worksheet.Cell(3, 7).GetString());
        }
        finally
        {
            if (File.Exists(exportPath))
            {
                File.Delete(exportPath);
            }
        }
    }
}
