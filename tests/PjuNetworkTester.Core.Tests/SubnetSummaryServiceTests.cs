using System.Net;
using PjuNetworkTester.Core.Scanning;

namespace PjuNetworkTester.Core.Tests;

public sealed class SubnetSummaryServiceTests
{
    [Fact]
    public void Summarize_keeps_online_hosts_inside_partly_reachable_subnet()
    {
        var range = new IpRangeParserResult("10.1.5.0/24");
        var results = new[]
        {
            ScanResult.Online(IPAddress.Parse("10.1.5.1"), hostname: "router.local", roundtripTimeMs: 2),
            ScanResult.Offline(IPAddress.Parse("10.1.5.2")),
            ScanResult.Online(IPAddress.Parse("10.1.5.20"), hostname: "server.local", roundtripTimeMs: 4),
        };

        var summarized = SubnetSummaryService.Summarize(range.Start, range.End, results, showOfflineAddresses: false);

        Assert.Collection(
            summarized,
            first => Assert.Equal("10.1.5.1", first.AddressOrRange),
            second => Assert.Equal("10.1.5.20", second.AddressOrRange));
    }

    [Fact]
    public void Summarize_replaces_fully_unreachable_24_with_one_summary_row()
    {
        var range = new IpRangeParserResult("10.1.4.0/24");
        var results = Enumerable.Range(0, 256)
            .Select(offset => ScanResult.Offline(IPAddress.Parse($"10.1.4.{offset}")))
            .ToArray();

        var summarized = SubnetSummaryService.Summarize(range.Start, range.End, results, showOfflineAddresses: true);

        var row = Assert.Single(summarized);
        Assert.Equal("10.1.4.0/24", row.AddressOrRange);
        Assert.Equal(ScanDisplayStatus.UnreachableSubnet, row.Status);
        Assert.Contains("nicht erreichbar", row.Note);
    }

    [Fact]
    public void Summarize_show_offline_false_hides_individual_offline_addresses()
    {
        var range = new IpRangeParserResult("10.1.5.0/30");
        var results = new[]
        {
            ScanResult.Online(IPAddress.Parse("10.1.5.1"), hostname: null, roundtripTimeMs: 1),
            ScanResult.Offline(IPAddress.Parse("10.1.5.2")),
        };

        var summarized = SubnetSummaryService.Summarize(range.Start, range.End, results, showOfflineAddresses: false);

        var row = Assert.Single(summarized);
        Assert.Equal("10.1.5.1", row.AddressOrRange);
    }

    private sealed class IpRangeParserResult
    {
        public IpRangeParserResult(string input)
        {
            var range = Networking.IpRangeParser.Parse(input);
            Start = range.Start;
            End = range.End;
        }

        public IPAddress Start { get; }

        public IPAddress End { get; }
    }
}
