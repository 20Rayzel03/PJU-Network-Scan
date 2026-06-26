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
using PjuNetworkTester.Core.Settings;

namespace PjuNetworkTester.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly NetworkScanner _scanner = new();
    private readonly AppSettingsStore _settingsStore = AppSettingsStore.CreateDefault();
    private readonly List<ScanDisplayRow> _currentDisplayRows = [];
    private AppLocalizer _localizer = new(AppLanguage.German);
    private CancellationTokenSource? _scanCancellation;
    private CancellationTokenSource? _progressHideCancellation;

    public MainWindowViewModel()
    {
        LanguageOptions =
        [
            new LanguageOption(AppLanguage.German, "Deutsch"),
            new LanguageOption(AppLanguage.English, "English")
        ];
        SelectedLanguage = LanguageOptions[0];
        _ = LoadSettingsAsync();
    }

    [ObservableProperty]
    private string _scanInput = "10.1.5.0/24";

    [ObservableProperty]
    private string _statusText = "Bereit.";

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private double _scanProgressPercent;

    [ObservableProperty]
    private string _scanProgressText = "0 %";

    [ObservableProperty]
    private bool _isScanProgressVisible;

    [ObservableProperty]
    private bool _showOfflineAddresses;

    [ObservableProperty]
    private LanguageOption _selectedLanguage;

    public IReadOnlyList<LanguageOption> LanguageOptions { get; }

    public string AppTitle => _localizer.Translate(AppText.AppTitle);

    public string RangePlaceholder => _localizer.Translate(AppText.RangePlaceholder);

    public string StartScanText => _localizer.Translate(AppText.StartScan);

    public string StopText => _localizer.Translate(AppText.Stop);

    public string ExportExcelText => _localizer.Translate(AppText.ExportExcel);

    public string ShowOfflineAddressesText => _localizer.Translate(AppText.ShowOfflineAddresses);

    public string LanguageText => _localizer.Translate(AppText.Language);

    public ObservableCollection<ScanResultRowViewModel> Results { get; } = [];

    [RelayCommand(CanExecute = nameof(CanStartScan))]
    private async Task StartScanAsync()
    {
        IsScanning = true;
        Results.Clear();
        _currentDisplayRows.Clear();
        ScanProgressPercent = 0;
        ScanProgressText = "0 %";
        ShowScanProgress();
        ExportCommand.NotifyCanExecuteChanged();
        _scanCancellation = new CancellationTokenSource();

        try
        {
            StatusText = _localizer.Translate(AppText.CheckingRange);
            var range = IpRangeParser.Parse(ScanInput);
            StatusText = SelectedLanguage.Language == AppLanguage.English
                ? $"Scan running: {range.Start} - {range.End} ({range.AddressCount} addresses)"
                : $"Scan läuft: {range.Start} - {range.End} ({range.AddressCount} Adressen)";

            var progress = new Progress<ScanProgress>(UpdateScanProgress);
            var scanResults = await _scanner.ScanAsync(range, ScanOptions.Default, _scanCancellation.Token, progress);
            var displayRows = SubnetSummaryService.Summarize(range.Start, range.End, scanResults, ShowOfflineAddresses);
            _currentDisplayRows.Clear();
            _currentDisplayRows.AddRange(displayRows);

            foreach (var row in _currentDisplayRows)
            {
                Results.Add(ScanResultRowViewModel.FromDisplayRow(row));
            }

            var onlineCount = scanResults.Count(result => result.IsOnline);
            StatusText = SelectedLanguage.Language == AppLanguage.English
                ? $"Scan completed. Online: {onlineCount}, displayed rows: {Results.Count}"
                : $"Scan abgeschlossen. Online: {onlineCount}, angezeigte Zeilen: {Results.Count}";
        }
        catch (OperationCanceledException)
        {
            StatusText = SelectedLanguage.Language == AppLanguage.English ? "Scan cancelled." : "Scan abgebrochen.";
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
            ScheduleScanProgressHide();
            ExportCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportAsync()
    {
        try
        {
            var exportDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "PJU Network Tester");
            var exportPath = Path.Combine(
                exportDirectory,
                $"PJU-Network-Scan_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx");

            await XlsxExporter.ExportAsync(_currentDisplayRows, exportPath, CancellationToken.None);
            StatusText = SelectedLanguage.Language == AppLanguage.English
                ? $"Excel export saved: {exportPath}"
                : $"Excel-Export gespeichert: {exportPath}";
        }
        catch (Exception exception)
        {
            StatusText = SelectedLanguage.Language == AppLanguage.English
                ? $"Excel export failed: {exception.Message}"
                : $"Excel-Export fehlgeschlagen: {exception.Message}";
        }
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

    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        if (value is null)
        {
            return;
        }

        _localizer = new AppLocalizer(value.Language);
        NotifyLocalizedPropertiesChanged();
        if (StatusText is "Bereit." or "Ready.")
        {
            StatusText = _localizer.Translate(AppText.Ready);
        }

        _ = SaveSettingsAsync();
    }

    partial void OnShowOfflineAddressesChanged(bool value)
    {
        _ = SaveSettingsAsync();
    }

    private void UpdateScanProgress(ScanProgress progress)
    {
        IsScanProgressVisible = true;
        ScanProgressPercent = progress.PercentComplete;
        ScanProgressText = $"{progress.PercentComplete} %";
    }

    private void ShowScanProgress()
    {
        _progressHideCancellation?.Cancel();
        _progressHideCancellation = null;
        IsScanProgressVisible = true;
    }

    private void ScheduleScanProgressHide()
    {
        _progressHideCancellation?.Cancel();
        var hideCancellation = new CancellationTokenSource();
        _progressHideCancellation = hideCancellation;
        _ = HideScanProgressAfterDelayAsync(hideCancellation);
    }

    private async Task HideScanProgressAfterDelayAsync(CancellationTokenSource hideCancellation)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(20), hideCancellation.Token);
            if (!hideCancellation.IsCancellationRequested && ReferenceEquals(_progressHideCancellation, hideCancellation))
            {
                IsScanProgressVisible = false;
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (ReferenceEquals(_progressHideCancellation, hideCancellation))
            {
                _progressHideCancellation = null;
            }

            hideCancellation.Dispose();
        }
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            var settings = await _settingsStore.LoadAsync(CancellationToken.None);
            var language = LanguageOptions.FirstOrDefault(option => option.Language == settings.Language) ?? LanguageOptions[0];
            SelectedLanguage = language;
            ShowOfflineAddresses = settings.ShowOfflineAddresses;
            StatusText = _localizer.Translate(AppText.Ready);
        }
        catch
        {
            StatusText = _localizer.Translate(AppText.Ready);
        }
    }

    private Task SaveSettingsAsync()
    {
        var settings = new AppSettings(SelectedLanguage.Language, ShowOfflineAddresses);
        return _settingsStore.SaveAsync(settings, CancellationToken.None);
    }

    private void NotifyLocalizedPropertiesChanged()
    {
        OnPropertyChanged(nameof(AppTitle));
        OnPropertyChanged(nameof(RangePlaceholder));
        OnPropertyChanged(nameof(StartScanText));
        OnPropertyChanged(nameof(StopText));
        OnPropertyChanged(nameof(ExportExcelText));
        OnPropertyChanged(nameof(ShowOfflineAddressesText));
        OnPropertyChanged(nameof(LanguageText));
    }

    private bool CanStartScan() => !IsScanning;

    private bool CanStopScan() => IsScanning;

    private bool CanExport() => !IsScanning && _currentDisplayRows.Count > 0;
}

public sealed record LanguageOption(AppLanguage Language, string DisplayName)
{
    public override string ToString() => DisplayName;
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
