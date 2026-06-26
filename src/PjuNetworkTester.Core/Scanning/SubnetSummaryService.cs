using System.Net;
using PjuNetworkTester.Core.Networking;

namespace PjuNetworkTester.Core.Scanning;

public enum ScanDisplayStatus
{
    Online,
    Offline,
    UnreachableSubnet
}

public sealed record ScanDisplayRow(
    string AddressOrRange,
    ScanDisplayStatus Status,
    string? Hostname,
    string? MacAddress,
    string? Vendor,
    long? RoundtripTimeMs,
    string? Note);

public static class SubnetSummaryService
{
    private const string UnreachableSubnetNote = "Adressbereich nicht erreichbar oder existiert nicht im Netzwerk.";

    public static IReadOnlyList<ScanDisplayRow> Summarize(
        IPAddress rangeStart,
        IPAddress rangeEnd,
        IEnumerable<ScanResult> results,
        bool showOfflineAddresses)
    {
        var startValue = IpRange.Ipv4ToUInt32(rangeStart);
        var endValue = IpRange.Ipv4ToUInt32(rangeEnd);
        var resultList = results.OrderBy(result => IpRange.Ipv4ToUInt32(result.Address)).ToList();
        var resultsByAddress = resultList.ToDictionary(result => IpRange.Ipv4ToUInt32(result.Address));
        var rows = new List<ScanDisplayRow>();

        var currentSubnetStart = startValue & 0xFFFFFF00u;
        while (currentSubnetStart <= endValue)
        {
            var currentSubnetEnd = currentSubnetStart | 0xFFu;
            var segmentStart = Math.Max(startValue, currentSubnetStart);
            var segmentEnd = Math.Min(endValue, currentSubnetEnd);
            var subnetResults = resultList
                .Where(result =>
                {
                    var value = IpRange.Ipv4ToUInt32(result.Address);
                    return value >= segmentStart && value <= segmentEnd;
                })
                .ToList();

            var coversFull24 = segmentStart == currentSubnetStart && segmentEnd == currentSubnetEnd;
            var isFullyUnreachable24 = coversFull24
                && subnetResults.Count == 256
                && subnetResults.All(result => !result.IsOnline);

            if (isFullyUnreachable24)
            {
                rows.Add(new ScanDisplayRow(
                    $"{IpRange.UInt32ToIpv4(currentSubnetStart)}/24",
                    ScanDisplayStatus.UnreachableSubnet,
                    Hostname: null,
                    MacAddress: null,
                    Vendor: null,
                    RoundtripTimeMs: null,
                    Note: UnreachableSubnetNote));
            }
            else
            {
                for (var value = segmentStart; value <= segmentEnd; value++)
                {
                    if (!resultsByAddress.TryGetValue(value, out var result))
                    {
                        continue;
                    }

                    if (!result.IsOnline && !showOfflineAddresses)
                    {
                        continue;
                    }

                    rows.Add(ToDisplayRow(result));
                }
            }

            if (currentSubnetStart > uint.MaxValue - 256u)
            {
                break;
            }

            currentSubnetStart += 256u;
        }

        return rows;
    }

    private static ScanDisplayRow ToDisplayRow(ScanResult result)
    {
        return new ScanDisplayRow(
            result.Address.ToString(),
            result.IsOnline ? ScanDisplayStatus.Online : ScanDisplayStatus.Offline,
            result.Hostname,
            result.MacAddress,
            result.Vendor,
            result.RoundtripTimeMs,
            result.Note);
    }
}
