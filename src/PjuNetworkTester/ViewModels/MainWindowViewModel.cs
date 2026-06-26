using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PjuNetworkTester.Core.Export;
using PjuNetworkTester.Core.Networking;
using PjuNetworkTester.Core.Scanning;

namespace PjuNetworkTester.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly NetworkScanner _scanner = new();
    private readonly List<ScanDisplayRow> _currentDisplayRows = [];
    private CancellationTokenSource? _scanCancellation;

    [ObservableProperty]
    private string _scanInput = "10.1.5.0/24";

    [ObservableProperty]
    private string _statusText = "Bereit.";

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private bool _showOfflineAddresses;

    public ObservableCollection<ScanResultRowViewModel> Results { get; } = [];

    [RelayCommand(CanExecute = nameof(CanStartScan))]
    private async Task StartScanAsync()
    {
        IsScanning = true;
        Results.Clear();
        _currentDisplayRows.Clear();
        ExportCommand.NotifyCanExecuteChanged();
        _scanCancellation = new CancellationTokenSource();

        try
        {
            StatusText = "IP-Bereich wird geprüft ...";
            var range = IpRangeParser.Parse(ScanInput);
            StatusText = $"Scan läuft: {range.Start} - {range.End} ({range.AddressCount} Adressen)";

            var scanResults = await _scanner.ScanAsync(range, ScanOptions.Default, _scanCancellation.Token);
            var displayRows = SubnetSummaryService.Summarize(range.Start, range.End, scanResults, ShowOfflineAddresses);
            _currentDisplayRows.Clear();
            _currentDisplayRows.AddRange(displayRows);

            foreach (var row in _currentDisplayRows)
            {
                Results.Add(ScanResultRowViewModel.FromDisplayRow(row));
            }

            var onlineCount = scanResults.Count(result => result.IsOnline);
            StatusText = $"Scan abgeschlossen. Online: {onlineCount}, angezeigte Zeilen: {Results.Count}";
        }
        catch (OperationCanceledException)
        {
            StatusText = "Scan abgebrochen.";
        }
        catch (FormatException exception)
        {
            StatusText = exception.Message;
        }
        finally
        {
            _scanCancellation?.Dispose();
            _scanCancellation = null;
            IsScanning = false;
            ExportCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportAsync()
    {
        var exportDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "PJU Network Tester");
        var exportPath = Path.Combine(
            exportDirectory,
            $"PJU-Network-Scan_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx");

        await XlsxExporter.ExportAsync(_currentDisplayRows, exportPath, CancellationToken.None);
        StatusText = $"Excel-Export gespeichert: {exportPath}";
    }

    [RelayCommand(CanExecute = nameof(CanStopScan))]
    private void StopScan()
    {
        _scanCancellation?.Cancel();
    }

    partial void OnIsScanningChanged(bool value)
    {
        StartScanCommand.NotifyCanExecuteChanged();
        StopScanCommand.NotifyCanExecuteChanged();
        ExportCommand.NotifyCanExecuteChanged();
    }

    private bool CanStartScan() => !IsScanning;

    private bool CanStopScan() => IsScanning;

    private bool CanExport() => !IsScanning && _currentDisplayRows.Count > 0;
}

public sealed class ScanResultRowViewModel
{
    public required string StatusIcon { get; init; }

    public required string AddressOrRange { get; init; }

    public required string Hostname { get; init; }

    public required string MacAddress { get; init; }

    public required string Vendor { get; init; }

    public required string RoundtripTime { get; init; }

    public required string Note { get; init; }

    public static ScanResultRowViewModel FromDisplayRow(ScanDisplayRow row)
    {
        return new ScanResultRowViewModel
        {
            StatusIcon = row.Status switch
            {
                ScanDisplayStatus.Online => "✅",
                ScanDisplayStatus.Offline => "⚪",
                ScanDisplayStatus.UnreachableSubnet => "⚠️",
                _ => "?"
            },
            AddressOrRange = row.AddressOrRange,
            Hostname = row.Hostname ?? "—",
            MacAddress = row.MacAddress ?? "—",
            Vendor = row.Vendor ?? "—",
            RoundtripTime = row.RoundtripTimeMs is null ? "—" : $"{row.RoundtripTimeMs} ms",
            Note = row.Note ?? ""
        };
    }
}
