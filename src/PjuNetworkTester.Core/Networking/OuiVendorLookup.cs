using System.Text.RegularExpressions;

namespace PjuNetworkTester.Core.Networking;

public sealed class OuiVendorLookup(IReadOnlyDictionary<string, string> vendors)
{
    public string? FindVendor(string? macAddress)
    {
        var prefix = NormalizeOuiPrefix(macAddress);
        if (prefix is null)
        {
            return null;
        }

        return vendors.TryGetValue(prefix, out var vendor) ? vendor : null;
    }

    private static string? NormalizeOuiPrefix(string? macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
        {
            return null;
        }

        var hex = Regex.Replace(macAddress, "[^0-9A-Fa-f]", string.Empty).ToUpperInvariant();
        return hex.Length >= 6 ? hex[..6] : null;
    }
}
