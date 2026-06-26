using PjuNetworkTester.Core.Networking;

namespace PjuNetworkTester.Core.Scanning;

public sealed class NetworkScanner(INetworkProbe? probe = null)
{
    private readonly INetworkProbe _probe = probe ?? new PingNetworkProbe();

    public async Task<IReadOnlyList<ScanResult>> ScanAsync(
        IpRange range,
        ScanOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= ScanOptions.Default;
        if (options.MaxConcurrency < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "MaxConcurrency muss mindestens 1 sein.");
        }

        var results = new ScanResult[range.AddressCount];
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
                    semaphore.Release();
                }
            }, cancellationToken));

            if (value == uint.MaxValue)
            {
                break;
            }
        }

        await Task.WhenAll(tasks);
        return results;
    }
}
