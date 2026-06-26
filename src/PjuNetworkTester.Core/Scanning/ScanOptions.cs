namespace PjuNetworkTester.Core.Scanning;

public sealed record ScanOptions(int MaxConcurrency, TimeSpan Timeout)
{
    public static ScanOptions Default { get; } = new(MaxConcurrency: 64, Timeout: TimeSpan.FromMilliseconds(750));
}
