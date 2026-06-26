using System.Net;

namespace PjuNetworkTester.Core.Networking;

public sealed record IpRange(IPAddress Start, IPAddress End)
{
    public uint StartValue { get; } = Ipv4ToUInt32(Start);

    public uint EndValue { get; } = Ipv4ToUInt32(End);

    public uint AddressCount => EndValue - StartValue + 1;

    public static uint Ipv4ToUInt32(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        if (bytes.Length != 4)
        {
            throw new FormatException("Der IP-Bereich darf nur IPv4-Adressen enthalten.");
        }

        return ((uint)bytes[0] << 24)
            | ((uint)bytes[1] << 16)
            | ((uint)bytes[2] << 8)
            | bytes[3];
    }

    public static IPAddress UInt32ToIpv4(uint value)
    {
        return new IPAddress([
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value
        ]);
    }
}
