using PjuNetworkTester.Core.Scanning;

namespace PjuNetworkTester.Core.Settings;

public enum AppText
{
    AppTitle,
    RangePlaceholder,
    StartScan,
    Stop,
    ExportExcel,
    ShowOfflineAddresses,
    Ready,
    CheckingRange,
    Language,
    Status,
    AddressOrRange,
    Hostname,
    MacAddress,
    Vendor,
    Ping,
    Note
}

public sealed class AppLocalizer(AppLanguage language)
{
    private static readonly IReadOnlyDictionary<AppText, string> German = new Dictionary<AppText, string>
    {
        [AppText.AppTitle] = "PJU Network Tester",
        [AppText.RangePlaceholder] = "10.1.5.0/24 oder 10.1.2.1 - 10.1.25.255",
        [AppText.StartScan] = "Scan starten",
        [AppText.Stop] = "Stop",
        [AppText.ExportExcel] = "Excel exportieren",
        [AppText.ShowOfflineAddresses] = "Offline-Adressen anzeigen",
        [AppText.Ready] = "Bereit.",
        [AppText.CheckingRange] = "IP-Bereich wird geprüft ...",
        [AppText.Language] = "Sprache",
        [AppText.Status] = "Status",
        [AppText.AddressOrRange] = "IP / Bereich",
        [AppText.Hostname] = "Hostname",
        [AppText.MacAddress] = "MAC-Adresse",
        [AppText.Vendor] = "Hersteller",
        [AppText.Ping] = "Ping",
        [AppText.Note] = "Bemerkung"
    };

    private static readonly IReadOnlyDictionary<AppText, string> English = new Dictionary<AppText, string>
    {
        [AppText.AppTitle] = "PJU Network Tester",
        [AppText.RangePlaceholder] = "10.1.5.0/24 or 10.1.2.1 - 10.1.25.255",
        [AppText.StartScan] = "Start scan",
        [AppText.Stop] = "Stop",
        [AppText.ExportExcel] = "Export Excel",
        [AppText.ShowOfflineAddresses] = "Show offline addresses",
        [AppText.Ready] = "Ready.",
        [AppText.CheckingRange] = "Checking IP range ...",
        [AppText.Language] = "Language",
        [AppText.Status] = "Status",
        [AppText.AddressOrRange] = "IP / range",
        [AppText.Hostname] = "Hostname",
        [AppText.MacAddress] = "MAC address",
        [AppText.Vendor] = "Vendor",
        [AppText.Ping] = "Ping",
        [AppText.Note] = "Note"
    };

    public string Translate(AppText text)
    {
        var dictionary = language == AppLanguage.English ? English : German;
        return dictionary[text];
    }

    public string TranslateStatusLabel(ScanDisplayStatus status)
    {
        return language switch
        {
            AppLanguage.English => status switch
            {
                ScanDisplayStatus.Online => "Online",
                ScanDisplayStatus.Offline => "Offline",
                ScanDisplayStatus.UnreachableSubnet => "Unreachable subnet",
                _ => status.ToString()
            },
            _ => status switch
            {
                ScanDisplayStatus.Online => "Online",
                ScanDisplayStatus.Offline => "Offline",
                ScanDisplayStatus.UnreachableSubnet => "Nicht erreichbarer Bereich",
                _ => status.ToString()
            }
        };
    }
}
