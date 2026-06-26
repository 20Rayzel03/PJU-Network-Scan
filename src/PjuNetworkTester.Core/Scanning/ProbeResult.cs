namespace PjuNetworkTester.Core.Scanning;

public sealed record ProbeResult(bool IsOnline, long? RoundtripTimeMs, string? Hostname)
{
    public static ProbeResult Online(long roundtripTimeMs, string? hostname = null)
    {
        return new ProbeResult(true, roundtripTimeMs, hostname);
    }

    public static ProbeResult Offline()
    {
        return new ProbeResult(false, null, null);
    }
}
