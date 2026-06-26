using PjuNetworkTester.Core.Networking;

namespace PjuNetworkTester.Core.Scanning;

public sealed class NetworkScanner(
    INetworkProbe? probe = null,
    IArpTableReader? arpTableReader = null,
    OuiVendorLookup? vendorLookup = null)
{
    private readonly INetworkProbe _probe = probe ?? new PingNetworkProbe();
    private readonly IArpTableReader _arpTableReader = arpTableReader ?? new ArpTableReader();
    private readonly OuiVendorLookup _vendorLookup = vendorLookup ?? new OuiVendorLookup(DefaultVendors.Vendors);

    public async Task<IReadOnlyList<ScanResult>> ScanAsync(
        IpRange range,
        ScanOptions? options = null,
        CancellationToken cancellationToken = default,
        IProgress<ScanProgress>? progress = null)
    {
        options ??= ScanOptions.Default;
        if (options.MaxConcurrency < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "MaxConcurrency muss mindestens 1 sein.");
        }

        var results = new ScanResult[range.AddressCount];
        var completedAddresses = 0;
        using var semaphore = new SemaphoreSlim(options.MaxConcurrency, options.MaxConcurrency);
        var tasks = new List<Task>((int)Math.Min(range.AddressCount, 4096));

        for (var value = range.StartValue; value <= range.EndValue; value++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var index = (int)(value - range.StartValue);
            var address = IpRange.UInt32ToIpv4(value);

            await semaphore.WaitAsync(cancellationToken);
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var probeResult = await _probe.ProbeAsync(address, options.Timeout, cancellationToken);
                    results[index] = probeResult.IsOnline
                        ? ScanResult.Online(address, probeResult.Hostname, roundtripTimeMs: probeResult.RoundtripTimeMs)
                        : ScanResult.Offline(address);
                }
                finally
                {
                    var completed = Interlocked.Increment(ref completedAddresses);
                    progress?.Report(new ScanProgress(completed, results.Length));
                    semaphore.Release();
                }
            }, cancellationToken));

            if (value == uint.MaxValue)
            {
                break;
            }
        }

        await Task.WhenAll(tasks);
        await EnrichWithMacAndVendorAsync(results, cancellationToken);
        return results;
    }

    private async Task EnrichWithMacAndVendorAsync(ScanResult[] results, CancellationToken cancellationToken)
    {
        if (!results.Any(result => result.IsOnline))
        {
            return;
        }

        IReadOnlyDictionary<System.Net.IPAddress, string> arpEntries;
        try
        {
            arpEntries = await _arpTableReader.ReadAsync(cancellationToken);
        }
        catch
        {
            return;
        }

        for (var index = 0; index < results.Length; index++)
        {
            var result = results[index];
            if (!result.IsOnline || !arpEntries.TryGetValue(result.Address, out var macAddress))
            {
                continue;
            }

            var normalizedMac = ArpTableParser.NormalizeMacAddress(macAddress);
            var vendor = _vendorLookup.FindVendor(normalizedMac);
            results[index] = result with
            {
                MacAddress = normalizedMac,
                Vendor = vendor
            };
        }
    }
}
