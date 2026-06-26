using System.Net;

namespace PjuNetworkTester.Core.Scanning;

public sealed record ScanResult(
    IPAddress Address,
    bool IsOnline,
    string? Hostname,
    string? MacAddress,
    string? Vendor,
    long? RoundtripTimeMs,
    string? Note)
{
    public static ScanResult Online(
        IPAddress address,
        string? hostname = null,
        string? macAddress = null,
        string? vendor = null,
        long? roundtripTimeMs = null,
        string? note = null)
    {
        return new ScanResult(address, true, hostname, macAddress, vendor, roundtripTimeMs, note);
    }

    public static ScanResult Offline(IPAddress address, string? note = null)
    {
        return new ScanResult(address, false, null, null, null, null, note);
    }
}
