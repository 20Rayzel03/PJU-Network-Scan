using System.Net;
using PjuNetworkTester.Core.Networking;
using PjuNetworkTester.Core.Scanning;

namespace PjuNetworkTester.Core.Tests;

public sealed class NetworkScannerTests
{
    [Fact]
    public async Task ScanAsync_scans_every_address_in_range()
    {
        var probe = new FakeNetworkProbe(new Dictionary<string, ProbeResult>
        {
            ["10.1.5.1"] = ProbeResult.Online(2, "router.local"),
            ["10.1.5.2"] = ProbeResult.Offline(),
            ["10.1.5.3"] = ProbeResult.Online(5, "server.local"),
        });
        var scanner = new NetworkScanner(probe);
        var range = IpRangeParser.Parse("10.1.5.1 - 10.1.5.3");

        var results = await scanner.ScanAsync(range, new ScanOptions(MaxConcurrency: 2, Timeout: TimeSpan.FromMilliseconds(50)), CancellationToken.None);

        Assert.Equal(new[] { "10.1.5.1", "10.1.5.2", "10.1.5.3" }, results.Select(result => result.Address.ToString()).ToArray());
        Assert.True(results[0].IsOnline);
        Assert.False(results[1].IsOnline);
        Assert.True(results[2].IsOnline);
        Assert.Equal("router.local", results[0].Hostname);
        Assert.Equal(5, results[2].RoundtripTimeMs);
    }

    [Fact]
    public async Task ScanAsync_respects_max_concurrency()
    {
        var probe = new TrackingNetworkProbe(delay: TimeSpan.FromMilliseconds(25));
        var scanner = new NetworkScanner(probe);
        var range = IpRangeParser.Parse("10.1.5.1 - 10.1.5.10");

        await scanner.ScanAsync(range, new ScanOptions(MaxConcurrency: 3, Timeout: TimeSpan.FromSeconds(1)), CancellationToken.None);

        Assert.True(probe.MaxObservedConcurrency <= 3, $"Observed concurrency was {probe.MaxObservedConcurrency}");
    }

    [Fact]
    public async Task ScanAsync_observes_cancellation()
    {
        var probe = new TrackingNetworkProbe(delay: TimeSpan.FromSeconds(5));
        var scanner = new NetworkScanner(probe);
        var range = IpRangeParser.Parse("10.1.5.1 - 10.1.5.254");
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(30));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            scanner.ScanAsync(range, new ScanOptions(MaxConcurrency: 4, Timeout: TimeSpan.FromSeconds(10)), cts.Token));
    }

    private sealed class FakeNetworkProbe(IReadOnlyDictionary<string, ProbeResult> results) : INetworkProbe
    {
        public Task<ProbeResult> ProbeAsync(IPAddress address, TimeSpan timeout, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(results.TryGetValue(address.ToString(), out var result) ? result : ProbeResult.Offline());
        }
    }

    private sealed class TrackingNetworkProbe(TimeSpan delay) : INetworkProbe
    {
        private int _currentConcurrency;

        public int MaxObservedConcurrency { get; private set; }

        public async Task<ProbeResult> ProbeAsync(IPAddress address, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var current = Interlocked.Increment(ref _currentConcurrency);
            MaxObservedConcurrency = Math.Max(MaxObservedConcurrency, current);

            try
            {
                await Task.Delay(delay, cancellationToken);
                return ProbeResult.Offline();
            }
            finally
            {
                Interlocked.Decrement(ref _currentConcurrency);
            }
        }
    }
}
