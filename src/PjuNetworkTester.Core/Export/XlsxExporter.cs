using ClosedXML.Excel;
using PjuNetworkTester.Core.Scanning;

namespace PjuNetworkTester.Core.Export;

public static class XlsxExporter
{
    private static readonly string[] Headers =
    [
        "Status",
        "IP / Bereich",
        "Hostname",
        "MAC-Adresse",
        "Hersteller",
        "Ping",
        "Bemerkung"
    ];

    public static Task ExportAsync(
        IEnumerable<ScanDisplayRow> rows,
        string exportPath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(exportPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Scan Ergebnisse");

        WriteHeaders(worksheet);
        WriteRows(worksheet, rows, cancellationToken);
        FormatWorksheet(worksheet);

        workbook.SaveAs(exportPath);
        return Task.CompletedTask;
    }

    private static void WriteHeaders(IXLWorksheet worksheet)
    {
        for (var column = 0; column < Headers.Length; column++)
        {
            var cell = worksheet.Cell(1, column + 1);
            cell.Value = Headers[column];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#111827");
            cell.Style.Font.FontColor = XLColor.White;
        }
    }

    private static void WriteRows(
        IXLWorksheet worksheet,
        IEnumerable<ScanDisplayRow> rows,
        CancellationToken cancellationToken)
    {
        var rowNumber = 2;
        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            worksheet.Cell(rowNumber, 1).Value = ToDisplayStatus(row.Status);
            worksheet.Cell(rowNumber, 2).Value = row.AddressOrRange;
            worksheet.Cell(rowNumber, 3).Value = row.Hostname ?? string.Empty;
            worksheet.Cell(rowNumber, 4).Value = row.MacAddress ?? string.Empty;
            worksheet.Cell(rowNumber, 5).Value = row.Vendor ?? string.Empty;
            worksheet.Cell(rowNumber, 6).Value = row.RoundtripTimeMs is null ? string.Empty : $"{row.RoundtripTimeMs} ms";
            worksheet.Cell(rowNumber, 7).Value = row.Note ?? string.Empty;

            if (row.Status == ScanDisplayStatus.UnreachableSubnet)
            {
                worksheet.Range(rowNumber, 1, rowNumber, Headers.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF3C7");
            }

            rowNumber++;
        }
    }

    private static void FormatWorksheet(IXLWorksheet worksheet)
    {
        var usedRange = worksheet.RangeUsed();
        if (usedRange is null)
        {
            return;
        }

        usedRange.SetAutoFilter();
        usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        worksheet.Columns().AdjustToContents();
        worksheet.SheetView.FreezeRows(1);
    }

    private static string ToDisplayStatus(ScanDisplayStatus status)
    {
        return status switch
        {
            ScanDisplayStatus.Online => "Online",
            ScanDisplayStatus.Offline => "Offline",
            ScanDisplayStatus.UnreachableSubnet => "Nicht erreichbarer Bereich",
            _ => status.ToString()
        };
    }
}
