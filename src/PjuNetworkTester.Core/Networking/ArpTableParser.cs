using System.Net;
using System.Text.RegularExpressions;

namespace PjuNetworkTester.Core.Networking;

public static class ArpTableParser
{
    private static readonly Regex WindowsArpLinePattern = new(
        @"^\s*(?<ip>\d{1,3}(?:\.\d{1,3}){3})\s+(?<mac>[0-9A-Fa-f]{2}(?:-[0-9A-Fa-f]{2}){5})\s+\S+\s*$",
        RegexOptions.Compiled);

    private static readonly Regex LinuxNeighborLinePattern = new(
        @"^\s*(?<ip>\d{1,3}(?:\.\d{1,3}){3})\s+.*?\slladdr\s+(?<mac>[0-9A-Fa-f]{2}(?::[0-9A-Fa-f]{2}){5})\s+.*$",
        RegexOptions.Compiled);

    public static IReadOnlyDictionary<IPAddress, string> ParseWindowsArpOutput(string output)
    {
        return Parse(output, WindowsArpLinePattern);
    }

    public static IReadOnlyDictionary<IPAddress, string> ParseLinuxIpNeighborOutput(string output)
    {
        return Parse(output, LinuxNeighborLinePattern);
    }

    private static IReadOnlyDictionary<IPAddress, string> Parse(string output, Regex linePattern)
    {
        var entries = new Dictionary<IPAddress, string>();

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var match = linePattern.Match(line);
            if (!match.Success)
            {
                continue;
            }

            if (!IPAddress.TryParse(match.Groups["ip"].Value, out var ipAddress))
            {
                continue;
            }

            entries[ipAddress] = NormalizeMacAddress(match.Groups["mac"].Value);
        }

        return entries;
    }

    public static string NormalizeMacAddress(string macAddress)
    {
        var hex = Regex.Replace(macAddress, "[^0-9A-Fa-f]", string.Empty).ToUpperInvariant();
        if (hex.Length != 12)
        {
            return macAddress;
        }

        return string.Join(':', Enumerable.Range(0, 6).Select(index => hex.Substring(index * 2, 2)));
    }
}
