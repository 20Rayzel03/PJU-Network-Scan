using System.Net;

namespace PjuNetworkTester.Core.Networking;

public static class IpRangeParser
{
    public static IpRange Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw InvalidRange();
        }

        var trimmed = input.Trim();

        if (trimmed.Contains('/'))
        {
            return ParseCidr(trimmed);
        }

        if (trimmed.Contains('-'))
        {
            return ParseExplicitRange(trimmed);
        }

        var singleAddress = ParseIpv4(trimmed);
        return new IpRange(singleAddress, singleAddress);
    }

    private static IpRange ParseExplicitRange(string input)
    {
        var parts = input.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            throw InvalidRange();
        }

        var start = ParseIpv4(parts[0]);
        var end = ParseIpv4(parts[1]);
        var range = new IpRange(start, end);

        if (range.StartValue > range.EndValue)
        {
            throw InvalidRange();
        }

        return range;
    }

    private static IpRange ParseCidr(string input)
    {
        var parts = input.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !int.TryParse(parts[1], out var prefixLength))
        {
            throw InvalidRange();
        }

        if (prefixLength < 0 || prefixLength > 32)
        {
            throw InvalidRange();
        }

        var address = ParseIpv4(parts[0]);
        var addressValue = IpRange.Ipv4ToUInt32(address);
        var mask = prefixLength == 0 ? 0u : uint.MaxValue << (32 - prefixLength);
        var network = addressValue & mask;
        var broadcast = network | ~mask;

        return new IpRange(IpRange.UInt32ToIpv4(network), IpRange.UInt32ToIpv4(broadcast));
    }

    private static IPAddress ParseIpv4(string value)
    {
        if (!IPAddress.TryParse(value, out var address) || address.GetAddressBytes().Length != 4)
        {
            throw InvalidRange();
        }

        return address;
    }

    private static FormatException InvalidRange()
    {
        return new FormatException("Der IP-Bereich ist ungültig. Unterstützt werden einzelne IPv4-Adressen, Bereiche und CIDR-Notation.");
    }
}
